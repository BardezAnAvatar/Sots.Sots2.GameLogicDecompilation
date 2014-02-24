using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Kerberos.Sots;
using Kerberos.Sots.Data;
using Kerberos.Sots.Data.ScenarioFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameTriggers;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.UI;

using Bardez.Projects.SwordOfTheStars.SotS2.Utility;
using Bardez.Projects.SwordOfTheStars.SotS2.Fixes.Sots.StarFleet;

using Original = Kerberos.Sots.Strategy;
using PerformanceData = Bardez.Projects.SwordOfTheStars.SotS2.Fixes.Sots.Data;

namespace Bardez.Projects.SwordOfTheStars.SotS2.Fixes.Sots.Strategy
{
    /// <summary>This class contains performance enhancements to the Kerberos.Sots.Strategy.GameSession class</summary>
	internal class GameSession
    {
        #region Fields
        protected Original.GameSession BaseGameSession;
        #endregion


        #region Kerberos GameSession reflection-exposed members
        #region Kerberos Fields
        public List<Original.TurnEvent> TurnEvents
        {
            get { return this.BaseGameSession.TurnEvents; }
            set { this.BaseGameSession.TurnEvents = value; }
        }

        public List<Trigger> ActiveTriggers
        {
            get { return this.BaseGameSession.ActiveTriggers; }
        }

        public List<EventStub> TriggerEvents
        {
            get { return this.BaseGameSession.TriggerEvents; }
            set { this.BaseGameSession.TriggerEvents = value; }
        }

        public Dictionary<string, float> TriggerScalars
        {
            get { return this.BaseGameSession.TriggerScalars; }
        }

        public IList<Kerberos.Sots.PlayerFramework.Player> OtherPlayers
        {
            get { return this.BaseGameSession.OtherPlayers; }
        }

        public Original.PendingCombat _currentCombat
        {
            get { return this.BaseGameSession._currentCombat; }
            set { this.BaseGameSession._currentCombat = value; }
        }

        public Original.SimState State
        {
            get { return this.BaseGameSession.State; }
            set { this.BaseGameSession.State = value; }
        }

        public OrbitCameraController StarMapCamera
        {
            get { return this.BaseGameSession.StarMapCamera; }
            set { this.BaseGameSession.StarMapCamera = value; }
        }

        public Object StarMapSelectedObject
        {
            get { return this.BaseGameSession.StarMapSelectedObject; }
            set { this.BaseGameSession.StarMapSelectedObject = value; }
        }

        protected String _saveGameName
        {
            get { return ReflectionHelper.PrivateField<Original.GameSession, String>(this.BaseGameSession, "_saveGameName"); }
            set { ReflectionHelper.PrivateField<Original.GameSession, String>(this.BaseGameSession, "_saveGameName", value); }
        }

        protected GameObjectSet _crits
        {
            get { return ReflectionHelper.PrivateField<Original.GameSession, GameObjectSet>(this.BaseGameSession, "_crits"); }
            set { ReflectionHelper.PrivateField<Original.GameSession, GameObjectSet>(this.BaseGameSession, "_crits", value); }
        }

        protected Dictionary<int, int> _playerGateMap
        {
            get { return ReflectionHelper.PrivateField<Original.GameSession, Dictionary<int, int>>(this.BaseGameSession, "_playerGateMap"); }
            set { ReflectionHelper.PrivateField<Original.GameSession, Dictionary<int, int>>(this.BaseGameSession, "_playerGateMap", value); }
        }

        protected Original.TurnTimer _turnTimer
        {
            get { return ReflectionHelper.PrivateField<Original.GameSession, Original.TurnTimer>(this.BaseGameSession, "_turnTimer"); }
            set { ReflectionHelper.PrivateField<Original.GameSession, Original.TurnTimer>(this.BaseGameSession, "_turnTimer", value); }
        }

        protected Dialog DialogCombatsPending
        {
            get { return ReflectionHelper.PrivateField<Original.GameSession, Dialog>(this.BaseGameSession, "DialogCombatsPending"); }
            set { ReflectionHelper.PrivateField<Original.GameSession, Dialog>(this.BaseGameSession, "DialogCombatsPending", value); }
        }

