using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
namespace Kerberos.Sots.ShipFramework
{
	internal static class ShipSparksLibrary
	{
		public static IEnumerable<LogicalShipSpark> Enumerate(XmlDocument doc)
		{
			XmlElement xmlElement = doc["CommonAssets"];
			XmlElement source = xmlElement["ShipSparks"];
			foreach (XmlElement current in source.OfType<XmlElement>())
			{
				yield return new LogicalShipSpark
				{
					Type = (LogicalShipSpark.ShipSparkType)Enum.Parse(typeof(LogicalShipSpark.ShipSparkType), current.GetAttribute("type")),
					SparkEffect = new LogicalEffect
					{
						Name = current.GetAttribute("effect") ?? string.Empty
					}
				};
			}
			yield break;
		}
	}
}
