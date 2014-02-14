using Kerberos.Sots.Data.Xml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
namespace Kerberos.Sots.Engine
{
	[GameObjectType(InteropGameObjectType.IGOT_HOTKEYMANAGER)]
	internal class HotKeyManager : GameObject
	{
		public enum HotKeyActions
		{
			NoAction = -1,
			State_Starmap,
			State_BuildScreen,
			State_DesignScreen,
			State_ResearchScreen,
			State_ComparativeAnalysysScreen,
			State_EmpireSummaryScreen,
			State_SotspediaScreen,
			State_StarSystemScreen,
			State_FleetManagerScreen,
			State_DefenseManagerScreen,
			State_BattleRiderScreen,
			State_DiplomacyScreen,
			Starmap_EndTurn,
			Starmap_NextFleet,
			Starmap_LastFleet,
			Starmap_NextIdleFleet,
			Starmap_LastIdleFleet,
			Starmap_NextSystem,
			Starmap_LastSystem,
			Starmap_NextIncomingFleet,
			Starmap_OpenFleetManager,
			Starmap_OpenPlanetManager,
			Starmap_OpenStationManager,
			Starmap_OpenRepairScreen,
			Starmap_OpenPopulationManager,
			Starmap_NormalViewFilter,
			Starmap_SurveyViewFilter,
			Starmap_ProvinceFilter,
			Starmap_SupportRangeFilter,
			Starmap_SensorRangeFilter,
			Starmap_TerrainFilter,
			Starmap_TradeViewFilter,
			Starmap_NextNewsEvent,
			Starmap_LastNewsEvent,
			Starmap_OpenMenu,
			Research_NextTree,
			Research_LastTree,
			Combat_FocusCNC,
			Combat_SelectHome,
			Combat_SelectNext,
			Combat_FocusOnMouse,
			Combat_ToggleSensorView,
			Combat_StopSelectedShips,
			Combat_Pause,
			Combat_AccelTime,
			Combat_DecelTime,
			Combat_ExitAccelTime,
			Combat_FreeCamera,
			Combat_TrackCamera
		}
		public class KeyCombo
		{
			public bool shift;
			public bool control;
			public bool alt;
			public Keys key;
			public string states = "";
		}
		private const string _nodeHotkey = "hotkey";
		private const string _nodeEvent = "event";
		private const string _nodeStates = "states";
		private const string _nodeShift = "shift";
		private const string _nodeCtrl = "ctrl";
		private const string _nodeAlt = "alt";
		private const string _nodeKey = "key";
		private static string _profileRootDirectory;
		private string _profileName;
		private bool _loaded;
		private Dictionary<HotKeyManager.HotKeyActions, HotKeyManager.KeyCombo> HotKeys;
		private Dictionary<HotKeyManager.HotKeyActions, HotKeyManager.KeyCombo> DefaultHotKeys;
		private List<IKeyBindListener> _listeners;
		private List<IHotkeyVKListener> _vkListeners;
		private string _assetdir;
		private bool _enabled;
		public static string HotkeyProfileRootDirectory
		{
			get
			{
				return HotKeyManager._profileRootDirectory;
			}
		}
		public string HotkeyProfileName
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
		public HotKeyManager(App game, string assetdir)
		{
			game.AddExistingObject(this, new object[0]);
			this.HotKeys = new Dictionary<HotKeyManager.HotKeyActions, HotKeyManager.KeyCombo>();
			this.DefaultHotKeys = new Dictionary<HotKeyManager.HotKeyActions, HotKeyManager.KeyCombo>();
			this._listeners = new List<IKeyBindListener>();
			this._vkListeners = new List<IHotkeyVKListener>();
			this._assetdir = assetdir;
			this._enabled = true;
		}
		public void SetEnabled(bool value)
		{
			this._enabled = value;
			this.PostSetProp("SetLocked", !this._enabled);
		}
		public bool GetEnabled()
		{
			return this._enabled;
		}
		public void AddListener(IKeyBindListener listener)
		{
			if (!this._listeners.Contains(listener))
			{
				this._listeners.Add(listener);
			}
		}
		public void AddVKListener(IHotkeyVKListener listener)
		{
			if (!this._vkListeners.Contains(listener))
			{
				this._vkListeners.Add(listener);
			}
		}
		public void RemoveVKListener(IHotkeyVKListener listener)
		{
			this._vkListeners.Remove(listener);
		}
		public void SetVkReportMode(bool value)
		{
			this.PostSetProp("SetVKReportMode", value);
		}
		public HotKeyManager.KeyCombo GetHotKeyCombo(HotKeyManager.HotKeyActions action)
		{
			if (this.HotKeys.ContainsKey(action))
			{
				return this.HotKeys[action];
			}
			return null;
		}
		public List<HotKeyManager.HotKeyActions> SetHotKeyCombo(HotKeyManager.HotKeyActions action, HotKeyManager.KeyCombo combo)
		{
			List<HotKeyManager.HotKeyActions> list = new List<HotKeyManager.HotKeyActions>();
			string[] source = combo.states.Split(new char[]
			{
				'|'
			});
			foreach (HotKeyManager.HotKeyActions current in this.HotKeys.Keys)
			{
				if (current != action && this.HotKeys[current].key == combo.key && this.HotKeys[current].alt == combo.alt && this.HotKeys[current].control == combo.control && this.HotKeys[current].shift == combo.shift)
				{
					string[] array = this.HotKeys[current].states.Split(new char[]
					{
						'|'
					});
					string[] array2 = array;
					for (int i = 0; i < array2.Length; i++)
					{
						string value = array2[i];
						if (source.Contains(value))
						{
							this.HotKeys[current].key = Keys.None;
							list.Add(current);
						}
					}
				}
			}
			this.HotKeys[action] = combo;
			list.Add(action);
			return list;
		}
		public void RemoveListener(IKeyBindListener listener)
		{
			this._listeners.Remove(listener);
		}
		public void ClearListeners()
		{
			this._listeners.Clear();
		}
		public void SyncKeyProfile(string state = "")
		{
			if (!this._loaded)
			{
				return;
			}
			if (base.App.CurrentState != null && state == "")
			{
				this.SyncKeyProfileState(base.App.CurrentState.Name);
				return;
			}
			this.SyncKeyProfileState(state);
		}
		private void SyncKeyProfileState(string state)
		{
			if (!this._loaded)
			{
				return;
			}
			this.PostSetProp("ClearCombos", new object[0]);
			foreach (HotKeyManager.HotKeyActions current in 
				from x in this.HotKeys.Keys
				where this.HotKeys[x].states.Contains(state)
				select x)
			{
				if (this.HotKeys[current].key != Keys.None || current == HotKeyManager.HotKeyActions.NoAction)
				{
					this.PostSetProp("KeyCombos", new object[]
					{
						(int)current,
						this.HotKeys[current].shift,
						this.HotKeys[current].control,
						this.HotKeys[current].alt,
						this.HotKeys[current].key
					});
				}
			}
		}
		public override bool OnEngineMessage(InteropMessageID messageId, ScriptMessageReader message)
		{
			if (messageId == InteropMessageID.IMID_SCRIPT_KEYCOMBO)
			{
				if (!this._enabled)
				{
					return true;
				}
				HotKeyManager.HotKeyActions hotKeyActions = (HotKeyManager.HotKeyActions)message.ReadInteger();
				if (this.HotKeys.ContainsKey(hotKeyActions))
				{
					foreach (IKeyBindListener current in this._listeners)
					{
						if (current.OnKeyBindPressed(hotKeyActions, this.HotKeys[hotKeyActions].states))
						{
							break;
						}
					}
				}
				return true;
			}
			else
			{
				if (messageId != InteropMessageID.IMID_SCRIPT_VKREPORT)
				{
					return false;
				}
				if (!this._enabled)
				{
					return true;
				}
				Keys key = (Keys)message.ReadInteger();
				bool shift = message.ReadBool();
				bool ctrl = message.ReadBool();
				bool alt = message.ReadBool();
				foreach (IHotkeyVKListener current2 in this._vkListeners)
				{
					if (current2.OnVKReported(key, shift, ctrl, alt))
					{
						break;
					}
				}
				return true;
			}
		}
		public static void SetHotkeyProfileDirectory(string directory)
		{
			HotKeyManager._profileRootDirectory = directory;
		}
		public string GetStringforKey(Keys key)
		{
			if (key >= Keys.D0 && key <= Keys.D9)
			{
				return key.ToString().Substring(1);
			}
			string result;
			switch (key)
			{
			case Keys.Prior:
				result = "PageUp";
				break;
			case Keys.Next:
				result = "PageDown";
				break;
			default:
				switch (key)
				{
				case Keys.OemSemicolon:
					result = ";";
					break;
				case Keys.Oemplus:
					result = "+";
					break;
				case Keys.Oemcomma:
					result = ",";
					break;
				case Keys.OemMinus:
					result = "-";
					break;
				case Keys.OemPeriod:
					result = ".";
					break;
				case Keys.OemQuestion:
					result = "/";
					break;
				case Keys.Oemtilde:
					result = "`";
					break;
				default:
					switch (key)
					{
					case Keys.OemOpenBrackets:
						result = "[";
						break;
					case Keys.OemPipe:
						result = "\\";
						break;
					case Keys.OemCloseBrackets:
						result = "]";
						break;
					case Keys.OemQuotes:
						result = "'";
						break;
					default:
						result = key.ToString();
						break;
					}
					break;
				}
				break;
			}
			return result;
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
				text = HotKeyManager._profileRootDirectory + "\\" + profileName + ".keys";
			}
			if (profileName == "~Default")
			{
				text = this._assetdir + "\\" + profileName + ".keys";
			}
			if (!File.Exists(text))
			{
				return false;
			}
			this.HotKeys.Clear();
			if (profileName == "~Default")
			{
				this.DefaultHotKeys.Clear();
			}
			this._profileName = profileName;
			Stream stream;
			if (App.GetStreamForFile(text) == null)
			{
				stream = new FileStream(text, FileMode.Open);
			}
			else
			{
				stream = App.GetStreamForFile(text);
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
						if (xPathNavigator.HasChildren)
						{
							HotKeyManager.KeyCombo keyCombo = new HotKeyManager.KeyCombo();
							HotKeyManager.HotKeyActions hotKeyActions = HotKeyManager.HotKeyActions.NoAction;
							xPathNavigator.MoveToFirstChild();
							do
							{
								string name;
								if ((name = xPathNavigator.Name) != null)
								{
									if (!(name == "event"))
									{
										if (!(name == "states"))
										{
											if (!(name == "shift"))
											{
												if (!(name == "ctrl"))
												{
													if (!(name == "alt"))
													{
														if (name == "key")
														{
															if (Enum.IsDefined(typeof(Keys), xPathNavigator.Value))
															{
																keyCombo.key = (Keys)Enum.Parse(typeof(Keys), xPathNavigator.Value, true);
															}
															else
															{
																keyCombo.key = Keys.None;
															}
														}
													}
													else
													{
														keyCombo.alt = bool.Parse(xPathNavigator.Value);
													}
												}
												else
												{
													keyCombo.control = bool.Parse(xPathNavigator.Value);
												}
											}
											else
											{
												keyCombo.shift = bool.Parse(xPathNavigator.Value);
											}
										}
										else
										{
											keyCombo.states = xPathNavigator.Value;
										}
									}
									else
									{
										if (Enum.IsDefined(typeof(HotKeyManager.HotKeyActions), xPathNavigator.Value))
										{
											hotKeyActions = (HotKeyManager.HotKeyActions)Enum.Parse(typeof(HotKeyManager.HotKeyActions), xPathNavigator.Value, true);
										}
									}
								}
							}
							while (xPathNavigator.MoveToNext());
							if (hotKeyActions != HotKeyManager.HotKeyActions.NoAction && !this.HotKeys.ContainsKey(hotKeyActions))
							{
								this.HotKeys.Add(hotKeyActions, keyCombo);
								if (profileName == "~Default")
								{
									this.DefaultHotKeys.Add(hotKeyActions, keyCombo);
								}
							}
							xPathNavigator.MoveToParent();
						}
					}
					while (xPathNavigator.MoveToNext());
				}
			}
			while (xPathNavigator.MoveToNext());
			this._loaded = true;
			Array values = Enum.GetValues(typeof(HotKeyManager.HotKeyActions));
			foreach (HotKeyManager.HotKeyActions hotKeyActions2 in values)
			{
				if (!this.HotKeys.Keys.Contains(hotKeyActions2) && this.DefaultHotKeys.Keys.Contains(hotKeyActions2))
				{
					this.SetHotKeyCombo(hotKeyActions2, this.DefaultHotKeys[hotKeyActions2]);
				}
			}
			this.SyncKeyProfile("");
			this.SaveProfile();
			return true;
		}
		public bool CreateProfile(string profileName)
		{
			if (!this.HotKeys.Any<KeyValuePair<HotKeyManager.HotKeyActions, HotKeyManager.KeyCombo>>() && !this.LoadProfile("~Default", false))
			{
				return false;
			}
			this._profileName = profileName;
			string path = HotKeyManager._profileRootDirectory + "\\" + profileName + ".keys";
			if (File.Exists(path))
			{
				return false;
			}
			this.SaveProfile();
			this._loaded = true;
			return true;
		}
		public bool SaveProfile()
		{
			if (this._profileName == "~Default")
			{
				return false;
			}
			string text = HotKeyManager._profileRootDirectory + "\\" + this._profileName + ".keys";
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml("<hotkeys></hotkeys>");
			XmlElement xmlElement = xmlDocument["hotkeys"];
			foreach (HotKeyManager.HotKeyActions current in this.HotKeys.Keys)
			{
				XmlHelper.AddNode("", "hotkey", ref xmlElement);
				XmlElement xmlElement2 = (XmlElement)xmlElement.LastChild;
				XmlHelper.AddNode(current.ToString(), "event", ref xmlElement2);
				XmlHelper.AddNode(this.HotKeys[current].states, "states", ref xmlElement2);
				XmlHelper.AddNode(this.HotKeys[current].shift, "shift", ref xmlElement2);
				XmlHelper.AddNode(this.HotKeys[current].control, "ctrl", ref xmlElement2);
				XmlHelper.AddNode(this.HotKeys[current].alt, "alt", ref xmlElement2);
				XmlHelper.AddNode(this.HotKeys[current].key.ToString(), "key", ref xmlElement2);
			}
			if (App.GetStreamForFile(text) == null)
			{
				xmlDocument.Save(text);
				App.LockFileStream(text);
			}
			else
			{
				Stream streamForFile = App.GetStreamForFile(text);
				streamForFile.SetLength(0L);
				xmlDocument.Save(streamForFile);
			}
			return true;
		}
		public static List<string> GetAvailableProfiles()
		{
			string[] files = Directory.GetFiles(HotKeyManager._profileRootDirectory);
			List<string> list = new List<string>();
			string[] array = files;
			for (int i = 0; i < array.Length; i++)
			{
				string text = array[i];
				if (!text.Contains("~Default") && text.EndsWith(".keys"))
				{
					string path = text;
					if (File.Exists(path))
					{
						list.Add(text);
					}
				}
			}
			return list;
		}
		public void DeleteProfile()
		{
			this.HotKeys.Clear();
			string text = HotKeyManager._profileRootDirectory + "\\" + this._profileName + ".keys";
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
