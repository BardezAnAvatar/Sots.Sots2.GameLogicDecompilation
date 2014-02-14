using Kerberos.Sots.Framework;
using Kerberos.Sots.Strategy;
using System;
namespace Kerberos.Sots.Data
{
	internal class DiplomacyInfo : IIDProvider
	{
		public int PlayerID;
		public int TowardsPlayerID;
		public DiplomacyState State;
		public bool isEncountered;
		public static int DefaultDeplomacyRelations = 1000;
		public static int MinDeplomacyRelations = 0;
		public static int MaxDeplomacyRelations = 2000;
		private int _relations;
		public int ID
		{
			get;
			set;
		}
		public int Relations
		{
			get
			{
				return this._relations;
			}
			set
			{
				this._relations = value;
				this._relations.Clamp(DiplomacyInfo.MinDeplomacyRelations, DiplomacyInfo.MaxDeplomacyRelations);
			}
		}
		public DiplomaticMood GetDiplomaticMood()
		{
			if (this.Relations <= 200)
			{
				return DiplomaticMood.Hatred;
			}
			if (this.Relations <= 600)
			{
				return DiplomaticMood.Hostility;
			}
			if (this.Relations <= 900)
			{
				return DiplomaticMood.Distrust;
			}
			if (this.Relations <= 1100)
			{
				return DiplomaticMood.Indifference;
			}
			if (this.Relations <= 1400)
			{
				return DiplomaticMood.Trust;
			}
			if (this.Relations <= 1800)
			{
				return DiplomaticMood.Friendship;
			}
			return DiplomaticMood.Love;
		}
		public static string GetDiplomaticMoodSprite(DiplomaticMood mood)
		{
			if (mood == DiplomaticMood.Hatred)
			{
				return "Hate";
			}
			if (mood == DiplomaticMood.Love)
			{
				return "Love";
			}
			return null;
		}
		public string GetDiplomaticMoodSprite()
		{
			return DiplomacyInfo.GetDiplomaticMoodSprite(this.GetDiplomaticMood());
		}
	}
}
