using System;
using SkyDome.Entity;
using UnityEngine;
namespace SkyDome.Utilities
{
	public static class ViewportUtility
	{
		public static Vector3 WorldPointToScreenPoint(Vector3 worldPoint)
		{
			Vector3 vector = PlayerStateTracker.MainCamera.WorldToScreenPoint(worldPoint);
			float num = (float)Screen.height / (float)PlayerStateTracker.MainCamera.scaledPixelHeight;
			vector.y = (float)Screen.height - vector.y * num;
			vector.x *= num;
			return vector;
		}

        public static bool IsScreenPointVisible(Vector3 screenPoint)
		{
			return screenPoint.z > 0.01f && screenPoint.x > -5f && screenPoint.y > -5f && screenPoint.x < (float)Screen.width && screenPoint.y < (float)Screen.height;
		}
	}
}