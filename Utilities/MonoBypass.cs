using System;
using System.Collections.Generic;
using System.Reflection;
using SkyDome.RuntimeDetour;

namespace SkyDome.Utilities
{
public static class MonoBypass
{
private static bool _initialized = false;

private static MethodHook _hookGetExportedTypes;
private delegate Type[] GetExportedTypesDelegate(Assembly self);
private static GetExportedTypesDelegate _originalGetExportedTypes;

private static MethodHook _hookGetLoadedModules;
private delegate Module[] GetLoadedModulesDelegate(Assembly self);
private static GetLoadedModulesDelegate _originalGetLoadedModules;

private static MethodHook _hookGetModules;
private delegate Module[] GetModulesDelegate(Assembly self);
private static GetModulesDelegate _originalGetModules;

private static MethodHook _hookGetReferencedAssemblies;
private delegate AssemblyName[] GetReferencedAssembliesDelegate(Assembly self);
private static GetReferencedAssembliesDelegate _originalGetReferencedAssemblies;

private static MethodHook _hookCreateInstance;
private delegate object CreateInstanceDelegate(Assembly self, string typeName);
private static CreateInstanceDelegate _originalCreateInstance;

private static readonly HashSet<string> _hiddenNameFragments = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
{
    "SkyDome", "SkyCore", "MonoMod", "Cecil", "DotNetDetour",
};

private static bool IsSensitiveType(Type t)
{
    if (t == null) return false;
    try
    {
        string fullName = t.FullName;
        if (!string.IsNullOrEmpty(fullName))
        {
            foreach (var frag in _hiddenNameFragments)
                if (fullName.IndexOf(frag, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
        }
        string asmName = t.Assembly?.GetName()?.Name;
        if (!string.IsNullOrEmpty(asmName))
        {
            foreach (var frag in _hiddenNameFragments)
                if (asmName.IndexOf(frag, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
        }
    }
    catch { return true; }
    return false;
}

private static bool IsSensitiveAssembly(Assembly asm)
{
    if (asm == null) return false;
    try
    {
        string name = asm.FullName;
        if (!string.IsNullOrEmpty(name))
        {
            foreach (var frag in _hiddenNameFragments)
                if (name.IndexOf(frag, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
        }
        string simpleName = asm.GetName()?.Name;
        if (!string.IsNullOrEmpty(simpleName))
        {
            foreach (var frag in _hiddenNameFragments)
                if (simpleName.IndexOf(frag, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
        }
    }
    catch { return true; }
    return false;
}

private static Type[] FilterTypes(Type[] types)
{
    if (types == null) return null;
    var list = new List<Type>(types.Length);
    foreach (Type t in types)
        if (t != null && !IsSensitiveType(t))
            list.Add(t);
    return list.ToArray();
}

private static Module[] FilterModules(Module[] modules)
{
    if (modules == null) return null;
    var list = new List<Module>(modules.Length);
    foreach (Module m in modules)
    {
        if (m == null) continue;
        try { if (!IsSensitiveAssembly(m.Assembly)) list.Add(m); }
        catch { }
    }
    return list.ToArray();
}

private static AssemblyName[] FilterAssemblyNames(AssemblyName[] names)
{
    if (names == null) return null;
    var list = new List<AssemblyName>(names.Length);
    foreach (AssemblyName n in names)
    {
        if (n == null) continue;
        bool hide = false;
        foreach (var frag in _hiddenNameFragments)
        {
            if (n.Name.IndexOf(frag, StringComparison.OrdinalIgnoreCase) >= 0)
            { hide = true; break; }
        }
        if (!hide) list.Add(n);
    }
    return list.ToArray();
}

private static Type[] hk_GetExportedTypes(Assembly self)
{
    Type[] types = null;
    if (_originalGetExportedTypes != null)
        try { types = _originalGetExportedTypes(self); } catch { }
    if (types == null && _hookGetExportedTypes != null)
    {
        lock (_hookGetExportedTypes)
        {
            _hookGetExportedTypes.Undo();
            try { types = self.GetExportedTypes(); }
            finally { _hookGetExportedTypes.Apply(); }
        }
    }
    return FilterTypes(types);
}

private static Module[] hk_GetLoadedModules(Assembly self)
{
    Module[] modules = null;
    if (_originalGetLoadedModules != null)
        try { modules = _originalGetLoadedModules(self); } catch { }
    if (modules == null && _hookGetLoadedModules != null)
    {
        lock (_hookGetLoadedModules)
        {
            _hookGetLoadedModules.Undo();
            try { modules = self.GetLoadedModules(); }
            finally { _hookGetLoadedModules.Apply(); }
        }
    }
    return FilterModules(modules);
}

private static Module[] hk_GetModules(Assembly self)
{
    Module[] modules = null;
    if (_originalGetModules != null)
        try { modules = _originalGetModules(self); } catch { }
    if (modules == null && _hookGetModules != null)
    {
        lock (_hookGetModules)
        {
            _hookGetModules.Undo();
            try { modules = self.GetModules(); }
            finally { _hookGetModules.Apply(); }
        }
    }
    return FilterModules(modules);
}

private static AssemblyName[] hk_GetReferencedAssemblies(Assembly self)
{
    AssemblyName[] names = null;
    if (_originalGetReferencedAssemblies != null)
        try { names = _originalGetReferencedAssemblies(self); } catch { }
    if (names == null && _hookGetReferencedAssemblies != null)
    {
        lock (_hookGetReferencedAssemblies)
        {
            _hookGetReferencedAssemblies.Undo();
            try { names = self.GetReferencedAssemblies(); }
            finally { _hookGetReferencedAssemblies.Apply(); }
        }
    }
    return FilterAssemblyNames(names);
}

private static object hk_CreateInstance(Assembly self, string typeName)
{
    if (!string.IsNullOrEmpty(typeName))
    {
        foreach (var frag in _hiddenNameFragments)
        {
            if (typeName.IndexOf(frag, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                SceneInitializer.SafeLog("[MonoBypass] 阻止了敏感的 CreateInstance: " + typeName);
                return null;
            }
        }
    }
    if (_originalCreateInstance != null)
        return _originalCreateInstance(self, typeName);
    return self.CreateInstance(typeName);
}

public static void Install()
{
    if (_initialized) return;
    _initialized = true;

    BindingFlags flags = BindingFlags.Instance | BindingFlags.Static |
        BindingFlags.Public | BindingFlags.NonPublic;

    SceneInitializer.CrashLog("[MonoBypass] 开始安装额外枚举API钩子...");

    TryHook("GetExportedTypes", () =>
    {
        var m = typeof(Assembly).GetMethod("GetExportedTypes", flags, null, new Type[0], null);
        if (m == null) m = typeof(Assembly).GetMethod("GetExportedTypes", Type.EmptyTypes);
        if (m != null)
        {
            var hk = typeof(MonoBypass).GetMethod("hk_GetExportedTypes", BindingFlags.Static | BindingFlags.NonPublic);
            _hookGetExportedTypes = new MethodHook(m, hk);
            try { _originalGetExportedTypes = _hookGetExportedTypes.GenerateTrampoline<GetExportedTypesDelegate>(); } catch { }
        }
    });

    TryHook("GetLoadedModules", () =>
    {
        var m = typeof(Assembly).GetMethod("GetLoadedModules", flags, null, new Type[0], null);
        if (m == null) m = typeof(Assembly).GetMethod("GetLoadedModules", Type.EmptyTypes);
        if (m != null)
        {
            var hk = typeof(MonoBypass).GetMethod("hk_GetLoadedModules", BindingFlags.Static | BindingFlags.NonPublic);
            _hookGetLoadedModules = new MethodHook(m, hk);
            try { _originalGetLoadedModules = _hookGetLoadedModules.GenerateTrampoline<GetLoadedModulesDelegate>(); } catch { }
        }
    });

    TryHook("GetModules", () =>
    {
        var m = typeof(Assembly).GetMethod("GetModules", flags, null, new Type[0], null);
        if (m == null) m = typeof(Assembly).GetMethod("GetModules", Type.EmptyTypes);
        if (m != null)
        {
            var hk = typeof(MonoBypass).GetMethod("hk_GetModules", BindingFlags.Static | BindingFlags.NonPublic);
            _hookGetModules = new MethodHook(m, hk);
            try { _originalGetModules = _hookGetModules.GenerateTrampoline<GetModulesDelegate>(); } catch { }
        }
    });

    TryHook("GetReferencedAssemblies", () =>
    {
        var m = typeof(Assembly).GetMethod("GetReferencedAssemblies", Type.EmptyTypes);
        if (m != null)
        {
            var hk = typeof(MonoBypass).GetMethod("hk_GetReferencedAssemblies", BindingFlags.Static | BindingFlags.NonPublic);
            _hookGetReferencedAssemblies = new MethodHook(m, hk);
            try { _originalGetReferencedAssemblies = _hookGetReferencedAssemblies.GenerateTrampoline<GetReferencedAssembliesDelegate>(); } catch { }
        }
    });

    TryHook("CreateInstance", () =>
    {
        var m = typeof(Assembly).GetMethod("CreateInstance", new Type[] { typeof(string) });
        if (m != null)
        {
            var hk = typeof(MonoBypass).GetMethod("hk_CreateInstance", BindingFlags.Static | BindingFlags.NonPublic);
            _hookCreateInstance = new MethodHook(m, hk);
            try { _originalCreateInstance = _hookCreateInstance.GenerateTrampoline<CreateInstanceDelegate>(); } catch { }
        }
    });

    SceneInitializer.CrashLog("[MonoBypass] 额外枚举API钩子安装完成");
}

private static void TryHook(string name, Action installAction)
{
    try
    {
        installAction();
        SceneInitializer.SafeLog("[MonoBypass] 钩子 " + name + " 安装完成");
    }
    catch (Exception ex)
    {
        SceneInitializer.SafeLogError("[MonoBypass] 钩子 " + name + " 安装失败: " + ex.Message);
    }
}
}
}
