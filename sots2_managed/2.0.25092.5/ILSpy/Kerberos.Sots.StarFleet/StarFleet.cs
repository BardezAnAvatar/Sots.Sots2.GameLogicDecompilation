using Kerberos.Sots.Data;
using Kerberos.Sots.Data.ModuleFramework;
using Kerberos.Sots.Data.ShipFramework;
using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.StarSystemPathing;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using Kerberos.Sots.UI;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.StarFleet
{
	internal class StarFleet
	{
		public class EvaluatedNode
		{
			public int SystemID;
			public int FromNodeID;
			public float FCost;
			public float HCost;
			public bool Evaluated;
			public EvaluatedNode(int system, int from, float fCost, float hCost)
			{
				this.SystemID = system;
				this.FromNodeID = from;
				this.FCost = fCost;
				this.HCost = hCost;
				this.Evaluated = false;
			}
		}
		private const int GATE_COST = 2000;
		public static bool FleetsAlwaysInRange;
		public static bool IsFleetAvailableForMission(GameSession game, int fleetID, int systemID)
		{
			MissionInfo missionByFleetID = game.GameDatabase.GetMissionByFleetID(fleetID);
			return missionByFleetID == null && StarFleet.IsFleetInRange(game, fleetID, systemID, null, null, null);
		}
		public static int GetRepairPointsMax(App app, DesignInfo des)
		{
			int num = 0;
			for (int i = 0; i < des.DesignSections.Count<DesignSectionInfo>(); i++)
			{
				ShipSectionAsset shipSectionAsset = app.AssetDatabase.GetShipSectionAsset(des.DesignSections[i].FilePath);
				num += shipSectionAsset.RepairPoints;
				foreach (DesignModuleInfo current in des.DesignSections[i].Modules)
				{
					string mPath = app.GameDatabase.GetModuleAsset(current.ModuleID);
					LogicalModule logicalModule = app.AssetDatabase.Modules.FirstOrDefault((LogicalModule x) => x.ModulePath == mPath);
					num += logicalModule.RepairPointsBonus;
				}
			}
			return num;
		}
		public static bool GetIsSalavageCapable(App app, DesignInfo des)
		{
			for (int i = 0; i < des.DesignSections.Count<DesignSectionInfo>(); i++)
			{
				ShipSectionAsset shipSectionAsset = app.AssetDatabase.GetShipSectionAsset(des.DesignSections[i].FilePath);
				if (shipSectionAsset.SectionName == "cr_mis_repair" || shipSectionAsset.SectionName == "cr_mis_repair_salvage" || shipSectionAsset.SectionName == "dn_mis_supply")
				{
					return true;
				}
			}
			return false;
		}
		public static int GetSalvageChance(App app, DesignInfo des)
		{
			Faction faction = app.GetPlayer(des.PlayerID).Faction;
			for (int i = 0; i < des.DesignSections.Count<DesignSectionInfo>(); i++)
			{
				ShipSectionAsset shipSectionAsset = app.AssetDatabase.GetShipSectionAsset(des.DesignSections[i].FilePath);
				if (shipSectionAsset.SectionName == "cr_mis_repair" || shipSectionAsset.SectionName == "cr_mis_repair_salvage" || shipSectionAsset.SectionName == "dn_mis_supply")
				{
					return faction.RepSel;
				}
			}
			return faction.DefaultRepSel;
		}
        public static int[] GetHealthAndHealthMax(GameSession game, DesignInfo design, int shipid)
        {
            int num = 0;
            int num2 = 0;
            int num3 = 0;
            int num4 = 0;
            List<SectionInstanceInfo> source = game.GameDatabase.GetShipSectionInstances(shipid).ToList<SectionInstanceInfo>();
            Func<SectionInstanceInfo, bool> predicate = null;
            for (int i = 0; i < design.DesignSections.Count<DesignSectionInfo>(); i++)
            {
                if (predicate == null)
                {
                    predicate = x => x.SectionID == design.DesignSections[i].ID;
                }
                SectionInstanceInfo info = source.FirstOrDefault<SectionInstanceInfo>(predicate);
                ShipSectionAsset shipSectionAsset = game.AssetDatabase.GetShipSectionAsset(design.DesignSections[i].FilePath);
                List<string> techs = new List<string>();
                if (design.DesignSections[i].Techs.Count > 0)
                {
                    foreach (int num5 in design.DesignSections[i].Techs)
                    {
                        techs.Add(game.GameDatabase.GetTechFileID(num5));
                    }
                }
                int num6 = Ship.GetStructureWithTech(game.AssetDatabase, techs, shipSectionAsset.Structure);
                num2 += num6;
                num += (info != null) ? info.Structure : num6;
                if (info != null)
                {
                    Dictionary<ArmorSide, DamagePattern> armorInstances = game.GameDatabase.GetArmorInstances(info.ID);
                    if (armorInstances.Count > 0)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            num2 += (armorInstances[(ArmorSide)j].Width * armorInstances[(ArmorSide)j].Height) * 3;
                            ArmorSide side = (ArmorSide)j;
                            for (int k = 0; k < armorInstances[side].Width; k++)
                            {
                                for (int m = 0; m < armorInstances[side].Height; m++)
                                {
                                    if (!armorInstances[side].GetValue(k, m))
                                    {
                                        num += 3;
                                    }
                                }
                            }
                        }
                    }
                    List<ModuleInstanceInfo> list3 = game.GameDatabase.GetModuleInstances(info.ID).ToList<ModuleInstanceInfo>();
                    List<DesignModuleInfo> module = design.DesignSections[i].Modules;
                    Func<ModuleInstanceInfo, bool> func = null;
                    for (int mod = 0; mod < module.Count; mod++)
                    {
                        if (func == null)
                        {
                            func = x => x.ModuleNodeID == module[mod].MountNodeName;
                        }
                        ModuleInstanceInfo info2 = list3.FirstOrDefault<ModuleInstanceInfo>(func);
                        string modAsset = game.GameDatabase.GetModuleAsset(module[mod].ModuleID);
                        LogicalModule logicalModule = (from x in game.AssetDatabase.Modules
                                                where x.ModulePath == modAsset
                                                select x).First<LogicalModule>();
                        num2 += (int)logicalModule.Structure;
                        num += (info2 != null) ? info2.Structure : ((int)Math.Ceiling((double)logicalModule.Structure));
                        if (module[mod].DesignID.HasValue)
                        {
                            foreach (LogicalMount mount in logicalModule.Mounts)
                            {
                                if (WeaponEnums.IsWeaponBattleRider(mount.Bank.TurretClass))
                                {
                                    if (num3 == 0)
                                    {
                                        num3 = module[mod].DesignID.Value;
                                    }
                                    num4++;
                                }
                            }
                        }
                    }
                    foreach (WeaponInstanceInfo info3 in game.GameDatabase.GetWeaponInstances(info.ID).ToList<WeaponInstanceInfo>())
                    {
                        num2 += (int)Math.Ceiling((double)info3.MaxStructure);
                        num += (int)Math.Ceiling((double)info3.Structure);
                    }
                    Func<WeaponBankInfo, bool> func2 = null;
                    foreach (LogicalMount mount in shipSectionAsset.Mounts)
                    {
                        if (WeaponEnums.IsWeaponBattleRider(mount.Bank.TurretClass))
                        {
                            if (num3 == 0)
                            {
                                if (func2 == null)
                                {
                                    func2 = delegate(WeaponBankInfo x)
                                    {
                                        if (!(x.BankGUID == mount.Bank.GUID) || !x.DesignID.HasValue)
                                        {
                                            return false;
                                        }
                                        int? designID = x.DesignID;
                                        if (designID.GetValueOrDefault() == 0)
                                        {
                                            return !designID.HasValue;
                                        }
                                        return true;
                                    };
                                }
                                WeaponBankInfo info4 = design.DesignSections[i].WeaponBanks.FirstOrDefault<WeaponBankInfo>(func2);
                                num3 = ((info4 != null) && info4.DesignID.HasValue) ? info4.DesignID.Value : 0;
                            }
                            num4++;
                        }
                    }
                }
            }
            List<ShipInfo> list5 = game.GameDatabase.GetBattleRidersByParentID(shipid).ToList<ShipInfo>();
            if (num4 > 0)
            {
                int num10 = num4;
                foreach (ShipInfo info5 in list5)
                {
                    DesignInfo designInfo = game.GameDatabase.GetDesignInfo(info5.DesignID);
                    if (designInfo != null)
                    {
                        DesignSectionInfo info7 = designInfo.DesignSections.FirstOrDefault<DesignSectionInfo>(x => x.ShipSectionAsset.Type == ShipSectionType.Mission);
                        if ((info7 != null) && ShipSectionAsset.IsBattleRiderClass(info7.ShipSectionAsset.RealClass))
                        {
                            num10--;
                        }
                    }
                }
                int structure = 0;
                if (num3 != 0)
                {
                    foreach (DesignSectionInfo info9 in game.GameDatabase.GetDesignInfo(num3).DesignSections)
                    {
                        ShipSectionAsset asset2 = game.AssetDatabase.GetShipSectionAsset(info9.FilePath);
                        structure = asset2.Structure;
                        int repairPoints = asset2.RepairPoints;
                        if (asset2.Armor.Length > 0)
                        {
                            for (int n = 0; n < 4; n++)
                            {
                                structure += (asset2.Armor[n].X * asset2.Armor[n].Y) * 3;
                            }
                        }
                    }
                }
                num2 += structure * num4;
                num += structure * (num4 - num10);
            }
            return new int[] { num, num2 };
        }
        private static void Warn(string message)
		{
			App.Log.Warn(message, "game");
		}
        public static void RepairShip(App app, ShipInfo ship, int points)
        {
            List<SectionInstanceInfo> source = app.GameDatabase.GetShipSectionInstances(ship.ID).ToList<SectionInstanceInfo>();
            List<DesignSectionInfo> sections = app.GameDatabase.GetShipInfo(ship.ID, true).DesignInfo.DesignSections.ToList<DesignSectionInfo>();
            List<int> list2 = new List<int>();
            int item = 0;
            int num2 = 0;
            int num3 = 0;
            int num4 = source.Count * 5;
            Func<SectionInstanceInfo, bool> predicate = null;
            for (int j = 0; j < sections.Count; j++)
            {
                if (predicate == null)
                {
                    predicate = x => x.SectionID == sections[j].ID;
                }
                SectionInstanceInfo info = source.First<SectionInstanceInfo>(predicate);
                List<ModuleInstanceInfo> list3 = app.GameDatabase.GetModuleInstances(info.ID).ToList<ModuleInstanceInfo>();
                num4 += list3.Count;
                num4 += app.GameDatabase.GetWeaponInstances(info.ID).ToList<WeaponInstanceInfo>().Count;
                ShipSectionAsset shipSectionAsset = app.AssetDatabase.GetShipSectionAsset(sections[j].FilePath);
                Func<WeaponBankInfo, bool> func = null;
                foreach (LogicalMount mount in shipSectionAsset.Mounts)
                {
                    if (WeaponEnums.IsWeaponBattleRider(mount.Bank.TurretClass))
                    {
                        if (num2 == 0)
                        {
                            if (func == null)
                            {
                                func = delegate(WeaponBankInfo x)
                                {
                                    if (!(x.BankGUID == mount.Bank.GUID) || !x.DesignID.HasValue)
                                    {
                                        return false;
                                    }
                                    int? designID = x.DesignID;
                                    if (designID.GetValueOrDefault() == 0)
                                    {
                                        return !designID.HasValue;
                                    }
                                    return true;
                                };
                            }
                            WeaponBankInfo info2 = sections[j].WeaponBanks.FirstOrDefault<WeaponBankInfo>(func);
                            num2 = ((info2 != null) && info2.DesignID.HasValue) ? info2.DesignID.Value : 0;
                        }
                        num3++;
                        list2.Add(item);
                        item++;
                    }
                    else if (WeaponEnums.IsBattleRider(mount.Bank.TurretClass))
                    {
                        item++;
                    }
                }
                if (list3.Count > 0)
                {
                    using (List<ModuleInstanceInfo>.Enumerator enumerator = list3.GetEnumerator())
                    {
                        Func<DesignModuleInfo, bool> func2 = null;
                        ModuleInstanceInfo mii;
                        while (enumerator.MoveNext())
                        {
                            mii = enumerator.Current;
                            if (func2 == null)
                            {
                                func2 = x => x.MountNodeName == mii.ModuleNodeID;
                            }
                            DesignModuleInfo info3 = sections[j].Modules.First<DesignModuleInfo>(func2);
                            if (info3.DesignID.HasValue)
                            {
                                string modAsset = app.GameDatabase.GetModuleAsset(info3.ModuleID);
                                foreach (LogicalMount mount in (from x in app.AssetDatabase.Modules
                                                                where x.ModulePath == modAsset
                                                                select x).First<LogicalModule>().Mounts)
                                {
                                    if (WeaponEnums.IsWeaponBattleRider(mount.Bank.TurretClass))
                                    {
                                        if (num2 == 0)
                                        {
                                            num2 = info3.DesignID.Value;
                                        }
                                        num3++;
                                        list2.Add(item);
                                        item++;
                                    }
                                    else
                                    {
                                        item++;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            int num5 = num3;
            int num6 = 0;
            int num7 = 0;
            foreach (ShipInfo info4 in app.GameDatabase.GetBattleRidersByParentID(ship.ID).ToList<ShipInfo>())
            {
                if (info4.DesignID == num2)
                {
                    num5--;
                    list2.Remove(info4.RiderIndex);
                }
            }
            DesignInfo designInfo = app.GameDatabase.GetDesignInfo(num2);
            if (designInfo != null)
            {
                foreach (DesignSectionInfo info6 in designInfo.DesignSections)
                {
                    num6 += info6.ShipSectionAsset.Structure;
                    num7 += info6.ShipSectionAsset.RepairPoints;
                    if (info6.ShipSectionAsset.Armor.Length > 0)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            num6 += (info6.ShipSectionAsset.Armor[i].X * info6.ShipSectionAsset.Armor[i].Y) * 3;
                        }
                    }
                }
            }
            num4 += num5;
            if (num4 <= 0)
            {
                Warn("StarFleet.RepairShip: thingsToRepair <= 0");
            }
            else
            {
                int num9 = points / sections.Count;
                if (num9 == 0)
                {
                    num9 = points;
                }
                int num10 = Math.Min(num9 + (3 - (num9 % 3)), points);
                int num11 = 0;
                if (num9 <= 0)
                {
                    Warn("StarFleet.RepairShip: pointsPerSection <= 0");
                }
                else
                {
                    int num12 = Math.Max(50, points / num9);
                    while (((points > 0) && (num11 != num4)) && (num12 > 0))
                    {
                        num12--;
                        Func<SectionInstanceInfo, bool> func5 = null;
                        for (int j = 0; j < sections.Count; j++)
                        {
                            if (func5 == null)
                            {
                                func5 = x => x.SectionID == sections[j].ID;
                            }
                            SectionInstanceInfo info7 = source.First<SectionInstanceInfo>(func5);
                            List<ModuleInstanceInfo> list5 = app.GameDatabase.GetModuleInstances(info7.ID).ToList<ModuleInstanceInfo>();
                            List<DesignModuleInfo> module = sections[j].Modules;
                            List<WeaponInstanceInfo> list6 = app.GameDatabase.GetWeaponInstances(info7.ID).ToList<WeaponInstanceInfo>();
                            ShipSectionAsset asset2 = app.AssetDatabase.GetShipSectionAsset(sections[j].FilePath);
                            List<string> techs = new List<string>();
                            if (sections[j].Techs.Count > 0)
                            {
                                foreach (int num13 in sections[j].Techs)
                                {
                                    techs.Add(app.GameDatabase.GetTechFileID(num13));
                                }
                            }
                            int num14 = Ship.GetStructureWithTech(app.AssetDatabase, techs, asset2.Structure);
                            int num15 = num14 - info7.Structure;
                            if (num15 > 0)
                            {
                                if (num15 > num9)
                                {
                                    num15 = num9;
                                }
                                if (num15 > points)
                                {
                                    num15 = points;
                                }
                                info7.Structure += num15;
                                if (info7.Structure == num14)
                                {
                                    num11++;
                                }
                                points -= num15;
                            }
                            Dictionary<ArmorSide, DamagePattern> armorInstances = app.GameDatabase.GetArmorInstances(info7.ID);
                            if (armorInstances.Count > 0)
                            {
                                for (int k = 0; k < 4; k++)
                                {
                                    ArmorSide side = (ArmorSide)k;
                                    int num17 = (armorInstances[side].Width * armorInstances[side].Height) * 3;
                                    int num18 = armorInstances[side].GetTotalFilled() * 3;
                                    num15 = num17 - num18;
                                    if (num15 > 0)
                                    {
                                        if (num15 > num10)
                                        {
                                            num15 = num10;
                                        }
                                        if (num15 > points)
                                        {
                                            num15 = points;
                                        }
                                        num18 += num15;
                                        if (num18 == num17)
                                        {
                                            num11++;
                                        }
                                        points -= num15;
                                    }
                                    int num19 = num15;
                                    if (num19 > 0)
                                    {
                                        for (int m = armorInstances[side].Height - 1; m >= 0; m--)
                                        {
                                            for (int n = 0; n < armorInstances[side].Width; n++)
                                            {
                                                if (armorInstances[side].GetValue(n, m) && (num19 >= 3))
                                                {
                                                    armorInstances[side].SetValue(n, m, false);
                                                    num19 -= 3;
                                                }
                                                if (num19 <= 0)
                                                {
                                                    break;
                                                }
                                            }
                                            if (num19 <= 0)
                                            {
                                                break;
                                            }
                                        }
                                    }
                                }
                                app.GameDatabase.UpdateArmorInstances(info7.ID, armorInstances);
                            }
                            if (list6.Count > 0)
                            {
                                foreach (WeaponInstanceInfo info8 in list6)
                                {
                                    num15 = (int)(info8.MaxStructure - info8.Structure);
                                    if (num15 > 0)
                                    {
                                        if (num15 > num9)
                                        {
                                            num15 = num9;
                                        }
                                        if (num15 > points)
                                        {
                                            num15 = points;
                                        }
                                        info8.Structure += num15;
                                        if (info8.Structure == info8.MaxStructure)
                                        {
                                            num11++;
                                        }
                                        points -= num15;
                                    }
                                    app.GameDatabase.UpdateWeaponInstance(info8);
                                }
                            }
                            if (list5.Count == module.Count)
                            {
                                Func<ModuleInstanceInfo, bool> func4 = null;
                                for (int mod = 0; mod < module.Count; mod++)
                                {
                                    string modAsset = app.GameDatabase.GetModuleAsset(module[mod].ModuleID);
                                    LogicalModule module2 = (from x in app.AssetDatabase.Modules
                                                             where x.ModulePath == modAsset
                                                             select x).First<LogicalModule>();
                                    if (func4 == null)
                                    {
                                        func4 = x => x.ModuleNodeID == module[mod].MountNodeName;
                                    }
                                    ModuleInstanceInfo info9 = list5.First<ModuleInstanceInfo>(func4);
                                    num15 = ((int)module2.Structure) - info9.Structure;
                                    if (num15 > 0)
                                    {
                                        if (num15 > num9)
                                        {
                                            num15 = num9;
                                        }
                                        if (num15 > points)
                                        {
                                            num15 = points;
                                        }
                                        info9.Structure += num15;
                                        if (list5[mod].Structure == module2.Structure)
                                        {
                                            num11++;
                                        }
                                        points -= num15;
                                    }
                                    app.GameDatabase.UpdateModuleInstance(info9);
                                }
                            }
                            if ((num2 != 0) && (num5 > 0))
                            {
                                int num22 = 0;
                                num15 = num6;
                                for (int num23 = 0; num23 < num5; num23++)
                                {
                                    if (((num15 <= 0) || (num15 > points)) || (list2.Count == 0))
                                    {
                                        break;
                                    }
                                    points -= num15;
                                    int? aiFleetID = null;
                                    int shipID = app.GameDatabase.InsertShip(ship.FleetID, num2, null, 0, aiFleetID, 0);
                                    app.GameDatabase.SetShipParent(shipID, ship.ID);
                                    app.GameDatabase.UpdateShipRiderIndex(shipID, list2.First<int>());
                                    list2.RemoveAt(0);
                                    num22++;
                                }
                                num5 -= num22;
                                num11 += num22;
                            }
                        }
                    }
                }
            }
            foreach (SectionInstanceInfo info10 in source)
            {
                app.GameDatabase.UpdateSectionInstance(info10);
            }
        }
        public static int GetFleetEndurance(GameSession game, int fleetID)
		{
			List<ShipInfo> list = game.GameDatabase.GetShipInfoByFleetID(fleetID, false).ToList<ShipInfo>();
			if (list.Count<ShipInfo>() == 0)
			{
				return 0;
			}
			int num = 0;
			int num2 = 0;
			if (game.AssetDatabase.GetFaction(game.GameDatabase.GetPlayerInfo(game.GameDatabase.GetFleetInfo(fleetID).PlayerID).FactionID).Name == "loa")
			{
				num = 15;
			}
			else
			{
				foreach (ShipInfo current in list)
				{
					DesignInfo designInfo = game.GameDatabase.GetDesignInfo(current.DesignID);
					RealShipClasses? realShipClass = designInfo.GetRealShipClass();
					if (realShipClass.HasValue)
					{
						switch (realShipClass.Value)
						{
						case RealShipClasses.BattleRider:
						case RealShipClasses.Drone:
						case RealShipClasses.BoardingPod:
						case RealShipClasses.EscapePod:
						case RealShipClasses.AssaultShuttle:
						case RealShipClasses.Biomissile:
							continue;
						}
					}
					num += designInfo.GetEndurance(game);
					num2++;
				}
				if (num2 == 0)
				{
					return 0;
				}
				num /= num2;
			}
			FleetInfo fleetInfo = game.GameDatabase.GetFleetInfo(fleetID);
			IEnumerable<AdmiralInfo.TraitType> admiralTraits = game.GameDatabase.GetAdmiralTraits(fleetInfo.AdmiralID);
			int num3 = 0;
			if (admiralTraits.Contains(AdmiralInfo.TraitType.TrueGrit))
			{
				num3 += 2;
			}
			if (admiralTraits.Contains(AdmiralInfo.TraitType.Thrifty))
			{
				num3 += (int)((float)num * 0.2f);
			}
			if (admiralTraits.Contains(AdmiralInfo.TraitType.Wastrel))
			{
				num3 -= (int)((float)num * 0.2f);
			}
			if (admiralTraits.Contains(AdmiralInfo.TraitType.DrillSergeant))
			{
				num3 -= (int)((float)num * 0.05f);
			}
			return num + num3;
		}
		public static bool IsFleetExhausted(GameSession game, FleetInfo fleet)
		{
			int fleetEndurance = StarFleet.GetFleetEndurance(game, fleet.ID);
			return fleet.TurnsAway >= fleetEndurance * 2 || fleet.SupplyRemaining <= 0f;
		}
		public static float GetFleetRange(GameSession game, FleetInfo fi)
		{
			Faction faction = game.AssetDatabase.GetFaction(game.GameDatabase.GetPlayerFactionID(fi.PlayerID));
			float supportRange = GameSession.GetSupportRange(game.AssetDatabase, game.GameDatabase, fi.PlayerID);
			int num = Math.Max(StarFleet.GetFleetEndurance(game, fi.ID) - fi.TurnsAway, 0);
			int num2;
			if (faction.CanUseGate())
			{
				num2 = Math.Max(num - 1, 0);
			}
			else
			{
				num2 = num / 2;
			}
			float num3 = (float)num2 * StarFleet.GetFleetTravelSpeed(game, fi.ID, faction.CanUseNodeLine(null));
			return supportRange + num3;
		}
		public static bool IsFleetInRange(GameSession game, int fleetID, int systemID, float? fleetRange = null, float? travelSpeed = null, float? nodeTravelSpeed = null)
		{
			if (StarFleet.FleetsAlwaysInRange)
			{
				return true;
			}
			FleetInfo fleetInfo = game.GameDatabase.GetFleetInfo(fleetID);
			return StarFleet.IsFleetInRange(game, fleetInfo, systemID, fleetRange, travelSpeed, nodeTravelSpeed);
		}
		public static bool IsFleetInRange(GameSession game, FleetInfo fleet, int systemID, float? fleetRange = null, float? travelSpeed = null, float? nodeTravelSpeed = null)
		{
			if (StarFleet.FleetsAlwaysInRange)
			{
				return true;
			}
			float num = fleetRange.HasValue ? fleetRange.Value : StarFleet.GetFleetRange(game, fleet);
			int num2;
			float num3;
			StarFleet.GetBestTravelPath(game, fleet.ID, fleet.SystemID, systemID, out num2, out num3, false, travelSpeed, nodeTravelSpeed);
			return num3 <= num;
		}
		public static List<FleetInfo> GetFleetsInRangeOfSystem(GameSession game, int systemID, Dictionary<FleetInfo, FleetRangeData> fleetRanges, float scale = 1f)
		{
			List<FleetInfo> list = new List<FleetInfo>();
			foreach (KeyValuePair<FleetInfo, FleetRangeData> current in fleetRanges)
			{
				int num;
				float num2;
				StarFleet.GetBestTravelPath(game, current.Key.ID, current.Key.SystemID, systemID, out num, out num2, false, current.Value.FleetTravelSpeed, current.Value.FleetNodeTravelSpeed);
				if (num2 <= current.Value.FleetRange * scale)
				{
					list.Add(current.Key);
				}
			}
			return list;
		}
		public static bool IsSuulkaFleet(GameDatabase db, FleetInfo fleet)
		{
			IEnumerable<ShipInfo> shipInfoByFleetID = db.GetShipInfoByFleetID(fleet.ID, true);
			return StarFleet.HasSuulkaInList(shipInfoByFleetID.ToList<ShipInfo>());
		}
		public static bool HasSuulkaInList(List<ShipInfo> ships)
		{
			foreach (ShipInfo current in ships)
			{
				DesignSectionInfo[] designSections = current.DesignInfo.DesignSections;
				for (int i = 0; i < designSections.Length; i++)
				{
					DesignSectionInfo designSectionInfo = designSections[i];
					if (designSectionInfo.ShipSectionAsset.IsSuulka)
					{
						return true;
					}
				}
			}
			return false;
		}
		public static ShipInfo GetFleetSuulkaShipInfo(GameDatabase db, FleetInfo fleet)
		{
			IEnumerable<ShipInfo> shipInfoByFleetID = db.GetShipInfoByFleetID(fleet.ID, true);
			foreach (ShipInfo current in shipInfoByFleetID)
			{
				DesignSectionInfo[] designSections = current.DesignInfo.DesignSections;
				for (int i = 0; i < designSections.Length; i++)
				{
					DesignSectionInfo designSectionInfo = designSections[i];
					if (designSectionInfo.ShipSectionAsset.IsSuulka)
					{
						return current;
					}
				}
			}
			return null;
		}
		public static bool IsGardenerFleet(GameSession game, FleetInfo fleet)
		{
			if (game.ScriptModules.Gardeners == null)
			{
				return false;
			}
			List<ShipInfo> source = game.GameDatabase.GetShipInfoByFleetID(fleet.ID, false).ToList<ShipInfo>();
			return source.Any((ShipInfo x) => x.DesignID == game.ScriptModules.Gardeners.GardenerDesignId);
		}
		public static bool HasBoreShip(App app, int fleetID)
		{
			List<ShipInfo> source = app.GameDatabase.GetShipInfoByFleetID(fleetID, true).ToList<ShipInfo>();
			return source.Any((ShipInfo x) => x.DesignInfo.DesignSections.Any((DesignSectionInfo y) => y.ShipSectionAsset.IsBoreShip));
		}
		public static bool DesignIsSuulka(App app, DesignInfo design)
		{
			DesignSectionInfo[] designSections = design.DesignSections;
			for (int i = 0; i < designSections.Length; i++)
			{
				DesignSectionInfo designSectionInfo = designSections[i];
				if (designSectionInfo.ShipSectionAsset.IsSuulka)
				{
					return true;
				}
			}
			return false;
		}
		public static List<int> GetFactionRequiredDesignsForFleet(GameSession game, int fleetID, int targetSystemId)
		{
			GameDatabase gameDatabase = game.GameDatabase;
			List<int> list = new List<int>();
			if (game.GameDatabase.GetUnixTimeCreated() != 0.0)
			{
				return list;
			}
			FleetInfo fleetInfo = gameDatabase.GetFleetInfo(fleetID);
			PlayerInfo playerInfo = gameDatabase.GetPlayerInfo(fleetInfo.PlayerID);
			string factionName = gameDatabase.GetFactionName(playerInfo.FactionID);
			string a;
			if ((a = factionName) != null && a == "zuul")
			{
				Player playerObject = game.GetPlayerObject(fleetInfo.PlayerID);
				if (playerObject != null && playerObject.IsAI() && StarFleet.GetNodeTravelPath(gameDatabase, fleetInfo.SystemID, targetSystemId, playerInfo.ID, false, true, false).Count<int>() == 0 && !GameSession.FleetHasBore(gameDatabase, fleetID))
				{
					int num = 0;
					IEnumerable<DesignInfo> designInfosForPlayer = gameDatabase.GetDesignInfosForPlayer(playerInfo.ID);
					foreach (DesignInfo current in designInfosForPlayer)
					{
						DesignSectionInfo[] designSections = current.DesignSections;
						for (int i = 0; i < designSections.Length; i++)
						{
							DesignSectionInfo designSectionInfo = designSections[i];
							ShipSectionAsset shipSectionAsset = gameDatabase.AssetDatabase.GetShipSectionAsset(designSectionInfo.FilePath);
							if (shipSectionAsset.IsBoreShip)
							{
								num = current.ID;
								break;
							}
						}
						if (num != 0)
						{
							break;
						}
					}
					if (num != 0)
					{
						list.Add(num);
					}
				}
			}
			return list;
		}
		private static void MissionTrace(string message)
		{
			App.Log.Trace(message, "game");
		}
		public static int SetColonizationMission(GameSession game, int fleetID, int systemID, bool useDirectRoute, int planetID, List<int> designIDs, int? ReBaseTarget = null)
		{
			GameDatabase gameDatabase = game.GameDatabase;
			if (designIDs != null)
			{
				designIDs.AddRange(StarFleet.GetFactionRequiredDesignsForFleet(game, fleetID, systemID));
			}
			else
			{
				designIDs = StarFleet.GetFactionRequiredDesignsForFleet(game, fleetID, systemID);
			}
			int num = gameDatabase.InsertMission(fleetID, MissionType.COLONIZATION, systemID, planetID, 0, 1, useDirectRoute, null);
			if (designIDs != null && designIDs.Count<int>() > 0)
			{
				StarFleet.AddShipsForMission(gameDatabase, fleetID, num, designIDs);
			}
			StarFleet.SetWaypointsForMission(game, MissionType.COLONIZATION, num, fleetID, systemID, 0, ReBaseTarget);
			StarFleet.MissionTrace(string.Concat(new object[]
			{
				"Fleet ",
				fleetID,
				" sent on Colonization mission to system ",
				systemID
			}));
			return num;
		}
		public static int SetEvacuationMission(GameSession game, int fleetID, int systemID, bool useDirectRoute, int planetID, List<int> designIDs)
		{
			GameDatabase gameDatabase = game.GameDatabase;
			if (designIDs != null)
			{
				designIDs.AddRange(StarFleet.GetFactionRequiredDesignsForFleet(game, fleetID, systemID));
			}
			else
			{
				designIDs = StarFleet.GetFactionRequiredDesignsForFleet(game, fleetID, systemID);
			}
			int num = gameDatabase.InsertMission(fleetID, MissionType.EVACUATE, systemID, planetID, 0, 1, useDirectRoute, null);
			if (designIDs != null && designIDs.Count<int>() > 0)
			{
				StarFleet.AddShipsForMission(gameDatabase, fleetID, num, designIDs);
			}
			StarFleet.SetWaypointsForMission(game, MissionType.EVACUATE, num, fleetID, systemID, 0, null);
			StarFleet.MissionTrace(string.Concat(new object[]
			{
				"Fleet ",
				fleetID,
				" sent on Evacuation mission to system ",
				systemID
			}));
			return num;
		}
		public static int SetRelocationMission(GameSession game, int fleetID, int systemId, bool useDirectRoute, List<int> designIDs)
		{
			GameDatabase gameDatabase = game.GameDatabase;
			if (designIDs != null)
			{
				designIDs.AddRange(StarFleet.GetFactionRequiredDesignsForFleet(game, fleetID, systemId));
			}
			else
			{
				designIDs = StarFleet.GetFactionRequiredDesignsForFleet(game, fleetID, systemId);
			}
			int num = gameDatabase.InsertMission(fleetID, MissionType.RELOCATION, systemId, 0, 0, 1, useDirectRoute, null);
			if (designIDs != null && designIDs.Count<int>() > 0)
			{
				StarFleet.AddShipsForMission(gameDatabase, fleetID, num, designIDs);
			}
			StarFleet.SetWaypointsForMission(game, MissionType.RELOCATION, num, fleetID, systemId, 0, null);
			StarFleet.MissionTrace(string.Concat(new object[]
			{
				"Fleet ",
				fleetID,
				" sent on Transfer mission to system: ",
				gameDatabase.GetStarSystemInfo(systemId).Name
			}));
			return num;
		}
		public static int SetPatrolMission(GameSession game, int fleetID, int systemId, bool useDirectRoute, List<int> designIDs, int? ReBaseTarget = null)
		{
			GameDatabase gameDatabase = game.GameDatabase;
			if (designIDs != null)
			{
				designIDs.AddRange(StarFleet.GetFactionRequiredDesignsForFleet(game, fleetID, systemId));
			}
			else
			{
				designIDs = StarFleet.GetFactionRequiredDesignsForFleet(game, fleetID, systemId);
			}
			int num = gameDatabase.InsertMission(fleetID, MissionType.PATROL, systemId, 0, 0, 1, useDirectRoute, null);
			if (designIDs != null && designIDs.Count<int>() > 0)
			{
				StarFleet.AddShipsForMission(gameDatabase, fleetID, num, designIDs);
			}
			StarFleet.SetWaypointsForMission(game, MissionType.PATROL, num, fleetID, systemId, 0, ReBaseTarget);
			StarFleet.MissionTrace(string.Concat(new object[]
			{
				"Fleet ",
				fleetID,
				" sent on Patrol mission to system: ",
				systemId
			}));
			return num;
		}
		public static int SetNPGMission(GameSession game, int fleetId, int systemId, bool useDirectRoute, List<int> gatepoints, List<int> designIds, int? ReBaseTarget = null)
		{
			GameDatabase gameDatabase = game.GameDatabase;
			gatepoints.Sort();
			if (designIds != null)
			{
				designIds.AddRange(StarFleet.GetFactionRequiredDesignsForFleet(game, fleetId, systemId));
			}
			else
			{
				designIds = StarFleet.GetFactionRequiredDesignsForFleet(game, fleetId, systemId);
			}
			int num = gameDatabase.InsertMission(fleetId, MissionType.DEPLOY_NPG, systemId, 0, 0, 1, useDirectRoute, null);
			if (designIds != null && designIds.Count<int>() > 0)
			{
				StarFleet.AddShipsForMission(gameDatabase, fleetId, num, designIds);
			}
			StarFleet.SetWaypointsForMission(game, MissionType.DEPLOY_NPG, num, fleetId, systemId, 0, ReBaseTarget);
			StarFleet.ConvertFleetIntoLoaCubes(game, fleetId);
			StarSystemInfo starSystemInfo = game.GameDatabase.GetStarSystemInfo(game.GameDatabase.GetFleetInfo(fleetId).SystemID);
			StarSystemInfo starSystemInfo2 = game.GameDatabase.GetStarSystemInfo(systemId);
			if (gatepoints.Any<int>())
			{
				foreach (int gatepoint in gatepoints)
				{
					Vector3 v = starSystemInfo2.Origin - starSystemInfo.Origin;
					Vector3 toCoords = v * ((float)gatepoint / 100f) + starSystemInfo.Origin;
					Vector3 arg_15B_0;
					if (gatepoint != gatepoints.First<int>())
					{
						arg_15B_0 = v * ((float)gatepoints[gatepoints.FindIndex((int x) => x == gatepoint) - 1] / 100f) + starSystemInfo.Origin;
					}
					else
					{
						arg_15B_0 = starSystemInfo.Origin;
					}
					Vector3 fromCoords = arg_15B_0;
					game.GameDatabase.InsertMoveOrder(fleetId, (gatepoint == gatepoints.First<int>()) ? game.GameDatabase.GetFleetInfo(fleetId).SystemID : 0, fromCoords, (gatepoint == gatepoints.Last<int>()) ? 0 : 0, toCoords);
				}
				Vector3 v2 = starSystemInfo2.Origin - starSystemInfo.Origin;
				Vector3 fromCoords2 = v2 * ((float)gatepoints.Last<int>() / 100f) + starSystemInfo.Origin;
				game.GameDatabase.InsertMoveOrder(fleetId, 0, fromCoords2, systemId, Vector3.Zero);
			}
			StarFleet.MissionTrace(string.Concat(new object[]
			{
				"Fleet ",
				fleetId,
				" sent on NPG mission to system: ",
				systemId
			}));
			return num;
		}
		public static IEnumerable<Vector3> GetAccelGateSlotsBetweenSystems(GameDatabase db, int systemida, int systemidb)
		{
			StarSystemInfo starSystemInfo = db.GetStarSystemInfo(systemida);
			StarSystemInfo starSystemInfo2 = db.GetStarSystemInfo(systemidb);
			List<Vector3> list = new List<Vector3>();
			float length = (starSystemInfo.Origin - starSystemInfo2.Origin).Length;
			int num = (int)Math.Floor((double)(length / db.AssetDatabase.LoaDistanceBetweenGates));
			for (int i = 0; i < num; i++)
			{
				Vector3 v = starSystemInfo2.Origin - starSystemInfo.Origin;
				Vector3 vector = v * (db.AssetDatabase.LoaDistanceBetweenGates * (float)(i + 1) / length) + starSystemInfo.Origin;
				float length2 = (vector - starSystemInfo2.Origin).Length;
				if (length2 >= db.AssetDatabase.LoaGateSystemMargin)
				{
					list.Add(vector);
				}
			}
			return list;
		}
		public static IEnumerable<int> GetAccelGatePercentPointsBetweenSystems(GameDatabase db, int systemida, int systemidb)
		{
			StarSystemInfo starSystemInfo = db.GetStarSystemInfo(systemida);
			StarSystemInfo starSystemInfo2 = db.GetStarSystemInfo(systemidb);
			float length = (starSystemInfo.Origin - starSystemInfo2.Origin).Length;
			List<int> list = new List<int>();
			List<Vector3> list2 = StarFleet.GetAccelGateSlotsBetweenSystems(db, starSystemInfo.ID, starSystemInfo2.ID).ToList<Vector3>();
			int count = list2.Count;
			for (int i = 0; i < count; i++)
			{
				list.Add((int)((starSystemInfo.Origin - list2[i]).Length / length * 100f));
			}
			return list;
		}
		public static int GetMaxLoaFleetCubeMassForTransit(GameSession game, int playerid)
		{
			Faction faction = game.AssetDatabase.GetFaction(game.GameDatabase.GetPlayerFactionID(playerid));
			if (faction.Name != "loa")
			{
				return 0;
			}
			int result = game.AssetDatabase.LoaBaseMaxMass;
			if (game.GameDatabase.GetStratModifier<bool>(StratModifiers.StandingNeutrinoWaves, playerid))
			{
				result = game.AssetDatabase.LoaMassStandingPulseWavesMaxMass;
			}
			if (game.GameDatabase.GetStratModifier<bool>(StratModifiers.MassInductionProjectors, playerid))
			{
				result = game.AssetDatabase.LoaMassInductionProjectorsMaxMass;
			}
			return result;
		}
		public static int SetGateMission(GameSession game, int fleetId, int systemId, bool useDirectRoute, List<int> designIds, int? ReBaseTarget = null)
		{
			GameDatabase gameDatabase = game.GameDatabase;
			if (designIds != null)
			{
				designIds.AddRange(StarFleet.GetFactionRequiredDesignsForFleet(game, fleetId, systemId));
			}
			else
			{
				designIds = StarFleet.GetFactionRequiredDesignsForFleet(game, fleetId, systemId);
			}
			int num = gameDatabase.InsertMission(fleetId, MissionType.GATE, systemId, 0, 0, 1, useDirectRoute, null);
			if (designIds != null && designIds.Count<int>() > 0)
			{
				StarFleet.AddShipsForMission(gameDatabase, fleetId, num, designIds);
			}
			StarFleet.SetWaypointsForMission(game, MissionType.GATE, num, fleetId, systemId, 0, ReBaseTarget);
			StarFleet.MissionTrace(string.Concat(new object[]
			{
				"Fleet ",
				fleetId,
				" sent on Gate mission to system: ",
				systemId
			}));
			return num;
		}
		public static int SetSurveyMission(GameSession game, int fleetId, int systemId, bool useDirectRoute, List<int> designIds, int? ReBaseTarget = null)
		{
			GameDatabase gameDatabase = game.GameDatabase;
			if (designIds != null)
			{
				designIds.AddRange(StarFleet.GetFactionRequiredDesignsForFleet(game, fleetId, systemId));
			}
			else
			{
				designIds = StarFleet.GetFactionRequiredDesignsForFleet(game, fleetId, systemId);
			}
			gameDatabase.GetFleetInfo(fleetId);
			int surveyPointsRequiredForSystem = StarFleet.GetSurveyPointsRequiredForSystem(gameDatabase, systemId);
			int num = gameDatabase.InsertMission(fleetId, MissionType.SURVEY, systemId, 0, 0, surveyPointsRequiredForSystem, useDirectRoute, null);
			if (designIds != null && designIds.Count<int>() > 0)
			{
				StarFleet.AddShipsForMission(gameDatabase, fleetId, num, designIds);
			}
			StarFleet.SetWaypointsForMission(game, MissionType.SURVEY, num, fleetId, systemId, 0, ReBaseTarget);
			StarFleet.MissionTrace(string.Concat(new object[]
			{
				"Fleet ",
				fleetId,
				" sent on Survey mission to system ",
				systemId
			}));
			return num;
		}
		public static int SetInterdictionMission(GameSession game, int fleetId, int systemId, bool useDirectRoute, int duration, List<int> designIds)
		{
			GameDatabase gameDatabase = game.GameDatabase;
			if (designIds != null)
			{
				designIds.AddRange(StarFleet.GetFactionRequiredDesignsForFleet(game, fleetId, systemId));
			}
			else
			{
				designIds = StarFleet.GetFactionRequiredDesignsForFleet(game, fleetId, systemId);
			}
			int num = gameDatabase.InsertMission(fleetId, MissionType.INTERDICTION, systemId, 0, 0, duration, useDirectRoute, null);
			if (designIds != null && designIds.Count<int>() > 0)
			{
				StarFleet.AddShipsForMission(gameDatabase, fleetId, num, designIds);
			}
			StarFleet.SetWaypointsForMission(game, MissionType.INTERDICTION, num, fleetId, systemId, 0, null);
			StarFleet.MissionTrace(string.Concat(new object[]
			{
				"Fleet ",
				fleetId,
				" sent on Interdiction mission to system ",
				systemId
			}));
			return num;
		}
		public static int SetStrikeMission(GameSession game, int fleetId, int systemId, bool useDirectRoute, int orbitalObjectId, int targetFleetId, List<int> designIds)
		{
			GameDatabase gameDatabase = game.GameDatabase;
			if (designIds != null)
			{
				designIds.AddRange(StarFleet.GetFactionRequiredDesignsForFleet(game, fleetId, systemId));
			}
			else
			{
				designIds = StarFleet.GetFactionRequiredDesignsForFleet(game, fleetId, systemId);
			}
			int num = gameDatabase.InsertMission(fleetId, MissionType.STRIKE, systemId, orbitalObjectId, targetFleetId, 1, useDirectRoute, null);
			if (designIds != null && designIds.Count<int>() > 0)
			{
				StarFleet.AddShipsForMission(gameDatabase, fleetId, num, designIds);
			}
			StarFleet.SetWaypointsForMission(game, MissionType.STRIKE, num, fleetId, systemId, 0, null);
			StarFleet.MissionTrace(string.Concat(new object[]
			{
				"Fleet ",
				fleetId,
				" sent on Strike mission to system ",
				systemId
			}));
			return num;
		}
		public static int SetPiracyMission(GameSession game, int fleetId, int systemId, bool useDirectRoute, int orbitalObjectId, int targetFleetId, List<int> designIds)
		{
			GameDatabase gameDatabase = game.GameDatabase;
			if (designIds != null)
			{
				designIds.AddRange(StarFleet.GetFactionRequiredDesignsForFleet(game, fleetId, systemId));
			}
			else
			{
				designIds = StarFleet.GetFactionRequiredDesignsForFleet(game, fleetId, systemId);
			}
			int num = gameDatabase.InsertMission(fleetId, MissionType.PIRACY, systemId, orbitalObjectId, targetFleetId, 1, useDirectRoute, null);
			if (designIds != null && designIds.Count<int>() > 0)
			{
				StarFleet.AddShipsForMission(gameDatabase, fleetId, num, designIds);
			}
			StarFleet.SetWaypointsForMission(game, MissionType.PIRACY, num, fleetId, systemId, 0, null);
			StarFleet.MissionTrace(string.Concat(new object[]
			{
				"Fleet ",
				fleetId,
				" sent on Piracy mission to system ",
				systemId
			}));
			return num;
		}
		public static int SetFleetInterceptMission(GameSession game, int fleetId, int targetFleet, bool useDirectRoute, List<int> designIds)
		{
			GameDatabase gameDatabase = game.GameDatabase;
			int num = gameDatabase.InsertMission(fleetId, MissionType.INTERCEPT, 0, 0, targetFleet, 6, useDirectRoute, null);
			if (designIds != null && designIds.Count<int>() > 0)
			{
				StarFleet.AddShipsForMission(gameDatabase, fleetId, num, designIds);
			}
			StarFleet.SetWaypointsForMission(game, MissionType.INTERCEPT, num, fleetId, targetFleet, 0, null);
			StarFleet.MissionTrace(string.Concat(new object[]
			{
				"Fleet ",
				fleetId,
				" sent on Intercept mission to fleet ",
				targetFleet
			}));
			return num;
		}
		public static int SetInvasionMission(GameSession game, int fleetId, int systemId, bool useDirectRoute, int orbitalObjectId, List<int> designIds)
		{
			GameDatabase gameDatabase = game.GameDatabase;
			if (designIds != null)
			{
				designIds.AddRange(StarFleet.GetFactionRequiredDesignsForFleet(game, fleetId, systemId));
			}
			else
			{
				designIds = StarFleet.GetFactionRequiredDesignsForFleet(game, fleetId, systemId);
			}
			int num = gameDatabase.InsertMission(fleetId, MissionType.INVASION, systemId, orbitalObjectId, 0, 6, useDirectRoute, null);
			if (designIds != null && designIds.Count<int>() > 0)
			{
				StarFleet.AddShipsForMission(gameDatabase, fleetId, num, designIds);
			}
			StarFleet.SetWaypointsForMission(game, MissionType.INVASION, num, fleetId, systemId, 0, null);
			StarFleet.MissionTrace(string.Concat(new object[]
			{
				"Fleet ",
				fleetId,
				" sent on Invasion mission to system ",
				systemId
			}));
			return num;
		}
		public static int SetSupportMission(GameSession game, int fleetId, int systemId, bool useDirectRoute, int orbitalObjectId, List<int> designIds, int numTrips, int? ReBaseTarget = null)
		{
			GameDatabase gameDatabase = game.GameDatabase;
			if (designIds != null)
			{
				designIds.AddRange(StarFleet.GetFactionRequiredDesignsForFleet(game, fleetId, systemId));
			}
			else
			{
				designIds = StarFleet.GetFactionRequiredDesignsForFleet(game, fleetId, systemId);
			}
			int num = gameDatabase.InsertMission(fleetId, MissionType.SUPPORT, systemId, orbitalObjectId, 0, 0, useDirectRoute, null);
			if (designIds != null && designIds.Count<int>() > 0)
			{
				StarFleet.AddShipsForMission(gameDatabase, fleetId, num, designIds);
			}
			StarFleet.SetWaypointsForMission(game, MissionType.SUPPORT, num, fleetId, systemId, numTrips, ReBaseTarget);
			StarFleet.MissionTrace(string.Concat(new object[]
			{
				"Fleet ",
				fleetId,
				" sent on Invasion mission to system ",
				systemId
			}));
			return num;
		}
		public static int SetConstructionMission(GameSession game, int fleetID, int systemID, bool useDirectRoute, int orbitalObjectID, List<int> designIDs, StationType type, int? ReBaseTarget = null)
		{
			string factionName = game.GameDatabase.GetFactionName(game.GameDatabase.GetFleetFaction(fleetID));
			int stationConstructionCost = StarFleet.GetStationConstructionCost(game, type, factionName, 1);
			MissionType missionType = MissionType.CONSTRUCT_STN;
			int num = game.GameDatabase.InsertMission(fleetID, missionType, systemID, orbitalObjectID, 0, stationConstructionCost, useDirectRoute, new int?((int)type));
			if (designIDs != null && designIDs.Count<int>() > 0)
			{
				StarFleet.AddShipsForMission(game.GameDatabase, fleetID, num, designIDs);
			}
			StarFleet.SetWaypointsForMission(game, missionType, num, fleetID, systemID, 0, ReBaseTarget);
			StarFleet.MissionTrace(string.Concat(new object[]
			{
				"Fleet ",
				fleetID,
				" sent to ",
				missionType.ToString(),
				" in system ",
				systemID
			}));
			return num;
		}
		public static int SetUpgradeStationMission(GameSession game, int fleetID, int systemID, bool useDirectRoute, int orbitalObjectID, List<int> designIDs, StationType type, int? ReBaseTarget = null)
		{
			StationInfo stationInfo = game.GameDatabase.GetStationInfo(orbitalObjectID);
			if (stationInfo != null)
			{
				string factionName = game.GameDatabase.GetFactionName(game.GameDatabase.GetFleetFaction(fleetID));
				int stationConstructionCost = StarFleet.GetStationConstructionCost(game, type, factionName, stationInfo.DesignInfo.StationLevel + 1);
				MissionType missionType = MissionType.CONSTRUCT_STN;
				if (stationInfo.DesignInfo.StationLevel > 0)
				{
					missionType = MissionType.UPGRADE_STN;
				}
				int num = game.GameDatabase.InsertMission(fleetID, missionType, systemID, orbitalObjectID, 0, stationConstructionCost, useDirectRoute, null);
				if (designIDs != null && designIDs.Count<int>() > 0)
				{
					StarFleet.AddShipsForMission(game.GameDatabase, fleetID, num, designIDs);
				}
				StarFleet.SetWaypointsForMission(game, missionType, num, fleetID, systemID, 0, ReBaseTarget);
				StarFleet.MissionTrace(string.Concat(new object[]
				{
					"Fleet ",
					fleetID,
					" sent to ",
					missionType.ToString(),
					" in system ",
					systemID
				}));
				return num;
			}
			return 0;
		}
		public static int SetSpecialConstructionMission(GameSession game, int fleetID, int targetFleetID, bool useDirectRoute, List<int> designIDs, StationType type)
		{
			string factionName = game.GameDatabase.GetFactionName(game.GameDatabase.GetFleetFaction(fleetID));
			int stationConstructionCost = StarFleet.GetStationConstructionCost(game, type, factionName, 1);
			MissionType missionType = MissionType.SPECIAL_CONSTRUCT_STN;
			int num = game.GameDatabase.InsertMission(fleetID, missionType, 0, 0, targetFleetID, stationConstructionCost, useDirectRoute, new int?((int)type));
			if (designIDs != null && designIDs.Count<int>() > 0)
			{
				StarFleet.AddShipsForMission(game.GameDatabase, fleetID, num, designIDs);
			}
			StarFleet.SetWaypointsForMission(game, missionType, num, fleetID, 0, 0, null);
			StarFleet.MissionTrace(string.Concat(new object[]
			{
				"Fleet ",
				fleetID,
				" sent to build ",
				missionType.ToString(),
				" in system of GM fleet ",
				targetFleetID
			}));
			return num;
		}
		public static void ForceReturnMission(GameDatabase game, FleetInfo fleetInfo)
		{
			int missionID = game.InsertMission(fleetInfo.ID, MissionType.RETURN, 0, 0, 0, 0, false, null);
			game.InsertWaypoint(missionID, WaypointType.ReturnHome, null);
		}
		public static void CancelMission(GameSession game, FleetInfo fleetInfo, bool removeStation = true)
		{
			if (fleetInfo == null)
			{
				return;
			}
			MoveOrderInfo moveOrderInfoByFleetID = game.GameDatabase.GetMoveOrderInfoByFleetID(fleetInfo.ID);
			MissionInfo missionByFleetID = game.GameDatabase.GetMissionByFleetID(fleetInfo.ID);
			if (missionByFleetID == null || missionByFleetID.Type == MissionType.RETREAT || missionByFleetID.Type == MissionType.RETURN)
			{
				return;
			}
			bool flag = missionByFleetID.Type == MissionType.CONSTRUCT_STN && removeStation;
			int targetOrbitalObjectID = missionByFleetID.TargetOrbitalObjectID;
			if (moveOrderInfoByFleetID == null && fleetInfo.SystemID == fleetInfo.SupportingSystemID)
			{
				game.GameDatabase.RemoveMission(missionByFleetID.ID);
			}
			else
			{
				if (moveOrderInfoByFleetID != null && moveOrderInfoByFleetID.FromSystemID != 0 && moveOrderInfoByFleetID.ToSystemID != 0)
				{
					Faction faction = game.AssetDatabase.GetFaction(game.GameDatabase.GetPlayerFactionID(fleetInfo.PlayerID));
					if (((game.GameDatabase.GetNodeLineBetweenSystems(fleetInfo.PlayerID, moveOrderInfoByFleetID.FromSystemID, moveOrderInfoByFleetID.ToSystemID, true, false) == null || !faction.CanUseNodeLine(new bool?(true))) && (game.GameDatabase.GetNodeLineBetweenSystems(fleetInfo.PlayerID, moveOrderInfoByFleetID.FromSystemID, moveOrderInfoByFleetID.ToSystemID, false, false) == null || !faction.CanUseNodeLine(new bool?(false)))) || faction.Name == "loa")
					{
						int num;
						float num2;
						List<int> bestTravelPath = StarFleet.GetBestTravelPath(game, fleetInfo.ID, fleetInfo.SystemID, game.GameDatabase.GetHomeSystem(game, missionByFleetID.ID, fleetInfo), out num, out num2, missionByFleetID.UseDirectRoute, null, null);
						if (bestTravelPath[1] == moveOrderInfoByFleetID.FromSystemID || (faction.CanUseGate() && game.GameDatabase.SystemHasGate(moveOrderInfoByFleetID.FromSystemID, fleetInfo.PlayerID) && !game.GameDatabase.SystemHasGate(moveOrderInfoByFleetID.ToSystemID, fleetInfo.PlayerID) && !game.GameDatabase.SystemHasGate(fleetInfo.SystemID, fleetInfo.PlayerID)))
						{
							game.GameDatabase.InsertMoveOrder(moveOrderInfoByFleetID.FleetID, 0, game.GameDatabase.GetFleetLocation(fleetInfo.ID, false).Coords, moveOrderInfoByFleetID.FromSystemID, Vector3.Zero);
							game.GameDatabase.RemoveMoveOrder(moveOrderInfoByFleetID.ID);
						}
					}
				}
				missionByFleetID.Type = MissionType.RETURN;
				missionByFleetID.TargetSystemID = 0;
				missionByFleetID.TargetOrbitalObjectID = 0;
				missionByFleetID.TargetFleetID = 0;
				game.GameDatabase.UpdateMission(missionByFleetID);
				game.GameDatabase.ClearWaypoints(missionByFleetID.ID);
				game.GameDatabase.InsertWaypoint(missionByFleetID.ID, WaypointType.ReturnHome, null);
			}
			if (flag)
			{
				StationInfo stationInfo = game.GameDatabase.GetStationInfo(targetOrbitalObjectID);
				if (stationInfo != null && stationInfo.DesignInfo.StationLevel == 0)
				{
					game.GameDatabase.DestroyStation(game, targetOrbitalObjectID, missionByFleetID.ID);
				}
			}
		}
		public static ShipSectionAsset GetStationAsset(GameSession game, StationType type, string faction, int level)
		{
			List<ShipSectionAsset> source = (
				from x in game.AssetDatabase.ShipSections
				where x.Class == ShipClass.Station && x.Faction == faction
				select x).ToList<ShipSectionAsset>();
			return source.FirstOrDefault((ShipSectionAsset x) => x.StationType == type && x.StationLevel == level);
		}
		public static int GetStationSavingsCost(GameSession game, StationType type, string faction, int level)
		{
			ShipSectionAsset stationAsset = StarFleet.GetStationAsset(game, type, faction, level);
			if (stationAsset != null)
			{
				return stationAsset.SavingsCost;
			}
			return 0;
		}
		public static int GetStationConstructionCost(GameSession game, StationType type, string faction, int level)
		{
			ShipSectionAsset stationAsset = StarFleet.GetStationAsset(game, type, faction, level);
			if (stationAsset != null)
			{
				return stationAsset.ProductionCost;
			}
			return 0;
		}
		public static void ConfigureFleetForMission(GameSession game, int fleetID)
		{
			GameDatabase gameDatabase = game.GameDatabase;
			FleetInfo fleetInfo = gameDatabase.GetFleetInfo(fleetID);
			if (fleetInfo == null || fleetInfo.IsReserveFleet || fleetInfo.IsDefenseFleet)
			{
				return;
			}
			FleetInfo fleetInfo2 = gameDatabase.InsertOrGetReserveFleetInfo(fleetInfo.SupportingSystemID, fleetInfo.PlayerID);
			MissionInfo missionByFleetID = gameDatabase.GetMissionByFleetID(fleetID);
			if (missionByFleetID == null || fleetInfo.SystemID != fleetInfo.SupportingSystemID)
			{
				return;
			}
			List<ShipInfo> list = gameDatabase.GetShipInfoByFleetID(fleetInfo.ID, true).ToList<ShipInfo>();
			ShipRole[] source = StarFleet.CollectShipRolesForMission(missionByFleetID.Type);
			int num = 0;
			foreach (ShipInfo current in list)
			{
				if (current.DesignInfo.Role == ShipRole.COMMAND)
				{
					int designCommandPointQuota = gameDatabase.GetDesignCommandPointQuota(gameDatabase.AssetDatabase, current.DesignID);
					if (designCommandPointQuota > num)
					{
						num = designCommandPointQuota;
					}
				}
			}
			num -= gameDatabase.GetFleetCommandPointCost(fleetInfo.ID);
			if (num > 0)
			{
				ShipInfo[] array = gameDatabase.GetShipInfoByFleetID(fleetInfo2.ID, true).ToArray<ShipInfo>();
				for (int i = 0; i < array.Length; i++)
				{
					ShipInfo shipInfo = array[i];
					if (source.Contains(shipInfo.DesignInfo.Role))
					{
						int commandPointCost = gameDatabase.GetCommandPointCost(shipInfo.DesignID);
						if (commandPointCost < num)
						{
							gameDatabase.TransferShip(shipInfo.ID, fleetInfo.ID);
							num -= commandPointCost;
						}
					}
				}
			}
		}
		private static ShipRole[] CollectShipRolesForMission(MissionType mission)
		{
			ShipRole[] result;
			switch (mission)
			{
			case MissionType.COLONIZATION:
			case MissionType.SUPPORT:
			case MissionType.EVACUATE:
				result = new ShipRole[]
				{
					ShipRole.COMMAND,
					ShipRole.SUPPLY,
					ShipRole.COLONIZER,
					ShipRole.COMBAT
				};
				return result;
			case MissionType.SURVEY:
			case MissionType.PATROL:
			case MissionType.STRIKE:
			case MissionType.INVASION:
			case MissionType.PIRACY:
				result = new ShipRole[]
				{
					ShipRole.COMMAND,
					ShipRole.SUPPLY,
					ShipRole.COMBAT
				};
				return result;
			case MissionType.CONSTRUCT_STN:
			case MissionType.SPECIAL_CONSTRUCT_STN:
				result = new ShipRole[]
				{
					ShipRole.COMMAND,
					ShipRole.SUPPLY,
					ShipRole.CONSTRUCTOR,
					ShipRole.COMBAT
				};
				return result;
			case MissionType.GATE:
				result = new ShipRole[]
				{
					ShipRole.COMMAND,
					ShipRole.SUPPLY,
					ShipRole.GATE,
					ShipRole.COMBAT
				};
				return result;
			}
			result = new ShipRole[]
			{
				ShipRole.COMMAND,
				ShipRole.SUPPLY,
				ShipRole.COMBAT
			};
			return result;
		}
		private static void AddShipsForMission(GameDatabase db, int fleetID, int missionID, IList<int> designIDs)
		{
			FleetInfo fleetInfo = db.GetFleetInfo(fleetID);
			db.InsertBuildOrders(fleetInfo.SystemID, designIDs, 1, missionID, null, null);
		}
		public static float GetSensorTravelDistance(GameSession game, int startId, int endId, int fleetId)
		{
			if (startId == 0 || endId == 0)
			{
				return 3.40282347E+38f;
			}
			float num = 0f;
			int num2;
			List<int> bestTravelPath = StarFleet.GetBestTravelPath(game, fleetId, startId, endId, out num2, out num, false, null, null);
			num = 0f;
			for (int i = 0; i < bestTravelPath.Count - 1; i++)
			{
				num += (game.GameDatabase.GetStarSystemOrigin(bestTravelPath[i]) - game.GameDatabase.GetStarSystemOrigin(bestTravelPath[i + 1])).Length;
			}
			return num;
		}
		public static bool DoAutoPatrol(GameSession game, FleetInfo fleet, MissionInfo currentMission)
		{
			App app = game.App;
			MissionType type = currentMission.Type;
			switch (type)
			{
			case MissionType.COLONIZATION:
			case MissionType.SUPPORT:
			case MissionType.RELOCATION:
			case MissionType.PATROL:
				break;
			case MissionType.SURVEY:
			case MissionType.CONSTRUCT_STN:
			case MissionType.UPGRADE_STN:
				goto IL_4B;
			default:
				switch (type)
				{
				case MissionType.INTERCEPT:
				case MissionType.RETURN:
					break;
				case MissionType.GATE:
					goto IL_4B;
				default:
					goto IL_4B;
				}
				break;
			}
			return false;
			IL_4B:
			if (currentMission == null || currentMission.TargetSystemID == 0)
			{
				return false;
			}
			if (StarFleet.IsFleetExhausted(game, fleet))
			{
				return false;
			}
			if (game.GameDatabase.GetWaypointsByMissionID(currentMission.ID).Any((WaypointInfo x) => x.Type == WaypointType.ReBase))
			{
				return false;
			}
			app.GameDatabase.ClearWaypoints(currentMission.ID);
			app.GameDatabase.RemoveMission(currentMission.ID);
			StarFleet.SetPatrolMission(game, currentMission.FleetID, currentMission.TargetSystemID, false, null, null);
			return true;
		}
		public static bool SetReturnMission(GameSession game, FleetInfo fleet, MissionInfo currentMission)
		{
			MissionType type = currentMission.Type;
			switch (type)
			{
			case MissionType.COLONIZATION:
			case MissionType.SUPPORT:
			case MissionType.RELOCATION:
				break;
			case MissionType.SURVEY:
				goto IL_38;
			default:
				switch (type)
				{
				case MissionType.INTERCEPT:
				case MissionType.RETURN:
					break;
				case MissionType.GATE:
					goto IL_38;
				default:
					goto IL_38;
				}
				break;
			}
			return false;
			IL_38:
			if (game.GameDatabase.GetWaypointsByMissionID(currentMission.ID).Any((WaypointInfo x) => x.Type == WaypointType.ReBase))
			{
				return false;
			}
			if (fleet.SupportingSystemID != fleet.SystemID)
			{
				currentMission.Type = MissionType.RETURN;
				currentMission.TargetSystemID = 0;
				currentMission.TargetOrbitalObjectID = 0;
				currentMission.TargetFleetID = 0;
				game.GameDatabase.UpdateMission(currentMission);
				game.GameDatabase.ClearWaypoints(currentMission.ID);
				game.GameDatabase.InsertWaypoint(currentMission.ID, WaypointType.ReturnHome, null);
				return true;
			}
			return false;
		}
		public static void SetWaypointsForMission(GameSession game, MissionType type, int missionID, int fleetID, int systemID, int numTrips = 0, int? ReBaseTarget = null)
		{
			GameDatabase gameDatabase = game.GameDatabase;
			WaypointType type2 = WaypointType.ReturnHome;
			if (ReBaseTarget.HasValue)
			{
				type2 = WaypointType.ReBase;
			}
			switch (type)
			{
			case MissionType.COLONIZATION:
				gameDatabase.InsertWaypoint(missionID, WaypointType.TravelTo, new int?(systemID));
				gameDatabase.InsertWaypoint(missionID, WaypointType.DoMission, null);
				gameDatabase.InsertWaypoint(missionID, type2, null);
				gameDatabase.InsertWaypoint(missionID, WaypointType.CheckSupportColony, null);
				if (ReBaseTarget.HasValue)
				{
					gameDatabase.InsertWaypoint(missionID, WaypointType.TravelTo, new int?(ReBaseTarget.Value));
					gameDatabase.InsertWaypoint(missionID, type2, null);
					return;
				}
				break;
			case MissionType.SUPPORT:
				for (int i = 0; i < numTrips; i++)
				{
					if (gameDatabase.GetFleetInfo(fleetID).SupportingSystemID != systemID)
					{
						gameDatabase.InsertWaypoint(missionID, WaypointType.TravelTo, new int?(systemID));
					}
					gameDatabase.InsertWaypoint(missionID, WaypointType.DoMission, null);
					gameDatabase.InsertWaypoint(missionID, type2, null);
				}
				gameDatabase.InsertWaypoint(missionID, WaypointType.CheckSupportColony, null);
				if (ReBaseTarget.HasValue)
				{
					gameDatabase.InsertWaypoint(missionID, WaypointType.TravelTo, new int?(ReBaseTarget.Value));
					gameDatabase.InsertWaypoint(missionID, type2, null);
					return;
				}
				break;
			case MissionType.SURVEY:
			case MissionType.CONSTRUCT_STN:
			case MissionType.UPGRADE_STN:
			case MissionType.PATROL:
			case MissionType.INTERDICTION:
			case MissionType.STRIKE:
			case MissionType.INVASION:
			case MissionType.GATE:
			case MissionType.PIRACY:
				gameDatabase.InsertWaypoint(missionID, WaypointType.TravelTo, new int?(systemID));
				gameDatabase.InsertWaypoint(missionID, WaypointType.DoMission, null);
				if (ReBaseTarget.HasValue)
				{
					gameDatabase.InsertWaypoint(missionID, WaypointType.TravelTo, new int?(ReBaseTarget.Value));
				}
				gameDatabase.InsertWaypoint(missionID, type2, null);
				return;
			case MissionType.RELOCATION:
				gameDatabase.InsertWaypoint(missionID, WaypointType.TravelTo, new int?(systemID));
				gameDatabase.InsertWaypoint(missionID, WaypointType.DoMission, null);
				return;
			case MissionType.INTERCEPT:
				gameDatabase.InsertWaypoint(missionID, WaypointType.DoMission, null);
				gameDatabase.InsertWaypoint(missionID, type2, null);
				return;
			case MissionType.RETURN:
			case MissionType.RETREAT:
				break;
			case MissionType.DEPLOY_NPG:
				gameDatabase.InsertWaypoint(missionID, WaypointType.DoMission, new int?(systemID));
				gameDatabase.InsertWaypoint(missionID, type2, null);
				break;
			case MissionType.EVACUATE:
				gameDatabase.InsertWaypoint(missionID, WaypointType.TravelTo, new int?(systemID));
				gameDatabase.InsertWaypoint(missionID, WaypointType.DoMission, null);
				gameDatabase.InsertWaypoint(missionID, type2, null);
				gameDatabase.InsertWaypoint(missionID, WaypointType.CheckEvacuate, null);
				return;
			case MissionType.SPECIAL_CONSTRUCT_STN:
				gameDatabase.InsertWaypoint(missionID, WaypointType.DoMission, null);
				gameDatabase.InsertWaypoint(missionID, type2, null);
				return;
			default:
				return;
			}
		}
		public static int GetTurnsToSurveySystem(GameDatabase db, int systemID, IEnumerable<ShipInfo> ships)
		{
			int fleetSurveyPoints = StarFleet.GetFleetSurveyPoints(db, ships);
			if (fleetSurveyPoints < 1)
			{
				return -1;
			}
			int surveyPointsRequiredForSystem = StarFleet.GetSurveyPointsRequiredForSystem(db, systemID);
			int val = surveyPointsRequiredForSystem / fleetSurveyPoints;
			return Math.Max(val, 1);
		}
		public static int GetTurnsToSurveySystem(GameDatabase db, int systemID, int fleetID)
		{
			return StarFleet.GetTurnsToSurveySystem(db, systemID, db.GetShipInfoByFleetID(fleetID, true));
		}
		public static int GetFleetSurveyPoints(GameDatabase db, IEnumerable<ShipInfo> ships)
		{
			List<int> list = new List<int>();
			int num = 0;
			foreach (ShipInfo current in ships)
			{
				if (current.DesignInfo.Class == ShipClass.BattleRider)
				{
					if (list.Contains(current.ParentID))
					{
						continue;
					}
					list.Add(current.ParentID);
				}
				bool flag = db.GetDesignAttributesForDesign(current.DesignID).Contains(SectionEnumerations.DesignAttribute.Louis_And_Clark);
				num += (flag ? 4 : 2);
				foreach (string current2 in db.GetDesignSectionNames(current.DesignID))
				{
					if (current2.Contains("deepscan"))
					{
						num += (flag ? 16 : 8);
						break;
					}
				}
			}
			if (ships.Count<ShipInfo>() > 0)
			{
				List<AdmiralInfo.TraitType> list2 = db.GetAdmiralTraits(db.GetFleetInfo(ships.First<ShipInfo>().FleetID).AdmiralID).ToList<AdmiralInfo.TraitType>();
				if (list2.Contains(AdmiralInfo.TraitType.Pathfinder))
				{
					num = (int)Math.Floor((double)((float)num * 1.25f));
				}
				else
				{
					if (list2.Contains(AdmiralInfo.TraitType.Livingstone))
					{
						num = (int)Math.Floor((double)((float)num * 0.75f));
					}
				}
			}
			return num;
		}
		public static int GetFleetSurveyPoints(GameDatabase db, int fleetID)
		{
			return StarFleet.GetFleetSurveyPoints(db, db.GetShipInfoByFleetID(fleetID, true));
		}
		public static int GetSurveyPointsRequiredForSystem(GameDatabase db, int systemID)
		{
			int num = 0;
			PlanetInfo[] starSystemPlanetInfos = db.GetStarSystemPlanetInfos(systemID);
			for (int i = 0; i < starSystemPlanetInfos.Length; i++)
			{
				PlanetInfo planetInfo = starSystemPlanetInfos[i];
				OrbitalObjectInfo orbitalObjectInfo = db.GetOrbitalObjectInfo(planetInfo.ID);
				if (orbitalObjectInfo.ParentID.HasValue)
				{
					num += 5;
				}
				else
				{
					if (planetInfo.Type == "gaseous")
					{
						num += 15;
					}
					else
					{
						num += 10;
					}
				}
			}
			foreach (AsteroidBeltInfo arg_76_0 in db.GetStarSystemAsteroidBeltInfos(systemID))
			{
				num += 20;
			}
			return num;
		}
		public static List<int> GetBestTravelPath(GameSession game, int fleetId, int fromSystem, int toSystem, out int tripTime, out float tripDistance, bool useDirectRoute, float? travelSpeed = null, float? nodeTravelSpeed = null)
		{
			FleetInfo fleetInfo = game.GameDatabase.GetFleetInfo(fleetId);
			Faction faction = game.AssetDatabase.GetFaction(game.GameDatabase.GetPlayerFactionID(fleetInfo.PlayerID));
			int num = 2147483647;
			float num2 = 3.40282347E+38f;
			tripDistance = 3.40282347E+38f;
			tripTime = 2147483647;
			List<int> result;
			if (faction.CanUseNodeLine(new bool?(false)) && !useDirectRoute && fromSystem != 0 && toSystem != 0)
			{
				result = StarFleet.GetTempNodeTravelPath(game, fleetInfo, fromSystem, toSystem, out tripTime, out tripDistance, travelSpeed, nodeTravelSpeed).ToList<int>();
			}
			else
			{
				result = StarFleet.GetLinearTravelPath(game, fleetInfo, fromSystem, toSystem, out tripTime, out tripDistance, travelSpeed).ToList<int>();
			}
			if (faction.CanUseNodeLine(new bool?(true)))
			{
				List<int> list = StarFleet.GetPermanentNodeTravelPath(game, fleetInfo, fromSystem, toSystem, out num, out num2, travelSpeed, nodeTravelSpeed).ToList<int>();
				if (num < tripTime || tripTime == -2147483648)
				{
					tripTime = num;
					result = list;
					tripDistance = num2;
				}
			}
			if (faction.CanUseGate() && fromSystem != 0 && toSystem != 0)
			{
				List<int> list = StarFleet.GetGateTravelPath(game, fleetInfo, fromSystem, toSystem, out num, out num2, travelSpeed).ToList<int>();
				if (num < tripTime || tripTime == -2147483648)
				{
					tripTime = num;
					result = list;
					tripDistance = num2;
				}
			}
			if (faction.CanUseAccelerators() && fromSystem != 0 && toSystem != 0)
			{
				List<int> list = StarFleet.GetLoaAcceleratorTravelPath(game, fleetInfo, fromSystem, toSystem, out num, out num2, travelSpeed, nodeTravelSpeed).ToList<int>();
				if (num < tripTime)
				{
					tripTime = num;
					result = list;
					tripDistance = num2;
				}
			}
			return result;
		}
		public static List<int> GetLinearTravelPath(GameSession game, FleetInfo fleet, int fromSystemID, int toSystemID, out int tripTime, out float tripDistance, float? travelSpeed = null)
		{
			float num = travelSpeed.HasValue ? travelSpeed.Value : StarFleet.GetFleetTravelSpeed(game, fleet.ID, false);
			MoveOrderInfo moveOrderInfo = new MoveOrderInfo
			{
				FleetID = fleet.ID,
				FromSystemID = fromSystemID,
				ToSystemID = toSystemID
			};
			if (fromSystemID != 0 && toSystemID != 0)
			{
				if (game.AssetDatabase.GetFaction(game.GameDatabase.GetPlayerFactionID(fleet.PlayerID)).CanUseGravityWell() && !game.GameDatabase.FleetHasCurvatureComp(fleet))
				{
					tripDistance = GameSession.GetGravityWellTravelDistance(game.GameDatabase, moveOrderInfo);
				}
				else
				{
					tripDistance = GameSession.GetSystemToSystemDistance(game.GameDatabase, fromSystemID, toSystemID);
				}
			}
			else
			{
				Vector3 v;
				if (moveOrderInfo.FromSystemID != 0)
				{
					v = game.GameDatabase.GetStarSystemInfo(moveOrderInfo.FromSystemID).Origin;
				}
				else
				{
					v = moveOrderInfo.FromCoords;
				}
				Vector3 v2;
				if (moveOrderInfo.ToSystemID != 0)
				{
					v2 = game.GameDatabase.GetStarSystemInfo(moveOrderInfo.ToSystemID).Origin;
				}
				else
				{
					v2 = moveOrderInfo.ToCoords;
				}
				tripDistance = (v2 - v).Length;
			}
			tripTime = (int)Math.Ceiling((double)(tripDistance / num));
			return new List<int>
			{
				fromSystemID,
				toSystemID
			};
		}
		public static List<int> GetLoaAcceleratorTravelPath(GameSession game, FleetInfo fleet, int fromSystemID, int toSystemID, out int tripTime, out float tripDistance, float? travelSpeed = null, float? nodeTravelSpeed = null)
		{
			float num = nodeTravelSpeed.HasValue ? nodeTravelSpeed.Value : StarFleet.GetFleetTravelSpeed(game, fleet.ID, true);
			List<int> list = StarFleet.GetNodeTravelPath(game.GameDatabase, fromSystemID, toSystemID, fleet.PlayerID, false, false, true).ToList<int>();
			Vector3 v = game.GameDatabase.GetStarSystemOrigin(fromSystemID);
			tripDistance = 0f;
			foreach (int current in list)
			{
				Vector3 starSystemOrigin = game.GameDatabase.GetStarSystemOrigin(current);
				tripDistance += (v - starSystemOrigin).Length;
				v = starSystemOrigin;
			}
			tripTime = (int)Math.Ceiling((double)(tripDistance / num));
			if (list.Count <= 1)
			{
				return StarFleet.GetLinearTravelPath(game, fleet, fromSystemID, toSystemID, out tripTime, out tripDistance, travelSpeed);
			}
			return list;
		}
		public static List<int> GetTempNodeTravelPath(GameSession game, FleetInfo fleet, int fromSystemID, int toSystemID, out int tripTime, out float tripDistance, float? travelSpeed = null, float? nodeTravelSpeed = null)
		{
			float num = nodeTravelSpeed.HasValue ? nodeTravelSpeed.Value : StarFleet.GetFleetTravelSpeed(game, fleet.ID, true);
			List<int> list = StarFleet.GetNodeTravelPath(game.GameDatabase, fromSystemID, toSystemID, fleet.PlayerID, false, false, game.AssetDatabase.GetFaction(game.GameDatabase.GetPlayerFactionID(fleet.PlayerID)).Name == "loa").ToList<int>();
			Vector3 v = game.GameDatabase.GetStarSystemOrigin(fromSystemID);
			tripDistance = 0f;
			foreach (int current in list)
			{
				Vector3 starSystemOrigin = game.GameDatabase.GetStarSystemOrigin(current);
				tripDistance += (v - starSystemOrigin).Length;
				v = starSystemOrigin;
			}
			tripTime = (int)Math.Ceiling((double)(tripDistance / num));
			if (list.Count <= 1)
			{
				return StarFleet.GetLinearTravelPath(game, fleet, fromSystemID, toSystemID, out tripTime, out tripDistance, travelSpeed);
			}
			return list;
		}
		public static List<int> GetPermanentNodeTravelPath(GameSession game, FleetInfo fleet, int fromSystemID, int toSystemID, out int tripTime, out float tripDistance, float? travelSpeed = null, float? nodeTravelSpeed = null)
		{
			float num = nodeTravelSpeed.HasValue ? nodeTravelSpeed.Value : StarFleet.GetFleetTravelSpeed(game, fleet.ID, true);
			List<int> list = StarFleet.GetNodeTravelPath(game.GameDatabase, fromSystemID, toSystemID, fleet.PlayerID, true, true, false).ToList<int>();
			Vector3 v;
			if (fromSystemID == 0)
			{
				v = game.GameDatabase.GetFleetLocation(fleet.ID, false).Coords;
			}
			else
			{
				v = game.GameDatabase.GetStarSystemOrigin(fromSystemID);
			}
			tripDistance = 0f;
			foreach (int current in list)
			{
				Vector3 starSystemOrigin = game.GameDatabase.GetStarSystemOrigin(current);
				tripDistance += (v - starSystemOrigin).Length;
				v = starSystemOrigin;
			}
			tripTime = (int)Math.Ceiling((double)(tripDistance / num));
			if (list.Count <= 1)
			{
				return StarFleet.GetLinearTravelPath(game, fleet, fromSystemID, toSystemID, out tripTime, out tripDistance, travelSpeed);
			}
			return list;
		}
		public static List<int> GetGateTravelPath(GameSession game, FleetInfo fleet, int fromSystemID, int toSystemID, out int tripTime, out float tripDistance, float? travelSpeed = null)
		{
			List<int> list = new List<int>();
			list.Add(fromSystemID);
			if (game.GameDatabase.SystemHasGate(fromSystemID, fleet.PlayerID))
			{
				if (game.GameDatabase.SystemHasGate(toSystemID, fleet.PlayerID))
				{
					list.Add(toSystemID);
					tripTime = 1;
					tripDistance = 0.1f;
					return list;
				}
				StarSystemInfo starSystemInfo = game.GameDatabase.GetStarSystemInfo(fromSystemID);
				StarSystemInfo starSystemInfo2 = game.GameDatabase.GetStarSystemInfo(toSystemID);
				float num = travelSpeed.HasValue ? travelSpeed.Value : StarFleet.GetFleetTravelSpeed(game, fleet.ID, false);
				float length = (starSystemInfo2.Origin - starSystemInfo.Origin).Length;
				List<StarSystemInfo> closestGates = game.GetClosestGates(fleet.PlayerID, starSystemInfo2, length);
				if (closestGates.Count > 0)
				{
					StarSystemInfo starSystemInfo3 = closestGates.First<StarSystemInfo>();
					if (starSystemInfo3.ID != fromSystemID)
					{
						list.Add(starSystemInfo3.ID);
					}
					list.Add(toSystemID);
					float num2 = Math.Max(0f, (starSystemInfo3.Origin - starSystemInfo2.Origin).Length - game.GameDatabase.GetStratModifier<float>(StratModifiers.GateCastDistance, fleet.PlayerID));
					tripDistance = num2;
					tripTime = Math.Max((int)Math.Ceiling((double)(num2 / num)), 1);
					return list;
				}
				return StarFleet.GetLinearTravelPath(game, fleet, fromSystemID, toSystemID, out tripTime, out tripDistance, new float?(num));
			}
			else
			{
				StarSystemInfo starSystemInfo4 = game.GameDatabase.GetStarSystemInfo(fromSystemID);
				StarSystemInfo starSystemInfo5 = game.GameDatabase.GetStarSystemInfo(toSystemID);
				float num3 = travelSpeed.HasValue ? travelSpeed.Value : StarFleet.GetFleetTravelSpeed(game, fleet.ID, false);
				float length2 = (starSystemInfo5.Origin - starSystemInfo4.Origin).Length;
				List<StarSystemInfo> closestGates2 = game.GetClosestGates(fleet.PlayerID, starSystemInfo4, length2);
				List<StarSystemInfo> closestGates3 = game.GetClosestGates(fleet.PlayerID, starSystemInfo5, length2);
				if (closestGates2.Count <= 0 || closestGates3.Count <= 0)
				{
					return StarFleet.GetLinearTravelPath(game, fleet, fromSystemID, toSystemID, out tripTime, out tripDistance, new float?(num3));
				}
				StarSystemInfo starSystemInfo6 = closestGates2.First<StarSystemInfo>();
				StarSystemInfo starSystemInfo7 = closestGates3.First<StarSystemInfo>();
				float num4 = (starSystemInfo4.Origin - starSystemInfo6.Origin).Length + Math.Max(0f, (starSystemInfo5.Origin - starSystemInfo7.Origin).Length - game.GameDatabase.GetStratModifier<float>(StratModifiers.GateCastDistance, fleet.PlayerID));
				if (num4 < length2)
				{
					if (starSystemInfo6.ID != fromSystemID)
					{
						list.Add(starSystemInfo6.ID);
					}
					list.Add(starSystemInfo7.ID);
					if (starSystemInfo7.ID != starSystemInfo5.ID)
					{
						list.Add(starSystemInfo5.ID);
					}
					tripDistance = num4;
					tripTime = Math.Max((int)Math.Ceiling((double)(num4 / num3)), 1);
					return list;
				}
				return StarFleet.GetLinearTravelPath(game, fleet, fromSystemID, toSystemID, out tripTime, out tripDistance, new float?(num3));
			}
		}
		internal static IEnumerable<int> GetNodeTravelPath(GameDatabase db, int fromSystemID, int toSystemID, int playerID, bool permanent, bool nodeLinesOnly = false, bool loalines = false)
		{
            return Kerberos.Sots.StarSystemPathing.StarSystemPathing.FindClosestPath(db, playerID, fromSystemID, toSystemID, nodeLinesOnly);
		}
		public static void AddNodeToList(List<StarFleet.EvaluatedNode> list, StarFleet.EvaluatedNode node)
		{
			foreach (StarFleet.EvaluatedNode current in list)
			{
				if (current.SystemID == node.SystemID)
				{
					if (current.FCost > node.FCost)
					{
						current.FCost = node.FCost;
						current.FromNodeID = node.FromNodeID;
					}
					return;
				}
			}
			list.Add(node);
		}
		public static List<FleetInfo> GetFleetInfosForMission(GameSession game, int systemId)
		{
			return (
				from x in game.GameDatabase.GetFleetInfosByPlayerID(game.LocalPlayer.ID, FleetType.FL_NORMAL | FleetType.FL_RESERVE)
				where StarFleet.IsFleetAvailableForMission(game, x.ID, systemId)
				select x).ToList<FleetInfo>();
		}
		public static int GetTurnsRemainingForMissionFleet(GameSession sim, FleetInfo fleet)
		{
			MissionInfo missionByFleetID = sim.GameDatabase.GetMissionByFleetID(fleet.ID);
			int result = 0;
			if (missionByFleetID != null && fleet.PlayerID == sim.LocalPlayer.ID)
			{
				StationInfo stationInfo = sim.GameDatabase.GetStationInfo(missionByFleetID.TargetOrbitalObjectID);
				MissionEstimate missionEstimate;
				if (stationInfo != null)
				{
					missionEstimate = StarFleet.GetMissionEstimate(sim, missionByFleetID.Type, stationInfo.DesignInfo.StationType, fleet.ID, missionByFleetID.TargetSystemID, missionByFleetID.TargetOrbitalObjectID, null, 1, false, null, null);
				}
				else
				{
					missionEstimate = StarFleet.GetMissionEstimate(sim, missionByFleetID.Type, StationType.INVALID_TYPE, fleet.ID, missionByFleetID.TargetSystemID, missionByFleetID.TargetOrbitalObjectID, null, 1, false, null, null);
				}
				if (missionEstimate != null)
				{
					result = missionEstimate.TotalTurns + missionByFleetID.TurnStarted - sim.GameDatabase.GetTurnCount();
				}
			}
			return result;
		}
		public static int? GetTravelTime(GameSession game, FleetInfo fleetInfo, int toSystemID, bool restrictToPermanentNodeLines = false, float? travelSpeed = null, float? nodeTravelSpeed = null)
		{
			int value = 0;
			float num = 0f;
			if (fleetInfo.SupportingSystemID != 0 && toSystemID != 0)
			{
				List<int> bestTravelPath = StarFleet.GetBestTravelPath(game, fleetInfo.ID, fleetInfo.SupportingSystemID, toSystemID, out value, out num, false, travelSpeed, nodeTravelSpeed);
				if (restrictToPermanentNodeLines)
				{
					for (int i = 1; i < bestTravelPath.Count; i++)
					{
						int systemA = bestTravelPath[i - 1];
						int systemB = bestTravelPath[i];
						if (game.GameDatabase.GetNodeLineBetweenSystems(fleetInfo.PlayerID, systemA, systemB, true, false) == null)
						{
							return null;
						}
					}
				}
				return new int?(value);
			}
			return null;
		}
		public static MissionEstimate GetMissionEstimate(GameSession sim, MissionType type, StationType stationType, int fleetID, int systemID, int planetID, List<int> designsToBuild = null, int stationLevel = 1, bool ReBase = false, float? travelSpeed = null, float? nodeTravelSpeed = null)
		{
			FleetInfo fleetInfo = sim.GameDatabase.GetFleetInfo(fleetID);
			List<ShipInfo> list = sim.GameDatabase.GetShipInfoByFleetID(fleetID, true).ToList<ShipInfo>();
			int turnsForConstruction = 0;
			double num = 0.0;
			if (designsToBuild != null && designsToBuild.Count<int>() > 0)
			{
				int num2 = 0;
				foreach (int current in designsToBuild)
				{
					DesignInfo designInfo = sim.GameDatabase.GetDesignInfo(current);
					num2 += designInfo.GetPlayerProductionCost(sim.GameDatabase, sim.LocalPlayer.ID, !designInfo.isPrototyped, null);
					num += (double)designInfo.SavingsCost;
					list.Add(new ShipInfo
					{
						DesignID = designInfo.ID,
						DesignInfo = designInfo,
						FleetID = fleetInfo.ID,
						SerialNumber = 0,
						ShipName = string.Empty
					});
				}
				int num3 = StarFleet.PredictProductionOutputForSystem(sim, fleetInfo.SupportingSystemID, fleetInfo.PlayerID);
				if (num3 > 0)
				{
					turnsForConstruction = (num2 + (num3 - 1)) / num3;
				}
				else
				{
					turnsForConstruction = 0;
				}
			}
			int num4 = 0;
			int? travelTime = StarFleet.GetTravelTime(sim, fleetInfo, systemID, false, travelSpeed, nodeTravelSpeed);
			if (travelTime.HasValue)
			{
				num4 = travelTime.Value;
			}
			Faction faction = sim.AssetDatabase.GetFaction(sim.GameDatabase.GetPlayerFactionID(fleetInfo.PlayerID));
			int turnsAtTarget = 0;
			int num5 = 0;
			int num6 = 0;
			if (type == MissionType.COLONIZATION && planetID != 0)
			{
				double colonizationSpace = StarFleet.GetColonizationSpace(sim, list, faction.Name);
				double terraformingSpace = StarFleet.GetTerraformingSpace(sim, list);
				int numColonizationShips = StarFleet.GetNumColonizationShips(sim, list);
				if (colonizationSpace > 0.0 && numColonizationShips > 0)
				{
					ColonyInfo colonyInfo = new ColonyInfo();
					colonyInfo.OrbitalObjectID = planetID;
					colonyInfo.ImperialPop = colonizationSpace;
					colonyInfo.CivilianWeight = 0f;
					colonyInfo.PlayerID = fleetInfo.PlayerID;
					colonyInfo.InfraRate = 0.5f;
					colonyInfo.TerraRate = 0.5f;
					colonyInfo.DamagedLastTurn = false;
					PlanetInfo planetInfo = sim.GameDatabase.GetPlanetInfo(colonyInfo.OrbitalObjectID);
					planetInfo.Infrastructure = (float)(colonizationSpace * 0.0001);
					num5 = Colony.SupportTripsTillSelfSufficient(sim, colonyInfo, planetInfo, colonizationSpace, terraformingSpace, fleetInfo);
					num6 = Colony.PredictTurnsToPhase1Completion(sim, colonyInfo, planetInfo);
					if (num6 > -1)
					{
						num6 += (num5 + 1) * 2 * num4;
					}
				}
			}
			else
			{
				if (type == MissionType.EVACUATE && planetID != 0)
				{
					int numColonizationShips2 = StarFleet.GetNumColonizationShips(sim, list);
					double civilianPopulation = sim.GameDatabase.GetCivilianPopulation(planetID, 0, false);
					if (numColonizationShips2 > 0)
					{
						double num7 = (double)numColonizationShips2 * (double)sim.AssetDatabase.EvacCivPerCol;
						num5 = (int)Math.Ceiling(civilianPopulation / num7);
						num6 += (num5 + num4) * 2;
					}
				}
				else
				{
					if (type == MissionType.SURVEY && systemID != 0)
					{
						turnsAtTarget = StarFleet.GetTurnsToSurveySystem(sim.GameDatabase, systemID, list);
					}
					else
					{
						if (type == MissionType.GATE && systemID != 0)
						{
							turnsAtTarget = 1;
						}
						else
						{
							if ((type == MissionType.PATROL || type == MissionType.INVASION || type == MissionType.INTERDICTION) && systemID != 0)
							{
								if (sim.IsSystemInSupplyRange(systemID, fleetInfo.PlayerID))
								{
									turnsAtTarget = StarFleet.GetFleetEndurance(sim, fleetInfo.ID) * 2;
								}
								else
								{
									turnsAtTarget = (int)(fleetInfo.SupplyRemaining / StarFleet.GetSupplyConsumption(sim, fleetInfo.ID));
								}
							}
							else
							{
								if (type == MissionType.CONSTRUCT_STN)
								{
									int num8 = (int)Math.Ceiling((double)StarFleet.GetConstructionPointsForFleet(sim, list));
									if (num8 > 0)
									{
										string factionName = sim.GameDatabase.GetFactionName(sim.GameDatabase.GetFleetFaction(fleetID));
										int stationConstructionCost = StarFleet.GetStationConstructionCost(sim, stationType, factionName, stationLevel);
										turnsAtTarget = (stationConstructionCost + (num8 - 1)) / num8;
										num += (double)StarFleet.GetStationSavingsCost(sim, stationType, factionName, stationLevel);
									}
									else
									{
										turnsAtTarget = 0;
									}
								}
							}
						}
					}
				}
			}
			return new MissionEstimate
			{
				TurnsToTarget = num4,
				TurnsToReturn = (type == MissionType.RELOCATION || ReBase) ? 0 : (faction.CanUseGate() ? 1 : num4),
				TurnsAtTarget = turnsAtTarget,
				TurnsForConstruction = turnsForConstruction,
				ConstructionCost = (float)num,
				TripsTillSelfSufficeincy = num5,
				TurnsTillPhase1Completion = num6
			};
		}
		public static double GetFleetSlaves(GameSession game, int fleetID)
		{
			List<ShipInfo> source = game.GameDatabase.GetShipInfoByFleetID(fleetID, false).ToList<ShipInfo>();
			return source.Sum((ShipInfo x) => x.SlavesObtained);
		}
		public static float GetFleetTravelSpeed(GameSession game, int fleetID, IEnumerable<ShipInfo> ships, bool nodeTravel)
		{
			float num = 0f;
			float num2 = 0f;
			FleetInfo fleetInfo = game.GameDatabase.GetFleetInfo(fleetID);
			Faction faction = game.GetPlayerObject(fleetInfo.PlayerID).Faction;
			bool flag = false;
			float num3 = 1f;
			float num4 = 1f;
			foreach (ShipInfo current in ships)
			{
				if (current.ParentID == 0)
				{
					DesignSectionInfo[] designSections = current.DesignInfo.DesignSections;
					for (int i = 0; i < designSections.Length; i++)
					{
						DesignSectionInfo designSectionInfo = designSections[i];
						ShipSectionAsset shipSectionAsset = designSectionInfo.ShipSectionAsset;
						if (shipSectionAsset != null)
						{
							num3 = Math.Max(shipSectionAsset.FleetSpeedModifier, num3);
							num4 = Math.Min(shipSectionAsset.FleetSpeedModifier, num4);
							if (!flag && faction.CanUseNodeLine(new bool?(false)) && shipSectionAsset.FileName.Contains("bore"))
							{
								flag = true;
							}
							if (shipSectionAsset.NodeSpeed > 0f && (num2 <= 0f || num2 > shipSectionAsset.NodeSpeed))
							{
								num2 = shipSectionAsset.NodeSpeed;
							}
							if (game.App.GetStratModifier<bool>(StratModifiers.UseFastestShipForFTLSpeed, fleetInfo.PlayerID))
							{
								num = Math.Max(num, shipSectionAsset.FtlSpeed);
							}
							else
							{
								if (shipSectionAsset.FtlSpeed > 0f && (num <= 0f || num > shipSectionAsset.FtlSpeed))
								{
									num = shipSectionAsset.FtlSpeed;
								}
							}
						}
					}
				}
			}
			float num5;
			if (nodeTravel)
			{
				num5 = num2;
			}
			else
			{
				if (flag)
				{
					num5 = num2 * game.GameDatabase.GetStratModifier<float>(StratModifiers.BoreSpeedModifier, fleetInfo.PlayerID);
				}
				else
				{
					num5 = num;
				}
			}
			if (faction.CanUseFlockBonus())
			{
				num5 += num5 * StarFleet.GetMorrigiFlockBonus(game, fleetID);
			}
			if (faction.CanUseAccelerators())
			{
				if (fleetInfo.LastTurnAccelerated != game.GameDatabase.GetTurnCount())
				{
					bool flag2;
					if (fleetInfo.SystemID != 0)
					{
						flag2 = game.GameDatabase.GetFleetsByPlayerAndSystem(fleetInfo.PlayerID, fleetInfo.SystemID, FleetType.FL_ACCELERATOR).Any<FleetInfo>();
					}
					else
					{
						flag2 = game.GameDatabase.IsInAccelRange(fleetInfo.ID);
					}
					if (flag2)
					{
						game.GameDatabase.UpdateFleetAccelerated(game, fleetInfo.ID, null);
						fleetInfo = game.GameDatabase.GetFleetInfo(fleetInfo.ID);
					}
				}
				int num6 = Math.Max(game.GameDatabase.GetTurnCount() - fleetInfo.LastTurnAccelerated - 1, 0);
				if (StarFleet.GetFleetLoaCubeValue(game, fleetInfo.ID) > StarFleet.GetMaxLoaFleetCubeMassForTransit(game, fleetInfo.PlayerID))
				{
					num6 = 100;
				}
				num5 = Math.Max(num, StarFleet.LoaPlayerFleetSpeed(game, fleetInfo.PlayerID) - (float)num6);
			}
			return num5 + num5 * (num3 - 1f) + num5 * (num4 - 1f);
		}
		public static float GetFleetTravelSpeed(GameSession game, int fleetID, bool nodeTravel)
		{
			return StarFleet.GetFleetTravelSpeed(game, fleetID, game.GameDatabase.GetShipInfoByFleetID(fleetID, true), nodeTravel);
		}
		public static float LoaPlayerFleetSpeed(GameSession game, int playerid)
		{
			float result = 4f;
			if (game.GameDatabase.GetStratModifier<bool>(StratModifiers.StandingNeutrinoWaves, playerid))
			{
				result = 6f;
			}
			return result;
		}
		public static int PredictProductionOutputForSystem(GameSession sim, int systemID, int playerID)
		{
			int num = 0;
			foreach (int current in sim.GameDatabase.GetStarSystemPlanets(systemID))
			{
				PlanetInfo planetInfo = sim.GameDatabase.GetPlanetInfo(current);
				ColonyInfo colonyInfoForPlanet = sim.GameDatabase.GetColonyInfoForPlanet(planetInfo.ID);
				if (colonyInfoForPlanet != null && colonyInfoForPlanet.PlayerID == playerID)
				{
					num += (int)Colony.GetShipConstResources(sim, colonyInfoForPlanet, planetInfo);
				}
			}
			return num;
		}
		private static float GetMorrigiFlockBonus(GameSession game, int fleetID)
		{
			FleetInfo fleetInfo = game.GameDatabase.GetFleetInfo(fleetID);
			float num = 0f;
			float num2 = 0f;
			float num3 = 0f;
			float num4 = 0f;
			float stratModifier = game.GameDatabase.GetStratModifier<float>(StratModifiers.MaxFlockBonusMod, fleetInfo.PlayerID);
			int num5 = (int)((float)game.AssetDatabase.FlockBRCountBonus * stratModifier);
			int num6 = (int)((float)game.AssetDatabase.FlockCRCountBonus * stratModifier);
			int num7 = (int)((float)game.AssetDatabase.FlockDNCountBonus * stratModifier);
			int num8 = (int)((float)game.AssetDatabase.FlockLVCountBonus * stratModifier);
			float flockMaxBonus = game.AssetDatabase.FlockMaxBonus;
			foreach (ShipInfo current in game.GameDatabase.GetShipInfoByFleetID(fleetID, false))
			{
				DesignInfo designInfo = current.DesignInfo;
				switch (designInfo.Class)
				{
				case ShipClass.Cruiser:
					if (num6 > 0)
					{
						num2 += game.AssetDatabase.FlockCRBonus;
						num6--;
					}
					break;
				case ShipClass.Dreadnought:
					if (num7 > 0)
					{
						num3 += game.AssetDatabase.FlockDNBonus;
						num7--;
					}
					break;
				case ShipClass.Leviathan:
					if (num8 > 0)
					{
						num4 += game.AssetDatabase.FlockLVBonus;
						num8--;
					}
					break;
				case ShipClass.BattleRider:
					if (num5 > 0)
					{
						num += game.AssetDatabase.FlockBRBonus;
						num5--;
					}
					break;
				}
			}
			num = Math.Min(flockMaxBonus, num);
			num2 = Math.Min(flockMaxBonus, num2);
			num3 = Math.Min(flockMaxBonus, num3);
			num4 = Math.Min(flockMaxBonus, num4);
			return num + num2 + num3 + num4;
		}
		public static int GetDesignForColonizer(GameSession game, int playerID)
		{
			int result = 0;
			int num = 0;
			foreach (DesignInfo current in game.GameDatabase.GetDesignInfosForPlayer(playerID))
			{
				foreach (string sectionName in game.GameDatabase.GetDesignSectionNames(current.ID))
				{
					ShipSectionAsset shipSectionAsset = game.AssetDatabase.ShipSections.First((ShipSectionAsset x) => x.FileName == sectionName);
					if (shipSectionAsset != null && shipSectionAsset.ColonizationSpace > num)
					{
						result = current.ID;
						num = shipSectionAsset.ColonizationSpace;
					}
				}
			}
			return result;
		}
		public static double GetTerraformingSpace(GameSession sim, IEnumerable<ShipInfo> ships)
		{
			double num = 0.0;
			foreach (ShipInfo current in ships)
			{
				foreach (string sectionName in sim.GameDatabase.GetDesignSectionNames(current.DesignID))
				{
					ShipSectionAsset shipSectionAsset = sim.AssetDatabase.ShipSections.First((ShipSectionAsset x) => x.FileName == sectionName);
					if (shipSectionAsset != null && shipSectionAsset.TerraformingSpace > 0)
					{
						num += (double)shipSectionAsset.TerraformingSpace;
					}
				}
			}
			return num;
		}
		public static double GetColonizationSpace(GameSession sim, IEnumerable<ShipInfo> ships, string FactionName)
		{
			double num = 0.0;
			bool flag = FactionName == "loa";
			if (flag)
			{
				if (ships.Any<ShipInfo>())
				{
					PlayerInfo playerInfo = sim.GameDatabase.GetPlayerInfo(sim.GameDatabase.GetFleetInfo(ships.First<ShipInfo>().FleetID).PlayerID);
					DesignInfo designInfo = sim.GameDatabase.GetDesignInfosForPlayer(playerInfo.ID).FirstOrDefault((DesignInfo x) => x.DesignSections.Any((DesignSectionInfo j) => j.ShipSectionAsset.ColonizationSpace > 0));
					if (designInfo != null)
					{
						DesignSectionInfo[] designSections = designInfo.DesignSections;
						for (int i = 0; i < designSections.Length; i++)
						{
							DesignSectionInfo designSectionInfo = designSections[i];
							num += (double)designSectionInfo.ShipSectionAsset.ColonizationSpace;
						}
					}
				}
			}
			else
			{
				foreach (ShipInfo current in ships)
				{
					foreach (string sectionName in sim.GameDatabase.GetDesignSectionNames(current.DesignID))
					{
						ShipSectionAsset shipSectionAsset = sim.AssetDatabase.ShipSections.First((ShipSectionAsset x) => x.FileName == sectionName);
						if (shipSectionAsset != null && shipSectionAsset.ColonizationSpace > 0)
						{
							num += (double)shipSectionAsset.ColonizationSpace;
						}
					}
				}
			}
			return num;
		}
		public static double GetTerraformingSpace(GameSession game, int fleetID)
		{
			FleetInfo fleetInfo = game.GameDatabase.GetFleetInfo(fleetID);
			float num = 1f;
			List<AdmiralInfo.TraitType> list = game.GameDatabase.GetAdmiralTraits(fleetInfo.AdmiralID).ToList<AdmiralInfo.TraitType>();
			if (list.Contains(AdmiralInfo.TraitType.GreenThumb))
			{
				num += 0.1f;
			}
			if (list.Contains(AdmiralInfo.TraitType.BlackThumb))
			{
				num -= 0.1f;
			}
			return StarFleet.GetTerraformingSpace(game, game.GameDatabase.GetShipInfoByFleetID(fleetID, false)) * (double)num;
		}
		public static double GetColonizationSpace(GameSession game, int fleetID)
		{
			FleetInfo fleetInfo = game.GameDatabase.GetFleetInfo(fleetID);
			float num = 1f;
			List<AdmiralInfo.TraitType> list = game.GameDatabase.GetAdmiralTraits(fleetInfo.AdmiralID).ToList<AdmiralInfo.TraitType>();
			if (list.Contains(AdmiralInfo.TraitType.GoodShepherd))
			{
				num += 0.1f;
			}
			if (list.Contains(AdmiralInfo.TraitType.BadShepherd))
			{
				num -= 0.1f;
			}
			return StarFleet.GetColonizationSpace(game, game.GameDatabase.GetShipInfoByFleetID(fleetID, false), game.AssetDatabase.GetFaction(game.GameDatabase.GetPlayerInfo(fleetInfo.PlayerID).FactionID).Name) * (double)num;
		}
		public static bool CanDesignColonize(GameSession game, int designID)
		{
			foreach (string sectionName in game.GameDatabase.GetDesignSectionNames(designID))
			{
				ShipSectionAsset shipSectionAsset = game.AssetDatabase.ShipSections.First((ShipSectionAsset x) => x.FileName == sectionName);
				if (shipSectionAsset != null && shipSectionAsset.ColonizationSpace > 0)
				{
					return true;
				}
			}
			return false;
		}
		public static bool CanDesignConstruct(GameSession game, int designID)
		{
			foreach (string sectionName in game.GameDatabase.GetDesignSectionNames(designID))
			{
				ShipSectionAsset shipSectionAsset = game.AssetDatabase.ShipSections.First((ShipSectionAsset x) => x.FileName == sectionName);
				if (shipSectionAsset != null && shipSectionAsset.isConstructor)
				{
					return true;
				}
			}
			return false;
		}
		public static int GetNumColonizationShips(GameSession game, IEnumerable<ShipInfo> ships)
		{
			int num = 0;
			foreach (ShipInfo current in ships)
			{
				foreach (string sectionName in game.GameDatabase.GetDesignSectionNames(current.DesignID))
				{
					ShipSectionAsset shipSectionAsset = game.AssetDatabase.ShipSections.First((ShipSectionAsset x) => x.FileName == sectionName);
					if (shipSectionAsset != null && shipSectionAsset.ColonizationSpace > 0)
					{
						num++;
					}
				}
			}
			return num;
		}
		public static ShipInfo GetFirstConstructionShip(GameSession game, FleetInfo fleet)
		{
			IEnumerable<ShipInfo> shipInfoByFleetID = game.GameDatabase.GetShipInfoByFleetID(fleet.ID, false);
			foreach (ShipInfo current in shipInfoByFleetID)
			{
				foreach (string sectionName in game.GameDatabase.GetDesignSectionNames(current.DesignID))
				{
					ShipSectionAsset shipSectionAsset = game.AssetDatabase.ShipSections.First((ShipSectionAsset x) => x.FileName == sectionName);
					if (shipSectionAsset != null && shipSectionAsset.ColonizationSpace > 0)
					{
						return current;
					}
				}
			}
			return null;
		}
		public static int GetNumColonizationShips(GameSession game, int fleetID)
		{
			return StarFleet.GetNumColonizationShips(game, game.GameDatabase.GetShipInfoByFleetID(fleetID, false));
		}
		public static int GetNumConstructionShips(GameSession game, int fleetID)
		{
			int num = 0;
			foreach (ShipInfo current in game.GameDatabase.GetShipInfoByFleetID(fleetID, false))
			{
				foreach (string sectionName in game.GameDatabase.GetDesignSectionNames(current.DesignID))
				{
					ShipSectionAsset shipSectionAsset = game.AssetDatabase.ShipSections.First((ShipSectionAsset x) => x.FileName == sectionName);
					if (shipSectionAsset != null && shipSectionAsset.isConstructor)
					{
						num++;
					}
				}
			}
			return num;
		}
		public static int ProjectNumColonizationRuns(GameSession sim, int planetID, int fleetID, List<int> colonizersToBuild, int turnTimeout)
		{
			return 1;
		}
		public static int GetTerraformingSpaceForDesign(GameSession game, int designID)
		{
			DesignInfo designInfo = game.GameDatabase.GetDesignInfo(designID);
			int num = 0;
			foreach (string sectionName in game.GameDatabase.GetDesignSectionNames(designInfo.ID))
			{
				ShipSectionAsset shipSectionAsset = game.AssetDatabase.ShipSections.First((ShipSectionAsset x) => x.FileName == sectionName);
				if (shipSectionAsset != null)
				{
					num += shipSectionAsset.TerraformingSpace;
				}
			}
			return num;
		}
		public static int GetColonizationSpaceForDesign(GameSession game, int designID)
		{
			DesignInfo designInfo = game.GameDatabase.GetDesignInfo(designID);
			int num = 0;
			foreach (string sectionName in game.GameDatabase.GetDesignSectionNames(designInfo.ID))
			{
				ShipSectionAsset shipSectionAsset = game.AssetDatabase.ShipSections.First((ShipSectionAsset x) => x.FileName == sectionName);
				if (shipSectionAsset != null)
				{
					num += shipSectionAsset.ColonizationSpace;
				}
			}
			return num;
		}
		public static float GetSupplyCapacity(GameDatabase game, int fleetID)
		{
			float num = 0f;
			foreach (ShipInfo current in game.GetShipInfoByFleetID(fleetID, false))
			{
				foreach (string sectionName in game.GetDesignSectionNames(current.DesignID))
				{
					ShipSectionAsset shipSectionAsset = game.AssetDatabase.ShipSections.First((ShipSectionAsset x) => x.FileName == sectionName);
					if (shipSectionAsset != null && (float)shipSectionAsset.Supply > 0f)
					{
						num += (float)shipSectionAsset.Supply;
					}
				}
			}
			return num;
		}
		public static float GetSupplyConsumption(GameSession game, int fleetID)
		{
			int num = 0;
			foreach (ShipInfo current in game.GameDatabase.GetShipInfoByFleetID(fleetID, true))
			{
				num = current.DesignInfo.SupplyRequired;
			}
			return (float)num;
		}
		public static float GetConstructionPointsForFleet(GameSession game, IEnumerable<ShipInfo> ships)
		{
			if (GameSession.InstaBuildHackEnabled)
			{
				return 1E+09f;
			}
			float num = 0f;
			foreach (ShipInfo current in ships)
			{
				foreach (string sectionName in game.GameDatabase.GetDesignSectionNames(current.DesignID))
				{
					ShipSectionAsset shipSectionAsset = game.AssetDatabase.ShipSections.First((ShipSectionAsset x) => x.FileName == sectionName);
					if (shipSectionAsset != null && shipSectionAsset.isConstructor)
					{
						num += (float)shipSectionAsset.ConstructionPoints;
					}
				}
			}
			return num;
		}
		public static float GetConstructionPointsForFleet(GameSession game, int fleetID)
		{
			FleetInfo fleetInfo = game.GameDatabase.GetFleetInfo(fleetID);
			float num = 1f;
			List<AdmiralInfo.TraitType> list = game.GameDatabase.GetAdmiralTraits(fleetInfo.AdmiralID).ToList<AdmiralInfo.TraitType>();
			if (list.Contains(AdmiralInfo.TraitType.Architect))
			{
				num += 0.1f;
			}
			return StarFleet.GetConstructionPointsForFleet(game, game.GameDatabase.GetShipInfoByFleetID(fleetID, false)) * num;
		}
		public static bool CanDoSurveyMissionToTarget(GameSession game, int systemID, int fleetID, float? fleetRange = null, float? travelSpeed = null, float? nodeTravelSpeed = null)
		{
			MissionInfo missionByFleetID = game.GameDatabase.GetMissionByFleetID(fleetID);
			if (missionByFleetID != null)
			{
				MissionType type = missionByFleetID.Type;
				if (type != MissionType.PATROL && type != MissionType.RETURN)
				{
					return false;
				}
			}
			FleetInfo fleetInfo = game.GameDatabase.GetFleetInfo(fleetID);
			return !fleetInfo.IsReserveFleet && !StarFleet.IsGardenerFleet(game, fleetInfo) && !StarFleet.IsFleetExhausted(game, fleetInfo) && StarFleet.IsFleetInRange(game, fleetID, systemID, fleetRange, travelSpeed, nodeTravelSpeed);
		}
		public static bool HasRelocatableDefResAssetsInRange(GameSession game, int systemID)
		{
			if (!game.GameDatabase.IsSurveyed(game.LocalPlayer.ID, systemID) || (game.GameDatabase.GetNavalStationForSystemAndPlayer(systemID, game.LocalPlayer.ID) == null && !game.GameDatabase.GetColonyInfosForSystem(systemID).Any((ColonyInfo x) => x.PlayerID == game.LocalPlayer.ID)))
			{
				return false;
			}
			List<StarSystemInfo> source = game.GameDatabase.GetVisibleStarSystemInfos(game.LocalPlayer.ID).ToList<StarSystemInfo>();
			source = (
				from x in source
				where (
					from j in game.GameDatabase.GetFleetInfoBySystemID(x.ID, FleetType.FL_RESERVE | FleetType.FL_DEFENSE)
					where j.PlayerID == game.LocalPlayer.ID && game.GameDatabase.GetShipsByFleetID(j.ID).Any<int>()
					select j).Any<FleetInfo>()
				select x).ToList<StarSystemInfo>();
			List<MissionInfo> source2 = (
				from x in game.GameDatabase.GetMissionsBySystemDest(systemID)
				where x.Type == MissionType.INTERDICTION
				select x).ToList<MissionInfo>();
			return source.Any<StarSystemInfo>() && !source2.Any<MissionInfo>();
		}
		public static bool CanDoRelocationMissionToTarget(GameSession game, int systemId, int fleetID)
		{
			FleetInfo fleetInfo = game.GameDatabase.GetFleetInfo(fleetID);
			if (StarFleet.IsSuulkaFleet(game.GameDatabase, fleetInfo))
			{
				return true;
			}
			if (StarFleet.IsGardenerFleet(game, fleetInfo))
			{
				return true;
			}
			if (!StarFleet.CanSystemSupportFleet(game, systemId, fleetID))
			{
				return false;
			}
			IEnumerable<ColonyInfo> colonyInfosForSystem = game.GameDatabase.GetColonyInfosForSystem(systemId);
			StationInfo navalStationForSystemAndPlayer = game.GameDatabase.GetNavalStationForSystemAndPlayer(systemId, fleetInfo.PlayerID);
			if (colonyInfosForSystem == null && navalStationForSystemAndPlayer == null)
			{
				return false;
			}
			bool flag = (
				from x in colonyInfosForSystem
				where x.PlayerID == fleetInfo.PlayerID
				select x).Any<ColonyInfo>();
			bool flag2 = navalStationForSystemAndPlayer != null;
			if (fleetInfo.SupportingSystemID == systemId || (!flag && !flag2))
			{
				return false;
			}
			MissionInfo missionByFleetID = game.GameDatabase.GetMissionByFleetID(fleetInfo.ID);
			return missionByFleetID == null && !fleetInfo.IsReserveFleet;
		}
		public static bool CanSystemSupportFleet(GameSession sim, int systemId, int fleetId)
		{
			FleetInfo fleetInfo = sim.GameDatabase.GetFleetInfo(fleetId);
			int remainingSupportPoints = sim.GameDatabase.GetRemainingSupportPoints(sim, systemId, fleetInfo.PlayerID);
			int fleetCruiserEquivalent = sim.GameDatabase.GetFleetCruiserEquivalent(fleetId);
			App.Log.Trace(string.Concat(new object[]
			{
				"Station Support Check, Support Points: ",
				remainingSupportPoints,
				" Required Points: ",
				fleetCruiserEquivalent
			}), "game", LogLevel.Verbose);
			return remainingSupportPoints >= fleetCruiserEquivalent;
		}
		public static bool CanDoEvacuationMissionToTarget(GameSession game, int systemID, int fleetID, int fleetSupportingSystimID, float? fleetRange = null, float? travelSpeed = null, float? nodeTravelSpeed = null)
		{
			return systemID != fleetSupportingSystimID && StarFleet.GetNumColonizationShips(game, fleetID) > 0 && StarFleet.CanDoSurveyMissionToTarget(game, systemID, fleetID, fleetRange, travelSpeed, nodeTravelSpeed);
		}
		public static bool CanDoColonizeMissionToTarget(GameSession game, int systemID, int fleetID, float? fleetRange = null, float? travelSpeed = null, float? nodeTravelSpeed = null)
		{
			return StarFleet.CanDoSurveyMissionToTarget(game, systemID, fleetID, fleetRange, travelSpeed, nodeTravelSpeed);
		}
		public static bool CanDoTransferMissionToTarget(GameSession game, int systemID, int fleetID)
		{
			if (game.GameDatabase.GetMissionByFleetID(fleetID) != null)
			{
				return false;
			}
			FleetInfo fleetInfo = game.GameDatabase.GetFleetInfo(fleetID);
			if (fleetInfo.IsReserveFleet)
			{
				return false;
			}
			if (fleetInfo == null || fleetInfo.SupportingSystemID == systemID)
			{
				return false;
			}
			string name = game.GameDatabase.GetFactionInfo(game.GameDatabase.GetFleetFaction(fleetID)).Name;
			if (name == "human")
			{
				foreach (int current in StarFleet.GetNodeTravelPath(game.GameDatabase, fleetInfo.SystemID, fleetInfo.SupportingSystemID, fleetInfo.PlayerID, true, false, false))
				{
					if (current == 0)
					{
						return false;
					}
					StarSystemInfo starSystemInfo = game.GameDatabase.GetStarSystemInfo(current);
					App.Log.Trace("Human node path: " + starSystemInfo.Name, "game");
				}
				return true;
			}
			return true;
		}
		public static bool CanDoPatrolMissionToTarget(GameSession game, int systemID, int fleetID, float? fleetRange = null, float? travelSpeed = null, float? nodeTravelSpeed = null)
		{
			return StarFleet.CanDoSurveyMissionToTarget(game, systemID, fleetID, fleetRange, travelSpeed, nodeTravelSpeed);
		}
		public static bool CanDoDeployNPGToTarget(GameSession game, int systemID, int fleetID, float? fleetRange = null, float? travelSpeed = null, float? nodeTravelSpeed = null)
		{
			FleetInfo fleetInfo = game.GameDatabase.GetFleetInfo(fleetID);
			if (!StarFleet.CanDoSurveyMissionToTarget(game, systemID, fleetID, fleetRange, travelSpeed, nodeTravelSpeed))
			{
				return false;
			}
			if (fleetInfo.SystemID == 0)
			{
				return false;
			}
			if (!game.AssetDatabase.GetFaction(game.GameDatabase.GetPlayerInfo(fleetInfo.PlayerID).FactionID).CanUseAccelerators())
			{
				return false;
			}
			DesignInfo designInfo = game.GameDatabase.GetDesignInfosForPlayer(fleetInfo.PlayerID).FirstOrDefault((DesignInfo x) => x.IsAccelerator());
			int productionCost = designInfo.DesignSections.First<DesignSectionInfo>().ShipSectionAsset.ProductionCost;
			int fleetLoaCubeValue = StarFleet.GetFleetLoaCubeValue(game, fleetID);
			return fleetLoaCubeValue >= productionCost;
		}
		public static bool CanDoGateMissionToTarget(GameSession sim, int systemId, int fleetId, float? fleetRange = null, float? travelSpeed = null, float? nodeTravelSpeed = null)
		{
			bool flag = false;
			List<ShipInfo> list = sim.GameDatabase.GetShipInfoByFleetID(fleetId, true).ToList<ShipInfo>();
			if (list == null)
			{
				return false;
			}
			foreach (ShipInfo current in list)
			{
				if (current.DesignInfo.Role == ShipRole.GATE)
				{
					flag = true;
					break;
				}
			}
			return flag && StarFleet.CanDoSurveyMissionToTarget(sim, systemId, fleetId, fleetRange, travelSpeed, nodeTravelSpeed);
		}
		public static bool CanDoInterdictionMissionToTarget(GameSession game, int systemID, int fleetID, float? fleetRange = null, float? travelSpeed = null, float? nodeTravelSpeed = null)
		{
			return StarFleet.CanDoSurveyMissionToTarget(game, systemID, fleetID, fleetRange, travelSpeed, nodeTravelSpeed);
		}
		public static bool CanDoStrikeMissionToTarget(GameSession game, int systemID, int fleetID, float? fleetRange = null, float? travelSpeed = null, float? nodeTravelSpeed = null)
		{
			return StarFleet.CanDoSurveyMissionToTarget(game, systemID, fleetID, fleetRange, travelSpeed, nodeTravelSpeed);
		}
		public static bool CanDoInvasionMissionToTarget(GameSession game, int systemID, int fleetID, float? fleetRange = null, float? travelSpeed = null, float? nodeTravelSpeed = null)
		{
			return StarFleet.CanDoSurveyMissionToTarget(game, systemID, fleetID, fleetRange, travelSpeed, nodeTravelSpeed);
		}
		public static bool CanDoSupportMissionToTarget(GameSession game, int systemID, int fleetID, float? fleetRange = null, float? travelSpeed = null, float? nodeTravelSpeed = null)
		{
			return StarFleet.CanDoColonizeMissionToTarget(game, systemID, fleetID, fleetRange, travelSpeed, nodeTravelSpeed);
		}
		public static bool CanDoPiracyMissionToTarget(GameSession game, int systemID, int fleetID, float? fleetRange = null, float? travelSpeed = null, float? nodeTravelSpeed = null)
		{
			if (game.GameDatabase.GetMissionByFleetID(fleetID) != null)
			{
				return false;
			}
			FleetInfo fleetInfo = game.GameDatabase.GetFleetInfo(fleetID);
			if (fleetInfo.IsReserveFleet || StarFleet.IsGardenerFleet(game, fleetInfo))
			{
				return false;
			}
			List<ShipInfo> source = game.GameDatabase.GetShipInfoByFleetID(fleetID, false).ToList<ShipInfo>();
			return !source.Any((ShipInfo x) => x.DesignInfo.GetRealShipClass() == RealShipClasses.Dreadnought || x.DesignInfo.GetRealShipClass() == RealShipClasses.Leviathan) && StarFleet.IsFleetInRange(game, fleetID, systemID, fleetRange, travelSpeed, nodeTravelSpeed);
		}
		public static bool CanDoConstructionMissionToTarget(GameSession game, int systemID, int fleetID, bool forUI)
		{
			if (game.GameDatabase.GetMissionByFleetID(fleetID) != null)
			{
				return false;
			}
			FleetInfo fleetInfo = game.GameDatabase.GetFleetInfo(fleetID);
			if (fleetInfo.IsReserveFleet)
			{
				return false;
			}
			string name = game.GameDatabase.GetFactionInfo(game.GameDatabase.GetFleetFaction(fleetID)).Name;
			if (name == "human")
			{
				foreach (int current in StarFleet.GetNodeTravelPath(game.GameDatabase, fleetInfo.SystemID, systemID, fleetInfo.PlayerID, true, false, false))
				{
					if (current == 0)
					{
						return false;
					}
					StarSystemInfo starSystemInfo = game.GameDatabase.GetStarSystemInfo(current);
					App.Log.Trace("Human node path: " + starSystemInfo.Name, "game");
				}
			}
			if (!forUI)
			{
				float constructionPointsForFleet = StarFleet.GetConstructionPointsForFleet(game, fleetID);
				if (constructionPointsForFleet < 1f)
				{
					return false;
				}
			}
			return true;
		}
		public static bool IsFleetWaitingForBuildOrders(App game, int missionID, int fleetID)
		{
			FleetInfo fleetInfo = game.GameDatabase.GetFleetInfo(fleetID);
			foreach (BuildOrderInfo current in game.GameDatabase.GetBuildOrdersForSystem(fleetInfo.SystemID))
			{
				if (current.MissionID == missionID)
				{
					return true;
				}
			}
			return false;
		}
		public static List<int> GetMissionCapableShips(GameSession game, int fleetID, MissionType missionType)
		{
			List<int> list = new List<int>();
			switch (missionType)
			{
			case MissionType.COLONIZATION:
			case MissionType.SUPPORT:
				break;
			case MissionType.SURVEY:
			case MissionType.RELOCATION:
				goto IL_CC;
			case MissionType.CONSTRUCT_STN:
				using (IEnumerator<ShipInfo> enumerator = game.GameDatabase.GetShipInfoByFleetID(fleetID, false).GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						ShipInfo current = enumerator.Current;
						if (StarFleet.CanDesignConstruct(game, current.DesignID))
						{
							list.Add(current.ID);
						}
					}
					return list;
				}
				break;
			default:
				if (missionType != MissionType.EVACUATE)
				{
					goto IL_CC;
				}
				break;
			}
			using (IEnumerator<ShipInfo> enumerator2 = game.GameDatabase.GetShipInfoByFleetID(fleetID, false).GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					ShipInfo current2 = enumerator2.Current;
					if (StarFleet.CanDesignColonize(game, current2.DesignID))
					{
						list.Add(current2.ID);
					}
				}
				return list;
			}
			IL_CC:
			list.AddRange(game.GameDatabase.GetShipsByFleetID(fleetID));
			return list;
		}
		public static string GetShipClassAbbr(ShipClass shipClass)
		{
			switch (shipClass)
			{
			case ShipClass.Cruiser:
				return "CR";
			case ShipClass.Dreadnought:
				return "DN";
			case ShipClass.Leviathan:
				return "LV";
			case ShipClass.BattleRider:
				return "BR";
			case ShipClass.Station:
				return "SN";
			default:
				throw new ArgumentOutOfRangeException("shipClass");
			}
		}
		public static StationTypeFlags MissionStringToStationType(string missionType)
		{
			return (StationTypeFlags)Enum.Parse(typeof(StationTypeFlags), missionType.Split(new char[]
			{
				' '
			})[0]);
		}
		public static void CheckSystemCanSupportResidentFleets(App App, int systemId, int playerId)
		{
			int num = App.GameDatabase.GetRemainingSupportPoints(App.Game, systemId, playerId);
			bool flag = (
				from x in App.GameDatabase.GetColonyInfosForSystem(systemId)
				where x.PlayerID == playerId
				select x).Any<ColonyInfo>();
			if (num < 0)
			{
				List<FleetInfo> list = (
					from x in App.GameDatabase.GetFleetInfoBySystemID(systemId, FleetType.FL_NORMAL)
					where App.GameDatabase.GetMissionByFleetID(x.ID) == null
					select x).ToList<FleetInfo>();
				foreach (FleetInfo current in list)
				{
					if (num >= 0)
					{
						break;
					}
					num += App.GameDatabase.GetFleetCruiserEquivalent(current.ID);
					bool flag2 = StarFleet.IsSuulkaFleet(App.GameDatabase, current);
					if (flag)
					{
						if (!flag2)
						{
							int? reserveFleetID = App.GameDatabase.GetReserveFleetID(current.PlayerID, current.SystemID);
							if (!reserveFleetID.HasValue)
							{
								App.GameDatabase.RemoveFleet(current.ID);
							}
							else
							{
								List<ShipInfo> list2 = App.GameDatabase.GetShipInfoByFleetID(current.ID, false).ToList<ShipInfo>();
								foreach (ShipInfo current2 in list2)
								{
									App.GameDatabase.UpdateShipAIFleetID(current2.ID, null);
									App.GameDatabase.TransferShip(current2.ID, reserveFleetID.Value);
								}
								App.GameDatabase.RemoveFleet(current.ID);
							}
						}
					}
					else
					{
						int num2 = App.GameDatabase.FindNewHomeSystem(current);
						if (num2 != 0)
						{
							bool flag3 = false;
							if (!flag2 && App.GameDatabase.GetRemainingSupportPoints(App.Game, num2, current.PlayerID) < App.GameDatabase.GetFleetCruiserEquivalent(current.ID))
							{
								flag3 = true;
							}
							int missionID = App.GameDatabase.InsertMission(current.ID, MissionType.RELOCATION, num2, 0, 0, 1, false, null);
							App.GameDatabase.InsertWaypoint(missionID, WaypointType.TravelTo, new int?(num2));
							if (flag3)
							{
								App.GameDatabase.InsertWaypoint(missionID, WaypointType.DisbandFleet, null);
							}
							App.GameDatabase.InsertWaypoint(missionID, WaypointType.DoMission, null);
						}
					}
				}
			}
		}
		public static IEnumerable<int> CollectAvailableSystemsForFleetMission(GameDatabase db, GameSession game, int fleetid, MissionType mission, bool forUI)
		{
			List<StarSystemInfo> source = game.GameDatabase.GetStarSystemInfos().ToList<StarSystemInfo>();
			FleetInfo fleetInfo = game.GameDatabase.GetFleetInfo(fleetid);
			int playerID = fleetInfo.PlayerID;
			string factionName = game.GameDatabase.GetFactionName(game.GameDatabase.GetPlayerFactionID(fleetInfo.PlayerID));
			game.AssetDatabase.GetFaction(factionName);
			List<int> list = new List<int>();
			foreach (int current in 
				from x in source
				select x.ID)
			{
				bool flag = false;
				switch (mission)
				{
				case MissionType.COLONIZATION:
					if (game.GameDatabase.CanColonize(playerID, current, game.GameDatabase.GetStratModifier<int>(StratModifiers.MaxColonizableHazard, playerID)))
					{
						flag = (StarFleet.CanDoColonizeMissionToTarget(game, current, fleetInfo.ID, null, null, null) && (StarFleet.GetNumColonizationShips(game, fleetInfo.ID) > 0 || factionName == "loa"));
					}
					break;
				case MissionType.SUPPORT:
					if (game.GameDatabase.CanSupport(playerID, current))
					{
						flag = StarFleet.CanDoSupportMissionToTarget(game, current, fleetInfo.ID, null, null, null);
					}
					break;
				case MissionType.SURVEY:
					if (game.GameDatabase.CanSurvey(playerID, current))
					{
						flag = StarFleet.CanDoSurveyMissionToTarget(game, current, fleetInfo.ID, null, null, null);
					}
					break;
				case MissionType.RELOCATION:
					if (game.GameDatabase.CanRelocate(game, playerID, current))
					{
						flag = StarFleet.CanDoRelocationMissionToTarget(game, current, fleetInfo.ID);
					}
					break;
				case MissionType.PATROL:
					if (game.GameDatabase.CanPatrol(playerID, current))
					{
						flag = StarFleet.CanDoPatrolMissionToTarget(game, current, fleetInfo.ID, null, null, null);
					}
					break;
				case MissionType.INTERDICTION:
					if (game.GameDatabase.CanInterdict(playerID, current))
					{
						flag = StarFleet.CanDoInterdictionMissionToTarget(game, current, fleetInfo.ID, null, null, null);
					}
					break;
				case MissionType.STRIKE:
					if (game.GameDatabase.CanStrike(playerID, current))
					{
						flag = StarFleet.CanDoStrikeMissionToTarget(game, current, fleetInfo.ID, null, null, null);
					}
					break;
				case MissionType.INVASION:
					if (game.GameDatabase.CanInvade(playerID, current))
					{
						flag = StarFleet.CanDoInvasionMissionToTarget(game, current, fleetInfo.ID, null, null, null);
					}
					break;
				case MissionType.PIRACY:
					if (game.GameDatabase.CanPirate(playerID, current))
					{
						flag = StarFleet.CanDoPiracyMissionToTarget(game, current, fleetInfo.ID, null, null, null);
					}
					break;
				case MissionType.EVACUATE:
					flag = (StarFleet.CanDoEvacuationMissionToTarget(game, current, fleetInfo.ID, fleetInfo.SupportingSystemID, null, null, null) || factionName == "loa");
					break;
				}
				if (flag)
				{
					if (!list.Contains(current))
					{
						list.Add(current);
					}
					if (forUI)
					{
						break;
					}
				}
			}
			return list;
		}
		public static IEnumerable<FleetInfo> CollectAvailableFleets(GameSession game, int playerId, int systemId, MissionType missionType, bool forUI)
		{
			IEnumerable<FleetInfo> fleetInfosByPlayerID = game.GameDatabase.GetFleetInfosByPlayerID(playerId, FleetType.FL_NORMAL);
			string factionName = game.GameDatabase.GetFactionName(game.GameDatabase.GetPlayerFactionID(playerId));
			Faction faction = game.AssetDatabase.GetFaction(factionName);
			switch (missionType)
			{
			case MissionType.COLONIZATION:
				if (game.GameDatabase.CanColonize(playerId, systemId, game.GameDatabase.GetStratModifier<int>(StratModifiers.MaxColonizableHazard, playerId)))
				{
					return 
						from x in fleetInfosByPlayerID
						where StarFleet.CanDoColonizeMissionToTarget(game, systemId, x.ID, null, null, null) && (StarFleet.GetNumColonizationShips(game, x.ID) > 0 || factionName == "loa")
						select x;
				}
				goto IL_427;
			case MissionType.SUPPORT:
				if (game.GameDatabase.CanSupport(playerId, systemId))
				{
					return 
						from x in fleetInfosByPlayerID
						where StarFleet.CanDoSupportMissionToTarget(game, systemId, x.ID, null, null, null)
						select x;
				}
				goto IL_427;
			case MissionType.SURVEY:
				if (game.GameDatabase.CanSurvey(playerId, systemId))
				{
					return 
						from x in fleetInfosByPlayerID
						where StarFleet.CanDoSurveyMissionToTarget(game, systemId, x.ID, null, null, null)
						select x;
				}
				goto IL_427;
			case MissionType.RELOCATION:
				if (game.GameDatabase.CanRelocate(game, playerId, systemId))
				{
					return 
						from x in fleetInfosByPlayerID
						where StarFleet.CanDoRelocationMissionToTarget(game, systemId, x.ID)
						select x;
				}
				goto IL_427;
			case MissionType.CONSTRUCT_STN:
				if (game.GameDatabase.CanConstructStation(game.App, playerId, systemId, faction.CanUseGate()))
				{
					return 
						from x in fleetInfosByPlayerID
						where StarFleet.CanDoConstructionMissionToTarget(game, systemId, x.ID, forUI)
						select x;
				}
				goto IL_427;
			case MissionType.UPGRADE_STN:
			case MissionType.INTERCEPT:
			case MissionType.RETURN:
			case MissionType.RETREAT:
				break;
			case MissionType.PATROL:
				if (game.GameDatabase.CanPatrol(playerId, systemId))
				{
					return 
						from x in fleetInfosByPlayerID
						where StarFleet.CanDoPatrolMissionToTarget(game, systemId, x.ID, null, null, null)
						select x;
				}
				goto IL_427;
			case MissionType.INTERDICTION:
				if (game.GameDatabase.CanInterdict(playerId, systemId))
				{
					return 
						from x in fleetInfosByPlayerID
						where StarFleet.CanDoInterdictionMissionToTarget(game, systemId, x.ID, null, null, null)
						select x;
				}
				goto IL_427;
			case MissionType.STRIKE:
				if (game.GameDatabase.CanStrike(playerId, systemId))
				{
					return 
						from x in fleetInfosByPlayerID
						where StarFleet.CanDoStrikeMissionToTarget(game, systemId, x.ID, null, null, null)
						select x;
				}
				goto IL_427;
			case MissionType.INVASION:
				if (game.GameDatabase.CanInvade(playerId, systemId))
				{
					return 
						from x in fleetInfosByPlayerID
						where StarFleet.CanDoInvasionMissionToTarget(game, systemId, x.ID, null, null, null)
						select x;
				}
				goto IL_427;
			case MissionType.GATE:
				if (game.GameDatabase.CanGate(playerId, systemId))
				{
					return 
						from x in fleetInfosByPlayerID
						where StarFleet.CanDoGateMissionToTarget(game, systemId, x.ID, null, null, null)
						select x;
				}
				goto IL_427;
			case MissionType.PIRACY:
				if (game.GameDatabase.CanPirate(playerId, systemId))
				{
					return 
						from x in fleetInfosByPlayerID
						where StarFleet.CanDoPiracyMissionToTarget(game, systemId, x.ID, null, null, null)
						select x;
				}
				goto IL_427;
			case MissionType.DEPLOY_NPG:
				if (faction.CanUseAccelerators())
				{
					return 
						from x in fleetInfosByPlayerID
						where StarFleet.CanDoDeployNPGToTarget(game, systemId, x.ID, null, null, null)
						select x;
				}
				goto IL_427;
			case MissionType.EVACUATE:
				return 
					from x in fleetInfosByPlayerID
					where StarFleet.CanDoEvacuationMissionToTarget(game, systemId, x.ID, x.SupportingSystemID, null, null, null) || factionName == "loa"
					select x;
			case MissionType.SPECIAL_CONSTRUCT_STN:
				return 
					from x in fleetInfosByPlayerID
					where StarFleet.GetConstructionPointsForFleet(game, x.ID) > 0f
					select x;
			default:
				if (missionType == MissionType.REACTION)
				{
					return Enumerable.Empty<FleetInfo>();
				}
				break;
			}
			return 
				from x in fleetInfosByPlayerID
				where game.GameDatabase.GetMissionByFleetID(x.ID) == null
				select x;
			IL_427:
			return Enumerable.Empty<FleetInfo>();
		}
		public static int CalculateRetrofitCost(App App, DesignInfo olddesign, DesignInfo RetrofitDesign)
		{
			IEnumerable<DesignInfo> visibleDesignInfosForPlayer = App.GameDatabase.GetVisibleDesignInfosForPlayer(App.LocalPlayer.ID);
			int num = 1;
			if (RetrofitDesign.RetrofitBaseID != 0)
			{
				num = StarFleet.RetrofitCostMultiplier(RetrofitDesign, visibleDesignInfosForPlayer);
			}
			else
			{
				num = StarFleet.RetrofitCostMultiplier(olddesign, visibleDesignInfosForPlayer) + 1;
			}
			int num2 = 0;
			DesignSectionInfo[] designSections = RetrofitDesign.DesignSections;
			DesignSectionInfo desi;
			for (int i = 0; i < designSections.Length; i++)
			{
				desi = designSections[i];
				DesignSectionInfo designSectionInfo = olddesign.DesignSections.FirstOrDefault((DesignSectionInfo x) => x.FilePath == desi.FilePath);
				foreach (WeaponBankInfo wbi in desi.WeaponBanks)
				{
					if (designSectionInfo.WeaponBanks.Any((WeaponBankInfo j) => j.BankGUID == wbi.BankGUID && ((j.WeaponID != wbi.WeaponID && !j.DesignID.HasValue) || j.DesignID != wbi.DesignID)))
					{
						string weaponFile = App.GameDatabase.GetWeaponAsset(wbi.WeaponID.Value);
						LogicalWeapon logicalWeapon = App.Game.AssetDatabase.Weapons.FirstOrDefault((LogicalWeapon x) => x.FileName == weaponFile);
						if (logicalWeapon != null)
						{
							num2 += logicalWeapon.Cost * 2 * num;
						}
						if (wbi.DesignID.HasValue)
						{
							WeaponBankInfo weaponBankInfo = designSectionInfo.WeaponBanks.FirstOrDefault((WeaponBankInfo x) => x.BankGUID == wbi.BankGUID);
							LogicalBank lb = desi.ShipSectionAsset.Banks.FirstOrDefault((LogicalBank x) => x.GUID == wbi.BankGUID);
							int num3 = (
								from x in desi.ShipSectionAsset.Mounts
								where x.Bank == lb
								select x).Count<LogicalMount>();
							num3 = ((num3 > 0) ? num3 : 1);
							DesignInfo designInfo = App.GameDatabase.GetDesignInfo(wbi.DesignID.Value);
							DesignInfo designInfo2 = App.GameDatabase.GetDesignInfo(weaponBankInfo.DesignID.Value);
							int num4 = designInfo.SavingsCost - designInfo2.SavingsCost;
							num4 = ((num4 >= 0) ? num4 : 0);
							if (designInfo != null)
							{
								num2 += num4 * num * num3;
							}
						}
					}
					else
					{
						if (!designSectionInfo.WeaponBanks.Any((WeaponBankInfo j) => j.BankGUID == wbi.BankGUID))
						{
							string weaponFile = App.GameDatabase.GetWeaponAsset(wbi.WeaponID.Value);
							LogicalWeapon logicalWeapon2 = App.Game.AssetDatabase.Weapons.FirstOrDefault((LogicalWeapon x) => x.FileName == weaponFile);
							if (logicalWeapon2 != null)
							{
								num2 += logicalWeapon2.Cost * 2 * num;
							}
							if (wbi.DesignID.HasValue)
							{
								DesignInfo designInfo3 = App.GameDatabase.GetDesignInfo(wbi.DesignID.Value);
								if (designInfo3 != null)
								{
									num2 += designInfo3.ProductionCost * num;
								}
							}
						}
					}
				}
				foreach (DesignModuleInfo dmi in desi.Modules)
				{
					if (designSectionInfo.Modules.Any((DesignModuleInfo j) => j.MountNodeName == dmi.MountNodeName && j.ModuleID != dmi.ModuleID))
					{
						string moduleAsset = App.GameDatabase.GetModuleAsset(dmi.ModuleID);
						LogicalModule logicalModule = App.AssetDatabase.Modules.FirstOrDefault((LogicalModule x) => x.ModulePath == moduleAsset);
						if (logicalModule != null)
						{
							num2 += logicalModule.SavingsCost * 2 * num;
						}
					}
					else
					{
						if (!designSectionInfo.Modules.Any((DesignModuleInfo j) => j.MountNodeName == dmi.MountNodeName))
						{
							string moduleAsset = App.GameDatabase.GetModuleAsset(dmi.ModuleID);
							LogicalModule logicalModule2 = App.AssetDatabase.Modules.FirstOrDefault((LogicalModule x) => x.ModulePath == moduleAsset);
							if (logicalModule2 != null)
							{
								num2 += logicalModule2.SavingsCost * 2 * num;
							}
						}
					}
				}
			}
			return num2;
		}
		public static int CalculateStationRetrofitCost(App App, DesignInfo olddesign, DesignInfo RetrofitDesign)
		{
			int num = 0;
			foreach (DesignModuleInfo dmi in RetrofitDesign.DesignSections[0].Modules)
			{
				DesignModuleInfo designModuleInfo = olddesign.DesignSections[0].Modules.FirstOrDefault((DesignModuleInfo x) => x.MountNodeName == dmi.MountNodeName);
				if (designModuleInfo != null && designModuleInfo.WeaponID != dmi.WeaponID)
				{
					string weaponFile = App.GameDatabase.GetWeaponAsset(dmi.WeaponID.Value);
					LogicalWeapon logicalWeapon = App.Game.AssetDatabase.Weapons.FirstOrDefault((LogicalWeapon x) => x.FileName == weaponFile);
					if (logicalWeapon != null)
					{
						num += logicalWeapon.Cost;
					}
				}
			}
			return num;
		}
		public static bool CanRetrofitStation(App App, int shipid)
		{
			List<StationRetrofitOrderInfo> source = App.GameDatabase.GetStationRetrofitOrders().ToList<StationRetrofitOrderInfo>();
			if (source.Any((StationRetrofitOrderInfo x) => x.ShipID == shipid))
			{
				return false;
			}
			ShipInfo shipInfo = App.GameDatabase.GetShipInfo(shipid, true);
			List<DesignModuleInfo> source2 = App.GameDatabase.GetQueuedStationModules(shipInfo.DesignInfo.DesignSections[0]).ToList<DesignModuleInfo>();
			if (source2.Any<DesignModuleInfo>())
			{
				return false;
			}
			if (shipInfo.DesignInfo.StationType == StationType.INVALID_TYPE)
			{
				return false;
			}
			foreach (DesignModuleInfo current in shipInfo.DesignInfo.DesignSections[0].Modules)
			{
				string moduleass = App.GameDatabase.GetModuleAsset(current.ModuleID);
				LogicalModule logicalModule = App.AssetDatabase.Modules.First((LogicalModule x) => x.ModulePath == moduleass);
				if (logicalModule.Banks.Count<LogicalBank>() > 0)
				{
					return true;
				}
			}
			return false;
		}
		public static int RetrofitCostMultiplier(DesignInfo RetrofitDesign, IEnumerable<DesignInfo> designs)
		{
			DesignInfo NewestDesign = StarFleet.GetNewestRetrofitDesign(RetrofitDesign, designs);
			int num = 0;
			while (NewestDesign != null)
			{
				NewestDesign = designs.FirstOrDefault((DesignInfo x) => x.ID == NewestDesign.RetrofitBaseID);
				num++;
			}
			return num;
		}
		public static bool IsNewestRetrofit(DesignInfo design, IEnumerable<DesignInfo> designs)
		{
			return (
				from x in designs
				where x.RetrofitBaseID == design.ID
				select x).Count<DesignInfo>() == 0;
		}
		public static DesignInfo GetNewestRetrofitDesign(DesignInfo design, IEnumerable<DesignInfo> designs)
		{
			if (design.ID != 0 && (
				from x in designs
				where x.RetrofitBaseID == design.ID
				select x).Count<DesignInfo>() != 0)
			{
				return StarFleet.GetNewestRetrofitDesign((
					from x in designs
					where x.RetrofitBaseID == design.ID
					select x).First<DesignInfo>(), designs);
			}
			return design;
		}
		public static DesignInfo GetRetrofitBaseDesign(DesignInfo design, IEnumerable<DesignInfo> designs)
		{
			if (design.RetrofitBaseID == 0)
			{
				return design;
			}
			return StarFleet.GetRetrofitBaseDesign((
				from x in designs
				where x.RetrofitBaseID == design.ID
				select x).First<DesignInfo>(), designs);
		}
		public static double GetTimeRequiredToRetrofit(App App, ShipInfo shipinfo, int numships)
		{
			if (numships == 0)
			{
				return 0.0;
			}
			FleetInfo fleetInfo = App.GameDatabase.GetFleetInfo(shipinfo.FleetID);
			if (fleetInfo == null)
			{
				return 0.0;
			}
			StationInfo navalStationForSystemAndPlayer = App.GameDatabase.GetNavalStationForSystemAndPlayer(fleetInfo.SystemID, App.LocalPlayer.ID);
			if (navalStationForSystemAndPlayer != null)
			{
				int num = 0;
				DesignSectionInfo[] designSections = navalStationForSystemAndPlayer.DesignInfo.DesignSections;
				for (int i = 0; i < designSections.Length; i++)
				{
					DesignSectionInfo designSectionInfo = designSections[i];
					num += (
						from x in designSectionInfo.Modules
						where x.StationModuleType == ModuleEnums.StationModuleType.Dock
						select x).Count<DesignModuleInfo>();
				}
				return Math.Ceiling((double)numships / (double)num);
			}
			return 0.0;
		}
		public static bool SystemSupportsRetrofitting(App App, int systemID, int playerid)
		{
			List<ColonyInfo> source = App.GameDatabase.GetColonyInfosForSystem(systemID).ToList<ColonyInfo>();
			if (!source.Any((ColonyInfo x) => x.PlayerID == playerid))
			{
				return false;
			}
			StationInfo navalStationForSystemAndPlayer = App.GameDatabase.GetNavalStationForSystemAndPlayer(systemID, playerid);
			if (navalStationForSystemAndPlayer != null)
			{
				int num = 0;
				DesignSectionInfo[] designSections = navalStationForSystemAndPlayer.DesignInfo.DesignSections;
				for (int i = 0; i < designSections.Length; i++)
				{
					DesignSectionInfo designSectionInfo = designSections[i];
					num += (
						from x in designSectionInfo.Modules
						where x.StationModuleType == ModuleEnums.StationModuleType.Dock
						select x).Count<DesignModuleInfo>();
				}
				return num > 0 && navalStationForSystemAndPlayer.DesignInfo.StationLevel >= 3;
			}
			return false;
		}
		public static int GetSystemRetrofitCapacity(App App, int systemID, int playerId)
		{
			StationInfo navalStationForSystemAndPlayer = App.GameDatabase.GetNavalStationForSystemAndPlayer(systemID, playerId);
			if (navalStationForSystemAndPlayer != null)
			{
				int num = 0;
				DesignSectionInfo[] designSections = navalStationForSystemAndPlayer.DesignInfo.DesignSections;
				for (int i = 0; i < designSections.Length; i++)
				{
					DesignSectionInfo designSectionInfo = designSections[i];
					num += (
						from x in designSectionInfo.Modules
						where x.StationModuleType == ModuleEnums.StationModuleType.Dock
						select x).Count<DesignModuleInfo>();
				}
				return num;
			}
			return 0;
		}
		public static int GetFleetCommandPoints(App App, int fleetid, IEnumerable<int> excludeShips = null)
		{
			int num = 0;
			List<ShipInfo> list = App.GameDatabase.GetShipInfoByFleetID(fleetid, true).ToList<ShipInfo>();
			foreach (ShipInfo inf in list)
			{
				if (excludeShips != null)
				{
					if (excludeShips.Any((int x) => x == inf.ID))
					{
						continue;
					}
				}
				num += inf.DesignInfo.GetCommandPoints();
			}
			return num;
		}
		public static int GetFleetCommandCost(App App, int fleetid, IEnumerable<int> excludeShips = null)
		{
			int num = 0;
			List<ShipInfo> list = App.GameDatabase.GetShipInfoByFleetID(fleetid, true).ToList<ShipInfo>();
			foreach (ShipInfo inf in list)
			{
				if (excludeShips != null)
				{
					if (excludeShips.Any((int x) => x == inf.ID))
					{
						continue;
					}
				}
				num += inf.DesignInfo.CommandPointCost;
			}
			return num;
		}
		public static bool FleetCanFunctionWithoutShips(App App, int fleetid, IEnumerable<int> excludeShips)
		{
			FleetInfo fleetInfo = App.GameDatabase.GetFleetInfo(fleetid);
			if (fleetInfo == null)
			{
				return false;
			}
			if (fleetInfo.Type == FleetType.FL_RESERVE)
			{
				return true;
			}
			App.GameDatabase.GetShipInfoByFleetID(fleetid, true).ToList<ShipInfo>();
			int fleetCommandPoints = StarFleet.GetFleetCommandPoints(App, fleetid, excludeShips);
			int fleetCommandPoints2 = StarFleet.GetFleetCommandPoints(App, fleetid, null);
			if (fleetCommandPoints2 <= fleetCommandPoints)
			{
				return true;
			}
			int fleetCommandCost = StarFleet.GetFleetCommandCost(App, fleetid, excludeShips);
			return fleetCommandPoints >= fleetCommandCost;
		}
		public static bool FleetCanFunctionWithoutShip(App App, int fleetid, int shipid)
		{
			return StarFleet.FleetCanFunctionWithoutShips(App, fleetid, new List<int>
			{
				shipid
			});
		}
		private static CombatZonePositionInfo GetZoneFromPosition(App App, Vector3 Position, Vector3 Origin, List<CombatZonePositionInfo> cz)
		{
			float num = (float)Math.Abs((Math.Atan2((double)Position.Z, (double)Position.X) + 12.566370614359172) % 6.2831853071795862);
			float length = (Position - Origin).Length;
			foreach (CombatZonePositionInfo current in cz)
			{
				if (length >= current.RadiusLower && length <= current.RadiusUpper && num >= current.AngleLeft && num <= current.AngleRight)
				{
					return current;
				}
			}
			return null;
		}
		private static Vector3 PickRandomPositionAroundOrigin(App App, Vector3 Origin, int distance)
		{
			Random random = new Random();
			float num = (float)random.Next(360);
			Vector3 vector = new Vector3((float)Math.Cos((double)num), 0f, (float)Math.Sin((double)num));
			vector = Origin - vector;
			return vector * (float)distance;
		}
		private static bool CanPlacePlatformInZone(App App, List<ShipInfo> PlacedDefShips, List<CombatZonePositionInfo> combatzones, ShipInfo Ship, Matrix Position)
		{
			CombatZonePositionInfo zoneFromPosition = StarFleet.GetZoneFromPosition(App, Position.Position, new Vector3(0f, 0f, 0f), combatzones);
			if (zoneFromPosition == null)
			{
				return false;
			}
			int num = (zoneFromPosition.RingIndex == 0) ? 1 : ((zoneFromPosition.RingIndex == 2) ? 3 : 2);
			foreach (ShipInfo current in PlacedDefShips)
			{
				if (current != Ship && current.ShipSystemPosition.HasValue)
				{
					CombatZonePositionInfo zoneFromPosition2 = StarFleet.GetZoneFromPosition(App, current.ShipSystemPosition.Value.Position, new Vector3(0f, 0f, 0f), combatzones);
					if (zoneFromPosition2 != null)
					{
						if (zoneFromPosition2.RingIndex == zoneFromPosition.RingIndex && zoneFromPosition2.ZoneIndex == zoneFromPosition.ZoneIndex)
						{
							num--;
						}
						if (num == 0)
						{
							return false;
						}
					}
				}
			}
			return true;
		}
		private static bool CanPlaceAsset(App App, List<ShipInfo> PlacedDefShips, List<PlanetInfo> planets, ShipInfo Ship, Matrix Position)
		{
			foreach (PlanetInfo current in planets)
			{
				Matrix orbitalTransform = App.GameDatabase.GetOrbitalTransform(current.ID);
				StellarBody.Params stellarBodyParams = App.AssetDatabase.PlanetGenerationRules.GetStellarBodyParams(App.Game, current.ID);
				float length = (Position.Position - orbitalTransform.Position).Length;
				if (length < stellarBodyParams.Radius + 6000f)
				{
					bool result = false;
					return result;
				}
			}
			foreach (ShipInfo current2 in PlacedDefShips)
			{
				if (current2 != Ship && current2.ShipSystemPosition.HasValue)
				{
					float length2 = (current2.ShipSystemPosition.Value.Position - Position.Position).Length;
					if (length2 < 5000f)
					{
						bool result = false;
						return result;
					}
				}
			}
			return true;
		}
		private static bool AutoPlaceSDB(App App, ShipInfo DefAsset, StarSystemInfo ssi)
		{
			FleetInfo fi = App.GameDatabase.GetFleetInfo(DefAsset.FleetID);
			if (fi == null)
			{
				return false;
			}
			List<CombatZonePositionInfo> combatZonesForSystem = Kerberos.Sots.GameStates.StarSystem.GetCombatZonesForSystem(App.Game, ssi.ID, 1f);
			if (combatZonesForSystem == null || combatZonesForSystem.Count == 0)
			{
				return false;
			}
			List<PlanetInfo> source = App.GameDatabase.GetStarSystemPlanetInfos(ssi.ID).ToList<PlanetInfo>();
			List<ColonyInfo> colonies = (
				from x in App.GameDatabase.GetColonyInfosForSystem(ssi.ID)
				where x.PlayerID == fi.PlayerID
				select x).ToList<ColonyInfo>();
			List<ColonyInfo> list = (
				from x in colonies
				where x.PlayerID == fi.PlayerID
				select x).ToList<ColonyInfo>();
			list = (
				from x in list
				orderby App.GameDatabase.GetTotalPopulation(x)
				select x).Reverse<ColonyInfo>().ToList<ColonyInfo>();
			Random random = new Random();
			new List<ShipInfo>();
			int? defenseFleetID = App.GameDatabase.GetDefenseFleetID(ssi.ID, fi.PlayerID);
			if (defenseFleetID.HasValue)
			{
				(
					from x in App.GameDatabase.GetShipInfoByFleetID(defenseFleetID.Value, false)
					where x.ShipSystemPosition.HasValue
					select x).ToList<ShipInfo>();
			}
			int num = 5;
			if (list.Count > 0)
			{
				for (int i = 0; i < num * 2; i++)
				{
					ColonyInfo colonyInfo = list[random.Next(list.Count)];
					if (colonyInfo != null)
					{
						Matrix orbitalTransform = App.GameDatabase.GetOrbitalTransform(colonyInfo.OrbitalObjectID);
						Vector3 arg_1BF_0 = orbitalTransform.Position;
						StellarBody.Params stellarBodyParams = App.AssetDatabase.PlanetGenerationRules.GetStellarBodyParams(App.Game, colonyInfo.OrbitalObjectID);
						Vector3 trans = StarFleet.PickRandomPositionAroundOrigin(App, new Vector3(0f, 0f, 0f), (int)(stellarBodyParams.Radius + 6000f));
						Matrix value = Matrix.CreateTranslation(trans) * orbitalTransform;
						PlanetInfo planetInfo = App.GameDatabase.GetPlanetInfo(colonyInfo.OrbitalObjectID);
						int num2 = 1;
						if (App.AssetDatabase.IsGasGiant(planetInfo.Type))
						{
							num2 = 3;
						}
						else
						{
							if (App.AssetDatabase.IsPotentialyHabitable(planetInfo.Type))
							{
								num2 = 2;
							}
						}
						List<SDBInfo> list2 = App.GameDatabase.GetSDBInfoFromOrbital(planetInfo.ID).ToList<SDBInfo>();
						if (num2 - list2.Count > 0)
						{
							App.GameDatabase.UpdateShipSystemPosition(DefAsset.ID, new Matrix?(value));
							App.GameDatabase.InsertSDB(planetInfo.ID, DefAsset.ID);
							return true;
						}
					}
				}
			}
			List<PlanetInfo> list3 = (
				from x in source
				where !colonies.Any(j => j.OrbitalObjectID == x.ID)
				select x).ToList<PlanetInfo>();
			if (list3.Count > 0)
			{
				for (int j = 0; j < num; j++)
				{
					PlanetInfo planetInfo2 = list3[random.Next(list3.Count)];
					if (planetInfo2 != null)
					{
						StellarBody.Params stellarBodyParams2 = App.AssetDatabase.PlanetGenerationRules.GetStellarBodyParams(App.Game, planetInfo2.ID);
						Matrix orbitalTransform2 = App.GameDatabase.GetOrbitalTransform(planetInfo2.ID);
						Vector3 arg_398_0 = orbitalTransform2.Position;
						Vector3 trans2 = StarFleet.PickRandomPositionAroundOrigin(App, new Vector3(0f, 0f, 0f), (int)(stellarBodyParams2.Radius + 6000f));
						Matrix value2 = Matrix.CreateTranslation(trans2) * orbitalTransform2;
						int num3 = 1;
						if (App.AssetDatabase.IsGasGiant(planetInfo2.Type))
						{
							num3 = 3;
						}
						else
						{
							if (App.AssetDatabase.IsPotentialyHabitable(planetInfo2.Type))
							{
								num3 = 2;
							}
						}
						List<SDBInfo> list4 = App.GameDatabase.GetSDBInfoFromOrbital(planetInfo2.ID).ToList<SDBInfo>();
						if (num3 - list4.Count > 0)
						{
							App.GameDatabase.UpdateShipSystemPosition(DefAsset.ID, new Matrix?(value2));
							App.GameDatabase.InsertSDB(planetInfo2.ID, DefAsset.ID);
							return true;
						}
					}
				}
			}
			return false;
		}
		private static bool AutoPlacePoliceShip(App App, ShipInfo DefAsset, StarSystemInfo ssi)
		{
			FleetInfo fi = App.GameDatabase.GetFleetInfo(DefAsset.FleetID);
			if (fi == null)
			{
				return false;
			}
			List<CombatZonePositionInfo> combatZonesForSystem = Kerberos.Sots.GameStates.StarSystem.GetCombatZonesForSystem(App.Game, ssi.ID, 1f);
			if (combatZonesForSystem == null || combatZonesForSystem.Count == 0)
			{
				return false;
			}
			List<PlanetInfo> list = App.GameDatabase.GetStarSystemPlanetInfos(ssi.ID).ToList<PlanetInfo>();
			List<ColonyInfo> colonies = (
				from x in App.GameDatabase.GetColonyInfosForSystem(ssi.ID)
				where x.PlayerID == fi.PlayerID
				select x).ToList<ColonyInfo>();
			List<ColonyInfo> list2 = (
				from x in colonies
				where x.PlayerID == fi.PlayerID
				select x).ToList<ColonyInfo>();
			list2 = (
				from x in list2
				orderby App.GameDatabase.GetTotalPopulation(x)
				select x).Reverse<ColonyInfo>().ToList<ColonyInfo>();
			Random random = new Random();
			List<ShipInfo> placedDefShips = new List<ShipInfo>();
			int? defenseFleetID = App.GameDatabase.GetDefenseFleetID(ssi.ID, fi.PlayerID);
			if (defenseFleetID.HasValue)
			{
				placedDefShips = (
					from x in App.GameDatabase.GetShipInfoByFleetID(defenseFleetID.Value, false)
					where x.ShipSystemPosition.HasValue
					select x).ToList<ShipInfo>();
			}
			int num = 5;
			if (list2.Count > 0)
			{
				for (int i = 0; i < num * 2; i++)
				{
					ColonyInfo colonyInfo = list2[random.Next(list2.Count)];
					if (colonyInfo != null)
					{
						Matrix orbitalTransform = App.GameDatabase.GetOrbitalTransform(colonyInfo.OrbitalObjectID);
						Vector3 arg_1C1_0 = orbitalTransform.Position;
						int num2 = random.Next(9000);
						StellarBody.Params stellarBodyParams = App.AssetDatabase.PlanetGenerationRules.GetStellarBodyParams(App.Game, colonyInfo.OrbitalObjectID);
						Vector3 trans = StarFleet.PickRandomPositionAroundOrigin(App, new Vector3(0f, 0f, 0f), (int)(stellarBodyParams.Radius + 6000f + (float)num2));
						Matrix matrix = Matrix.CreateTranslation(trans) * orbitalTransform;
						if (StarFleet.CanPlaceAsset(App, placedDefShips, list, DefAsset, matrix))
						{
							App.GameDatabase.UpdateShipSystemPosition(DefAsset.ID, new Matrix?(matrix));
							return true;
						}
					}
				}
			}
			List<PlanetInfo> list3 = (
				from x in list
				where !colonies.Any(j => j.OrbitalObjectID == x.ID)
				select x).ToList<PlanetInfo>();
			if (list3.Count > 0)
			{
				for (int j = 0; j < num; j++)
				{
					PlanetInfo planetInfo = list3[random.Next(list3.Count)];
					if (planetInfo != null)
					{
						StellarBody.Params stellarBodyParams2 = App.AssetDatabase.PlanetGenerationRules.GetStellarBodyParams(App.Game, planetInfo.ID);
						Matrix orbitalTransform2 = App.GameDatabase.GetOrbitalTransform(planetInfo.ID);
						Vector3 arg_31B_0 = orbitalTransform2.Position;
						int num3 = random.Next(9000);
						Vector3 trans2 = StarFleet.PickRandomPositionAroundOrigin(App, new Vector3(0f, 0f, 0f), (int)(stellarBodyParams2.Radius + 6000f + (float)num3));
						Matrix matrix2 = Matrix.CreateTranslation(trans2) * orbitalTransform2;
						if (StarFleet.CanPlaceAsset(App, placedDefShips, list, DefAsset, matrix2))
						{
							App.GameDatabase.UpdateShipSystemPosition(DefAsset.ID, new Matrix?(matrix2));
							return true;
						}
					}
				}
			}
			return false;
		}
		private static bool AutoPlaceMinefield(App App, ShipInfo DefAsset, StarSystemInfo ssi)
		{
			FleetInfo fi = App.GameDatabase.GetFleetInfo(DefAsset.FleetID);
			if (fi == null)
			{
				return false;
			}
			List<CombatZonePositionInfo> combatZonesForSystem = Kerberos.Sots.GameStates.StarSystem.GetCombatZonesForSystem(App.Game, ssi.ID, 1f);
			if (combatZonesForSystem == null || combatZonesForSystem.Count == 0)
			{
				return false;
			}
			List<PlanetInfo> list = App.GameDatabase.GetStarSystemPlanetInfos(ssi.ID).ToList<PlanetInfo>();
			List<ColonyInfo> colonies = (
				from x in App.GameDatabase.GetColonyInfosForSystem(ssi.ID)
				where x.PlayerID == fi.PlayerID
				select x).ToList<ColonyInfo>();
			List<ColonyInfo> list2 = (
				from x in colonies
				where x.PlayerID == fi.PlayerID
				select x).ToList<ColonyInfo>();
			list2 = (
				from x in list2
				orderby App.GameDatabase.GetTotalPopulation(x)
				select x).Reverse<ColonyInfo>().ToList<ColonyInfo>();
			Random random = new Random();
			List<ShipInfo> placedDefShips = new List<ShipInfo>();
			int? defenseFleetID = App.GameDatabase.GetDefenseFleetID(ssi.ID, fi.PlayerID);
			if (defenseFleetID.HasValue)
			{
				placedDefShips = (
					from x in App.GameDatabase.GetShipInfoByFleetID(defenseFleetID.Value, false)
					where x.ShipSystemPosition.HasValue
					select x).ToList<ShipInfo>();
			}
			int num = 5;
			if (list2.Count > 0)
			{
				for (int i = 0; i < num * 2; i++)
				{
					ColonyInfo colonyInfo = list2[random.Next(list2.Count)];
					if (colonyInfo != null)
					{
						Matrix orbitalTransform = App.GameDatabase.GetOrbitalTransform(colonyInfo.OrbitalObjectID);
						Vector3 arg_1C1_0 = orbitalTransform.Position;
						int num2 = random.Next(8000);
						StellarBody.Params stellarBodyParams = App.AssetDatabase.PlanetGenerationRules.GetStellarBodyParams(App.Game, colonyInfo.OrbitalObjectID);
						Vector3 trans = StarFleet.PickRandomPositionAroundOrigin(App, new Vector3(0f, 0f, 0f), (int)(stellarBodyParams.Radius + 6000f + (float)num2));
						Matrix matrix = Matrix.CreateTranslation(trans) * orbitalTransform;
						if (StarFleet.CanPlaceAsset(App, placedDefShips, list, DefAsset, matrix))
						{
							App.GameDatabase.UpdateShipSystemPosition(DefAsset.ID, new Matrix?(matrix));
							return true;
						}
					}
				}
			}
			List<PlanetInfo> list3 = (
				from x in list
				where !colonies.Any(j => j.OrbitalObjectID == x.ID)
				select x).ToList<PlanetInfo>();
			if (list3.Count > 0)
			{
				for (int j = 0; j < num; j++)
				{
					PlanetInfo planetInfo = list3[random.Next(list3.Count)];
					if (planetInfo != null)
					{
						StellarBody.Params stellarBodyParams2 = App.AssetDatabase.PlanetGenerationRules.GetStellarBodyParams(App.Game, planetInfo.ID);
						Matrix orbitalTransform2 = App.GameDatabase.GetOrbitalTransform(planetInfo.ID);
						Vector3 arg_31B_0 = orbitalTransform2.Position;
						int num3 = random.Next(5000);
						Vector3 trans2 = StarFleet.PickRandomPositionAroundOrigin(App, new Vector3(0f, 0f, 0f), (int)(stellarBodyParams2.Radius + 6000f + (float)num3));
						Matrix matrix2 = Matrix.CreateTranslation(trans2) * orbitalTransform2;
						if (StarFleet.CanPlaceAsset(App, placedDefShips, list, DefAsset, matrix2))
						{
							App.GameDatabase.UpdateShipSystemPosition(DefAsset.ID, new Matrix?(matrix2));
							return true;
						}
					}
				}
			}
			return false;
		}
		private static bool AutoPlacePlatform(App App, ShipInfo DefAsset, StarSystemInfo ssi)
		{
			FleetInfo fi = App.GameDatabase.GetFleetInfo(DefAsset.FleetID);
			if (fi == null)
			{
				return false;
			}
			List<CombatZonePositionInfo> combatZonesForSystem = Kerberos.Sots.GameStates.StarSystem.GetCombatZonesForSystem(App.Game, ssi.ID, 1f);
			if (combatZonesForSystem == null || combatZonesForSystem.Count == 0)
			{
				return false;
			}
			List<PlanetInfo> list = App.GameDatabase.GetStarSystemPlanetInfos(ssi.ID).ToList<PlanetInfo>();
			List<ColonyInfo> colonies = (
				from x in App.GameDatabase.GetColonyInfosForSystem(ssi.ID)
				where x.PlayerID == fi.PlayerID
				select x).ToList<ColonyInfo>();
			List<ColonyInfo> list2 = (
				from x in colonies
				where x.PlayerID == fi.PlayerID
				select x).ToList<ColonyInfo>();
			list2 = (
				from x in list2
				orderby App.GameDatabase.GetTotalPopulation(x)
				select x).Reverse<ColonyInfo>().ToList<ColonyInfo>();
			Random random = new Random();
			List<ShipInfo> placedDefShips = new List<ShipInfo>();
			int? defenseFleetID = App.GameDatabase.GetDefenseFleetID(ssi.ID, fi.PlayerID);
			if (defenseFleetID.HasValue)
			{
				placedDefShips = (
					from x in App.GameDatabase.GetShipInfoByFleetID(defenseFleetID.Value, false)
					where x.ShipSystemPosition.HasValue
					select x).ToList<ShipInfo>();
			}
			int num = 5;
			if (list2.Count > 0)
			{
				for (int i = 0; i < num * 2; i++)
				{
					ColonyInfo colonyInfo = list2[random.Next(list2.Count)];
					if (colonyInfo != null)
					{
						Matrix orbitalTransform = App.GameDatabase.GetOrbitalTransform(colonyInfo.OrbitalObjectID);
						Vector3 arg_1C1_0 = orbitalTransform.Position;
						int num2 = random.Next(5000);
						StellarBody.Params stellarBodyParams = App.AssetDatabase.PlanetGenerationRules.GetStellarBodyParams(App.Game, colonyInfo.OrbitalObjectID);
						Vector3 trans = StarFleet.PickRandomPositionAroundOrigin(App, new Vector3(0f, 0f, 0f), (int)(stellarBodyParams.Radius + 6000f + (float)num2));
						Matrix matrix = Matrix.CreateTranslation(trans) * orbitalTransform;
						if (StarFleet.CanPlacePlatformInZone(App, placedDefShips, combatZonesForSystem, DefAsset, matrix) && StarFleet.CanPlaceAsset(App, placedDefShips, list, DefAsset, matrix))
						{
							App.GameDatabase.UpdateShipSystemPosition(DefAsset.ID, new Matrix?(matrix));
							return true;
						}
					}
				}
			}
			List<PlanetInfo> list3 = (
				from x in list
				where !colonies.Any(j => j.OrbitalObjectID == x.ID)
				select x).ToList<PlanetInfo>();
			if (list3.Count > 0)
			{
				for (int j = 0; j < num; j++)
				{
					PlanetInfo planetInfo = list3[random.Next(list3.Count)];
					if (planetInfo != null)
					{
						StellarBody.Params stellarBodyParams2 = App.AssetDatabase.PlanetGenerationRules.GetStellarBodyParams(App.Game, planetInfo.ID);
						Matrix orbitalTransform2 = App.GameDatabase.GetOrbitalTransform(planetInfo.ID);
						Vector3 arg_32F_0 = orbitalTransform2.Position;
						int num3 = random.Next(5000);
						Vector3 trans2 = StarFleet.PickRandomPositionAroundOrigin(App, new Vector3(0f, 0f, 0f), (int)(stellarBodyParams2.Radius + 6000f + (float)num3));
						Matrix matrix2 = Matrix.CreateTranslation(trans2) * orbitalTransform2;
						if (StarFleet.CanPlacePlatformInZone(App, placedDefShips, combatZonesForSystem, DefAsset, matrix2) && StarFleet.CanPlaceAsset(App, placedDefShips, list, DefAsset, matrix2))
						{
							App.GameDatabase.UpdateShipSystemPosition(DefAsset.ID, new Matrix?(matrix2));
							return true;
						}
					}
				}
			}
			return false;
		}
		public static bool AutoPlaceDefenseAsset(App App, int shipid, int systemid)
		{
			ShipInfo shipInfo = App.GameDatabase.GetShipInfo(shipid, false);
			StarSystemInfo starSystemInfo = App.GameDatabase.GetStarSystemInfo(systemid);
			if (shipInfo == null || starSystemInfo == null)
			{
				return false;
			}
			if (shipInfo.IsPlatform())
			{
				return StarFleet.AutoPlacePlatform(App, shipInfo, starSystemInfo);
			}
			if (shipInfo.IsMinelayer())
			{
				return StarFleet.AutoPlaceMinefield(App, shipInfo, starSystemInfo);
			}
			if (shipInfo.IsPoliceShip())
			{
				return StarFleet.AutoPlacePoliceShip(App, shipInfo, starSystemInfo);
			}
			return shipInfo.IsSDB() && StarFleet.AutoPlaceSDB(App, shipInfo, starSystemInfo);
		}
		public static string GetAdmiralAvatar(App App, int admiralid)
		{
			string text = "";
			AdmiralInfo admiralInfo = App.GameDatabase.GetAdmiralInfo(admiralid);
			if (admiralInfo != null)
			{
				text = string.Format("admiral_{0}", admiralInfo.Race);
				bool flag = admiralInfo.Gender == "female" && (admiralInfo.Race == "tarka" || admiralInfo.Race == "human" || admiralInfo.Race == "morrigi");
				if (flag)
				{
					text += '2';
				}
				if (admiralInfo.Engram)
				{
					text = "admiral_robot";
				}
			}
			return text;
		}
		public static int GetShipLoaCubeValue(GameSession game, int shipid)
		{
			ShipInfo shipInfo = game.GameDatabase.GetShipInfo(shipid, true);
			if (shipInfo == null || shipInfo.DesignInfo.GetRealShipClass() == RealShipClasses.BoardingPod || shipInfo.DesignInfo.GetRealShipClass() == RealShipClasses.Drone || shipInfo.DesignInfo.GetRealShipClass() == RealShipClasses.AssaultShuttle)
			{
				return 0;
			}
			if (shipInfo.DesignInfo.IsLoaCube())
			{
				return shipInfo.LoaCubes;
			}
			int[] healthAndHealthMax = StarFleet.GetHealthAndHealthMax(game, shipInfo.DesignInfo, shipInfo.ID);
			float val = (float)healthAndHealthMax[0] / (float)healthAndHealthMax[1];
			return (int)((float)shipInfo.DesignInfo.GetPlayerProductionCost(game.GameDatabase, shipInfo.DesignInfo.PlayerID, !shipInfo.DesignInfo.isPrototyped, null) * Math.Min(Math.Max(val, 0f), 1f));
		}
		public static int GetShipLoaCubeValue(GameSession game, DesignInfo design)
		{
			if (design == null || design.GetRealShipClass() == RealShipClasses.BoardingPod || design.GetRealShipClass() == RealShipClasses.Drone || design.GetRealShipClass() == RealShipClasses.AssaultShuttle)
			{
				return 0;
			}
			if (design.IsLoaCube())
			{
				return 0;
			}
			return design.GetPlayerProductionCost(game.GameDatabase, design.PlayerID, !design.isPrototyped, null);
		}
		public static int GetFleetLoaCubeValue(GameSession game, int fleetid)
		{
			int num = 0;
			foreach (int current in game.GameDatabase.GetShipsByFleetID(fleetid))
			{
				num += StarFleet.GetShipLoaCubeValue(game, current);
			}
			return num;
		}
		public static int ConvertFleetIntoLoaCubes(GameSession game, int fleetid)
		{
			FleetInfo fleetInfo = game.GameDatabase.GetFleetInfo(fleetid);
			if (StarFleet.IsGardenerFleet(game, fleetInfo) || !game.AssetDatabase.GetFaction(game.GameDatabase.GetPlayerInfo(fleetInfo.PlayerID).FactionID).CanUseAccelerators())
			{
				return 0;
			}
			List<ShipInfo> list = game.GameDatabase.GetShipInfoByFleetID(fleetid, true).ToList<ShipInfo>();
			if (!list.Any<ShipInfo>())
			{
				return 0;
			}
			ShipInfo shipInfo = list.FirstOrDefault((ShipInfo x) => x.DesignInfo.IsLoaCube());
			if (shipInfo == null)
			{
				int shipID = game.GameDatabase.InsertShip(fleetid, game.GameDatabase.GetDesignInfosForPlayer(fleetInfo.PlayerID).FirstOrDefault((DesignInfo x) => x.IsLoaCube()).ID, "Cube", (ShipParams)0, null, 0);
				shipInfo = game.GameDatabase.GetShipInfo(shipID, false);
			}
			foreach (ShipInfo current in list)
			{
				if (current.ID != shipInfo.ID && !(current.DesignInfo.GetRealShipClass() == RealShipClasses.Drone) && !(current.DesignInfo.GetRealShipClass() == RealShipClasses.BoardingPod) && !(current.DesignInfo.GetRealShipClass() == RealShipClasses.AssaultShuttle))
				{
					shipInfo.LoaCubes += StarFleet.GetShipLoaCubeValue(game, current.ID);
				}
			}
			foreach (ShipInfo current2 in list)
			{
				if (current2.ID != shipInfo.ID)
				{
					game.GameDatabase.RemoveShip(current2.ID);
				}
			}
			game.GameDatabase.UpdateShipLoaCubes(shipInfo.ID, shipInfo.LoaCubes);
			return shipInfo.ID;
		}
		public static IEnumerable<DesignInfo> GetDesignBuildOrderForComposition(GameSession game, int fleetid, LoaFleetComposition composition, MissionType mission_type = MissionType.NO_MISSION)
		{
			MissionInfo missionByFleetID = game.GameDatabase.GetMissionByFleetID(fleetid);
			if (missionByFleetID != null)
			{
				mission_type = missionByFleetID.Type;
			}
			List<DesignInfo> list = new List<DesignInfo>();
			foreach (LoaFleetShipDef current in composition.designs)
			{
				DesignInfo designInfo = game.GameDatabase.GetDesignInfo(current.DesignID);
				if (designInfo != null)
				{
					list.Add(designInfo);
				}
			}
			DesignInfo designInfo2 = null;
			bool flag = false;
			foreach (DesignInfo current2 in list)
			{
				if (current2.GetCommandPoints() > 0 && (designInfo2 == null || current2.GetCommandPoints() > designInfo2.GetCommandPoints() || (!flag && current2.isPrototyped)))
				{
					designInfo2 = current2;
					flag = current2.isPrototyped;
				}
			}
			if (designInfo2 != null)
			{
				list.Remove(designInfo2);
				list.Insert(0, designInfo2);
			}
			if (mission_type != MissionType.NO_MISSION)
			{
				List<DesignInfo> list2 = new List<DesignInfo>();
				if (mission_type == MissionType.COLONIZATION || mission_type == MissionType.SUPPORT || mission_type == MissionType.EVACUATE)
				{
					list2 = (
						from x in list
						where x.DesignSections.Any((DesignSectionInfo j) => j.ShipSectionAsset.ColonizationSpace > 0)
						select x).ToList<DesignInfo>();
				}
				if (mission_type == MissionType.CONSTRUCT_STN || mission_type == MissionType.UPGRADE_STN || mission_type == MissionType.SPECIAL_CONSTRUCT_STN)
				{
					list2 = (
						from x in list
						where x.DesignSections.Any((DesignSectionInfo j) => j.ShipSectionAsset.isConstructor)
						select x).ToList<DesignInfo>();
				}
				foreach (DesignInfo current3 in list2)
				{
					list.Remove(current3);
				}
				foreach (DesignInfo current4 in list2)
				{
					list.Insert(1, current4);
				}
			}
			return list;
		}
		public static LoaFleetComposition ObtainFleetComposition(GameSession game, FleetInfo fleetInfo, int? compositionid)
		{
			LoaFleetComposition loaFleetComposition = null;
			if (compositionid.HasValue)
			{
				loaFleetComposition = game.GameDatabase.GetLoaFleetCompositions().FirstOrDefault((LoaFleetComposition x) => x.ID == compositionid.Value);
			}
			if (!compositionid.HasValue)
			{
				PlayerInfo playerInfo = game.GameDatabase.GetPlayerInfo(fleetInfo.PlayerID);
				if (game.GetPlayerObject(playerInfo.ID).IsAI())
				{
					AIFleetInfo aifi = game.GameDatabase.GetAIFleetInfos(playerInfo.ID).FirstOrDefault((AIFleetInfo x) => x.FleetID == fleetInfo.ID);
					if (aifi != null)
					{
						List<LoaFleetShipDef> list = new List<LoaFleetShipDef>();
						Dictionary<int, int> fleetDesignsFromTemplate = game.GetFleetDesignsFromTemplate(game.GetPlayerObject(playerInfo.ID), aifi.FleetTemplate);
						foreach (int current in fleetDesignsFromTemplate.Keys)
						{
							for (int i = 0; i < fleetDesignsFromTemplate[current]; i++)
							{
								list.Add(new LoaFleetShipDef
								{
									DesignID = current
								});
							}
						}
						FleetTemplate fleetTemplate = game.AssetDatabase.FleetTemplates.First((FleetTemplate x) => x.Name == aifi.FleetTemplate);
						loaFleetComposition = new LoaFleetComposition();
						loaFleetComposition.Name = fleetTemplate.FleetName;
						loaFleetComposition.PlayerID = playerInfo.ID;
						loaFleetComposition.designs = list;
					}
				}
			}
			return loaFleetComposition;
		}
		public static void BuildFleetFromCompositionID(GameSession game, int fleetid, int? compositionid, MissionType missionType = MissionType.NO_MISSION)
		{
			FleetInfo fi = game.GameDatabase.GetFleetInfo(fleetid);
			if (StarFleet.IsGardenerFleet(game, fi))
			{
				return;
			}
			LoaFleetComposition loaFleetComposition = null;
			PlayerInfo playerInfo = game.GameDatabase.GetPlayerInfo(fi.PlayerID);
			if (!compositionid.HasValue)
			{
				if (!game.GetPlayerObject(playerInfo.ID).IsAI())
				{
					return;
				}
				AIFleetInfo aifi = game.GameDatabase.GetAIFleetInfos(playerInfo.ID).FirstOrDefault((AIFleetInfo x) => x.FleetID == fi.ID);
				if (aifi != null)
				{
					List<LoaFleetShipDef> list = new List<LoaFleetShipDef>();
					Dictionary<int, int> fleetDesignsFromTemplate = game.GetFleetDesignsFromTemplate(game.GetPlayerObject(playerInfo.ID), aifi.FleetTemplate);
					foreach (int current in fleetDesignsFromTemplate.Keys)
					{
						for (int i = 0; i < fleetDesignsFromTemplate[current]; i++)
						{
							list.Add(new LoaFleetShipDef
							{
								DesignID = current
							});
						}
					}
					FleetTemplate fleetTemplate = game.AssetDatabase.FleetTemplates.First((FleetTemplate x) => x.Name == aifi.FleetTemplate);
					loaFleetComposition = new LoaFleetComposition();
					loaFleetComposition.Name = fleetTemplate.FleetName;
					loaFleetComposition.PlayerID = playerInfo.ID;
					loaFleetComposition.designs = list;
				}
			}
			StarFleet.ConvertFleetIntoLoaCubes(game, fleetid);
			game.GameDatabase.GetShipInfoByFleetID(fleetid, true).ToList<ShipInfo>();
			ShipInfo shipInfo = game.GameDatabase.GetShipInfoByFleetID(fleetid, true).FirstOrDefault((ShipInfo x) => x.DesignInfo.IsLoaCube());
			if (shipInfo == null)
			{
				shipInfo = game.GameDatabase.GetShipInfo(StarFleet.ConvertFleetIntoLoaCubes(game, fleetid), false);
			}
			if (shipInfo == null)
			{
				return;
			}
			if (loaFleetComposition == null)
			{
				List<LoaFleetComposition> source = game.GameDatabase.GetLoaFleetCompositions().ToList<LoaFleetComposition>();
				if (!source.Any<LoaFleetComposition>())
				{
					return;
				}
				loaFleetComposition = source.FirstOrDefault((LoaFleetComposition x) => x.ID == compositionid);
			}
			if (loaFleetComposition == null)
			{
				return;
			}
			float num = (float)StarFleet.GetFleetLoaCubeValue(game, fleetid);
			List<DesignInfo> list2 = StarFleet.GetDesignBuildOrderForComposition(game, fleetid, loaFleetComposition, missionType).ToList<DesignInfo>();
			int num2 = 0;
			List<DesignInfo> list3 = (
				from X in list2
				where X.Class == ShipClass.BattleRider
				select X).ToList<DesignInfo>();
			DesignInfo designInfo = list2.FirstOrDefault((DesignInfo x) => x.GetCommandPoints() > 0);
			if (designInfo != null)
			{
				if (num >= (float)designInfo.GetPlayerProductionCost(game.GameDatabase, playerInfo.ID, !designInfo.isPrototyped, null))
				{
					num2 += designInfo.GetPlayerProductionCost(game.GameDatabase, playerInfo.ID, !designInfo.isPrototyped, null);
					int num3 = game.GameDatabase.InsertShip(fleetid, designInfo.ID, designInfo.Name, (ShipParams)0, null, 0);
					game.AddDefaultStartingRiders(fleetid, designInfo.ID, num3);
					list2.Remove(designInfo);
					List<CarrierWingData> list4 = RiderManager.GetDesignBattleriderWingData(game.App, designInfo).ToList<CarrierWingData>();
					using (List<CarrierWingData>.Enumerator enumerator2 = list4.GetEnumerator())
					{
						CarrierWingData wd;
						while (enumerator2.MoveNext())
						{
							wd = enumerator2.Current;
							List<DesignInfo> classriders = (
								from x in list3
								where StrategicAI.BattleRiderMountSet.GetMatchingTurretClass(x) == wd.Class
								select x).ToList<DesignInfo>();
							if (classriders.Any<DesignInfo>() && wd.SlotIndexes.Any<int>())
							{
								BattleRiderTypes SelectedType = (
									from x in classriders
									where classriders.Count((DesignInfo j) => j.ID == x.ID) >= wd.SlotIndexes.Count
									select x).First<DesignInfo>().GetMissionSectionAsset().BattleRiderType;
								DesignInfo designInfo2 = classriders.FirstOrDefault((DesignInfo x) => x.GetMissionSectionAsset().BattleRiderType == SelectedType && classriders.Count((DesignInfo j) => j.ID == x.ID) >= wd.SlotIndexes.Count);
								foreach (int current2 in wd.SlotIndexes)
								{
									if (designInfo2 != null && num >= (float)(designInfo2.GetPlayerProductionCost(game.GameDatabase, playerInfo.ID, !designInfo2.isPrototyped, null) + num2))
									{
										num2 += designInfo2.GetPlayerProductionCost(game.GameDatabase, playerInfo.ID, !designInfo2.isPrototyped, null);
										int num4 = game.GameDatabase.InsertShip(fleetid, designInfo2.ID, designInfo2.Name, (ShipParams)0, null, 0);
										game.AddDefaultStartingRiders(fleetid, designInfo2.ID, num4);
										game.GameDatabase.SetShipParent(num4, num3);
										game.GameDatabase.UpdateShipRiderIndex(num4, current2);
										list3.Remove(designInfo2);
									}
								}
							}
						}
						goto IL_59C;
					}
				}
				return;
			}
			IL_59C:
			foreach (DesignInfo current3 in list2)
			{
				if (current3.Class != ShipClass.BattleRider && !(current3.GetRealShipClass() == RealShipClasses.AssaultShuttle) && !(current3.GetRealShipClass() == RealShipClasses.Drone) && !(current3.GetRealShipClass() == RealShipClasses.EscapePod) && num >= (float)(current3.GetPlayerProductionCost(game.GameDatabase, playerInfo.ID, !current3.isPrototyped, null) + num2))
				{
					num2 += current3.GetPlayerProductionCost(game.GameDatabase, playerInfo.ID, !current3.isPrototyped, null);
					int num5 = game.GameDatabase.InsertShip(fleetid, current3.ID, current3.Name, (ShipParams)0, null, 0);
					game.AddDefaultStartingRiders(fleetid, current3.ID, num5);
					List<CarrierWingData> list5 = RiderManager.GetDesignBattleriderWingData(game.App, current3).ToList<CarrierWingData>();
					foreach (CarrierWingData wd in list5)
					{
						List<DesignInfo> list6 = (
							from x in list3
							where StrategicAI.BattleRiderMountSet.GetMatchingTurretClass(x) == wd.Class
							select x).ToList<DesignInfo>();
						if (list6.Any<DesignInfo>() && wd.SlotIndexes.Any<int>())
						{
							DesignInfo designInfo3 = App.GetSafeRandom().Choose(list6);
							foreach (int current4 in wd.SlotIndexes)
							{
								if (designInfo3 != null && num >= (float)(designInfo3.GetPlayerProductionCost(game.GameDatabase, playerInfo.ID, !designInfo3.isPrototyped, null) + num2))
								{
									num2 += designInfo3.GetPlayerProductionCost(game.GameDatabase, playerInfo.ID, !designInfo3.isPrototyped, null);
									int num6 = game.GameDatabase.InsertShip(fleetid, designInfo3.ID, designInfo3.Name, (ShipParams)0, null, 0);
									game.AddDefaultStartingRiders(fleetid, designInfo3.ID, num6);
									game.GameDatabase.SetShipParent(num6, num5);
									game.GameDatabase.UpdateShipRiderIndex(num6, current4);
									list3.Remove(designInfo3);
								}
							}
						}
					}
				}
			}
			shipInfo.LoaCubes = (int)num - num2;
			if (shipInfo.LoaCubes <= 0)
			{
				game.GameDatabase.RemoveShip(shipInfo.ID);
				return;
			}
			game.GameDatabase.UpdateShipLoaCubes(shipInfo.ID, shipInfo.LoaCubes);
		}
		public static void BuildFleetFromComposition(GameSession game, int fleetid, MissionType missionType = MissionType.NO_MISSION)
		{
			FleetInfo fleetInfo = game.GameDatabase.GetFleetInfo(fleetid);
			if (fleetInfo != null && !StarFleet.IsGardenerFleet(game, fleetInfo))
			{
				StarFleet.BuildFleetFromCompositionID(game, fleetid, fleetInfo.FleetConfigID, missionType);
			}
		}
	}
}
