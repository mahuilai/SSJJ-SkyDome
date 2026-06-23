using System;
using System.Collections.Generic;
using NetData;
using SkyDome.Cfg;
using SkyDome.Entity;
using SkyDome.Feature;
using SkyDome.Feature.Visuals;
using UnityEngine;

namespace SkyDome.Features
{
	public class Menu : MonoBehaviour
	{
		private void DoWin(int id)
		{
			// 1. Draw Sidebar Background
			GUI.DrawTexture(new Rect(0f, 0f, 200f, 760f), Menu.S.SidebarBgTex);

			// 2. Draw Left Logo, Title & Subtitle (Neverlose Style)
			Rect logoRect = new Rect(25f, 25f, 32f, 32f);
			GUI.DrawTexture(logoRect, Menu.S.LogoTex);
			GUI.Label(logoRect, "NL", Menu.S.LogoTextStyle);

			GUI.Label(new Rect(70f, 23f, 120f, 20f), "Neverlose", Menu.S.SidebarTitleStyle);
			GUI.Label(new Rect(70f, 41f, 120f, 15f), "Minecraft", Menu.S.SidebarSubtitleStyle);

			// 3. Draw Sidebar Tabs (Chinese)
			string[] tabNames = new string[] { "战斗", "视觉", "玩家", "世界", "杂项", "配置" };
			for (int i = 0; i < tabNames.Length; i++)
			{
				Rect tabRect = new Rect(15f, 90f + i * 48f, 170f, 40f);
				bool isSelected = (i == this.tab);

				Event current = Event.current;
				if (current.type == EventType.MouseDown && tabRect.Contains(current.mousePosition))
				{
					this.tab = i;
					this.CloseAllDropdowns();
					current.Use();
				}

				if (isSelected)
				{
					GUI.DrawTexture(tabRect, Menu.S.TabSelectedTex);
					GUI.DrawTexture(new Rect(tabRect.x + 2f, tabRect.y + 10f, 3f, 20f), Menu.S.AccentBlueTex);
				}
				else if (tabRect.Contains(current.mousePosition))
				{
					GUI.DrawTexture(tabRect, Menu.S.TabHoverTex);
				}

				GUI.Label(new Rect(tabRect.x + 15f, tabRect.y + 10f, 140f, 20f), tabNames[i], isSelected ? Menu.S.TabActiveTextStyle : Menu.S.TabNormalTextStyle);
			}

			// 4. Draw Avatar block at the bottom of sidebar
			GUI.DrawTexture(new Rect(15f, 702f, 36f, 36f), Menu.S.AvatarTex);
			GUI.Label(new Rect(58f, 701f, 130f, 18f), "Player765", Menu.S.AvatarNameStyle);
			GUI.Label(new Rect(58f, 719f, 130f, 15f), "Beta", Menu.S.AvatarBetaStyle);

			// 5. Draw Right Top Header / Search Bar
			Rect searchRect = new Rect(220f, 25f, 835f, 36f);
			GUI.DrawTexture(searchRect, Menu.S.SearchBgTex);
			GUI.Label(new Rect(232f, 33f, 20f, 20f), "O", Menu.S.SearchIconStyle);
			this.searchQuery = GUI.TextField(new Rect(255f, 33f, 790f, 20f), this.searchQuery, Menu.S.SearchTextFieldStyle);

			// 6. Draw Content Area
			GUILayout.BeginArea(new Rect(220f, 75f, 835f, 660f));
			this.scroll = GUILayout.BeginScrollView(this.scroll, false, false, GUILayout.Width(835f), GUILayout.Height(660f));

			GUILayout.BeginHorizontal();

			// Left Column (width 410)
			GUILayout.BeginVertical(GUILayout.Width(410f));
			this.DrawLeftColumn(this.tab);
			GUILayout.EndVertical();

			GUILayout.Space(15f);

			// Right Column (width 410)
			GUILayout.BeginVertical(GUILayout.Width(410f));
			this.DrawRightColumn(this.tab);
			GUILayout.EndVertical();

			GUILayout.EndHorizontal();

			GUILayout.EndScrollView();
			GUILayout.EndArea();

			// 7. Draw Active Dropdown Popup on top
			this.DrawActiveDropdown();

			// 8. Draw Toast Notification
			if (this.popup && Time.time - this.popupTime < 3f)
			{
				Rect popRect = new Rect(415f, 702f, 250f, 36f);
				GUI.DrawTexture(popRect, Menu.S.TabSelectedTex);
				GUI.Label(popRect, this.popupMsg, Menu.S.CenterLabelStyle);
			}

			// Drag window by clicking header
			GUI.DragWindow(new Rect(0f, 0f, 1080f, 60f));
		}

		private void DrawLeftColumn(int selectedTab)
		{
			switch (selectedTab)
			{
				case 0:
					this.DrawTargetSelectorCard();
					break;
				case 1:
					this.DrawESPPlayerCard();
					break;
				case 2:
					this.DrawPlayerAttributesCard();
					break;
				case 3:
					this.DrawWorldCard();
					break;
				case 4:
					this.DrawRecoilCard();
					this.DrawMiscOtherCard();
					break;
				case 5:
					this.DrawSettingsStoreListCard();
					break;
			}
		}

		private void DrawRightColumn(int selectedTab)
		{
			switch (selectedTab)
			{
				case 0:
					this.DrawShotCalculatorCard();
					this.DrawPitchSynchronizerCard();
					break;
				case 1:
					this.DrawESPEffectsCard();
					this.DrawESPIndicatorsCard();
					break;
				case 2:
					this.DrawSkinsCard();
					break;
				case 3:
					this.DrawCameraViewCard();
					break;
				case 4:
					this.DrawAutoFireControllerCard();
					this.DrawSpammerCard();
					break;
				case 5:
					this.DrawSettingsStoreCreateCard();
					break;
			}
		}

