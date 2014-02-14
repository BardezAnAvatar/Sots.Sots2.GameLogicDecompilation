using Kerberos.Sots.Data;
using Kerberos.Sots.Data.Xml;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using Kerberos.Sots.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
namespace Kerberos.Sots.GameStates
{
	internal class PlanetGraphicsRules
	{
		private XmlElement _root;
		public PlanetGraphicsRules()
		{
			this.Reload();
		}
		public void Reload()
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(ScriptHost.FileSystem, "commonassets.xml");
			this._root = xmlDocument["CommonAssets"]["planets"];
		}
		private XmlElement GetRandomPlanetType(string name, int? typeVariant, int orbitalId)
		{
			IEnumerable<XmlElement> enumerable = 
				from x in this._root.ChildNodes.OfType<XmlElement>()
				where x.Name == "type" && x.GetAttribute("name") == name.ToLowerInvariant()
				select x;
			if (!typeVariant.HasValue)
			{
				Random random = new Random(orbitalId);
				return random.Choose(enumerable);
			}
			XmlElement[] array = enumerable.ToArray<XmlElement>();
			return array[typeVariant.Value % array.Length];
		}
		private XmlElement[] GetHazardPointBrackets(string type, int? typeVariant, float hazard, int orbitalId)
		{
			XmlElement randomPlanetType = this.GetRandomPlanetType(type, typeVariant, orbitalId);
			XmlElement[] array = (
				from x in randomPlanetType.ChildNodes.OfType<XmlElement>()
				orderby float.Parse(x.GetAttribute("value"))
				select x).ToArray<XmlElement>();
			if (array.Length == 0)
			{
				return null;
			}
			if (array.Length == 1)
			{
				return new XmlElement[]
				{
					array[0],
					array[0]
				};
			}
			float num = float.Parse(array[0].GetAttribute("value"));
			if (num >= hazard)
			{
				return new XmlElement[]
				{
					array[0],
					array[0]
				};
			}
			for (int i = 0; i < array.Length - 1; i++)
			{
				float num2 = num;
				num = float.Parse(array[i + 1].GetAttribute("value"));
				if (num2 <= hazard && num >= hazard)
				{
					return new XmlElement[]
					{
						array[i],
						array[i + 1]
					};
				}
			}
			return new XmlElement[]
			{
				array[array.Length - 1],
				array[array.Length - 1]
			};
		}
		private XmlElement GetAtmo(string name)
		{
			return this._root.ChildNodes.OfType<XmlElement>().FirstOrDefault((XmlElement x) => x.Name == "atmo" && x.GetAttribute("name") == name);
		}
		private XmlElement GetHeightGen(string name)
		{
			return this._root.ChildNodes.OfType<XmlElement>().FirstOrDefault((XmlElement x) => x.Name == "heightgen" && x.GetAttribute("name") == name);
		}
		private XmlElement GetFaction(string name)
		{
			return this._root.ChildNodes.OfType<XmlElement>().FirstOrDefault((XmlElement x) => x.Name == "faction" && x.GetAttribute("name") == name);
		}
		public StellarBody.Params GetStellarBodyParams(GameSession game, int orbitalId)
		{
			OrbitalObjectInfo orbitalObjectInfo = game.GameDatabase.GetOrbitalObjectInfo(orbitalId);
			ColonyInfo colonyInfoForPlanet = game.GameDatabase.GetColonyInfoForPlanet(orbitalId);
			PlanetInfo planetInfo = game.GameDatabase.GetPlanetInfo(orbitalId);
			SystemColonyType colonyType = SystemColonyType.Normal;
			PlayerInfo povPlayerInfo;
			int num;
			double population;
			if (colonyInfoForPlanet != null)
			{
				num = colonyInfoForPlanet.PlayerID;
				population = colonyInfoForPlanet.ImperialPop;
				povPlayerInfo = game.GameDatabase.GetPlayerInfo(num);
				if (orbitalObjectInfo != null && povPlayerInfo != null)
				{
					HomeworldInfo homeworldInfo = game.GameDatabase.GetHomeworlds().FirstOrDefault((HomeworldInfo x) => x.SystemID == orbitalObjectInfo.StarSystemID);
					if (homeworldInfo != null && homeworldInfo.SystemID != 0 && homeworldInfo.PlayerID == povPlayerInfo.ID)
					{
						colonyType = SystemColonyType.Home;
					}
					else
					{
						if (game.GameDatabase.GetProvinceInfos().Any((ProvinceInfo x) => x.CapitalSystemID == orbitalObjectInfo.StarSystemID && x.PlayerID == povPlayerInfo.ID && x.CapitalSystemID != povPlayerInfo.Homeworld))
						{
							colonyType = SystemColonyType.Capital;
						}
					}
				}
			}
			else
			{
				num = 0;
				population = 0.0;
				povPlayerInfo = game.GameDatabase.GetPlayerInfo(game.LocalPlayer.ID);
			}
			FactionInfo factionInfo = game.GameDatabase.GetFactionInfo(povPlayerInfo.FactionID);
			float num2 = Math.Abs(planetInfo.Suitability - factionInfo.IdealSuitability);
			float maxHazard = (float)game.App.GetStratModifier<int>(StratModifiers.MaxColonizableHazard, povPlayerInfo.ID);
			float radius = StarSystemVars.Instance.SizeToRadius(planetInfo.Size);
			Matrix transform = orbitalObjectInfo.OrbitalPath.GetTransform(0.0);
			string iconSpriteName = StarSystemMapUI.SelectIcon(planetInfo, game.GameDatabase.GetStarSystemOrbitalObjectInfos(orbitalObjectInfo.StarSystemID), game.GameDatabase.GetStarSystemPlanetInfos(orbitalObjectInfo.StarSystemID));
			return this.GetStellarBodyParams(iconSpriteName, transform.Position, radius, orbitalId, num, planetInfo.Type, num2, maxHazard, factionInfo.Name, (float)planetInfo.Biosphere, population, null, Colony.GetColonyStage(game.GameDatabase, num, (double)num2), colonyType);
		}
		public string[] GetFactions()
		{
			string[] result;
			try
			{
				IEnumerable<XmlElement> source = 
					from x in this._root.ChildNodes.OfType<XmlElement>()
					where x.Name == "faction"
					select x;
				result = (
					from x in source
					select x.GetAttribute("name")).ToArray<string>();
			}
			catch (Exception ex)
			{
				App.Log.Warn(string.Format("Parse error resolving planet display rules:\n" + ex.ToString(), new object[0]), "data");
				result = new string[0];
			}
			return result;
		}
		public string[] GetStellarBodyTypes()
		{
			string[] result;
			try
			{
				IEnumerable<XmlElement> source = 
					from x in this._root.ChildNodes.OfType<XmlElement>()
					where x.Name == "type"
					select x;
				result = (
					from x in source
					select x.GetAttribute("name")).Distinct<string>().ToArray<string>();
			}
			catch (Exception ex)
			{
				App.Log.Warn(string.Format("Parse error resolving planet display rules:\n" + ex.ToString(), new object[0]), "data");
				result = new string[0];
			}
			return result;
		}
		public StellarBody.Params GetStellarBodyParams(string iconSpriteName, Vector3 position, float radius, int orbitalId, int colonyPlayerId, string type, float hazard, float maxHazard, string faction, float biosphere, double population, int? typeVariant, Kerberos.Sots.Strategy.InhabitedPlanet.ColonyStage colonyStage, SystemColonyType colonyType)
		{
			StellarBody.Params result;
			try
			{
				XmlElement[] hazardPointBrackets = this.GetHazardPointBrackets(type, typeVariant, hazard, orbitalId);
				float num = float.Parse(hazardPointBrackets[1].GetAttribute("value"));
				float num2 = float.Parse(hazardPointBrackets[0].GetAttribute("value"));
				XmlElement atmo = this.GetAtmo(hazardPointBrackets[0].GetAttribute("atmo"));
				XmlElement atmo2 = this.GetAtmo(hazardPointBrackets[1].GetAttribute("atmo"));
				XmlElement heightGen = this.GetHeightGen(hazardPointBrackets[0].GetAttribute("heightgen"));
				XmlElement heightGen2 = this.GetHeightGen(hazardPointBrackets[1].GetAttribute("heightgen"));
				XmlElement faction2 = this.GetFaction(faction);
				Vector2 vector = (faction2 != null) ? Vector2.Parse(faction2.GetAttribute("cityaltrange")) : Vector2.Zero;
				float num3 = 0f;
				if (num - num2 >= 1.401298E-45f)
				{
					num3 = ((hazard - num2) / (num - num2)).Saturate();
				}
				StellarBody.Params @params = default(StellarBody.Params);
				@params.Civilians = new StellarBody.PlanetCivilianData[0];
				@params.IconSpriteName = iconSpriteName;
				@params.ColonyPlayerID = colonyPlayerId;
				@params.AtmoThickness = ScalarExtensions.Lerp(float.Parse(atmo.GetAttribute("thick")), float.Parse(atmo2.GetAttribute("thick")), num3);
				@params.AtmoKm = ScalarExtensions.Lerp(float.Parse(atmo.GetAttribute("km")), float.Parse(atmo2.GetAttribute("km")), num3);
				@params.AtmoKr = ScalarExtensions.Lerp(float.Parse(atmo.GetAttribute("kr")), float.Parse(atmo2.GetAttribute("kr")), num3);
				@params.AtmoScatterWaveLengths = Vector3.Lerp(Vector3.Parse(atmo.GetAttribute("filter")), Vector3.Parse(atmo2.GetAttribute("filter")), num3);
				@params.AtmoScaleDepth = ScalarExtensions.Lerp(float.Parse(atmo.GetAttribute("scaledepth")), float.Parse(atmo2.GetAttribute("scaledepth")), num3);
				@params.CloudDiffuseTexture = atmo.GetAttribute("clouddiffuse");
				@params.CloudSpecularTexture = atmo.GetAttribute("cloudspecular");
				@params.CloudOpacity = ScalarExtensions.Lerp(float.Parse(atmo.GetAttribute("cloudcover")), float.Parse(atmo2.GetAttribute("cloudcover")), num3);
				@params.CloudDiffuseColor = Vector3.Lerp(Vector3.Parse(atmo.GetAttribute("clouddiffusecolor")), Vector3.Parse(atmo2.GetAttribute("clouddiffusecolor")), num3);
				@params.CloudSpecularColor = Vector3.Lerp(Vector3.Parse(atmo.GetAttribute("cloudspecularcolor")), Vector3.Parse(atmo2.GetAttribute("cloudspecularcolor")), num3);
				@params.CityEmissiveTexture = faction2.GetAttribute("cityemissive");
				@params.CitySprawl = (float)(population / 1000000000.0);
				@params.HeightGradient1Texture = hazardPointBrackets[0].GetAttribute("heightgrad");
				@params.HeightGradient2Texture = hazardPointBrackets[1].GetAttribute("heightgrad");
				@params.HeightGradient2Blend = num3;
				@params.HeightGradient3Texture = faction2.GetAttribute("idealgrad");
				@params.HeightGradient3Blend = 1f - (hazard / maxHazard).Saturate();
				@params.MaxCityAltitude = vector.Y;
				@params.MinCityAltitude = vector.X;
				@params.WaterSpecularColor = Vector3.Lerp(Vector3.Parse(hazardPointBrackets[0].GetAttribute("waterspec")), Vector3.Parse(hazardPointBrackets[1].GetAttribute("waterspec")), num3);
				@params.MaxLandHeight = ScalarExtensions.Lerp(float.Parse(hazardPointBrackets[0].GetAttribute("maxlandalt")), float.Parse(hazardPointBrackets[1].GetAttribute("maxlandalt")), num3);
				@params.MaxWaterDepth = ScalarExtensions.Lerp(float.Parse(hazardPointBrackets[0].GetAttribute("maxseadepth")), float.Parse(hazardPointBrackets[1].GetAttribute("maxseadepth")), num3);
				@params.WaterLevel = ScalarExtensions.Lerp(float.Parse(hazardPointBrackets[0].GetAttribute("sealevel")), float.Parse(hazardPointBrackets[1].GetAttribute("sealevel")), num3);
				float bumpiness = ScalarExtensions.Lerp(float.Parse(hazardPointBrackets[0].GetAttribute("bumpiness")), float.Parse(hazardPointBrackets[1].GetAttribute("bumpiness")), num3);
				@params.Position = position;
				@params.Radius = radius;
				@params.RandomSeed = orbitalId;
				@params.UseHeightMap = true;
				@params.TextureSize = 512;
				@params.BodyType = type;
				@params.ColonyStage = colonyStage;
				@params.ColonyType = colonyType;
				string attribute = heightGen.GetAttribute("type");
				string attribute2 = heightGen2.GetAttribute("type");
				if (attribute == "planecuts")
				{
					StellarBody.HGPlaneCutsParams hGPlaneCutsParams = new StellarBody.HGPlaneCutsParams();
					hGPlaneCutsParams.Bumpiness = bumpiness;
					if (attribute2 == attribute)
					{
						hGPlaneCutsParams.BaseHeight = ScalarExtensions.Lerp(float.Parse(heightGen.GetAttribute("baseheight")), float.Parse(heightGen2.GetAttribute("baseheight")), num3);
						hGPlaneCutsParams.Iterations = (int)ScalarExtensions.Lerp((float)int.Parse(heightGen.GetAttribute("iterations")), (float)int.Parse(heightGen2.GetAttribute("iterations")), num3);
						hGPlaneCutsParams.Shift = ScalarExtensions.Lerp(float.Parse(heightGen.GetAttribute("shift")), float.Parse(heightGen2.GetAttribute("shift")), num3);
					}
					else
					{
						hGPlaneCutsParams.BaseHeight = float.Parse(heightGen.GetAttribute("baseheight"));
						hGPlaneCutsParams.Iterations = int.Parse(heightGen.GetAttribute("iterations"));
						hGPlaneCutsParams.Shift = float.Parse(heightGen.GetAttribute("shift"));
					}
					@params.HeightGen = hGPlaneCutsParams;
				}
				else
				{
					if (attribute == "blend")
					{
						StellarBody.HGBlendParams hGBlendParams = new StellarBody.HGBlendParams();
						hGBlendParams.Bumpiness = bumpiness;
						hGBlendParams.Layer1Texture = heightGen.GetAttribute("base");
						if (attribute2 == attribute)
						{
							hGBlendParams.Layer2Texture = heightGen2.GetAttribute("base");
							hGBlendParams.Layer2Amount = num3;
						}
						else
						{
							hGBlendParams.Layer2Texture = hGBlendParams.Layer1Texture;
							hGBlendParams.Layer2Amount = 0f;
						}
						@params.HeightGen = hGBlendParams;
					}
				}
				result = @params;
			}
			catch (Exception ex)
			{
				App.Log.Warn(string.Format("Parse error resolving planet display rules (type={0}, will use defaults instead):\n" + ex.ToString(), type ?? "null"), "data");
				result = StellarBody.Params.Default;
			}
			return result;
		}
	}
}
