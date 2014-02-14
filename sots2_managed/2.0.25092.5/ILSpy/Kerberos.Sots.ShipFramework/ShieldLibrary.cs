using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
namespace Kerberos.Sots.ShipFramework
{
	internal static class ShieldLibrary
	{
		public static IEnumerable<LogicalShield> Enumerate(XmlDocument doc)
		{
			XmlElement xmlElement = doc["CommonAssets"];
			XmlElement source = xmlElement["Shields"];
			foreach (XmlElement current in source.OfType<XmlElement>())
			{
				yield return new LogicalShield
				{
					Name = current.GetAttribute("name"),
					TechID = current.GetAttribute("techID"),
					Type = (LogicalShield.ShieldType)Enum.Parse(typeof(LogicalShield.ShieldType), current.GetAttribute("type")),
					CRShieldData = 
					{
						Structure = float.Parse(current.GetAttribute("crHealth")),
						RechargeTime = float.Parse(current.GetAttribute("crRechargeTime")),
						RicochetMod = float.Parse(current.GetAttribute("crRicochetMod")),
						ModelFileName = current.GetAttribute("crModelFileName"),
						ImpactEffectName = current.GetAttribute("crImpactEffectName")
					},
					DNShieldData = 
					{
						Structure = float.Parse(current.GetAttribute("dnHealth")),
						RechargeTime = float.Parse(current.GetAttribute("dnRechargeTime")),
						RicochetMod = float.Parse(current.GetAttribute("dnRicochetMod")),
						ModelFileName = current.GetAttribute("dnModelFileName"),
						ImpactEffectName = current.GetAttribute("dnImpactEffectName")
					}
				};
			}
			yield break;
		}
	}
}
