using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using Kerberos.Sots;
using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.PlayerFramework;

using Bardez.Projects.SwordOfTheStars.SotS2.Utility;

using Original = Kerberos.Sots.Strategy;

namespace Bardez.Projects.SwordOfTheStars.SotS2.Fixes.Sots.Strategy
{
    /// <summary>Contains performance enhancements to Kerberos.Sots.Strategy.StrategicAI</summary>
	internal class StrategicAI
    {
        #region Fields
        /// <summary>Base class instance being extended</summary>
        protected Original.StrategicAI BaseInstance;
        #endregion


        #region Kerberos GameSession reflection-exposed members
        #region Kerbeos Fields
        protected GameDatabase _db
        {
            get { return ReflectionHelper.PrivateField<Original.StrategicAI, GameDatabase>(this.BaseInstance, "_db"); }
        }

        protected Player _player
        {
            get { return ReflectionHelper.PrivateField<Original.StrategicAI, Player>(this.BaseInstance, "_player"); }
        }

        protected int _dropInActivationTurn
        {
            get { return ReflectionHelper.PrivateField<Original.StrategicAI, int>(this.BaseInstance, "_dropInActivationTurn"); }
        }

        protected bool m_IsOldSave
        {
            get { return ReflectionHelper.PrivateField<Original.StrategicAI, bool>(this.BaseInstance, "m_IsOldSave"); }
        }

        protected Original.MissionManager _missionMan
        {
            get { return ReflectionHelper.PrivateField<Original.StrategicAI, Original.MissionManager>(this.BaseInstance, "_missionMan"); }
        }

        protected Original.GameSession _game
        {
            get { return ReflectionHelper.PrivateField<Original.StrategicAI, Original.GameSession>(this.BaseInstance, "_game"); }
        }
        #endregion


        #region Kerberos Properties
        public Original.AIStance? LastStance
        {
            get { return this.BaseInstance.LastStance; }
            set { ReflectionHelper.PublicProperty<Original.StrategicAI, Original.AIStance?>(this.BaseInstance, "LastStance", value); }
        }

        public Original.GameSession Game
        {
            get { return this.BaseInstance.Game; }
        }
        #endregion


        #region Kerberos Methods
        protected static void TraceVerbose(string message)
        {
            MethodInfo mi = ReflectionHelper.PrivateStaticMethod<Original.StrategicAI>("TraceVerbose");
            mi.Invoke(null, new Object[] { message });
        }

        protected void ResetData()
        {
            MethodInfo mi = ReflectionHelper.PrivateMethod<Original.StrategicAI>("ResetData");
            mi.Invoke(this.BaseInstance, null);
        }

        protected void UpdateEmpire(AIInfo aiInfo, out double startOfTurnSavings)
        {
            MethodInfo mi = ReflectionHelper.PrivateMethod<Original.StrategicAI>("UpdateEmpire");

            Object[] parameters = new Object[] { aiInfo, null };
            mi.Invoke(this.BaseInstance, parameters);
            startOfTurnSavings = (Double)(parameters[1]);
        }

        protected void UpdateStance(AIInfo aiInfo, Original.StrategicAI.UpdateInfo updateInfo)
        {
            MethodInfo mi = ReflectionHelper.PrivateMethod<Original.StrategicAI>("UpdateStance");
            mi.Invoke(this.BaseInstance, new Object[] { aiInfo, updateInfo });
        }

        protected void ManageColonies(AIInfo aiInfo)
        {
            MethodInfo mi = ReflectionHelper.PrivateMethod<Original.StrategicAI>("ManageColonies");
            mi.Invoke(this.BaseInstance, new Object[] { aiInfo });
        }

        public void DesignShips(AIInfo aiInfo)
        {
            this.BaseInstance.DesignShips(aiInfo);
        }

        protected void ManageStations(AIInfo aiInfo)
        {
            MethodInfo mi = ReflectionHelper.PrivateMethod<Original.StrategicAI>("ManageStations");
            mi.Invoke(this.BaseInstance, new Object[] { aiInfo });
        }

        protected void ManageFleets(AIInfo aiInfo)
        {
            MethodInfo mi = ReflectionHelper.PrivateMethod<Original.StrategicAI>("ManageFleets");
            mi.Invoke(this.BaseInstance, new Object[] { aiInfo });
        }

