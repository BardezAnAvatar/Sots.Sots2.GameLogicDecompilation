using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Reflection;

using Kerberos.Sots;
using Kerberos.Sots.Data;
using Kerberos.Sots.Data.ModuleFramework;
using Kerberos.Sots.Data.ShipFramework;
using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Data.SQLite;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;

using Bardez.Projects.SwordOfTheStars.SotS2.Fixes.Sots.Additions;
using Bardez.Projects.SwordOfTheStars.SotS2.Utility;

using Original = Kerberos.Sots.Data;

namespace Bardez.Projects.SwordOfTheStars.SotS2.Fixes.Sots.Data
{
    /// <summary>This class contains code fixes to general SotS2 logic in namespace Kerberos.Sots.Data.GameDatabase</summary>
	internal class GameDatabase
    {
        #region Fields
        /// <summary>Kerberos GameDatabase to extend</summary>
        protected Original.GameDatabase BaseInstance;
        #endregion


        #region Kerberos exposed members
        #region Fields
        protected DataObjectCache _dom
        {
            get { return ReflectionHelper.PrivateField<Original.GameDatabase, DataObjectCache>(this.BaseInstance, "_dom"); }
        }

        protected AssetDatabase assetdb
        {
            get { return ReflectionHelper.PrivateField<Original.GameDatabase, AssetDatabase>(this.BaseInstance, "assetdb"); }
        }
        #endregion


        #region Properties
        public AssetDatabase AssetDatabase
        {
            get { return this.BaseInstance.AssetDatabase; }
        }
        #endregion


        #region Methods
        protected void AddNumShipsBuiltFromDesign(int designID, int count)
        {
            MethodInfo mi = ReflectionHelper.PrivateMethod<Kerberos.Sots.Data.GameDatabase>("AddNumShipsBuiltFromDesign");
            mi.Invoke(this.BaseInstance, new Object[] { designID, count });
        }

        protected void TryAddFleetShip(int shipId, int fleetId)
        {
            MethodInfo mi = ReflectionHelper.PrivateMethod<Kerberos.Sots.Data.GameDatabase>("TryAddFleetShip");
            mi.Invoke(this.BaseInstance, new Object[] { shipId, fleetId });
        }

        protected FleetInfo GetFleetInfo(Row row)
        {
            MethodInfo mi = ReflectionHelper.PrivateMethod<Kerberos.Sots.Data.GameDatabase>("GetFleetInfo");
            Object result = mi.Invoke(this.BaseInstance, new Object[] { row });
            return result as FleetInfo;
        }

        protected void AddCachedShipNameReference(int playerId, string shipName)
        {
            MethodInfo mi = ReflectionHelper.PrivateMethod<Kerberos.Sots.Data.GameDatabase>("AddCachedShipNameReference");
            mi.Invoke(this.BaseInstance, new Object[] { playerId, shipName });
        }

        public FleetInfo GetFleetInfo(int fleetID)
        {
            return this.BaseInstance.GetFleetInfo(fleetID);
        }

        public DesignInfo GetDesignInfo(int designID)
        {
            return this.BaseInstance.GetDesignInfo(designID);
        }

        public int GetPlayerFactionID(int playerId)
        {
            return this.BaseInstance.GetPlayerFactionID(playerId);
        }

        public int GetNumShipsBuiltFromDesign(int designID)
        {
            return this.BaseInstance.GetNumShipsBuiltFromDesign(designID);
        }

        public string GetDefaultShipName(int designID, int serial)
        {
            return this.BaseInstance.GetDefaultShipName(designID, serial);
        }

        public string ResolveNewShipName(int playerId, string name)
        {
            return this.BaseInstance.ResolveNewShipName(playerId, name);
        }

        public int GetTurnCount()
        {
            return this.BaseInstance.GetTurnCount();
        }

        public IEnumerable<SectionEnumerations.DesignAttribute> GetDesignAttributesForDesign(int id)
        {
            return this.BaseInstance.GetDesignAttributesForDesign(id);
        }

        public string GetTechFileID(int techId)
        {
            return this.BaseInstance.GetTechFileID(techId);
        }

        public string GetModuleAsset(int moduleId)
        {
            return this.BaseInstance.GetModuleAsset(moduleId);
        }

        public string GetWeaponAsset(int weaponId)
        {
            return this.BaseInstance.GetWeaponAsset(weaponId);
        }
        #endregion
        #endregion


        #region Construction
        /// <summary>Definition constructor</summary>
        /// <param name="instance">Kerberos instance to extend</param>
        public GameDatabase(Original.GameDatabase instance)
        {
            this.BaseInstance = instance;
        }
        #endregion


        /// <summary>Returns the specified system's strategic sensor range for the specified player</summary>
        /// <param name="db">Game database to query</param>
        /// <param name="systemid">Unique ID of the system to check on</param>
        /// <param name="playerid">Unique ID of the player to check on</param>
        /// <param name="allies">Pre-retrieved list of a player's allies.</param>
        /// <returns>The sensor range of the player in the given system</returns>
        public static Single GetSystemStratSensorRange(Kerberos.Sots.Data.GameDatabase db, Int32 systemid, Int32 playerid, IList<PlayerInfo> allies)
        {
            Single sensorRange = 0F;

            DataObjectCache.SystemPlayerID key = new DataObjectCache.SystemPlayerID
            {
                SystemID = systemid,
                PlayerID = playerid
            };
            
            //HACK: use reflection to access a private type.
            DataObjectCache cache = ReflectionHelper.PrivateField<Kerberos.Sots.Data.GameDatabase, DataObjectCache>(db, "_dom");

            if (!cache.CachedSystemStratSensorRanges.TryGetValue(key, out sensorRange))
            {
                //if any ally has a colony in the system, range is 5
                foreach (ColonyInfo colony in db.GetColonyInfosForSystem(systemid))
                {
                    if (allies.Any((PlayerInfo x) => x.ID == colony.PlayerID))
                        sensorRange = 5f;
                }

                //if any ally has a station in the system, use its range
                foreach (PlayerInfo current in allies)
                {
                    foreach (StationInfo current2 in db.GetStationForSystemAndPlayer(systemid, current.ID))
                        sensorRange = Math.Max(sensorRange, db.GetStationStratSensorRange(current2));
                }

                foreach (PlayerInfo current in allies)
                {
                    foreach (FleetInfo fleet in db.GetFleetsByPlayerAndSystem(current.ID, systemid, FleetType.FL_ALL))
                    {
                        sensorRange = Math.Max(sensorRange, GameSession.GetFleetSensorRange(db.AssetDatabase, db, fleet.ID));
                    }
                }
                cache.CachedSystemStratSensorRanges.Add(key, sensorRange);
            }

            return sensorRange;
        }

