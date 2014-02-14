using System;
using System.Collections.Generic;
namespace Kerberos.Sots.Data
{
	internal class LoaFleetComposition : IIDProvider
	{
		public int PlayerID;
		public string Name;
		public List<LoaFleetShipDef> designs;
		public int ID
		{
			get;
			set;
		}
	}
}
