using Kerberos.Sots.Data;
using Kerberos.Sots.Data.TechnologyFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.UI
{
	internal class LimitationTreatyBrokenDialog : Dialog
	{
		private const string IncentiveList = "lstIncentives";
		private const string ConsequencesList = "lstLimitationTreatyConsequences";
		private const string AcceptButton = "btnAccept";
		private const string DeclineButton = "btnDecline";
		private bool _isVictim;
		private TreatyInfo _treatyInfo;
		private Dictionary<DiplomacyState, string> _diplomacyTypeLocMap = new Dictionary<DiplomacyState, string>
		{

			{
				DiplomacyState.ALLIED,
				"@UI_DIPLOMACY_STATE_ALLIED"
			},

			{
				DiplomacyState.CEASE_FIRE,
				"@UI_DIPLOMACY_STATE_CEASE_FIRE"
			},

			{
				DiplomacyState.NEUTRAL,
				"@UI_DIPLOMACY_STATE_NEUTRAL"
			},

			{
				DiplomacyState.NON_AGGRESSION,
				"@UI_DIPLOMACY_STATE_NON_AGGRESSION"
			},

			{
				DiplomacyState.PEACE,
				"@UI_DIPLOMACY_STATE_PEACE"
			},

			{
				DiplomacyState.WAR,
				"@UI_DIPLOMACY_STATE_WAR"
			}
		};
		private Dictionary<TreatyType, string> _treatyPanelMap = new Dictionary<TreatyType, string>
		{

			{
				TreatyType.Armistice,
				"pnlArmisticeTreaty"
			},

			{
				TreatyType.Limitation,
				"pnlLimitationTreaty"
			},

			{
				TreatyType.Trade,
				"pnlTradeTreaty"
			},

			{
				TreatyType.Incorporate,
				"pnlIncorporateTreaty"
			},

			{
				TreatyType.Protectorate,
				"pnlProtectorateTreaty"
			}
		};
		private Dictionary<ShipClass, string> _shipClassLocStringMap = new Dictionary<ShipClass, string>
		{

			{
				ShipClass.BattleRider,
				"@SHIPCLASSES_BATTLE_RIDER"
			},

			{
				ShipClass.Cruiser,
				"@SHIPCLASSES_CRUISER"
			},

			{
				ShipClass.Dreadnought,
				"@SHIPCLASSES_DREADNOUGHT"
			},

			{
				ShipClass.Leviathan,
				"@SHIPCLASSES_LEVIATHAN"
			}
		};
		private Dictionary<StationType, string> _stationTypeLocStringMap = new Dictionary<StationType, string>
		{

			{
				StationType.CIVILIAN,
				"@STATION_TYPE_CIVILIAN"
			},

			{
				StationType.DEFENCE,
				"@STATION_TYPE_DEFENCE"
			},

			{
				StationType.DIPLOMATIC,
				"@STATION_TYPE_DIPLOMATIC"
			},

			{
				StationType.GATE,
				"@STATION_TYPE_GATE"
			},

			{
				StationType.MINING,
				"@STATION_TYPE_MINING"
			},

			{
				StationType.NAVAL,
				"@STATION_TYPE_NAVAL"
			},

			{
				StationType.SCIENCE,
				"@STATION_TYPE_SCIENCE"
			}
		};
		public LimitationTreatyBrokenDialog(App game, int treatyId, bool isVictim) : base(game, "dialogTreatyBroken_Limitation")
		{
			List<TreatyInfo> source = game.GameDatabase.GetTreatyInfos().ToList<TreatyInfo>();
			this._treatyInfo = source.FirstOrDefault((TreatyInfo x) => x.ID == treatyId);
			this._isVictim = isVictim;
			if (this._treatyInfo == null)
			{
				game.UI.CloseDialog(this, true);
			}
		}
		public override void Initialize()
		{
			foreach (KeyValuePair<TreatyType, string> current in this._treatyPanelMap)
			{
				this._app.UI.SetVisible(current.Value, false);
			}
			this._app.UI.SetVisible(this._treatyPanelMap[TreatyType.Limitation], true);
			PlayerInfo playerInfo = this._app.GameDatabase.GetPlayerInfo(this._treatyInfo.InitiatingPlayerId);
			this._app.UI.SetPropertyString(base.UI.Path(new string[]
			{
				base.ID,
				"lblIncentives"
			}), "text", App.Localize("@UI_TREATY_REQUEST_INCENTIVES"));
			PlayerInfo playerInfo2 = this._app.GameDatabase.GetPlayerInfo(this._treatyInfo.ReceivingPlayerId);
			DiplomacyCard diplomacyCard = new DiplomacyCard(this._app, (playerInfo.ID != this._app.LocalPlayer.ID) ? playerInfo.ID : playerInfo2.ID, base.UI, "playerdiplocard1");
			diplomacyCard.Initialize();
			this._app.UI.ClearItems("lstIncentives");
			foreach (TreatyIncentiveInfo current2 in this._treatyInfo.Incentives)
			{
				this._app.UI.AddItem("lstIncentives", string.Empty, current2.ID, string.Format("{0} {1}", App.Localize(TreatyEditDialog.IncentiveTypeLocMap[current2.Type]), current2.IncentiveValue));
			}
			switch (this._treatyInfo.Type)
			{
			case TreatyType.Armistice:
				this._app.UI.SetPropertyString(base.UI.Path(new string[]
				{
					base.ID,
					"lblArmisticeTreatyDesc"
				}), "text", string.Format(App.Localize("@UI_TREATY_REQUEST_ARMISTICE_DESC"), App.Localize(this._diplomacyTypeLocMap[((ArmisticeTreatyInfo)this._treatyInfo).SuggestedDiplomacyState]), playerInfo.Name));
				break;
			case TreatyType.Trade:
				this._app.UI.SetPropertyString(base.UI.Path(new string[]
				{
					base.ID,
					"lblTradeTreatyDesc"
				}), "text", string.Format(App.Localize("@UI_TREATY_REQUEST_TRADE_DESC"), this._treatyInfo.Duration, playerInfo.Name));
				break;
			case TreatyType.Limitation:
			{
				LimitationTreatyInfo lti = (LimitationTreatyInfo)this._treatyInfo;
				this._app.UI.ClearItems("lstLimitationTreatyConsequences");
				foreach (TreatyConsequenceInfo current3 in lti.Consequences)
				{
					this._app.UI.AddItem("lstLimitationTreatyConsequences", string.Empty, current3.ID, string.Format("{0} {1}", App.Localize(TreatyEditDialog.ConsequenceTypeLocMap[current3.Type]), current3.ConsequenceValue));
				}
				switch (lti.LimitationType)
				{
				case LimitationTreatyType.FleetSize:
					this._app.UI.SetPropertyString(base.UI.Path(new string[]
					{
						base.ID,
						"lblLimitationTreatyDesc"
					}), "text", string.Format(App.Localize("@UI_TREATY_REQUEST_LIMITATION_BROKEN_FLEETSIZE"), lti.Duration, lti.LimitationAmount, playerInfo2.Name));
					break;
				case LimitationTreatyType.ShipClass:
				{
					string arg = App.Localize(this._shipClassLocStringMap[(ShipClass)int.Parse(lti.LimitationGroup)]);
					this._app.UI.SetPropertyString(base.UI.Path(new string[]
					{
						base.ID,
						"lblLimitationTreatyDesc"
					}), "text", string.Format(App.Localize("@UI_TREATY_REQUEST_LIMITATION_BROKEN_SHIPCLASS"), new object[]
					{
						lti.Duration,
						lti.LimitationGroup,
						lti.LimitationAmount,
						playerInfo2.Name
					}));
					break;
				}
				case LimitationTreatyType.Weapon:
				{
					string arg = this._app.AssetDatabase.Weapons.First((LogicalWeapon x) => x.UniqueWeaponID == int.Parse(lti.LimitationGroup)).Name;
					this._app.UI.SetPropertyString(base.UI.Path(new string[]
					{
						base.ID,
						"lblLimitationTreatyDesc"
					}), "text", string.Format(App.Localize("@UI_TREATY_REQUEST_LIMITATION_BROKEN_WEAPON"), lti.Duration, lti.LimitationGroup, playerInfo2.Name));
					break;
				}
				case LimitationTreatyType.ResearchTree:
				{
					string arg = this._app.AssetDatabase.MasterTechTree.TechFamilies.First((TechFamily x) => x.Id == lti.LimitationGroup).Name;
					this._app.UI.SetPropertyString(base.UI.Path(new string[]
					{
						base.ID,
						"lblLimitationTreatyDesc"
					}), "text", string.Format(App.Localize("@UI_TREATY_REQUEST_LIMITATION_BROKEN_RESEARCHTREE"), lti.Duration, arg, playerInfo2.Name));
					break;
				}
				case LimitationTreatyType.ResearchTech:
				{
					string arg = this._app.AssetDatabase.MasterTechTree.Technologies.First((Tech x) => x.Id == lti.LimitationGroup).Name;
					this._app.UI.SetPropertyString(base.UI.Path(new string[]
					{
						base.ID,
						"lblLimitationTreatyDesc"
					}), "text", string.Format(App.Localize("@UI_TREATY_REQUEST_LIMITATION_BROKEN_RESEARCHTECH"), lti.Duration, arg, playerInfo2.Name));
					break;
				}
				case LimitationTreatyType.EmpireSize:
					this._app.UI.SetPropertyString(base.UI.Path(new string[]
					{
						base.ID,
						"lblLimitationTreatyDesc"
					}), "text", string.Format(App.Localize("@UI_TREATY_REQUEST_LIMITATION_BROKEN_EMPIRESIZE"), lti.Duration, lti.LimitationAmount, playerInfo2.Name));
					break;
				case LimitationTreatyType.ForgeGemWorlds:
					this._app.UI.SetPropertyString(base.UI.Path(new string[]
					{
						base.ID,
						"lblLimitationTreatyDesc"
					}), "text", string.Format(App.Localize("@UI_TREATY_REQUEST_LIMITATION_BROKEN_FORGEGEMWORLDS"), lti.Duration, lti.LimitationAmount, playerInfo2.Name));
					break;
				case LimitationTreatyType.StationType:
				{
					string arg = App.Localize(this._stationTypeLocStringMap[(StationType)int.Parse(lti.LimitationGroup)]);
					this._app.UI.SetPropertyString(base.UI.Path(new string[]
					{
						base.ID,
						"lblLimitationTreatyDesc"
					}), "text", string.Format(App.Localize("@UI_TREATY_REQUEST_LIMITATION_BROKEN_STATIONTYPE"), new object[]
					{
						lti.Duration,
						lti.LimitationGroup,
						lti.LimitationAmount,
						playerInfo2.Name
					}));
					break;
				}
				}
				break;
			}
			}
			if (!this._isVictim)
			{
				this._app.UI.SetPropertyString(base.UI.Path(new string[]
				{
					base.ID,
					"lblLimitationTreatyConsequences"
				}), "text", string.Format(App.Localize("@UI_TREATY_BROKEN_VICTIM"), playerInfo2.Name));
				return;
			}
			this._app.UI.SetPropertyString(base.UI.Path(new string[]
			{
				base.ID,
				"lblLimitationTreatyConsequences"
			}), "text", string.Format(App.Localize("@UI_TREATY_BROKEN_OFFENDER"), playerInfo.Name));
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked" && panelName == "btnAccept")
			{
				this._app.UI.CloseDialog(this, true);
			}
		}
		public override string[] CloseDialog()
		{
			return null;
		}
	}
}