        /// <summary>Retrieves ship designs from the save file and returns them</summary>
        /// <param name="conn">SQLiteConnection to execute</param>
        /// <param name="assets">Asset Database to probe</param>
        /// <returns>The collection of saved ship designs</returns>
        public static Dictionary<Int32, DesignInfo> RetrieveSavedShipDesigns(SQLiteConnection conn, AssetDatabase assets)
        {
            //This method populates the universe of ship designs for the entire save game. Based on this,
            //  it needs to retrieve all designs, design secions, design modules, weapon bank infos and techs for each section.
            //  To do this, it is MUCH faster to alleviate the data access layer and to simply retrieve all techs,
            //  all weapon banks, all modules


            //Originally, the output was a set of lists. I watched performance after the refactoring and
            //  (while greatly improved) I noticed that the original replacement Linq filters were still getting hit hard.
            //  If I pre-group items that they are matched on, I could save time. Creating dictionaries
            //  speeds this up remarkably


            //Retrieve data in order of most granular to least granular; bottom request (psionics, techs, weapons) up (to design sections then designs)
            Dictionary<Int32, List<Int32>> designSectionTechs = GameDatabase.RetrieveDesignSectionTechs(conn);
            Dictionary<Int32, List<ModulePsionicInfo>> modulePsionicInfos = GameDatabase.RetrieveModulePsionicInfos(conn);
            Dictionary<Int32, List<WeaponBankInfo>> weaponBankInfos = GameDatabase.RetieveWeaponBankInfos(conn);
            Dictionary<Int32, List<DesignModuleInfo>> designModules = GameDatabase.RetrieveDesignModules(conn, modulePsionicInfos);
            Dictionary<Int32, List<DesignSectionInfo>> designSections = GameDatabase.RetrieveDesignSections(conn, assets, weaponBankInfos, designSectionTechs, designModules);
            List<DesignInfo> designs = GameDatabase.RetreiveDesigns(conn, designSections);

            //Create a dictionary for the Designs based on thier own IDs
            Dictionary<Int32, DesignInfo> output = new Dictionary<Int32, DesignInfo>();
            foreach (DesignInfo design in designs)
                output[design.ID] = design;

            return output;
        }

        /// <summary>Groups Ship Sections from the GameDatabase's cache by the ship ID</summary>
        /// <param name="db">Game database to query</param>
        /// <returns>The collection of ship sections (whose ship ID is non-null) grouped by the Ship ID</returns>
        public static Dictionary<Int32, List<SectionInstanceInfo>> GroupCachedShipSectionsByShip(Kerberos.Sots.Data.GameDatabase db)
        {
            //HACK: use reflection to access a private type.
            DataObjectCache cache = null;
            FieldInfo dom = typeof(Kerberos.Sots.Data.GameDatabase).GetField("_dom", BindingFlags.NonPublic | BindingFlags.Instance);
            if (dom == null)
                throw new NullReferenceException("Could not retrieve the \"_dom\" FieldInfo for the Kerberos GameDatabase.");
            else
                cache = dom.GetValue(db) as DataObjectCache;

            Dictionary<Int32, List<SectionInstanceInfo>> sections = new Dictionary<Int32, List<SectionInstanceInfo>>();

            foreach (SectionInstanceInfo section in cache.section_instances.Values)
            {
                if (section != null && section.ShipID != null)
                {
                    Int32 shipId = section.ShipID.Value;
                    if (!sections.ContainsKey(shipId))
                        sections[shipId] = new List<SectionInstanceInfo>();

                    sections[shipId].Add(section);
                }
            }

            return sections;
        }

        /// <summary>Looks up the command point bonuses associated with their bonus techs that the specified player has unlocked</summary>
        /// <param name="db">Game database to query</param>
        /// <param name="playerId">Player upon which to check command point bonuses for</param>
        /// <returns>The total command point bonus unlocked by the player</returns>
        public static Int32 GetCommandPointBonus(Kerberos.Sots.Data.GameDatabase db, Int32 playerId)
        {
            Int32 cpBonus = 0;

            if (db.PlayerHasTech(playerId, "CCC_Combat_Algorithms"))
                cpBonus += db.AssetDatabase.GetTechBonus<Int32>("CCC_Combat_Algorithms", "commandpoints");

            if (db.PlayerHasTech(playerId, "CCC_Flag_Central_Command"))
                cpBonus += db.AssetDatabase.GetTechBonus<Int32>("CCC_Flag_Central_Command", "commandpoints");

            if (db.PlayerHasTech(playerId, "CCC_Holo-Tactics"))
                cpBonus += db.AssetDatabase.GetTechBonus<Int32>("CCC_Holo-Tactics", "commandpoints");

            if (db.PlayerHasTech(playerId, "PSI_Warmind"))
                cpBonus += db.AssetDatabase.GetTechBonus<Int32>("PSI_Warmind", "commandpoints");

            return cpBonus;
        }

        /// <summary>Gets the total number of command points provided by the ship design</summary>
        /// <param name="db">Game database to query</param>
        /// <param name="designId">Ship design to query upon</param>
        /// <param name="researchedCommandPointBonuses"></param>
        /// <returns>The total amount of command points provided by this ship design</returns>
        public static Int32 GetDesignCommandPointQuota(Kerberos.Sots.Data.GameDatabase db, Int32 designId, Int32 researchedCommandPointBonuses)
        {
            Int32 commandPointsTotal = 0;

            Int32 commandShipCmdPoints = 0;
            DesignInfo designInfo = db.GetDesignInfo(designId);
            Int32 commandPointsBonus = researchedCommandPointBonuses;

            if (designInfo != null)
            {
                DesignSectionInfo[] designSections = designInfo.DesignSections;
                for (int i = 0; i < designSections.Length; i++)
                {
                    DesignSectionInfo designSectionInfo = designSections[i];
                    foreach (DesignModuleInfo module in designSectionInfo.Modules)
                    {
                        if (module.StationModuleType == ModuleEnums.StationModuleType.Command)
                            commandPointsBonus++;
                    }

                    //Take the largest amount of CP that any given ship provides (cruiser, dreadnaught, leviathan)
                    commandShipCmdPoints = Math.Max(commandShipCmdPoints, db.AssetDatabase.GetShipSectionAsset(designSectionInfo.FilePath).CommandPoints);
                }
            }

            if (commandShipCmdPoints != 0)  //make sure that there is in fact a command ship around
                commandPointsTotal = commandShipCmdPoints + commandPointsBonus;

            return commandPointsTotal;
        }

