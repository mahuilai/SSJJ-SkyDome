using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using DotNetDetour;

public class MethodHook
{
    [CompilerGenerated]
    private bool _isHooked;

    public bool isHooked
    {
        [CompilerGenerated]
        get
        {
            return this._isHooked;
        }
        [CompilerGenerated]
        private set
        {
            this._isHooked = value;
        }
    }

    static MethodHook()
    {
        byte[] array = new byte[14];
        array[0] = byte.MaxValue;
        array[1] = 37;
        MethodHook.s_jmpBuff_64 = array;
        MethodHook.s_jmpBuff_arm32_arm = new byte[] { 4, 240, 31, 229, 0, 0, 0, 0 };
        MethodHook.s_jmpBuff_arm32_thumb = new byte[]
        {
            0, 181, 16, 180, 3, 180, 120, 70, 22, 48,
            0, 104, 105, 70, 8, 49, 8, 96, 121, 70,
            14, 49, 142, 70, 1, 188, 2, 188, 0, 189,
            192, 70, 0, 0, 0, 0, 0, 189
        };
        MethodHook.s_jmpBuff_arm64 = new byte[]
        {
            4, 240, 31, 229, 0, 0, 0, 0, 0, 0,
            0, 0
        };

        if (LDasm.IsAndroidARM())
        {
            MethodHook.s_addrOffset = 4;
            if (IntPtr.Size == 4)
            {
                MethodHook.s_jmpBuff = MethodHook.s_jmpBuff_arm32_arm;
                return;
            }
            MethodHook.s_jmpBuff = MethodHook.s_jmpBuff_arm64;
            return;
        }
        else
        {
            if (IntPtr.Size == 4)
            {
                MethodHook.s_jmpBuff = MethodHook.s_jmpBuff_32;
                MethodHook.s_addrOffset = 1;
                return;
            }
            MethodHook.s_jmpBuff = MethodHook.s_jmpBuff_64;
            MethodHook.s_addrOffset = 6;
            return;
        }
    }

    // Generates a delegate that calls the original (unhooked) method.
    // It creates a trampoline by copying the original bytes and appending a jump
    // back to the original method after the overwritten region.
    public TDelegate GenerateTrampoline<TDelegate>() where TDelegate : class
    {
        // Ensure we have saved the original bytes.
        if (_proxyBuff == null || _proxyBuff.Length == 0)
            throw new InvalidOperationException("Proxy buffer not initialized; cannot generate trampoline.");

        int totalSize = _proxyBuff.Length + s_jmpBuff.Length;
        IntPtr trampoline = Marshal.AllocHGlobal(totalSize);
        unsafe
        {
            byte* dst = (byte*)trampoline.ToPointer();
            // Copy original bytes (the stolen instructions).
            for (int i = 0; i < _proxyBuff.Length; i++)
            {
                dst[i] = _proxyBuff[i];
            }
            // Copy jump buffer template.
            for (int i = 0; i < s_jmpBuff.Length; i++)
            {
                dst[_proxyBuff.Length + i] = s_jmpBuff[i];
            }
            // Patch the jump address to point to the continuation of the original method.
            byte* addrPtr = dst + _proxyBuff.Length + s_addrOffset;
            if (IntPtr.Size == 4)
            {
                *((int*)addrPtr) = _targetPtr.ToInt32() + _proxyBuff.Length;
            }
            else
            {
                *((long*)addrPtr) = _targetPtr.ToInt64() + _proxyBuff.Length;
            }
        }
        // Create a delegate that points to the trampoline.
        return Marshal.GetDelegateForFunctionPointer<TDelegate>(trampoline);
    }
    public MethodHook(MethodBase targetMethod, MethodBase replacementMethod, MethodBase proxyMethod = null)
    {
        this._targetMethod = targetMethod;
        this._replacementMethod = replacementMethod;
        this._proxyMethod = proxyMethod;
        this._targetPtr = this.GetFunctionAddr(this._targetMethod);
        this._replacementPtr = this.GetFunctionAddr(this._replacementMethod);
        if (proxyMethod != null)
        {
            this._proxyPtr = this.GetFunctionAddr(this._proxyMethod);
        }
        this._jmpBuff = new byte[MethodHook.s_jmpBuff.Length];
    }

    public void Install()
    {
        if (LDasm.IsiOS())
        {
            return;
        }
        if (this.isHooked)
        {
            return;
        }
        HookRegistry.AddHooker(this._targetMethod, this);
        this.InitProxyBuff();
        this.BackupHeader();
        this.PatchTargetMethod();
        this.PatchProxyMethod();
        this.isHooked = true;
    }

    public unsafe void Uninstall()
    {
        if (!this.isHooked)
        {
            return;
        }
        byte* ptr = (byte*)this._targetPtr.ToPointer();
        for (int i = 0; i < this._proxyBuff.Length; i++)
        {
            *(ptr++) = this._proxyBuff[i];
        }
        this.isHooked = false;
        HookRegistry.RemoveHooker(this._targetMethod);
    }

