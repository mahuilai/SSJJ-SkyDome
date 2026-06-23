using System;
using System.Collections.Generic;
using Assets.Sources.Constant;
using cakeslice;
using SkyDome.Cfg;
using SkyDome.Entity;
using SSJJBase.Obscure;
using UnityEngine;
namespace SkyDome.Feature.Visuals
{
	public class MaterialOverlay : MonoBehaviour
	{
		private void UpdatePlayerOutlines()
		{
			if (PlayerStateTracker.LocalEntity != null && PlayerStateTracker.EntityList != null)
			{
				int team = PlayerStateTracker.LocalEntity.Team;
				foreach (SkyDome.Entity.PlayerData PlayerData in PlayerStateTracker.EntityList)
				{
					if (PlayerData != null)
					{
						this.UpdatePlayerOutline(PlayerData, team);
					}
				}
				return;
			}
		}

        private bool ValidateComponents()
		{
			if (this._outlineEffect != null && this._mainCamera != null)
			{
				return true;
			}
			this.InitializeOutlineEffect();
			return this._outlineEffect != null && this._mainCamera != null;
		}
		private void InitializeOutlineEffect()
		{
			this._mainCamera = PlayerStateTracker.MainCamera;
			if (this._mainCamera == null)
			{
				return;
			}
			this._outlineEffect = this._mainCamera.GetComponent<OutlineEffect>();
			if (this._outlineEffect == null)
			{
				this._outlineEffect = this._mainCamera.gameObject.AddComponent<OutlineEffect>();
			}
		}

        private void Update()
		{
			if (!this.ValidateComponents())
			{
				return;
			}
			this.SettingsStoreureOutlineEffect();
			this.UpdatePlayerOutlines();
		}
		private void ApplyOutlines(SkinnedMeshRenderer[] renderers)
		{
			foreach (SkinnedMeshRenderer skinnedMeshRenderer in renderers)
			{
				Outline outline;
				if (!(skinnedMeshRenderer == null) && !skinnedMeshRenderer.TryGetComponent<Outline>(out outline))
				{
					skinnedMeshRenderer.gameObject.AddComponent<Outline>();
				}
			}
		}
		private void Start()
		{
			this.InitializeOutlineEffect();
		}

        private SkinnedMeshRenderer[] GetPlayerRenderers(SkyDome.Entity.PlayerData player)
		{
			if (player.ThirdPersonUnityObjects == null)
			{
				return null;
			}
			if (!RuleUtilty.EnableAvater())
			{
				List<SkinnedMeshRenderer> careerSkins = player.ThirdPersonUnityObjects.CareerSkins;
				if (careerSkins == null)
				{
					return null;
				}
				return careerSkins.ToArray();
			}
			else
			{
				ThirdTran thirdTran = player.ThirdPersonUnityObjects.ThirdTran;
				if (thirdTran == null)
				{
					return null;
				}
				Transform bodyTransform = thirdTran.BodyTransform;
				if (bodyTransform == null)
				{
					return null;
				}
				return bodyTransform.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
			}
		}
		private void UpdatePlayerOutline(SkyDome.Entity.PlayerData player, int localTeam)
		{
			if (player.Team == localTeam)
			{
				return;
			}
			SkinnedMeshRenderer[] playerRenderers = this.GetPlayerRenderers(player);
			if (playerRenderers == null || playerRenderers.Length == 0)
			{
				return;
			}
			if (!player.IsDead && SettingsStore.MaterialOverlay)
			{
				this.ApplyOutlines(playerRenderers);
				return;
			}
			this.RemoveOutlines(playerRenderers);
		}
		private void SettingsStoreureOutlineEffect()
		{
			this._outlineEffect.addLinesBetweenColors = false;
			this._outlineEffect.lineColor0 = new Color(0.9f, 0.5f, 0.75f, 1f);
			this._outlineEffect.lineColor1 = Color.clear;
			this._outlineEffect.lineColor2 = Color.clear;
			this._outlineEffect.additiveRendering = false;
			this._outlineEffect.cornerOutlines = true;
			this._outlineEffect.fillAmount = 0f;
			this._outlineEffect.lineThickness = 0.4f;
			this._outlineEffect.lineIntensity = 2f;
			this._outlineEffect.alphaCutoff = 0.9f;
			this._outlineEffect.backfaceCulling = true;
		}

        private void RemoveOutlines(SkinnedMeshRenderer[] renderers)
		{
			foreach (SkinnedMeshRenderer skinnedMeshRenderer in renderers)
			{
				if (!(skinnedMeshRenderer == null))
				{
					Outline component = skinnedMeshRenderer.GetComponent<Outline>();
					if (component != null)
					{
						global::UnityEngine.Object.Destroy(component);
					}
				}
			}
		}
		private OutlineEffect _outlineEffect;
		private Camera _mainCamera;
	}
}