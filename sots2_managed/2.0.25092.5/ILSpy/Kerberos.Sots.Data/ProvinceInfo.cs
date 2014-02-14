using System;
namespace Kerberos.Sots.Data
{
	internal class ProvinceInfo : IIDProvider
	{
		public int PlayerID;
		public string Name;
		public int CapitalSystemID;
		public int ID
		{
			get;
			set;
		}
	}
}
