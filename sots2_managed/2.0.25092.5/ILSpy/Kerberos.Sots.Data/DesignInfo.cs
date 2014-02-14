using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Data
{
	internal class DesignInfo : IIDProvider
	{
		public int PlayerID;
		public string Name;
		public int Armour;
		public float Structure;
		public int NumTurrets;
		public float Mass;
		public float Acceleration;
		public float TopSpeed;
		public int SavingsCost;
		public int ProductionCost;
		public ShipClass Class;
		public int CrewAvailable;
		public int PowerAvailable;
		public int SupplyAvailable;
		public int CrewRequired;
		public int PowerRequired;
		public int SupplyRequired;
		public int NumBuilt;
		public int DesignDate;
		public bool isPrototyped;
		public bool isAttributesDiscovered;
		public ShipRole Role;
		public WeaponRole WeaponRole;
		public DesignSectionInfo[] DesignSections;
		public StationType StationType;
		public int StationLevel;
		public float TacSensorRange;
		public float StratSensorRange;
		public string PriorityWeaponName;
		public int NumDestroyed;
		public int RetrofitBaseID;
		public int ID
		{
			get;
			set;
		}
		public int CommandPointCost
		{
			get
			{
				int result = 0;
				switch (this.Class)
				{
				case ShipClass.Cruiser:
					result = 6;
					break;
				case ShipClass.Dreadnought:
					result = 18;
					break;
				case ShipClass.Leviathan:
					result = 54;
					break;
				}
				return result;
			}
		}
		public void HackValidateRole()
		{
			if (this.Class == ShipClass.Station && this.Role != ShipRole.PLATFORM)
			{
				this.Role = ShipRole.UNDEFINED;
			}
		}
		public bool IsAccelerator()
		{
			DesignSectionInfo[] designSections = this.DesignSections;
			for (int i = 0; i < designSections.Length; i++)
			{
				DesignSectionInfo designSectionInfo = designSections[i];
				if (designSectionInfo.ShipSectionAsset.IsAccelerator)
				{
					return true;
				}
			}
			return false;
		}
		public bool IsLoaCube()
		{
			DesignSectionInfo[] designSections = this.DesignSections;
			for (int i = 0; i < designSections.Length; i++)
			{
				DesignSectionInfo designSectionInfo = designSections[i];
				if (designSectionInfo.ShipSectionAsset.IsLoaCube)
				{
					return true;
				}
			}
			return false;
		}
		public bool IsSuperTransport()
		{
			return this.DesignSections.Any((DesignSectionInfo x) => x.ShipSectionAsset.IsSuperTransport);
		}
		public bool IsPoliceShip()
		{
			return this.DesignSections.Any((DesignSectionInfo x) => x.ShipSectionAsset.Type == ShipSectionType.Mission && x.ShipSectionAsset.isPolice);
		}
		public bool IsSDB()
		{
			return this.DesignSections.Any((DesignSectionInfo x) => x.ShipSectionAsset.RealClass == RealShipClasses.SystemDefenseBoat);
		}
		public bool IsMinelayer()
		{
			return this.DesignSections.Any((DesignSectionInfo x) => x.ShipSectionAsset.isMineLayer);
		}
		public bool IsSuulka()
		{
			return this.DesignSections.Any((DesignSectionInfo x) => x.ShipSectionAsset.IsSuulka);
		}
		public bool CanHaveAttributes()
		{
			return !this.DesignSections.Any((DesignSectionInfo x) => x.ShipSectionAsset.isPropaganda) && !this.IsPlatform() && !this.IsPoliceShip() && (!this.GetRealShipClass().HasValue || !(this.GetRealShipClass() == RealShipClasses.Drone)) && (!this.GetRealShipClass().HasValue || !(this.GetRealShipClass() == RealShipClasses.BattleRider));
		}
		public PlatformTypes? GetPlatformType()
		{
			ShipSectionAsset missionSectionAsset = this.GetMissionSectionAsset();
			if (missionSectionAsset != null)
			{
				return missionSectionAsset.GetPlatformType();
			}
			return null;
		}
		public int GetPlayerProductionCost(GameDatabase db, int player, bool isPrototype, float? overrideProductionCost = null)
		{
			float num = overrideProductionCost.HasValue ? overrideProductionCost.Value : ((float)this.ProductionCost);
			float num2 = 1f;
			float num3 = 1f;
			switch (this.Class)
			{
			case ShipClass.Cruiser:
				num2 = db.GetStratModifierFloatToApply(StratModifiers.ConstructionCostModifierCR, player);
				if (isPrototype)
				{
					num3 = db.GetStratModifierFloatToApply(StratModifiers.PrototypeConstructionCostModifierCR, player);
				}
				break;
			case ShipClass.Dreadnought:
				num2 = db.GetStratModifierFloatToApply(StratModifiers.ConstructionCostModifierDN, player);
				if (isPrototype)
				{
					num3 = db.GetStratModifierFloatToApply(StratModifiers.PrototypeConstructionCostModifierDN, player);
				}
				break;
			case ShipClass.Leviathan:
				num2 = db.GetStratModifierFloatToApply(StratModifiers.ConstructionCostModifierLV, player);
				if (isPrototype)
				{
					num3 = db.GetStratModifierFloatToApply(StratModifiers.PrototypeConstructionCostModifierLV, player);
				}
				break;
			case ShipClass.Station:
				if (this.GetRealShipClass() == RealShipClasses.Platform)
				{
					num2 = db.GetStratModifierFloatToApply(StratModifiers.ConstructionCostModifierSN, player);
					if (isPrototype)
					{
						num3 = db.GetStratModifierFloatToApply(StratModifiers.PrototypeConstructionCostModifierPF, player);
					}
				}
				else
				{
					num2 = db.GetStratModifierFloatToApply(StratModifiers.ConstructionCostModifierSN, player);
				}
				break;
			}
			return (int)(num * num2 * num3);
		}
		public DesignInfo()
		{
		}
		public DesignInfo(int playerID, string name, params string[] sections) : this(playerID, name, (IEnumerable<string>)sections)
		{
		}
		public DesignInfo(int playerID, string name, IEnumerable<string> sections)
		{
			this.PlayerID = playerID;
			this.Name = name;
			this.DesignSections = (
				from x in sections
				select new DesignSectionInfo
				{
					FilePath = x,
					DesignInfo = this
				}).ToArray<DesignSectionInfo>();
		}
		public int GetEndurance(GameSession game)
		{
			float stratModifier = game.GameDatabase.GetStratModifier<float>(StratModifiers.ShipSupplyModifier, this.PlayerID);
			float num = (float)this.SupplyAvailable;
			float num2 = (float)this.SupplyRequired;
			float val = (float)this.CrewRequired;
			return Math.Max((int)((num * stratModifier - num2) / (Math.Max(val, 2f) / 2f)), 1);
		}
		public int GetCommandPoints()
		{
			ShipSectionAsset missionSectionAsset = this.GetMissionSectionAsset();
			if (missionSectionAsset == null)
			{
				return 0;
			}
			return missionSectionAsset.CommandPoints;
		}
		public ShipSectionAsset GetMissionSectionAsset()
		{
			return (
				from x in this.DesignSections
				select x.ShipSectionAsset).FirstOrDefault((ShipSectionAsset y) => y.Type == ShipSectionType.Mission);
		}
		public RealShipClasses? GetRealShipClass()
		{
			ShipSectionAsset missionSectionAsset = this.GetMissionSectionAsset();
			if (missionSectionAsset == null)
			{
				return null;
			}
			return new RealShipClasses?(missionSectionAsset.RealClass);
		}
		public ShipSectionAsset GetCommandSectionAsset(AssetDatabase assetdb)
		{
			DesignSectionInfo[] designSections = this.DesignSections;
			for (int i = 0; i < designSections.Length; i++)
			{
				DesignSectionInfo designSectionInfo = designSections[i];
				ShipSectionAsset shipSectionAsset = assetdb.GetShipSectionAsset(designSectionInfo.FilePath);
				if (shipSectionAsset.Type == ShipSectionType.Command)
				{
					return shipSectionAsset;
				}
			}
			return null;
		}
		public bool IsPlatform()
		{
			return this.GetRealShipClass() == RealShipClasses.Platform;
		}
		public override string ToString()
		{
			return string.Format("ID={0},Name={1},Role={2},WeaponRole={3}", new object[]
			{
				this.ID,
				this.Name,
				this.Role,
				this.WeaponRole
			});
		}
	}
}
