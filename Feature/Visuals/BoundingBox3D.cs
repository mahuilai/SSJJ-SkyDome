using System;
using SkyDome.Cfg;
using SkyDome.Entity;
using SkyDome.Render;
using SkyDome.Utilities;
using UnityEngine;
namespace SkyDome.Feature.Visuals
{
	public class EntityBoxRenderer : MonoBehaviour
	{
		private void OnGUI()
		{
			if (!SettingsStore.Show3DBox || !SettingsStore.EntityVisualizer || PlayerStateTracker.EntityList == null)
			{
				return;
			}
			if (PlayerStateTracker.MainCamera == null)
			{
				return;
			}
			foreach (SkyDome.Entity.PlayerData PlayerData in PlayerStateTracker.EntityList)
			{
				if (PlayerData.Team != PlayerStateTracker.LocalEntity.Team && !PlayerData.IsDead)
				{
					this.Draw3DBoundingBox(PlayerData);
				}
			}
		}

        private void DrawBox3DEdges(Vector2[] p, Color color)
		{
			float num = 1.5f;
			FastRenderer.DrawLine(p[0], p[1], color, num);
			FastRenderer.DrawLine(p[1], p[2], color, num);
			FastRenderer.DrawLine(p[2], p[3], color, num);
			FastRenderer.DrawLine(p[3], p[0], color, num);
			FastRenderer.DrawLine(p[4], p[5], color, num);
			FastRenderer.DrawLine(p[5], p[6], color, num);
			FastRenderer.DrawLine(p[6], p[7], color, num);
			FastRenderer.DrawLine(p[7], p[4], color, num);
			FastRenderer.DrawLine(p[0], p[4], color, num);
			FastRenderer.DrawLine(p[1], p[5], color, num);
			FastRenderer.DrawLine(p[2], p[6], color, num);
			FastRenderer.DrawLine(p[3], p[7], color, num);
		}
		private Bounds CalculateTightBounds(SkyDome.Entity.PlayerData player)
		{
			Vector3 vector = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
			Vector3 vector2 = new Vector3(float.MinValue, float.MinValue, float.MinValue);
			bool flag = false;
			foreach (string text in EntityBoxRenderer.KeyBones)
			{
				Transform playerTransform = player.GetPlayerTransform(text);
				if (!(playerTransform == null))
				{
					Vector3 position = playerTransform.position;
					vector = Vector3.Min(vector, position);
					vector2 = Vector3.Max(vector2, position);
					flag = true;
				}
			}
			if (!flag)
			{
				return default(Bounds);
			}
			Vector3 vector3 = (vector + vector2) * 0.5f;
			Vector3 vector4 = vector2 - vector;
			return new Bounds(vector3, vector4);
		}
		static EntityBoxRenderer()
		{
			string[] array = new string[41];
			array[0] = "Bip01";
			array[1] = "Bip01_Pelvis";
			array[2] = "Bip01_Spine";
			array[3] = "Bip01_Spine1";
			array[4] = "Bip01_Neck";
			array[5] = "Bip01_Head";
			array[6] = "Bip01_HeadNub";
			array[7] = "Bip01_L_Thigh";
			array[8] = "Bip01_L_Calf";
			array[9] = "Bip01_L_Foot";
			array[10] = "Bip01_L_Toe0";
			array[11] = "Bip01_R_Thigh";
			array[12] = "Bip01_R_Calf";
			array[13] = "Bip01_R_Foot";
			array[14] = "Bip01_R_Toe0";
			array[15] = "Bip01_L_Clavicle";
			array[16] = "Bip01_L_UpperArm";
			array[17] = "Bip01_L_Forearm";
			array[18] = "Bip01_L_Hand";
			array[19] = "Bip01_L_Finger0";
			array[20] = "Bip01_L_Finger01";
			array[21] = "Bip01_L_Finger0Nub";
			array[22] = "Bip01_L_Finger1";
			array[23] = "Bip01_L_Finger11";
			array[24] = "Bip01_L_Finger1Nub";
			array[25] = "Bip01_L_Finger2";
			array[26] = "Bip01_L_Finger21";
			array[27] = "Bip01_L_Finger2Nub";
			array[28] = "Bip01_R_Clavicle";
			array[29] = "Bip01_R_UpperArm";
			array[30] = "Bip01_R_Forearm";
			array[31] = "Bip01_R_Hand";
			array[32] = "Bip01_R_Finger0";
			array[33] = "Bip01_R_Finger01";
			array[34] = "Bip01_R_Finger0Nub";
			array[35] = "Bip01_R_Finger1";
			array[36] = "Bip01_R_Finger11";
			array[37] = "Bip01_R_Finger1Nub";
			array[38] = "Bip01_R_Finger2";
			array[39] = "Bip01_R_Finger21";
			array[40] = "Bip01_R_Finger2Nub";
			EntityBoxRenderer.KeyBones = array;
		}

        private Vector3[] GetBoundsCorners(Bounds bounds)
		{
			Vector3 min = bounds.min;
			Vector3 max = bounds.max;
			Vector3[] array = new Vector3[8];
			array[0] = new Vector3(min.x, min.y, min.z);
			array[1] = new Vector3(max.x, min.y, min.z);
			array[2] = new Vector3(max.x, min.y, max.z);
			array[3] = new Vector3(min.x, min.y, max.z);
			array[4] = new Vector3(min.x, max.y, min.z);
			array[5] = new Vector3(max.x, max.y, min.z);
			array[6] = new Vector3(max.x, max.y, max.z);
			array[7] = new Vector3(min.x, max.y, max.z);
			return array;
		}
		private void Draw3DBoundingBox(SkyDome.Entity.PlayerData player)
		{
			Bounds bounds = this.CalculateTightBounds(player);
			if (bounds.size == Vector3.zero)
			{
				return;
			}
			Vector3[] boundsCorners = this.GetBoundsCorners(bounds);
			Vector2[] array = new Vector2[8];
			for (int i = 0; i < 8; i++)
			{
				Vector3 vector = this.WorldToScreenPoint(boundsCorners[i]);
				if (vector.z < 0f)
				{
					return;
				}
				array[i] = new Vector2(vector.x, vector.y);
			}
			this.DrawBox3DEdges(array, Color.cyan);
		}
		private Vector3 WorldToScreenPoint(Vector3 worldPoint)
		{
			Camera mainCamera = PlayerStateTracker.MainCamera;
			Vector3 vector = mainCamera.WorldToScreenPoint(worldPoint);
			float num = (float)Screen.height / (float)mainCamera.scaledPixelHeight;
			vector.x *= num;
			vector.y *= num;
			return vector;
		}
		private static readonly string[] KeyBones;
	}
}