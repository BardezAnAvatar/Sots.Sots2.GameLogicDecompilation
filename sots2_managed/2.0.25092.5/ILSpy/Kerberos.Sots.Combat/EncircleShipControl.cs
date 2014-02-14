using System;
namespace Kerberos.Sots.Combat
{
	internal class EncircleShipControl : BaseAttackShipControl
	{
		public EncircleShipControl(App game, TacticalObjective to, CombatAI commanderAI, float offset) : base(game, to, commanderAI)
		{
			this.m_Type = ShipControlType.Encircle;
		}
		protected override void OnAttackUpdate(int framesElapsed)
		{
		}
	}
}
