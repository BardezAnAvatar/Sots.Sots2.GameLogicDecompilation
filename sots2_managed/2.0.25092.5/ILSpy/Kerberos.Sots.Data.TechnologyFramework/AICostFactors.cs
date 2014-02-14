using Kerberos.Sots.Data.Xml;
using System;
using System.Collections.Generic;
using System.Xml;
namespace Kerberos.Sots.Data.TechnologyFramework
{
	public struct AICostFactors
	{
		public const string XmlHiverName = "HiverAICostFactor";
		public const string XmlHumanName = "HumanAICostFactor";
		public const string XmlLiirZuulName = "LiirZuulAICostFactor";
		public const string XmlMorrigiName = "MorrigiAICostFactor";
		public const string XmlTarkaName = "TarkaAICostFactor";
		public const string XmlZuulName = "ZuulAICostFactor";
		public const string XmlLoaName = "LoaAICostFactor";
		public static readonly AICostFactors Default = new AICostFactors
		{
			Hiver = 1f,
			Human = 1f,
			LiirZuul = 1f,
			Morrigi = 1f,
			Tarka = 1f,
			Zuul = 1f,
			Loa = 1f
		};
		public static readonly AICostFactors Zero = new AICostFactors
		{
			Hiver = 0f,
			Human = 0f,
			LiirZuul = 0f,
			Morrigi = 0f,
			Tarka = 0f,
			Zuul = 0f,
			Loa = 0f
		};
		public float Hiver;
		public float Human;
		public float LiirZuul;
		public float Morrigi;
		public float Tarka;
		public float Zuul;
		public float Loa;
		private static readonly string[] _factions = new string[]
		{
			"hiver",
			"human",
			"liir_zuul",
			"morrigi",
			"tarkas",
			"zuul",
			"loa"
		};
		public static IList<string> Factions
		{
			get
			{
				return AICostFactors._factions;
			}
		}
		public float Faction(string faction)
		{
			switch (faction)
			{
			case "hiver":
				return this.Hiver;
			case "human":
				return this.Human;
			case "liir_zuul":
				return this.LiirZuul;
			case "morrigi":
				return this.Morrigi;
			case "tarkas":
				return this.Tarka;
			case "zuul":
				return this.Zuul;
			case "loa":
				return this.Loa;
			}
			return 1f;
		}
		public void SetFaction(string faction, float value)
		{
			switch (faction)
			{
			case "hiver":
				this.Hiver = value;
				return;
			case "human":
				this.Human = value;
				return;
			case "liir_zuul":
				this.LiirZuul = value;
				return;
			case "morrigi":
				this.Morrigi = value;
				return;
			case "tarkas":
				this.Tarka = value;
				return;
			case "zuul":
				this.Zuul = value;
				return;
			case "loa":
				this.Loa = value;
				return;
			}
			throw new ArgumentOutOfRangeException("faction");
		}
		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode(this.Hiver, "HiverAICostFactor", ref node);
			XmlHelper.AddNode(this.Human, "HumanAICostFactor", ref node);
			XmlHelper.AddNode(this.LiirZuul, "LiirZuulAICostFactor", ref node);
			XmlHelper.AddNode(this.Morrigi, "MorrigiAICostFactor", ref node);
			XmlHelper.AddNode(this.Tarka, "TarkaAICostFactor", ref node);
			XmlHelper.AddNode(this.Zuul, "ZuulAICostFactor", ref node);
			XmlHelper.AddNode(this.Loa, "LoaAICostFactor", ref node);
		}
		public void LoadFromXmlNode(XmlElement node)
		{
			this.Hiver = XmlHelper.GetDataOrDefault<float>(node["HiverAICostFactor"], 1f);
			this.Human = XmlHelper.GetDataOrDefault<float>(node["HumanAICostFactor"], 1f);
			this.LiirZuul = XmlHelper.GetDataOrDefault<float>(node["LiirZuulAICostFactor"], 1f);
			this.Morrigi = XmlHelper.GetDataOrDefault<float>(node["MorrigiAICostFactor"], 1f);
			this.Tarka = XmlHelper.GetDataOrDefault<float>(node["TarkaAICostFactor"], 1f);
			this.Zuul = XmlHelper.GetDataOrDefault<float>(node["ZuulAICostFactor"], 1f);
			this.Loa = XmlHelper.GetDataOrDefault<float>(node["LoaAICostFactor"], 1f);
		}
	}
}