        protected void ManageDefenses(AIInfo aiInfo)
        {
            MethodInfo mi = ReflectionHelper.PrivateMethod<Original.StrategicAI>("ManageDefenses");
            mi.Invoke(this.BaseInstance, new Object[] { aiInfo });
        }

        protected void ManageReserves(AIInfo aiInfo)
        {
            MethodInfo mi = ReflectionHelper.PrivateMethod<Original.StrategicAI>("ManageReserves");
            mi.Invoke(this.BaseInstance, new Object[] { aiInfo });
        }

        protected void SetFleetOrders(AIInfo aiInfo)
        {
            MethodInfo mi = ReflectionHelper.PrivateMethod<Original.StrategicAI>("SetFleetOrders");
            mi.Invoke(this.BaseInstance, new Object[] { aiInfo });
        }

        protected void SetResearchOrders(AIInfo aiInfo)
        {
            MethodInfo mi = ReflectionHelper.PrivateMethod<Original.StrategicAI>("SetResearchOrders");
            mi.Invoke(this.BaseInstance, new Object[] { aiInfo });
        }

        protected void UpdateRelations(AIInfo aiInfo, Original.StrategicAI.UpdateInfo updateInfo)
        {
            MethodInfo mi = ReflectionHelper.PrivateMethod<Original.StrategicAI>("UpdateRelations");
            mi.Invoke(this.BaseInstance, new Object[] { aiInfo, updateInfo });
        }

        protected void OfferTreaties(AIInfo aiInfo, Original.StrategicAI.UpdateInfo updateInfo)
        {
            MethodInfo mi = ReflectionHelper.PrivateMethod<Original.StrategicAI>("OfferTreaties");
            mi.Invoke(this.BaseInstance, new Object[] { aiInfo, updateInfo });
        }

        protected void FinalizeEmpire(AIInfo aiInfo, double startOfTurnSavings)
        {
            MethodInfo mi = ReflectionHelper.PrivateMethod<Original.StrategicAI>("FinalizeEmpire");
            mi.Invoke(this.BaseInstance, new Object[] { aiInfo, startOfTurnSavings });
        }

        protected void GatherAllResources(Original.AIStance stance)
        {
            MethodInfo mi = ReflectionHelper.PrivateMethod<Original.StrategicAI>("GatherAllResources");
            mi.Invoke(this.BaseInstance, new Object[] { stance });
        }

        protected void GatherAllTasks()
        {
            MethodInfo mi = ReflectionHelper.PrivateMethod<Original.StrategicAI>("GatherAllTasks");
            mi.Invoke(this.BaseInstance, null);
        }

        protected void ApplyScores(Original.AIStance stance)
        {
            MethodInfo mi = ReflectionHelper.PrivateMethod<Original.StrategicAI>("ApplyScores");
            mi.Invoke(this.BaseInstance, new Object[] { stance });
        }

        protected void AssignFleetsToTasks(Original.AIStance stance)
        {
            MethodInfo mi = ReflectionHelper.PrivateMethod<Original.StrategicAI>("AssignFleetsToTasks");
            mi.Invoke(this.BaseInstance, new Object[] { stance });
        }

        protected void BuildFleets(AIInfo aiInfo)
        {
            MethodInfo mi = ReflectionHelper.PrivateMethod<Original.StrategicAI>("BuildFleets");
            mi.Invoke(this.BaseInstance, new Object[] { aiInfo });
        }

        protected void ManageDebtLevels()
        {
            MethodInfo mi = ReflectionHelper.PrivateMethod<Original.StrategicAI>("ManageDebtLevels");
            mi.Invoke(this.BaseInstance, null);
        }
        #endregion
        #endregion


        #region Construction
        /// <summary>Definition constructor</summary>
        /// <param name="original">Instance of Kerberos.Sots.Strategy.StrategicAI to extend</param>
        public StrategicAI(Original.StrategicAI original)
        {
            this.BaseInstance = original;
        }
        #endregion


