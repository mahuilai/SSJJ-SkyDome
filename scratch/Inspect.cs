using System;
using System.IO;
using System.Reflection;

class Program
{
    static void Main()
    {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string binPath = Path.GetFullPath(Path.Combine(baseDir, @"..\bin\Debug"));
        string resPath = Path.GetFullPath(Path.Combine(baseDir, @"..\Resources"));
        
        AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
        {
            try
            {
                string shortName = new AssemblyName(args.Name).Name;
                foreach (string file in Directory.GetFiles(binPath, "*.dll", SearchOption.AllDirectories))
                {
                    string filename = Path.GetFileNameWithoutExtension(file);
                    if (filename.Equals(shortName, StringComparison.OrdinalIgnoreCase) ||
                        (shortName.Contains("MonoMod.Utils") && filename.Equals("Utils", StringComparison.OrdinalIgnoreCase)) ||
                        (shortName.Contains("MonoMod.RuntimeDetour") && filename.Equals("RuntimeDetour", StringComparison.OrdinalIgnoreCase)) ||
                        (shortName.Contains("Mono.Cecil") && filename.Equals("Cecil", StringComparison.OrdinalIgnoreCase)))
                    {
                        return Assembly.LoadFrom(file);
                    }
                }
                foreach (string file in Directory.GetFiles(resPath, "*.dll", SearchOption.AllDirectories))
                {
                    string filename = Path.GetFileNameWithoutExtension(file);
                    if (filename.Equals(shortName, StringComparison.OrdinalIgnoreCase) ||
                        (shortName.Contains("MonoMod.Utils") && filename.Equals("Utils", StringComparison.OrdinalIgnoreCase)) ||
                        (shortName.Contains("MonoMod.RuntimeDetour") && filename.Equals("RuntimeDetour", StringComparison.OrdinalIgnoreCase)) ||
                        (shortName.Contains("Mono.Cecil") && filename.Equals("Cecil", StringComparison.OrdinalIgnoreCase)))
                    {
                        return Assembly.LoadFrom(file);
                    }
                }
            }
            catch {}
            return null;
        };

        string skyDomePath = Path.Combine(binPath, "SkyDome.dll");
        Assembly skyDomeAsm = Assembly.LoadFrom(skyDomePath);
        
        Assembly asmCSharp = Assembly.LoadFrom(Path.Combine(binPath, "Assembly-CSharp.dll"));
        Assembly asmEntitas = Assembly.LoadFrom(Path.Combine(binPath, "SSJJEntitas_Library.dll"));

        Type hookManagerType = skyDomeAsm.GetType("SkyDome.HookManager");
        BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        
        Type tpsCameraLogicType = asmCSharp.GetType("Assets.Sources.Info.Camera.CameraLogic.TpsCameraLogic");
        Type cameraFunctionType = asmCSharp.GetType("Assets.Sources.Info.Camera.CameraLogic.CameraFunction");
        Type uiIEventConditionType = asmCSharp.GetType("Assets.Sources.Modules.Ui.UiEventCondition.UiIEventCondition");
        Type commandsComponentType = asmEntitas.GetType("Assets.Sources.Components.UserComand.CommandsComponent");
        Type playerEntityType = asmEntitas.GetType("PlayerEntity");
        Type predSystemType = asmCSharp.GetType("Assets.Sources.Modules.Player.Orientation.PlayerOrientationPredicationSystem");
        Type playbackSystemType = asmCSharp.GetType("Assets.Sources.Modules.Player.Orientation.PlayerOrientationPlabackSystem");

        Action<string, Type, string, Type, string> tryHook = (name, targetType, targetMethod, hookClass, hookMethod) =>
        {
            Console.Write("Testing Hook: " + name + "... ");
            try
            {
                if (targetType == null)
                {
                    Console.WriteLine("FAIL (Target Type is NULL)");
                    return;
                }
                MethodInfo mTarget = targetType.GetMethod(targetMethod, flags);
                if (mTarget == null)
                {
                    Console.WriteLine("FAIL (Target Method " + targetMethod + " not found)");
                    return;
                }
                MethodInfo mHook = hookClass.GetMethod(hookMethod, flags);
                if (mHook == null)
                {
                    Console.WriteLine("FAIL (Hook Method " + hookMethod + " not found)");
                    return;
                }

                Type hookType = skyDomeAsm.GetType("MonoMod.RuntimeDetour.Hook");
                if (hookType == null)
                {
                    Assembly monoModAsm = Assembly.LoadFrom(Path.Combine(resPath, @"MonoMod\RuntimeDetour.dll"));
                    hookType = monoModAsm.GetType("MonoMod.RuntimeDetour.Hook");
                }
                
                object hookInstance = Activator.CreateInstance(hookType, new object[] { mTarget, mHook });
                Console.WriteLine("SUCCESS!");
            }
            catch (Exception ex)
            {
                Exception current = ex;
                if (ex is TargetInvocationException && ex.InnerException != null)
                {
                    current = ex.InnerException;
                }
                Console.WriteLine("FAILED: " + current.GetType().Name + ": " + current.Message);
            }
        };

        tryHook("IsActive", tpsCameraLogicType, "IsActive", hookManagerType, "hk_IsActive");
        tryHook("Update", tpsCameraLogicType, "Update", hookManagerType, "hk_Update");
        tryHook("GetCurrentCmdYaw", cameraFunctionType, "GetCurrentCmdYaw", hookManagerType, "hk_GetCurrentCmdYaw");
        tryHook("GetCurrentCmdPitch", cameraFunctionType, "GetCurrentCmdPitch", hookManagerType, "hk_GetCurrentCmdPitch");
        tryHook("Get_ControlEntityData_Yaw", uiIEventConditionType, "Get_ControlEntityData_Yaw", hookManagerType, "hk_Get_ControlEntityData_Yaw");
        tryHook("Get_cameraOwnerData_Yaw", uiIEventConditionType, "Get_cameraOwnerData_Yaw", hookManagerType, "hk_Get_cameraOwnerData_Yaw");
        tryHook("LastCameraYaw", commandsComponentType, "LastCameraYaw", hookManagerType, "hk_LastCameraYaw");
        tryHook("LastCameraPitch", commandsComponentType, "LastCameraPitch", hookManagerType, "hk_LastCameraPitch");
        tryHook("get_fov", playerEntityType, "get_fov", hookManagerType, "hk_get_fov");
        tryHook("OnPredicate", predSystemType, "OnPredicate", hookManagerType, "hk_OnPredicate");
        tryHook("OnPlayback", playbackSystemType, "OnPlayback", hookManagerType, "hk_OnPlayback");
        tryHook("PredictCmdOnCamera", predSystemType, "PredictCmdOnCamera", hookManagerType, "hk_PredictCmdOnCamera");
    }
}