        protected Original.CombatDataHelper _combatData
        {
            get { return ReflectionHelper.PrivateField<Original.GameSession, Original.CombatDataHelper>(this.BaseGameSession, "_combatData"); }
            set { ReflectionHelper.PrivateField<Original.GameSession, Original.CombatDataHelper>(this.BaseGameSession, "_combatData", value); }
        }

        protected Dictionary<int, double> _incomeFromTrade
        {
            get { return ReflectionHelper.PrivateField<Original.GameSession, Dictionary<int, double>>(this.BaseGameSession, "_incomeFromTrade"); }
            set { ReflectionHelper.PrivateField<Original.GameSession, Dictionary<int, double>>(this.BaseGameSession, "_incomeFromTrade", value); }
        }

        protected App _app
        {
            get { return ReflectionHelper.PrivateField<Original.GameSession, App>(this.BaseGameSession, "_app"); }
            set { ReflectionHelper.PrivateField<Original.GameSession, App>(this.BaseGameSession, "_app", value); }
        }

        protected GameDatabase _db
        {
            get { return ReflectionHelper.PrivateField<Original.GameSession, GameDatabase>(this.BaseGameSession, "_db"); }
            set { ReflectionHelper.PrivateField<Original.GameSession, GameDatabase>(this.BaseGameSession, "_db", value); }
        }

        protected Random _random
        {
            get { return ReflectionHelper.PrivateField<Original.GameSession, Random>(this.BaseGameSession, "_random"); }
            set { ReflectionHelper.PrivateField<Original.GameSession, Random>(this.BaseGameSession, "_random", value); }
        }

        protected uint m_GameID
        {
            get { return ReflectionHelper.PrivateField<Original.GameSession, uint>(this.BaseGameSession, "m_GameID"); }
            set { ReflectionHelper.PrivateField<Original.GameSession, uint>(this.BaseGameSession, "m_GameID", value); }
        }

        protected List<Kerberos.Sots.PlayerFramework.Player> m_Players
        {
            get { return ReflectionHelper.PrivateField<Original.GameSession, List<Kerberos.Sots.PlayerFramework.Player>>(this.BaseGameSession, "m_Players"); }
        }

        protected List<Kerberos.Sots.StarSystem> m_Systems
        {
            get { return ReflectionHelper.PrivateField<Original.GameSession, List<Kerberos.Sots.StarSystem>>(this.BaseGameSession, "m_Systems"); }
        }

        protected List<Original.PendingCombat> m_Combats
        {
            get { return ReflectionHelper.PrivateField<Original.GameSession, List<Original.PendingCombat>>(this.BaseGameSession, "m_Combats"); }
            set { ReflectionHelper.PrivateField<Original.GameSession, List<Original.PendingCombat>>(this.BaseGameSession, "m_Combats", value); }
        }

        protected List<FleetInfo> fleetsInCombat
        {
            get { return ReflectionHelper.PrivateField<Original.GameSession, List<FleetInfo>>(this.BaseGameSession, "fleetsInCombat"); }
            set { ReflectionHelper.PrivateField<Original.GameSession, List<FleetInfo>>(this.BaseGameSession, "fleetsInCombat", value); }
        }

        protected List<Original.ReactionInfo> _reactions
        {
            get { return ReflectionHelper.PrivateField<Original.GameSession, List<Original.ReactionInfo>>(this.BaseGameSession, "_reactions"); }
            set { ReflectionHelper.PrivateField<Original.GameSession, List<Original.ReactionInfo>>(this.BaseGameSession, "_reactions", value); }
        }

        protected Original.MultiCombatCarryOverData m_MCCarryOverData
        {
            get { return ReflectionHelper.PrivateField<Original.GameSession, Original.MultiCombatCarryOverData>(this.BaseGameSession, "m_MCCarryOverData"); }
            set { ReflectionHelper.PrivateField<Original.GameSession, Original.MultiCombatCarryOverData>(this.BaseGameSession, "m_MCCarryOverData", value); }
        }

        protected Original.OpenCloseSystemToggleData m_OCSystemToggleData
        {
            get { return ReflectionHelper.PrivateField<Original.GameSession, Original.OpenCloseSystemToggleData>(this.BaseGameSession, "m_OCSystemToggleData"); }
            set { ReflectionHelper.PrivateField<Original.GameSession, Original.OpenCloseSystemToggleData>(this.BaseGameSession, "m_OCSystemToggleData", value); }
        }

