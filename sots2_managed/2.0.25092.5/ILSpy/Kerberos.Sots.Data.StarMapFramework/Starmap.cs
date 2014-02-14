using Kerberos.Sots.Data.Xml;
using Kerberos.Sots.Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
namespace Kerberos.Sots.Data.StarMapFramework
{
	public class Starmap
	{
		public class StarmapInfo
		{
			public string FileName;
			public string Title;
			public string Description;
			public int NumPlayers;
			public string PreviewTexture;
			public string GetFallbackTitle()
			{
				if (!string.IsNullOrEmpty(this.Title))
				{
					return this.Title;
				}
				return Path.GetFileNameWithoutExtension(this.FileName);
			}
			public StarmapInfo(string filename)
			{
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.Load(ScriptHost.FileSystem, filename);
				XmlElement xmlElement = xmlDocument["Starmap"];
				this.FileName = filename;
				this.NumPlayers = xmlElement["NumPlayers"].ExtractIntegerOrDefault(0);
				this.PreviewTexture = xmlElement["PreviewTexture"].ExtractStringOrDefault(string.Empty);
				this.Title = xmlElement["Title"].ExtractStringOrDefault(string.Empty);
				this.Description = xmlElement["Description"].ExtractStringOrDefault(string.Empty);
			}
		}
		internal const string XmlNumPlayersName = "NumPlayers";
		internal const string XmlStarmapName = "Starmap";
		internal const string XmlPreviewTextureName = "PreviewTexture";
		internal const string XmlFeaturesName = "Features";
		internal const string XmlNodeLinesName = "NodeLines";
		internal const string XmlProvincesName = "Provinces";
		internal const string XmlTitleName = "Title";
		internal const string XmlDescriptionName = "Description";
		public int NumPlayers = 8;
		public string Title = string.Empty;
		public string Description = string.Empty;
		public string PreviewTexture;
		public List<Feature> Features = new List<Feature>();
		public List<NodeLine> NodeLines = new List<NodeLine>();
		public List<Province> Provinces = new List<Province>();
		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.NumPlayers, "NumPlayers", ref node);
			XmlHelper.AddNode(this.PreviewTexture, "PreviewTexture", ref node);
			XmlHelper.AddObjectCollectionNode(this.Features, "Features", ref node);
			XmlHelper.AddObjectCollectionNode(this.NodeLines, "NodeLines", "NodeLine", ref node);
			XmlHelper.AddObjectCollectionNode(this.Provinces, "Provinces", "Province", ref node);
			XmlHelper.AddNode(this.Title, "Title", ref node);
			XmlHelper.AddNode(this.Description, "Description", ref node);
		}
		public void LoadFromXmlNode(XmlElement node)
		{
			if (node != null)
			{
				this.NumPlayers = XmlHelper.GetData<int>(node, "NumPlayers");
				if (this.NumPlayers == 0)
				{
					this.NumPlayers = 8;
				}
				this.PreviewTexture = XmlHelper.GetData<string>(node, "PreviewTexture");
				this.Features = XmlHelper.GetDataObjectCollection<Feature>(node, "Features", Feature.TypeMap);
				this.NodeLines = XmlHelper.GetDataObjectCollection<NodeLine>(node, "NodeLines", "NodeLine");
				this.Provinces = XmlHelper.GetDataObjectCollection<Province>(node, "Provinces", "Province");
				this.Title = XmlHelper.GetData<string>(node, "Title");
				this.Description = XmlHelper.GetData<string>(node, "Description");
			}
		}
	}
}
