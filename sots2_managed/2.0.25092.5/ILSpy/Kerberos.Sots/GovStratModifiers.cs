using Kerberos.Sots.Data;
using System;
using System.Collections.Generic;
using System.Xml;
namespace Kerberos.Sots
{
	internal class GovStratModifiers
	{
		private Dictionary<StratModifiers, KeyValuePair<EffectAvailability, float>> StratEffects;
		public GovStratModifiers()
		{
			this.StratEffects = new Dictionary<StratModifiers, KeyValuePair<EffectAvailability, float>>();
		}
		public void LoadStratEffects(XmlElement stratEffects)
		{
			if (stratEffects == null)
			{
				return;
			}
			this.StratEffects.Clear();
			foreach (StratModifiers stratModifiers in Enum.GetValues(typeof(StratModifiers)))
			{
				XmlElement xmlElement = stratEffects[stratModifiers.ToString()];
				if (xmlElement != null)
				{
					if (xmlElement.HasAttributes)
					{
						this.StratEffects.Add(stratModifiers, new KeyValuePair<EffectAvailability, float>((xmlElement.GetAttribute("condition") == "war") ? EffectAvailability.War : EffectAvailability.Peace, float.Parse(xmlElement.GetAttribute("value"))));
					}
					else
					{
						this.StratEffects.Add(stratModifiers, new KeyValuePair<EffectAvailability, float>(EffectAvailability.All, float.Parse(xmlElement.InnerText)));
					}
				}
			}
		}
		public float GetResultingStratModifierValue(GameDatabase gamedb, StratModifiers sm, int player, float modValue)
		{
			KeyValuePair<EffectAvailability, float> keyValuePair;
			if (this.StratEffects.TryGetValue(sm, out keyValuePair))
			{
				if (keyValuePair.Key != EffectAvailability.All)
				{
					bool flag = GovernmentEffects.IsPlayerAtWar(gamedb, player);
					if ((flag && keyValuePair.Key == EffectAvailability.War) || (!flag && keyValuePair.Key == EffectAvailability.Peace))
					{
						modValue += keyValuePair.Value;
					}
				}
				else
				{
					modValue += keyValuePair.Value;
				}
			}
			return modValue;
		}
		public int GetResultingStratModifierValue(GameDatabase gamedb, StratModifiers sm, int player, int modValue)
		{
			KeyValuePair<EffectAvailability, float> keyValuePair;
			if (this.StratEffects.TryGetValue(sm, out keyValuePair))
			{
				if (keyValuePair.Key != EffectAvailability.All)
				{
					bool flag = GovernmentEffects.IsPlayerAtWar(gamedb, player);
					if ((flag && keyValuePair.Key == EffectAvailability.War) || (!flag && keyValuePair.Key == EffectAvailability.Peace))
					{
						modValue += (int)keyValuePair.Value;
					}
				}
				else
				{
					modValue += (int)keyValuePair.Value;
				}
			}
			return modValue;
		}
	}
}
