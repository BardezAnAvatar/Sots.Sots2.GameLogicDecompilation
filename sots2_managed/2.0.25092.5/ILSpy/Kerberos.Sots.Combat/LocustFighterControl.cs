using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.Combat
{
	internal class LocustFighterControl : CombatAIController
	{
		private App m_Game;
		private Ship m_LocustFighter;
		private LocustNestControl m_LocustNest;
		private IGameObject m_Target;
		private LocustFighterStates m_State;
		public static int NumFightersPerShip(ShipClass sc)
		{
			switch (sc)
			{
			case ShipClass.Cruiser:
				return 6;
			case ShipClass.Dreadnought:
				return 10;
			case ShipClass.Leviathan:
			case ShipClass.Station:
				return 15;
			case ShipClass.BattleRider:
				return 2;
			default:
				return 5;
			}
		}
		public override Ship GetShip()
		{
			return this.m_LocustFighter;
		}
		public override void SetTarget(IGameObject target)
		{
			if (target != this.m_Target)
			{
				this.m_Target = target;
				int targetId = (target != null) ? target.ObjectID : 0;
				this.m_LocustFighter.SetShipTarget(targetId, Vector3.Zero, true, 0);
			}
		}
		public override IGameObject GetTarget()
		{
			return this.m_Target;
		}
		public LocustFighterControl(App game, Ship ship)
		{
			this.m_Game = game;
			this.m_LocustFighter = ship;
		}
		public override void Initialize()
		{
			this.m_Target = null;
			this.m_LocustNest = null;
			this.m_State = LocustFighterStates.IDLE;
		}
		public override void Terminate()
		{
		}
		public override void ObjectRemoved(IGameObject obj)
		{
			if (this.m_LocustNest != null && this.m_LocustNest.GetShip() == obj)
			{
				this.m_LocustNest = null;
			}
			if (this.m_Target == obj)
			{
				this.m_Target = null;
			}
		}
		public override void OnThink()
		{
			if (this.m_LocustFighter == null)
			{
				return;
			}
			switch (this.m_State)
			{
			case LocustFighterStates.IDLE:
				this.ThinkIdle();
				return;
			case LocustFighterStates.SEEK:
				this.ThinkSeek();
				return;
			case LocustFighterStates.TRACK:
				this.ThinkTrack();
				return;
			case LocustFighterStates.ASSAULT:
				this.ThinkAssault();
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
			return false;
		}
		public override bool RequestingNewTarget()
		{
			return (this.m_LocustFighter == null || !this.m_LocustFighter.DockedWithParent) && (this.m_Target == null || (this.m_LocustNest != null && this.m_Target == this.m_LocustNest.GetShip()));
		}
		public override void FindNewTarget(IEnumerable<IGameObject> objs)
		{
			if (this.m_LocustNest == null)
			{
				float num = 1E+17f;
				IGameObject target = null;
				foreach (IGameObject current in objs)
				{
					if (current is Ship)
					{
						Ship ship = current as Ship;
						if (ship.Player != this.m_LocustFighter.Player && Ship.IsActiveShip(ship) && ship.IsDetected(this.m_LocustFighter.Player))
						{
							float lengthSquared = (ship.Position - this.m_LocustFighter.Position).LengthSquared;
							if (lengthSquared < num)
							{
								target = ship;
								num = lengthSquared;
							}
						}
					}
				}
				this.SetTarget(target);
				return;
			}
			this.m_LocustNest.RequestTargetFromParent(this);
		}
		public override bool NeedsAParent()
		{
			return this.m_LocustNest == null;
		}
		public override void FindParent(IEnumerable<CombatAIController> controllers)
		{
			foreach (CombatAIController current in controllers)
			{
				if (current is LocustNestControl)
				{
					LocustNestControl locustNestControl = current as LocustNestControl;
					if (locustNestControl.IsThisMyNest(this.m_LocustFighter))
					{
						locustNestControl.AddFighter(this);
						this.m_LocustNest = locustNestControl;
						break;
					}
				}
			}
		}
		public void ClearPlanetTarget()
		{
			if (this.m_Target is StellarBody && this.m_State != LocustFighterStates.ASSAULT)
			{
				this.SetTarget(null);
			}
		}
		private void ThinkIdle()
		{
			if (this.m_Target != null && this.m_Target != this.m_LocustNest)
			{
				this.m_State = LocustFighterStates.TRACK;
			}
		}
		private void ThinkSeek()
		{
			if (this.m_Target != null)
			{
				if (this.m_Target != this.m_LocustNest)
				{
					this.m_State = LocustFighterStates.TRACK;
					return;
				}
				this.m_State = LocustFighterStates.IDLE;
			}
		}
		private void ThinkTrack()
		{
			if (this.m_Target != null)
			{
				if (this.m_Target is StellarBody)
				{
					StellarBody stellarBody = this.m_Target as StellarBody;
					float num = stellarBody.Parameters.Radius + this.m_LocustFighter.ShipSphere.radius;
					if ((stellarBody.Parameters.Position - this.m_LocustFighter.Position).LengthSquared < num * num)
					{
						this.m_State = LocustFighterStates.ASSAULT;
						if (this.m_LocustNest != null)
						{
							this.m_LocustNest.NotifyFighterHasLanded();
						}
					}
				}
				return;
			}
			if (this.m_Target != this.m_LocustNest)
			{
				this.m_State = LocustFighterStates.SEEK;
				return;
			}
			this.m_State = LocustFighterStates.IDLE;
		}
		private void ThinkAssault()
		{
			bool flag = this.m_Target == null;
			if (this.m_Target is StellarBody)
			{
				StellarBody stellarBody = this.m_Target as StellarBody;
				float num = stellarBody.Parameters.Radius + this.m_LocustFighter.ShipSphere.radius;
				if ((stellarBody.Parameters.Position - this.m_LocustFighter.Position).LengthSquared > num * num)
				{
					flag = true;
				}
			}
			if (flag)
			{
				this.m_State = LocustFighterStates.SEEK;
				this.SetTarget(null);
			}
		}
	}
}
