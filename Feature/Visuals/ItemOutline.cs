using System;
using System.Collections.Generic;
using cakeslice;
using NetData;
using SkyDome.Cfg;
using SkyDome.Entity;
using SSJJAsset.Asset;
using UnityEngine;
namespace SkyDome.Feature.Visuals
{
	public class PickupOutliner : MonoBehaviour
	{
		private void OnDisable()
		{
			this.CleanupAllOutlines();
		}
		private void EnsureOutlineEffect()
		{
			if (this._mainCamera == null)
			{
				this._mainCamera = PlayerStateTracker.MainCamera;
			}
			if (this._mainCamera == null)
			{
				return;
			}
			if (this._outlineEffect == null)
			{
				this._outlineEffect = this._mainCamera.GetComponent<OutlineEffect>();
				if (this._outlineEffect == null)
				{
					this._outlineEffect = this._mainCamera.gameObject.AddComponent<OutlineEffect>();
				}
			}
			if (this._outlineEffect != null && SettingsStore.ShowPickupOutliner)
			{
				this._outlineEffect.lineColor1 = new Color(0.5f, 1f, 0.3f, 1f);
			}
		}

        private void OnDestroy()
		{
			this.CleanupAllOutlines();
		}
		private void UpdatePickupOutliners()
		{
			Contexts sharedInstance = Contexts.sharedInstance;
			SceneObjectContext sceneObjectContext = ((sharedInstance != null) ? sharedInstance.sceneObject : null);
			if (sceneObjectContext == null)
			{
				return;
			}
			HashSet<int> currentItems = new HashSet<int>();
			foreach (SceneObjectEntity sceneObjectEntity in sceneObjectContext.GetGroup(SceneObjectMatcher.SceneWeapon))
			{
				if (sceneObjectEntity != null && sceneObjectEntity.hasSceneWeapon)
				{
					SceneWeaponEntityData current = sceneObjectEntity.sceneWeapon.Current;
					if (current != null)
					{
						int id2 = current.Id;
						currentItems.Add(id2);
						if (sceneObjectEntity.hasUnityObjects)
						{
							LoadResults loadResults = sceneObjectEntity.unityObjects.LoadResults;
							if (loadResults != null)
							{
								GameObject gameObject = null;
								if (loadResults.ContainsKey("WeaponModel"))
								{
									gameObject = loadResults["WeaponModel"].GameObject;
								}
								else if (loadResults.ContainsKey(0))
								{
									gameObject = loadResults[0].GameObject;
								}
								if (!(gameObject == null))
								{
									if (SettingsStore.ShowPickupOutliner)
									{
										this.ApplyOutlineToItem(gameObject, id2);
									}
									else
									{
										this.RemoveOutlineFromItem(gameObject, id2);
									}
								}
							}
						}
					}
				}
			}
			this._outlinedItems.RemoveWhere((int id) => (currentItems.Contains(id) ? 1 : 0) == 0);
		}

        private void Update()
		{
			this.EnsureOutlineEffect();
			this.UpdatePickupOutliners();
		}
		private void CleanupAllOutlines()
		{
			Contexts sharedInstance = Contexts.sharedInstance;
			SceneObjectContext sceneObjectContext = ((sharedInstance != null) ? sharedInstance.sceneObject : null);
			if (sceneObjectContext == null)
			{
				return;
			}
			foreach (SceneObjectEntity sceneObjectEntity in sceneObjectContext.GetGroup(SceneObjectMatcher.SceneWeapon))
			{
				if (sceneObjectEntity != null && sceneObjectEntity.hasUnityObjects)
				{
					LoadResults loadResults = sceneObjectEntity.unityObjects.LoadResults;
					if (loadResults != null)
					{
						GameObject gameObject = null;
						if (loadResults.ContainsKey("WeaponModel"))
						{
							gameObject = loadResults["WeaponModel"].GameObject;
						}
						else if (loadResults.ContainsKey(0))
						{
							gameObject = loadResults[0].GameObject;
						}
						if (!(gameObject == null))
						{
							Outline[] componentsInChildren = gameObject.GetComponentsInChildren<Outline>(true);
							for (int i = 0; i < componentsInChildren.Length; i += 1)
							{
								Outline outline = componentsInChildren[i];
								if (outline != null)
								{
									global::UnityEngine.Object.Destroy(outline);
								}
							}
						}
					}
				}
			}
			this._outlinedItems.Clear();
		}

        private void ApplyOutlineToItem(GameObject weaponGO, int itemId)
		{
			if (this._outlinedItems.Contains(itemId))
			{
				return;
			}
			foreach (SkinnedMeshRenderer skinnedMeshRenderer in weaponGO.GetComponentsInChildren<SkinnedMeshRenderer>(true))
			{
				if (!(skinnedMeshRenderer == null) && skinnedMeshRenderer.GetComponent<Outline>() == null)
				{
					skinnedMeshRenderer.gameObject.AddComponent<Outline>().color = 1;
				}
			}
			foreach (MeshRenderer meshRenderer in weaponGO.GetComponentsInChildren<MeshRenderer>(true))
			{
				if (!(meshRenderer == null) && meshRenderer.GetComponent<Outline>() == null)
				{
					meshRenderer.gameObject.AddComponent<Outline>().color = 1;
				}
			}
			this._outlinedItems.Add(itemId);
        }

        // 你他妈毕业于肇庆技师学院还敢自称“生来强大”？强大个鸡巴，你就配被我们天天羞辱
        private void RemoveOutlineFromItem(GameObject weaponGO, int itemId)
		{
			if (!this._outlinedItems.Contains(itemId))
			{
				return;
			}
			foreach (Outline outline in weaponGO.GetComponentsInChildren<Outline>(true))
			{
				if (outline != null)
				{
					global::UnityEngine.Object.Destroy(outline);
				}
			}
			this._outlinedItems.Remove(itemId);
		}
		private Camera _mainCamera;
		private OutlineEffect _outlineEffect;
		private HashSet<int> _outlinedItems = new HashSet<int>();
	}
}