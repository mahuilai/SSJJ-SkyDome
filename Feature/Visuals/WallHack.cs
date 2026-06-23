using System;
using System.Collections.Generic;
using Assets.Sources.Utils.Weapon;
using share;
using SkyDome.Cfg;
using SkyDome.Entity;
using SkyDome.Feature.AutoTrigger;
using SkyDome.Feature.Legit;
using SkyDome.Render;
using SkyDome.Utilities;
using SSJJMath;
using UnityEngine;
namespace SkyDome.Feature.Visuals
{
	public class EntityVisualizer : MonoBehaviour
	{

        private static Color? GetWeaponColor(SkyDome.Entity.PlayerData player)
		{
			if (player.CurrentWeaponId == 4)
			{
				Color? color;
				using (HashSet<string>.Enumerator enumerator = EntityVisualizer.SpecialThrowables.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						string text = enumerator.Current;
						if (player.Weapon.Contains(text))
						{
							color = null;
							return color;
						}
					}
					goto IL_005D;
				}
				return color;
				IL_005D:
				return new Color?(Color.red);
			}
			return null;
		}
		private static string GetThrowableDisplayText(string weaponName)
		{
			using (HashSet<string>.Enumerator enumerator = EntityVisualizer.SpecialThrowables.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					string text = enumerator.Current;
					if (weaponName.Contains(text))
					{
						return "[投掷物]" + weaponName;
					}
				}
				goto IL_00C6;
			}
			string text2;
			return text2;
			IL_00C6:
			return "[手雷]" + weaponName;
		}
		private static bool IsPlayerOnWindSpiritPath(SkyDome.Entity.PlayerData player)
		{
			if (SkyDome.Feature.AutoTrigger.AutoRecall.EnemiesOnPaths != null && SkyDome.Feature.AutoTrigger.AutoRecall.EnemiesOnPaths.Count != 0)
			{
				foreach (List<SkyDome.Feature.AutoTrigger.AutoRecall.EnemyOnPath> list in SkyDome.Feature.AutoTrigger.AutoRecall.EnemiesOnPaths.Values)
				{
					using (List<SkyDome.Feature.AutoTrigger.AutoRecall.EnemyOnPath>.Enumerator enumerator2 = list.GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							if (enumerator2.Current.Player._entity == player._entity)
							{
								return true;
							}
						}
					}
				}
				return false;
			}
			return false;
		}
		private static string GetWeaponDisplayText(SkyDome.Entity.PlayerData player)
		{
			int currentWeaponId = player.CurrentWeaponId;
			int weaponDetailType = player.WeaponDetailType;
			string weapon = player.Weapon;
			if (currentWeaponId == 1)
			{
				return EntityVisualizer.GetWeaponTypeName(weaponDetailType, weapon);
			}
			if (currentWeaponId == 2)
			{
				return "[副武器]" + weapon;
			}
			if (currentWeaponId == 3)
			{
				return "[近战]" + weapon;
			}
			if (currentWeaponId == 4)
			{
				return EntityVisualizer.GetThrowableDisplayText(weapon);
			}
			if (currentWeaponId == 5)
			{
				return "[战术]" + weapon;
			}
			return string.Format("[{0}]{1}", currentWeaponId, weapon);
		}

        unsafe static EntityVisualizer()
		{
			EntityVisualizer.TextRule[] array = new EntityVisualizer.TextRule[4];
			array[0] = new EntityVisualizer.TextRule((SkyDome.Entity.PlayerData p) => SettingsStore.ShowName, (SkyDome.Entity.PlayerData p) => p.PlayerName, null);
			array[1] = new EntityVisualizer.TextRule((SkyDome.Entity.PlayerData p) => SettingsStore.ShowWeapon, (SkyDome.Entity.PlayerData p) => EntityVisualizer.GetWeaponDisplayText(p), (SkyDome.Entity.PlayerData p) => EntityVisualizer.GetWeaponColor(p));
			array[2] = new EntityVisualizer.TextRule((SkyDome.Entity.PlayerData p) => SettingsStore.ShowYaw, (SkyDome.Entity.PlayerData p) => string.Format("Yaw: {0}°", p.ViewYaw), null);
			array[3] = new EntityVisualizer.TextRule((SkyDome.Entity.PlayerData p) => SettingsStore.ShowPitch, (SkyDome.Entity.PlayerData p) => string.Format("Pitch: {0}°", p.ViewPitch), null);
			EntityVisualizer.TopRules = array;
			EntityVisualizer.TextRule[] array2 = new EntityVisualizer.TextRule[4];
			array2[0] = new EntityVisualizer.TextRule((SkyDome.Entity.PlayerData p) => SettingsStore.ShowHp, delegate(SkyDome.Entity.PlayerData p)
			{
				string text = (Mathf.Approximately(p.Hp, Mathf.Round(p.Hp)) ? "F0" : "F2");
				return "HP " + p.Hp.ToString(text);
			}, null);
			array2[1] = new EntityVisualizer.TextRule((SkyDome.Entity.PlayerData p) => SettingsStore.ShowDistance, (SkyDome.Entity.PlayerData p) => string.Format("{0:F0}m", p.Distance), null);
			array2[2] = new EntityVisualizer.TextRule((SkyDome.Entity.PlayerData p) => SettingsStore.ShowC4 && p.HasC4, (SkyDome.Entity.PlayerData p) => "[C4]", (SkyDome.Entity.PlayerData p) => new Color?(Color.red));
			array2[3] = new EntityVisualizer.TextRule(delegate(SkyDome.Entity.PlayerData p)
			{
				if (PlayerStateTracker.LocalEntity != null && PlayerStateTracker.LocalEntity.CurrentWeaponName == "wind_spirit" && SkyDome.Feature.AutoTrigger.AutoRecall.EnemiesOnPaths != null)
				{
					return EntityVisualizer.IsPlayerOnWindSpiritPath(p);
				}
				return false;
			}, (SkyDome.Entity.PlayerData p) => "[Wind Path]", (SkyDome.Entity.PlayerData p) => new Color?(new Color(1f, 0.5f, 0f)));
			EntityVisualizer.BottomRules = array2;
			EntityVisualizer.SpecialThrowables = new HashSet<string>
			{
				"闪光弹",
				"FLash-X",
				"烟雾弹",
				"雾藤",
				"万象",
				"镇宇",
				"天枢",
				"玉衡",
				"月隐",
				"胡峰",
				"极光",
				"暗蚀"
			};
		}
		private bool IsVisible(SkyDome.Entity.PlayerData target)
		{
			Vector3 vector = VectorCoordConverter.UnityToSsjj(Camera.main.transform.forward);
			int entityId = SkyDome.Utilities.PathRendererHelper.GetEntityId(
				Contexts.sharedInstance.battleRoom.pyEngine.PyEngine, 
				PlayerStateTracker.LocalEntity._entity, 
				Contexts.sharedInstance.player, 
				100000f, 
				new Vector3D((double)vector.x, (double)vector.y, (double)vector.z), 
				new float[3], 
				new float[3], 
				false
			);
			return entityId == target.Id;
		}
		private void OnGUI()
		{
			if (SettingsStore.EntityVisualizer && PlayerStateTracker.EntityList != null)
			{
				foreach (SkyDome.Entity.PlayerData PlayerData in PlayerStateTracker.EntityList)
				{
					if (PlayerData.Team != PlayerStateTracker.LocalEntity.Team && !PlayerData.IsDead)
					{
						this.DrawEnemy(PlayerData);
					}
				}
				return;
			}
		}
		private void DrawStackedText(SkyDome.Entity.PlayerData player, Rect rect, Color defaultColor)
		{
			float x = rect.center.x;
			float num = 0f;
			bool flag = true;
			EntityVisualizer.TextRule[] array = EntityVisualizer.TopRules;
			for (int i = 0; i < array.Length; i += 1)
			{
				EntityVisualizer.TextRule textRule = array[i];
				if (textRule.IsEnabled(player))
				{
					if (flag)
					{
						num = (float)Screen.height - rect.yMax - 15f;
						flag = false;
					}
					else
					{
						num -= 12f;
					}
					Color color = ((textRule.GetColor != null) ? textRule.GetColor(player).GetValueOrDefault(defaultColor) : defaultColor);
					FastRenderer.DrawString(new Vector2(x, num), textRule.GetText(player), color, true, 10);
				}
			}
			float num2 = 0f;
			bool flag2 = true;
			array = EntityVisualizer.BottomRules;
			for (int i = 0; i < array.Length; i += 1)
			{
				EntityVisualizer.TextRule textRule2 = array[i];
				if (textRule2.IsEnabled(player))
				{
					if (flag2)
					{
						num2 = (float)Screen.height - rect.y;
						flag2 = false;
					}
					else
					{
						num2 += 12f;
					}
					Color color2 = ((textRule2.GetColor != null) ? textRule2.GetColor(player).GetValueOrDefault(defaultColor) : defaultColor);
					FastRenderer.DrawString(new Vector2(x, num2), textRule2.GetText(player), color2, true, 10);
				}
			}
		}
		private bool TryGetBoundingBox(SkyDome.Entity.PlayerData player, out Rect rect, out Color color)
		{
			rect = default(Rect);
			color = Color.green;
			Vector3 vector = ViewportUtility.WorldPointToScreenPoint(player.GetPlayerTransform(player.PlayerName).position);
			Vector3 vector2 = ViewportUtility.WorldPointToScreenPoint(player.GetValidHeadNub().position);
			if (!ViewportUtility.IsScreenPointVisible(vector))
			{
				return false;
			}
			float num = Mathf.Abs(vector2.y - vector.y);
			float num2 = num / 2.3f;
			Vector2 vector3 = (vector2 + vector) * 0.5f;
			rect = new Rect(vector3.x - num2 / 2f - 1f, (float)Screen.height - vector3.y - num / 2f, num2, num);
			SkyDome.Entity.PlayerData currentTarget = TargetSelector._currentTarget;
			if (((currentTarget != null) ? currentTarget._entity : null) == player._entity)
			{
				color = Color.yellow;
			}
			else if (this.IsVisible(player))
			{
				color = Color.red;
			}
			return true;
		}
		private void DrawEnemy(SkyDome.Entity.PlayerData player)
		{
			Rect rect;
			Color color;
			if (!this.TryGetBoundingBox(player, out rect, out color))
			{
				return;
			}
			this.DrawVisuals(player, rect, color);
			this.DrawStackedText(player, rect, color);
		}
		private static string GetWeaponTypeName(int weaponType, string weaponName)
		{
			switch (weaponType)
			{
			case 0:
				return "[手枪]" + weaponName;
			case 1:
				return "[步枪]" + weaponName;
			case 2:
				return "[近战]" + weaponName;
			case 3:
				return "[投掷物]" + weaponName;
			case 5:
				return "[狙击枪]" + weaponName;
			case 6:
				return "[霰弹]" + weaponName;
			case 10:
				return "[机枪]" + weaponName;
			case 12:
				return "[冲锋枪]" + weaponName;
			}
			return string.Format("[{0}]{1}", weaponType, weaponName);
		}
		private void DrawVisuals(SkyDome.Entity.PlayerData player, Rect rect, Color color)
		{
			Color neonColor = FastRenderer.GetRainbowColor(3f, player.Id * 0.05f);
			if (SettingsStore.ShowRect)
			{
				if (SettingsStore.Rect_Style == 0)
				{
					FastRenderer.DrawBoxOutline(rect, neonColor, 2f);
				}
				else
				{
					FastRenderer.DrawCornerBox(rect, neonColor, 2f, 10f, true);
				}
			}
			if (SettingsStore.ShowHpBar)
			{
				SkyDome.Render.OverlayHost.DrawVerticalHealthBar(rect, player.HpPercent, 5.3f, 3f, true);
			}
			if (SettingsStore.ShowSkeleton)
			{
				SkyDome.Render.OverlayHost.DrawSkeleton(player, neonColor, 2f);
			}
			if (SettingsStore.ShowAirLine)
			{
				FastRenderer.DrawLine(new Vector2((float)Screen.width / 2f, (float)Screen.height), new Vector2(rect.center.x, rect.yMax), FastRenderer.GetRainbowColor(3f), 2f);
			}
		}
		private static readonly HashSet<string> SpecialThrowables;
		private static readonly EntityVisualizer.TextRule[] TopRules;
		private static readonly EntityVisualizer.TextRule[] BottomRules;
		private readonly struct TextRule
		{
			public TextRule(Func<SkyDome.Entity.PlayerData, bool> enabled, Func<SkyDome.Entity.PlayerData, string> text, Func<SkyDome.Entity.PlayerData, Color?> color = null)
			{
				this.IsEnabled = enabled;
				this.GetText = text;
				this.GetColor = color;
			}
			public readonly Func<SkyDome.Entity.PlayerData, bool> IsEnabled;
			public readonly Func<SkyDome.Entity.PlayerData, string> GetText;
			public readonly Func<SkyDome.Entity.PlayerData, Color?> GetColor;
		}
	}
}
