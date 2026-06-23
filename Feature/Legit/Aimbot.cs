using System;
using Assets.Sources.Utils.Weapon;
using share;
using SkyDome.Cfg;
using SkyDome.Engine;
using SkyDome.Entity;
using SkyDome.Render;
using SkyDome.Utilities;
using SSJJMath;
using UnityEngine;
using weapon.utils;
namespace SkyDome.Feature.Legit
{
	public class TargetSelector : MonoBehaviour
	{

        private void Update()
		{
			Contexts sharedInstance = Contexts.sharedInstance;
			if (sharedInstance != null)
			{
				if (sharedInstance.weapon != null)
				{
					WeaponEntity currentWeaponEntity = Contexts.sharedInstance.weapon.currentWeaponEntity;
					if (currentWeaponEntity != null && currentWeaponEntity.basicInfo != null && currentWeaponEntity.basicInfo.Data != null && currentWeaponEntity.basicInfo.Info != null)
					{
						this._currentSpreadFactor = FireUtility.CalShotsFiredSpread(currentWeaponEntity.basicInfo.Data.ShotsFiredSpreadMin, currentWeaponEntity.basicInfo.Data.ShotsFiredSpreadMax, currentWeaponEntity.basicInfo.Data.ShotsFiredSpreadTime, currentWeaponEntity.basicInfo.Data.ShotsFired, currentWeaponEntity.basicInfo.Info.AttackInterval);
						return;
					}
					return;
				}
			}
		}
		public TargetSelector()
		{
		}

        private void DrawTargetSelectorFOVCircle()
		{
			if (PlayerStateTracker.MainCamera == null)
			{
				return;
			}
			float num = (float)SettingsStore.TargetSelectorFOV * 0.0174532924f * 0.5f;
			float num2 = (float)Screen.height;
			float fieldOfView = PlayerStateTracker.MainCamera.fieldOfView;
			float num3 = Mathf.Tan(num) / Mathf.Tan(fieldOfView * 0.0174532924f * 0.5f) * num2 * 0.5f;
			Color rainbow = FastRenderer.GetRainbowColor(5f);
			FastRenderer.DrawRainbowCircleOutline(this.ScreenCenter, num3, 64, 5f, true);
			FastRenderer.DrawRainbowCircleOutline(this.ScreenCenter, num3 - 2f, 48, 5f, true);
		}
		private Vector2 ScreenCenter
		{
			get
			{
				return new Vector2((float)Screen.width * 0.5f, (float)Screen.height * 0.5f);
			}
		}
		private global::UnityEngine.Vector3 GetAimPosition(SkyDome.Entity.PlayerData player)
		{
			Transform transform;
			switch (SettingsStore.AimPos)
			{
			case 0:
			{
				Transform playerTransform = player.GetPlayerTransform("Bip01_Head");
				Transform validHeadNub = player.GetValidHeadNub();
				if (playerTransform != null && validHeadNub != null)
				{
					return (playerTransform.position + validHeadNub.position) * 0.5f;
				}
				transform = playerTransform;
				break;
			}
			case 1:
				transform = player.GetValidHeadNub();
				break;
			case 2:
				transform = player.GetPlayerTransform("Bip01_Neck");
				break;
			case 3:
				transform = player.GetPlayerTransform("Bip01_Spine");
				break;
			case 4:
				transform = player.GetPlayerTransform("Bip01_L_Clavicle");
				break;
			case 5:
				transform = player.GetPlayerTransform("Bip01_R_Clavicle");
				break;
			case 6:
				transform = player.GetPlayerTransform("Bip01_L_UpperArm");
				break;
			case 7:
				transform = player.GetPlayerTransform("Bip01_R_UpperArm");
				break;
			case 8:
				transform = player.GetPlayerTransform("Bip01_L_Forearm");
				break;
			case 9:
				transform = player.GetPlayerTransform("Bip01_R_Forearm");
				break;
			case 10:
				transform = player.GetPlayerTransform("Bip01_L_Hand");
				break;
			case 11:
				transform = player.GetPlayerTransform("Bip01_R_Hand");
				break;
			case 12:
				transform = player.GetPlayerTransform("Bip01_L_Finger0");
				break;
			case 13:
				transform = player.GetPlayerTransform("Bip01_R_Finger0");
				break;
			case 14:
				transform = player.GetPlayerTransform("Bip01_Pelvis");
				break;
			case 15:
				transform = player.GetPlayerTransform("Bip01_L_Thigh");
				break;
			case 16:
				transform = player.GetPlayerTransform("Bip01_R_Thigh");
				break;
			case 17:
				transform = player.GetPlayerTransform("Bip01_L_Calf");
				break;
			case 18:
				transform = player.GetPlayerTransform("Bip01_R_Calf");
				break;
			case 19:
				transform = player.GetPlayerTransform("Bip01_L_Foot");
				break;
			case 20:
				transform = player.GetPlayerTransform("Bip01_R_Foot");
				break;
			case 21:
				transform = player.GetPlayerTransform("Bip01_L_Toe0");
				break;
			case 22:
				transform = player.GetPlayerTransform("Bip01_R_Toe0");
				break;
			default:
			{
				Transform playerTransform2 = player.GetPlayerTransform("Bip01_Head");
				Transform validHeadNub2 = player.GetValidHeadNub();
				if (playerTransform2 != null && validHeadNub2 != null)
				{
					return (playerTransform2.position + validHeadNub2.position) * 0.5f;
				}
				transform = playerTransform2;
				break;
			}
			}
			if (!(transform != null))
			{
				return player.GetPlayerTransform(player.PlayerName).position;
			}
			return transform.position;
		}

