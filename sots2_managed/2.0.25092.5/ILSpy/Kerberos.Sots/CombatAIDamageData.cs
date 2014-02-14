using System;
using System.Xml;
namespace Kerberos.Sots
{
	public struct CombatAIDamageData
	{
		public int Crew;
		public int Population;
		public float InfraDamage;
		public float TeraDamage;
		public void SetDataFromElement(XmlElement data)
		{
			this.Crew = int.Parse(data.GetAttribute("crew"));
			this.Population = int.Parse(data.GetAttribute("population"));
			this.InfraDamage = float.Parse(data.GetAttribute("infra"));
			this.TeraDamage = float.Parse(data.GetAttribute("tera"));
		}
	}
}
