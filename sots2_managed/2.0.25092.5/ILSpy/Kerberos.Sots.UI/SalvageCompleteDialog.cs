using Kerberos.Sots.Data;
using Kerberos.Sots.Data.TechnologyFramework;
using System;
using System.Linq;
namespace Kerberos.Sots.UI
{
	internal class SalvageCompleteDialog : Dialog
	{
		public const string OKButton = "okButton";
		private int _techID;
		public SalvageCompleteDialog(App game, int techid) : base(game, "dialogSalvageEvent")
		{
			this._techID = techid;
		}
		public override void Initialize()
		{
			PlayerTechInfo pti = this._app.GameDatabase.GetPlayerTechInfo(this._app.LocalPlayer.ID, this._techID);
			Tech tech = this._app.AssetDatabase.MasterTechTree.Technologies.First((Tech x) => x.Id == pti.TechFileID);
			this._app.UI.SetText(this._app.UI.Path(new string[]
			{
				base.ID,
				"research_title"
			}), this._app.AssetDatabase.GetLocalizedTechnologyName(tech.Id));
			this._app.UI.SetText(this._app.UI.Path(new string[]
			{
				base.ID,
				"research_details"
			}), App.Localize("@TECH_DESC_" + tech.Id));
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked" && panelName == "okButton")
			{
				this._app.UI.CloseDialog(this, true);
			}
		}
		public override string[] CloseDialog()
		{
			return null;
		}
	}
}
