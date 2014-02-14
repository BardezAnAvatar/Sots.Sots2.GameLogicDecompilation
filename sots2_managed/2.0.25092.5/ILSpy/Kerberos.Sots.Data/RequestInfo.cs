using System;
namespace Kerberos.Sots.Data
{
	internal class RequestInfo
	{
		public int InitiatingPlayer;
		public int ReceivingPlayer;
		public RequestType Type;
		public float RequestValue;
		public AgreementState State;
		public int ID
		{
			get;
			set;
		}
		public string ToString(GameDatabase game)
		{
			string result = "Unknown request type!";
			if (this.RequestValue == 0f)
			{
				return "";
			}
			switch (this.Type)
			{
			case RequestType.SavingsRequest:
				result = string.Format(App.Localize("@UI_DIPLOMACY_REQUEST_SAVINGS_DESC"), this.RequestValue);
				break;
			case RequestType.SystemInfoRequest:
				result = string.Format(App.Localize("@UI_DIPLOMACY_REQUEST_SYSTEMINFO_DESC"), game.GetStarSystemInfo((int)this.RequestValue).Name);
				break;
			case RequestType.ResearchPointsRequest:
				result = string.Format(App.Localize("@UI_DIPLOMACY_REQUEST_RESEARCHPOINTS_DESC"), this.RequestValue);
				break;
			case RequestType.MilitaryAssistanceRequest:
				result = string.Format(App.Localize("@UI_DIPLOMACY_REQUEST_MILITARYASSISTANCE_DESC"), game.GetStarSystemInfo((int)this.RequestValue).Name);
				break;
			case RequestType.GatePermissionRequest:
				result = string.Format(App.Localize("@UI_DIPLOMACY_REQUEST_GATEPERMISSION_DESC"), game.GetStarSystemInfo((int)this.RequestValue).Name);
				break;
			case RequestType.EstablishEnclaveRequest:
				result = string.Format(App.Localize("@UI_DIPLOMACY_REQUEST_ESTABLISHENCLAVE_DESC"), game.GetStarSystemInfo((int)this.RequestValue).Name);
				break;
			}
			return result;
		}
	}
}