        protected Dictionary<Faction, float> m_SpeciesIdealSuitability
        {
            get { return ReflectionHelper.PrivateField<Original.GameSession, Dictionary<Faction, float>>(this.BaseGameSession, "m_SpeciesIdealSuitability"); }
        }
        #endregion

        #region Kerberos Properties
        public Kerberos.Sots.PlayerFramework.Player LocalPlayer
        {
            get { return this.BaseGameSession.LocalPlayer; }
            protected set { ReflectionHelper.PublicProperty<Original.GameSession, Kerberos.Sots.PlayerFramework.Player>(this.BaseGameSession, "LocalPlayer", value); }
        }

        public NamesPool NamesPool
        {
            get { return this.BaseGameSession.NamesPool; }
            protected set { ReflectionHelper.PublicProperty<Original.GameSession, NamesPool>(this.BaseGameSession, "NamesPool", value); }
        }

        public bool IsMultiplayer
        {
            get { return this.BaseGameSession.IsMultiplayer; }
            protected set { ReflectionHelper.PublicProperty<Original.GameSession, bool>(this.BaseGameSession, "IsMultiplayer", value); }
        }

        public Original.ScriptModules ScriptModules
        {
            get { return this.BaseGameSession.ScriptModules; }
            protected set { ReflectionHelper.PublicProperty<Original.GameSession, Original.ScriptModules>(this.BaseGameSession, "ScriptModules", value); }
        }

        public bool HomeworldNamed
        {
            get { return this.BaseGameSession.HomeworldNamed; }
            set { this.BaseGameSession.HomeworldNamed = value; }
        }

        public Random Random
        {
            get { return this.BaseGameSession.Random; }
        }

        public string SaveGameName
        {
            get { return this.BaseGameSession.SaveGameName; }
        }

        public AssetDatabase AssetDatabase
        {
            get { return this.BaseGameSession.AssetDatabase; }
        }

        public App App
        {
            get { return this.BaseGameSession.App; }
        }

        public UICommChannel UI
        {
            get { return this.BaseGameSession.UI; }
        }

        public Original.TurnTimer TurnTimer
        {
            get { return this.BaseGameSession.TurnTimer; }
        }

        public Original.CombatDataHelper CombatData
        {
            get { return this.BaseGameSession.CombatData; }
        }

        public GameDatabase GameDatabase
        {
            get { return this.BaseGameSession.GameDatabase; }
        }

        public Original.MultiCombatCarryOverData MCCarryOverData
        {
            get { return this.BaseGameSession.MCCarryOverData; }
        }

        public Original.OpenCloseSystemToggleData OCSystemToggleData
        {
            get { return this.BaseGameSession.OCSystemToggleData; }
        }
        #endregion

        #region Kerberos Methods
        public void Autosave(string suffix)
        {
            this.BaseGameSession.Autosave(suffix);
        }

        public void ProcessMidTurn()
        {
            this.BaseGameSession.ProcessMidTurn();
        }
        #endregion
        #endregion


        #region Construction
        /// <summary>Definition constructor</summary>
        /// <param name="original">Kerberos Gamesession to wrap</param>
        internal GameSession(Original.GameSession original)
        {
            this.BaseGameSession = original;
        }
        #endregion


        #region Performance Enhancements
        public void ProcessEndTurn()
        {
            this.TurnTimer.StopTurnTimer();
            this.App.HotKeyManager.SetEnabled(false);
            if (this.m_Combats.Count() > 0)
            {
                return;
            }
            if (this.App.Network.IsHosting || !this.App.GameSetup.IsMultiplayer)
            {
                //The Ship Section Instances are slammed during end of turn processing. I think it best to index them by Ship and pass them around.
                Dictionary<Int32, IList<SectionInstanceInfo>> shipSectionInstances = PerformanceData.GameDatabase.GetShipSectionInstances(this.GameDatabase);

                Original.StrategicAI.UpdateInfo updateInfo = new Original.StrategicAI.UpdateInfo(this.GameDatabase);
                foreach (Kerberos.Sots.PlayerFramework.Player current in this.m_Players)
                {
                    if (current.IsAI() && current.Faction.IsPlayable)
                    {
                        Original.StrategicAI stratAI = current.GetAI();

                        StrategicAI performanceSAI = new StrategicAI(stratAI);
                        performanceSAI.Update(updateInfo, shipSectionInstances);

                        //stratAI.Update(updateInfo);
                    }
                }
            }
            if (Original.GameSession.SimAITurns == 0 && this.App.GameSettings.AutoSave)
            {
                this.Autosave("(Precombat)");
            }
            this.State = Original.SimState.SS_COMBAT;
            if (!this.App.GameSetup.IsMultiplayer)
            {
                this.ProcessMidTurn();
            }
        }

