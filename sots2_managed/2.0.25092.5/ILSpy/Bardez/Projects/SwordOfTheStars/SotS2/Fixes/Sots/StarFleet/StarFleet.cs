using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Kerberos.Sots;
using Kerberos.Sots.Data;
using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.UI;

using Bardez.Projects.SwordOfTheStars.SotS2.Fixes.Sots.Additions;
using Bardez.Projects.SwordOfTheStars.SotS2.Fixes.Sots.Data;
using Bardez.Projects.SwordOfTheStars.SotS2.Utility;

using Original = Kerberos.Sots.StarFleet;
using PerformanceData = Bardez.Projects.SwordOfTheStars.SotS2.Fixes.Sots.Data;

namespace Bardez.Projects.SwordOfTheStars.SotS2.Fixes.Sots.StarFleet
{
    /// <summary>Contains performance fixes for Kerberos.Sots.StarFleet.StarFleet</summary>
	public static class StarFleet
    {
        #region Kerberos members exposed via reflection
        private static void Warn(string message)
        {
            MethodInfo mi = ReflectionHelper.PrivateStaticMethod<Kerberos.Sots.StarFleet.StarFleet>("Warn");
            mi.Invoke(null, new Object[] { message });
        }
        #endregion


        /// <summary>Lists all constructor ship section assets' file names</summary>
        /// <param name="game">Game Session to utilize</param>
        /// <returns>The collection of all constructor ship section assets' file names</returns>
        internal static IList<String> GetConstructionSectionAssetNames(GameSession game)
        {
            return game.AssetDatabase.ShipSections.Where(section => section != null && section.isConstructor).Select(section => section.FileName).ToList();
        }

        /// <summary>Lists all colonization ship section assets' file names</summary>
        /// <param name="game">Game Session to utilize</param>
        /// <returns>The collection of all colonization ship section assets' file names</returns>
        internal static IList<String> GetColonizationSectionAssetNames(GameSession game)
        {
            return game.AssetDatabase.ShipSections.Where(section => section != null && section.ColonizationSpace > 0).Select(section => section.FileName).ToList();
        }

