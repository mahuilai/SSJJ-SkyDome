using System;
using System.Collections.Generic;
using System.Reflection;
using Assets.Sources.Components.Player.UnityObjects;
using Entitas;
using UnityEngine;
namespace SkyDome.Entity
{
	public class PlayerStateTracker : MonoBehaviour
	{

        private void RetrievePlayerData(IGroup<PlayerEntity> playerEntities, out PlayerData localPlayer, out PlayerData cameraOwner, out PlayerData predictionTarget, out List<PlayerData> entityList)
		{
			localPlayer = null;
			cameraOwner = null;
			predictionTarget = null;
			entityList = new List<PlayerData>();
			if (playerEntities == null)
			{
				return;
			}
			foreach (PlayerEntity playerEntity in playerEntities)
			{
				bool flag = false;
				if (playerEntity.isCameraOwner)
				{
					cameraOwner = new PlayerData(playerEntity);
					flag = true;
				}
				if (playerEntity.isMyPlayer)
				{
					localPlayer = new PlayerData(playerEntity);
					flag = true;
				}
				if (playerEntity.isPrediction)
				{
					predictionTarget = new PlayerData(playerEntity);
					flag = true;
				}
				if (!flag)
				{
					entityList.Add(new PlayerData(playerEntity));
				}
			}
		}

		private int _frameCount = 0;

        private void Update()
		{
			try
			{
				_frameCount++;
				if (_frameCount <= 5 || _frameCount % 60 == 0)
					Debug.Log(string.Format("[加载器] PlayerStateTracker.Update 第 {0} 帧", _frameCount));
				IGroup<PlayerEntity> group = this.PlayerEntityUpdate();
				this.ResetPlayerTransformCache(group);
				this.RetrievePlayerData(group, out PlayerStateTracker.LocalEntity, out PlayerStateTracker.CameraEntity, out PlayerStateTracker.PredictionEntity, out PlayerStateTracker.EntityList);
				PlayerStateTracker.MainCamera = Camera.main;
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		private IGroup<PlayerEntity> PlayerEntityUpdate()
		{
			IContext<PlayerEntity> player = Contexts.sharedInstance.player;
			IMatcher<PlayerEntity>[] array = new IMatcher<PlayerEntity>[2];
			array[0] = PlayerMatcher.BasicInfo;
			array[1] = PlayerMatcher.ThirdPersonUnityObjects;
			return player.GetGroup(PlayerMatcher.AllOf(array));
		}

        private void ResetPlayerTransformCache(IGroup<PlayerEntity> playerEntities)
		{
			Type typeFromHandle = typeof(ThirdPersonUnityObjectsComponent);
			FieldInfo field = typeFromHandle.GetField("_playerCached", 927 + BindingFlags.Static - 897);
			FieldInfo field2 = typeFromHandle.GetField("_playerCache", -305 + BindingFlags.IgnoreCase + 342);
			foreach (PlayerEntity playerEntity in playerEntities)
			{
				if (playerEntity != null && playerEntity.thirdPersonUnityObjects != null)
				{
					if (field != null)
					{
						field.SetValue(playerEntity.thirdPersonUnityObjects, false);
					}
					if (field2 != null)
					{
						field2.SetValue(playerEntity.thirdPersonUnityObjects, null);
					}
				}
			}
		}
		public static PlayerData PredictionEntity;
		public static List<PlayerData> EntityList;
		public static Camera MainCamera;
		public static PlayerData CameraEntity;
		public static PlayerData LocalEntity;
	}
}