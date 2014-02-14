using Kerberos.Sots.Framework;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.Data
{
	internal class PlayerInfo : IIDProvider
	{
		public string Name;
		public int FactionID;
		public int SubfactionIndex;
		public Vector3 PrimaryColor;
		public Vector3 SecondaryColor;
		public string BadgeAssetPath;
		public string AvatarAssetPath;
		public int? Homeworld;
		public double Savings;
		public int LastCombatTurn;
		public int LastEncounterTurn;
		public AIDifficulty AIDifficulty;
		public double ResearchBoostFunds;
		public bool AutoPlaceDefenseAssets;
		public bool AutoRepairShips;
		public bool AutoUseGoopModules;
		public bool AutoUseJokerModules;
		public bool AutoAoe;
		public int Team;
		public bool AutoPatrol;
		public float RateGovernmentResearch;
		public float RateResearchCurrentProject;
		public float RateResearchSpecialProject;
		public float RateResearchSalvageResearch;
		public float RateGovernmentStimulus;
		public float RateGovernmentSecurity;
		public float RateGovernmentSavings;
		public float RateStimulusMining;
		public float RateStimulusColonization;
		public float RateStimulusTrade;
		public float RateSecurityOperations;
		public float RateSecurityIntelligence;
		public float RateSecurityCounterIntelligence;
		public bool isStandardPlayer = true;
		public int GenericDiplomacyPoints;
		public float RateTax;
		public float RateTaxPrev;
		public float RateImmigration;
		public int IntelPoints;
		public int CounterIntelPoints;
		public int OperationsPoints;
		public int IntelAccumulator;
		public int CounterIntelAccumulator;
		public int OperationsAccumulator;
		public int CivilianMiningAccumulator;
		public int CivilianColonizationAccumulator;
		public int CivilianTradeAccumulator;
		public int AdditionalResearchPoints;
		public int PsionicPotential;
		public bool isDefeated;
		public double CurrentTradeIncome;
		public bool includeInDiplomacy;
		public bool isAIRebellionPlayer;
		public Dictionary<int, int> FactionDiplomacyPoints = new Dictionary<int, int>();
		public int ID
		{
			get;
			set;
		}
		private static void Normalize3(ref float a, ref float b, ref float c)
		{
			float num = a + b + c;
			if (num < 1.401298E-45f)
			{
				a = 0f;
				b = 0f;
				c = 0f;
				return;
			}
			a /= num;
			b /= num;
			c /= num;
		}
		public void NormalizeRates()
		{
			PlayerInfo.Normalize3(ref this.RateGovernmentStimulus, ref this.RateGovernmentSecurity, ref this.RateGovernmentSavings);
			PlayerInfo.Normalize3(ref this.RateResearchCurrentProject, ref this.RateResearchSpecialProject, ref this.RateResearchSalvageResearch);
			PlayerInfo.Normalize3(ref this.RateStimulusMining, ref this.RateStimulusColonization, ref this.RateStimulusTrade);
			PlayerInfo.Normalize3(ref this.RateSecurityOperations, ref this.RateSecurityIntelligence, ref this.RateSecurityCounterIntelligence);
		}
		public int GetTotalDiplomacyPoints(int factionID)
		{
			return this.GenericDiplomacyPoints / 2 + (this.FactionDiplomacyPoints.ContainsKey(factionID) ? this.FactionDiplomacyPoints[factionID] : 0);
		}
		public override string ToString()
		{
			return string.Format("{0},{1}", this.ID, this.Name);
		}
		public void SetResearchRate(float value)
		{
			this.RateGovernmentResearch = 1f - value;
		}
		public float GetResearchRate()
		{
			return 1f - this.RateGovernmentResearch;
		}
		public bool IsOnTeam(PlayerInfo otherplayer)
		{
			return this.Team != 0 && this.Team == otherplayer.Team;
		}
		public bool CanDebtSpend(AssetDatabase assetdb)
		{
			return !(assetdb.GetFaction(this.FactionID).Name == "loa") || this.Savings > 0.0;
		}
	}
}
