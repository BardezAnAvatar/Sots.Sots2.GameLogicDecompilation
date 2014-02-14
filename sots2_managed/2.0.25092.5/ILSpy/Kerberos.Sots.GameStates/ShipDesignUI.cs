using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.GameStates
{
	internal static class ShipDesignUI
	{
		public static void SyncCost(App game, string panel, DesignInfo design)
		{
			int num = design.SavingsCost;
			int playerProductionCost = design.GetPlayerProductionCost(game.GameDatabase, game.LocalPlayer.ID, true, null);
			string text = GameSession.CalculateShipUpkeepCost(game.AssetDatabase, design, 1f, false).ToString("N0");
			string text2 = string.Format("({0})", GameSession.CalculateShipUpkeepCost(game.AssetDatabase, design, 1f, true).ToString("N0"));
			switch (design.Class)
			{
			case ShipClass.Cruiser:
				num = (int)((float)design.SavingsCost * game.GetStratModifier<float>(StratModifiers.PrototypeSavingsCostModifierCR, game.LocalPlayer.ID));
				break;
			case ShipClass.Dreadnought:
				num = (int)((float)design.SavingsCost * game.GetStratModifier<float>(StratModifiers.PrototypeSavingsCostModifierDN, game.LocalPlayer.ID));
				break;
			case ShipClass.Leviathan:
				num = (int)((float)design.SavingsCost * game.GetStratModifier<float>(StratModifiers.PrototypeSavingsCostModifierLV, game.LocalPlayer.ID));
				break;
			case ShipClass.Station:
				if (design.GetRealShipClass() == RealShipClasses.Platform)
				{
					num = (int)((float)design.SavingsCost * game.GetStratModifier<float>(StratModifiers.PrototypeSavingsCostModifierPF, game.LocalPlayer.ID));
				}
				break;
			}
			string text3 = design.SavingsCost.ToString("N0");
			string text4 = design.GetPlayerProductionCost(game.GameDatabase, game.LocalPlayer.ID, false, null).ToString("N0");
			string text5 = num.ToString("N0");
			string text6 = playerProductionCost.ToString("N0");
			game.UI.SetText(game.UI.Path(new string[]
			{
				panel,
				"gameShipSavCost"
			}), text3);
			game.UI.SetText(game.UI.Path(new string[]
			{
				panel,
				"gameShipConCost"
			}), text4);
			game.UI.SetText(game.UI.Path(new string[]
			{
				panel,
				"gameProtoSavCost"
			}), text5);
			game.UI.SetText(game.UI.Path(new string[]
			{
				panel,
				"gameProtoConCost"
			}), text6);
			game.UI.SetText(game.UI.Path(new string[]
			{
				panel,
				"gameShipUpkeepCost"
			}), text);
			game.UI.SetText(game.UI.Path(new string[]
			{
				panel,
				"gameShipResUpkeepCost"
			}), text2);
		}
		public static void SyncSpeed(App game, DesignInfo design)
		{
			float arg_06_0 = design.Mass;
			float acceleration = design.Acceleration;
			float topSpeed = design.TopSpeed;
			float num = topSpeed / acceleration;
			float num2 = 0f;
			float num3 = 0f;
			DesignSectionInfo[] designSections = design.DesignSections;
			for (int i = 0; i < designSections.Length; i++)
			{
				DesignSectionInfo designSectionInfo = designSections[i];
				ShipSectionAsset shipSectionAsset = designSectionInfo.ShipSectionAsset;
				if (shipSectionAsset != null)
				{
					if (shipSectionAsset.NodeSpeed > 0f)
					{
						num3 = shipSectionAsset.NodeSpeed;
					}
					if (shipSectionAsset.FtlSpeed > 0f)
					{
						num2 = shipSectionAsset.FtlSpeed;
					}
				}
			}
			float num4 = 0f;
			DesignSectionInfo[] designSections2 = design.DesignSections;
			DesignSectionInfo sectionInfo;
			for (int j = 0; j < designSections2.Length; j++)
			{
				sectionInfo = designSections2[j];
				ShipSectionAsset shipSectionAsset2 = game.AssetDatabase.ShipSections.First((ShipSectionAsset x) => x.FileName == sectionInfo.FilePath);
				num4 += shipSectionAsset2.Maneuvering.RotationSpeed;
			}
			string text = string.Format("{0} kg", design.Mass);
			string text2 = string.Format("{0} km/s²", design.Acceleration);
			string text3 = string.Format("{0} deg/s", num4);
			string text4 = string.Format("{0} km/s (in {1}s)", topSpeed, Math.Max(1, (int)num));
			PlayerInfo playerInfo = game.GameDatabase.GetPlayerInfo(design.PlayerID);
			Faction faction = game.AssetDatabase.GetFaction(playerInfo.FactionID);
			if (faction.CanUseNodeLine(null))
			{
				game.UI.SetText("gameShipFTLSpeed", string.Format("{0} ly", num3));
			}
			else
			{
				game.UI.SetText("gameShipFTLSpeed", string.Format("{0} ly", num2));
			}
			game.UI.SetText("gameShipTopSpeedTime", text4);
			game.UI.SetText("gameShipTurnSpeed", text3);
			game.UI.SetText("gameShipThrust", text2);
			game.UI.SetText("gameShipMass", text);
			game.UI.SetPropertyFloat("gameSpeedGraph", "accel", acceleration);
			game.UI.SetPropertyFloat("gameSpeedGraph", "max_speed", topSpeed);
		}
		public static void SyncSupplies(App game, DesignInfo design)
		{
			int num = (int)((float)design.SupplyAvailable * game.GetStratModifier<float>(StratModifiers.ShipSupplyModifier, design.PlayerID));
			int supplyRequired = design.SupplyRequired;
			int propertyValue = num;
			int crewAvailable = design.CrewAvailable;
			int crewRequired = design.CrewRequired;
			int propertyValue2 = crewAvailable;
			int powerAvailable = design.PowerAvailable;
			int powerRequired = design.PowerRequired;
			int propertyValue3 = powerAvailable;
			int endurance = design.GetEndurance(game.Game);
			float num2 = 1f;
			PlayerInfo pi = game.GameDatabase.GetPlayerInfo(design.PlayerID);
			if (pi != null)
			{
				Faction faction = game.AssetDatabase.Factions.FirstOrDefault((Faction x) => x.ID == pi.FactionID);
				if (faction != null)
				{
					num2 = faction.CrewEfficiencyValue;
				}
			}
			game.UI.SetPropertyInt("gameShipSupply", "max", num);
			game.UI.SetPropertyInt("gameShipSupply", "req", supplyRequired);
			game.UI.SetPropertyInt("gameShipSupply", "cur", propertyValue);
			game.UI.SetPropertyInt("gameShipCrew", "max", crewAvailable);
			game.UI.SetPropertyInt("gameShipCrew", "req", (int)((float)crewRequired / num2));
			game.UI.SetPropertyInt("gameShipCrew", "cur", propertyValue2);
			game.UI.SetPropertyInt("gameShipEnergy", "max", powerAvailable);
			game.UI.SetPropertyInt("gameShipEnergy", "req", powerRequired);
			game.UI.SetPropertyInt("gameShipEnergy", "cur", propertyValue3);
			game.UI.SetPropertyString("gameShipEnduranceVal", "text", string.Format("{0}T", endurance));
			if (design.Role == ShipRole.DRONE || design.Role == ShipRole.ASSAULTSHUTTLE)
			{
				game.UI.SetVisible("gameShipEnduranceVal", false);
				game.UI.SetVisible("gameShipEnduranceLab", false);
			}
			else
			{
				game.UI.SetVisible("gameShipEnduranceVal", true);
				game.UI.SetVisible("gameShipEnduranceLab", true);
			}
			if (game.AssetDatabase.GetFaction(game.GameDatabase.GetPlayerFactionID(design.PlayerID)).Name == "loa" || design.Role == ShipRole.DRONE || design.Role == ShipRole.ASSAULTSHUTTLE)
			{
				game.UI.SetVisible("gameShipCrew", false);
				game.UI.SetVisible("gameShipCrewIcon", false);
				return;
			}
			game.UI.SetVisible("gameShipCrew", true);
			game.UI.SetVisible("gameShipCrewIcon", true);
		}
		public static void PopulateDesignList(App game, string designListId, IEnumerable<InvoiceInfo> invoices)
		{
			game.UI.ClearItems(designListId);
			foreach (InvoiceInfo current in invoices)
			{
				game.UI.AddItem(designListId, string.Empty, current.ID, current.Name);
				game.UI.SetItemPropertyString(designListId, string.Empty, current.ID, "designName", "text", current.Name);
				game.UI.SetItemPropertyString(designListId, string.Empty, current.ID, "designDeleteButton", "id", "designDeleteButton|" + current.ID.ToString() + "|invoice");
			}
		}
		public static void PopulateDesignList(App game, string designListId, IEnumerable<DesignInfo> designs)
		{
			game.UI.ClearItems(designListId);
			foreach (DesignInfo current in designs)
			{
                if (!Kerberos.Sots.StarFleet.StarFleet.DesignIsSuulka(game, current) && !current.IsAccelerator() && !current.IsLoaCube() && BuildScreenState.IsShipRoleAllowed(current.Role))
				{
					game.UI.AddItem(designListId, string.Empty, current.ID, current.Name);
					game.UI.SetItemPropertyString(designListId, string.Empty, current.ID, "designName", "text", current.Name);
					game.UI.SetItemPropertyString(designListId, string.Empty, current.ID, "designDeleteButton", "id", "designDeleteButton|" + current.ID.ToString());
					if (!current.isPrototyped)
					{
						List<BuildOrderInfo> list = game.GameDatabase.GetDesignBuildOrders(current).ToList<BuildOrderInfo>();
						if (list.Count > 0)
						{
							game.UI.SetItemPropertyColor(designListId, string.Empty, current.ID, "designName", "color", new Vector3(0f, 80f, 104f));
						}
						else
						{
							game.UI.SetItemPropertyColor(designListId, string.Empty, current.ID, "designName", "color", new Vector3(147f, 64f, 147f));
						}
					}
					else
					{
						game.UI.SetItemPropertyColor(designListId, string.Empty, current.ID, "designName", "color", new Vector3(11f, 157f, 194f));
					}
				}
			}
		}
		public static ShipSectionAsset GetSectionAsset(App game, DesignInfo design, ShipSectionType sectionType)
		{
			DesignSectionInfo[] designSections = design.DesignSections;
			DesignSectionInfo designSection;
			for (int i = 0; i < designSections.Length; i++)
			{
				designSection = designSections[i];
				ShipSectionAsset shipSectionAsset = game.AssetDatabase.ShipSections.First((ShipSectionAsset x) => x.FileName == designSection.FilePath);
				if (shipSectionAsset.Type == sectionType)
				{
					return shipSectionAsset;
				}
			}
			return null;
		}
		public static void SyncSectionArmor(App game, string panelId, ShipSectionAsset sectionAsset, DesignInfo design)
		{
			string panelId2 = game.UI.Path(new string[]
			{
				panelId,
				"partArmor"
			});
			string panelId3 = game.UI.Path(new string[]
			{
				panelId,
				"partArmorTop"
			});
			string panelId4 = game.UI.Path(new string[]
			{
				panelId,
				"partArmorBtm"
			});
			string panelId5 = game.UI.Path(new string[]
			{
				panelId,
				"partArmorSide"
			});
			string panelId6 = game.UI.Path(new string[]
			{
				panelId,
				"partStruct"
			});
			string panelId7 = game.UI.Path(new string[]
			{
				panelId,
				"partStructBar"
			});
			int num = 0;
			int num2 = sectionAsset.Structure;
			if (design != null)
			{
				DesignSectionInfo designSectionInfo = design.DesignSections.FirstOrDefault((DesignSectionInfo x) => x.FilePath == sectionAsset.FileName);
				List<string> techs = (
					from x in designSectionInfo.Techs
					select game.GameDatabase.GetTechFileID(x)).ToList<string>();
				num = Ship.GetArmorBonusFromTech(game.AssetDatabase, techs);
				num2 = Ship.GetStructureWithTech(game.AssetDatabase, techs, num2);
				foreach (DesignModuleInfo current in designSectionInfo.Modules)
				{
					string moduleAsset = game.GameDatabase.GetModuleAsset(current.ModuleID);
					LogicalModule logicalModule = game.AssetDatabase.Modules.FirstOrDefault((LogicalModule x) => x.ModulePath == moduleAsset);
					if (logicalModule != null)
					{
						num2 += (int)logicalModule.StructureBonus;
					}
				}
			}
			int propertyValue = sectionAsset.Armor[1].Y + num;
			int propertyValue2 = sectionAsset.Armor[3].Y + num;
			int propertyValue3 = Math.Max(sectionAsset.Armor[2].Y, sectionAsset.Armor[0].Y) + num;
			int num3 = sectionAsset.Armor[1].X * (sectionAsset.Armor[1].Y + num) + sectionAsset.Armor[3].X * (sectionAsset.Armor[3].Y + num) + sectionAsset.Armor[0].X * (sectionAsset.Armor[0].Y + num) + sectionAsset.Armor[2].X * (sectionAsset.Armor[2].Y + num);
			int propertyValue4 = 10;
			int propertyValue5 = 10000;
			string text = num3.ToString("N0");
			string text2 = num2.ToString("N0");
			game.UI.SetText(panelId2, text);
			game.UI.SetPropertyInt(panelId3, "value", propertyValue);
			game.UI.SetPropertyInt(panelId4, "value", propertyValue2);
			game.UI.SetPropertyInt(panelId5, "value", propertyValue3);
			game.UI.SetPropertyInt(panelId3, "max_value", propertyValue4);
			game.UI.SetPropertyInt(panelId4, "max_value", propertyValue4);
			game.UI.SetPropertyInt(panelId5, "max_value", propertyValue4);
			game.UI.SetText(panelId6, text2);
			game.UI.SetPropertyInt(panelId7, "value", num2);
			game.UI.SetPropertyInt(panelId7, "max_value", propertyValue5);
		}
	}
}
