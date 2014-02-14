using Kerberos.Sots.Engine;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.UI
{
	internal class PlayerWidget : PanelBinding
	{
		private const string WidgetPanel = "playerDropdown";
		private App App;
		private int _prevNumBusyPlayers = -1;
		private List<PlayerDetails> _prevPlayerDetails = new List<PlayerDetails>();
		private bool _visible;
		private bool _initialized;
		public bool Visible
		{
			get
			{
				return this._visible;
			}
			set
			{
				base.UI.SetVisible("playerDropdown", value);
			}
		}
		public bool Initialized
		{
			get
			{
				return this._initialized;
			}
		}
		public PlayerWidget(App game, UICommChannel ui, string id) : base(ui, id)
		{
			this.App = game;
			this.App.UI.GameEvent += new UIEventGameEvent(this.UICommChannel_GameEvent);
		}
		public void Initialize()
		{
			int num = 0;
			foreach (PlayerSetup current in this.App.GameSetup.Players)
			{
				PlayerDetails playerDetails = new PlayerDetails();
				playerDetails.Slot = current.slot;
				playerDetails.Status = current.Status;
				playerDetails.AI = current.AI;
				this.App.UI.AddItem(this.App.UI.Path(new string[]
				{
					"playerDropdown",
					"playerList"
				}), "", num, "");
				playerDetails.ItemID = this.App.UI.GetItemGlobalID(this.App.UI.Path(new string[]
				{
					"playerDropdown",
					"playerList"
				}), "", num, "");
				string arg = current.Name;
				if (current.AI)
				{
					arg = "AI Player";
				}
				this.App.UI.SetPropertyString(this.App.UI.Path(new string[]
				{
					playerDetails.ItemID,
					"name"
				}), "text", string.Format(App.Localize("@PLAYERS_WIDGET_PLAYER_NAME_OF"), arg, current.EmpireName));
				this.App.UI.SetPropertyColorNormalized(this.App.UI.Path(new string[]
				{
					playerDetails.ItemID,
					"name"
				}), "color", this.App.GameSetup.GetEmpireColor(current.EmpireColor));
				this.App.UI.SetPropertyString(this.App.UI.Path(new string[]
				{
					playerDetails.ItemID,
					"status"
				}), "text", GameSetup.PlayerStatusToString(current.Status));
				this.App.UI.SetPropertyString(this.App.UI.Path(new string[]
				{
					playerDetails.ItemID,
					"avatar"
				}), "sprite", current.Avatar ?? string.Empty);
				this.App.UI.SetPropertyString(this.App.UI.Path(new string[]
				{
					playerDetails.ItemID,
					"badge"
				}), "sprite", current.Badge ?? string.Empty);
				this.App.UI.SetVisible(this.App.UI.Path(new string[]
				{
					playerDetails.ItemID,
					"eliminatedState"
				}), current.Status == NPlayerStatus.PS_DEFEATED);
				this._prevPlayerDetails.Add(playerDetails);
				this.App.UI.ForceLayout("playerList");
				num++;
			}
			this._initialized = true;
		}
		public void UpdateSlotStatus(PlayerDetails details, PlayerSetup player)
		{
			this.App.UI.SetPropertyString(this.App.UI.Path(new string[]
			{
				details.ItemID,
				"status"
			}), "text", GameSetup.PlayerStatusToString(player.Status));
			this.App.UI.SetVisible(this.App.UI.Path(new string[]
			{
				details.ItemID,
				"eliminatedState"
			}), player.Status == NPlayerStatus.PS_DEFEATED);
			details.Status = player.Status;
		}
		public void Sync()
		{
			int num = 0;
			string arg = "";
			for (int i = 0; i < this.App.GameSetup.Players.Count<PlayerSetup>(); i++)
			{
				if (this.App.GameSetup.Players[i].Status != NPlayerStatus.PS_WAIT && this.App.GameSetup.Players[i].Status != NPlayerStatus.PS_DEFEATED && !this.App.GameSetup.Players[i].AI)
				{
					num++;
					arg = this.App.GameSetup.Players[i].Name;
				}
				if (this._prevPlayerDetails[i].Status != this.App.GameSetup.Players[i].Status)
				{
					this.UpdateSlotStatus(this._prevPlayerDetails[i], this.App.GameSetup.Players[i]);
				}
				if (this._prevPlayerDetails[i].AI != this.App.GameSetup.Players[i].AI)
				{
					if (this.App.GameSetup.Players[i].AI)
					{
						this.App.UI.SetPropertyString(this.App.UI.Path(new string[]
						{
							this._prevPlayerDetails[i].ItemID,
							"name"
						}), "text", string.Format(App.Localize("@PLAYERS_WIDGET_PLAYER_NAME_OF"), "AI Player", this.App.GameSetup.Players[i].EmpireName));
					}
					else
					{
						this.App.UI.SetPropertyString(this.App.UI.Path(new string[]
						{
							this._prevPlayerDetails[i].ItemID,
							"name"
						}), "text", string.Format(App.Localize("@PLAYERS_WIDGET_PLAYER_NAME_OF"), this.App.GameSetup.Players[i].Name, this.App.GameSetup.Players[i].EmpireName));
					}
					this._prevPlayerDetails[i].AI = this.App.GameSetup.Players[i].AI;
				}
			}
			if (num != this._prevNumBusyPlayers)
			{
				if (num == 0)
				{
					this.App.UI.SetPropertyString(this.App.UI.Path(new string[]
					{
						"playerDropdown",
						"playersRemaining"
					}), "text", App.Localize("@PLAYERS_WIDGET_PLAYER_FINISHED"));
				}
				else
				{
					if (num == 1)
					{
						this.App.UI.SetPropertyString(this.App.UI.Path(new string[]
						{
							"playerDropdown",
							"playersRemaining"
						}), "text", string.Format(App.Localize("@PLAYERS_WIDGET_WAITING_ON"), arg));
					}
					else
					{
						this.App.UI.SetPropertyString(this.App.UI.Path(new string[]
						{
							"playerDropdown",
							"playersRemaining"
						}), "text", string.Format(App.Localize("@PLAYERS_WIDGET_PLAYERS_REMAINING"), num.ToString()));
					}
				}
			}
			this._prevNumBusyPlayers = num;
			if (this.App.GameSetup.StrategicTurnLength != 3.40282347E+38f)
			{
				float num2 = this.App.GameSetup.StrategicTurnLength;
				TimeSpan turnTime = this.App.Game.TurnTimer.GetTurnTime();
				num2 *= 60f;
				float num3 = num2 - ((float)turnTime.Minutes * 60f + (float)turnTime.Seconds);
				int num4 = (int)(num3 / 60f);
				string text = (num3 - (float)num4 * 60f).ToString();
				if (text.Count<char>() == 1)
				{
					text = "0" + text;
				}
				this.App.UI.SetPropertyString(this.App.UI.Path(new string[]
				{
					"playerDropdown",
					"timeRemaining"
				}), "text", num4.ToString() + ":" + text);
				this.App.UI.SetVisible(this.App.UI.Path(new string[]
				{
					"playerDropdown",
					"timeRemaining"
				}), true);
				return;
			}
			this.App.UI.SetVisible(this.App.UI.Path(new string[]
			{
				"playerDropdown",
				"timeRemaining"
			}), false);
		}
		public void Terminate()
		{
			this._initialized = false;
			this.App.UI.GameEvent -= new UIEventGameEvent(this.UICommChannel_GameEvent);
			this.App.UI.ClearItems(this.App.UI.Path(new string[]
			{
				"playerDropdown",
				"playerList"
			}));
		}
		private void UICommChannel_GameEvent(string eventName, string[] eventParams)
		{
		}
	}
}
