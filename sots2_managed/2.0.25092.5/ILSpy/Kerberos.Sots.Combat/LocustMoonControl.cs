using Kerberos.Sots.Engine;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.ShipFramework;
using System;
namespace Kerberos.Sots.Combat
{
	internal class LocustMoonControl : LocustNestControl
	{
		public LocustMoonControl(App game, Ship ship, int fleetId) : base(game, ship, fleetId)
		{
		}
		protected override void PickTarget()
		{
			IGameObject gameObject = null;
			float num = 3.40282347E+38f;
			ShipClass sc = ShipClass.Cruiser;
			foreach (LocustTarget current in this.m_TargetList)
			{
				if (current.Target != null && !Ship.IsShipClassBigger(sc, current.Target.ShipClass, false))
				{
					float lengthSquared = (current.Target.Position - this.m_LocustNest.Position).LengthSquared;
					if (gameObject == null || lengthSquared < num)
					{
						gameObject = current.Target;
						num = lengthSquared;
					}
				}
			}
			if (gameObject == null)
			{
				foreach (StellarBody current2 in this.m_Planets)
				{
					float lengthSquared2 = (current2.Parameters.Position - this.m_LocustNest.Maneuvering.Position).LengthSquared;
					if (lengthSquared2 < num)
					{
						gameObject = current2;
						num = lengthSquared2;
					}
				}
			}
			this.SetTarget(gameObject);
		}
	}
}