        /// <summary>Gets the designs for a given player matching the asset paths given</summary>
        /// <param name="db">Game database to query</param>
        /// <param name="playerId">Unique ID of the player to query on</param>
        /// <param name="assetNames">Names of the assets to locate</param>
        /// <returns>The collection of designs matching the input criteria</returns>
        public static List<Int32> GetDesignsMatchingAssetsForPlayer(Kerberos.Sots.Data.GameDatabase db, Int32 playerId, IList<String> assetNames)
        {
            //HACK: use reflection to access a private type.
            SQLiteConnection dbConn = null;
            FieldInfo dom = typeof(Kerberos.Sots.Data.GameDatabase).GetField("db", BindingFlags.NonPublic | BindingFlags.Instance);
            if (dom == null)
                throw new NullReferenceException("Could not retrieve the \"db\" FieldInfo for the Kerberos GameDatabase.");
            else
                dbConn = dom.GetValue(db) as SQLiteConnection;
            
            //I suppose there is some risk of SQL injection or something if the file names are completely bizarre. I don't belive it would be the case, however.
            IEnumerable<String> matchingAssets = assetNames.Select(s => String.Concat("'", s, "'"));
            String assetPaths = String.Join(",", matchingAssets);
            String query = String.Format(Queries.GetDesignsForPlayerMatchingAssets, playerId, assetPaths);

            List<Int32> designIds = new List<Int32>();

            Table table = dbConn.ExecuteTableQuery(query, false);
            foreach (Row row in table)
                designIds.Add(Int32.Parse(row[0]));

            return designIds;
        }

        /// <summary>Exposes the entire collection of SectionInstanceInfos, indexed by their ship IDs</summary>
        /// <param name="db">Kerberos GameDatabase to query</param>
        /// <returns>Entire collection of SectionInstanceInfos, indexed by their ship IDs</returns>
        /// <remarks>
        ///     Used to speed up the end of turn processing.
        ///     This will drop ship section instances that have no ship ID, but this should be OK;
        ///     the other retrieval methods filter on a non-nullable Ship ID, so this
        ///     should retain functionality.
        /// </remarks>
        public static Dictionary<Int32, IList<SectionInstanceInfo>> GetShipSectionInstances(Kerberos.Sots.Data.GameDatabase db)
        {
            //HACK: use reflection to access a private type.
            DataObjectCache cache = ReflectionHelper.PrivateField<Kerberos.Sots.Data.GameDatabase, DataObjectCache>(db, "_dom");

            Dictionary<Int32, IList<SectionInstanceInfo>> indexedShipSectionInstances = new Dictionary<Int32, IList<SectionInstanceInfo>>();

            foreach (SectionInstanceInfo section in cache.section_instances.Values)
            {
                Int32? shipId = section.ShipID;
                if (shipId != null)
                {
                    if (!indexedShipSectionInstances.ContainsKey(shipId.Value))
                        indexedShipSectionInstances[shipId.Value] = new List<SectionInstanceInfo>();

                    indexedShipSectionInstances[shipId.Value].Add(section);
                }
            }

            return indexedShipSectionInstances;
        }

        /// <summary>Gathers all data and performs bulk insertion of Ships</summary>
        /// <param name="shipsToInsert">Collection of ShipInsertionParameters containing data to insert on</param>
        public void BulkInsertShips(IList<ShipInsertionParameters> shipsToInsert)
        {
            //Dictionary to track the number of ships inserted for each design; the key is DesignID, value is a tuple containing the base count as Item1 and the number to add as Item2
            Dictionary<Int32, Tuple<Int32, Int32>> designBuiltCount = new Dictionary<Int32, Tuple<Int32, Int32>>();
            List<ShipInfo> shipsBeingInserted = new List<ShipInfo>();
            List<ShipSectionInsertionParameters> shipSectionsBeingInserted = new List<ShipSectionInsertionParameters>();

            foreach (ShipInsertionParameters ship in shipsToInsert)
            {
                Int32 fleetID = ship.FleetID;
                Int32 designID = ship.DesignID;
                String shipName = ship.DesignName;

                DesignInfo designInfo = this.GetDesignInfo(designID);
                FleetInfo fleetInfo = this.GetFleetInfo(fleetID);
                if (fleetInfo != null && designInfo.PlayerID != fleetInfo.PlayerID)
                    throw new InvalidOperationException(String.Format("Mismatched design and fleet players (designID={0},design playerID={1},fleet playerID={2}).", designID, designInfo.PlayerID, fleetInfo.PlayerID));

                Boolean isSuulka = false;
                int psionicPower = 0;

                DesignSectionInfo[] designSections = designInfo.DesignSections;
                for (int i = 0; i < designSections.Length; i++)
                {
                    DesignSectionInfo designSectionInfo = designSections[i];
                    ShipSectionAsset shipSectionAsset = this.AssetDatabase.GetShipSectionAsset(designSectionInfo.FilePath);
                    psionicPower += (int)shipSectionAsset.PsionicPowerLevel;
                    isSuulka = (isSuulka || shipSectionAsset.IsSuulka);
                }
                
                if (isSuulka && shipName == null)
                    shipName = designInfo.Name;

                if (!isSuulka)
                {
                    int playerFactionID = this.GetPlayerFactionID(designInfo.PlayerID);
                    Faction faction = this.AssetDatabase.GetFaction(playerFactionID);
                    if (faction != null)
                        psionicPower = (int)(designInfo.CrewAvailable * faction.PsionicPowerPerCrew);
                }

                //change the shift in designs built to work on a bulk level
                if (!designBuiltCount.ContainsKey(designID))
                    designBuiltCount[designID] = new Tuple<Int32, Int32>(this.GetNumShipsBuiltFromDesign(designID), 1);
                else
                    designBuiltCount[designID] = new Tuple<Int32, Int32>(designBuiltCount[designID].Item1, designBuiltCount[designID].Item2 + 1);

                //GameDatabase.AddNumShipsBuiltFromDesign(db, designID, 1);
                //int numShipsBuiltFromDesign = db.GetNumShipsBuiltFromDesign(designID);
                int numShipsBuiltFromDesign = designBuiltCount[designID].Item1 + designBuiltCount[designID].Item2;

                if (!isSuulka && (shipName == null || shipName == designInfo.Name))
                    shipName = this.GetDefaultShipName(designID, numShipsBuiltFromDesign);
                shipName = this.ResolveNewShipName(designInfo.PlayerID, shipName);

                int turnCount = this.GetTurnCount();
                int parentID = 0;

                ShipInfo shipToAdd = new ShipInfo
                {
                    FleetID = fleetID,
                    DesignID = designID,
                    DesignInfo = designInfo,
                    ParentID = parentID,
                    ShipName = shipName,
                    SerialNumber = numShipsBuiltFromDesign,
                    Params = ship.ShipFlags,
                    RiderIndex = -1,
                    PsionicPower = psionicPower,
                    AIFleetID = ship.AIFleetID,
                    ComissionDate = turnCount,
                    LoaCubes = ship.LoaCubes
                };
                ship.ShipInfo = shipToAdd;  //populate for backreferencing the ship ID

                shipsBeingInserted.Add(shipToAdd);
                shipSectionsBeingInserted.Add(new ShipSectionInsertionParameters(designInfo, shipToAdd, null));
                
                //int num2 = this._dom.ships.Insert(null, shipToAdd);
                //this.TryAddFleetShip(num2, fleetID);
                //this.InsertNewShipSectionInstances(designInfo, new int?(num2), null);
                //this.AddCachedShipNameReference(designInfo.PlayerID, shipName);
            }

            //process the bulk insert
            this.BulkInsertShipInfos(shipsBeingInserted);
            this.BulkInsertShipSections(shipSectionsBeingInserted);

            //post-process the insertions
            foreach (ShipInfo insertedShip in shipsBeingInserted)
            {
                this.TryAddFleetShip(insertedShip.ID, insertedShip.FleetID);
                this.AddCachedShipNameReference(insertedShip.DesignInfo.PlayerID, insertedShip.ShipName);
            }
        }

