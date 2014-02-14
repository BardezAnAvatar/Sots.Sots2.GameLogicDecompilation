using Kerberos.Sots.Data;
using System;
namespace Kerberos.Sots.StarFleet
{
	internal class ShipItem
	{
		public readonly int ShipID;
		public readonly string Name = string.Empty;
		public readonly int DesignID;
		public int NumAdded;
		public ShipItem(ShipInfo shipInfo)
		{
			this.ShipID = shipInfo.ID;
			this.DesignID = shipInfo.DesignID;
			this.Name = shipInfo.ShipName;
		}
	}
}
