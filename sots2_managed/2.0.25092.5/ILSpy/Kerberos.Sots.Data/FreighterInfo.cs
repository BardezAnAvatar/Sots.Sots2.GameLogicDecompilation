using System;
namespace Kerberos.Sots.Data
{
	internal class FreighterInfo
	{
		public int ShipId;
		public int SystemId;
		public int PlayerId;
		public bool IsPlayerBuilt;
		public DesignInfo Design;
		public int ID
		{
			get;
			set;
		}
	}
}
