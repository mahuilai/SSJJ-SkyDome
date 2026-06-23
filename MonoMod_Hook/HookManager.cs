using System;
using System.Reflection;
using UnityEngine;

using Assets.Sources.Info.Camera.CameraLogic;
using Assets.Sources.Modules.Ui.UiEventCondition;
using Assets.Sources.Components.UserComand;
using Assets.Sources.Components.Player;
using Assets.Sources.Components.Camera;
using Assets.Sources.Modules.Player.Orientation;
using SSJJUserCmd;
using SkyDome.Cfg;
using SkyDome.RuntimeDetour;
using SkyDome.Entity;
using SkyDome.Features;

namespace SkyDome
{
	public static class HookEngine
	{
		public delegate bool TpsCameraLogic_IsActive_Delegate(TpsCameraLogic self);
		public delegate void TpsCameraLogic_Update_Delegate(TpsCameraLogic self);
		public delegate float CameraFunction_GetCurrentCmdYaw_Delegate(object _icameraLogic);
		public delegate float CameraFunction_GetCurrentCmdPitch_Delegate(object _icameraLogic);
		public delegate float UiIEventCondition_Get_ControlEntityData_Yaw_Delegate();
		public delegate float UiIEventCondition_Get_cameraOwnerData_Yaw_Delegate();
		public delegate short CommandsComponent_LastCameraYaw_Delegate(CommandsComponent self);
		public delegate short CommandsComponent_LastCameraPitch_Delegate(CommandsComponent self);
		public delegate FovComponent PlayerEntity_get_fov_Delegate(PlayerEntity self);
		public delegate void PlayerOrientationPredicationSystem_OnPredicate_Delegate(PlayerOrientationPredicationSystem self, PlayerEntity myPlayer, IUserCmd cmd);
		public delegate void PlayerOrientationPlabackSystem_OnPlayback_Delegate(PlayerOrientationPlabackSystem self);
		public delegate void PlayerOrientationPredicationSystem_PredictCmdOnCamera_Delegate(PlayerOrientationPredicationSystem self, PlayerEntity player, IUserCmd cmd);

		public static TpsCameraLogic_IsActive_Delegate original_IsActive;
		public static TpsCameraLogic_Update_Delegate original_Update;
		public static CameraFunction_GetCurrentCmdYaw_Delegate original_GetCurrentCmdYaw;
		public static CameraFunction_GetCurrentCmdPitch_Delegate original_GetCurrentCmdPitch;
		public static UiIEventCondition_Get_ControlEntityData_Yaw_Delegate original_Get_ControlEntityData_Yaw;
		public static UiIEventCondition_Get_cameraOwnerData_Yaw_Delegate original_Get_cameraOwnerData_Yaw;
		public static CommandsComponent_LastCameraYaw_Delegate original_LastCameraYaw;
		public static CommandsComponent_LastCameraPitch_Delegate original_LastCameraPitch;
		public static PlayerEntity_get_fov_Delegate original_get_fov;
		public static PlayerOrientationPredicationSystem_OnPredicate_Delegate original_OnPredicate;
		public static PlayerOrientationPlabackSystem_OnPlayback_Delegate original_OnPlayback;
		public static PlayerOrientationPredicationSystem_PredictCmdOnCamera_Delegate original_PredictCmdOnCamera;

		private static MethodHook hook_IsActive;
		private static MethodHook hook_Update;
		private static MethodHook hook_GetCurrentCmdYaw;
		private static MethodHook hook_GetCurrentCmdPitch;
		private static MethodHook hook_Get_ControlEntityData_Yaw;
		private static MethodHook hook_Get_cameraOwnerData_Yaw;
		private static MethodHook hook_LastCameraYaw;
		private static MethodHook hook_LastCameraPitch;
		private static MethodHook hook_get_fov;
		private static MethodHook hook_OnPredicate;
		private static MethodHook hook_OnPlayback;
		private static MethodHook hook_PredictCmdOnCamera;

