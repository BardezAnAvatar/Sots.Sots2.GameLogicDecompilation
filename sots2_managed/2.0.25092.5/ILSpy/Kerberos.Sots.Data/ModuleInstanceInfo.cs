using System;
namespace Kerberos.Sots.Data
{
	internal class ModuleInstanceInfo : IIDProvider
	{
		public int SectionInstanceID;
		public string ModuleNodeID;
		public int Structure;
		public int RepairPoints;
		public int ID
		{
			get;
			set;
		}
	}
}
