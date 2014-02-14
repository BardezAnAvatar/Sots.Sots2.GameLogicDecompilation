using System;
namespace Kerberos.Sots.Data
{
	internal class FactionInfo : IIDProvider
	{
		public string Name;
		public float IdealSuitability;
		public int ID
		{
			get;
			set;
		}
	}
}
