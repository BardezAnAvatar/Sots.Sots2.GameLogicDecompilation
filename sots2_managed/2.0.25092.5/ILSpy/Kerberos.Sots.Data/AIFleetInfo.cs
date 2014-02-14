using System;
namespace Kerberos.Sots.Data
{
	internal class AIFleetInfo : IIDProvider
	{
		public int? FleetID;
		public int FleetType;
		public int SystemID;
		public int? InvoiceID;
		public int? AdmiralID;
		public string FleetTemplate;
		public int ID
		{
			get;
			set;
		}
		public AIFleetInfo()
		{
		}
		public AIFleetInfo(int? fleetID, int fleetType, int systemID, int? invoiceID, int admiralID, string fleetTemplate)
		{
			this.FleetID = fleetID;
			this.FleetType = fleetType;
			this.SystemID = systemID;
			this.InvoiceID = invoiceID;
			this.AdmiralID = new int?(admiralID);
			this.FleetTemplate = fleetTemplate;
		}
		public override string ToString()
		{
			return string.Format("ID={0},FleetID={1},FleetType={2}", this.ID, this.FleetID.HasValue ? this.FleetID.Value.ToString() : "null", this.FleetType.ToString());
		}
	}
}
