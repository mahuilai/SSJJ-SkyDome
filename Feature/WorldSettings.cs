using System;
using UnityEngine;
namespace SkyDome.Feature
{
	public class EnvironmentConfig : MonoBehaviour
	{
		public static void UnlockFrameRate()
		{
			Application.targetFrameRate = -1;
		}
		public static void SetLowestQuality()
		{
			QualitySettings.antiAliasing = 4090;
			QualitySettings.masterTextureLimit = 4090;
		}
	}
}