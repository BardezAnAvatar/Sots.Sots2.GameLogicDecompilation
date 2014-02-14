using Kerberos.Sots.Data.Xml;
using Kerberos.Sots.Engine;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
namespace Kerberos.Sots.Data
{
	internal class NamesPool
	{
		private interface iNameCollection
		{
			string GetName(string sex);
			void LoadLists(XmlElement e);
		}
		private class HumanNameCollection : NamesPool.iNameCollection
		{
			private List<string> UsedNames = new List<string>();
			private Dictionary<string, List<string>> ListA = new Dictionary<string, List<string>>();
			private List<string> ListB = new List<string>();
			private List<string> ListC = new List<string>();
			public string GetName(string sex)
			{
				string result = string.Format("{0} {1}", NamesPool.Choose(this.ListA[sex]), NamesPool.Choose(this.ListB));
				NamesPool.AppendDuplicate(ref result, ref this.UsedNames);
				return result;
			}
			public void LoadLists(XmlElement e)
			{
				this.ListA.Add("male", XmlHelper.GetDataCollection<string>(e["ListA"], "male", "string"));
				this.ListA.Add("female", XmlHelper.GetDataCollection<string>(e["ListA"], "female", "string"));
				this.ListB = XmlHelper.GetDataCollection<string>(e, "ListB", "string");
			}
		}
		private class HiverNameCollection : NamesPool.iNameCollection
		{
			private List<string> UsedNames = new List<string>();
			private List<string> ListA = new List<string>();
			private List<string> ListB = new List<string>();
			private List<string> ListC = new List<string>();
			public string GetName(string sex)
			{
				string result = string.Format("{0} {1} {2}", NamesPool.Choose(this.ListA), NamesPool.Choose(this.ListB), NamesPool.Choose(this.ListC));
				NamesPool.AppendDuplicate(ref result, ref this.UsedNames);
				return result;
			}
			public void LoadLists(XmlElement e)
			{
				this.ListA = XmlHelper.GetDataCollection<string>(e, "ListA", "string");
				this.ListB = XmlHelper.GetDataCollection<string>(e, "ListB", "string");
				this.ListC = XmlHelper.GetDataCollection<string>(e, "ListC", "string");
			}
		}
		private class TarkaNameCollection : NamesPool.iNameCollection
		{
			private List<string> UsedNames = new List<string>();
			private Dictionary<string, List<string>> ListA = new Dictionary<string, List<string>>();
			private List<string> ListB = new List<string>();
			private List<string> ListC = new List<string>();
			public string GetName(string sex)
			{
				string result = string.Format("{0} {1} {2}", NamesPool.Choose(this.ListA[sex]), NamesPool.Choose(this.ListB), NamesPool.Choose(this.ListC));
				NamesPool.AppendDuplicate(ref result, ref this.UsedNames);
				return result;
			}
			public void LoadLists(XmlElement e)
			{
				this.ListA.Add("male", XmlHelper.GetDataCollection<string>(e["ListA"], "male", "string"));
				this.ListA.Add("female", XmlHelper.GetDataCollection<string>(e["ListA"], "female", "string"));
				this.ListB = XmlHelper.GetDataCollection<string>(e, "ListB", "string");
				this.ListC = XmlHelper.GetDataCollection<string>(e, "ListC", "string");
			}
		}
		private class MorrigiNameCollection : NamesPool.iNameCollection
		{
			private List<string> UsedNames = new List<string>();
			private Dictionary<string, List<string>> ListA = new Dictionary<string, List<string>>();
			private List<string> ListB = new List<string>();
			public string GetName(string sex)
			{
				string result = string.Format("{0} {1}", NamesPool.Choose(this.ListA[sex]), NamesPool.Choose(this.ListB));
				NamesPool.AppendDuplicate(ref result, ref this.UsedNames);
				return result;
			}
			public void LoadLists(XmlElement e)
			{
				this.ListA.Add("male", XmlHelper.GetDataCollection<string>(e["ListA"], "male", "string"));
				this.ListA.Add("female", XmlHelper.GetDataCollection<string>(e["ListA"], "female", "string"));
				this.ListB = XmlHelper.GetDataCollection<string>(e, "ListB", "string");
			}
		}
		private class ZuulNameCollection : NamesPool.iNameCollection
		{
			private List<string> UsedNames = new List<string>();
			private List<string> ListA = new List<string>();
			private List<string> ListB = new List<string>();
			private List<string> ListC = new List<string>();
			public string GetName(string sex)
			{
				string result = string.Format("{0} {1} {2}", NamesPool.Choose(this.ListA), NamesPool.Choose(this.ListB), NamesPool.Choose(this.ListC));
				NamesPool.AppendDuplicate(ref result, ref this.UsedNames);
				return result;
			}
			public void LoadLists(XmlElement e)
			{
				this.ListA = XmlHelper.GetDataCollection<string>(e, "ListA", "string");
				this.ListB = XmlHelper.GetDataCollection<string>(e, "ListB", "string");
				this.ListC = XmlHelper.GetDataCollection<string>(e, "ListC", "string");
			}
		}
		private class LiirNameCollection : NamesPool.iNameCollection
		{
			private List<string> UsedNames = new List<string>();
			private List<string> ListA = new List<string>();
			public string GetName(string sex)
			{
				string result = NamesPool.Choose(this.ListA);
				NamesPool.AppendDuplicate(ref result, ref this.UsedNames);
				return result;
			}
			public void LoadLists(XmlElement e)
			{
				this.ListA = XmlHelper.GetDataCollection<string>(e, "ListA", "string");
			}
		}
		private class ShipNameCollection
		{
			private List<List<KeyValuePair<string, string>>> Lists = new List<List<KeyValuePair<string, string>>>();
			public string GetName(GameSession game, int playerid, ShipClass shipclass, IEnumerable<string> checknames = null)
			{
				string text = "";
				foreach (List<KeyValuePair<string, string>> current in this.Lists)
				{
					if (current.Any((KeyValuePair<string, string> x) => x.Key == "" || x.Key.ToLower().Contains(shipclass.ToString().ToLower())))
					{
						text += string.Format("{0}", NamesPool.Choose((
							from x in current
							where x.Key == "" || x.Key.ToLower().Contains(shipclass.ToString().ToLower())
							select x.Value).ToList<string>()));
					}
				}
				List<string> list = new List<string>();
				list.AddRange(
					from x in game.GameDatabase.GetShipInfos(true)
					where x.DesignInfo.PlayerID == playerid
					select x.ShipName);
				foreach (InvoiceInfo current2 in game.GameDatabase.GetInvoiceInfosForPlayer(playerid))
				{
					foreach (BuildOrderInfo current3 in game.GameDatabase.GetBuildOrdersForInvoiceInstance(current2.ID))
					{
						list.Add(current3.ShipName);
					}
				}
				if (checknames != null)
				{
					list.AddRange(checknames);
				}
				NamesPool.AppendDuplicate(ref text, ref list);
				return text;
			}
			public void LoadLists(XmlElement e)
			{
				foreach (XmlNode xmlNode in e.ChildNodes)
				{
					List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
					foreach (XmlNode xmlNode2 in xmlNode.ChildNodes)
					{
						string text = ((XmlElement)xmlNode2).GetAttribute("class");
						text = ((text != null) ? text : "");
						string innerText = ((XmlElement)xmlNode2).InnerText;
						list.Add(new KeyValuePair<string, string>(text, innerText));
					}
					if (list.Any<KeyValuePair<string, string>>())
					{
						this.Lists.Add(list);
					}
				}
			}
		}
		private class LoaNameCollection : NamesPool.iNameCollection
		{
			private List<string> UsedNames = new List<string>();
			private List<string> ListA = new List<string>();
			private List<string> ListB = new List<string>();
			public string GetName(string sex)
			{
				string result = string.Format("{0} {1}", NamesPool.Choose(this.ListA), NamesPool.Choose(this.ListB));
				NamesPool.AppendDuplicate(ref result, ref this.UsedNames);
				return result;
			}
			public void LoadLists(XmlElement e)
			{
				this.ListA = XmlHelper.GetDataCollection<string>(e, "ListA", "string");
				this.ListB = XmlHelper.GetDataCollection<string>(e, "ListB", "string");
			}
		}
		private const string XmlSet1Name = "set1";
		private const string XmlSet2Name = "set2";
		private const string XmlListAName = "ListA";
		private const string XmlListBName = "ListB";
		private const string XmlListCName = "ListC";
		private const string XmlMaleName = "male";
		private const string XmlFemaleName = "female";
		private const string XmlStringName = "string";
		private const string XmlSystemNamesName = "SystemNames";
		private const string XmlProvinceNamesName = "ProvinceNames";
		private const string XmlAdmiralNamesName = "AdmiralNames";
		private const string XmlFleetNamesName = "FleetNames";
		private const string XmlSalvageProjectNames = "SalvageProjectNames";
		private const string XmlShipsNamesName = "ShipNames";
		private string _lastFileName = "";
		private int planetIteration = 1;
		private readonly Dictionary<string, Type> RaceNameCollectionMap = new Dictionary<string, Type>
		{

			{
				"human",
				typeof(NamesPool.HumanNameCollection)
			},

			{
				"hiver",
				typeof(NamesPool.HiverNameCollection)
			},

			{
				"tarka",
				typeof(NamesPool.TarkaNameCollection)
			},

			{
				"morrigi",
				typeof(NamesPool.MorrigiNameCollection)
			},

			{
				"hordezuul",
				typeof(NamesPool.ZuulNameCollection)
			},

			{
				"presterzuul",
				typeof(NamesPool.ZuulNameCollection)
			},

			{
				"liir",
				typeof(NamesPool.LiirNameCollection)
			},

			{
				"loa",
				typeof(NamesPool.LoaNameCollection)
			}
		};
		private static List<string> SystemNames = new List<string>();
		private static Dictionary<string, List<string>> ProvinceNames = new Dictionary<string, List<string>>();
		private static Dictionary<string, NamesPool.iNameCollection> AdmiralNames = new Dictionary<string, NamesPool.iNameCollection>();
		private static Dictionary<string, List<string>> FleetNames = new Dictionary<string, List<string>>();
		private static Dictionary<string, List<string>> SalvageProjectNames = new Dictionary<string, List<string>>();
		private static Dictionary<string, NamesPool.ShipNameCollection> ShipNames = new Dictionary<string, NamesPool.ShipNameCollection>();
		private static string GetExclusiveName(ref List<string> list)
		{
			Random safeRandom = App.GetSafeRandom();
			int index = safeRandom.Next(list.Count<string>());
			string text = list[index];
			list.Remove(text);
			return text;
		}
		private void GetNewSystemNameList()
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(ScriptHost.FileSystem, this._lastFileName);
			XmlElement documentElement = xmlDocument.DocumentElement;
			NamesPool.SystemNames.Clear();
			NamesPool.SystemNames = XmlHelper.GetDataCollection<string>(documentElement, "SystemNames", "string");
			for (int i = 0; i < NamesPool.SystemNames.Count; i++)
			{
				List<string> systemNames;
				int index;
				(systemNames = NamesPool.SystemNames)[index = i] = systemNames[index] + " " + NamesPool.ToRomanNumeral(this.planetIteration);
			}
		}
		private static void AppendDuplicate(ref string name, ref List<string> usedNames)
		{
			if (!usedNames.Contains(name))
			{
				return;
			}
			int num = 1;
			string text = string.Format("{0} {1}", name, NamesPool.ToRomanNumeral(num));
			while (usedNames.Contains(text))
			{
				num++;
				text = string.Format("{0} {1}", name, NamesPool.ToRomanNumeral(num));
			}
			usedNames.Add(text);
			name = text;
		}
		private static string Choose(List<string> list)
		{
			Random safeRandom = App.GetSafeRandom();
			return list[safeRandom.Next(list.Count<string>())];
		}
		private static string ToRomanNumeral(int value)
		{
			if (value > 399 || value < 1)
			{
				return "";
			}
			StringBuilder stringBuilder = new StringBuilder();
			int[] array = new int[]
			{
				100,
				90,
				50,
				40,
				10,
				9,
				5,
				4,
				1
			};
			string[] array2 = new string[]
			{
				"C",
				"XC",
				"L",
				"XL",
				"X",
				"IX",
				"V",
				"IV",
				"I"
			};
			for (int i = 0; i < 9; i++)
			{
				while (value >= array[i])
				{
					value -= array[i];
					stringBuilder.Append(array2[i]);
				}
			}
			return stringBuilder.ToString();
		}
		public string GetSystemName()
		{
			if (NamesPool.SystemNames.Count == 0)
			{
				this.planetIteration++;
				this.GetNewSystemNameList();
			}
			return NamesPool.GetExclusiveName(ref NamesPool.SystemNames);
		}
		public string GetProvinceName(string faction)
		{
			List<string> value = NamesPool.ProvinceNames[faction];
			string exclusiveName = NamesPool.GetExclusiveName(ref value);
			NamesPool.ProvinceNames[faction] = value;
			return exclusiveName;
		}
		public string GetAdmiralName(string race, string sex = "")
		{
			return NamesPool.AdmiralNames[race].GetName(sex);
		}
		public string GetFleetName(string faction)
		{
			return NamesPool.Choose(NamesPool.FleetNames[faction]);
		}
		public List<string> GetFleetNamesForFaction(string faction)
		{
			return NamesPool.FleetNames[faction];
		}
		public string GetSalvageProjectName(string projtype)
		{
			return App.Localize(NamesPool.Choose(NamesPool.SalvageProjectNames[projtype]));
		}
		public string GetShipName(GameSession game, int playerid, ShipClass shipclass, IEnumerable<string> checknames = null)
		{
			string name = game.AssetDatabase.GetFaction(game.GameDatabase.GetPlayerInfo(playerid).FactionID).Name;
			return NamesPool.ShipNames[name].GetName(game, playerid, shipclass, checknames);
		}
		public NamesPool(string filename)
		{
			this._lastFileName = filename;
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(ScriptHost.FileSystem, filename);
			XmlElement documentElement = xmlDocument.DocumentElement;
			NamesPool.SystemNames.Clear();
			NamesPool.AdmiralNames.Clear();
			NamesPool.ProvinceNames.Clear();
			NamesPool.FleetNames.Clear();
			NamesPool.SalvageProjectNames.Clear();
			NamesPool.ShipNames.Clear();
			NamesPool.SystemNames = XmlHelper.GetDataCollection<string>(documentElement, "SystemNames", "string");
			foreach (XmlElement xmlElement in documentElement["AdmiralNames"])
			{
				NamesPool.AdmiralNames.Add(xmlElement.Name, (NamesPool.iNameCollection)Activator.CreateInstance(this.RaceNameCollectionMap[xmlElement.Name]));
				NamesPool.AdmiralNames[xmlElement.Name].LoadLists(xmlElement);
			}
			foreach (XmlElement xmlElement2 in documentElement["ProvinceNames"])
			{
				List<string> dataCollection = XmlHelper.GetDataCollection<string>(documentElement["ProvinceNames"], xmlElement2.Name, "string");
				NamesPool.ProvinceNames.Add(xmlElement2.Name, dataCollection);
				foreach (string current in dataCollection)
				{
					int num = 0;
					foreach (List<string> current2 in NamesPool.ProvinceNames.Values)
					{
						if (current2.Contains(current))
						{
							num++;
						}
					}
				}
			}
			foreach (XmlElement xmlElement3 in documentElement["FleetNames"])
			{
				NamesPool.FleetNames.Add(xmlElement3.Name, XmlHelper.GetDataCollection<string>(documentElement["FleetNames"], xmlElement3.Name, "string"));
			}
			foreach (XmlElement xmlElement4 in documentElement["SalvageProjectNames"])
			{
				NamesPool.SalvageProjectNames.Add(xmlElement4.Name, XmlHelper.GetDataCollection<string>(documentElement["SalvageProjectNames"], xmlElement4.Name, "string"));
			}
			foreach (XmlElement xmlElement5 in documentElement["ShipNames"])
			{
				NamesPool.ShipNames.Add(xmlElement5.Name, new NamesPool.ShipNameCollection());
				NamesPool.ShipNames[xmlElement5.Name].LoadLists(xmlElement5);
			}
		}
	}
}
