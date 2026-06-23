using System;
using Assets.Sources.Components.Interface.Info.Weapon;
using Assets.Sources.Utils.Player;
using Assets.Sources.Utils.Weapon;
using data;
using physics;
using SkyDome.Cfg;
using SkyDome.Entity;
using SSJJUserCmd;
using UnityEngine;
public static class AngleCorrector
{
	public static float SharedPitch { get; private set; }
	private static void FixMove(float targetYaw, float originalYaw, ref float forwardMove, ref float rightMove)
	{
		float num = ((originalYaw >= 0f) ? originalYaw : (originalYaw + 360f));
		float num2 = AngleCorrector.CalculateAngleDifference((targetYaw >= 0f) ? targetYaw : (targetYaw + 360f), num);
		float num3 = 360f - num2;
		float num4 = forwardMove;
		float num5 = rightMove;
		float num6 = Mathf.Cos(num3 * 0.0174532924f);
		float num7 = Mathf.Sin(num3 * 0.0174532924f);
		float num8 = Mathf.Cos((num3 + 90f) * 0.0174532924f);
		float num9 = Mathf.Sin((num3 + 90f) * 0.0174532924f);
		forwardMove = num6 * num4 + num8 * num5;
		rightMove = num7 * num4 + num9 * num5;
		forwardMove = Mathf.Clamp(forwardMove, -100f, 100f);
		rightMove = Mathf.Clamp(rightMove, -100f, 100f);
	}

    public static bool IsSilentAiming { get; private set; }
	public static float SharedYaw { get; private set; }
	public static void SetYawAngle()
	{
		if (Input.GetKeyDown((KeyCode)122))
		{
			SettingsStore.AngleCorrector_Yaw = -90f;
		}
		if (Input.GetKeyDown((KeyCode)120))
		{
			SettingsStore.AngleCorrector_Yaw = -180f;
		}
		if (Input.GetKeyDown((KeyCode)99))
		{
			SettingsStore.AngleCorrector_Yaw = 90f;
		}
	}
	public static void SetPitchAngle(ref float pitch)
	{
		pitch = SettingsStore.AngleCorrector_PitchAngle;
	}

    public static void ExecuteAngleCorrector(ref float pitch, UserCmd userCmd, ref float _pitch, ref float _yaw, ref float _moveforward, ref float _moveright, ref int _buttons, ref bool _silenting)
	{
		if (PlayerStateTracker.LocalEntity != null && PlayerStateTracker.LocalEntity._entity != null && PlayerStateTracker.EntityList != null && Contexts.sharedInstance != null && Contexts.sharedInstance.weapon != null)
		{
			AngleCorrector.SetYawAngle();
			float num = 0f;
			if (SettingsStore.AngleCorrector_Mode == 2)
			{
				num = global::UnityEngine.Random.Range(SettingsStore.AngleCorrector_Jitter1, SettingsStore.AngleCorrector_Jitter2);
			}
			float num2 = (float)userCmd.CameraYaw / 100f;
			float num3 = (180f + num2 - SettingsStore.AngleCorrector_Yaw + num) % 360f - 180f;
			float num4;
			if (pitch != 0f)
			{
				num4 = pitch;
			}
			else
			{
				num4 = (float)userCmd.CameraPitch / 100f;
			}
			AngleCorrector.SetPitchAngle(ref num4);
			if (SettingsStore.AngleCorrector_Mode == 1)
			{
				num3 = (180f + num2 + 180f + (float)(userCmd.Seq * SettingsStore.AngleCorrector_SpinFactor % 360)) % 360f - 180f;
			}
			float num5 = userCmd.MoveForward;
			float num6 = userCmd.MoveRight;
			int num7 = userCmd.Buttons;
			bool flag = false;
			bool flag2 = Contexts.sharedInstance.weapon.currentWeaponEntity != null;
			AngleCorrector.WeaponSpread = AngleCorrector.CalculateWeaponSpread(userCmd);
			if (flag2)
			{
				bool flag3 = WeaponUtility.CanAttack(Contexts.sharedInstance.weapon.currentWeaponEntity, PlayerStateTracker.LocalEntity.CilentTime + userCmd.FrameInterval) && AngleCorrector.WeaponSpread >= SettingsStore.Accurary / 100f;
				flag = flag3;
			}
			bool flag4 = false;
			bool flag5 = flag && ShotCalculator.SilentTargetSelector(PlayerStateTracker.EntityList, PlayerStateTracker.LocalEntity, ref _yaw, ref _pitch);
			if (flag5)
			{
				WeaponEntity currentWeaponEntity = Contexts.sharedInstance.weapon.currentWeaponEntity;
				if (currentWeaponEntity != null && currentWeaponEntity.hasClip && currentWeaponEntity.clip != null && !userCmd.IsAttackOn)
				{
					if (currentWeaponEntity.clip.Clip > 0)
					{
						userCmd.Buttons |= 64;
						num7 |= 64;
					}
					if (currentWeaponEntity.clip.Clip2 > 0)
					{
						userCmd.Buttons |= 512;
						num7 |= 512;
					}
				}
				num3 = _yaw;
				num4 = _pitch;
				flag4 = true;
			}
			AngleCorrector.FixMove(num3, num2, ref num5, ref num6);
			bool flag6 = PlayerStateTracker.LocalEntity == null || PlayerStateTracker.LocalEntity.IsDead || (!SettingsStore.AngleCorrector && !flag4) || (!flag4 && flag && (userCmd.IsAttackOn || userCmd.IsSecondaryAttackOn));
			if (flag6)
			{
				num3 = num2;
				num4 = (float)userCmd.CameraPitch / 100f;
				num5 = userCmd.MoveForward;
				num6 = userCmd.MoveRight;
			}
			AngleCorrector.SharedYaw = num3;
			AngleCorrector.SharedPitch = num4;
			_pitch = num4;
			_yaw = num3;
			_buttons = num7;
			_moveforward = num5;
			_moveright = num6;
			_silenting = flag4;
			AngleCorrector.IsSilentAiming = flag4;
			return;
		}
		_pitch = (float)userCmd.CameraPitch / 100f;
		_yaw = (float)userCmd.CameraYaw / 100f;
		_moveforward = userCmd.MoveForward;
		_moveright = userCmd.MoveRight;
		_buttons = userCmd.Buttons;
		_silenting = false;
	}

