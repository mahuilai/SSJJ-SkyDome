using System;
using System.Collections.Generic;
using Assets.Sources.Modules.Player.HitBox;
using Assets.Sources.Utils.Weapon;
using share;
using SkyDome.Cfg;
using SkyDome.Entity;
using SkyDome.Utilities;
using SSJJBase.String;
using SSJJMath;
using UnityEngine;
using weapon.utils;
public static class ShotCalculator
{

    static ShotCalculator()
	{
		string[] array = new string[-187 + 8 + 188];
		array[-919 + 919] = "Bip01_Head";
		array[-884 + 1 + 884] = "Bip01_Neck";
		array[855 - 16 - 837] = "Bip01_R_Forearm";
		array[-697 + 4 + 696] = "Bip01_L_Forearm";
		array[2 * 2] = "Bip01_R_Hand";
		array[22 + 1 - 18] = "Bip01_L_Hand";
		array[929 - 923] = "Bip01_Pelvis";
		array[-664 - 16 + 687] = "Bip01_R_Calf";
		array[-573 - 1 + 582] = "Bip01_L_Calf";
		ShotCalculator.BoneNames = array;
		ShotCalculator.InitializeBones();
	}
	private static global::UnityEngine.Vector3 CalculateSpreadOffset(global::UnityEngine.Vector3 baseDirection)
	{
		int seq = Contexts.sharedInstance.userCommand.commands.CommandToSendList.Last.Value.Seq;
		WeaponEntity currentWeaponEntity = Contexts.sharedInstance.weapon.currentWeaponEntity;
		double num = FireUtility.CalShotsFiredSpread(currentWeaponEntity.basicInfo.Data.ShotsFiredSpreadMin, currentWeaponEntity.basicInfo.Data.ShotsFiredSpreadMax, currentWeaponEntity.basicInfo.Data.ShotsFiredSpreadTime, currentWeaponEntity.basicInfo.Data.ShotsFired, currentWeaponEntity.basicInfo.Info.AttackInterval);
		double num2 = UniformRandom.RandomFloat(seq, -0.5, 0.5) + UniformRandom.RandomFloat(seq + 1, -0.5, 0.5);
		double num3 = UniformRandom.RandomFloat(seq + 2, -0.5, 0.5) + UniformRandom.RandomFloat(seq + 3, -0.5, 0.5);
		float spread = Contexts.sharedInstance.weapon.currentWeaponEntity.spread.Spread;
		float spreadScaleY = Contexts.sharedInstance.weapon.currentWeaponEntity.spread.SpreadScaleY;
		double num4 = (double)spread * num2 * num;
		double num5 = (double)spread * num3 * num * (double)spreadScaleY;
		global::UnityEngine.Vector3 normalized = global::UnityEngine.Vector3.Cross(baseDirection, global::UnityEngine.Vector3.up).normalized;
		global::UnityEngine.Vector3 normalized2 = global::UnityEngine.Vector3.Cross(normalized, baseDirection).normalized;
		return normalized * (float)num4 + normalized2 * (float)num5;
	}
	private static void InitializeBones()
	{
		foreach (string text in ShotCalculator.BoneNames)
		{
			ShotCalculator.BoneHashes.Add(new IgnoreCaseString(text).GetHashCode());
		}
	}

    public static bool SilentTargetSelector(List<SkyDome.Entity.PlayerData> players, SkyDome.Entity.PlayerData localPlayer, ref float yaw, ref float pitch)
	{
		if (!SettingsStore.ShotCalculator)
		{
			return false;
		}
		int num = 0;
		float num2 = float.MaxValue;
		Transform transform = null;
		SkyDome.Entity.PlayerData PlayerData = null;
		Vector2 vector = new Vector2((float)Screen.width / 2f, (float)Screen.height / 2f);
		global::UnityEngine.Vector3 vector2 = VectorCoordConverter.SsjjToUnity(localPlayer.Position + new global::UnityEngine.Vector3(0f, 0f, (float)localPlayer.Move.PyPlayerMove.GetViewHeight()));
		foreach (SkyDome.Entity.PlayerData PlayerData2 in players)
		{
			if (PlayerData2 != null && PlayerData2 != localPlayer && PlayerData2._entity.hasHitBox && PlayerData2._entity.hasThirdPersonUnityObjects && PlayerData2.Team != localPlayer.Team && (double)PlayerData2.HpPercent >= 0.0 && !PlayerData2.IsDead && !PlayerData2.State.GetPlayerStateType(1))
			{
				if (PlayerData2._entity.hitBox.HitBoxBrushDirty)
				{
					PlayerHitBoxBrushUtility.UpdatePlayerAllHitBoxBrush(PlayerData2._entity);
				}
				foreach (int num3 in ShotCalculator.BoneHashes)
				{
					Transform transform2;
					if (PlayerData2._entity.hitBox.BonetTransform.TryGetValue(num3, out transform2) && !(Camera.main == null))
					{
						global::UnityEngine.Vector3 vector3 = ViewportUtility.WorldPointToScreenPoint(transform2.position);
						if (vector3.z > 0.01f)
						{
							Vector2 vector4 = new Vector2(vector3.x, vector3.y);
							float num4 = Vector2.Distance(vector, vector4);
							if (num4 < num2)
							{
								num2 = num4;
								transform = transform2;
								PlayerData = PlayerData2;
								num = PlayerData2.Id;
							}
							if (ShotCalculator.CanAim(localPlayer, PlayerData2, vector2, transform2.position))
							{
								global::UnityEngine.Vector3 vector5 = ShotCalculator.FixPosition(localPlayer, PlayerData2, vector2, transform2.position);
								yaw = vector5.y;
								pitch = vector5.x;
								return true;
							}
							if (!ShotCalculator.checkAllbones)
							{
								break;
							}
						}
					}
				}
			}
		}
		ShotCalculator._currentTargetId = num;
		if (SettingsStore.AngleCorrector && PlayerData != null && ShotCalculator.CanAim(localPlayer, PlayerData, vector2, transform.position))
		{
			global::UnityEngine.Vector3 vector6 = ShotCalculator.FixPosition(localPlayer, PlayerData, vector2, transform.position);
			yaw = vector6.y;
			pitch = vector6.x;
			return true;
		}
		return false;
	}

