using Kerberos.Sots.Framework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Strategy;
using System;
namespace Kerberos.Sots.GameStates
{
	internal struct FormationCreationData
	{
		public int ShipID;
		public int DesignID;
		public ShipRole ShipRole;
		public ShipClass ShipClass;
		public Vector3 FormationPosition;
	}
}