        /// <summary>Gathers all data and performs bulk insertion of Ship Sections</summary>
        /// <param name="ShipSectionInsertion">Collection of ShipSectionInsertionParameters containing data to insert on</param>
        protected void BulkInsertShipSections(IList<ShipSectionInsertionParameters> ShipSectionInsertion)
        {
            List<SectionInstanceInfo> sectionInfosToInsert = new List<SectionInstanceInfo>();
            List<ArmorInstanceInsertionParameters> armorInstancesToInsert = new List<ArmorInstanceInsertionParameters>();
            List<SectionWeaponInstanceInsertionParameters> weaponsToInsert = new List<SectionWeaponInstanceInsertionParameters>();
            List<ShipModuleInstanceInsertionParameters> modulesToInsert = new List<ShipModuleInstanceInsertionParameters>();
            List<WeaponInstanceInfo> weaponInstancesToInsert = new List<WeaponInstanceInfo>();

            foreach (ShipSectionInsertionParameters section in ShipSectionInsertion)
            {
                DesignSectionInfo[] designSections = section.DesignInfo.DesignSections;
                for (int i = 0; i < designSections.Length; i++)
                {
                    DesignSectionInfo designSectionInfo = designSections[i];
                    ShipSectionAsset shipSectionAsset = this.assetdb.GetShipSectionAsset(designSectionInfo.FilePath);
                    List<string> list = new List<string>();
                    if (designSectionInfo.Techs.Count > 0)
                    {
                        foreach (int current in designSectionInfo.Techs)
                        {
                            list.Add(this.GetTechFileID(current));
                        }
                    }
                    int supplyWithTech = Ship.GetSupplyWithTech(this.assetdb, list, shipSectionAsset.Supply);
                    int structureWithTech = Ship.GetStructureWithTech(this.assetdb, list, shipSectionAsset.Structure);

                    SectionInstanceInfo sectionInfo = new SectionInstanceInfo
                    {
                        SectionID = designSectionInfo.ID,
                        ShipID = section.ShipInfo.ID,
                        StationID = section.StationID,
                        Structure = structureWithTech,
                        Supply = supplyWithTech,
                        Crew = shipSectionAsset.Crew,
                        Signature = shipSectionAsset.Signature,
                        RepairPoints = shipSectionAsset.RepairPoints
                    };

                    sectionInfosToInsert.Add(sectionInfo);
                    armorInstancesToInsert.Add(new ArmorInstanceInsertionParameters(shipSectionAsset, this.GetDesignAttributesForDesign(section.DesignInfo.ID), sectionInfo));
                    weaponsToInsert.Add(new SectionWeaponInstanceInsertionParameters(shipSectionAsset.Mounts, designSectionInfo.WeaponBanks, sectionInfo));

                    //int num = this.InsertSectionInstance(designSectionInfo.ID, shipId, stationId, structureWithTech, (float)supplyWithTech, shipSectionAsset.Crew, shipSectionAsset.Signature, shipSectionAsset.RepairPoints);
                    //this.InsertNewArmorInstances(shipSectionAsset, this.GetDesignAttributesForDesign(designInfo.ID).ToList<SectionEnumerations.DesignAttribute>(), num);
                    //this.InsertNewShipWeaponInstancesForSection(shipSectionAsset.Mounts.ToList<LogicalMount>(), designSectionInfo.WeaponBanks.ToList<WeaponBankInfo>(), num);
                    if (designSectionInfo.Modules != null)
                    {
                        modulesToInsert.Add(new ShipModuleInstanceInsertionParameters(shipSectionAsset, designSectionInfo.Modules, sectionInfo));
                        //this.InsertNewShipModuleInstances(shipSectionAsset, designSectionInfo.Modules, num);
                    }
                }
            }

            //perform bulk inserts; in code it goes section;armor;weapon;modules, but I think I will swap modules and weapons since Modules can have Weapons
            this.BulkInsertSectionInstanceInfos(sectionInfosToInsert);
            this.BulkInsertSectionArmor(armorInstancesToInsert);
            this.BulkInsertSectionModuleInstances(modulesToInsert, weaponInstancesToInsert);
            this.BulkInsertSectionWeaponInstances(weaponsToInsert, weaponInstancesToInsert);
            this.BulkInsertWeaponInstances(weaponInstancesToInsert);   //insert all of the weapon instances
        }

