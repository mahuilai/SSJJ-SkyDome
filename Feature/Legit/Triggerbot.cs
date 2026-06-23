using System;
using Assets.Sources.Components.Interface.Info.Weapon;
using Assets.Sources.Components.Weapon;
using Assets.Sources.Utils.Weapon;
using NetData;
using share;
using SkyDome.Cfg;
using SkyDome.Engine;
using SkyDome.Entity;
using SSJJMath;
using SSJJPhysics;
using UnityEngine;
using weapon;
namespace SkyDome.Feature.Legit
{
	public class AutoFireController : MonoBehaviour
	{
		private void TryTriggerShooting()
		{
			if (Time.time - this._lastTriggerTime >= 0.01f)
			{
				SkyDome.Engine.InputDriver.ForceMouseButton(0, (SkyDome.Engine.InputDriver.InputState)2);
				this._lastTriggerTime = Time.time;
			}
		}
		public static bool IsActive
		{
			get
			{
				return AutoFireController._isAutoFireControllerActive;
			}
		}
		private Vector3D CalculateShotDirection()
		{
			float num = PlayerStateTracker.LocalEntity.ViewPos.y + 2f * PlayerStateTracker.LocalEntity.Punch.y;
			float num2 = PlayerStateTracker.LocalEntity.ViewPos.x + 2f * PlayerStateTracker.LocalEntity.Punch.x;
			int num3 = Contexts.sharedInstance.userCommand.commands.CommandToSendList.Last.Value.Seq + 1;
			float spread = Contexts.sharedInstance.weapon.currentWeaponEntity.spread.Spread;
			float spreadScaleY = Contexts.sharedInstance.weapon.currentWeaponEntity.spread.SpreadScaleY;
			BasicInfoComponent basicInfo = Contexts.sharedInstance.weapon.currentWeaponEntity.basicInfo;
			IEntitsWeaponInfo info = basicInfo.Info;
			BaseWeaponData data = basicInfo.Data;
			double num4 = FireUtility.CalShotsFiredSpread(data.ShotsFiredSpreadMin, data.ShotsFiredSpreadMax, data.ShotsFiredSpreadTime, data.ShotsFired, info.AttackInterval);
			return ShootingDirUtils.CalculateShotingDir(num3, (double)num, (double)num2, (double)spread, spreadScaleY, num4);
		}
		public static float RemainingTime
		{
			get
			{
				if (!AutoFireController._isAutoFireControllerActive)
				{
					return 0f;
				}
				float num = Time.time - AutoFireController._AutoFireControllerActivatedTime;
				return Mathf.Max(0f, SettingsStore.AutoFireControllerActiveDuration - num);
			}
		}

        private void Update()
		{
			if (!SettingsStore.AutoFireController)
			{
				AutoFireController._isAutoFireControllerActive = false;
				this._lastShotsFired = 0;
				this._lastWeaponSlot = 0;
				return;
			}
			if (PlayerStateTracker.LocalEntity == null || PlayerStateTracker.LocalEntity.IsDead)
			{
				AutoFireController._isAutoFireControllerActive = false;
				this._lastShotsFired = 0;
				this._lastWeaponSlot = 0;
				return;
			}
			WeaponContext weapon = Contexts.sharedInstance.weapon;
			if (weapon == null || weapon.currentWeaponEntity == null)
			{
				return;
			}
			int currentWeaponId = PlayerStateTracker.LocalEntity.CurrentWeaponId;
			if (currentWeaponId != this._lastWeaponSlot)
			{
				AutoFireController._isAutoFireControllerActive = false;
				this._lastShotsFired = 0;
				this._lastWeaponSlot = currentWeaponId;
				return;
			}
			bool flag = weapon.currentWeaponEntity.basicInfo.Info.WeaponType == 5;
			bool flag2 = currentWeaponId == 1;
			bool flag3 = SettingsStore.AutoFireControllerDelayedActivation && flag2 && !flag;
			if (flag && !PlayerStateTracker.LocalEntity.Fov.IsZoom() && SettingsStore.ExcludeSniper)
			{
				return;
			}
			int shotsFired = weapon.currentWeaponEntity.basicInfo.Data.ShotsFired;
			if (shotsFired > this._lastShotsFired && flag3)
			{
				AutoFireController._isAutoFireControllerActive = true;
				AutoFireController._AutoFireControllerActivatedTime = Time.time;
			}
			this._lastShotsFired = shotsFired;
			if (flag3)
			{
				if (AutoFireController._isAutoFireControllerActive && Time.time - AutoFireController._AutoFireControllerActivatedTime > SettingsStore.AutoFireControllerActiveDuration)
				{
					AutoFireController._isAutoFireControllerActive = false;
				}
				if (!AutoFireController._isAutoFireControllerActive)
				{
					return;
				}
			}
			Vector3 vector = VectorCoordConverter.UnityToSsjj(Camera.main.transform.forward);
			Vector3D vector3D = (SettingsStore.SpreadPredict ? this.CalculateShotDirection() : new Vector3D((double)vector.x, (double)vector.y, (double)vector.z));
			int entityId = SkyDome.Utilities.PathRendererHelper.GetEntityId(
				Contexts.sharedInstance.battleRoom.pyEngine.PyEngine, 
				PlayerStateTracker.LocalEntity._entity, 
				Contexts.sharedInstance.player, 
				100000f, 
				vector3D, 
				new float[3], 
				new float[3], 
				false
			);
			if (entityId <= 0)
			{
				return;
			}
			foreach (SkyDome.Entity.PlayerData PlayerData in PlayerStateTracker.EntityList)
			{
				if (PlayerData.Team != PlayerStateTracker.LocalEntity.Team && PlayerData.Id == entityId)
				{
					if (this.IsValidTarget(PlayerData))
					{
						this.TryTriggerShooting();
						break;
					}
					break;
				}
			}
		}

        public static float ActivatedTime
		{
			get
			{
				return AutoFireController._AutoFireControllerActivatedTime;
			}
		}
		private bool IsValidTarget(SkyDome.Entity.PlayerData target)
		{
			return target != null && !target.IsDead && target.Team != PlayerStateTracker.LocalEntity.Team && !target.State.GetPlayerStateType(1);
		}
		private static bool _isAutoFireControllerActive;
		private float _lastTriggerTime;
		private int _lastWeaponSlot;
		private static float _AutoFireControllerActivatedTime;
		private int _lastShotsFired;
	}
}