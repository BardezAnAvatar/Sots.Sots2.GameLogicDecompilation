using Kerberos.Sots.Framework;
using System;
namespace Kerberos.Sots.Data
{
	internal class MoveOrderInfo : IIDProvider
	{
		public int FleetID;
		public int FromSystemID;
		public Vector3 FromCoords;
		public int ToSystemID;
		public Vector3 ToCoords;
		public float Progress;
		public int ID
		{
			get;
			set;
		}
	}
}
