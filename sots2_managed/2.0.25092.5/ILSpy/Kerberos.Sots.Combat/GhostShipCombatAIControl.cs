using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.Combat
{
	internal class GhostShipCombatAIControl : CombatAIController
	{
		private App m_Game;
		private Ship m_GhostShip;
		private IGameObject m_CurrentTarget;
		private GhostShipAIStates m_State;
		public override Ship GetShip()
		{
			return this.m_GhostShip;
		}
		public override void SetTarget(IGameObject target)
		{
			this.m_CurrentTarget = target;
		}
		public override IGameObject GetTarget()
		{
			return this.m_CurrentTarget;
		}
		public GhostShipCombatAIControl(App game, Ship ship)
		{
			this.m_Game = game;
			this.m_GhostShip = ship;
		}
		public override void Initialize()
		{
			this.m_CurrentTarget = null;
			this.m_State = GhostShipAIStates.SEEK;
		}
		public override void Terminate()
		{
		}
		public override void ObjectRemoved(IGameObject obj)
		{
			if (obj == this.m_CurrentTarget)
			{
				this.m_CurrentTarget = null;
			}
		}
		public override void OnThink()
		{
			if (this.m_GhostShip == null)
			{
				return;
			}
			switch (this.m_State)
			{
			case GhostShipAIStates.SEEK:
				this.ThinkSeek();
				return;
			case GhostShipAIStates.TRACK:
				this.ThinkTrack();
				return;
			case GhostShipAIStates.FLEE:
				this.ThinkFlee();
				return;
			default:
				return;
			}
		}
		public override void ForceFlee()
		{
		}
		public override bool VictoryConditionIsMet()
		{
			return this.m_GhostShip != null && this.m_GhostShip.HasRetreated;
		}
		public override bool RequestingNewTarget()
		{
			return this.m_State == GhostShipAIStates.SEEK && this.m_CurrentTarget == null;
		}
		public override void FindNewTarget(IEnumerable<IGameObject> objs)
		{
			this.m_CurrentTarget = null;
			float num = 3.40282347E+38f;
			List<StellarBody> list = new List<StellarBody>();
			List<Ship> list2 = new List<Ship>();
			foreach (IGameObject current in objs)
			{
				if (current != this.m_GhostShip)
				{
					if (current is Ship)
					{
						Ship ship = current as Ship;
						if (ship.Player.ID != this.m_GhostShip.Player.ID && ship.RealShipClass == RealShipClasses.Station)
						{
							list2.Add(ship);
						}
					}
					else
					{
						if (current is StellarBody)
						{
							StellarBody stellarBody = current as StellarBody;
							if (stellarBody.Population > 0.0 && stellarBody.Parameters.ColonyPlayerID != this.m_GhostShip.Player.ID)
							{
								list.Add(current as StellarBody);
							}
						}
					}
				}
			}
			foreach (Ship current2 in list2)
			{
				float lengthSquared = (this.m_GhostShip.Position - current2.Position).LengthSquared;
				if (lengthSquared < num)
				{
					num = lengthSquared;
					this.m_CurrentTarget = current2;
				}
			}
			if (this.m_CurrentTarget == null)
			{
				foreach (StellarBody current3 in list)
				{
					float lengthSquared2 = (this.m_GhostShip.Position - current3.Parameters.Position).LengthSquared;
					if (lengthSquared2 < num)
					{
						num = lengthSquared2;
						this.m_CurrentTarget = current3;
					}
				}
			}
		}
		public override bool NeedsAParent()
		{
			return false;
		}
		public override void FindParent(IEnumerable<CombatAIController> controllers)
		{
		}
		private void ThinkSeek()
		{
			if (this.m_CurrentTarget != null)
			{
				if (this.m_CurrentTarget is Ship)
				{
					this.m_GhostShip.SetShipTarget(this.m_CurrentTarget.ObjectID, (this.m_CurrentTarget as Ship).ShipSphere.center, true, 0);
				}
				else
				{
					if (this.m_CurrentTarget is StellarBody)
					{
						this.m_GhostShip.SetShipTarget(this.m_CurrentTarget.ObjectID, Vector3.Zero, true, 0);
					}
				}
				this.m_State = GhostShipAIStates.TRACK;
			}
		}
		private void ThinkTrack()
		{
			bool flag = false;
			if (this.m_CurrentTarget == null)
			{
				this.m_State = GhostShipAIStates.SEEK;
			}
			else
			{
				if (this.m_CurrentTarget is Ship)
				{
					Ship ship = this.m_CurrentTarget as Ship;
					float s = ship.ShipSphere.radius + this.m_GhostShip.ShipSphere.radius + 500f;
					Vector3 vector = ship.Position - this.m_GhostShip.Position;
					vector.Normalize();
					this.m_GhostShip.Maneuvering.PostAddGoal(ship.Position - vector * s, vector);
					if (ship.IsDestroyed)
					{
						flag = true;
					}
				}
				else
				{
					if (this.m_CurrentTarget is StellarBody)
					{
						StellarBody stellarBody = this.m_CurrentTarget as StellarBody;
						float s2 = stellarBody.Parameters.Radius + this.m_GhostShip.ShipSphere.radius + 750f;
						Vector3 vector2 = stellarBody.Parameters.Position - this.m_GhostShip.Position;
						vector2.Normalize();
						this.m_GhostShip.Maneuvering.PostAddGoal(stellarBody.Parameters.Position - vector2 * s2, vector2);
						if ((this.m_CurrentTarget as StellarBody).Population <= 0.0)
						{
							flag = true;
						}
					}
				}
			}
			if (flag)
			{
				this.m_CurrentTarget = null;
				this.m_State = GhostShipAIStates.FLEE;
			}
		}
		private void ThinkFlee()
		{
			if (this.m_GhostShip == null)
			{
				return;
			}
			if (this.m_GhostShip.CombatStance != CombatStance.RETREAT)
			{
				this.m_GhostShip.SetCombatStance(CombatStance.RETREAT);
			}
		}
	}
}
