try {
    $currentDir = Get-Location
    $binPath = Join-Path $currentDir "bin\Debug"
    $resPath = Join-Path $currentDir "Resources"
    
    $dlls = (Get-ChildItem -Path $binPath -Filter *.dll -Recurse) + 
            (Get-ChildItem -Path $resPath -Filter *.dll -Recurse)
    
    # Register AssemblyResolve
    [System.AppDomain]::CurrentDomain.add_AssemblyResolve({
        param($sender, $args)
        $name = (New-Object System.Reflection.AssemblyName($args.Name)).Name
        $match = $dlls | Where-Object { $_.BaseName -eq $name }
        if ($match) {
            Write-Host "Resolving Assembly: $name from $($match.FullName)"
            return [System.Reflection.Assembly]::LoadFrom($match.FullName)
        }
        return $null
    })

    # Load SkyDome
    $skyDomePath = Join-Path $binPath "SkyDome.dll"
    Write-Host "Loading SkyDome from $skyDomePath"
    $skyDomeAsm = [System.Reflection.Assembly]::LoadFrom($skyDomePath)

    # Call HookManager.StartHook
    Write-Host "Calling HookManager.StartHook()..."
    [SkyDome.HookManager]::StartHook()
    Write-Host "StartHook completed without unhandled crash exception!"

} catch {
    Write-Error "Error during test: $_"
    if ($_.Exception) {
        Write-Error $_.Exception.ToString()
        if ($_.Exception.InnerException) {
            Write-Error "Inner Exception: $($_.Exception.InnerException.ToString())"
        }
    }
}
