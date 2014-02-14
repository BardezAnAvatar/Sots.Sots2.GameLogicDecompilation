using Kerberos.Sots.Data;
using Kerberos.Sots.Data.ModuleFramework;
using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.UI
{
	[GameObjectType(InteropGameObjectType.IGOT_RIDERMANAGER)]
	internal class RiderManager : GameObject, IDisposable
	{
		private new App App;
		private bool _ready;
		private List<int> _syncedFleets;
		private bool _contentChanged;
		public RiderManager(App game, string rootPanel)
		{
			this.App = game;
			game.AddExistingObject(this, new object[]
			{
				rootPanel
			});
			this.App.UI.GameEvent += new UIEventGameEvent(this.UICommChannel_GameEvent);
			this.App.UI.PanelMessage += new UIEventPanelMessage(this.UICommChannel_OnPanelMessage);
		}
		public void SetSyncedFleets(List<FleetInfo> fleets)
		{
			List<int> list = new List<int>();
			foreach (FleetInfo current in fleets)
			{
				list.Add(current.ID);
			}
			this.SetSyncedFleets(list);
		}
		public void SetSyncedFleets(List<int> fleets)
		{
			this._syncedFleets = fleets;
			this._contentChanged = true;
			this.Refresh();
		}
		private void Refresh()
		{
			if (this._contentChanged && this._ready)
			{
				foreach (int current in this._syncedFleets)
				{
					this.SyncFleet(current);
				}
				this._contentChanged = false;
			}
		}
		private void ClearFleets()
		{
			this.PostSetProp("ClearFleets", new object[0]);
		}
		private void UICommChannel_GameEvent(string eventName, string[] eventParams)
		{
			if (eventName == "RiderManagerReady")
			{
				this._ready = true;
				this.Refresh();
				return;
			}
			if (eventName == "RiderParentEvent")
			{
				int shipID = int.Parse(eventParams[0]);
				int num = int.Parse(eventParams[1]);
				int index = int.Parse(eventParams[2]);
				this.App.GameDatabase.SetShipParent(shipID, num);
				this.App.GameDatabase.UpdateShipRiderIndex(shipID, index);
				ShipInfo shipInfo = this.App.GameDatabase.GetShipInfo(num, false);
				if (shipInfo != null)
				{
					this.App.GameDatabase.TransferShip(shipID, shipInfo.FleetID);
				}
				else
				{
					ShipInfo shipInfo2 = this.App.GameDatabase.GetShipInfo(shipID, false);
					if (shipInfo2 != null)
					{
						FleetInfo fleetInfo = this.App.GameDatabase.GetFleetInfo(shipInfo2.FleetID);
						if (fleetInfo != null)
						{
							this.App.GameDatabase.TransferShip(shipID, this.App.GameDatabase.InsertOrGetReserveFleetInfo(fleetInfo.SystemID, this.App.LocalPlayer.ID).ID);
						}
					}
				}
				this.Refresh();
			}
		}
		protected void UICommChannel_OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked" && panelName.StartsWith("btnRemoveRider"))
			{
				string[] array = panelName.Split(new char[]
				{
					'|'
				});
				int num = int.Parse(array[1]);
				this.App.GameDatabase.RemoveShip(num);
				this.PostSetProp("RemoveRider", num);
			}
		}
		public List<int> GetSyncedShips()
		{
			List<int> list = new List<int>();
			foreach (int current in this._syncedFleets)
			{
				this.App.GameDatabase.GetFleetInfo(current);
				IEnumerable<ShipInfo> shipInfoByFleetID = this.App.GameDatabase.GetShipInfoByFleetID(current, false);
				foreach (ShipInfo current2 in shipInfoByFleetID)
				{
					DesignInfo designInfo = current2.DesignInfo;
					int num = 0;
					BattleRiderTypes type = BattleRiderTypes.Unspecified;
					DesignSectionInfo[] designSections = designInfo.DesignSections;
					for (int i = 0; i < designSections.Length; i++)
					{
						DesignSectionInfo designSectionInfo = designSections[i];
						ShipSectionAsset shipSectionAsset = designSectionInfo.ShipSectionAsset;
						if (shipSectionAsset.BattleRiderType != BattleRiderTypes.Unspecified)
						{
							type = shipSectionAsset.BattleRiderType;
						}
						num += RiderManager.GetNumRiderSlots(this.App, designSectionInfo);
					}
					int num2 = 0;
					DesignSectionInfo[] designSections2 = designInfo.DesignSections;
					for (int j = 0; j < designSections2.Length; j++)
					{
						DesignSectionInfo designSectionInfo2 = designSections2[j];
						ShipSectionAsset shipSectionAsset2 = designSectionInfo2.ShipSectionAsset;
						num2 += shipSectionAsset2.ReserveSize;
					}
					if (num2 > 0 || num > 0 || type.IsBattleRiderType())
					{
						list.Add(current2.ID);
					}
				}
			}
			return list;
		}
		public static bool IsRiderMount(LogicalMount mount)
		{
			return RiderManager.IsRiderBank(mount.Bank);
		}
		public static bool IsRiderBank(LogicalBank bank)
		{
			switch (bank.TurretClass)
			{
			case WeaponEnums.TurretClasses.DestroyerRider:
			case WeaponEnums.TurretClasses.CruiserRider:
			case WeaponEnums.TurretClasses.DreadnoughtRider:
				return true;
			default:
				return false;
			}
		}
		public static int GetNumRiderSlots(App game, DesignSectionInfo info)
		{
			ShipSectionAsset shipSectionAsset = game.AssetDatabase.GetShipSectionAsset(info.FilePath);
			int num = 0;
			LogicalMount[] mounts = shipSectionAsset.Mounts;
			for (int i = 0; i < mounts.Length; i++)
			{
				LogicalMount mount = mounts[i];
				if (RiderManager.IsRiderMount(mount))
				{
					num++;
				}
			}
			foreach (DesignModuleInfo current in info.Modules)
			{
				string path = game.GameDatabase.GetModuleAsset(current.ModuleID);
				LogicalModule logicalModule = game.AssetDatabase.Modules.FirstOrDefault((LogicalModule x) => x.ModulePath == path);
				LogicalMount[] mounts2 = logicalModule.Mounts;
				for (int j = 0; j < mounts2.Length; j++)
				{
					LogicalMount mount2 = mounts2[j];
					if (RiderManager.IsRiderMount(mount2))
					{
						num++;
					}
				}
			}
			return num;
		}
        public static IEnumerable<int> GetBattleriderIndexes(App app, ShipInfo ship)
        {
            List<SectionInstanceInfo> source = app.GameDatabase.GetShipSectionInstances(ship.ID).ToList<SectionInstanceInfo>();
            List<DesignSectionInfo> sections = app.GameDatabase.GetShipInfo(ship.ID, true).DesignInfo.DesignSections.ToList<DesignSectionInfo>();
            List<int> list2 = new List<int>();
            int item = 0;
            Func<SectionInstanceInfo, bool> predicate = null;
            for (int j = 0; j < sections.Count; j++)
            {
                if (predicate == null)
                {
                    predicate = x => x.SectionID == sections[j].ID;
                }
                SectionInstanceInfo info = source.First<SectionInstanceInfo>(predicate);
                List<ModuleInstanceInfo> list3 = app.GameDatabase.GetModuleInstances(info.ID).ToList<ModuleInstanceInfo>();
                foreach (LogicalMount mount in app.AssetDatabase.GetShipSectionAsset(sections[j].FilePath).Mounts)
                {
                    if (WeaponEnums.IsWeaponBattleRider(mount.Bank.TurretClass))
                    {
                        item++;
                    }
                    else if (WeaponEnums.IsBattleRider(mount.Bank.TurretClass))
                    {
                        list2.Add(item);
                        item++;
                    }
                }
                if (list3.Count > 0)
                {
                    using (List<ModuleInstanceInfo>.Enumerator enumerator = list3.GetEnumerator())
                    {
                        Func<DesignModuleInfo, bool> func = null;
                        ModuleInstanceInfo mii;
                        while (enumerator.MoveNext())
                        {
                            mii = enumerator.Current;
                            if (func == null)
                            {
                                func = x => x.MountNodeName == mii.ModuleNodeID;
                            }
                            DesignModuleInfo info2 = sections[j].Modules.First<DesignModuleInfo>(func);
                            if (info2.DesignID.HasValue)
                            {
                                string modAsset = app.GameDatabase.GetModuleAsset(info2.ModuleID);
                                foreach (LogicalMount mount2 in (from x in app.AssetDatabase.Modules
                                                                 where x.ModulePath == modAsset
                                                                 select x).First<LogicalModule>().Mounts)
                                {
                                    if (WeaponEnums.IsWeaponBattleRider(mount2.Bank.TurretClass))
                                    {
                                        item++;
                                    }
                                    else if (WeaponEnums.IsBattleRider(mount2.Bank.TurretClass))
                                    {
                                        list2.Add(item);
                                        item++;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return list2;
        }
        public static IEnumerable<CarrierWingData> GetDesignBattleriderWingData(App App, DesignInfo des)
        {
            int num = 0;
            int item = 0;
            List<CarrierWingData> source = new List<CarrierWingData>();
            foreach (DesignSectionInfo info in des.DesignSections)
            {
                ShipSectionAsset shipSectionAsset = App.AssetDatabase.GetShipSectionAsset(info.FilePath);
                if (shipSectionAsset.Type == ShipSectionType.Mission)
                {
                    BattleRiderTypes battleRiderType = shipSectionAsset.BattleRiderType;
                }
                num += GetNumRiderSlots(App, info);
                Func<LogicalMount, bool> predicate = null;
                Func<LogicalMount, bool> func3 = null;
                foreach (LogicalBank bank in shipSectionAsset.Banks)
                {
                    if (IsRiderBank(bank))
                    {
                        Func<CarrierWingData, bool> func = null;
                        if (predicate == null)
                        {
                            predicate = x => x.Bank == bank;
                        }
                        List<LogicalMount> list2 = shipSectionAsset.Mounts.Where<LogicalMount>(predicate).ToList<LogicalMount>();
                        WeaponEnums.TurretClasses mountClass = bank.TurretClass;
                        int count = list2.Count;
                        int minRiderSlotsPerSquad = BattleRiderSquad.GetMinRiderSlotsPerSquad(mountClass, des.Class);
                        int numRidersPerSquad = BattleRiderSquad.GetNumRidersPerSquad(mountClass, des.Class, Math.Max(count, minRiderSlotsPerSquad));
                        int num5 = (numRidersPerSquad > count) ? 1 : (count / numRidersPerSquad);
                        for (int i = 0; i < num5; i++)
                        {
                            int num7 = Math.Min(count, numRidersPerSquad);
                            List<int> collection = new List<int>();
                            for (int j = 0; j < num7; j++)
                            {
                                collection.Add(item);
                                item++;
                            }
                            if (func == null)
                            {
                                func = x => (x.Class == mountClass) && (x.SlotIndexes.Count < numRidersPerSquad);
                            }
                            CarrierWingData data = source.FirstOrDefault<CarrierWingData>(func);
                            if (data != null)
                            {
                                data.SlotIndexes.AddRange(collection);
                            }
                            else
                            {
                                CarrierWingData data2 = new CarrierWingData
                                {
                                    SlotIndexes = collection,
                                    Class = mountClass
                                };
                                source.Add(data2);
                            }
                        }
                    }
                    else if (WeaponEnums.IsWeaponBattleRider(bank.TurretClass))
                    {
                        if (func3 == null)
                        {
                            func3 = x => x.Bank == bank;
                        }
                        item += shipSectionAsset.Mounts.Count<LogicalMount>(func3);
                    }
                }
                foreach (DesignModuleInfo info2 in info.Modules)
                {
                    string path = App.GameDatabase.GetModuleAsset(info2.ModuleID);
                    LogicalModule module = App.AssetDatabase.Modules.FirstOrDefault<LogicalModule>(x => x.ModulePath == path);
                    Func<LogicalMount, bool> func4 = null;
                    Func<LogicalMount, bool> func5 = null;
                    foreach (LogicalBank bank in module.Banks)
                    {
                        if (IsRiderBank(bank))
                        {
                            if (func4 == null)
                            {
                                func4 = x => x.Bank == bank;
                            }
                            int num9 = module.Mounts.Where<LogicalMount>(func4).ToList<LogicalMount>().Count;
                            List<int> list5 = new List<int>();
                            for (int k = 0; k < num9; k++)
                            {
                                list5.Add(item);
                                item++;
                            }
                            CarrierWingData data3 = new CarrierWingData
                            {
                                SlotIndexes = list5,
                                Class = bank.TurretClass,
                                DefaultType = (module.AbilityType == ModuleEnums.ModuleAbilities.KingfisherRider) ? BattleRiderTypes.scout : BattleRiderTypes.Unspecified
                            };
                            source.Add(data3);
                        }
                        else if (WeaponEnums.IsWeaponBattleRider(bank.TurretClass))
                        {
                            if (func5 == null)
                            {
                                func5 = x => x.Bank == bank;
                            }
                            item += module.Mounts.Count<LogicalMount>(func5);
                        }
                    }
                }
            }
            return source;
        }
        private void SyncFleet(int fleetID)
        {
            this.App.GameDatabase.GetFleetInfo(fleetID);
            IEnumerable<ShipInfo> shipInfoByFleetID = this.App.GameDatabase.GetShipInfoByFleetID(fleetID, false);
            List<object> list = new List<object>();
            int item = 0;
            foreach (ShipInfo info in shipInfoByFleetID)
            {
                DesignInfo designInfo = info.DesignInfo;
                int num2 = 0;
                int count = list.Count;
                BattleRiderTypes unspecified = BattleRiderTypes.Unspecified;
                int num4 = 0;
                List<CarrierWingData> source = new List<CarrierWingData>();
                foreach (DesignSectionInfo info3 in designInfo.DesignSections)
                {
                    ShipSectionAsset shipSectionAsset = this.App.AssetDatabase.GetShipSectionAsset(info3.FilePath);
                    if (shipSectionAsset.Type == ShipSectionType.Mission)
                    {
                        unspecified = shipSectionAsset.BattleRiderType;
                    }
                    num2 += GetNumRiderSlots(this.App, info3);
                    Func<LogicalMount, bool> predicate = null;
                    Func<LogicalMount, bool> func3 = null;
                    foreach (LogicalBank bank in shipSectionAsset.Banks)
                    {
                        if (IsRiderBank(bank))
                        {
                            Func<CarrierWingData, bool> func = null;
                            if (predicate == null)
                            {
                                predicate = x => x.Bank == bank;
                            }
                            List<LogicalMount> list3 = shipSectionAsset.Mounts.Where<LogicalMount>(predicate).ToList<LogicalMount>();
                            WeaponEnums.TurretClasses mountClass = bank.TurretClass;
                            int num5 = list3.Count;
                            int minRiderSlotsPerSquad = BattleRiderSquad.GetMinRiderSlotsPerSquad(mountClass, designInfo.Class);
                            int numRidersPerSquad = BattleRiderSquad.GetNumRidersPerSquad(mountClass, designInfo.Class, Math.Max(num5, minRiderSlotsPerSquad));
                            int num7 = (numRidersPerSquad > num5) ? 1 : (num5 / numRidersPerSquad);
                            for (int i = 0; i < num7; i++)
                            {
                                int num9 = Math.Min(num5, numRidersPerSquad);
                                List<int> collection = new List<int>();
                                for (int j = 0; j < num9; j++)
                                {
                                    collection.Add(num4);
                                    num4++;
                                }
                                if (func == null)
                                {
                                    func = x => (x.Class == mountClass) && (x.SlotIndexes.Count < numRidersPerSquad);
                                }
                                CarrierWingData data = source.FirstOrDefault<CarrierWingData>(func);
                                if (data != null)
                                {
                                    data.SlotIndexes.AddRange(collection);
                                }
                                else
                                {
                                    CarrierWingData data2 = new CarrierWingData
                                    {
                                        SlotIndexes = collection,
                                        Class = mountClass
                                    };
                                    source.Add(data2);
                                }
                            }
                            using (List<LogicalMount>.Enumerator enumerator2 = list3.GetEnumerator())
                            {
                                while (enumerator2.MoveNext())
                                {
                                    LogicalMount current = enumerator2.Current;
                                    list.Add((int)bank.TurretClass);
                                }
                                continue;
                            }
                        }
                        if (WeaponEnums.IsWeaponBattleRider(bank.TurretClass))
                        {
                            if (func3 == null)
                            {
                                func3 = x => x.Bank == bank;
                            }
                            num4 += shipSectionAsset.Mounts.Count<LogicalMount>(func3);
                        }
                    }
                    foreach (DesignModuleInfo info4 in info3.Modules)
                    {
                        string path = this.App.GameDatabase.GetModuleAsset(info4.ModuleID);
                        LogicalModule module = this.App.AssetDatabase.Modules.FirstOrDefault<LogicalModule>(x => x.ModulePath == path);
                        Func<LogicalMount, bool> func4 = null;
                        Func<LogicalMount, bool> func5 = null;
                        foreach (LogicalBank bank in module.Banks)
                        {
                            if (IsRiderBank(bank))
                            {
                                if (func4 == null)
                                {
                                    func4 = x => x.Bank == bank;
                                }
                                List<LogicalMount> list5 = module.Mounts.Where<LogicalMount>(func4).ToList<LogicalMount>();
                                int num11 = list5.Count;
                                List<int> list6 = new List<int>();
                                for (int k = 0; k < num11; k++)
                                {
                                    list6.Add(num4);
                                    num4++;
                                }
                                CarrierWingData data3 = new CarrierWingData
                                {
                                    SlotIndexes = list6,
                                    Class = bank.TurretClass,
                                    DefaultType = (module.AbilityType == ModuleEnums.ModuleAbilities.KingfisherRider) ? BattleRiderTypes.scout : BattleRiderTypes.Unspecified
                                };
                                source.Add(data3);
                                using (List<LogicalMount>.Enumerator enumerator4 = list5.GetEnumerator())
                                {
                                    while (enumerator4.MoveNext())
                                    {
                                        LogicalMount local2 = enumerator4.Current;
                                        list.Add((int)bank.TurretClass);
                                    }
                                    continue;
                                }
                            }
                            if (WeaponEnums.IsWeaponBattleRider(bank.TurretClass))
                            {
                                if (func5 == null)
                                {
                                    func5 = x => x.Bank == bank;
                                }
                                num4 += module.Mounts.Count<LogicalMount>(func5);
                            }
                        }
                    }
                }
                list.Insert(count, num2);
                int num13 = 0;
                string str = "";
                string str2 = "";
                foreach (DesignSectionInfo info5 in designInfo.DesignSections)
                {
                    ShipSectionAsset asset2 = info5.ShipSectionAsset;
                    num13 += asset2.ReserveSize;
                    if (asset2.Type == ShipSectionType.Mission)
                    {
                        str = App.Localize(asset2.Title);
                    }
                    if (asset2.Type == ShipSectionType.Engine)
                    {
                        str2 = App.Localize(asset2.Title);
                    }
                }
                if (((num13 > 0) || (num2 > 0)) || unspecified.IsBattleRiderType())
                {
                    list.Add(info.DesignID);
                    list.Add(info.ID);
                    list.Add(designInfo.Name);
                    list.Add(info.ShipName);
                    list.Add(num13);
                    list.Add((int)designInfo.Class);
                    list.Add((int)unspecified);
                    list.Add(info.ParentID);
                    list.Add(info.RiderIndex);
                    list.Add(str);
                    list.Add(str2);
                    if (num2 > 0)
                    {
                        list.Add(source.Count);
                        foreach (CarrierWingData data4 in source)
                        {
                            list.Add(data4.SlotIndexes.Count);
                            foreach (int num14 in data4.SlotIndexes)
                            {
                                list.Add(num14);
                            }
                            list.Add((int)data4.Class);
                            list.Add((int)data4.DefaultType);
                        }
                    }
                    else if (num13 > 0)
                    {
                        list.Add(0);
                    }
                    item++;
                }
                else
                {
                    list.RemoveRange(count, list.Count - count);
                }
            }
            list.Insert(0, item);
            this.PostSetProp("SyncShips", list.ToArray());
        }
        public void Dispose()
		{
			this.App.UI.GameEvent -= new UIEventGameEvent(this.UICommChannel_GameEvent);
			this.App.UI.PanelMessage -= new UIEventPanelMessage(this.UICommChannel_OnPanelMessage);
			if (this.App != null)
			{
				this.App.ReleaseObject(this);
			}
		}
	}
}