		private bool matchesSearch(string name)
		{
			if (string.IsNullOrEmpty(this.searchQuery)) return true;
			return name.ToLower().Contains(this.searchQuery.ToLower());
		}

		private void BeginCard(string title)
		{
			GUILayout.BeginVertical(Menu.S.CardStyle);
			GUILayout.Label(title, Menu.S.CardTitleStyle);
			GUILayout.Space(8f);
		}

		private void BeginCard(string title, ref bool enabled)
		{
			GUILayout.BeginVertical(Menu.S.CardStyle);
			GUILayout.BeginHorizontal();
			GUILayout.Label(title, Menu.S.CardTitleStyle);
			GUILayout.FlexibleSpace();
			enabled = this.CustomToggle(enabled);
			GUILayout.EndHorizontal();
			GUILayout.Space(8f);
		}

		private void EndCard()
		{
			GUILayout.EndVertical();
			GUILayout.Space(10f);
		}

		private bool CustomToggle(bool value, string label = null)
		{
			GUILayout.BeginHorizontal();
			if (!string.IsNullOrEmpty(label))
			{
				GUILayout.Label(label, Menu.S.LabelStyle);
				GUILayout.FlexibleSpace();
			}

			Rect rect = GUILayoutUtility.GetRect(34f, 18f, GUILayout.Width(34f), GUILayout.Height(18f));
			Event current = Event.current;
			if (current.type == EventType.MouseDown && rect.Contains(current.mousePosition))
			{
				value = !value;
				current.Use();
			}

			GUI.DrawTexture(rect, value ? Menu.S.ToggleOnTex : Menu.S.ToggleOffTex);

			Rect thumbRect = new Rect(rect.x + (value ? 18f : 2f), rect.y + 2f, 14f, 14f);
			GUI.DrawTexture(thumbRect, Menu.S.WhiteCircleTex);

			GUILayout.EndHorizontal();
			return value;
		}

		private void CustomSlider(string label, ref float value, float min, float max, string format = "F1")
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label(label, Menu.S.LabelStyle, GUILayout.Width(100f));

			Rect sliderRect = GUILayoutUtility.GetRect(100f, 16f, GUILayout.ExpandWidth(true));
			float progress = (value - min) / (max - min);

			Event current = Event.current;
			if ((current.type == EventType.MouseDown || current.type == EventType.MouseDrag) && sliderRect.Contains(current.mousePosition))
			{
				float clickProgress = (current.mousePosition.x - sliderRect.x) / sliderRect.width;
				value = min + Mathf.Clamp01(clickProgress) * (max - min);
				current.Use();
			}

			Rect trackRect = new Rect(sliderRect.x, sliderRect.y + 6f, sliderRect.width, 4f);
			GUI.DrawTexture(trackRect, Menu.S.SliderTrackTex);

			Rect fillRect = new Rect(sliderRect.x, sliderRect.y + 6f, sliderRect.width * progress, 4f);
			GUI.DrawTexture(fillRect, Menu.S.SliderFillTex);

			Rect thumbRect = new Rect(sliderRect.x + sliderRect.width * progress - 4f, sliderRect.y + 4f, 8f, 8f);
			GUI.DrawTexture(thumbRect, Menu.S.WhiteCircleTex);

			GUILayout.Space(10f);
			GUILayout.Label(value.ToString(format), Menu.S.ValueLabelStyle, GUILayout.Width(40f));
			GUILayout.EndHorizontal();
		}

		private void CustomSliderInt(string label, ref int value, int min, int max, string unit = "")
		{
			float val = value;
			this.CustomSlider(label, ref val, min, max, "F0");
			value = Mathf.RoundToInt(val);
		}

		private void CustomDropdown(string key, string label, List<string> items, Action<string> onSelect = null)
		{
			if (items == null || items.Count == 0) return;
			if (!this.dropdowns.ContainsKey(key))
			{
				this.dropdowns[key] = new Menu.DropdownState();
			}
			Menu.DropdownState dropdownState = this.dropdowns[key];

			GUILayout.BeginHorizontal();
			GUILayout.Label(label, Menu.S.LabelStyle, GUILayout.Width(100f));
			GUILayout.FlexibleSpace();

			string text = ((dropdownState.SelectedIndex < 0 || dropdownState.SelectedIndex >= items.Count) ? "请选择" : items[dropdownState.SelectedIndex]);

			Rect btnRect = GUILayoutUtility.GetRect(120f, 22f, GUILayout.Width(120f));
			if (GUI.Button(btnRect, text + "  v", Menu.S.DropdownStyle))
			{
				dropdownState.IsOpen = !dropdownState.IsOpen;
				if (dropdownState.IsOpen)
				{
					foreach (var kvp in this.dropdowns)
					{
						if (kvp.Key != key) kvp.Value.IsOpen = false;
					}
					this.activeDropdownKey = key;
					this.activeDropdownItems = items;
					this.activeDropdownOnSelect = onSelect;

					Vector2 screenPos = GUIUtility.GUIToScreenPoint(new Vector2(btnRect.x, btnRect.y));
					float windowX = screenPos.x - this.winRect.x;
					float windowY = screenPos.y - this.winRect.y;
					dropdownState.Rect = new Rect(windowX, windowY + btnRect.height, btnRect.width, Mathf.Min(600f, items.Count * 22f));
				}
				else
				{
					if (this.activeDropdownKey == key)
					{
						this.activeDropdownKey = null;
					}
				}
			}

			GUILayout.EndHorizontal();
		}

		private void CustomKey(string id, string label, KeyCode key)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label(label, Menu.S.LabelStyle, GUILayout.Width(100f));
			GUILayout.FlexibleSpace();

			bool isBindingThis = this.bindWait && this.bindKey == id;
			string text = isBindingThis ? "[按任意键]" : (key == KeyCode.None ? "[点击绑定]" : key.ToString());

