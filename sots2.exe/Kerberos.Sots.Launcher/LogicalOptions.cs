using Kerberos.Sots.Launcher.Properties;
using System;
using System.Text;
using System.Threading;
namespace Kerberos.Sots.Launcher
{
	public class LogicalOptions
	{
		public int Width;
		public int Height;
		public bool Windowed;
		public bool WindowedFullscreen;
		public bool FXAA;
		public bool Bloom;
		public bool FocalBlur;
		public bool Refraction;
		public bool AmbientOcclusion;
		public bool ProceduralPlanets;
		public bool Decals;
		public bool AllowConsole;
		public int TextureQuality;
		public bool CustomNetworkSettings;
		public uint NetworkMTUAffinity;
		public ushort NetworkListenPort;
		public string TwoLetterISOLanguageName;
		public string SteamConnectID;
		public LogicalOptions()
		{
			this.LoadFromSettings();
		}
		public void LoadFromSettings()
		{
			Settings @default = Settings.Default;
			this.Width = @default.ScreenWidth;
			this.Height = @default.ScreenHeight;
			this.Windowed = @default.Windowed;
			this.WindowedFullscreen = @default.WindowedFullscreen;
			this.FXAA = @default.FXAA;
			this.Bloom = @default.Bloom;
			this.FocalBlur = @default.FocalBlur;
			this.Refraction = @default.Refraction;
			this.AmbientOcclusion = @default.AmbientOcclusion;
			this.ProceduralPlanets = @default.ProceduralPlanets;
			this.Decals = @default.Decals;
			this.AllowConsole = @default.AllowConsole;
			this.TextureQuality = @default.TextureQuality;
			this.CustomNetworkSettings = @default.CustomNetworkSettings;
			this.NetworkMTUAffinity = @default.NetworkMTUAffinity;
			this.NetworkListenPort = @default.NetworkListenPort;
			this.TwoLetterISOLanguageName = @default.TwoLetterISOLanguageName;
		}
		public void SaveToSettings()
		{
			Settings @default = Settings.Default;
			@default.ScreenWidth = this.Width;
			@default.ScreenHeight = this.Height;
			@default.Windowed = this.Windowed;
			@default.WindowedFullscreen = this.WindowedFullscreen;
			@default.FXAA = this.FXAA;
			@default.Bloom = this.Bloom;
			@default.FocalBlur = this.FocalBlur;
			@default.Refraction = this.Refraction;
			@default.AmbientOcclusion = this.AmbientOcclusion;
			@default.ProceduralPlanets = this.ProceduralPlanets;
			@default.AllowConsole = this.AllowConsole;
			@default.TextureQuality = this.TextureQuality;
			@default.CustomNetworkSettings = this.CustomNetworkSettings;
			@default.NetworkMTUAffinity = this.NetworkMTUAffinity;
			@default.NetworkListenPort = this.NetworkListenPort;
			@default.TwoLetterISOLanguageName = this.TwoLetterISOLanguageName;
			@default.Save();
		}
		public string ToCommandLineString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("-n\"Sword of the Stars II\"");
			stringBuilder.Append(" -g\"..\\..\\assets\"");
			if (!Settings.Default.DevMode)
			{
				stringBuilder.Append(" +deployed");
			}
			stringBuilder.Append(" -w" + this.Width);
			stringBuilder.Append(" -h" + this.Height);
			string twoLetterISOLanguageName;
			if (string.IsNullOrEmpty(this.TwoLetterISOLanguageName))
			{
				twoLetterISOLanguageName = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;
			}
			else
			{
				twoLetterISOLanguageName = this.TwoLetterISOLanguageName;
			}
			stringBuilder.Append(" -locale:" + twoLetterISOLanguageName);
			stringBuilder.Append(this.Windowed ? " +window" : " -window");
			stringBuilder.Append((this.WindowedFullscreen && this.Windowed) ? " +fwindow" : string.Empty);
			stringBuilder.Append(this.FXAA ? " +rfxaa" : " -rfxaa");
			stringBuilder.Append(this.Bloom ? " +rbloom" : " -rbloom");
			stringBuilder.Append(this.FocalBlur ? " +rfblur" : " -rfblur");
			stringBuilder.Append(this.Refraction ? " +rrefract" : " -rrefract");
			stringBuilder.Append(this.AmbientOcclusion ? " +rocclude" : " -rocclude");
			stringBuilder.Append(this.ProceduralPlanets ? " +planetgen" : " -planetgen");
			stringBuilder.Append(this.Decals ? " +rdecal" : " -rdecal");
			stringBuilder.Append(this.AllowConsole ? " +console" : " -console");
			stringBuilder.Append(" -rmipbias" + Math.Max(5 - this.TextureQuality, 0));
			if (this.SteamConnectID != null)
			{
				stringBuilder.Append(" +connect_lobby:" + this.SteamConnectID);
			}
			if (this.CustomNetworkSettings)
			{
				stringBuilder.Append(" -port:" + this.NetworkListenPort);
			}
			return stringBuilder.ToString();
		}
	}
}
