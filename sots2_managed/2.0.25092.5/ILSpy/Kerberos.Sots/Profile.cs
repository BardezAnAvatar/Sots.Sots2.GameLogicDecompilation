using Kerberos.Sots.Data.Xml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.XPath;
namespace Kerberos.Sots
{
	public class Profile
	{
		private const string _nodeProfile = "profile";
		private const string _nodeName = "name";
		private const string _nodeTechs = "techs";
		private const string _nodeUsername = "username";
		private const string _nodePassword = "password";
		private const string _nodeLastGamePlayed = "last_game_played";
		private const string _nodeAutoPlaceDefenses = "auto_place_defenses";
		private const string _nodeAutoRepairFleets = "auto_repair_fleets";
		private const string _nodeAutoUseGoop = "auto_goop";
		private const string _nodeAutoUseJoker = "auto_joker";
		private const string _nodeAutoAOE = "auto_aoe";
		private const string _nodeAutoPatrol = "auto_patrol";
		private static string _profileRootDirectory;
		private string _profileName;
		private bool _loaded;
		private List<string> _researchedTechs;
		public static string ProfileRootDirectory
		{
			get
			{
				return Profile._profileRootDirectory;
			}
		}
		public string ProfileName
		{
			get
			{
				return this._profileName;
			}
		}
		public bool Loaded
		{
			get
			{
				return this._loaded;
			}
		}
		public List<string> ResearchedTechs
		{
			get
			{
				return this._researchedTechs;
			}
		}
		public string Username
		{
			get;
			set;
		}
		public string Password
		{
			get;
			set;
		}
		public string LastGamePlayed
		{
			get;
			set;
		}
		public bool AutoPlaceDefenseAssets
		{
			get;
			set;
		}
		public bool AutoRepairFleets
		{
			get;
			set;
		}
		public bool AutoUseGoop
		{
			get;
			set;
		}
		public bool AutoUseJoker
		{
			get;
			set;
		}
		public bool AutoAOE
		{
			get;
			set;
		}
		public bool AutoPatrol
		{
			get;
			set;
		}
		public Profile(string dir)
		{
			Profile._profileRootDirectory = dir;
			this._researchedTechs = new List<string>();
			this.Username = string.Empty;
			this.Password = string.Empty;
			this.LastGamePlayed = string.Empty;
			this.AutoPlaceDefenseAssets = false;
			this.AutoRepairFleets = false;
			this.AutoUseGoop = false;
			this.AutoUseJoker = false;
			this.AutoAOE = false;
			this.AutoPatrol = false;
		}
		public Profile()
		{
			this._researchedTechs = new List<string>();
			this.AutoPlaceDefenseAssets = false;
			this.AutoRepairFleets = false;
			this.AutoUseGoop = false;
			this.AutoUseJoker = false;
			this.AutoAOE = false;
			this.AutoPatrol = false;
		}
		public static void SetProfileDirectory(string directory)
		{
			Profile._profileRootDirectory = directory;
		}
		public bool LoadProfile(string profileName, bool absolutepath = false)
		{
			string text;
			if (absolutepath)
			{
				text = profileName;
			}
			else
			{
				text = Profile._profileRootDirectory + "\\" + profileName + ".xml";
			}
			if (!File.Exists(text))
			{
				return false;
			}
			Stream stream = App.GetStreamForFile(text);
			if (stream == null)
			{
				stream = new FileStream(text, FileMode.Open);
			}
			XPathDocument xPathDocument = new XPathDocument(stream);
			XPathNavigator xPathNavigator = xPathDocument.CreateNavigator();
			xPathNavigator.MoveToFirstChild();
			do
			{
				if (xPathNavigator.HasChildren)
				{
					xPathNavigator.MoveToFirstChild();
					do
					{
						string name;
						switch (name = xPathNavigator.Name)
						{
						case "name":
							this._profileName = xPathNavigator.Value;
							break;
						case "techs":
						{
							string value = xPathNavigator.Value;
							this._researchedTechs.Clear();
							string[] array = value.Split(new char[]
							{
								'!'
							});
							for (int i = 0; i < array.Length; i++)
							{
								if (array[i].Length > 0)
								{
									this._researchedTechs.Add(array[i]);
								}
							}
							break;
						}
						case "username":
							this.Username = (xPathNavigator.Value ?? string.Empty);
							break;
						case "password":
							this.Password = (xPathNavigator.Value ?? string.Empty);
							break;
						case "last_game_played":
							this.LastGamePlayed = (xPathNavigator.Value ?? string.Empty);
							break;
						case "auto_place_defenses":
							this.AutoPlaceDefenseAssets = bool.Parse(xPathNavigator.Value);
							break;
						case "auto_repair_fleets":
							this.AutoRepairFleets = bool.Parse(xPathNavigator.Value);
							break;
						case "auto_goop":
							this.AutoUseGoop = bool.Parse(xPathNavigator.Value);
							break;
						case "auto_joker":
							this.AutoUseJoker = bool.Parse(xPathNavigator.Value);
							break;
						case "auto_aoe":
							this.AutoAOE = bool.Parse(xPathNavigator.Value);
							break;
						case "auto_patrol":
							this.AutoPatrol = bool.Parse(xPathNavigator.Value);
							break;
						}
					}
					while (xPathNavigator.MoveToNext());
				}
			}
			while (xPathNavigator.MoveToNext());
			this._loaded = true;
			return true;
		}
		public bool SaveProfile()
		{
			string text = Profile._profileRootDirectory + "\\" + this._profileName + ".xml";
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml("<profile></profile>");
			XmlElement xmlElement = xmlDocument["profile"];
			XmlHelper.AddNode(this._profileName, "name", ref xmlElement);
			XmlHelper.AddNode(this.Username, "username", ref xmlElement);
			XmlHelper.AddNode(this.Password, "password", ref xmlElement);
			string text2 = "";
			for (int i = 0; i < this._researchedTechs.Count; i++)
			{
				text2 = text2 + this._researchedTechs[i].ToString() + "!";
			}
			XmlHelper.AddNode(text2, "techs", ref xmlElement);
			XmlHelper.AddNode(this.LastGamePlayed, "last_game_played", ref xmlElement);
			XmlHelper.AddNode(this.AutoPlaceDefenseAssets.ToString(), "auto_place_defenses", ref xmlElement);
			XmlHelper.AddNode(this.AutoRepairFleets.ToString(), "auto_repair_fleets", ref xmlElement);
			XmlHelper.AddNode(this.AutoUseGoop.ToString(), "auto_goop", ref xmlElement);
			XmlHelper.AddNode(this.AutoUseJoker.ToString(), "auto_joker", ref xmlElement);
			XmlHelper.AddNode(this.AutoAOE.ToString(), "auto_aoe", ref xmlElement);
			XmlHelper.AddNode(this.AutoPatrol.ToString(), "auto_patrol", ref xmlElement);
			if (App.GetStreamForFile(text) == null)
			{
				xmlDocument.Save(text);
			}
			else
			{
				Stream streamForFile = App.GetStreamForFile(text);
				streamForFile.SetLength(0L);
				xmlDocument.Save(streamForFile);
			}
			return true;
		}
		public bool CreateProfile(string profileName)
		{
			this._profileName = profileName;
			string text = Profile._profileRootDirectory + "\\" + profileName + ".xml";
			if (File.Exists(text))
			{
				return false;
			}
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml("<profile><name>" + profileName + "</name></profile>");
			xmlDocument.Save(text);
			App.LockFileStream(text);
			this._loaded = true;
			return true;
		}
		public static List<Profile> GetAvailableProfiles()
		{
			string[] files = Directory.GetFiles(Profile._profileRootDirectory);
			List<Profile> list = new List<Profile>();
			string[] array = files;
			for (int i = 0; i < array.Length; i++)
			{
				string text = array[i];
				if (!text.EndsWith(".keys"))
				{
					Profile profile = new Profile();
					profile.LoadProfile(text, true);
					list.Add(profile);
				}
			}
			return list;
		}
		public void DeleteProfile()
		{
			string text = Profile._profileRootDirectory + "\\" + this._profileName + ".xml";
			App.UnLockFileStream(text);
			if (File.Exists(text))
			{
				File.Delete(text);
			}
			this._profileName = string.Empty;
			this._loaded = false;
		}
	}
}
