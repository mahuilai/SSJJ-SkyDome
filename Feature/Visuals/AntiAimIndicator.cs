using System;
using SkyDome.Cfg;
using SkyDome.Entity;
using SkyDome.Render;
using UnityEngine;
namespace SkyDome.Feature.Visuals
{
	public class DirectionDisplay : MonoBehaviour
	{
		private void DrawDirectionIndicator(float angle)
		{
			float num = (float)Screen.width / 2f;
			float num2 = (float)Screen.height / 2f;
			float num3 = 15f;
			float num4 = 30f;
			bool flag = angle == -90f;
			Color white = Color.white;
			Color color = Color.white * 0.2f;
			Vector2 vector = new Vector2(num - num4 - num3, num2);
			Vector2 vector2 = new Vector2(num - num4, num2 - 7.5f);
			Vector2 vector3 = new Vector2(num - num4, num2 + 7.5f);
			Vector2 vector4 = new Vector2(num + num4 + num3, num2);
			Vector2 vector5 = new Vector2(num + num4, num2 - 7.5f);
			Vector2 vector6 = new Vector2(num + num4, num2 + 7.5f);
			if (angle == -180f)
			{
				FastRenderer.DrawFilledTriangle(vector4, vector6, vector5, color);
				FastRenderer.DrawFilledTriangle(vector, vector2, vector3, color);
				return;
			}
			FastRenderer.DrawFilledTriangle(vector4, vector6, vector5, flag ? color : white);
			FastRenderer.DrawFilledTriangle(vector, vector2, vector3, flag ? white : color);
		}

        private void OnGUI()
		{
			if (SettingsStore.AngleCorrector && PlayerStateTracker.LocalEntity != null && !PlayerStateTracker.LocalEntity.IsDead)
			{
				if (SettingsStore.AngleCorrector_Yaw == 90f || SettingsStore.AngleCorrector_Yaw == -90f || SettingsStore.AngleCorrector_Yaw == -180f)
				{
					this.DrawDirectionIndicator(SettingsStore.AngleCorrector_Yaw);
				}
				return;
			}
		}
	}
}