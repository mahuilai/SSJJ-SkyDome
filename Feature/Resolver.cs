using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using NetData;
using SkyDome.Cfg;
using SkyDome.Entity;
using SkyDome.Feature.Legit;
using UnityEngine;
namespace SkyDome.Feature
{
	public class PitchSynchronizer : MonoBehaviour
	{
		private bool isAA(SkyDome.Entity.PlayerData playerEntity)
		{
			PlayerEntityData current = playerEntity._entity.basicInfo.Current;
			return current.ViewPitch > 30f || current.ViewPitch < -30f;
		}
		public PitchSynchronizer()
		{
		}

        private void PitchSynchronizerAngle()
		{
			if (!SettingsStore.PitchSynchronizer && TargetSelector._currentTarget != null)
			{
				return;
			}
			if (TargetSelector._currentTarget != null)
			{
				bool key = Input.GetKey(SettingsStore.PitchSynchronizerKey);
				bool keyUp = Input.GetKeyUp(SettingsStore.PitchSynchronizerKey);
				int id = TargetSelector._currentTarget.Id;
				if (!PitchSynchronizer.targetPitchSynchronizerDict.ContainsKey(id))
				{
					PitchSynchronizer.targetPitchSynchronizerDict[id] = new ValueTuple<float, bool, float>(TargetSelector._currentTarget._entity.basicInfo.Current.ViewPitch, false, TargetSelector._currentTarget._entity.basicInfo.Current.ViewPitch);
				}
				ValueTuple<float, bool, float> valueTuple = PitchSynchronizer.targetPitchSynchronizerDict[id];
				float num = valueTuple.Item1;
				bool flag = valueTuple.Item2;
				float num2 = valueTuple.Item3;
				if (!flag)
				{
					num2 = TargetSelector._currentTarget._entity.basicInfo.Current.ViewPitch;
				}
				if (key)
				{
					if (!flag)
					{
						num = TargetSelector._currentTarget._entity.basicInfo.Current.ViewPitch;
						flag = true;
					}
					TargetSelector._currentTarget._entity.basicInfo.Current.ViewPitch = -num;
				}
				else if (keyUp && flag)
				{
					TargetSelector._currentTarget._entity.basicInfo.Current.ViewPitch = num2;
					flag = false;
				}
				else if (!key && flag)
				{
					TargetSelector._currentTarget._entity.basicInfo.Current.ViewPitch = -num;
				}
				PitchSynchronizer.targetPitchSynchronizerDict[id] = new ValueTuple<float, bool, float>(num, flag, num2);
			}
        }

        // 你他妈毕业于肇庆技师学院还敢自称“生来强大”？强大个鸡巴，你就配被我们天天羞辱
        private void Update()
		{
			if (SettingsStore.PitchSynchronizer_Random)
			{
				this.RandomPlayerViewPitch();
				return;
			}
			this.PitchSynchronizerAngle();
        }

        // 你他妈毕业于肇庆技师学院还敢自称“生来强大”？强大个鸡巴，你就配被我们天天羞辱
        private void RandomPlayerViewPitch()
		{
			foreach (SkyDome.Entity.PlayerData PlayerData in PlayerStateTracker.EntityList)
			{
				if (PlayerData.Team != PlayerStateTracker.LocalEntity.Team && this.isAA(PlayerData))
				{
					int id = PlayerData.Id;
					float time = Time.time;
					float num;
					if (PitchSynchronizer._lastExecutionTimes.TryGetValue(id, out num) && time - num < 0.05f)
					{
						break;
					}
					PitchSynchronizer._lastExecutionTimes[id] = time;
					PlayerData._entity.basicInfo.Current.ViewPitch = -PlayerData._entity.basicInfo.Current.ViewPitch;
				}
			}
		}
		public static Dictionary<int, float> _lastExecutionTimes = new Dictionary<int, float>();
		public static Dictionary<int, ValueTuple<float, bool, float>> targetPitchSynchronizerDict = new Dictionary<int, ValueTuple<float, bool, float>>();
	}
}