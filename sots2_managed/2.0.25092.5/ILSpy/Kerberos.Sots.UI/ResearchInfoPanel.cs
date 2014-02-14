using Kerberos.Sots.Data.TechnologyFramework;
using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.ShipFramework;
using System;
using System.IO;
using System.Linq;
namespace Kerberos.Sots.UI
{
	internal class ResearchInfoPanel : PanelBinding
	{
		private const string UITechIcon = "techicon";
		private const string UIFamilyIcon = "familyicon";
		private const string UITechTitle = "tech_title";
		private const string UITechDescription = "tech_desc";
		private const string weaponPanel = "TechWeaponDetails";
		private string _contentPanelID;
		private int _techID;
		private WeaponInfoPanel _weaponinfopanel;
		public ResearchInfoPanel(UICommChannel ui, string id) : base(ui, id)
		{
			this._contentPanelID = base.UI.Path(new string[]
			{
				id,
				"content"
			});
			this._weaponinfopanel = new WeaponInfoPanel(ui, "TechWeaponDetails");
		}
		private static string IconTextureToSpriteName(string texture)
		{
			return Path.GetFileNameWithoutExtension(texture);
		}
		private LogicalWeapon GetWeaponUnlockedByTech(App app, Kerberos.Sots.Data.TechnologyFramework.Tech tech)
		{
			return app.AssetDatabase.Weapons.FirstOrDefault((LogicalWeapon x) => x.RequiredTechs.Any((Kerberos.Sots.Data.WeaponFramework.Tech y) => y.Name == tech.Id));
		}
		public void SetTech(App app, int TechID)
		{
			this._techID = TechID;
			string techid = app.GameDatabase.GetTechFileID(this._techID);
			int techID = this._techID;
			app.GameDatabase.GetPlayerTechInfo(app.LocalPlayer.ID, techID);
			Kerberos.Sots.Data.TechnologyFramework.Tech tech = app.AssetDatabase.MasterTechTree.Technologies.First((Kerberos.Sots.Data.TechnologyFramework.Tech x) => x.Id == techid);
			string propertyValue = ResearchInfoPanel.IconTextureToSpriteName(tech.Icon);
			TechFamily techFamily = app.AssetDatabase.MasterTechTree.TechFamilies.First((TechFamily x) => x.Id == tech.Family);
			string propertyValue2 = ResearchInfoPanel.IconTextureToSpriteName(techFamily.Icon);
			app.UI.SetPropertyString("tech_title", "text", App.Localize("@TECH_NAME_" + tech.Id));
			app.UI.SetPropertyString("techicon", "sprite", propertyValue);
			app.UI.SetPropertyString("familyicon", "sprite", propertyValue2);
			app.UI.SetText("tech_desc", App.Localize("@TECH_DESC_" + tech.Id));
			LogicalWeapon weaponUnlockedByTech = this.GetWeaponUnlockedByTech(app, tech);
			if (weaponUnlockedByTech != null)
			{
				app.UI.SetVisible("TechWeaponDetails", true);
				this._weaponinfopanel.SetWeapons(weaponUnlockedByTech, null);
				return;
			}
			app.UI.SetVisible("TechWeaponDetails", false);
		}
	}
}
