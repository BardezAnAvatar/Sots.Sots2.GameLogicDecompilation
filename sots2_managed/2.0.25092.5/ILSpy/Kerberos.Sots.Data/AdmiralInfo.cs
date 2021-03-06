using System;
namespace Kerberos.Sots.Data
{
	internal class AdmiralInfo : IIDProvider
	{
		public enum TraitType
		{
			Thrifty,
			Wastrel,
			Pathfinder,
			Slippery,
			Livingstone,
			Conscript,
			TrueBeliever,
			GoodShepherd,
			BadShepherd,
			GreenThumb,
			BlackThumb,
			DrillSergeant,
			Vigilant,
			Architect,
			Inquisitor,
			Evangelist,
			HeadHunter,
			TrueGrit,
			Hunter,
			Defender,
			Attacker,
			ArtilleryExpert,
			Psion,
			Skeptic,
			MediaHero,
			GloryHound,
			Sherman,
			Technophobe,
			Elite
		}
		public int PlayerID;
		public int? HomeworldID;
		public string Name;
		public string Race;
		public float Age;
		public string Gender;
		public int ReactionBonus;
		public int EvasionBonus;
		public int Loyalty;
		public int BattlesFought;
		public int BattlesWon;
		public int MissionsAssigned;
		public int MissionsAccomplished;
		public int TurnCreated;
		public bool Engram;
		public int ID
		{
			get;
			set;
		}
		public static string GetTraitDescription(AdmiralInfo.TraitType t, int level)
		{
			return string.Format(App.Localize(string.Format("@ADMIRALTRAITS_{0}_DESC", t.ToString().ToUpper())), level, 10 * level);
		}
		public static bool IsGoodTrait(AdmiralInfo.TraitType t)
		{
			switch (t)
			{
			case AdmiralInfo.TraitType.Thrifty:
			case AdmiralInfo.TraitType.Pathfinder:
			case AdmiralInfo.TraitType.Slippery:
			case AdmiralInfo.TraitType.TrueBeliever:
			case AdmiralInfo.TraitType.GoodShepherd:
			case AdmiralInfo.TraitType.GreenThumb:
			case AdmiralInfo.TraitType.DrillSergeant:
			case AdmiralInfo.TraitType.Vigilant:
			case AdmiralInfo.TraitType.Architect:
			case AdmiralInfo.TraitType.Inquisitor:
			case AdmiralInfo.TraitType.Evangelist:
			case AdmiralInfo.TraitType.HeadHunter:
			case AdmiralInfo.TraitType.TrueGrit:
			case AdmiralInfo.TraitType.Hunter:
			case AdmiralInfo.TraitType.Defender:
			case AdmiralInfo.TraitType.Attacker:
			case AdmiralInfo.TraitType.ArtilleryExpert:
			case AdmiralInfo.TraitType.Psion:
			case AdmiralInfo.TraitType.MediaHero:
			case AdmiralInfo.TraitType.Sherman:
			case AdmiralInfo.TraitType.Elite:
				return true;
			}
			return false;
		}
		public static bool AreTraitsMutuallyExclusive(AdmiralInfo.TraitType tA, AdmiralInfo.TraitType tB)
		{
			switch (tA)
			{
			case AdmiralInfo.TraitType.Pathfinder:
				return tB == AdmiralInfo.TraitType.Livingstone;
			case AdmiralInfo.TraitType.Livingstone:
				return tB == AdmiralInfo.TraitType.Pathfinder;
			case AdmiralInfo.TraitType.GoodShepherd:
				return tB == AdmiralInfo.TraitType.BadShepherd;
			case AdmiralInfo.TraitType.BadShepherd:
				return tB == AdmiralInfo.TraitType.GoodShepherd;
			case AdmiralInfo.TraitType.GreenThumb:
				return tB == AdmiralInfo.TraitType.BlackThumb;
			case AdmiralInfo.TraitType.BlackThumb:
				return tB == AdmiralInfo.TraitType.GreenThumb;
			case AdmiralInfo.TraitType.Psion:
				return tB == AdmiralInfo.TraitType.Skeptic;
			case AdmiralInfo.TraitType.Skeptic:
				return tB == AdmiralInfo.TraitType.Psion;
			}
			return false;
		}
		public static bool CanRaceHaveTrait(AdmiralInfo.TraitType t, string race)
		{
			if (t == AdmiralInfo.TraitType.Inquisitor)
			{
				return race == "hordezuul" || race == "presterzuul";
			}
			if (t == AdmiralInfo.TraitType.Evangelist)
			{
				return race == "liir" || race == "presterzuul";
			}
			if (t == AdmiralInfo.TraitType.HeadHunter)
			{
				return race == "morrigi";
			}
			if (t == AdmiralInfo.TraitType.TrueGrit)
			{
				return race == "human";
			}
			return (t != AdmiralInfo.TraitType.Psion && t != AdmiralInfo.TraitType.Skeptic && t != AdmiralInfo.TraitType.Technophobe) || race != "loa";
		}
		public string GetAdmiralSoundCueContext(AssetDatabase assetdb)
		{
			string race;
			switch (race = this.Race)
			{
			case "human":
			case "tarkas":
			case "morrigi":
			{
				string gender;
				if ((gender = this.Gender) != null)
				{
					if (gender == "male")
					{
						return "A_";
					}
					if (gender == "female")
					{
						return "B_";
					}
				}
				break;
			}
			case "hiver":
			case "hordezuul":
			case "presterzuul":
			case "liir":
				if (this.ID % 2 == 0)
				{
					return "A_";
				}
				return "B_";
			}
			return "";
		}
		public override string ToString()
		{
			return string.Format("ID={0},Name={1},Race={2}", this.ID, this.Name, this.Race.ToString());
		}
	}
}
