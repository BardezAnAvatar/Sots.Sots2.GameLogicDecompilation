using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.GameStates
{
	internal class ShipBuilder : IDisposable
	{
		public const string CommandSectionIdentifier = "command";
		public const string MissionSectionIdentifier = "mission";
		public const string EngineSectionIdentifier = "engine";
		public const string CommandSectionNodeName = "Command";
		public const string EngineSectionNodeName = "Engine";
		private readonly App _game;
		private ShipSectionAsset[] _sections;
		private Ship[] _ship;
		private List<Ship> _attachedShips;
		private int _curShip;
		private int _loadShip;
		private bool _ready;
		private bool _loading = true;
		public bool Loading
		{
			get
			{
				return this._loading;
			}
		}
		public IEnumerable<ShipSectionAsset> Sections
		{
			get
			{
				return this._sections;
			}
		}
		public Ship Ship
		{
			get
			{
				if (!this._ready)
				{
					return this._ship[this._loadShip];
				}
				return this._ship[this._curShip];
			}
		}
		public ShipBuilder(App game)
		{
			this._game = game;
			this._curShip = 0;
			this._loadShip = 1;
			this._ship = new Ship[2];
			this._attachedShips = new List<Ship>();
		}
		public void Clear()
		{
			if (this._ship[this._loadShip] == null)
			{
				return;
			}
			foreach (Ship current in this._attachedShips)
			{
				current.Dispose();
			}
			this._attachedShips.Clear();
			this._ship[this._loadShip].Dispose();
			this._ship[this._loadShip] = null;
			this._ready = false;
		}
		public void ForceSyncRiders()
		{
			foreach (Ship current in this._attachedShips)
			{
				current.PostSetProp("ForceInstantSync", new object[0]);
			}
		}
		public void New(Player player, IEnumerable<ShipSectionAsset> sections, IEnumerable<LogicalTurretHousing> turretHousings, IEnumerable<LogicalWeapon> weapons, IEnumerable<LogicalWeapon> preferredWeapons, IEnumerable<WeaponAssignment> assignedWeapons, IEnumerable<LogicalModule> modules, IEnumerable<LogicalModule> preferredModules, IEnumerable<ModuleAssignment> assignedModules, IEnumerable<LogicalPsionic> psionics, DesignSectionInfo[] techs, Faction faction, string shipName, string priorityWeapon)
		{
			this.Clear();
			this._sections = sections.ToArray<ShipSectionAsset>();
			CreateShipParams createShipParams = new CreateShipParams();
			createShipParams.player = player;
			createShipParams.sections = sections;
			createShipParams.turretHousings = turretHousings;
			createShipParams.weapons = weapons;
			createShipParams.preferredWeapons = preferredWeapons;
			createShipParams.assignedWeapons = assignedWeapons;
			createShipParams.modules = modules;
			createShipParams.preferredModules = preferredModules;
			createShipParams.addPsionics = false;
			createShipParams.defenceBoatIsActive = true;
			createShipParams.priorityWeapon = priorityWeapon;
			createShipParams.assignedModules = assignedModules;
			createShipParams.psionics = psionics;
			createShipParams.faction = faction;
			createShipParams.shipName = shipName;
			DesignSectionInfo dsi;
			for (int i = 0; i < techs.Length; i++)
			{
				dsi = techs[i];
				ShipSectionAsset shipSectionAsset = dsi.ShipSectionAsset;
				if (shipSectionAsset == null)
				{
					shipSectionAsset = this._game.AssetDatabase.ShipSections.First((ShipSectionAsset x) => x.FileName == dsi.FilePath);
				}
				foreach (int current in dsi.Techs)
				{
					createShipParams.assignedTechs[(int)shipSectionAsset.Type].Techs.Add(current);
				}
			}
			createShipParams.isKillable = false;
			createShipParams.enableAI = false;
			this._ship[this._loadShip] = Ship.CreateShip(this._game, createShipParams);
			this.PostNewShip(player);
			this._ready = false;
			this._loading = true;
		}
		public void New(Player player, DesignInfo design, string shipName, int serialNumber, bool autoAddDrawable = true)
		{
			this.Clear();
			this._ship[this._loadShip] = Ship.CreateShip(this._game, Matrix.Identity, design, shipName, serialNumber, 0, 0, player, 0, -1, -1, autoAddDrawable, true, false, null);
			this.PostNewShip(player);
			this._ready = false;
			this._loading = true;
		}
		private void PostNewShip(Player player)
		{
			foreach (WeaponBank current in this._ship[this._loadShip].WeaponBanks)
			{
				current.PostSetProp("OnlyFireOnClick", true);
			}
			foreach (BattleRiderMount current2 in this._ship[this._loadShip].BattleRiderMounts)
			{
				if (current2.DesignID != 0)
				{
					ShipInfo shipInfo = new ShipInfo();
					shipInfo.DesignID = current2.DesignID;
					shipInfo.FleetID = 0;
					shipInfo.ParentID = this._ship[this._loadShip].DatabaseID;
					shipInfo.SerialNumber = 0;
					shipInfo.ShipName = string.Empty;
					shipInfo.RiderIndex = this._attachedShips.Count;
					Ship ship = Ship.CreateShip(this._game.Game, Matrix.Identity, shipInfo, this._ship[this._loadShip].ObjectID, 0, player.ObjectID, false, null);
					ship.PostSetProp("SetKillable", false);
					this._attachedShips.Add(ship);
				}
			}
		}
		public void Update()
		{
			if (!this._ready && this._ship[this._loadShip] != null && this.AllAttachedShipsLoaded() && this._ship[this._loadShip].ObjectStatus != GameObjectStatus.Pending && !this._ship[this._loadShip].Active)
			{
				this._ready = true;
				if (this._ship[this._curShip] != null)
				{
					this._ship[this._curShip].Active = false;
				}
				this._ship[this._loadShip].Active = true;
				foreach (Ship current in this._attachedShips)
				{
					current.Active = true;
				}
				int curShip = this._curShip;
				this._curShip = this._loadShip;
				this._loadShip = curShip;
				this._loading = false;
			}
		}
		private bool AllAttachedShipsLoaded()
		{
			bool result = true;
			foreach (Ship current in this._attachedShips)
			{
				if (current.ObjectStatus != GameObjectStatus.Ready)
				{
					result = false;
				}
			}
			return result;
		}
		public void Dispose()
		{
			foreach (Ship current in this._attachedShips)
			{
				current.Dispose();
			}
			this._attachedShips.Clear();
			if (this._ship[this._loadShip] != null)
			{
				this._ship[this._loadShip].Dispose();
				this._ship[this._loadShip] = null;
			}
			if (this._ship[this._curShip] != null)
			{
				this._ship[this._curShip].Dispose();
				this._ship[this._curShip] = null;
			}
		}
	}
}