    public static global::UnityEngine.Vector3 NormalizeAngles(global::UnityEngine.Vector3 angles)
	{
		if (angles.x > 89f)
		{
			angles.x -= 180f;
		}
		if (angles.x < -89f)
		{
			angles.x += 180f;
		}
		angles.y %= 360f;
		if (angles.y > 180f)
		{
			angles.y -= 360f;
		}
		return angles;
	}
	public static void ApplyViewStabilizer(SkyDome.Entity.PlayerData player, ref global::UnityEngine.Vector3 angles)
	{
		if (player != null)
		{
			angles.x -= 2f * player.Punch.x;
			angles.y -= 2f * player.Punch.y;
		}
	}
	private static float ClampValue(float value, float min, float max)
	{
		if (value <= min)
		{
			return min;
		}
		if (value >= max)
		{
			return max;
		}
		return value;
	}

    public static global::UnityEngine.Vector3 ClampAngles(global::UnityEngine.Vector3 angles)
	{
		angles.x = ShotCalculator.ClampValue(angles.x, -89f, 89f);
		angles.y = ShotCalculator.ClampValue(angles.y, -180f, 180f);
		angles.z = 0f;
		return angles;
	}
	private static bool CanAim(SkyDome.Entity.PlayerData shooter, SkyDome.Entity.PlayerData target, global::UnityEngine.Vector3 startPos, global::UnityEngine.Vector3 endPos)
	{
		global::UnityEngine.Vector3 vector = VectorCoordConverter.UnityToSsjj((endPos - startPos).normalized);
		int entityId = SkyDome.Utilities.PathRendererHelper.GetEntityId(
			Contexts.sharedInstance.battleRoom.pyEngine.PyEngine, 
			shooter._entity, 
			Contexts.sharedInstance.player, 
			100000f, 
			new Vector3D((double)vector.x, (double)vector.y, (double)vector.z), 
			new float[3], 
			new float[3], 
			false
		);
		return entityId == target.Id;
	}
	public static global::UnityEngine.Vector3 FixPosition(SkyDome.Entity.PlayerData shooter, SkyDome.Entity.PlayerData target, global::UnityEngine.Vector3 shooterPos, global::UnityEngine.Vector3 targetPos)
	{
		float num = (float)Contexts.sharedInstance.userCommand.commands.CommandToSendList.Last.Value.FrameInterval * 0.001f;
		global::UnityEngine.Vector3 vector = shooterPos + VectorCoordConverter.SsjjToUnity(shooter.Move.Velocity) * num;
		global::UnityEngine.Vector3 vector2 = targetPos + VectorCoordConverter.SsjjToUnity(target.Move.Velocity) * num;
		global::UnityEngine.Vector3 vector3 = ShotCalculator.CalculateAimAngle(vector, vector2);
		vector3 = ShotCalculator.NormalizeAngles(vector3);
		vector3 = ShotCalculator.ClampAngles(vector3);
		ShotCalculator.ApplyViewStabilizer(shooter, ref vector3);
		vector3 = ShotCalculator.NormalizeAngles(vector3);
		return ShotCalculator.ClampAngles(vector3);
	}
	public static global::UnityEngine.Vector3 CalculateAimAngle(global::UnityEngine.Vector3 startPos, global::UnityEngine.Vector3 targetPos)
	{
		global::UnityEngine.Vector3 normalized = (targetPos - startPos).normalized;
		global::UnityEngine.Vector3 vector = ShotCalculator.CalculateSpreadOffset(normalized);
		global::UnityEngine.Vector3 normalized2 = (normalized + vector).normalized;
		float num = Mathf.Atan2(normalized2.z, normalized2.x) * 57.29578f - 90f;
		float num2 = Mathf.Asin(normalized2.y / normalized2.magnitude) * 57.29578f;
		if (num < -180f)
		{
			num += 360f;
		}
		if (num > 180f)
		{
			num -= 360f;
		}
		num = Mathf.Clamp(num, -180f, 180f);
		return new global::UnityEngine.Vector3(Mathf.Clamp(num2, -89f, 89f), num, 0f);
	}
	private static bool checkAllbones;
	private static int _currentTargetId;
	private static readonly List<int> BoneHashes = new List<int>();
	private static readonly string[] BoneNames;
}