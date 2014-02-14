using Kerberos.Sots.Engine;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.GameObjects
{
	[GameObjectType(InteropGameObjectType.IGOT_SHIPDUMMY)]
	internal class ShipDummy : CompoundGameObject, IDisposable, IActive
	{
		private class ShipDummyPart
		{
			public StaticModel Model;
			public IGameObject AttachedModel;
			public string AttachedNodeName = "";
			public bool IsSection;
		}
		private readonly List<IGameObject> _objects = new List<IGameObject>();
		private List<ShipDummy.ShipDummyPart> _dummyParts = new List<ShipDummy.ShipDummyPart>();
		private bool _active;
		private bool _checkStatusBootstrapped;
		public int ShipID;
		private int _fleetID;
		private ShipClass _shipClass;
		public int FleetID
		{
			get
			{
				return this._fleetID;
			}
			set
			{
				this._fleetID = value;
				this.PostSetProp("SetFleetID", value);
			}
		}
		public RigidBody RigidBody
		{
			get
			{
				return this._objects.OfType<RigidBody>().First<RigidBody>();
			}
		}
		public ShipClass ShipClass
		{
			get
			{
				return this._shipClass;
			}
		}
		public bool Active
		{
			get
			{
				return this._active;
			}
			set
			{
				if (value == this._active)
				{
					return;
				}
				this._active = value;
				this.PostSetActive(this._active);
			}
		}
		public ShipDummy(App game, CreateShipDummyParams dummyParams)
		{
			this.ShipID = dummyParams.ShipID;
			this._checkStatusBootstrapped = false;
			this._shipClass = dummyParams.Sections.First<ShipSectionAsset>().Class;
			ShipSectionAsset shipSectionAsset = dummyParams.Sections.FirstOrDefault((ShipSectionAsset x) => x.Type == ShipSectionType.Mission);
			ShipDummy.ShipDummyPart shipDummyPart = new ShipDummy.ShipDummyPart();
			shipDummyPart.Model = game.AddObject<StaticModel>(new object[]
			{
				Ship.FixAssetNameForDLC(shipSectionAsset.ModelName, dummyParams.PreferredMount)
			});
			shipDummyPart.IsSection = true;
			this._dummyParts.Add(shipDummyPart);
			this._objects.Add(shipDummyPart.Model);
			foreach (ShipSectionAsset current in dummyParams.Sections)
			{
				ShipDummy.ShipDummyPart shipDummyPart2 = shipDummyPart;
				if (current != shipSectionAsset)
				{
					ShipDummy.ShipDummyPart shipDummyPart3 = new ShipDummy.ShipDummyPart();
					shipDummyPart3.Model = game.AddObject<StaticModel>(new object[]
					{
						Ship.FixAssetNameForDLC(current.ModelName, dummyParams.PreferredMount)
					});
					shipDummyPart3.AttachedModel = shipDummyPart.Model;
					shipDummyPart3.AttachedNodeName = current.Type.ToString();
					shipDummyPart3.IsSection = true;
					this._dummyParts.Add(shipDummyPart3);
					this._objects.Add(shipDummyPart3.Model);
					shipDummyPart2 = shipDummyPart3;
				}
				for (int i = 0; i < current.Banks.Length; i++)
				{
					LogicalBank bank = current.Banks[i];
					this.AddTurretsToShipDummy(game, dummyParams.PreferredMount, dummyParams.ShipFaction, current, shipDummyPart2, dummyParams.AssignedWeapons, dummyParams.PreferredWeapons, game.AssetDatabase.Weapons, game.AssetDatabase.TurretHousings, null, null, bank);
				}
				for (int j = 0; j < current.Modules.Length; j++)
				{
					LogicalModuleMount moduleMount = current.Modules[j];
					if (dummyParams.AssignedModules != null)
					{
						LogicalModule logicalModule = null;
						ModuleAssignment moduleAssignment = dummyParams.AssignedModules.FirstOrDefault((ModuleAssignment x) => x.ModuleMount == moduleMount);
						if (moduleAssignment != null)
						{
							logicalModule = moduleAssignment.Module;
						}
						if (logicalModule == null)
						{
							logicalModule = LogicalModule.EnumerateModuleFits(dummyParams.PreferredModules, current, j, false).FirstOrDefault<LogicalModule>();
						}
						if (logicalModule != null)
						{
							ShipDummy.ShipDummyPart shipDummyPart4 = new ShipDummy.ShipDummyPart();
							shipDummyPart4.Model = game.AddObject<StaticModel>(new object[]
							{
								logicalModule.ModelPath
							});
							shipDummyPart4.AttachedModel = shipDummyPart2.Model;
							shipDummyPart4.AttachedNodeName = moduleMount.NodeName;
							this._dummyParts.Add(shipDummyPart4);
							this._objects.Add(shipDummyPart4.Model);
							for (int k = 0; k < logicalModule.Banks.Length; k++)
							{
								LogicalBank bank2 = logicalModule.Banks[k];
								this.AddTurretsToShipDummy(game, dummyParams.PreferredMount, dummyParams.ShipFaction, current, shipDummyPart2, dummyParams.AssignedWeapons, dummyParams.PreferredWeapons, game.AssetDatabase.Weapons, game.AssetDatabase.TurretHousings, logicalModule, shipDummyPart4, bank2);
							}
						}
					}
				}
			}
			RigidBody item = game.AddObject<RigidBody>(new object[]
			{
				1f,
				false
			});
			this._objects.Add(item);
		}
		private void AddTurretsToShipDummy(App game, string preferredMount, Faction faction, ShipSectionAsset section, ShipDummy.ShipDummyPart sectionPart, IEnumerable<WeaponAssignment> assignedWeapons, IEnumerable<LogicalWeapon> preferredWeapons, IEnumerable<LogicalWeapon> weapons, IEnumerable<LogicalTurretHousing> turretHousings, LogicalModule module, ShipDummy.ShipDummyPart modulePart, LogicalBank bank)
		{
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			string moduleNodeName = (modulePart != null) ? modulePart.AttachedNodeName : "";
			LogicalWeapon weapon = ShipDummy.SelectWeapon(section, bank, assignedWeapons, preferredWeapons, weapons, moduleNodeName, out num, out num2, out num3);
			LogicalTurretClass weaponTurretClass = weapon.GetLogicalTurretClassForMount(bank.TurretSize, bank.TurretClass);
			if (weaponTurretClass == null)
			{
				App.Log.Warn(string.Format(string.Concat(new string[]
				{
					"Ship Dummy - did not find weapon turret class for: Bank Size [",
					bank.TurretSize.ToString(),
					"], Bank Class [",
					bank.TurretClass.ToString(),
					"] in section [",
					section.FileName,
					"]"
				}), new object[0]), "design");
				return;
			}
			LogicalTurretHousing housing = turretHousings.First((LogicalTurretHousing housingCandidate) => weaponTurretClass.TurretClass == housingCandidate.Class && weapon.DefaultWeaponSize == housingCandidate.WeaponSize && bank.TurretSize == housingCandidate.MountSize);
			MountObject.WeaponModels weaponModels = new MountObject.WeaponModels();
			weaponModels.FillOutModelFilesWithWeapon(weapon, faction, weapons);
			LogicalBank localBank = bank;
			foreach (LogicalMount current in 
				from x in section.Mounts
				where x.Bank == localBank
				select x)
			{
				string baseModel = Ship.FixAssetNameForDLC(weaponTurretClass.GetBaseModel(faction, current, housing), preferredMount);
				string turretModelName = Ship.FixAssetNameForDLC(weaponTurretClass.GetTurretModelName(faction, current, housing), preferredMount);
				string barrelModelName = Ship.FixAssetNameForDLC(weaponTurretClass.GetBarrelModelName(faction, current), preferredMount);
				this.AddTurretModels(game, baseModel, turretModelName, barrelModelName, current.NodeName, sectionPart);
			}
			if (modulePart != null && module != null)
			{
				foreach (LogicalMount current2 in 
					from x in module.Mounts
					where x.Bank == localBank
					select x)
				{
					string baseModel2 = Ship.FixAssetNameForDLC(weaponTurretClass.GetBaseModel(faction, current2, housing), preferredMount);
					string turretModelName2 = Ship.FixAssetNameForDLC(weaponTurretClass.GetTurretModelName(faction, current2, housing), preferredMount);
					string barrelModelName2 = Ship.FixAssetNameForDLC(weaponTurretClass.GetBarrelModelName(faction, current2), preferredMount);
					this.AddTurretModels(game, baseModel2, turretModelName2, barrelModelName2, current2.NodeName, modulePart);
				}
			}
		}
		private void AddTurretModels(App game, string baseModel, string turretModelName, string barrelModelName, string attachedNodeName, ShipDummy.ShipDummyPart attachedPart)
		{
			if (!string.IsNullOrEmpty(baseModel))
			{
				ShipDummy.ShipDummyPart shipDummyPart = new ShipDummy.ShipDummyPart();
				shipDummyPart.Model = game.AddObject<StaticModel>(new object[]
				{
					baseModel
				});
				shipDummyPart.AttachedModel = attachedPart.Model;
				shipDummyPart.AttachedNodeName = attachedNodeName;
				this._dummyParts.Add(shipDummyPart);
				this._objects.Add(shipDummyPart.Model);
			}
			if (!string.IsNullOrEmpty(turretModelName))
			{
				ShipDummy.ShipDummyPart shipDummyPart2 = new ShipDummy.ShipDummyPart();
				shipDummyPart2.Model = game.AddObject<StaticModel>(new object[]
				{
					turretModelName
				});
				shipDummyPart2.AttachedModel = attachedPart.Model;
				shipDummyPart2.AttachedNodeName = attachedNodeName;
				this._dummyParts.Add(shipDummyPart2);
				this._objects.Add(shipDummyPart2.Model);
				if (!string.IsNullOrEmpty(barrelModelName))
				{
					ShipDummy.ShipDummyPart shipDummyPart3 = new ShipDummy.ShipDummyPart();
					shipDummyPart3.Model = game.AddObject<StaticModel>(new object[]
					{
						barrelModelName
					});
					shipDummyPart3.AttachedModel = shipDummyPart2.Model;
					shipDummyPart3.AttachedNodeName = "barrel";
					this._dummyParts.Add(shipDummyPart3);
					this._objects.Add(shipDummyPart3.Model);
				}
			}
		}
		private static LogicalWeapon SelectWeapon(ShipSectionAsset section, LogicalBank bank, IEnumerable<WeaponAssignment> assignedWeapons, IEnumerable<LogicalWeapon> preferredWeapons, IEnumerable<LogicalWeapon> weapons, string moduleNodeName, out int designID, out int targetFilter, out int fireMode)
		{
			LogicalWeapon logicalWeapon = null;
			designID = 0;
			targetFilter = 0;
			fireMode = 0;
			if (assignedWeapons != null)
			{
				WeaponAssignment weaponAssignment = assignedWeapons.FirstOrDefault((WeaponAssignment x) => x.Bank == bank && (x.ModuleNode == null || x.ModuleNode == moduleNodeName));
				if (weaponAssignment != null)
				{
					logicalWeapon = weaponAssignment.Weapon;
					designID = weaponAssignment.DesignID;
					targetFilter = (weaponAssignment.InitialTargetFilter ?? 0);
					fireMode = (weaponAssignment.InitialFireMode ?? 0);
				}
			}
			if (logicalWeapon == null && !string.IsNullOrEmpty(bank.DefaultWeaponName))
			{
				logicalWeapon = weapons.FirstOrDefault((LogicalWeapon x) => string.Equals(x.WeaponName, bank.DefaultWeaponName, StringComparison.InvariantCultureIgnoreCase));
			}
			if (logicalWeapon == null && preferredWeapons != null)
			{
				logicalWeapon = LogicalWeapon.EnumerateWeaponFits(section.Faction, section.SectionName, preferredWeapons, bank.TurretSize, bank.TurretClass).FirstOrDefault<LogicalWeapon>();
			}
			if (logicalWeapon == null)
			{
				logicalWeapon = LogicalWeapon.EnumerateWeaponFits(section.Faction, section.SectionName, weapons, bank.TurretSize, bank.TurretClass).FirstOrDefault<LogicalWeapon>();
			}
			if (logicalWeapon == null)
			{
				logicalWeapon = weapons.First<LogicalWeapon>();
			}
			return logicalWeapon;
		}
		protected override GameObjectStatus OnCheckStatus()
		{
			GameObjectStatus gameObjectStatus = base.OnCheckStatus();
			if (gameObjectStatus != GameObjectStatus.Ready)
			{
				return gameObjectStatus;
			}
			if (this._objects.Any((IGameObject x) => x.ObjectStatus == GameObjectStatus.Pending))
			{
				return GameObjectStatus.Pending;
			}
			if (!this._checkStatusBootstrapped)
			{
				this._checkStatusBootstrapped = true;
				ShipDummy.ShipDummyPart shipDummyPart = this._dummyParts.First((ShipDummy.ShipDummyPart x) => x.IsSection && x.AttachedModel == null);
				foreach (ShipDummy.ShipDummyPart current in this._dummyParts)
				{
					if (current != shipDummyPart)
					{
						if (current.IsSection)
						{
							current.Model.PostSetParent(current.AttachedModel, current.AttachedNodeName, current.AttachedNodeName);
						}
						else
						{
							current.Model.PostSetParent(current.AttachedModel, current.AttachedNodeName);
						}
					}
				}
				shipDummyPart.Model.PostSetParent(this.RigidBody);
				IGameObject[] first = new IGameObject[]
				{
					this.RigidBody
				};
				this.PostObjectAddObjects(first.Concat(
					from x in this._dummyParts
					select x.Model).ToArray<IGameObject>());
				this.RigidBody.PostSetAggregate(this);
				List<StaticModel> list = (
					from x in this._dummyParts
					where x.IsSection
					select x into y
					select y.Model).ToList<StaticModel>();
				List<object> list2 = new List<object>();
				list2.Add(list.Count);
				foreach (StaticModel current2 in list)
				{
					list2.Add(current2.ObjectID);
				}
				this.PostSetProp("CreateBoundingBox", list2.ToArray());
				this.PostSetProp("Activate", new object[0]);
			}
			return GameObjectStatus.Ready;
		}
		public void Dispose()
		{
			base.App.ReleaseObjects(this._objects.Concat(new ShipDummy[]
			{
				this
			}));
		}
	}
}
