using Kerberos.Sots.Data.Xml;
using Kerberos.Sots.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
namespace Kerberos.Sots.GameObjects
{
	internal static class SkyDefinitions
	{
		public static SkyDefinition[] LoadFromXml()
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(ScriptHost.FileSystem, "commonassets.xml");
			XmlElement xmlElement = xmlDocument["CommonAssets"];
			if (xmlElement == null)
			{
				return new SkyDefinition[0];
			}
			List<SkyDefinition> list = new List<SkyDefinition>();
			foreach (XmlElement current in 
				from element in xmlElement.OfType<XmlElement>()
				where element.Name == "sky"
				select element)
			{
				SkyDefinition skyDefinition = new SkyDefinition();
				skyDefinition.MaterialName = current.GetAttribute("material");
				string attribute = current.GetAttribute("usage");
				if (!string.IsNullOrEmpty(attribute))
				{
					skyDefinition.Usage = (SkyUsage)Enum.Parse(typeof(SkyUsage), attribute);
				}
				list.Add(skyDefinition);
			}
			return list.ToArray();
		}
	}
}
