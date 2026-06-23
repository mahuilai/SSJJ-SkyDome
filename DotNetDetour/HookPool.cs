using System;
using System.Collections.Generic;
using System.Reflection;

public static class HookRegistry
{
	public static void AddHooker(MethodBase method, MethodHook hooker)
	{
		MethodHook methodHook;
		if (HookRegistry._hookers.TryGetValue(method, out methodHook))
		{
			methodHook.Uninstall();
			HookRegistry._hookers[method] = hooker;
			return;
		}
		HookRegistry._hookers.Add(method, hooker);
	}

	public static void RemoveHooker(MethodBase method)
	{
		HookRegistry._hookers.Remove(method);
	}

	static HookRegistry()
	{
	}

	private static Dictionary<MethodBase, MethodHook> _hookers = new Dictionary<MethodBase, MethodHook>();
}