        /// <summary>Repairs (AI) player's fleets at the specified system</summary>
        /// <param name="systemid">Unique ID of the system to repair at</param>
        /// <param name="playerid">Unique ID of the player to perform repairs for</param>
        /// <param name="shipSectionInstances">Pre-indexed collection of ship section instances to compute on</param>
        public void RepairFleetsAtSystem(int systemid, int playerid, Dictionary<Int32, IList<SectionInstanceInfo>> shipSectionInstances)
        {
            int num = this.GetSystemRepairPoints(systemid, playerid, shipSectionInstances);
            if (num <= 0)
            {
                return;
            }
            List<FleetInfo> list = (
                from x in this._db.GetFleetInfoBySystemID(systemid, FleetType.FL_NORMAL | FleetType.FL_RESERVE | FleetType.FL_DEFENSE | FleetType.FL_GATE | FleetType.FL_STATION | FleetType.FL_CARAVAN | FleetType.FL_ACCELERATOR)
                where x.PlayerID == playerid
                select x).ToList<FleetInfo>();
            foreach (FleetInfo current in list)
            {
                num = this.RepairFleet(current.ID, num, shipSectionInstances);  //use the pre-indexed list
            }
        }

        /// <summary>Returns the total repair points available for the specified player in the specified system</summary>
        /// <param name="systemid">Unique ID of the system to repair at</param>
        /// <param name="playerid">Unique ID of the player to perform repairs for</param>
        /// <param name="shipSectionInstances">Pre-indexed collection of ship section instances to compute on</param>
        /// <returns>The total repair points for the player in the system</returns>
        public int GetSystemRepairPoints(int systemid, int playerid, Dictionary<Int32, IList<SectionInstanceInfo>> shipSectionInstances)
        {
            List<ColonyInfo> list = (
                from x in this._db.GetColonyInfosForSystem(systemid)
                where x.PlayerID == playerid
                select x).ToList<ColonyInfo>();
            int num = 0;
            foreach (ColonyInfo current in list)
            {
                num += current.RepairPoints;
            }
            List<StationInfo> list2 = this._db.GetStationForSystemAndPlayer(systemid, playerid).ToList<StationInfo>();
            foreach (StationInfo current2 in list2)
            {
                IList<SectionInstanceInfo> list3 = shipSectionInstances[current2.ShipID];
                if (list3 != null)
                    foreach (SectionInstanceInfo current3 in list3)
                    {
                        num += current3.RepairPoints;
                    }
            }
            List<FleetInfo> list4 = this._db.GetFleetInfoBySystemID(systemid, FleetType.FL_NORMAL).ToList<FleetInfo>();
            foreach (FleetInfo current4 in list4)
            {
                List<ShipInfo> list5 = this._db.GetShipInfoByFleetID(current4.ID, false).ToList<ShipInfo>();
                foreach (ShipInfo current5 in list5)
                {
                    IList<SectionInstanceInfo> list6 = shipSectionInstances[current5.ID];
                    if (list6 != null)
                        foreach (SectionInstanceInfo current6 in list6)
                        {
                            num += current6.RepairPoints;
                        }
                }
            }
            return num;
        }