        /// <summary>Gathers all data and performs bulk insertion of ship section armor</summary>
        /// <param name="armorInstancesToInsert">Collection or armor parameters to insert</param>
        protected void BulkInsertSectionArmor(IList<ArmorInstanceInsertionParameters> armorInstancesToInsert)
        {
            List<ArmorInstanceInsertion> armorToInsert = new List<ArmorInstanceInsertion>();

            foreach (ArmorInstanceInsertionParameters armorInstance in armorInstancesToInsert)
            {
                Dictionary<ArmorSide, DamagePattern> dictionary = new Dictionary<ArmorSide, DamagePattern>();
                int armorSide = 0;
                Kerberos.Sots.Framework.Size[] armor = armorInstance.ShipSectionAsset.Armor;
                for (int i = 0; i < armor.Length; i++)
                {
                    DamagePattern damagePattern = armorInstance.ShipSectionAsset.CreateFreshArmor((ArmorSide)armorSide, Ship.CalcArmorWidthModifier(armorInstance.Attributes.ToList(), 0));
                    if (damagePattern.Height != 0 && damagePattern.Width != 0)
                    {
                        dictionary[(ArmorSide)armorSide] = damagePattern;
                        armorSide++;
                    }
                }

                armorToInsert.Add(new ArmorInstanceInsertion(dictionary, armorInstance.ShipSection));
                //this._dom.armor_instances.Insert(new int?(armorInstance.ShipSection.SectionID), dictionary);
            }

            this.BulkInsertSectionArmorInstances(armorToInsert);
        }

        /// <summary>Gathers all data and performs bulk insertion of ship section modules</summary>
        /// <param name="modulesToInsert">Collection of modules to insert</param>
        /// <param name="weaponInfosToInsert">Collection of related weapon instances to insert at a later point; this collection is simply added to</param>
        protected void BulkInsertSectionModuleInstances(IList<ShipModuleInstanceInsertionParameters> modulesToInsert, IList<WeaponInstanceInfo> weaponInfosToInsert)
        {
            Dictionary<Int32, String> moduleAssets = new Dictionary<Int32, String>();
            List<ModuleInstanceInfo> moduleInstancesToInsert = new List<ModuleInstanceInfo>();
            List<ModuleWeaponInstanceInsertionParameters> moduleWeaponsToInsert = new List<ModuleWeaponInstanceInsertionParameters>();

            foreach (ShipModuleInstanceInsertionParameters moduleParameters in modulesToInsert)
            {
                foreach (DesignModuleInfo module in moduleParameters.Modules)
                {
                    if (!moduleAssets.ContainsKey(module.ModuleID))
                        moduleAssets[module.ModuleID] = this.GetModuleAsset(module.ModuleID);

                    string mPath = moduleAssets[module.ModuleID];
                    LogicalModule logicalModule = this.assetdb.Modules.FirstOrDefault((LogicalModule x) => x.ModulePath == mPath);

                    LogicalModuleMount mount = moduleParameters.SectionAsset.Modules.First((LogicalModuleMount x) => x.NodeName == module.MountNodeName);
                    ModuleInstanceInfo moduleInfo = new ModuleInstanceInfo
                    {
                        SectionInstanceID = moduleParameters.ShipSection.ID,
                        RepairPoints = logicalModule.RepairPointsBonus,
                        Structure = (int)logicalModule.Structure,
                        ModuleNodeID = mount.NodeName
                    };
                    moduleInstancesToInsert.Add(moduleInfo);

                    //int moduleInstId = this.InsertModuleInstance(moduleParameters.SectionAsset.Modules.First((LogicalModuleMount x) => x.NodeName == module.MountNodeName), logicalModule, moduleParameters.ShipSection.SectionID);
                    if (logicalModule.Mounts.Count<LogicalMount>() > 0)
                    {
                        moduleWeaponsToInsert.Add(new ModuleWeaponInstanceInsertionParameters(logicalModule.Mounts, module.WeaponID, moduleParameters.ShipSection, moduleInfo));
                        //this.InsertNewShipWeaponInstancesForModule(logicalModule.Mounts.ToList<LogicalMount>(), module.WeaponID, moduleParameters.ShipSection.SectionID, moduleInstId);
                    }
                }
            }

            //perform the bulk insertion
            this.BulkInsertSectionModuleInstances(moduleInstancesToInsert);
            this.BulkInsertModuleWeaponInstances(moduleWeaponsToInsert, weaponInfosToInsert);
        }

        /// <summary>Gathers all data for the bulk insertion of ship section weapons and outputs the insertion records to provided collection</summary>
        /// <param name="weaponsToInsert">Ship section weapon parameters to create weapons for</param>
        /// <param name="weaponInfosToInsert">Collection to add WeaponInstanceInfo objects generated into</param>
        protected void BulkInsertSectionWeaponInstances(List<SectionWeaponInstanceInsertionParameters> weaponsToInsert, IList<WeaponInstanceInfo> weaponInfosToInsert)
        {
            foreach (SectionWeaponInstanceInsertionParameters weaponParams in weaponsToInsert)
            {
                int weaponIndex = 0;
                foreach (LogicalMount mount in weaponParams.Mounts)
                {
                    if (!WeaponEnums.IsBattleRider(mount.Bank.TurretClass))
                    {
                        float turretHealth = Ship.GetTurretHealth(mount.Bank.TurretSize);
                        WeaponBankInfo weaponBankInfo = weaponParams.Banks.FirstOrDefault((WeaponBankInfo x) => x.BankGUID == mount.Bank.GUID);
                        if (weaponBankInfo != null && weaponBankInfo.WeaponID.HasValue)
                        {
                            string weapon = this.GetWeaponAsset(weaponBankInfo.WeaponID.Value);
                            LogicalWeapon logicalWeapon = this.assetdb.Weapons.FirstOrDefault((LogicalWeapon x) => x.FileName == weapon);
                            if (logicalWeapon != null)
                            {
                                turretHealth += logicalWeapon.Health;
                            }
                        }

                        WeaponInstanceInfo newWeapon = new WeaponInstanceInfo
                        {
                            SectionInstanceID = weaponParams.ShipSection.ID,
                            ModuleInstanceID = null,
                            Structure = turretHealth,
                            MaxStructure = turretHealth,
                            WeaponID = weaponIndex,
                            NodeName = mount.NodeName
                        };
                        weaponInfosToInsert.Add(newWeapon);
                        
                        //this.InsertWeaponInstance(mount, sectionInstId, null, weaponIndex, turretHealth, mount.NodeName);
                        weaponIndex++;
                    }
                }
            }
        }

