try {
    # Use relative path to avoid encoding issues with Chinese characters in path
    $currentDir = Get-Location
    $depDir = Resolve-Path (Join-Path $currentDir "..\SSJJ_Mods\TriggerBot\依赖")
    Write-Host "Resolved dependency directory: $depDir"
    
    # Load all dependencies first in ReflectionOnly context
    $dlls = Get-ChildItem -Path $depDir -Filter *.dll
    foreach ($dll in $dlls) {
        if ($dll.Name -ne "Assembly-CSharp.dll") {
            try {
                [System.Reflection.Assembly]::ReflectionOnlyLoadFrom($dll.FullName) > $null
            } catch {}
        }
    }

    # Now load Assembly-CSharp
    $asmPath = Join-Path $depDir "Assembly-CSharp.dll"
    $asm = [System.Reflection.Assembly]::ReflectionOnlyLoadFrom($asmPath)
    foreach ($type in $asm.GetTypes()) {
        $name = $type.Name.ToLower()
        if ($name -like "*playerentity*" -or $name -like "*camera*" -or $name -like "*thirdperson*" -or $name -like "*ruleutility*") {
            foreach ($m in $type.GetMembers([System.Reflection.BindingFlags]"Public, NonPublic, Instance, Static")) {
                $mName = $m.Name.ToLower()
                if ($mName -like "*thirdperson*" -or $mName -like "*isthirdperson*" -or $mName -like "*switchthird*" -or $mName -like "*observethird*" -or $mName -like "*allowthird*") {
                    Write-Host "$($type.FullName) | $($m.MemberType) | $($m.Name)"
                }
            }
        }
    }
} catch {
    Write-Error $_
}
