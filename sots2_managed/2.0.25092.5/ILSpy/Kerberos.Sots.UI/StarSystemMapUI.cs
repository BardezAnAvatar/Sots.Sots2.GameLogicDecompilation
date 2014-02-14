using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.StarMapElements;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.UI
{
	public class StarSystemMapUI
	{
		private struct IconParams
		{
			public int ObjectID;
			public float X;
			public float Y;
			public float Scale;
			public Vector4 Color;
			public string Icon;
			public bool HasOrbit;
			public bool Clickable;
			public string Text;
			public static StarSystemMapUI.IconParams Default
			{
				get
				{
					return new StarSystemMapUI.IconParams
					{
						ObjectID = 0,
						X = 0f,
						Y = 0f,
						Scale = 1f,
						Color = Vector4.One,
						Icon = string.Empty,
						HasOrbit = false,
						Clickable = false,
						Text = string.Empty
					};
				}
			}
			public void SetPos(App game, IEnumerable<OrbitalObjectInfo> orbitalObjectInfos, float time, int objectId)
			{
				Matrix matrix = GameDatabase.CalcTransform(objectId, time, orbitalObjectInfos);
				this.X = matrix.Position.X;
				this.Y = matrix.Position.Z;
				this.HasOrbit = orbitalObjectInfos.First((OrbitalObjectInfo o) => o.ID == objectId).ParentID.HasValue;
			}
		}
		public const string SystemMapSpritePlanetMoon = "sysmap_planetmoon";
		public const string SystemMapSpritePlanet = "sysmap_planet";
		public const string SystemMapSpriteGasGiantPlanet = "sysmap_gasgiantplanet";
		public const string SystemMapSpriteGasGiantMoon = "sysmap_gasgiantmoon";
		public const string SystemMapSpriteGasGiantRings = "sysmap_gasgiantrings";
		public const string SystemMapSpriteGasGiant = "sysmap_gasgiant";
		internal static string SelectIcon(PlanetInfo planetInfo, IEnumerable<OrbitalObjectInfo> orbitalObjectInfos, IEnumerable<PlanetInfo> planetInfos)
		{
			if (StarSystemMapUI.IsTerrestrialPlanet(planetInfo))
			{
				if (StarSystemMapUI.HasMoons(planetInfo, orbitalObjectInfos, planetInfos))
				{
					return "sysmap_planetmoon";
				}
				return "sysmap_planet";
			}
			else
			{
				if (!StarSystemMapUI.IsGasGiant(planetInfo))
				{
					return string.Empty;
				}
				if (StarSystemMapUI.HasTerrestrialMoons(planetInfo, orbitalObjectInfos, planetInfos))
				{
					return "sysmap_gasgiantplanet";
				}
				if (StarSystemMapUI.HasMoons(planetInfo, orbitalObjectInfos, planetInfos))
				{
					return "sysmap_gasgiantmoon";
				}
				if (StarSystemMapUI.HasRing(planetInfo))
				{
					return "sysmap_gasgiantrings";
				}
				return "sysmap_gasgiant";
			}
		}
		internal static void Sync(App game, int systemId, string mapPanelId, bool isClickable)
		{
			bool flag = StarMap.IsInRange(game.Game.GameDatabase, game.LocalPlayer.ID, game.GameDatabase.GetStarSystemInfo(systemId), null);
			StarSystemMapUI.ResetMap(game, mapPanelId);
			if (systemId == 0)
			{
				return;
			}
			float time = 0f;
			GameDatabase gameDatabase = game.GameDatabase;
			IEnumerable<OrbitalObjectInfo> starSystemOrbitalObjectInfos = gameDatabase.GetStarSystemOrbitalObjectInfos(systemId);
			StarSystemInfo starSystemInfo = gameDatabase.GetStarSystemInfo(systemId);
			StellarClass stellarClass = StellarClass.Parse(starSystemInfo.StellarClass);
			if (starSystemInfo.IsDeepSpace)
			{
				return;
			}
			float num = StarHelper.CalcRadius(StellarSize.Ia);
			float num2 = StarHelper.CalcRadius(StellarSize.VII);
			float num3 = StarHelper.CalcRadius(stellarClass.Size);
			float t = (num3 - num2) / (num - num2);
			float scale = ScalarExtensions.Lerp(0.67f, 3f, t);
			StarSystemMapUI.IconParams @default = StarSystemMapUI.IconParams.Default;
			@default.ObjectID = StarSystemDetailsUI.StarItemID;
			@default.Text = starSystemInfo.Name;
			@default.Icon = "sysmap_star";
			@default.X = 0f;
			@default.Y = 0f;
			@default.Scale = scale;
			@default.Color = StarHelper.CalcIconColor(stellarClass);
			@default.Clickable = isClickable;
			StarSystemMapUI.AddMapIcon(game, mapPanelId, @default);
			IEnumerable<AsteroidBeltInfo> starSystemAsteroidBeltInfos = gameDatabase.GetStarSystemAsteroidBeltInfos(systemId);
			foreach (AsteroidBeltInfo asteroidBelt in starSystemAsteroidBeltInfos)
			{
				OrbitalObjectInfo orbitalObjectInfo = starSystemOrbitalObjectInfos.First((OrbitalObjectInfo x) => x.ID == asteroidBelt.ID);
				StarSystemMapUI.IconParams iconParams = default(StarSystemMapUI.IconParams);
				iconParams.SetPos(game, starSystemOrbitalObjectInfos, time, orbitalObjectInfo.ID);
				iconParams.ObjectID = orbitalObjectInfo.ID;
				iconParams.Icon = "sysmap_roiddust";
				iconParams.Scale = 0.85f;
				iconParams.Color = Vector4.One;
				iconParams.Text = orbitalObjectInfo.Name;
				iconParams.Clickable = false;
				StarSystemMapUI.AddMapIcon(game, mapPanelId, iconParams);
			}
			PlanetInfo[] starSystemPlanetInfos = gameDatabase.GetStarSystemPlanetInfos(systemId);
			foreach (OrbitalObjectInfo orbital in 
				from x in starSystemOrbitalObjectInfos
				where !x.ParentID.HasValue
				select x)
			{
				PlanetInfo planetInfo = starSystemPlanetInfos.FirstOrDefault((PlanetInfo x) => x.ID == orbital.ID);
				if (planetInfo != null)
				{
					string text = StarSystemMapUI.SelectIcon(planetInfo, starSystemOrbitalObjectInfos, starSystemPlanetInfos);
					if (string.IsNullOrEmpty(text))
					{
						App.Log.Trace(string.Format("Planet {0} does not have an icon to represent it in the mini system map.", orbital.Name), "gui");
					}
					else
					{
						AIColonyIntel colonyIntelForPlanet = game.GameDatabase.GetColonyIntelForPlanet(game.LocalPlayer.ID, planetInfo.ID);
						if (colonyIntelForPlanet != null && flag)
						{
							Vector3 primaryColor = game.GameDatabase.GetPlayerInfo(colonyIntelForPlanet.OwningPlayerID).PrimaryColor;
							Vector4 color = new Vector4(primaryColor.X, primaryColor.Y, primaryColor.Z, 1f);
							StarSystemMapUI.IconParams default2 = StarSystemMapUI.IconParams.Default;
							default2.SetPos(game, starSystemOrbitalObjectInfos, time, planetInfo.ID);
							default2.ObjectID = 0;
							default2.Icon = "sysmap_ownerring";
							default2.Scale = 0.85f;
							default2.Color = color;
							default2.Text = string.Empty;
							default2.Clickable = false;
							StarSystemMapUI.AddMapIcon(game, mapPanelId, default2);
						}
						StarSystemMapUI.IconParams iconParams2 = default(StarSystemMapUI.IconParams);
						iconParams2.SetPos(game, starSystemOrbitalObjectInfos, time, planetInfo.ID);
						iconParams2.ObjectID = planetInfo.ID;
						iconParams2.Icon = text;
						iconParams2.Scale = 0.85f;
						iconParams2.Color = Vector4.One;
						iconParams2.Text = orbital.Name;
						iconParams2.Clickable = isClickable;
						StarSystemMapUI.AddMapIcon(game, mapPanelId, iconParams2);
					}
				}
			}
		}
		private static bool HasRing(PlanetInfo planetInfo)
		{
			return planetInfo.RingID.HasValue;
		}
		private static bool HasMoons(PlanetInfo planetInfo, IEnumerable<OrbitalObjectInfo> orbitalObjectInfos, IEnumerable<PlanetInfo> planetInfos)
		{
			IEnumerable<OrbitalObjectInfo> enumerable = 
				from x in orbitalObjectInfos
				where x.ParentID.HasValue && x.ParentID.Value == planetInfo.ID
				select x;
			foreach (OrbitalObjectInfo moon in enumerable)
			{
				if (planetInfos.Any((PlanetInfo x) => x.ID == moon.ID))
				{
					return true;
				}
			}
			return false;
		}
		private static bool HasTerrestrialMoons(PlanetInfo planetInfo, IEnumerable<OrbitalObjectInfo> orbitalObjectInfos, IEnumerable<PlanetInfo> planetInfos)
		{
			IEnumerable<OrbitalObjectInfo> enumerable = 
				from x in orbitalObjectInfos
				where x.ParentID.HasValue && x.ParentID.Value == planetInfo.ID
				select x;
			foreach (OrbitalObjectInfo moon in enumerable)
			{
				PlanetInfo planetInfo2 = planetInfos.FirstOrDefault((PlanetInfo x) => x.ID == moon.ID);
				if (planetInfo2 != null && StarSystemMapUI.IsTerrestrialPlanet(planetInfo2))
				{
					return true;
				}
			}
			return false;
		}
		private static bool IsGasGiant(PlanetInfo planetInfo)
		{
			return planetInfo.Type.ToLowerInvariant() == StellarBodyTypes.Gaseous;
		}
		private static bool IsTerrestrialPlanet(PlanetInfo planetInfo)
		{
			return StellarBodyTypes.IsTerrestrial(planetInfo.Type.ToLowerInvariant());
		}
		private static void ResetMap(App game, string mapPanelId)
		{
			float parentRadius = StarHelper.CalcRadius(StellarSize.Ia);
			float num = Orbit.CalcOrbitRadius(parentRadius, StarSystemVars.Instance.StarOrbitStep, 12);
			float num2 = num * 1.1f;
			game.UI.Send(new object[]
			{
				"ResetMap",
				mapPanelId,
				num2
			});
		}
		private static void AddMapIcon(App game, string mapPanelId, StarSystemMapUI.IconParams iconParams)
		{
			game.UI.Send(new object[]
			{
				"AddMapIcon",
				mapPanelId,
				iconParams.ObjectID,
				iconParams.X,
				iconParams.Y,
				iconParams.Scale,
				iconParams.Color.X,
				iconParams.Color.Y,
				iconParams.Color.Z,
				iconParams.Icon,
				iconParams.Text,
				iconParams.HasOrbit,
				iconParams.Clickable
			});
		}
	}
}
