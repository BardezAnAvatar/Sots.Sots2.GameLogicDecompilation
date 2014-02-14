using Kerberos.Sots.Data;
using Kerberos.Sots.Data.StarMapFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.GameStates
{
	[GameObjectType(InteropGameObjectType.IGOT_STELLARBODY)]
	internal class StellarBody : GameObject, IActive, IDisposable
	{
		public class HeightGenParams
		{
			public float Bumpiness = 1f;
		}
		public class HGBlendParams : StellarBody.HeightGenParams
		{
			public string Layer1Texture = string.Empty;
			public string Layer2Texture = string.Empty;
			public float Layer2Amount;
		}
		public class HGPlaneCutsParams : StellarBody.HeightGenParams
		{
			public float BaseHeight = 0.5f;
			public int Iterations = 5000;
			public float Shift = 1000f;
		}
		public struct PlanetCivilianData
		{
			public string Faction;
			public double Population;
		}
		public struct Params
		{
			public int ColonyPlayerID;
			public int OrbitalID;
			public string IconSpriteName;
			public string SurfaceMaterial;
			public Vector3 Position;
			public float Radius;
			public float AtmoThickness;
			public Vector3 AtmoScatterWaveLengths;
			public float AtmoKm;
			public float AtmoKr;
			public float AtmoScaleDepth;
			public bool UseHeightMap;
			public bool IsInCombat;
			public int RandomSeed;
			public int TextureSize;
			public string HeightGradient1Texture;
			public string HeightGradient2Texture;
			public string HeightGradient3Texture;
			public float HeightGradient2Blend;
			public float HeightGradient3Blend;
			public string CityEmissiveTexture;
			public float MinCityAltitude;
			public float MaxCityAltitude;
			public float CitySprawl;
			public string CloudDiffuseTexture;
			public string CloudSpecularTexture;
			public float CloudOpacity;
			public Vector3 CloudDiffuseColor;
			public Vector3 CloudSpecularColor;
			public float WaterLevel;
			public Vector3 WaterSpecularColor;
			public float MaxWaterDepth;
			public float MaxLandHeight;
			public StellarBody.HeightGenParams HeightGen;
			public StellarBody.PlanetCivilianData[] Civilians;
			public double ImperialPopulation;
			public float Infrastructure;
			public float Suitability;
			public string BodyType;
			public string BodyName;
			public Kerberos.Sots.Strategy.InhabitedPlanet.ColonyStage ColonyStage;
			public SystemColonyType ColonyType;
			public static readonly StellarBody.Params Default;
			static Params()
			{
				StellarBody.Params.Default = new StellarBody.Params
				{
					ColonyPlayerID = 0,
					OrbitalID = 0,
					IconSpriteName = "sysmap_planet",
					SurfaceMaterial = "planet_earth2",
					Position = Vector3.Zero,
					Radius = 5000f,
					AtmoThickness = 0.06f,
					AtmoScatterWaveLengths = new Vector3(0.65f, 0.57f, 0.475f),
					AtmoKm = 0.0025f,
					AtmoKr = 0.001f,
					AtmoScaleDepth = 0.05f,
					UseHeightMap = false,
					IsInCombat = true,
					RandomSeed = 0,
					TextureSize = 512,
					HeightGradient1Texture = string.Empty,
					HeightGradient2Texture = string.Empty,
					HeightGradient3Texture = string.Empty,
					HeightGradient2Blend = 0f,
					HeightGradient3Blend = 0f,
					CityEmissiveTexture = string.Empty,
					MinCityAltitude = 0f,
					MaxCityAltitude = 1f,
					CitySprawl = 1f,
					CloudDiffuseTexture = "props\\textures\\Earth_Clouds_Diffuse.tga",
					CloudSpecularTexture = "props\\textures\\Earth_Clouds_Specular.tga",
					CloudOpacity = 1f,
					CloudDiffuseColor = Vector3.One,
					CloudSpecularColor = Vector3.One,
					WaterLevel = 0.5f,
					WaterSpecularColor = Vector3.One,
					MaxWaterDepth = 0.5f,
					MaxLandHeight = 0.5f,
					HeightGen = null,
					Civilians = new StellarBody.PlanetCivilianData[0],
					ImperialPopulation = 0.0,
					Infrastructure = 0f,
					Suitability = 0f,
					BodyType = "normal",
					BodyName = "unnamed_body",
					ColonyStage = Kerberos.Sots.Strategy.InhabitedPlanet.ColonyStage.Open,
					ColonyType = SystemColonyType.Normal
				};
			}
		}
		private double _totalPopulation;
		private IGameObject _lastAttackingObject;
		private List<PlanetWeaponBank> _weaponBanks = new List<PlanetWeaponBank>();
		private PlanetInfo _planetInfo;
		private bool _active;
		private StellarBody.Params _params;
		public double Population
		{
			get
			{
				return this._totalPopulation;
			}
			set
			{
				this._totalPopulation = value;
			}
		}
		public IGameObject LastAttackingObject
		{
			get
			{
				return this._lastAttackingObject;
			}
		}
		public List<PlanetWeaponBank> WeaponBanks
		{
			get
			{
				return this._weaponBanks;
			}
			set
			{
				this._weaponBanks = value;
			}
		}
		public PlanetInfo PlanetInfo
		{
			get
			{
				return this._planetInfo;
			}
			set
			{
				this._planetInfo = value;
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
		public StellarBody.Params Parameters
		{
			get
			{
				return this._params;
			}
			set
			{
				this._params = value;
			}
		}
		public static StellarBody Create(App game, StellarBody.Params p)
		{
			Player player = game.GetPlayer(p.ColonyPlayerID);
			List<object> list = new List<object>();
			list.AddRange(new object[]
			{
				(player != null) ? player.ObjectID : 0,
				p.OrbitalID,
				p.IconSpriteName,
				p.SurfaceMaterial ?? "planet_earth2",
				p.Position,
				p.Radius,
				p.IsInCombat,
				game.AssetDatabase.DefaultPlanetSensorRange,
				p.AtmoThickness,
				p.AtmoScatterWaveLengths,
				p.AtmoKm,
				p.AtmoKr,
				p.AtmoScaleDepth,
				p.UseHeightMap,
				p.RandomSeed,
				p.TextureSize,
				p.HeightGradient1Texture,
				p.HeightGradient2Texture,
				p.HeightGradient3Texture,
				p.HeightGradient2Blend,
				p.HeightGradient3Blend,
				p.CityEmissiveTexture,
				p.MinCityAltitude,
				p.MaxCityAltitude,
				p.CitySprawl,
				(p.CloudOpacity != 0f) ? p.CloudDiffuseTexture : string.Empty,
				(p.CloudOpacity != 0f) ? p.CloudSpecularTexture : string.Empty,
				p.CloudOpacity,
				p.CloudDiffuseColor,
				p.CloudSpecularColor,
				p.WaterLevel,
				p.WaterSpecularColor,
				p.MaxWaterDepth,
				p.MaxLandHeight,
				p.BodyType,
				p.BodyName ?? string.Empty,
				(int)p.ColonyStage,
				(int)p.ColonyType
			});
			if (p.HeightGen != null)
			{
				if (p.HeightGen is StellarBody.HGBlendParams)
				{
					StellarBody.HGBlendParams hGBlendParams = p.HeightGen as StellarBody.HGBlendParams;
					list.AddRange(new object[]
					{
						"blend",
						hGBlendParams.Bumpiness,
						hGBlendParams.Layer1Texture,
						hGBlendParams.Layer2Texture,
						hGBlendParams.Layer2Amount
					});
				}
				else
				{
					if (p.HeightGen is StellarBody.HGPlaneCutsParams)
					{
						StellarBody.HGPlaneCutsParams hGPlaneCutsParams = p.HeightGen as StellarBody.HGPlaneCutsParams;
						list.AddRange(new object[]
						{
							"planecuts",
							hGPlaneCutsParams.Bumpiness,
							hGPlaneCutsParams.BaseHeight,
							hGPlaneCutsParams.Iterations,
							hGPlaneCutsParams.Shift
						});
					}
					else
					{
						list.Add("none");
					}
				}
			}
			else
			{
				list.Add("none");
			}
			list.Add(p.Civilians.Length);
			StellarBody.PlanetCivilianData[] civilians = p.Civilians;
			for (int i = 0; i < civilians.Length; i++)
			{
				StellarBody.PlanetCivilianData planetCivilianData = civilians[i];
				list.Add(planetCivilianData.Faction);
				list.Add(planetCivilianData.Population);
			}
			list.Add(p.ImperialPopulation);
			list.Add(p.Infrastructure);
			list.Add(p.Suitability);
			list.Add(Constants.MinSuitability);
			list.Add(Constants.MaxSuitability);
			StellarBody stellarBody = game.AddObject<StellarBody>(list.ToArray());
			stellarBody.Parameters = p;
			return stellarBody;
		}
		public override bool OnEngineMessage(InteropMessageID messageId, ScriptMessageReader message)
		{
			if (messageId == InteropMessageID.IMID_SCRIPT_OBJECT_SETPROP)
			{
				string a = message.ReadString();
				if (a == "PlanetPopUpdate")
				{
					this.Population = message.ReadDouble();
					this._lastAttackingObject = base.App.GetGameObject(message.ReadInteger());
					return true;
				}
			}
			else
			{
				App.Log.Warn("Unhandled message (id=" + messageId + ").", "game");
			}
			return false;
		}
		public void Dispose()
		{
			this._lastAttackingObject = null;
			foreach (PlanetWeaponBank current in this._weaponBanks)
			{
				base.App.ReleaseObject(current);
			}
			this._weaponBanks.Clear();
			base.App.ReleaseObject(this);
		}
	}
}
