using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.GameStates
{
	internal class ClassShipData
	{
		public RealShipClasses Class;
		public readonly List<SectionTypeShipData> SectionTypes = new List<SectionTypeShipData>();
	}
}
