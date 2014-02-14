using Kerberos.Sots.Data.Xml;
using System;
using System.Collections.Generic;
using System.Xml;
namespace Kerberos.Sots.Data.StarMapFramework
{
	public class StarSystem : Feature
	{
		internal const string XmlSystemName = "System";
		private const string XmlProvinceIdName = "ProvinceId";
		private const string XmlGuidName = "Guid";
		private const string XmlTypeName = "Type";
		private const string XmlSubTypeName = "SubType";
		private const string XmlSizeName = "Size";
		private const string XmlPlayerSlotName = "PlayerSlot";
		private const string XmlRandomizeName = "Randomize";
		private const string XmlIsStartLocationName = "IsStartLocation";
		private const string XmlOrbitsName = "Orbits";
		public int? ProvinceId;
		public bool isRandom;
		public bool isStartLocation;
		public int? Guid;
		public int? PlayerSlot;
		public string Type = "";
		public string SubType = "";
		public string Size = "";
		public List<Orbit> Orbits = new List<Orbit>();
		public override string XmlName
		{
			get
			{
				return "System";
			}
		}
		public override void AttachToXmlNode(ref XmlElement node)
		{
			base.AttachToXmlNode(ref node);
			XmlHelper.AddNode(this.ProvinceId, "ProvinceId", ref node);
			XmlHelper.AddNode(this.Guid, "Guid", ref node);
			XmlHelper.AddNode(this.isRandom, "Randomize", ref node);
			XmlHelper.AddNode(this.isStartLocation, "IsStartLocation", ref node);
			XmlHelper.AddNode(this.PlayerSlot, "PlayerSlot", ref node);
			XmlHelper.AddNode(this.Type, "Type", ref node);
			XmlHelper.AddNode(this.SubType, "SubType", ref node);
			XmlHelper.AddNode(this.Size, "Size", ref node);
			XmlHelper.AddObjectCollectionNode(this.Orbits, "Orbits", ref node);
		}
		public override void LoadFromXmlNode(XmlElement node)
		{
			if (node != null)
			{
				base.LoadFromXmlNode(node);
				this.ProvinceId = XmlHelper.GetData<int?>(node, "ProvinceId");
				if (this.ProvinceId == -1)
				{
					this.ProvinceId = null;
				}
				this.Guid = XmlHelper.GetData<int?>(node, "Guid");
				bool arg_63_0 = this.Guid.HasValue;
				this.isRandom = XmlHelper.GetData<bool>(node, "Randomize");
				this.isStartLocation = XmlHelper.GetData<bool>(node, "IsStartLocation");
				this.PlayerSlot = XmlHelper.GetData<int?>(node, "PlayerSlot");
				this.Type = XmlHelper.GetData<string>(node, "Type");
				this.SubType = XmlHelper.GetData<string>(node, "SubType");
				this.Size = XmlHelper.GetData<string>(node, "Size");
				this.Orbits = XmlHelper.GetDataObjectCollection<Orbit>(node, "Orbits", Orbit.TypeMap);
			}
		}
	}
}
