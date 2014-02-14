using Kerberos.Sots.Data.WeaponFramework;
using System;
namespace Kerberos.Sots.Data
{
	internal class PlagueInfo
	{
		public int ColonyId;
		public WeaponEnums.PlagueType PlagueType;
		public float InfectionRate;
		public double InfectedPopulationImperial;
		public double InfectedPopulationCivilian;
		public int LaunchingPlayer;
		public int ID
		{
			get;
			set;
		}
		public double InfectedPopulation
		{
			get
			{
				return this.InfectedPopulationCivilian + this.InfectedPopulationImperial;
			}
		}
	}
}
