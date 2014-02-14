using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.StarMapElements;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace Kerberos.Sots.UI
{
	internal class DialogCombatsPending : Dialog
	{
		private const string ContentList = "contentList";
		private const string SimWidget = "simWidget";
		private const string ResolvingText = "reolvetext";
		private const string Combatslabel = "combatsremaininglbl";
		private const string AvatarsInCombat = "combatPlayers";
		private const string AvatarsInother = "remainingPlayers";
		private List<PendingCombat> _listedcombats;
		private List<SystemWidget> _systemWidgets = new List<SystemWidget>();
		public DialogCombatsPending(App app) : base(app, "dialogCombatsPending")
		{
			this._listedcombats = new List<PendingCombat>();
		}
		private void UpdateCombatList()
		{
			if (!this._app.Game.GetPendingCombats().Any((PendingCombat x) => this._listedcombats.Any((PendingCombat j) => j.ConflictID == x.ConflictID)))
			{
				this._listedcombats.Clear();
				foreach (SystemWidget current in this._systemWidgets)
				{
					current.Terminate();
				}
				this._systemWidgets.Clear();
				this._app.UI.ClearItems(this._app.UI.Path(new string[]
				{
					base.ID,
					"contentList"
				}));
			}
			foreach (PendingCombat cmb in this._app.Game.GetPendingCombats())
			{
				bool flag = this._app.Game.CombatData.GetCombat(this._app.GameDatabase, cmb.ConflictID, cmb.SystemID, this._app.GameDatabase.GetTurnCount()) != null;
				string itemGlobalID;
				if (this._listedcombats.Any((PendingCombat x) => x.ConflictID == cmb.ConflictID))
				{
					if (this._listedcombats.FirstOrDefault((PendingCombat x) => x.ConflictID == cmb.ConflictID).complete == flag)
					{
						continue;
					}
					this._listedcombats.FirstOrDefault((PendingCombat x) => x.ConflictID == cmb.ConflictID).complete = flag;
					itemGlobalID = this._app.UI.GetItemGlobalID(this._app.UI.Path(new string[]
					{
						base.ID,
						"contentList"
					}), "", cmb.ConflictID, "");
					this._listedcombats.Remove(this._listedcombats.FirstOrDefault((PendingCombat x) => x.ConflictID == cmb.ConflictID));
				}
				else
				{
					this._app.UI.AddItem(this._app.UI.Path(new string[]
					{
						base.ID,
						"contentList"
					}), "", cmb.ConflictID, "");
					itemGlobalID = this._app.UI.GetItemGlobalID(this._app.UI.Path(new string[]
					{
						base.ID,
						"contentList"
					}), "", cmb.ConflictID, "");
					this._app.GameDatabase.GetStarSystemInfo(cmb.SystemID);
					this._listedcombats.Add(cmb);
					this._app.UI.ClearItems(this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"combatPlayers"
					}));
					bool flag2 = StarMap.IsInRange(this._app.GameDatabase, this._app.LocalPlayer.ID, cmb.SystemID);
					foreach (int current2 in cmb.PlayersInCombat)
					{
						this._app.UI.AddItem(this._app.UI.Path(new string[]
						{
							itemGlobalID,
							"combatPlayers"
						}), "", current2, "");
						string itemGlobalID2 = this._app.UI.GetItemGlobalID(this._app.UI.Path(new string[]
						{
							itemGlobalID,
							"combatPlayers"
						}), "", current2, "");
						this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
						{
							itemGlobalID2,
							"smallAvatar"
						}), "sprite", Path.GetFileNameWithoutExtension(this._app.GameDatabase.GetPlayerInfo(current2).AvatarAssetPath));
						if (flag2 && Path.GetFileNameWithoutExtension(this._app.GameDatabase.GetPlayerInfo(current2).AvatarAssetPath) != string.Empty)
						{
							this._app.UI.SetVisible(this._app.UI.Path(new string[]
							{
								itemGlobalID2,
								"smallAvatar"
							}), true);
							this._app.UI.SetVisible(this._app.UI.Path(new string[]
							{
								itemGlobalID2,
								"UnknownText"
							}), false);
						}
						else
						{
							this._app.UI.SetVisible(this._app.UI.Path(new string[]
							{
								itemGlobalID2,
								"smallAvatar"
							}), false);
							this._app.UI.SetVisible(this._app.UI.Path(new string[]
							{
								itemGlobalID2,
								"UnknownText"
							}), true);
						}
						CombatState.SetPlayerCardOutlineColor(this._app, this._app.UI.Path(new string[]
						{
							itemGlobalID2,
							"bgPlayerColor"
						}), flag2 ? this._app.GameDatabase.GetPlayerInfo(current2).PrimaryColor : new Vector3(0f, 0f, 0f));
					}
					this._app.UI.ClearItems(this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"remainingPlayers"
					}));
					foreach (int current3 in cmb.NPCPlayersInCombat)
					{
						this._app.UI.AddItem(this._app.UI.Path(new string[]
						{
							itemGlobalID,
							"remainingPlayers"
						}), "", current3, "");
						string itemGlobalID3 = this._app.UI.GetItemGlobalID(this._app.UI.Path(new string[]
						{
							itemGlobalID,
							"remainingPlayers"
						}), "", current3, "");
						this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
						{
							itemGlobalID3,
							"smallAvatar"
						}), "sprite", Path.GetFileNameWithoutExtension(this._app.GameDatabase.GetPlayerInfo(current3).AvatarAssetPath));
						if (flag2 && Path.GetFileNameWithoutExtension(this._app.GameDatabase.GetPlayerInfo(current3).AvatarAssetPath) != string.Empty)
						{
							this._app.UI.SetVisible(this._app.UI.Path(new string[]
							{
								itemGlobalID3,
								"smallAvatar"
							}), true);
							this._app.UI.SetVisible(this._app.UI.Path(new string[]
							{
								itemGlobalID3,
								"UnknownText"
							}), false);
						}
						else
						{
							this._app.UI.SetVisible(this._app.UI.Path(new string[]
							{
								itemGlobalID3,
								"smallAvatar"
							}), false);
							this._app.UI.SetVisible(this._app.UI.Path(new string[]
							{
								itemGlobalID3,
								"UnknownText"
							}), true);
						}
						CombatState.SetPlayerCardOutlineColor(this._app, this._app.UI.Path(new string[]
						{
							itemGlobalID3,
							"bgPlayerColor"
						}), flag2 ? this._app.GameDatabase.GetPlayerInfo(current3).PrimaryColor : new Vector3(0f, 0f, 0f));
					}
					this.SyncSystemOwnershipEffect(this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"systemTitleCard"
					}), cmb.SystemID, !flag2);
				}
				this._app.UI.SetVisible(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"pendingBG"
				}), !flag);
				this._app.UI.SetVisible(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"completeBG"
				}), flag);
				bool isMultiplayer = this._app.GameSetup.IsMultiplayer;
				this._app.UI.SetVisible(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"simWidget"
				}), false);
				string text = "PENDING";
				if (this._app.CurrentState == this._app.GetGameState<CommonCombatState>() || this._app.CurrentState == this._app.GetGameState<SimCombatState>())
				{
					CommonCombatState commonCombatState = (CommonCombatState)this._app.CurrentState;
					if (commonCombatState != null && commonCombatState.GetCombatID() == cmb.ConflictID)
					{
						if (commonCombatState.PlayersInCombat.Any((Player x) => x.ID == this._app.LocalPlayer.ID))
						{
							text = "RESOLVING";
						}
						this._app.UI.SetVisible(this._app.UI.Path(new string[]
						{
							itemGlobalID,
							"simWidget"
						}), true);
					}
				}
				if (isMultiplayer)
				{
					text = "RESOLVING";
				}
				if (flag)
				{
					text = "RESOLVED";
				}
				this._app.UI.SetText(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"reolvetext"
				}), text);
				this._app.UI.SetText(this._app.UI.Path(new string[]
				{
					base.ID,
					"combatsremaininglbl"
				}), (
					from x in this._listedcombats
					where !x.complete
					select x).Count<PendingCombat>().ToString() + (((
					from x in this._listedcombats
					where !x.complete
					select x).Count<PendingCombat>() != 1) ? " Combats" : " Combat") + " Pending");
			}
		}
        private void SyncSystemOwnershipEffect(string itemID, int systemid, bool cloaksystem)
        {
            Func<HomeworldInfo, bool> predicate = null;
            StarSystemInfo starSystemInfo = base._app.GameDatabase.GetStarSystemInfo(systemid);
            if ((starSystemInfo == null) || starSystemInfo.IsDeepSpace)
            {
                string globalID = base._app.UI.GetGlobalID(base._app.UI.Path(new string[] { itemID, "systemDeepspace" }));
                base._app.UI.SetVisible(globalID, true);
                base._app.UI.SetText(base._app.UI.Path(new string[] { itemID, "title" }), cloaksystem ? "Unknown" : starSystemInfo.Name);
            }
            else
            {
                this._systemWidgets.Add(new SystemWidget(base._app, itemID));
                this._systemWidgets.Last<SystemWidget>().Sync(systemid);
                if (cloaksystem)
                {
                    base._app.UI.SetText(base._app.UI.Path(new string[] { itemID, "title" }), "Unknown");
                }
                if (predicate == null)
                {
                    predicate = x => x.SystemID == systemid;
                }
                HomeworldInfo info2 = base._app.GameDatabase.GetHomeworlds().ToList<HomeworldInfo>().FirstOrDefault<HomeworldInfo>(predicate);
                int? systemOwningPlayer = base._app.GameDatabase.GetSystemOwningPlayer(systemid);
                PlayerInfo Owner = base._app.GameDatabase.GetPlayerInfo(systemOwningPlayer.HasValue ? systemOwningPlayer.Value : 0);
                if (((info2 != null) && (info2.SystemID != 0)) && !cloaksystem)
                {
                    string panelId = base._app.UI.GetGlobalID(base._app.UI.Path(new string[] { itemID, "systemHome" }));
                    base._app.UI.SetVisible(panelId, true);
                    base._app.UI.SetPropertyColor(panelId, "color", (Vector3)(base._app.GameDatabase.GetPlayerInfo(info2.PlayerID).PrimaryColor * 255f));
                }
                else if ((!cloaksystem && (Owner != null)) && base._app.GameDatabase.GetProvinceInfos().Where<ProvinceInfo>(delegate(ProvinceInfo x)
                {
                    if ((x.CapitalSystemID != systemid) || (x.PlayerID != Owner.ID))
                    {
                        return false;
                    }
                    int capitalSystemID = x.CapitalSystemID;
                    int? homeworld = Owner.Homeworld;
                    if (capitalSystemID == homeworld.GetValueOrDefault())
                    {
                        return !homeworld.HasValue;
                    }
                    return true;
                }).Any<ProvinceInfo>())
                {
                    string str3 = base._app.UI.GetGlobalID(base._app.UI.Path(new string[] { itemID, "systemCapital" }));
                    base._app.UI.SetVisible(str3, true);
                    base._app.UI.SetPropertyColor(str3, "color", (Vector3)(Owner.PrimaryColor * 255f));
                }
                else
                {
                    string str4 = base._app.UI.GetGlobalID(base._app.UI.Path(new string[] { itemID, "systemOwnership" }));
                    base._app.UI.SetVisible(str4, true);
                    if ((Owner != null) && !cloaksystem)
                    {
                        base._app.UI.SetPropertyColor(str4, "color", (Vector3)(Owner.PrimaryColor * 255f));
                    }
                }
            }
        }
        public override void Initialize()
		{
			this._app.UI.SetListCleanClear(this._app.UI.Path(new string[]
			{
				base.ID,
				"contentList"
			}), true);
		}
		protected override void OnUpdate()
		{
			this.UpdateCombatList();
			foreach (SystemWidget current in this._systemWidgets)
			{
				current.Update();
			}
		}
        protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
        {
            bool flag1 = msgType == "button_clicked";
        }
        public override string[] CloseDialog()
		{
			foreach (SystemWidget current in this._systemWidgets)
			{
				current.Terminate();
			}
			return null;
		}
	}
}
