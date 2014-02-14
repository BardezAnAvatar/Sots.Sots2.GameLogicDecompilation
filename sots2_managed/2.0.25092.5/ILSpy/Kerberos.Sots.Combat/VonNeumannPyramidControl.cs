using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.Combat
{
	internal class VonNeumannPyramidControl : CombatAIController
	{
		private App m_Game;
		private Ship m_Pyramid;
		private IGameObject m_Target;
		private VonNeumannNeoBerserkerControl m_ParentBerserker;
		public override Ship GetShip()
		{
			return this.m_Pyramid;
		}
		public override void SetTarget(IGameObject target)
		{
			if (target != this.m_Target && this.m_Pyramid != null)
			{
				this.m_Target = target;
				this.m_Pyramid.SetShipTarget((target != null) ? target.ObjectID : 0, Vector3.Zero, true, 0);
			}
		}
		public override IGameObject GetTarget()
		{
			return this.m_Target;
		}
		public VonNeumannPyramidControl(App game, Ship ship)
		{
			this.m_Game = game;
			this.m_Pyramid = ship;
		}
		public override void Initialize()
		{
			this.m_Target = null;
			this.m_ParentBerserker = null;
		}
		public override void Terminate()
		{
		}
		public override void ObjectRemoved(IGameObject obj)
		{
			if (this.m_Target == obj)
			{
				this.SetTarget(null);
			}
			if (this.m_ParentBerserker != null && this.m_ParentBerserker.GetShip() == obj)
			{
				this.m_ParentBerserker = null;
			}
		}
		public override void OnThink()
		{
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
			if (this.m_ParentBerserker == null || !(this.m_ParentBerserker.GetTarget() is StellarBody))
			{
				float num = 3.40282347E+38f;
				IGameObject target = null;
				foreach (IGameObject current in objs)
				{
					if (current is StellarBody)
					{
						StellarBody stellarBody = current as StellarBody;
						if (stellarBody.Parameters.ColonyPlayerID != this.m_Pyramid.Player.ID)
						{
							float lengthSquared = (stellarBody.Parameters.Position - this.m_Pyramid.Position).LengthSquared;
							if (lengthSquared < num)
							{
								target = stellarBody;
								num = lengthSquared;
							}
						}
					}
				}
				this.SetTarget(target);
				return;
			}
			this.SetTarget(this.m_ParentBerserker.GetTarget());
		}
		public override bool NeedsAParent()
		{
			return this.m_ParentBerserker == null;
		}
		public override void FindParent(IEnumerable<CombatAIController> controllers)
		{
			foreach (CombatAIController current in controllers)
			{
				if (current is VonNeumannNeoBerserkerControl)
				{
					VonNeumannNeoBerserkerControl vonNeumannNeoBerserkerControl = current as VonNeumannNeoBerserkerControl;
					if (vonNeumannNeoBerserkerControl.IsThisMyMom(this.m_Pyramid))
					{
						vonNeumannNeoBerserkerControl.AddChild(this);
						this.m_ParentBerserker = vonNeumannNeoBerserkerControl;
						break;
					}
				}
			}
		}
	}
}
