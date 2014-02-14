using Kerberos.Sots.Strategy;
using System;
namespace Kerberos.Sots.Data
{
	internal class SpecialProjectInfo : IIDProvider
	{
		public int PlayerID;
		public string Name;
		public int Progress;
		public int Cost;
		public SpecialProjectType Type;
		public int TechID;
		public float Rate;
		public int EncounterID;
		public int FleetID;
		public int TargetPlayerID;
		public int ID
		{
			get;
			set;
		}
	}
}
