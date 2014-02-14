using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.UI;
using System;
using System.Collections.Generic;
using System.IO;
namespace Kerberos.Sots.GameStates
{
	internal class MainMenuState : GameState
	{
		private MainMenuScene _scene;
		private GameState _postLoginState;
		private List<object> _postLoginParms = new List<object>();
		private string _profileGUID = "";
		public MainMenuState(App game) : base(game)
		{
		}
		protected void SetNextState(GameState state, params object[] parms)
		{
			this._postLoginState = state;
			this._postLoginParms.Clear();
			this._postLoginParms.AddRange(parms);
		}
		private void ShowProfileDialog()
		{
			this._profileGUID = base.App.UI.CreateDialog(new SelectProfileDialog(base.App), null);
		}
		protected override void OnPrepare(GameState prev, object[] parms)
		{
			if (base.App.GameSettings.LoadMenuCombat)
			{
				this._scene = new MainMenuScene();
			}
			if (this._scene != null)
			{
				this._scene.Enter(base.App);
			}
			base.App.UI.LoadScreen("MainMenu");
			base.App.Network.EnableChatWidgetPlayerList(false);
			base.App.Network.EnableChatWidget(false);
		}
		private bool CanContinueGame()
		{
			return base.App.UserProfile != null && File.Exists(base.App.UserProfile.LastGamePlayed);
		}
		protected override void OnEnter()
		{
			base.App.UI.SetScreen("MainMenu");
			base.App.UI.SetVisible("mainMenuElements", true);
			base.App.UI.SetVisible("gameCredits", false);
			base.App.UI.SetEnabled("gameContinueButton", this.CanContinueGame());
			base.App.UI.SetEnabled("gameCinematicsButton", false);
			base.App.UI.SetText("verNumLabel", "2.0.25092.5");
			bool flag = base.App.Steam.BLoggedOn();
			base.App.UI.SetEnabled("gameMultiplayerButton", flag);
			base.App.UI.SetTooltip("gameMultiplayerButton", flag ? "" : App.Localize("@UI_LOGINTO_STEAM_MULTIPLAYER"));
			base.App.UI.PanelMessage += new UIEventPanelMessage(this.UICommChannel_PanelMessage);
			this._postLoginState = null;
			this._postLoginParms.Clear();
			base.App.PostPlayMusic("Main_Menu");
			base.App.PostEnableEffectsSounds(false);
			base.App.PostEnableSpeechSounds(false);
			if (this._scene != null)
			{
				this._scene.Activate();
			}
			if (!base.App.ProfileSelected)
			{
				base.App.UI.CreateDialog(new SelectProfileDialog(base.App), null);
			}
			if (GameDatabase.CheckForPre_EOFSaves(base.App))
			{
				base.App.UI.CreateDialog(new GenericTextDialog(base.App, App.Localize("@PRE_EOF_GAMESAVES_DETECTED"), App.Localize("@PRE_EOF_GAMESAVES_DETECTED_MESSAGE"), "dialogGenericMessage"), null);
			}
		}
		private void UICommChannel_PanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "dialog_closed")
			{
				if (panelName == this._profileGUID && this._postLoginState != null)
				{
					base.App.SwitchGameState(this._postLoginState, this._postLoginParms.ToArray());
					return;
				}
			}
			else
			{
				if (msgType == "button_clicked")
				{
					if (panelName == "gameExitButton")
					{
						base.App.RequestExit();
						return;
					}
					if (panelName == "gameContinueButton")
					{
						if (this.CanContinueGame())
						{
							base.App.GameSetup.IsMultiplayer = false;
							base.App.UILoadGame(base.App.UserProfile.LastGamePlayed);
							return;
						}
						base.App.UI.SetEnabled("gameContinueButton", false);
						return;
					}
					else
					{
						if (panelName == "gameCreateGameButton")
						{
							if (!base.App.ProfileSelected)
							{
								this.SetNextState(base.App.GetGameState<GameSetupState>(), new object[]
								{
									true
								});
								this.ShowProfileDialog();
								return;
							}
							base.App.ResetGameSetup();
							base.App.GameSetup.IsMultiplayer = false;
							base.App.SwitchGameState<GameSetupState>(new object[]
							{
								true
							});
							return;
						}
						else
						{
							if (panelName == "gameLoadGameButton")
							{
								base.App.ResetGameSetup();
								base.App.GameSetup.IsMultiplayer = false;
								base.App.UI.CreateDialog(new LoadGameDialog(base.App, null), null);
								return;
							}
							if (panelName == "gameMultiplayerButton")
							{
								if (!base.App.ProfileSelected)
								{
									base.App.GameSetup.IsMultiplayer = true;
									this.SetNextState(base.App.GetGameState<StarMapLobbyState>(), new object[]
									{
										LobbyEntranceState.Browser
									});
									this.ShowProfileDialog();
									return;
								}
								base.App.ResetGameSetup();
								base.App.GameSetup.IsMultiplayer = true;
								base.App.SwitchGameState<StarMapLobbyState>(new object[]
								{
									LobbyEntranceState.Browser
								});
								return;
							}
							else
							{
								if (panelName == "gameSotspediaButton")
								{
									base.App.SwitchGameState("SotspediaState");
									return;
								}
								if (panelName == "gameProfileButton")
								{
									this.ShowProfileDialog();
									return;
								}
								if (panelName == "gameOptionsButton")
								{
									base.App.UI.CreateDialog(new OptionsDialog(base.App, "OptionsDialog"), null);
									return;
								}
								if (panelName == "gameCinematicsButton")
								{
									base.App.SwitchGameState("CinematicsState");
									return;
								}
								if (panelName == "gameCreditsButton")
								{
									base.App.UI.SetVisible("mainMenuElements", false);
									base.App.UI.SetVisible("gameCredits", true);
									base.App.UI.SetTextFile("gameCreditsText", Path.Combine(AssetDatabase.CommonStrings.UnrootedDirectory, "credits.txt"));
									base.App.UI.Send(new object[]
									{
										"SetCreditScrollPosition",
										"gameCreditsText",
										0f
									});
									base.App.UI.ForceLayout("gameCredits");
									return;
								}
								if (panelName == "gameCreditsCloseButton")
								{
									base.App.UI.SetVisible("mainMenuElements", true);
									base.App.UI.SetVisible("gameCredits", false);
								}
							}
						}
					}
				}
			}
		}
		protected override void OnExit(GameState prev, ExitReason reason)
		{
			if (this._scene != null)
			{
				this._scene.Exit();
				this._scene = null;
			}
			base.App.UI.PanelMessage -= new UIEventPanelMessage(this.UICommChannel_PanelMessage);
			base.App.UI.DeleteScreen("MainMenu");
		}
		protected override void OnUpdate()
		{
			if (this._scene != null)
			{
				this._scene.Update();
			}
		}
		public override bool IsReady()
		{
			return (this._scene == null || this._scene.IsReady()) && base.IsReady();
		}
		public override void OnEngineMessage(InteropMessageID messageID, ScriptMessageReader mr)
		{
			switch (messageID)
			{
			case InteropMessageID.IMID_SCRIPT_OBJECT_ADD:
				if (this._scene != null)
				{
					this._scene.AddObject(mr);
					return;
				}
				return;
			case InteropMessageID.IMID_SCRIPT_OBJECT_RELEASE:
				if (this._scene != null)
				{
					this._scene.RemoveObject(mr);
					return;
				}
				return;
			case InteropMessageID.IMID_SCRIPT_OBJECTS_RELEASE:
				if (this._scene != null)
				{
					this._scene.RemoveObjects(mr);
					return;
				}
				return;
			}
			App.Log.Warn("Unhandled message (id=" + messageID + ").", "engine");
		}
	}
}