		private static MethodInfo m_IsActive;
		private static MethodInfo m_Update;
		private static MethodInfo m_GetCurrentCmdYaw;
		private static MethodInfo m_GetCurrentCmdPitch;
		private static MethodInfo m_Get_ControlEntityData_Yaw;
		private static MethodInfo m_Get_cameraOwnerData_Yaw;
		private static MethodInfo m_LastCameraYaw;
		private static MethodInfo m_LastCameraPitch;
		private static MethodInfo m_get_fov;
		private static MethodInfo m_OnPredicate;
		private static MethodInfo m_OnPlayback;
		private static MethodInfo m_PredictCmdOnCamera;

		// 缓存反射字段，避免每帧调用 GetField（性能关键）
		private static FieldInfo _field_yaw;
		private static FieldInfo _field_pitch;
		private static FieldInfo _field_viewOrgPosition;
		private static FieldInfo _field_distance;

		// 错误流限制：防止异常每帧刷日志导致卡顿
		private static readonly System.Collections.Generic.Dictionary<string, float> _errLastTime
			= new System.Collections.Generic.Dictionary<string, float>();
		private static void ThrottledLogError(string key, string msg, float intervalSec = 5f)
		{
			float now = Time.realtimeSinceStartup;
			if (!_errLastTime.TryGetValue(key, out float last) || now - last > intervalSec)
			{
				_errLastTime[key] = now;
				Debug.LogError(msg);
			}
		}

		private static void TryInstallHook(string name, Action installer, Func<bool> checkTrampoline)
		{
			try
			{
				installer();
				bool trampOk = checkTrampoline();
				SceneInitializer.SafeLog(string.Format("[加载器] Hook {0} 安装成功 (trampoline={1})", name, trampOk ? "ok" : "null"));
			}
			catch (Exception ex)
			{
				SceneInitializer.SafeLogError(string.Format("[加载器] Hook {0} 安装失败: {1}", name, ex.ToString()));
			}
		}

