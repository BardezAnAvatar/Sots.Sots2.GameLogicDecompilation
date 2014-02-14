using Kerberos.Sots.Data.Xml;
using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
using System.Xml;
namespace Kerberos.Sots.Data.WeaponFramework
{
	public class TurretClass : IXmlLoadSave
	{
		private const string XmlTurretClassName = "TurretClass";
		private const string XmlTurretClassSizesName = "TurretClassSizes";
		public string ActualTurretClass = "";
		public List<TurretClassSize> TurretClassSizes = new List<TurretClassSize>();
		public string XmlName
		{
			get
			{
				throw XmlHelper.NoXmlNameException;
			}
		}
		private static string MakeModelName(string barrel)
		{
			if (string.IsNullOrWhiteSpace(barrel))
			{
				return barrel;
			}
			return barrel + ".scene";
		}
		public IEnumerable<LogicalTurretClass> GetLogicalTurretClasses(bool ignoreInvalidData)
		{
			foreach (TurretClassSize current in this.TurretClassSizes)
			{
				WeaponEnums.TurretClasses turretClass;
				WeaponEnums.WeaponSizes turretSize;
				if (Enum.TryParse<WeaponEnums.TurretClasses>(this.ActualTurretClass, out turretClass) && Enum.TryParse<WeaponEnums.WeaponSizes>(current.TurretSize, out turretSize))
				{
					yield return new LogicalTurretClass
					{
						TurretClass = turretClass,
						TurretSize = turretSize,
						BarrelModelName = TurretClass.MakeModelName(current.Barrel),
						TurretModelName = TurretClass.MakeModelName(current.Turret),
						BaseModelName = TurretClass.MakeModelName(current.Base)
					};
				}
			}
			yield break;
		}
		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.ActualTurretClass, "TurretClass", ref node);
			XmlHelper.AddObjectCollectionNode(this.TurretClassSizes, "TurretClassSizes", "TurretClassSize", ref node);
		}
		public void LoadFromXmlNode(XmlElement node)
		{
			if (node != null)
			{
				this.ActualTurretClass = XmlHelper.GetData<string>(node, "TurretClass");
				this.TurretClassSizes = XmlHelper.GetDataObjectCollection<TurretClassSize>(node, "TurretClassSizes", "TurretClassSize");
			}
		}
	}
}
