using Kerberos.Sots.Data;
using Kerberos.Sots.Data.ShipFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Combat
{
	internal class NPCFactionCombatAI : CombatAI
	{
		protected int m_SystemID;
		protected List<CombatAIController> m_CombatAIControls;
		internal int NumAddedResources;
		internal int NumPlanetStruckAsteroids;
		internal List<int> PlanetsAttackedByNPC = new List<int>();
		public List<CombatAIController> CombatAIControllers
		{
			get
			{
				return this.m_CombatAIControls;
			}
		}
		public NPCFactionCombatAI(App game, Player player, bool playerControlled, int systemID, Kerberos.Sots.GameStates.StarSystem starSystem, Dictionary<int, DiplomacyState> diploStates) : base(game, player, playerControlled, starSystem, diploStates, false)
		{
			this.m_SystemID = systemID;
			this.m_CombatAIControls = new List<CombatAIController>();
		}
		public override void Shutdown()
		{
			foreach (CombatAIController current in this.m_CombatAIControls)
			{
				current.Terminate();
			}
			this.m_CombatAIControls.Clear();
			base.Shutdown();
		}
		public override void ObjectRemoved(IGameObject obj)
		{
			base.ObjectRemoved(obj);
			foreach (CombatAIController current in this.m_CombatAIControls)
			{
				if (current.GetShip() == obj)
				{
					current.ObjectRemoved(obj);
					this.m_CombatAIControls.Remove(current);
					if (!(current.GetType() == typeof(MeteorCombatAIControl)))
					{
						break;
					}
					this.NumAddedResources += ((MeteorCombatAIControl)current).m_AddedResources;
					if (((MeteorCombatAIControl)current).m_StruckPlanet)
					{
						this.NumPlanetStruckAsteroids++;
						break;
					}
					break;
				}
			}
			foreach (CombatAIController current2 in this.m_CombatAIControls)
			{
				current2.ObjectRemoved(obj);
			}
		}
		public override bool VictoryConditionsAreMet()
		{
			bool result = true;
			if (this.m_CombatAIControls.Count == 0)
			{
				return false;
			}
			foreach (CombatAIController current in this.m_CombatAIControls)
			{
				if (!current.VictoryConditionIsMet())
				{
					result = false;
					break;
				}
			}
			return result;
		}
		public override void Update(List<IGameObject> objs)
		{
			if (!App.m_bAI_Enabled || this.m_bIsHumanPlayerControlled)
			{
				return;
			}
			List<IGameObject> list = new List<IGameObject>();
			foreach (IGameObject current in objs)
			{
				if (current is Ship)
				{
					Ship ship = current as Ship;
					list.Add(ship);
					if (ship.Player == this.m_Player)
					{
						if (!this.m_CombatAIControls.Any((CombatAIController x) => x.GetShip() == ship))
						{
							CombatAIController combatAIController = this.CreateNewCombatAIController(ship);
							if (combatAIController != null)
							{
								combatAIController.Initialize();
								this.m_CombatAIControls.Add(combatAIController);
							}
						}
					}
				}
				else
				{
					if (current is StellarBody)
					{
						list.Add(current);
					}
					else
					{
						if (current is StarModel)
						{
							list.Add(current);
						}
					}
				}
			}
			foreach (CombatAIController current2 in this.m_CombatAIControls)
			{
				if (current2.NeedsAParent())
				{
					current2.FindParent(this.m_CombatAIControls);
				}
				if ((current2.GetShip().Active && Ship.IsActiveShip(current2.GetShip())) || current2 is MeteorCombatAIControl)
				{
					if (current2.RequestingNewTarget())
					{
						current2.FindNewTarget(list);
					}
					if (current2.GetTarget() != null && current2.GetTarget() is StellarBody && !this.PlanetsAttackedByNPC.Contains(current2.GetTarget().ObjectID))
					{
						this.PlanetsAttackedByNPC.Add(current2.GetTarget().ObjectID);
					}
					current2.OnThink();
				}
			}
		}
		private CombatAIController CreateNewCombatAIController(Ship ship)
		{
			CombatAIController result = null;
			switch (ship.CombatAI)
			{
			case SectionEnumerations.CombatAiType.TrapDrone:
				result = new ColonyTrapDroneControl(this.m_Game, ship, this, this.m_FleetID);
				break;
			case SectionEnumerations.CombatAiType.Swarmer:
				result = new SwarmerAttackerControl(this.m_Game, ship, SwarmerAttackerType.SWARMER);
				break;
			case SectionEnumerations.CombatAiType.SwarmerGuardian:
				result = new SwarmerAttackerControl(this.m_Game, ship, SwarmerAttackerType.GAURDIAN);
				break;
			case SectionEnumerations.CombatAiType.SwarmerHive:
				result = new SwarmerHiveControl(this.m_Game, ship, this.m_SystemID);
				break;
			case SectionEnumerations.CombatAiType.SwarmerQueenLarva:
				result = new SwarmerQueenLarvaControl(this.m_Game, ship);
				break;
			case SectionEnumerations.CombatAiType.SwarmerQueen:
				result = new SwarmerQueenControl(this.m_Game, ship, this.m_SystemID);
				break;
			case SectionEnumerations.CombatAiType.VonNeumannCollectorMotherShip:
				result = new VonNeumannMomControl(this.m_Game, ship, this.m_FleetID);
				break;
			case SectionEnumerations.CombatAiType.VonNeumannCollectorProbe:
				result = new VonNeumannChildControl(this.m_Game, ship);
				break;
			case SectionEnumerations.CombatAiType.VonNeumannSeekerMotherShip:
				result = new VonNeumannSeekerControl(this.m_Game, ship);
				break;
			case SectionEnumerations.CombatAiType.VonNeumannBerserkerMotherShip:
				result = new VonNeumannBerserkerControl(this.m_Game, ship);
				break;
			case SectionEnumerations.CombatAiType.VonNeumannNeoBerserker:
				result = new VonNeumannNeoBerserkerControl(this.m_Game, ship);
				break;
			case SectionEnumerations.CombatAiType.VonNeumannDisc:
				result = new VonNeumannDiscControl(this.m_Game, ship);
				break;
			case SectionEnumerations.CombatAiType.VonNeumannPyramid:
				result = new VonNeumannPyramidControl(this.m_Game, ship);
				break;
			case SectionEnumerations.CombatAiType.VonNeumannPlanetKiller:
				result = new VonNeumannPlanetKillerCombatAIControl(this.m_Game, ship, this.m_SystemID);
				break;
			case SectionEnumerations.CombatAiType.LocustMoon:
				result = new LocustMoonControl(this.m_Game, ship, this.m_FleetID);
				break;
			case SectionEnumerations.CombatAiType.LocustWorld:
				result = new LocustNestControl(this.m_Game, ship, this.m_FleetID);
				break;
			case SectionEnumerations.CombatAiType.LocustFighter:
				result = new LocustFighterControl(this.m_Game, ship);
				break;
			case SectionEnumerations.CombatAiType.SystemKiller:
				result = new SystemKillerCombatAIControl(this.m_Game, ship);
				break;
			case SectionEnumerations.CombatAiType.MorrigiRelic:
				result = new MorrigiRelicControl(this.m_Game, ship, this.m_FleetID);
				break;
			case SectionEnumerations.CombatAiType.MorrigiCrow:
				result = new MorrigiCrowControl(this.m_Game, ship);
				break;
			case SectionEnumerations.CombatAiType.Meteor:
				result = new MeteorCombatAIControl(this.m_Game, ship);
				break;
			case SectionEnumerations.CombatAiType.Comet:
				result = new CometCombatAIControl(this.m_Game, ship);
				break;
			case SectionEnumerations.CombatAiType.Specter:
				result = new SpecterCombatAIControl(this.m_Game, ship);
				break;
			case SectionEnumerations.CombatAiType.Gardener:
				result = new GardenerCombatAIControl(this.m_Game, ship);
				break;
			case SectionEnumerations.CombatAiType.Protean:
				result = new ProteanCombatAIControl(this.m_Game, ship);
				break;
			case SectionEnumerations.CombatAiType.CommandMonitor:
				result = new CommandMonitorCombatAIControl(this.m_Game, ship, this.m_Game.GameDatabase.GetEncounterIDAtSystem(EasterEgg.EE_ASTEROID_MONITOR, this.m_SystemID));
				break;
			case SectionEnumerations.CombatAiType.NormalMonitor:
				result = new NormalMonitorCombatAIControl(this.m_Game, ship);
				break;
			case SectionEnumerations.CombatAiType.GhostShip:
				result = new GhostShipCombatAIControl(this.m_Game, ship);
				break;
			}
			return result;
		}
		public void HandlePostCombat(List<Player> playersInCombat, List<int> fleetIdsInCombat, int systemId)
		{
			if (this.m_Game.Game.ScriptModules.MorrigiRelic != null && this.m_Game.Game.ScriptModules.MorrigiRelic.PlayerID == this.m_Player.ID)
			{
				FleetInfo relicFleet = null;
				List<FleetInfo> list = new List<FleetInfo>();
				foreach (int current in fleetIdsInCombat)
				{
					FleetInfo fleetInfo = this.m_Game.GameDatabase.GetFleetInfo(current);
					if (fleetInfo != null)
					{
						if (fleetInfo.PlayerID == this.m_Player.ID)
						{
							relicFleet = fleetInfo;
						}
						else
						{
							list.Add(fleetInfo);
						}
					}
				}
				if (relicFleet == null)
				{
					return;
				}
				List<MorrigiRelicInfo> source = this.m_Game.GameDatabase.GetMorrigiRelicInfos().ToList<MorrigiRelicInfo>();
				MorrigiRelicInfo morrigiRelicInfo = source.FirstOrDefault((MorrigiRelicInfo x) => x.FleetId == relicFleet.ID);
				if (morrigiRelicInfo == null || !morrigiRelicInfo.IsAggressive)
				{
					return;
				}
				List<ShipInfo> list2 = new List<ShipInfo>();
				bool flag = true;
				foreach (CombatAIController current2 in this.m_CombatAIControls)
				{
					if (!current2.VictoryConditionIsMet())
					{
						flag = false;
						break;
					}
					if (current2 is MorrigiRelicControl && current2.GetShip() != null && !current2.GetShip().IsDestroyed)
					{
						list2.Add(this.m_Game.GameDatabase.GetShipInfo(current2.GetShip().DatabaseID, true));
					}
				}
				if (!flag)
				{
					return;
				}
				List<Player> list3 = new List<Player>();
				foreach (Player p in playersInCombat)
				{
					if (p.ID != this.m_Player.ID)
					{
						if (list.Any((FleetInfo x) => x.PlayerID == p.ID))
						{
							list3.Add(p);
						}
					}
				}
				this.m_Game.Game.ScriptModules.MorrigiRelic.ApplyRewardsToPlayers(this.m_Game, morrigiRelicInfo, list2, list3);
			}
		}
	}
}
