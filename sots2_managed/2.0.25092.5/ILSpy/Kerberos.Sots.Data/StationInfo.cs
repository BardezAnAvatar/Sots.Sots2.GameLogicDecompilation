using Kerberos.Sots.ShipFramework;
using System;
namespace Kerberos.Sots.Data
{
	internal class StationInfo
	{
		public OrbitalObjectInfo OrbitalObjectInfo;
		public int PlayerID;
		public DesignInfo DesignInfo;
		public int WarehousedGoods;
		public int ShipID;
		public int OrbitalObjectID
		{
			get
			{
				return this.OrbitalObjectInfo.ID;
			}
		}
		public int ID
		{
			get
			{
				return this.OrbitalObjectInfo.ID;
			}
		}
		public float GetBaseStratSensorRange()
		{
			ShipSectionAsset shipSectionAsset = this.DesignInfo.DesignSections[0].ShipSectionAsset;
			if (shipSectionAsset != null)
			{
				return shipSectionAsset.StrategicSensorRange;
			}
			return 0f;
		}
	}
}
