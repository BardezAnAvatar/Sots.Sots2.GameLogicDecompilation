using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.UI
{
	internal class DialogLoaFleetSelector : Dialog
	{
		private static string UIProfileList = "gameProfileList";
		private static string UIShipList = "gameWorkingFleet";
		private static string UICreateCompositionBtn = "createButton";
		private static string SelectCompositionBtn = "okButton";
		private static string CancelBtn = "cancelButton";
		private static string UIBRAmount = "brAmount";
		private static string UICRAmount = "crAmount";
		private static string UIDNAmount = "dnAmount";
		private static string UILVAmount = "lvAmount";
		private static string UICPAmount = "cpAmount";
		private static string UIConstrictionPoints = "constructionAmount";
		private string CreateCompoDialog;
		private string ConfirmDeleteCompo;
		private int CompoToDelete = -1;
		private bool _forceSelection;
		private int? selectedcompo = null;
		private MissionType _mission;
		private FleetInfo _basefleet;
		private static int designlistid = 0;
		public DialogLoaFleetSelector(App app, MissionType mission, FleetInfo basefleet, bool ForceSelection = false) : base(app, "dialogLoaFleetSelector")
		{
			this._mission = mission;
			this._basefleet = basefleet;
			this._forceSelection = ForceSelection;
		}
		public override void Initialize()
		{
			this.SyncDesignList();
			this._app.UI.SetEnabled(this._app.UI.Path(new string[]
			{
				base.ID,
				DialogLoaFleetSelector.CancelBtn
			}), !this._forceSelection);
		}
		protected void SyncDesignList()
		{
			List<LoaFleetComposition> list = (
				from x in this._app.GameDatabase.GetLoaFleetCompositions()
				where x.PlayerID == this._app.LocalPlayer.ID
				select x).ToList<LoaFleetComposition>();
			if (this._mission == MissionType.CONSTRUCT_STN || this._mission == MissionType.UPGRADE_STN || this._mission == MissionType.SPECIAL_CONSTRUCT_STN)
			{
				list = (
					from x in list
					where x.designs.Any((LoaFleetShipDef j) => this._app.GameDatabase.GetDesignInfo(j.DesignID).DesignSections.Any((DesignSectionInfo i) => i.ShipSectionAsset.ConstructionPoints > 0))
					select x).ToList<LoaFleetComposition>();
			}
			if (this._mission == MissionType.COLONIZATION || this._mission == MissionType.SUPPORT || this._mission == MissionType.EVACUATE)
			{
				list = (
					from x in list
					where x.designs.Any((LoaFleetShipDef j) => this._app.GameDatabase.GetDesignInfo(j.DesignID).DesignSections.Any((DesignSectionInfo i) => i.ShipSectionAsset.ColonizationSpace > 0))
					select x).ToList<LoaFleetComposition>();
			}
			this._app.UI.ClearItems(this._app.UI.Path(new string[]
			{
				base.ID,
				DialogLoaFleetSelector.UIProfileList
			}));
			bool flag = false;
			foreach (LoaFleetComposition current in list)
			{
				this._app.UI.AddItem(this._app.UI.Path(new string[]
				{
					base.ID,
					DialogLoaFleetSelector.UIProfileList
				}), "", current.ID, current.Name);
				string itemGlobalID = this._app.UI.GetItemGlobalID(this._app.UI.Path(new string[]
				{
					base.ID,
					DialogLoaFleetSelector.UIProfileList
				}), "", current.ID, "");
				if (!flag)
				{
					this._app.UI.SetSelection(this._app.UI.Path(new string[]
					{
						base.ID,
						DialogLoaFleetSelector.UIProfileList
					}), current.ID);
					flag = true;
				}
				this._app.UI.SetText(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"designName"
				}), current.Name);
				this._app.UI.SetVisible(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"designDeleteButton"
				}), true);
				this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"designDeleteButton"
				}), "id", "designDeleteButton|" + current.ID.ToString());
                int fleetLoaCubeValue = Kerberos.Sots.StarFleet.StarFleet.GetFleetLoaCubeValue(this._app.Game, this._basefleet.ID);
				int num = 0;
				foreach (LoaFleetShipDef current2 in current.designs)
				{
					num += this._app.GameDatabase.GetDesignInfo(current2.DesignID).GetPlayerProductionCost(this._app.GameDatabase, current.PlayerID, false, null);
				}
				this._app.UI.SetPropertyColor(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"designName"
				}), "color", (num <= fleetLoaCubeValue) ? new Vector3(255f, 255f, 255f) : new Vector3(255f, 0f, 0f));
			}
		}
		protected void SyncCompoShips(int Compositionid)
		{
			LoaFleetComposition loaFleetComposition = this._app.GameDatabase.GetLoaFleetCompositions().FirstOrDefault((LoaFleetComposition x) => x.ID == Compositionid);
			if (loaFleetComposition != null)
			{
				this.selectedcompo = new int?(Compositionid);
                List<DesignInfo> list = Kerberos.Sots.StarFleet.StarFleet.GetDesignBuildOrderForComposition(this._app.Game, this._basefleet.ID, loaFleetComposition, this._mission).ToList<DesignInfo>();
                int fleetLoaCubeValue = Kerberos.Sots.StarFleet.StarFleet.GetFleetLoaCubeValue(this._app.Game, this._basefleet.ID);
				int num = 0;
				List<DesignInfo> list2 = (
					from X in list
					where X.Class == ShipClass.BattleRider
					select X).ToList<DesignInfo>();
				this._app.UI.ClearItems(this._app.UI.Path(new string[]
				{
					base.ID,
					DialogLoaFleetSelector.UIShipList
				}));
				foreach (DesignInfo current in list)
				{
					if (current.Class != ShipClass.BattleRider && !(current.GetRealShipClass() == RealShipClasses.BoardingPod) && !(current.GetRealShipClass() == RealShipClasses.Drone) && !(current.GetRealShipClass() == RealShipClasses.EscapePod))
					{
						this._app.UI.AddItem(this._app.UI.Path(new string[]
						{
							base.ID,
							DialogLoaFleetSelector.UIShipList
						}), "", DialogLoaFleetSelector.designlistid, current.Name);
						string itemGlobalID = this._app.UI.GetItemGlobalID(this._app.UI.Path(new string[]
						{
							base.ID,
							DialogLoaFleetSelector.UIShipList
						}), "", DialogLoaFleetSelector.designlistid, "");
						this._app.UI.SetText(this._app.UI.Path(new string[]
						{
							itemGlobalID,
							"designName"
						}), current.Name);
						this._app.UI.SetVisible(this._app.UI.Path(new string[]
						{
							itemGlobalID,
							"designDeleteButton"
						}), false);
						if (num + current.GetPlayerProductionCost(this._app.GameDatabase, this._basefleet.PlayerID, false, null) <= fleetLoaCubeValue)
						{
							this._app.UI.SetPropertyColor(this._app.UI.Path(new string[]
							{
								itemGlobalID,
								"designName"
							}), "color", new Vector3(255f, 255f, 255f));
							num += current.GetPlayerProductionCost(this._app.GameDatabase, this._basefleet.PlayerID, false, null);
						}
						else
						{
							this._app.UI.SetPropertyColor(this._app.UI.Path(new string[]
							{
								itemGlobalID,
								"designName"
							}), "color", new Vector3(255f, 0f, 0f));
						}
						DialogLoaFleetSelector.designlistid++;
						List<CarrierWingData> list3 = RiderManager.GetDesignBattleriderWingData(this._app, current).ToList<CarrierWingData>();
						foreach (CarrierWingData wd in list3)
						{
							List<DesignInfo> classriders = (
								from x in list2
								where StrategicAI.BattleRiderMountSet.GetMatchingTurretClass(x) == wd.Class
								select x).ToList<DesignInfo>();
							if (classriders.Any<DesignInfo>() && wd.SlotIndexes.Any<int>())
							{
								BattleRiderTypes SelectedType = (
									from x in classriders
									where classriders.Count((DesignInfo j) => j.ID == x.ID) >= wd.SlotIndexes.Count
									select x).First<DesignInfo>().GetMissionSectionAsset().BattleRiderType;
								DesignInfo designInfo = classriders.FirstOrDefault((DesignInfo x) => x.GetMissionSectionAsset().BattleRiderType == SelectedType && classriders.Count((DesignInfo j) => j.ID == x.ID) >= wd.SlotIndexes.Count);
								foreach (int arg_4E1_0 in wd.SlotIndexes)
								{
									if (designInfo != null)
									{
										this._app.UI.AddItem(this._app.UI.Path(new string[]
										{
											base.ID,
											DialogLoaFleetSelector.UIShipList
										}), "", DialogLoaFleetSelector.designlistid, designInfo.Name);
										itemGlobalID = this._app.UI.GetItemGlobalID(this._app.UI.Path(new string[]
										{
											base.ID,
											DialogLoaFleetSelector.UIShipList
										}), "", DialogLoaFleetSelector.designlistid, "");
										this._app.UI.SetText(this._app.UI.Path(new string[]
										{
											itemGlobalID,
											"designName"
										}), designInfo.Name);
										this._app.UI.SetVisible(this._app.UI.Path(new string[]
										{
											itemGlobalID,
											"designDeleteButton"
										}), false);
										if (num + designInfo.GetPlayerProductionCost(this._app.GameDatabase, this._basefleet.PlayerID, false, null) <= fleetLoaCubeValue)
										{
											this._app.UI.SetPropertyColor(this._app.UI.Path(new string[]
											{
												itemGlobalID,
												"designName"
											}), "color", new Vector3(255f, 255f, 255f));
											num += designInfo.GetPlayerProductionCost(this._app.GameDatabase, this._basefleet.PlayerID, false, null);
										}
										else
										{
											this._app.UI.SetPropertyColor(this._app.UI.Path(new string[]
											{
												itemGlobalID,
												"designName"
											}), "color", new Vector3(255f, 0f, 0f));
										}
										list2.Remove(designInfo);
										DialogLoaFleetSelector.designlistid++;
									}
								}
							}
						}
					}
				}
				this.SyncCompoInfo(loaFleetComposition);
			}
		}
		protected void SyncCompoInfo(LoaFleetComposition composition)
		{
			List<DesignInfo> list = new List<DesignInfo>();
			foreach (int current in 
				from x in composition.designs
				select x.DesignID)
			{
				list.Add(this._app.GameDatabase.GetDesignInfo(current));
			}
			int num = 0;
			int num2 = 0;
			foreach (DesignInfo current2 in list)
			{
				if (!(current2.GetRealShipClass() == RealShipClasses.BoardingPod) && !(current2.GetRealShipClass() == RealShipClasses.Drone) && !(current2.GetRealShipClass() == RealShipClasses.EscapePod))
				{
					DesignSectionInfo[] designSections = current2.DesignSections;
					for (int i = 0; i < designSections.Length; i++)
					{
						DesignSectionInfo info = designSections[i];
						num += RiderManager.GetNumRiderSlots(this._app, info);
					}
					if (current2.Class == ShipClass.BattleRider)
					{
						num2++;
					}
				}
			}
			this._app.UI.SetText(this._app.UI.Path(new string[]
			{
				base.ID,
				DialogLoaFleetSelector.UIBRAmount
			}), num2.ToString() + "/" + num.ToString());
			this._app.UI.SetText(this._app.UI.Path(new string[]
			{
				base.ID,
				"totalShipsAmount"
			}), list.Count<DesignInfo>().ToString());
			this._app.UI.SetText(this._app.UI.Path(new string[]
			{
				base.ID,
				DialogLoaFleetSelector.UICRAmount
			}), list.Count((DesignInfo x) => x.Class == ShipClass.Cruiser).ToString());
			this._app.UI.SetText(this._app.UI.Path(new string[]
			{
				base.ID,
				DialogLoaFleetSelector.UIDNAmount
			}), list.Count((DesignInfo x) => x.Class == ShipClass.Dreadnought).ToString());
			this._app.UI.SetText(this._app.UI.Path(new string[]
			{
				base.ID,
				DialogLoaFleetSelector.UILVAmount
			}), list.Count((DesignInfo x) => x.Class == ShipClass.Leviathan).ToString());
			int num3 = 0;
			int num4 = 0;
			int num5 = 0;
			bool flag = false;
			foreach (DesignInfo current3 in list)
			{
				num4 += current3.CommandPointCost;
				if (current3.GetCommandPoints() > 0 && !flag)
				{
					num5 += this._app.GameDatabase.GetDesignCommandPointQuota(this._app.AssetDatabase, current3.ID);
					flag = true;
				}
				num3 += current3.GetPlayerProductionCost(this._app.GameDatabase, this._basefleet.PlayerID, false, null);
			}
			this._app.UI.SetText(this._app.UI.Path(new string[]
			{
				base.ID,
				DialogLoaFleetSelector.UICPAmount
			}), num4.ToString("N0") + "/" + num5.ToString("N0"));
			this._app.UI.SetText(this._app.UI.Path(new string[]
			{
				base.ID,
				DialogLoaFleetSelector.UIConstrictionPoints
			}), num3.ToString("N0") + "/" + Kerberos.Sots.StarFleet.StarFleet.GetFleetLoaCubeValue(this._app.Game, this._basefleet.ID).ToString("N0"));
		}
		protected override void OnUpdate()
		{
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "list_item_dblclk")
			{
				return;
			}
			if (msgType == "list_sel_changed")
			{
				if (panelName == DialogLoaFleetSelector.UIProfileList)
				{
					int compositionid = int.Parse(msgParams[0]);
					this.SyncCompoShips(compositionid);
					return;
				}
			}
			else
			{
				if (msgType == "button_clicked")
				{
					if (panelName == DialogLoaFleetSelector.UICreateCompositionBtn)
					{
						this.CreateCompoDialog = this._app.UI.CreateDialog(new DialogLoaFleetCompositor(this._app, this._mission), null);
						return;
					}
					if (panelName == DialogLoaFleetSelector.SelectCompositionBtn)
					{
						if (!this.selectedcompo.HasValue)
						{
							return;
						}
						if (this._forceSelection)
						{
							this._app.GameDatabase.UpdateFleetCompositionID(this._basefleet.ID, this.selectedcompo);
						}
						this._app.UI.CloseDialog(this, true);
						return;
					}
					else
					{
						if (panelName == DialogLoaFleetSelector.CancelBtn)
						{
							this.selectedcompo = null;
							this._app.UI.CloseDialog(this, true);
							return;
						}
						if (panelName.StartsWith("designDeleteButton"))
						{
							string[] array = panelName.Split(new char[]
							{
								'|'
							});
							int compid = int.Parse(array[1]);
							LoaFleetComposition loaFleetComposition = this._app.GameDatabase.GetLoaFleetCompositions().FirstOrDefault((LoaFleetComposition x) => x.ID == compid);
							if (loaFleetComposition != null)
							{
								this.CompoToDelete = compid;
								this.ConfirmDeleteCompo = this._app.UI.CreateDialog(new GenericQuestionDialog(this._app, App.Localize("@UI_LOACOMP_CONFIRM_DELETE_TITLE"), string.Format(App.Localize("@UI_LOACOMP_CONFIRM_DELETE_MSG"), loaFleetComposition.Name), "dialogGenericQuestion"), null);
								return;
							}
						}
					}
				}
				else
				{
					if (msgType == "dialog_closed")
					{
						if (panelName == this.CreateCompoDialog)
						{
							this.SyncDesignList();
							return;
						}
						if (panelName == this.ConfirmDeleteCompo)
						{
							if (bool.Parse(msgParams[0]))
							{
								this._app.GameDatabase.DeleteLoaFleetCompositon(this.CompoToDelete);
								this.SyncDesignList();
							}
							this.CompoToDelete = -1;
							this.ConfirmDeleteCompo = null;
						}
					}
				}
			}
		}
		public override string[] CloseDialog()
		{
			return new string[]
			{
				this.selectedcompo.ToString()
			};
		}
	}
}
