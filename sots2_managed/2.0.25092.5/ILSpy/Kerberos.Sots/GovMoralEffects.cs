using Kerberos.Sots.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
namespace Kerberos.Sots
{
	internal class GovMoralEffects
	{
		private struct MoralEffect
		{
			public MoralCondition Condition;
			public float Value;
		}
		private Dictionary<MoralEvent, Dictionary<EffectAvailability, GovMoralEffects.MoralEffect>> MoralEffects;
		private Dictionary<EffectAvailability, GovMoralEffects.MoralEffect> AllMoralEffects;
		public GovMoralEffects()
		{
			this.MoralEffects = new Dictionary<MoralEvent, Dictionary<EffectAvailability, GovMoralEffects.MoralEffect>>();
			this.AllMoralEffects = new Dictionary<EffectAvailability, GovMoralEffects.MoralEffect>();
		}
		public void LoadMoralEffects(XmlElement moralEffects)
		{
			if (moralEffects == null)
			{
				return;
			}
			this.MoralEffects.Clear();
			this.AllMoralEffects.Clear();
			this.LoadEffects(EffectAvailability.All, moralEffects);
			this.LoadEffects(EffectAvailability.War, moralEffects["War"]);
			this.LoadEffects(EffectAvailability.Peace, moralEffects["Peace"]);
		}
		public void LoadEffects(EffectAvailability availability, XmlElement moralEffects)
		{
			if (moralEffects == null)
			{
				return;
			}
			XmlElement xmlElement = moralEffects["ALL"];
			if (xmlElement != null)
			{
				if (xmlElement.HasAttributes)
				{
					GovMoralEffects.MoralEffect value = default(GovMoralEffects.MoralEffect);
					string attribute = xmlElement.GetAttribute("condition");
					string attribute2 = xmlElement.GetAttribute("value");
					if (attribute == "all")
					{
						value.Condition = MoralCondition.All;
					}
					else
					{
						if (attribute == "negative")
						{
							value.Condition = MoralCondition.Negative;
						}
						else
						{
							if (attribute == "positive")
							{
								value.Condition = MoralCondition.Positive;
							}
							else
							{
								value.Condition = MoralCondition.None;
							}
						}
					}
					value.Value = (string.IsNullOrEmpty(attribute2) ? 0f : float.Parse(attribute2));
					this.AllMoralEffects.Add(availability, value);
				}
				else
				{
					string innerText = xmlElement.InnerText;
					GovMoralEffects.MoralEffect value2 = default(GovMoralEffects.MoralEffect);
					if (innerText == "immune")
					{
						value2.Condition = MoralCondition.All;
					}
					else
					{
						if (innerText == "immune_neg")
						{
							value2.Condition = MoralCondition.Negative;
						}
						else
						{
							if (innerText == "immune_pos")
							{
								value2.Condition = MoralCondition.Positive;
							}
							else
							{
								value2.Condition = MoralCondition.None;
							}
						}
					}
					value2.Value = ((value2.Condition != MoralCondition.None) ? 0f : float.Parse(innerText));
					this.AllMoralEffects.Add(availability, value2);
				}
			}
			foreach (MoralEvent moralEvent in Enum.GetValues(typeof(MoralEvent)))
			{
				xmlElement = moralEffects[moralEvent.ToString()];
				if (xmlElement != null)
				{
					if (xmlElement.HasAttributes)
					{
						GovMoralEffects.MoralEffect value3 = default(GovMoralEffects.MoralEffect);
						string attribute3 = xmlElement.GetAttribute("immune");
						string attribute4 = xmlElement.GetAttribute("value");
						if (attribute3 == "all")
						{
							value3.Condition = MoralCondition.All;
						}
						else
						{
							if (attribute3 == "negative")
							{
								value3.Condition = MoralCondition.Negative;
							}
							else
							{
								if (attribute3 == "positive")
								{
									value3.Condition = MoralCondition.Positive;
								}
								else
								{
									value3.Condition = MoralCondition.None;
								}
							}
						}
						value3.Value = (string.IsNullOrEmpty(attribute4) ? 0f : float.Parse(attribute4));
						this.MoralEffects.Add(moralEvent, new Dictionary<EffectAvailability, GovMoralEffects.MoralEffect>
						{

							{
								availability,
								value3
							}
						});
					}
					else
					{
						string innerText2 = xmlElement.InnerText;
						GovMoralEffects.MoralEffect value4 = default(GovMoralEffects.MoralEffect);
						if (innerText2 == "immune")
						{
							value4.Condition = MoralCondition.All;
						}
						else
						{
							if (innerText2 == "immune_neg")
							{
								value4.Condition = MoralCondition.Negative;
							}
							else
							{
								if (innerText2 == "immune_pos")
								{
									value4.Condition = MoralCondition.Positive;
								}
								else
								{
									value4.Condition = MoralCondition.None;
								}
							}
						}
						value4.Value = ((value4.Condition != MoralCondition.None) ? 0f : float.Parse(innerText2));
						this.MoralEffects.Add(moralEvent, new Dictionary<EffectAvailability, GovMoralEffects.MoralEffect>
						{

							{
								availability,
								value4
							}
						});
					}
				}
			}
		}
		private int GetValueWithImmunity(GovMoralEffects.MoralEffect effect, int moral)
		{
			switch (effect.Condition)
			{
			case MoralCondition.All:
				moral = 0;
				return moral;
			case MoralCondition.Negative:
				if (moral < 0)
				{
					moral = ((effect.Value == 0f) ? 0 : (moral + (int)((float)moral * effect.Value)));
					return moral;
				}
				return moral;
			case MoralCondition.Positive:
				if (moral > 0)
				{
					moral = ((effect.Value == 0f) ? 0 : (moral + (int)((float)moral * effect.Value)));
					return moral;
				}
				return moral;
			}
			moral += (int)((float)moral * effect.Value);
			return moral;
		}
		public int GetResultingMoral(GameDatabase gamedb, MoralEvent me, int player, int moral)
		{
			bool flag = false;
			bool flag2 = false;
			if (this.AllMoralEffects.Keys.Any((EffectAvailability x) => x == EffectAvailability.Peace || x == EffectAvailability.War))
			{
				flag = GovernmentEffects.IsPlayerAtWar(gamedb, player);
				flag2 = true;
				if (flag && this.AllMoralEffects.ContainsKey(EffectAvailability.War))
				{
					moral = this.GetValueWithImmunity(this.AllMoralEffects[EffectAvailability.War], moral);
				}
				else
				{
					if (!flag && this.AllMoralEffects.ContainsKey(EffectAvailability.Peace))
					{
						moral = this.GetValueWithImmunity(this.AllMoralEffects[EffectAvailability.Peace], moral);
					}
				}
			}
			Dictionary<EffectAvailability, GovMoralEffects.MoralEffect> dictionary = new Dictionary<EffectAvailability, GovMoralEffects.MoralEffect>();
			if (this.MoralEffects.TryGetValue(me, out dictionary))
			{
				if (dictionary.Keys.Any((EffectAvailability x) => x == EffectAvailability.Peace || x == EffectAvailability.War))
				{
					if (!flag2)
					{
						flag = GovernmentEffects.IsPlayerAtWar(gamedb, player);
					}
					if (flag && dictionary.ContainsKey(EffectAvailability.War))
					{
						moral = this.GetValueWithImmunity(dictionary[EffectAvailability.War], moral);
					}
					else
					{
						if (!flag && dictionary.ContainsKey(EffectAvailability.Peace))
						{
							moral = this.GetValueWithImmunity(dictionary[EffectAvailability.Peace], moral);
						}
					}
				}
				else
				{
					if (dictionary.ContainsKey(EffectAvailability.All))
					{
						moral = this.GetValueWithImmunity(dictionary[EffectAvailability.All], moral);
					}
				}
			}
			return moral;
		}
	}
}
