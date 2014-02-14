using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.WeaponFramework
{
	public class GridDescriptor : IXmlLoadSave
	{
		private const int DEFAULT_GRID_HEIGHT = 10;
		private const int DEFAULT_GRID_WIDTH = 10;
		private const string XmlDimensionsName = "Dimensions";
		private const string XmlDataName = "Data";
		private const string XmlCollisionPointName = "CollisionPoint";
		public string Data = "";
		public int Width;
		public int Height;
		public int CollisionX;
		public int CollisionY;
		public string XmlName
		{
			get
			{
				throw XmlHelper.NoXmlNameException;
			}
		}
		public GridDescriptor()
		{
			this.Height = 10;
			this.Width = 10;
		}
		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(string.Format("{0},{1}", this.Width, this.Height), "Dimensions", ref node);
			XmlHelper.AddNode(string.Format("{0},{1}", this.CollisionX, this.CollisionY), "CollisionPoint", ref node);
			XmlHelper.AddNode(this.Data, "Data", ref node);
		}
		public void LoadFromXmlNode(XmlElement node)
		{
			if (node == null)
			{
				return;
			}
			string data = XmlHelper.GetData<string>(node["Dimensions"]);
			this.Width = int.Parse(data.Split(new char[]
			{
				','
			})[0]);
			this.Height = int.Parse(data.Split(new char[]
			{
				','
			})[1]);
			string data2 = XmlHelper.GetData<string>(node["CollisionPoint"]);
			this.CollisionX = int.Parse(data2.Split(new char[]
			{
				','
			})[0]);
			this.CollisionY = int.Parse(data2.Split(new char[]
			{
				','
			})[1]);
			this.Data = XmlHelper.GetData<string>(node["Data"]);
		}
	}
}
