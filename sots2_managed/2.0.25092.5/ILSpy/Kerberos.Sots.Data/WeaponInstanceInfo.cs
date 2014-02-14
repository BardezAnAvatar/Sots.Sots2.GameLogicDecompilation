using System;
namespace Kerberos.Sots.Data
{
	internal class WeaponInstanceInfo : IIDProvider
	{
		public int SectionInstanceID;
		public int? ModuleInstanceID;
		public int WeaponID;
		public string NodeName;
		public float Structure;
		public float MaxStructure;
		public int ID
		{
			get;
			set;
		}
	}
}
