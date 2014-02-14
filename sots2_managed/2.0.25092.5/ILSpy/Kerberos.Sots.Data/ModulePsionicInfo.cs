using Kerberos.Sots.Data.ShipFramework;
using System;
namespace Kerberos.Sots.Data
{
	internal class ModulePsionicInfo : IIDProvider
	{
		public int DesignModuleID;
		public SectionEnumerations.PsionicAbility Ability;
		public int ID
		{
			get;
			set;
		}
	}
}
