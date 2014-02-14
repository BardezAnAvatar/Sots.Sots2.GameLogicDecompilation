using System;
namespace Kerberos.Sots.Data
{
	internal class GiveInfo
	{
		public int InitiatingPlayer;
		public int ReceivingPlayer;
		public GiveType Type;
		public float GiveValue;
		public int ID
		{
			get;
			set;
		}
		public string ToString(GameDatabase game)
		{
			string result = "Unknown request type!";
			if (this.GiveValue == 0f)
			{
				return "";
			}
			switch (this.Type)
			{
			case GiveType.GiveSavings:
				result = string.Format(App.Localize("@UI_DIPLOMACY_GIVE_SAVINGS"), this.GiveValue);
				break;
			case GiveType.GiveResearchPoints:
				result = string.Format(App.Localize("@UI_DIPLOMACY_GIVE_RESEARCH_MONEY"), this.GiveValue);
				break;
			}
			return result;
		}
	}
}