        #region Performance Enhancements
        /// <summary>Performs end-of-turn updates to the AI player</summary>
        /// <param name="updateInfo">UpdateInfo to process</param>
        /// <param name="shipSectionInstances">Pre-indexed collection of ship sections</param>
        public void Update(Original.StrategicAI.UpdateInfo updateInfo, Dictionary<Int32, IList<SectionInstanceInfo>> shipSectionInstances)
		{
			AIInfo aIInfo = this._db.GetAIInfo(this._player.ID);
            StrategicAI.TraceVerbose(string.Format("---- Processing AI for player {0} ----", aIInfo.PlayerInfo));
			if (this._db.GetTurnCount() < this._dropInActivationTurn)
			{
                StrategicAI.TraceVerbose(string.Format("AI processing skipped; drop-in moratorium is in place until turn {0}.", this._dropInActivationTurn));
				return;
			}
			if (aIInfo.PlayerInfo.isDefeated || (!aIInfo.PlayerInfo.isStandardPlayer && !aIInfo.PlayerInfo.isAIRebellionPlayer))
			{
                StrategicAI.TraceVerbose("AI processing skipped; player is exempt.");
				return;
			}
			this.LastStance = new Original.AIStance?(aIInfo.Stance);
			if (this.m_IsOldSave)
			{
				this._missionMan.Update();
			}
			this.ResetData();
			double startOfTurnSavings;
			this.UpdateEmpire(aIInfo, out startOfTurnSavings);
			this.UpdateStance(aIInfo, updateInfo);
			this.ManageColonies(aIInfo);
			this.DesignShips(aIInfo);
            this.DoRepairs(aIInfo, shipSectionInstances);
			this.ManageStations(aIInfo);
			if (this.m_IsOldSave)
			{
				this.ManageFleets(aIInfo);
				this.ManageDefenses(aIInfo);
				this.ManageReserves(aIInfo);
				this.SetFleetOrders(aIInfo);
			}
			else
			{
				this.ManageDefenses(aIInfo);
			}
			this.SetResearchOrders(aIInfo);
			this.UpdateRelations(aIInfo, updateInfo);
			this.OfferTreaties(aIInfo, updateInfo);
			this.FinalizeEmpire(aIInfo, startOfTurnSavings);
			if (!this.m_IsOldSave)
			{
				this.GatherAllResources(aIInfo.Stance);
				this.GatherAllTasks();
				this.ApplyScores(aIInfo.Stance);
				this.AssignFleetsToTasks(aIInfo.Stance);
				this.BuildFleets(aIInfo);
				this.ManageDebtLevels();
			}
			if (this._player.Faction.Name == "loa")
			{
				List<FleetInfo> source = this._db.GetFleetInfosByPlayerID(this._player.ID, FleetType.FL_NORMAL).ToList<FleetInfo>();
				List<LoaFleetComposition> list = (
					from x in this._db.GetLoaFleetCompositions()
					where x.PlayerID == this._player.ID
					select x).ToList<LoaFleetComposition>();
				foreach (LoaFleetComposition comp in list)
				{
					if (!source.Any((FleetInfo x) => x.FleetConfigID.HasValue && x.FleetConfigID.Value == comp.ID))
					{
						this._db.DeleteLoaFleetCompositon(comp.ID);
					}
				}
			}
			if (App.Log.Level >= LogLevel.Verbose)
			{
				StringBuilder stringBuilder = new StringBuilder();
				Original.DesignLab.PrintPlayerDesignSummary(stringBuilder, this.Game.App, this._player.ID, false);
                StrategicAI.TraceVerbose(stringBuilder.ToString());
			}
		}

        /// <summary>Repairs the AI's ships</summary>
        /// <param name="aiInfo">AIInfo used to process</param>
        /// <param name="shipSectionInstances">Pre-indexed collection of ship sections</param>
        private void DoRepairs(AIInfo aiInfo, Dictionary<Int32, IList<SectionInstanceInfo>> shipSectionInstances)
        {
            foreach (StarSystemInfo current in this._db.GetStarSystemInfos())
            {
                GameSession performanceGameSession = new GameSession(this._game);
                performanceGameSession.RepairFleetsAtSystem(current.ID, this._player.ID, shipSectionInstances);
                //this._game.RepairFleetsAtSystem(current.ID, this._player.ID);
            }
        }
        #endregion
    }
}