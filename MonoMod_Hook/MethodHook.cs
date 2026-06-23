using System.Reflection;
using SkyDome.RuntimeDetour;

public class MethodHook : Hook
{
    public MethodHook(MethodBase from, MethodInfo to) : base(from, to) { }

    // Expose generic trampoline generation with correct delegate constraint
    public new TDelegate GenerateTrampoline<TDelegate>() where TDelegate : class, System.Delegate
    {
        return base.GenerateTrampoline<TDelegate>();
    }
}
