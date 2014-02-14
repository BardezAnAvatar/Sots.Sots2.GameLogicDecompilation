using System;
namespace Kerberos.Sots.Data
{
	public class GovernmentInfo
	{
		public enum GovernmentType
		{
			Centrism,
			Communalism,
			Junta,
			Plutocracy,
			Socialism,
			Mercantilism,
			Cooperativism,
			Anarchism,
			Liberationism
		}
		public int PlayerID;
		public float Authoritarianism;
		public float EconomicLiberalism;
		public GovernmentInfo.GovernmentType CurrentType;
	}
}
