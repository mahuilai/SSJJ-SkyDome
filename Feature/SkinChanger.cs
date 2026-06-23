using System;
using System.Collections.Generic;
using System.Reflection;
using NetData;
using SkyDome.Entity;
using UnityEngine;
namespace SkyDome.Feature
{
	public class AppearanceModifier : MonoBehaviour
	{
		private static Type FindType(string fullName)
		{
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			for (int i = 0; i < assemblies.Length; i++)
			{
				Type type = assemblies[i].GetType(fullName);
				if (type != null)
				{
					return type;
				}
			}
			return null;
		}
		private static void LoadConstantsFromType(Type type, List<string> targetList)
		{
			if (type == null)
			{
				return;
			}
			foreach (FieldInfo fieldInfo in type.GetFields((BindingFlags)88))
			{
				if (fieldInfo.FieldType == typeof(string))
				{
					object value = fieldInfo.GetValue(null);
					if (value != null)
					{
						targetList.Add(value.ToString());
					}
				}
			}
		}

        public static void ChangeHeadEnlarge(float scale)
		{
			if (PlayerStateTracker.LocalEntity == null)
			{
				return;
			}
			PlayerStateTracker.LocalEntity._entity.basicInfo.Current.HeadEnlarge = scale;
		}
		public static void ChangeSelfAlpha(int alpha)
		{
			if (PlayerStateTracker.LocalEntity == null)
			{
				return;
			}
			PlayerStateTracker.LocalEntity._entity.basicInfo.Current.SelfAlpha = alpha;
		}
		public static void ChangeCharacter(string name)
		{
			if (PlayerStateTracker.LocalEntity == null)
			{
				return;
			}
			PlayerEntityData current = PlayerStateTracker.LocalEntity._entity.basicInfo.Current;
			current.Career = name;
			current.CurrentHandName = name;
		}
		public static void ChangeAlpha(int alpha)
		{
			if (PlayerStateTracker.LocalEntity == null)
			{
				return;
			}
			PlayerStateTracker.LocalEntity._entity.basicInfo.Current.Alpha = alpha;
		}

        public static void ChangeBackAccessory(string name)
		{
			if (PlayerStateTracker.LocalEntity == null)
			{
				return;
			}
			PlayerStateTracker.LocalEntity._entity.basicInfo.Current.BackAccessory = name;
		}
		public static void ChangeWeapon(string name)
		{
			if (PlayerStateTracker.LocalEntity == null)
			{
				return;
			}
			PlayerStateTracker.LocalEntity._entity.basicInfo.Current.CurrentWeaponName = name;
		}
		public static void Initialize()
		{
			if (AppearanceModifier._isInitialized)
			{
				return;
			}
			try
			{
				Type type = AppearanceModifier.FindType("share.constant.PlayerCareerConstant");
				if (type != null)
				{
					AppearanceModifier.LoadConstantsFromType(type, AppearanceModifier.CharacterNames);
				}
				Type type2 = AppearanceModifier.FindType("Assets.Sources.Constant.Weapon.FreeWeaponConstant");
				if (type2 != null)
				{
					AppearanceModifier.LoadConstantsFromType(type2, AppearanceModifier.WeaponNames);
				}
				Type type3 = AppearanceModifier.FindType("Assets.Sources.Constant.Weapon.WeaponConstant");
				if (type3 != null)
				{
					AppearanceModifier.LoadConstantsFromType(type3, AppearanceModifier.WeaponNames);
				}
				Type type4 = AppearanceModifier.FindType("share.constant.BackAccessoryConstant");
				if (type4 != null)
				{
					AppearanceModifier.LoadConstantsFromType(type4, AppearanceModifier.BackAccessoryNames);
				}
				else
				{
					AppearanceModifier.BackAccessoryNames.AddRange(new List<string>
					{
						"jetpack",
						"wing_zhandouopen",
						"chibang1open"
					});
				}
				AppearanceModifier._isInitialized = true;
			}
			catch
			{
			}
		}
		public static void ChangeScale(float scale)
		{
			if (PlayerStateTracker.LocalEntity == null)
			{
				return;
			}
			PlayerStateTracker.LocalEntity._entity.basicInfo.Current.Scale = scale;
		}

        public static void ChangeTeam(int teamId)
		{
			if (PlayerStateTracker.LocalEntity == null)
			{
				return;
			}
			PlayerStateTracker.LocalEntity._entity.basicInfo.Current.Team = teamId;
		}
		public static List<string> BackAccessoryNames = new List<string>();
		private static bool _isInitialized = false;
		public static List<string> WeaponNames = new List<string>();
		public static List<string> CharacterNames = new List<string>();
	}
}