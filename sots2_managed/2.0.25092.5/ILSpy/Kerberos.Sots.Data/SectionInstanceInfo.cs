using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.Data
{
	internal class SectionInstanceInfo : IIDProvider
	{
		public int SectionID;
		public int? ShipID;
		public int? StationID;
		public int Structure;
		public int Supply;
		public int Crew;
		public float Signature;
		public int RepairPoints;
		public Dictionary<ArmorSide, DamagePattern> Armor = new Dictionary<ArmorSide, DamagePattern>();
		public List<WeaponInstanceInfo> WeaponInstances = new List<WeaponInstanceInfo>();
		public List<ModuleInstanceInfo> ModuleInstances = new List<ModuleInstanceInfo>();
		public int ID
		{
			get;
			set;
		}
	}
}
