using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using Assets.Scripts.Input;
using SkyDome.Engine;
using SkyDome.Entity;
using SkyDome.Feature;
using SkyDome.Feature.AutoTrigger;
using SkyDome.Feature.Legit;
using SkyDome.Feature.Visuals;
using SkyDome.Features;
using UnityEngine;
namespace SkyDome
{
	public class Main : MonoBehaviour
	{
		public static void LoadAllAssembly()
		{
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			string[] manifestResourceNames = executingAssembly.GetManifestResourceNames();
			string assemblyResourceSuffix = ".dll";
			for (int i = 0; i < manifestResourceNames.Length; i++)
			{
				string resourceName = manifestResourceNames[i];
				if (resourceName.EndsWith(assemblyResourceSuffix))
				{
					using (Stream manifestResourceStream = executingAssembly.GetManifestResourceStream(resourceName))
					{
						if (manifestResourceStream != null)
						{
							byte[] array = new byte[manifestResourceStream.Length];
							manifestResourceStream.Read(array, 0, array.Length);
							Assembly.Load(array);
						}
					}
				}
			}
		}
		private void Destroy()
		{
			foreach (GameObject gameObject in this._hookList)
			{
				if (gameObject != null)
				{
					global::UnityEngine.Object.Destroy(gameObject);
				}
			}
			if (Main._hookObject != null)
			{
				global::UnityEngine.Object.Destroy(Main._hookObject);
			}
		}
		private void Init()
		{
			try
			{
				SceneInitializer.CrashLog("[Init-1/4] 开始 Init...");
				Debug.Log("[加载器] [Init-1/4] 开始 Init...");
				if (Main._hookObject != null)
				{
					Debug.Log("[加载器] [Init-1/4] _hookObject 已存在，跳过初始化");
					return;
				}
				GameObject gameObject = GameObject.Find(Main.HookObjectName);
				if (gameObject != null)
				{
					Main._hookObject = gameObject;
					Debug.Log("[加载器] [Init-1/4] 找到已存在的 HookObject，跳过初始化");
					return;
				}
				Debug.Log("[加载器] [Init-1/4] 创建 HookObject...");
				Main._hookObject = new GameObject(Main.HookObjectName);
				SceneInitializer.CrashLog("[Init-2/4] 注册组件...");
				Debug.Log("[加载器] [Init-2/4] 注册组件...");
				this.RegisterHookComponents();
				SceneInitializer.CrashLog("[Init-2/4] 组件注册完成");
				Debug.Log("[加载器] [Init-2/4] 组件注册完成");
				SceneInitializer.CrashLog("[Init-3/4] 设置 InputDriver...");
				Debug.Log("[加载器] [Init-3/4] 设置 InputDriver...");
				if (InputCollector.Instance != null)
				{
					InputCollector.Instance.SetDeviceInput(new SkyDome.Engine.InputDriver());
					Debug.Log("[加载器] [Init-3/4] InputDriver 设置成功");
				}
				else
				{
					Debug.LogWarning("[加载器] InputCollector.Instance 为空，已跳过 SetDeviceInput");
				}
				SceneInitializer.CrashLog("[Init-4/4] 启动 HookEngine...");
				Debug.Log("[加载器] [Init-4/4] 启动 HookEngine...");
				HookEngine.StartHook();
				SceneInitializer.CrashLog("[Init-4/4] HookEngine 启动完成");
				Debug.Log("[加载器] [Init-4/4] HookEngine 启动完成");
			}
			catch (Exception ex)
			{
				Debug.LogError("[加载器] Init 初始化捕获到异常: " + ex.ToString());
			}
		}

        private void RegisterHookComponents()
		{
			this.AddComponent<PlayerStateTracker>("PlayerStateTrackerObject");
			this.AddComponent<EntityVisualizer>("EntityVisualizerObject");
			this.AddComponent<ObserverTracker>("ObserverTrackerObject");
			this.AddComponent<DirectionDisplay>("DirectionDisplayObject");
			this.AddComponent<PathRenderer>("PathRendererObject");
			this.AddComponent<MaterialOverlay>("MaterialOverlayObject");
			this.AddComponent<SkyDome.Feature.Visuals.MiniMapOverlay>("MiniMapOverlayObject");
			this.AddComponent<BombTimer>("BombTimerObject");
			this.AddComponent<ReticleRenderer>("ReticleRendererObject");
			this.AddComponent<TargetSelector>("TargetSelectorObject");
			this.AddComponent<AutoFireController>("AutoFireControllerObject");
			this.AddComponent<ViewStabilizer>("ViewStabilizerObject");
			this.AddComponent<PitchSynchronizer>("PitchSynchronizerObject");
			this.AddComponent<MessageDispatcher>("MessageDispatcherObject");
			this.AddComponent<Menu>("MenuObject");
			this.AddComponent<EntityBoxRenderer>("EntityBoxRendererObject");
			this.AddComponent<StatusDisplay>("StatusDisplayObject");
			this.AddComponent<AutoSheath>("AutoSheathAutoSheathObject");
			this.AddComponent<SkyDome.Feature.AutoTrigger.AutoRecall>("AutoRecallObject");
			this.AddComponent<AutoDance>("AutoDanceObject");
			this.AddComponent<HealthDisplay>("HealthDisplayObject");
			this.AddComponent<PickupHighlighter>("PickupHighlighterObject");
			this.AddComponent<PickupOutliner>("PickupOutlinerObject");
			this.AddComponent<DynamicEntityESP>("DynamicEntityESPObject");
			this.AddComponent<BuffDisplay>("BuffDisplayObject");
		}
		private void AddComponent<T>(string objectName) where T : Component
		{
			if (global::UnityEngine.Object.FindObjectOfType<T>() == null)
			{
				GameObject gameObject = new GameObject(objectName);
				gameObject.AddComponent<T>();
				global::UnityEngine.Object.DontDestroyOnLoad(gameObject);
				this._hookList.Add(gameObject);
			}
		}

        private void Awake()
		{
			this.Init();
		}
		private static GameObject _hookObject;
		private const string HookObjectName = "HookObject";
		private readonly List<GameObject> _hookList = new List<GameObject>();
	}
}