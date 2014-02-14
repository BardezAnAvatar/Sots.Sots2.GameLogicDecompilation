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
	internal class TreatyEditDialog : Dialog
	{
		private const string _armisticePanel = "pnlArmisticePanel";
		private const string _durationPanel = "pnlDurationPanel";
		private const string _durationField = "txtDuration";
		private const string _limitationPanel = "pnlLimitationPanel";
		private const string _consequenceList = "lstConsequences";
		private const string _incentivesList = "lstIncentives";
		private const string _armisticeTypeList = "lstArmisticeType";
		private const string _treatyTypeList = "lstTreatyType";
		private const string _limitationTypeList = "lstLimitationType";
		private const string _limitationGroupList = "lstLimitationGroup";
		private const string _limitationValue = "txtLimitationValue";
		private const string _doneButton = "btnDone";
		private const string _cancelButton = "btnCancel";
		private const string _removeConsequenceButton = "btnRemoveConsequence";
		private const string _editConsequenceButton = "btnEditConsequence";
		private const string _addConsequenceButton = "btnAddConsequence";
		private const string _removeIncentiveButton = "btnRemoveIncentive";
		private const string _editIncentiveButton = "btnEditIncentive";
		private const string _addIncentiveButton = "btnAddIncentive";
		private bool hasTreaty;
		private int? _selectedConsequence = null;
		private int? _selectedIncentive = null;
		private string _consequenceDialogId = "";
		private string _incentiveDialogId = "";
		private TreatyInfo _editedTreaty;
		private PlayerInfo _receivingPlayer;
		private ValueBoundSpinner _durationSpinner;
		private ValueBoundSpinner _limitationValueSpinner;
		public static Dictionary<LimitationTreatyType, SpinnerValueDescriptor> LimitationTypeSpinnerDescriptors = new Dictionary<LimitationTreatyType, SpinnerValueDescriptor>
		{

			{
				LimitationTreatyType.FleetSize,
				new SpinnerValueDescriptor
				{
					min = 1.0,
					max = 100.0,
					rateOfChange = 1.0
				}
			},

			{
				LimitationTreatyType.ShipClass,
				new SpinnerValueDescriptor
				{
					min = 1.0,
					max = 100.0,
					rateOfChange = 1.0
				}
			},

			{
				LimitationTreatyType.EmpireSize,
				new SpinnerValueDescriptor
				{
					min = 1.0,
					max = 100.0,
					rateOfChange = 1.0
				}
			},

			{
				LimitationTreatyType.ForgeGemWorlds,
				new SpinnerValueDescriptor
				{
					min = 1.0,
					max = 100.0,
					rateOfChange = 1.0
				}
			},

			{
				LimitationTreatyType.StationType,
				new SpinnerValueDescriptor
				{
					min = 1.0,
					max = 100.0,
					rateOfChange = 1.0
				}
			}
		};
		public static List<LimitationTreatyType> EnableGroupList = new List<LimitationTreatyType>
		{
			LimitationTreatyType.ShipClass,
			LimitationTreatyType.Weapon,
			LimitationTreatyType.ResearchTree,
			LimitationTreatyType.ResearchTech,
			LimitationTreatyType.StationType
		};
		public static List<LimitationTreatyType> EnableValueList = new List<LimitationTreatyType>
		{
			LimitationTreatyType.FleetSize,
			LimitationTreatyType.ShipClass,
			LimitationTreatyType.EmpireSize,
			LimitationTreatyType.ForgeGemWorlds,
			LimitationTreatyType.StationType
		};
		public static Dictionary<ShipClass, string> ShipClassLimitationGroups = new Dictionary<ShipClass, string>
		{

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
		public static Dictionary<StationType, string> StationTypeLimitationGroups = new Dictionary<StationType, string>
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
		public static Dictionary<TreatyType, string> TreatyTypeLocMap = new Dictionary<TreatyType, string>
		{

			{
				TreatyType.Armistice,
				"@UI_TREATY_ARMISTICE"
			},

			{
				TreatyType.Trade,
				"@UI_TREATY_TRADE"
			},

			{
				TreatyType.Limitation,
				"@UI_TREATY_LIMITATION"
			},

			{
				TreatyType.Incorporate,
				"@UI_TREATY_INCORPORATE"
			},

			{
				TreatyType.Protectorate,
				"@UI_TREATY_PROTECTORATE"
			}
		};
		public static Dictionary<LimitationTreatyType, string> LimitationTreatyTypeLocMap = new Dictionary<LimitationTreatyType, string>
		{

			{
				LimitationTreatyType.EmpireSize,
				"@UI_TREATY_LIMITATION_EMPIRESIZE"
			},

			{
				LimitationTreatyType.FleetSize,
				"@UI_TREATY_LIMITATION_FLEETSIZE"
			},

			{
				LimitationTreatyType.ForgeGemWorlds,
				"@UI_TREATY_LIMITATION_FORGEGEMWORLD"
			},

			{
				LimitationTreatyType.ResearchTech,
				"@UI_TREATY_LIMITATION_RESEARCH"
			},

			{
				LimitationTreatyType.ResearchTree,
				"@UI_TREATY_LIMITATION_RESEARCHTREE"
			},

			{
				LimitationTreatyType.ShipClass,
				"@UI_TREATY_LIMITATION_SHIPCLASS"
			},

			{
				LimitationTreatyType.StationType,
				"@UI_TREATY_LIMITATION_STATIONTYPE"
			},

			{
				LimitationTreatyType.Weapon,
				"@UI_TREATY_LIMITATION_WEAPON"
			}
		};
		public static Dictionary<IncentiveType, string> IncentiveTypeLocMap = new Dictionary<IncentiveType, string>
		{

			{
				IncentiveType.Savings,
				"@UI_TREATY_INCENTIVE_SAVINGS"
			}
		};
		public static Dictionary<ConsequenceType, string> ConsequenceTypeLocMap = new Dictionary<ConsequenceType, string>
		{

			{
				ConsequenceType.DiplomaticPointPenalty,
				"@UI_TREATY_CONSEQUENCE_DIPLOMATIC_POINTS"
			},

			{
				ConsequenceType.DiplomaticStatusPenalty,
				"@UI_TREATY_CONSEQUENCE_DIPLOMATIC_STATUS"
			},

			{
				ConsequenceType.Fine,
				"@UI_TREATY_CONSEQUENCE_FINE"
			},

			{
				ConsequenceType.Sanction,
				"@UI_TREATY_CONSEQUENCE_SANCTION"
			},

			{
				ConsequenceType.Trade,
				"@UI_TREATY_CONSEQUENCE_TRADE"
			},

			{
				ConsequenceType.War,
				"@UI_TREATY_CONSEQUENCE_WAR"
			}
		};
		public static Dictionary<DiplomacyState, string> ArmisticeTypeLocMap = new Dictionary<DiplomacyState, string>
		{

			{
				DiplomacyState.WAR,
				"@UI_DIPLOMACY_STATE_WAR"
			},

			{
				DiplomacyState.CEASE_FIRE,
				"@UI_DIPLOMACY_STATE_CEASE_FIRE"
			},

			{
				DiplomacyState.NON_AGGRESSION,
				"@UI_DIPLOMACY_STATE_NON_AGGRESSION"
			},

			{
				DiplomacyState.NEUTRAL,
				"@UI_DIPLOMACY_STATE_NEUTRAL"
			},

			{
				DiplomacyState.PEACE,
				"@UI_DIPLOMACY_STATE_PEACE"
			},

			{
				DiplomacyState.ALLIED,
				"@UI_DIPLOMACY_STATE_ALLIED"
			}
		};
		private Dictionary<DiplomacyStateChange, int> StateChangeMap
		{
			get
			{
				return this._app.AssetDatabase.DiplomacyStateChangeMap;
			}
		}
		public TreatyEditDialog(App game, TreatyInfo treaty, string template = "TreatyConfigurationPopup") : base(game, template)
		{
			this._editedTreaty = treaty;
		}
		public TreatyEditDialog(App game, int OpposingPlayerId, string template = "TreatyConfigurationPopup") : base(game, template)
		{
			this._editedTreaty = new LimitationTreatyInfo();
			this._editedTreaty.InitiatingPlayerId = this._app.Game.LocalPlayer.ID;
			this._editedTreaty.ReceivingPlayerId = OpposingPlayerId;
			this._editedTreaty.Type = TreatyType.Armistice;
			this._editedTreaty.StartingTurn = this._app.GameDatabase.GetTurnCount() + 1;
		}
		public override void Initialize()
		{
			this.InitializePanel();
			this._receivingPlayer = this._app.GameDatabase.GetPlayerInfo(this._editedTreaty.ReceivingPlayerId);
			DiplomacyUI.SyncPanelColor(this._app, "pnlBackground", this._receivingPlayer.PrimaryColor);
			this.hasTreaty = true;
			this.SyncTreatyEditor();
			this._durationSpinner = new ValueBoundSpinner(base.UI, "spnDuration", 1.0, 500.0, 1.0, 1.0);
			this._limitationValueSpinner = new ValueBoundSpinner(base.UI, "spnLimitationValue", 1.0, 2147483647.0, 1.0, 1.0);
			this._durationSpinner.ValueChanged += new ValueChangedEventHandler(this._durationSpinner_ValueChanged);
			this._limitationValueSpinner.ValueChanged += new ValueChangedEventHandler(this._limitationValueSpinner_ValueChanged);
			if (!this._receivingPlayer.isStandardPlayer)
			{
				this._editedTreaty.Type = TreatyType.Incorporate;
				this.SyncTreatyEditor();
			}
		}
		private void _limitationValueSpinner_ValueChanged(object sender, ValueChangedEventArgs e)
		{
			LimitationTreatyInfo limitationTreatyInfo = (LimitationTreatyInfo)this._editedTreaty;
			limitationTreatyInfo.LimitationAmount = (float)e.NewValue;
			this._app.UI.SetText("txtLimitationValue", limitationTreatyInfo.LimitationAmount.ToString());
		}
		private void _durationSpinner_ValueChanged(object sender, ValueChangedEventArgs e)
		{
			this._editedTreaty.Duration = (int)e.NewValue;
			this._app.UI.SetText("txtDuration", this._editedTreaty.Duration.ToString());
		}
		public void InitializePanel()
		{
			DiplomacyState diplomacyStateBetweenPlayers = this._app.GameDatabase.GetDiplomacyStateBetweenPlayers(this._editedTreaty.InitiatingPlayerId, this._editedTreaty.ReceivingPlayerId);
			this._app.UI.ClearItems("lstLimitationType");
			foreach (LimitationTreatyType limitationTreatyType in Enum.GetValues(typeof(LimitationTreatyType)))
			{
				this._app.UI.AddItem("lstLimitationType", string.Empty, (int)limitationTreatyType, App.Localize(TreatyEditDialog.LimitationTreatyTypeLocMap[limitationTreatyType]));
			}
			this._app.UI.ClearItems("lstArmisticeType");
			List<DiplomacyState> armisticeTypeMoves = this.GetArmisticeTypeMoves(diplomacyStateBetweenPlayers);
			foreach (DiplomacyState current in armisticeTypeMoves)
			{
				this._app.UI.AddItem("lstArmisticeType", string.Empty, (int)current, App.Localize(TreatyEditDialog.ArmisticeTypeLocMap[current]));
			}
		}
		private List<DiplomacyState> GetArmisticeTypeMoves(DiplomacyState currentState)
		{
			List<DiplomacyState> list = new List<DiplomacyState>();
			foreach (DiplomacyStateChange current in this.StateChangeMap.Keys)
			{
				if (current.lower == currentState)
				{
					List<DiplomacyState> armisticeTypeMoves = this.GetArmisticeTypeMoves(current.upper);
					if (!armisticeTypeMoves.Contains(current.upper))
					{
						armisticeTypeMoves.Add(current.upper);
					}
					foreach (DiplomacyState current2 in armisticeTypeMoves)
					{
						if (!list.Contains(current2))
						{
							list.Add(current2);
						}
					}
				}
			}
			return list;
		}
		public void SyncTreatyEditor()
		{
			this._app.UI.ClearItems("lstTreatyType");
			DiplomacyState diplomacyStateBetweenPlayers = this._app.GameDatabase.GetDiplomacyStateBetweenPlayers(this._editedTreaty.InitiatingPlayerId, this._editedTreaty.ReceivingPlayerId);
			if (this._receivingPlayer.isStandardPlayer)
			{
				if (this._durationSpinner != null)
				{
					this._durationSpinner.SetEnabled(true);
				}
				this._app.UI.AddItem("lstTreatyType", string.Empty, 0, App.Localize(TreatyEditDialog.TreatyTypeLocMap[TreatyType.Armistice]));
				if (diplomacyStateBetweenPlayers != DiplomacyState.WAR)
				{
					this._app.UI.AddItem("lstTreatyType", string.Empty, 2, App.Localize(TreatyEditDialog.TreatyTypeLocMap[TreatyType.Limitation]));
				}
				if ((!this._app.AssetDatabase.GetFaction(this._app.GameDatabase.GetPlayerFactionID(this._editedTreaty.InitiatingPlayerId)).IsFactionIndependentTrader() || this._app.GameDatabase.GetPlayerFactionID(this._editedTreaty.InitiatingPlayerId) == this._app.GameDatabase.GetPlayerFactionID(this._editedTreaty.ReceivingPlayerId)) && (diplomacyStateBetweenPlayers == DiplomacyState.NON_AGGRESSION || diplomacyStateBetweenPlayers == DiplomacyState.PEACE || diplomacyStateBetweenPlayers == DiplomacyState.ALLIED))
				{
					this._app.UI.AddItem("lstTreatyType", string.Empty, 1, App.Localize(TreatyEditDialog.TreatyTypeLocMap[TreatyType.Trade]));
				}
			}
			else
			{
				this.hasTreaty = false;
				if (this._durationSpinner != null)
				{
					this._durationSpinner.SetEnabled(false);
				}
				if (this._app.GetStratModifier<bool>(StratModifiers.AllowIncorporate, this._editedTreaty.InitiatingPlayerId))
				{
					this._app.UI.AddItem("lstTreatyType", string.Empty, 4, App.Localize(TreatyEditDialog.TreatyTypeLocMap[TreatyType.Incorporate]));
					this.hasTreaty = true;
				}
				bool arg_21A_0;
				if (this._app.GetStratModifier<bool>(StratModifiers.AllowProtectorate, this._editedTreaty.InitiatingPlayerId))
				{
					arg_21A_0 = !this._app.GameDatabase.GetTreatyInfos().ToList<TreatyInfo>().Any((TreatyInfo x) => x.ReceivingPlayerId == this._editedTreaty.ReceivingPlayerId && x.Active);
				}
				else
				{
					arg_21A_0 = false;
				}
				bool flag = arg_21A_0;
				if (flag)
				{
					this._app.UI.AddItem("lstTreatyType", string.Empty, 3, App.Localize(TreatyEditDialog.TreatyTypeLocMap[TreatyType.Protectorate]));
					this.hasTreaty = true;
				}
				this._app.UI.SetEnabled("btnDone", this.hasTreaty);
			}
			this._app.UI.SetSelection("lstTreatyType", (int)this._editedTreaty.Type);
			this.SyncIncentives();
			this.SyncTreatyTypePanels();
		}
		public void SyncTreatyTypePanels()
		{
			if (this._editedTreaty.Type == TreatyType.Armistice)
			{
				this._app.UI.SetVisible("pnlArmisticePanel", true);
				this._app.UI.SetVisible("pnlDurationPanel", false);
				int receivingPlayerId = this._editedTreaty.ReceivingPlayerId;
				int iD = this._editedTreaty.ID;
				if (!typeof(ArmisticeTreatyInfo).IsAssignableFrom(this._editedTreaty.GetType()))
				{
					this._editedTreaty = new ArmisticeTreatyInfo();
					this._editedTreaty.ID = iD;
					this._editedTreaty.InitiatingPlayerId = this._app.Game.LocalPlayer.ID;
					this._editedTreaty.ReceivingPlayerId = receivingPlayerId;
					this._editedTreaty.Type = TreatyType.Armistice;
					this._editedTreaty.StartingTurn = this._app.GameDatabase.GetTurnCount() + 1;
				}
				ArmisticeTreatyInfo armisticeTreatyInfo = (ArmisticeTreatyInfo)this._editedTreaty;
				this._app.UI.SetEnabled("btnDone", armisticeTreatyInfo.SuggestedDiplomacyState >= DiplomacyState.CEASE_FIRE);
				this._app.UI.SetSelection("lstArmisticeType", (int)armisticeTreatyInfo.SuggestedDiplomacyState);
				return;
			}
			this._app.UI.SetVisible("pnlArmisticePanel", false);
			this._app.UI.SetVisible("pnlDurationPanel", true);
			if (this._durationSpinner != null)
			{
				this._durationSpinner.SetValue((double)this._editedTreaty.Duration);
			}
			this._app.UI.SetPropertyString("txtDuration", "text", this._editedTreaty.Duration.ToString());
			bool flag = this._editedTreaty.Type == TreatyType.Limitation;
			this._app.UI.SetVisible("pnlLimitationPanel", flag);
			if (flag)
			{
				int receivingPlayerId2 = this._editedTreaty.ReceivingPlayerId;
				int iD2 = this._editedTreaty.ID;
				if (!typeof(LimitationTreatyInfo).IsAssignableFrom(this._editedTreaty.GetType()))
				{
					this._editedTreaty = new LimitationTreatyInfo();
					this._editedTreaty.ID = iD2;
					this._editedTreaty.InitiatingPlayerId = this._app.Game.LocalPlayer.ID;
					this._editedTreaty.ReceivingPlayerId = receivingPlayerId2;
					this._editedTreaty.Type = TreatyType.Limitation;
					this._editedTreaty.StartingTurn = this._app.GameDatabase.GetTurnCount() + 1;
				}
				LimitationTreatyInfo limitationTreatyInfo = (LimitationTreatyInfo)this._editedTreaty;
				this._app.UI.SetSelection("lstLimitationType", (int)limitationTreatyInfo.LimitationType);
				this._app.UI.SetEnabled("txtLimitationValue", TreatyEditDialog.EnableValueList.Contains(limitationTreatyInfo.LimitationType));
				this._app.UI.SetEnabled("lstLimitationGroup", TreatyEditDialog.EnableGroupList.Contains(limitationTreatyInfo.LimitationType));
				if (TreatyEditDialog.LimitationTypeSpinnerDescriptors.ContainsKey(limitationTreatyInfo.LimitationType))
				{
					this._limitationValueSpinner.SetValue((double)limitationTreatyInfo.LimitationAmount);
					this._limitationValueSpinner.SetValueDescriptor(TreatyEditDialog.LimitationTypeSpinnerDescriptors[limitationTreatyInfo.LimitationType]);
					limitationTreatyInfo.LimitationAmount = (float)this._limitationValueSpinner.Value;
					this._app.UI.SetPropertyString("txtLimitationValue", "text", limitationTreatyInfo.LimitationAmount.ToString());
				}
				this.SyncConsequences();
				this.SyncLimitationGroup(limitationTreatyInfo.LimitationType);
			}
		}
		private void SyncLimitationGroup(LimitationTreatyType ltt)
		{
			this._app.UI.ClearItems("lstLimitationGroup");
			switch (ltt)
			{
			case LimitationTreatyType.FleetSize:
			case LimitationTreatyType.EmpireSize:
			case LimitationTreatyType.ForgeGemWorlds:
				return;
			case LimitationTreatyType.ShipClass:
				goto IL_193;
			case LimitationTreatyType.Weapon:
				goto IL_250;
			case LimitationTreatyType.ResearchTree:
				break;
			case LimitationTreatyType.ResearchTech:
				using (List<Tech>.Enumerator enumerator = this._app.AssetDatabase.MasterTechTree.Technologies.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Tech current = enumerator.Current;
						int techID = this._app.GameDatabase.GetTechID(current.Id);
						PlayerTechInfo playerTechInfo = this._app.GameDatabase.GetPlayerTechInfo(this._editedTreaty.InitiatingPlayerId, techID);
						PlayerTechInfo playerTechInfo2 = this._app.GameDatabase.GetPlayerTechInfo(this._editedTreaty.ReceivingPlayerId, techID);
						if (playerTechInfo != null && playerTechInfo.State != TechStates.Researched && playerTechInfo.State != TechStates.Researching && playerTechInfo2 != null && playerTechInfo2.State != TechStates.Researched && playerTechInfo2.State != TechStates.Researching)
						{
							this._app.UI.AddItem("lstLimitationGroup", string.Empty, techID, current.Name);
						}
					}
					return;
				}
				break;
			case LimitationTreatyType.StationType:
				goto IL_1F3;
			default:
				return;
			}
			int num = 0;
			using (List<TechFamily>.Enumerator enumerator2 = this._app.AssetDatabase.MasterTechTree.TechFamilies.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					TechFamily current2 = enumerator2.Current;
					this._app.UI.AddItem("lstLimitationGroup", string.Empty, num, current2.Name);
					num++;
				}
				return;
			}
			IL_193:
			using (Dictionary<ShipClass, string>.Enumerator enumerator3 = TreatyEditDialog.ShipClassLimitationGroups.GetEnumerator())
			{
				while (enumerator3.MoveNext())
				{
					KeyValuePair<ShipClass, string> current3 = enumerator3.Current;
					this._app.UI.AddItem("lstLimitationGroup", string.Empty, (int)current3.Key, App.Localize(current3.Value));
				}
				return;
			}
			IL_1F3:
			using (Dictionary<StationType, string>.Enumerator enumerator4 = TreatyEditDialog.StationTypeLimitationGroups.GetEnumerator())
			{
				while (enumerator4.MoveNext())
				{
					KeyValuePair<StationType, string> current4 = enumerator4.Current;
					this._app.UI.AddItem("lstLimitationGroup", string.Empty, (int)current4.Key, App.Localize(current4.Value));
				}
				return;
			}
			IL_250:
			foreach (LogicalWeapon current5 in this._app.AssetDatabase.Weapons)
			{
				this._app.UI.AddItem("lstLimitationGroup", string.Empty, current5.UniqueWeaponID, current5.WeaponName);
			}
		}
		private void SyncConsequences()
		{
			this._app.UI.ClearItems("lstConsequences");
			for (int i = 0; i < this._editedTreaty.Consequences.Count; i++)
			{
				this._app.UI.AddItem("lstConsequences", string.Empty, i, string.Empty);
				string itemGlobalID = this._app.UI.GetItemGlobalID("lstConsequences", string.Empty, i, string.Empty);
				this._app.UI.SetText(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"lblHeader"
				}), App.Localize(TreatyEditDialog.ConsequenceTypeLocMap[this._editedTreaty.Consequences[i].Type]));
				this._app.UI.SetText(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"lblValue"
				}), this._editedTreaty.Consequences[i].ConsequenceValue.ToString());
			}
			if (this._selectedConsequence.HasValue)
			{
				this._app.UI.SetSelection("lstConsequences", this._selectedConsequence.Value);
			}
		}
		private void SyncIncentives()
		{
			this._app.UI.ClearItems("lstIncentives");
			for (int i = 0; i < this._editedTreaty.Incentives.Count; i++)
			{
				if (this._editedTreaty.Incentives[i].Type != IncentiveType.Savings || this._app.LocalPlayer.PlayerInfo.CanDebtSpend(this._app.AssetDatabase))
				{
					this._app.UI.AddItem("lstIncentives", string.Empty, i, string.Empty);
					string itemGlobalID = this._app.UI.GetItemGlobalID("lstIncentives", string.Empty, i, string.Empty);
					this._app.UI.SetText(this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"lblHeader"
					}), App.Localize(TreatyEditDialog.IncentiveTypeLocMap[this._editedTreaty.Incentives[i].Type]));
					this._app.UI.SetText(this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"lblValue"
					}), this._editedTreaty.Incentives[i].IncentiveValue.ToString());
				}
			}
			if (this._selectedIncentive.HasValue)
			{
				this._app.UI.SetSelection("lstIncentives", this._selectedIncentive.Value);
			}
		}
		private void SyncRDPCost(TreatyInfo ti)
		{
			int treatyRdpCost = this._app.Game.GetTreatyRdpCost(ti);
			this._app.UI.SetButtonText("btnDone", string.Format("{0} ({1}: {2})", App.Localize("@UI_GENERAL_DONE"), App.Localize("@DIPLOMACY_RDP"), treatyRdpCost));
			bool flag = true;
			if (this._editedTreaty.Type == TreatyType.Armistice)
			{
				ArmisticeTreatyInfo armisticeTreatyInfo = (ArmisticeTreatyInfo)this._editedTreaty;
				flag = (armisticeTreatyInfo.SuggestedDiplomacyState >= DiplomacyState.CEASE_FIRE);
			}
			this._app.UI.SetEnabled("btnDone", treatyRdpCost <= this._app.GameDatabase.GetPlayerInfo(this._app.LocalPlayer.ID).GetTotalDiplomacyPoints(this._app.GameDatabase.GetPlayerFactionID(ti.ReceivingPlayerId)) && this.hasTreaty && flag);
		}
		protected override void OnPanelMessage(string panelId, string msgType, string[] msgParams)
		{
			if (this._durationSpinner.TryPanelMessage(panelId, msgType, msgParams, PanelBinding.PanelMessageTargetFlags.Self | PanelBinding.PanelMessageTargetFlags.Recursive))
			{
				return;
			}
			if (this._limitationValueSpinner.TryPanelMessage(panelId, msgType, msgParams, PanelBinding.PanelMessageTargetFlags.Self | PanelBinding.PanelMessageTargetFlags.Recursive))
			{
				return;
			}
			if (msgType == "list_sel_changed")
			{
				if (panelId == "lstTreatyType")
				{
					this._editedTreaty.Type = (TreatyType)int.Parse(msgParams[0]);
					this.SyncTreatyTypePanels();
					this.SyncRDPCost(this._editedTreaty);
				}
				else
				{
					if (panelId == "lstLimitationType")
					{
						LimitationTreatyInfo limitationTreatyInfo = (LimitationTreatyInfo)this._editedTreaty;
						limitationTreatyInfo.LimitationType = (LimitationTreatyType)int.Parse(msgParams[0]);
						if (TreatyEditDialog.EnableValueList.Contains(limitationTreatyInfo.LimitationType))
						{
							this._limitationValueSpinner.SetValueDescriptor(TreatyEditDialog.LimitationTypeSpinnerDescriptors[limitationTreatyInfo.LimitationType]);
							limitationTreatyInfo.LimitationAmount = (float)this._limitationValueSpinner.Value;
							this._app.UI.SetPropertyString("txtLimitationValue", "text", limitationTreatyInfo.LimitationAmount.ToString());
						}
						this.SyncLimitationGroup(limitationTreatyInfo.LimitationType);
						this._app.UI.ClearSelection("lstLimitationGroup");
						this.SyncRDPCost(limitationTreatyInfo);
					}
					else
					{
						if (panelId == "lstArmisticeType")
						{
							ArmisticeTreatyInfo armisticeTreatyInfo = (ArmisticeTreatyInfo)this._editedTreaty;
							armisticeTreatyInfo.SuggestedDiplomacyState = (DiplomacyState)int.Parse(msgParams[0]);
							this.SyncRDPCost(armisticeTreatyInfo);
						}
						else
						{
							if (panelId == "lstLimitationGroup")
							{
								if (int.Parse(msgParams[0]) == -1)
								{
									return;
								}
								LimitationTreatyInfo limitationTreatyInfo2 = (LimitationTreatyInfo)this._editedTreaty;
								switch (limitationTreatyInfo2.LimitationType)
								{
								case LimitationTreatyType.ShipClass:
									limitationTreatyInfo2.LimitationGroup = msgParams[0];
									break;
								case LimitationTreatyType.Weapon:
									limitationTreatyInfo2.LimitationGroup = msgParams[0];
									break;
								case LimitationTreatyType.ResearchTree:
									limitationTreatyInfo2.LimitationGroup = this._app.AssetDatabase.MasterTechTree.TechFamilies[int.Parse(msgParams[0])].Id;
									break;
								case LimitationTreatyType.ResearchTech:
									limitationTreatyInfo2.LimitationGroup = msgParams[0];
									break;
								case LimitationTreatyType.StationType:
									limitationTreatyInfo2.LimitationGroup = msgParams[0];
									break;
								}
							}
							else
							{
								if (panelId == "lstConsequences")
								{
									if (!string.IsNullOrEmpty(msgParams[0]))
									{
										this._selectedConsequence = new int?(int.Parse(msgParams[0]));
									}
									else
									{
										this._selectedConsequence = null;
									}
								}
								else
								{
									if (panelId == "lstIncentives")
									{
										if (!string.IsNullOrEmpty(msgParams[0]))
										{
											this._selectedIncentive = new int?(int.Parse(msgParams[0]));
										}
										else
										{
											this._selectedIncentive = null;
										}
									}
								}
							}
						}
					}
				}
			}
			else
			{
				if (msgType == "button_clicked")
				{
					if (panelId == "btnCancel")
					{
						this._app.UI.CloseDialog(this, true);
					}
					if (panelId == "btnDone")
					{
						this._app.GameDatabase.SpendDiplomacyPoints(this._app.GameDatabase.GetPlayerInfo(this._editedTreaty.InitiatingPlayerId), this._app.GameDatabase.GetPlayerFactionID(this._editedTreaty.ReceivingPlayerId), this._app.Game.GetTreatyRdpCost(this._editedTreaty));
						this._app.GameDatabase.DeleteTreatyInfo(this._editedTreaty.ID);
						this._app.GameDatabase.InsertTreaty(this._editedTreaty);
						this._app.UI.CloseDialog(this, true);
					}
					else
					{
						if (panelId == "btnRemoveConsequence")
						{
							if (this._selectedConsequence.HasValue)
							{
								this._editedTreaty.Consequences.RemoveAt(this._selectedConsequence.Value);
								this.SyncConsequences();
							}
						}
						else
						{
							if (panelId == "btnEditConsequence")
							{
								if (this._selectedConsequence.HasValue)
								{
									TreatyConsequenceInfo treatyConsequenceInfo = this._editedTreaty.Consequences[this._selectedConsequence.Value];
									this._consequenceDialogId = this._app.UI.CreateDialog(new ConsequenceEditDialog(this._app, ref treatyConsequenceInfo, this._receivingPlayer.PrimaryColor, "TreatyConsequencePopup"), null);
								}
							}
							else
							{
								if (panelId == "btnAddConsequence")
								{
									TreatyConsequenceInfo treatyConsequenceInfo2 = new TreatyConsequenceInfo();
									treatyConsequenceInfo2.TreatyId = this._editedTreaty.ID;
									this._editedTreaty.Consequences.Add(treatyConsequenceInfo2);
									this._consequenceDialogId = this._app.UI.CreateDialog(new ConsequenceEditDialog(this._app, ref treatyConsequenceInfo2, this._receivingPlayer.PrimaryColor, "TreatyConsequencePopup"), null);
								}
								else
								{
									if (panelId == "btnRemoveIncentive")
									{
										if (this._selectedIncentive.HasValue)
										{
											this._editedTreaty.Incentives.RemoveAt(this._selectedIncentive.Value);
											this.SyncIncentives();
										}
									}
									else
									{
										if (panelId == "btnEditIncentive")
										{
											if (this._selectedIncentive.HasValue)
											{
												TreatyIncentiveInfo treatyIncentiveInfo = this._editedTreaty.Incentives[this._selectedIncentive.Value];
												this._incentiveDialogId = this._app.UI.CreateDialog(new IncentiveEditDialog(this._app, ref treatyIncentiveInfo, this._receivingPlayer.PrimaryColor, "TreatyConsequencePopup"), null);
											}
										}
										else
										{
											if (panelId == "btnAddIncentive")
											{
												TreatyIncentiveInfo treatyIncentiveInfo2 = new TreatyIncentiveInfo();
												treatyIncentiveInfo2.TreatyId = this._editedTreaty.ID;
												this._editedTreaty.Incentives.Add(treatyIncentiveInfo2);
												this._incentiveDialogId = this._app.UI.CreateDialog(new IncentiveEditDialog(this._app, ref treatyIncentiveInfo2, this._receivingPlayer.PrimaryColor, "TreatyConsequencePopup"), null);
											}
										}
									}
								}
							}
						}
					}
				}
				else
				{
					if (msgType == "dialog_closed")
					{
						if (panelId == this._incentiveDialogId)
						{
							this.SyncIncentives();
						}
						else
						{
							if (panelId == this._consequenceDialogId)
							{
								this.SyncConsequences();
							}
						}
					}
					else
					{
						if (msgType == "text_changed")
						{
							if (panelId == "txtDuration")
							{
								int duration = 0;
								if (int.TryParse(msgParams[0], out duration))
								{
									this._editedTreaty.Duration = duration;
								}
							}
							else
							{
								if (panelId == "txtLimitationValue")
								{
									float limitationAmount = 0f;
									if (float.TryParse(msgParams[0], out limitationAmount))
									{
										((LimitationTreatyInfo)this._editedTreaty).LimitationAmount = limitationAmount;
									}
								}
							}
						}
					}
				}
			}
			base.OnPanelMessage(panelId, msgType, msgParams);
		}
		public override string[] CloseDialog()
		{
			List<string> list = new List<string>();
			return list.ToArray();
		}
	}
}
