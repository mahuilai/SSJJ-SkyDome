using System;
using System.Reflection;
using UnityEngine;
namespace SkyDome.Extension
{
	public static class ReflectionExtensions
	{

        public static T InvokeMethod<T>(this object target, string methodName, params object[] parameters)
		{
			if (target == null)
			{
				throw new ArgumentNullException("target");
			}
			MethodInfo method = target.GetType().GetMethod(methodName, (BindingFlags)(52));
			if (method == null)
			{
				string[] array = new string[5];
				array[0] = "Method '";
				array[1] = methodName;
				array[2] = "' not found in type ";
				array[3] = target.GetType().Name;
				array[4] = ".";
				throw new ArgumentException(string.Concat(array));
			}
			return (T)((object)method.Invoke(target, parameters));
		}
		public static bool TryInvokeMethod(this object target, string methodName, params object[] parameters)
		{
			bool flag;
			try
			{
				target.InvokeMethod(methodName, parameters);
				flag = true;
			}
			catch (Exception ex)
			{
				Debug.LogError("调用方法 '" + methodName + "' 失败: " + ex.Message);
				flag = false;
			}
			return flag;
		}
		public static bool TryInvokeMethod<T>(this object target, string methodName, out T result, params object[] parameters)
		{
			result = default(T);
			bool flag;
			try
			{
				result = target.InvokeMethod<T>(methodName, parameters);
				flag = true;
			}
			catch (Exception ex)
			{
				Debug.LogError("调用方法 '" + methodName + "' 失败: " + ex.Message);
				flag = false;
			}
			return flag;
		}
		public static void InvokeMethod(this object target, string methodName, params object[] parameters)
		{
			if (target == null)
			{
				throw new ArgumentNullException("target");
			}
			MethodInfo method = target.GetType().GetMethod(methodName, 871 + BindingFlags.Public - 835);
			if (method == null)
			{
				string[] array = new string[5];
				array[0] = "Method '";
				array[1] = methodName;
				array[2] = "' not found in type ";
				array[3] = target.GetType().Name;
				array[4] = ".";
				throw new ArgumentException(string.Concat(array));
			}
			method.Invoke(target, parameters);
		}

        public static T GetFieldValue<T>(this object source, string fieldName)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			if (string.IsNullOrWhiteSpace(fieldName))
			{
				throw new ArgumentException("Field name cannot be null or whitespace.", "fieldName");
			}
			Type type = source.GetType();
			FieldInfo field = type.GetField(fieldName, (BindingFlags)(52));
			if (field == null)
			{
				string[] array = new string[5];
				array[0] = "Field '";
				array[1] = fieldName;
				array[2] = "' not found in type ";
				array[3] = type.Name;
				array[4] = ".";
				throw new ArgumentException(string.Concat(array));
			}
			return (T)((object)field.GetValue(source));
		}
		private const BindingFlags DefaultBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
	}
}