    private static float CalculateAngleDifference(float angle1, float angle2)
	{
		if (angle1 < angle2)
		{
			return Mathf.Abs(angle1 - angle2);
		}
		return 360f - Mathf.Abs(angle1 - angle2);
	}
	public static float CalculateWeaponSpread(UserCmd userCommand)
	{
		if (Contexts.sharedInstance == null || Contexts.sharedInstance.weapon == null || Contexts.sharedInstance.battleRoom == null || Contexts.sharedInstance.weapon.currentWeaponEntity == null || Contexts.sharedInstance.player == null)
		{
			return 0f;
		}
		WeaponEntity currentWeaponEntity = Contexts.sharedInstance.weapon.currentWeaponEntity;
		if (currentWeaponEntity.basicInfo == null || currentWeaponEntity.basicInfo.Info == null)
		{
			return 0f;
		}
		if (!currentWeaponEntity.hasSpread || currentWeaponEntity.spread == null || !currentWeaponEntity.hasAccuracy || currentWeaponEntity.accuracy == null)
		{
			return 0f;
		}
		if (PlayerStateTracker.LocalEntity == null || PlayerStateTracker.LocalEntity._entity == null)
		{
			return 0f;
		}
		IEntitsWeaponInfo info = currentWeaponEntity.basicInfo.Info;
		if (Contexts.sharedInstance.battleRoom.pyEngine == null)
		{
			return 0f;
		}
		IPyEngine pyEngine = Contexts.sharedInstance.battleRoom.pyEngine.PyEngine;
		PlayerEntity entity = PlayerStateTracker.LocalEntity._entity;
		WeaponEntity currentWeaponEntity2 = Contexts.sharedInstance.weapon.currentWeaponEntity;
		if (pyEngine == null)
		{
			return 0f;
		}
		if (entity.hasClientTime && entity.clientTime != null)
		{
			SceneMoveData sceneMoveData = pyEngine.GetWorld().GetSceneMoveData() as SceneMoveData;
			bool flag = sceneMoveData != null && sceneMoveData.isWeightlessness;
			if (!userCommand.PredicatedOnce && info.AccuracyLogic != null && info.SpreadLogic != null)
			{
				info.SpreadLogic.BeforeFire(out currentWeaponEntity2.spread.Spread, entity, currentWeaponEntity2, userCommand, flag);
				info.AccuracyLogic.BeforeFire(userCommand.Seq, entity, currentWeaponEntity2, entity.clientTime.ClientTime);
			}
			float num = currentWeaponEntity2.spread.Spread;
			int weaponType = info.WeaponType;
			float num2;
			switch (weaponType)
			{
			case 0:
				num2 = currentWeaponEntity2.accuracy.Accuracy * 100f / 92f;
				goto IL_0313;
			case 1:
			case 6:
				goto IL_02D0;
			case 2:
			case 3:
			case 4:
				break;
			case 5:
			{
				num2 = 1f;
				float num3 = PlayerUtility.PlayerLength2D(entity);
				num = ((num3 > 350f) ? 0.4f : ((num3 > 25f) ? 0.7f : 0f));
				goto IL_0313;
			}
			default:
				switch (weaponType - 10)
				{
				case 0:
				case 2:
					num2 = 1f - (currentWeaponEntity2.accuracy.Accuracy - info.AccuracyOffset) * 100f / ((info.MaxInaccuracy - info.AccuracyOffset) * 100f);
					num = currentWeaponEntity2.spread.Spread;
					goto IL_0313;
				case 4:
					goto IL_02D0;
				}
				break;
			}
			num2 = 0f;
			num = currentWeaponEntity2.spread.Spread;
			goto IL_0313;
			IL_02D0:
			num2 = 1f - (currentWeaponEntity2.accuracy.Accuracy - info.DefaultAccuracy) * 100f / ((info.MaxInaccuracy - info.DefaultAccuracy) * 100f);
			num = currentWeaponEntity2.spread.Spread;
			IL_0313:
			return Mathf.Clamp(num2 - num, 0f, 1f);
		}
		return 0f;
	}
	private const float DegreesToRadians = 0.0174532924f;
	private static float WeaponSpread;
}