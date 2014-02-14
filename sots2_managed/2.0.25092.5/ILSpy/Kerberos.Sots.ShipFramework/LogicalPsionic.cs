using Kerberos.Sots.Data;
using Kerberos.Sots.Data.ShipFramework;
using System;
namespace Kerberos.Sots.ShipFramework
{
	internal class LogicalPsionic
	{
		public string Name = "";
		public SectionEnumerations.PsionicAbility Ability;
		public string Model = "";
		public string PsionicTitle = "";
		public string Description = "";
		public string Icon = "";
		public int MinPower;
		public int MaxPower;
		public int BaseCost;
		public float Range;
		public float BaseDamage;
		public LogicalEffect CastorEffect;
		public LogicalEffect CastEffect;
		public LogicalEffect ApplyEffect;
		public string RequiredTechID;
		public bool RequiresSuulka;
		public bool IsAvailable(GameDatabase db, int playerId, bool forSuulka)
		{
			if (this.Ability == SectionEnumerations.PsionicAbility.AbaddonLaser)
			{
				return false;
			}
			if (!forSuulka && this.RequiresSuulka)
			{
				return false;
			}
			if (!string.IsNullOrEmpty(this.RequiredTechID))
			{
				PlayerTechInfo playerTechInfo = db.GetPlayerTechInfo(playerId, db.GetTechID(this.RequiredTechID));
				if (playerTechInfo == null || playerTechInfo.State != TechStates.Researched)
				{
					return false;
				}
			}
			return true;
		}
	}
}