		public static void StartHook()
		{
			SceneInitializer.CrashLog("[步骤5/5-HookEngine] 开始安装 12 个钩子...");
			Debug.Log("[加载器] [HookEngine] 开始安装 12 个钩子...");
			BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
			BindingFlags fieldFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

			TryInstallHook("[1/11] TpsCameraLogic.IsActive", () => {
				m_IsActive = typeof(TpsCameraLogic).GetMethod("IsActive", flags);
				hook_IsActive = new MethodHook(m_IsActive, typeof(HookEngine).GetMethod("hk_IsActive", flags));
				original_IsActive = hook_IsActive.GenerateTrampoline<TpsCameraLogic_IsActive_Delegate>();
			}, () => original_IsActive != null);
			TryInstallHook("[2/11] TpsCameraLogic.Update", () => {
				m_Update = typeof(TpsCameraLogic).GetMethod("Update", flags);
				hook_Update = new MethodHook(m_Update, typeof(HookEngine).GetMethod("hk_Update", flags));
				original_Update = hook_Update.GenerateTrampoline<TpsCameraLogic_Update_Delegate>();
			}, () => original_Update != null);
			TryInstallHook("[3/11] GetCurrentCmdYaw", () => {
				m_GetCurrentCmdYaw = typeof(CameraFunction).GetMethod("GetCurrentCmdYaw", flags);
				hook_GetCurrentCmdYaw = new MethodHook(m_GetCurrentCmdYaw, typeof(HookEngine).GetMethod("hk_GetCurrentCmdYaw", flags));
				original_GetCurrentCmdYaw = hook_GetCurrentCmdYaw.GenerateTrampoline<CameraFunction_GetCurrentCmdYaw_Delegate>();
			}, () => original_GetCurrentCmdYaw != null);
			TryInstallHook("[4/11] GetCurrentCmdPitch", () => {
				m_GetCurrentCmdPitch = typeof(CameraFunction).GetMethod("GetCurrentCmdPitch", flags);
				hook_GetCurrentCmdPitch = new MethodHook(m_GetCurrentCmdPitch, typeof(HookEngine).GetMethod("hk_GetCurrentCmdPitch", flags));
				original_GetCurrentCmdPitch = hook_GetCurrentCmdPitch.GenerateTrampoline<CameraFunction_GetCurrentCmdPitch_Delegate>();
			}, () => original_GetCurrentCmdPitch != null);
			TryInstallHook("[5/11] Get_ControlEntityData_Yaw", () => {
				m_Get_ControlEntityData_Yaw = typeof(UiIEventCondition).GetMethod("Get_ControlEntityData_Yaw", flags);
				hook_Get_ControlEntityData_Yaw = new MethodHook(m_Get_ControlEntityData_Yaw, typeof(HookEngine).GetMethod("hk_Get_ControlEntityData_Yaw", flags));
				original_Get_ControlEntityData_Yaw = hook_Get_ControlEntityData_Yaw.GenerateTrampoline<UiIEventCondition_Get_ControlEntityData_Yaw_Delegate>();
			}, () => original_Get_ControlEntityData_Yaw != null);
			TryInstallHook("[6/11] Get_cameraOwnerData_Yaw", () => {
				m_Get_cameraOwnerData_Yaw = typeof(UiIEventCondition).GetMethod("Get_cameraOwnerData_Yaw", flags);
				hook_Get_cameraOwnerData_Yaw = new MethodHook(m_Get_cameraOwnerData_Yaw, typeof(HookEngine).GetMethod("hk_Get_cameraOwnerData_Yaw", flags));
				original_Get_cameraOwnerData_Yaw = hook_Get_cameraOwnerData_Yaw.GenerateTrampoline<UiIEventCondition_Get_cameraOwnerData_Yaw_Delegate>();
			}, () => original_Get_cameraOwnerData_Yaw != null);
			TryInstallHook("[7/11] LastCameraYaw", () => {
				m_LastCameraYaw = typeof(CommandsComponent).GetMethod("LastCameraYaw", flags);
				hook_LastCameraYaw = new MethodHook(m_LastCameraYaw, typeof(HookEngine).GetMethod("hk_LastCameraYaw", flags));
				original_LastCameraYaw = hook_LastCameraYaw.GenerateTrampoline<CommandsComponent_LastCameraYaw_Delegate>();
			}, () => original_LastCameraYaw != null);
			TryInstallHook("[8/11] LastCameraPitch", () => {
				m_LastCameraPitch = typeof(CommandsComponent).GetMethod("LastCameraPitch", flags);
				hook_LastCameraPitch = new MethodHook(m_LastCameraPitch, typeof(HookEngine).GetMethod("hk_LastCameraPitch", flags));
				original_LastCameraPitch = hook_LastCameraPitch.GenerateTrampoline<CommandsComponent_LastCameraPitch_Delegate>();
			}, () => original_LastCameraPitch != null);
			TryInstallHook("[9/11] get_fov", () => {
				m_get_fov = typeof(PlayerEntity).GetMethod("get_fov", flags);
				hook_get_fov = new MethodHook(m_get_fov, typeof(HookEngine).GetMethod("hk_get_fov", flags));
				original_get_fov = hook_get_fov.GenerateTrampoline<PlayerEntity_get_fov_Delegate>();
			}, () => original_get_fov != null);
			TryInstallHook("[10/11] OnPredicate", () => {
				m_OnPredicate = typeof(PlayerOrientationPredicationSystem).GetMethod("OnPredicate", flags);
				hook_OnPredicate = new MethodHook(m_OnPredicate, typeof(HookEngine).GetMethod("hk_OnPredicate", flags));
				original_OnPredicate = hook_OnPredicate.GenerateTrampoline<PlayerOrientationPredicationSystem_OnPredicate_Delegate>();
			}, () => original_OnPredicate != null);
			TryInstallHook("[11/11] OnPlayback", () => {
				m_OnPlayback = typeof(PlayerOrientationPlabackSystem).GetMethod("OnPlayback", flags);
				hook_OnPlayback = new MethodHook(m_OnPlayback, typeof(HookEngine).GetMethod("hk_OnPlayback", flags));
				original_OnPlayback = hook_OnPlayback.GenerateTrampoline<PlayerOrientationPlabackSystem_OnPlayback_Delegate>();
			}, () => original_OnPlayback != null);
			TryInstallHook("[12/12] PredictCmdOnCamera", () => {
				m_PredictCmdOnCamera = typeof(PlayerOrientationPredicationSystem).GetMethod("PredictCmdOnCamera", flags);
				hook_PredictCmdOnCamera = new MethodHook(m_PredictCmdOnCamera, typeof(HookEngine).GetMethod("hk_PredictCmdOnCamera", flags));
				original_PredictCmdOnCamera = hook_PredictCmdOnCamera.GenerateTrampoline<PlayerOrientationPredicationSystem_PredictCmdOnCamera_Delegate>();
			}, () => original_PredictCmdOnCamera != null);

			// 缓存反射字段
			try
			{
				_field_yaw           = typeof(TpsCameraLogic).GetField("_yaw", fieldFlags);
				_field_pitch         = typeof(TpsCameraLogic).GetField("_pitch", fieldFlags);
				_field_viewOrgPosition = typeof(TpsCameraLogic).GetField("_viewOrgPosition", fieldFlags);
				_field_distance      = typeof(TpsCameraLogic).GetField("_distance", fieldFlags);
			}
			catch (Exception ex)
			{
				SceneInitializer.SafeLogError("[加载器] 缓存 TpsCameraLogic 字段失败: " + ex.Message);
			}
			SceneInitializer.CrashLog("[步骤5/5-HookEngine] 所有 12 个钩子安装完成");
			Debug.Log("[加载器] [HookEngine] 所有钩子安装完成");
		}