        private bool IsVisible(SkyDome.Entity.PlayerData target)
		{
			if (target != null && !(PlayerStateTracker.MainCamera == null))
			{
				global::UnityEngine.Vector3 aimPosition = this.GetAimPosition(target);
				global::UnityEngine.Vector3 position = PlayerStateTracker.MainCamera.transform.position;
				global::UnityEngine.Vector3 vector = VectorCoordConverter.UnityToSsjj((aimPosition - position).normalized);
				int entityId = SkyDome.Utilities.PathRendererHelper.GetEntityId(
					Contexts.sharedInstance.battleRoom.pyEngine.PyEngine, 
					PlayerStateTracker.LocalEntity._entity, 
					Contexts.sharedInstance.player, 
					10000f, 
					new Vector3D((double)vector.x, (double)vector.y, (double)vector.z), 
					new float[3], 
					new float[3], 
					false
				);
				return entityId == target.Id;
			}
			return false;
		}
		private void UpdateTarget()
		{
			if (!(PlayerStateTracker.MainCamera == null) && PlayerStateTracker.EntityList != null && PlayerStateTracker.LocalEntity != null)
			{
				if (TargetSelector._isActive && TargetSelector._currentTarget != null && !TargetSelector._currentTarget.IsDead)
				{
					if (!SettingsStore.VisibleCheck || this.IsVisible(TargetSelector._currentTarget))
					{
						return;
					}
					TargetSelector._currentTarget = null;
					TargetSelector._isActive = false;
				}
				TargetSelector._currentTarget = null;
				float num = float.MaxValue;
				global::UnityEngine.Vector3 forward = PlayerStateTracker.MainCamera.transform.forward;
				global::UnityEngine.Vector3 position = PlayerStateTracker.MainCamera.transform.position;
				foreach (SkyDome.Entity.PlayerData PlayerData in PlayerStateTracker.EntityList)
				{
					if (PlayerData.Team != PlayerStateTracker.LocalEntity.Team && !PlayerData.IsDead && (!SettingsStore.VisibleCheck || this.IsVisible(PlayerData)))
					{
						global::UnityEngine.Vector3 normalized = (this.GetAimPosition(PlayerData) - position).normalized;
						float num2 = global::UnityEngine.Vector3.Angle(forward, normalized);
						if (num2 <= (float)SettingsStore.TargetSelectorFOV * 0.5f && num2 < num)
						{
							num = num2;
							TargetSelector._currentTarget = PlayerData;
						}
					}
				}
				return;
			}
		}
		private void OnGUI()
		{
			if (SettingsStore.TargetSelector && SettingsStore.AimRange_Show)
			{
				this.DrawTargetSelectorFOVCircle();
			}
			if (SettingsStore.ShowAimLine)
			{
				this.DrawTargetLine();
			}
		}

