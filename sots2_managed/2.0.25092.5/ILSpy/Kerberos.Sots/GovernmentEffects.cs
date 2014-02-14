using Kerberos.Sots.Data;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
namespace Kerberos.Sots
{
	internal class GovernmentEffects
	{
		private Dictionary<GovernmentInfo.GovernmentType, GovEffectCollection> DiplomaticEffects;
		public GovernmentEffects()
		{
			this.DiplomaticEffects = new Dictionary<GovernmentInfo.GovernmentType, GovEffectCollection>();
		}
		public void LoadFromFile(XmlDocument govEffects)
		{
			if (govEffects == null)
			{
				return;
			}
			XmlElement xmlElement = govEffects["GovernmentEffects"];
			if (xmlElement == null)
			{
				return;
			}
			this.DiplomaticEffects.Clear();
			foreach (GovernmentInfo.GovernmentType governmentType in Enum.GetValues(typeof(GovernmentInfo.GovernmentType)))
			{
				XmlElement xmlElement2 = xmlElement[governmentType.ToString()];
				if (xmlElement2 != null)
				{
					GovEffectCollection value = default(GovEffectCollection);
					value.DiplomaticEffects = new GovDiplomaticEffects();
					value.MoralEffects = new GovMoralEffects();
					value.StratEffects = new GovStratModifiers();
					this.DiplomaticEffects.Add(governmentType, value);
					this.DiplomaticEffects[governmentType].DiplomaticEffects.LoadDiplmaticEffects(xmlElement2["DiplomaticEffects"]);
					this.DiplomaticEffects[governmentType].MoralEffects.LoadMoralEffects(xmlElement2["MoralEffects"]);
					this.DiplomaticEffects[governmentType].StratEffects.LoadStratEffects(xmlElement2["StratModifiers"]);
				}
			}
		}
		public float GetDiplomacyBonus(GameDatabase gamedb, AssetDatabase assetdb, PlayerInfo player, PlayerInfo toPlayer)
		{
			GovernmentInfo governmentInfo = gamedb.GetGovernmentInfo(player.ID);
			GovEffectCollection govEffectCollection;
			if (governmentInfo != null && this.DiplomaticEffects.TryGetValue(governmentInfo.CurrentType, out govEffectCollection))
			{
				return govEffectCollection.DiplomaticEffects.GetDiplomaticBonusBetweenGovernmentTypes(gamedb, assetdb, player, toPlayer);
			}
			return 0f;
		}
		public int GetMoralTotal(GameDatabase gamedb, GovernmentInfo.GovernmentType gt, MoralEvent me, int player, int moral)
		{
			GovEffectCollection govEffectCollection;
			if (this.DiplomaticEffects.TryGetValue(gt, out govEffectCollection))
			{
				return govEffectCollection.MoralEffects.GetResultingMoral(gamedb, me, player, moral);
			}
			return moral;
		}
		public int GetStratModifierTotal(GameDatabase gamedb, StratModifiers sm, int playerId, int modValue)
		{
			GovernmentInfo governmentInfo = gamedb.GetGovernmentInfo(playerId);
			GovEffectCollection govEffectCollection;
			if (governmentInfo != null && this.DiplomaticEffects.TryGetValue(governmentInfo.CurrentType, out govEffectCollection))
			{
				return govEffectCollection.StratEffects.GetResultingStratModifierValue(gamedb, sm, playerId, modValue);
			}
			return modValue;
		}
		public float GetStratModifierTotal(GameDatabase gamedb, StratModifiers sm, int playerId, float modValue)
		{
			GovernmentInfo governmentInfo = gamedb.GetGovernmentInfo(playerId);
			GovEffectCollection govEffectCollection;
			if (governmentInfo != null && this.DiplomaticEffects.TryGetValue(governmentInfo.CurrentType, out govEffectCollection))
			{
				return govEffectCollection.StratEffects.GetResultingStratModifierValue(gamedb, sm, playerId, modValue);
			}
			return modValue;
		}
		public static bool IsPlayerAtWar(GameDatabase gamedb, int player)
		{
			bool result = false;
			List<int> list = gamedb.GetStandardPlayerIDs().ToList<int>();
			foreach (int current in list)
			{
				if (current != player && gamedb.GetDiplomacyStateBetweenPlayers(player, current) == DiplomacyState.WAR)
				{
					result = true;
					break;
				}
			}
			return result;
		}
	}
}
