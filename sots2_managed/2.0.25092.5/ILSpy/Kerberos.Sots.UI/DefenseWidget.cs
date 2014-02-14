using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace Kerberos.Sots.UI
{
	[GameObjectType(InteropGameObjectType.IGOT_DEFENSEWIDGET)]
	internal class DefenseWidget : GameObject, IDisposable
	{
		private App _game;
		private string _rootName;
		private int _widgetID;
		private bool _ready;
		private int _syncedFleet;
		private static int _nextWidgetID = 100000;
		public DefenseWidget(App game, string rootList)
		{
			this._game = game;
			game.AddExistingObject(this, new object[]
			{
				rootList,
				DefenseWidget._nextWidgetID,
				this._game.LocalPlayer.ID
			});
			this._widgetID = DefenseWidget._nextWidgetID;
			DefenseWidget._nextWidgetID++;
			this._rootName = rootList;
			this._game.UI.GameEvent += new UIEventGameEvent(this.UICommChannel_GameEvent);
			this._game.UI.PanelMessage += new UIEventPanelMessage(this.UICommChannel_OnPanelMessage);
		}
		private void Refresh()
		{
			if (!this._ready)
			{
				return;
			}
			this.SyncFleet();
		}
		private void SyncFleet()
		{
			FleetInfo fleetInfo = base.App.GameDatabase.GetFleetInfo(this._syncedFleet);
			if (fleetInfo != null)
			{
				IEnumerable<ShipInfo> shipInfoByFleetID = base.App.GameDatabase.GetShipInfoByFleetID(fleetInfo.ID, true);
				List<object> list = new List<object>();
				int num = 0;
				foreach (ShipInfo ship in shipInfoByFleetID)
				{
					bool flag = true;
					num++;
					list.Add(true);
					list.Add(ship.DesignID);
					list.Add(ship.ID);
					list.Add(ship.DesignInfo.Name);
					list.Add(ship.ShipName);
					bool flag2 = false;
					string item = "";
					PlatformTypes? platformType = ship.DesignInfo.GetPlatformType();
					bool flag3 = false;
					bool flag4 = ship.IsPoliceShip();
					int defenseAssetCPCost = base.App.AssetDatabase.DefenseManagerSettings.GetDefenseAssetCPCost(ship.DesignInfo);
					if (ship.IsMinelayer())
					{
						flag2 = true;
						DesignSectionInfo[] designSections = ship.DesignInfo.DesignSections;
						for (int m = 0; m < designSections.Length; m++)
						{
							DesignSectionInfo designSectionInfo = designSections[m];
							foreach (WeaponBankInfo current in designSectionInfo.WeaponBanks)
							{
								string wasset = base.App.GameDatabase.GetWeaponAsset(current.WeaponID.Value);
								if (wasset.Contains("Min_"))
								{
									LogicalWeapon logicalWeapon = base.App.AssetDatabase.Weapons.FirstOrDefault((LogicalWeapon x) => x.FileName == wasset);
									if (logicalWeapon != null)
									{
										item = logicalWeapon.IconSpriteName;
										break;
									}
								}
							}
						}
					}
					else
					{
						if (ship.IsSDB())
						{
							flag3 = true;
						}
						else
						{
							if (ship.IsPoliceShip())
							{
								flag4 = true;
							}
						}
					}
					list.Add(flag2);
					list.Add(flag3);
					list.Add(item);
					list.Add(platformType.HasValue ? platformType.Value.ToString() : string.Empty);
					list.Add(flag4);
					if (defenseAssetCPCost == 0)
					{
						list.Add(base.App.GameDatabase.GetShipCommandPointCost(ship.ID, true));
					}
					else
					{
						list.Add(defenseAssetCPCost);
					}
					list.Add(base.App.GameDatabase.GetDesignCommandPointQuota(base.App.AssetDatabase, ship.DesignInfo.ID));
					list.Add(flag);
					int num2 = 0;
					int num3 = 0;
					int num4 = 0;
					int num5 = 0;
					int num6 = 0;
					int num7 = 0;
					ShipSectionAsset shipSectionAsset = null;
					List<SectionInstanceInfo> list2 = base.App.GameDatabase.GetShipSectionInstances(ship.ID).ToList<SectionInstanceInfo>();
					if (list2.Count != ship.DesignInfo.DesignSections.Length)
					{
						throw new InvalidDataException(string.Format("Mismatched design section vs ship section instance count for designId={0} and shipId={1}.", ship.DesignInfo.ID, ship.ID));
					}
					int i;
					for (i = 0; i < ship.DesignInfo.DesignSections.Count<DesignSectionInfo>(); i++)
					{
						if (list2.Count <= i)
						{
							App.Log.Warn("Tried syncing ship with no section", "game");
						}
						else
						{
							ShipSectionAsset shipSectionAsset2 = base.App.AssetDatabase.GetShipSectionAsset(ship.DesignInfo.DesignSections[i].FilePath);
							if (shipSectionAsset2.Type == ShipSectionType.Mission)
							{
								shipSectionAsset = shipSectionAsset2;
							}
							SectionInstanceInfo sectionInstanceInfo = list2.First((SectionInstanceInfo x) => x.SectionID == ship.DesignInfo.DesignSections[i].ID);
							num6 += shipSectionAsset2.ConstructionPoints;
							num7 += shipSectionAsset2.ColonizationSpace;
							num5 += shipSectionAsset2.Structure;
							num3 += shipSectionAsset2.RepairPoints;
							num4 += sectionInstanceInfo.Structure;
							num2 += sectionInstanceInfo.RepairPoints;
							Dictionary<ArmorSide, DamagePattern> armorInstances = base.App.GameDatabase.GetArmorInstances(sectionInstanceInfo.ID);
							if (armorInstances.Count > 0)
							{
								for (int j = 0; j < 4; j++)
								{
									num5 += armorInstances[(ArmorSide)j].Width * armorInstances[(ArmorSide)j].Height * 3;
									for (int k = 0; k < armorInstances[(ArmorSide)j].Width; k++)
									{
										for (int l = 0; l < armorInstances[(ArmorSide)j].Height; l++)
										{
											if (!armorInstances[(ArmorSide)j].GetValue(k, l))
											{
												num4 += 3;
											}
										}
									}
								}
							}
							List<ModuleInstanceInfo> source = base.App.GameDatabase.GetModuleInstances(sectionInstanceInfo.ID).ToList<ModuleInstanceInfo>();
							List<DesignModuleInfo> module = ship.DesignInfo.DesignSections[i].Modules;
							int mod;
							for (mod = 0; mod < module.Count; mod++)
							{
								ModuleInstanceInfo moduleInstanceInfo = source.First((ModuleInstanceInfo x) => x.ModuleNodeID == module[mod].MountNodeName);
								string modAsset = base.App.GameDatabase.GetModuleAsset(module[mod].ModuleID);
								LogicalModule logicalModule = (
									from x in base.App.AssetDatabase.Modules
									where x.ModulePath == modAsset
									select x).First<LogicalModule>();
								num5 += (int)logicalModule.Structure;
								num4 += moduleInstanceInfo.Structure;
								num3 += logicalModule.RepairPointsBonus;
								num2 += moduleInstanceInfo.RepairPoints;
							}
							List<WeaponInstanceInfo> list3 = base.App.GameDatabase.GetWeaponInstances(list2[i].ID).ToList<WeaponInstanceInfo>();
							foreach (WeaponInstanceInfo current2 in list3)
							{
								num5 += (int)current2.MaxStructure;
								num4 += (int)current2.Structure;
							}
						}
					}
					list.Add(num4);
					list.Add(num5);
					list.Add(num2);
					list.Add(num3);
					list.Add(num6);
					list.Add(num7);
					IEnumerable<ShipInfo> battleRidersByParentID = base.App.GameDatabase.GetBattleRidersByParentID(ship.ID);
					list.Add(battleRidersByParentID.Count<ShipInfo>());
					foreach (ShipInfo current3 in battleRidersByParentID)
					{
						list.Add(current3.ID);
					}
					list.Add(0);
					list.Add(shipSectionAsset.RealClass);
					list.Add(shipSectionAsset.BattleRiderType);
					Matrix? shipSystemPosition = base.App.GameDatabase.GetShipSystemPosition(ship.ID);
					list.Add(shipSystemPosition.HasValue ? 1 : 0);
					if (shipSystemPosition.HasValue)
					{
						list.Add(shipSystemPosition.Value);
					}
				}
				list.Insert(0, num);
				int systemDefensePoints = base.App.GameDatabase.GetSystemDefensePoints(fleetInfo.SystemID, base.App.LocalPlayer.ID);
				list.Insert(1, systemDefensePoints);
				list.Insert(2, fleetInfo.ID);
				this.PostSetProp("SyncShips", list.ToArray());
			}
		}
		public void SetSyncedFleet(int fleetID)
		{
			this._syncedFleet = fleetID;
			this.Refresh();
		}
		public int GetSynchedFleet()
		{
			return this._syncedFleet;
		}
		private void UICommChannel_GameEvent(string eventName, string[] eventParams)
		{
			if (eventName == "DefenseWidgetReady" && int.Parse(eventParams[0]) == this._widgetID)
			{
				this._ready = true;
				this.Refresh();
			}
		}
		protected void UICommChannel_OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
		}
		public void Dispose()
		{
			this._game.UI.GameEvent -= new UIEventGameEvent(this.UICommChannel_GameEvent);
			this._game.UI.PanelMessage -= new UIEventPanelMessage(this.UICommChannel_OnPanelMessage);
			if (this._game != null)
			{
				this._game.ReleaseObject(this);
			}
		}
	}
}
