using Kerberos.Sots.Data;
using Kerberos.Sots.Data.Xml;
using Kerberos.Sots.PlayerFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
namespace Kerberos.Sots
{
	internal class GovDiplomaticEffects
	{
		public float IndependantBonus;
		public float SameFactionBonus;
		public Dictionary<GovernmentInfo.GovernmentType, float> DiplomaticEffect;
		public Dictionary<GovernmentInfo.GovernmentType, float> NonPlayerDiplomaticEffect;
		public GovDiplomaticEffects()
		{
			this.DiplomaticEffect = new Dictionary<GovernmentInfo.GovernmentType, float>();
			this.NonPlayerDiplomaticEffect = new Dictionary<GovernmentInfo.GovernmentType, float>();
		}
		public void LoadDiplmaticEffects(XmlElement diploEffects)
		{
			if (diploEffects == null)
			{
				return;
			}
			this.DiplomaticEffect.Clear();
			this.NonPlayerDiplomaticEffect.Clear();
			foreach (GovernmentInfo.GovernmentType governmentType in Enum.GetValues(typeof(GovernmentInfo.GovernmentType)))
			{
				XmlElement xmlElement = diploEffects[governmentType.ToString()];
				if (xmlElement != null)
				{
					this.DiplomaticEffect.Add(governmentType, float.Parse(xmlElement.InnerText));
				}
			}
			XmlElement xmlElement2 = diploEffects["NonPlayer"];
			if (xmlElement2 != null)
			{
				if (xmlElement2.ChildNodes.Count == 0)
				{
					float num = float.Parse(xmlElement2.InnerText);
					IEnumerator enumerator2 = Enum.GetValues(typeof(GovernmentInfo.GovernmentType)).GetEnumerator();
					try
					{
						while (enumerator2.MoveNext())
						{
							GovernmentInfo.GovernmentType key = (GovernmentInfo.GovernmentType)enumerator2.Current;
							this.NonPlayerDiplomaticEffect.Add(key, num + this.DiplomaticEffect[key]);
						}
						goto IL_1A0;
					}
					finally
					{
						IDisposable disposable2 = enumerator2 as IDisposable;
						if (disposable2 != null)
						{
							disposable2.Dispose();
						}
					}
				}
				foreach (GovernmentInfo.GovernmentType governmentType2 in Enum.GetValues(typeof(GovernmentInfo.GovernmentType)))
				{
					XmlElement xmlElement3 = xmlElement2[governmentType2.ToString()];
					if (xmlElement3 != null)
					{
						this.NonPlayerDiplomaticEffect.Add(governmentType2, float.Parse(xmlElement3.InnerText) + this.DiplomaticEffect[governmentType2]);
					}
				}
			}
			IL_1A0:
			this.IndependantBonus = XmlHelper.GetData<float>(diploEffects, "Independant");
			this.SameFactionBonus = XmlHelper.GetData<float>(diploEffects, "SameFaction");
		}
		public float GetDiplomaticBonusBetweenGovernmentTypes(GameDatabase gamedb, AssetDatabase assetdb, PlayerInfo player, PlayerInfo toPlayer)
		{
			float num = 0f;
			Faction faction = assetdb.Factions.FirstOrDefault((Faction x) => x.ID == player.FactionID);
			Faction faction2 = assetdb.Factions.FirstOrDefault((Faction x) => x.ID == toPlayer.FactionID);
			GovernmentInfo governmentInfo = gamedb.GetGovernmentInfo(toPlayer.ID);
			float num2;
			if (governmentInfo != null && this.DiplomaticEffect.TryGetValue(governmentInfo.CurrentType, out num2))
			{
				num += num2;
			}
			if (!toPlayer.isStandardPlayer)
			{
				if (faction2 != null && faction2.IsIndependent())
				{
					num += this.IndependantBonus;
				}
				else
				{
					if (this.NonPlayerDiplomaticEffect.TryGetValue(governmentInfo.CurrentType, out num2))
					{
						num += num2;
					}
				}
			}
			if (faction == faction2)
			{
				num += this.SameFactionBonus;
			}
			return num;
		}
	}
}
