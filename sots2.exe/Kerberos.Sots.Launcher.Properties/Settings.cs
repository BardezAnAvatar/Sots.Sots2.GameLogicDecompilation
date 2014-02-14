using System;
using System.CodeDom.Compiler;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;
namespace Kerberos.Sots.Launcher.Properties
{
	[GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "10.0.0.0"), CompilerGenerated]
	internal sealed class Settings : ApplicationSettingsBase
	{
		private static Settings defaultInstance = (Settings)SettingsBase.Synchronized(new Settings());
		public static Settings Default
		{
			get
			{
				return Settings.defaultInstance;
			}
		}
		[DefaultSettingValue("1024"), UserScopedSetting, DebuggerNonUserCode]
		public int ScreenWidth
		{
			get
			{
				return (int)this["ScreenWidth"];
			}
			set
			{
				this["ScreenWidth"] = value;
			}
		}
		[DefaultSettingValue("720"), UserScopedSetting, DebuggerNonUserCode]
		public int ScreenHeight
		{
			get
			{
				return (int)this["ScreenHeight"];
			}
			set
			{
				this["ScreenHeight"] = value;
			}
		}
		[DefaultSettingValue("True"), UserScopedSetting, DebuggerNonUserCode]
		public bool Bloom
		{
			get
			{
				return (bool)this["Bloom"];
			}
			set
			{
				this["Bloom"] = value;
			}
		}
		[DefaultSettingValue("True"), UserScopedSetting, DebuggerNonUserCode]
		public bool FocalBlur
		{
			get
			{
				return (bool)this["FocalBlur"];
			}
			set
			{
				this["FocalBlur"] = value;
			}
		}
		[DefaultSettingValue("True"), UserScopedSetting, DebuggerNonUserCode]
		public bool Refraction
		{
			get
			{
				return (bool)this["Refraction"];
			}
			set
			{
				this["Refraction"] = value;
			}
		}
		[DefaultSettingValue("True"), UserScopedSetting, DebuggerNonUserCode]
		public bool AmbientOcclusion
		{
			get
			{
				return (bool)this["AmbientOcclusion"];
			}
			set
			{
				this["AmbientOcclusion"] = value;
			}
		}
		[DefaultSettingValue("5"), UserScopedSetting, DebuggerNonUserCode]
		public int TextureQuality
		{
			get
			{
				return (int)this["TextureQuality"];
			}
			set
			{
				this["TextureQuality"] = value;
			}
		}
		[DefaultSettingValue("..\\..\\assets\\{0}"), UserScopedSetting, DebuggerNonUserCode]
		public string AssetRoot
		{
			get
			{
				return (string)this["AssetRoot"];
			}
			set
			{
				this["AssetRoot"] = value;
			}
		}
		[UserScopedSetting, DebuggerNonUserCode]
		public StringCollection GameComponents
		{
			get
			{
				return (StringCollection)this["GameComponents"];
			}
			set
			{
				this["GameComponents"] = value;
			}
		}
		[DefaultSettingValue("False"), UserScopedSetting, DebuggerNonUserCode]
		public bool AllowConsole
		{
			get
			{
				return (bool)this["AllowConsole"];
			}
			set
			{
				this["AllowConsole"] = value;
			}
		}
		[DefaultSettingValue("False"), UserScopedSetting, DebuggerNonUserCode]
		public bool DevMode
		{
			get
			{
				return (bool)this["DevMode"];
			}
			set
			{
				this["DevMode"] = value;
			}
		}
		[DefaultSettingValue("..\\..\\assets\\{0}"), UserScopedSetting, DebuggerNonUserCode]
		public string AssetBinRoot
		{
			get
			{
				return (string)this["AssetBinRoot"];
			}
			set
			{
				this["AssetBinRoot"] = value;
			}
		}
		[DefaultSettingValue("..\\..\\assets"), UserScopedSetting, DebuggerNonUserCode]
		public string ComponentRoot
		{
			get
			{
				return (string)this["ComponentRoot"];
			}
			set
			{
				this["ComponentRoot"] = value;
			}
		}
		[DefaultSettingValue("True"), UserScopedSetting, DebuggerNonUserCode]
		public bool ProceduralPlanets
		{
			get
			{
				return (bool)this["ProceduralPlanets"];
			}
			set
			{
				this["ProceduralPlanets"] = value;
			}
		}
		[DefaultSettingValue("True"), UserScopedSetting, DebuggerNonUserCode]
		public bool Decals
		{
			get
			{
				return (bool)this["Decals"];
			}
			set
			{
				this["Decals"] = value;
			}
		}
		[DefaultSettingValue("True"), UserScopedSetting, DebuggerNonUserCode]
		public bool Windowed
		{
			get
			{
				return (bool)this["Windowed"];
			}
			set
			{
				this["Windowed"] = value;
			}
		}
		[DefaultSettingValue("True"), UserScopedSetting, DebuggerNonUserCode]
		public bool Prefer64BitProcess
		{
			get
			{
				return (bool)this["Prefer64BitProcess"];
			}
			set
			{
				this["Prefer64BitProcess"] = value;
			}
		}
		[DefaultSettingValue("True"), UserScopedSetting, DebuggerNonUserCode]
		public bool FXAA
		{
			get
			{
				return (bool)this["FXAA"];
			}
			set
			{
				this["FXAA"] = value;
			}
		}
		[DefaultSettingValue(""), UserScopedSetting, DebuggerNonUserCode]
		public string TwoLetterISOLanguageName
		{
			get
			{
				return (string)this["TwoLetterISOLanguageName"];
			}
			set
			{
				this["TwoLetterISOLanguageName"] = value;
			}
		}
		[DefaultSettingValue("http://www.kerberos-productions.com/sots2_motd/index.html"), UserScopedSetting, DebuggerNonUserCode]
		public Uri HomePageURL
		{
			get
			{
				return (Uri)this["HomePageURL"];
			}
			set
			{
				this["HomePageURL"] = value;
			}
		}
		[DefaultSettingValue("35000"), UserScopedSetting, DebuggerNonUserCode]
		public ushort NetworkListenPort
		{
			get
			{
				return (ushort)this["NetworkListenPort"];
			}
			set
			{
				this["NetworkListenPort"] = value;
			}
		}
		[DefaultSettingValue("False"), UserScopedSetting, DebuggerNonUserCode]
		public bool CustomNetworkSettings
		{
			get
			{
				return (bool)this["CustomNetworkSettings"];
			}
			set
			{
				this["CustomNetworkSettings"] = value;
			}
		}
		[DefaultSettingValue("500"), UserScopedSetting, DebuggerNonUserCode]
		public uint NetworkMTUAffinity
		{
			get
			{
				return (uint)this["NetworkMTUAffinity"];
			}
			set
			{
				this["NetworkMTUAffinity"] = value;
			}
		}
		[DefaultSettingValue("False"), UserScopedSetting, DebuggerNonUserCode]
		public bool WindowedFullscreen
		{
			get
			{
				return (bool)this["WindowedFullscreen"];
			}
			set
			{
				this["WindowedFullscreen"] = value;
			}
		}
	}
}
