using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;
namespace SkyDome.Properties
{
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "18.0.0.0")]
	[DebuggerNonUserCode]
	[CompilerGenerated]
	public class Resources
	{
		public static byte[] MonoMod_Utils
		{
			get
			{
				return (byte[])Resources.ResourceManager.GetObject("MonoMod_Utils", Resources.resourceCulture);
			}
		}
		public static byte[] Mono_Cecil
		{
			get
			{
				return (byte[])Resources.ResourceManager.GetObject("Mono_Cecil", Resources.resourceCulture);
			}
		}
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public static CultureInfo Culture
		{
			get
			{
				return Resources.resourceCulture;
			}
			set
			{
				Resources.resourceCulture = value;
			}
		}
		public static byte[] MonoMod_RuntimeDetour1
		{
			get
			{
				return (byte[])Resources.ResourceManager.GetObject("MonoMod_RuntimeDetour1", Resources.resourceCulture);
			}
		}
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public static ResourceManager ResourceManager
		{
			get
			{
				if (Resources.resourceMan == null)
				{
					Resources.resourceMan = new ResourceManager("SkyDome.Properties.Resources", typeof(Resources).Assembly);
				}
				return Resources.resourceMan;
			}
		}
		internal Resources()
		{
		}
		public static byte[] ProggyTiny
		{
			get
			{
				return (byte[])Resources.ResourceManager.GetObject("ProggyTiny", Resources.resourceCulture);
			}
		}
		public static byte[] menu_font
		{
			get
			{
				return (byte[])Resources.ResourceManager.GetObject("menu_font", Resources.resourceCulture);
			}
		}
		private static ResourceManager resourceMan;
		private static CultureInfo resourceCulture;
	}
}