        private void DrawTargetLine()
		{
			if (PlayerStateTracker.EntityList != null && !(PlayerStateTracker.MainCamera == null))
			{
				foreach (SkyDome.Entity.PlayerData PlayerData in PlayerStateTracker.EntityList)
				{
					if (PlayerData != null && PlayerData._entity != null)
					{
						global::UnityEngine.Vector3 aimPosition = this.GetAimPosition(PlayerData);
						global::UnityEngine.Vector3 vector = PlayerStateTracker.MainCamera.WorldToScreenPoint(aimPosition);
						if (ViewportUtility.IsScreenPointVisible(vector))
						{
							Vector2 vector2 = new Vector2(vector.x, vector.y);
							if (TargetSelector._currentTarget != null && TargetSelector._currentTarget._entity != null && TargetSelector._currentTarget._entity == PlayerData._entity)
							{
								FastRenderer.DrawLine(this.ScreenCenter, vector2, FastRenderer.GetRainbowColor(3f), 2f);
							}
						}
					}
				}
				return;
			}
		}
		private void Start()
		{
			SkyDome.Engine.InputDriver.PreInputCallback += this.OnPreInput;
		}
		private void ProcessAiming()
		{
			if (!Input.GetKey(SettingsStore.AimKey) || TargetSelector._currentTarget == null)
			{
				TargetSelector._isActive = false;
				return;
			}
			WeaponEntity currentWeaponEntity = Contexts.sharedInstance.weapon.currentWeaponEntity;
			if (currentWeaponEntity != null && currentWeaponEntity.slot.Slot > 3)
			{
				TargetSelector._isActive = false;
				return;
			}
			TargetSelector._isActive = true;
			global::UnityEngine.Vector3 aimPosition = this.GetAimPosition(TargetSelector._currentTarget);
			if (SettingsStore.TargetSelector_Smooth)
			{
				global::UnityEngine.Vector3 vector = PlayerStateTracker.MainCamera.WorldToScreenPoint(aimPosition);
				if (vector.z < 0f)
				{
					return;
				}
				Vector2 vector2 = new Vector2(vector.x, vector.y);
				Vector2 screenCenter = this.ScreenCenter;
				Vector2 vector3 = vector2 - screenCenter;
				float num = Mathf.Max(1f, SettingsStore.TargetSelector_SmoothFactor);
				float num2 = 0.15f;
				Vector2 vector4 = vector3 / num * num2;
				SkyDome.Engine.InputDriver.ForceAxisDelta += vector4;
				return;
			}
			else
			{
				Vector2 vector5 = this.CalculateAimAngles(aimPosition);
				if (SettingsStore.ViewStabilizer)
				{
					Contexts.sharedInstance.userCommand.input.Pitch = vector5.x - PlayerStateTracker.LocalEntity.Punch.x * 2f;
					Contexts.sharedInstance.userCommand.input.Yaw = vector5.y - PlayerStateTracker.LocalEntity.Punch.y * 2f;
					return;
				}
				Contexts.sharedInstance.userCommand.input.Pitch = vector5.x;
				Contexts.sharedInstance.userCommand.input.Yaw = vector5.y;
				return;
			}
		}

        private void OnPreInput()
		{
			if (SettingsStore.TargetSelector)
			{
				this.UpdateTarget();
				this.ProcessAiming();
				return;
			}
			TargetSelector._currentTarget = null;
			TargetSelector._isActive = false;
		}
		private global::UnityEngine.Vector3 ApplySpreadOffset(global::UnityEngine.Vector3 baseDirection)
		{
			int num = Contexts.sharedInstance.userCommand.commands.CommandToSendList.Last.Value.Seq + (1);
			double num2 = UniformRandom.RandomFloat(num, -0.5, 0.5) + UniformRandom.RandomFloat(num + (1), -0.5, 0.5);
			double num3 = UniformRandom.RandomFloat(num + (2), -0.5, 0.5) + UniformRandom.RandomFloat(num + 3, -0.5, 0.5);
			float spread = Contexts.sharedInstance.weapon.currentWeaponEntity.spread.Spread;
			float spreadScaleY = Contexts.sharedInstance.weapon.currentWeaponEntity.spread.SpreadScaleY;
			double num4 = (double)spread * num2 * this._currentSpreadFactor;
			double num5 = (double)spread * num3 * this._currentSpreadFactor * (double)spreadScaleY;
			global::UnityEngine.Vector3 normalized = global::UnityEngine.Vector3.Cross(baseDirection, global::UnityEngine.Vector3.up).normalized;
			global::UnityEngine.Vector3 normalized2 = global::UnityEngine.Vector3.Cross(normalized, baseDirection).normalized;
			return (baseDirection + normalized * (float)num4 + normalized2 * (float)num5).normalized;
		}

        private Vector2 CalculateAimAngles(global::UnityEngine.Vector3 targetPosition)
		{
			global::UnityEngine.Vector3 position = PlayerStateTracker.MainCamera.transform.position;
			global::UnityEngine.Vector3 vector = VectorCoordConverter.SsjjToUnity(PlayerStateTracker.LocalEntity.Move.Velocity);
			float num = (float)Contexts.sharedInstance.userCommand.commands.CommandToSendList.Last.Value.FrameInterval * 0.001f;
			global::UnityEngine.Vector3 vector2 = position + vector * num;
			global::UnityEngine.Vector3 vector3 = (targetPosition - vector2).normalized;
			if (SettingsStore.SpreadPredict && Input.GetKey(KeyCode.Mouse0))
			{
				vector3 = this.ApplySpreadOffset(vector3);
			}
			float num2 = Mathf.Asin(vector3.y) * 57.29578f;
			float num3 = Mathf.Atan2(vector3.z, vector3.x) * 57.29578f - 90f;
			float num4 = Mathf.Clamp(num2, -89f, 89f);
			num3 %= 360f;
			num3 = ((num3 > 180f) ? (num3 - 360f) : num3);
			return new Vector2(num4, num3);
		}
		public static bool _isActive;
		private double _currentSpreadFactor;
		public static SkyDome.Entity.PlayerData _currentTarget;
	}
}