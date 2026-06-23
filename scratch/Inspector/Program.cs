using System;
using System.IO;
using System.Reflection;

class Program
{
    static void Main()
    {
        try
        {
            string currentDir = Directory.GetCurrentDirectory();
            string searchPath = currentDir;
            string targetPath = "";
            
            while (!string.IsNullOrEmpty(searchPath))
            {
                string checkPath = Path.Combine(searchPath, "SSJJ_Mods");
                if (Directory.Exists(checkPath))
                {
                    targetPath = Path.Combine(checkPath, "TriggerBot", "依赖", "Assembly-CSharp.dll");
                    if (File.Exists(targetPath))
                    {
                        break;
                    }
                }
                searchPath = Path.GetDirectoryName(searchPath);
            }

            if (string.IsNullOrEmpty(targetPath) || !File.Exists(targetPath))
            {
                targetPath = @"D:\项目\阳翳\SSJJ_Mods\TriggerBot\依赖\Assembly-CSharp.dll";
            }

            Console.WriteLine("Loading: " + targetPath);
            
            string depDir = Path.GetDirectoryName(targetPath);
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                string name = args.Name.Split(',')[0];
                string expectedDll = Path.Combine(depDir, name + ".dll");
                if (File.Exists(expectedDll))
                {
                    return Assembly.LoadFrom(expectedDll);
                }
                return null;
            };

            foreach (string dll in Directory.GetFiles(depDir, "*.dll"))
            {
                try
                {
                    Assembly.LoadFrom(dll);
                }
                catch {}
            }
            
            Assembly.LoadFrom(targetPath);
            
            PrintTypeAcrossAll("UiIEventCondition");
            PrintTypeAcrossAll("CommandsComponent");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.ToString());
        }
    }

    static void PrintTypeAcrossAll(string typeName)
    {
        Type targetType = null;
        Assembly foundInAsm = null;
        foreach (Assembly loadedAsm in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type[] types;
            try { types = loadedAsm.GetTypes(); }
            catch (ReflectionTypeLoadException ex) { types = ex.Types; }
            catch { continue; }

            if (types == null) continue;

            foreach (Type t in types)
            {
                if (t == null) continue;
                if (t.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase))
                {
                    targetType = t;
                    foundInAsm = loadedAsm;
                    break;
                }
            }
            if (targetType != null) break;
        }

        if (targetType != null)
        {
            Console.WriteLine($"\n--- TYPE: {targetType.FullName} found in {foundInAsm.GetName().Name} ---");
            foreach (MethodInfo m in targetType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
            {
                string pars = "";
                foreach (ParameterInfo p in m.GetParameters())
                {
                    pars += $"{p.ParameterType.FullName} {p.Name}, ";
                }
                if (pars.Length > 2) pars = pars.Substring(0, pars.Length - 2);
                Console.WriteLine($"  Method: {m.ReturnType.FullName} {m.Name}({pars})");
            }
        }
        else
        {
            Console.WriteLine($"{typeName} not found anywhere!");
        }
    }
}