			if (GUILayout.Button(text, Menu.S.DropdownStyle, GUILayout.Width(120f)))
			{
				this.bindKey = isBindingThis ? null : id;
				this.bindWait = !isBindingThis;
			}
			GUILayout.EndHorizontal();
		}

		private void DrawActiveDropdown()
		{
			if (string.IsNullOrEmpty(this.activeDropdownKey) || !this.dropdowns.ContainsKey(this.activeDropdownKey))
			{
				return;
			}
			Menu.DropdownState dropdownState = this.dropdowns[this.activeDropdownKey];
			if (!dropdownState.IsOpen || this.activeDropdownItems == null)
			{
				return;
			}

			float itemHeight = 20f;
			float viewHeight = dropdownState.Rect.height;
			float contentHeight = this.activeDropdownItems.Count * itemHeight;
			float scrollBarWidth = 16f;
			
			GUI.Box(dropdownState.Rect, "", Menu.S.DropdownPopupStyle);
			Rect viewRect = new Rect(0f, 0f, dropdownState.Rect.width - scrollBarWidth, contentHeight);
			dropdownState.Scroll = GUI.BeginScrollView(
				new Rect(dropdownState.Rect.x, dropdownState.Rect.y, dropdownState.Rect.width, dropdownState.Rect.height),
				dropdownState.Scroll,
				viewRect,
				false,
				contentHeight > viewHeight);
			
			for (int i = 0; i < this.activeDropdownItems.Count; i++)
			{
				string item = this.activeDropdownItems[i];
				Rect btnRect = new Rect(0f, i * itemHeight, viewRect.width, itemHeight);
				if (GUI.Button(btnRect, item, Menu.S.DropdownItemStyle))
				{
					dropdownState.SelectedIndex = i;
					if (this.activeDropdownOnSelect != null)
					{
						this.activeDropdownOnSelect(item);
					}
					dropdownState.IsOpen = false;
					this.activeDropdownKey = null;
					this.activeDropdownItems = null;
					this.activeDropdownOnSelect = null;
				}
			}
			GUI.EndScrollView();
		}

		private void DrawTargetSelectorCard()
		{
			if (!this.matchesSearch("自瞄设置")) return;

			this.BeginCard("自瞄设置", ref SettingsStore.TargetSelector);
			if (SettingsStore.TargetSelector)
			{
				SettingsStore.AimRange_Show = this.CustomToggle(SettingsStore.AimRange_Show, "显示自瞄范围");
				SettingsStore.VisibleCheck = this.CustomToggle(SettingsStore.VisibleCheck, "可见性检查");
				SettingsStore.ShowAimLine = this.CustomToggle(SettingsStore.ShowAimLine, "绘制自瞄线");

				GUILayout.Space(5f);
				this.CustomKey("Aim", "自瞄按键", SettingsStore.AimKey);

				GUILayout.Space(5f);
				this.CustomSliderInt("自瞄范围", ref SettingsStore.TargetSelectorFOV, 0, 180, "°");

				SettingsStore.TargetSelector_Smooth = this.CustomToggle(SettingsStore.TargetSelector_Smooth, "平滑自瞄");
				if (SettingsStore.TargetSelector_Smooth)
				{
					this.CustomSlider("平滑系数", ref SettingsStore.TargetSelector_SmoothFactor, 1f, 30f);
				}

				GUILayout.Space(5f);
				this.CustomDropdown("aim", "瞄准部位", new List<string>(Menu.Bones), delegate(string name)
				{
					SettingsStore.AimPos = Array.IndexOf<string>(Menu.Bones, name);
				});
			}
			this.EndCard();
		}

		private void DrawShotCalculatorCard()
		{
			if (!this.matchesSearch("静默自瞄")) return;

			this.BeginCard("静默自瞄", ref SettingsStore.ShotCalculator);
			if (SettingsStore.ShotCalculator)
			{
				SettingsStore.ShotCalculator_OnKey = this.CustomToggle(SettingsStore.ShotCalculator_OnKey, "仅按键触发");
				if (SettingsStore.ShotCalculator_OnKey)
				{
					this.CustomKey("Rage", "触发按键", SettingsStore.ShotCalculator_Key);
				}
			}
			this.EndCard();
		}

		private void DrawPitchSynchronizerCard()
		{
			if (!this.matchesSearch("解析器")) return;

			this.BeginCard("解析器", ref SettingsStore.PitchSynchronizer);
			if (SettingsStore.PitchSynchronizer)
			{
				SettingsStore.PitchSynchronizer_Random = this.CustomToggle(SettingsStore.PitchSynchronizer_Random, "随机角度");
				this.CustomKey("Res", "强制解析按键", SettingsStore.PitchSynchronizerKey);
			}
			this.EndCard();
		}

		private void DrawESPPlayerCard()
		{
			if (!this.matchesSearch("玩家透视")) return;

			this.BeginCard("玩家透视", ref SettingsStore.EntityVisualizer);
			if (SettingsStore.EntityVisualizer)
			{
				SettingsStore.ShowRect = this.CustomToggle(SettingsStore.ShowRect, "显示方框");
				SettingsStore.ShowSkeleton = this.CustomToggle(SettingsStore.ShowSkeleton, "显示骨骼");
				SettingsStore.ShowHpBar = this.CustomToggle(SettingsStore.ShowHpBar, "显示血量");
				SettingsStore.ShowHp = this.CustomToggle(SettingsStore.ShowHp, "显示血量");
				SettingsStore.ShowName = this.CustomToggle(SettingsStore.ShowName, "显示名字");
				SettingsStore.ShowDistance = this.CustomToggle(SettingsStore.ShowDistance, "显示距离");
				SettingsStore.ShowWeapon = this.CustomToggle(SettingsStore.ShowWeapon, "显示武器");
				SettingsStore.ShowC4 = this.CustomToggle(SettingsStore.ShowC4, "显示C4");
				SettingsStore.ShowAirLine = this.CustomToggle(SettingsStore.ShowAirLine, "射线示踪");
				SettingsStore.ShowYaw = this.CustomToggle(SettingsStore.ShowYaw, "显示偏航角");
				SettingsStore.ShowPitch = this.CustomToggle(SettingsStore.ShowPitch, "显示俯仰角");
				SettingsStore.Show3DBox = this.CustomToggle(SettingsStore.Show3DBox, "显示3D方框");

				GUILayout.Space(5f);
				GUILayout.BeginHorizontal();
				GUILayout.Label("方框样式:", Menu.S.LabelStyle);
				GUILayout.FlexibleSpace();
				int rect_Style = SettingsStore.Rect_Style;
				string[] styles = new string[] { "完整", "四角" };
				SettingsStore.Rect_Style = GUILayout.Toolbar(rect_Style, styles, Menu.S.DropdownStyle, GUILayout.Width(120f));
				GUILayout.EndHorizontal();
			}
			this.EndCard();
		}

		private void DrawESPEffectsCard()
		{
			if (!this.matchesSearch("视觉效果")) return;

			this.BeginCard("视觉效果");
			SettingsStore.MaterialOverlay = this.CustomToggle(SettingsStore.MaterialOverlay, "人物发光 (MaterialOverlay)");
			SettingsStore.ShowPickupHighlighter = this.CustomToggle(SettingsStore.ShowPickupHighlighter, "物品透视");
			SettingsStore.ShowPickupOutliner = this.CustomToggle(SettingsStore.ShowPickupOutliner, "物品发光");
			SettingsStore.ShowReticleRenderer = this.CustomToggle(SettingsStore.ShowReticleRenderer, "绘制准星");
			this.EndCard();
		}

		private void DrawESPIndicatorsCard()
		{
			if (!this.matchesSearch("全局辅助")) return;

			this.BeginCard("全局辅助");
			SettingsStore.ShowMiniMapOverlay = this.CustomToggle(SettingsStore.ShowMiniMapOverlay, "2D雷达");
			SettingsStore.ShowIndicators = this.CustomToggle(SettingsStore.ShowIndicators, "状态指示器");
			this.EndCard();
		}

		private void DrawPlayerAttributesCard()
		{
			if (!this.matchesSearch("玩家属性")) return;

			this.BeginCard("玩家属性");
			SkyDome.Entity.PlayerData localEntity = PlayerStateTracker.LocalEntity;
			if (localEntity != null && localEntity._entity != null && localEntity._entity.hasBasicInfo)
			{
				PlayerEntityData info = localEntity._entity.basicInfo.Current;

				float scale = info.Scale;
				this.CustomSlider("模型大小", ref scale, -5f, 5f);
				if (Math.Abs(scale - info.Scale) > 0.01f)
				{
					AppearanceModifier.ChangeScale(scale);
				}

				float head = info.HeadEnlarge;
				this.CustomSlider("头部大小", ref head, -5f, 5f);
				if (Math.Abs(head - info.HeadEnlarge) > 0.01f)
				{
					AppearanceModifier.ChangeHeadEnlarge(head);
				}

				float teamVal = info.Team;
				this.CustomSlider("阵营ID", ref teamVal, 0f, 13f, "F0");
				int team = Mathf.RoundToInt(teamVal);
				if (team != info.Team)
				{
					AppearanceModifier.ChangeTeam(team);
				}

				float alphaVal = info.Alpha;
				this.CustomSlider("透明度", ref alphaVal, 0f, 100f, "F0");
				int alpha = Mathf.RoundToInt(alphaVal);
				if (alpha != info.Alpha)
				{
					AppearanceModifier.ChangeAlpha(alpha);
				}

				float selfAlphaVal = info.SelfAlpha;
				this.CustomSlider("自身透明度", ref selfAlphaVal, 0f, 100f, "F0");
				int selfAlpha = Mathf.RoundToInt(selfAlphaVal);
				if (selfAlpha != info.SelfAlpha)
				{
					AppearanceModifier.ChangeSelfAlpha(selfAlpha);
				}
			}
			else
			{
				GUILayout.Label("正在等待玩家生成...", Menu.S.HintStyle);
			}
			this.EndCard();
		}

		private void DrawSkinsCard()
		{
			if (!this.matchesSearch("换肤与配置")) return;

			this.BeginCard("换肤与配置");
			if (AppearanceModifier.BackAccessoryNames.Count > 0)
			{
				this.CustomDropdown("backAccessory", "背饰选择", AppearanceModifier.BackAccessoryNames, delegate(string name)
				{
					AppearanceModifier.ChangeBackAccessory(name);
				});
				GUILayout.Space(5f);
			}
			if (AppearanceModifier.CharacterNames.Count > 0)
			{
				this.CustomDropdown("character", "角色选择", AppearanceModifier.CharacterNames, delegate(string name)
				{
					AppearanceModifier.ChangeCharacter(name);
				});
				GUILayout.Space(5f);
			}
			if (AppearanceModifier.WeaponNames.Count > 0)
			{
				this.CustomDropdown("weapon", "武器选择", AppearanceModifier.WeaponNames, delegate(string name)
				{
					AppearanceModifier.ChangeWeapon(name);
				});
			}
			if (AppearanceModifier.BackAccessoryNames.Count == 0 && AppearanceModifier.CharacterNames.Count == 0 && AppearanceModifier.WeaponNames.Count == 0)
			{
				GUILayout.Label("当前无可用皮肤数据", Menu.S.HintStyle);
			}
			this.EndCard();
		}

		private void DrawWorldCard()
		{
			if (!this.matchesSearch("世界设置")) return;

			this.BeginCard("世界设置");
			if (GUILayout.Button("最低画质", Menu.S.ButtonStyle, GUILayout.Height(28f)))
			{
				EnvironmentConfig.SetLowestQuality();
			}
			GUILayout.Space(8f);
			if (GUILayout.Button("解锁帧数", Menu.S.ButtonStyle, GUILayout.Height(28f)))
			{
				EnvironmentConfig.UnlockFrameRate();
			}
			this.EndCard();
		}

		private void DrawCameraViewCard()
		{
			if (!this.matchesSearch("视角模式")) return;

			this.BeginCard("视角模式");
			SettingsStore.ThirdPerson = this.CustomToggle(SettingsStore.ThirdPerson, "强制第三人称");
			this.CustomKey("3rd", "切换按键", SettingsStore.ThirdPersonKey);

			GUILayout.Space(5f);
			SettingsStore.Fov = this.CustomToggle(SettingsStore.Fov, "视角FOV自定义");
			if (SettingsStore.Fov)
			{
				float tpFov = SettingsStore.ThirdPersonFov;
				this.CustomSlider("第三人称FOV", ref tpFov, 0f, 150f, "F0");
				SettingsStore.ThirdPersonFov = Mathf.RoundToInt(tpFov);

				this.CustomSlider("第一人称FOV", ref SettingsStore.FirstPersonFov, 0f, 150f);
			}
			this.EndCard();
		}

		private void DrawRecoilCard()
		{
			if (!this.matchesSearch("后座力控制")) return;

			this.BeginCard("后座力控制", ref SettingsStore.ViewStabilizer);
			if (SettingsStore.ViewStabilizer)
			{
				SettingsStore.SmoothControl = this.CustomToggle(SettingsStore.SmoothControl, "平滑移动");
			}
			this.EndCard();
		}

		private void DrawMiscOtherCard()
		{
			if (!this.matchesSearch("其他功能")) return;

			this.BeginCard("其他功能");
			SettingsStore.AntiMouse1 = this.CustomToggle(SettingsStore.AntiMouse1, "屏蔽右键");
			SettingsStore.SpreadPredict = this.CustomToggle(SettingsStore.SpreadPredict, "扩散预测");
			this.EndCard();
		}

		private void DrawAutoFireControllerCard()
		{
			if (!this.matchesSearch("自动扳机")) return;

			this.BeginCard("自动扳机", ref SettingsStore.AutoFireController);
			if (SettingsStore.AutoFireController)
			{
				SettingsStore.ExcludeSniper = this.CustomToggle(SettingsStore.ExcludeSniper, "排除狙击枪");
				SettingsStore.AutoFireControllerDelayedActivation = this.CustomToggle(SettingsStore.AutoFireControllerDelayedActivation, "延迟扳机");
				if (SettingsStore.AutoFireControllerDelayedActivation)
				{
					this.CustomSlider("持续时长", ref SettingsStore.AutoFireControllerActiveDuration, 0f, 10f);
				}
			}
			this.EndCard();
		}

		private void DrawSpammerCard()
		{
			if (!this.matchesSearch("自动喊话")) return;

			this.BeginCard("自动喊话", ref SettingsStore.MessageDispatcher);
			if (SettingsStore.MessageDispatcher)
			{
				GUILayout.Label("喊话内容:", Menu.S.LabelStyle);
				SettingsStore.SendMsg = GUILayout.TextField(SettingsStore.SendMsg, Menu.S.TextFieldStyle, GUILayout.Height(24f));
			}
			this.EndCard();
		}

		private void DrawSettingsStoreListCard()
		{
			if (!this.matchesSearch("配置管理")) return;

			this.BeginCard("已保存配置列表");
			this.cfgScroll = GUILayout.BeginScrollView(this.cfgScroll, GUILayout.Height(220f));
			foreach (string cfgName in SettingsHelper.Names)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label(cfgName, Menu.S.LabelStyle, GUILayout.Width(100f));
				GUILayout.FlexibleSpace();

				if (GUILayout.Button("载入", Menu.S.ButtonStyle, GUILayout.Width(50f), GUILayout.Height(22f)))
				{
					SettingsHelper.Load(cfgName);
					this.Pop("已载入配置 " + cfgName);
				}
				GUILayout.Space(5f);
				if (cfgName != "default")
				{
					if (GUILayout.Button("删除", Menu.S.ButtonStyle, GUILayout.Width(50f), GUILayout.Height(22f)))
					{
						this.delCfg = cfgName;
						this.confirmDel = true;
					}
				}
				GUILayout.EndHorizontal();
				GUILayout.Space(5f);
			}
			GUILayout.EndScrollView();
			this.EndCard();
		}

		private void DrawSettingsStoreCreateCard()
		{
			if (!this.matchesSearch("新建配置")) return;

			this.BeginCard("新建/保存配置");
			GUILayout.Label("当前配置: " + SettingsHelper.Current, Menu.S.HintStyle);
			GUILayout.Space(8f);

			GUILayout.BeginHorizontal();
			this.newCfg = GUILayout.TextField(this.newCfg, Menu.S.TextFieldStyle, GUILayout.Height(24f));
			GUILayout.Space(8f);
			if (GUILayout.Button("保存/新建", Menu.S.ButtonStyle, GUILayout.Width(90f), GUILayout.Height(24f)) && !string.IsNullOrEmpty(this.newCfg))
			{
				SettingsHelper.Save(this.newCfg);
				this.Pop("已保存配置 " + this.newCfg);
				this.newCfg = "";
			}
			GUILayout.EndHorizontal();
			this.EndCard();
		}

		private void AssignKey(string id, KeyCode k)
		{
			if (id == "Aim")
			{
				SettingsStore.AimKey = k;
				return;
			}
			if (id == "Rage")
			{
				SettingsStore.ShotCalculator_Key = k;
				return;
			}
			if (id == "Res")
			{
				SettingsStore.PitchSynchronizerKey = k;
				return;
			}
			if (!(id == "3rd"))
			{
				return;
			}
			SettingsStore.ThirdPersonKey = k;
		}

		private void Start()
		{
			this.show = true;
			base.useGUILayout = true;
			Debug.Log("[菜单] Menu组件初始化完成，默认开启菜单，F12为开关按钮");
			SettingsHelper.Init();
			AppearanceModifier.Initialize();
			this.dropdowns["aim"] = new Menu.DropdownState();
			this.dropdowns["backAccessory"] = new Menu.DropdownState();
			this.dropdowns["character"] = new Menu.DropdownState();
			this.dropdowns["weapon"] = new Menu.DropdownState();
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.F12))
			{
				this.show = !this.show;
				base.useGUILayout = this.show;
				Debug.Log("[菜单] 按下 F12，切换菜单显示状态 " + this.show);
			}
			if (Input.GetKeyDown(SettingsStore.ThirdPersonKey))
			{
				SettingsStore.ThirdPerson = !SettingsStore.ThirdPerson;
			}
			if (this.popup && Time.time - this.popupTime > 3f)
			{
				this.popup = false;
			}
			if (this.bindWait)
			{
				this.HandleKeyBinding();
			}
		}

		private void DoDeleteWindow(int id)
		{
			GUI.backgroundColor = Color.white;
			GUILayout.FlexibleSpace();
			GUILayout.Label("确定要永久删除配置\n[" + this.delCfg + "] 吗?", Menu.S.CenterLabelStyle, Array.Empty<GUILayoutOption>());
			GUILayout.FlexibleSpace();
			GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
			if (GUILayout.Button("取消", Menu.S.ButtonStyle, GUILayout.Height(24f)))
			{
				this.confirmDel = false;
				this.delCfg = "";
			}
			GUILayout.Space(10f);
			if (GUILayout.Button("删除", Menu.S.ButtonStyle, GUILayout.Height(24f)))
			{
				SettingsHelper.Delete(this.delCfg);
				this.confirmDel = false;
				this.delCfg = "";
				this.Pop("配置已删除");
			}
			GUILayout.EndHorizontal();
			GUILayout.Space(10f);
			GUI.DragWindow(new Rect(0f, 0f, 1000f, 20f));
		}

		private void CloseAllDropdowns()
		{
			foreach (Menu.DropdownState dropdownState in this.dropdowns.Values)
			{
				dropdownState.IsOpen = false;
			}
			this.activeDropdownKey = null;
		}

		private void HandleKeyBinding()
		{
			foreach (object obj in Enum.GetValues(typeof(KeyCode)))
			{
				KeyCode keyCode = (KeyCode)obj;
				if (Input.GetKeyDown(keyCode))
				{
					if (keyCode == (KeyCode)27)
					{
						this.AssignKey(this.bindKey, KeyCode.None);
					}
					else
					{
						if (keyCode == (KeyCode)278)
						{
							continue;
						}
						this.AssignKey(this.bindKey, keyCode);
					}
					this.bindWait = false;
					this.bindKey = null;
					break;
				}
			}
			for (int i = 0; i < 3; i++)
			{
				if (Input.GetMouseButtonDown(i))
				{
					this.AssignKey(this.bindKey, (KeyCode)((i == 0) ? 323 : ((i == 1) ? 324 : 325)));
					this.bindWait = false;
					this.bindKey = null;
					return;
				}
			}
		}

		private void Pop(string m)
		{
			this.popup = true;
			this.popupMsg = m;
			this.popupTime = Time.time;
		}

		private void OnGUI()
		{
			if (!this.show)
			{
				return;
			}
			Menu.S.Setup();
			Event current = Event.current;
			if (current.type == EventType.MouseDown)
			{
				if (!string.IsNullOrEmpty(this.activeDropdownKey) && this.dropdowns.ContainsKey(this.activeDropdownKey))
				{
					var dropdownState = this.dropdowns[this.activeDropdownKey];
					if (dropdownState.IsOpen && !dropdownState.Rect.Contains(current.mousePosition))
					{
						dropdownState.IsOpen = false;
						this.activeDropdownKey = null;
					}
				}
			}
			if (current.type == EventType.ScrollWheel)
			{
				if (!string.IsNullOrEmpty(this.activeDropdownKey) && this.dropdowns.ContainsKey(this.activeDropdownKey))
				{
					var dropdownState = this.dropdowns[this.activeDropdownKey];
					if (dropdownState.IsOpen && this.activeDropdownItems != null)
					{
						Rect screenRect = new Rect(
							dropdownState.Rect.x + this.winRect.x,
							dropdownState.Rect.y + this.winRect.y,
							dropdownState.Rect.width,
							dropdownState.Rect.height);
						if (screenRect.Contains(current.mousePosition))
						{
							float contentHeight = this.activeDropdownItems.Count * 20f;
							float maxScroll = Mathf.Max(0f, contentHeight - dropdownState.Rect.height);
							dropdownState.Scroll.y += current.delta.y * 20f;
							dropdownState.Scroll.y = Mathf.Clamp(dropdownState.Scroll.y, 0f, maxScroll);
							current.Use();
						}
					}
				}
			}

			GUIStyle windowStyle = Menu.S.WindowStyle;
			this.winRect = GUI.Window(0, this.winRect, new GUI.WindowFunction(this.DoWin), "", windowStyle);

			if (this.confirmDel)
			{
				this.delWinRect = new Rect(this.winRect.x + (this.winRect.width - 260f) / 2f, this.winRect.y + (this.winRect.height - 130f) / 2f, 260f, 130f);
				GUI.Window(1, this.delWinRect, new GUI.WindowFunction(this.DoDeleteWindow), "", Menu.S.DropdownPopupStyle);
				GUI.BringWindowToFront(1);
			}
		}

		private bool confirmDel;
		private float popupTime;
		private Rect delWinRect;
		private string newCfg = "";
		private string bindKey;
		private Vector2 scroll;
		private bool bindWait;
		private string popupMsg = "";
		private Dictionary<string, Menu.DropdownState> dropdowns = new Dictionary<string, Menu.DropdownState>();
		private static readonly string[] Bones = new string[]
		{
			"头心", "头顶", "脖子", "腹部", "左锁骨", "右锁骨", "左上臂", "右上臂",
			"左前臂", "右前臂", "左手", "右手", "左指", "右指", "骨盆", "左腿",
			"右腿", "左膝", "右膝", "左脚", "右脚", "左趾", "右趾"
		};
		private Vector2 cfgScroll;
		private Rect winRect = new Rect(100f, 100f, 1080f, 760f);
		private bool popup;
		private int tab;
		private string delCfg = "";
		private bool show = true;

		private string searchQuery = "";
		private string activeDropdownKey = null;
		private List<string> activeDropdownItems = null;
		private Action<string> activeDropdownOnSelect = null;

		private class DropdownState
		{
			public Vector2 Scroll;
			public int SelectedIndex;
			public Rect Rect;
			public bool IsOpen;
		}

		private static class S
		{
			public static Texture2D Tex;
			public static Texture2D SidebarBgTex;
			public static Texture2D TabSelectedTex;
			public static Texture2D TabHoverTex;
			public static Texture2D AccentBlueTex;
			public static Texture2D SearchBgTex;
			public static Texture2D CardBgTex;
			public static Texture2D ToggleOnTex;
			public static Texture2D ToggleOffTex;
			public static Texture2D SliderTrackTex;
			public static Texture2D SliderFillTex;
			public static Texture2D DropdownTex;
			public static Texture2D DropdownPopupTex;
			public static Texture2D WhiteCircleTex;
			public static Texture2D LogoTex;
			public static Texture2D AvatarTex;

			public static GUIStyle WindowStyle;
			public static GUIStyle LogoTextStyle;
			public static GUIStyle SidebarTitleStyle;
			public static GUIStyle SidebarSubtitleStyle;
			public static GUIStyle TabNormalTextStyle;
			public static GUIStyle TabActiveTextStyle;
			public static GUIStyle AvatarNameStyle;
			public static GUIStyle AvatarBetaStyle;
			public static GUIStyle SearchIconStyle;
			public static GUIStyle SearchTextFieldStyle;
			public static GUIStyle CardStyle;
			public static GUIStyle CardTitleStyle;
			public static GUIStyle LabelStyle;
			public static GUIStyle ValueLabelStyle;
			public static GUIStyle DropdownStyle;
			public static GUIStyle DropdownPopupStyle;
			public static GUIStyle DropdownItemStyle;
			public static GUIStyle ButtonStyle;
			public static GUIStyle TextFieldStyle;
			public static GUIStyle HintStyle;
			public static GUIStyle CenterLabelStyle;

			public static bool Init;

			public static Texture2D CreateRoundedTex(int width, int height, int radius, Color color)
			{
				Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
				Color[] pixels = new Color[width * height];
				for (int y = 0; y < height; y++)
				{
					for (int x = 0; x < width; x++)
					{
						bool inside = true;
						if (x < radius && y < radius)
						{
							inside = (x - radius) * (x - radius) + (y - radius) * (y - radius) <= radius * radius;
						}
						else if (x < radius && y >= height - radius)
						{
							inside = (x - radius) * (x - radius) + (y - (height - radius)) * (y - (height - radius)) <= radius * radius;
						}
						else if (x >= width - radius && y < radius)
						{
							inside = (x - (width - radius)) * (x - (width - radius)) + (y - radius) * (y - radius) <= radius * radius;
						}
						else if (x >= width - radius && y >= height - radius)
						{
							inside = (x - (width - radius)) * (x - (width - radius)) + (y - (height - radius)) * (y - (height - radius)) <= radius * radius;
						}
						pixels[y * width + x] = inside ? color : Color.clear;
					}
				}
				tex.SetPixels(pixels);
				tex.Apply();
				return tex;
			}

			public static Texture2D CreateSteveAvatar()
			{
				Texture2D tex = new Texture2D(8, 8, TextureFormat.RGBA32, false);
				tex.filterMode = FilterMode.Point;
				Color hair = new Color(0.37f, 0.22f, 0.13f);
				Color skin = new Color(0.77f, 0.56f, 0.42f);
				Color eyeB = new Color(0.36f, 0.56f, 0.68f);
				Color eyeW = Color.white;
				Color mouth = new Color(0.61f, 0.37f, 0.28f);
				Color[] p = new Color[64]
				{
					skin, skin, skin, skin, skin, skin, skin, skin,
					skin, skin, skin, skin, skin, skin, skin, skin,
					skin, skin, mouth, mouth, mouth, mouth, skin, skin,
					skin, skin, skin, mouth, mouth, skin, skin, skin,
					skin, eyeW, eyeB, skin, skin, eyeB, eyeW, skin,
					hair, skin, skin, skin, skin, skin, skin, hair,
					hair, hair, hair, hair, hair, hair, hair, hair,
					hair, hair, hair, hair, hair, hair, hair, hair
				};
				tex.SetPixels(p);
				tex.Apply();
				return tex;
			}

			public static void Setup()
			{
				if (Init) return;

				Tex = Texture2D.whiteTexture;

				Color winColor = new Color(0.06f, 0.07f, 0.09f, 0.98f);
				Color sidebarColor = new Color(0.035f, 0.04f, 0.05f, 1f);
				Color cardColor = new Color(0.09f, 0.10f, 0.13f, 1f);
				Color buttonColor = new Color(0.12f, 0.13f, 0.17f, 1f);
				Color tabHoverColor = new Color(0.08f, 0.09f, 0.12f, 0.6f);
				Color tabSelectedColor = new Color(0.09f, 0.10f, 0.13f, 1f);
				Color accentBlue = new Color(0f, 0.59f, 1f, 1f);
				Color labelGrey = new Color(0.55f, 0.56f, 0.61f, 1f);
				Color toggleOffColor = new Color(0.12f, 0.13f, 0.16f, 1f);
				Color sliderTrackColor = new Color(0.07f, 0.08f, 0.10f, 1f);
				Color greenBeta = new Color(0f, 0.8f, 0.35f, 1f);

				SidebarBgTex = CreateRoundedTex(200, 760, 0, sidebarColor);
				TabSelectedTex = CreateRoundedTex(170, 40, 6, tabSelectedColor);
				TabHoverTex = CreateRoundedTex(170, 40, 6, tabHoverColor);
				AccentBlueTex = CreateRoundedTex(3, 20, 1, accentBlue);
				SearchBgTex = CreateRoundedTex(715, 36, 6, cardColor);
				CardBgTex = CreateRoundedTex(345, 32, 8, cardColor);
				ToggleOnTex = CreateRoundedTex(34, 18, 9, accentBlue);
				ToggleOffTex = CreateRoundedTex(34, 18, 9, toggleOffColor);
				SliderTrackTex = CreateRoundedTex(100, 4, 2, sliderTrackColor);
				SliderFillTex = CreateRoundedTex(100, 4, 2, accentBlue);
				DropdownTex = CreateRoundedTex(100, 22, 4, buttonColor);
				DropdownPopupTex = CreateRoundedTex(100, 100, 4, cardColor);
				WhiteCircleTex = CreateRoundedTex(14, 14, 7, Color.white);
				LogoTex = CreateRoundedTex(32, 32, 16, accentBlue);
				AvatarTex = CreateSteveAvatar();

				WindowStyle = new GUIStyle();
				WindowStyle.normal.background = CreateRoundedTex(1080, 760, 12, winColor);

				LogoTextStyle = new GUIStyle(GUI.skin.label)
				{
					alignment = TextAnchor.MiddleCenter,
					fontSize = 14,
					fontStyle = FontStyle.Bold,
					normal = { textColor = Color.white }
				};

				SidebarTitleStyle = new GUIStyle(GUI.skin.label)
				{
					fontSize = 15,
					fontStyle = FontStyle.Bold,
					normal = { textColor = Color.white }
				};

				SidebarSubtitleStyle = new GUIStyle(GUI.skin.label)
				{
					fontSize = 12,
					normal = { textColor = labelGrey }
				};

				TabNormalTextStyle = new GUIStyle(GUI.skin.label)
				{
					fontSize = 13,
					normal = { textColor = labelGrey }
				};

				TabActiveTextStyle = new GUIStyle(GUI.skin.label)
				{
					fontSize = 13,
					fontStyle = FontStyle.Bold,
					normal = { textColor = Color.white }
				};

				AvatarNameStyle = new GUIStyle(GUI.skin.label)
				{
					fontSize = 13,
					fontStyle = FontStyle.Bold,
					normal = { textColor = Color.white }
				};

				AvatarBetaStyle = new GUIStyle(GUI.skin.label)
				{
					fontSize = 11,
					normal = { textColor = greenBeta }
				};

				SearchIconStyle = new GUIStyle(GUI.skin.label)
				{
					alignment = TextAnchor.MiddleCenter,
					fontSize = 13,
					normal = { textColor = labelGrey }
				};

				SearchTextFieldStyle = new GUIStyle(GUI.skin.textField)
				{
					fontSize = 13,
					normal = { textColor = Color.white, background = Tex },
					focused = { textColor = Color.white, background = Tex }
				};
				SearchTextFieldStyle.border = new RectOffset(0, 0, 0, 0);

				CardStyle = new GUIStyle(GUI.skin.box);
				CardStyle.normal.background = CardBgTex;
				CardStyle.border = new RectOffset(8, 8, 8, 8);
				CardStyle.padding = new RectOffset(12, 12, 12, 12);
				CardStyle.margin = new RectOffset(0, 0, 0, 15);

				CardTitleStyle = new GUIStyle(GUI.skin.label)
				{
					fontSize = 14,
					fontStyle = FontStyle.Bold,
					normal = { textColor = Color.white }
				};

				LabelStyle = new GUIStyle(GUI.skin.label)
				{
					fontSize = 13,
					normal = { textColor = labelGrey }
				};

				ValueLabelStyle = new GUIStyle(GUI.skin.label)
				{
					alignment = TextAnchor.MiddleRight,
					fontSize = 12,
					normal = { textColor = Color.white }
				};

				DropdownStyle = new GUIStyle(GUI.skin.button)
				{
					fontSize = 12,
					normal = { textColor = Color.white, background = DropdownTex },
					hover = { textColor = Color.white, background = DropdownTex },
					active = { textColor = Color.white, background = DropdownTex }
				};
				DropdownStyle.border = new RectOffset(4, 4, 4, 4);

				DropdownPopupStyle = new GUIStyle(GUI.skin.box);
				DropdownPopupStyle.normal.background = DropdownPopupTex;
				DropdownPopupStyle.border = new RectOffset(4, 4, 4, 4);

				DropdownItemStyle = new GUIStyle(GUI.skin.button)
				{
					fontSize = 12,
					alignment = TextAnchor.MiddleLeft,
					normal = { textColor = labelGrey, background = Tex },
					hover = { textColor = Color.white, background = TabSelectedTex }
				};
				DropdownItemStyle.padding = new RectOffset(8, 0, 0, 0);

				ButtonStyle = new GUIStyle(GUI.skin.button)
				{
					fontSize = 13,
					normal = { textColor = Color.white, background = DropdownTex },
					hover = { textColor = Color.white, background = DropdownTex }
				};
				ButtonStyle.border = new RectOffset(4, 4, 4, 4);

				TextFieldStyle = new GUIStyle(GUI.skin.textField)
				{
					fontSize = 13,
					normal = { textColor = Color.white, background = DropdownTex },
					focused = { textColor = Color.white, background = DropdownTex }
				};
				TextFieldStyle.border = new RectOffset(4, 4, 4, 4);

				HintStyle = new GUIStyle(GUI.skin.label)
				{
					fontSize = 12,
					normal = { textColor = labelGrey }
				};

				CenterLabelStyle = new GUIStyle(GUI.skin.label)
				{
					alignment = TextAnchor.MiddleCenter,
					fontSize = 13,
					normal = { textColor = Color.white }
				};

				Init = true;
			}
		}
	}
}