        /// <summary>Gets the current health and the maximum health for a ship</summary>
        /// <param name="game">Game Session to utilize</param>
        /// <param name="design">Design information to query</param>
        /// <param name="shipid">Unique ID of the shio to query current health of</param>
        /// <param name="shipSectionInstances">Pre-indexed collection of ship sections</param>
        /// <returns>An array containing the current and maximum health of the ship</returns>
        internal static Int32[] GetHealthAndHealthMax(GameSession game, DesignInfo design, int shipid, Dictionary<Int32, IList<SectionInstanceInfo>> shipSectionInstances)
        {
            int num = 0;
            int num2 = 0;
            int num3 = 0;
            int num4 = 0;
            //List<SectionInstanceInfo> source = game.GameDatabase.GetShipSectionInstances(shipid).ToList<SectionInstanceInfo>();
            IList<SectionInstanceInfo> source = shipSectionInstances[shipid];
            if (source != null)
                for (int i = 0; i < design.DesignSections.Count<DesignSectionInfo>(); i++)
                {
                    SectionInstanceInfo info = source.FirstOrDefault<SectionInstanceInfo>(x => x.SectionID == design.DesignSections[i].ID);
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

        /// <summary>Repairs a ship</summary>
        /// <param name="app">App to use to update the ship</param>
        /// <param name="ship">Ship to repair</param>
        /// <param name="points">Repair points used to repair the ship</param>
        /// <param name="shipSectionInstances">Pre-indexed collection of ship sections</param>
        internal static void RepairShip(App app, ShipInfo ship, Int32 points, Dictionary<Int32, IList<SectionInstanceInfo>> shipSectionInstances)
        {
            //List<SectionInstanceInfo> source = app.GameDatabase.GetShipSectionInstances(ship.ID).ToList<SectionInstanceInfo>();
            IList<SectionInstanceInfo> source = shipSectionInstances[ship.ID];
            if (source == null)
                source = new List<SectionInstanceInfo>();

            List<DesignSectionInfo> sections = app.GameDatabase.GetShipInfo(ship.ID, true).DesignInfo.DesignSections.ToList<DesignSectionInfo>();
            List<int> list2 = new List<int>();
            int item = 0;
            int num2 = 0;
            int num3 = 0;
            int thingsToRepair = source.Count * 5;
            Func<SectionInstanceInfo, bool> predicate = null;
            for (int j = 0; j < sections.Count; j++)
            {
                if (predicate == null)
                {
                    predicate = x => x.SectionID == sections[j].ID;
                }
                SectionInstanceInfo info = source.First<SectionInstanceInfo>(predicate);
                List<ModuleInstanceInfo> list3 = app.GameDatabase.GetModuleInstances(info.ID).ToList<ModuleInstanceInfo>();
                thingsToRepair += list3.Count;
                thingsToRepair += app.GameDatabase.GetWeaponInstances(info.ID).ToList<WeaponInstanceInfo>().Count;
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
            thingsToRepair += num5;
            if (thingsToRepair <= 0)
            {
                Warn("StarFleet.RepairShip: thingsToRepair <= 0");
            }
            else
            {
                int pointsPerSection = points / sections.Count;
                if (pointsPerSection == 0)
                {
                    pointsPerSection = points;
                }
                int num10 = Math.Min(pointsPerSection + (3 - (pointsPerSection % 3)), points);
                int num11 = 0;
                if (pointsPerSection <= 0)
                {
                    Warn("StarFleet.RepairShip: pointsPerSection <= 0");
                }
                else
                {
                    int num12 = Math.Max(50, points / pointsPerSection);
                    while (((points > 0) && (num11 != thingsToRepair)) && (num12 > 0))
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
                                if (num15 > pointsPerSection)
                                {
                                    num15 = pointsPerSection;
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
                                        if (num15 > pointsPerSection)
                                        {
                                            num15 = pointsPerSection;
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
                                        if (num15 > pointsPerSection)
                                        {
                                            num15 = pointsPerSection;
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

        /// <summary>Populates a Loa Fleet based on specified composition</summary>
        /// <param name="game">Game Session to utilize</param>
        /// <param name="fleetid">Fleet being composed</param>
        /// <param name="compositionid">Unique ID of the composition being used to populate the fleet</param>
        /// <param name="missionType">Mission type of the fleet, defaulted to none</param>
        internal static void BuildFleetFromCompositionID(GameSession game, int fleetid, int? compositionid, Original.MissionType missionType = Original.MissionType.NO_MISSION)
        {
            FleetInfo fi = game.GameDatabase.GetFleetInfo(fleetid);
            if (Original.StarFleet.IsGardenerFleet(game, fi))
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
            Original.StarFleet.ConvertFleetIntoLoaCubes(game, fleetid);
            game.GameDatabase.GetShipInfoByFleetID(fleetid, true).ToList<ShipInfo>();
            ShipInfo loaCubesShipInfo = game.GameDatabase.GetShipInfoByFleetID(fleetid, true).FirstOrDefault((ShipInfo x) => x.DesignInfo.IsLoaCube());
            if (loaCubesShipInfo == null)
            {
                loaCubesShipInfo = game.GameDatabase.GetShipInfo(Original.StarFleet.ConvertFleetIntoLoaCubes(game, fleetid), false);
            }
            if (loaCubesShipInfo == null)
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


            Int32 cubeValue = Original.StarFleet.GetFleetLoaCubeValue(game, fleetid);
            List<DesignInfo> compositionDesigns = Original.StarFleet.GetDesignBuildOrderForComposition(game, fleetid, loaFleetComposition, missionType).ToList<DesignInfo>();
            int productionCost = 0;
            List<DesignInfo> battleRiders = compositionDesigns.Where(X => X.Class == ShipClass.BattleRider).ToList();
            DesignInfo designInfo = compositionDesigns.FirstOrDefault((DesignInfo x) => x.GetCommandPoints() > 0);
            if (designInfo != null)
            {
                /************************************************************************************************************************
                *   This is where the major difference crops up. Adding ships is very intensive, since the weapon insertion syncs after *
                *   every insert. I think it is logical to collect up all the ships and performing bulk inserts on them. Since the      *
                *   post-insert behavior for ships appears to be identical, I should be able to start at the ship level and proceed     *
                *   to more granular levels from there. Due to this method's complexity, I don't know if the fleet processing could     *
                *   benefit from this approach.                                                                                         *
                ************************************************************************************************************************/
                List<ShipInsertionParameters> shipsToInsert = new List<ShipInsertionParameters>();

                if (cubeValue >= designInfo.GetPlayerProductionCost(game.GameDatabase, playerInfo.ID, !designInfo.isPrototyped, null))
                {
                    productionCost += designInfo.GetPlayerProductionCost(game.GameDatabase, playerInfo.ID, !designInfo.isPrototyped, null);

                    //Changing the ship insertion to a bulk insert.
                    ShipInsertionParameters commandShip = new ShipInsertionParameters(fleetid, designInfo.ID, designInfo.Name, (ShipParams)0, null, 0, null, -1);   //no parent ship that it rides
                    shipsToInsert.Add(commandShip);
                    //int commandShipId = game.GameDatabase.InsertShip(fleetid, designInfo.ID, designInfo.Name, (ShipParams)0, null, 0);
                    //game.AddDefaultStartingRiders(fleetid, designInfo.ID, commandShipId);

                    compositionDesigns.Remove(designInfo);

                    //Pull the battle rider selection out into a Func<> generic
                    Func<List<DesignInfo>, CarrierWingData, DesignInfo> riderSelection = (List<DesignInfo> riders, CarrierWingData cwd) =>
                    {
                        BattleRiderTypes SelectedType = riders.Where(x => riders.Count((DesignInfo j) => j.ID == x.ID) >= cwd.SlotIndexes.Count).First().GetMissionSectionAsset().BattleRiderType;
                        DesignInfo rider = riders.FirstOrDefault((DesignInfo x) => x.GetMissionSectionAsset().BattleRiderType == SelectedType && riders.Count((DesignInfo j) => j.ID == x.ID) >= cwd.SlotIndexes.Count);
                        return rider;
                    };

                    //Moved the battle rider population into a private method
                    productionCost = StarFleet.BuildFleetFromCompositionID_ProcessBattleRiders(game, fleetid, playerInfo, cubeValue, productionCost, battleRiders, designInfo, commandShip, riderSelection, shipsToInsert);
                }

                foreach (DesignInfo shipDesign in compositionDesigns)   //ships other than the first command ship
                {
                    RealShipClasses? realShip = shipDesign.GetRealShipClass();
                    if (shipDesign.Class != ShipClass.BattleRider && realShip != RealShipClasses.AssaultShuttle && realShip != RealShipClasses.Drone && realShip != RealShipClasses.EscapePod)
                    {
                        if (cubeValue >= (shipDesign.GetPlayerProductionCost(game.GameDatabase, playerInfo.ID, !shipDesign.isPrototyped, null) + productionCost))
                        {
                            productionCost += shipDesign.GetPlayerProductionCost(game.GameDatabase, playerInfo.ID, !shipDesign.isPrototyped, null);

                            //Changing the ship insertion to a bulk insert.
                            ShipInsertionParameters nonRiderShip = new ShipInsertionParameters(fleetid, shipDesign.ID, shipDesign.Name, (ShipParams)0, null, 0, null, -1);   //no parent ship that it rides
                            shipsToInsert.Add(nonRiderShip);
                            //int num5 = game.GameDatabase.InsertShip(fleetid, shipDesign.ID, shipDesign.Name, (ShipParams)0, null, 0);
                            //game.AddDefaultStartingRiders(fleetid, shipDesign.ID, num5);

                            //Pull the battle rider selection out into a Func<> generic
                            Func<List<DesignInfo>, CarrierWingData, DesignInfo> remainderRiderSelection = (List<DesignInfo> riders, CarrierWingData cwd) =>
                            {
                                DesignInfo rider = App.GetSafeRandom().Choose(riders);
                                return rider;
                            };

                            //Moved the battle rider population into a private method
                            productionCost = StarFleet.BuildFleetFromCompositionID_ProcessBattleRiders(game, fleetid, playerInfo, cubeValue, productionCost, battleRiders, designInfo, nonRiderShip, remainderRiderSelection, shipsToInsert);
                        }
                    }
                }

                //Perform the bulk insert
                PerformanceData.GameDatabase gdb = new PerformanceData.GameDatabase(game.GameDatabase);
                gdb.BulkInsertShips(shipsToInsert);


                //process the actual ship insertions and their result
                foreach (ShipInsertionParameters ship in shipsToInsert)
                {
                    Int32 shipId = ship.ShipInfo.ID;

                    game.AddDefaultStartingRiders(fleetid, ship.DesignID, shipId);

                    if (ship.ParentShip != null)    //non-rider ships don't do the following
                    {
                        game.GameDatabase.SetShipParent(shipId, ship.ParentShip.ShipInfo.ID);
                        game.GameDatabase.UpdateShipRiderIndex(shipId, ship.SlotIndex);
                    }
                }

                //clean up
                loaCubesShipInfo.LoaCubes = cubeValue - productionCost;
                if (loaCubesShipInfo.LoaCubes <= 0)
                {
                    game.GameDatabase.RemoveShip(loaCubesShipInfo.ID);
                    return;
                }
                game.GameDatabase.UpdateShipLoaCubes(loaCubesShipInfo.ID, loaCubesShipInfo.LoaCubes);
            }
        }

        /// <summary>Sub-function of BuildFleetFromCompositionID extracted that attaches battle riders to a potential carrier</summary>
        /// <param name="game">Game Session to utilize</param>
        /// <param name="fleetid">Fleet being composed</param>
        /// <param name="playerInfo">Owning Player information</param>
        /// <param name="cubeValue">Value of cubes</param>
        /// <param name="productionCost">Cost to produce the ships</param>
        /// <param name="battleRiders">Collection of battle ridets to populate carrier ship with</param>
        /// <param name="parentShipDesign">Ship design of the parent ship</param>
        /// <param name="motherShip">Parent ship possibly carrying riders</param>
        /// <param name="riderSelection">Function used to select battle riders</param>
        /// <param name="shipsToInsert">Collection of ships to insert into the database to add to</param>
        /// <returns>The updated production cost</returns>
        private static Int32 BuildFleetFromCompositionID_ProcessBattleRiders(GameSession game, Int32 fleetid, PlayerInfo playerInfo, Int32 cubeValue, Int32 productionCost, List<DesignInfo> battleRiders, DesignInfo parentShipDesign, ShipInsertionParameters motherShip, Func<List<DesignInfo>, CarrierWingData, DesignInfo> riderSelection, List<ShipInsertionParameters> shipsToInsert)
        {
            foreach (CarrierWingData cwd in RiderManager.GetDesignBattleriderWingData(game.App, parentShipDesign))
            {
                List<DesignInfo> classRiders = battleRiders.Where(x => StrategicAI.BattleRiderMountSet.GetMatchingTurretClass(x) == cwd.Class).ToList();
                if (classRiders.Any<DesignInfo>() && cwd.SlotIndexes.Any<int>())
                {
                    DesignInfo riderDesign = riderSelection(battleRiders, cwd);  //this is different between the command ship and other ship cases, so abstracted into a passed-in Func<>
                    foreach (int slot in cwd.SlotIndexes)
                    {
                        if (riderDesign != null && cubeValue >= (riderDesign.GetPlayerProductionCost(game.GameDatabase, playerInfo.ID, !riderDesign.isPrototyped, null) + productionCost))
                        {
                            productionCost += riderDesign.GetPlayerProductionCost(game.GameDatabase, playerInfo.ID, !riderDesign.isPrototyped, null);

                            //Changing the ship insertion to a bulk insert.
                            ShipInsertionParameters riderInsertion = new ShipInsertionParameters(fleetid, riderDesign.ID, riderDesign.Name, (ShipParams)0, null, 0, motherShip, slot);
                            shipsToInsert.Add(riderInsertion);
                            //int num6 = game.GameDatabase.InsertShip(fleetid, riderDesign.ID, riderDesign.Name, (ShipParams)0, null, 0);

                            /************************************************************************
                            *   These are abstracted out to happen after the inserts are processed  *
                            ************************************************************************/
                            //game.AddDefaultStartingRiders(fleetid, riderDesign.ID, num6);
                            //game.GameDatabase.SetShipParent(num6, num5);
                            //game.GameDatabase.UpdateShipRiderIndex(num6, slot);

                            battleRiders.Remove(riderDesign);
                        }
                    }
                }
            }

            return productionCost;
        }
	}
}