        /// <summary>Gathers all data for the bulk insertion of ship section modules' weapons and outputs the insertion records to provided collection</summary>
        /// <param name="weaponsToInsert">Module weapon parameters to create weapons for</param>
        /// <param name="weaponInfosToInsert">Collection to add WeaponInstanceInfo objects generated into</param>
        protected void BulkInsertModuleWeaponInstances(List<ModuleWeaponInstanceInsertionParameters> weaponsToInsert, IList<WeaponInstanceInfo> weaponInfosToInsert)
        {
            foreach (ModuleWeaponInstanceInsertionParameters weaponParams in weaponsToInsert)
            {
                string weapon = weaponParams.WeaponID.HasValue ? this.GetWeaponAsset(weaponParams.WeaponID.Value) : String.Empty;
                LogicalWeapon logicalWeapon = (!string.IsNullOrEmpty(weapon)) ? this.assetdb.Weapons.FirstOrDefault((LogicalWeapon x) => x.FileName == weapon) : null;
                int weaponId = 0;
                foreach (LogicalMount currentMount in weaponParams.Mounts)
                {
                    if (!WeaponEnums.IsBattleRider(currentMount.Bank.TurretClass))
                    {
                        float turretHealth = Ship.GetTurretHealth(currentMount.Bank.TurretSize);
                        if (logicalWeapon != null)
                            turretHealth += logicalWeapon.Health;
                        
                        WeaponInstanceInfo newWeapon = new WeaponInstanceInfo
                        {
                            SectionInstanceID = weaponParams.ShipSection.ID,
                            ModuleInstanceID = weaponParams.ModuleInstance.ID,
                            Structure = turretHealth,
                            MaxStructure = turretHealth,
                            WeaponID = weaponId,
                            NodeName = currentMount.NodeName
                        };
                        weaponInfosToInsert.Add(newWeapon);

                        //this.InsertWeaponInstance(currentMount, sectionInstId, new int?(moduleInstId), weaponId, turretHealth, currentMount.NodeName);
                        weaponId++;
                    }
                }
            }
        }

        /// <summary>Performs the actual bulk insert of ShipInfo records</summary>
        /// <param name="shipsBeingInserted">Collection of ShipInfoInsertion records containing ShipInfos to insert</param>
        protected void BulkInsertShipInfos(IList<ShipInfo> shipsBeingInserted)
        {
            ShipsCache cache = new ShipsCache(this._dom.ships);
            cache.BulkInsert(shipsBeingInserted);
        }

        /// <summary>Performs the actual bulk insert of SectionInstanceInfo records</summary>
        /// <param name="shipSectionsBeingInserted">Collection of SectionInstanceInfo records containing SectionInstanceInfo to insert</param>
        protected void BulkInsertSectionInstanceInfos(IList<SectionInstanceInfo> shipSectionsBeingInserted)
        {
            SectionInstancesCache cache = new SectionInstancesCache(this._dom.section_instances);
            cache.BulkInsert(shipSectionsBeingInserted);
        }

        /// <summary>Performs the actual bulk insert of armor records</summary>
        /// <param name="armorBeingInserted">Collection of ArmorInstanceInsertion records containing armor to insert</param>
        protected void BulkInsertSectionArmorInstances(IList<ArmorInstanceInsertion> armorBeingInserted)
        {
            ArmorInstancesCache cache = new ArmorInstancesCache(this._dom.armor_instances);
            cache.BulkInsert(armorBeingInserted);
        }

        /// <summary>Performs the actual bulk insert of module records</summary>
        /// <param name="modulesBeingInserted">Collection of ModuleInstanceInfo records containing modules to insert</param>
        protected void BulkInsertSectionModuleInstances(List<ModuleInstanceInfo> modulesBeingInserted)
        {
            ModuleInstancesCache cache = new ModuleInstancesCache(this._dom.module_instances);
            cache.BulkInsert(modulesBeingInserted);
        }

        /// <summary>Performs the actual bulk insert of weapon records</summary>
        /// <param name="weaponsBeingInserted">Collection of WeaponInstanceInfo records containing weapons to insert</param>
        protected void BulkInsertWeaponInstances(List<WeaponInstanceInfo> weaponsBeingInserted)
        {
            WeaponInstancesCache cache = new WeaponInstancesCache(this._dom.weapon_instances);
            cache.BulkInsert(weaponsBeingInserted);
        }


        #region RetrieveSavedShipDesigns(...) Data retrieval methods
        /// <summary>Retrieves the collection of design section Tech IDs for all design sections</summary>
        /// <param name="conn">SQLiteConnection to execute the retrieval query upon</param>
        /// <returns>An indexed dictionary of the Design Sections' Techs grouped on the Design Section ID</returns>
        public static Dictionary<Int32, List<Int32>> RetrieveDesignSectionTechs(SQLiteConnection conn)
        {
            //key = design section ID, value = tech ID
            Dictionary<Int32, List<Int32>> designSectionTechs = new Dictionary<Int32, List<Int32>>();
            IEnumerable<Row> techs = conn.ExecuteTableQuery(Queries.GetDesignSectionTechs, false);  //false since there is no need to split a single query
            foreach (Row result in techs)
            {
                Int32 sectionId = Int32.Parse(result[0]);
                Int32 techId = Int32.Parse(result[1]);

                if (!designSectionTechs.ContainsKey(sectionId))
                    designSectionTechs[sectionId] = new List<Int32>();

                designSectionTechs[sectionId].Add(techId);
            }

            return designSectionTechs;
        }

        /// <summary>Retrieves the collection of ModulePsionicInfo for each Design Module</summary>
        /// <param name="conn">SQLiteConnection to execute the retrieval query upon</param>
        /// <returns>The collection of all ModulePsionicInfo, grouped on the Design's Module ID</returns>
        public static Dictionary<Int32, List<ModulePsionicInfo>> RetrieveModulePsionicInfos(SQLiteConnection conn)
        {
            Dictionary<Int32, List<ModulePsionicInfo>> modulePsionicInfos = new Dictionary<Int32, List<ModulePsionicInfo>>();
            IEnumerable<Row> psionicModules = conn.ExecuteTableQuery(Queries.GetModulePsionics, false);
            foreach (Row result in psionicModules)
            {
                Int32 id = Int32.Parse(result[0]);
                Int32 designModuleId = Int32.Parse(result[1]);
                SectionEnumerations.PsionicAbility ability = (SectionEnumerations.PsionicAbility)Int32.Parse(result[2]);

                ModulePsionicInfo info = new ModulePsionicInfo();
                info.ID = id;
                info.DesignModuleID = designModuleId;
                info.Ability = ability;

                if (!modulePsionicInfos.ContainsKey(designModuleId))
                    modulePsionicInfos[designModuleId] = new List<ModulePsionicInfo>();

                modulePsionicInfos[designModuleId].Add(info);
            }

            return modulePsionicInfos;
        }

