using Kerberos.Sots.Data.ShipFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
namespace Kerberos.Sots.ShipFramework
{
	internal static class SuulkaPsiBonusLibrary
	{
		public static IEnumerable<SuulkaPsiBonus> Enumerate(XmlDocument doc)
		{
			foreach (XmlElement current in 
				from x in doc["CommonAssets"].OfType<XmlElement>()
				where x.Name.Equals("SuulkaPsiBonuses", StringComparison.InvariantCulture)
				select x)
			{
				foreach (XmlElement current2 in current.OfType<XmlElement>())
				{
					SuulkaPsiBonus suulkaPsiBonus = new SuulkaPsiBonus();
					suulkaPsiBonus.Name = current2.GetAttribute("name");
					string attribute = current2.GetAttribute("ability");
					if (!string.IsNullOrEmpty(attribute))
					{
						suulkaPsiBonus.Ability = (SuulkaPsiBonusAbilityType)Enum.Parse(typeof(SuulkaPsiBonusAbilityType), attribute);
					}
					XmlElement xmlElement = current2["PsiEfficiency"];
					if (xmlElement != null)
					{
						foreach (XmlElement current3 in xmlElement.OfType<XmlElement>())
						{
							attribute = current3.GetAttribute("psi");
							if (!string.IsNullOrEmpty(attribute))
							{
								SectionEnumerations.PsionicAbility psionicAbility = (SectionEnumerations.PsionicAbility)Enum.Parse(typeof(SectionEnumerations.PsionicAbility), attribute);
								attribute = current3.GetAttribute("rate");
								if (!string.IsNullOrEmpty(attribute))
								{
									suulkaPsiBonus.Rate[(int)psionicAbility] = float.Parse(attribute);
								}
								attribute = current3.GetAttribute("efficiency");
								if (!string.IsNullOrEmpty(attribute))
								{
									suulkaPsiBonus.PsiEfficiency[(int)psionicAbility] = float.Parse(attribute);
								}
							}
						}
					}
					attribute = current2.GetAttribute("psidrain");
					if (!string.IsNullOrEmpty(attribute))
					{
						suulkaPsiBonus.PsiDrainMultiplyer = 1f;
					}
					attribute = current2.GetAttribute("lifedrain");
					if (!string.IsNullOrEmpty(attribute))
					{
						suulkaPsiBonus.LifeDrainMultiplyer = float.Parse(attribute);
					}
					attribute = current2.GetAttribute("tkfist");
					if (!string.IsNullOrEmpty(attribute))
					{
						suulkaPsiBonus.TKFistMultiplyer = float.Parse(attribute);
					}
					attribute = current2.GetAttribute("crush");
					if (!string.IsNullOrEmpty(attribute))
					{
						suulkaPsiBonus.CrushMultiplyer = float.Parse(attribute);
					}
					attribute = current2.GetAttribute("fear");
					if (!string.IsNullOrEmpty(attribute))
					{
						suulkaPsiBonus.FearMultiplyer = float.Parse(attribute);
					}
					attribute = current2.GetAttribute("control");
					if (!string.IsNullOrEmpty(attribute))
					{
						suulkaPsiBonus.ControlDuration = float.Parse(attribute);
					}
					attribute = current2.GetAttribute("movement");
					if (!string.IsNullOrEmpty(attribute))
					{
						suulkaPsiBonus.MovementMultiplyer = float.Parse(attribute);
					}
					attribute = current2.GetAttribute("biomissile");
					if (!string.IsNullOrEmpty(attribute))
					{
						suulkaPsiBonus.BioMissileMultiplyer = float.Parse(attribute);
					}
					for (int i = 0; i <= 19; i++)
					{
						if (suulkaPsiBonus.Rate[i] <= 0f)
						{
							suulkaPsiBonus.Rate[i] = 1f;
						}
						if (suulkaPsiBonus.PsiEfficiency[i] <= 0f)
						{
							suulkaPsiBonus.PsiEfficiency[i] = 1f;
						}
					}
					yield return suulkaPsiBonus;
				}
			}
			yield break;
		}
	}
}
