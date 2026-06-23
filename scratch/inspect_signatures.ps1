try {
    $depPath = "bin\Debug"
    $dlls = Get-ChildItem -Path $depPath -Filter *.dll
    
    # Load all assemblies first
    $assemblies = @{}
    foreach ($dll in $dlls) {
        try {
            $assemblies[$dll.BaseName] = [System.Reflection.Assembly]::LoadFrom($dll.FullName)
        } catch {}
    }

    $flags = [System.Reflection.BindingFlags]"Public, NonPublic, Instance, Static"

    function Test-Reflection-GetMethod($asmName, $typeName, $methodName) {
        $asm = $assemblies[$asmName]
        if (-not $asm) {
            Write-Host "[FAIL] Assembly $asmName not loaded"
            return
        }
        $type = $asm.GetType($typeName)
        if (-not $type) {
            Write-Host "[FAIL] Type $typeName not found in $asmName"
            return
        }
        $method = $type.GetMethod($methodName, $flags)
        if ($method) {
            Write-Host "[OK] Found $typeName.$methodName"
        } else {
            Write-Host "[FAIL] Method $methodName not found in $typeName"
        }
    }

    Test-Reflection-GetMethod "Assembly-CSharp" "Assets.Sources.Info.Camera.CameraLogic.TpsCameraLogic" "IsActive"
    Test-Reflection-GetMethod "Assembly-CSharp" "Assets.Sources.Info.Camera.CameraLogic.TpsCameraLogic" "Update"
    Test-Reflection-GetMethod "Assembly-CSharp" "Assets.Sources.Info.Camera.CameraLogic.CameraFunction" "GetCurrentCmdYaw"
    Test-Reflection-GetMethod "Assembly-CSharp" "Assets.Sources.Info.Camera.CameraLogic.CameraFunction" "GetCurrentCmdPitch"
    Test-Reflection-GetMethod "Assembly-CSharp" "Assets.Sources.Modules.Ui.UiEventCondition.UiIEventCondition" "Get_ControlEntityData_Yaw"
    Test-Reflection-GetMethod "Assembly-CSharp" "Assets.Sources.Modules.Ui.UiEventCondition.UiIEventCondition" "Get_cameraOwnerData_Yaw"
    Test-Reflection-GetMethod "SSJJEntitas_Library" "Assets.Sources.Components.UserComand.CommandsComponent" "LastCameraYaw"
    Test-Reflection-GetMethod "SSJJEntitas_Library" "Assets.Sources.Components.UserComand.CommandsComponent" "LastCameraPitch"
    Test-Reflection-GetMethod "SSJJEntitas_Library" "PlayerEntity" "get_fov"
    Test-Reflection-GetMethod "Assembly-CSharp" "Assets.Sources.Modules.Player.Orientation.PlayerOrientationPredicationSystem" "OnPredicate"
    Test-Reflection-GetMethod "Assembly-CSharp" "Assets.Sources.Modules.Player.Orientation.PlayerOrientationPlabackSystem" "OnPlayback"
    Test-Reflection-GetMethod "Assembly-CSharp" "Assets.Sources.Modules.Player.Orientation.PlayerOrientationPredicationSystem" "PredictCmdOnCamera"

} catch {
    Write-Error $_
}
