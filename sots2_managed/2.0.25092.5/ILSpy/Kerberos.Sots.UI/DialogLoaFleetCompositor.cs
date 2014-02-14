using Kerberos.Sots.Data;
using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Framework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.UI
{
	internal class DialogLoaFleetCompositor : Dialog
	{
		private class RiderStruct
		{
			public int CarrierDesignID;
			public int SelectedDesignCarrierKey;
			public List<DialogLoaFleetCompositor.RiderWingStruct> WingData;
			public RiderStruct()
			{
				this.WingData = new List<DialogLoaFleetCompositor.RiderWingStruct>();
			}
		}
		private class RiderWingStruct
		{
			public CarrierWingData wingdata;
			public List<int> riders;
			public RiderWingStruct()
			{
				this.riders = new List<int>();
			}
		}
		private const string designlist = "gameFleetList";
		private const string workingdesignlist = "gameWorkingFleet";
		private const string okbtn = "okButton";
		private const string singleokbtn = "single_okButton";
		private const string cancelbtn = "cancelButton";
		private const string UIClassDDL = "gameClassList";
		private const string UIMissingRole = "requirement";
		private const string UICommandPoints = "CommandPointValue";
		private const string UIConstructionPoint = "ConstructionPointValue";
		private const string UIRiderValue = "BattleRiderValue";
		private Dictionary<int, int> SelectedDesigns;
		private Dictionary<int, int> ListDesignMap;
		private List<DialogLoaFleetCompositor.RiderStruct> RiderListMap;
		private string namedialog = "";
		private string invoicename = "IHaveNoIdeaWhatImDoing";
		private DesignDetailsCard DesignDetailCard;
		private RealShipClasses SelectedClass;
		private MissionType _mission;
		private int _outputID;
		private static int _WorkingDesignListid;
		public DialogLoaFleetCompositor(App app, MissionType mission) : base(app, "dialogLoaFleetCompositor")
		{
			this._mission = mission;
		}
		public override void Initialize()
		{
			this.DesignDetailCard = new DesignDetailsCard(this._app, this._app.GameDatabase.GetDesignInfosForPlayer(this._app.LocalPlayer.ID, RealShipClasses.Cruiser, true).First<DesignInfo>().ID, null, base.UI, this._app.UI.Path(new string[]
			{
				base.ID,
				"DesignDetailsCard"
			}));
			this.SelectedDesigns = new Dictionary<int, int>();
			this.ListDesignMap = new Dictionary<int, int>();
			this.RiderListMap = new List<DialogLoaFleetCompositor.RiderStruct>();
			this.PopulateClassList(RealShipClasses.Cruiser);
			this.CheckMissionRequirements();
			this.SyncCompositionStats();
		}
		protected void SyncDesignListList(RealShipClasses shipclass)
		{
			this._app.UI.ClearItems(this._app.UI.Path(new string[]
			{
				base.ID,
				"gameFleetList"
			}));
			bool flag = false;
			foreach (DesignInfo current in this.GetAvailableDesigns(shipclass))
			{
				this._app.UI.AddItem(this._app.UI.Path(new string[]
				{
					base.ID,
					"gameFleetList"
				}), "", current.ID, current.Name);
				string itemGlobalID = this._app.UI.GetItemGlobalID(this._app.UI.Path(new string[]
				{
					base.ID,
					"gameFleetList"
				}), "", current.ID, "");
				if (!flag)
				{
					this._app.UI.SetSelection(this._app.UI.Path(new string[]
					{
						base.ID,
						"gameFleetList"
					}), current.ID);
					flag = true;
				}
				this._app.UI.SetPropertyColor(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"designName"
				}), "color", new Vector3(255f, 255f, 255f));
				string text = "";
				if (current.Class == ShipClass.BattleRider)
				{
					text = "   " + App.Localize(current.GetMissionSectionAsset().Title);
					if (!this.CanMountBattleRider(current))
					{
						this._app.UI.SetPropertyColor(this._app.UI.Path(new string[]
						{
							itemGlobalID,
							"designName"
						}), "color", new Vector3(255f, 0f, 0f));
					}
				}
				this._app.UI.SetText(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"designName"
				}), string.Concat(new string[]
				{
					current.Name,
					"  [",
					((float)current.GetPlayerProductionCost(this._app.GameDatabase, current.PlayerID, !current.isPrototyped, null) / 1000f).ToString("0.0K"),
					"]",
					text
				}));
				this._app.UI.SetVisible(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"designDeleteButton"
				}), false);
			}
		}
		protected void SyncDesignWorkingList()
		{
			this._app.UI.ClearItems(this._app.UI.Path(new string[]
			{
				base.ID,
				"gameWorkingFleet"
			}));
			this.ListDesignMap.Clear();
			foreach (int current in this.SelectedDesigns.Keys)
			{
				for (int i = 0; i < this.SelectedDesigns[current]; i++)
				{
					DesignInfo designInfo = this._app.GameDatabase.GetDesignInfo(current);
					this._app.UI.AddItem(this._app.UI.Path(new string[]
					{
						base.ID,
						"gameWorkingFleet"
					}), "", DialogLoaFleetCompositor._WorkingDesignListid, designInfo.Name);
					string itemGlobalID = this._app.UI.GetItemGlobalID(this._app.UI.Path(new string[]
					{
						base.ID,
						"gameWorkingFleet"
					}), "", DialogLoaFleetCompositor._WorkingDesignListid, "");
					this._app.UI.SetText(this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"designName"
					}), designInfo.Name);
					this._app.UI.SetVisible(this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"designDeleteButton"
					}), true);
					this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"designDeleteButton"
					}), "id", string.Concat(new string[]
					{
						"designDeleteButton|",
						designInfo.ID.ToString(),
						"|",
						DialogLoaFleetCompositor._WorkingDesignListid.ToString(),
						"|",
						i.ToString()
					}));
					this._app.UI.SetPropertyString(itemGlobalID, "id", designInfo.ID.ToString() + "|" + i.ToString());
					this.ListDesignMap.Add(DialogLoaFleetCompositor._WorkingDesignListid, designInfo.ID);
					DialogLoaFleetCompositor._WorkingDesignListid++;
				}
			}
			this.CheckMissionRequirements();
			this.SyncCompositionStats();
		}
		protected void SyncCompositionStats()
		{
			List<DesignInfo> list = new List<DesignInfo>();
			foreach (int current in this.SelectedDesigns.Keys)
			{
				for (int i = 0; i < this.SelectedDesigns[current]; i++)
				{
					DesignInfo designInfo = this._app.GameDatabase.GetDesignInfo(current);
					list.Add(designInfo);
				}
			}
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			foreach (DesignInfo current2 in list)
			{
				num2 += current2.CommandPointCost;
				if (this._app.GameDatabase.GetDesignCommandPointQuota(this._app.AssetDatabase, current2.ID) > num3)
				{
					num3 = this._app.GameDatabase.GetDesignCommandPointQuota(this._app.AssetDatabase, current2.ID);
				}
				num += current2.GetPlayerProductionCost(this._app.GameDatabase, this._app.LocalPlayer.ID, false, null);
			}
			this._app.UI.SetText(this._app.UI.Path(new string[]
			{
				base.ID,
				"CommandPointValue"
			}), num2.ToString("N0") + "/" + num3.ToString("N0"));
            int maxLoaFleetCubeMassForTransit = Kerberos.Sots.StarFleet.StarFleet.GetMaxLoaFleetCubeMassForTransit(this._app.Game, this._app.LocalPlayer.ID);
			this._app.UI.SetText(this._app.UI.Path(new string[]
			{
				base.ID,
				"ConstructionPointValue"
			}), num.ToString("N0") + "/" + maxLoaFleetCubeMassForTransit.ToString("N0"));
			int num4 = 0;
			int num5 = 0;
			foreach (DialogLoaFleetCompositor.RiderStruct current3 in this.RiderListMap)
			{
				foreach (DialogLoaFleetCompositor.RiderWingStruct current4 in current3.WingData)
				{
					num5 += current4.wingdata.SlotIndexes.Count;
					num4 += current4.riders.Count;
				}
			}
			this._app.UI.SetText(this._app.UI.Path(new string[]
			{
				base.ID,
				"BattleRiderValue"
			}), num4.ToString() + "/" + num5.ToString());
		}
		protected override void OnUpdate()
		{
		}
		private void commitdesigns()
		{
			List<int> list = new List<int>();
			foreach (int current in this.SelectedDesigns.Keys)
			{
				for (int i = 0; i < this.SelectedDesigns[current]; i++)
				{
					list.Add(current);
				}
			}
			this._outputID = this._app.GameDatabase.InsertLoaFleetComposition(this._app.LocalPlayer.ID, this.invoicename, list);
		}
		private void PopulateClassList(RealShipClasses shipClass)
		{
			List<RealShipClasses> list = this.GetAllowedShipClasses().ToList<RealShipClasses>();
			this._app.UI.ClearItems("gameClassList");
			foreach (RealShipClasses current in list)
			{
				if (this.GetAvailableDesigns(current).Count<DesignInfo>() > 0)
				{
					this._app.UI.AddItem("gameClassList", string.Empty, (int)current, current.Localize());
				}
			}
			this.SelectedClass = shipClass;
			this._app.UI.SetSelection("gameClassList", (int)shipClass);
			this.SyncDesignListList(shipClass);
		}
		private IEnumerable<DesignInfo> GetAvailableDesigns(RealShipClasses shipClass)
		{
			IEnumerable<DesignInfo> visibleDesignInfosForPlayer = this._app.GameDatabase.GetVisibleDesignInfosForPlayer(this._app.LocalPlayer.ID, shipClass);
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
		private IEnumerable<RealShipClasses> GetAllowedShipClasses()
		{
			try
			{
				RealShipClasses[] realShipClasses = ShipClassExtensions.RealShipClasses;
				for (int i = 0; i < realShipClasses.Length; i++)
				{
					RealShipClasses realShipClasses2 = realShipClasses[i];
					if (DialogLoaFleetCompositor.IsShipClassAllowed(new RealShipClasses?(realShipClasses2)))
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
            return DialogLoaFleetCompositor.IsShipClassAllowed(designInfo.GetRealShipClass()) && DialogLoaFleetCompositor.IsShipRoleAllowed(designInfo.Role) && !Kerberos.Sots.StarFleet.StarFleet.DesignIsSuulka(this._app, designInfo) && designInfo.isPrototyped;
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
				switch (role)
				{
				case ShipRole.ACCELERATOR_GATE:
				case ShipRole.LOA_CUBE:
					break;
				default:
					return true;
				}
				break;
			}
			return false;
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
				return true;
			case RealShipClasses.Drone:
			case RealShipClasses.BoardingPod:
			case RealShipClasses.EscapePod:
			case RealShipClasses.AssaultShuttle:
			case RealShipClasses.Biomissile:
			case RealShipClasses.Station:
			case RealShipClasses.Platform:
			case RealShipClasses.SystemDefenseBoat:
			case RealShipClasses.NumShipClasses:
				return false;
			default:
				throw new ArgumentOutOfRangeException("value");
			}
		}
		private void CheckMissionRequirements()
		{
			ShipRole shipRole = this.CheckRequiredShips();
			List<DesignInfo> list = new List<DesignInfo>();
			foreach (int current in this.SelectedDesigns.Keys)
			{
				for (int i = 0; i < this.SelectedDesigns[current]; i++)
				{
					DesignInfo designInfo = this._app.GameDatabase.GetDesignInfo(current);
					list.Add(designInfo);
				}
			}
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			foreach (DesignInfo current2 in list)
			{
				num2 += current2.CommandPointCost;
				if (this._app.GameDatabase.GetDesignCommandPointQuota(this._app.AssetDatabase, current2.ID) > num3)
				{
					num3 = this._app.GameDatabase.GetDesignCommandPointQuota(this._app.AssetDatabase, current2.ID);
				}
				num += current2.GetPlayerProductionCost(this._app.GameDatabase, this._app.LocalPlayer.ID, false, null);
			}
			if (num3 < num2)
			{
				if (num3 < num2)
				{
					this._app.UI.SetEnabled(this._app.UI.Path(new string[]
					{
						base.ID,
						"okButton"
					}), false);
					this._app.UI.SetText(this._app.UI.Path(new string[]
					{
						base.ID,
						"requirement"
					}), "Not Enough CP to support Fleet");
				}
				return;
			}
			this._app.UI.SetEnabled(this._app.UI.Path(new string[]
			{
				base.ID,
				"okButton"
			}), false);
			switch (shipRole)
			{
			case ShipRole.COMMAND:
				this._app.UI.SetText(this._app.UI.Path(new string[]
				{
					base.ID,
					"requirement"
				}), "Fleet Requires Command Ship");
				return;
			case ShipRole.COLONIZER:
				this._app.UI.SetText(this._app.UI.Path(new string[]
				{
					base.ID,
					"requirement"
				}), "Fleet Requires Colonizer");
				return;
			case ShipRole.CONSTRUCTOR:
				this._app.UI.SetText(this._app.UI.Path(new string[]
				{
					base.ID,
					"requirement"
				}), "Fleet Requires Construction Ship");
				return;
			default:
				this._app.UI.SetText(this._app.UI.Path(new string[]
				{
					base.ID,
					"requirement"
				}), "");
				this._app.UI.SetEnabled(this._app.UI.Path(new string[]
				{
					base.ID,
					"okButton"
				}), true);
				return;
			}
		}
		private ShipRole CheckRequiredShips()
		{
			List<ShipRole> list = new List<ShipRole>();
			list.Add(ShipRole.COMMAND);
			if (this._mission == MissionType.CONSTRUCT_STN || this._mission == MissionType.UPGRADE_STN || this._mission == MissionType.SPECIAL_CONSTRUCT_STN)
			{
				list.Add(ShipRole.CONSTRUCTOR);
			}
			if (this._mission == MissionType.COLONIZATION || this._mission == MissionType.SUPPORT || this._mission == MissionType.EVACUATE)
			{
				list.Add(ShipRole.COLONIZER);
			}
			foreach (int current in this.SelectedDesigns.Keys)
			{
				if (this.SelectedDesigns[current] > 0)
				{
					DesignInfo designInfo = this._app.GameDatabase.GetDesignInfo(current);
					if (list.Contains(designInfo.Role))
					{
						list.Remove(designInfo.Role);
					}
				}
			}
			return list.FirstOrDefault<ShipRole>();
		}
		public bool CompositionCanSupportShip(int designid)
		{
			return true;
		}
		private void RemoveDesign(int designid, bool listid = true)
		{
			int removedinex = 0;
			if (listid)
			{
				removedinex = this.SelectedDesigns[this.ListDesignMap[designid]];
				if (this.SelectedDesigns.ContainsKey(this.ListDesignMap[designid]))
				{
					Dictionary<int, int> selectedDesigns;
					int key;
					(selectedDesigns = this.SelectedDesigns)[key = this.ListDesignMap[designid]] = selectedDesigns[key] - 1;
				}
				designid = this.ListDesignMap[designid];
			}
			else
			{
				if (this.SelectedDesigns.ContainsKey(designid))
				{
					Dictionary<int, int> selectedDesigns2;
					int designid2;
					(selectedDesigns2 = this.SelectedDesigns)[designid2 = designid] = selectedDesigns2[designid2] - 1;
				}
			}
			bool flag = false;
			foreach (DialogLoaFleetCompositor.RiderStruct current in 
				from x in this.RiderListMap
				where x.CarrierDesignID == designid && x.SelectedDesignCarrierKey == removedinex
				select x)
			{
				flag = true;
				foreach (DialogLoaFleetCompositor.RiderWingStruct current2 in current.WingData)
				{
					using (List<int>.Enumerator enumerator3 = current2.riders.GetEnumerator())
					{
						if (enumerator3.MoveNext())
						{
							int current3 = enumerator3.Current;
							this.RemoveDesign(current3, false);
						}
					}
				}
			}
			if (flag)
			{
				this.RiderListMap.Remove(this.RiderListMap.First((DialogLoaFleetCompositor.RiderStruct x) => x.CarrierDesignID == designid && x.SelectedDesignCarrierKey == removedinex));
			}
			DesignInfo designInfo = this._app.GameDatabase.GetDesignInfo(designid);
			if (designInfo.Class == ShipClass.BattleRider)
			{
				WeaponEnums.TurretClasses? turretclass = StrategicAI.BattleRiderMountSet.GetMatchingTurretClass(designInfo);
				DialogLoaFleetCompositor.RiderStruct riderStruct = this.RiderListMap.FirstOrDefault((DialogLoaFleetCompositor.RiderStruct x) => x.WingData.Any((DialogLoaFleetCompositor.RiderWingStruct j) => j.wingdata.Class == turretclass && j.riders.Any((int k) => k == designid)));
				if (riderStruct != null)
				{
					DialogLoaFleetCompositor.RiderWingStruct riderWingStruct = riderStruct.WingData.First((DialogLoaFleetCompositor.RiderWingStruct x) => x.wingdata.Class == turretclass && x.riders.Any((int k) => k == designid));
					foreach (int arg_25C_0 in riderWingStruct.wingdata.SlotIndexes)
					{
						riderWingStruct.riders.Remove(designid);
					}
					Dictionary<int, int> selectedDesigns3;
					int designid3;
					(selectedDesigns3 = this.SelectedDesigns)[designid3 = designid] = selectedDesigns3[designid3] - (riderWingStruct.wingdata.SlotIndexes.Count - 1);
				}
				if (this.SelectedClass == RealShipClasses.BattleRider || this.SelectedClass == RealShipClasses.BattleShip || this.SelectedClass == RealShipClasses.BattleCruiser)
				{
					this.SyncDesignListList(this.SelectedClass);
				}
			}
		}
		private bool CanMountBattleRider(DesignInfo design)
		{
			WeaponEnums.TurretClasses? turretclass = StrategicAI.BattleRiderMountSet.GetMatchingTurretClass(design);
			DialogLoaFleetCompositor.RiderStruct riderStruct = this.RiderListMap.FirstOrDefault((DialogLoaFleetCompositor.RiderStruct x) => x.WingData.Any((DialogLoaFleetCompositor.RiderWingStruct j) => j.wingdata.Class == turretclass && !j.riders.Any<int>()));
			return riderStruct != null;
		}
		private void AddDesign(int designid)
		{
			DesignInfo designInfo = this._app.GameDatabase.GetDesignInfo(designid);
			if (designInfo.Class == ShipClass.BattleRider && !this.CanMountBattleRider(designInfo))
			{
				return;
			}
			if (!this.SelectedDesigns.ContainsKey(designid))
			{
				this.SelectedDesigns.Add(designid, 1);
			}
			else
			{
				Dictionary<int, int> selectedDesigns;
				(selectedDesigns = this.SelectedDesigns)[designid] = selectedDesigns[designid] + 1;
			}
			int selectedDesignCarrierKey = this.SelectedDesigns[designid];
			List<CarrierWingData> list = RiderManager.GetDesignBattleriderWingData(this._app, designInfo).ToList<CarrierWingData>();
			if (list.Any<CarrierWingData>())
			{
				DialogLoaFleetCompositor.RiderStruct riderStruct = new DialogLoaFleetCompositor.RiderStruct();
				foreach (CarrierWingData current in list)
				{
					DialogLoaFleetCompositor.RiderWingStruct riderWingStruct = new DialogLoaFleetCompositor.RiderWingStruct();
					riderWingStruct.wingdata = current;
					riderStruct.WingData.Add(riderWingStruct);
				}
				riderStruct.CarrierDesignID = designid;
				riderStruct.SelectedDesignCarrierKey = selectedDesignCarrierKey;
				this.RiderListMap.Add(riderStruct);
			}
			if (designInfo.Class == ShipClass.BattleRider)
			{
				WeaponEnums.TurretClasses? turretclass = StrategicAI.BattleRiderMountSet.GetMatchingTurretClass(designInfo);
				DialogLoaFleetCompositor.RiderStruct riderStruct2 = this.RiderListMap.FirstOrDefault((DialogLoaFleetCompositor.RiderStruct x) => x.WingData.Any((DialogLoaFleetCompositor.RiderWingStruct j) => j.wingdata.Class == turretclass && !j.riders.Any<int>()));
				if (riderStruct2 != null)
				{
					DialogLoaFleetCompositor.RiderWingStruct riderWingStruct2 = riderStruct2.WingData.First((DialogLoaFleetCompositor.RiderWingStruct x) => x.wingdata.Class == turretclass && !x.riders.Any<int>());
					foreach (int arg_172_0 in riderWingStruct2.wingdata.SlotIndexes)
					{
						riderWingStruct2.riders.Add(designid);
					}
					Dictionary<int, int> selectedDesigns2;
					(selectedDesigns2 = this.SelectedDesigns)[designid] = selectedDesigns2[designid] + (riderWingStruct2.wingdata.SlotIndexes.Count - 1);
				}
				if (this.SelectedClass == RealShipClasses.BattleRider || this.SelectedClass == RealShipClasses.BattleShip || this.SelectedClass == RealShipClasses.BattleCruiser)
				{
					this.SyncDesignListList(this.SelectedClass);
				}
			}
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "list_item_dblclk")
			{
				if (panelName == "gameFleetList")
				{
					int designid = int.Parse(msgParams[0]);
					this.AddDesign(designid);
					this.SyncDesignWorkingList();
					return;
				}
				if (panelName == "gameWorkingFleet")
				{
					string[] array = msgParams[0].Split(new char[]
					{
						'|'
					});
					int designid2 = int.Parse(array[0]);
					this.RemoveDesign(designid2, true);
					this.SyncDesignWorkingList();
					return;
				}
			}
			else
			{
				if (msgType == "list_sel_changed")
				{
					if (panelName == "gameFleetList")
					{
						string[] array2 = msgParams[0].Split(new char[]
						{
							'|'
						});
						int designID = int.Parse(array2[0]);
						this.DesignDetailCard.SyncDesign(designID, null);
						return;
					}
					if (panelName == "gameWorkingFleet")
					{
						string[] array3 = msgParams[0].Split(new char[]
						{
							'|'
						});
						int key = int.Parse(array3[0]);
						if (this.ListDesignMap.ContainsKey(key))
						{
							this.DesignDetailCard.SyncDesign(this.ListDesignMap[key], null);
							return;
						}
					}
					else
					{
						if (panelName == "gameClassList")
						{
							RealShipClasses realShipClasses = (RealShipClasses)int.Parse(msgParams[0]);
							if (this.SelectedClass != realShipClasses)
							{
								this.PopulateClassList(realShipClasses);
								return;
							}
						}
					}
				}
				else
				{
					if (msgType == "button_clicked")
					{
						if (panelName == "okButton" || panelName == "single_okButton")
						{
							this.namedialog = this._app.UI.CreateDialog(new GenericTextEntryDialog(this._app, "Enter Name for Composition", "input a name for your fleet composition", "Composition", 1024, 3, false, EditBoxFilterMode.None), null);
							return;
						}
						if (panelName.StartsWith("designDeleteButton"))
						{
							string[] array4 = panelName.Split(new char[]
							{
								'|'
							});
							int designid3 = int.Parse(array4[2]);
							this.RemoveDesign(designid3, true);
							this.SyncDesignWorkingList();
							return;
						}
						if (panelName == "cancelButton")
						{
							this._app.UI.CloseDialog(this, true);
							return;
						}
					}
					else
					{
						if (msgType == "dialog_closed" && panelName == this.namedialog && bool.Parse(msgParams[0]))
						{
							this.invoicename = msgParams[1];
							this.commitdesigns();
							this._app.UI.CloseDialog(this, true);
						}
					}
				}
			}
		}
		public override string[] CloseDialog()
		{
			return new string[]
			{
				this._outputID.ToString()
			};
		}
	}
}