    private unsafe void InitProxyBuff()
    {
        byte* ptr = (byte*)this._targetPtr.ToPointer();
        uint num = LDasm.SizeofMinNumByte((void*)ptr, MethodHook.s_jmpBuff.Length);
        this._proxyBuff = new byte[num];
        this.EnableAddrModifiable(this._targetPtr, num);
    }

    private unsafe void BackupHeader()
    {
        byte* ptr = (byte*)this._targetPtr.ToPointer();
        for (int i = 0; i < this._proxyBuff.Length; i++)
        {
            this._proxyBuff[i] = *(ptr++);
        }
    }

    private unsafe void PatchTargetMethod()
    {
        Array.Copy(MethodHook.s_jmpBuff, this._jmpBuff, this._jmpBuff.Length);
        fixed (byte* ptr = &this._jmpBuff[MethodHook.s_addrOffset])
        {
            byte* ptr2 = ptr;
            if (IntPtr.Size == 4)
            {
                *(int*)ptr2 = this._replacementPtr.ToInt32();
            }
            else
            {
                *(long*)ptr2 = this._replacementPtr.ToInt64();
            }
        }
        byte* ptr3 = (byte*)this._targetPtr.ToPointer();
        if (ptr3 != null)
        {
            int i = 0;
            int num = this._jmpBuff.Length;
            while (i < num)
            {
                *(ptr3++) = this._jmpBuff[i];
                i++;
            }
        }
    }

    private unsafe void PatchProxyMethod()
    {
        if (this._proxyPtr == IntPtr.Zero)
        {
            return;
        }
        this.EnableAddrModifiable(this._proxyPtr, (uint)this._proxyBuff.Length);
        byte* ptr = (byte*)this._proxyPtr.ToPointer();

        for (int i = 0; i < this._proxyBuff.Length; i++)
        {
            *(ptr++) = this._proxyBuff[i];
        }

        fixed (byte* ptr2 = &this._jmpBuff[MethodHook.s_addrOffset])
        {
            byte* ptr3 = ptr2;
            if (IntPtr.Size == 4)
            {
                int targetAddr = this._targetPtr.ToInt32();
                int offset = this._proxyBuff.Length;
                *(int*)ptr3 = targetAddr + offset;
            }
            else
            {
                long targetAddr = this._targetPtr.ToInt64();
                long offset = (long)this._proxyBuff.Length;
                *(long*)ptr3 = targetAddr + offset;
            }
        }

        for (int j = 0; j < this._jmpBuff.Length; j++)
        {
            *(ptr++) = this._jmpBuff[j];
        }
    }

    private void EnableAddrModifiable(IntPtr ptr, uint size)
    {
        if (!LDasm.IsIL2CPP())
        {
            return;
        }
        uint num;
        IL2CPPHelper.VirtualProtect(ptr, size, IL2CPPHelper.Protection.PAGE_EXECUTE_READWRITE, out num);
    }

    private unsafe IntPtr GetFunctionAddr(MethodBase method)
    {
        if (!LDasm.IsIL2CPP())
        {
            return method.MethodHandle.GetFunctionPointer();
        }

        MethodHook.__ForCopy forCopy = new MethodHook.__ForCopy();
        forCopy.method = method;

        long* ptr = &forCopy.__dummy;
        ptr++;

        IntPtr zero = IntPtr.Zero;

        if (sizeof(IntPtr) == 8)
        {
            long ptrValue = *ptr;
            long* secondPtr = (long*)(ptrValue + sizeof(IntPtr) * 2);
            long finalAddr = *secondPtr;
            zero = new IntPtr(finalAddr);
        }
        else
        {
            int ptrValue = *(int*)ptr;
            int* secondPtr = (int*)(ptrValue + sizeof(IntPtr) * 2);
            int finalAddr = *secondPtr;
            zero = new IntPtr(finalAddr);
        }
        return zero;
    }

    private MethodBase _targetMethod;
    private MethodBase _replacementMethod;
    private MethodBase _proxyMethod;
    private IntPtr _targetPtr;
    private IntPtr _replacementPtr;
    private IntPtr _proxyPtr;

    private static readonly byte[] s_jmpBuff;
    private static readonly byte[] s_jmpBuff_32 = new byte[] { 104, 0, 0, 0, 0, 195 };
    private static readonly byte[] s_jmpBuff_64;
    private static readonly byte[] s_jmpBuff_arm32_arm;
    private static readonly byte[] s_jmpBuff_arm32_thumb;
    private static readonly byte[] s_jmpBuff_arm64;
    private static readonly int s_addrOffset;

    private byte[] _jmpBuff;
    private byte[] _proxyBuff;

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct __ForCopy
    {
        public long __dummy;
        public MethodBase method;
    }
}
