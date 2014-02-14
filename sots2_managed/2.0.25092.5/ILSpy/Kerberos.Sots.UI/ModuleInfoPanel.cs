using Kerberos.Sots.Engine;
using Kerberos.Sots.ShipFramework;
using System;
namespace Kerberos.Sots.UI
{
	internal class ModuleInfoPanel : PanelBinding
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
		public ModuleInfoPanel(UICommChannel ui, string id) : base(ui, id)
		{
			this._contentPanelID = base.UI.Path(new string[]
			{
				id,
				"content"
			});
		}
		public void SetModule(LogicalModule primary)
		{
			if (primary == null)
			{
				return;
			}
			base.UI.SetPropertyString("moduleIcon", "sprite", primary.Icon);
			base.UI.SetVisible("moduleIcon", true);
			base.UI.SetText("moduleTitle", primary.ModuleTitle ?? string.Empty);
			base.UI.SetText("moduleAbility", primary.Description ?? string.Empty);
			base.UI.SetText("powerAttribute", primary.PowerBonus.ToString());
			base.UI.SetText("supplyAttribute", primary.Supply.ToString());
			base.UI.SetText("crewAttribute", primary.Crew.ToString());
			base.UI.SetText("structureValue", primary.Structure.ToString());
			base.UI.SetText("costDisplay.costValue", primary.SavingsCost.ToString());
		}
	}
}
