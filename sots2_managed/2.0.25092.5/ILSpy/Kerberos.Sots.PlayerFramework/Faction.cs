using Kerberos.Sots.Data;
using Kerberos.Sots.Data.StarMapFramework;
using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Data.Xml;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Steam;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
namespace Kerberos.Sots.PlayerFramework
{
	internal class Faction
	{
		public string FactionFileName;
		public Subfaction[] Subfactions;
		public string Directory;
		public string AssetPath;
		public string WeaponModelPath;
		public string Name;
		public int ID;
		public bool IsPlayable;
		public bool UsesNPCCombatAI;
		public string NoAvatar;
		public string[] MaterialDictionaries;
		public string[] BadgeTexturePaths;
		public string[] AvatarTexturePaths;
		public readonly float[] MaxPopulationMod = new float[3];
		public readonly float[] MaxAlienPopulationMod = new float[3];
		public readonly float[] AIFastResearchRate = new float[6];
		public float EntryPointOffset;
		public float StarTearTechEnteryPointOffset;
		public float PsionicPowerPerCrew;
		public float PsionicPowerModifier;
		public float CrewEfficiencyValue;
		public BoardingActionModifiers BoardingActionMods;
		public FactionDecalInfo[] StructDecalInfo;
		public FactionDecalInfo[] ScorchDecalInfo;
		public readonly ShipRole[] DefaultBattleRiderShipRoles;
		public readonly ShipRole[] DefaultStandardShipRoles;
		public readonly ShipRole[] DefaultAIShipRoles;
		public readonly InitialDesign[] InitialDesigns;
		public readonly DiplomacyActionWeights DiplomacyWeights;
		public readonly IndyDesc IndyDescrition;
		public readonly float ResearchBoostFailureMod;
		private Dictionary<string, int> _defaultReactionValue = new Dictionary<string, int>();
		private Dictionary<string, float> _ImmigrationPopBonusValue = new Dictionary<string, float>();
		private Dictionary<string, float> _SpyingBonusValue = new Dictionary<string, float>();
		private Dictionary<GovernmentInfo.GovernmentType, Dictionary<MoralEvent, int>> _factionSpecificMoral = new Dictionary<GovernmentInfo.GovernmentType, Dictionary<MoralEvent, int>>();
		public readonly ShipRole[] DefaultCombinedShipRoles;
		private XmlElement StratModifiers;
		public int RepSel = 3;
		public int DefaultRepSel;
		public int? _dlcID;
		private FactionObject _factionObject;
		private int _factionObjectCount;
		public IEnumerable<string> TechTreeModels
		{
			get;
			private set;
		}
		public IEnumerable<string> TechTreeRoots
		{
			get;
			private set;
		}
		public LocalizedNameGrabBag DesignNames
		{
			get;
			private set;
		}
		public LocalizedNameGrabBag EmpireNames
		{
			get;
			private set;
		}
		public int? DlcID
		{
			get
			{
				return this._dlcID;
			}
		}
		public FactionObject FactionObj
		{
			get
			{
				return this._factionObject;
			}
		}
		public int GetDefaultReactionToFaction(Faction faction)
		{
			int result = 0;
			if (this._defaultReactionValue.TryGetValue(faction.Name, out result))
			{
				return result;
			}
			return DiplomacyInfo.DefaultDeplomacyRelations;
		}
		public float GetImmigrationPopBonusValueForFaction(Faction faction)
		{
			float result = 1f;
			if (this._ImmigrationPopBonusValue.TryGetValue(faction.Name, out result))
			{
				return result;
			}
			return 1f;
		}
		public float GetSpyingBonusValueForFaction(Faction faction)
		{
			float result = 0f;
			if (this._SpyingBonusValue.TryGetValue(faction.Name, out result))
			{
				return result;
			}
			return 0f;
		}
		public int GetMoralValue(GovernmentInfo.GovernmentType gt, MoralEvent me, int originalValue)
		{
			Dictionary<MoralEvent, int> dictionary;
			if (this._factionSpecificMoral.TryGetValue(gt, out dictionary))
			{
				int result = 0;
				if (this._factionSpecificMoral[gt].TryGetValue(me, out result))
				{
					return result;
				}
			}
			return originalValue;
		}
		public bool CanFactionObtainTechBranch(string branchType)
		{
			return !(this.Name == "loa") || (branchType != "PSI" && branchType != "CYB");
		}
		private static IEnumerable<ShipRole> EnumerateDefaultBattleRiderShipRoles(string factionName)
		{
			if (factionName == "zuul")
			{
				yield return ShipRole.SLAVEDISK;
			}
			yield return ShipRole.DRONE;
			yield return ShipRole.ASSAULTSHUTTLE;
			yield return ShipRole.BOARDINGPOD;
			yield return ShipRole.BIOMISSILE;
			yield break;
		}
		private static IEnumerable<ShipRole> EnumerateDefaultStandardShipRoles(string factionName)
		{
			yield return ShipRole.COMMAND;
			yield return ShipRole.COMBAT;
			yield return ShipRole.COLONIZER;
			yield return ShipRole.SUPPLY;
			yield return ShipRole.CONSTRUCTOR;
			yield return ShipRole.TRAPDRONE;
			if (factionName == "hiver")
			{
				yield return ShipRole.GATE;
			}
			if (factionName == "zuul")
			{
				yield return ShipRole.BORE;
			}
			if (factionName == "loa")
			{
				yield return ShipRole.ACCELERATOR_GATE;
				yield return ShipRole.LOA_CUBE;
			}
			yield return ShipRole.POLICE;
			yield break;
		}
		private static IEnumerable<ShipRole> EnumerateDefaultAIShipRoles(string factionName)
		{
			yield return ShipRole.BR_ESCORT;
			yield return ShipRole.BR_INTERCEPTOR;
			yield return ShipRole.BR_PATROL;
			yield return ShipRole.BR_SCOUT;
			yield return ShipRole.BR_SPINAL;
			yield return ShipRole.BR_TORPEDO;
			yield return ShipRole.BATTLECRUISER;
			yield return ShipRole.BATTLESHIP;
			foreach (ShipRole current in Faction.EnumerateDefaultStandardShipRoles(factionName))
			{
				yield return current;
			}
			yield break;
		}
		private static IEnumerable<ShipRole> EnumerateDefaultCombinedShipRoles(string factionName)
		{
			return Faction.EnumerateDefaultBattleRiderShipRoles(factionName).Concat(Faction.EnumerateDefaultStandardShipRoles(factionName));
		}
		private Faction(string filename)
		{
			for (int i = 0; i < 3; i++)
			{
				this.MaxPopulationMod[i] = 1f;
				this.MaxAlienPopulationMod[i] = 1f;
			}
			for (int j = 0; j < 6; j++)
			{
				this.AIFastResearchRate[j] = 0.6f;
			}
			this.FactionFileName = filename;
			this.Directory = Path.GetDirectoryName(filename);
			this.Name = Path.GetFileNameWithoutExtension(Path.GetDirectoryName(filename));
			this.DefaultBattleRiderShipRoles = Faction.EnumerateDefaultBattleRiderShipRoles(this.Name).ToArray<ShipRole>();
			this.DefaultStandardShipRoles = Faction.EnumerateDefaultStandardShipRoles(this.Name).ToArray<ShipRole>();
			this.DefaultCombinedShipRoles = Faction.EnumerateDefaultCombinedShipRoles(this.Name).ToArray<ShipRole>();
			this.DefaultAIShipRoles = Faction.EnumerateDefaultAIShipRoles(this.Name).ToArray<ShipRole>();
			string name;
			switch (name = this.Name)
			{
			case "human":
			case "tarkas":
			case "liir_zuul":
			case "zuul":
			case "morrigi":
			case "hiver":
			case "loa":
				this.IsPlayable = true;
				goto IL_1D7;
			}
			this.IsPlayable = false;
			IL_1D7:
			string name2;
			switch (name2 = this.Name)
			{
			case "human":
				this._dlcID = new int?(202240);
				this.Subfactions = new Subfaction[]
				{
					new Subfaction(),
					new Subfaction
					{
						DlcID = new SteamDLCIdentifiers?(SteamDLCIdentifiers.SolForceImmersionPack),
						MountName = "dlc_human"
					}
				};
				goto IL_4E2;
			case "tarkas":
				this._dlcID = new int?(202220);
				this.Subfactions = new Subfaction[]
				{
					new Subfaction(),
					new Subfaction
					{
						DlcID = new SteamDLCIdentifiers?(SteamDLCIdentifiers.HiverAndTarkasImmersionPack),
						MountName = "dlc_tarkas"
					}
				};
				goto IL_4E2;
			case "hiver":
				this._dlcID = new int?(202220);
				this.Subfactions = new Subfaction[]
				{
					new Subfaction(),
					new Subfaction
					{
						DlcID = new SteamDLCIdentifiers?(SteamDLCIdentifiers.HiverAndTarkasImmersionPack),
						MountName = "dlc_hiver"
					}
				};
				goto IL_4E2;
			case "liir_zuul":
				this._dlcID = new int?(202230);
				this.Subfactions = new Subfaction[]
				{
					new Subfaction(),
					new Subfaction
					{
						DlcID = new SteamDLCIdentifiers?(SteamDLCIdentifiers.LiirAndMorrigiImmersionPack),
						MountName = "dlc_liir_zuul"
					}
				};
				goto IL_4E2;
			case "morrigi":
				this._dlcID = new int?(202230);
				this.Subfactions = new Subfaction[]
				{
					new Subfaction(),
					new Subfaction
					{
						DlcID = new SteamDLCIdentifiers?(SteamDLCIdentifiers.LiirAndMorrigiImmersionPack),
						MountName = "dlc_morrigi"
					}
				};
				goto IL_4E2;
			case "zuul":
				this._dlcID = new int?(203050);
				this.Subfactions = new Subfaction[]
				{
					new Subfaction(),
					new Subfaction
					{
						DlcID = new SteamDLCIdentifiers?(SteamDLCIdentifiers.TheHordeImmersionPack),
						MountName = "dlc_zuul"
					}
				};
				goto IL_4E2;
			case "loa":
				this.Subfactions = new Subfaction[]
				{
					new Subfaction
					{
						DlcID = null,
						MountName = "eof"
					}
				};
				goto IL_4E2;
			}
			this.Subfactions = new Subfaction[]
			{
				new Subfaction()
			};
			IL_4E2:
			this.AssetPath = Path.Combine("factions", this.Name);
			this.WeaponModelPath = Path.Combine(this.AssetPath, "models", "weapons");
			XmlDocument xmlDocument = Faction.LoadMergedXMLDocument(filename);
			string name3 = "Faction";
			XmlElement xmlElement = xmlDocument[name3];
			XmlElement xmlElement2 = xmlElement["DiplomacyActionWeights"];
			if (xmlElement2 != null)
			{
				XmlDocument xmlDocument2 = new XmlDocument();
				xmlDocument2.Load(ScriptHost.FileSystem, xmlElement2.InnerText);
				this.DiplomacyWeights = new DiplomacyActionWeights(xmlDocument2);
			}
			else
			{
				this.DiplomacyWeights = new DiplomacyActionWeights();
			}
			this.IndyDescrition = null;
			if (this.IsIndependent())
			{
				XmlElement xmlElement3 = xmlElement["IndyDescriptions"];
				if (xmlElement3 != null)
				{
					this.IndyDescrition = new IndyDesc();
					this.IndyDescrition.CoreSpecialAttributes = new List<SpecialAttribute>();
					this.IndyDescrition.RandomSpecialAttributes = new List<SpecialAttribute>();
					this.IndyDescrition.TechLevel = ((xmlElement3["TechLevel"] != null) ? int.Parse(xmlElement3["TechLevel"].InnerText) : 1);
					this.IndyDescrition.MinPlanetSize = ((xmlElement3["MinPlanetSize"] != null) ? int.Parse(xmlElement3["MinPlanetSize"].InnerText) : 0);
					this.IndyDescrition.MaxPlanetSize = ((xmlElement3["MaxPlanetSize"] != null) ? int.Parse(xmlElement3["MaxPlanetSize"].InnerText) : 0);
					this.IndyDescrition.StellarBodyType = ((xmlElement3["StellarBodyType"] != null) ? xmlElement3["StellarBodyType"].InnerText : string.Empty);
					this.IndyDescrition.BaseFactionSuitability = ((xmlElement3["Hazard"] != null) ? xmlElement3["Hazard"].GetAttribute("faction").ToLower() : string.Empty);
					this.IndyDescrition.Suitability = (float)((xmlElement3["Hazard"] != null) ? int.Parse(xmlElement3["Hazard"].GetAttribute("deviation")) : 0);
					this.IndyDescrition.BasePopulationMod = ((xmlElement3["BasePopulationMod"] != null) ? float.Parse(xmlElement3["BasePopulationMod"].InnerText) : 1f);
					this.IndyDescrition.BiosphereMod = ((xmlElement3["BiosphereMod"] != null) ? float.Parse(xmlElement3["BiosphereMod"].InnerText) : 0f);
					this.IndyDescrition.TradeFTL = ((xmlElement3["TradeFTL"] != null) ? float.Parse(xmlElement3["TradeFTL"].InnerText) : 0f);
				}
			}
			this.ID = int.Parse(xmlElement.GetAttribute("ID"));
			this.MaterialDictionaries = (
				from x in xmlElement.OfType<XmlElement>()
				where x.Name.Equals("MaterialDictionary", StringComparison.InvariantCulture)
				select x.InnerText).ToArray<string>();
			this.BadgeTexturePaths = (
				from x in xmlElement.OfType<XmlElement>()
				where x.Name.Equals("Badge", StringComparison.InvariantCulture)
				select x.GetAttribute("texture").ToLowerInvariant()).ToArray<string>();
			this.AvatarTexturePaths = (
				from x in xmlElement.OfType<XmlElement>()
				where x.Name.Equals("Avatar", StringComparison.InvariantCulture)
				select x.GetAttribute("texture").ToLowerInvariant()).ToArray<string>();
			List<string> list = new List<string>();
			List<string> list2 = new List<string>();
			foreach (XmlElement current in 
				from x in xmlElement.OfType<XmlElement>()
				where x.Name == "TechTree"
				select x)
			{
				list.AddRange(AssetDatabase.LoadTechTreeModels(current));
				list2.AddRange(AssetDatabase.LoadTechTreeRoots(current));
			}
			this.TechTreeModels = list;
			this.TechTreeRoots = list2;
			Random random = new Random();
			this.DesignNames = new LocalizedNameGrabBag(xmlElement["DesignNames"], random);
			this.EmpireNames = new LocalizedNameGrabBag(xmlElement["EmpireNames"], random);
			this.UsesNPCCombatAI = bool.Parse(xmlElement["UseNPCCombatAI"].InnerText);
			this.EntryPointOffset = ((xmlElement["EntryPointOffset"] != null) ? float.Parse(xmlElement["EntryPointOffset"].InnerText) : 0f);
			this.StarTearTechEnteryPointOffset = ((xmlElement["StarTearTechEnteryPointOffset"] != null) ? float.Parse(xmlElement["StarTearTechEnteryPointOffset"].InnerText) : 0f);
			this.PsionicPowerPerCrew = ((xmlElement["PsiPowerPerCrew"] != null) ? float.Parse(xmlElement["PsiPowerPerCrew"].InnerText) : 0f);
			this.PsionicPowerModifier = ((xmlElement["PsiPowerModifier"] != null) ? float.Parse(xmlElement["PsiPowerModifier"].InnerText) : 1f);
			this.CrewEfficiencyValue = ((xmlElement["CrewEfficiencyValue"] != null) ? float.Parse(xmlElement["CrewEfficiencyValue"].InnerText) : 1f);
			this.ResearchBoostFailureMod = ((xmlElement["ResearchBoostAccidentMod"] != null) ? float.Parse(xmlElement["ResearchBoostAccidentMod"].InnerText) : 1f);
			if (xmlElement["NoAvatar"] != null)
			{
				this.NoAvatar = xmlElement["NoAvatar"].GetAttribute("texture");
			}
			List<FactionDecalInfo> list3 = new List<FactionDecalInfo>();
			List<FactionDecalInfo> list4 = new List<FactionDecalInfo>();
			XmlElement xmlElement4 = xmlElement["FactionDamageDecals"];
			if (xmlElement4 != null)
			{
				IEnumerable<XmlElement> enumerable = 
					from x in xmlElement4.OfType<XmlElement>()
					where x.Name.Equals("Structure", StringComparison.InvariantCulture)
					select x;
				foreach (XmlElement current2 in enumerable)
				{
					FactionDecalInfo fdi = default(FactionDecalInfo);
					fdi.DecalShipClass = (ShipClass)Enum.Parse(typeof(ShipClass), current2.GetAttribute("class"));
					if (!list3.Any((FactionDecalInfo x) => x.DecalShipClass == fdi.DecalShipClass))
					{
						List<DecalStageInfo> list5 = new List<DecalStageInfo>();
						foreach (XmlElement current3 in 
							from x in enumerable
							where (ShipClass)Enum.Parse(typeof(ShipClass), x.GetAttribute("class")) == fdi.DecalShipClass
							select x)
						{
							DecalStageInfo item;
							item.DecalStage = int.Parse(current3.GetAttribute("stage"));
							item.DecalSize = float.Parse(current3.GetAttribute("size"));
							item.DecalMaterial = current3.GetAttribute("material");
							list5.Add(item);
						}
						fdi.DecalStages = list5.ToArray();
						list3.Add(fdi);
					}
				}
				IEnumerable<XmlElement> enumerable2 = 
					from x in xmlElement4.OfType<XmlElement>()
					where x.Name.Equals("Scorch", StringComparison.InvariantCulture)
					select x;
				foreach (XmlElement current4 in enumerable2)
				{
					FactionDecalInfo fdi = default(FactionDecalInfo);
					fdi.DecalShipClass = (ShipClass)Enum.Parse(typeof(ShipClass), current4.GetAttribute("class"));
					if (!list4.Any((FactionDecalInfo x) => x.DecalShipClass == fdi.DecalShipClass))
					{
						List<DecalStageInfo> list6 = new List<DecalStageInfo>();
						foreach (XmlElement current5 in 
							from x in enumerable
							where (ShipClass)Enum.Parse(typeof(ShipClass), x.GetAttribute("class")) == fdi.DecalShipClass
							select x)
						{
							DecalStageInfo item2;
							item2.DecalStage = int.Parse(current5.GetAttribute("stage"));
							item2.DecalSize = float.Parse(current5.GetAttribute("size"));
							item2.DecalMaterial = current5.GetAttribute("material");
							list6.Add(item2);
						}
						fdi.DecalStages = list6.ToArray();
						list4.Add(fdi);
					}
				}
			}
			this.StructDecalInfo = list3.ToArray();
			this.ScorchDecalInfo = list4.ToArray();
			XmlElement xmlElement5 = xmlElement["BoardingActionModifiers"];
			if (xmlElement5 != null)
			{
				this.BoardingActionMods.FreshAgentStrength = ((xmlElement5["FreshAgentStrength"] != null) ? float.Parse(xmlElement5["FreshAgentStrength"].InnerText) : 1f);
				this.BoardingActionMods.TiredAgentStrength = ((xmlElement5["TiredAgentStrength"] != null) ? float.Parse(xmlElement5["TiredAgentStrength"].InnerText) : 0.5f);
				this.BoardingActionMods.ExhaustedAgentStrength = ((xmlElement5["ExhaustedAgentStrength"] != null) ? float.Parse(xmlElement5["ExhaustedAgentStrength"].InnerText) : 0.25f);
				this.BoardingActionMods.LocationStrength.Default = ((xmlElement5["AgentLocationStrength"] != null) ? float.Parse(xmlElement5["AgentLocationStrength"].GetAttribute("default")) : 1f);
				this.BoardingActionMods.LocationStrength.Cruiser = ((xmlElement5["AgentLocationStrength"] != null) ? float.Parse(xmlElement5["AgentLocationStrength"].GetAttribute("cruiser")) : 1f);
				this.BoardingActionMods.LocationStrength.Dreadnought = ((xmlElement5["AgentLocationStrength"] != null) ? float.Parse(xmlElement5["AgentLocationStrength"].GetAttribute("dreadnought")) : 1f);
				this.BoardingActionMods.LocationStrength.Leviathan = ((xmlElement5["AgentLocationStrength"] != null) ? float.Parse(xmlElement5["AgentLocationStrength"].GetAttribute("leviathan")) : 1f);
				this.BoardingActionMods.EfficiencyVSBoarding.Default = ((xmlElement5["EfficiencyVSBoarding"] != null) ? float.Parse(xmlElement5["EfficiencyVSBoarding"].GetAttribute("default")) : 0.5f);
				this.BoardingActionMods.EfficiencyVSBoarding.Cruiser = ((xmlElement5["EfficiencyVSBoarding"] != null) ? float.Parse(xmlElement5["EfficiencyVSBoarding"].GetAttribute("cruiser")) : 0.5f);
				this.BoardingActionMods.EfficiencyVSBoarding.Dreadnought = ((xmlElement5["EfficiencyVSBoarding"] != null) ? float.Parse(xmlElement5["EfficiencyVSBoarding"].GetAttribute("dreadnought")) : 0.5f);
				this.BoardingActionMods.EfficiencyVSBoarding.Leviathan = ((xmlElement5["EfficiencyVSBoarding"] != null) ? float.Parse(xmlElement5["EfficiencyVSBoarding"].GetAttribute("leviathan")) : 0.5f);
			}
			else
			{
				this.BoardingActionMods.FreshAgentStrength = 1f;
				this.BoardingActionMods.TiredAgentStrength = 0.5f;
				this.BoardingActionMods.ExhaustedAgentStrength = 0.25f;
				this.BoardingActionMods.LocationStrength.Default = 1f;
				this.BoardingActionMods.LocationStrength.Cruiser = 1f;
				this.BoardingActionMods.LocationStrength.Dreadnought = 1f;
				this.BoardingActionMods.LocationStrength.Leviathan = 1f;
				this.BoardingActionMods.EfficiencyVSBoarding.Default = 0.5f;
				this.BoardingActionMods.EfficiencyVSBoarding.Cruiser = 0.5f;
				this.BoardingActionMods.EfficiencyVSBoarding.Dreadnought = 0.5f;
				this.BoardingActionMods.EfficiencyVSBoarding.Leviathan = 0.5f;
			}
			XmlElement xmlElement6 = xmlElement["DefaultDiplomacyReactions"];
			if (xmlElement6 != null)
			{
				foreach (XmlElement current6 in xmlElement6.OfType<XmlElement>())
				{
					string attribute = current6.GetAttribute("faction");
					if (!this._defaultReactionValue.ContainsKey(attribute))
					{
						this._defaultReactionValue.Add(attribute, Math.Min(Math.Max(int.Parse(current6.GetAttribute("value")), DiplomacyInfo.MinDeplomacyRelations), DiplomacyInfo.MaxDeplomacyRelations));
					}
				}
			}
			XmlElement xmlElement7 = xmlElement["ImmigrationPopBonus"];
			if (xmlElement7 != null)
			{
				foreach (XmlElement current7 in xmlElement7.OfType<XmlElement>())
				{
					string attribute2 = current7.GetAttribute("faction");
					if (!this._ImmigrationPopBonusValue.ContainsKey(attribute2))
					{
						this._ImmigrationPopBonusValue.Add(attribute2, float.Parse(current7.GetAttribute("value")));
					}
				}
			}
			XmlElement xmlElement8 = xmlElement["SpyingBonus"];
			if (xmlElement8 != null)
			{
				foreach (XmlElement current8 in xmlElement8.OfType<XmlElement>())
				{
					string attribute3 = current8.GetAttribute("faction");
					if (!this._SpyingBonusValue.ContainsKey(attribute3))
					{
						this._SpyingBonusValue.Add(attribute3, float.Parse(current8.GetAttribute("value")));
					}
				}
			}
			this.StratModifiers = xmlElement["StratModifiers"];
			XmlElement xmlElement9 = xmlElement["SalvageModifiers"];
			if (xmlElement9 != null)
			{
				this.RepSel = int.Parse(xmlElement9["RepSal"].InnerText);
				this.DefaultRepSel = int.Parse(xmlElement9["default"].InnerText);
			}
			XmlElement xmlElement10 = xmlElement["MoralEventModifiers"];
			if (xmlElement10 != null)
			{
				foreach (GovernmentInfo.GovernmentType governmentType in Enum.GetValues(typeof(GovernmentInfo.GovernmentType)))
				{
					XmlElement xmlElement11 = xmlElement10[governmentType.ToString()];
					if (xmlElement11 != null)
					{
						foreach (MoralEvent moralEvent in Enum.GetValues(typeof(MoralEvent)))
						{
							XmlElement xmlElement12 = xmlElement11[moralEvent.ToString()];
							if (xmlElement12 != null)
							{
								if (!this._factionSpecificMoral.ContainsKey(governmentType))
								{
									this._factionSpecificMoral.Add(governmentType, new Dictionary<MoralEvent, int>());
								}
								this._factionSpecificMoral[governmentType].Add(moralEvent, int.Parse(xmlElement12.InnerText));
							}
						}
					}
				}
			}
			XmlElement xmlElement13 = xmlElement["ResearchRates"];
			if (xmlElement13 != null)
			{
				foreach (AIStance aIStance in Enum.GetValues(typeof(AIStance)))
				{
					XmlElement xmlElement14 = xmlElement13[aIStance.ToString()];
					if (xmlElement14 != null)
					{
						float val = float.Parse(xmlElement14.InnerText);
						this.AIFastResearchRate[(int)aIStance] = Math.Max(Math.Min(val, 1f), 0f);
					}
				}
			}
			XmlElement xmlElement15 = xmlElement["InitialDesigns"];
			if (xmlElement15 != null)
			{
				string weaponBiasTechFamilyID = xmlElement15.GetAttribute("weaponbias");
				this.InitialDesigns = (
					from x in xmlElement15.OfType<XmlElement>()
					where x.Name == "Design"
					select x).Select(delegate(XmlElement y)
				{
					InitialDesign initialDesign = new InitialDesign();
					initialDesign.Name = y.GetAttribute("name");
					initialDesign.WeaponBiasTechFamilyID = weaponBiasTechFamilyID;
					initialDesign.Sections = (
						from z in y.OfType<XmlElement>()
						where z.Name == "Section"
						select z into w
						select w.GetAttribute("name")).ToArray<string>();
					return initialDesign;
				}).ToArray<InitialDesign>();
				return;
			}
			this.InitialDesigns = null;
		}
		public override string ToString()
		{
			return this.Name + ", " + this.FactionFileName;
		}
		public object GetStratModifier(string name)
		{
			if (this.StratModifiers == null)
			{
				return null;
			}
			XmlElement xmlElement = this.StratModifiers[name];
			if (xmlElement == null)
			{
				return null;
			}
			return xmlElement.InnerText;
		}
		public static XmlDocument LoadMergedXMLDocument(string filename)
		{
			string[] array = ScriptHost.FileSystem.FindFiles(filename);
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(ScriptHost.FileSystem, array[0]);
			for (int i = 1; i < array.Length; i++)
			{
				XmlDocument xmlDocument2 = new XmlDocument();
				xmlDocument2.Load(ScriptHost.FileSystem, array[i]);
				foreach (XmlNode current in xmlDocument2.DocumentElement.ChildNodes.OfType<XmlElement>())
				{
					xmlDocument.DocumentElement.AppendChild(xmlDocument.ImportNode(current, true));
				}
			}
			return xmlDocument;
		}
		public static Faction LoadXml(string filename)
		{
			return new Faction(filename);
		}
		public string GetWeaponModelPath(string filenameWithoutDirectory)
		{
			return Path.Combine(this.WeaponModelPath, filenameWithoutDirectory);
		}
		public bool CanSupportPopulation(PopulationType type)
		{
			return this.MaxPopulationMod[(int)type] > 0f;
		}
		public float GetAIResearchRate(AIStance stance)
		{
			return this.AIFastResearchRate[(int)stance];
		}
		public bool IsPlagueImmune(WeaponEnums.PlagueType pt)
		{
			return (this.Name == "zuul" && pt != WeaponEnums.PlagueType.XOMBIE) || (this.Name == "loa" && pt != WeaponEnums.PlagueType.NANO);
		}
		public string SplinterAvatarPath()
		{
			return "Independent_Avatar_" + char.ToUpper(this.Name[0]) + this.Name.Substring(1);
		}
		public bool IsIndependent()
		{
			return this.Name == "enki" || this.Name == "kaeru" || this.Name == "mindi" || this.Name == "nandi" || this.Name == "tatzel" || this.Name == "utukku" || this.Name == "deeroz" || this.Name == "m'kkkose";
		}
		public bool IsFactionIndependentTrader()
		{
			return this.Name == "zuul";
		}
		public bool HasSlaves()
		{
			return this.Name == "zuul";
		}
		public bool CanUseGate()
		{
			return this.Name == "hiver";
		}
		public bool CanUseAccelerators()
		{
			return this.Name == "loa";
		}
		public bool CanUseNodeLine(bool? permanent = null)
		{
			if (!permanent.HasValue)
			{
				return this.Name == "human" || this.Name == "zuul" || this.Name == "loa";
			}
			if (permanent.Value)
			{
				return this.Name == "human";
			}
			return this.Name == "zuul" || this.Name == "loa";
		}
		public bool CanUseGravityWell()
		{
			return this.Name == "liir_zuul";
		}
		public bool CanUseFlockBonus()
		{
			return this.Name == "morrigi";
		}
		public float ChooseIdealSuitability(Random random)
		{
			int num = 500;
			return random.NextInclusive(Constants.MinSuitability + (float)num, Constants.MaxSuitability - (float)num);
		}
		public void AddFactionReference(App game)
		{
			this._factionObjectCount++;
			if (this._factionObjectCount == 1)
			{
				List<object> list = new List<object>();
				list.Add(this.Name);
				list.Add(this.StructDecalInfo.Length);
				FactionDecalInfo[] structDecalInfo = this.StructDecalInfo;
				for (int i = 0; i < structDecalInfo.Length; i++)
				{
					FactionDecalInfo factionDecalInfo = structDecalInfo[i];
					list.Add(factionDecalInfo.DecalShipClass);
					list.Add(factionDecalInfo.DecalStages.Length);
					DecalStageInfo[] decalStages = factionDecalInfo.DecalStages;
					for (int j = 0; j < decalStages.Length; j++)
					{
						DecalStageInfo decalStageInfo = decalStages[j];
						list.Add(decalStageInfo.DecalStage);
						list.Add(decalStageInfo.DecalSize);
						list.Add(decalStageInfo.DecalMaterial);
					}
				}
				list.Add(this.ScorchDecalInfo.Length);
				FactionDecalInfo[] scorchDecalInfo = this.ScorchDecalInfo;
				for (int k = 0; k < scorchDecalInfo.Length; k++)
				{
					FactionDecalInfo factionDecalInfo2 = scorchDecalInfo[k];
					list.Add(factionDecalInfo2.DecalShipClass);
					list.Add(factionDecalInfo2.DecalStages.Length);
					DecalStageInfo[] decalStages2 = factionDecalInfo2.DecalStages;
					for (int l = 0; l < decalStages2.Length; l++)
					{
						DecalStageInfo decalStageInfo2 = decalStages2[l];
						list.Add(decalStageInfo2.DecalStage);
						list.Add(decalStageInfo2.DecalSize);
						list.Add(decalStageInfo2.DecalMaterial);
					}
				}
				list.Add(this.PsionicPowerPerCrew);
				list.Add(this.CrewEfficiencyValue);
				list.Add(this.BoardingActionMods.FreshAgentStrength);
				list.Add(this.BoardingActionMods.TiredAgentStrength);
				list.Add(this.BoardingActionMods.ExhaustedAgentStrength);
				list.Add(this.BoardingActionMods.LocationStrength.Default);
				list.Add(this.BoardingActionMods.LocationStrength.Cruiser);
				list.Add(this.BoardingActionMods.LocationStrength.Dreadnought);
				list.Add(this.BoardingActionMods.LocationStrength.Leviathan);
				list.Add(this.BoardingActionMods.EfficiencyVSBoarding.Default);
				list.Add(this.BoardingActionMods.EfficiencyVSBoarding.Cruiser);
				list.Add(this.BoardingActionMods.EfficiencyVSBoarding.Dreadnought);
				list.Add(this.BoardingActionMods.EfficiencyVSBoarding.Leviathan);
				this._factionObject = game.AddObject<FactionObject>(list.ToArray());
			}
		}
		public void ReleaseFactionReference(App game)
		{
			if (this._factionObject != null)
			{
				if (this._factionObjectCount == 0)
				{
					throw new InvalidOperationException("Weapon reference count already 0.");
				}
				this._factionObjectCount--;
				if (this._factionObjectCount == 0)
				{
					game.ReleaseObject(this._factionObject);
					this._factionObject = null;
				}
			}
		}
	}
}
