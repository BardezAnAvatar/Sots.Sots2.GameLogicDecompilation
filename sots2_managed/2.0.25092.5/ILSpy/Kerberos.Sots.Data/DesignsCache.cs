using Kerberos.Sots.Data.ModuleFramework;
using Kerberos.Sots.Data.ShipFramework;
using Kerberos.Sots.Data.SQLite;
using Kerberos.Sots.Engine;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace Kerberos.Sots.Data
{
	internal sealed class DesignsCache : RowCache<int, DesignInfo>
	{
		public DesignsCache(SQLiteConnection db, AssetDatabase assets) : base(db, assets)
		{
		}
		private static ModulePsionicInfo GetModulePsionicInfoFromRow(Row row)
		{
			return new ModulePsionicInfo
			{
				ID = row[0].SQLiteValueToInteger(),
				DesignModuleID = row[1].SQLiteValueToInteger(),
				Ability = (SectionEnumerations.PsionicAbility)row[2].SQLiteValueToInteger()
			};
		}
		private static IEnumerable<ModulePsionicInfo> GetModulePsionicInfosByDesignModule(SQLiteConnection db, int designModuleId)
		{
			foreach (Row current in db.ExecuteTableQuery(string.Format(Queries.GetModulePsionicsByDesignModule, designModuleId.ToSQLiteValue()), true))
			{
				yield return DesignsCache.GetModulePsionicInfoFromRow(current);
			}
			yield break;
		}
		internal static DesignModuleInfo GetDesignModuleInfoFromRow(SQLiteConnection db, Row row, DesignSectionInfo designSection)
		{
			int num = row[0].SQLiteValueToInteger();
			row[1].SQLiteValueToInteger();
			DesignModuleInfo designModuleInfo = new DesignModuleInfo();
			designModuleInfo.ID = num;
			designModuleInfo.DesignSectionInfo = designSection;
			designModuleInfo.ModuleID = row[2].SQLiteValueToInteger();
			designModuleInfo.WeaponID = row[3].SQLiteValueToNullableInteger();
			designModuleInfo.MountNodeName = row[4].SQLiteValueToString();
			DesignModuleInfo arg_92_0 = designModuleInfo;
			int? num2 = row[5].SQLiteValueToNullableInteger();
			arg_92_0.StationModuleType = (num2.HasValue ? new ModuleEnums.StationModuleType?((ModuleEnums.StationModuleType)num2.GetValueOrDefault()) : null);
			designModuleInfo.DesignID = ((row.Count<string>() > 6) ? row[6].SQLiteValueToNullableInteger() : null);
			designModuleInfo.PsionicAbilities = DesignsCache.GetModulePsionicInfosByDesignModule(db, num).ToList<ModulePsionicInfo>();
			return designModuleInfo;
		}
		private static IEnumerable<DesignModuleInfo> GetModuleInfosForDesignSection(SQLiteConnection db, DesignSectionInfo designSection)
		{
			foreach (Row current in db.ExecuteTableQuery(string.Format(Queries.GetDesignModuleInfos, designSection.ID.ToSQLiteValue()), true))
			{
				yield return DesignsCache.GetDesignModuleInfoFromRow(db, current, designSection);
			}
			yield break;
		}
		private static WeaponBankInfo GetWeaponBankInfoFromRow(Row row)
		{
			return new WeaponBankInfo
			{
				ID = row[0].SQLiteValueToInteger(),
				WeaponID = row[1].SQLiteValueToNullableInteger(),
				DesignID = row[2].SQLiteValueToNullableInteger(),
				BankGUID = row[3].SQLiteValueToGuid(),
				FiringMode = row[4].SQLiteValueToNullableInteger(),
				FilterMode = row[5].SQLiteValueToNullableInteger()
			};
		}
		private static IEnumerable<WeaponBankInfo> GetWeaponBankInfosForDesignSection(SQLiteConnection db, int designSectionID)
		{
			foreach (Row current in db.ExecuteTableQuery(string.Format(Queries.GetWeaponBankInfos, designSectionID.ToSQLiteValue()), true))
			{
				yield return DesignsCache.GetWeaponBankInfoFromRow(current);
			}
			yield break;
		}
		private static IEnumerable<int> GetTechsForDesignSection(SQLiteConnection db, int designSectionId)
		{
			foreach (Row current in db.ExecuteTableQuery(string.Format(Queries.GetDesignSectionTechs, designSectionId), true))
			{
				yield return current[0].SQLiteValueToInteger();
			}
			yield break;
		}
		private static DesignSectionInfo GetDesignSectionInfoFromRow(Row row, SQLiteConnection db, AssetDatabase assets)
		{
			int num = row[0].SQLiteValueToInteger();
			string text = row[1].SQLiteValueToString();
			DesignSectionInfo designSectionInfo = new DesignSectionInfo
			{
				ID = num,
				FilePath = text,
				ShipSectionAsset = assets.GetShipSectionAsset(text),
				WeaponBanks = DesignsCache.GetWeaponBankInfosForDesignSection(db, num).ToList<WeaponBankInfo>(),
				Techs = DesignsCache.GetTechsForDesignSection(db, num).ToList<int>()
			};
			List<DesignModuleInfo> list = DesignsCache.GetModuleInfosForDesignSection(db, designSectionInfo).ToList<DesignModuleInfo>();
			designSectionInfo.Modules = new List<DesignModuleInfo>();
			LogicalModuleMount[] modules = designSectionInfo.ShipSectionAsset.Modules;
			LogicalModuleMount mount;
			for (int i = 0; i < modules.Length; i++)
			{
				mount = modules[i];
				DesignModuleInfo designModuleInfo = list.FirstOrDefault((DesignModuleInfo x) => x.MountNodeName == mount.NodeName);
				if (designModuleInfo != null)
				{
					designSectionInfo.Modules.Add(designModuleInfo);
					list.Remove(designModuleInfo);
				}
			}
			return designSectionInfo;
		}
		private static IEnumerable<DesignSectionInfo> GetDesignSectionInfos(SQLiteConnection db, AssetDatabase assets, int designID)
		{
			foreach (Row current in db.ExecuteTableQuery(string.Format(Queries.GetDesignSectionInfo, designID), true))
			{
				yield return DesignsCache.GetDesignSectionInfoFromRow(current, db, assets);
			}
			yield break;
		}
		private static DesignInfo GetDesignInfoFromRow(SQLiteConnection db, AssetDatabase assets, Row row)
		{
			int num = row[0].SQLiteValueToInteger();
			DesignInfo designInfo = new DesignInfo
			{
				ID = num,
				DesignSections = DesignsCache.GetDesignSectionInfos(db, assets, num).ToArray<DesignSectionInfo>(),
				PlayerID = row[1].SQLiteValueToOneBasedIndex(),
				Name = row[2],
				Armour = int.Parse(row[3]),
				Structure = float.Parse(row[4]),
				NumTurrets = int.Parse(row[5]),
				Mass = float.Parse(row[6]),
				Acceleration = float.Parse(row[8]),
				TopSpeed = float.Parse(row[9]),
				SavingsCost = int.Parse(row[10]),
				ProductionCost = int.Parse(row[11]),
				Class = (ShipClass)int.Parse(row[12]),
				CrewAvailable = int.Parse(row[13]),
				PowerAvailable = int.Parse(row[14]),
				SupplyAvailable = int.Parse(row[15]),
				CrewRequired = int.Parse(row[16]),
				PowerRequired = int.Parse(row[17]),
				SupplyRequired = int.Parse(row[18]),
				NumBuilt = int.Parse(row[19]),
				DesignDate = int.Parse(row[20]),
				Role = (ShipRole)int.Parse(row[21]),
				WeaponRole = (WeaponRole)int.Parse(row[22]),
				isPrototyped = bool.Parse(row[23]),
				isAttributesDiscovered = bool.Parse(row[24]),
				StationType = (StationType)int.Parse(row[25]),
				StationLevel = int.Parse(row[26]),
				PriorityWeaponName = row[28] ?? string.Empty,
				NumDestroyed = int.Parse(row[29]),
				RetrofitBaseID = row[30].SQLiteValueToOneBasedIndex()
			};
			designInfo.HackValidateRole();
			DesignSectionInfo[] designSections = designInfo.DesignSections;
			for (int i = 0; i < designSections.Length; i++)
			{
				DesignSectionInfo designSectionInfo = designSections[i];
				designSectionInfo.DesignInfo = designInfo;
			}
			return designInfo;
		}
		protected override IEnumerable<KeyValuePair<int, DesignInfo>> OnSynchronizeWithDatabase(SQLiteConnection db, IEnumerable<int> range)
		{
            IEnumerable<KeyValuePair<int, DesignInfo>> values = null;

			if (range == null)
			{
                //foreach (Row current in db.ExecuteTableQuery(Queries.GetDesignInfos, true))
                //{
                //    DesignInfo designInfoFromRow = DesignsCache.GetDesignInfoFromRow(db, base.Assets, current);
                //    yield return new KeyValuePair<int, DesignInfo>(designInfoFromRow.ID, designInfoFromRow);
                //}

                values = Bardez.Projects.SwordOfTheStars.SotS2.Fixes.Sots.Data.GameDatabase.RetrieveSavedShipDesigns(db, base.Assets);
			}
			else
			{
                List<KeyValuePair<Int32, DesignInfo>> rangedOutput = new List<KeyValuePair<Int32, DesignInfo>>();

				foreach (int current2 in range)
				{
					foreach (Row current3 in db.ExecuteTableQuery(string.Format(Queries.GetDesignInfo, current2.ToSQLiteValue()), true))
					{
						DesignInfo designInfoFromRow2 = DesignsCache.GetDesignInfoFromRow(db, base.Assets, current3);
                        rangedOutput.Add(new KeyValuePair<int, DesignInfo>(designInfoFromRow2.ID, designInfoFromRow2));
					}
				}

                values = rangedOutput;
			}

            return values;
		}
		private static int InsertWeaponBank(SQLiteConnection db, int designSectionId, WeaponBankInfo value)
		{
			return db.ExecuteIntegerQuery(string.Format(Queries.InsertWeaponBank, new object[]
			{
				designSectionId.ToSQLiteValue(),
				value.WeaponID.ToNullableSQLiteValue(),
				value.DesignID.ToNullableSQLiteValue(),
				value.FiringMode.ToNullableSQLiteValue(),
				value.FilterMode.ToNullableSQLiteValue(),
				value.BankGUID.ToSQLiteValue()
			}));
		}
		internal static int InsertDesignModuleInfo(SQLiteConnection db, DesignModuleInfo value)
		{
			string arg_89_0 = Queries.InsertDesignModule;
			object[] array = new object[6];
			array[0] = value.DesignSectionInfo.ID.ToSQLiteValue();
			array[1] = value.MountNodeName.ToSQLiteValue();
			array[2] = value.ModuleID.ToSQLiteValue();
			array[3] = value.WeaponID.ToNullableSQLiteValue();
			object[] arg_79_0 = array;
			int arg_79_1 = 4;
			ModuleEnums.StationModuleType? stationModuleType = value.StationModuleType;
			arg_79_0[arg_79_1] = (stationModuleType.HasValue ? new int?((int)stationModuleType.GetValueOrDefault()) : null).ToNullableSQLiteValue();
			array[5] = value.DesignID.ToNullableSQLiteValue();
			int num = db.ExecuteIntegerQuery(string.Format(arg_89_0, array));
			if (value.PsionicAbilities == null)
			{
				value.PsionicAbilities = new List<ModulePsionicInfo>();
			}
			foreach (ModulePsionicInfo current in value.PsionicAbilities)
			{
				current.ID = db.ExecuteIntegerQuery(string.Format(Queries.InsertModulePsionicAbility, num.ToSQLiteValue(), ((int)current.Ability).ToSQLiteValue()));
			}
			return num;
		}
		protected override int OnInsert(SQLiteConnection db, int? key, DesignInfo value)
		{
			if (key.HasValue)
			{
				throw new ArgumentOutOfRangeException("key", "Design insertion does not permit explicit specification of an ID.");
			}
			if (ScriptHost.AllowConsole)
			{
				if (value.DesignSections == null || value.DesignSections.Length == 0)
				{
					throw new InvalidDataException("DesignInfo does not supply any DesignSections.");
				}
			}
			else
			{
				RowCache<int, DesignInfo>.Warn(string.Format("DesignsCache.OnInsert: DesignInfo does not supply any DesignSections (player={0}, stationType={1}, stationLevel={2})", value.PlayerID, value.StationType.ToString(), value.StationLevel.ToString()));
			}
			value.HackValidateRole();
			int num = db.ExecuteIntegerQuery(string.Format(Queries.InsertDesign, new object[]
			{
				value.PlayerID.ToOneBasedSQLiteValue(),
				value.Name.ToSQLiteValue(),
				value.Armour.ToSQLiteValue(),
				value.Structure.ToSQLiteValue(),
				value.NumTurrets.ToSQLiteValue(),
				value.Mass.ToSQLiteValue(),
				0.ToSQLiteValue(),
				value.Acceleration.ToSQLiteValue(),
				value.TopSpeed.ToSQLiteValue(),
				value.SavingsCost.ToSQLiteValue(),
				value.ProductionCost.ToSQLiteValue(),
				((int)value.Class).ToSQLiteValue(),
				value.CrewAvailable.ToSQLiteValue(),
				value.PowerAvailable.ToSQLiteValue(),
				value.SupplyAvailable.ToSQLiteValue(),
				value.CrewRequired.ToSQLiteValue(),
				value.PowerRequired.ToSQLiteValue(),
				value.SupplyRequired.ToSQLiteValue(),
				GameDatabase.GetTurnCount(db).ToSQLiteValue(),
				((int)value.Role).ToSQLiteValue(),
				((int)value.WeaponRole).ToSQLiteValue(),
				value.isPrototyped.ToSQLiteValue(),
				value.isAttributesDiscovered.ToSQLiteValue(),
				((int)value.StationType).ToSQLiteValue(),
				value.StationLevel.ToSQLiteValue(),
				value.PriorityWeaponName.ToSQLiteValue(),
				value.RetrofitBaseID.ToOneBasedSQLiteValue()
			}));
			value.ID = num;
			DesignSectionInfo[] designSections = value.DesignSections;
			for (int i = 0; i < designSections.Length; i++)
			{
				DesignSectionInfo designSectionInfo = designSections[i];
				designSectionInfo.DesignInfo = value;
				int num2 = db.ExecuteIntegerQuery(string.Format(Queries.InsertDesignSection.ToSQLiteValue(), num.ToSQLiteValue(), designSectionInfo.FilePath.ToSQLiteValue()));
				designSectionInfo.ID = num2;
				if (designSectionInfo.WeaponBanks == null)
				{
					designSectionInfo.WeaponBanks = new List<WeaponBankInfo>();
				}
				foreach (WeaponBankInfo current in designSectionInfo.WeaponBanks)
				{
					current.ID = DesignsCache.InsertWeaponBank(db, num2, current);
				}
				if (designSectionInfo.Modules == null)
				{
					designSectionInfo.Modules = new List<DesignModuleInfo>();
				}
				foreach (DesignModuleInfo current2 in designSectionInfo.Modules)
				{
					current2.DesignSectionInfo = designSectionInfo;
					current2.ID = DesignsCache.InsertDesignModuleInfo(db, current2);
				}
				if (designSectionInfo.Techs == null)
				{
					designSectionInfo.Techs = new List<int>();
				}
				foreach (int current3 in designSectionInfo.Techs)
				{
					db.ExecuteIntegerQuery(string.Format(Queries.InsertDesignSectionTech, num2.ToSQLiteValue(), current3.ToSQLiteValue()));
				}
			}
			return num;
		}
		protected override void OnRemove(SQLiteConnection db, int key)
		{
			db.ExecuteIntegerQuery(string.Format(Queries.RemoveDesign, key.ToSQLiteValue()));
		}
		protected override void OnUpdate(SQLiteConnection db, int key, DesignInfo value)
		{
			db.ExecuteIntegerQuery(string.Format(Queries.UpdateDesign, new object[]
			{
				value.ID.ToSQLiteValue(),
				value.PlayerID.ToSQLiteValue(),
				value.Name.ToSQLiteValue(),
				value.Armour.ToSQLiteValue(),
				value.Structure.ToSQLiteValue(),
				value.NumTurrets.ToSQLiteValue(),
				value.Mass.ToSQLiteValue(),
				0.ToSQLiteValue(),
				value.Acceleration.ToSQLiteValue(),
				value.TopSpeed.ToSQLiteValue(),
				value.SavingsCost.ToSQLiteValue(),
				value.ProductionCost.ToSQLiteValue(),
				((int)value.Class).ToSQLiteValue(),
				value.CrewAvailable.ToSQLiteValue(),
				value.PowerAvailable.ToSQLiteValue(),
				value.SupplyAvailable.ToSQLiteValue(),
				value.CrewRequired.ToSQLiteValue(),
				value.PowerRequired.ToSQLiteValue(),
				value.SupplyRequired.ToSQLiteValue(),
				GameDatabase.GetTurnCount(db).ToSQLiteValue(),
				((int)value.Role).ToSQLiteValue(),
				((int)value.WeaponRole).ToSQLiteValue(),
				value.isPrototyped.ToSQLiteValue(),
				value.isAttributesDiscovered.ToSQLiteValue(),
				((int)value.StationType).ToSQLiteValue(),
				value.StationLevel.ToSQLiteValue()
			}));
		}
	}
}