		public static bool hk_IsActive(TpsCameraLogic self)
		{
			try
			{
				bool origVal = false;
				if (original_IsActive != null)
				{
					origVal = original_IsActive(self);
				}
				else if (hook_IsActive != null && m_IsActive != null)
				{
					lock (hook_IsActive)
					{
						hook_IsActive.Undo();
						try { origVal = (bool)m_IsActive.Invoke(self, null); }
						finally { hook_IsActive.Apply(); }
					}
				}

				bool isTpsEnabled = SettingsStore.ThirdPerson;

				if (Contexts.sharedInstance != null &&
					Contexts.sharedInstance.worldCamera != null &&
					Contexts.sharedInstance.worldCamera.cameraData != null)
				{
					CameraDataComponent cameraData = Contexts.sharedInstance.worldCamera.cameraData;

					// FOV：只有在用户明确开启自定义 FOV 时才改
					if (SettingsStore.Fov)
					{
						int targetFov = isTpsEnabled
							? (SettingsStore.ThirdPersonFov > 0 ? SettingsStore.ThirdPersonFov : 90)
							: (SettingsStore.FirstPersonFov > 0 ? (int)SettingsStore.FirstPersonFov : 90);
						cameraData.Fov = targetFov;
					}

					if (_field_yaw != null && _field_pitch != null)
					{
						float yaw = (float)_field_yaw.GetValue(self);
						float pitch = (float)_field_pitch.GetValue(self);
						cameraData.CameraYawAddValue = yaw;
						cameraData.CameraPitchAddValue = pitch - 5f;
					}

					if (isTpsEnabled)
					{
						if (Contexts.sharedInstance.time != null && Contexts.sharedInstance.time.time != null)
						{
							cameraData.TransTime = Mathf.Max(230, cameraData.TransTime + Contexts.sharedInstance.time.time.FrameInterval);
						}
					}
					else
					{
						cameraData.TransTime = 0;
					}
					cameraData.IsTps = isTpsEnabled;
				}

				return isTpsEnabled ? isTpsEnabled : origVal;
			}
			catch (Exception ex)
			{
				ThrottledLogError("IsActive", "[第三人称] hk_IsActive 异常: " + ex);
				return false;
			}
		}

