using Kerberos.Sots.Data.ShipFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
namespace Kerberos.Sots.ShipFramework
{
	internal static class PsionicLibrary
	{
		public static IEnumerable<LogicalPsionic> Enumerate(XmlDocument doc)
		{
			foreach (XmlElement current in 
				from x in doc["CommonAssets"].OfType<XmlElement>()
				where x.Name.Equals("psionics", StringComparison.InvariantCulture)
				select x)
			{
				foreach (XmlElement current2 in current.OfType<XmlElement>())
				{
					LogicalPsionic logicalPsionic = new LogicalPsionic();
					logicalPsionic.Name = current2.GetAttribute("name");
					logicalPsionic.Ability = (SectionEnumerations.PsionicAbility)Enum.Parse(typeof(SectionEnumerations.PsionicAbility), logicalPsionic.Name);
					logicalPsionic.PsionicTitle = current2.GetAttribute("title");
					logicalPsionic.Description = current2.GetAttribute("description");
					logicalPsionic.Icon = current2.GetAttribute("icon");
					logicalPsionic.Model = current2.GetAttribute("mesh");
					logicalPsionic.MinPower = int.Parse(current2.GetAttribute("minPower"));
					logicalPsionic.MaxPower = int.Parse(current2.GetAttribute("maxPower"));
					logicalPsionic.BaseCost = int.Parse(current2.GetAttribute("cost"));
					logicalPsionic.Range = float.Parse(current2.GetAttribute("range"));
					logicalPsionic.BaseDamage = float.Parse(current2.GetAttribute("baseDamage"));
					logicalPsionic.CastorEffect = new LogicalEffect
					{
						Name = current2.GetAttribute("castorEffect") ?? string.Empty
					};
					logicalPsionic.CastEffect = new LogicalEffect
					{
						Name = current2.GetAttribute("castEffect") ?? string.Empty
					};
					logicalPsionic.ApplyEffect = new LogicalEffect
					{
						Name = current2.GetAttribute("applyEffect") ?? string.Empty
					};
					logicalPsionic.PsionicTitle = logicalPsionic.Name;
					logicalPsionic.RequiredTechID = current2.GetAttribute("tech");
					logicalPsionic.RequiresSuulka = bool.Parse(current2.GetAttribute("suulka_only"));
					yield return logicalPsionic;
				}
			}
			yield break;
		}
	}
}
