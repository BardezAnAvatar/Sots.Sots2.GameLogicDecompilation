using Kerberos.Sots.Engine;
using Kerberos.Sots.GameStates;
using System;
namespace Kerberos.Sots
{
	internal sealed class Settings : SettingsBase
	{
		public string LastProfile
		{
			get;
			set;
		}
		public bool LoadMenuCombat
		{
			get;
			set;
		}
		public bool AudioEnabled
		{
			get;
			set;
		}
		public bool SeperateStarMapFocus
		{
			get;
			set;
		}
		public bool SpeechSubtitles
		{
			get;
			set;
		}
		public bool CheckForInactiveFleets
		{
			get;
			set;
		}
		public bool JoinGlobalChat
		{
			get;
			set;
		}
		public bool AutoSave
		{
			get;
			set;
		}
		public int gfxAntialiasting
		{
			get;
			set;
		}
		public int gfxPreferredDisplay
		{
			get;
			set;
		}
		public int gfxDisplayMode
		{
			get;
			set;
		}
		public int gfxTextureQuality
		{
			get;
			set;
		}
		public int gfxDepthOfField
		{
			get;
			set;
		}
		public int gfxShadowQuality
		{
			get;
			set;
		}
		public int gfxCreaseShading
		{
			get;
			set;
		}
		public int gfxParticleDetail
		{
			get;
			set;
		}
		public int MusicVolume
		{
			get;
			set;
		}
		public int SpeechVolume
		{
			get;
			set;
		}
		public int EffectsVolume
		{
			get;
			set;
		}
		public int EndTurnDelay
		{
			get;
			set;
		}
		public Settings(string settingsDirectory) : base(settingsDirectory)
		{
			this.LoadMenuCombat = true;
			this.AudioEnabled = true;
			this.SeperateStarMapFocus = false;
			this.SpeechSubtitles = false;
			this.CheckForInactiveFleets = true;
			this.JoinGlobalChat = true;
			this.AutoSave = true;
			this.EndTurnDelay = 3;
			this.MusicVolume = 100;
			this.EffectsVolume = 100;
			this.SpeechVolume = 100;
			base.Load();
		}
		public void CopyFrom(Settings rhs)
		{
			this.LastProfile = rhs.LastProfile;
			this.LoadMenuCombat = rhs.LoadMenuCombat;
			this.AudioEnabled = rhs.AudioEnabled;
			this.SeperateStarMapFocus = rhs.SeperateStarMapFocus;
			this.SpeechSubtitles = rhs.SpeechSubtitles;
			this.gfxAntialiasting = rhs.gfxAntialiasting;
			this.gfxPreferredDisplay = rhs.gfxPreferredDisplay;
			this.gfxDisplayMode = rhs.gfxDisplayMode;
			this.gfxTextureQuality = rhs.gfxTextureQuality;
			this.gfxDepthOfField = rhs.gfxDepthOfField;
			this.gfxShadowQuality = rhs.gfxShadowQuality;
			this.gfxCreaseShading = rhs.gfxCreaseShading;
			this.gfxParticleDetail = rhs.gfxParticleDetail;
			this.MusicVolume = rhs.MusicVolume;
			this.SpeechVolume = rhs.SpeechVolume;
			this.EffectsVolume = rhs.EffectsVolume;
			this.EndTurnDelay = rhs.EndTurnDelay;
			this.CheckForInactiveFleets = rhs.CheckForInactiveFleets;
			this.JoinGlobalChat = rhs.JoinGlobalChat;
			this.AutoSave = rhs.AutoSave;
		}
		public void Commit(App app)
		{
			if (this.AudioEnabled)
			{
				app.TurnOnSound();
			}
			else
			{
				app.TurnOffSound();
			}
			app.PostSetVolumeMusic(this.MusicVolume);
			app.PostSetVolumeSpeech(this.SpeechVolume);
			app.PostSetVolumeEffects(this.EffectsVolume);
			app.PostSpeechSubtitles(this.SpeechSubtitles);
			StarMapState gameState = app.GetGameState<StarMapState>();
			if (app.CurrentState == gameState)
			{
				gameState.SetEndTurnTimeout(this.EndTurnDelay);
				gameState.EnableFleetCheck = this.CheckForInactiveFleets;
			}
		}
	}
}