        /// <summary>Repairs the specified fleet given available repair points remaining</summary>
        /// <param name="fleetid">Unique ID of the Fleet to repair</param>
        /// <param name="availpoints">Total repair points available</param>
        /// <param name="shipSectionInstances">Pre-indexed collection of ship section instances to compute on</param>
        /// <returns>The remaining repair points available after the repair operation</returns>
        protected int RepairFleet(int fleetid, int availpoints, Dictionary<Int32, IList<SectionInstanceInfo>> shipSectionInstances)
        {
            FleetInfo fleetInfo = this._db.GetFleetInfo(fleetid);
            if (fleetInfo == null || fleetInfo.SystemID == 0)
            {
                return availpoints;
            }
            if (availpoints <= 0)
            {
                return availpoints;
            }
            List<ShipInfo> list = this._db.GetShipInfoByFleetID(fleetInfo.ID, true).ToList<ShipInfo>();
            int num = 0;
            foreach (ShipInfo current in list)
            {
                //switched to the Performance version
                int[] healthAndHealthMax = StarFleet.StarFleet.GetHealthAndHealthMax(this.BaseGameSession, current.DesignInfo, current.ID, shipSectionInstances);
                int num2 = healthAndHealthMax[1] - healthAndHealthMax[0];
                if (availpoints <= 0)
                {
                    break;
                }
                if (num2 > 0)
                {
                    if (num2 > availpoints)
                    {
                        num2 = availpoints;
                    }
                    if (num2 > availpoints)
                    {
                        break;
                    }
                    num += num2;
                    availpoints -= num2;

                    //switched to the Performance version
                    StarFleet.StarFleet.RepairShip(this.App, current, num2, shipSectionInstances);
                }
            }
            this.UseSystemRepairPoints(fleetInfo.SystemID, fleetInfo.PlayerID, num, shipSectionInstances);
            return availpoints - num;
        }

        /// <summary>Uses repair points within the specified system for the specified player</summary>
        /// <param name="systemid">Unique ID of the system in which to use repair points</param>
        /// <param name="playerid">Unique ID of the playe for whom to use repair points</param>
        /// <param name="points">Total points of repair to be consumed</param>
        /// <param name="shipSectionInstances">Pre-indexed collection of ship section instances to compute on</param>
        public void UseSystemRepairPoints(int systemid, int playerid, int points, Dictionary<Int32, IList<SectionInstanceInfo>> shipSectionInstances)
        {
            List<ColonyInfo> list = (
                from x in this._db.GetColonyInfosForSystem(systemid)
                where x.PlayerID == playerid
                select x).ToList<ColonyInfo>();
            foreach (ColonyInfo current in list)
            {
                if (points <= 0)
                {
                    return;
                }
                int num = current.RepairPoints - points;
                if (num < 0)
                {
                    num = 0;
                }
                points -= current.RepairPoints;
                current.RepairPoints = num;
                this.App.GameDatabase.UpdateColony(current);
            }
            List<StationInfo> list2 = this._db.GetStationForSystemAndPlayer(systemid, playerid).ToList<StationInfo>();
            foreach (StationInfo current2 in list2)
            {
                IList<SectionInstanceInfo> list3 = shipSectionInstances[current2.ShipID];
                if (list3 != null)
                    foreach (SectionInstanceInfo current3 in list3)
                    {
                        if (points <= 0)
                        {
                            return;
                        }
                        int num2 = current3.RepairPoints - points;
                        if (num2 < 0)
                        {
                            num2 = 0;
                        }
                        points -= current3.RepairPoints;
                        current3.RepairPoints -= num2;
                        this.App.GameDatabase.UpdateSectionInstance(current3);
                    }
            }
            List<FleetInfo> list4 = this._db.GetFleetInfoBySystemID(systemid, FleetType.FL_NORMAL).ToList<FleetInfo>();
            foreach (FleetInfo current4 in list4)
            {
                List<ShipInfo> list5 = this._db.GetShipInfoByFleetID(current4.ID, false).ToList<ShipInfo>();
                foreach (ShipInfo current5 in list5)
                {
                    IList<SectionInstanceInfo> list6 = shipSectionInstances[current5.ID];
                    if (list6 != null)
                        foreach (SectionInstanceInfo current6 in list6)
                        {
                            if (points <= 0)
                            {
                                return;
                            }
                            int num3 = current6.RepairPoints - points;
                            if (num3 < 0)
                            {
                                num3 = 0;
                            }
                            points -= current6.RepairPoints;
                            current6.RepairPoints -= num3;
                            this.App.GameDatabase.UpdateSectionInstance(current6);
                        }
                }
            }
        }
        #endregion
    }
}