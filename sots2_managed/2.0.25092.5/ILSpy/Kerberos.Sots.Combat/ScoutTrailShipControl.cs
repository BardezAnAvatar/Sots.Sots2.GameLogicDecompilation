using Kerberos.Sots.Framework;
using Kerberos.Sots.GameStates;
using System;
using System.Linq;
namespace Kerberos.Sots.Combat
{
	internal class ScoutTrailShipControl : TaskGroupShipControl
	{
		private ScoutShipControl m_ScoutShip;
		public ScoutTrailShipControl(App game, TacticalObjective to, CombatAI commanderAI, ScoutShipControl scoutShip) : base(game, to, commanderAI)
		{
			this.m_Type = ShipControlType.ScoutTrail;
			this.m_ScoutShip = scoutShip;
		}
		public override void Update(int framesElapsed)
		{
			if (this.m_TaskGroupObjective == null || this.m_ScoutShip == null)
			{
				return;
			}
			Vector3 vector = Vector3.Zero;
			Vector3 vector2 = -Vector3.UnitZ;
			float num = this.m_CommanderAI.PlanetsInSystem.Any((StellarBody x) => x.Parameters.ColonyPlayerID == this.m_CommanderAI.m_Player.ID) ? 10000f : 3000f;
			if (this.m_ScoutShip != null && this.m_ScoutShip.EncounteredEnemy)
			{
				vector = this.m_ScoutShip.GetCurrentPosition();
				vector2 = vector - base.GetCurrentPosition();
				vector2.Y = 0f;
				vector2.Normalize();
			}
			else
			{
				vector2 = this.m_ScoutShip.GetCurrentPosition() - base.GetCurrentPosition();
				vector2.Y = 0f;
				vector2.Normalize();
				vector = this.m_ScoutShip.GetCurrentPosition() - vector2 * num;
			}
			float lengthSquared = (this.m_ScoutShip.GetCurrentPosition() - base.GetCurrentPosition()).LengthSquared;
			if (lengthSquared > num * num || !this.m_Formation.DestinationSet)
			{
				base.SetFUP(vector, vector2);
			}
		}
	}
}
