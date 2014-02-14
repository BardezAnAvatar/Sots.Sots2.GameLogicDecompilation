using Kerberos.Sots.Engine;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Combat
{
	internal class SlaverCombatAI : CombatAI
	{
		private List<Ship> m_SlaverShips;
		public SlaverCombatAI(App game, Player player, bool playerControlled, Kerberos.Sots.GameStates.StarSystem starSystem, Dictionary<int, DiplomacyState> diploStates) : base(game, player, playerControlled, starSystem, diploStates, false)
		{
			base.SetAIType(OverallAIType.SLAVER);
			this.m_SlaverShips = new List<Ship>();
		}
		public override void Update(List<IGameObject> objs)
		{
			base.Update(objs);
			int num = 0;
			foreach (IGameObject current in objs)
			{
				if (current is Ship)
				{
					Ship ship = current as Ship;
					if (ship.Player == this.m_Player && ship.Active && !ship.IsDestroyed)
					{
						if (ship.IsBattleRider && ship.BattleRiderType == BattleRiderTypes.assaultshuttle)
						{
							Ship ship2 = this.m_Friendly.FirstOrDefault((Ship x) => x.DatabaseID == ship.ParentDatabaseID);
							if (ship2 != null && ship2.CombatStance != CombatStance.RETREAT)
							{
								num++;
							}
						}
						if (ship.IsWraithAbductor && ship.CombatStance != CombatStance.RETREAT)
						{
							num++;
						}
						if (!this.m_SlaverShips.Contains(ship))
						{
							this.m_SlaverShips.Add(ship);
							if (ship.WeaponControls != null)
							{
								List<AssaultShuttleLaunchControl> list = ship.WeaponControls.OfType<AssaultShuttleLaunchControl>().ToList<AssaultShuttleLaunchControl>();
								foreach (AssaultShuttleLaunchControl current2 in list)
								{
									current2.SetMinAttackRange(Math.Min(5000f, current2.GetMinAttackRange()));
								}
							}
						}
					}
				}
			}
			if (this.m_Friendly.Count <= 0 || num != 0)
			{
				if (this.m_Planets.Sum((StellarBody x) => x.Population) != 0.0)
				{
					return;
				}
			}
			foreach (TaskGroup current3 in this.m_TaskGroups)
			{
				if (!(current3.Objective is RetreatObjective))
				{
					current3.Objective = base.GetRetreatObjective(current3);
				}
			}
		}
		public override DiplomacyState GetDiplomacyState(int playerID)
		{
			if (playerID != this.m_Player.ID)
			{
				return DiplomacyState.WAR;
			}
			return DiplomacyState.PEACE;
		}
	}
}
