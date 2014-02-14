using Kerberos.Sots.Data;
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
	internal class PirateCombatAI : CombatAI
	{
		private List<BoardingPodLaunchControl> m_BoardingPodControls;
		private List<DroneLaunchControl> m_DroneControls;
		private bool m_IsPirateBase;
		public PirateCombatAI(App game, Player player, bool playerControlled, Kerberos.Sots.GameStates.StarSystem starSystem, Dictionary<int, DiplomacyState> diploStates) : base(game, player, playerControlled, starSystem, diploStates, false)
		{
			base.SetAIType(OverallAIType.PIRATE);
			this.m_BoardingPodControls = new List<BoardingPodLaunchControl>();
			this.m_DroneControls = new List<DroneLaunchControl>();
			this.m_IsPirateBase = (game.GameDatabase.GetPirateBaseInfos().FirstOrDefault((PirateBaseInfo x) => x.SystemId == starSystem.SystemID) != null);
		}
		public override void Update(List<IGameObject> objs)
		{
			base.Update(objs);
			if (this.m_IsPirateBase)
			{
				return;
			}
			this.m_BoardingPodControls = this.m_ShipWeaponControls.OfType<BoardingPodLaunchControl>().ToList<BoardingPodLaunchControl>();
			this.m_DroneControls = this.m_ShipWeaponControls.OfType<DroneLaunchControl>().ToList<DroneLaunchControl>();
			foreach (BoardingPodLaunchControl current in this.m_BoardingPodControls)
			{
				if (current.ControlledShip != null)
				{
					if (current.ControlledShip.Target is Ship)
					{
						Ship ship = current.ControlledShip.Target as Ship;
						current.DisableWeaponFire = (ship.ShipRole != ShipRole.FREIGHTER);
					}
					else
					{
						current.DisableWeaponFire = true;
					}
				}
			}
			foreach (DroneLaunchControl current2 in this.m_DroneControls)
			{
				if (current2.ControlledShip != null)
				{
					if (current2.ControlledShip.Target is Ship)
					{
						Ship ship2 = current2.ControlledShip.Target as Ship;
						current2.DisableWeaponFire = (ship2.ShipRole == ShipRole.FREIGHTER && this.m_BoardingPodControls.Count > 0);
					}
					else
					{
						current2.DisableWeaponFire = true;
					}
				}
			}
			bool flag = this.m_Enemy.Any((Ship x) => x.ShipRole == ShipRole.FREIGHTER);
			if (!this.m_bEnemyShipsInSystem || !flag)
			{
				foreach (TaskGroup current3 in this.m_TaskGroups)
				{
					if (!(current3.Objective is RetreatObjective))
					{
						TacticalObjective retreatObjective = base.GetRetreatObjective(current3);
						if (retreatObjective is RetreatObjective)
						{
							RetreatObjective retreatObjective2 = retreatObjective as RetreatObjective;
							retreatObjective2.ResetRetreatPosition(current3);
							current3.Objective = retreatObjective2;
						}
					}
				}
			}
		}
	}
}
