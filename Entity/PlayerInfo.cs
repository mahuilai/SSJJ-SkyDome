using System;
using Assets.Sources.Components.Player;
using Assets.Sources.Components.Player.UnityObjects;
using SSJJMath;
using UnityEngine;
namespace SkyDome.Entity
{
	public class PlayerData
	{
		public int Team
		{
			get
			{
				return this._entity.basicInfo.Current.Team;
			}
		}

        public string Career
		{
			get
			{
				return this._entity.basicInfo.Current.Career;
			}
		}
		public int CurrentWeaponId
		{
			get
			{
				return this._entity.basicInfo.Current.CurrentWeapon;
			}
		}
		public float HpPercent
		{
			get
			{
				if (this.MaxHp <= 0f)
				{
					return 0f;
				}
				return this.Hp / this.MaxHp;
			}
		}
		public MoveComponent Move
		{
			get
			{
				return this._entity.move;
			}
		}
		public bool OnGround
		{
			get
			{
				return this._entity.OnGround();
			}
		}

        public StateComponent State
		{
			get
			{
				return this._entity.state;
			}
		}
		public Vector3 Position
		{
			get
			{
				return this._entity.GetCompenstatePos(this._entity.fpos.Change.PosIndex);
			}
		}
		public float MoveYaw
		{
			get
			{
				return this._entity.basicInfo.Current.MoveYaw;
			}
		}

        public int CilentTime
		{
			get
			{
				return this._entity.GetClientTime();
			}
		}
		public PlayerData(PlayerEntity playerEntity)
		{
			if (playerEntity == null)
			{
				throw new ArgumentNullException("playerEntity");
			}
			this._entity = playerEntity;
		}
		public float MaxHp
		{
			get
			{
				return this._entity.basicInfo.Current.MaxHp;
			}
		}
		public float ViewYaw
		{
			get
			{
				return this._entity.basicInfo.Current.ViewYaw;
			}
		}
		public string Weapon
		{
			get
			{
				return this._entity.currentWeapon.WeaponInfo.StringName;
			}
		}
		public string CurrentWeaponName
		{
			get
			{
				return this._entity.basicInfo.Current.CurrentWeaponName;
			}
		}

        public ThirdPersonUnityObjectsComponent ThirdPersonUnityObjects
		{
			get
			{
				return this._entity.thirdPersonUnityObjects;
			}
		}
		public Vector2 Punch
		{
			get
			{
				return new Vector2(this._entity.GetPunchPitch(), this._entity.GetPunchYaw());
			}
		}
		public string PlayerName
		{
			get
			{
				string playerName = this._entity.basicInfo.Current.PlayerName;
				string text;
				if (playerName != null)
				{
					if ((text = playerName.TrimEnd(new char[1])) != null)
					{
						return text;
					}
				}
				text = "";
				return text;
			}
		}
		public float MovePitch
		{
			get
			{
				return this._entity.basicInfo.Current.MovePitch;
			}
		}
		public bool HasC4
		{
			get
			{
				return this._entity.basicInfo.Current.HasC4;
			}
		}
		public int WeaponDetailType
		{
			get
			{
				return this._entity.currentWeapon.WeaponInfo.WeaponType;
			}
		}
		public float ViewPitch
		{
			get
			{
				return this._entity.basicInfo.Current.ViewPitch;
			}
		}
		public bool IsDead
		{
			get
			{
				return this._entity.basicInfo.Current.IsDead;
			}
		}
		public int Distance
		{
			get
			{
				if (!(PlayerStateTracker.MainCamera != null))
				{
					return 0;
				}
				return (int)(Vector3.Distance(PlayerStateTracker.MainCamera.transform.position, VectorCoordConverter.SsjjToUnity(this.Position)) * 0.01f);
			}
		}

        public float Hp
		{
			get
			{
				return this._entity.basicInfo.Current.Hp;
			}
		}
		public Vector2 ViewPos
		{
			get
			{
				return new Vector2(this._entity.GetViewPitch(), this._entity.GetViewYaw());
			}
		}
		public int Id
		{
			get
			{
				return this._entity.GetId();
			}
		}
		public FovComponent Fov
		{
			get
			{
				return this._entity.fov;
			}
		}
		public readonly PlayerEntity _entity;
	}
}