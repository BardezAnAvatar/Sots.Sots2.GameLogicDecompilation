using Kerberos.Sots.Data;
using Kerberos.Sots.PlayerFramework;
using System;
namespace Kerberos.Sots.Strategy
{
	internal class TurnEventMacros
	{
		private delegate TDataObject MapDataObject<TKey, TDataObject>(TKey id) where TDataObject : new();
		private delegate string EvaluateDataObjectDelegate<TDataObject>(TDataObject o) where TDataObject : new();
		private readonly GameDatabase db;
		private readonly AssetDatabase assetdb;
		private readonly TurnEvent evidence;
		private string playerName;
		private string admiralName;
		private string targetPlayerName;
		private string systemName;
		private string system2Name;
		private string orbitalName;
		private string colonyName;
		private string fleetName;
		private string techName;
		private string stationType;
		private string stationLevel;
		private string designName;
		private string specialProjectName;
		private string shipName;
		private string provinceName;
		public string PlayerName
		{
			get
			{
				return this.EvaluateDataObject<int, PlayerInfo>(ref this.playerName, new TurnEventMacros.MapDataObject<int, PlayerInfo>(this.db.GetPlayerInfo), (PlayerInfo o) => o.Name, this.evidence.PlayerID);
			}
		}
		public string TargetPlayerName
		{
			get
			{
				return this.EvaluateDataObject<int, PlayerInfo>(ref this.targetPlayerName, new TurnEventMacros.MapDataObject<int, PlayerInfo>(this.db.GetPlayerInfo), (PlayerInfo o) => o.Name, this.evidence.TargetPlayerID);
			}
		}
		public string SystemName
		{
			get
			{
				return this.EvaluateDataObject<int, StarSystemInfo>(ref this.systemName, new TurnEventMacros.MapDataObject<int, StarSystemInfo>(this.db.GetStarSystemInfo), (StarSystemInfo o) => o.Name, this.evidence.SystemID);
			}
		}
		public string OrbitalName
		{
			get
			{
				return this.EvaluateDataObject<int, OrbitalObjectInfo>(ref this.orbitalName, new TurnEventMacros.MapDataObject<int, OrbitalObjectInfo>(this.db.GetOrbitalObjectInfo), (OrbitalObjectInfo o) => o.Name, this.evidence.OrbitalID);
			}
		}
		public string AdmiralName
		{
			get
			{
				return this.EvaluateDataObject<int, AdmiralInfo>(ref this.admiralName, new TurnEventMacros.MapDataObject<int, AdmiralInfo>(this.db.GetAdmiralInfo), (AdmiralInfo o) => o.Name, this.evidence.AdmiralID);
			}
		}
		public string ColonyName
		{
			get
			{
				return this.EvaluateDataObject<int, ColonyInfo>(ref this.colonyName, new TurnEventMacros.MapDataObject<int, ColonyInfo>(this.db.GetColonyInfo), new TurnEventMacros.EvaluateDataObjectDelegate<ColonyInfo>(this.GetColonyName), this.evidence.ColonyID);
			}
		}
		public string FleetName
		{
			get
			{
				return this.EvaluateDataObject<int, FleetInfo>(ref this.fleetName, new TurnEventMacros.MapDataObject<int, FleetInfo>(this.db.GetFleetInfo), new TurnEventMacros.EvaluateDataObjectDelegate<FleetInfo>(this.GetFleetName), this.evidence.FleetID);
			}
		}
		public string FeasibilityPercent
		{
			get
			{
				return this.evidence.FeasibilityPercent.ToString();
			}
		}
		public string TechName
		{
			get
			{
				return this.EvaluateDataObject<PlayerTechInfo.PrimaryKey, PlayerTechInfo>(ref this.techName, new TurnEventMacros.MapDataObject<PlayerTechInfo.PrimaryKey, PlayerTechInfo>(this.db.GetPlayerTechInfo), new TurnEventMacros.EvaluateDataObjectDelegate<PlayerTechInfo>(this.GetTechName), this.evidence.PlayerTechID);
			}
		}
		public string StationType
		{
			get
			{
				return this.EvaluateDataObject<int, StationInfo>(ref this.stationType, new TurnEventMacros.MapDataObject<int, StationInfo>(this.db.GetStationInfo), new TurnEventMacros.EvaluateDataObjectDelegate<StationInfo>(this.GetStationType), this.evidence.OrbitalID);
			}
		}
		public string StationLevel
		{
			get
			{
				return this.EvaluateDataObject<int, StationInfo>(ref this.stationLevel, new TurnEventMacros.MapDataObject<int, StationInfo>(this.db.GetStationInfo), new TurnEventMacros.EvaluateDataObjectDelegate<StationInfo>(this.GetStationLevel), this.evidence.OrbitalID);
			}
		}
		public string StationName
		{
			get
			{
				return this.EvaluateDataObject<int, StationInfo>(ref this.stationType, new TurnEventMacros.MapDataObject<int, StationInfo>(this.db.GetStationInfo), new TurnEventMacros.EvaluateDataObjectDelegate<StationInfo>(this.GetStationName), this.evidence.OrbitalID);
			}
		}
		public string DesignName
		{
			get
			{
				return this.EvaluateDataObject<int, DesignInfo>(ref this.designName, new TurnEventMacros.MapDataObject<int, DesignInfo>(this.GetDesignInfo), (DesignInfo o) => o.Name, this.evidence.DesignID);
			}
		}
		public string SpecialProjectName
		{
			get
			{
				return this.EvaluateDataObject<int, SpecialProjectInfo>(ref this.specialProjectName, new TurnEventMacros.MapDataObject<int, SpecialProjectInfo>(this.db.GetSpecialProjectInfo), (SpecialProjectInfo o) => o.Name, this.evidence.SpecialProjectID);
			}
		}
		public string System2Name
		{
			get
			{
				return this.EvaluateDataObject<int, StarSystemInfo>(ref this.system2Name, new TurnEventMacros.MapDataObject<int, StarSystemInfo>(this.db.GetStarSystemInfo), (StarSystemInfo o) => o.Name, this.evidence.SystemID2);
			}
		}
		public string ShipName
		{
			get
			{
				return this.EvaluateDataObject<int, ShipInfo>(ref this.shipName, new TurnEventMacros.MapDataObject<int, ShipInfo>(this.GetShipInfo), (ShipInfo o) => o.ShipName, this.evidence.ShipID);
			}
		}
		public string ProvinceName
		{
			get
			{
				return this.EvaluateDataObject<int, ProvinceInfo>(ref this.provinceName, new TurnEventMacros.MapDataObject<int, ProvinceInfo>(this.GetProvinceInfo), (ProvinceInfo o) => o.Name, this.evidence.ProvinceID);
			}
		}
		public string PlagueType
		{
			get
			{
				return App.Localize(string.Format("@UI_PLAGUE_{0}", this.evidence.PlagueType.ToString()));
			}
		}
		public string ImperialPop
		{
			get
			{
				return this.evidence.ImperialPop.ToString("N" + 0);
			}
		}
		public string CivilianPop
		{
			get
			{
				return this.evidence.CivilianPop.ToString("N" + 0);
			}
		}
		public string Infrastructure
		{
			get
			{
				return this.evidence.Infrastructure.ToString();
			}
		}
		public string DesignAttribute
		{
			get
			{
				return App.Localize("@UI_" + this.evidence.DesignAttribute.ToString());
			}
		}
		public string ArrivalTurns
		{
			get
			{
				return this.evidence.ArrivalTurns.ToString();
			}
		}
		public string ShipNames
		{
			get
			{
				return this.evidence.NamesList;
			}
		}
		public string NumShips
		{
			get
			{
				return this.evidence.NumShips.ToString();
			}
		}
		public string Savings
		{
			get
			{
				return this.evidence.Savings.ToString("N");
			}
		}
		private string GetFleetName(FleetInfo f)
		{
			if (!string.IsNullOrEmpty(f.Name))
			{
				return f.Name;
			}
			return string.Format(App.Localize("@DEFAULT_FLEET_NAME"), this.db.GetPlayerInfo(f.PlayerID).Name);
		}
		private string GetColonyName(ColonyInfo o)
		{
			return this.db.GetOrbitalObjectInfo(o.OrbitalObjectID).Name;
		}
		private string GetTechName(PlayerTechInfo o)
		{
			return this.assetdb.GetLocalizedTechnologyName(o.TechFileID);
		}
		private string GetStationType(StationInfo o)
		{
			Faction faction = this.assetdb.GetFaction(this.db.GetPlayerFactionID(o.PlayerID));
			return this.assetdb.GetLocalizedStationTypeName(o.DesignInfo.StationType, faction.HasSlaves());
		}
		private string GetStationName(StationInfo o)
		{
			ShipInfo shipInfo = this.db.GetShipInfo(o.ShipID, false);
			string result = "";
			if (shipInfo != null)
			{
				result = shipInfo.ShipName;
			}
			return result;
		}
		private string GetStationLevel(StationInfo o)
		{
			return AssetDatabase.CommonStrings.Localize(this.assetdb.GetShipSectionAsset(o.DesignInfo.DesignSections[0].FilePath).Title);
		}
		private DesignInfo GetDesignInfo(int designId)
		{
			if (designId == 0)
			{
				return null;
			}
			return this.db.GetDesignInfo(designId);
		}
		private ShipInfo GetShipInfo(int shipId)
		{
			if (shipId == 0)
			{
				return null;
			}
			return this.db.GetShipInfo(shipId, false);
		}
		private ProvinceInfo GetProvinceInfo(int provinceId)
		{
			if (provinceId == 0)
			{
				return null;
			}
			return this.db.GetProvinceInfo(provinceId);
		}
		private string EvaluateDataObject<TKey, TDataObject>(ref string currentEvaluation, TurnEventMacros.MapDataObject<TKey, TDataObject> mapDataObject, TurnEventMacros.EvaluateDataObjectDelegate<TDataObject> evaluateDataObject, TKey id) where TDataObject : new()
		{
			if (currentEvaluation != null)
			{
				return currentEvaluation;
			}
			TDataObject tDataObject = mapDataObject(id);
			if (tDataObject == null)
			{
				currentEvaluation = string.Empty;
			}
			else
			{
				currentEvaluation = evaluateDataObject(tDataObject);
			}
			return currentEvaluation;
		}
		public TurnEventMacros(AssetDatabase assetdb, GameDatabase db, TurnEvent evidence)
		{
			this.assetdb = assetdb;
			this.db = db;
			this.evidence = evidence;
		}
	}
}
