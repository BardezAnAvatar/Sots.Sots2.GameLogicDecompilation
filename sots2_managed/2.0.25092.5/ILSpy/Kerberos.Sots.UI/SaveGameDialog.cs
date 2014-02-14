using Kerberos.Sots.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace Kerberos.Sots.UI
{
	internal class SaveGameDialog : Dialog
	{
		public class GameSummary
		{
			public readonly PlayerInfo[] Players;
			public readonly int Turn;
			public readonly string MapName;
			public GameSummary(string filename, AssetDatabase assetdb)
			{
				this.Players = new PlayerInfo[0];
				this.Turn = 0;
				this.MapName = string.Empty;
				try
				{
					using (GameDatabase gameDatabase = GameDatabase.Connect(filename, assetdb))
					{
						try
						{
							this.Players = (
								from x in gameDatabase.GetPlayerInfos()
								where x.isStandardPlayer
								select x).ToArray<PlayerInfo>();
						}
						catch (Exception)
						{
						}
						try
						{
							this.Turn = gameDatabase.GetTurnCount();
						}
						catch (Exception)
						{
						}
						try
						{
							this.MapName = gameDatabase.GetMapName();
						}
						catch (Exception)
						{
						}
					}
				}
				catch (Exception)
				{
				}
			}
		}
		public const string EditBoxPanel = "gameSaveName";
		public const string SaveButton = "buttonSave";
		public const string DeleteButton = "buttonDelete";
		public const string CancelButton = "buttonCancel";
		public const string GameList = "gameList";
		public const string FileExtension = ".sots2save";
		private string _deleteFilename;
		private int _deleteIndex;
		private string _enteredText;
		private int _minChars = 1;
		private int _maxChars = 64;
		private bool _choice;
		protected int _selectedIndex = -1;
		protected Dictionary<int, string> _selectionFileNames = new Dictionary<int, string>();
		private string _reallyDeleteDialog;
		private string _fileExistsDialog;
		public SaveGameDialog(App game, string defaultName, string template = "dialogSaveGame") : base(game, template)
		{
			this._enteredText = (defaultName ?? string.Empty);
		}
		protected virtual void OnSelectionCleared()
		{
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (panelName == "buttonSave")
				{
					this.Confirm();
					return;
				}
				if (panelName == "buttonCancel")
				{
					this._choice = false;
					this._app.UI.CloseDialog(this, true);
					return;
				}
				if (panelName.StartsWith("buttonDelete"))
				{
					string[] array = panelName.Split(new char[]
					{
						'|'
					});
					this._deleteIndex = int.Parse(array[1]);
					this._deleteFilename = array[2];
					this._reallyDeleteDialog = this._app.UI.CreateDialog(new GenericQuestionDialog(this._app, "@DELETE_SAVED_GAME", "@DELETE_SAVED_GAME_TEXT", "dialogGenericQuestion"), null);
					return;
				}
			}
			else
			{
				if (msgType == "dialog_closed")
				{
					if (panelName == this._reallyDeleteDialog)
					{
						if (bool.Parse(msgParams[0]))
						{
							this._app.UI.RemoveItems(this._app.UI.Path(new string[]
							{
								base.ID,
								"gameList"
							}), this._deleteIndex);
							File.Delete(this._deleteFilename);
							this._selectedIndex = -1;
							this.OnSelectionCleared();
							this._selectionFileNames.Remove(this._deleteIndex);
						}
					}
					else
					{
						if (panelName == this._fileExistsDialog && bool.Parse(msgParams[0]))
						{
							this.SaveGame();
						}
					}
				}
				else
				{
					if (msgType == "edit_confirmed")
					{
						if (panelName == "gameSaveName")
						{
							this.Confirm();
							return;
						}
					}
					else
					{
						if (msgType == "text_changed")
						{
							if (panelName == "gameSaveName")
							{
								this._enteredText = msgParams[0];
								return;
							}
						}
						else
						{
							if (msgType == "list_sel_changed")
							{
								if (string.IsNullOrEmpty(msgParams[0]))
								{
									return;
								}
								this._selectedIndex = int.Parse(msgParams[0]);
								this._enteredText = Path.GetFileNameWithoutExtension(this._selectionFileNames[this._selectedIndex]);
								this._app.UI.SetPropertyString("gameSaveName", "text", this._enteredText);
							}
						}
					}
				}
			}
			base.OnPanelMessage(panelName, msgType, msgParams);
		}
		public virtual void Confirm()
		{
			if (this._enteredText.Count<char>() < this._minChars)
			{
				this._app.UI.CreateDialog(new GenericTextDialog(this._app, App.Localize("@INVALID_NAME"), string.Format(App.Localize("@INVALID_NAME_TEXT"), this._minChars), "dialogGenericMessage"), null);
				return;
			}
			if (File.Exists(Path.Combine(this._app.SaveDir, this._enteredText + ".sots2save")))
			{
				this._fileExistsDialog = this._app.UI.CreateDialog(new GenericQuestionDialog(this._app, App.Localize("@OVERWRITE_SAVED_GAME"), App.Localize("@OVERWRITE_SAVED_GAME_TEXT"), "dialogGenericQuestion"), null);
				return;
			}
			this.SaveGame();
		}
		public void SaveGame()
		{
			this._app.Game.Save(Path.Combine(this._app.SaveDir, this._enteredText + ".sots2save"));
			this._choice = true;
			this._app.UI.CloseDialog(this, true);
			this._app.UI.CreateDialog(new GenericTextDialog(this._app, App.Localize("@DIALOG_SUCCESS_HEADER"), App.Localize("@DIALOG_GAME_SAVED_SUCCESS"), "dialogGenericMessage"), null);
		}
		public override void Initialize()
		{
			base.Initialize();
			this._app.UI.Send(new object[]
			{
				"SetMaxChars",
				"gameSaveName",
				this._maxChars
			});
			this._app.UI.Send(new object[]
			{
				"SetFileMode",
				"gameSaveName",
				true
			});
			this._app.UI.SetPropertyString("gameSaveName", "text", this._enteredText);
			SavedGameFilename[] availableSavedGames = this._app.GetAvailableSavedGames();
			int num = 0;
			SavedGameFilename[] array = availableSavedGames;
			for (int i = 0; i < array.Length; i++)
			{
				SavedGameFilename savedGameFilename = array[i];
				this._app.UI.AddItem(this._app.UI.Path(new string[]
				{
					base.ID,
					"gameList"
				}), "", num, "");
				string itemGlobalID = this._app.UI.GetItemGlobalID(this._app.UI.Path(new string[]
				{
					base.ID,
					"gameList"
				}), "", num, "");
				this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"saveName"
				}), "text", Path.GetFileNameWithoutExtension(savedGameFilename.RootedFilename));
				SaveGameDialog.GameSummary gameSummary = new SaveGameDialog.GameSummary(savedGameFilename.RootedFilename, this._app.AssetDatabase);
				int num2 = gameSummary.Players.Length;
				this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"numPlayers"
				}), "text", num2.ToString() + " " + App.Localize("@PLAYERNAME"));
				this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"numTurns"
				}), "text", App.Localize("@GENERAL_TURN") + " " + gameSummary.Turn.ToString());
				this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"mapName"
				}), "text", App.Localize("@LOADGAME_MAP") + " " + gameSummary.MapName);
				this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"buttonDelete"
				}), "id", "buttonDelete|" + num.ToString() + "|" + savedGameFilename.RootedFilename);
				this._selectionFileNames.Add(num, savedGameFilename.RootedFilename);
				int num3 = 1;
				PlayerInfo[] players = gameSummary.Players;
				for (int j = 0; j < players.Length; j++)
				{
					PlayerInfo playerInfo = players[j];
					string text = this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"player" + num3.ToString() + "info"
					});
					this._app.UI.SetVisible(text, true);
					this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
					{
						text,
						"playernum"
					}), "text", "?");
					this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
					{
						text,
						"playeravatar"
					}), "sprite", Path.GetFileNameWithoutExtension(playerInfo.AvatarAssetPath));
					this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
					{
						text,
						"badge"
					}), "sprite", Path.GetFileNameWithoutExtension(playerInfo.BadgeAssetPath));
					this._app.UI.SetVisible(this._app.UI.Path(new string[]
					{
						text,
						"eliminatedState"
					}), playerInfo.isDefeated);
					this._app.UI.SetPropertyColorNormalized(this._app.UI.Path(new string[]
					{
						text,
						"primaryColor"
					}), "color", playerInfo.PrimaryColor);
					this._app.UI.SetPropertyColorNormalized(this._app.UI.Path(new string[]
					{
						text,
						"secondaryColor"
					}), "color", playerInfo.SecondaryColor);
					this._app.UI.AddItem(this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"playerList"
					}), "", num3, "?- " + playerInfo.Name);
					string itemGlobalID2 = this._app.UI.GetItemGlobalID(this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"playerList"
					}), "", num3, "?- " + playerInfo.Name);
					this._app.UI.SetPropertyColorNormalized(this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"playerList",
						itemGlobalID2
					}), "color", playerInfo.PrimaryColor);
					num3++;
				}
				num++;
			}
		}
		public override string[] CloseDialog()
		{
			return new string[]
			{
				this._choice.ToString(),
				this._enteredText
			};
		}
	}
}
