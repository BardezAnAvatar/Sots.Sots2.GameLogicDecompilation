using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;
namespace Kerberos.Sots.Data.TechnologyFramework
{
	public class Allowable : IXmlLoadSave
	{
		internal const string XmlAllowedTechName = "AllowedTech";
		private const string XmlIdName = "Id";
		private const string XmlIsPermanentName = "IsPermanent";
		private const string XmlHiverPercentName = "HiverPercent";
		private const string XmlHumanPercentName = "HumanPercent";
		private const string XmlLiirZuulPercentName = "LiirZuulPercent";
		private const string XmlMorrigiPercentName = "MorrigiPercent";
		private const string XmlTarkaPercentName = "TarkaPercent";
		private const string XmlZuulPercentName = "ZuulPercent";
		private const string XmlLoaPercentName = "LoaPercent";
		private const string XmlResearchPointsName = "ResearchPoints";
		public string Id = "";
		public bool IsPermanent;
		public float HiverPercent;
		public float HumanPercent;
		public float LiirZuulPercent;
		public float MorrigiPercent;
		public float TarkaPercent;
		public float ZuulPercent;
		public float LoaPercent;
		public int ResearchPoints;
		public string XmlName
		{
			get
			{
				return "AllowedTech";
			}
		}
		public override string ToString()
		{
			return string.Concat(new object[]
			{
				"Allows {",
				this.Id,
				"} $",
				this.ResearchPoints
			});
		}
		public float GetFactionProbabilityPercentage(string faction)
		{
			if (this.IsPermanent)
			{
				return 100f;
			}
			switch (faction)
			{
			case "hiver":
				return this.HiverPercent;
			case "human":
				return this.HumanPercent;
			case "liir_zuul":
				return this.LiirZuulPercent;
			case "morrigi":
				return this.MorrigiPercent;
			case "tarkas":
				return this.TarkaPercent;
			case "zuul":
				return this.ZuulPercent;
			case "loa":
				return this.LoaPercent;
			}
			return 0f;
		}
		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.Id, "Id", ref node);
			XmlHelper.AddNode(this.IsPermanent, "IsPermanent", ref node);
			XmlHelper.AddNode(this.HiverPercent, "HiverPercent", ref node);
			XmlHelper.AddNode(this.HumanPercent, "HumanPercent", ref node);
			XmlHelper.AddNode(this.LiirZuulPercent, "LiirZuulPercent", ref node);
			XmlHelper.AddNode(this.MorrigiPercent, "MorrigiPercent", ref node);
			XmlHelper.AddNode(this.TarkaPercent, "TarkaPercent", ref node);
			XmlHelper.AddNode(this.ZuulPercent, "ZuulPercent", ref node);
			XmlHelper.AddNode(this.LoaPercent, "LoaPercent", ref node);
			XmlHelper.AddNode(this.ResearchPoints, "ResearchPoints", ref node);
		}
		public void LoadFromXmlNode(XmlElement node)
		{
			this.Id = XmlHelper.GetData<string>(node, "Id");
			this.IsPermanent = XmlHelper.GetData<bool>(node, "IsPermanent");
			this.HiverPercent = XmlHelper.GetData<float>(node, "HiverPercent");
			this.HumanPercent = XmlHelper.GetData<float>(node, "HumanPercent");
			this.LiirZuulPercent = XmlHelper.GetData<float>(node, "LiirZuulPercent");
			this.MorrigiPercent = XmlHelper.GetData<float>(node, "MorrigiPercent");
			this.TarkaPercent = XmlHelper.GetData<float>(node, "TarkaPercent");
			this.ZuulPercent = XmlHelper.GetData<float>(node, "ZuulPercent");
			this.LoaPercent = XmlHelper.GetData<float>(node, "LoaPercent");
			this.ResearchPoints = XmlHelper.GetData<int>(node, "ResearchPoints");
		}
	}
}
