using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.GameObjects
{
	internal class CreateShipParams
	{
		public Player player;
		public IEnumerable<ShipSectionAsset> sections = new ShipSectionAsset[0];
		public IEnumerable<SectionInstanceInfo> sectionInstances = new SectionInstanceInfo[0];
		public IEnumerable<LogicalTurretHousing> turretHousings = new LogicalTurretHousing[0];
		public IEnumerable<LogicalWeapon> weapons = new LogicalWeapon[0];
		public IEnumerable<LogicalWeapon> preferredWeapons = new LogicalWeapon[0];
		public IEnumerable<WeaponAssignment> assignedWeapons = new WeaponAssignment[0];
		public IEnumerable<LogicalModule> modules = new LogicalModule[0];
		public IEnumerable<LogicalModule> preferredModules = new LogicalModule[0];
		public IEnumerable<ModuleAssignment> assignedModules = new ModuleAssignment[0];
		public IEnumerable<LogicalPsionic> psionics = new LogicalPsionic[0];
		public List<Player> playersInCombat = new List<Player>();
		public AssignedSectionTechs[] assignedTechs = new AssignedSectionTechs[3];
		public Faction faction;
		public Matrix? spawnMatrix = null;
		public string shipName = "";
		public string shipDesignName = "";
		public string priorityWeapon = "";
		public int serialNumber;
		public int parentID;
		public int inputID;
		public ShipRole role;
		public WeaponRole wpnRole;
		public int databaseId;
		public int designId;
		public bool isKillable = true;
		public bool enableAI = true;
		public bool addPsionics = true;
		public bool isInDeepSpace;
		public int riderindex = -1;
		public int parentDBID;
		public int curPsiPower;
		public bool defenceBoatIsActive;
		public int defenceBoatOrbitalID;
		public double obtainedSlaves;
		public bool AutoAddDrawable = true;
		public CreateShipParams()
		{
			for (int i = 0; i <= 2; i++)
			{
				this.assignedTechs[i] = new AssignedSectionTechs();
			}
		}
	}
}
