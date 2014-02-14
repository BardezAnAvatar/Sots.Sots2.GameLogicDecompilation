using System;
namespace Kerberos.Sots.Data
{
	internal class DemandInfo
	{
		public int InitiatingPlayer;
		public int ReceivingPlayer;
		public DemandType Type;
		public float DemandValue;
		public AgreementState State;
		public int ID
		{
			get;
			set;
		}
		public string ToString(GameDatabase game)
		{
			string result = "Unknown demand type!";
			if (this.Type != DemandType.SurrenderDemand && this.DemandValue == 0f)
			{
				return "";
			}
			switch (this.Type)
			{
			case DemandType.SavingsDemand:
				result = string.Format(App.Localize("@UI_DIPLOMACY_DEMAND_SAVINGS_DESC"), this.DemandValue);
				break;
			case DemandType.SystemInfoDemand:
				result = string.Format(App.Localize("@UI_DIPLOMACY_DEMAND_SYSTEMINFO_DESC"), game.GetStarSystemInfo((int)this.DemandValue).Name);
				break;
			case DemandType.ResearchPointsDemand:
				result = string.Format(App.Localize("@UI_DIPLOMACY_DEMAND_RESEARCH_DESC"), this.DemandValue);
				break;
			case DemandType.SlavesDemand:
				result = string.Format(App.Localize("@UI_DIPLOMACY_DEMAND_SLAVES_DESC"), this.DemandValue);
				break;
			case DemandType.WorldDemand:
				result = string.Format(App.Localize("@UI_DIPLOMACY_DEMAND_WORLD_DESC"), game.GetStarSystemInfo((int)this.DemandValue).Name);
				break;
			case DemandType.ProvinceDemand:
				result = string.Format(App.Localize("@UI_DIPLOMACY_DEMAND_PROVINCE_DESC"), game.GetProvinceInfo((int)this.DemandValue).Name);
				break;
			case DemandType.SurrenderDemand:
				result = App.Localize("@UI_DIPLOMACY_DEMAND_SURRENDER_DESC");
				break;
			}
			return result;
		}
	}
}
