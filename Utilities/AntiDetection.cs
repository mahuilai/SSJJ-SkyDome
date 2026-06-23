using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Linq;

namespace SkyDome.Utilities
{
    /// <summary>
    /// Anti-cheat detection evasion: PE header erasure, section name overwrite,
    /// import directory overwrite, RWX to RX, junk memory allocation
    /// </summary>
    public static class RuntimeProtection
    {
        #region P/Invoke
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool VirtualProtect(IntPtr lpAddress, UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);

        [DllImport("kernel32.dll")]
        private static extern IntPtr VirtualAlloc(IntPtr lpAddress, UIntPtr dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll")]
        private static extern bool VirtualFree(IntPtr lpAddress, UIntPtr dwSize, uint dwFreeType);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetCurrentProcess();

        [DllImport("psapi.dll", SetLastError = true)]
        private static extern bool GetModuleInformation(IntPtr hProcess, IntPtr hModule, out MODULEINFO lpmodinfo, uint cb);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int VirtualQuery(IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, UIntPtr dwLength);

        [StructLayout(LayoutKind.Sequential)]
        private struct MODULEINFO
        {
            public IntPtr lpBaseOfDll;
            public uint SizeOfImage;
            public IntPtr EntryPoint;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MEMORY_BASIC_INFORMATION
        {
            public IntPtr BaseAddress;
            public IntPtr AllocationBase;
            public uint AllocationProtect;
            public UIntPtr RegionSize;
            public uint State;
            public uint Protect;
            public uint Type;
        }

        private const uint PAGE_EXECUTE_READWRITE = 0x40;
        private const uint PAGE_EXECUTE_READ = 0x20;
        private const uint PAGE_READWRITE = 0x04;
        private const uint MEM_COMMIT = 0x1000;
        private const uint MEM_RESERVE = 0x2000;
        private const uint MEM_RELEASE = 0x8000;
        private const uint MEM_FREE = 0x10000;
        #endregion

        private static IntPtr _moduleBase = IntPtr.Zero;
        private static uint _moduleSize = 0;
        private static readonly List<IntPtr> _junkAllocations = new List<IntPtr>();
        private static bool _initialized = false;

        /// <summary>
        /// Execute all anti-detection measures
        /// </summary>
        public static void Initialize()
        {
            if (_initialized) return;
            _initialized = true;

            try
            {
                LocateOwnModule();
                ErasePEHeaders();
                OverwriteSectionNames();
                OverwriteImportDirectory();
                FixRWXPages();
                AllocateJunkMemory();
                // Re-run FixRWXPages to handle MonoMod trampolines allocated later
                System.Threading.ThreadPool.QueueUserWorkItem(_ => FixRWXPages());
            }
            catch (Exception ex)
            {
                // Silent - anti-detection failure should not crash
                System.Diagnostics.Debug.WriteLine("[AD] Init error: " + ex.Message);
            }
        }

        /// <summary>
        /// Locate own module base address and size
        /// </summary>
        private static void LocateOwnModule()
        {
            try
            {
                Assembly asm = Assembly.GetExecutingAssembly();
                string moduleName = asm.ManifestModule.Name.Replace(".dll", "");
                IntPtr hModule = GetModuleHandle(moduleName);
                if (hModule == IntPtr.Zero)
                {
                    // Try full path
                    string fullName = asm.ManifestModule.FullyQualifiedName;
                    hModule = GetModuleHandle(fullName);
                    if (hModule == IntPtr.Zero)
                    {
                        // Enumerate loaded assemblies
                        foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
                        {
                            try
                            {
                                string name = a.GetName().Name;
                                hModule = GetModuleHandle(name);
                                if (hModule != IntPtr.Zero)
                                {
                                    if (a == asm) break;
                                }
                            }
                            catch { }
                        }
                    }
                }

                if (hModule != IntPtr.Zero)
                {
                    _moduleBase = hModule;
                    IntPtr hProcess = GetCurrentProcess();
                    MODULEINFO mi;
                    if (GetModuleInformation(hProcess, hModule, out mi, (uint)Marshal.SizeOf<MODULEINFO>()))
                    {
                        _moduleSize = mi.SizeOfImage;
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// Erase DOS header, PE signature, and sensitive optional header fields
        /// </summary>
        public static void ErasePEHeaders()
        {
            if (_moduleBase == IntPtr.Zero || _moduleSize == 0) return;
            try
            {
                uint oldProtect;
                // Erase DOS header (first 64 bytes) + PE signature (4 bytes) = 512 bytes
                int eraseSize = 512;
                if (_moduleSize < eraseSize) eraseSize = (int)_moduleSize;

                VirtualProtect(_moduleBase, (UIntPtr)eraseSize, PAGE_READWRITE, out oldProtect);

                byte[] zeros = new byte[eraseSize];
                Marshal.Copy(zeros, 0, _moduleBase, eraseSize);

                VirtualProtect(_moduleBase, (UIntPtr)eraseSize, oldProtect, out _);
            }
            catch { }
        }

        /// <summary>
        /// Overwrite all section names with random ASCII
        /// </summary>
        public static void OverwriteSectionNames()
        {
            if (_moduleBase == IntPtr.Zero || _moduleSize == 0) return;
            try
            {
                // Read PE offset at 0x3C (e_lfanew)
                int peOffset = ReadInt32At(_moduleBase, 0x3C);
                if (peOffset <= 0 || peOffset > _moduleSize - 4) return;

                IntPtr peHeader = IntPtr.Add(_moduleBase, peOffset);
                // PE signature (4 bytes) + COFF header (20 bytes); section table starts at peHeader+24
                short numSections = ReadInt16At(peHeader, 6);  // NumberOfSections
                short optionalHeaderSize = ReadInt16At(peHeader, 20); // SizeOfOptionalHeader

                int sectionTableOffset = 24 + optionalHeaderSize;  // Offset relative to PE signature
                int sectionEntrySize = 40;  // IMAGE_SECTION_HEADER size

                Random rng = new Random();

                for (int i = 0; i < numSections; i++)
                {
                    int sectionNameOffset = peOffset + sectionTableOffset + i * sectionEntrySize;
                    IntPtr sectionNamePtr = IntPtr.Add(_moduleBase, sectionNameOffset);

                    uint oldProtect;
                    VirtualProtect(sectionNamePtr, (UIntPtr)8, PAGE_READWRITE, out oldProtect);

                    // Section name (8 bytes) -> random ASCII
                    byte[] randomName = new byte[8];
                    rng.NextBytes(randomName);
                    for (int j = 0; j < 8; j++)
                    {
                        randomName[j] = (byte)((randomName[j] % 26) + 65); // 'A'-'Z'
                    }
                    Marshal.Copy(randomName, 0, sectionNamePtr, 8);

                    VirtualProtect(sectionNamePtr, (UIntPtr)8, oldProtect, out _);
                }
            }
            catch { }
        }

        /// <summary>
        /// Overwrite the import directory table entry
        /// </summary>
        public static void OverwriteImportDirectory()
        {
            if (_moduleBase == IntPtr.Zero || _moduleSize == 0) return;
            try
            {
                int peOffset = ReadInt32At(_moduleBase, 0x3C);
                if (peOffset <= 0 || peOffset > _moduleSize - 100) return;

                IntPtr peHeader = IntPtr.Add(_moduleBase, peOffset);
                short optionalHeaderSize = ReadInt16At(peHeader, 20);

                // For .NET PE32+ optional header, import directory RVA is at PE sig + 24 + 104 (DataDirectory[1])
                int importDirOffset = peOffset + 24 + 104; // Simplified: regular position

                // In PE32+, optional header Magic=0x20B at COFF offset 0
                short magic = ReadInt16At(peHeader, 24);
                if (magic == 0x20B) // PE32+
                {
                    importDirOffset = peOffset + 24 + 112; // Slightly different offset
                }

                // DataDirectory[1] = ImportTable, 8 bytes (RVA + Size)
                IntPtr importDirPtr = IntPtr.Add(_moduleBase, importDirOffset);

                uint oldProtect;
                VirtualProtect(importDirPtr, (UIntPtr)8, PAGE_READWRITE, out oldProtect);

                // Zero out import table RVA and Size
                byte[] zero = new byte[8];
                Marshal.Copy(zero, 0, importDirPtr, 8);

                VirtualProtect(importDirPtr, (UIntPtr)8, oldProtect, out _);
            }
            catch { }
        }

        /// <summary>
        /// Scan and fix all RWX memory pages to RX (for MonoMod trampolines)
        /// </summary>
        public static void FixRWXPages()
        {
            try
            {
                IntPtr address = IntPtr.Zero;
                UIntPtr querySize = (UIntPtr)Marshal.SizeOf<MEMORY_BASIC_INFORMATION>();

                while (true)
                {
                    MEMORY_BASIC_INFORMATION mbi;
                    int result = VirtualQuery(address, out mbi, querySize);
                    if (result == 0) break;

                    // RWX page + committed + private memory (MonoMod trampoline signature)
                    if (mbi.Protect == PAGE_EXECUTE_READWRITE &&
                        mbi.State == MEM_COMMIT &&
                        mbi.Type == 0x20000) // MEM_PRIVATE
                    {
                        // Remove write permission: RWX -> RX
                        uint oldProtect;
                        VirtualProtect(mbi.BaseAddress, mbi.RegionSize, PAGE_EXECUTE_READ, out oldProtect);
                    }

                    // Move to next region
                    long nextAddr = mbi.BaseAddress.ToInt64() + (long)mbi.RegionSize;
                    if (nextAddr > (IntPtr.Size == 8 ? 0x7FFFFFFFFFFF : int.MaxValue)) break;
                    if (nextAddr < 0) break;
                    address = new IntPtr(nextAddr);
                }
            }
            catch { }
        }

        /// <summary>
        /// Allocate random-size junk memory to dilute memory signature density
        /// </summary>
        public static void AllocateJunkMemory()
        {
            try
            {
                Random rng = new Random();
                int junkCount = rng.Next(20, 50); // 20-50 random memory blocks
                for (int i = 0; i < junkCount; i++)
                {
                    // Random size: 4KB - 64KB
                    uint size = (uint)(rng.Next(4096, 65536));
                    IntPtr mem = VirtualAlloc(IntPtr.Zero, (UIntPtr)size, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
                    if (mem != IntPtr.Zero)
                    {
                        // Write random data to make it look like legitimate memory
                        byte[] junk = new byte[size];
                        rng.NextBytes(junk);
                        // Insert random bytes that look like valid Unicode strings
                        for (int j = 0; j < size - 8; j += rng.Next(16, 128))
                        {
                            junk[j] = (byte)rng.Next(32, 127);
                        }
                        Marshal.Copy(junk, 0, mem, (int)Math.Min(size, int.MaxValue));
                        _junkAllocations.Add(mem);
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// Free all junk memory allocations
        /// </summary>
        public static void Cleanup()
        {
            foreach (var ptr in _junkAllocations)
            {
                try { VirtualFree(ptr, UIntPtr.Zero, MEM_RELEASE); } catch { }
            }
            _junkAllocations.Clear();
        }

        #region Helpers
        private static int ReadInt32At(IntPtr baseAddr, int offset)
        {
            try
            {
                byte[] buffer = new byte[4];
                Marshal.Copy(IntPtr.Add(baseAddr, offset), buffer, 0, 4);
                return BitConverter.ToInt32(buffer, 0);
            }
            catch { return 0; }
        }

        private static short ReadInt16At(IntPtr baseAddr, int offset)
        {
            try
            {
                byte[] buffer = new byte[2];
                Marshal.Copy(IntPtr.Add(baseAddr, offset), buffer, 0, 2);
                return BitConverter.ToInt16(buffer, 0);
            }
            catch { return 0; }
        }
        #endregion
    }
}
