using Kerberos.Sots.Framework;
using System;
namespace Kerberos.Sots.GameStates
{
	internal class CombatZonePositionInfo
	{
		public int Player;
		public int RingIndex;
		public int ZoneIndex;
		public Vector3 Center = Vector3.Zero;
		public float RadiusLower;
		public float RadiusUpper;
		public float AngleLeft;
		public float AngleRight;
	}
}
