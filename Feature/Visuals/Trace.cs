using System;
using System.Collections.Generic;
using SkyDome.Cfg;
using SkyDome.Entity;
using SkyDome.Render;
using UnityEngine;
namespace SkyDome.Feature.Visuals
{
	public class PathRenderer : MonoBehaviour
	{

        private void CheckForNewShot()
		{
			Contexts sharedInstance = Contexts.sharedInstance;
			if (sharedInstance != null)
			{
				if (sharedInstance.weapon != null)
				{
					WeaponEntity currentWeaponEntity = Contexts.sharedInstance.weapon.currentWeaponEntity;
					if (currentWeaponEntity == null || currentWeaponEntity.basicInfo == null || currentWeaponEntity.basicInfo.Data == null)
					{
						return;
					}
					int shotsFired = currentWeaponEntity.basicInfo.Data.ShotsFired;
					if (shotsFired == 0)
					{
						this._lastShotIndex = 0;
						return;
					}
					if (shotsFired > this._lastShotIndex)
					{
						this.CreatePathRendererr(shotsFired);
						this._lastShotIndex = shotsFired;
					}
					return;
				}
			}
		}
		private void Update()
		{
			PathRenderer._activePathRendererrs.RemoveAll((PathRenderer.PathRendererrData PathRendererr) => PathRendererr.IsExpired());
			this.CheckForNewShot();
		}
		public static void AddPathRendererr(Vector3 start, Vector3 end, Color color, float duration, int shotIndex)
		{
			PathRenderer._activePathRendererrs.Add(new PathRenderer.PathRendererrData(start, end, color, duration, shotIndex));
		}
		private void OnGUI()
		{
			if (Camera.main != null && SettingsStore.ShowPathRendererrs)
			{
				foreach (PathRenderer.PathRendererrData PathRendererrData in PathRenderer._activePathRendererrs)
				{
					FastRenderer.DrawLinearPathRendererr(PathRendererrData.Start, PathRendererrData.End, FastRenderer.GetRainbowColor(3f));
				}
				return;
			}
		}
		private void CreatePathRendererr(int shotIndex)
		{
			if (PlayerStateTracker.MainCamera == null && Contexts.sharedInstance.player.myPlayerEntity.currentWeapon.Weapon > 2)
			{
				return;
			}
			Vector3 position = PlayerStateTracker.MainCamera.transform.position;
			Vector3 forward = PlayerStateTracker.MainCamera.transform.forward;
			Ray ray = new Ray(position, forward);
			float num = 5000f;
			RaycastHit raycastHit;
			Vector3 vector;
			if (Physics.Raycast(ray, out raycastHit, num))
			{
				vector = raycastHit.point;
			}
			else
			{
				vector = position + forward * num;
			}
			PathRenderer._activePathRendererrs.Add(new PathRenderer.PathRendererrData((position), (vector), (Color.black), (1f), (shotIndex)));
		}

        public static void ClearAllPathRendererrs()
		{
			PathRenderer._activePathRendererrs.Clear();
		}
		private static List<PathRenderer.PathRendererrData> _activePathRendererrs = new List<PathRenderer.PathRendererrData>();
		private int _lastShotIndex;
		private class PathRendererrData
		{
			public bool IsExpired()
			{
				return Time.time - this.CreateTime >= this.Duration;
			}
			public PathRendererrData(Vector3 start, Vector3 end, Color color, float duration, int shotIndex)
			{
				this.Start = start;
				this.End = end;
				this.Color = color;
				this.CreateTime = Time.time;
				this.Duration = duration;
				this.ShotIndex = shotIndex;
			}
			public Color Color;
			public float CreateTime;
			public float Duration;
			public int ShotIndex;
			public Vector3 Start;
			public Vector3 End;
		}
	}
}