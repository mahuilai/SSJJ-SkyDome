using System;
using System.Collections.Generic;
using SkyDome.Entity;
using SkyDome.Utilities;
using SSJJBase.String;
using UnityEngine;
namespace SkyDome.Render
{
	public class OverlayHost
	{
		private static void DrawBoneConnection(Dictionary<IgnoreCaseString, Vector3> boneScreenPositions, string boneName1, string boneName2, Color color, float thickness = 1f)
		{
			IgnoreCaseString ignoreCaseString = boneName1;
			IgnoreCaseString ignoreCaseString2 = boneName2;
			Vector3 vector;
			Vector3 vector2;
			if (boneScreenPositions.TryGetValue(ignoreCaseString, out vector) && boneScreenPositions.TryGetValue(ignoreCaseString2, out vector2))
			{
				Vector2 vector3 = new Vector2(vector.x, (float)Screen.height - vector.y);
				Vector2 vector4 = new Vector2(vector2.x, (float)Screen.height - vector2.y);
				FastRenderer.DrawLine(vector3, vector4, color, thickness);
			}
		}

        public static void DrawSkeleton(SkyDome.Entity.PlayerData enemy, Color color, float thickness = 1f)
		{
			if (enemy == null)
			{
				return;
			}
			Dictionary<IgnoreCaseString, Vector3> dictionary = new Dictionary<IgnoreCaseString, Vector3>();
			Dictionary<IgnoreCaseString, Transform> playerAllTransform = enemy.GetPlayerAllTransform();
			if (playerAllTransform == null)
			{
				return;
			}
			foreach (KeyValuePair<IgnoreCaseString, Transform> keyValuePair in playerAllTransform)
			{
				if (!(keyValuePair.Value == null))
				{
					Vector3 vector = ViewportUtility.WorldPointToScreenPoint(keyValuePair.Value.position);
					dictionary[keyValuePair.Key] = vector;
				}
			}
			OverlayHost.DrawBoneConnection(dictionary, "Bip01_Pelvis", "Bip01_Spine", color, thickness);
			OverlayHost.DrawBoneConnection(dictionary, "Bip01_Spine", "Bip01_Spine1", color, thickness);
			OverlayHost.DrawBoneConnection(dictionary, "Bip01_Spine1", "Bip01_Spine2", color, thickness);
			OverlayHost.DrawBoneConnection(dictionary, "Bip01_Spine2", "Bip01_Neck", color, thickness);
			OverlayHost.DrawBoneConnection(dictionary, "Bip01_Neck", "Bip01_Head", color, thickness);
			Vector3 vector2;
			if (dictionary.TryGetValue("Bip01_Head", out vector2) && enemy.GetPlayerTransform("Bip01_Head").GetChild(0) != null)
			{
				Vector3 vector3 = ViewportUtility.WorldPointToScreenPoint(enemy.GetPlayerTransform("Bip01_Head").GetChild(0).position);
				Vector3 vector4 = (vector2 + vector3) * 0.5f;
				Vector2 vector5 = new Vector2(vector4.x, (float)Screen.height - vector4.y);
				float num = Vector3.Distance(vector2, vector3) * 0.5f;
				FastRenderer.DrawCircleOutline(vector5, num, 32, color, true);
			}
			OverlayHost.DrawBoneConnection(dictionary, "Bip01_Pelvis", "Bip01_L_Thigh", color, thickness);
			OverlayHost.DrawBoneConnection(dictionary, "Bip01_L_Thigh", "Bip01_L_Calf", color, thickness);
			OverlayHost.DrawBoneConnection(dictionary, "Bip01_L_Calf", "Bip01_L_Foot", color, thickness);
			OverlayHost.DrawBoneConnection(dictionary, "Bip01_Pelvis", "Bip01_R_Thigh", color, thickness);
			OverlayHost.DrawBoneConnection(dictionary, "Bip01_R_Thigh", "Bip01_R_Calf", color, thickness);
			OverlayHost.DrawBoneConnection(dictionary, "Bip01_R_Calf", "Bip01_R_Foot", color, thickness);
			OverlayHost.DrawBoneConnection(dictionary, "Bip01_Spine2", "Bip01_L_Clavicle", color, thickness);
			OverlayHost.DrawBoneConnection(dictionary, "Bip01_L_Clavicle", "Bip01_L_UpperArm", color, thickness);
			OverlayHost.DrawBoneConnection(dictionary, "Bip01_L_UpperArm", "Bip01_L_Forearm", color, thickness);
			OverlayHost.DrawBoneConnection(dictionary, "Bip01_L_Forearm", "Bip01_L_Hand", color, thickness);
			OverlayHost.DrawBoneConnection(dictionary, "Bip01_Spine2", "Bip01_R_Clavicle", color, thickness);
			OverlayHost.DrawBoneConnection(dictionary, "Bip01_R_Clavicle", "Bip01_R_UpperArm", color, thickness);
			OverlayHost.DrawBoneConnection(dictionary, "Bip01_R_UpperArm", "Bip01_R_Forearm", color, thickness);
			OverlayHost.DrawBoneConnection(dictionary, "Bip01_R_Forearm", "Bip01_R_Hand", color, thickness);
		}
		public static void DrawVerticalHealthBar(Rect targetRect, float healthPercent, float barWidth = 4f, float barSpacing = 2f, bool onLeft = true)
		{
			Rect rect;
			if (onLeft)
			{
				rect = new Rect(targetRect.x - barWidth - barSpacing, targetRect.y, barWidth, targetRect.height);
			}
			else
			{
				rect = new Rect(targetRect.x + targetRect.width + barSpacing, targetRect.y, barWidth, targetRect.height);
			}
			float num = rect.height * healthPercent;
			Rect rect2 = new Rect(rect.x, rect.y, rect.width, num);
			FastRenderer.DrawBoxFilled(rect, new Color(0.3f, 0.3f, 0.3f, 0.7f));
			Color color = Color.Lerp(Color.red, Color.green, healthPercent);
			FastRenderer.DrawBoxFilled(rect2, color);
			FastRenderer.DrawBoxOutline(rect, Color.black, 1f);
		}
	}
}