        /// <summary>Retrieves the entire collection of WeaponBankInfo for each Design Section</summary>
        /// <param name="conn">SQLiteConnection to execute the retrieval query upon</param>
        /// <returns>The global collection of WeaponBankInfo, indexed by Design Section ID</returns>
        public static Dictionary<Int32, List<WeaponBankInfo>> RetieveWeaponBankInfos(SQLiteConnection conn)
        {
            Dictionary<Int32, List<WeaponBankInfo>> weaponBankInfos = new Dictionary<Int32, List<WeaponBankInfo>>();
            IEnumerable<Row> weaponBanks = conn.ExecuteTableQuery(Queries.GetWeaponBanks, false);
            foreach (Row row in weaponBanks)
            {
                Int32 id = Int32.Parse(row[0]);
                Int32? weaponId = row[1].SQLiteValueToNullableInteger();
                Int32? designId = row[2].SQLiteValueToNullableInteger();
                Guid bankId = Guid.Parse(row[3]);
                Int32? firingMode = row[4].SQLiteValueToNullableInteger();
                Int32? filterMode = row[5].SQLiteValueToNullableInteger();
                Int32 designSectionId = Int32.Parse(row[6]);

                WeaponBankInfo info = new WeaponBankInfo();
                info.ID = id;
                info.WeaponID = weaponId;
                info.DesignID = designId;
                info.BankGUID = bankId;
                info.FiringMode = firingMode;
                info.FilterMode = filterMode;

                if (!weaponBankInfos.ContainsKey(designSectionId))
                    weaponBankInfos[designSectionId] = new List<WeaponBankInfo>();

                weaponBankInfos[designSectionId].Add(info);
            }

            return weaponBankInfos;
        }

        /// <summary>Retrieves the entire collection of WeaponBankInfo for each Design Section</summary>
        /// <param name="conn">SQLiteConnection to execute the retrieval query upon</param>
        /// <param name="psionics">Collection of ModulePsionicInfo already populated and grouped by Design Module ID</param>
        /// <returns>The collection of all DesignModuleInfos grouped and indexed by  Design Section ID</returns>
        public static Dictionary<Int32, List<DesignModuleInfo>> RetrieveDesignModules(SQLiteConnection conn, Dictionary<Int32, List<ModulePsionicInfo>> psionics)
        {
            Dictionary<Int32, List<DesignModuleInfo>> designModules = new Dictionary<Int32, List<DesignModuleInfo>>();
            IEnumerable<Row> modules = conn.ExecuteTableQuery(Queries.GetDesignModules, false);
            foreach (Row row in modules)
            {
                Int32 id = row[0].SQLiteValueToInteger();
                Int32 sectionId = row[1].SQLiteValueToInteger();
                Int32 moduleId = row[2].SQLiteValueToInteger();
                Int32? weaponId = row[3].SQLiteValueToNullableInteger();
                String mountNodeName = row[4].SQLiteValueToString();
                Int32? stationModuleTypeNum = row[5].SQLiteValueToNullableInteger();
                ModuleEnums.StationModuleType? stationModuleType = null;
                if (stationModuleTypeNum.HasValue)
                    stationModuleType = (ModuleEnums.StationModuleType)stationModuleTypeNum;
                Int32? designId = row[6].SQLiteValueToNullableInteger();

                DesignModuleInfo designModuleInfo = new DesignModuleInfo();
                designModuleInfo.ID = id;
                designModuleInfo.ModuleID = moduleId;
                designModuleInfo.WeaponID = weaponId;
                designModuleInfo.MountNodeName = mountNodeName;
                designModuleInfo.StationModuleType = stationModuleType;
                designModuleInfo.DesignID = designId;
                designModuleInfo.PsionicAbilities = psionics.ContainsKey(id) ? psionics[id] : new List<ModulePsionicInfo>();

                //I'm not quite sure why a lower property would need to access its parent, but since they are not
                //  yet available, set it to null for now and set it when assigning this instance to a Design Section
                designModuleInfo.DesignSectionInfo = null;


                if (!designModules.ContainsKey(sectionId))
                    designModules[sectionId] = new List<DesignModuleInfo>();

                designModules[sectionId].Add(designModuleInfo);
            }

            return designModules;
        }

