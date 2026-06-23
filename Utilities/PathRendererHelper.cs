using System;
using System.Reflection;

namespace SkyDome.Utilities
{
    public static class PathRendererHelper
    {
        private static MethodInfo _bulletPathRendererMethod;
        private static FieldInfo _entityIdField;
        private static PropertyInfo _entityIdProperty;
        private static bool _initialized;

        public static void Initialize()
        {
            if (_initialized) return;
            try
            {
                // Find Assets.Sources.Utils.Weapon.FireUtility type in AppDomain assemblies
                Type fireUtilityType = null;
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    fireUtilityType = assembly.GetType("Assets.Sources.Utils.Weapon.FireUtility");
                    if (fireUtilityType != null) break;
                }

                if (fireUtilityType != null)
                {
                    // Look for static method BulletPathRenderer
                    _bulletPathRendererMethod = fireUtilityType.GetMethod("BulletPathRenderer", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                }

                // Find SSJJPhysics.PathRendererResult type in AppDomain assemblies
                Type resultType = null;
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    resultType = assembly.GetType("SSJJPhysics.PathRendererResult");
                    if (resultType != null) break;
                }

                if (resultType != null)
                {
                    _entityIdField = resultType.GetField("EntityId", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (_entityIdField == null)
                    {
                        _entityIdProperty = resultType.GetProperty("EntityId", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    }
                }
                
                _initialized = true;
                UnityEngine.Debug.Log("[PathRendererHelper] Initialize completed. Method found: " + (_bulletPathRendererMethod != null) + ", Field found: " + (_entityIdField != null) + ", Property found: " + (_entityIdProperty != null));
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError("[PathRendererHelper] Error initializing: " + ex);
            }
        }

        public static int GetEntityId(object pyEngine, object shooterEntity, object playerContext, float distance, object directionVector3D, float[] arg6, float[] arg7, bool arg8)
        {
            try
            {
                Initialize();
                if (_bulletPathRendererMethod == null)
                {
                    return 0;
                }

                object result = _bulletPathRendererMethod.Invoke(null, new object[] {
                    pyEngine, shooterEntity, playerContext, distance, directionVector3D, arg6, arg7, arg8
                });

                if (result == null)
                {
                    return 0;
                }

                if (_entityIdField != null)
                {
                    return (int)_entityIdField.GetValue(result);
                }
                if (_entityIdProperty != null)
                {
                    return (int)_entityIdProperty.GetValue(result, null);
                }
            }
            catch (Exception)
            {
                // Suppress errors to prevent performance drop or log spam during frame updates
            }
            return 0;
        }
    }
}
