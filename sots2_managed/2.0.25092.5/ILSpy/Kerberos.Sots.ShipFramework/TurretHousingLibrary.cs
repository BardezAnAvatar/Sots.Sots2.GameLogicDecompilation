using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Data.Xml;
using Kerberos.Sots.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
namespace Kerberos.Sots.ShipFramework
{
	internal static class TurretHousingLibrary
	{
		public static IEnumerable<LogicalTurretHousing> Enumerate()
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(ScriptHost.FileSystem, "weapons\\turrets.xml");
			foreach (XmlElement current in 
				from x in xmlDocument["Turrets"].OfType<XmlElement>()
				where x.Name == "Turret"
				select x)
			{
				yield return new LogicalTurretHousing
				{
					MountSize = (WeaponEnums.WeaponSizes)Enum.Parse(typeof(WeaponEnums.WeaponSizes), current.GetAttribute("MountSize")),
					WeaponSize = (WeaponEnums.WeaponSizes)Enum.Parse(typeof(WeaponEnums.WeaponSizes), current.GetAttribute("WeaponSize")),
					Class = (WeaponEnums.TurretClasses)Enum.Parse(typeof(WeaponEnums.TurretClasses), current.GetAttribute("Class")),
					TrackSpeed = float.Parse(current.GetAttribute("TrackSpeed")),
					ModelName = current.GetAttribute("Model"),
					BaseModelName = current.GetAttribute("BaseModel")
				};
			}
			yield break;
		}
	}
}
