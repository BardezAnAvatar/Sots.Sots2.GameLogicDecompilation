using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.GameStates
{
	internal class SectionTypeShipData
	{
		public ShipSectionType SectionType;
		public readonly List<SectionShipData> Sections = new List<SectionShipData>();
		public SectionShipData SelectedSection;
	}
}
