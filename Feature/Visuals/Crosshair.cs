using System;
using SkyDome.Cfg;
using SkyDome.Entity;
using SkyDome.Render;
using UnityEngine;
namespace SkyDome.Feature.Visuals
{
	public class ReticleRenderer : MonoBehaviour
	{

        private void OnGUI()
		{
			if (!SettingsStore.ShowReticleRenderer)
			{
				return;
			}
			if (PlayerStateTracker.LocalEntity == null || PlayerStateTracker.LocalEntity.IsDead)
			{
				return;
			}
			WeaponContext weapon = Contexts.sharedInstance.weapon;
			if (weapon != null && weapon.currentWeaponEntity != null)
			{
				bool flag = weapon.currentWeaponEntity.basicInfo.Info.WeaponType == 5;
				bool flag2 = PlayerStateTracker.LocalEntity.Fov.IsZoom();
				if (flag && !flag2)
				{
					this.DrawReticleRenderer();
				}
				return;
			}
		}
		private void DrawReticleRenderer()
		{
			FastRenderer.DrawReticleRenderer(new Vector2((float)Screen.width / 2f, (float)Screen.height / 2f), new Color(1f, 0.75f, 0.8f), 15f, 2f, 5f, false, false);
		}
	}
}