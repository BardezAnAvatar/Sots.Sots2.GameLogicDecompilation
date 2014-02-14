using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.ShipFramework;
using System;
namespace Kerberos.Sots.Combat
{
	internal class SupportGroupShipControl : TaskGroupShipControl
	{
		private TaskGroupShipControl m_SupportGroup;
		public SupportGroupShipControl(App game, TacticalObjective to, CombatAI commanderAI, TaskGroupShipControl supportTaskGroup) : base(game, to, commanderAI)
		{
			this.m_Type = ShipControlType.SupportGroup;
			this.m_SupportGroup = supportTaskGroup;
		}
		public override void Update(int framesElapsed)
		{
			if (this.m_TaskGroupObjective == null)
			{
				return;
			}
			Vector3 currentPosition = base.GetCurrentPosition();
			Vector3 vector = (this.m_SupportGroup != null && this.m_SupportGroup.m_Formation != null && this.m_SupportGroup.m_Formation.DestinationSet) ? this.m_SupportGroup.m_Formation.Destination : this.m_TaskGroupObjective.GetObjectiveLocation();
			Vector3 destFacing = vector - currentPosition;
			destFacing.Y = 0f;
			destFacing.Normalize();
			foreach (Ship current in this.m_Ships)
			{
				float lengthSquared = (current.Position - vector).LengthSquared;
				float lengthSquared2 = (current.Position - this.m_TaskGroupObjective.GetObjectiveLocation()).LengthSquared;
				base.SetShipSpeed(current, (lengthSquared < TaskGroup.ATTACK_GROUP_RANGE * TaskGroup.ATTACK_GROUP_RANGE && lengthSquared2 < lengthSquared) ? ShipSpeedState.Normal : ShipSpeedState.Overthrust);
			}
			base.SetFUP(vector, destFacing);
		}
	}
}
