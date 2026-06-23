using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using SkyDome.RuntimeDetour;
using SkyDome.Utilities;
using UnityEngine;

namespace SkyDome
{
	public class SceneInitializer : MonoBehaviour
	{
		private static string _crashLogPath = null;

		static SceneInitializer()
		{
			RegisterPitchSynchronizer();
			try
			{
				string logDir = Path.Combine(Application.dataPath, "..", "log");
				Directory.CreateDirectory(logDir);
				_crashLogPath = Path.Combine(logDir, "skydome_init.log");
				File.WriteAllText(_crashLogPath, DateTime.Now.ToString("HH:mm:ss.fff") + " [SkyDome] 静态构造函数\n");
			}
			catch { }
		}

		public static void CrashLog(string message)
		{
			try
			{
				string line = DateTime.Now.ToString("HH:mm:ss.fff") + " " + message + "\n";
				if (_crashLogPath != null)
					File.AppendAllText(_crashLogPath, line);
			}
			catch { }
		}

		public static void SafeLog(string message)
		{
			try
			{
				Debug.Log(message);
			}
			catch
			{
				try
				{
					Console.WriteLine("[INFO] " + message);
				}
				catch {}
			}
		}

		public static void SafeLogError(string message)
		{
			try
			{
				Debug.LogError(message);
			}
			catch
			{
				try
				{
					Console.WriteLine("[ERROR] " + message);
				}
				catch {}
			}
		}

		public static void RegisterPitchSynchronizer()
		{
			AppDomain.CurrentDomain.AssemblyResolve += ResolveHelper;
		}

