using System;
using System.Collections.Generic;
using SkyDome.Entity;
using UnityEngine;

namespace SkyDome.Feature.AutoTrigger
{
	public class AutoRecall : MonoBehaviour
	{
		public class EnemyOnPath
		{
			public PlayerData Player { get; set; }
		}

		public static Dictionary<int, List<EnemyOnPath>> EnemiesOnPaths = new Dictionary<int, List<EnemyOnPath>>();
	}
}
