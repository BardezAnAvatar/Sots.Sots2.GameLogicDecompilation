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
	internal class LimitationTreatyRequestedDialog : Dialog
	{
		private const string IncentiveList = "lstIncentives";
		private const string ConsequencesList = "lstLimitationTreatyConsequences";
		private const string AcceptButton = "btnAccept";
		private const string DeclineButton = "btnDecline";
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
		public LimitationTreatyRequestedDialog(App game, int treatyId) : base(game, "dialogTreatyRequest_Limitation")
		{
			List<TreatyInfo> source = game.GameDatabase.GetTreatyInfos().ToList<TreatyInfo>();
			this._treatyInfo = source.First((TreatyInfo x) => x.ID == treatyId);
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
			DiplomacyCard diplomacyCard = new DiplomacyCard(this._app, playerInfo.ID, base.UI, "playerdiplocard1");
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
				return;
			case TreatyType.Trade:
				this._app.UI.SetPropertyString(base.UI.Path(new string[]
				{
					base.ID,
					"lblTradeTreatyDesc"
				}), "text", string.Format(App.Localize("@UI_TREATY_REQUEST_TRADE_DESC"), this._treatyInfo.Duration, playerInfo.Name));
				return;
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
					}), "text", string.Format(App.Localize("@UI_TREATY_REQUEST_LIMITATION_FLEETSIZE"), lti.Duration, lti.LimitationAmount, playerInfo.Name));
					return;
				case LimitationTreatyType.ShipClass:
				{
					string arg = App.Localize(this._shipClassLocStringMap[(ShipClass)int.Parse(lti.LimitationGroup)]);
					this._app.UI.SetPropertyString(base.UI.Path(new string[]
					{
						base.ID,
						"lblLimitationTreatyDesc"
					}), "text", string.Format(App.Localize("@UI_TREATY_REQUEST_LIMITATION_SHIPCLASS"), new object[]
					{
						lti.Duration,
						lti.LimitationGroup,
						lti.LimitationAmount,
						playerInfo.Name
					}));
					return;
				}
				case LimitationTreatyType.Weapon:
				{
					string arg = this._app.AssetDatabase.Weapons.First((LogicalWeapon x) => x.UniqueWeaponID == int.Parse(lti.LimitationGroup)).Name;
					this._app.UI.SetPropertyString(base.UI.Path(new string[]
					{
						base.ID,
						"lblLimitationTreatyDesc"
					}), "text", string.Format(App.Localize("@UI_TREATY_REQUEST_LIMITATION_WEAPON"), lti.Duration, lti.LimitationGroup, playerInfo.Name));
					break;
				}
				case LimitationTreatyType.ResearchTree:
				{
					string arg = this._app.AssetDatabase.MasterTechTree.TechFamilies.First((TechFamily x) => x.Id == lti.LimitationGroup).Name;
					this._app.UI.SetPropertyString(base.UI.Path(new string[]
					{
						base.ID,
						"lblLimitationTreatyDesc"
					}), "text", string.Format(App.Localize("@UI_TREATY_REQUEST_LIMITATION_RESEARCHTREE"), lti.Duration, arg, playerInfo.Name));
					return;
				}
				case LimitationTreatyType.ResearchTech:
				{
					string arg = this._app.AssetDatabase.MasterTechTree.Technologies.First((Tech x) => x.Id == lti.LimitationGroup).Name;
					this._app.UI.SetPropertyString(base.UI.Path(new string[]
					{
						base.ID,
						"lblLimitationTreatyDesc"
					}), "text", string.Format(App.Localize("@UI_TREATY_REQUEST_LIMITATION_RESEARCHTECH"), lti.Duration, arg, playerInfo.Name));
					return;
				}
				case LimitationTreatyType.EmpireSize:
					this._app.UI.SetPropertyString(base.UI.Path(new string[]
					{
						base.ID,
						"lblLimitationTreatyDesc"
					}), "text", string.Format(App.Localize("@UI_TREATY_REQUEST_LIMITATION_EMPIRESIZE"), lti.Duration, lti.LimitationAmount, playerInfo.Name));
					return;
				case LimitationTreatyType.ForgeGemWorlds:
					this._app.UI.SetPropertyString(base.UI.Path(new string[]
					{
						base.ID,
						"lblLimitationTreatyDesc"
					}), "text", string.Format(App.Localize("@UI_TREATY_REQUEST_LIMITATION_FORGEGEMWORLDS"), lti.Duration, lti.LimitationAmount, playerInfo.Name));
					return;
				case LimitationTreatyType.StationType:
				{
					string arg = App.Localize(this._stationTypeLocStringMap[(StationType)int.Parse(lti.LimitationGroup)]);
					this._app.UI.SetPropertyString(base.UI.Path(new string[]
					{
						base.ID,
						"lblLimitationTreatyDesc"
					}), "text", string.Format(App.Localize("@UI_TREATY_REQUEST_LIMITATION_STATIONTYPE"), new object[]
					{
						lti.Duration,
						lti.LimitationGroup,
						lti.LimitationAmount,
						playerInfo.Name
					}));
					return;
				}
				default:
					return;
				}
				break;
			}
			case TreatyType.Protectorate:
			case TreatyType.Incorporate:
				break;
			default:
				return;
			}
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (panelName == "btnAccept")
				{
					this._app.Game.AcceptTreaty(this._treatyInfo);
					this._app.UI.CloseDialog(this, true);
					return;
				}
				if (panelName == "btnDecline")
				{
					this._app.Game.DeclineTreaty(this._treatyInfo);
					this._app.UI.CloseDialog(this, true);
				}
			}
		}
		public override string[] CloseDialog()
		{
			return null;
		}
	}
}
