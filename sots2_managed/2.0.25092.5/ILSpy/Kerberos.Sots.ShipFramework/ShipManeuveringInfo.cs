using Kerberos.Sots.Framework;
using System;
namespace Kerberos.Sots.ShipFramework
{
	internal class ShipManeuveringInfo
	{
		public float LinearAccel = 20f;
		public Vector3 RotAccel = new Vector3(10f, 10f, 10f);
		public float Deacceleration;
		public float LinearSpeed = 500f;
		public float RotationSpeed = 500f;
	}
}
