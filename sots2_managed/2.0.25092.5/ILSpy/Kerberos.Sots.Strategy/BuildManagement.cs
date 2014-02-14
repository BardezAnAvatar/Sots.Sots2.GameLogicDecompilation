using Kerberos.Sots.Data;
using Kerberos.Sots.GameStates;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.Strategy
{
	internal class BuildManagement
	{
		public FleetInfo FleetToFill;
		public List<BuildScreenState.InvoiceItem> Invoices;
		public int BuildTime;
		public int? AIFleetID;
		public BuildManagement()
		{
			this.FleetToFill = null;
			this.Invoices = new List<BuildScreenState.InvoiceItem>();
			this.BuildTime = 0;
			this.AIFleetID = null;
		}
	}
}
