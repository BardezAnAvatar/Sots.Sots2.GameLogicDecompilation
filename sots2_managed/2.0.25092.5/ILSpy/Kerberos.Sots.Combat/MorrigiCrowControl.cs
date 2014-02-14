using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.Combat
{
	internal class MorrigiCrowControl : CombatAIController
	{
		private App m_Game;
		private Ship m_MorrigiCrow;
		private MorrigiRelicControl m_MorrigiRelic;
		private IGameObject m_Target;
		private MorrigiCrowstates m_State;
		private int m_RefreshTargetDelay;
		public override Ship GetShip()
		{
			return this.m_MorrigiCrow;
		}
		public override void SetTarget(IGameObject target)
		{
			if (target != this.m_Target)
			{
				this.m_Target = target;
				int targetId = (target != null) ? target.ObjectID : 0;
				this.m_MorrigiCrow.SetShipTarget(targetId, Vector3.Zero, true, 0);
			}
		}
		public override IGameObject GetTarget()
		{
			return this.m_Target;
		}
		public MorrigiCrowControl(App game, Ship ship)
		{
			this.m_Game = game;
			this.m_MorrigiCrow = ship;
		}
		public override void Initialize()
		{
			this.m_Target = null;
			this.m_MorrigiRelic = null;
			this.m_State = MorrigiCrowstates.SEEK;
			this.m_RefreshTargetDelay = 0;
		}
		public override void Terminate()
		{
		}
		public override void ObjectRemoved(IGameObject obj)
		{
			if (this.m_MorrigiRelic != null && this.m_MorrigiRelic.GetShip() == obj)
			{
				this.m_MorrigiRelic = null;
			}
			if (this.m_Target == obj)
			{
				this.m_Target = null;
			}
		}
		public override void OnThink()
		{
			if (this.m_MorrigiCrow == null)
			{
				return;
			}
			this.UpdateTarget();
			switch (this.m_State)
			{
			case MorrigiCrowstates.IDLE:
				this.ThinkIdle();
				return;
			case MorrigiCrowstates.SEEK:
				this.ThinkSeek();
				return;
			case MorrigiCrowstates.TRACK:
				this.ThinkTrack();
				return;
			default:
				return;
			}
		}
		public void UpdateTarget()
		{
			if (this.m_Target != null)
			{
				this.m_RefreshTargetDelay--;
				if (this.m_RefreshTargetDelay <= 0)
				{
					this.SetTarget(null);
					this.m_RefreshTargetDelay = 300;
					return;
				}
			}
			else
			{
				this.m_RefreshTargetDelay = 300;
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
			return this.m_Target == null;
		}
		public override void FindNewTarget(IEnumerable<IGameObject> objs)
		{
			if (this.m_MorrigiRelic == null)
			{
				float num = 3.40282347E+38f;
				IGameObject target = null;
				foreach (IGameObject current in objs)
				{
					if (current is Ship)
					{
						Ship ship = current as Ship;
						if (ship.Player != this.m_MorrigiCrow.Player && ship.Active && Ship.IsActiveShip(ship) && ship.IsDetected(this.m_MorrigiCrow.Player))
						{
							float lengthSquared = (ship.Position - this.m_MorrigiCrow.Position).LengthSquared;
							if (lengthSquared < num)
							{
								target = ship;
								num = lengthSquared;
							}
						}
					}
				}
				this.SetTarget(target);
			}
		}
		public override bool NeedsAParent()
		{
			return this.m_MorrigiRelic == null;
		}
		public override void FindParent(IEnumerable<CombatAIController> controllers)
		{
			foreach (CombatAIController current in controllers)
			{
				if (current is MorrigiRelicControl)
				{
					MorrigiRelicControl morrigiRelicControl = current as MorrigiRelicControl;
					if (morrigiRelicControl.IsThisMyRelic(this.m_MorrigiCrow))
					{
						morrigiRelicControl.AddCrow(this);
						this.m_MorrigiRelic = morrigiRelicControl;
						break;
					}
				}
			}
		}
		private void ThinkIdle()
		{
		}
		private void ThinkSeek()
		{
			if (this.m_Target != null)
			{
				this.m_State = MorrigiCrowstates.TRACK;
			}
		}
		private void ThinkTrack()
		{
			if (this.m_Target == null)
			{
				this.m_State = MorrigiCrowstates.SEEK;
			}
		}
	}
}
