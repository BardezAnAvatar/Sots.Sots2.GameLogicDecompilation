using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Combat
{
	internal class EnemyGroup
	{
		public List<Ship> m_Ships;
		public Vector3 m_LastKnownPosition;
		public Vector3 m_LastKnownHeading;
		public Vector3 m_LastKnownDestination;
		public EnemyGroup()
		{
			this.m_Ships = new List<Ship>();
			this.m_LastKnownPosition = Vector3.Zero;
			this.m_LastKnownHeading = Vector3.UnitX;
			this.m_LastKnownDestination = Vector3.Zero;
		}
		public void PurgeDestroyed()
		{
			List<Ship> list = new List<Ship>();
			foreach (Ship current in this.m_Ships)
			{
				if (!Ship.IsActiveShip(current))
				{
					list.Add(current);
				}
			}
			foreach (Ship current2 in list)
			{
				this.m_Ships.Remove(current2);
			}
		}
		public Vector3 GetAverageVelocity(CombatAI detectingAi)
		{
			Vector3 vector = Vector3.Zero;
			int num = 0;
			foreach (Ship current in this.m_Ships)
			{
				if (detectingAi.IsShipDetected(current))
				{
					vector += current.Maneuvering.Velocity;
					num++;
				}
			}
			if (num > 0)
			{
				vector /= (float)num;
				this.m_LastKnownHeading = vector;
			}
			return vector;
		}
		public bool IsDetected(CombatAI detectingAi)
		{
			foreach (Ship current in this.m_Ships)
			{
				if (detectingAi.IsShipDetected(current))
				{
					return true;
				}
			}
			return false;
		}
		public Ship GetClosestShip(Vector3 fromCoords, CombatAI detectingAi)
		{
			Ship ship = null;
			float num = 0f;
			foreach (Ship current in this.m_Ships)
			{
				if (detectingAi.IsShipDetected(current))
				{
					float lengthSquared = (fromCoords - current.Maneuvering.Position).LengthSquared;
					if (ship == null || lengthSquared < num)
					{
						ship = current;
						num = lengthSquared;
					}
				}
			}
			return ship;
		}
		public Ship GetClosestShip(Vector3 fromCoords, float range)
		{
			Ship result = null;
			float num = range * range;
			foreach (Ship current in this.m_Ships)
			{
				float lengthSquared = (fromCoords - current.Maneuvering.Position).LengthSquared;
				if (lengthSquared < num)
				{
					result = current;
					num = lengthSquared;
				}
			}
			return result;
		}
		public float GetGroupRadius()
		{
			if (this.m_Ships.Count == 0)
			{
				return 0f;
			}
			Vector3 vector = Vector3.Zero;
			foreach (Ship current in this.m_Ships)
			{
				vector += current.Maneuvering.Position;
			}
			vector /= (float)this.m_Ships.Count;
			float num = 0f;
			foreach (Ship current2 in this.m_Ships)
			{
				num = Math.Max((vector - current2.Maneuvering.Position).LengthSquared, num);
			}
			return (float)Math.Sqrt((double)num);
		}
		public bool IsFreighterEnemyGroup()
		{
			return this.m_Ships.Any((Ship x) => x.IsNPCFreighter);
		}
		public bool IsEncounterEnemyGroup(CombatAI ai)
		{
			return this.m_Ships.Any((Ship x) => ai.IsEncounterPlayer(x.Player.ID));
		}
		public bool IsHigherPriorityThan(EnemyGroup eg, CombatAI ai, bool defendObjAsking = false)
		{
			if (eg == null)
			{
				return true;
			}
			EnemyGroupData enemyGroupData = EnemyGroup.GetEnemyGroupData(ai, this, ai.PlanetsInSystem);
			EnemyGroupData enemyGroupData2 = EnemyGroup.GetEnemyGroupData(ai, eg, ai.PlanetsInSystem);
			if (ai.GetAIType() == OverallAIType.PIRATE)
			{
				if (enemyGroupData.IsFreighter || enemyGroupData2.IsFreighter)
				{
					if (enemyGroupData.IsFreighter && !enemyGroupData2.IsFreighter)
					{
						return true;
					}
					if (!enemyGroupData.IsFreighter && enemyGroupData2.IsFreighter)
					{
						return false;
					}
					float num = 3.40282347E+38f;
					float num2 = 3.40282347E+38f;
					bool flag = false;
					foreach (TaskGroup current in ai.GetTaskGroups())
					{
						Vector3 baseGroupPosition = current.GetBaseGroupPosition();
						Ship closestShip = this.GetClosestShip(baseGroupPosition, 100000f);
						Ship closestShip2 = eg.GetClosestShip(baseGroupPosition, 100000f);
						if (closestShip != null && closestShip2 != null)
						{
							num = Math.Min((closestShip.Position - baseGroupPosition).LengthSquared, num);
							num2 = Math.Min((closestShip2.Position - baseGroupPosition).LengthSquared, num2);
							flag = true;
						}
					}
					if (flag)
					{
						return num < num2;
					}
				}
				return CombatAI.AssessGroupStrength(this.m_Ships) > CombatAI.AssessGroupStrength(eg.m_Ships);
			}
			if (enemyGroupData.IsEncounter || enemyGroupData2.IsEncounter)
			{
				return (enemyGroupData.IsEncounter && !enemyGroupData2.IsEncounter) || (enemyGroupData.IsEncounter && !enemyGroupData.IsStation);
			}
			if (ai.OwnsSystem)
			{
				if (enemyGroupData.IsAttackingPlanetOrStation || enemyGroupData2.IsAttackingPlanetOrStation || defendObjAsking)
				{
					if (enemyGroupData.IsAttackingPlanetOrStation && !enemyGroupData2.IsAttackingPlanetOrStation)
					{
						return true;
					}
					if (!enemyGroupData.IsAttackingPlanetOrStation && enemyGroupData2.IsAttackingPlanetOrStation)
					{
						return false;
					}
					if (enemyGroupData.DistanceFromColony > 0f && enemyGroupData2.DistanceFromColony > 0f)
					{
						return enemyGroupData.DistanceFromColony < enemyGroupData2.DistanceFromColony;
					}
				}
				if (enemyGroupData.NumAggressive > 0 || enemyGroupData2.NumAggressive > 0)
				{
					return enemyGroupData.NumAggressive > enemyGroupData2.NumAggressive;
				}
				if (enemyGroupData.NumPassive > 0 || enemyGroupData2.NumPassive > 0)
				{
					return enemyGroupData.NumPassive > enemyGroupData2.NumPassive;
				}
				if (enemyGroupData.NumCivilian > 0 || enemyGroupData2.NumCivilian > 0)
				{
					return enemyGroupData.NumCivilian > enemyGroupData2.NumCivilian;
				}
				if (enemyGroupData.NumUnarmed > 0 || enemyGroupData2.NumUnarmed > 0)
				{
					return enemyGroupData.NumUnarmed > enemyGroupData2.NumUnarmed;
				}
			}
			else
			{
				if (enemyGroupData.IsStation || enemyGroupData2.IsStation)
				{
					if (enemyGroupData.IsAttackingPlanetOrStation && !enemyGroupData2.IsAttackingPlanetOrStation)
					{
						return true;
					}
					if (!enemyGroupData.IsAttackingPlanetOrStation && enemyGroupData2.IsAttackingPlanetOrStation)
					{
						return false;
					}
					if (enemyGroupData.DistanceFromColony > 0f && enemyGroupData2.DistanceFromColony > 0f)
					{
						return enemyGroupData.DistanceFromColony < enemyGroupData2.DistanceFromColony;
					}
				}
				if (enemyGroupData.NumAggressive > 0 || enemyGroupData2.NumAggressive > 0)
				{
					return enemyGroupData.NumAggressive > enemyGroupData2.NumAggressive;
				}
				if (enemyGroupData.NumPassive > 0 || enemyGroupData2.NumPassive > 0)
				{
					return enemyGroupData.NumPassive > enemyGroupData2.NumPassive;
				}
				if (enemyGroupData.NumCivilian > 0 || enemyGroupData2.NumCivilian > 0)
				{
					return enemyGroupData.NumCivilian > enemyGroupData2.NumCivilian;
				}
				if (enemyGroupData.NumUnarmed > 0 || enemyGroupData2.NumUnarmed > 0)
				{
					return enemyGroupData.NumUnarmed > enemyGroupData2.NumUnarmed;
				}
			}
			return CombatAI.AssessGroupStrength(this.m_Ships) > CombatAI.AssessGroupStrength(eg.m_Ships);
		}
		public static EnemyGroupData GetEnemyGroupData(CombatAI ai, EnemyGroup eg, List<StellarBody> planets)
		{
			EnemyGroupData result = default(EnemyGroupData);
			result.DistanceFromColony = 0f;
			result.NumAggressive = 0;
			result.NumCivilian = 0;
			result.NumPassive = 0;
			result.NumUnarmed = 0;
			result.IsAttackingPlanetOrStation = false;
			result.IsEncounter = false;
			result.IsFreighter = false;
			result.IsStation = false;
			foreach (Ship current in eg.m_Ships)
			{
				if (current.ShipRole == ShipRole.FREIGHTER)
				{
					result.IsFreighter = true;
				}
				if (TaskGroup.IsValidTaskGroupShip(current))
				{
					switch (TaskGroup.GetTaskTypeFromShip(current))
					{
					case TaskGroupType.Aggressive:
					case TaskGroupType.Police:
					case TaskGroupType.BoardingGroup:
					case TaskGroupType.FollowGroup:
					case TaskGroupType.PlanetAssault:
						result.NumAggressive++;
						break;
					case TaskGroupType.Passive:
						result.NumPassive++;
						break;
					case TaskGroupType.Civilian:
						result.NumCivilian++;
						break;
					case TaskGroupType.UnArmed:
						result.NumUnarmed++;
						break;
					}
				}
				else
				{
					if (current.ShipClass == ShipClass.Station)
					{
						result.IsStation = true;
					}
				}
				if (current.Target != null && !result.IsAttackingPlanetOrStation)
				{
					if (current.Target is Ship)
					{
						Ship ship = current.Target as Ship;
						result.IsAttackingPlanetOrStation = (ship.Player == ai.m_Player && ship.ShipClass == ShipClass.Station);
					}
					else
					{
						if (current.Target is StellarBody && (current.Target as StellarBody).Parameters.ColonyPlayerID == ai.m_Player.ID)
						{
							result.IsAttackingPlanetOrStation = true;
						}
					}
				}
				result.IsEncounter = (result.IsEncounter || ai.IsEncounterPlayer(current.Player.ID));
				foreach (StellarBody current2 in planets)
				{
					if (current2.Parameters.ColonyPlayerID == ai.m_Player.ID)
					{
						float lengthSquared = (current2.Parameters.Position - current.Position).LengthSquared;
						if (result.DistanceFromColony == 0f || lengthSquared < result.DistanceFromColony)
						{
							result.DistanceFromColony = lengthSquared;
						}
					}
				}
			}
			if (result.DistanceFromColony > 0f)
			{
				result.DistanceFromColony = (float)Math.Sqrt((double)result.DistanceFromColony);
			}
			return result;
		}
	}
}
