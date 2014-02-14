using Kerberos.Sots.Engine;
using Kerberos.Sots.GameStates;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.UI
{
	internal class OptionsDialog : Dialog
	{
		private const string ProfileName = "lblProfileName";
		private const string EndTurnDelay = "preferencesEndTurnDelayValueSlider";
		private const string EndTurnDelayLabel = "preferencesEndTurnDelayValueSliderLabel";
		private const string MusicVolume = "audioMusicVolumeValueSlider";
		private const string MusicVolumeLabel = "audioMusicVolumeValueSliderLabel";
		private const string SpeechVolume = "audioSpeechVolumeValueSlider";
		private const string SpeechVolumeLabel = "audioSpeechVolumeValueSliderLabel";
		private const string EffectsVolume = "audioEffectsVolumeValueSlider";
		private const string EffectsVolumeLabel = "audioEffectsVolumeValueSliderLabel";
		private const string SeperateStarMapFocus = "preferencesSeparateStarMapFocusDDL";
		private const string MenuBackgroundCombat = "preferencesMenuBackgroundCombat";
		private const string FleetCheck = "preferencesInactiveFleets";
		private const string SpeechSubtitles = "preferencesSpeechSubtitles";
		private const string AudioEnabled = "audioEnabledDDL";
		private const string gfxPreferredDisplay = "graphicsPreferredDisplayDDL";
		private const string gfxDisplayMode = "graphicsDisplayModeDDL";
		private const string gfxAntialiasting = "graphicsAntialiasingDDL";
		private const string gfxTextureQuality = "graphicsTextureQualityDDL";
		private const string gfxDepthOfField = "graphicsDepthOfFieldDDL";
		private const string gfxShadowQuality = "graphicsShadowQualityDDL";
		private const string gfxCreaseShading = "graphicsCreaseShadingDDL";
		private const string gfxParticleDetail = "graphicsParticleDetailDDL";
		private const string JoinGlobal = "preferencesJoinGlobal";
		private const string AutoSave = "preferencesAutoSave";
		private const string EndTurnDelayFormat = "{0} sec";
		private const string VolumeFormat = "{0}%";
		private static string[] EnabledListItems = new string[]
		{
			App.Localize("@UI_OPTIONS_DIALOG_NO"),
			App.Localize("@UI_OPTIONS_DIALOG_YES")
		};
		private static string[] DetailItems = new string[]
		{
			App.Localize("@UI_OPTIONS_DIALOG_DETAIL_LOW"),
			App.Localize("@UI_OPTIONS_DIALOG_DETAIL_MEDIUM"),
			App.Localize("@UI_OPTIONS_DIALOG_DETAIL_HIGH")
		};
		private Settings _currentSettings;
		public OptionsDialog(App game, string template = "OptionsDialog") : base(game, template)
		{
		}
		public override void Initialize()
		{
			this.InitializeComponents();
		}
        protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
        {
            if (msgType == "button_clicked")
            {
                if (panelName == "gameOptionsOK")
                {
                    this.CommitSettings();
                    base._app.UI.CloseDialog(this, true);
                }
                else if (panelName == "gameOptionsReset")
                {
                    base._app.UI.CloseDialog(this, true);
                }
            }
            else if (!(msgType == "slider_value"))
            {
                if (msgType == "list_sel_changed")
                {
                    switch (panelName)
                    {
                        case "graphicsAntialiasingDDL":
                            this._currentSettings.gfxAntialiasting = int.Parse(msgParams[0]);
                            return;

                        case "graphicsPreferredDisplayDDL":
                            this._currentSettings.gfxPreferredDisplay = int.Parse(msgParams[0]);
                            return;

                        case "graphicsDisplayModeDDL":
                            this._currentSettings.gfxDisplayMode = int.Parse(msgParams[0]);
                            return;

                        case "graphicsCreaseShadingDDL":
                            this._currentSettings.gfxCreaseShading = int.Parse(msgParams[0]);
                            return;

                        case "graphicsDepthOfFieldDDL":
                            this._currentSettings.gfxDepthOfField = int.Parse(msgParams[0]);
                            return;

                        case "graphicsParticleDetailDDL":
                            this._currentSettings.gfxParticleDetail = int.Parse(msgParams[0]);
                            return;

                        case "graphicsShadowQualityDDL":
                            this._currentSettings.gfxShadowQuality = int.Parse(msgParams[0]);
                            return;

                        case "graphicsTextureQualityDDL":
                            this._currentSettings.gfxTextureQuality = int.Parse(msgParams[0]);
                            return;

                        case "preferencesSeparateStarMapFocusDDL":
                            if (int.Parse(msgParams[0]) <= 0)
                            {
                                this._currentSettings.SeperateStarMapFocus = false;
                                return;
                            }
                            this._currentSettings.SeperateStarMapFocus = true;
                            return;

                        case "audioEnabledDDL":
                            if (int.Parse(msgParams[0]) <= 0)
                            {
                                this._currentSettings.AudioEnabled = false;
                                return;
                            }
                            this._currentSettings.AudioEnabled = true;
                            return;

                        case "preferencesMenuBackgroundCombat":
                            if (int.Parse(msgParams[0]) <= 0)
                            {
                                this._currentSettings.LoadMenuCombat = false;
                                return;
                            }
                            this._currentSettings.LoadMenuCombat = true;
                            return;

                        case "preferencesInactiveFleets":
                            {
                                int num4 = int.Parse(msgParams[0]);
                                this._currentSettings.CheckForInactiveFleets = num4 > 0;
                                return;
                            }
                        case "preferencesJoinGlobal":
                            {
                                int num5 = int.Parse(msgParams[0]);
                                this._currentSettings.JoinGlobalChat = num5 > 0;
                                return;
                            }
                        case "preferencesAutoSave":
                            {
                                int num6 = int.Parse(msgParams[0]);
                                this._currentSettings.AutoSave = num6 > 0;
                                return;
                            }
                        case "preferencesSpeechSubtitles":
                            if (int.Parse(msgParams[0]) <= 0)
                            {
                                this._currentSettings.SpeechSubtitles = false;
                                return;
                            }
                            this._currentSettings.SpeechSubtitles = true;
                            return;
                    }
                }
            }
            else
            {
                string str = panelName;
                if (str != null)
                {
                    if (!(str == "preferencesEndTurnDelayValueSlider"))
                    {
                        if (!(str == "audioMusicVolumeValueSlider"))
                        {
                            if (!(str == "audioSpeechVolumeValueSlider"))
                            {
                                if (str == "audioEffectsVolumeValueSlider")
                                {
                                    base._app.UI.SetText("audioEffectsVolumeValueSliderLabel", string.Format("{0}%", msgParams[0]));
                                    this._currentSettings.EffectsVolume = int.Parse(msgParams[0]);
                                    base._app.PostSetVolumeEffects(this._currentSettings.EffectsVolume);
                                    base._app.PostRequestEffectSound("Explode_BattleRiderDeath");
                                    base._app.PostEnableEffectsSounds(true);
                                }
                                return;
                            }
                            base._app.UI.SetText("audioSpeechVolumeValueSliderLabel", string.Format("{0}%", msgParams[0]));
                            this._currentSettings.SpeechVolume = int.Parse(msgParams[0]);
                            base._app.PostSetVolumeSpeech(this._currentSettings.SpeechVolume);
                            base._app.PostEnableSpeechSounds(true);
                            base._app.PostRequestSpeech("COMBAT_005-01_human_SelectionAffirmed", 0x3e8, 120, 0f);
                            return;
                        }
                    }
                    else
                    {
                        base._app.UI.SetText("preferencesEndTurnDelayValueSliderLabel", string.Format("{0} sec", msgParams[0]));
                        this._currentSettings.EndTurnDelay = int.Parse(msgParams[0]);
                        return;
                    }
                    base._app.UI.SetText("audioMusicVolumeValueSliderLabel", string.Format("{0}%", msgParams[0]));
                    this._currentSettings.MusicVolume = int.Parse(msgParams[0]);
                    base._app.PostSetVolumeMusic(this._currentSettings.MusicVolume);
                }
            }
        }
        protected void PopulateListItems(string listId, string[] options, int? defaultOption = null)
		{
			this._app.UI.ClearItems(listId);
			for (int i = 0; i < options.Count<string>(); i++)
			{
				this._app.UI.AddItem(listId, string.Empty, i, options[i]);
			}
			if (defaultOption.HasValue)
			{
				this._app.UI.SetSelection(listId, defaultOption.Value);
			}
		}
		protected void InitializeComponents()
		{
			this._currentSettings = new Settings(this._app.SettingsDir);
			this._currentSettings.CopyFrom(this._app.GameSettings);
			this._app.UI.SetText("lblProfileName", this._app.UserProfile.ProfileName);
			int endTurnDelay = this._app.GameSettings.EndTurnDelay;
			this._app.UI.SetSliderValue("preferencesEndTurnDelayValueSlider", endTurnDelay);
			this._app.UI.SetText("preferencesEndTurnDelayValueSliderLabel", string.Format("{0} sec", endTurnDelay));
			this._app.UI.SetSliderValue("audioMusicVolumeValueSlider", this._app.GameSettings.MusicVolume);
			this._app.UI.SetText("audioMusicVolumeValueSliderLabel", string.Format("{0}%", this._app.GameSettings.MusicVolume));
			this._app.UI.SetSliderValue("audioSpeechVolumeValueSlider", this._app.GameSettings.SpeechVolume);
			this._app.UI.SetText("audioSpeechVolumeValueSliderLabel", string.Format("{0}%", this._app.GameSettings.SpeechVolume));
			this._app.UI.SetSliderValue("audioEffectsVolumeValueSlider", this._app.GameSettings.EffectsVolume);
			this._app.UI.SetText("audioEffectsVolumeValueSliderLabel", string.Format("{0}%", this._app.GameSettings.EffectsVolume));
			this.PopulateListItems("preferencesSeparateStarMapFocusDDL", OptionsDialog.EnabledListItems, new int?(this._app.GameSettings.SeperateStarMapFocus ? 1 : 0));
			this.PopulateListItems("preferencesMenuBackgroundCombat", OptionsDialog.EnabledListItems, new int?(this._app.GameSettings.LoadMenuCombat ? 1 : 0));
			this.PopulateListItems("preferencesInactiveFleets", OptionsDialog.EnabledListItems, new int?(this._app.GameSettings.CheckForInactiveFleets ? 1 : 0));
			this.PopulateListItems("preferencesJoinGlobal", OptionsDialog.EnabledListItems, new int?(this._app.GameSettings.JoinGlobalChat ? 1 : 0));
			this.PopulateListItems("preferencesAutoSave", OptionsDialog.EnabledListItems, new int?(this._app.GameSettings.AutoSave ? 1 : 0));
			this.PopulateListItems("preferencesSpeechSubtitles", OptionsDialog.EnabledListItems, new int?(this._app.GameSettings.SpeechSubtitles ? 1 : 0));
			this.PopulateListItems("audioEnabledDDL", OptionsDialog.EnabledListItems, new int?(this._app.GameSettings.AudioEnabled ? 1 : 0));
			this.PopulateListItems("graphicsTextureQualityDDL", OptionsDialog.DetailItems, new int?(this._app.GameSettings.gfxTextureQuality));
			this.PopulateListItems("graphicsDepthOfFieldDDL", OptionsDialog.DetailItems, new int?(this._app.GameSettings.gfxDepthOfField));
			this.PopulateListItems("graphicsShadowQualityDDL", OptionsDialog.DetailItems, new int?(this._app.GameSettings.gfxShadowQuality));
			this.PopulateListItems("graphicsCreaseShadingDDL", OptionsDialog.DetailItems, new int?(this._app.GameSettings.gfxCreaseShading));
			this.PopulateListItems("graphicsParticleDetailDDL", OptionsDialog.DetailItems, new int?(this._app.GameSettings.gfxParticleDetail));
		}
		protected void CommitSettings()
		{
			this._app.GameSettings.CopyFrom(this._currentSettings);
			this._app.GameSettings.Save();
			this._app.GameSettings.Commit(this._app);
		}
		public override string[] CloseDialog()
		{
			if (this._app.CurrentState == this._app.GetGameState<MainMenuState>())
			{
				this._app.PostEnableEffectsSounds(false);
				this._app.PostEnableSpeechSounds(false);
			}
			return null;
		}
	}
}
