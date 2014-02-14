using Kerberos.Sots.Engine;
using Kerberos.Sots.ShipFramework;
using System;
namespace Kerberos.Sots.UI
{
	internal class PsionicInfoPanel : PanelBinding
	{
		private const string UIModuleIcon = "moduleIcon";
		private const string UIModuleTitle = "moduleTitle";
		private const string UIModuleAbility = "moduleAbility";
		private const string UISupplyAttribute = "supplyAttribute";
		private const string UIPowerAttribute = "powerAttribute";
		private const string UICrewAttribute = "crewAttribute";
		private const string UIStructureValue = "structureValue";
		private const string UICost = "costDisplay.costValue";
		private string _contentPanelID;
		public PsionicInfoPanel(UICommChannel ui, string id) : base(ui, id)
		{
			this._contentPanelID = base.UI.Path(new string[]
			{
				id,
				"content"
			});
		}
		public void SetPsionic(LogicalPsionic primary)
		{
			if (primary == null)
			{
				return;
			}
			base.UI.SetPropertyString("moduleIcon", "sprite", primary.Icon);
			base.UI.SetVisible("moduleIcon", true);
			base.UI.SetText("moduleTitle", primary.PsionicTitle ?? string.Empty);
			base.UI.SetText("moduleAbility", primary.Description ?? string.Empty);
		}
	}
}
