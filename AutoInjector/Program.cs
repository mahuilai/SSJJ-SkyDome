using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using SharpMonoInjector;

namespace AutoInjector
{
    internal class Program
    {
        private const string GAME_PROCESS = "SSJJ_BattleClient_Unity";
        private static string _skyDomePath;
        private static DateTime _lastInjectTime = DateTime.MinValue;
        private static readonly List<int> _injectedPids = new List<int>();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        private const int SW_HIDE = 0;

        private static bool _stealthMode = false;

        private static void Main(string[] args)
        {
            Console.Title = "SkyDome Injector";
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(@"
  ___  _  ___   ___  ___
 / __|/_\|   \ / _ \|   \    SkyDome Auto Injector
 \__ \ _ \ |) | (_) | |) |   Built on SharpMonoInjector 2.7
 |___/_/ \_\___/\___/|___/    TheHolyOneZ Edition
");
            Console.ResetColor();

            _stealthMode = args.Any(a => a.Equals("-stealth", StringComparison.OrdinalIgnoreCase) ||
                                         a.Equals("-s", StringComparison.OrdinalIgnoreCase));
            bool noWait = args.Any(a => a.Equals("-nowait", StringComparison.OrdinalIgnoreCase) ||
                                        a.Equals("-nw", StringComparison.OrdinalIgnoreCase));

            _skyDomePath = LocateSkyDomeDll();
            if (_skyDomePath == null)
            {
                WriteError("未找到 SkyDome.dll！请将本程序放在 SkyDome.dll 同目录下运行。");
                Pause();
                return;
            }
            WriteInfo("SkyDome.dll 路径: " + _skyDomePath);

            if (Debugger.IsAttached)
                WriteWarn("警告：检测到调试器！某些反作弊可能会阻止注入。");

            string gameDir = DetectGameDirectory();
            if (gameDir != null)
                WriteInfo("检测到游戏目录: " + gameDir);

            if (_stealthMode)
            {
                EnableStealthMode();
                WriteInfo("Stealth 模式已启用（窗口已隐藏）");
            }

            WriteInfo("正在等待游戏进程 " + GAME_PROCESS + " ...");

            while (true)
            {
                try
                {
                    Process[] processes = Process.GetProcessesByName(GAME_PROCESS);

                    foreach (var process in processes)
                    {
                        try
                        {
                            int pid = process.Id;
                            if (_injectedPids.Contains(pid))
                                continue;

                            if (!IsMonoProcess(process))
                            {
                                WriteInfo("进程 " + pid + " 已启动但 Mono 未就绪，等待中...");
                                continue;
                            }

                            WriteInfo("检测到游戏进程 PID: " + pid + "，准备注入...");
                            Thread.Sleep(800);

                            InjectDll(pid);

                            _injectedPids.Add(pid);
                            _lastInjectTime = DateTime.Now;

                            WriteSuccess("注入成功！PID: " + pid);

                            if (noWait)
                            {
                                WriteInfo("no-wait 模式，5秒后退出...");
                                Thread.Sleep(5000);
                                return;
                            }
                        }
                        catch (InvalidOperationException) { }
                        catch (Exception ex)
                        {
                            WriteError("注入异常: " + ex.Message);
                        }
                    }

                    if (processes.Length == 0 && _injectedPids.Count > 0)
                    {
                        _injectedPids.RemoveAll(pid =>
                        {
                            try { Process.GetProcessById(pid); return false; }
                            catch { return true; }
                        });
                    }
                }
                catch (Exception ex)
                {
                    WriteError("监控异常: " + ex.Message);
                }

                Thread.Sleep(2000);
            }
        }

        private static void EnableStealthMode()
        {
            IntPtr hWnd = GetConsoleWindow();
            if (hWnd != IntPtr.Zero)
                ShowWindow(hWnd, SW_HIDE);
        }

        private static string LocateSkyDomeDll()
        {
            string dir = AppDomain.CurrentDomain.BaseDirectory;
            string[] searchPaths = {
                Path.Combine(dir, "SkyDome.dll"),
                Path.Combine(dir, "bin\\Release\\SkyDome.dll"),
                Path.Combine(dir, "..\\bin\\Release\\SkyDome.dll"),
                @"D:\项目\阳翳\SkyDome杜海鹏内部开源\bin\Release\SkyDome.dll",
            };

            foreach (var path in searchPaths)
            {
                string full = Path.GetFullPath(path);
                if (File.Exists(full))
                    return full;
            }
            return null;
        }

        private static string DetectGameDirectory()
        {
            string[] commonPaths = {
                @"D:\SSJJ-4399\battle\11_64",
            };
            foreach (var path in commonPaths)
            {
                if (Directory.Exists(path) &&
                    File.Exists(Path.Combine(path, GAME_PROCESS + ".exe")))
                    return path;
            }
            return null;
        }

        private static bool IsMonoProcess(Process process)
        {
            try
            {
                IntPtr hProcess = Native.OpenProcess(
                    ProcessAccessRights.PROCESS_QUERY_INFORMATION | ProcessAccessRights.PROCESS_VM_READ,
                    false, process.Id);
                try
                {
                    if (hProcess == IntPtr.Zero)
                        return false;
                    return ProcessUtils.GetMonoModule(hProcess, out _);
                }
                finally
                {
                    if (hProcess != IntPtr.Zero)
                        Native.CloseHandle(hProcess);
                }
            }
            catch
            {
                return false;
            }
        }

        private static void InjectDll(int pid)
        {
            byte[] assemblyBytes = File.ReadAllBytes(_skyDomePath);

            var options = new InjectionOptions
            {
                RandomizeMemory = true,
                HideThreads = true,
                DelayExecution = false,
                DelayMs = 100,
            };

            using (var injector = new Injector(pid))
            {
                injector.Options = options;
                PerformAntiDebugEvasion(pid);
                WriteInfo("  命名空间: SkyDome, 类: SceneInitializer, 方法: Load");
                injector.Inject(assemblyBytes, "SkyDome", "SceneInitializer", "Load");
            }
        }

        private static void PerformAntiDebugEvasion(int pid)
        {
            try
            {
                IntPtr hProcess = Native.OpenProcess(
                    ProcessAccessRights.PROCESS_QUERY_INFORMATION | ProcessAccessRights.PROCESS_SET_INFORMATION,
                    false, pid);
                try
                {
                    if (hProcess == IntPtr.Zero)
                        return;

                    bool isDebugged = false;
                    Native.CheckRemoteDebuggerPresent(hProcess, ref isDebugged);
                    if (isDebugged)
                        WriteWarn("  目标进程已被调试器附加！");
                }
                finally
                {
                    if (hProcess != IntPtr.Zero)
                        Native.CloseHandle(hProcess);
                }
            }
            catch { }
        }

        private static void WriteInfo(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("[*] " + msg);
            Console.ResetColor();
        }

        private static void WriteSuccess(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[+] " + msg);
            Console.ResetColor();
        }

        private static void WriteError(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[!] " + msg);
            Console.ResetColor();
        }

        private static void WriteWarn(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("[!] " + msg);
            Console.ResetColor();
        }

        private static void Pause()
        {
            Console.WriteLine();
            Console.Write("按任意键退出...");
            Console.ReadKey();
        }
    }
}
