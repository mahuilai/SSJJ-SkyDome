using System;
using Assets.Sources.Utils.Ui;
using NetData;
using SkyDome.Cfg;
using SkyDome.Entity;
using SkyDome.Render;
using SSJJMath;
using UnityEngine;
namespace SkyDome.Feature.Visuals
{
	public class PickupHighlighter : MonoBehaviour
	{
		private void OnGUI()
		{
			if (SettingsStore.ShowPickupHighlighter && PlayerStateTracker.LocalEntity != null && !(PlayerStateTracker.MainCamera == null))
			{
				this.DrawDroppedItems();
				return;
			}
		}

        private void DrawDroppedItems()
		{
			Contexts sharedInstance = Contexts.sharedInstance;
			SceneObjectContext sceneObjectContext = ((sharedInstance != null) ? sharedInstance.sceneObject : null);
			if (sceneObjectContext == null)
			{
				return;
			}
			Vector3 position = PlayerStateTracker.MainCamera.transform.position;
			foreach (SceneObjectEntity sceneObjectEntity in sceneObjectContext.GetGroup(SceneObjectMatcher.SceneWeapon))
			{
				if (sceneObjectEntity != null && sceneObjectEntity.hasSceneWeapon)
				{
					SceneWeaponEntityData current = sceneObjectEntity.sceneWeapon.Current;
					if (current != null)
					{
						Vector3 vector = VectorCoordConverter.SsjjToUnity(new Vector3(current.X, current.Y, current.Z));
						Vector3 vector2 = PlayerStateTracker.MainCamera.WorldToScreenPoint(vector);
						if (vector2.z > 0f)
						{
							string text = current.WeaponName ?? "";
							string text2 = text;
							if (!string.IsNullOrEmpty(text))
							{
								try
								{
									text2 = LanguageUtils.GetWeaponCnName(text);
									goto IL_032B;
								}
								catch
								{
									goto IL_032B;
								}
								goto IL_0199;
							}
							goto IL_032B;
							IL_023D:
							string text4;
							string text3 = text4;
							FastRenderer.DrawCircleFilled(new Vector2(vector2.x, vector2.y), 3f, Color.white, 8);
							FastRenderer.DrawString(new Vector2(vector2.x, (float)Screen.height - vector2.y + 10f), text3, Color.white, true, 11);
							continue;
							IL_032B:
							float num = Vector3.Distance(position, vector) * 0.01f;
							if (SettingsStore.ShowPickupHighlighter)
							{
								text4 = string.Format("{0} [{1:F0}m]", text2, num);
								goto IL_023D;
							}
							IL_0199:
							text4 = text2;
							goto IL_023D;
						}
					}
				}
			}
		}
		public PickupHighlighter()
		{
		}
	}
}