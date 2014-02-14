using System;
namespace Kerberos.Sots.Data
{
	internal class BuildOrderInfo : IIDProvider
	{
		public int SystemID;
		public int DesignID;
		public int Priority;
		public int Progress;
		public int ProductionTarget;
		public int MissionID;
		public string ShipName = string.Empty;
		public int? InvoiceID;
		public int? AIFleetID;
		public int LoaCubes;
		public int ID
		{
			get;
			set;
		}
	}
}
