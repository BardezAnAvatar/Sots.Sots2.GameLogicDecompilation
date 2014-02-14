using Kerberos.Sots.Data.Xml;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace Kerberos.Sots.Combat
{
	internal static class CombatConfig
	{
		private class DataContext
		{
			public Matrix Origin;
			public int InputID;
			public CombatConfig.DataContext Clone()
			{
				return new CombatConfig.DataContext
				{
					Origin = this.Origin,
					InputID = this.InputID
				};
			}
			public void TransformPositionAndRotation(ref Vector3 pos, ref Vector3 rot)
			{
				Matrix lhs = Matrix.CreateRotationYPR(Vector3.DegreesToRadians(rot));
				lhs.Position = pos;
				lhs *= this.Origin;
				pos = lhs.Position;
				rot = Vector3.RadiansToDegrees(lhs.EulerAngles);
			}
			public void TransformPosition(ref Vector3 pos)
			{
				Vector3 zero = Vector3.Zero;
				this.TransformPositionAndRotation(ref pos, ref zero);
			}
		}
		public static bool ParentBattleRiders = true;
        private static IGameObject CreateShipCompound(App game, DataContext context, XmlElement node)
        {
            string innerText;
            IEnumerable<string> weapons;
            IEnumerable<string> modules;
            XmlElement element = node["ShipName"];
            XmlElement element2 = node["ShipDesign"];
            XmlElement source = element2["Weapons"];
            XmlElement element4 = element2["Modules"];
            XmlElement element1 = element2["WeaponAssignments"];
            XmlElement element9 = element2["ModuleAssignments"];
            XmlElement element5 = element2["Sections"];
            List<WeaponAssignment> list = new List<WeaponAssignment>();
            List<ModuleAssignment> list2 = new List<ModuleAssignment>();
            List<ShipSectionAsset> sectionAssets = new List<ShipSectionAsset>();
            new List<LogicalModule>();
            if (element5 != null)
            {
                List<XmlElement> list3 = (from x in element5.OfType<XmlElement>()
                    where x.Name.Equals("Section", StringComparison.InvariantCulture)
                    select x).ToList<XmlElement>();
                
                //LINQ confusion?
                //from x in list3 select x["SectionFile"].InnerText;
                
                foreach (XmlElement element6 in list3)
                {
                    string sectionFile = element6["SectionFile"].InnerText;
                    ShipSectionAsset item = game.AssetDatabase.ShipSections.FirstOrDefault<ShipSectionAsset>(x => x.FileName == sectionFile);
                    sectionAssets.Add(item);
                    List<LogicalModule> list4 = new List<LogicalModule>();
                    foreach (XmlElement element7 in from x in element6.OfType<XmlElement>()
                        where x.Name.Equals("Module", StringComparison.InvariantCulture)
                        select x)
                    {
                        string moduleNodeName = element7["Mount"].InnerText;
                        string modulePath = element7["ModuleId"].InnerText;
                        LogicalModule module = game.AssetDatabase.Modules.First<LogicalModule>(x => x.ModulePath == modulePath);
                        if (!list4.Contains(module))
                        {
                            list4.Add(module);
                        }
                        ModuleAssignment assignment = new ModuleAssignment {
                            ModuleMount = item.Modules.First<LogicalModuleMount>(x => x.NodeName == moduleNodeName),
                            Module = module,
                            PsionicAbilities = null
                        };
                        list2.Add(assignment);
                    }
                    //IEnumerable<LogicalBank> enumerable2 = item.Banks.Concat<LogicalBank>(from x in list4 select x.Banks);
                    IEnumerable<LogicalBank> enumerable2 = item.Banks.Concat<LogicalBank>(list4.SelectMany(x => x.Banks));
                    foreach (XmlElement element8 in from x in element6.OfType<XmlElement>()
                        where x.Name.Equals("Bank", StringComparison.InvariantCulture)
                        select x)
                    {
                        Guid bankGuid = Guid.Parse(element8["Id"].InnerText);
                        string weaponName = element8["Weapon"].InnerText;
                        WeaponAssignment assignment2 = new WeaponAssignment {
                            Bank = enumerable2.First<LogicalBank>(x => x.GUID == bankGuid),
                            Weapon = game.AssetDatabase.Weapons.First<LogicalWeapon>(x => x.WeaponName == weaponName)
                        };
                        list.Add(assignment2);
                    }
                }
            }
            if (source != null)
            {
                weapons = from x in source.OfType<XmlElement>()
                    where x.Name.Equals("string", StringComparison.InvariantCulture)
                    select x.InnerText;
            }
            else
            {
                weapons = new string[0];
            }
            if (element4 != null)
            {
                modules = from x in element4.OfType<XmlElement>()
                    where x.Name.Equals("string", StringComparison.InvariantCulture)
                    select x.InnerText;
            }
            else
            {
                modules = new string[0];
            }
            if (element != null)
            {
                innerText = element.InnerText;
            }
            else
            {
                innerText = "USS Placeholder";
            }
            int playerId = node["PlayerID"].ExtractIntegerOrDefault(0);
            Player player = game.GetPlayer(playerId);
            CreateShipParams createShipParams = new CreateShipParams {
                player = player,
                sections = sectionAssets,
                turretHousings = game.AssetDatabase.TurretHousings,
                weapons = game.AssetDatabase.Weapons,
                preferredWeapons = from x in game.AssetDatabase.Weapons
                    where weapons.Contains<string>(x.Name)
                    select x,
                assignedWeapons = list,
                modules = game.AssetDatabase.Modules,
                preferredModules = from x in game.AssetDatabase.Modules
                    where modules.Contains<string>(x.ModuleName)
                    select x,
                assignedModules = list2,
                psionics = game.AssetDatabase.Psionics,
                faction = game.AssetDatabase.Factions.First<Faction>(x => sectionAssets.First<ShipSectionAsset>().Faction == x.Name),
                shipName = innerText,
                inputID = context.InputID
            };
            Ship ship = Ship.CreateShip(game, createShipParams);
            Vector3 pos = node["Position"].ExtractVector3OrDefault(Vector3.Zero);
            Vector3 rot = node["Rotation"].ExtractVector3OrDefault(Vector3.Zero);
            context.TransformPositionAndRotation(ref pos, ref rot);
            ship.Position = pos;
            ship.Rotation = rot;
            return ship;
        }
		private static IGameObject CreateProceduralStellarBody(App game, CombatConfig.DataContext context, XmlElement node)
		{
			float radius = node["Radius"].ExtractSingleOrDefault(5000f);
			int orbitalId = node["RandomSeed"].ExtractIntegerOrDefault(0);
			string type = node["PlanetType"].ExtractStringOrDefault("normal");
			float hazard = node["HazardRating"].ExtractSingleOrDefault(0f);
			string faction = node["Faction"].ExtractStringOrDefault("human");
			float biosphere = node["Biosphere"].ExtractSingleOrDefault(0f);
			double population = node["Population"].ExtractDoubleOrDefault(0.0);
			Vector3 position = node["Position"].ExtractVector3OrDefault(Vector3.Zero);
			context.TransformPosition(ref position);
			int? typeVariant = null;
			if (node["Variant"] != null)
			{
				typeVariant = new int?(node["Variant"].ExtractIntegerOrDefault(0));
			}
			StellarBody.Params stellarBodyParams = game.AssetDatabase.PlanetGenerationRules.GetStellarBodyParams("sysmap_planet", position, radius, orbitalId, 0, type, hazard, 750f, faction, biosphere, population, typeVariant, ColonyStage.Open, SystemColonyType.Normal);
			stellarBodyParams.Civilians = new StellarBody.PlanetCivilianData[0];
			stellarBodyParams.ImperialPopulation = 0.0;
			stellarBodyParams.Suitability = 0f;
			stellarBodyParams.Infrastructure = 0f;
			return StellarBody.Create(game, stellarBodyParams);
		}
		private static IGameObject CreateLegacyStellarBody(App game, CombatConfig.DataContext context, XmlElement node)
		{
			StellarBody.Params @default = StellarBody.Params.Default;
			@default.SurfaceMaterial = node["Asset"].ExtractStringOrDefault(string.Empty);
			@default.Position = node["Position"].ExtractVector3OrDefault(Vector3.Zero);
			context.TransformPosition(ref @default.Position);
			@default.Radius = node["Scale"].ExtractSingleOrDefault(0f);
			@default.AtmoThickness = node["AtmosphereThickness"].ExtractSingleOrDefault(0f);
			@default.AtmoScatterWaveLengths = node["AtmosphereScatteringWavelengths"].ExtractVector3OrDefault(Vector3.Zero);
			@default.AtmoKm = node["AtmosphereMieConstant"].ExtractSingleOrDefault(0f);
			@default.AtmoKr = node["AtmosphereRayleighConstant"].ExtractSingleOrDefault(0f);
			@default.AtmoScaleDepth = node["AtmosphereScaleDepth"].ExtractSingleOrDefault(0f);
			@default.Civilians = new StellarBody.PlanetCivilianData[0];
			@default.ImperialPopulation = 0.0;
			@default.Suitability = 0f;
			@default.Infrastructure = 0f;
			return StellarBody.Create(game, @default);
		}
		private static IGameObject CreateProceduralStar(App game, CombatConfig.DataContext context, XmlElement node)
		{
			string str = node["StellarClass"].ExtractStringOrDefault("G2V");
			string name = node["Name"].ExtractStringOrDefault(string.Empty);
			Vector3 origin = node["Position"].ExtractVector3OrDefault(Vector3.Zero);
			context.TransformPosition(ref origin);
			return Kerberos.Sots.GameStates.StarSystem.CreateStar(game, origin, StellarClass.Parse(str), name, 1f, true);
		}
		private static IGameObject CreateLegacyStarModel(App game, CombatConfig.DataContext context, XmlElement node)
		{
			bool impostorEnabled = true;
			Vector3 position = node["Position"].ExtractVector3OrDefault(Vector3.Zero);
			context.TransformPosition(ref position);
			return new StarModel(game, node["Asset"].ExtractStringOrDefault(string.Empty), position, node["Scale"].ExtractSingleOrDefault(0f), true, node["ImpostorMaterial"].ExtractStringOrDefault(string.Empty), node["ImpostorSpriteScale"].ExtractVector2OrDefault(Vector2.One), node["ImpostorRange"].ExtractVector2OrDefault(Vector2.Zero), node["ImpostorVertexColor"].ExtractVector3OrDefault(Vector3.One), impostorEnabled, string.Empty);
		}
		private static IGameObject CreateDefaultGameObject(App game, CombatConfig.DataContext context, XmlElement node)
		{
			XmlElement xmlElement = node["Asset"];
			IGameObject gameObject = game.AddObject((InteropGameObjectType)Enum.Parse(typeof(InteropGameObjectType), node["Type"].InnerText), new string[]
			{
				(xmlElement != null) ? xmlElement.InnerText : string.Empty
			});
			Vector3 position = node["Position"].ExtractVector3OrDefault(Vector3.Zero);
			Vector3 rotation = node["Rotation"].ExtractVector3OrDefault(Vector3.Zero);
			context.TransformPositionAndRotation(ref position, ref rotation);
			if (gameObject is IPosition)
			{
				(gameObject as IPosition).Position = position;
			}
			if (gameObject is IScalable)
			{
				(gameObject as IScalable).Scale = node["Scale"].ExtractSingleOrDefault(1f);
			}
			if (gameObject is IOrientatable)
			{
				(gameObject as IOrientatable).Rotation = rotation;
			}
			return gameObject;
		}
		private static IGameObject CreateAsteroidBelt(App game, CombatConfig.DataContext context, XmlElement node)
		{
			int randomSeed = node["RandomSeed"].ExtractIntegerOrDefault(0);
			Vector3 center = node["Center"].ExtractVector3OrDefault(Vector3.Zero);
			context.TransformPosition(ref center);
			float innerRadius = node["InnerRadius"].ExtractSingleOrDefault(10000f);
			float outterRadius = node["OuterRadius"].ExtractSingleOrDefault(20000f);
			float minHeight = node["MinimumHeight"].ExtractSingleOrDefault(0f);
			float maxHeight = node["MaximumHeight"].ExtractSingleOrDefault(0f);
			int numAsteroids = node["InitialCount"].ExtractIntegerOrDefault(1000);
			return new AsteroidBelt(game, randomSeed, center, innerRadius, outterRadius, minHeight, maxHeight, numAsteroids);
		}
		private static void CreateGameObjectsCore(Dictionary<IGameObject, XmlElement> map, App game, CombatConfig.DataContext context, Vector3 shipStartPositionHack, XmlElement root)
		{
			foreach (XmlElement current in 
				from x in root.OfType<XmlElement>()
				where x.Name == "GameObject" || x.Name == "Group"
				select x)
			{
				if (current.Name == "Group")
				{
					Vector3 position = current["Position"].ExtractVector3OrDefault(Vector3.Zero);
					Vector3 vector = current["Rotation"].ExtractVector3OrDefault(Vector3.Zero);
					Matrix lhs = Matrix.CreateRotationYPR(MathHelper.DegreesToRadians(vector.X), MathHelper.DegreesToRadians(vector.Y), MathHelper.DegreesToRadians(vector.Z));
					lhs.Position = position;
					CombatConfig.DataContext dataContext = context.Clone();
					dataContext.Origin = lhs * context.Origin;
					CombatConfig.CreateGameObjectsCore(map, game, dataContext, shipStartPositionHack, current);
				}
				else
				{
					string innerText = current["Type"].InnerText;
					if (innerText == "AsteroidBelt")
					{
						map[CombatConfig.CreateAsteroidBelt(game, context, current)] = current;
					}
					else
					{
						if (innerText == "Star")
						{
							map[CombatConfig.CreateProceduralStar(game, context, current)] = current;
						}
						else
						{
							if (innerText == "StellarBody")
							{
								map[CombatConfig.CreateProceduralStellarBody(game, context, current)] = current;
							}
							else
							{
								if (innerText == "Ship")
								{
									CombatConfig.DataContext dataContext2 = context;
									if (shipStartPositionHack != Vector3.Zero)
									{
										dataContext2 = context.Clone();
										CombatConfig.DataContext expr_180_cp_0 = dataContext2;
										expr_180_cp_0.Origin.Position = expr_180_cp_0.Origin.Position - shipStartPositionHack;
									}
									map[CombatConfig.CreateShipCompound(game, dataContext2, current)] = current;
								}
								else
								{
									if (innerText == "LegacyStellarBody")
									{
										map[CombatConfig.CreateLegacyStellarBody(game, context, current)] = current;
									}
									else
									{
										if (innerText == "LegacyStar")
										{
											map[CombatConfig.CreateLegacyStarModel(game, context, current)] = current;
										}
										else
										{
											map[CombatConfig.CreateDefaultGameObject(game, context, current)] = current;
										}
									}
								}
							}
						}
					}
				}
			}
		}
		public static Dictionary<IGameObject, XmlElement> CreateGameObjects(App game, Vector3 origin, XmlDocument doc, int inputId)
		{
			Dictionary<IGameObject, XmlElement> dictionary = new Dictionary<IGameObject, XmlElement>();
			XmlElement xmlElement = doc["CombatConfig"];
			if (xmlElement == null)
			{
				return dictionary;
			}
			XmlElement xmlElement2 = xmlElement["GameObjects"];
			if (xmlElement2 == null)
			{
				return dictionary;
			}
			CombatConfig.DataContext dataContext = new CombatConfig.DataContext();
			dataContext.Origin = Matrix.Identity;
			dataContext.InputID = inputId;
			XmlElement xmlElement3 = xmlElement2["StartPoint"];
			Vector3 shipStartPositionHack = Vector3.Zero;
			if (xmlElement3 != null)
			{
				shipStartPositionHack = xmlElement3["Position"].ExtractVector3OrDefault(Vector3.Zero);
			}
			foreach (XmlElement current in 
				from x in xmlElement2.OfType<XmlElement>()
				where x.Name == "Player"
				select x)
			{
				int playerId = current["ID"].ExtractIntegerOrDefault(1);
				Player player = game.GetPlayer(playerId);
				if (current["EmpireColorIndex"] != null)
				{
					int empireColor = current["EmpireColorIndex"].ExtractIntegerOrDefault(0);
					player.SetEmpireColor(empireColor);
				}
				if (current["PlayerColor"] != null)
				{
					Vector3 playerColor = current["PlayerColor"].ExtractVector3OrDefault(Vector3.Zero);
					player.SetPlayerColor(playerColor);
				}
				if (current["Badge"] != null)
				{
					string badgeTexture = current["Badge"].ExtractStringOrDefault(string.Empty);
					player.SetBadgeTexture(badgeTexture);
				}
			}
			CombatConfig.CreateGameObjectsCore(dictionary, game, dataContext, shipStartPositionHack, xmlElement2);
			return dictionary;
		}
		public static void ChangeXmlElementPositionAndRotation(XmlElement gameObjectElement, Vector3 position, Vector3 rotation)
		{
			XmlElement xmlElement = gameObjectElement["Position"];
			XmlElement xmlElement2 = gameObjectElement["Rotation"];
			xmlElement.InnerText = position.ToString();
			xmlElement2.InnerText = rotation.ToString();
		}
        public static XmlElement ExportXmlElementFromShipParameters(App game, XmlDocument owner, IEnumerable<string> sectionFileNames, IEnumerable<WeaponAssignment> weaponAssignments, IEnumerable<ModuleAssignment> moduleAssignments, int playerID, Vector3 position, Vector3 rotation)
        {
            XmlElement element = owner.CreateElement("GameObject", null);
            XmlElement newChild = owner.CreateElement("Type", null);
            newChild.InnerText = "Ship";
            element.AppendChild(newChild);
            XmlElement element3 = owner.CreateElement("ShipName", null);
            element3.InnerText = sectionFileNames.Any<string>() ? Path.GetFileNameWithoutExtension(sectionFileNames.First<string>()) : "USS Placeholder";
            element.AppendChild(element3);
            XmlElement element4 = owner.CreateElement("PlayerID", null);
            element4.InnerText = playerID.ToString();
            element.AppendChild(element4);
            XmlElement element5 = owner.CreateElement("Position", null);
            element5.InnerText = position.ToString();
            element.AppendChild(element5);
            XmlElement element6 = owner.CreateElement("Rotation", null);
            element6.InnerText = rotation.ToString();
            element.AppendChild(element6);
            XmlElement element7 = owner.CreateElement("ShipDesign", null);
            XmlElement element8 = owner.CreateElement("Sections", null);
            using (IEnumerator<string> enumerator = sectionFileNames.GetEnumerator())
            {
                Func<ShipSectionAsset, bool> predicate = null;
                string sectionFileName;
                while (enumerator.MoveNext())
                {
                    sectionFileName = enumerator.Current;
                    Func<ModuleAssignment, bool> func = null;
                    if (predicate == null)
                    {
                        predicate = x => x.FileName == sectionFileName;
                    }
                    ShipSectionAsset section = game.AssetDatabase.ShipSections.First<ShipSectionAsset>(predicate);
                    XmlElement element9 = owner.CreateElement("Section", null);
                    XmlElement element10 = owner.CreateElement("SectionFile", null);
                    element10.InnerText = sectionFileName;
                    element9.AppendChild(element10);
                    if (func == null)
                    {
                        func = x => section.Modules.Contains<LogicalModuleMount>(x.ModuleMount);
                    }
                    List<ModuleAssignment> list = moduleAssignments.Where<ModuleAssignment>(func).ToList<ModuleAssignment>();
                    foreach (ModuleAssignment assignment in list)
                    {
                        XmlElement element11 = owner.CreateElement("Mount", null);
                        element11.InnerText = assignment.ModuleMount.NodeName;
                        XmlElement element12 = owner.CreateElement("ModuleId", null);
                        element12.InnerText = assignment.Module.ModulePath;
                        XmlElement element13 = owner.CreateElement("Module", null);
                        element13.AppendChild(element11);
                        element13.AppendChild(element12);
                        element9.AppendChild(element13);
                    }
                    
                    //List<LogicalBank> sectionBanks = section.Banks.Concat<LogicalBank>((from x in list select x.Module.Banks)).ToList<LogicalBank>();
                    List<LogicalBank> sectionBanks = section.Banks.Concat<LogicalBank>(list.SelectMany(x => x.Module.Banks)).ToList<LogicalBank>();
                    foreach (WeaponAssignment assignment2 in from x in weaponAssignments
                                                             where sectionBanks.Contains(x.Bank)
                                                             select x)
                    {
                        XmlElement element14 = owner.CreateElement("Id", null);
                        element14.InnerText = assignment2.Bank.GUID.ToString();
                        XmlElement element15 = owner.CreateElement("Weapon", null);
                        element15.InnerText = assignment2.Weapon.WeaponName;
                        XmlElement element16 = owner.CreateElement("Bank", null);
                        element16.AppendChild(element14);
                        element16.AppendChild(element15);
                        element9.AppendChild(element16);
                    }
                    element8.AppendChild(element9);
                }
            }
            element7.AppendChild(element8);
            element.AppendChild(element7);
            return element;
        }
		public static XmlDocument CreateEmptyCombatConfigXml()
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<CombatConfig xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\n<GameObjects>\n</GameObjects>\n</CombatConfig>\n");
			return xmlDocument;
		}
		public static XmlElement GetGameObjectsElement(XmlDocument d)
		{
			return d["CombatConfig"]["GameObjects"];
		}
		public static void AppendConfigXml(XmlDocument destination, XmlDocument source)
		{
			XmlElement gameObjectsElement = CombatConfig.GetGameObjectsElement(destination);
			XmlElement gameObjectsElement2 = CombatConfig.GetGameObjectsElement(source);
			foreach (XmlElement current in gameObjectsElement2.OfType<XmlElement>())
			{
				gameObjectsElement.AppendChild(gameObjectsElement.OwnerDocument.ImportNode(current, true));
			}
		}
	}
}
