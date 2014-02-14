using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.ShipFramework;
using System;
using System.Linq;
namespace Kerberos.Sots.Combat
{
	internal class RetreatShipControl : TaskGroupShipControl
	{
		private bool m_UseRetreatPos;
		public RetreatShipControl(App game, TacticalObjective to, CombatAI commanderAI, bool useRetreatPos) : base(game, to, commanderAI)
		{
			this.m_Type = ShipControlType.Retreat;
			this.m_UseRetreatPos = useRetreatPos;
		}
		public override void Update(int framesElapsed)
		{
			if (this.m_TaskGroupObjective == null)
			{
				return;
			}
			Vector3 vector = base.GetCurrentPosition();
			vector.Y = 0f;
			vector *= 2f;
			if (!this.m_Ships.Any((Ship x) => x.IsNPCFreighter))
			{
				foreach (Ship current in 
					from x in this.m_Ships
					where !x.IsNPCFreighter
					select x)
				{
					base.SetShipSpeed(current, ShipSpeedState.Overthrust);
				}
			}
			foreach (Ship current2 in this.m_Ships)
			{
				if (current2.CombatStance != CombatStance.RETREAT)
				{
					if (this.m_UseRetreatPos)
					{
						current2.Maneuvering.RetreatDestination = this.m_TaskGroupObjective.GetObjectiveLocation();
					}
					current2.SetCombatStance(CombatStance.RETREAT);
				}
			}
			Vector3 destFacing = vector;
			destFacing.Normalize();
			base.SetFUP(vector, destFacing);
		}
	}
}
