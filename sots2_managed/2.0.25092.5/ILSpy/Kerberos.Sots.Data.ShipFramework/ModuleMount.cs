using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.ShipFramework
{
	public class ModuleMount : IXmlLoadSave
	{
		public string Id = string.Empty;
		public string AssignedModuleName;
		public string NodeName;
		public string Size = "Cruiser";
		public string Type = "";
		public int FrameX;
		public int FrameY;
		internal static readonly string XmlNameModule = "Module";
		private static readonly string XmlIdName = "Id";
		private static readonly string XmlAssignedModuleName = "AssignedModule";
		private static readonly string XmlNodeNameName = "NodeName";
		private static readonly string XmlSizeName = "Size";
		private static readonly string XmlTypeName = "Type";
		private static readonly string XmlFrameXName = "FrameX";
		private static readonly string XmlFrameYName = "FrameY";
		public string XmlName
		{
			get
			{
				return ModuleMount.XmlNameModule;
			}
		}
		public ModuleMount()
		{
			if (ToolEnvironment.IsRunningInTool)
			{
				this.Id = Guid.NewGuid().ToString();
			}
		}
		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.Id, ModuleMount.XmlIdName, ref node);
			XmlHelper.AddNode(this.AssignedModuleName, ModuleMount.XmlAssignedModuleName, ref node);
			XmlHelper.AddNode(this.NodeName, ModuleMount.XmlNodeNameName, ref node);
			XmlHelper.AddNode(this.Size, ModuleMount.XmlSizeName, ref node);
			XmlHelper.AddNode(this.Type, ModuleMount.XmlTypeName, ref node);
			XmlHelper.AddNode(this.FrameX, ModuleMount.XmlFrameXName, ref node);
			XmlHelper.AddNode(this.FrameY, ModuleMount.XmlFrameYName, ref node);
		}
		public void LoadFromXmlNode(XmlElement node)
		{
			this.Id = XmlHelper.GetData<string>(node, ModuleMount.XmlIdName);
			this.AssignedModuleName = XmlHelper.GetData<string>(node, ModuleMount.XmlAssignedModuleName);
			this.NodeName = XmlHelper.GetData<string>(node, ModuleMount.XmlNodeNameName);
			this.Size = XmlHelper.GetData<string>(node, ModuleMount.XmlSizeName);
			this.Type = XmlHelper.GetData<string>(node, ModuleMount.XmlTypeName);
			this.FrameX = XmlHelper.GetData<int>(node, ModuleMount.XmlFrameXName);
			this.FrameY = XmlHelper.GetData<int>(node, ModuleMount.XmlFrameYName);
		}
	}
}
