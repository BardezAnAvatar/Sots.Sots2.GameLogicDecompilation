using Kerberos.Sots.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Strategy
{
	internal class PlayerCombatData
	{
		private int _playerID;
		private int _fleetCount;
		private GameSession.VictoryStatus _victoryStatus;
		private List<WeaponData> _weaponData;
		private List<PlanetData> _planetData;
		private List<ShipData> _shipData;
		public int PlayerID
		{
			get
			{
				return this._playerID;
			}
		}
		public int FleetCount
		{
			get
			{
				return this._fleetCount;
			}
			set
			{
				this._fleetCount = value;
			}
		}
		public GameSession.VictoryStatus VictoryStatus
		{
			get
			{
				return this._victoryStatus;
			}
			set
			{
				this._victoryStatus = value;
			}
		}
		public List<WeaponData> WeaponData
		{
			get
			{
				return this._weaponData;
			}
		}
		public List<PlanetData> PlanetData
		{
			get
			{
				return this._planetData;
			}
		}
		public List<ShipData> ShipData
		{
			get
			{
				return this._shipData;
			}
		}
		private void Construct()
		{
			this._weaponData = new List<WeaponData>();
			this._planetData = new List<PlanetData>();
			this._shipData = new List<ShipData>();
		}
		public PlayerCombatData(int playerID)
		{
			this._playerID = playerID;
			this.Construct();
		}
		public PlayerCombatData(ScriptMessageReader mr, int version)
		{
			this._playerID = mr.ReadInteger();
			this._victoryStatus = (GameSession.VictoryStatus)mr.ReadInteger();
			this.Construct();
			int num = mr.ReadInteger();
			for (int i = 0; i < num; i++)
			{
				ShipData item = new ShipData
				{
					designID = mr.ReadInteger(),
					damageDealt = mr.ReadSingle(),
					damageReceived = mr.ReadSingle(),
					killCount = mr.ReadInteger(),
					destroyed = mr.ReadBool()
				};
				this._shipData.Add(item);
			}
			int num2 = mr.ReadInteger();
			for (int j = 0; j < num2; j++)
			{
				PlanetData item2 = default(PlanetData);
				item2.orbitalObjectID = mr.ReadInteger();
				item2.imperialDamage = mr.ReadDouble();
				int num3 = mr.ReadInteger();
				item2.civilianDamage = new List<PopulationData>();
				for (int k = 0; k < num3; k++)
				{
					PopulationData populationData = default(PopulationData);
					populationData.faction = mr.ReadString();
					populationData.damage = mr.ReadDouble();
				}
				item2.infrastructureDamage = mr.ReadSingle();
				item2.terraDamage = mr.ReadSingle();
				this._planetData.Add(item2);
			}
			int num4 = mr.ReadInteger();
			for (int l = 0; l < num4; l++)
			{
				WeaponData item3 = new WeaponData
				{
					weaponID = mr.ReadInteger(),
					damageDealt = mr.ReadSingle()
				};
				this._weaponData.Add(item3);
			}
			if (version >= 1)
			{
				this._fleetCount = mr.ReadInteger();
				return;
			}
			this._fleetCount = 0;
		}
		public void AddWeaponData(int ID, float damage)
		{
			WeaponData item = new WeaponData
			{
				weaponID = ID,
				damageDealt = damage
			};
			this._weaponData.Add(item);
		}
		public void AddShipData(int designID, float damageDealt, float damageReceived, int kills, bool destroyed)
		{
			ShipData item = new ShipData
			{
				designID = designID,
				damageDealt = damageDealt,
				damageReceived = damageReceived,
				killCount = kills,
				destroyed = destroyed
			};
			this._shipData.Add(item);
		}
		public void AddPlanetData(int orbitalObjectID, float terraDamage, float infraDamage, double imperialDamage, List<PopulationData> civilianDamage)
		{
			PlanetData item = new PlanetData
			{
				imperialDamage = imperialDamage,
				civilianDamage = civilianDamage,
				orbitalObjectID = orbitalObjectID,
				terraDamage = terraDamage,
				infrastructureDamage = infraDamage
			};
			this._planetData.Add(item);
		}
		public List<object> ToList()
		{
			List<object> list = new List<object>();
			list.Add(this._playerID);
			list.Add((int)this._victoryStatus);
			list.Add(this._shipData.Count<ShipData>());
			foreach (ShipData current in this._shipData)
			{
				list.Add(current.designID);
				list.Add(current.damageDealt);
				list.Add(current.damageReceived);
				list.Add(current.killCount);
				list.Add(current.destroyed);
			}
			list.Add(this._planetData.Count<PlanetData>());
			foreach (PlanetData current2 in this._planetData)
			{
				list.Add(current2.orbitalObjectID);
				list.Add(current2.imperialDamage);
				list.Add(current2.civilianDamage.Count<PopulationData>());
				foreach (PopulationData current3 in current2.civilianDamage)
				{
					list.Add(current3.faction);
					list.Add(current3.damage);
				}
				list.Add(current2.infrastructureDamage);
				list.Add(current2.terraDamage);
			}
			list.Add(this._weaponData.Count<WeaponData>());
			foreach (WeaponData current4 in this._weaponData)
			{
				list.Add(current4.weaponID);
				list.Add(current4.damageDealt);
			}
			list.Add(this._fleetCount);
			return list;
		}
	}
}
