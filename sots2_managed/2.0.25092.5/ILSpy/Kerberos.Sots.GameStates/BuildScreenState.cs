using Kerberos.Sots.Data;
using Kerberos.Sots.Data.ShipFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using Kerberos.Sots.UI;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.GameStates
{
	internal class BuildScreenState : GameState, IKeyBindListener
	{
		public class InvoiceItem
		{
			public int DesignID;
			public int TempOrderID;
			public string ShipName;
			public int Progress;
			public bool isPrototypeOrder;
			public int LoaCubes;
		}
		private const string UIPiechartPanelId = "piechart";
		private ShipHoloView _shipHoloView;
		private GameObjectSet _crits;
		private OrbitCameraController _camera;
		private ShipBuilder _builder;
		private BudgetPiechart _piechart;
		private bool _addToFavorites;
		private int _deleteItemID;
		private string _deleteItemDialog;
		private string _deleteInvoiceDialog;
		private string _confirmDiscardInvoiceDialog;
		private bool _confirmInvoiceDialogActive;
		private int _loacubeval;
		private int _minLoaCubeval = 1000;
		private int _maxLoaCubeval = 10000;
		private int _loaSliderNotch = 10000;
		private float _totalShipProductionRate;
		private string _invoiceName = "";
		private int _selectedOrder;
		private static readonly string UIInvoiceItemList = "lstInvoiceItemList";
		private static readonly string UIBuildOrderList = "lstBuildOrderList";
		private static readonly string UIInvoiceTotalSavings = "lblInvoiceTotalSavings";
		private static readonly string UIInvoiceTotalTurns = "lblInvoiceTotalTurns";
		private static readonly string UIInvoiceList = "gameInvoiceList";
		private static readonly string UIClassList = "gameClassList";
		private static readonly string UIDesignList = "gameDesignList";
		private static readonly string UIPlanetDetails = "gamePlanetDetails";
		private static readonly string UISubmitOrder = "gameSubmitOrder";
		private static readonly string UICloseInvoiceSummary = "btnCloseInvoiceSummary";
		private static readonly string UIRemoveInvoiceItem = "btnRemoveInvoiceItem";
		private static readonly string UIRemoveOrderItem = "btnRemoveOrderItem";
		private static readonly string UIOrderInvoiceItems = "ConstructionOrder";
		private static readonly string UIInvoiceSummaryName = "lblInvoiceName";
		private static readonly string UIBuildOrderPanel = "pnlInvoiceSummary";
		private static readonly string UIExitButton = "gameExitButton";
		private static readonly string UIDesignScreenButton = "gameDesignScreen";
		private static readonly string UISystemMap = "partMiniSystem";
		private static readonly string UICurrentMaintenance = "financeCurrent";
		private static readonly string UIProjectedCost = "financeProjectedCost";
		private static readonly string UITotalMaintenance = "financeTotal";
		private static readonly string UISysName = "sysnameValue";
		private static readonly string UISysProductionValue = "sysproductionValue";
		private static readonly string UISysIncomeValue = "sysincomeValue";
		private static readonly string UICvsTValue = "CvsTValue";
		private static readonly string UISubmitDialogOk = "submit_dialog_ok";
		private static readonly string UISubmitDialogCancel = "submit_dialog_cancel";
		private static readonly string UIInvoiceName = "edit_design_name";
		private static readonly string UILoaDialogOk = "loa_dialog_ok";
		private static readonly string UILoaDialogCancel = "loa_dialog_cancel";
		private static readonly string UIInvoiceRemove = "invoiceRemove";
		private static readonly string UIFavInvoiceRemove = "faveinvoiceRemove";
		private static readonly string UIShipRemove = "shipRemove";
		private static readonly string UIAddToInvoiceFavorites = "addToInvoiceFavorites";
		private static readonly string UIInvoiceSummaryPopup = "pnlInvoiceSummaryPopup";
		private int _selectedSystem;
		private int? _selectedInvoiceItem = null;
		private int? _selectedBuildOrder = null;
		private int? _selectedInvoice = null;
		private int? _selectedFavInvoice = null;
		private RealShipClasses? _selectedClass;
		private List<DesignInfo> _designList = new List<DesignInfo>();
		private List<InvoiceInfo> _invoiceList = new List<InvoiceInfo>();
		private WeaponHoverPanel _weaponTooltip;
		private ModuleHoverPanel _moduleTooltip;
		private int _selectedDesign;
		private int HACK_OrderID;
		private List<BuildScreenState.InvoiceItem> _invoiceItems = new List<BuildScreenState.InvoiceItem>();
		private int SelectedOrder
		{
			get
			{
				return this._selectedOrder;
			}
			set
			{
				if (this._selectedOrder == value)
				{
					return;
				}
				this._selectedOrder = value;
				if (this.SelectedOrder != 0)
				{
					BuildOrderInfo buildOrderInfo = base.App.GameDatabase.GetBuildOrdersForSystem(this._selectedSystem).First((BuildOrderInfo x) => x.ID == this.SelectedOrder);
					DesignInfo designInfo = base.App.GameDatabase.GetDesignInfo(buildOrderInfo.DesignID);
					this.SetSelectedDesign(designInfo.ID, string.Empty);
				}
			}
		}
		private int SelectedSystem
		{
			get
			{
				return this._selectedSystem;
			}
		}
		private RealShipClasses? SelectedClass
		{
			get
			{
				return this._selectedClass;
			}
		}
		private int SelectedDesign
		{
			get
			{
				return this._selectedDesign;
			}
		}
		private void SyncWeaponUi()
		{
		}
		public void SyncSystemDetails()
		{
			int selectedSystem = this._selectedSystem;
			if (selectedSystem == 0)
			{
				return;
			}
			StarSystemInfo starSystemInfo = base.App.GameDatabase.GetStarSystemInfo(selectedSystem);
			base.App.UI.SetPropertyString(BuildScreenState.UISysName, "text", starSystemInfo.Name);
			StarSystemMapUI.Sync(base.App, selectedSystem, BuildScreenState.UISystemMap, false);
		}
		public BuildScreenState(App game) : base(game)
		{
		}
		protected override void OnPrepare(GameState prev, object[] stateParams)
		{
			base.App.UI.LoadScreen("Build");
			if (base.App.LocalPlayer == null)
			{
				base.App.NewGame();
				int? homeworld = base.App.GameDatabase.GetPlayerInfo(base.App.LocalPlayer.ID).Homeworld;
				if (!homeworld.HasValue)
				{
					throw new ArgumentException("Build screen requires a home world.");
				}
				this._selectedSystem = homeworld.Value;
			}
			else
			{
				if (stateParams.Count<object>() > 0)
				{
					this._selectedSystem = (int)stateParams[0];
				}
			}
			this._crits = new GameObjectSet(base.App);
			this._camera = this._crits.Add<OrbitCameraController>(new object[]
			{
				string.Empty
			});
			this._shipHoloView = new ShipHoloView(base.App, this._camera);
			this._crits.Add(this._shipHoloView);
			this._builder = new ShipBuilder(base.App);
		}
		protected override void OnEnter()
		{
			base.App.UI.SetScreen("Build");
			this._piechart = new BudgetPiechart(base.App.UI, "piechart", base.App.AssetDatabase);
			this._confirmInvoiceDialogActive = false;
			base.App.UI.ClearItems(BuildScreenState.UIDesignList);
			base.App.UI.SetPropertyBool(base.App.UI.Path(new string[]
			{
				BuildScreenState.UIPlanetDetails,
				"partTradeSlider"
			}), "only_user_events", true);
			base.App.UI.SetPropertyBool(base.App.UI.Path(new string[]
			{
				BuildScreenState.UIPlanetDetails,
				"partShipConSlider"
			}), "only_user_events", true);
			base.App.UI.Send(new object[]
			{
				"SetGameObject",
				"designShip",
				this._shipHoloView.ObjectID
			});
			EmpireBarUI.SyncTitleFrame(base.App);
			int selectedSystem = this._selectedSystem;
			StarSystemInfo starSystemInfo = base.App.GameDatabase.GetStarSystemInfo(selectedSystem);
			StationInfo stationForSystemPlayerAndType = base.App.GameDatabase.GetStationForSystemPlayerAndType(selectedSystem, base.App.LocalPlayer.ID, StationType.NAVAL);
			string arg = "";
			if (stationForSystemPlayerAndType != null)
			{
				arg = string.Format(", {0}", stationForSystemPlayerAndType.DesignInfo.Name);
			}
			this._camera.Active = true;
			this._camera.MaxDistance = 2000f;
			this._camera.DesiredDistance = 800f;
			this._camera.DesiredYaw = MathHelper.DegreesToRadians(-90f);
			this.SetSelectedSystem(selectedSystem, "init");
			this.PopulateClassList(new RealShipClasses?(RealShipClasses.Cruiser));
			this.PopulateDesignList(this._selectedClass);
			this.SyncConstructionSite(base.App);
			this.SyncFinancialDetails(base.App);
			this.SyncSystemDetails();
			this.PopulateInvoiceList();
			this.SyncInvoiceItemsList(BuildScreenState.UIOrderInvoiceItems, BuildScreenState.UIInvoiceItemList, this._invoiceItems, App.Localize("@UI_BUILD_NEW_INVOICE"), false, 0);
			base.App.UI.ForceLayout(BuildScreenState.UIInvoiceList);
			base.App.UI.SetPropertyString("gameScreenFrame.TopBar.Screen_Title", "text", string.Format("System: {0}{1}", starSystemInfo.Name, arg));
			base.App.UI.AutoSize("gameScreenFrame.TopBar.Screen_Title");
			this._minLoaCubeval = base.App.AssetDatabase.MinLoaCubesOnBuild;
			this._maxLoaCubeval = base.App.AssetDatabase.MaxLoaCubesOnBuild;
			this._loacubeval = this._minLoaCubeval;
			base.App.UI.SetPropertyString(base.App.UI.Path(new string[]
			{
				"LoaCubeDialog",
				"LoaPointSlider",
				"right_label"
			}), "text", this._loacubeval.ToString());
			base.App.UI.SetSliderRange(base.App.UI.Path(new string[]
			{
				"LoaCubeDialog",
				"LoaPointSlider"
			}), this._minLoaCubeval, this._maxLoaCubeval);
			base.App.UI.SetSliderValue(base.App.UI.Path(new string[]
			{
				"LoaCubeDialog",
				"LoaPointSlider"
			}), this._minLoaCubeval);
			base.App.UI.SetSliderTolerance(base.App.UI.Path(new string[]
			{
				"LoaCubeDialog",
				"LoaPointSlider"
			}), 1000);
			for (int i = this._minLoaCubeval; i <= this._maxLoaCubeval; i += this._loaSliderNotch)
			{
				base.App.UI.AddSliderNotch(base.App.UI.Path(new string[]
				{
					"LoaCubeDialog",
					"LoaPointSlider"
				}), i);
			}
			base.App.UI.SetText(base.App.UI.Path(new string[]
			{
				"LoaCubeDialog",
				"LoaPointValue"
			}), this._minLoaCubeval.ToString());
			base.App.UI.SetVisible("Build.loaBuildWarning", base.App.LocalPlayer.Faction.Name == "loa" && base.App.GameDatabase.GetPlayerInfo(base.App.LocalPlayer.ID).Savings < 0.0);
			base.App.HotKeyManager.AddListener(this);
			base.App.UI.SetVisible("title_prev", true);
			base.App.UI.SetVisible("title_next", true);
		}
		protected void SelectNextSystem(bool reverse = false)
		{
			List<int> list = base.App.GameDatabase.GetPlayerColonySystemIDs(base.App.LocalPlayer.ID).ToList<int>();
			list.Sort();
			if (reverse)
			{
				list.Reverse();
			}
			int num;
			if (this._selectedSystem == list.Last<int>())
			{
				num = list.First<int>();
			}
			else
			{
				num = list[list.IndexOf(this._selectedSystem) + 1];
			}
			if (num == -1 || num == this._selectedSystem)
			{
				return;
			}
			this._selectedSystem = num;
			int selectedSystem = this._selectedSystem;
			StarSystemInfo starSystemInfo = base.App.GameDatabase.GetStarSystemInfo(selectedSystem);
			StationInfo stationForSystemPlayerAndType = base.App.GameDatabase.GetStationForSystemPlayerAndType(selectedSystem, base.App.LocalPlayer.ID, StationType.NAVAL);
			string arg = "";
			if (stationForSystemPlayerAndType != null)
			{
				arg = string.Format(", {0}", stationForSystemPlayerAndType.DesignInfo.Name);
			}
			this._camera.Active = true;
			this._camera.MaxDistance = 2000f;
			this._camera.DesiredDistance = 800f;
			this._camera.DesiredYaw = MathHelper.DegreesToRadians(-90f);
			this.SetSelectedSystem(selectedSystem, "init");
			this.PopulateClassList(new RealShipClasses?(RealShipClasses.Cruiser));
			this.PopulateDesignList(this._selectedClass);
			this.SyncConstructionSite(base.App);
			this.SyncFinancialDetails(base.App);
			this.SyncSystemDetails();
			this.PopulateInvoiceList();
			this.SyncInvoiceItemsList(BuildScreenState.UIOrderInvoiceItems, BuildScreenState.UIInvoiceItemList, this._invoiceItems, App.Localize("@UI_BUILD_NEW_INVOICE"), false, 0);
			base.App.UI.ForceLayout(BuildScreenState.UIInvoiceList);
			base.App.UI.SetPropertyString("gameScreenFrame.TopBar.Screen_Title", "text", string.Format("System: {0}{1}", starSystemInfo.Name, arg));
			base.App.UI.AutoSize("gameScreenFrame.TopBar.Screen_Title");
		}
		protected override void OnExit(GameState prev, ExitReason reason)
		{
			base.App.HotKeyManager.RemoveListener(this);
			this._piechart = null;
			this._camera.Active = false;
			this._camera.TargetID = 0;
			this._builder.Dispose();
			if (this._crits != null)
			{
				this._crits.Dispose();
				this._crits = null;
			}
		}
		protected override void UICommChannel_OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (this._piechart.TryPanelMessage(panelName, msgType, msgParams, PanelBinding.PanelMessageTargetFlags.Self))
			{
				return;
			}
			if (msgType == "dialog_closed")
			{
				if (panelName == this._confirmDiscardInvoiceDialog)
				{
					bool flag = bool.Parse(msgParams[0]);
					if (flag)
					{
						this._invoiceItems.Clear();
						base.App.UI.ClearItems(BuildScreenState.UIInvoiceItemList);
						base.App.UI.LockUI();
						base.App.SwitchGameState<StarMapState>(new object[0]);
					}
				}
				else
				{
					if (panelName == this._deleteItemDialog)
					{
						bool flag2 = bool.Parse(msgParams[0]);
						if (flag2)
						{
							base.App.GameDatabase.RemovePlayerDesign(this._deleteItemID);
							this.PopulateClassList(this._selectedClass);
							this.PopulateDesignList(this._selectedClass);
						}
					}
					else
					{
						if (panelName == this._deleteInvoiceDialog)
						{
							bool flag3 = bool.Parse(msgParams[0]);
							if (flag3)
							{
								base.App.GameDatabase.RemoveFavoriteInvoice(this._deleteItemID);
								this.PopulateDesignList(this._selectedClass);
							}
						}
					}
				}
			}
			else
			{
				if (msgType == "button_clicked")
				{
					if (panelName.Contains("designDeleteButton"))
					{
						string[] array = panelName.Split(new char[]
						{
							'|'
						});
						this._deleteItemID = int.Parse(array[1]);
						if (array.Count<string>() == 3)
						{
							InvoiceInfo invoiceInfo = base.App.GameDatabase.GetInvoiceInfo(this._deleteItemID, base.App.LocalPlayer.ID);
							if (invoiceInfo != null)
							{
								this._deleteInvoiceDialog = base.App.UI.CreateDialog(new GenericQuestionDialog(base.App, App.Localize("@UI_DESIGN_DELETE_INVOICE_TITLE"), string.Format(App.Localize("@UI_DESIGN_DELETE_INVOICE_DESC"), invoiceInfo.Name), "dialogGenericQuestion"), null);
							}
						}
						else
						{
							DesignInfo designInfo = base.App.GameDatabase.GetDesignInfo(this._deleteItemID);
							if (designInfo != null)
							{
								this._deleteItemDialog = base.App.UI.CreateDialog(new GenericQuestionDialog(base.App, App.Localize("@UI_DESIGN_DELETE_TITLE"), string.Format(App.Localize("@UI_DESIGN_DELETE_DESC"), designInfo.Name), "dialogGenericQuestion"), null);
							}
						}
					}
					else
					{
						if (panelName == BuildScreenState.UIAddToInvoiceFavorites)
						{
							this._addToFavorites = !this._addToFavorites;
						}
						else
						{
							if (panelName == "gameTutorialButton")
							{
								base.App.UI.SetVisible("BuildScreenTutorial", true);
							}
							else
							{
								if (panelName == "buildScreenTutImage")
								{
									base.App.UI.SetVisible("BuildScreenTutorial", false);
								}
								else
								{
									if (panelName == BuildScreenState.UIInvoiceRemove)
									{
										this.RemoveInvoice(this._selectedInvoice);
										this._selectedInvoice = null;
									}
									else
									{
										if (panelName == BuildScreenState.UICloseInvoiceSummary)
										{
											base.App.UI.SetVisible(BuildScreenState.UIInvoiceSummaryPopup, false);
											this.PopulateInvoiceList();
										}
										else
										{
											if (panelName == BuildScreenState.UIFavInvoiceRemove)
											{
												this.RemoveFavInvoice(this._selectedFavInvoice);
												this._selectedFavInvoice = null;
											}
											else
											{
												if (panelName == BuildScreenState.UIRemoveInvoiceItem)
												{
													this.RemoveBuildOrder(this._selectedBuildOrder);
													this.SyncCost(base.App, base.App.GameDatabase.GetDesignInfo(this.SelectedDesign));
													this._selectedBuildOrder = null;
												}
												else
												{
													if (panelName == BuildScreenState.UIRemoveOrderItem)
													{
														this.RemoveInvoiceItem(this._selectedInvoiceItem);
														this.SyncCost(base.App, base.App.GameDatabase.GetDesignInfo(this.SelectedDesign));
														this._selectedInvoiceItem = null;
													}
													else
													{
														if (panelName == BuildScreenState.UISubmitDialogOk)
														{
															this.SubmitOrder(this._invoiceName);
															base.App.UI.SetPropertyString(BuildScreenState.UIInvoiceName, "text", "");
															base.App.UI.SetEnabled(BuildScreenState.UISubmitDialogOk, false);
															base.App.UI.SetVisible("submit_dialog", false);
															this._confirmInvoiceDialogActive = false;
															this._selectedInvoiceItem = null;
														}
														else
														{
															if (panelName == BuildScreenState.UISubmitDialogCancel)
															{
																base.App.UI.SetVisible("submit_dialog", false);
																this._confirmInvoiceDialogActive = false;
															}
															else
															{
																if (panelName == BuildScreenState.UILoaDialogOk)
																{
																	this.AddOrder(this._designList.First((DesignInfo x) => x.IsLoaCube()).ID, true, true);
																	base.App.UI.SetVisible("LoaCubeDialog", false);
																}
																else
																{
																	if (panelName == BuildScreenState.UILoaDialogCancel)
																	{
																		base.App.UI.SetVisible("LoaCubeDialog", false);
																	}
																	else
																	{
																		if (panelName == "game_budget_pie")
																		{
																			base.App.UI.LockUI();
																			base.App.SwitchGameState<EmpireSummaryState>(new object[0]);
																		}
																		else
																		{
																			if (panelName == BuildScreenState.UIExitButton)
																			{
																				if (this._invoiceItems.Count > 0)
																				{
																					this._confirmDiscardInvoiceDialog = base.App.UI.CreateDialog(new GenericQuestionDialog(base.App, "@UI_BUILD_PENDING_INVOICE_TITLE", "@UI_BUILD_PENDING_INVOICE_DESC", "dialogGenericQuestion"), null);
																				}
																				else
																				{
																					base.App.UI.LockUI();
																					base.App.SwitchGameState<StarMapState>(new object[0]);
																				}
																			}
																			else
																			{
																				if (panelName == BuildScreenState.UIDesignScreenButton)
																				{
																					base.App.UI.LockUI();
																					base.App.SwitchGameState<DesignScreenState>(new object[]
																					{
																						false,
																						"BuildScreenState"
																					});
																				}
																				else
																				{
																					if (panelName == BuildScreenState.UISubmitOrder)
																					{
																						IEnumerable<InvoiceInfo> invoiceInfosForPlayer = base.App.GameDatabase.GetInvoiceInfosForPlayer(base.App.LocalPlayer.ID);
																						int num = 1;
																						while (true)
																						{
																							this._invoiceName = string.Format("Invoice #{0}", num);
																							bool flag4 = false;
																							foreach (InvoiceInfo current in invoiceInfosForPlayer)
																							{
																								if (current.Name == this._invoiceName)
																								{
																									flag4 = true;
																									break;
																								}
																							}
																							if (!flag4)
																							{
																								break;
																							}
																							num++;
																						}
																						base.App.UI.SetEnabled(BuildScreenState.UISubmitDialogOk, true);
																						base.App.UI.SetPropertyString(BuildScreenState.UIInvoiceName, "text", this._invoiceName);
																						base.App.UI.SetVisible("submit_dialog", true);
																						this._confirmInvoiceDialogActive = true;
																					}
																					else
																					{
																						if (panelName == "title_prev")
																						{
																							this.SelectNextSystem(true);
																						}
																						else
																						{
																							if (panelName == "title_next")
																							{
																								this.SelectNextSystem(false);
																							}
																							else
																							{
																								if (panelName == "gameAddDesignButton")
																								{
																									if (!this.SelectedClass.HasValue)
																									{
																										if (this._selectedFavInvoice.HasValue && this._selectedFavInvoice.HasValue)
																										{
																											this.SetSelectedInvoice(this._selectedFavInvoice.Value);
																										}
																									}
																									else
																									{
																										this.AddOrder(this.SelectedDesign, true, false);
																									}
																								}
																								else
																								{
																									if (panelName.StartsWith("DeleteOrder"))
																									{
																										this.RemoveInvoiceItem(new int?(int.Parse(panelName.Replace("DeleteOrder", string.Empty))));
																									}
																									else
																									{
																										if (panelName == "gameRandomizeSummaryShipNames")
																										{
																											new List<BuildScreenState.InvoiceItem>();
																											List<BuildOrderInfo> list = base.App.GameDatabase.GetBuildOrdersForInvoiceInstance(this._selectedInvoice.Value).ToList<BuildOrderInfo>();
																											using (List<BuildOrderInfo>.Enumerator enumerator2 = list.GetEnumerator())
																											{
																												while (enumerator2.MoveNext())
																												{
																													BuildOrderInfo current2 = enumerator2.Current;
																													DesignInfo designInfo2 = base.App.GameDatabase.GetDesignInfo(current2.DesignID);
																													current2.ShipName = base.App.Game.NamesPool.GetShipName(base.App.Game, base.App.LocalPlayer.ID, designInfo2.Class, null);
																													string listId = base.App.UI.Path(new string[]
																													{
																														BuildScreenState.UIBuildOrderPanel,
																														BuildScreenState.UIBuildOrderList
																													});
																													string subPanelId = string.Format("{0}{1}", BuildScreenState.UIBuildOrderList, current2.ID);
																													base.App.UI.SetItemPropertyString(listId, string.Empty, current2.ID, subPanelId, "text", current2.ShipName);
																													base.App.GameDatabase.UpdateBuildOrder(current2);
																												}
																												goto IL_10ED;
																											}
																										}
																										if (panelName == "gameRandomizeShipNames")
																										{
																											foreach (BuildScreenState.InvoiceItem current3 in this._invoiceItems)
																											{
																												DesignInfo designInfo3 = base.App.GameDatabase.GetDesignInfo(current3.DesignID);
																												current3.ShipName = base.App.Game.NamesPool.GetShipName(base.App.Game, base.App.LocalPlayer.ID, designInfo3.Class, 
																													from x in this._invoiceItems
																													select x.ShipName);
																											}
																											if (this._invoiceItems.Any<BuildScreenState.InvoiceItem>())
																											{
																												this.SyncInvoiceItemsList(BuildScreenState.UIOrderInvoiceItems, BuildScreenState.UIInvoiceItemList, this._invoiceItems, App.Localize("@UI_BUILD_NEW_INVOICE"), false, 0);
																											}
																										}
																									}
																								}
																							}
																						}
																					}
																				}
																			}
																		}
																	}
																}
															}
														}
													}
												}
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
					if (msgType == "list_sel_changed")
					{
						if (panelName == BuildScreenState.UIClassList)
						{
							int num2 = BuildScreenState.ParseId(msgParams[0]);
							this.SetSelectedShipClass((num2 != -1) ? new RealShipClasses?((RealShipClasses)num2) : null, string.Empty);
						}
						else
						{
							if (panelName == BuildScreenState.UIDesignList)
							{
								if (this._selectedClass.HasValue)
								{
									int designId = BuildScreenState.ParseId(msgParams[0]);
									this.SetSelectedDesign(designId, BuildScreenState.UIDesignList);
								}
								else
								{
									int value = BuildScreenState.ParseId(msgParams[0]);
									this.SetFavInvoice(new int?(value));
								}
							}
							else
							{
								if (panelName == BuildScreenState.UIInvoiceList)
								{
									this._selectedInvoice = new int?(BuildScreenState.ParseId(msgParams[0]));
								}
								else
								{
									if (panelName == BuildScreenState.UIInvoiceItemList)
									{
										this._selectedInvoiceItem = new int?(BuildScreenState.ParseId(msgParams[0]));
									}
									else
									{
										if (panelName == BuildScreenState.UIBuildOrderList)
										{
											this._selectedBuildOrder = new int?(BuildScreenState.ParseId(msgParams[0]));
										}
									}
								}
							}
						}
					}
					else
					{
						if (msgType == "list_item_dblclk")
						{
							if (panelName == BuildScreenState.UIDesignList)
							{
								int num3 = BuildScreenState.ParseId(msgParams[0]);
								if (!this.SelectedClass.HasValue)
								{
									this.SetSelectedInvoice(num3);
								}
								else
								{
									this.AddOrder(num3, true, false);
								}
							}
							else
							{
								if (panelName == BuildScreenState.UIInvoiceList)
								{
									List<BuildScreenState.InvoiceItem> list2 = new List<BuildScreenState.InvoiceItem>();
									List<BuildOrderInfo> list3 = base.App.GameDatabase.GetBuildOrdersForInvoiceInstance(this._selectedInvoice.Value).ToList<BuildOrderInfo>();
									foreach (BuildOrderInfo current4 in list3)
									{
										list2.Insert(0, new BuildScreenState.InvoiceItem
										{
											DesignID = current4.DesignID,
											ShipName = current4.ShipName,
											TempOrderID = current4.ID,
											Progress = (int)((float)current4.Progress * 100f / (float)current4.ProductionTarget),
											isPrototypeOrder = !base.App.GameDatabase.GetDesignInfo(current4.DesignID).isPrototyped
										});
									}
									bool retrofitinvoice = false;
									int shipid = 0;
									List<RetrofitOrderInfo> list4 = base.App.GameDatabase.GetRetrofitOrdersForInvoiceInstance(this._selectedInvoice.Value).ToList<RetrofitOrderInfo>();
									foreach (RetrofitOrderInfo current5 in list4)
									{
										ShipInfo shipInfo = base.App.GameDatabase.GetShipInfo(current5.ShipID, true);
										list2.Insert(0, new BuildScreenState.InvoiceItem
										{
											DesignID = current5.DesignID,
											ShipName = shipInfo.ShipName + " (" + shipInfo.DesignInfo.Name + ")",
											TempOrderID = current5.ID,
											Progress = 0,
											isPrototypeOrder = false
										});
										retrofitinvoice = true;
										shipid = current5.ShipID;
									}
									this.SyncInvoiceItemsList(BuildScreenState.UIBuildOrderPanel, BuildScreenState.UIBuildOrderList, list2, base.App.GameDatabase.GetInvoiceInstanceInfo(this._selectedInvoice.Value).Name, retrofitinvoice, shipid);
									base.App.UI.SetVisible(BuildScreenState.UIInvoiceSummaryPopup, true);
								}
							}
						}
						else
						{
							if (msgType == "slider_value")
							{
								if (panelName == "LoaPointSlider")
								{
									this._loacubeval = (int)float.Parse(msgParams[0]);
									base.App.UI.SetPropertyString(base.App.UI.Path(new string[]
									{
										"LoaCubeDialog",
										"LoaPointSlider",
										"right_label"
									}), "text", this._loacubeval.ToString());
									base.App.UI.SetText(base.App.UI.Path(new string[]
									{
										"LoaCubeDialog",
										"LoaPointValue"
									}), this._loacubeval.ToString());
									this.SyncCost(base.App, base.App.GameDatabase.GetDesignInfo(this.SelectedDesign));
								}
							}
							else
							{
								if (msgType == "text_changed")
								{
									if (panelName.StartsWith(BuildScreenState.UIInvoiceItemList))
									{
										int orderId = int.Parse(panelName.Replace(BuildScreenState.UIInvoiceItemList, ""));
										this._invoiceItems.Single((BuildScreenState.InvoiceItem x) => x.TempOrderID == orderId).ShipName = msgParams[0];
									}
									else
									{
										if (panelName.StartsWith(BuildScreenState.UIBuildOrderList))
										{
											int buildOrderId = int.Parse(panelName.Replace(BuildScreenState.UIBuildOrderList, ""));
											BuildOrderInfo buildOrderInfo = base.App.GameDatabase.GetBuildOrderInfo(buildOrderId);
											buildOrderInfo.ShipName = msgParams[0];
											base.App.GameDatabase.UpdateBuildOrder(buildOrderInfo);
										}
										else
										{
											if (panelName == BuildScreenState.UIInvoiceName)
											{
												this._invoiceName = msgParams[0];
											}
											else
											{
												if (panelName == "LoaPointValue")
												{
													int val;
													if (int.TryParse(msgParams[0], out val))
													{
														int num4 = Math.Max(this._minLoaCubeval, Math.Min(val, this._maxLoaCubeval));
														base.App.UI.SetSliderValue(base.App.UI.Path(new string[]
														{
															"LoaCubeDialog",
															"LoaPointSlider"
														}), num4);
														base.App.UI.SetPropertyString(base.App.UI.Path(new string[]
														{
															"LoaCubeDialog",
															"LoaPointSlider",
															"right_label"
														}), "text", num4.ToString());
														this._loacubeval = num4;
													}
													else
													{
														if (msgParams[0] == string.Empty)
														{
															int minLoaCubeval = this._minLoaCubeval;
															base.App.UI.SetSliderValue(base.App.UI.Path(new string[]
															{
																"LoaCubeDialog",
																"LoaPointSlider"
															}), minLoaCubeval);
															base.App.UI.SetPropertyString(base.App.UI.Path(new string[]
															{
																"LoaCubeDialog",
																"LoaPointSlider",
																"right_label"
															}), "text", minLoaCubeval.ToString());
															this._loacubeval = minLoaCubeval;
														}
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
			IL_10ED:
			base.App.UI.SetEnabled(BuildScreenState.UIInvoiceRemove, this._selectedInvoice.HasValue);
			base.App.UI.SetEnabled(BuildScreenState.UIShipRemove, this._selectedInvoiceItem.HasValue);
			base.App.UI.SetEnabled(BuildScreenState.UIFavInvoiceRemove, this._selectedFavInvoice.HasValue);
			bool flag5 = false;
			bool flag6 = false;
			DesignInfo designInfo4 = base.App.GameDatabase.GetDesignInfo(this._selectedDesign);
			if (designInfo4 != null)
			{
				if (!designInfo4.isPrototyped)
				{
					List<BuildOrderInfo> list5 = base.App.GameDatabase.GetDesignBuildOrders(designInfo4).ToList<BuildOrderInfo>();
					if (list5.Count > 0)
					{
						flag5 = true;
						base.App.UI.SetTooltip("gameAddDesignButton", App.Localize("@UI_TOOLTIP_PROTOTYPE_ALREADY"));
					}
					else
					{
						if (this._invoiceItems.Count<BuildScreenState.InvoiceItem>() > 0)
						{
							flag5 = true;
							base.App.UI.SetTooltip("gameAddDesignButton", App.Localize("@UI_TOOLTIP_CANNOT_ADD_PROTOTYPE"));
							DesignInfo designInfo5 = base.App.GameDatabase.GetDesignInfo(this._invoiceItems[0].DesignID);
							if (designInfo5 != null && !designInfo5.isPrototyped)
							{
								flag6 = true;
							}
						}
					}
				}
				else
				{
					if (this._invoiceItems.Count<BuildScreenState.InvoiceItem>() > 0)
					{
						DesignInfo designInfo6 = base.App.GameDatabase.GetDesignInfo(this._invoiceItems[0].DesignID);
						if (designInfo6 != null && !designInfo6.isPrototyped)
						{
							flag5 = true;
							base.App.UI.SetTooltip("gameAddDesignButton", App.Localize("@UI_TOOLTIP_CANNOT_ADD_PROTOTYPE"));
							flag6 = true;
						}
					}
				}
			}
			if (flag5)
			{
				base.App.UI.SetEnabled("gameAddDesignButton", false);
				if (flag6)
				{
					base.App.UI.SetVisible("invoicePanelNormal", false);
					base.App.UI.SetVisible("invoicePanelPrototype", true);
					base.App.UI.SetPropertyString("newInvoiceText", "text", App.Localize("@UI_BUILD_NEW_PROTOTYPE"));
					return;
				}
			}
			else
			{
				base.App.UI.SetEnabled("gameAddDesignButton", true);
				base.App.UI.SetTooltip("gameAddDesignButton", App.Localize("@UI_TOOLTIP_ADDTOBUILD"));
				base.App.UI.SetVisible("invoicePanelNormal", true);
				base.App.UI.SetVisible("invoicePanelPrototype", false);
				base.App.UI.SetPropertyString("newInvoiceText", "text", App.Localize("@UI_BUILD_NEW_INVOICE"));
			}
		}
		private void RemoveInvoice(int? invoiceId)
		{
			if (!invoiceId.HasValue)
			{
				return;
			}
			List<BuildOrderInfo> source = base.App.GameDatabase.GetBuildOrdersForInvoiceInstance(invoiceId.Value).ToList<BuildOrderInfo>();
			if (source.Any((BuildOrderInfo x) => !base.App.GameDatabase.GetDesignInfo(x.DesignID).isPrototyped))
			{
				List<BuildOrderInfo> list = (
					from x in source
					where !base.App.GameDatabase.GetDesignInfo(x.DesignID).isPrototyped
					select x).ToList<BuildOrderInfo>();
				foreach (BuildOrderInfo bi in list)
				{
					if (this._invoiceItems.Any((BuildScreenState.InvoiceItem x) => x.DesignID == bi.DesignID))
					{
						this._invoiceItems.First((BuildScreenState.InvoiceItem x) => x.DesignID == bi.DesignID).isPrototypeOrder = true;
					}
				}
			}
			IEnumerable<BuildOrderInfo> buildOrdersForInvoiceInstance = base.App.GameDatabase.GetBuildOrdersForInvoiceInstance(invoiceId.Value);
			foreach (BuildOrderInfo current in buildOrdersForInvoiceInstance)
			{
				base.App.GameDatabase.RemoveBuildOrder(current.ID);
			}
			IEnumerable<RetrofitOrderInfo> retrofitOrdersForInvoiceInstance = base.App.GameDatabase.GetRetrofitOrdersForInvoiceInstance(invoiceId.Value);
			foreach (RetrofitOrderInfo current2 in retrofitOrdersForInvoiceInstance)
			{
				base.App.GameDatabase.RemoveRetrofitOrder(current2.ID, false, false);
			}
			base.App.UI.RemoveItems(BuildScreenState.UIInvoiceList, invoiceId.Value);
			base.App.GameDatabase.RemoveInvoiceInstance(invoiceId.Value);
			this.SyncInvoiceItemsList(BuildScreenState.UIOrderInvoiceItems, BuildScreenState.UIInvoiceItemList, this._invoiceItems, App.Localize("@UI_BUILD_NEW_INVOICE"), false, 0);
			this.SyncCost(base.App, base.App.GameDatabase.GetDesignInfo(this._selectedDesign));
		}
		private void RemoveBuildOrder(int? orderId)
		{
			if (!orderId.HasValue)
			{
				return;
			}
			List<BuildOrderInfo> list = base.App.GameDatabase.GetBuildOrdersForInvoiceInstance(this._selectedInvoice.Value).ToList<BuildOrderInfo>();
			if (list.Count <= 1)
			{
				return;
			}
			BuildOrderInfo bi = list.First((BuildOrderInfo x) => x.ID == orderId);
			DesignInfo designInfo = base.App.GameDatabase.GetDesignInfo(bi.DesignID);
			if (!base.App.GameDatabase.GetDesignInfo(bi.DesignID).isPrototyped && list.Any((BuildOrderInfo x) => x != bi && x.DesignID == bi.DesignID))
			{
				BuildOrderInfo buildOrderInfo = list.First((BuildOrderInfo x) => x != bi && x.DesignID == bi.DesignID);
				buildOrderInfo.ProductionTarget = designInfo.GetPlayerProductionCost(base.App.GameDatabase, base.App.LocalPlayer.ID, true, null);
				base.App.GameDatabase.UpdateBuildOrder(buildOrderInfo);
			}
			base.App.GameDatabase.RemoveBuildOrder(orderId.Value);
			List<BuildScreenState.InvoiceItem> list2 = new List<BuildScreenState.InvoiceItem>();
			list = base.App.GameDatabase.GetBuildOrdersForInvoiceInstance(this._selectedInvoice.Value).ToList<BuildOrderInfo>();
			foreach (BuildOrderInfo current in list)
			{
				list2.Insert(0, new BuildScreenState.InvoiceItem
				{
					DesignID = current.DesignID,
					ShipName = current.ShipName,
					TempOrderID = current.ID,
					Progress = (int)((float)current.Progress * 100f / (float)current.ProductionTarget),
					isPrototypeOrder = base.App.GameDatabase.GetDesignInfo(current.DesignID).ProductionCost < current.ProductionTarget
				});
			}
			this.SyncInvoiceItemsList(BuildScreenState.UIBuildOrderPanel, BuildScreenState.UIBuildOrderList, list2, base.App.GameDatabase.GetInvoiceInstanceInfo(this._selectedInvoice.Value).Name, false, 0);
		}
		private void RemoveInvoiceItem(int? orderId)
		{
			if (!orderId.HasValue)
			{
				return;
			}
			BuildScreenState.InvoiceItem found = this._invoiceItems.FirstOrDefault((BuildScreenState.InvoiceItem x) => x.TempOrderID == orderId);
			if (found == null)
			{
				return;
			}
			if (found.isPrototypeOrder && this._invoiceItems.Any((BuildScreenState.InvoiceItem x) => x != found && x.DesignID == found.DesignID))
			{
				BuildScreenState.InvoiceItem invoiceItem = this._invoiceItems.First((BuildScreenState.InvoiceItem x) => x != found && x.DesignID == found.DesignID);
				invoiceItem.isPrototypeOrder = true;
			}
			this._invoiceItems.Remove(found);
			this.SyncInvoiceItemsList(BuildScreenState.UIOrderInvoiceItems, BuildScreenState.UIInvoiceItemList, this._invoiceItems, App.Localize("@UI_BUILD_NEW_INVOICE"), false, 0);
			base.App.UI.SetEnabled(BuildScreenState.UISubmitOrder, this._invoiceItems.Count != 0);
			this.SyncFinancialDetails(base.App);
		}
		private void RemoveFavInvoice(int? favInvoiceId)
		{
			if (favInvoiceId.HasValue)
			{
				base.App.GameDatabase.RemoveFavoriteInvoice(favInvoiceId.Value);
				this.PopulateDesignList(this._selectedClass);
			}
		}
		private static bool IsShipClassAllowed(RealShipClasses? value)
		{
			if (!value.HasValue)
			{
				return false;
			}
			switch (value.Value)
			{
			case RealShipClasses.Cruiser:
			case RealShipClasses.Dreadnought:
			case RealShipClasses.Leviathan:
			case RealShipClasses.BattleRider:
			case RealShipClasses.BattleCruiser:
			case RealShipClasses.BattleShip:
			case RealShipClasses.Platform:
			case RealShipClasses.SystemDefenseBoat:
				return true;
			case RealShipClasses.Drone:
			case RealShipClasses.BoardingPod:
			case RealShipClasses.EscapePod:
			case RealShipClasses.AssaultShuttle:
			case RealShipClasses.Biomissile:
			case RealShipClasses.Station:
			case RealShipClasses.NumShipClasses:
				return false;
			default:
				throw new ArgumentOutOfRangeException("value");
			}
		}
		public static bool IsShipRoleAllowed(ShipRole role)
		{
			switch (role)
			{
			case ShipRole.BOARDINGPOD:
			case ShipRole.BIOMISSILE:
			case ShipRole.TRAPDRONE:
				break;
			default:
				if (role != ShipRole.ACCELERATOR_GATE)
				{
					return true;
				}
				break;
			}
			return false;
		}
		private IEnumerable<RealShipClasses> GetAllowedShipClasses()
		{
			try
			{
				RealShipClasses[] realShipClasses = ShipClassExtensions.RealShipClasses;
				for (int i = 0; i < realShipClasses.Length; i++)
				{
					RealShipClasses realShipClasses2 = realShipClasses[i];
					if (BuildScreenState.IsShipClassAllowed(new RealShipClasses?(realShipClasses2)))
					{
						yield return realShipClasses2;
					}
				}
			}
			finally
			{
			}
			yield break;
		}
		private bool IsDesignAllowed(DesignInfo designInfo)
		{
            return BuildScreenState.IsShipClassAllowed(designInfo.GetRealShipClass()) && BuildScreenState.IsShipRoleAllowed(designInfo.Role) && !Kerberos.Sots.StarFleet.StarFleet.DesignIsSuulka(base.App, designInfo);
		}
		private IEnumerable<DesignInfo> GetAvailableDesigns(RealShipClasses shipClass)
		{
			IEnumerable<DesignInfo> visibleDesignInfosForPlayer = base.App.GameDatabase.GetVisibleDesignInfosForPlayer(base.App.LocalPlayer.ID, shipClass);
			List<DesignInfo> list = visibleDesignInfosForPlayer.ToList<DesignInfo>();
			foreach (DesignInfo current in visibleDesignInfosForPlayer)
			{
                if (!this.IsDesignAllowed(current) || !Kerberos.Sots.StarFleet.StarFleet.IsNewestRetrofit(current, visibleDesignInfosForPlayer))
				{
					list.Remove(current);
				}
			}
			return list;
		}
		private void SetSelectedSystem(int systemId, string trigger)
		{
			this._selectedSystem = systemId;
			StarSystemInfo starSystemInfo = base.App.GameDatabase.GetStarSystemInfo(systemId);
			string text = string.Format("{0} System", starSystemInfo.Name);
			base.App.UI.SetText("gameSystemTitle", text);
			StarSystemMapUI.Sync(base.App, systemId, "gameSystemMiniMap", false);
		}
		private void PopulateClassList(RealShipClasses? shipClass)
		{
			List<RealShipClasses> list = this.GetAllowedShipClasses().ToList<RealShipClasses>();
			base.App.UI.ClearItems(BuildScreenState.UIClassList);
			foreach (RealShipClasses current in list)
			{
				if (this.GetAvailableDesigns(current).Count<DesignInfo>() > 0)
				{
					base.App.UI.AddItem(BuildScreenState.UIClassList, string.Empty, (int)current, current.Localize());
				}
			}
			base.App.UI.AddItem(BuildScreenState.UIClassList, string.Empty, -1, App.Localize("@UI_BUILD_FAVORITE_INVOICES"));
			this._selectedDesign = 0;
			if (!shipClass.HasValue || this.GetAvailableDesigns(shipClass.Value).Any<DesignInfo>())
			{
				this.SetSelectedShipClass(shipClass, string.Empty);
				return;
			}
			int num = list.FindIndex((RealShipClasses x) => this.GetAvailableDesigns(x).Any<DesignInfo>());
			if (num < 0)
			{
				this.SetSelectedShipClass(null, string.Empty);
				return;
			}
			this.SetSelectedShipClass(new RealShipClasses?(list[num]), string.Empty);
		}
		private void SetSelectedInvoice(int invoiceId)
		{
			this._invoiceItems.Clear();
			base.App.UI.ClearItems(BuildScreenState.UIInvoiceItemList);
			bool playSound = true;
			List<InvoiceBuildOrderInfo> list = base.App.GameDatabase.GetInvoiceBuildOrders(invoiceId).ToList<InvoiceBuildOrderInfo>();
			foreach (InvoiceBuildOrderInfo current in list)
			{
				int loacubeval = base.App.GameDatabase.GetDesignInfo(current.DesignID).IsLoaCube() ? current.LoaCubes : 0;
				this._loacubeval = loacubeval;
				this.AddOrder(current.DesignID, playSound, true);
				playSound = false;
			}
		}
		private void SetSelectedShipClass(RealShipClasses? id, string trigger)
		{
			if (this._selectedClass == id)
			{
				return;
			}
			this._selectedClass = id;
			if (trigger != BuildScreenState.UIClassList)
			{
				base.App.UI.SetSelection(BuildScreenState.UIClassList, (int)(id.HasValue ? id.Value : ((RealShipClasses)(-1))));
			}
			this.PopulateDesignList(id);
		}
		private void PopulateDesignList(RealShipClasses? shipClass)
		{
			if (shipClass.HasValue)
			{
				this._designList = this.GetAvailableDesigns(shipClass.Value).ToList<DesignInfo>();
				BuildScreenState.PopulateDesignList(base.App, BuildScreenState.UIDesignList, this._designList);
				if (this._designList.Count > 0)
				{
					this.SetSelectedDesign(this._designList[0].ID, string.Empty);
					return;
				}
			}
			else
			{
				this._invoiceList = (
					from x in base.App.GameDatabase.GetInvoiceInfosForPlayer(base.App.Game.LocalPlayer.ID)
					where x.isFavorite
					select x).ToList<InvoiceInfo>();
				ShipDesignUI.PopulateDesignList(base.App, BuildScreenState.UIDesignList, this._invoiceList);
				if (this._invoiceList.Count > 0)
				{
					this.SetFavInvoice(new int?(this._invoiceList[0].ID));
				}
			}
		}
		private void SetSelectedDesign(int designId, string trigger)
		{
			if (trigger != BuildScreenState.UIDesignList)
			{
				if (designId == 0)
				{
					base.App.UI.ClearSelection(BuildScreenState.UIDesignList);
				}
				else
				{
					base.App.UI.SetSelection(BuildScreenState.UIDesignList, designId);
				}
			}
			if (this._selectedDesign == designId)
			{
				return;
			}
			this._selectedDesign = designId;
			if (designId != 0)
			{
				DesignInfo designInfo = base.App.GameDatabase.GetDesignInfo(designId);
				base.App.UI.SetEnabled("gameAddDesignButton", true);
				base.App.UI.SetText("designNameTag", designInfo.Name);
				if (designInfo.isAttributesDiscovered)
				{
					IEnumerable<SectionEnumerations.DesignAttribute> designAttributesForDesign = base.App.GameDatabase.GetDesignAttributesForDesign(designInfo.ID);
					if (designAttributesForDesign.Count<SectionEnumerations.DesignAttribute>() > 0)
					{
						base.App.UI.SetVisible("attributeNameTagPanel", true);
						base.App.UI.SetText("attributeNameTagPanel.attributeNameTag", App.Localize("@UI_" + designAttributesForDesign.First<SectionEnumerations.DesignAttribute>().ToString()));
						base.App.UI.SetTooltip("attributeNameTagPanel", App.Localize("@UI_" + designAttributesForDesign.First<SectionEnumerations.DesignAttribute>().ToString() + "_TOOLTIP"));
					}
					else
					{
						base.App.UI.SetVisible("attributeNameTagPanel", false);
					}
				}
				else
				{
					base.App.UI.SetVisible("attributeNameTagPanel", false);
				}
				if (designInfo.DesignSections.Count<DesignSectionInfo>() > 2)
				{
					base.App.UI.SetVisible("commandTag", true);
					base.App.UI.SetVisible("missionTag", true);
					base.App.UI.SetVisible("engineTag", true);
				}
				else
				{
					if (designInfo.DesignSections.Count<DesignSectionInfo>() > 1)
					{
						base.App.UI.SetVisible("commandTag", false);
						base.App.UI.SetVisible("missionTag", true);
						base.App.UI.SetVisible("engineTag", true);
					}
					else
					{
						if (designInfo.DesignSections.Count<DesignSectionInfo>() > 0)
						{
							base.App.UI.SetVisible("commandTag", false);
							base.App.UI.SetVisible("missionTag", true);
							base.App.UI.SetVisible("engineTag", false);
						}
					}
				}
				DesignSectionInfo[] designSections = designInfo.DesignSections;
				for (int i = 0; i < designSections.Length; i++)
				{
					DesignSectionInfo designSectionInfo = designSections[i];
					switch (designSectionInfo.ShipSectionAsset.Type)
					{
					case ShipSectionType.Command:
						base.App.UI.SetText("commandTag", App.Localize(designSectionInfo.ShipSectionAsset.Title));
						break;
					case ShipSectionType.Mission:
						base.App.UI.SetText("missionTag", App.Localize(designSectionInfo.ShipSectionAsset.Title));
						break;
					case ShipSectionType.Engine:
						base.App.UI.SetText("engineTag", App.Localize(designSectionInfo.ShipSectionAsset.Title));
						break;
					}
				}
				if (!designInfo.isPrototyped)
				{
					base.App.UI.SetText("gameAddDesignButton", App.Localize("@UI_BUILD_PROTOTYPE_DESIGN"));
					base.App.UI.SetTooltip("gameAddDesignButton", App.Localize("@UI_TOOLTIP_PROTOTYPE_DESIGN"));
				}
				else
				{
					base.App.UI.SetText("gameAddDesignButton", App.Localize("@UI_BUILD_ADD_TO_INVOICE"));
					base.App.UI.SetTooltip("gameAddDesignButton", App.Localize("@UI_TOOLTIP_ADDTOBUILD"));
					base.App.UI.SetText("gameShipsProduced", base.App.GameDatabase.GetNumShipsBuiltFromDesign(this._selectedDesign).ToString());
					base.App.UI.SetText("gameShipsDestroyed", base.App.GameDatabase.GetNumShipsDestroyedFromDesign(this._selectedDesign).ToString());
					base.App.UI.SetText("gameDesignComissionHeader", string.Format(App.Localize("@UI_DESIGN_DATE_HEADER"), designInfo.DesignDate));
				}
				this.SyncCost(base.App, designInfo);
				ShipDesignUI.SyncSupplies(base.App, designInfo);
				ShipDesignUI.SyncSpeed(base.App, designInfo);
				if (this._weaponTooltip == null)
				{
					this._weaponTooltip = new WeaponHoverPanel(base.App.UI, "ShipInfo.WeaponPanel", "weaponInfo");
				}
				if (this._moduleTooltip == null)
				{
					this._moduleTooltip = new ModuleHoverPanel(base.App.UI, "ShipInfo.WeaponPanel", "moduleInfo");
				}
				List<LogicalWeapon> list = new List<LogicalWeapon>();
				DesignSectionInfo[] designSections2 = designInfo.DesignSections;
				for (int j = 0; j < designSections2.Length; j++)
				{
					DesignSectionInfo designSectionInfo2 = designSections2[j];
					IEnumerable<WeaponBankInfo> weaponBanks = designSectionInfo2.WeaponBanks;
					foreach (WeaponBankInfo current in weaponBanks)
					{
						if (current.WeaponID.HasValue)
						{
							string weaponPath = base.App.GameDatabase.GetWeaponAsset(current.WeaponID.Value);
							LogicalWeapon weapon = base.App.AssetDatabase.Weapons.First((LogicalWeapon x) => x.FileName == weaponPath);
							if (weapon != null && (
								from x in list
								where x.FileName == weapon.FileName
								select x).Count<LogicalWeapon>() == 0)
							{
								list.Add(weapon);
							}
						}
					}
				}
				this._weaponTooltip.SetAvailableWeapons(list, true);
				List<LogicalModule> list2 = new List<LogicalModule>();
				DesignSectionInfo[] designSections3 = designInfo.DesignSections;
				for (int k = 0; k < designSections3.Length; k++)
				{
					DesignSectionInfo designSectionInfo3 = designSections3[k];
					IEnumerable<DesignModuleInfo> modules = designSectionInfo3.Modules;
					foreach (DesignModuleInfo current2 in modules)
					{
						string modulePath = base.App.GameDatabase.GetModuleAsset(current2.ModuleID);
						LogicalModule module = base.App.AssetDatabase.Modules.First((LogicalModule x) => x.ModulePath == modulePath);
						if (module != null && (
							from x in list2
							where x.ModulePath == module.ModulePath
							select x).Count<LogicalModule>() == 0)
						{
							list2.Add(module);
						}
					}
				}
				this._moduleTooltip.SetAvailableModules(list2, null, false);
				this._builder.New(base.App.LocalPlayer, designInfo, designInfo.Name, 0, true);
			}
		}
		private void SetFavInvoice(int? favInvoiceId)
		{
			this._selectedFavInvoice = favInvoiceId;
			if (this._selectedFavInvoice.HasValue)
			{
				List<InvoiceBuildOrderInfo> list = base.App.GameDatabase.GetInvoiceBuildOrders(this._selectedFavInvoice.Value).ToList<InvoiceBuildOrderInfo>();
				if (list.Count > 0)
				{
					this.SetSelectedDesign(list[0].DesignID, BuildScreenState.UIDesignList);
				}
			}
		}
		private List<DesignInfo> GetAdditionalShipDesigns(GameDatabase db, int playerID)
		{
			List<DesignInfo> list = new List<DesignInfo>();
			foreach (InvoiceInstanceInfo current in db.GetInvoicesForSystem(playerID, this._selectedSystem).ToList<InvoiceInstanceInfo>())
			{
				list.AddRange(
					from x in db.GetBuildOrdersForInvoiceInstance(current.ID)
					select db.GetDesignInfo(x.DesignID));
			}
			list.AddRange(
				from x in this._invoiceItems
				select db.GetDesignInfo(x.DesignID));
			return list;
		}
		public void SyncFinancialDetails(App game)
		{
			Budget budget = Budget.GenerateBudget(game.Game, game.GameDatabase.GetPlayerInfo(game.LocalPlayer.ID), this.GetAdditionalShipDesigns(game.GameDatabase, game.LocalPlayer.ID), BudgetProjection.Pessimistic);
			base.App.UI.SetPropertyString(BuildScreenState.UICurrentMaintenance, "text", budget.CurrentShipUpkeepExpenses.ToString("N0"));
			base.App.UI.SetPropertyString(BuildScreenState.UIProjectedCost, "text", budget.AdditionalUpkeepExpenses.ToString("N0"));
			base.App.UI.SetPropertyString(BuildScreenState.UITotalMaintenance, "text", budget.UpkeepExpenses.ToString("N0"));
			this._piechart.SetSlices(budget);
		}
		public void SyncConstructionSite(App app)
		{
			double num = 0.0;
			double num2 = 0.0;
			this._totalShipProductionRate = 0f;
			BuildScreenState.ObtainConstructionCosts(out this._totalShipProductionRate, out num, out num2, app, this._selectedSystem, app.LocalPlayer.ID);
			app.UI.SetPropertyString(BuildScreenState.UISysProductionValue, "text", this._totalShipProductionRate.ToString("N0"));
			app.UI.SetPropertyString(BuildScreenState.UISysIncomeValue, "text", num2.ToString("N0"));
			app.UI.SetPropertyString(BuildScreenState.UICvsTValue, "text", string.Format("{0}%", num.ToString("N0")));
		}
		public static void ObtainConstructionCosts(out float productionRate, out double constructionRate, out double totalRevenue, App app, int systemID, int playerID)
		{
			productionRate = 0f;
			constructionRate = 0.0;
			totalRevenue = 0.0;
			List<ColonyInfo> list = new List<ColonyInfo>();
			List<int> list2 = app.GameDatabase.GetStarSystemPlanets(systemID).ToList<int>();
			foreach (int current in list2)
			{
				ColonyInfo colonyInfoForPlanet = app.GameDatabase.GetColonyInfoForPlanet(current);
				if (colonyInfoForPlanet != null)
				{
					totalRevenue += Colony.GetTaxRevenue(app, colonyInfoForPlanet);
					constructionRate += (double)colonyInfoForPlanet.ShipConRate;
					productionRate += Colony.GetConstructionPoints(app.Game, colonyInfoForPlanet);
					list.Add(colonyInfoForPlanet);
				}
			}
			productionRate *= app.Game.GetStationBuildModifierForSystem(systemID, playerID);
			constructionRate = constructionRate * 100.0 / (double)list.Count;
		}
		public void SyncCost(App app, DesignInfo design)
		{
			if (design == null)
			{
				return;
			}
			bool flag = design.isPrototyped || this._invoiceItems.Any((BuildScreenState.InvoiceItem x) => x.DesignID == design.ID);
			if (flag)
			{
				app.UI.SetVisible("ShipCost", false);
				app.UI.SetVisible("ShipProductionCost", true);
				this.SyncProductionCost(app, "ShipProductionCost", design);
				return;
			}
			app.UI.SetVisible("ShipCost", true);
			app.UI.SetVisible("ShipProductionCost", false);
			ShipDesignUI.SyncCost(app, "ShipCost", design);
		}
		public static int ParseId(string msgParam)
		{
			if (string.IsNullOrEmpty(msgParam))
			{
				return 0;
			}
			return int.Parse(msgParam);
		}
		protected override void OnUpdate()
		{
			this._builder.Update();
			if (this._builder.Ship != null && !this._builder.Loading && this._builder.Ship.Active && this._camera.TargetID != this._builder.Ship.ObjectID)
			{
				this._camera.TargetID = this._builder.Ship.ObjectID;
				this._shipHoloView.SetUseViewport(true);
				this._shipHoloView.SetShip(this._builder.Ship);
			}
		}
		public override bool IsReady()
		{
			return this._crits != null && this._crits.IsReady() && base.IsReady();
		}
		public override void OnEngineMessage(InteropMessageID messageID, ScriptMessageReader mr)
		{
		}
		private void AddOrder(int designId, bool playSound = true, bool bypassLoa = false)
		{
			if (designId == 0)
			{
				return;
			}
			DesignInfo designInfo = base.App.GameDatabase.GetDesignInfo(designId);
			if (designInfo.IsLoaCube() && (!bypassLoa || this._loacubeval <= 0))
			{
				base.App.UI.SetVisible("LoaCubeDialog", true);
				return;
			}
			this.HACK_OrderID++;
            designInfo = Kerberos.Sots.StarFleet.StarFleet.GetNewestRetrofitDesign(designInfo, base.App.GameDatabase.GetDesignInfosForPlayer(base.App.LocalPlayer.ID));
			string shipName = designInfo.Name ?? "USS Placeholder";
			if (this._invoiceItems.Count > 0 && this._invoiceItems[0].isPrototypeOrder)
			{
				return;
			}
			if (!designInfo.isPrototyped)
			{
				base.App.UI.SetEnabled("gameAddDesignButton", false);
				base.App.UI.SetTooltip("gameAddDesignButton", App.Localize("@UI_TOOLTIP_PROTOTYPE_ALREADY"));
				if (this._invoiceItems.Count > 0 || base.App.GameDatabase.GetDesignBuildOrders(designInfo).Count<BuildOrderInfo>() > 0)
				{
					return;
				}
			}
			else
			{
				base.App.UI.SetEnabled("gameAddDesignButton", true);
				base.App.UI.SetTooltip("gameAddDesignButton", App.Localize("@UI_TOOLTIP_ADDTOBUILD"));
			}
			List<InvoiceInstanceInfo> source = base.App.GameDatabase.GetInvoicesForSystem(base.App.LocalPlayer.ID, this._selectedSystem).ToList<InvoiceInstanceInfo>();
			this._invoiceItems.Add(new BuildScreenState.InvoiceItem
			{
				DesignID = designId,
				TempOrderID = this.HACK_OrderID,
				ShipName = shipName,
				Progress = -1,
				isPrototypeOrder = !designInfo.isPrototyped && !this._invoiceItems.Any((BuildScreenState.InvoiceItem x) => x.DesignID == designInfo.ID && x.isPrototypeOrder) && !source.Any((InvoiceInstanceInfo x) => this.App.GameDatabase.GetBuildOrdersForInvoiceInstance(x.ID).Any((BuildOrderInfo y) => y.DesignID == designInfo.ID)),
				LoaCubes = designInfo.IsLoaCube() ? this._loacubeval : 0
			});
			if (playSound)
			{
				base.App.PostRequestGuiSound("build_addtoinvoice");
			}
			this.SyncInvoiceItemsList(BuildScreenState.UIOrderInvoiceItems, BuildScreenState.UIInvoiceItemList, this._invoiceItems, App.Localize("@UI_BUILD_NEW_INVOICE"), false, 0);
			DesignInfo designInfo2 = base.App.GameDatabase.GetDesignInfo(designId);
			this.SyncCost(base.App, designInfo2);
			this.SyncFinancialDetails(base.App);
			base.App.UI.SetEnabled(BuildScreenState.UISubmitOrder, this._invoiceItems.Count != 0);
		}
		public static void PopulateDesignList(App game, string designListId, IEnumerable<DesignInfo> designs)
		{
			game.UI.ClearItems(designListId);
			foreach (DesignInfo current in designs)
			{
                if (!Kerberos.Sots.StarFleet.StarFleet.DesignIsSuulka(game, current) && !current.IsAccelerator() && BuildScreenState.IsShipRoleAllowed(current.Role))
				{
					game.UI.AddItem(designListId, string.Empty, current.ID, current.Name);
					if (game.LocalPlayer.Faction.Name == "loa" && !current.IsLoaCube())
					{
						game.UI.SetItemPropertyString(designListId, string.Empty, current.ID, "designName", "text", current.Name + "  [" + ((float)current.GetPlayerProductionCost(game.GameDatabase, current.PlayerID, !current.isPrototyped, null) / 1000f).ToString("0.0K") + "]");
					}
					else
					{
						game.UI.SetItemPropertyString(designListId, string.Empty, current.ID, "designName", "text", current.Name);
					}
					if (current.IsLoaCube())
					{
						string itemGlobalID = game.UI.GetItemGlobalID(designListId, string.Empty, current.ID, "");
						game.UI.SetVisible(game.UI.Path(new string[]
						{
							itemGlobalID,
							"designDeleteButton"
						}), false);
					}
					game.UI.SetItemPropertyString(designListId, string.Empty, current.ID, "designDeleteButton", "id", "designDeleteButton|" + current.ID.ToString());
					if (!current.isPrototyped)
					{
						List<BuildOrderInfo> list = game.GameDatabase.GetDesignBuildOrders(current).ToList<BuildOrderInfo>();
						if (list.Count > 0)
						{
							game.UI.SetItemPropertyColor(designListId, string.Empty, current.ID, "designName", "color", new Vector3(0f, 80f, 104f));
						}
						else
						{
							game.UI.SetItemPropertyColor(designListId, string.Empty, current.ID, "designName", "color", new Vector3(147f, 64f, 147f));
						}
					}
					else
					{
						game.UI.SetItemPropertyColor(designListId, string.Empty, current.ID, "designName", "color", new Vector3(11f, 157f, 194f));
					}
				}
			}
		}
		public static int GetBuildInvoiceCost(App app, List<BuildScreenState.InvoiceItem> items)
		{
			int num = 0;
			foreach (BuildScreenState.InvoiceItem current in items)
			{
				DesignInfo designInfo = app.GameDatabase.GetDesignInfo(current.DesignID);
				num += BuildScreenState.GetDesignCost(app, designInfo, current.LoaCubes);
			}
			return num;
		}
		public static int GetDesignCost(App app, DesignInfo shipDesign, int loaCubes)
		{
			if (shipDesign == null)
			{
				return 0;
			}
			int result = shipDesign.SavingsCost;
			if (shipDesign.IsLoaCube())
			{
				result = loaCubes * app.AssetDatabase.LoaCostPerCube;
			}
			else
			{
				if (!shipDesign.isPrototyped)
				{
					switch (shipDesign.Class)
					{
					case ShipClass.Cruiser:
						result = (int)((float)shipDesign.SavingsCost * app.GetStratModifier<float>(StratModifiers.PrototypeSavingsCostModifierCR, shipDesign.PlayerID));
						break;
					case ShipClass.Dreadnought:
						result = (int)((float)shipDesign.SavingsCost * app.GetStratModifier<float>(StratModifiers.PrototypeSavingsCostModifierDN, shipDesign.PlayerID));
						break;
					case ShipClass.Leviathan:
						result = (int)((float)shipDesign.SavingsCost * app.GetStratModifier<float>(StratModifiers.PrototypeSavingsCostModifierLV, shipDesign.PlayerID));
						break;
					case ShipClass.Station:
						if (shipDesign.GetRealShipClass() == RealShipClasses.Platform)
						{
							result = (int)((float)shipDesign.SavingsCost * app.GetStratModifier<float>(StratModifiers.PrototypeSavingsCostModifierPF, shipDesign.PlayerID));
						}
						break;
					}
				}
			}
			return result;
		}
		private void SyncInvoiceItemsList(string parentPanel, string listPanel, List<BuildScreenState.InvoiceItem> items, string title, bool retrofitinvoice = false, int shipid = 0)
		{
			string listId = base.App.UI.Path(new string[]
			{
				parentPanel,
				listPanel
			});
			base.App.UI.SetPropertyString(BuildScreenState.UIInvoiceSummaryName, "text", title);
			base.App.UI.SetVisible("gameRandomizeSummaryShipNames", !retrofitinvoice);
			base.App.UI.ClearItems(listId);
			foreach (BuildScreenState.InvoiceItem current in items)
			{
				DesignInfo designInfo = base.App.GameDatabase.GetDesignInfo(current.DesignID);
				string propertyValue = string.Format("{0}{1}", listPanel, current.TempOrderID);
				string name = designInfo.Name;
				string value = designInfo.GetRealShipClass().LocalizeAbbr();
				base.App.UI.AddItem(listId, string.Empty, current.TempOrderID, string.Empty);
				base.App.UI.SetItemPropertyString(listId, string.Empty, current.TempOrderID, "class", "text", value);
				base.App.UI.SetItemPropertyString(listId, string.Empty, current.TempOrderID, "design", "text", current.isPrototypeOrder ? string.Format("{0} {1}", App.Localize("@UI_BUILD_PROTOTYPE"), name) : name);
				base.App.UI.SetItemPropertyString(listId, string.Empty, current.TempOrderID, "name", "text", current.ShipName);
				base.App.UI.SetItemPropertyString(listId, string.Empty, current.TempOrderID, "progress", "text", (current.Progress == -1) ? "" : string.Format("{0}%", current.Progress));
				string itemGlobalID = base.App.UI.GetItemGlobalID(listId, string.Empty, current.TempOrderID, string.Empty);
				base.App.UI.SetEnabled(base.App.UI.Path(new string[]
				{
					itemGlobalID,
					"name"
				}), !retrofitinvoice);
				base.App.UI.SetPropertyString(base.App.UI.Path(new string[]
				{
					itemGlobalID,
					"name"
				}), "id", propertyValue);
			}
			if (items.Count > 0)
			{
				if (!retrofitinvoice)
				{
					int buildInvoiceCost = BuildScreenState.GetBuildInvoiceCost(base.App, items);
					int buildTime = BuildScreenState.GetBuildTime(base.App, items, this._totalShipProductionRate);
					string text = (buildTime == 1) ? string.Format("1 {0}", App.Localize("@UI_GENERAL_TURN")) : string.Format("{0} {1}", buildTime, App.Localize("@UI_GENERAL_TURNS"));
					if (buildTime == 0)
					{
						text = "∞";
					}
					base.App.UI.SetText(base.App.UI.Path(new string[]
					{
						parentPanel,
						BuildScreenState.UIInvoiceTotalSavings
					}), buildInvoiceCost.ToString("N0"));
					base.App.UI.SetText(base.App.UI.Path(new string[]
					{
						parentPanel,
						BuildScreenState.UIInvoiceTotalTurns
					}), text);
					base.App.UI.SetEnabled(base.App.UI.Path(new string[]
					{
						parentPanel,
						BuildScreenState.UISubmitOrder
					}), true);
					return;
				}
				ShipInfo shipInfo = base.App.GameDatabase.GetShipInfo(shipid, true);
				if (shipInfo != null)
				{
                    int num = Kerberos.Sots.StarFleet.StarFleet.CalculateRetrofitCost(base.App, shipInfo.DesignInfo, Kerberos.Sots.StarFleet.StarFleet.GetNewestRetrofitDesign(shipInfo.DesignInfo, base.App.GameDatabase.GetVisibleDesignInfosForPlayer(base.App.LocalPlayer.ID)));
                    double timeRequiredToRetrofit = Kerberos.Sots.StarFleet.StarFleet.GetTimeRequiredToRetrofit(base.App, shipInfo, items.Count);
					string text2 = (timeRequiredToRetrofit == 1.0) ? string.Format("1 {0}", App.Localize("@UI_GENERAL_TURN")) : string.Format("{0} {1}", timeRequiredToRetrofit, App.Localize("@UI_GENERAL_TURNS"));
					if (timeRequiredToRetrofit == 0.0)
					{
						text2 = "∞";
					}
					base.App.UI.SetText(base.App.UI.Path(new string[]
					{
						parentPanel,
						BuildScreenState.UIInvoiceTotalSavings
					}), num.ToString("N0"));
					base.App.UI.SetText(base.App.UI.Path(new string[]
					{
						parentPanel,
						BuildScreenState.UIInvoiceTotalTurns
					}), text2);
					base.App.UI.SetEnabled(base.App.UI.Path(new string[]
					{
						parentPanel,
						BuildScreenState.UISubmitOrder
					}), true);
					return;
				}
			}
			else
			{
				base.App.UI.SetText(base.App.UI.Path(new string[]
				{
					parentPanel,
					BuildScreenState.UIInvoiceTotalSavings
				}), "0");
				base.App.UI.SetText(base.App.UI.Path(new string[]
				{
					parentPanel,
					BuildScreenState.UIInvoiceTotalTurns
				}), "0");
				base.App.UI.SetEnabled(base.App.UI.Path(new string[]
				{
					parentPanel,
					BuildScreenState.UISubmitOrder
				}), false);
			}
		}
		private void SyncProductionCost(App game, string panel, DesignInfo design)
		{
			string text = design.IsLoaCube() ? (this._loacubeval * game.AssetDatabase.LoaCostPerCube).ToString("N0") : design.SavingsCost.ToString("N0");
			string text2 = design.GetPlayerProductionCost(game.GameDatabase, game.LocalPlayer.ID, !design.isPrototyped, design.IsLoaCube() ? new float?((float)this._loacubeval) : null).ToString("N0");
			string text3 = GameSession.CalculateShipUpkeepCost(game.AssetDatabase, design, 1f, false).ToString("N0");
			string text4 = string.Format("({0})", GameSession.CalculateShipUpkeepCost(game.AssetDatabase, design, 1f, true).ToString("N0"));
			game.UI.SetText(game.UI.Path(new string[]
			{
				panel,
				"gameShipSavCost"
			}), text);
			game.UI.SetText(game.UI.Path(new string[]
			{
				panel,
				"gameShipConCost"
			}), text2);
			game.UI.SetText(game.UI.Path(new string[]
			{
				panel,
				"gameShipUpkeepCost"
			}), text3);
			game.UI.SetText(game.UI.Path(new string[]
			{
				panel,
				"gameShipResUpkeepCost"
			}), text4);
		}
		private void SubmitOrder(string invoiceName)
		{
			if (this._invoiceItems.Count == 0)
			{
				return;
			}
			int invoiceId = base.App.GameDatabase.InsertInvoice(invoiceName, base.App.LocalPlayer.ID, this._addToFavorites);
			int value = base.App.GameDatabase.InsertInvoiceInstance(base.App.LocalPlayer.ID, this._selectedSystem, invoiceName);
			foreach (BuildScreenState.InvoiceItem current in this._invoiceItems)
			{
				base.App.GameDatabase.InsertInvoiceBuildOrder(invoiceId, current.DesignID, current.ShipName, current.LoaCubes);
				DesignInfo designInfo = base.App.GameDatabase.GetDesignInfo(current.DesignID);
				bool flag;
				if (base.App.GameDatabase.canBuildDesignOrder(designInfo, this._selectedSystem, out flag))
				{
					base.App.GameDatabase.InsertBuildOrder(this._selectedSystem, current.DesignID, 0, 0, current.ShipName, designInfo.IsLoaCube() ? current.LoaCubes : designInfo.GetPlayerProductionCost(base.App.GameDatabase, base.App.LocalPlayer.ID, flag, null), new int?(value), null, current.LoaCubes);
					if (flag)
					{
						base.App.PostRequestSpeech(string.Format("STRAT_037-01_{0}_PrototypeOrderConfirm", base.App.LocalPlayer.Faction.Name), 50, 120, 0f);
					}
					else
					{
						base.App.PostRequestSpeech(string.Format("STRAT_033-01_{0}_OrderToBuildShipsConfirmation", base.App.LocalPlayer.Faction.Name), 50, 120, 0f);
					}
				}
			}
			base.App.PostRequestGuiSound("build_submitorder");
			this._invoiceItems.Clear();
			base.App.UI.ClearItems(BuildScreenState.UIInvoiceItemList);
			this.PopulateInvoiceList();
			this.PopulateClassList(this._selectedClass);
			this.PopulateDesignList(this._selectedClass);
			this.SyncInvoiceItemsList(BuildScreenState.UIOrderInvoiceItems, BuildScreenState.UIInvoiceItemList, this._invoiceItems, App.Localize("@UI_BUILD_NEW_INVOICE"), false, 0);
			this._addToFavorites = false;
			base.App.UI.SetChecked(BuildScreenState.UIAddToInvoiceFavorites, false);
			base.App.UI.SetEnabled(BuildScreenState.UISubmitOrder, this._invoiceItems.Count != 0);
		}
		public static int GetBuildTime(App app, IEnumerable<BuildScreenState.InvoiceItem> DesignIds, float productionRate)
		{
			if (productionRate < 1f)
			{
				return 0;
			}
			float stratModifierFloatToApply = app.GameDatabase.GetStratModifierFloatToApply(StratModifiers.PrototypeTimeModifier, app.LocalPlayer.ID);
			int num = 0;
			foreach (BuildScreenState.InvoiceItem current in DesignIds)
			{
				DesignInfo designInfo = app.GameDatabase.GetDesignInfo(current.DesignID);
				int num2 = designInfo.GetPlayerProductionCost(app.GameDatabase, app.LocalPlayer.ID, current.isPrototypeOrder, null);
				if (designInfo.IsLoaCube())
				{
					num2 = designInfo.GetPlayerProductionCost(app.GameDatabase, app.LocalPlayer.ID, current.isPrototypeOrder, new float?((float)current.LoaCubes));
				}
				if (current.isPrototypeOrder)
				{
					num2 = (int)((float)num2 * stratModifierFloatToApply);
				}
				num += num2;
			}
			return (int)Math.Ceiling((double)((float)num / productionRate));
		}
		private void PopulateInvoiceList()
		{
			List<InvoiceInstanceInfo> list = base.App.GameDatabase.GetInvoicesForSystem(base.App.LocalPlayer.ID, this._selectedSystem).ToList<InvoiceInstanceInfo>();
			base.App.UI.ClearItems(BuildScreenState.UIInvoiceList);
			foreach (InvoiceInstanceInfo current in list)
			{
				List<BuildOrderInfo> list2 = base.App.GameDatabase.GetBuildOrdersForInvoiceInstance(current.ID).ToList<BuildOrderInfo>();
				if (list2 != null && list2.Count > 0)
				{
					float totalShipProductionRate = this._totalShipProductionRate;
					int num = 0;
					int num2 = 0;
					foreach (BuildOrderInfo current2 in list2)
					{
						num2 += current2.Progress;
						num += current2.ProductionTarget;
					}
					int num3 = (int)Math.Ceiling((double)((float)(num - num2) / totalShipProductionRate));
					base.App.UI.AddItem(BuildScreenState.UIInvoiceList, string.Empty, current.ID, string.Empty);
					base.App.UI.SetItemPropertyString(BuildScreenState.UIInvoiceList, string.Empty, current.ID, "invoiceName", "text", current.Name);
					if (totalShipProductionRate < 1f)
					{
						base.App.UI.SetItemPropertyString(BuildScreenState.UIInvoiceList, string.Empty, current.ID, "invoiceTime", "text", string.Format("-", new object[0]));
					}
					else
					{
						if (num3 != 1)
						{
							base.App.UI.SetItemPropertyString(BuildScreenState.UIInvoiceList, string.Empty, current.ID, "invoiceTime", "text", string.Format("{0} {1}", num3, App.Localize("@UI_GENERAL_TURNS")));
						}
						else
						{
							base.App.UI.SetItemPropertyString(BuildScreenState.UIInvoiceList, string.Empty, current.ID, "invoiceTime", "text", string.Format("{0} {1}", num3, App.Localize("@UI_GENERAL_TURN")));
						}
					}
					string itemGlobalID = base.App.UI.GetItemGlobalID(BuildScreenState.UIInvoiceList, string.Empty, current.ID, string.Empty);
					if (num2 > 0)
					{
						float num4 = (float)num2 * 100f / (float)num;
						base.App.UI.SetVisible(base.App.UI.Path(new string[]
						{
							itemGlobalID,
							"invoicePercent"
						}), true);
						base.App.UI.SetItemPropertyString(BuildScreenState.UIInvoiceList, string.Empty, current.ID, "invoicePercent", "text", string.Format("{0:0}%", num4));
					}
					else
					{
						base.App.UI.SetVisible(base.App.UI.Path(new string[]
						{
							itemGlobalID,
							"invoicePercent"
						}), false);
					}
				}
				List<RetrofitOrderInfo> list3 = base.App.GameDatabase.GetRetrofitOrdersForInvoiceInstance(current.ID).ToList<RetrofitOrderInfo>();
				if (list3 != null && list3.Count > 0)
				{
					foreach (RetrofitOrderInfo arg_345_0 in list3)
					{
					}
					ShipInfo shipInfo = base.App.GameDatabase.GetShipInfo(list3[0].ShipID, true);
                    double timeRequiredToRetrofit = Kerberos.Sots.StarFleet.StarFleet.GetTimeRequiredToRetrofit(base.App, shipInfo, list3.Count);
					base.App.UI.AddItem(BuildScreenState.UIInvoiceList, string.Empty, current.ID, string.Empty);
					base.App.UI.SetItemPropertyString(BuildScreenState.UIInvoiceList, string.Empty, current.ID, "invoiceName", "text", current.Name);
					if (timeRequiredToRetrofit != 1.0)
					{
						base.App.UI.SetItemPropertyString(BuildScreenState.UIInvoiceList, string.Empty, current.ID, "invoiceTime", "text", string.Format("{0} {1}", timeRequiredToRetrofit, App.Localize("@UI_GENERAL_TURNS")));
					}
					else
					{
						base.App.UI.SetItemPropertyString(BuildScreenState.UIInvoiceList, string.Empty, current.ID, "invoiceTime", "text", string.Format("{0} {1}", timeRequiredToRetrofit, App.Localize("@UI_GENERAL_TURN")));
					}
					string itemGlobalID2 = base.App.UI.GetItemGlobalID(BuildScreenState.UIInvoiceList, string.Empty, current.ID, string.Empty);
					base.App.UI.SetVisible(base.App.UI.Path(new string[]
					{
						itemGlobalID2,
						"invoicePercent"
					}), false);
				}
			}
		}
		public bool OnKeyBindPressed(HotKeyManager.HotKeyActions action, string gamestates)
		{
			if (gamestates.Contains(base.Name))
			{
				if (base.App.UI.GetTopDialog() != null || this._confirmInvoiceDialogActive)
				{
					return false;
				}
				switch (action)
				{
				case HotKeyManager.HotKeyActions.State_Starmap:
					this._invoiceItems.Clear();
					base.App.UI.ClearItems(BuildScreenState.UIInvoiceItemList);
					base.App.UI.LockUI();
					base.App.SwitchGameState<StarMapState>(new object[0]);
					return true;
				case HotKeyManager.HotKeyActions.State_BuildScreen:
					return false;
				case HotKeyManager.HotKeyActions.State_DesignScreen:
					this._invoiceItems.Clear();
					base.App.UI.ClearItems(BuildScreenState.UIInvoiceItemList);
					base.App.UI.LockUI();
					base.App.SwitchGameState<DesignScreenState>(new object[]
					{
						false,
						base.Name
					});
					return true;
				case HotKeyManager.HotKeyActions.State_ResearchScreen:
					this._invoiceItems.Clear();
					base.App.UI.ClearItems(BuildScreenState.UIInvoiceItemList);
					base.App.UI.LockUI();
					base.App.SwitchGameState<ResearchScreenState>(new object[0]);
					return true;
				case HotKeyManager.HotKeyActions.State_ComparativeAnalysysScreen:
					return false;
				case HotKeyManager.HotKeyActions.State_EmpireSummaryScreen:
					this._invoiceItems.Clear();
					base.App.UI.ClearItems(BuildScreenState.UIInvoiceItemList);
					base.App.UI.LockUI();
					base.App.SwitchGameState<EmpireSummaryState>(new object[0]);
					return true;
				case HotKeyManager.HotKeyActions.State_SotspediaScreen:
					this._invoiceItems.Clear();
					base.App.UI.ClearItems(BuildScreenState.UIInvoiceItemList);
					base.App.UI.LockUI();
					base.App.SwitchGameState<SotspediaState>(new object[0]);
					return true;
				case HotKeyManager.HotKeyActions.State_DiplomacyScreen:
					this._invoiceItems.Clear();
					base.App.UI.ClearItems(BuildScreenState.UIInvoiceItemList);
					base.App.UI.LockUI();
					base.App.SwitchGameState<DiplomacyScreenState>(new object[0]);
					return true;
				}
			}
			return false;
		}
	}
}
