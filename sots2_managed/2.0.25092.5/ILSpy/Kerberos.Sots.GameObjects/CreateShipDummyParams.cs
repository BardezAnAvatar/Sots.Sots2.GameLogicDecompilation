using Kerberos.Sots.Data;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace Kerberos.Sots.GameObjects
{
	internal class CreateShipDummyParams
	{
		public int ShipID;
		public string PreferredMount = "";
		public Faction ShipFaction;
		public IEnumerable<ShipSectionAsset> Sections = new ShipSectionAsset[0];
		public IEnumerable<LogicalWeapon> PreferredWeapons = new LogicalWeapon[0];
		public IEnumerable<WeaponAssignment> AssignedWeapons = new WeaponAssignment[0];
		public IEnumerable<LogicalModule> PreferredModules = new LogicalModule[0];
		public IEnumerable<ModuleAssignment> AssignedModules = new ModuleAssignment[0];
        public static CreateShipDummyParams ObtainShipDummyParams(App game, ShipInfo shipInfo)
        {
            IEnumerable<string> modules = from x in game.AssetDatabase.Modules select x.ModuleName;
            IEnumerable<string> weapons = from x in game.AssetDatabase.Weapons select x.Name;
            DesignInfo designInfo = shipInfo.DesignInfo;
            List<ShipSectionAsset> source = new List<ShipSectionAsset>();
            List<ModuleAssignment> list2 = new List<ModuleAssignment>();
            List<WeaponAssignment> list3 = new List<WeaponAssignment>();
            Func<ShipSectionAsset, bool> predicate = null;
            foreach (DesignSectionInfo sectionInfo in designInfo.DesignSections)
            {
                if (predicate == null)
                {
                    predicate = x => x.FileName == sectionInfo.FilePath;
                }
                ShipSectionAsset item = game.AssetDatabase.ShipSections.First<ShipSectionAsset>(predicate);
                source.Add(item);
                Func<WeaponBankInfo, bool> func = null;
                Func<LogicalWeapon, bool> func2 = null;
                foreach (LogicalBank bank in item.Banks)
                {
                    if (func == null)
                    {
                        func = x => x.BankGUID == bank.GUID;
                    }
                    WeaponBankInfo info2 = sectionInfo.WeaponBanks.FirstOrDefault<WeaponBankInfo>(func);
                    bool flag = false;
                    if ((info2 != null) && info2.WeaponID.HasValue)
                    {
                        string weaponName = Path.GetFileNameWithoutExtension(game.GameDatabase.GetWeaponAsset(info2.WeaponID.Value));
                        WeaponAssignment assignment2 = new WeaponAssignment
                        {
                            ModuleNode = "",
                            Bank = bank,
                            Weapon = game.AssetDatabase.Weapons.First<LogicalWeapon>(weapon => string.Equals(weapon.WeaponName, weaponName, StringComparison.InvariantCultureIgnoreCase)),
                            DesignID = ((info2 != null) && info2.DesignID.HasValue) ? info2.DesignID.Value : 0
                        };
                        int? filterMode = info2.FilterMode;
                        assignment2.InitialTargetFilter = new int?(filterMode.HasValue ? filterMode.GetValueOrDefault() : 0);
                        int? firingMode = info2.FiringMode;
                        assignment2.InitialFireMode = new int?(firingMode.HasValue ? firingMode.GetValueOrDefault() : 0);
                        WeaponAssignment assignment = assignment2;
                        list3.Add(assignment);
                        flag = true;
                    }
                    if (!flag && !string.IsNullOrEmpty(bank.DefaultWeaponName))
                    {
                        WeaponAssignment assignment4 = new WeaponAssignment
                        {
                            ModuleNode = "",
                            Bank = bank
                        };
                        if (func2 == null)
                        {
                            func2 = weapon => string.Equals(weapon.WeaponName, bank.DefaultWeaponName, StringComparison.InvariantCultureIgnoreCase);
                        }
                        assignment4.Weapon = game.AssetDatabase.Weapons.First<LogicalWeapon>(func2);
                        assignment4.DesignID = ((info2 != null) && info2.DesignID.HasValue) ? info2.DesignID.Value : 0;
                        WeaponAssignment assignment3 = assignment4;
                        list3.Add(assignment3);
                        flag = true;
                    }
                }
                Func<DesignModuleInfo, bool> func3 = null;
                foreach (LogicalModuleMount sectionModule in item.Modules)
                {
                    string path;
                    if (func3 == null)
                    {
                        func3 = x => x.MountNodeName == sectionModule.NodeName;
                    }
                    DesignModuleInfo info3 = sectionInfo.Modules.FirstOrDefault<DesignModuleInfo>(func3);
                    if (info3 != null)
                    {
                        path = game.GameDatabase.GetModuleAsset(info3.ModuleID);
                        LogicalModule module = game.AssetDatabase.Modules.FirstOrDefault<LogicalModule>(x => x.ModulePath == path);
                        ModuleAssignment assignment7 = new ModuleAssignment
                        {
                            ModuleMount = sectionModule,
                            Module = module
                        };
                        list2.Add(assignment7);
                        if (info3.WeaponID.HasValue)
                        {
                            string weaponPath = game.GameDatabase.GetWeaponAsset(info3.WeaponID.Value);
                            WeaponAssignment assignment5 = new WeaponAssignment
                            {
                                ModuleNode = info3.MountNodeName,
                                Bank = module.Banks[0],
                                Weapon = game.AssetDatabase.Weapons.First<LogicalWeapon>(x => x.FileName == weaponPath),
                                DesignID = 0
                            };
                            list3.Add(assignment5);
                        }
                    }
                }
            }
            ShipSectionAsset missionSection = source.FirstOrDefault<ShipSectionAsset>(x => x.Type == ShipSectionType.Mission);
            Faction faction = game.AssetDatabase.Factions.First<Faction>(x => missionSection.Faction == x.Name);
            Player playerObject = game.Game.GetPlayerObject(designInfo.PlayerID);
            Subfaction subfaction1 = faction.Subfactions[Math.Min(playerObject.SubfactionIndex, faction.Subfactions.Length - 1)];
            return new CreateShipDummyParams
            {
                ShipID = shipInfo.ID,
                PreferredMount = Ship.GetPreferredMount(game, playerObject, faction, source),
                ShipFaction = faction,
                Sections = source.ToArray(),
                AssignedModules = list2.ToArray(),
                PreferredModules = from x in game.AssetDatabase.Modules
                                   where modules.Contains<string>(x.ModuleName)
                                   select x,
                AssignedWeapons = list3.ToArray(),
                PreferredWeapons = from x in game.AssetDatabase.Weapons
                                   where weapons.Contains<string>(x.Name)
                                   select x
            };
        }
    }
}