		public static void hk_Update(TpsCameraLogic self)
		{
			try
			{
				if (original_Update != null)
				{
					original_Update(self);
				}
				else if (hook_Update != null && m_Update != null)
				{
					lock (hook_Update)
					{
						hook_Update.Undo();
						try { m_Update.Invoke(self, null); }
						finally { hook_Update.Apply(); }
					}
				}

				var own = PlayerStateTracker.LocalEntity;
				if (own != null && !own.IsDead)
				{
					if (Contexts.sharedInstance != null && 
						Contexts.sharedInstance.worldCamera != null && 
						Contexts.sharedInstance.worldCamera.cameraData != null && 
						Contexts.sharedInstance.player != null && 
						Contexts.sharedInstance.player.myPlayerEntity != null)
					{
						CameraDataComponent cameraData = Contexts.sharedInstance.worldCamera.cameraData;
						PlayerEntity myPlayerEntity = Contexts.sharedInstance.player.myPlayerEntity;

						if (_field_viewOrgPosition != null && _field_distance != null && _field_yaw != null && _field_pitch != null)
						{
							Vector3 viewOrgPosition = (Vector3)_field_viewOrgPosition.GetValue(self);
							float distance = (float)_field_distance.GetValue(self);
							float yaw = (float)_field_yaw.GetValue(self);
							float pitch = (float)_field_pitch.GetValue(self);

							Vector3 vector = default(Vector3);
							bool isTpsEnabled = SettingsStore.ThirdPerson;
							if (isTpsEnabled)
							{
								vector = self.GetCalculateCameraEndPos(viewOrgPosition, cameraData.CameraYawAddValue, cameraData.CameraPitchAddValue, distance, 10f);
								
								// AngleUtility.AnglesToVectors2 replacement in standard math
								float pitchRad = pitch * 0.0174532924f;
								float yawRad = yaw * 0.0174532924f;
								float cosPitch = Mathf.Cos(pitchRad);
								float sinPitch = Mathf.Sin(pitchRad);
								float cosYaw = Mathf.Cos(yawRad);
								float sinYaw = Mathf.Sin(yawRad);

								vector = self.GetCalculateCameraEndPos(vector, cameraData.CameraYawAddValue, 0f, 50f, 10f);
								
								if (myPlayerEntity.fov != null && myPlayerEntity.fov.Fov != cameraData.Fov)
								{
									myPlayerEntity.fov.Fov = cameraData.Fov;
									myPlayerEntity.fov.DelayFov = cameraData.Fov;
								}
							}
							if (isTpsEnabled && cameraData.TransTime != 0)
							{
								self.InterpolateCamareDeadEndPos(viewOrgPosition, vector, cameraData.TransTime);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				ThrottledLogError("Update", "[第三人称] hk_Update 异常: " + ex);
			}
		}

		public static float hk_GetCurrentCmdYaw(object _icameraLogic)
		{
			try
			{
				var own = PlayerStateTracker.LocalEntity;
				if (own != null && !own.IsDead && 
					Contexts.sharedInstance != null && 
					Contexts.sharedInstance.worldCamera != null && 
					Contexts.sharedInstance.worldCamera.cameraTransform != null)
				{
					return Contexts.sharedInstance.worldCamera.cameraTransform.Yaw;
				}
			}
			catch (Exception ex)
			{
				ThrottledLogError("CmdYaw", "[第三人称] hk_GetCurrentCmdYaw 异常: " + ex);
			}

			if (original_GetCurrentCmdYaw != null)
			{
				return original_GetCurrentCmdYaw(_icameraLogic);
			}
			else if (hook_GetCurrentCmdYaw != null && m_GetCurrentCmdYaw != null)
			{
				lock (hook_GetCurrentCmdYaw)
				{
					hook_GetCurrentCmdYaw.Undo();
					try
					{
						object obj = m_GetCurrentCmdYaw.IsStatic ? null : _icameraLogic;
						object[] parameters = m_GetCurrentCmdYaw.IsStatic ? new object[] { _icameraLogic } : null;
						return (float)m_GetCurrentCmdYaw.Invoke(obj, parameters);
					}
					finally { hook_GetCurrentCmdYaw.Apply(); }
				}
			}
			return 0f;
		}

		public static float hk_GetCurrentCmdPitch(object _icameraLogic)
		{
			try
			{
				var own = PlayerStateTracker.LocalEntity;
				if (own != null && !own.IsDead && 
					Contexts.sharedInstance != null && 
					Contexts.sharedInstance.worldCamera != null && 
					Contexts.sharedInstance.worldCamera.cameraTransform != null)
				{
					return Contexts.sharedInstance.worldCamera.cameraTransform.Pitch;
				}
			}
			catch (Exception ex)
			{
				ThrottledLogError("CmdPitch", "[第三人称] hk_GetCurrentCmdPitch 异常: " + ex);
			}

			if (original_GetCurrentCmdPitch != null)
			{
				return original_GetCurrentCmdPitch(_icameraLogic);
			}
			else if (hook_GetCurrentCmdPitch != null && m_GetCurrentCmdPitch != null)
			{
				lock (hook_GetCurrentCmdPitch)
				{
					hook_GetCurrentCmdPitch.Undo();
					try
					{
						object obj = m_GetCurrentCmdPitch.IsStatic ? null : _icameraLogic;
						object[] parameters = m_GetCurrentCmdPitch.IsStatic ? new object[] { _icameraLogic } : null;
						return (float)m_GetCurrentCmdPitch.Invoke(obj, parameters);
					}
					finally { hook_GetCurrentCmdPitch.Apply(); }
				}
			}
			return 0f;
		}

		public static float hk_Get_ControlEntityData_Yaw()
		{
			try
			{
				var own = PlayerStateTracker.LocalEntity;
				if (own != null && !own.IsDead)
				{
					return UiIEventCondition.Get_cameraOwnerData_Yaw();
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("[第三人称] hk_Get_ControlEntityData_Yaw 异常: " + ex);
			}

			if (original_Get_ControlEntityData_Yaw != null)
			{
				return original_Get_ControlEntityData_Yaw();
			}
			else if (hook_Get_ControlEntityData_Yaw != null && m_Get_ControlEntityData_Yaw != null)
			{
				lock (hook_Get_ControlEntityData_Yaw)
				{
					hook_Get_ControlEntityData_Yaw.Undo();
					try { return (float)m_Get_ControlEntityData_Yaw.Invoke(null, null); }
					finally { hook_Get_ControlEntityData_Yaw.Apply(); }
				}
			}
			return 0f;
		}

		public static float hk_Get_cameraOwnerData_Yaw()
		{
			try
			{
				var own = PlayerStateTracker.LocalEntity;
				if (own != null && !own.IsDead && 
					Contexts.sharedInstance != null && 
					Contexts.sharedInstance.worldCamera != null && 
					Contexts.sharedInstance.worldCamera.cameraTransform != null)
				{
					return Contexts.sharedInstance.worldCamera.cameraTransform.Yaw;
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("[第三人称] hk_Get_cameraOwnerData_Yaw 异常: " + ex);
			}

			if (original_Get_cameraOwnerData_Yaw != null)
			{
				return original_Get_cameraOwnerData_Yaw();
			}
			else if (hook_Get_cameraOwnerData_Yaw != null && m_Get_cameraOwnerData_Yaw != null)
			{
				lock (hook_Get_cameraOwnerData_Yaw)
				{
					hook_Get_cameraOwnerData_Yaw.Undo();
					try { return (float)m_Get_cameraOwnerData_Yaw.Invoke(null, null); }
					finally { hook_Get_cameraOwnerData_Yaw.Apply(); }
				}
			}
			return 0f;
		}

		public static short hk_LastCameraYaw(CommandsComponent self)
		{
			try
			{
				var own = PlayerStateTracker.LocalEntity;
				if (own != null && !own.IsDead && 
					Contexts.sharedInstance != null && 
					Contexts.sharedInstance.worldCamera != null && 
					Contexts.sharedInstance.worldCamera.cameraTransform != null)
				{
					return (short)(Contexts.sharedInstance.worldCamera.cameraTransform.Yaw * 100f);
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("[第三人称] hk_LastCameraYaw 异常: " + ex);
			}

			if (original_LastCameraYaw != null)
			{
				return original_LastCameraYaw(self);
			}
			else if (hook_LastCameraYaw != null && m_LastCameraYaw != null)
			{
				lock (hook_LastCameraYaw)
				{
					hook_LastCameraYaw.Undo();
					try { return (short)m_LastCameraYaw.Invoke(self, null); }
					finally { hook_LastCameraYaw.Apply(); }
				}
			}
			return 0;
		}

		public static short hk_LastCameraPitch(CommandsComponent self)
		{
			try
			{
				var own = PlayerStateTracker.LocalEntity;
				if (own != null && !own.IsDead && 
					Contexts.sharedInstance != null && 
					Contexts.sharedInstance.worldCamera != null && 
					Contexts.sharedInstance.worldCamera.cameraTransform != null)
				{
					return (short)(Contexts.sharedInstance.worldCamera.cameraTransform.Pitch * 100f);
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("[第三人称] hk_LastCameraPitch 异常: " + ex);
			}

			if (original_LastCameraPitch != null)
			{
				return original_LastCameraPitch(self);
			}
			else if (hook_LastCameraPitch != null && m_LastCameraPitch != null)
			{
				lock (hook_LastCameraPitch)
				{
					hook_LastCameraPitch.Undo();
					try { return (short)m_LastCameraPitch.Invoke(self, null); }
					finally { hook_LastCameraPitch.Apply(); }
				}
			}
			return 0;
		}

		public static FovComponent hk_get_fov(PlayerEntity self)
		{
			try
			{
				FovComponent fovComponent = null;
				if (original_get_fov != null)
				{
					fovComponent = original_get_fov(self);
				}
				else if (hook_get_fov != null && m_get_fov != null)
				{
					lock (hook_get_fov)
					{
						hook_get_fov.Undo();
						try { fovComponent = (FovComponent)m_get_fov.Invoke(self, null); }
						finally { hook_get_fov.Apply(); }
					}
				}

				if (fovComponent != null && SettingsStore.ThirdPerson && SettingsStore.Fov)
				{
					fovComponent.Fov = SettingsStore.ThirdPersonFov;
				}
				return fovComponent;
			}
			catch (Exception ex)
			{
				Debug.LogError("[第三人称] hk_get_fov 异常: " + ex);
				if (original_get_fov != null)
				{
					return original_get_fov(self);
				}
				return null;
			}
		}

		public static void hk_OnPredicate(PlayerOrientationPredicationSystem self, PlayerEntity myPlayer, IUserCmd cmd)
		{
			try
			{
				if (Contexts.sharedInstance != null && Contexts.sharedInstance.player != null && Contexts.sharedInstance.player.cameraOwnerEntity != null)
				{
					PlayerEntity cameraOwnerEntity = Contexts.sharedInstance.player.cameraOwnerEntity;
					if (cameraOwnerEntity.orientation != null)
					{
						var own = PlayerStateTracker.LocalEntity;
						if (own != null && !own.IsDead)
						{
							cameraOwnerEntity.orientation.Pitch = AngleCorrector.SharedPitch;
							cameraOwnerEntity.orientation.Yaw = AngleCorrector.SharedYaw;
						}
					}
				}
			}
			catch (Exception ex)
			{
				ThrottledLogError("OnPredicate", "[第三人称] hk_OnPredicate 异常: " + ex);
			}

			if (original_OnPredicate != null)
			{
				original_OnPredicate(self, myPlayer, cmd);
			}
			else if (hook_OnPredicate != null && m_OnPredicate != null)
			{
				lock (hook_OnPredicate)
				{
					hook_OnPredicate.Undo();
					try { m_OnPredicate.Invoke(self, new object[] { myPlayer, cmd }); }
					finally { hook_OnPredicate.Apply(); }
				}
			}
		}

		public static void hk_PredictCmdOnCamera(PlayerOrientationPredicationSystem self, PlayerEntity player, IUserCmd cmd)
		{
			try
			{
				var own = PlayerStateTracker.LocalEntity;
				if (own == null || own.IsDead || !SettingsStore.ThirdPerson)
				{
					if (original_PredictCmdOnCamera != null)
					{
						original_PredictCmdOnCamera(self, player, cmd);
					}
					else if (hook_PredictCmdOnCamera != null && m_PredictCmdOnCamera != null)
					{
						lock (hook_PredictCmdOnCamera)
						{
							hook_PredictCmdOnCamera.Undo();
							try { m_PredictCmdOnCamera.Invoke(self, new object[] { player, cmd }); }
							finally { hook_PredictCmdOnCamera.Apply(); }
						}
					}
				}
				else
				{
					// 当开启了第三人称且玩家存活时，执行原始逻辑，但参数中可能需要进行同步或直接执行
					if (original_PredictCmdOnCamera != null)
					{
						original_PredictCmdOnCamera(self, player, cmd);
					}
					else if (hook_PredictCmdOnCamera != null && m_PredictCmdOnCamera != null)
					{
						lock (hook_PredictCmdOnCamera)
						{
							hook_PredictCmdOnCamera.Undo();
							try { m_PredictCmdOnCamera.Invoke(self, new object[] { player, cmd }); }
							finally { hook_PredictCmdOnCamera.Apply(); }
						}
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("[第三人称] hk_PredictCmdOnCamera 异常: " + ex);
				if (original_PredictCmdOnCamera != null)
				{
					original_PredictCmdOnCamera(self, player, cmd);
				}
			}
		}

		public static void hk_OnPlayback(PlayerOrientationPlabackSystem self)
		{
			try
			{
				if (original_OnPlayback != null)
				{
					original_OnPlayback(self);
				}
				else if (hook_OnPlayback != null && m_OnPlayback != null)
				{
					lock (hook_OnPlayback)
					{
						hook_OnPlayback.Undo();
						try { m_OnPlayback.Invoke(self, null); }
						finally { hook_OnPlayback.Apply(); }
					}
				}
			}
			catch (Exception ex)
			{
				ThrottledLogError("OnPlayback_orig", "[第三人称] original_OnPlayback 异常: " + ex);
			}

			try
			{
				if (Contexts.sharedInstance == null) return;
				if (Contexts.sharedInstance.player == null) return;
				if (Contexts.sharedInstance.player.cameraOwnerEntity == null) return;

				var own = PlayerStateTracker.LocalEntity;
				if (own == null) return;

				bool isDead;
				try { isDead = own.IsDead; } catch { return; }
				if (isDead) return;

				PlayerEntity cameraOwnerEntity = Contexts.sharedInstance.player.cameraOwnerEntity;
				if (cameraOwnerEntity == null) return;
				if (cameraOwnerEntity.orientation == null) return;
				if (cameraOwnerEntity.basicInfo == null) return;
				if (cameraOwnerEntity.punchOrientation == null) return;

				var next = cameraOwnerEntity.basicInfo.Next;

				cameraOwnerEntity.orientation.Pitch = AngleCorrector.SharedPitch;
				cameraOwnerEntity.orientation.Yaw = AngleCorrector.SharedYaw;

				if (next != null)
				{
					cameraOwnerEntity.punchOrientation.PunchPitch = next.PunchPitch;
					cameraOwnerEntity.punchOrientation.PunchYaw = next.PunchYaw;
				}

				cameraOwnerEntity.orientation.MoveYaw = AngleCorrector.SharedYaw;
				cameraOwnerEntity.orientation.ActThirdMoveInterYaw = AngleCorrector.SharedYaw;
			}
			catch (Exception ex)
			{
				ThrottledLogError("OnPlayback", "[第三人称] hk_OnPlayback 异常: " + ex);
			}
		}
	}
}
