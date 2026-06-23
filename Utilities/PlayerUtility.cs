using System;
using System.Collections.Generic;
using Assets.Sources.Components.Player.UnityObjects;
using SkyDome.Entity;
using SSJJBase.String;
using UnityEngine;
namespace SkyDome.Utilities
{
	public static class PlayerUtility
	{
		public static Transform GetPlayerTransform(this SkyDome.Entity.PlayerData player, string name)
		{
			if (player == null)
			{
				throw new ArgumentNullException("player");
			}
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentException("Transform name cannot be null or empty", "name");
			}
			ThirdPersonUnityObjectsComponent thirdPersonUnityObjects = player.ThirdPersonUnityObjects;
			Dictionary<IgnoreCaseString, Transform> dictionary = ((thirdPersonUnityObjects != null) ? thirdPersonUnityObjects.AllPlayerTransforms : null);
			if (dictionary != null && dictionary.Count != 0)
			{
				IgnoreCaseString ignoreCaseString = name;
				using (Dictionary<IgnoreCaseString, Transform>.Enumerator enumerator = dictionary.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						KeyValuePair<IgnoreCaseString, Transform> keyValuePair = enumerator.Current;
						if (keyValuePair.Key.Equals(ignoreCaseString))
						{
							return keyValuePair.Value;
						}
					}
					goto IL_0229;
				}
				Transform transform;
				return transform;
				IL_0229:
				return null;
			}
			return null;
		}

        public static Transform GetValidHeadNub(this SkyDome.Entity.PlayerData player)
		{
			Transform playerTransform = player.GetPlayerTransform("Bip01_HeadNub");
			if (playerTransform != null)
			{
				return playerTransform;
			}
			Transform playerTransform2 = player.GetPlayerTransform("Bip01_Head");
			if (playerTransform2 == null)
			{
				return null;
			}
			if (playerTransform2.childCount == 0)
			{
				return new GameObject("fake_HeadNub")
				{
					transform = 
					{
						parent = playerTransform2,
						localPosition = new Vector3(-21.7f, 0f, 0f)
					}
				}.transform;
			}
			return playerTransform2.GetChild(0);
		}
		public static Dictionary<IgnoreCaseString, Transform> GetPlayerAllTransform(this SkyDome.Entity.PlayerData player)
		{
			ThirdPersonUnityObjectsComponent thirdPersonUnityObjects = player.ThirdPersonUnityObjects;
			if (thirdPersonUnityObjects == null)
			{
				return null;
			}
			return thirdPersonUnityObjects.AllPlayerTransforms;
		}
		public static float Length(this Vector3 vector)
		{
			return Mathf.Sqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);
		}

        public static Transform GetPlayerTransform(this PlayerEntity player, string name)
		{
			if (player == null)
			{
				throw new ArgumentNullException("player");
			}
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentException("Transform name cannot be null or empty", "name");
			}
			ThirdPersonUnityObjectsComponent thirdPersonUnityObjects = player.thirdPersonUnityObjects;
			Dictionary<IgnoreCaseString, Transform> dictionary = ((thirdPersonUnityObjects != null) ? thirdPersonUnityObjects.AllPlayerTransforms : null);
			if (dictionary != null && dictionary.Count != 0)
			{
				IgnoreCaseString ignoreCaseString = name;
				using (Dictionary<IgnoreCaseString, Transform>.Enumerator enumerator = dictionary.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						KeyValuePair<IgnoreCaseString, Transform> keyValuePair = enumerator.Current;
						if (keyValuePair.Key.Equals(ignoreCaseString))
						{
							return keyValuePair.Value;
						}
					}
					goto IL_01BD;
				}
				Transform transform;
				return transform;
				IL_01BD:
				return null;
			}
			return null;
		}
	}
}