		private static Assembly ResolveHelper(object sender, ResolveEventArgs args)
		{
			try
			{
				string shortName = new AssemblyName(args.Name).Name;

				// 1. 安全沙箱：只允许我们自己的程序集触发对重命名程序集的解析
				// 防止游戏反作弊（如 RuntimeCheatGuard）通过 Type.GetType 或 Assembly.Load 主动查询敏感类型
				bool isAllowed = false;
				Assembly requestingAssembly = args.RequestingAssembly;
				if (requestingAssembly != null)
				{
					string reqName = requestingAssembly.GetName().Name;
					if (reqName.IndexOf("SkyDome", StringComparison.OrdinalIgnoreCase) >= 0 ||
						reqName.IndexOf("SkyCore", StringComparison.OrdinalIgnoreCase) >= 0)
					{
						isAllowed = true;
					}
				}

				if (!isAllowed)
				{
					// 兜底方案：如果是通过 Reflection/Type.GetType 调用，RequestingAssembly 可能为空
					// 此时我们通过 StackTrace 检查调用栈中是否存在我们自己的程序集代码
					var stack = new System.Diagnostics.StackTrace();
					for (int i = 0; i < stack.FrameCount; i++)
					{
						var frame = stack.GetFrame(i);
						var method = frame.GetMethod();
						if (method != null && method.DeclaringType != null)
						{
							string asmName = method.DeclaringType.Assembly.GetName().Name;
							if (asmName.IndexOf("SkyDome", StringComparison.OrdinalIgnoreCase) >= 0 ||
								asmName.IndexOf("SkyCore", StringComparison.OrdinalIgnoreCase) >= 0)
							{
								isAllowed = true;
								break;
							}
						}
					}
				}

				// 如果不是我们自己的代码触发的，拒绝解析并返回 null，向反作弊隐瞒程序集的存在
				if (!isAllowed && (shortName.Contains("MonoMod") || shortName.Contains("SkyCore") || shortName.Contains("SkyDome") || shortName.Contains("Cecil") || shortName.Contains("DotNetDetour")))
				{
					SafeLog("[加载器] 拒绝了非授信程序集对敏感类型 " + shortName + " 的 AssemblyResolve 解析请求");
					return null;
				}

				// 2. 映射重命名的程序集名称，用于从已加载的程序集中查找
				string targetAssemblyName = shortName;
				if (shortName == "MonoMod.Utils")
				{
					targetAssemblyName = "SkyDome.Utils";
				}
				else if (shortName == "MonoMod.RuntimeDetour")
				{
					targetAssemblyName = "SkyDome.CoreRuntime";
				}

				foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
				{
					if (assembly.GetName().Name == targetAssemblyName)
					{
						return assembly;
					}
				}

				// 3. 映射重命名的程序集资源文件名，用于从嵌入资源中加载
				string resourceSearchKey = shortName;
				if (shortName == "MonoMod.Utils" || shortName == "SkyDome.Utils")
				{
					resourceSearchKey = "SkyCoreUtils";
				}
				else if (shortName == "MonoMod.RuntimeDetour" || shortName == "SkyDome.CoreRuntime")
				{
					resourceSearchKey = "SkyCore";
				}

				Assembly executingAssembly = Assembly.GetExecutingAssembly();
				foreach (string resourceName in executingAssembly.GetManifestResourceNames())
				{
					if (resourceName.EndsWith(".dll") && resourceName.IndexOf(resourceSearchKey, StringComparison.OrdinalIgnoreCase) >= 0)
					{
						using (Stream stream = executingAssembly.GetManifestResourceStream(resourceName))
						{
							if (stream != null)
							{
								byte[] array = new byte[stream.Length];
								stream.Read(array, 0, array.Length);
								return Assembly.Load(array);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				SafeLogError("[加载器] AssemblyResolve 错误: " + ex.ToString());
			}
			return null;
		}

		public static void Unload()
		{
			if (SceneInitializer._hookObject == null)
			{
				return;
			}
			try
			{
				global::UnityEngine.Object.DestroyImmediate(SceneInitializer._hookObject);
				SceneInitializer._hookObject = null;
				SafeLog("[Loader] SkyDome unloaded");
			}
			catch (Exception ex)
			{
				SafeLogError("[加载器] 卸载失败: " + ex.ToString());
			}
		}

        public static void Load()
		{
			RegisterPitchSynchronizer();
			LoadInternal();
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static void LoadInternal()
		{
			if (SceneInitializer._hookObject != null)
			{
				return;
			}
			try
			{
					CrashLog("[步骤1/5] 开始安装 GetAssembliesBypass...");
					SafeLog("[加载器] [步骤1/5] 开始安装 GetAssembliesBypass...");
				try
				{
					GetAssembliesBypass.Install();
					SafeLog("[加载器] [步骤1/5] GetAssembliesBypass 安装完成");
				}
				catch (Exception ex)
				{
					CrashLog("[步骤1/5] GetAssembliesBypass 异常: " + ex.ToString());
					SafeLogError("[加载器] [步骤1/5] GetAssembliesBypass 安装异常: " + ex.ToString());
					throw;
				}

				CrashLog("[步骤1b/5] 开始安装 MonoBypass...");
				SafeLog("[加载器] [步骤1b/5] 开始安装 MonoBypass...");
				try
				{
					MonoBypass.Install();
					SafeLog("[加载器] [步骤1b/5] MonoBypass 安装完成");
				}
				catch (Exception ex)
				{
					CrashLog("[步骤1b/5] MonoBypass 异常: " + ex.ToString());
					SafeLogError("[加载器] [步骤1b/5] MonoBypass 安装异常: " + ex.ToString());
					// 非致命，继续执行
				}

				CrashLog("[步骤2/5] 打印已加载程序集...");
				SafeLog("[加载器] [步骤2/5] 打印已加载程序集...");
				try
				{
					var assemblies = AppDomain.CurrentDomain.GetAssemblies();
					SafeLog(string.Format("[加载器] 注入时已加载程序集数: {0}", assemblies.Length));
					foreach (var asm in assemblies)
					{
						if (asm == null) continue;
						string name = asm.FullName;
						string loc = "";
						try { loc = asm.Location; } catch {}
						SafeLog(string.Format("  -> {0} | {1}", name, loc));
					}
				}
				catch (Exception ex)
				{
					SafeLogError("[加载器] 打印已加载程序集失败: " + ex.Message);
				}

				CrashLog("[步骤3/5] 加载嵌入程序集...");
				SafeLog("[加载器] [步骤3/5] 加载嵌入程序集...");
				Main.LoadAllAssembly();
				SafeLog("[加载器] [步骤3/5] 嵌入程序集加载完成");

				CrashLog("[步骤4/5] 创建 SkyDome_HookObject...");
				SafeLog("[加载器] [步骤4/5] 创建 SkyDome_HookObject...");
				SceneInitializer._hookObject = new GameObject("SkyDome_HookObject");
				if (SceneInitializer._hookObject.AddComponent<Main>() == null)
				{
					throw new MissingComponentException("Failed to add Main component to hook object");
				}
				global::UnityEngine.Object.DontDestroyOnLoad(SceneInitializer._hookObject);
				CrashLog("[步骤4/5] SkyDome_HookObject 创建完成");
				SafeLog("[加载器] [步骤4/5] SkyDome_HookObject 创建完成，Main.Awake 即将返回...");

				// 反检测措施 (由于可能会导致 Mono JIT 内存页属性冲突和 PE 头部读取失败导致闪退，暂时禁用)
				// SkyDome.Utilities.RuntimeProtection.Initialize();

				SafeLog(string.Format("[加载器] [步骤5/5] SkyDome 初始化成功，ID: {0}", SceneInitializer._hookObject.GetInstanceID()));
			}
			catch (Exception ex)
			{
				if (SceneInitializer._hookObject != null)
				{
					try
					{
						global::UnityEngine.Object.DestroyImmediate(SceneInitializer._hookObject);
					}
					catch {}
					SceneInitializer._hookObject = null;
				}
				SafeLogError(string.Format("[加载器] 初始化失? {0}", ex));
			}
		}

        private static GameObject _hookObject;
	}

	public static class GetAssembliesBypass
	{
		// 使用 NativeDetour 而非 Hook，因为 AppDomain.GetAssemblies 是 Mono icall（无 IL body）
		// NativeDetour 在原生代码层面工作，无需 IL body
		private static NativeDetour hook_GetAssemblies;
		public delegate Assembly[] GetAssembliesDelegate(AppDomain self);
		private static GetAssembliesDelegate original_GetAssemblies;

		private static NativeDetour hook_GetAssemblies2;
		public delegate Assembly[] GetAssemblies2Delegate(AppDomain self, bool refOnly);
		private static GetAssemblies2Delegate original_GetAssemblies2;
		private static MethodInfo method_GetAssemblies2;

		// 新增：挂钩 Assembly.GetTypes() 和 Module.GetTypes()，防止 RuntimeCheatGuard 枚举我们的类型
		private static MethodHook hook_AssemblyGetTypes;
		public delegate Type[] AssemblyGetTypesDelegate(Assembly self);
		private static AssemblyGetTypesDelegate original_AssemblyGetTypes;

		private static MethodHook hook_ModuleGetTypes;
		public delegate Type[] ModuleGetTypesDelegate(Module self);
		private static ModuleGetTypesDelegate original_ModuleGetTypes;

		private static bool IsSensitiveType(Type t)
		{
			if (t == null) return false;
			try
			{
				string fullName = t.FullName;
				if (!string.IsNullOrEmpty(fullName))
				{
					if (fullName.IndexOf("SkyDome",      StringComparison.OrdinalIgnoreCase) >= 0 ||
					    fullName.IndexOf("SkyCore",      StringComparison.OrdinalIgnoreCase) >= 0 ||
					    fullName.IndexOf("MonoMod",      StringComparison.OrdinalIgnoreCase) >= 0 ||
					    fullName.IndexOf("Cecil",        StringComparison.OrdinalIgnoreCase) >= 0 ||
					    fullName.IndexOf("DotNetDetour", StringComparison.OrdinalIgnoreCase) >= 0)
						return true;
				}
				string asmName = t.Assembly.GetName().Name;
				if (!string.IsNullOrEmpty(asmName))
				{
					if (asmName.IndexOf("SkyDome",      StringComparison.OrdinalIgnoreCase) >= 0 ||
					    asmName.IndexOf("SkyCore",      StringComparison.OrdinalIgnoreCase) >= 0 ||
					    asmName.IndexOf("MonoMod",      StringComparison.OrdinalIgnoreCase) >= 0 ||
					    asmName.IndexOf("Cecil",        StringComparison.OrdinalIgnoreCase) >= 0 ||
					    asmName.IndexOf("DotNetDetour", StringComparison.OrdinalIgnoreCase) >= 0)
						return true;
				}
			}
			catch { return true; }
			return false;
		}

		private static Type[] FilterTypes(Type[] types)
		{
			if (types == null) return null;
			var filtered = new System.Collections.Generic.List<Type>(types.Length);
			foreach (Type t in types)
			{
				if (t == null) continue;
				if (!IsSensitiveType(t))
					filtered.Add(t);
			}
			return filtered.ToArray();
		}

		public static Type[] hk_AssemblyGetTypes(Assembly self)
		{
			Type[] types = null;
			if (original_AssemblyGetTypes != null)
			{
				try { types = original_AssemblyGetTypes(self); }
				catch { }
			}
			if (types == null && hook_AssemblyGetTypes != null)
			{
				lock (hook_AssemblyGetTypes)
				{
					hook_AssemblyGetTypes.Undo();
					try { types = self.GetTypes(); }
					finally { hook_AssemblyGetTypes.Apply(); }
				}
			}
			return FilterTypes(types);
		}

		public static Type[] hk_ModuleGetTypes(Module self)
		{
			Type[] types = null;
			if (original_ModuleGetTypes != null)
			{
				try { types = original_ModuleGetTypes(self); }
				catch { }
			}
			if (types == null && hook_ModuleGetTypes != null)
			{
				lock (hook_ModuleGetTypes)
				{
					hook_ModuleGetTypes.Undo();
					try { types = self.GetTypes(); }
					finally { hook_ModuleGetTypes.Apply(); }
				}
			}
			return FilterTypes(types);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static void Install()
		{
			BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			BindingFlags staticPublic = BindingFlags.Static | BindingFlags.Public;
			bool anyOk = false;

			// 挂钩 1: GetAssemblies() — 每个 hook 独立 try-catch
			try
			{
				var m1 = typeof(AppDomain).GetMethod("GetAssemblies", flags, null, new Type[0], null);
				if (m1 != null)
				{
					var hk1 = typeof(GetAssembliesBypass).GetMethod("hk_GetAssemblies", staticPublic);
					RuntimeHelpers.PrepareMethod(hk1.MethodHandle);
					hook_GetAssemblies = new NativeDetour(m1, hk1);
					try { original_GetAssemblies = hook_GetAssemblies.GenerateTrampoline<GetAssembliesDelegate>(); }
					catch { /* 兜底路径：hk_GetAssemblies 会用 Undo/Redo */ }
					anyOk = true;
				}
			}
			catch (Exception ex)
			{
				SceneInitializer.SafeLogError("[加载器] GetAssemblies() NativeDetour 失败: " + ex.Message);
			}

			// 挂钩 2: GetAssemblies(bool)
			try
			{
				method_GetAssemblies2 = typeof(AppDomain).GetMethod("GetAssemblies", flags, null, new Type[] { typeof(bool) }, null);
				if (method_GetAssemblies2 != null)
				{
					var hk2 = typeof(GetAssembliesBypass).GetMethod("hk_GetAssemblies2", staticPublic);
					RuntimeHelpers.PrepareMethod(hk2.MethodHandle);
					hook_GetAssemblies2 = new NativeDetour(method_GetAssemblies2, hk2);
					try { original_GetAssemblies2 = hook_GetAssemblies2.GenerateTrampoline<GetAssemblies2Delegate>(); }
					catch { /* 兜底路径：hk_GetAssemblies2 会用 Undo/Redo */ }
					anyOk = true;
				}
			}
			catch (Exception ex)
			{
				SceneInitializer.SafeLogError("[加载器] GetAssemblies(bool) NativeDetour 失败: " + ex.Message);
			}

			// 挂钩 3: Assembly.GetTypes() — 防止反作弊通过枚举类型检测我们
			try
			{
				var m3 = typeof(Assembly).GetMethod("GetTypes", flags, null, new Type[0], null);
				if (m3 != null)
				{
					var hk3 = typeof(GetAssembliesBypass).GetMethod("hk_AssemblyGetTypes", staticPublic);
					RuntimeHelpers.PrepareMethod(hk3.MethodHandle);
					hook_AssemblyGetTypes = new MethodHook(m3, hk3);
					try { original_AssemblyGetTypes = hook_AssemblyGetTypes.GenerateTrampoline<AssemblyGetTypesDelegate>(); }
					catch { }
					anyOk = true;
					SceneInitializer.CrashLog("[Bypass] Assembly.GetTypes 钩子安装成功");
				}
			}
			catch (Exception ex)
			{
				SceneInitializer.SafeLogError("[加载器] Assembly.GetTypes() MethodHook 失败: " + ex.Message);
			}

			// 挂钩 4: Module.GetTypes() — 防止反作弊通过模块枚举类型检测我们
			try
			{
				var m4 = typeof(Module).GetMethod("GetTypes", flags, null, new Type[0], null);
				if (m4 != null)
				{
					var hk4 = typeof(GetAssembliesBypass).GetMethod("hk_ModuleGetTypes", staticPublic);
					RuntimeHelpers.PrepareMethod(hk4.MethodHandle);
					hook_ModuleGetTypes = new MethodHook(m4, hk4);
					try { original_ModuleGetTypes = hook_ModuleGetTypes.GenerateTrampoline<ModuleGetTypesDelegate>(); }
					catch { }
					anyOk = true;
					SceneInitializer.CrashLog("[Bypass] Module.GetTypes 钩子安装成功");
				}
			}
			catch (Exception ex)
			{
				SceneInitializer.SafeLogError("[加载器] Module.GetTypes() MethodHook 失败: " + ex.Message);
			}

			if (anyOk)
				SceneInitializer.SafeLog("[加载器] AppDomain.GetAssemblies NativeDetour 安装完成（trampoline1=" +
					(original_GetAssemblies != null ? "ok" : "undo/redo") + ", trampoline2=" +
					(original_GetAssemblies2 != null ? "ok" : "undo/redo") + ")");
			else
				SceneInitializer.SafeLogError("[加载器] AppDomain.GetAssemblies NativeDetour 全部失败");
		}

		private static Assembly[] FilterAssemblies(Assembly[] assemblies)
		{
			if (assemblies == null) return null;
			var filtered = new System.Collections.Generic.List<Assembly>(assemblies.Length);
			foreach (Assembly asm in assemblies)
			{
				if (asm == null) continue;
				bool hide = false;
				try
				{
					string name = asm.FullName;
					if (!string.IsNullOrEmpty(name))
					{
						if (name.IndexOf("SkyDome",      StringComparison.OrdinalIgnoreCase) >= 0 ||
						    name.IndexOf("SkyCore",      StringComparison.OrdinalIgnoreCase) >= 0 ||
						    name.IndexOf("MonoMod",      StringComparison.OrdinalIgnoreCase) >= 0 ||
						    name.IndexOf("Cecil",        StringComparison.OrdinalIgnoreCase) >= 0 ||
						    name.IndexOf("DotNetDetour", StringComparison.OrdinalIgnoreCase) >= 0)
						{
							hide = true;
						}
					}
				}
				catch { hide = true; } // 出现异常默认隐藏，极大提升安全性

				try
				{
					if (!hide)
					{
						string simpleName = asm.GetName().Name;
						if (!string.IsNullOrEmpty(simpleName))
						{
							if (simpleName.IndexOf("SkyDome",      StringComparison.OrdinalIgnoreCase) >= 0 ||
							    simpleName.IndexOf("SkyCore",      StringComparison.OrdinalIgnoreCase) >= 0 ||
							    simpleName.IndexOf("MonoMod",      StringComparison.OrdinalIgnoreCase) >= 0 ||
							    simpleName.IndexOf("Cecil",        StringComparison.OrdinalIgnoreCase) >= 0 ||
							    simpleName.IndexOf("DotNetDetour", StringComparison.OrdinalIgnoreCase) >= 0)
							{
								hide = true;
							}
						}
					}
				}
				catch { hide = true; }

				try
				{
					if (!hide)
					{
						string str = asm.ToString();
						if (!string.IsNullOrEmpty(str))
						{
							if (str.IndexOf("SkyDome",      StringComparison.OrdinalIgnoreCase) >= 0 ||
							    str.IndexOf("SkyCore",      StringComparison.OrdinalIgnoreCase) >= 0 ||
							    str.IndexOf("MonoMod",      StringComparison.OrdinalIgnoreCase) >= 0 ||
							    str.IndexOf("Cecil",        StringComparison.OrdinalIgnoreCase) >= 0 ||
							    str.IndexOf("DotNetDetour", StringComparison.OrdinalIgnoreCase) >= 0)
							{
								hide = true;
							}
						}
					}
				}
				catch { hide = true; }

				if (hide)
				{
					continue; // 过滤隐藏
				}
				filtered.Add(asm);
			}
			return filtered.ToArray();
		}

		public static Assembly[] hk_GetAssemblies(AppDomain self)
		{
			Assembly[] assemblies = null;
			if (original_GetAssemblies != null)
			{
				try
				{
					assemblies = original_GetAssemblies(self);
				}
				catch
				{
					// fallback
				}
			}

			if (assemblies == null)
			{
				if (hook_GetAssemblies != null)
				{
					lock (hook_GetAssemblies)
					{
						hook_GetAssemblies.Undo();
						try
						{
							assemblies = self.GetAssemblies();
						}
						finally
						{
							hook_GetAssemblies.Apply();
						}
					}
				}
				else
				{
					assemblies = self.GetAssemblies();
				}
			}

			return FilterAssemblies(assemblies);
		}

		public static Assembly[] hk_GetAssemblies2(AppDomain self, bool refOnly)
		{
			Assembly[] assemblies = null;
			if (original_GetAssemblies2 != null)
			{
				try
				{
					assemblies = original_GetAssemblies2(self, refOnly);
				}
				catch
				{
					// fallback
				}
			}

			if (assemblies == null)
			{
				if (hook_GetAssemblies2 != null && method_GetAssemblies2 != null)
				{
					lock (hook_GetAssemblies2)
					{
						hook_GetAssemblies2.Undo();
						try
						{
							assemblies = (Assembly[])method_GetAssemblies2.Invoke(self, new object[] { refOnly });
						}
						finally
						{
							hook_GetAssemblies2.Apply();
						}
					}
				}
			}

			return FilterAssemblies(assemblies);
		}
	}
}
