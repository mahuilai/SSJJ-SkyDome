using System;
using Assets.Sources.Free.Data;
using SkyDome.Cfg;
using SkyDome.Entity;
using SkyDome.Feature.Legit;
using UnityEngine;
namespace SkyDome.Feature
{
	public class ViewStabilizer : MonoBehaviour
	{
		private void Update()
		{
			if (SettingsStore.ViewStabilizer)
			{
				float x = PlayerStateTracker.LocalEntity.Punch.x;
				float y = PlayerStateTracker.LocalEntity.Punch.y;
				Contexts.sharedInstance.userCommand.input.Pitch -= 2f * (x - this.vector.x);
				Contexts.sharedInstance.userCommand.input.Yaw -= 2f * (y - this.vector.y);
				Camera.main.transform.Rotate(-this.vector.x - GameModelLocator.GetInstance().GameModel.ShakeAngleOffect.y, -this.vector.y - GameModelLocator.GetInstance().GameModel.ShakeAngleOffect.x, 0f);
				this.vector.x = x;
				this.vector.y = y;
			}
		}

        private void LateUpdate()
		{
			if (SettingsStore.ViewStabilizer && TargetSelector._isActive && Contexts.sharedInstance.weapon.currentWeaponEntity.slot.Slot < 3 && SettingsStore.SmoothControl)
			{
				if (Input.GetMouseButton(0))
				{
					Camera.main.transform.localRotation = Quaternion.Slerp(this.lastQua, Camera.main.transform.localRotation, Time.deltaTime * 11f);
				}
				this.lastQua = Camera.main.transform.localRotation;
			}
		}
		private Vector2 vector;
		private Quaternion lastQua;
	}
}