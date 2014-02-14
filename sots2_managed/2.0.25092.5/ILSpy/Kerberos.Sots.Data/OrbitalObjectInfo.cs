using System;
namespace Kerberos.Sots.Data
{
	internal class OrbitalObjectInfo : IIDProvider
	{
		public int? ParentID;
		public int StarSystemID;
		public OrbitalPath OrbitalPath;
		public string Name;
		public int ID
		{
			get;
			set;
		}
	}
}
