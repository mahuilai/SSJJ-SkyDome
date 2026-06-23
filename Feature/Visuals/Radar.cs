using System;
using SkyDome.Cfg;
using SkyDome.Entity;
using SkyDome.Render;
using SkyDome.Utilities;
using UnityEngine;
namespace SkyDome.Feature.Visuals
{
	public class MiniMapOverlay : MonoBehaviour
	{
		private void DrawMiniMapOverlayBackground()
		{
			FastRenderer.DrawPhallicOutline(this.ScreenCenter, 167.25f, FastRenderer.GetRainbowColor(5f), 2f);
			FastRenderer.DrawPhallicOutline(this.ScreenCenter, 155f, FastRenderer.GetRainbowColor(5f, 0.5f), 2f);
		}
		private void DrawMiniMapOverlayMarker(Vector2 position, Vector3 direction, Color color)
		{
			Vector2 vector = position - new Vector2(direction.x, direction.y) * 3.5f;
			FastRenderer.DrawCircleFilled(vector, 7f, color, 16);
			Vector2 vector2 = vector + new Vector2(direction.x, direction.y) * 12f;
			Vector2 vector3 = new Vector2(-direction.y, direction.x);
			FastRenderer.DrawLine(vector2, vector, color, 2f);
			for (int i = 1; i <= 4; i++)
			{
				Vector2 vector4 = vector + vector3 * (float)i;
				Vector2 vector5 = vector - vector3 * (float)i;
				FastRenderer.DrawLine(vector2, vector4, color, 1.5f);
				FastRenderer.DrawLine(vector2, vector5, color, 1.5f);
			}
		}

        private Vector2 CalculateMiniMapOverlayPosition(SkyDome.Entity.PlayerData enemy, Vector3 cameraPosition, Quaternion MiniMapOverlayRotation)
		{
			Vector3 vector = enemy.GetPlayerTransform(enemy.PlayerName).position - cameraPosition;
			Vector2 vector2 = new Vector2(vector.x, vector.z);
			return Vector2.ClampMagnitude(MiniMapOverlayRotation * vector2 * (float)Screen.height * 2.4E-07f * 167.25f, 159.25f) + this.ScreenCenter;
		}
		private Vector3 CalculateArrowDirection(float enemyYaw, Quaternion MiniMapOverlayRotation)
		{
			Quaternion quaternion = Quaternion.AngleAxis(enemyYaw, Vector3.forward);
			return MiniMapOverlayRotation * quaternion * Vector3.up;
		}
		private void OnGUI()
		{
			if (SettingsStore.ShowMiniMapOverlay && PlayerStateTracker.LocalEntity != null && !(PlayerStateTracker.MainCamera == null))
			{
				this.DrawMiniMapOverlayBackground();
				this.DrawEnemyMarkers();
				return;
			}
		}
		private Vector2 ScreenCenter
		{
			get
			{
				return new Vector2((float)Screen.width * 0.5f, (float)Screen.height * 0.5f);
			}
		}
		private bool ShouldSkipEnemy(SkyDome.Entity.PlayerData enemy, int localTeam)
		{
			if (enemy != null && enemy._entity.hasBasicInfo && !enemy.IsDead)
			{
				return enemy.Team == localTeam;
			}
			return true;
		}

        private void DrawEnemyMarkers()
		{
			if (PlayerStateTracker.EntityList == null)
			{
				return;
			}
			Vector3 position = PlayerStateTracker.MainCamera.transform.position;
			Quaternion quaternion = Quaternion.AngleAxis(PlayerStateTracker.LocalEntity.ViewPos.y, Vector3.back);
			int team = PlayerStateTracker.LocalEntity.Team;
			int enemyIndex = 0;
			foreach (SkyDome.Entity.PlayerData PlayerData in PlayerStateTracker.EntityList)
			{
				if (!this.ShouldSkipEnemy(PlayerData, team))
				{
					Vector2 vector = this.CalculateMiniMapOverlayPosition(PlayerData, position, quaternion);
					float y = PlayerData.ViewPos.y;
					Vector3 vector2 = this.CalculateArrowDirection(y, quaternion);
					Color enemyColor = FastRenderer.GetRainbowColor(5f, enemyIndex * 0.15f);
					this.DrawMiniMapOverlayMarker(vector, vector2, enemyColor);
					enemyIndex++;
				}
			}
		}
	}
}