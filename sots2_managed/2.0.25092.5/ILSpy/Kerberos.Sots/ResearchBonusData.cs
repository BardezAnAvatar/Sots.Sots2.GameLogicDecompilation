using System;
using System.Xml;
namespace Kerberos.Sots
{
	public class ResearchBonusData
	{
		public int NumTurns;
		public float Percent;
		public void SetDataFromElement(XmlElement data)
		{
			this.NumTurns = int.Parse(data.GetAttribute("turns"));
			this.Percent = float.Parse(data.GetAttribute("percent"));
		}
	}
}
