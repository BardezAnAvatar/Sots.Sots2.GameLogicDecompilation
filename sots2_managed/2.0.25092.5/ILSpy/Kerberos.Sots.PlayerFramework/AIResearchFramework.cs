using Kerberos.Sots.Data;
using Kerberos.Sots.Data.TechnologyFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
namespace Kerberos.Sots.PlayerFramework
{
	internal class AIResearchFramework
	{
		private class AITechLog
		{
			private readonly StringBuilder _recordText = new StringBuilder();
			private string _prefix;
			private bool _writingRecord;
			public void ClearRecord()
			{
				this._recordText.Clear();
			}
			public void BeginTurn(StrategicAI ai)
			{
				this._writingRecord = true;
				this._prefix = string.Format("Turn {0} ({1}): ", ai.Game.GameDatabase.GetTurnCount(), ai.Player.ID);
			}
			public void EndTurn()
			{
				if (this._writingRecord && this._recordText.Length > 0)
				{
					App.Log.Trace(this._prefix + this._recordText.ToString(), "ai");
				}
				this._prefix = null;
				this._writingRecord = false;
				this.ClearRecord();
			}
			public void Print(string text)
			{
				if (this._writingRecord)
				{
					this._recordText.Append(text);
				}
			}
		}
		private enum AITechReplacementContexts
		{
			Available,
			Researched
		}
		private class AITechReplacementRow
		{
			public Tech New;
			public Tech Old;
			public AIResearchFramework.AITechReplacementContexts Contexts;
		}
		private class AITechStyleRow
		{
			public TechFamilies[] Families;
			public float CostFactor;
			public AICostFactors SelectionChances;
			public override string ToString()
			{
				string text = string.Empty;
				bool flag = true;
				TechFamilies[] families = this.Families;
				for (int i = 0; i < families.Length; i++)
				{
					TechFamilies techFamilies = families[i];
					if (!flag)
					{
						text += ",";
					}
					else
					{
						flag = false;
					}
					text += techFamilies.ToString();
				}
				return text;
			}
			public IEnumerable<AITechStyleInfo> ToTechStyleInfos(int playerId, string faction)
			{
				try
				{
					TechFamilies[] families = this.Families;
					for (int i = 0; i < families.Length; i++)
					{
						TechFamilies techFamily = families[i];
						yield return new AITechStyleInfo
						{
							PlayerID = playerId,
							TechFamily = techFamily,
							CostFactor = this.CostFactor
						};
					}
				}
				finally
				{
				}
				yield break;
			}
			private static AICostFactors ReadChances(BinaryReader r)
			{
				AICostFactors zero = AICostFactors.Zero;
				int num = r.ReadInt32();
				for (int i = 0; i < num; i++)
				{
					string faction = r.ReadString();
					float value = r.ReadSingle();
					zero.SetFaction(faction, value);
				}
				return zero;
			}
			private static void WriteChances(AICostFactors value, BinaryWriter w)
			{
				w.Write(AICostFactors.Factions.Count);
				foreach (string current in AICostFactors.Factions)
				{
					w.Write(current);
					w.Write(value.Faction(current));
				}
			}
			public static AIResearchFramework.AITechStyleRow Read(BinaryReader r)
			{
				int num = r.ReadInt32();
				TechFamilies[] array = new TechFamilies[num];
				for (int i = 0; i < num; i++)
				{
					array[i] = (TechFamilies)Enum.Parse(typeof(TechFamilies), r.ReadString());
				}
				float costFactor = r.ReadSingle();
				AICostFactors selectionChances = AIResearchFramework.AITechStyleRow.ReadChances(r);
				return new AIResearchFramework.AITechStyleRow
				{
					CostFactor = costFactor,
					Families = array,
					SelectionChances = selectionChances
				};
			}
			public static void Write(AIResearchFramework.AITechStyleRow value, BinaryWriter w)
			{
				w.Write(value.Families.Length);
				TechFamilies[] families = value.Families;
				for (int i = 0; i < families.Length; i++)
				{
					TechFamilies techFamilies = families[i];
					w.Write(techFamilies.ToString());
				}
				w.Write(value.CostFactor);
				AIResearchFramework.AITechStyleRow.WriteChances(value.SelectionChances, w);
			}
			private static TechFamilies[] ParseFamilies(XmlElement e)
			{
				if (e == null)
				{
					return new TechFamilies[0];
				}
				return (
					from x in e.OfType<XmlElement>()
					where x.Name == "family"
					select x into y
					select (TechFamilies)Enum.Parse(typeof(TechFamilies), y.GetAttribute("value"))).ToArray<TechFamilies>();
			}
			private static AICostFactors ParseChances(XmlElement e)
			{
				if (e == null)
				{
					return AICostFactors.Zero;
				}
				AICostFactors zero = AICostFactors.Zero;
				foreach (XmlElement current in 
					from x in e.OfType<XmlElement>()
					where x.Name == "chance"
					select x)
				{
					zero.SetFaction(current.GetAttribute("faction"), float.Parse(current.GetAttribute("value")));
				}
				return zero;
			}
			public static AIResearchFramework.AITechStyleRow Parse(XmlElement e)
			{
				return new AIResearchFramework.AITechStyleRow
				{
					CostFactor = float.Parse(e.GetAttribute("costmul")),
					Families = AIResearchFramework.AITechStyleRow.ParseFamilies(e["families"]),
					SelectionChances = AIResearchFramework.AITechStyleRow.ParseChances(e["chances"])
				};
			}
		}
		private class AITechStyleGroup
		{
			public List<AIResearchFramework.AITechStyleRow> Styles;
			public static AIResearchFramework.AITechStyleGroup Read(BinaryReader r)
			{
				List<AIResearchFramework.AITechStyleRow> list = new List<AIResearchFramework.AITechStyleRow>();
				int num = r.ReadInt32();
				for (int i = 0; i < num; i++)
				{
					list.Add(AIResearchFramework.AITechStyleRow.Read(r));
				}
				return new AIResearchFramework.AITechStyleGroup
				{
					Styles = list
				};
			}
			public static void Write(AIResearchFramework.AITechStyleGroup value, BinaryWriter w)
			{
				w.Write(value.Styles.Count);
				foreach (AIResearchFramework.AITechStyleRow current in value.Styles)
				{
					AIResearchFramework.AITechStyleRow.Write(current, w);
				}
			}
			public static AIResearchFramework.AITechStyleGroup Parse(XmlElement e)
			{
				AIResearchFramework.AITechStyleGroup aITechStyleGroup = new AIResearchFramework.AITechStyleGroup();
				aITechStyleGroup.Styles = new List<AIResearchFramework.AITechStyleRow>(
					from x in e.OfType<XmlElement>()
					where x.Name == "style"
					select x into y
					select AIResearchFramework.AITechStyleRow.Parse(y));
				return aITechStyleGroup;
			}
		}
		private class AIResearchRhythm
		{
			public AIResearchModes[] Beats;
		}
		private struct TechBeat
		{
			public Tech Tech;
			public int Beat;
			public override string ToString()
			{
				return this.Tech.ToString() + "," + this.Beat.ToString();
			}
		}
		private const int BinaryResponseOverrideTurns = 5;
		private AIResearchFramework.AITechLog _log;
		private readonly AIResearchFramework.AITechReplacementRow[] _replacements;
		private readonly List<AIResearchFramework.AITechStyleGroup> _styleGroups;
		private readonly Dictionary<AIStance, AIResearchFramework.AIResearchRhythm> _rhythms;
		public AIResearchFramework()
		{
			this._replacements = new AIResearchFramework.AITechReplacementRow[0];
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml("\r\n<stylegroups>\r\n    <!-- Race value format = Human/Hiver/Tark/Liir/Zuul/Morrigi (% chance) -->\r\n    <!-- style, group, priority, % chance -->\r\n\r\n    <!--{ {EnergyWeapons},       1, 0.50, 40,10,30,90,20,70 },-->\r\n    <!--{ {BallisticWeapons},    1, 0.50, 40,80,60,10,70,20 },-->\r\n    <group>\r\n        <style costmul='0.5'>\r\n            <families>\r\n                <family value='EnergyWeapons'/>\r\n            </families>\r\n            <chances>\r\n                <chance faction='human'     value='40'/>\r\n                <chance faction='hiver'     value='10'/>\r\n                <chance faction='tarkas'    value='30'/>\r\n                <chance faction='liir_zuul' value='90'/>\r\n                <chance faction='zuul'      value='20'/>\r\n                <chance faction='morrigi'   value='70'/>\r\n            </chances>\r\n        </style>\r\n        <style costmul='0.5'>\r\n            <families>\r\n                <family value='BallisticWeapons'/>\r\n            </families>\r\n            <chances>\r\n                <chance faction='human'     value='40'/>\r\n                <chance faction='hiver'     value='80'/>\r\n                <chance faction='tarkas'    value='60'/>\r\n                <chance faction='liir_zuul' value='10'/>\r\n                <chance faction='zuul'      value='70'/>\r\n                <chance faction='morrigi'   value='20'/>\r\n            </chances>\r\n        </style>\r\n    </group>\r\n\r\n    <!--{ {ShieldTechnology},      2, 0.50, 40,10,30,70,10,60 },-->\r\n    <!--{ {IndustrialTechnology},  2, 0.50, 40,90,70,20,80,20 },-->\r\n    <group>\r\n        <style costmul='0.5'>\r\n            <families>\r\n                <family value='ShieldTechnology'/>\r\n            </families>\r\n            <chances>\r\n                <chance faction='human'     value='40'/>\r\n                <chance faction='hiver'     value='10'/>\r\n                <chance faction='tarkas'    value='30'/>\r\n                <chance faction='liir_zuul' value='70'/>\r\n                <chance faction='zuul'      value='10'/>\r\n                <chance faction='morrigi'   value='60'/>\r\n            </chances>\r\n        </style>\r\n        <style costmul='0.5'>\r\n            <families>\r\n                <family value='IndustrialTechnology'/>\r\n            </families>\r\n            <chances>\r\n                <chance faction='human'     value='40'/>\r\n                <chance faction='hiver'     value='90'/>\r\n                <chance faction='tarkas'    value='70'/>\r\n                <chance faction='liir_zuul' value='20'/>\r\n                <chance faction='zuul'      value='80'/>\r\n                <chance faction='morrigi'   value='20'/>\r\n            </chances>\r\n        </style>\r\n    </group>\r\n\r\n    <!--{ {Torpedos,EnergyWeapons},  3, 0.50, 60,55,70,30,50,70 },-->\r\n    <!--{ {BioTechnology},           3, 0.50, 20,15,20,60, 0,20 },-->\r\n    <group>\r\n        <style costmul='0.5'>\r\n            <families>\r\n                <family value='Torpedos'/>\r\n                <family value='EnergyWeapons'/>\r\n            </families>\r\n            <chances>\r\n                <chance faction='human'     value='60'/>\r\n                <chance faction='hiver'     value='55'/>\r\n                <chance faction='tarkas'    value='70'/>\r\n                <chance faction='liir_zuul' value='30'/>\r\n                <chance faction='zuul'      value='50'/>\r\n                <chance faction='morrigi'   value='70'/>\r\n            </chances>\r\n        </style>\r\n        <style costmul='0.5'>\r\n            <families>\r\n                <family value='BioTechnology'/>\r\n            </families>\r\n            <chances>\r\n                <chance faction='human'     value='20'/>\r\n                <chance faction='hiver'     value='15'/>\r\n                <chance faction='tarkas'    value='20'/>\r\n                <chance faction='liir_zuul' value='60'/>\r\n                <chance faction='zuul'      value='0'/>\r\n                <chance faction='morrigi'   value='20'/>\r\n            </chances>\r\n        </style>\r\n    </group>\r\n\r\n    <!--{ {RiderTechnology,Psionics},  4, 0.50, 40,35,30,90,50,70 },-->\r\n    <!--{ {WarheadTechnology},         4, 0.50, 40,45,50,10,40,20 },-->\r\n    <group>\r\n        <style costmul='0.5'>\r\n            <families>\r\n                <family value='RiderTechnology'/>\r\n                <family value='Psionics'/>\r\n            </families>\r\n            <chances>\r\n                <chance faction='human'     value='40'/>\r\n                <chance faction='hiver'     value='35'/>\r\n                <chance faction='tarkas'    value='30'/>\r\n                <chance faction='liir_zuul' value='90'/>\r\n                <chance faction='zuul'      value='50'/>\r\n                <chance faction='morrigi'   value='70'/>\r\n            </chances>\r\n        </style>\r\n        <style costmul='0.5'>\r\n            <families>\r\n                <family value='WarheadTechnology'/>\r\n            </families>\r\n            <chances>\r\n                <chance faction='human'     value='40'/>\r\n                <chance faction='hiver'     value='45'/>\r\n                <chance faction='tarkas'    value='50'/>\r\n                <chance faction='liir_zuul' value='10'/>\r\n                <chance faction='zuul'      value='40'/>\r\n                <chance faction='morrigi'   value='20'/>\r\n            </chances>\r\n        </style>\r\n    </group>\r\n</stylegroups>\r\n");
			this._styleGroups = new List<AIResearchFramework.AITechStyleGroup>(
				from x in xmlDocument["stylegroups"].OfType<XmlElement>()
				where x.Name == "group"
				select x into y
				select AIResearchFramework.AITechStyleGroup.Parse(y));
			this._rhythms = new Dictionary<AIStance, AIResearchFramework.AIResearchRhythm>();
			Dictionary<AIStance, AIResearchFramework.AIResearchRhythm> arg_C9_0 = this._rhythms;
			AIStance arg_C9_1 = AIStance.ARMING;
			AIResearchFramework.AIResearchRhythm aIResearchRhythm = new AIResearchFramework.AIResearchRhythm();
			AIResearchFramework.AIResearchRhythm arg_C3_0 = aIResearchRhythm;
			AIResearchModes[] array = new AIResearchModes[8];
			array[0] = AIResearchModes.Weapon;
			array[1] = AIResearchModes.Engine;
			array[2] = AIResearchModes.Weapon;
			array[3] = AIResearchModes.Expansion;
			array[5] = AIResearchModes.Weapon;
			array[6] = AIResearchModes.Engine;
			arg_C3_0.Beats = array;
			arg_C9_0[arg_C9_1] = aIResearchRhythm;
			this._rhythms[AIStance.CONQUERING] = new AIResearchFramework.AIResearchRhythm
			{
				Beats = new AIResearchModes[]
				{
					AIResearchModes.Weapon,
					AIResearchModes.Engine,
					AIResearchModes.Weapon,
					AIResearchModes.Weapon,
					AIResearchModes.Empire,
					AIResearchModes.Weapon,
					AIResearchModes.Engine
				}
			};
			this._rhythms[AIStance.DEFENDING] = new AIResearchFramework.AIResearchRhythm
			{
				Beats = new AIResearchModes[]
				{
					AIResearchModes.Weapon,
					AIResearchModes.Empire,
					AIResearchModes.Weapon,
					AIResearchModes.Weapon,
					AIResearchModes.Empire,
					AIResearchModes.Weapon,
					AIResearchModes.Engine
				}
			};
			this._rhythms[AIStance.DESTROYING] = new AIResearchFramework.AIResearchRhythm
			{
				Beats = new AIResearchModes[]
				{
					AIResearchModes.Weapon,
					AIResearchModes.Engine,
					AIResearchModes.Weapon,
					AIResearchModes.Weapon,
					AIResearchModes.Empire,
					AIResearchModes.Weapon,
					AIResearchModes.Engine
				}
			};
			this._rhythms[AIStance.EXPANDING] = new AIResearchFramework.AIResearchRhythm
			{
				Beats = new AIResearchModes[]
				{
					AIResearchModes.Expansion,
					AIResearchModes.Weapon,
					AIResearchModes.Empire,
					AIResearchModes.Weapon,
					AIResearchModes.Expansion,
					AIResearchModes.Engine,
					AIResearchModes.Weapon,
					AIResearchModes.Empire,
					AIResearchModes.Expansion
				}
			};
			Dictionary<AIStance, AIResearchFramework.AIResearchRhythm> arg_213_0 = this._rhythms;
			AIStance arg_213_1 = AIStance.HUNKERING;
			AIResearchFramework.AIResearchRhythm aIResearchRhythm2 = new AIResearchFramework.AIResearchRhythm();
			AIResearchFramework.AIResearchRhythm arg_20C_0 = aIResearchRhythm2;
			AIResearchModes[] array2 = new AIResearchModes[8];
			array2[1] = AIResearchModes.Weapon;
			array2[3] = AIResearchModes.Weapon;
			array2[4] = AIResearchModes.Expansion;
			array2[5] = AIResearchModes.Weapon;
			array2[6] = AIResearchModes.Engine;
			arg_20C_0.Beats = array2;
			arg_213_0[arg_213_1] = aIResearchRhythm2;
		}
		private bool AIHaveReplacementForTech(StrategicAI ai, Tech tech)
		{
			bool flag = false;
			AIResearchFramework.AITechReplacementRow[] replacements = this._replacements;
			for (int i = 0; i < replacements.Length; i++)
			{
				AIResearchFramework.AITechReplacementRow aITechReplacementRow = replacements[i];
				if (tech == aITechReplacementRow.Old)
				{
					PlayerTechInfo playerTechInfo = AIResearchFramework.AIGetPlayerTechInfo(ai, aITechReplacementRow.New);
					if (playerTechInfo != null)
					{
						switch (aITechReplacementRow.Contexts)
						{
						case AIResearchFramework.AITechReplacementContexts.Available:
							flag = (AIResearchFramework.AIIsTechAvailable(playerTechInfo.State) || AIResearchFramework.AIHaveTech(playerTechInfo.State));
							break;
						case AIResearchFramework.AITechReplacementContexts.Researched:
							flag = AIResearchFramework.AIHaveTech(playerTechInfo.State);
							break;
						}
						if (flag)
						{
							break;
						}
					}
				}
			}
			return flag;
		}
		private int AIGetPhase(StrategicAI ai)
		{
			return ai.Game.GameDatabase.GetPlayerTechInfos(ai.Player.ID).Count((PlayerTechInfo x) => x.TurnResearched.HasValue && x.TurnResearched > 1 && x.State == TechStates.Researched);
		}
		private static bool AIContainsTech(AIResearchModes? mode, TechFamilies? family, Tech tech)
		{
			return (!mode.HasValue || tech.AIResearchModeEnums.Contains(mode.Value)) && (!family.HasValue || (TechFamilies)Enum.Parse(typeof(TechFamilies), tech.Family) == family.Value);
		}
		private static bool AIIsTechAvailable(TechStates state)
		{
			switch (state)
			{
			case TechStates.Core:
			case TechStates.Branch:
			case TechStates.HighFeasibility:
				return true;
			}
			return false;
		}
		private static bool AIHaveTech(TechStates state)
		{
			return state == TechStates.Researched;
		}
		private static PlayerTechInfo AIGetPlayerTechInfo(StrategicAI ai, Tech tech)
		{
			return ai.Game.GameDatabase.GetPlayerTechInfo(ai.Player.ID, ai.Game.GameDatabase.GetTechID(tech.Id));
		}
		private static List<Tech> AISelectAvailableTechs(StrategicAI ai, AIResearchModes? mode, TechFamilies? family)
		{
			IEnumerable<Tech> source = 
				from x in ai.Game.GameDatabase.GetPlayerTechInfos(ai.Player.ID)
				where AIResearchFramework.AIIsTechAvailable(x.State)
				select x into y
				select ai.Game.AssetDatabase.MasterTechTree.Technologies.First((Tech z) => y.TechFileID == z.Id);
			return (
				from x in source
				where AIResearchFramework.AIContainsTech(mode, family, x)
				select x).ToList<Tech>();
		}
		private static int AICalcTechCost(double s, double r, double n)
		{
			double num = s * r * n;
			return (int)num;
		}
		private static AIResearchModes AIGetResearchMode(AIResearchFramework.AIResearchRhythm rhythm, int age)
		{
			return rhythm.Beats[age % rhythm.Beats.Length];
		}
		private AIStance AIGetStance(StrategicAI ai)
		{
			return ai.Game.GameDatabase.GetAIInfo(ai.Player.ID).Stance;
		}
		private AIResearchModes AIGetResearchMode(StrategicAI ai)
		{
			AIStance key = this.AIGetStance(ai);
			AIResearchFramework.AIResearchRhythm rhythm = this._rhythms[key];
			int age = this.AIGetPhase(ai);
			return AIResearchFramework.AIGetResearchMode(rhythm, age);
		}
		private static bool AITechStyleContains(StrategicAI ai, AITechStyleInfo style, Tech tech)
		{
			TechFamilies techFamilyEnum = ai.Game.AssetDatabase.MasterTechTree.GetTechFamilyEnum(tech);
			return style.TechFamily == techFamilyEnum;
		}
		private static double AICalcTechStyleCost(StrategicAI ai, AITechStyleInfo style, Tech tech)
		{
			if (AIResearchFramework.AITechStyleContains(ai, style, tech))
			{
				return (double)style.CostFactor;
			}
			return 1.0;
		}
		private static double AICalcCombinedTechStyleCost(StrategicAI ai, Tech tech)
		{
			double num = 1.0;
			foreach (AITechStyleInfo current in ai.TechStyles.TechStyleInfos)
			{
				num *= AIResearchFramework.AICalcTechStyleCost(ai, current, tech);
			}
			return num;
		}
		private static int AIGetTurnsToComplete(StrategicAI ai, Tech tech)
		{
			PlayerTechInfo playerTechInfo = ai.Game.GameDatabase.GetPlayerTechInfo(ai.Player.ID, ai.Game.GameDatabase.GetTechID(tech.Id));
			ai.Game.GameDatabase.GetPlayerInfo(ai.Player.ID);
			int cachedAvailableResearchPointsPerTurn = ai.CachedAvailableResearchPointsPerTurn;
			int? num = GameSession.CalculateTurnsToCompleteResearch(playerTechInfo.ResearchCost, playerTechInfo.Progress, cachedAvailableResearchPointsPerTurn);
			if (!num.HasValue)
			{
				return 2147483647;
			}
			return num.Value;
		}
		private static int AICalcTechCost(StrategicAI ai, Tech tech)
		{
			double s = AIResearchFramework.AICalcCombinedTechStyleCost(ai, tech);
			double r = (double)tech.AICostFactors.Faction(ai.Player.Faction.Name);
			double n = (double)AIResearchFramework.AIGetTurnsToComplete(ai, tech);
			return AIResearchFramework.AICalcTechCost(s, r, n);
		}
		private static Tech AISelectFavoriteTech(StrategicAI ai, IList<Tech> techs)
		{
			if (techs.Count == 0)
			{
				return null;
			}
			List<Tech> list = new List<Tech>();
			int num = AIResearchFramework.AICalcTechCost(ai, techs[0]);
			foreach (Tech current in techs)
			{
				if (num == AIResearchFramework.AICalcTechCost(ai, current))
				{
					list.Add(current);
				}
				else
				{
					if (AIResearchFramework.AIGetTurnsToComplete(ai, current) <= 15)
					{
						list.Add(current);
						break;
					}
					break;
				}
			}
			return list[ai.Random.Next(list.Count)];
		}
		private static char AIGetResearchModeSymbol(AIResearchModes mode)
		{
			switch (mode)
			{
			case AIResearchModes.Empire:
				return 'E';
			case AIResearchModes.Engine:
				return 'N';
			case AIResearchModes.Weapon:
				return 'W';
			case AIResearchModes.Expansion:
				return 'X';
			default:
				return ' ';
			}
		}
		private Tech AISelectDefaultTechPass(StrategicAI ai, AIResearchModes? mode, TechFamilies? family)
		{
			List<Tech> list = (
				from x in AIResearchFramework.AISelectAvailableTechs(ai, mode, family)
				orderby AIResearchFramework.AICalcTechCost(ai, x)
				select x).ToList<Tech>();
			if (list.Count > 0)
			{
				list.Insert(0, list[0]);
			}
			if (this._log != null)
			{
				string arg = mode.HasValue ? string.Format("{0}/{1}", AIResearchFramework.AIGetResearchModeSymbol(mode.Value), ai.Game.GameDatabase.GetAIInfo(ai.Player.ID).Stance.ToString()) : "ALL";
				this._log.Print(string.Format(" {0} prospects: ", arg));
				foreach (Tech current in list)
				{
					int num = AIResearchFramework.AICalcTechCost(ai, current);
					this._log.Print(string.Format("{0} ({1}); ", current.Id, num));
				}
			}
			return AIResearchFramework.AISelectFavoriteTech(ai, list);
		}
		private static Tech AIGetResearchingTech(StrategicAI ai)
		{
			int playerResearchingTechID = ai.Game.GameDatabase.GetPlayerResearchingTechID(ai.Player.ID);
			if (playerResearchingTechID == 0)
			{
				return null;
			}
			string fileId = ai.Game.GameDatabase.GetTechFileID(playerResearchingTechID);
			return ai.Game.AssetDatabase.MasterTechTree.Technologies.First((Tech x) => x.Id == fileId);
		}
		private IEnumerable<AIResearchModes> AIGetRhythm(AIStance stance)
		{
			return this._rhythms[stance].Beats;
		}
		private List<AIResearchModes> AIGetPhasedRhythm(IEnumerable<AIResearchModes> rhythm, int phase)
		{
			int num = phase % rhythm.Count<AIResearchModes>();
			return rhythm.Skip(num).Take(rhythm.Count<AIResearchModes>() - num).Concat(rhythm.Take(num)).ToList<AIResearchModes>();
		}
		private IEnumerable<Tech> AIGetAvailableTechs(StrategicAI ai)
		{
			return 
				from x in ai.Game.GameDatabase.GetPlayerTechInfos(ai.Player.ID)
				where AIResearchFramework.AIIsTechAvailable(x.State)
				select x into y
				select ai.Game.AssetDatabase.MasterTechTree.Technologies.First((Tech z) => y.TechFileID == z.Id);
		}
		private IEnumerable<AIResearchFramework.TechBeat> AIGetTechBeats(IEnumerable<Tech> techs, List<AIResearchModes> rhythm, List<PlayerTechInfo> desiredTech = null)
		{
			foreach (Tech tech in techs)
			{
				for (int i = 0; i < rhythm.Count; i++)
				{
					if (AIResearchFramework.AIContainsTech(new AIResearchModes?(rhythm[i]), null, tech))
					{
						if (desiredTech != null)
						{
							desiredTech.Any((PlayerTechInfo x) => x.TechFileID == tech.Id);
						}
						yield return new AIResearchFramework.TechBeat
						{
							Tech = tech,
							Beat = i
						};
					}
				}
			}
			yield break;
		}
		private bool AIIsShortTermTech(StrategicAI ai, Tech tech)
		{
			return AIResearchFramework.AIGetTurnsToComplete(ai, tech) < 15;
		}
		private IEnumerable<AIResearchFramework.TechBeat> AIGetProspects(StrategicAI ai, IEnumerable<AIResearchFramework.TechBeat> techs)
		{
			return techs.OrderBy(delegate(AIResearchFramework.TechBeat x)
			{
				if (!this.AIIsShortTermTech(ai, x.Tech))
				{
					return 1;
				}
				return 0;
			}).ThenBy((AIResearchFramework.TechBeat y) => y.Beat).ThenBy((AIResearchFramework.TechBeat z) => AIResearchFramework.AICalcTechCost(ai, z.Tech));
		}
		private List<AIResearchFramework.TechBeat> AIGetCulledProspects(StrategicAI ai, IEnumerable<AIResearchFramework.TechBeat> sorted)
		{
			int num = 0;
			double num2 = 0.0;
			List<AIResearchFramework.TechBeat> list = new List<AIResearchFramework.TechBeat>();
			foreach (AIResearchFramework.TechBeat current in sorted)
			{
				list.Add(current);
				if (num == 0)
				{
					list.Add(current);
					num2 = (double)AIResearchFramework.AICalcTechCost(ai, current.Tech);
				}
				if ((double)AIResearchFramework.AICalcTechCost(ai, current.Tech) != num2)
				{
					break;
				}
				num++;
			}
			return list;
		}
		private void LogProspects(StrategicAI ai, IList<AIResearchModes> phasedRhythm, IEnumerable<AIResearchFramework.TechBeat> prospects)
		{
			if (this._log != null)
			{
				foreach (AIResearchFramework.TechBeat current in prospects)
				{
					this._log.Print(string.Format("{0}/{1} ({2}{3}); ", new object[]
					{
						AIResearchFramework.AIGetResearchModeSymbol(phasedRhythm[current.Beat]),
						current.Tech.Id,
						AIResearchFramework.AICalcTechCost(ai, current.Tech),
						this.AIIsShortTermTech(ai, current.Tech) ? "" : "*"
					}));
				}
			}
		}
		private Tech AISelectDefaultTech(StrategicAI ai, List<PlayerTechInfo> desiredTech = null, Dictionary<string, int> familyWeights = null)
		{
			if (this._log != null)
			{
				this._log.Print("{");
				bool flag = true;
				foreach (AITechStyleInfo current in ai.TechStyles.TechStyleInfos)
				{
					if (!flag)
					{
						this._log.Print(",");
					}
					else
					{
						flag = false;
					}
					this._log.Print(current.TechFamily.ToString());
				}
				this._log.Print("}");
			}
			Tech tech = null;
			if (AIResearchFramework.AIGetResearchingTech(ai) == null && tech == null)
			{
				AIStance aIStance = this.AIGetStance(ai);
				int num = this.AIGetPhase(ai);
				IEnumerable<AIResearchModes> rhythm = this.AIGetRhythm(aIStance);
				List<AIResearchModes> list = this.AIGetPhasedRhythm(rhythm, num);
				IEnumerable<AIResearchFramework.TechBeat> enumerable = this.AIGetProspects(ai, this.AIGetTechBeats(this.AIGetAvailableTechs(ai), list, null));
				List<AIResearchFramework.TechBeat> list2 = this.AIGetCulledProspects(ai, enumerable);
				if (this._log != null)
				{
					this._log.Print(string.Format(" (phase {0}) {1}/{2} prospects: ", num, AIResearchFramework.AIGetResearchModeSymbol(list[0]), aIStance));
					this.LogProspects(ai, list, list2);
					if (App.Log.Level >= LogLevel.Verbose)
					{
						this._log.Print(string.Format(" ... (phase {0}) {1}/{2} ALL prospects: ", num, AIResearchFramework.AIGetResearchModeSymbol(list[0]), aIStance));
						this.LogProspects(ai, list, enumerable);
					}
				}
				if (list2.Any<AIResearchFramework.TechBeat>())
				{
					if (desiredTech != null)
					{
						if (list2.Any((AIResearchFramework.TechBeat x) => desiredTech.Any((PlayerTechInfo y) => y.TechFileID == x.Tech.Id)))
						{
							list2.RemoveAll((AIResearchFramework.TechBeat x) => !desiredTech.Any((PlayerTechInfo y) => y.TechFileID == x.Tech.Id));
						}
					}
					if (familyWeights != null)
					{
						List<Weighted<Tech>> list3 = new List<Weighted<Tech>>();
						foreach (Tech current2 in (
							from x in list2
							select x.Tech).ToList<Tech>())
						{
							int weight;
							if (familyWeights.TryGetValue(current2.Family, out weight))
							{
								list3.Add(new Weighted<Tech>
								{
									Value = current2,
									Weight = weight
								});
							}
						}
						if (list3.Count > 0)
						{
							tech = WeightedChoices.Choose<Tech>(ai.Random, list3);
						}
					}
					if (tech == null)
					{
						tech = ai.Random.Choose(list2).Tech;
					}
				}
			}
			return tech;
		}
		private static int AIGetTechProgress(StrategicAI ai, Tech tech)
		{
			PlayerTechInfo playerTechInfo = AIResearchFramework.AIGetPlayerTechInfo(ai, tech);
			if (playerTechInfo == null)
			{
				return 0;
			}
			return playerTechInfo.Progress;
		}
		private Tech AISelectPartialTech(StrategicAI ai)
		{
			Tech tech = null;
			if (AIResearchFramework.AIGetResearchingTech(ai) == null)
			{
				List<Tech> list = AIResearchFramework.AISelectAvailableTechs(ai, null, null);
				if (list.Count > 0)
				{
					int num = 0;
					foreach (Tech current in list)
					{
						int num2 = AIResearchFramework.AIGetTechProgress(ai, current);
						if (num2 > num)
						{
							num = num2;
							tech = current;
						}
					}
				}
			}
			if (this._log != null && tech != null)
			{
				this._log.Print(string.Format("Resuming {0}", tech.Id));
			}
			return tech;
		}
		private static bool AICanSelectBinaryResponseTech(StrategicAI ai)
		{
			Tech tech = AIResearchFramework.AIGetResearchingTech(ai);
			return tech == null || AIResearchFramework.AIGetTurnsToComplete(ai, tech) > 5;
		}
		private static Tech AISelectBinaryResponseTech(StrategicAI ai)
		{
			Tech result = null;
			AIResearchFramework.AICanSelectBinaryResponseTech(ai);
			return result;
		}
		private Tech AIBaseSelectNextTech(StrategicAI ai, List<PlayerTechInfo> desiredTech = null, Dictionary<string, int> familyWeights = null)
		{
			Tech tech = null;
			if (tech == null)
			{
				tech = this.AISelectPartialTech(ai);
			}
			if (tech == null)
			{
				tech = AIResearchFramework.AISelectBinaryResponseTech(ai);
			}
			if (tech == null)
			{
				tech = this.AISelectDefaultTech(ai, desiredTech, familyWeights);
			}
			Tech tech2 = AIResearchFramework.AIGetResearchingTech(ai);
			if (tech != null && tech != tech2)
			{
				if (this._log != null)
				{
					if (tech2 != null)
					{
						this._log.Print(string.Format("\n          >>> {0} (replacing {1})\n", tech.Id, tech2.Id));
					}
					else
					{
						this._log.Print(string.Format("\n          >>> {0}\n", tech.Id));
					}
				}
				return tech;
			}
			if (this._log != null)
			{
				this._log.ClearRecord();
			}
			return null;
		}
		private void PrepareLog()
		{
			if (this._log == null && ScriptHost.AllowConsole)
			{
				this._log = new AIResearchFramework.AITechLog();
			}
		}
		public Tech AISelectNextTech(StrategicAI ai, List<PlayerTechInfo> desiredTech = null, Dictionary<string, int> familyWeights = null)
		{
			this.PrepareLog();
			if (this._log != null)
			{
				this._log.BeginTurn(ai);
			}
			Tech result = null;
			try
			{
				result = this.AIBaseSelectNextTech(ai, desiredTech, familyWeights);
			}
			finally
			{
				if (this._log != null)
				{
					this._log.EndTurn();
				}
			}
			return result;
		}
		internal void TestTechStyleSelection(string factionName, int iterations)
		{
			Random random = new Random();
			for (int i = 0; i < iterations; i++)
			{
				App.Log.Trace(string.Concat(new object[]
				{
					"--- TestTechStyleSelection (",
					factionName,
					") ",
					i + 1,
					"/",
					iterations
				}), "ai");
				int playerId = 1;
				List<AITechStyleInfo> list = this.AISelectTechStylesCore(playerId, random, factionName);
				foreach (AITechStyleInfo current in list)
				{
					App.Log.Trace(current.ToString(), "ai");
				}
			}
		}
		private List<AITechStyleInfo> AISelectTechStylesCore(int playerId, Random random, string factionName)
		{
			this.PrepareLog();
			List<AITechStyleInfo> list = new List<AITechStyleInfo>();
			foreach (AIResearchFramework.AITechStyleGroup current in this._styleGroups)
			{
				float num = 0f;
				List<Weighted<AIResearchFramework.AITechStyleRow>> list2 = new List<Weighted<AIResearchFramework.AITechStyleRow>>();
				foreach (AIResearchFramework.AITechStyleRow current2 in current.Styles)
				{
					float num2 = current2.SelectionChances.Faction(factionName);
					num += num2;
					list2.Add(new Weighted<AIResearchFramework.AITechStyleRow>(current2, (int)(num2 * 1000f)));
				}
				float num3 = 100f - num;
				if (num3 >= 0.001f)
				{
					list2.Add(new Weighted<AIResearchFramework.AITechStyleRow>(null, (int)(num3 * 1000f)));
				}
				AIResearchFramework.AITechStyleRow aITechStyleRow = WeightedChoices.Choose<AIResearchFramework.AITechStyleRow>(random, list2);
				if (aITechStyleRow != null)
				{
					list.AddRange(aITechStyleRow.ToTechStyleInfos(playerId, factionName));
				}
			}
			return list;
		}
		public List<AITechStyleInfo> AISelectTechStyles(StrategicAI ai, Faction faction)
		{
			return this.AISelectTechStylesCore(ai.Player.ID, ai.Random, faction.Name);
		}
	}
}
