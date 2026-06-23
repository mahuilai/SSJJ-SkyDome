using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SkyDome.Cfg;
using SkyDome.Entity;
using SkyDome.Feature.Legit;
using SkyDome.Features;
using SkyDome.Render;
using SkyDome.Utilities;
using UnityEngine;
namespace SkyDome.Feature.Visuals
{
	public class StatusDisplay : MonoBehaviour
	{
		private void DrawFirstPersonIndicators(List<ValueTuple<string, Color>> indicators)
		{
			Vector2 firstPersonCenterPosition = (new Vector2(100f, (float)Screen.height / 2f));
			float num = 18f;
			float num2 = (float)indicators.Count * num;
			float num3 = firstPersonCenterPosition.y - num2 / 2f;
			for (int i = 0; i < indicators.Count; i++)
			{
				ValueTuple<string, Color> valueTuple = indicators[i];
				string item = valueTuple.Item1;
				Color item2 = valueTuple.Item2;
				FastRenderer.DrawString(new Vector2(firstPersonCenterPosition.x, num3 + (float)i * num), item, item2, false, 14);
			}
		}

        private void DrawThirdPersonIndicators(List<ValueTuple<string, Color>> indicators)
		{
			if (PlayerStateTracker.MainCamera == null)
			{
				return;
			}
			Transform playerTransform = PlayerStateTracker.LocalEntity.GetPlayerTransform("Bip01_Spine");
			if (playerTransform == null)
			{
				return;
			}
			Vector3 vector = ViewportUtility.WorldPointToScreenPoint(playerTransform.position);
			if (!ViewportUtility.IsScreenPointVisible(vector))
			{
				return;
			}
			Vector2 vector2 = new Vector2(vector.x + StatusDisplay.ThirdPersonOffset.x, vector.y);
			float num = 18f;
			float num2 = (float)indicators.Count * num;
			float num3 = vector2.y - num2 / 2f;
			for (int i = 0; i < indicators.Count; i++)
			{
				ValueTuple<string, Color> valueTuple = indicators[i];
				string item = valueTuple.Item1;
				Color item2 = valueTuple.Item2;
				FastRenderer.DrawString(new Vector2(vector2.x, num3 + (float)i * num), item, item2, false, 14);
			}
		}
		static StatusDisplay()
		{
			StatusDisplay.IndicatorRule[] array = new StatusDisplay.IndicatorRule[2];
			array[0] = new StatusDisplay.IndicatorRule(delegate
			{
				if (SettingsStore.AutoFireControllerDelayedActivation)
				{
					return AutoFireController.IsActive;
				}
				return false;
			}, () => string.Format("扳机 - {0:F1}s", AutoFireController.RemainingTime), () => Color.yellow);
			array[1] = new StatusDisplay.IndicatorRule(() => SettingsStore.FakeLag, () => string.Format("假延�?- {0}", SettingsStore.FakeLagChoke), () => Color.cyan);
			StatusDisplay.Rules = array;
			StatusDisplay.ThirdPersonOffset = new Vector3(-120f, 0f, 0f);
		}
		public StatusDisplay()
		{
		}

        private void OnGUI()
		{
			if (PlayerStateTracker.LocalEntity == null || PlayerStateTracker.LocalEntity.IsDead)
			{
				return;
			}
			List<ValueTuple<string, Color>> list = new List<ValueTuple<string, Color>>();
			foreach (StatusDisplay.IndicatorRule indicatorRule in StatusDisplay.Rules)
			{
				if (indicatorRule.IsEnabled())
				{
					list.Add(new ValueTuple<string, Color>(indicatorRule.GetText(), indicatorRule.GetColor()));
				}
			}
			if (list.Count == 0)
			{
				return;
			}
			if (SettingsStore.ThirdPerson)
			{
				this.DrawThirdPersonIndicators(list);
				return;
			}
			this.DrawFirstPersonIndicators(list);
		}
		private static readonly Vector3 ThirdPersonOffset;
		private static readonly StatusDisplay.IndicatorRule[] Rules;
		private readonly struct IndicatorRule
		{
			public IndicatorRule(Func<bool> enabled, Func<string> text, Func<Color> color = null)
			{
				this.IsEnabled = enabled;
				this.GetText = text;
				this.GetColor = color ?? (() => Color.white);
			}
			public readonly Func<bool> IsEnabled;
			public readonly Func<string> GetText;
			public readonly Func<Color> GetColor;
		}
	}
}