        /// <summary>Retrieves the entire collection of DesignSectionInfo for each Design</summary>
        /// <param name="conn">SQLiteConnection to execute the retrieval query upon</param>
        /// <param name="assets">Asset Database to probe</param>
        /// <param name="weaponBanks">Collection of WeaponBankInfos grouped by Design Section ID</param>
        /// <param name="techs">Collection of Tech IDs grouped by Design Section ID</param>
        /// <param name="modules">Collection of DesignModuleInfos grouped by Design Section ID</param>
        /// <returns>The collection of all Design Sections grouped and indexed by Design ID</returns>
        public static Dictionary<Int32, List<DesignSectionInfo>> RetrieveDesignSections(SQLiteConnection conn, AssetDatabase assets, Dictionary<Int32, List<WeaponBankInfo>> weaponBanks, Dictionary<Int32, List<Int32>> techs, Dictionary<Int32, List<DesignModuleInfo>> modules)
        {
            Dictionary<Int32, List<DesignSectionInfo>> designSections = new Dictionary<Int32, List<DesignSectionInfo>>();
            IEnumerable<Row> sections = conn.ExecuteTableQuery(Queries.GetDesignSections, false);
            foreach (Row row in sections)
            {
                Int32 sectionId = row[0].SQLiteValueToInteger();
                Int32 designId = Int32.Parse(row[1]);
                String assetName = row[2].SQLiteValueToString();
                ShipSectionAsset asset = assets.GetShipSectionAsset(assetName);
                List<WeaponBankInfo> sectionWeaponBanks = weaponBanks.ContainsKey(sectionId) ? weaponBanks[sectionId].OrderBy(wb => wb.ID).ToList() : new List<WeaponBankInfo>();
                List<Int32> sectionTechs = techs.ContainsKey(sectionId) ? techs[sectionId] : new List<Int32>();

                DesignSectionInfo designSection = new DesignSectionInfo();
                designSection.ID = sectionId;
                designSection.FilePath = assetName;
                designSection.ShipSectionAsset = asset;
                designSection.WeaponBanks = sectionWeaponBanks;
                designSection.Techs = sectionTechs;

                //the following I don't really understand the reasoning for, but this is what the reflected code does nonetheless
                //It appears to filter out modules from the collection that already matches the secion based on the modules' mount node name

                List<DesignModuleInfo> readModules = modules.ContainsKey(sectionId) ? modules[sectionId] : new List<DesignModuleInfo>();
                List<DesignModuleInfo> outputModules = new List<DesignModuleInfo>();
                IList<LogicalModuleMount> logicalModuleMounts = asset.Modules;
                for (int i = 0; i < logicalModuleMounts.Count; i++)
                {
                    LogicalModuleMount mount = logicalModuleMounts[i];
                    if (readModules != null)
                    {
                        DesignModuleInfo designModuleInfo = readModules.FirstOrDefault((DesignModuleInfo x) => x.MountNodeName == mount.NodeName);
                        if (designModuleInfo != null)
                        {
                            outputModules.Add(designModuleInfo);
                            readModules.Remove(designModuleInfo);
                        }
                    }
                }

                //since each DesignModuleInfo needs a parent back-reference, apply it here; loop through the outputModules
                //  and assign their DesignSectionInfo appropriately
                foreach (DesignModuleInfo module in outputModules)
                    module.DesignSectionInfo = designSection;

                designSection.Modules = outputModules;

                if (!designSections.ContainsKey(designId))
                    designSections[designId] = new List<DesignSectionInfo>();

                designSections[designId].Add(designSection);
            }

            return designSections;
        }

        /// <summary>Retrieves the entire collection of DesignInfos from the save file</summary>
        /// <param name="conn">SQLiteConnection to execute the retrieval query upon</param>
        /// <param name="designSections">Fully populated collection of DesignSectionInfos with all fields except DesignInfo populated</param>
        /// <returns>The entire collection of DesignInfos from the save file</returns>
        public static List<DesignInfo> RetreiveDesigns(SQLiteConnection conn, Dictionary<Int32, List<DesignSectionInfo>> designSections)
        {
            List<DesignInfo> designs = new List<DesignInfo>();
            IEnumerable<Row> designRows = conn.ExecuteTableQuery(Kerberos.Sots.Data.Queries.GetDesignInfos, false);
            foreach (Row row in designRows)
            {
                Int32 ID = row[0].SQLiteValueToInteger();
                Int32 PlayerID = row[1].SQLiteValueToOneBasedIndex();
                String Name = row[2];
                Int32 Armour = int.Parse(row[3]);
                Single Structure = float.Parse(row[4]);
                Int32 NumTurrets = int.Parse(row[5]);
                Single Mass = float.Parse(row[6]);
                Single Acceleration = float.Parse(row[8]);
                Single TopSpeed = float.Parse(row[9]);
                Int32 SavingsCost = int.Parse(row[10]);
                Int32 ProductionCost = int.Parse(row[11]);
                ShipClass Class = (ShipClass)int.Parse(row[12]);
                Int32 CrewAvailable = int.Parse(row[13]);
                Int32 PowerAvailable = int.Parse(row[14]);
                Int32 SupplyAvailable = int.Parse(row[15]);
                Int32 CrewRequired = int.Parse(row[16]);
                Int32 PowerRequired = int.Parse(row[17]);
                Int32 SupplyRequired = int.Parse(row[18]);
                Int32 NumBuilt = int.Parse(row[19]);
                Int32 DesignDate = int.Parse(row[20]);
                ShipRole Role = (ShipRole)int.Parse(row[21]);
                WeaponRole weaponRole = (WeaponRole)int.Parse(row[22]);
                Boolean isPrototyped = bool.Parse(row[23]);
                Boolean isAttributesDiscovered = bool.Parse(row[24]);
                StationType stationType = (StationType)int.Parse(row[25]);
                Int32 StationLevel = int.Parse(row[26]);
                String PriorityWeaponName = row[28] ?? string.Empty;
                Int32 NumDestroyed = int.Parse(row[29]);
                Int32 RetrofitBaseID = row[30].SQLiteValueToOneBasedIndex();

                DesignInfo designInfo = new DesignInfo();
                designInfo.ID = ID;
                designInfo.PlayerID = PlayerID;
                designInfo.Name = Name;
                designInfo.Armour = Armour;
                designInfo.Structure = Structure;
                designInfo.NumTurrets = NumTurrets;
                designInfo.Mass = Mass;
                designInfo.Acceleration = Acceleration;
                designInfo.TopSpeed = TopSpeed;
                designInfo.SavingsCost = SavingsCost;
                designInfo.ProductionCost = ProductionCost;
                designInfo.Class = Class;
                designInfo.CrewAvailable = CrewAvailable;
                designInfo.PowerAvailable = PowerAvailable;
                designInfo.SupplyAvailable = SupplyAvailable;
                designInfo.CrewRequired = CrewRequired;
                designInfo.PowerRequired = PowerRequired;
                designInfo.SupplyRequired = SupplyRequired;
                designInfo.NumBuilt = NumBuilt;
                designInfo.DesignDate = DesignDate;
                designInfo.Role = Role;
                designInfo.WeaponRole = weaponRole;
                designInfo.isPrototyped = isPrototyped;
                designInfo.isAttributesDiscovered = isAttributesDiscovered;
                designInfo.StationType = stationType;
                designInfo.StationLevel = StationLevel;
                designInfo.PriorityWeaponName = PriorityWeaponName;
                designInfo.NumDestroyed = NumDestroyed;
                designInfo.RetrofitBaseID = RetrofitBaseID;

                DesignSectionInfo[] designSectionInfos = designSections.ContainsKey(ID) ? designSections[ID].ToArray() : new DesignSectionInfo[0];
                foreach (DesignSectionInfo info in designSectionInfos)  //since each DesignSection needs a back-reference, apply it here
                    info.DesignInfo = designInfo;

                designInfo.DesignSections = designSectionInfos;

                designs.Add(designInfo);
            }

            return designs;
        }
        #endregion
    }
}