using Kerberos.Sots.Framework;
using Kerberos.Sots.ShipFramework;
using System;
namespace Kerberos.Sots.Combat
{
	internal class NodeMawInfo
	{
		public LogicalWeapon Weapon;
		public float Range;
		public Vector3 Pos = Vector3.Zero;
		public Quaternion Rot = Quaternion.Zero;
	}
}
