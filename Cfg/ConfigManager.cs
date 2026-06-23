using System;
using System.IO;
using System.Reflection;
using UnityEngine;
namespace SkyDome.Cfg
{
	public static class SettingsStoreManager
	{
		private static object DeserializeValue(string valueStr, Type targetType)
		{
			if (valueStr == "null")
			{
				return null;
			}
			if (targetType == typeof(bool))
			{
				return bool.Parse(valueStr);
			}
			if (targetType == typeof(int))
			{
				return int.Parse(valueStr);
			}
			if (targetType == typeof(float))
			{
				return float.Parse(valueStr);
			}
			if (targetType == typeof(string))
			{
				return valueStr;
			}
			if (targetType == typeof(KeyCode))
			{
				return (KeyCode)int.Parse(valueStr);
			}
			return null;
		}
		public static void LoadSettingsStore(string SettingsStoreName)
		{
			try
			{
				string SettingsStorePath = (Path.Combine(SettingsStoreManager.SettingsStoreDirectory, (SettingsStoreName)));
				if (!File.Exists(SettingsStorePath))
				{
					Debug.Log("[Config] Config file " + SettingsStoreName + " not found");
				}
				else
				{
					using (StreamReader streamReader = new StreamReader(SettingsStorePath))
					{
						string text;
						while ((text = streamReader.ReadLine()) != null)
						{
							if (!string.IsNullOrWhiteSpace(text))
							{
								string text2 = text;
								char[] array = new char[1];
								array[0] = (char)(61);
								string[] array2 = text2.Split(array);
								if (array2.Length == 2)
								{
									string fieldName = array2[0].Trim();
									string text3 = array2[1].Trim();
									FieldInfo fieldInfo = Array.Find<FieldInfo>(SettingsStoreManager.Fields, (FieldInfo f) => f.Name == fieldName);
									if (!(fieldInfo == null))
									{
										try
										{
											object obj = SettingsStoreManager.DeserializeValue(text3, fieldInfo.FieldType);
											fieldInfo.SetValue(null, obj);
										}
										catch (Exception ex)
										{
											Debug.LogWarning("[配置] 字段 " + fieldName + " 解析失败: " + ex.Message);
										}
									}
								}
							}
						}
					}
					Debug.Log("[配置] 已加载配�? " + SettingsStoreName);
				}
			}
			catch (Exception ex2)
			{
				Debug.LogError("[配置] 加载失败: " + ex2.Message);
			}
		}
		public static void DeleteSettingsStore(string SettingsStoreName)
		{
			try
			{
				string SettingsStorePath = (Path.Combine(SettingsStoreManager.SettingsStoreDirectory, (SettingsStoreName)));
				if (File.Exists(SettingsStorePath))
				{
					File.Delete(SettingsStorePath);
					Debug.Log("[配置] 已删除配�? " + SettingsStoreName);
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("[配置] 删除失败: " + ex.Message);
			}
		}
		public static string[] GetAllSettingsStoreNames()
		{
			string[] array;
			try
			{
				if (!Directory.Exists(SettingsStoreManager.SettingsStoreDirectory))
				{
					Directory.CreateDirectory(SettingsStoreManager.SettingsStoreDirectory);
					array = new string[0];
				}
				else
				{
					string[] files = Directory.GetFiles(SettingsStoreManager.SettingsStoreDirectory);
					string[] array2 = new string[files.Length];
					for (int i = 0; i < files.Length; i++)
					{
						array2[i] = Path.GetFileName(files[i]);
					}
					array = array2;
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("[配置] 获取配置列表失败: " + ex.Message);
				array = new string[0];
			}
			return array;
		}
		public static void SaveSettingsStore(string SettingsStoreName)
		{
			try
			{
				if (!Directory.Exists(SettingsStoreManager.SettingsStoreDirectory))
				{
					Directory.CreateDirectory(SettingsStoreManager.SettingsStoreDirectory);
				}
				string SettingsStorePath = (Path.Combine(SettingsStoreManager.SettingsStoreDirectory, (SettingsStoreName)));
				using (StreamWriter streamWriter = new StreamWriter(SettingsStorePath, false))
				{
					foreach (FieldInfo fieldInfo in SettingsStoreManager.Fields)
					{
						object value = fieldInfo.GetValue(null);
						string text = fieldInfo.Name + "=" + SettingsStoreManager.SerializeValue(value);
						streamWriter.WriteLine(text);
					}
				}
				Debug.Log("[配置] 已保存到: " + SettingsStorePath);
			}
			catch (Exception ex)
			{
				Debug.LogError("[配置] 保存失败: " + ex.Message);
			}
		}
		private static string SerializeValue(object value)
		{
			if (value == null)
			{
				return "null";
			}
			Type type = value.GetType();
			if (type == typeof(bool) || type == typeof(int) || type == typeof(float) || type == typeof(string))
			{
				return value.ToString();
			}
			if (type == typeof(KeyCode))
			{
				return ((int)((KeyCode)value)).ToString();
			}
			return value.ToString();
		}
		private static readonly FieldInfo[] Fields = typeof(SettingsStore).GetFields(843 + BindingFlags.Public - 835);
		private static readonly string SettingsStoreDirectory = Path.Combine(Application.persistentDataPath, "SkySettingsHelper");
	}

	public static class SettingsHelper
	{
		public static string Current { get; private set; } = "default";

		public static string[] Names => SettingsStoreManager.GetAllSettingsStoreNames();

		public static void Init()
		{
			Load("default");
		}

		public static void Load(string name)
		{
			SettingsStoreManager.LoadSettingsStore(name);
			Current = name;
		}

		public static void Save(string name)
		{
			SettingsStoreManager.SaveSettingsStore(name);
			Current = name;
		}

		public static void Delete(string name)
		{
			SettingsStoreManager.DeleteSettingsStore(name);
			if (Current == name)
			{
				Current = "default";
			}
		}
	}
}