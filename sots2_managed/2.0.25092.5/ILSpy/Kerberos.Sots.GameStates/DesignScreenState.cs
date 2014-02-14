using Kerberos.Sots.Combat;
using Kerberos.Sots.Data;
using Kerberos.Sots.Data.ModuleFramework;
using Kerberos.Sots.Data.ShipFramework;
using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Data.Xml;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.Steam;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
namespace Kerberos.Sots.GameStates
{
	internal class DesignScreenState : GameState, IKeyBindListener
	{
		private class CombatShipQueue
		{
			private float _offset;
			private float _offsetz;
			private float _AIoffset;
			private float _AIoffsetz = -3000f;
			private readonly XmlDocument _config = CombatConfig.CreateEmptyCombatConfigXml();
			public void AddDesign(App game, ShipBuilder builder, int count, int playerID)
			{
				for (int i = 0; i < count; i++)
				{
					List<WeaponAssignment> list = new List<WeaponAssignment>();
					if (builder.Ship != null)
					{
						foreach (WeaponBank current in builder.Ship.WeaponBanks)
						{
							list.Add(new WeaponAssignment
							{
								ModuleNode = "",
								Bank = current.LogicalBank,
								Weapon = current.Weapon,
								DesignID = current.DesignID,
								InitialFireMode = new int?(current.FireMode),
								InitialTargetFilter = new int?(current.TargetFilter)
							});
						}
					}
					List<ModuleAssignment> list2 = new List<ModuleAssignment>();
					if (builder.Ship != null)
					{
						foreach (Module current2 in builder.Ship.Modules)
						{
							list2.Add(new ModuleAssignment
							{
								ModuleMount = current2.Attachment,
								Module = current2.LogicalModule,
								PsionicAbilities = null
							});
						}
					}
					IEnumerable<string> sectionFileNames = 
						from x in builder.Sections
						select x.FileName;
					Vector3 position = new Vector3(this._offset, 0f, this._offsetz);
					Vector3 rotation = new Vector3(0f, 0f, 0f);
					if (playerID != game.LocalPlayer.ID)
					{
						position.X = this._AIoffset;
						position.Z = this._AIoffsetz;
						rotation.X = MathHelper.DegreesToRadians(180f);
						this._AIoffset += 500f;
						if (this._AIoffset >= 1999f)
						{
							this._AIoffsetz -= 500f;
							this._AIoffset = 0f;
						}
					}
					else
					{
						this._offset += 500f;
						if (this._offset >= 1999f)
						{
							this._offsetz += 500f;
							this._offset = 0f;
						}
					}
					XmlElement newChild = CombatConfig.ExportXmlElementFromShipParameters(game, this._config, sectionFileNames, list, list2, playerID, position, rotation);
					CombatConfig.GetGameObjectsElement(this._config).AppendChild(newChild);
				}
				DesignScreenState.CombatShipQueue.SetText(game, "Added ship " + CombatConfig.GetGameObjectsElement(this._config).ChildNodes.Count);
			}
			public void Start(App game, string levelFile, int system)
			{
				XmlDocument xmlDocument;
				if (!string.IsNullOrWhiteSpace(levelFile))
				{
					xmlDocument = new XmlDocument();
					xmlDocument.Load(ScriptHost.FileSystem, levelFile);
				}
				else
				{
					xmlDocument = CombatConfig.CreateEmptyCombatConfigXml();
				}
				CombatConfig.AppendConfigXml(xmlDocument, this._config);
				if (!string.Equals("data/scratch_combat.xml", levelFile))
				{
					xmlDocument.Save(Path.Combine(game.GameRoot, "data/scratch_combat.xml"));
				}
				CombatState gameState = game.GetGameState<CombatState>();
				game.SwitchGameState(gameState, new object[]
				{
					system,
					xmlDocument
				});
				DesignScreenState.CombatShipQueue.SetText(game, "Starting combat!");
			}
			private static void SetText(App game, string message)
			{
				App.Log.Trace(message, "design");
				game.UI.SetText("combatStatus", message);
			}
		}
		private class BankFilter
		{
			public object Value
			{
				get;
				set;
			}
			public bool Enabled
			{
				get;
				set;
			}
		}
		private const string UICommitButton = "gameCommitButton";
		private const string UIExitButton = "gameExitButton";
		private const string UIDesignRemovedButton = "designRemove";
		private const string UIModuleSelectorPanel = "gameModuleSelector";
		private const string UIModuleList = "gameModuleList";
		private const string UIWeaponDesignSelectorPanel = "gameDesignSelector";
		private const string UIWeaponDesignList = "gameWeaponDesignList";
		private const string UIClassList = "gameClassList";
		private const string UIDesignList = "gameDesignList";
		private const string UICommandSectionList = "gameCommandList";
		private const string UICommandPanel = "CommandDesign";
		private const string UIMissionPanel = "MissionDesign";
		private const string UIEnginePanel = "EngineDesign";
		private const string UIMissionSectionList = "gameMissionList";
		private const string UIEngineSectionList = "gameEngineList";
		private const string UIFilterAll = "filterAll";
		private const string UIFilterVeryLight = "filterVeryLight";
		private const string UIFilterLight = "filterLight";
		private const string UIFilterMedium = "filterMedium";
		private const string UIFilterHeavy = "filterHeavy";
		private const string UIFilterVeryHeavy = "filterVeryHeavy";
		private const string UIFilterSuperHeavy = "filterSuperHeavy";
		private const string UIFilterModules = "filterModules";
		private const string UIFiltersToggle = "expandFilters";
		private const string UIFilterPanel = "BankFilter";
		private const string UISpecialList = "specialList";
		private DesignScreenState.CombatShipQueue _combatShipQueue;
		private GameObjectSet _crits;
		private OrbitCameraController _camera;
		private CombatInput _input;
		private AllShipData _selection;
		private ShipBuilder _builder;
		private Vector3 _playerColour1;
		private Vector3 _playerColour2;
		private Sky _sky;
		private TargetArena _targetArena;
		private static readonly bool _defaultShowDebugControls = true;
		private bool _showDebugControls = DesignScreenState._defaultShowDebugControls;
		private List<Faction> _factionsList = new List<Faction>();
		private bool _inWeaponTestScreen;
		private ShipHoloView _shipHoloView;
		private string _previousState;
		private int? _selectedDesign = null;
		private string _selectedDesignPW = "";
		private bool _swappedShip;
		private bool _shouldRefresh;
		private bool _updateCamTarget;
		private WeaponSelector _weaponSelector;
		private ModuleSelector _moduleSelector;
		private PsionicSelector _psionicSelector;
		private string _designName;
		private string _originalName;
		private int _deleteItemID;
		private string _deleteItemDialog;
		private int _PsionicIndex;
		private bool _RetrofitMode;
		private static readonly string[] _debugAnimTracks = new string[]
		{
			"idle",
			"combat_ready",
			"combat_unready"
		};
		private static readonly string[] _debugAnimMode = new string[]
		{
			"once",
			"loop",
			"hold"
		};
		private string _debugCurrentAnimTrack = string.Empty;
		private int _debugCurrentAnimMode;
		private ModuleShipData _selectedModule;
		private IWeaponShipData _selectedWeaponBank;
		private static readonly string DebugFactionsList = "debugFactionsList";
		private static readonly string DebugEmpireColorList = "debugEmpireColors";
		private static readonly string DebugShipColor = "debugShipColor";
		private static readonly string DebugAnimEdit = "debugAnimEdit";
		private static readonly string DebugAnimTracks = "debugAnimTracks";
		private static readonly string DebugAnimMode = "debugAnimMode";
		private static readonly string DebugPlayAnim = "debugPlayAnim";
		private static readonly string DebugStopAnim = "debugStopAnim";
		private static Dictionary<string, DesignScreenState.BankFilter> BankFilters = new Dictionary<string, DesignScreenState.BankFilter>
		{

			{
				"filterAll",
				new DesignScreenState.BankFilter
				{
					Value = 0,
					Enabled = false
				}
			},

			{
				"filterVeryLight",
				new DesignScreenState.BankFilter
				{
					Value = WeaponEnums.WeaponSizes.VeryLight,
					Enabled = false
				}
			},

			{
				"filterLight",
				new DesignScreenState.BankFilter
				{
					Value = WeaponEnums.WeaponSizes.Light,
					Enabled = false
				}
			},

			{
				"filterMedium",
				new DesignScreenState.BankFilter
				{
					Value = WeaponEnums.WeaponSizes.Medium,
					Enabled = false
				}
			},

			{
				"filterHeavy",
				new DesignScreenState.BankFilter
				{
					Value = WeaponEnums.WeaponSizes.Heavy,
					Enabled = false
				}
			},

			{
				"filterVeryHeavy",
				new DesignScreenState.BankFilter
				{
					Value = WeaponEnums.WeaponSizes.VeryHeavy,
					Enabled = false
				}
			},

			{
				"filterSuperHeavy",
				new DesignScreenState.BankFilter
				{
					Value = WeaponEnums.WeaponSizes.SuperHeavy,
					Enabled = false
				}
			},

			{
				"filterModules",
				new DesignScreenState.BankFilter
				{
					Value = 0,
					Enabled = false
				}
			}
		};
		private static bool FilterVisible = true;
		private bool _currentShipDirty;
		private bool _screenReady;
		private Dictionary<ShipSectionType, List<Dictionary<string, bool>>> _shipOptionGroups = new Dictionary<ShipSectionType, List<Dictionary<string, bool>>>();
		private DesignScreenState.CombatShipQueue GuaranteedCombatShipQueue
		{
			get
			{
				if (this._combatShipQueue == null)
				{
					this._combatShipQueue = new DesignScreenState.CombatShipQueue();
				}
				return this._combatShipQueue;
			}
		}
		private RealShipClasses SelectedClass
		{
			get
			{
				return this._selection.Factions.Current.SelectedClass.Class;
			}
		}
		public override void OnEngineMessage(InteropMessageID messageID, ScriptMessageReader mr)
		{
		}
		private static bool DesignScreenAllowsShipClass(ShipSectionAsset value)
		{
			if (value.CombatAIType == SectionEnumerations.CombatAiType.TrapDrone)
			{
				return false;
			}
			switch (value.RealClass)
			{
			case RealShipClasses.BoardingPod:
			case RealShipClasses.EscapePod:
			case RealShipClasses.Biomissile:
			case RealShipClasses.Station:
				return false;
			}
			return true;
		}
		protected override void OnPrepare(GameState prev, object[] stateParams)
		{
			if (base.App.LocalPlayer == null)
			{
				base.App.NewGame();
				int? homeworld = base.App.GameDatabase.GetPlayerInfo(base.App.LocalPlayer.ID).Homeworld;
				if (!homeworld.HasValue)
				{
					throw new ArgumentException("Design screen requires a home world.");
				}
				this._showDebugControls = true;
			}
			else
			{
				if (stateParams.Length == 2)
				{
					this._previousState = (string)stateParams[1];
					this._showDebugControls = (bool)stateParams[0];
				}
				else
				{
					this._showDebugControls = false;
				}
			}
			this._targetArena = new TargetArena(base.App, "human");
			this._crits = new GameObjectSet(base.App);
			this._sky = new Sky(base.App, SkyUsage.InSystem, 0);
			this._crits.Add(this._sky);
			this._camera = this._crits.Add<OrbitCameraController>(new object[]
			{
				string.Empty
			});
			this._shipHoloView = new ShipHoloView(base.App, this._camera);
			this._shipHoloView.PostSetProp("AutoFitToView", false);
			this._crits.Add(this._shipHoloView);
			this._input = this._crits.Add<CombatInput>(new object[]
			{
				string.Empty
			});
			this._input.PlayerId = base.App.LocalPlayer.ObjectID;
			this._input.PostSetProp("DisableMouseTimer", true);
			this._input.PostSetProp("DisableDragSelect", true);
			this._input.PostSetProp("DisableUserUnitSelect", true);
			this._input.PostSetProp("DisablePsiBar", true);
			this._input.PostSetProp("WeaponLauncher", 0);
			this._selection = new AllShipData();
			this._builder = new ShipBuilder(base.App);
			this._playerColour1 = new Vector3(255f, 55f, 55f);
			this._playerColour2 = new Vector3(55f, 55f, 255f);
			base.App.UI.LoadScreen("Design");
			base.App.UI.LoadScreen("DesignWeaponTest");
			base.App.Game.AvailableShipSectionsChanged();
			this._input.CameraID = this._camera.ObjectID;
			this._camera.MaxDistance = 2000f;
			this._camera.DesiredDistance = 200f;
			this._camera.DesiredYaw = 90f;
			this._camera.YawEnabled = false;
			this._factionsList.Clear();
			this._factionsList.AddRange(base.App.AssetDatabase.Factions);
			if (this._showDebugControls)
			{
				List<ShipSectionAsset> source = new List<ShipSectionAsset>(base.App.Game.GetAvailableShipSections(base.App.LocalPlayer.ID));
				foreach (ShipSectionAsset ssa in base.App.AssetDatabase.ShipSections)
				{
					if (!source.Any((ShipSectionAsset x) => x.FileName == ssa.FileName))
					{
						base.App.GameDatabase.InsertSectionAsset(ssa.FileName, base.App.LocalPlayer.ID);
					}
				}
				List<LogicalWeapon> source2 = new List<LogicalWeapon>(base.App.GameDatabase.GetAvailableWeapons(base.App.AssetDatabase, base.App.LocalPlayer.ID));
				foreach (LogicalWeapon lw in base.App.AssetDatabase.Weapons)
				{
					if (!source2.Any((LogicalWeapon x) => x.FileName == lw.FileName))
					{
						base.App.GameDatabase.InsertWeapon(lw, base.App.LocalPlayer.ID);
					}
				}
				List<LogicalModule> source3 = new List<LogicalModule>(base.App.GameDatabase.GetAvailableModules(base.App.AssetDatabase, base.App.LocalPlayer.ID));
				foreach (LogicalModule lm in base.App.AssetDatabase.Modules)
				{
					if (!source3.Any((LogicalModule x) => x.ModulePath == lm.ModulePath))
					{
						base.App.GameDatabase.InsertModule(lm, base.App.LocalPlayer.ID);
					}
				}
			}
			IEnumerable<Faction> arg_511_0 = base.App.AssetDatabase.Factions;
			IEnumerable<ShipSectionAsset> arg_56E_0;
			if (!this._showDebugControls)
			{
				arg_56E_0 = 
					from x in base.App.Game.GetAvailableShipSections(base.App.LocalPlayer.ID)
					where DesignScreenState.DesignScreenAllowsShipClass(x)
					select x;
			}
			else
			{
				arg_56E_0 = base.App.AssetDatabase.ShipSections;
			}
			IEnumerable<ShipSectionAsset> enumerable = arg_56E_0;
			foreach (ShipSectionAsset current in enumerable)
			{
				this.CollectWeapons(current);
			}
		}
		private void CollectWeapons(ShipSectionAsset section)
		{
			IEnumerable<Faction> factions = base.App.AssetDatabase.Factions;
			FactionShipData factionShipData = this._selection.Factions.FirstOrDefault((FactionShipData x) => x.Faction.Name == section.Faction);
			if (factionShipData == null)
			{
				FactionShipData factionShipData2 = new FactionShipData();
				factionShipData2.Faction = factions.First((Faction x) => x.Name == section.Faction);
				factionShipData = factionShipData2;
				this._selection.Factions.Add(factionShipData);
			}
			ClassShipData classShipData = factionShipData.Classes.FirstOrDefault((ClassShipData x) => x.Class == section.RealClass);
			if (classShipData == null)
			{
				classShipData = new ClassShipData
				{
					Class = section.RealClass
				};
				factionShipData.Classes.Add(classShipData);
			}
			SectionTypeShipData sectionTypeShipData = classShipData.SectionTypes.FirstOrDefault((SectionTypeShipData x) => x.SectionType == section.Type);
			if (sectionTypeShipData == null)
			{
				sectionTypeShipData = new SectionTypeShipData
				{
					SectionType = section.Type
				};
				classShipData.SectionTypes.Add(sectionTypeShipData);
			}
			SectionShipData sectionShipData = new SectionShipData();
			sectionShipData.Section = section;
			for (int i = 0; i < section.Banks.Length; i++)
			{
				LogicalBank bank = section.Banks[i];
				WeaponBankShipData weaponBankShipData = new WeaponBankShipData();
				weaponBankShipData.Section = sectionShipData;
				weaponBankShipData.BankIndex = i;
				weaponBankShipData.Bank = bank;
				if (!string.IsNullOrEmpty(bank.DefaultWeaponName))
				{
					weaponBankShipData.SelectedWeapon = base.App.AssetDatabase.Weapons.FirstOrDefault((LogicalWeapon x) => string.Equals(x.WeaponName, bank.DefaultWeaponName, StringComparison.InvariantCultureIgnoreCase));
				}
				this.CollectWeapons(section, weaponBankShipData, bank.TurretSize, bank.TurretClass, weaponBankShipData.SelectedWeapon);
				sectionShipData.WeaponBanks.Add(weaponBankShipData);
			}
			for (int j = 0; j < section.Modules.Length; j++)
			{
				ModuleShipData moduleShipData = new ModuleShipData();
				moduleShipData.Section = sectionShipData;
				moduleShipData.ModuleIndex = j;
				moduleShipData.ModuleMount = section.Modules[j];
				IEnumerable<LogicalModule> enumerable = LogicalModule.EnumerateModuleFits(base.App.GameDatabase.GetAvailableModules(base.App.AssetDatabase, base.App.LocalPlayer.ID), section, j, this._showDebugControls).ToList<LogicalModule>();
				foreach (LogicalModule module in enumerable)
				{
					ModuleData moduleData = new ModuleData();
					moduleData.Module = module;
					moduleData.SelectedPsionic = new List<SectionEnumerations.PsionicAbility>();
					for (int k = 0; k < module.NumPsionicSlots; k++)
					{
						moduleData.SelectedPsionic.Add(SectionEnumerations.PsionicAbility.None);
					}
					if (module.Banks.Length > 0)
					{
						if (!string.IsNullOrEmpty(module.Banks[0].DefaultWeaponName))
						{
							moduleData.SelectedWeapon = base.App.AssetDatabase.Weapons.FirstOrDefault((LogicalWeapon x) => string.Equals(x.WeaponName, module.Banks[0].DefaultWeaponName, StringComparison.InvariantCultureIgnoreCase));
						}
						this.CollectWeapons(section, moduleData, module.Banks[0].TurretSize, module.Banks[0].TurretClass, moduleData.SelectedWeapon);
					}
					moduleShipData.Modules.Add(moduleData);
				}
				if (moduleShipData.Modules.Count > 0)
				{
					sectionShipData.Modules.Add(moduleShipData);
				}
			}
			sectionTypeShipData.Sections.Add(sectionShipData);
			if (sectionTypeShipData.Sections.Count > 0)
			{
				sectionTypeShipData.SelectedSection = sectionTypeShipData.Sections[0];
			}
		}
		private void RepopulateDebugControls()
		{
			if (this._showDebugControls)
			{
				base.App.UI.ClearItems(DesignScreenState.DebugEmpireColorList);
				for (int i = 0; i < Player.DefaultPrimaryPlayerColors.Count; i++)
				{
					Vector3 vector = Player.DefaultPrimaryPlayerColors[i];
					base.App.UI.Send(new object[]
					{
						"AddItem",
						DesignScreenState.DebugEmpireColorList,
						i.ToString(),
						"colorbox"
					});
					base.App.UI.Send(new object[]
					{
						"SetItemColor",
						DesignScreenState.DebugEmpireColorList,
						i.ToString(),
						vector.X,
						vector.Y,
						vector.Z
					});
				}
				base.App.UI.ClearItems(DesignScreenState.DebugAnimTracks);
				for (int j = 0; j < DesignScreenState._debugAnimTracks.Length; j++)
				{
					base.App.UI.AddItem(DesignScreenState.DebugAnimTracks, string.Empty, j, DesignScreenState._debugAnimTracks[j]);
				}
				this.SetSelectedAnimTrack(DesignScreenState._debugAnimTracks[0], string.Empty);
				base.App.UI.ClearItems(DesignScreenState.DebugAnimMode);
				for (int k = 0; k < DesignScreenState._debugAnimMode.Length; k++)
				{
					base.App.UI.AddItem(DesignScreenState.DebugAnimMode, string.Empty, k, DesignScreenState._debugAnimMode[k]);
				}
				base.App.UI.SetSelection(DesignScreenState.DebugAnimMode, 0);
			}
		}
		private bool CanWeaponUseDesign(ShipSectionAsset section, WeaponEnums.TurretClasses turretClass, DesignInfo design)
		{
			RealShipClasses realShipClasses;
			if (!WeaponEnums.RequiresDesign(turretClass, out realShipClasses))
			{
				return false;
			}
			if (section.IsWraithAbductor)
			{
				return false;
			}
			DesignSectionInfo[] designSections = design.DesignSections;
			for (int i = 0; i < designSections.Length; i++)
			{
				DesignSectionInfo designSectionInfo = designSections[i];
				ShipSectionAsset shipSectionAsset = base.App.AssetDatabase.GetShipSectionAsset(designSectionInfo.FilePath);
				if ((!section.IsScavenger || design.Role == ShipRole.SLAVEDISK) && (section.IsScavenger || design.Role != ShipRole.SLAVEDISK) && shipSectionAsset != null && shipSectionAsset.RealClass == realShipClasses)
				{
					return true;
				}
			}
			return false;
		}
		private IEnumerable<int> CollectWeaponDesigns(ShipSectionAsset section, WeaponEnums.TurretClasses turretClass)
		{
			IEnumerable<DesignInfo> designInfosForPlayer = base.App.GameDatabase.GetDesignInfosForPlayer(base.App.LocalPlayer.ID);
			foreach (DesignInfo current in designInfosForPlayer)
			{
				if (this.CanWeaponUseDesign(section, turretClass, current))
				{
					yield return current.ID;
				}
			}
			yield break;
		}
		private void CollectWeapons(ShipSectionAsset section, IWeaponShipData weaponBank, WeaponEnums.WeaponSizes turretSize, WeaponEnums.TurretClasses turretClass, LogicalWeapon includeSelectedWeapon)
		{
			if (WeaponEnums.IsBattleRider(turretClass))
			{
				LogicalWeapon logicalWeapon = LogicalWeapon.EnumerateWeaponFits(section.Faction, section.SectionName, 
					from weapon in base.App.GameDatabase.GetAvailableWeapons(base.App.AssetDatabase, base.App.LocalPlayer.ID)
					where weapon.IsVisible
					select weapon, turretSize, turretClass).FirstOrDefault<LogicalWeapon>();
				if (logicalWeapon != null && !section.IsWraithAbductor)
				{
					weaponBank.RequiresDesign = (turretClass != WeaponEnums.TurretClasses.Biomissile);
					weaponBank.DesignIsSelectable = WeaponEnums.DesignIsSelectable(turretClass);
					if (turretClass == WeaponEnums.TurretClasses.Biomissile)
					{
						IEnumerable<LogicalWeapon> collection = LogicalWeapon.EnumerateWeaponFits(section.Faction, section.SectionName, 
							from weapon in base.App.GameDatabase.GetAvailableWeapons(base.App.AssetDatabase, base.App.LocalPlayer.ID)
							where weapon.IsVisible
							select weapon, turretSize, turretClass);
						weaponBank.Weapons.AddRange(collection);
					}
					else
					{
						weaponBank.Weapons.Add(logicalWeapon);
					}
					weaponBank.Designs.AddRange(this.CollectWeaponDesigns(section, turretClass));
				}
			}
			else
			{
				IEnumerable<LogicalWeapon> collection2 = LogicalWeapon.EnumerateWeaponFits(section.Faction, section.SectionName, 
					from weapon in base.App.GameDatabase.GetAvailableWeapons(base.App.AssetDatabase, base.App.LocalPlayer.ID)
					where weapon.IsVisible
					select weapon, turretSize, turretClass);
				weaponBank.Weapons.AddRange(collection2);
			}
			if (includeSelectedWeapon != null && !weaponBank.Weapons.Contains(includeSelectedWeapon))
			{
				weaponBank.Weapons.Add(includeSelectedWeapon);
			}
			if (weaponBank.Weapons.Count > 0)
			{
				weaponBank.SelectedWeapon = weaponBank.Weapons[0];
			}
			if (weaponBank.Designs.Count > 0)
			{
				weaponBank.SelectedDesign = weaponBank.Designs[0];
			}
		}
		private void EnableWeaponTestMode(bool enabled)
		{
			base.App.UI.SetVisible("pnlMainDesign", !enabled);
			base.App.UI.SetVisible("pnlDesignWeaponTest", enabled);
			if (enabled)
			{
				base.App.UI.SetParent("gameWeaponsPanel2", "pnlWeaponsTest");
			}
			else
			{
				base.App.UI.SetParent("gameWeaponsPanel2", "pnlWeapons");
			}
			base.App.UI.SetVisible("psionicArea", false);
		}
		protected override void OnEnter()
		{
			base.App.UI.UnlockUI();
			if (base.App.LocalPlayer == null)
			{
				base.App.NewGame();
			}
			this._weaponSelector = new WeaponSelector(base.App.UI, "gameWeaponSelector", "");
			this._weaponSelector.SelectedWeaponChanged += new WeaponSelectionChangedEventHandler(this.WeaponSelectorSelectedWeaponChanged);
			this._moduleSelector = new ModuleSelector(base.App.UI, "gameModuleSelector", "");
			this._moduleSelector.SelectedModuleChanged += new ModuleSelectionChangedEventHandler(this.ModuleSelectorSelectedModuleChanged);
			this._psionicSelector = new PsionicSelector(base.App.UI, "psionicSelector", "");
			this._psionicSelector.SelectedPsionicChanged += new PsionicSelectionChangedEventHandler(this.PsionicSelectorSelectedPsionicChanged);
			this._targetArena.Activate();
			this._sky.Active = true;
			this._input.Active = true;
			this._camera.Active = true;
			this._input.PostSetProp("DisableCombatInputMouseOver", true);
			base.App.UI.SetScreen("Design");
			this._inWeaponTestScreen = false;
			base.App.UI.GameEvent += new UIEventGameEvent(this.UICommChannel_GameEvent);
			base.App.UI.SetVisible("gameDebugControls", this._showDebugControls);
			base.App.UI.Send(new object[]
			{
				"SetGameObject",
				"designShip",
				this._shipHoloView.ObjectID
			});
			base.App.UI.SetPropertyBool("gameModuleList", "only_user_events", true);
			base.App.UI.SetPropertyBool("gameWeaponDesignList", "only_user_events", true);
			base.App.UI.SetPropertyBool(DesignScreenState.UISectionList(ShipSectionType.Command), "only_user_events", true);
			base.App.UI.SetPropertyBool(DesignScreenState.UISectionList(ShipSectionType.Engine), "only_user_events", true);
			base.App.UI.SetPropertyBool(DesignScreenState.UISectionList(ShipSectionType.Mission), "only_user_events", true);
			this._RetrofitMode = false;
			EmpireBarUI.SyncTitleFrame(base.App);
			this.PopulateFactionsList();
			string factionName = base.App.GameDatabase.GetFactionName(base.App.GameDatabase.GetPlayerFactionID(base.App.LocalPlayer.ID));
			this.SetSelectedFaction(factionName, "init");
			this.SetSelectedClass(RealShipClasses.Cruiser, "init");
			this.PopulateSectionLists();
			this.PopulateDesignList();
			base.App.UI.SetEnabled("designRemove", false);
			this._screenReady = true;
			this._shouldRefresh = true;
			base.App.PostEngineMessage(new object[]
			{
				InteropMessageID.IMID_ENGINE_SET_AUTHORITIVE_STATE,
				true
			});
			base.App.UI.SetChecked("filterAll", true);
			this.SelectFilter("filterAll", true);
			base.App.UI.Send(new object[]
			{
				"EnableScriptMessages",
				"gameWeaponsPanel2",
				true
			});
			base.App.HotKeyManager.AddListener(this);
		}
		private void WeaponSelectorSelectedWeaponChanged(object sender, bool isRightClick)
		{
			if (this._selection == null || this._weaponSelector == null)
			{
				return;
			}
			WeaponAssignment weaponAssignment = this._selection.GetWeaponAssignments().FirstOrDefault((WeaponAssignment x) => x.Weapon == this._weaponSelector.SelectedWeapon);
			int? num = null;
			int? num2 = null;
			if (weaponAssignment != null)
			{
				num = weaponAssignment.InitialFireMode;
				num2 = weaponAssignment.InitialTargetFilter;
			}
			IWeaponShipData selectedBank = this.GetWeaponSelectorBank();
			if (selectedBank != null && selectedBank.SelectedWeapon != this._weaponSelector.SelectedWeapon)
			{
				selectedBank.SelectedWeapon = this._weaponSelector.SelectedWeapon;
				selectedBank.FiringMode = num;
				selectedBank.FilterMode = num2;
				WeaponAssignment weaponAssignment2 = this._selection.GetWeaponAssignments().FirstOrDefault((WeaponAssignment x) => x.Bank == selectedBank.Bank);
				if (weaponAssignment2 != null)
				{
					weaponAssignment2.InitialFireMode = num;
					weaponAssignment2.InitialTargetFilter = num2;
				}
				if (isRightClick)
				{
					LogicalBank logicalBank;
					if (selectedBank is WeaponBankShipData)
					{
						logicalBank = (selectedBank as WeaponBankShipData).Bank;
					}
					else
					{
						if (selectedBank is ModuleShipData)
						{
							logicalBank = (selectedBank as ModuleShipData).SelectedModule.Module.Banks.First<LogicalBank>();
						}
						else
						{
							logicalBank = null;
						}
					}
					if (logicalBank != null)
					{
						this.SetWeaponsAll(this._weaponSelector.SelectedWeapon, logicalBank.TurretSize, logicalBank.TurretClass, num, num2);
					}
				}
				if (!this._RetrofitMode)
				{
					this._selectedDesign = null;
					this._selectedDesignPW = "";
				}
			}
			this.CurrentShipChanged();
			this.HideWeaponSelector();
		}
		private void PsionicSelectorSelectedPsionicChanged(object sender, bool isRightClick)
		{
			ModuleShipData selectedModule = this._selectedModule;
			if (selectedModule != null)
			{
				if (this._psionicSelector.SelectedPsionic.Name != "No Psionic" && this._psionicSelector.SelectedPsionic.Name != "")
				{
					selectedModule.SelectedModule.SelectedPsionic[this._PsionicIndex] = (SectionEnumerations.PsionicAbility)Enum.Parse(typeof(SectionEnumerations.PsionicAbility), this._psionicSelector.SelectedPsionic.Name);
				}
				else
				{
					selectedModule.SelectedModule.SelectedPsionic[this._PsionicIndex] = SectionEnumerations.PsionicAbility.None;
				}
			}
			if (!this._RetrofitMode)
			{
				this._selectedDesign = null;
				this._selectedDesignPW = "";
			}
			this.CurrentShipChanged();
			this.HidePsionicSelector();
		}
		private void ModuleSelectorSelectedModuleChanged(object sender, bool isRightClick)
		{
			ModuleShipData selectedModule2 = this._selectedModule;
			if (selectedModule2 != null)
			{
				LogicalModule selectedModule = this._moduleSelector.SelectedModule;
				if (selectedModule.ModuleName == App.Localize("@UI_MODULENAME_NO_MODULE"))
				{
					selectedModule2.SelectedModule = null;
					this.CurrentShipChanged();
				}
				else
				{
					if (selectedModule2.SelectedModule == null || selectedModule != selectedModule2.SelectedModule.Module)
					{
						selectedModule2.SelectedModule = selectedModule2.Modules.First((ModuleData x) => x.Module == selectedModule);
						this.CurrentShipChanged();
					}
				}
			}
			if (!this._RetrofitMode)
			{
				this._selectedDesign = null;
				this._selectedDesignPW = "";
			}
			this.HideModuleSelector();
		}
		protected override void OnExit(GameState prev, ExitReason reason)
		{
			base.App.HotKeyManager.RemoveListener(this);
			this._designName = string.Empty;
			if (this._weaponSelector != null)
			{
				this._weaponSelector.Dispose();
				this._weaponSelector = null;
			}
			if (this._psionicSelector != null)
			{
				this._psionicSelector.Dispose();
				this._psionicSelector = null;
			}
			if (this._moduleSelector != null)
			{
				this._moduleSelector.Dispose();
				this._moduleSelector = null;
			}
			if (this._targetArena != null)
			{
				this._targetArena.Dispose();
				this._targetArena = null;
			}
			this._combatShipQueue = null;
			this._camera.Active = false;
			this._camera.TargetID = 0;
			this._builder.Dispose();
			this._input.Active = false;
			if (this._crits != null)
			{
				this._crits.Dispose();
				this._crits = null;
			}
			base.App.UI.GameEvent -= new UIEventGameEvent(this.UICommChannel_GameEvent);
			this._showDebugControls = DesignScreenState._defaultShowDebugControls;
		}
		protected override void OnUpdate()
		{
			if (this._targetArena != null)
			{
				this._targetArena.Update();
			}
			if (!this._builder.Loading)
			{
				if (!this._swappedShip)
				{
					this.SyncWeaponUi();
				}
				this._swappedShip = true;
			}
			else
			{
				this._swappedShip = false;
			}
			this._builder.Update();
			if (this._builder.Ship != null && this._builder.Ship.Active && (this._camera.TargetID != this._builder.Ship.ObjectID || this._updateCamTarget))
			{
				if (this._camera.TargetID == this._builder.Ship.GetObjectID() && this._updateCamTarget)
				{
					this._camera.PostSetProp("TargetID", this._builder.Ship.GetObjectID());
				}
				else
				{
					this._camera.TargetID = this._builder.Ship.GetObjectID();
				}
				this._updateCamTarget = false;
			}
			this.RefreshCurrentShip();
		}
		public override bool IsReady()
		{
			return this._crits != null && this._crits.IsReady() && base.IsReady();
		}
		public DesignScreenState(App game) : base(game)
		{
		}
		private void UICommChannel_GameEvent(string eventName, string[] eventParams)
		{
			if (eventName == "IconClicked" && eventParams.Length == 1)
			{
				string[] array = eventParams[0].Split(new char[]
				{
					':'
				});
				string a = array[0];
				if (a == "WeaponGroup")
				{
					WeaponBank gameObject = base.App.GetGameObject<WeaponBank>(int.Parse(array[1]));
					IWeaponShipData currentWeaponBank = this._selection.GetCurrentWeaponBank(gameObject);
					this.HideModuleSelector();
					if (this._selectedWeaponBank == currentWeaponBank)
					{
						this.HideWeaponSelector();
						return;
					}
					this.ShowWeaponSelector(currentWeaponBank);
					return;
				}
				else
				{
					if (a == "Module")
					{
						Kerberos.Sots.GameObjects.Section gameObject2 = base.App.GetGameObject<Kerberos.Sots.GameObjects.Section>(int.Parse(array[1]));
						string mountNodeName = array[2];
						ModuleShipData currentModuleMount = this._selection.GetCurrentModuleMount(gameObject2.ShipSectionAsset, mountNodeName);
						this.HideWeaponSelector();
						if (this._selectedModule == currentModuleMount)
						{
							this.HideModuleSelector();
							return;
						}
						this.ShowModuleSelector(currentModuleMount);
						return;
					}
					else
					{
						if (a == "Psionic")
						{
							Module gameObject3 = base.App.GetGameObject<Module>(int.Parse(array[1]));
							int num = int.Parse(array[2]);
							ModuleShipData currentModuleMount2 = this._selection.GetCurrentModuleMount(gameObject3.AttachedSection.ShipSectionAsset, gameObject3._attachment.NodeName);
							if (this._selectedModule == currentModuleMount2)
							{
								this.HidePsionicSelector();
								return;
							}
							this._selectedModule = currentModuleMount2;
							this._PsionicIndex = num;
							this.ShowPsionicSelector(gameObject3, num);
						}
					}
				}
			}
		}
		private void HideModuleSelector()
		{
			if (this._selectedModule != null)
			{
				this._shipHoloView.ClearSelection();
			}
			this._selectedModule = null;
			this._moduleSelector.SetVisible(false);
		}
		private void ShowModuleSelector(ModuleShipData moduleData)
		{
			bool flag = true;
			if (moduleData != null)
			{
				this._selectedModule = moduleData;
				if (this._RetrofitMode && this._selectedModule.SelectedModule != null)
				{
					if (this._selectedModule.SelectedModule.Module.Banks.Any((LogicalBank x) => WeaponEnums.IsBattleRider(x.TurretClass)))
					{
						return;
					}
				}
				if (moduleData.Modules.Count > 0)
				{
					this.PopulateModuleSelector(moduleData.Modules);
					flag = false;
				}
			}
			if (flag)
			{
				this.HideModuleSelector();
			}
		}
		private void HideWeaponSelector()
		{
			if (this._selectedWeaponBank != null)
			{
				this._shipHoloView.ClearSelection();
			}
			this._selectedWeaponBank = null;
			this._weaponSelector.SetVisible(false);
			base.App.UI.SetVisible("gameDesignSelector", false);
		}
		private void HidePsionicSelector()
		{
			if (this._selectedModule != null)
			{
				this._shipHoloView.ClearSelection();
			}
			this._selectedModule = null;
			this._psionicSelector.SetVisible(false);
			base.App.UI.SetVisible(this._psionicSelector.ID, false);
		}
		private void PopulateWeaponSelector(List<LogicalWeapon> weapons, LogicalWeapon selected, LogicalBank bank)
		{
			base.App.UI.MovePanelToMouse(this._weaponSelector.ID, UICommChannel.AnchorPoint.TopLeft, new Vector2(-4f, -4f));
			if (this._RetrofitMode && bank != null)
			{
				List<LogicalWeapon> list = (
					from x in weapons
					where x.TurretClasses.Any((LogicalTurretClass j) => j.TurretClass == bank.TurretClass) && x.PayloadType == selected.PayloadType && x.DefaultWeaponSize == selected.DefaultWeaponSize
					select x).ToList<LogicalWeapon>();
				if (selected.PayloadType == WeaponEnums.PayloadTypes.Bolt)
				{
					bool islaser = selected.Traits.Contains(WeaponEnums.WeaponTraits.Laser);
					list = (
						from x in list
						where islaser == x.Traits.Contains(WeaponEnums.WeaponTraits.Laser)
						select x).ToList<LogicalWeapon>();
					bool ballistic = selected.Traits.Contains(WeaponEnums.WeaponTraits.Ballistic);
					list = (
						from x in list
						where ballistic == x.Traits.Contains(WeaponEnums.WeaponTraits.Ballistic)
						select x).ToList<LogicalWeapon>();
				}
				this._weaponSelector.SetAvailableWeapons(list, selected);
			}
			else
			{
				this._weaponSelector.SetAvailableWeapons(
					from x in weapons
					orderby x.DefaultWeaponSize
					select x, selected);
			}
			this._weaponSelector.SetVisible(true);
		}
		private void PopulateModuleSelector(List<ModuleData> modules)
		{
			base.App.UI.MovePanelToMouse(this._moduleSelector.ID, UICommChannel.AnchorPoint.TopLeft, new Vector2(-4f, -4f));
			ModuleTilePanel arg_C4_0 = this._moduleSelector;
			IEnumerable<LogicalModule> arg_C4_1;
			if (!this._RetrofitMode)
			{
				arg_C4_1 = 
					from x in modules
					select x.Module;
			}
			else
			{
				arg_C4_1 = 
					from x in modules.Where(delegate(ModuleData x)
					{
						if (x.Module.Banks.Any<LogicalBank>())
						{
							return x.Module.Banks.Any((LogicalBank j) => !WeaponEnums.IsBattleRider(j.TurretClass));
						}
						return true;
					})
					select x.Module;
			}
			arg_C4_0.SetAvailableModules(arg_C4_1, (this._selectedModule.SelectedModule != null) ? this._selectedModule.SelectedModule.Module : null, true);
			this._moduleSelector.SetVisible(true);
		}
		private bool PsiNotSelected(LogicalPsionic psionic)
		{
			SectionEnumerations.PsionicAbility psi = (SectionEnumerations.PsionicAbility)Enum.Parse(typeof(SectionEnumerations.PsionicAbility), psionic.Name);
			IEnumerable<ModuleAssignment> moduleAssignments = this._selection.GetModuleAssignments();
			moduleAssignments.ToList<ModuleAssignment>();
			return !moduleAssignments.Any((ModuleAssignment x) => x.PsionicAbilities.Any((SectionEnumerations.PsionicAbility y) => y == psi));
		}
		private void PopulatePsionicSelector(IEnumerable<LogicalPsionic> psionics, LogicalPsionic selected)
		{
			base.App.UI.MovePanelToMouse(this._psionicSelector.ID, UICommChannel.AnchorPoint.TopLeft, new Vector2(-4f, 4f));
			IEnumerable<LogicalPsionic> modules = 
				from x in psionics
				where this.PsiNotSelected(x) && x.IsAvailable(base.App.GameDatabase, base.App.LocalPlayer.ID, false)
				select x;
			this._psionicSelector.SetAvailablePsionics(modules, selected, true);
			this._psionicSelector.SetVisible(true);
		}
		private void PopulateWeaponDesignSelector(List<int> weaponDesigns, int selected)
		{
			base.App.UI.SetVisible("gameDesignSelector", true);
			base.App.UI.ClearItems("gameWeaponDesignList");
			int num = -1;
			for (int num2 = 0; num2 != weaponDesigns.Count; num2++)
			{
				int num3 = weaponDesigns[num2];
				DesignInfo designInfo = base.App.GameDatabase.GetDesignInfo(num3);
				base.App.UI.AddItem("gameWeaponDesignList", string.Empty, num2, designInfo.Name);
				if (num3 == selected)
				{
					num = num2;
				}
			}
			if (num != -1)
			{
				base.App.UI.SetSelection("gameWeaponDesignList", num);
			}
		}
		private void ShowWeaponSelector(IWeaponShipData weaponBankData)
		{
			bool flag = true;
			if (weaponBankData != null)
			{
				this._selectedWeaponBank = weaponBankData;
				if (weaponBankData.RequiresDesign)
				{
					if (weaponBankData.DesignIsSelectable && weaponBankData.Designs.Count > 0)
					{
						this.PopulateWeaponDesignSelector(weaponBankData.Designs, weaponBankData.SelectedDesign);
						flag = false;
					}
				}
				else
				{
					if (weaponBankData.Weapons.Count > 0)
					{
						this.PopulateWeaponSelector(weaponBankData.Weapons, weaponBankData.SelectedWeapon, weaponBankData.Bank);
						flag = false;
					}
				}
			}
			if (flag)
			{
				this.HideWeaponSelector();
				this.HidePsionicSelector();
			}
		}
		private void ShowPsionicSelector(Module module, int psiindex)
		{
			bool flag = true;
			if (module != null)
			{
				this.PopulatePsionicSelector(base.App.AssetDatabase.Psionics, module._module.Psionics[psiindex]);
			}
			if (!flag)
			{
				this.HidePsionicSelector();
			}
		}
		private static string UISectionList(ShipSectionType sectionType)
		{
			switch (sectionType)
			{
			case ShipSectionType.Command:
				return "gameCommandList";
			case ShipSectionType.Mission:
				return "gameMissionList";
			case ShipSectionType.Engine:
				return "gameEngineList";
			default:
				throw new ArgumentOutOfRangeException("sectionType");
			}
		}
		private static string UISectionStats(ShipSectionType sectionType)
		{
			switch (sectionType)
			{
			case ShipSectionType.Command:
				return "CommandDesign";
			case ShipSectionType.Mission:
				return "MissionDesign";
			case ShipSectionType.Engine:
				return "EngineDesign";
			default:
				throw new ArgumentOutOfRangeException("sectionType");
			}
		}
		protected void SelectFilter(string panel, bool enabled)
		{
			if (DesignScreenState.BankFilters[panel].Enabled == enabled)
			{
				return;
			}
			DesignScreenState.BankFilters[panel].Enabled = enabled;
			if (enabled)
			{
				if (panel == "filterAll")
				{
					using (Dictionary<string, DesignScreenState.BankFilter>.Enumerator enumerator = DesignScreenState.BankFilters.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							KeyValuePair<string, DesignScreenState.BankFilter> current = enumerator.Current;
							if (current.Key != panel)
							{
								current.Value.Enabled = false;
								base.App.UI.SetChecked(current.Key, false);
							}
						}
						goto IL_C5;
					}
				}
				DesignScreenState.BankFilters["filterAll"].Enabled = false;
				base.App.UI.SetChecked("filterAll", false);
			}
			IL_C5:
			if (!(
				from x in DesignScreenState.BankFilters
				where x.Value.Enabled
				select x).Any<KeyValuePair<string, DesignScreenState.BankFilter>>())
			{
				DesignScreenState.BankFilters["filterAll"].Enabled = true;
				base.App.UI.SetChecked("filterAll", true);
			}
			this._currentShipDirty = true;
		}
		protected override void UICommChannel_OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "firing_mode")
			{
				this.HideModuleSelector();
				this.HidePsionicSelector();
				this.HideWeaponSelector();
				int id = int.Parse(msgParams[0]);
				int fireMode = int.Parse(msgParams[1]);
				WeaponBank weaponBank = (WeaponBank)base.App.GetGameObject(id);
				if (weaponBank != null)
				{
					this.SetWeaponsAllFireMode(weaponBank.Weapon, fireMode);
				}
			}
			else
			{
				if (msgType == "filter_mode")
				{
					this.HideModuleSelector();
					this.HidePsionicSelector();
					this.HideWeaponSelector();
					int id2 = int.Parse(msgParams[0]);
					int filterMode = int.Parse(msgParams[1]);
					WeaponBank weaponBank2 = (WeaponBank)base.App.GetGameObject(id2);
					if (weaponBank2 != null)
					{
						this.SetWeaponsAllFilterMode(weaponBank2.Weapon, filterMode);
					}
				}
				else
				{
					if (msgType == "dialog_closed")
					{
						if (panelName == this._deleteItemDialog)
						{
							bool flag = bool.Parse(msgParams[0]);
							if (flag)
							{
								base.App.GameDatabase.RemovePlayerDesign(this._deleteItemID);
								this.PopulateDesignList();
							}
						}
					}
					else
					{
						if (msgType == "text_changed")
						{
							if (panelName == "edit_design_name")
							{
								this._designName = msgParams[0];
							}
							if (panelName == DesignScreenState.DebugAnimEdit)
							{
								this.SetSelectedAnimTrack(msgParams[0], DesignScreenState.DebugAnimEdit);
							}
						}
						else
						{
							if (msgType == "checkbox_clicked")
							{
								if (panelName.Contains("filter"))
								{
									this.SelectFilter(panelName, int.Parse(msgParams[0]) != 0);
								}
								else
								{
									if (panelName.Contains("tech"))
									{
										this._RetrofitMode = false;
										this.SelectTech(panelName, int.Parse(msgParams[0]) != 0);
										this._shouldRefresh = true;
										base.App.UI.ClearSelection("gameDesignList");
										this._selectedDesign = null;
										this._selectedDesignPW = "";
										this.CurrentShipChanged();
									}
									else
									{
										if ("debugAutoAssignModules" == panelName)
										{
											bool debugAutoAssignModules = int.Parse(msgParams[0]) != 0;
											ModuleShipData.DebugAutoAssignModules = debugAutoAssignModules;
											this.CurrentShipChanged();
										}
									}
								}
							}
							else
							{
								if (msgType == "button_clicked")
								{
									if (panelName == "designRemove")
									{
										if (this._selectedDesign.HasValue)
										{
											base.App.GameDatabase.RemovePlayerDesign(this._selectedDesign.Value);
											this.PopulateDesignList();
										}
									}
									else
									{
										if (panelName.Contains("designDeleteButton"))
										{
											string[] array = panelName.Split(new char[]
											{
												'|'
											});
											this._deleteItemID = int.Parse(array[1]);
											DesignInfo designInfo = base.App.GameDatabase.GetDesignInfo(this._deleteItemID);
											if (designInfo != null)
											{
												this._deleteItemDialog = base.App.UI.CreateDialog(new GenericQuestionDialog(base.App, App.Localize("@UI_DESIGN_DELETE_TITLE"), string.Format(App.Localize("@UI_DESIGN_DELETE_DESC"), designInfo.Name), "dialogGenericQuestion"), null);
											}
										}
										else
										{
											if (panelName == "expandFilters")
											{
												DesignScreenState.FilterVisible = !DesignScreenState.FilterVisible;
												base.App.UI.SetVisible("BankFilter", DesignScreenState.FilterVisible);
											}
											else
											{
												if (panelName == "gameTutorialButton")
												{
													this.HideModuleSelector();
													this.HidePsionicSelector();
													this.HideWeaponSelector();
													base.App.UI.SetVisible("DesignScreenTutorial", true);
												}
												else
												{
													if (panelName == "designScreenTutImage")
													{
														this.HideModuleSelector();
														this.HidePsionicSelector();
														this.HideWeaponSelector();
														base.App.UI.SetVisible("DesignScreenTutorial", false);
													}
													else
													{
														if (panelName == "missileTest_1")
														{
															if (this._targetArena != null)
															{
																this._targetArena.LaunchWeapon(this._builder.Ship, 5);
															}
														}
														else
														{
															if (panelName == "missileTest_2")
															{
																if (this._targetArena != null)
																{
																	this._targetArena.LaunchWeapon(this._builder.Ship, 10);
																}
															}
															else
															{
																if (panelName == "missileTest_3")
																{
																	if (this._targetArena != null)
																	{
																		this._targetArena.LaunchWeapon(this._builder.Ship, 20);
																	}
																}
																else
																{
																	if (panelName == "gameExitButton")
																	{
																		if (this._inWeaponTestScreen)
																		{
																			this.EnableWeaponTestMode(false);
																			this._inWeaponTestScreen = false;
																			this._shouldRefresh = true;
																			this._updateCamTarget = true;
																			this._shipHoloView.PostSetProp("CenterCamera", new object[0]);
																			if (this._builder.Ship != null)
																			{
																				this._builder.Ship.Maneuvering.PostSetProp("ResetPosition", new object[0]);
																				this._builder.Ship.PostSetProp("SetValidateCurrentPosition", false);
																				this._builder.Ship.PostSetProp("StopAnims", new object[0]);
																				this._builder.Ship.PostSetProp("ClearDamageVisuals", new object[0]);
																				this._builder.Ship.SetShipTarget(0, Vector3.Zero, true, 0);
																				this._builder.Ship.PostSetProp("SetDisableLaunching", true);
																				this._builder.Ship.PostSetProp("FullyHealShip", new object[0]);
																				this._builder.Ship.PostSetProp("SetCombatReady", new object[]
																				{
																					true,
																					2f
																				});
																				this._targetArena.ResetTargetPositions();
																				this._builder.ForceSyncRiders();
																				base.App.PostEngineMessage(new object[]
																				{
																					InteropMessageID.IMID_ENGINE_CLEAR_WEAPON_SPAWNS
																				});
																			}
																			this._camera.DesiredYaw = MathHelper.DegreesToRadians(-91f);
																			this._camera.DesiredYaw = MathHelper.DegreesToRadians(-90f);
																			this._camera.YawEnabled = false;
																			this._camera.MaxDistance = 2000f;
																			this._input.PostSetProp("DisableCombatInputMouseOver", true);
																			this._input.PostSetProp("WeaponLauncher", 0);
																		}
																		else
																		{
																			base.App.UI.LockUI();
																			if (this._previousState == "StarMapState")
																			{
																				base.App.SwitchGameState<StarMapState>(new object[0]);
																			}
																			else
																			{
																				if (this._previousState == "BuildScreenState")
																				{
																					base.App.SwitchGameState<BuildScreenState>(new object[0]);
																				}
																				else
																				{
																					base.App.SwitchGameState<StarMapState>(new object[0]);
																				}
																			}
																		}
																	}
																	else
																	{
																		if (panelName == "gameCommitButton")
																		{
																			this.ShowCommitDialog();
																		}
																		else
																		{
																			if (panelName == "submit_dialog_ok")
																			{
																				base.App.SteamHelper.DoAchievement(AchievementType.SOTS2_SHIPYARD_SUPERVISOR);
																				this.CommitDesign();
																				base.App.UI.SetVisible("DesignUpdate1", true);
																				base.App.UI.SetVisible("DesignUpdate2", true);
																				base.App.UI.SetVisible("DesignUpdate3", true);
																				base.App.UI.SetVisible("DesignUpdate4", true);
																			}
																			else
																			{
																				if (panelName == "submit_dialog_cancel")
																				{
																					base.App.UI.SetVisible("submit_dialog", false);
																				}
																				else
																				{
																					if ("starmap" == panelName)
																					{
																						base.App.SwitchGameState<StarMapState>(new object[0]);
																					}
																					else
																					{
																						if ("combatAddCurrentDesign" == panelName)
																						{
																							this.GuaranteedCombatShipQueue.AddDesign(base.App, this._builder, 1, base.App.LocalPlayer.ID);
																						}
																						else
																						{
																							if ("combatAddCurrentDesignX10" == panelName)
																							{
																								this.GuaranteedCombatShipQueue.AddDesign(base.App, this._builder, 10, base.App.LocalPlayer.ID);
																							}
																							else
																							{
																								if ("combatAddCurrentDesignAsAI" == panelName)
																								{
																									this.GuaranteedCombatShipQueue.AddDesign(base.App, this._builder, 1, base.App.Game.OtherPlayers[0].ID);
																								}
																								else
																								{
																									if (!("combatStartRandom" == panelName))
																									{
																										if ("combatStart" == panelName)
																										{
																											this.GuaranteedCombatShipQueue.Start(base.App, null, 0);
																										}
																										else
																										{
																											if ("combatStartNearStar" == panelName)
																											{
																												this.GuaranteedCombatShipQueue.Start(base.App, "data\\CombatNearStar.xml", 0);
																											}
																											else
																											{
																												if ("combatStartNearBigStar" == panelName)
																												{
																													this.GuaranteedCombatShipQueue.Start(base.App, "data\\CombatNearGiantStar.xml", 0);
																												}
																												else
																												{
																													if ("combatStartNearEarth" == panelName)
																													{
																														this.GuaranteedCombatShipQueue.Start(base.App, "data\\CombatNearEarth.xml", 0);
																													}
																													else
																													{
																														if ("combatStartNearSaturn" == panelName)
																														{
																															this.GuaranteedCombatShipQueue.Start(base.App, "data\\CombatNearSaturn.xml", 0);
																														}
																														else
																														{
																															if ("combatNuPlanets" == panelName)
																															{
																																this.GuaranteedCombatShipQueue.Start(base.App, "data\\CombatNuPlanets.xml", 0);
																															}
																															else
																															{
																																if ("combatStartSystem" == panelName)
																																{
																																	this.GuaranteedCombatShipQueue.Start(base.App, "data\\CombatRandomSystem.xml", 0);
																																}
																																else
																																{
																																	if ("combatStartScratch" == panelName)
																																	{
																																		this.GuaranteedCombatShipQueue.Start(base.App, "data/scratch_combat.xml", 0);
																																	}
																																	else
																																	{
																																		if ("weaponTest" == panelName)
																																		{
																																			this.HideModuleSelector();
																																			this.HideWeaponSelector();
																																			this.HidePsionicSelector();
																																			base.App.PostEnableSpeechSounds(true);
																																			this.EnableWeaponTestMode(true);
																																			base.App.UI.SetVisible("debugStuff", this._showDebugControls);
																																			this.RepopulateDebugControls();
																																			this._inWeaponTestScreen = true;
																																			this._input.PostSetProp("DisableCombatInputMouseOver", false);
																																			this._input.PostSetProp("WeaponLauncher", this._targetArena.WeaponLauncherID);
																																			if (this._builder.Ship != null)
																																			{
																																				this._builder.Ship.PostSetProp("SetValidateCurrentPosition", false);
																																				this._builder.Ship.PostSetProp("StopAnims", new object[0]);
																																				this._builder.Ship.PostSetProp("SetCombatReady", new object[]
																																				{
																																					true,
																																					2f
																																				});
																																				this._builder.Ship.Maneuvering.PostSetProp("AllowFixedBeamRotate", false);
																																				this._builder.Ship.PostSetProp("SetDisableLaunching", false);
																																			}
																																			this._camera.YawEnabled = true;
																																			this._camera.MaxDistance = 7500f;
																																		}
																																		else
																																		{
																																			if (panelName == DesignScreenState.DebugPlayAnim)
																																			{
																																				this.PlayAnim();
																																			}
																																			else
																																			{
																																				if (panelName == DesignScreenState.DebugStopAnim)
																																				{
																																					this.StopAnim();
																																				}
																																				else
																																				{
																																					if (panelName == "gameRetrofitButton" && this._selectedDesign.HasValue)
																																					{
																																						DesignInfo designInfo2 = base.App.GameDatabase.GetDesignInfo(this._selectedDesign.Value);
																																						IEnumerable<PlayerTechInfo> playerTechInfos = base.App.GameDatabase.GetPlayerTechInfos(base.App.LocalPlayer.ID);
																																						bool flag2 = playerTechInfos.Any((PlayerTechInfo x) => x.State == TechStates.Researched && x.TechFileID == "ENG_Orbital_Drydocks");
																																						if (flag2 && designInfo2.isPrototyped && designInfo2.DesignDate != base.App.GameDatabase.GetTurnCount())
																																						{
																																							this._RetrofitMode = true;
																																							this._currentShipDirty = true;
																																							this.RefreshCurrentShip();
																																						}
																																					}
																																				}
																																			}
																																		}
																																	}
																																}
																															}
																														}
																													}
																												}
																											}
																										}
																									}
																								}
																							}
																						}
																					}
																				}
																			}
																		}
																	}
																}
															}
														}
													}
												}
											}
										}
									}
								}
								else
								{
									if (msgType == "color_changed")
									{
										if (panelName == DesignScreenState.DebugShipColor)
										{
											float x2 = float.Parse(msgParams[0]);
											float y = float.Parse(msgParams[1]);
											float z = float.Parse(msgParams[2]);
											base.App.LocalPlayer.PostSetProp("Color2", new object[]
											{
												new Vector4(x2, y, z, 1f)
											});
										}
									}
									else
									{
										if (msgType == "list_sel_changed")
										{
											if (panelName == "gameWeaponDesignList")
											{
												int index = -1;
												if (!string.IsNullOrEmpty(msgParams[0]))
												{
													index = int.Parse(msgParams[0]);
												}
												IWeaponShipData weaponSelectorBank = this.GetWeaponSelectorBank();
												if (weaponSelectorBank != null)
												{
													int num = weaponSelectorBank.Designs[index];
													if (num != weaponSelectorBank.SelectedDesign)
													{
														weaponSelectorBank.SelectedDesign = num;
														weaponSelectorBank.SelectedWeapon = null;
														if (!this._RetrofitMode)
														{
															this._selectedDesign = null;
															this._selectedDesignPW = "";
														}
														this.CurrentShipChanged();
													}
												}
												this.HideWeaponSelector();
												this.HidePsionicSelector();
											}
											else
											{
												if (panelName == "gameModuleList")
												{
													ModuleShipData selectedModule = this._selectedModule;
													if (selectedModule != null)
													{
														ModuleData moduleData = null;
														if (!string.IsNullOrEmpty(msgParams[0]))
														{
															int num2 = int.Parse(msgParams[0]);
															if (num2 >= 0)
															{
																moduleData = selectedModule.Modules[num2];
															}
														}
														if (moduleData != selectedModule.SelectedModule)
														{
															selectedModule.SelectedModule = moduleData;
															this.CurrentShipChanged();
														}
													}
													this.HideModuleSelector();
												}
												else
												{
													if (panelName == DesignScreenState.DebugFactionsList && this._showDebugControls)
													{
														int num3 = -1;
														if (!string.IsNullOrEmpty(msgParams[0]))
														{
															num3 = int.Parse(msgParams[0]);
														}
														if (num3 >= 0 && num3 < this._factionsList.Count)
														{
															string name = this._factionsList[num3].Name;
															this.SetSelectedFaction(name, DesignScreenState.DebugFactionsList);
														}
													}
													else
													{
														if (panelName == DesignScreenState.DebugEmpireColorList)
														{
															int index2 = int.Parse(msgParams[0]);
															Vector3 vector = Player.DefaultPrimaryPlayerColors[index2];
															base.App.LocalPlayer.PostSetProp("Color1", new object[]
															{
																new Vector4(vector.X, vector.Y, vector.Z, 1f)
															});
														}
														else
														{
															if (panelName == "gameCommandList")
															{
																this.SetSelectedSectionById(ShipSectionType.Command, msgParams[0], "gameCommandList");
																this.DeactivateShipOptionsForSection(ShipSectionType.Command);
																this._shouldRefresh = true;
																this._RetrofitMode = false;
																this.CurrentShipChanged();
															}
															else
															{
																if (panelName == "gameMissionList")
																{
																	this.SetSelectedSectionById(ShipSectionType.Mission, msgParams[0], "gameMissionList");
																	this.DeactivateShipOptionsForSection(ShipSectionType.Mission);
																	this._shouldRefresh = true;
																	this._RetrofitMode = false;
																	this.CurrentShipChanged();
																}
																else
																{
																	if (panelName == "gameEngineList")
																	{
																		this.SetSelectedSectionById(ShipSectionType.Engine, msgParams[0], "gameEngineList");
																		this.DeactivateShipOptionsForSection(ShipSectionType.Engine);
																		this._shouldRefresh = true;
																		this._RetrofitMode = false;
																		this.CurrentShipChanged();
																	}
																	else
																	{
																		if (panelName == "gameClassList")
																		{
																			RealShipClasses shipClass = (RealShipClasses)Enum.Parse(typeof(RealShipClasses), msgParams[0]);
																			this.SetSelectedClass(shipClass, "gameClassList");
																			this._RetrofitMode = false;
																			this._shouldRefresh = true;
																		}
																		else
																		{
																			if (panelName == "gameDesignList")
																			{
																				if (!string.IsNullOrEmpty(msgParams[0]))
																				{
																					int designId = int.Parse(msgParams[0]);
																					this.SetSelectedDesign(designId, "gameDesignList");
																					this._RetrofitMode = false;
																					this._shouldRefresh = true;
																				}
																			}
																			else
																			{
																				if (panelName == DesignScreenState.DebugAnimTracks)
																				{
																					if (string.IsNullOrEmpty(msgParams[0]))
																					{
																						this.SetSelectedAnimTrack(string.Empty, DesignScreenState.DebugAnimTracks);
																					}
																					else
																					{
																						string trackName = DesignScreenState._debugAnimTracks[int.Parse(msgParams[0])];
																						this.SetSelectedAnimTrack(trackName, DesignScreenState.DebugAnimTracks);
																					}
																				}
																				else
																				{
																					if (panelName == DesignScreenState.DebugAnimMode)
																					{
																						int debugCurrentAnimMode = 0;
																						if (!string.IsNullOrEmpty(msgParams[0]))
																						{
																							debugCurrentAnimMode = int.Parse(msgParams[0]);
																						}
																						this._debugCurrentAnimMode = debugCurrentAnimMode;
																					}
																				}
																			}
																		}
																	}
																}
															}
														}
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
			if (this._selectedDesign.HasValue)
			{
				base.App.UI.SetEnabled("designRemove", true);
				return;
			}
			base.App.UI.SetEnabled("designRemove", false);
		}
		private void SetWeaponsAll(LogicalWeapon weapon, WeaponEnums.WeaponSizes turretSize, WeaponEnums.TurretClasses turretClass, int? fireMode, int? filterMode)
		{
			foreach (WeaponBankShipData weaponBank in this._selection.GetCurrentWeaponBanks())
			{
				if (weaponBank.Bank.TurretClass == turretClass && weaponBank.Bank.TurretSize == turretSize && weaponBank.Weapons.Contains(weapon))
				{
					bool flag = false;
					if (this._RetrofitMode && weapon.PayloadType == weaponBank.SelectedWeapon.PayloadType && weapon.DefaultWeaponSize == weaponBank.SelectedWeapon.DefaultWeaponSize)
					{
						if (weapon.PayloadType == WeaponEnums.PayloadTypes.Bolt)
						{
							if (weapon.Traits.Contains(WeaponEnums.WeaponTraits.Laser) && weaponBank.SelectedWeapon.Traits.Contains(WeaponEnums.WeaponTraits.Laser))
							{
								flag = true;
							}
							if (weapon.Traits.Contains(WeaponEnums.WeaponTraits.Ballistic) && weaponBank.SelectedWeapon.Traits.Contains(WeaponEnums.WeaponTraits.Ballistic))
							{
								flag = true;
							}
						}
						else
						{
							flag = true;
						}
					}
					if (!this._RetrofitMode || (this._RetrofitMode && flag))
					{
						weaponBank.SelectedWeapon = weapon;
						weaponBank.FiringMode = fireMode;
						weaponBank.FilterMode = filterMode;
						WeaponAssignment weaponAssignment = this._selection.GetWeaponAssignments().FirstOrDefault((WeaponAssignment x) => x.Bank == weaponBank.Bank);
						if (weaponAssignment != null)
						{
							weaponAssignment.InitialFireMode = fireMode;
							weaponAssignment.InitialTargetFilter = filterMode;
						}
					}
				}
			}
		}
		private void SetWeaponsAllFireMode(LogicalWeapon weapon, int fireMode)
		{
			foreach (WeaponAssignment wa in this._selection.GetWeaponAssignments())
			{
				if (wa.Weapon == weapon)
				{
					wa.InitialFireMode = new int?(fireMode);
					WeaponBankShipData weaponBankShipData = this._selection.GetCurrentWeaponBanks().FirstOrDefault((WeaponBankShipData x) => x.Bank == wa.Bank);
					if (weaponBankShipData != null)
					{
						weaponBankShipData.FiringMode = new int?(fireMode);
					}
				}
			}
		}
		private void SetWeaponsAllFilterMode(LogicalWeapon weapon, int filterMode)
		{
			foreach (WeaponAssignment wa in this._selection.GetWeaponAssignments())
			{
				if (wa.Weapon == weapon)
				{
					wa.InitialTargetFilter = new int?(filterMode);
					WeaponBankShipData weaponBankShipData = this._selection.GetCurrentWeaponBanks().FirstOrDefault((WeaponBankShipData x) => x.Bank == wa.Bank);
					if (weaponBankShipData != null)
					{
						weaponBankShipData.FilterMode = new int?(filterMode);
					}
				}
			}
		}
		private void PlayAnim()
		{
			if (string.IsNullOrEmpty(this._debugCurrentAnimTrack))
			{
				return;
			}
			if (this._builder.Ship != null)
			{
				this._builder.Ship.PostSetProp("PlayAnim", new object[]
				{
					this._debugCurrentAnimTrack,
					this._debugCurrentAnimMode + 1
				});
			}
		}
		private void StopAnim()
		{
			if (this._builder.Ship != null)
			{
				this._builder.Ship.PostSetProp("StopAnims", new object[0]);
			}
		}
		private void SetSelectedAnimTrack(string trackName, string trigger)
		{
			if (this._debugCurrentAnimTrack == trackName)
			{
				return;
			}
			this._debugCurrentAnimTrack = trackName;
			if (trigger != DesignScreenState.DebugAnimEdit)
			{
				base.App.UI.SetText(DesignScreenState.DebugAnimEdit, trackName);
			}
			if (trigger == DesignScreenState.DebugAnimTracks)
			{
				int num = -1;
				for (int i = 0; i < DesignScreenState._debugAnimTracks.Length; i++)
				{
					if (DesignScreenState._debugAnimTracks[i] == trackName)
					{
						num = i;
						break;
					}
				}
				if (num >= 0)
				{
					base.App.UI.SetSelection(DesignScreenState.DebugAnimTracks, num);
					return;
				}
				base.App.UI.ClearSelection(DesignScreenState.DebugAnimTracks);
			}
		}
		private IWeaponShipData GetWeaponSelectorBank()
		{
			if (this._selectedWeaponBank != null)
			{
				return this._selectedWeaponBank;
			}
			if (this._selectedModule != null)
			{
				return this._selectedModule.SelectedModule;
			}
			return null;
		}
		private void SetSelectedClass(RealShipClasses shipClass, string trigger)
		{
			ClassShipData classShipData = this._selection.Factions.Current.Classes.FirstOrDefault((ClassShipData x) => x.Class == shipClass);
			if (classShipData == null)
			{
				return;
			}
			this._selection.Factions.Current.SelectedClass = classShipData;
			if (trigger != "gameClassList")
			{
				base.App.UI.SetSelection("gameClassList", (int)shipClass);
			}
			this.PopulateSectionLists();
			this.PopulateDesignList();
			this._RetrofitMode = false;
			if (trigger != "init")
			{
				this.CurrentShipChanged();
			}
		}
		private void SetSelectedDesign(int designId, string trigger)
		{
			if (designId <= 0)
			{
				return;
			}
			DesignInfo designInfo = base.App.GameDatabase.GetDesignInfo(designId);
			if (designInfo == null)
			{
				return;
			}
			if (trigger != "gameDesignList")
			{
				base.App.UI.SetSelection("gameDesignList", designId);
			}
			this._selectedDesign = new int?(designId);
			this._designName = designInfo.Name;
			this._originalName = designInfo.Name;
			this._selectedDesignPW = designInfo.PriorityWeaponName;
			foreach (ModuleShipData current in this._selection.GetCurrentSectionModules())
			{
				current.SelectedModule = null;
			}
			DesignSectionInfo[] designSections = designInfo.DesignSections;
			DesignSectionInfo section;
			for (int i = 0; i < designSections.Length; i++)
			{
				section = designSections[i];
				ShipSectionAsset shipSectionAsset = base.App.AssetDatabase.ShipSections.First((ShipSectionAsset x) => x.FileName == section.FilePath);
				ShipSectionType sectionType = (shipSectionAsset != null) ? shipSectionAsset.Type : ShipSectionType.Mission;
				this.SetSelectedSection(shipSectionAsset, sectionType, trigger, true, null);
				SectionShipData currentSectionData = this._selection.GetCurrentSectionData(sectionType);
				foreach (WeaponBankShipData current2 in currentSectionData.WeaponBanks)
				{
					current2.Designs.Clear();
					current2.Designs.AddRange(this.CollectWeaponDesigns(shipSectionAsset, current2.Bank.TurretClass));
				}
				int num = 0;
				foreach (WeaponBankInfo current3 in section.WeaponBanks)
				{
					WeaponBankShipData currentWeaponBank = this._selection.GetCurrentWeaponBank(base.App.GameDatabase, current3);
					if (current3.WeaponID.HasValue)
					{
						string weaponFile = base.App.GameDatabase.GetWeaponAsset(current3.WeaponID.Value);
						currentWeaponBank.SelectedWeapon = currentWeaponBank.Weapons.FirstOrDefault((LogicalWeapon x) => x.FileName == weaponFile);
						currentWeaponBank.FiringMode = current3.FiringMode;
						currentWeaponBank.FilterMode = current3.FilterMode;
					}
					if (current3.DesignID.HasValue)
					{
						currentWeaponBank.SelectedDesign = (current3.DesignID.HasValue ? current3.DesignID.Value : 0);
					}
					num++;
				}
				foreach (DesignModuleInfo current4 in section.Modules)
				{
					string moduleAsset = base.App.GameDatabase.GetModuleAsset(current4.ModuleID);
					ModuleShipData currentModuleMount = this._selection.GetCurrentModuleMount(base.App.AssetDatabase.ShipSections.First((ShipSectionAsset x) => x.FileName == section.FilePath), current4.MountNodeName);
					if (currentModuleMount == null)
					{
						DesignScreenState.Warn(string.Format("Module mount {0} not found for design '{1}', section '{2}'.", current4.MountNodeName, designInfo.Name, shipSectionAsset.FileName));
					}
					else
					{
						currentModuleMount.SelectedModule = currentModuleMount.Modules.FirstOrDefault((ModuleData x) => x.Module.ModulePath == moduleAsset);
						if (currentModuleMount.SelectedModule != null)
						{
							currentModuleMount.SelectedModule.SelectedPsionic.Clear();
						}
						if (current4.PsionicAbilities != null)
						{
							foreach (ModulePsionicInfo current5 in current4.PsionicAbilities)
							{
								currentModuleMount.SelectedModule.SelectedPsionic.Add(current5.Ability);
							}
						}
						if (currentModuleMount.SelectedModule != null && current4.WeaponID.HasValue)
						{
							string weaponFile = base.App.GameDatabase.GetWeaponAsset(current4.WeaponID.Value);
							currentModuleMount.SelectedModule.SelectedWeapon = currentModuleMount.SelectedModule.Weapons.First((LogicalWeapon x) => x.FileName == weaponFile);
						}
					}
				}
			}
			this._RetrofitMode = false;
			this.SyncSectionTechs(designInfo);
			this.CurrentShipChanged();
		}
		private void SetSelectedSectionById(ShipSectionType sectionType, string msgParam, string trigger)
		{
			SectionShipData sectionShipData = null;
			SectionTypeShipData currentSectionTypeShipData = this._selection.GetCurrentSectionTypeShipData(sectionType);
			int num;
			if (currentSectionTypeShipData != null && base.App.UI.ParseListItemId(msgParam, out num) && num >= 0 && num < currentSectionTypeShipData.Sections.Count)
			{
				sectionShipData = currentSectionTypeShipData.Sections[num];
			}
			ShipSectionAsset section = null;
			if (sectionShipData != null)
			{
				section = sectionShipData.Section;
			}
			this.SetSelectedSection(section, sectionType, trigger, true, null);
			SectionShipData currentSectionData = this._selection.GetCurrentSectionData(sectionType);
			foreach (WeaponBankShipData current in currentSectionData.WeaponBanks)
			{
				current.Designs.Clear();
				current.Designs.AddRange(this.CollectWeaponDesigns(section, current.Bank.TurretClass));
			}
		}
		private void SetSelectedSection(ShipSectionAsset section, ShipSectionType sectionType, string trigger, bool refreshOtherSections = true, DesignInfo design = null)
		{
			SectionTypeShipData currentSectionTypeShipData = this._selection.GetCurrentSectionTypeShipData(sectionType);
			if (currentSectionTypeShipData == null)
			{
				return;
			}
			this.HideWeaponSelector();
			this.HidePsionicSelector();
			SectionShipData sectionShipData = currentSectionTypeShipData.Sections.FirstOrDefault((SectionShipData x) => x.Section == section);
			if (trigger != "gameDesignList" && sectionShipData != null && this.IsExcluded(sectionShipData))
			{
				return;
			}
			currentSectionTypeShipData.SelectedSection = sectionShipData;
			if (trigger != "" && trigger != "gameDesignList")
			{
				base.App.UI.ClearSelection("gameDesignList");
				this._selectedDesign = null;
				this._selectedDesignPW = "";
			}
			if (trigger != DesignScreenState.UISectionList(sectionType))
			{
				base.App.UI.SetSelection(DesignScreenState.UISectionList(sectionType), currentSectionTypeShipData.Sections.IndexOf(sectionShipData));
			}
			if (section != null)
			{
				ShipDesignUI.SyncSectionArmor(base.App, DesignScreenState.UISectionStats(sectionType), section, design);
			}
			this.RefreshExcludedSections();
			if (refreshOtherSections && section != null)
			{
				switch (section.Type)
				{
				case ShipSectionType.Command:
				{
					SectionShipData currentSectionData = this._selection.GetCurrentSectionData(ShipSectionType.Mission);
					SectionShipData currentSectionData2 = this._selection.GetCurrentSectionData(ShipSectionType.Engine);
					SectionShipData sectionShipData2 = this.ConfirmIfAvailableSection(currentSectionData, ShipSectionType.Mission);
					SectionShipData sectionShipData3 = this.ConfirmIfAvailableSection(currentSectionData2, ShipSectionType.Engine);
					ShipSectionAsset section2 = (sectionShipData2 == null) ? null : sectionShipData2.Section;
					ShipSectionAsset section3 = (sectionShipData3 == null) ? null : sectionShipData3.Section;
					if (currentSectionData != sectionShipData2)
					{
						this.SetSelectedSection(section2, ShipSectionType.Mission, trigger, false, null);
					}
					if (currentSectionData2 != sectionShipData3)
					{
						this.SetSelectedSection(section3, ShipSectionType.Engine, trigger, false, null);
						return;
					}
					return;
				}
				case ShipSectionType.Engine:
				{
					SectionShipData currentSectionData3 = this._selection.GetCurrentSectionData(ShipSectionType.Command);
					SectionShipData currentSectionData4 = this._selection.GetCurrentSectionData(ShipSectionType.Mission);
					SectionShipData sectionShipData4 = this.ConfirmIfAvailableSection(currentSectionData3, ShipSectionType.Command);
					SectionShipData sectionShipData5 = this.ConfirmIfAvailableSection(currentSectionData4, ShipSectionType.Mission);
					ShipSectionAsset section4 = (sectionShipData4 == null) ? null : sectionShipData4.Section;
					ShipSectionAsset section5 = (sectionShipData5 == null) ? null : sectionShipData5.Section;
					if (currentSectionData3 != sectionShipData4)
					{
						this.SetSelectedSection(section4, ShipSectionType.Command, trigger, false, null);
					}
					if (currentSectionData4 != sectionShipData5)
					{
						this.SetSelectedSection(section5, ShipSectionType.Mission, trigger, false, null);
						return;
					}
					return;
				}
				}
				SectionShipData currentSectionData5 = this._selection.GetCurrentSectionData(ShipSectionType.Command);
				SectionShipData currentSectionData6 = this._selection.GetCurrentSectionData(ShipSectionType.Engine);
				SectionShipData sectionShipData6 = this.ConfirmIfAvailableSection(currentSectionData5, ShipSectionType.Command);
				SectionShipData sectionShipData7 = this.ConfirmIfAvailableSection(currentSectionData6, ShipSectionType.Engine);
				ShipSectionAsset section6 = (sectionShipData6 == null) ? null : sectionShipData6.Section;
				ShipSectionAsset section7 = (sectionShipData7 == null) ? null : sectionShipData7.Section;
				if (currentSectionData5 != sectionShipData6)
				{
					this.SetSelectedSection(section6, ShipSectionType.Command, trigger, true, null);
				}
				if (currentSectionData6 != sectionShipData7)
				{
					this.SetSelectedSection(section7, ShipSectionType.Engine, trigger, true, null);
				}
			}
		}
		private SectionShipData ConfirmIfAvailableSection(SectionShipData proposedSection, ShipSectionType sectionType)
		{
			if (proposedSection != null && !this.IsExcluded(proposedSection))
			{
				return proposedSection;
			}
			SectionTypeShipData currentSectionTypeShipData = this._selection.GetCurrentSectionTypeShipData(sectionType);
			if (currentSectionTypeShipData == null)
			{
				return null;
			}
			return currentSectionTypeShipData.Sections.FirstOrDefault((SectionShipData x) => !this.IsExcluded(x));
		}
		private bool IsExcluded(SectionShipData currSection, SectionShipData desSection)
		{
			return currSection != null && desSection != null && currSection.Section.SectionIsExcluded(desSection.Section);
		}
		private bool IsExcluded(SectionShipData section)
		{
			SectionShipData currentSectionData = this._selection.GetCurrentSectionData(ShipSectionType.Command);
			SectionShipData currentSectionData2 = this._selection.GetCurrentSectionData(ShipSectionType.Mission);
			SectionShipData currentSectionData3 = this._selection.GetCurrentSectionData(ShipSectionType.Engine);
			return this.IsExcluded(currentSectionData, section) || this.IsExcluded(currentSectionData2, section) || this.IsExcluded(currentSectionData3, section);
		}
		private void RefreshExcludedSections()
		{
			this.RefreshExcludedSections("gameMissionList", ShipSectionType.Mission);
			this.RefreshExcludedSections("gameEngineList", ShipSectionType.Engine);
			this.RefreshExcludedSections("gameCommandList", ShipSectionType.Command);
		}
		private void RefreshExcludedSections(string listId, ShipSectionType sectionType)
		{
			List<int> list = new List<int>();
			SectionTypeShipData currentSectionTypeShipData = this._selection.GetCurrentSectionTypeShipData(sectionType);
			if (currentSectionTypeShipData != null)
			{
				for (int i = 0; i < currentSectionTypeShipData.Sections.Count; i++)
				{
					SectionShipData section = currentSectionTypeShipData.Sections[i];
					if (this.IsExcluded(section))
					{
						list.Add(i);
					}
				}
			}
			base.App.UI.SetDisabledItems(listId, list);
		}
		private string StripSection(string str)
		{
			string[] array = str.Split(new char[]
			{
				'\\',
				'/'
			});
			if (array.Length > 0)
			{
				str = array[array.Length - 1];
			}
			str = str.Replace(".section", "");
			return str;
		}
		private string GetWeaponBankTooltip(IWeaponShipData weaponBank)
		{
			if (!weaponBank.RequiresDesign)
			{
				string text = weaponBank.SelectedWeapon.Name;
				if (weaponBank.Bank != null)
				{
					object obj = text;
					text = string.Concat(new object[]
					{
						obj,
						" (",
						weaponBank.Bank.TurretClass,
						", ",
						weaponBank.Bank.TurretSize,
						")"
					});
				}
				return text;
			}
			if (weaponBank.SelectedDesign == 0)
			{
				return App.Localize("@UI_DEFAULT") + " " + weaponBank.SelectedWeapon.Name;
			}
			DesignInfo designInfo = base.App.GameDatabase.GetDesignInfo(weaponBank.SelectedDesign);
			return designInfo.Name;
		}
		private void SyncWeaponUi()
		{
			if (this._builder.Ship == null)
			{
				return;
			}
			List<WeaponBankShipData> list = this._selection.GetCurrentWeaponBanks().ToList<WeaponBankShipData>();
			if (!DesignScreenState.BankFilters.Any((KeyValuePair<string, DesignScreenState.BankFilter> x) => x.Key == "filterAll" && x.Value.Enabled))
			{
				IEnumerable<KeyValuePair<string, DesignScreenState.BankFilter>> source = 
					from x in DesignScreenState.BankFilters
					where x.Value.Value is WeaponEnums.WeaponSizes && x.Value.Enabled
					select x;
				IEnumerable<WeaponEnums.WeaponSizes> enabledSizes = 
					from x in source
					select (WeaponEnums.WeaponSizes)x.Value.Value;
				list = (
					from x in list
					where enabledSizes.Contains(x.Bank.TurretSize)
					select x).ToList<WeaponBankShipData>();
			}
			if (!this._builder.Loading)
			{
				this._shipHoloView.SetShip(this._builder.Ship);
			}
			if (this._shouldRefresh)
			{
				this._shipHoloView.PostSetProp("FitToView", new object[0]);
				this._shouldRefresh = false;
			}
			foreach (WeaponBankShipData weaponBank in list)
			{
				WeaponBank weaponBank3 = this._builder.Ship.WeaponBanks.FirstOrDefault((WeaponBank shipWeaponBank) => shipWeaponBank.LogicalBank == weaponBank.Bank);
				if (weaponBank3 != null)
				{
					this._shipHoloView.AddWeaponGroupIcon(weaponBank3);
				}
			}
			if (DesignScreenState.BankFilters["filterModules"].Enabled || DesignScreenState.BankFilters["filterAll"].Enabled)
			{
				foreach (ModuleShipData moduleSlot in this._selection.GetCurrentSectionModules())
				{
					ModuleData selectedModuleData = moduleSlot.SelectedModule;
					Module selectedModule = null;
					if (selectedModuleData != null)
					{
						selectedModule = this._builder.Ship.Modules.FirstOrDefault((Module module) => module.LogicalModule == selectedModuleData.Module && moduleSlot.ModuleMount == module.Attachment);
					}
					string iconSpriteName = "moduleicon_no_selection";
					if (selectedModule != null)
					{
						iconSpriteName = selectedModule.LogicalModule.Icon;
					}
					this._shipHoloView.AddModuleIcon(selectedModule, this._builder.Ship.Sections.FirstOrDefault((Kerberos.Sots.GameObjects.Section section) => section.ShipSectionAsset == moduleSlot.Section.Section), moduleSlot.ModuleMount.NodeName, iconSpriteName);
					if (selectedModule != null && selectedModule.LogicalModule.AbilityType != ModuleEnums.ModuleAbilities.AbaddonLaser && (selectedModuleData.Module.Banks.Length > 0 || selectedModuleData.Module.Psionics.Length > 0))
					{
						LogicalBank[] banks = selectedModule.LogicalModule.Banks;
						LogicalBank bank;
						for (int i = 0; i < banks.Length; i++)
						{
							bank = banks[i];
							WeaponBank weaponBank2 = this._builder.Ship.WeaponBanks.FirstOrDefault((WeaponBank shipWeaponBank) => shipWeaponBank.LogicalBank == bank && shipWeaponBank.Module == selectedModule);
							if (weaponBank2 != null)
							{
								this._shipHoloView.AddWeaponGroupIcon(weaponBank2);
							}
						}
						if (selectedModuleData.SelectedPsionic != null)
						{
							int num = 0;
							foreach (SectionEnumerations.PsionicAbility current in selectedModuleData.SelectedPsionic)
							{
								int psionicid = Convert.ToInt32(current);
								this._shipHoloView.AddPsionicIcon(selectedModule, psionicid, num);
								num++;
							}
						}
					}
				}
			}
		}
		private void PopulateFactionsList()
		{
			base.App.UI.ClearItems(DesignScreenState.DebugFactionsList);
			for (int i = 0; i < this._factionsList.Count; i++)
			{
				base.App.UI.AddItem(DesignScreenState.DebugFactionsList, string.Empty, i, this._factionsList[i].Name);
			}
		}
		public void SetSelectedFaction(string factionName, string trigger)
		{
			FactionShipData factionShipData = this._selection.Factions.FirstOrDefault((FactionShipData x) => x.Faction.Name == factionName);
			if (factionShipData == null)
			{
				return;
			}
			ClassShipData oldShipClassData = this._selection.GetCurrentClassShipData();
			base.App.LocalPlayer.SetFaction(factionShipData.Faction);
			this._selection.Factions.SetCurrent(factionShipData);
			if (trigger != DesignScreenState.DebugFactionsList)
			{
				int userItemId = this._factionsList.IndexOf(factionShipData.Faction);
				base.App.UI.SetSelection(DesignScreenState.DebugFactionsList, userItemId);
			}
			if (trigger != "init")
			{
				this.CurrentShipChanged();
			}
			this.PopulateClassList();
			if (oldShipClassData != null && this._selection.Factions.Current.Classes.FirstOrDefault((ClassShipData x) => x.Class == oldShipClassData.Class) != null)
			{
				this.SetSelectedClass(oldShipClassData.Class, trigger);
				return;
			}
			if (this._selection.Factions.Current.Classes.FirstOrDefault((ClassShipData x) => x.Class == RealShipClasses.Cruiser) != null)
			{
				this.SetSelectedClass(RealShipClasses.Cruiser, trigger);
				return;
			}
			if (this._selection.Factions.Current.Classes.Any<ClassShipData>())
			{
				this.SetSelectedClass(this._selection.Factions.Current.Classes.First<ClassShipData>().Class, trigger);
			}
		}
		private IEnumerable<RealShipClasses> CollectAvailableShipClasses()
		{
			FactionShipData currentFactionShipData = this._selection.GetCurrentFactionShipData();
			foreach (RealShipClasses shipClass in Enum.GetValues(typeof(RealShipClasses)).Cast<RealShipClasses>())
			{
				if (currentFactionShipData != null)
				{
					if (!currentFactionShipData.Classes.Any((ClassShipData x) => x.Class == shipClass))
					{
						continue;
					}
				}
				yield return shipClass;
			}
			yield break;
		}
		private void PopulateClassList()
		{
			base.App.UI.ClearItems("gameClassList");
			foreach (RealShipClasses current in this.CollectAvailableShipClasses())
			{
				base.App.UI.AddItem("gameClassList", string.Empty, (int)current, current.Localize());
			}
		}
		private void PopulateSectionLists()
		{
			this.PopulateSectionList(ShipSectionType.Mission);
			this.PopulateSectionList(ShipSectionType.Command);
			this.PopulateSectionList(ShipSectionType.Engine);
		}
		private void PopulateSectionList(ShipSectionType sectionType)
		{
			string text = DesignScreenState.UISectionList(sectionType);
			SectionTypeShipData currentSectionTypeShipData = this._selection.GetCurrentSectionTypeShipData(sectionType);
			base.App.UI.SetVisible(text, currentSectionTypeShipData != null);
			ShipSectionAsset section = null;
			base.App.UI.ClearItems(text);
			if (currentSectionTypeShipData != null)
			{
				for (int i = 0; i < currentSectionTypeShipData.Sections.Count; i++)
				{
					if (this._showDebugControls || (!currentSectionTypeShipData.Sections[i].Section.IsSuulka && !currentSectionTypeShipData.Sections[i].Section.IsAccelerator && !currentSectionTypeShipData.Sections[i].Section.IsLoaCube))
					{
						base.App.UI.AddItem(text, string.Empty, i, App.Localize(currentSectionTypeShipData.Sections[i].Section.Title));
					}
				}
				if (currentSectionTypeShipData.SelectedSection != null)
				{
					section = currentSectionTypeShipData.SelectedSection.Section;
				}
			}
			this.SetSelectedSection(section, sectionType, "init", true, null);
		}
		private DesignSectionInfo SummarizeDesignSection(ShipSectionType type, int playerId)
		{
			string currentSectionAssetName = this._selection.GetCurrentSectionAssetName(type);
			if (string.IsNullOrEmpty(currentSectionAssetName))
			{
				return null;
			}
			SectionShipData currentSectionData = this._selection.GetCurrentSectionData(type);
			List<WeaponBankInfo> list = new List<WeaponBankInfo>();
			foreach (WeaponBankShipData current in currentSectionData.WeaponBanks)
			{
				int? weaponID = null;
				int? designID = null;
				if (current.SelectedWeapon != null)
				{
					weaponID = base.App.GameDatabase.GetWeaponID(current.SelectedWeapon.FileName, playerId);
					if (current.SelectedDesign != 0)
					{
						designID = new int?(current.SelectedDesign);
					}
				}
				list.Add(new WeaponBankInfo
				{
					WeaponID = weaponID,
					DesignID = designID,
					FiringMode = current.FiringMode,
					FilterMode = current.FilterMode,
					BankGUID = current.Bank.GUID
				});
			}
			List<DesignModuleInfo> list2 = new List<DesignModuleInfo>();
			foreach (ModuleShipData current2 in 
				from x in currentSectionData.Modules
				where x.SelectedModule != null
				select x)
			{
				int moduleID = base.App.GameDatabase.GetModuleID(current2.SelectedModule.Module.ModulePath, playerId);
				int? weaponID2 = null;
				int? designID2 = null;
				if (current2.SelectedModule.SelectedWeapon != null)
				{
					weaponID2 = base.App.GameDatabase.GetWeaponID(current2.SelectedModule.SelectedWeapon.FileName, playerId);
					if (current2.SelectedModule.SelectedDesign != 0)
					{
						designID2 = new int?(current2.SelectedModule.SelectedDesign);
					}
				}
				DesignModuleInfo designModuleInfo = new DesignModuleInfo
				{
					MountNodeName = current2.ModuleMount.NodeName,
					ModuleID = moduleID,
					WeaponID = weaponID2,
					DesignID = designID2
				};
				foreach (SectionEnumerations.PsionicAbility current3 in current2.SelectedModule.SelectedPsionic)
				{
					designModuleInfo.PsionicAbilities.Add(new ModulePsionicInfo
					{
						Ability = current3
					});
				}
				list2.Add(designModuleInfo);
			}
			List<int> list3 = new List<int>();
			if (this._shipOptionGroups != null && this._shipOptionGroups.ContainsKey(type))
			{
				foreach (Dictionary<string, bool> current4 in this._shipOptionGroups[type])
				{
					foreach (KeyValuePair<string, bool> current5 in current4)
					{
						if (current5.Value)
						{
							list3.Add(base.App.GameDatabase.GetTechID(current5.Key));
						}
					}
				}
			}
			return new DesignSectionInfo
			{
				FilePath = currentSectionAssetName,
				WeaponBanks = list,
				Modules = list2,
				Techs = list3
			};
		}
		private DesignInfo SummarizeDesign(int playerId, bool finalsummerize = false)
		{
			if (!this._selection.IsCurrentShipDataValid())
			{
				return null;
			}
			List<DesignSectionInfo> list = new List<DesignSectionInfo>();
			DesignSectionInfo designSectionInfo = this.SummarizeDesignSection(ShipSectionType.Command, playerId);
			if (designSectionInfo != null)
			{
				list.Add(designSectionInfo);
			}
			DesignSectionInfo mission = this.SummarizeDesignSection(ShipSectionType.Mission, playerId);
			if (mission != null)
			{
				base.App.AssetDatabase.ShipSections.First((ShipSectionAsset x) => x.FileName == mission.FilePath);
				list.Add(mission);
			}
			DesignSectionInfo designSectionInfo2 = this.SummarizeDesignSection(ShipSectionType.Engine, playerId);
			if (designSectionInfo2 != null)
			{
				list.Add(designSectionInfo2);
			}
			if (list.Count == 0)
			{
				return null;
			}
			DesignInfo designInfo = new DesignInfo();
			designInfo.PlayerID = base.App.LocalPlayer.ID;
			designInfo.Name = (string.IsNullOrEmpty(this._designName) ? string.Empty : this._designName);
			designInfo.DesignSections = list.ToArray();
			if (!this._RetrofitMode && !finalsummerize)
			{
				designInfo.Name = base.App.GameDatabase.ResolveNewDesignName(base.App.LocalPlayer.ID, string.Empty);
			}
			else
			{
				designInfo.Name = base.App.GameDatabase.ResolveNewDesignName(base.App.LocalPlayer.ID, designInfo.Name);
			}
			DesignLab.SummarizeDesign(base.App.AssetDatabase, base.App.GameDatabase, designInfo);
			return designInfo;
		}
		private void ShowCommitDialog()
		{
			this.HidePsionicSelector();
			this.HideWeaponSelector();
			this.HideModuleSelector();
			base.App.UI.SetVisible("submit_dialog", true);
			DesignInfo designInfo = this.SummarizeDesign(base.App.LocalPlayer.ID, false);
			if (!this._RetrofitMode)
			{
				this._designName = base.App.GameDatabase.ResolveNewDesignName(base.App.LocalPlayer.ID, designInfo.Name);
			}
			else
			{
				this._designName = base.App.GameDatabase.ResolveNewDesignName(base.App.LocalPlayer.ID, this._originalName);
			}
			base.App.UI.SetText("edit_design_name", this._designName);
			if (this._RetrofitMode)
			{
				base.App.UI.SetText("submit_dialog_title", App.Localize("@UI_DESIGN_CONFIRM_RETROFIT"));
				base.App.UI.SetEnabled("edit_design_name", false);
				return;
			}
			base.App.UI.SetText("submit_dialog_title", App.Localize("@UI_DESIGN_ENTER_DESIGN_NAME"));
			base.App.UI.SetEnabled("edit_design_name", true);
		}
		private void CommitDesign()
		{
			base.App.UI.SetVisible("submit_dialog", false);
			DesignInfo designInfo = this.SummarizeDesign(base.App.LocalPlayer.ID, true);
			designInfo.Name = base.App.GameDatabase.ResolveNewDesignName(base.App.LocalPlayer.ID, designInfo.Name);
			if (this._RetrofitMode)
			{
				if (this._selectedDesign.HasValue)
				{
					designInfo.RetrofitBaseID = this._selectedDesign.Value;
					DesignInfo designInfo2 = base.App.GameDatabase.GetDesignInfo(this._selectedDesign.Value);
					designInfo.isAttributesDiscovered = designInfo2.isAttributesDiscovered;
				}
				designInfo.isPrototyped = true;
			}
			if (base.App.GameDatabase.GetTurnCount() == 1)
			{
				designInfo.isPrototyped = true;
			}
			if (designInfo.Role == ShipRole.DRONE || designInfo.Role == ShipRole.ASSAULTSHUTTLE || designInfo.Class == ShipClass.BattleRider)
			{
				designInfo.isPrototyped = true;
			}
			if (this._builder.Ship != null)
			{
				designInfo.PriorityWeaponName = this._builder.Ship.PriorityWeaponName;
			}
			int num = base.App.GameDatabase.InsertDesignByDesignInfo(designInfo);
			if (this._RetrofitMode && this._selectedDesign.HasValue && base.App.GameDatabase.GetDesignAttributesForDesign(this._selectedDesign.Value).Any<SectionEnumerations.DesignAttribute>())
			{
				base.App.GameDatabase.InsertDesignAttribute(num, base.App.GameDatabase.GetDesignAttributesForDesign(this._selectedDesign.Value).First<SectionEnumerations.DesignAttribute>());
			}
			this.PopulateDesignList();
			base.App.UI.SetSelection("gameDesignList", num);
			this.UpdateWeaponDesigns_DesignAdded(num);
			base.App.PostEnableSpeechSounds(true);
			string cueName = string.Format("STRAT_036-01_{0}_DesignSaved", base.App.GameDatabase.GetFactionName(base.App.GameDatabase.GetPlayerFactionID(base.App.LocalPlayer.ID)));
			base.App.PostRequestSpeech(cueName, 50, 120, 0f);
			this._RetrofitMode = false;
		}
		private void UpdateWeaponDesigns_DesignAdded(int designId)
		{
		}
		private void PopulateDesignList()
		{
			if (this._showDebugControls)
			{
				return;
			}
			IEnumerable<DesignInfo> visibleDesignInfosForPlayer = base.App.GameDatabase.GetVisibleDesignInfosForPlayer(base.App.LocalPlayer.ID, this.SelectedClass);
			List<DesignInfo> list = visibleDesignInfosForPlayer.ToList<DesignInfo>();
			foreach (DesignInfo current in visibleDesignInfosForPlayer)
			{
                if (!Kerberos.Sots.StarFleet.StarFleet.IsNewestRetrofit(current, visibleDesignInfosForPlayer))
				{
					list.Remove(current);
				}
			}
			ShipDesignUI.PopulateDesignList(base.App, "gameDesignList", list);
		}
		private int GetBestBuildSite()
		{
			IEnumerable<StationInfo> stationInfosByPlayerID = base.App.GameDatabase.GetStationInfosByPlayerID(base.App.LocalPlayer.ID);
			if (stationInfosByPlayerID.Any<StationInfo>())
			{
				return stationInfosByPlayerID.First<StationInfo>().OrbitalObjectID;
			}
			return 0;
		}
		private void CurrentShipChanged()
		{
			this._currentShipDirty = true;
		}
		private void SelectTech(string panelId, bool enabled)
		{
			ShipSectionType shipSectionType;
			int num;
			string text;
			this.GetTechInfoFromPanel(panelId, out shipSectionType, out num, out text);
			if (this._shipOptionGroups[shipSectionType][num][text] == enabled)
			{
				return;
			}
			this.ChangeTechGroupComboSelection(shipSectionType, num, text, enabled);
			if (text == "IND_Stealth_Armor")
			{
				this.ChangeTechMultiSelection(text, enabled);
			}
			else
			{
				if (text.Contains("SLD"))
				{
					this.CheckShieldTech();
					this.CheckCloakingTech();
				}
			}
			this._currentShipDirty = true;
		}
		private string GetTechPanel(ShipSectionType sectionType, int optionIndex, string techId)
		{
			string text = string.Concat(new object[]
			{
				"tech|",
				sectionType,
				"|",
				optionIndex,
				"|",
				techId
			});
			return text.Replace('.', '¿');
		}
		private void GetTechInfoFromPanel(string panel, out ShipSectionType sectionType, out int optionIndex, out string tech)
		{
			string[] array = panel.Split(new char[]
			{
				'|'
			});
			sectionType = (ShipSectionType)Enum.Parse(typeof(ShipSectionType), array[1]);
			optionIndex = int.Parse(array[2]);
			tech = array[3];
			tech = tech.Replace('¿', '.');
		}
		private void ChangeTechMultiSelection(string tech, bool enabled)
		{
			foreach (KeyValuePair<ShipSectionType, List<Dictionary<string, bool>>> current in this._shipOptionGroups)
			{
				Dictionary<string, bool> dictionary = current.Value.FirstOrDefault((Dictionary<string, bool> x) => x.ContainsKey(tech));
				if (dictionary != null)
				{
					int optionIndex = current.Value.IndexOf(dictionary);
					string techPanel = this.GetTechPanel(current.Key, optionIndex, tech);
					base.App.UI.SetChecked(techPanel, enabled);
					this.ChangeTechGroupComboSelection(current.Key, optionIndex, tech, enabled);
				}
				else
				{
					if (enabled)
					{
						this.ChangeTechMultiSelection(tech, false);
						base.App.UI.CreateDialog(new GenericTextDialog(base.App, "Unable to Select Tech", "Unable to select this technology because it must be present on all sections.", "dialogGenericMessage"), null);
						break;
					}
				}
			}
		}
		private void ChangeTechGroupComboSelection(ShipSectionType section, int optionIndex, string tech, bool enabled)
		{
			this._shipOptionGroups[section][optionIndex][tech] = enabled;
			base.App.UI.SetChecked(this.GetTechPanel(section, optionIndex, tech), enabled);
			if (enabled)
			{
				Dictionary<string, bool> dictionary = this._shipOptionGroups[section][optionIndex];
				foreach (KeyValuePair<string, bool> current in dictionary.ToList<KeyValuePair<string, bool>>())
				{
					if (current.Key != tech)
					{
						if (current.Key == "IND_Stealth_Armor")
						{
							this.ChangeTechMultiSelection(current.Key, false);
						}
						else
						{
							string techPanel = this.GetTechPanel(section, optionIndex, current.Key);
							base.App.UI.SetChecked(techPanel, false);
							dictionary[current.Key] = false;
						}
					}
				}
			}
		}
		private void ClearTechSelections()
		{
			foreach (KeyValuePair<ShipSectionType, List<Dictionary<string, bool>>> current in this._shipOptionGroups)
			{
				foreach (Dictionary<string, bool> current2 in current.Value)
				{
					foreach (KeyValuePair<string, bool> current3 in current2.ToList<KeyValuePair<string, bool>>())
					{
						int optionIndex = current.Value.IndexOf(current2);
						string techPanel = this.GetTechPanel(current.Key, optionIndex, current3.Key);
						base.App.UI.SetChecked(techPanel, false);
						current2[current3.Key] = false;
					}
				}
			}
		}
		private void PopulateTechList(string sectionPanel, ShipSectionType sectionType)
		{
			ShipSectionAsset currentSection = this._selection.GetCurrentSection(sectionType);
			base.App.UI.SetListCleanClear(base.App.UI.Path(new string[]
			{
				sectionPanel,
				"specialList"
			}), true);
			base.App.UI.ClearItems(base.App.UI.Path(new string[]
			{
				sectionPanel,
				"specialList"
			}));
			if (currentSection != null)
			{
				this._shipOptionGroups[sectionType] = new List<Dictionary<string, bool>>();
				int userItemId = 0;
				int num = 0;
				foreach (string[] current in currentSection.ShipOptions)
				{
					Dictionary<string, bool> dictionary = new Dictionary<string, bool>();
					int num2 = 0;
					string[] array = current;
					for (int i = 0; i < array.Length; i++)
					{
						string text = array[i];
						if (base.App.GameDatabase.PlayerHasTech(base.App.LocalPlayer.ID, text))
						{
							dictionary[text] = false;
							base.App.UI.AddItem(base.App.UI.Path(new string[]
							{
								sectionPanel,
								"specialList"
							}), string.Empty, userItemId, base.App.AssetDatabase.GetLocalizedTechnologyName(text), "ShipOptionItem");
							string itemGlobalID = base.App.UI.GetItemGlobalID(base.App.UI.Path(new string[]
							{
								sectionPanel,
								"specialList"
							}), string.Empty, userItemId++, string.Empty);
							base.App.UI.SetTooltip(itemGlobalID, App.Localize("@TECH_DESC_" + text));
							base.App.UI.SetPropertyString(base.App.UI.Path(new string[]
							{
								itemGlobalID,
								"btnTech"
							}), "id", this.GetTechPanel(sectionType, num, text));
							num2++;
						}
					}
					if (num2 > 0)
					{
						this._shipOptionGroups[sectionType].Add(dictionary);
						base.App.UI.AddItem(base.App.UI.Path(new string[]
						{
							sectionPanel,
							"specialList"
						}), string.Empty, userItemId++, string.Empty, "ShipOptionDivider");
						num++;
					}
				}
				base.App.UI.ShakeViolently(base.App.UI.Path(new string[]
				{
					sectionPanel,
					"specialList"
				}));
				base.App.UI.Reshape(sectionPanel);
			}
		}
		private void DeactivateShipOptionsForSection(ShipSectionType type)
		{
			if (!this._shipOptionGroups.ContainsKey(type))
			{
				return;
			}
			int num = 0;
			foreach (Dictionary<string, bool> current in this._shipOptionGroups[type])
			{
				foreach (string current2 in current.Keys.ToList<string>())
				{
					this._shipOptionGroups[type][num][current2] = false;
				}
				num++;
			}
		}
		private void SyncSectionTechs(DesignInfo design)
		{
			DesignScreenState.Trace(" ==== Syncing Section Techs ====");
			this._shipOptionGroups.Clear();
			this.PopulateTechList("CommandDesign", ShipSectionType.Command);
			this.PopulateTechList("MissionDesign", ShipSectionType.Mission);
			this.PopulateTechList("EngineDesign", ShipSectionType.Engine);
			this.ClearTechSelections();
			DesignSectionInfo[] designSections = design.DesignSections;
			DesignSectionInfo section;
			for (int i = 0; i < designSections.Length; i++)
			{
				section = designSections[i];
				ShipSectionAsset shipSectionAsset = base.App.AssetDatabase.ShipSections.FirstOrDefault((ShipSectionAsset x) => x.FileName == section.FilePath);
				foreach (int current in section.Techs)
				{
					string tech = base.App.GameDatabase.GetTechFileID(current);
					Dictionary<string, bool> dictionary = this._shipOptionGroups[shipSectionAsset.Type].FirstOrDefault((Dictionary<string, bool> x) => x.Keys.Contains(tech));
					if (dictionary != null)
					{
						int optionIndex = this._shipOptionGroups[shipSectionAsset.Type].IndexOf(dictionary);
						this.ChangeTechGroupComboSelection(shipSectionAsset.Type, optionIndex, tech, true);
					}
				}
			}
			this.CheckShieldTech();
			this.CheckCloakingTech();
		}
		private void CheckShieldTech()
		{
			foreach (SectionShipData section in this._selection.GetCurrentSections())
			{
				if (this._shipOptionGroups.ContainsKey(section.Section.Type) && section.Section.FileName.Contains("shield"))
				{
					KeyValuePair<ShipSectionType, List<Dictionary<string, bool>>> keyValuePair = this._shipOptionGroups.FirstOrDefault((KeyValuePair<ShipSectionType, List<Dictionary<string, bool>>> x) => x.Key == section.Section.Type);
					Dictionary<string, bool> dictionary = keyValuePair.Value.FirstOrDefault((Dictionary<string, bool> x) => x.Any((KeyValuePair<string, bool> y) => y.Key != "SLD_Structural_Fields" && y.Key.Contains("SLD")));
					if (dictionary != null)
					{
						if (!dictionary.Any((KeyValuePair<string, bool> x) => x.Value))
						{
							int optionIndex = keyValuePair.Value.IndexOf(dictionary);
							KeyValuePair<string, bool> keyValuePair2 = dictionary.First((KeyValuePair<string, bool> x) => x.Key.Contains("SLD"));
							dictionary[keyValuePair2.Key] = true;
							string techPanel = this.GetTechPanel(keyValuePair.Key, optionIndex, keyValuePair2.Key);
							base.App.UI.SetChecked(techPanel, true);
						}
					}
				}
			}
		}
		private void CheckCloakingTech()
		{
			foreach (SectionShipData section in this._selection.GetCurrentSections())
			{
				if (this._shipOptionGroups.ContainsKey(section.Section.Type) && section.Section.cloakingType != CloakingType.None)
				{
					KeyValuePair<ShipSectionType, List<Dictionary<string, bool>>> keyValuePair = this._shipOptionGroups.FirstOrDefault((KeyValuePair<ShipSectionType, List<Dictionary<string, bool>>> x) => x.Key == section.Section.Type);
					Dictionary<string, bool> dictionary = keyValuePair.Value.FirstOrDefault((Dictionary<string, bool> x) => x.Any((KeyValuePair<string, bool> y) => y.Key == "SLD_Cloaking" || y.Key == "SLD_Improved_Cloaking" || y.Key == "SLD_Intangibility"));
					if (dictionary != null)
					{
						if (!dictionary.Any((KeyValuePair<string, bool> x) => x.Value))
						{
							int optionIndex = keyValuePair.Value.IndexOf(dictionary);
							KeyValuePair<string, bool> keyValuePair2 = dictionary.First((KeyValuePair<string, bool> x) => x.Key.Contains("SLD"));
							dictionary[keyValuePair2.Key] = true;
							string techPanel = this.GetTechPanel(keyValuePair.Key, optionIndex, keyValuePair2.Key);
							base.App.UI.SetChecked(techPanel, true);
						}
					}
				}
			}
		}
		private void RefreshCurrentShip()
		{
			if (!this._currentShipDirty || !this._screenReady)
			{
				return;
			}
			base.App.PostEnableSpeechSounds(false);
			this.EnableWeaponTestMode(false);
			this._input.SelectedID = 0;
			this._currentShipDirty = false;
			this._camera.TargetID = 0;
			this._camera.DesiredYaw = MathHelper.DegreesToRadians(-90f);
			this._camera.DesiredPitch = MathHelper.DegreesToRadians(-90f);
			DesignScreenState.Trace("Loading ship:");
			DesignScreenState.Trace("Faction : \"" + this._selection.GetCurrentFactionName() + "\".");
			DesignScreenState.Trace("Command : \"" + this._selection.GetCurrentSectionAssetName(ShipSectionType.Command) + "\".");
			DesignScreenState.Trace("Mission : \"" + this._selection.GetCurrentSectionAssetName(ShipSectionType.Mission) + "\".");
			DesignScreenState.Trace("Engine :  \"" + this._selection.GetCurrentSectionAssetName(ShipSectionType.Engine) + "\".");
			bool flag = this._selection.GetCurrentSection(ShipSectionType.Command) != null;
			bool flag2 = this._selection.GetCurrentSection(ShipSectionType.Engine) != null;
			base.App.UI.SetVisible("gameCommandList", flag);
			base.App.UI.SetVisible("CommandDesign", flag);
			base.App.UI.SetVisible("gameEngineList", flag2);
			base.App.UI.SetVisible("EngineDesign", flag2);
			string text = App.Localize(this._selection.GetCurrentSection(ShipSectionType.Mission).Description);
			string text2 = flag ? App.Localize(this._selection.GetCurrentSection(ShipSectionType.Command).Description) : string.Empty;
			string text3 = flag2 ? App.Localize(this._selection.GetCurrentSection(ShipSectionType.Engine).Description) : string.Empty;
			base.App.UI.SetTooltip("gameCommandList.expand", text2);
			base.App.UI.SetTooltip("gameMissionList.expand", text);
			base.App.UI.SetTooltip("gameEngineList.expand", text3);
			DesignInfo designInfo = this.SummarizeDesign(base.App.LocalPlayer.ID, false);
			IEnumerable<PlayerTechInfo> playerTechInfos = base.App.GameDatabase.GetPlayerTechInfos(base.App.LocalPlayer.ID);
			bool flag3 = playerTechInfos.Any((PlayerTechInfo x) => x.State == TechStates.Researched && x.TechFileID == "ENG_Orbital_Drydocks");
			if (designInfo != null)
			{
				DesignInfo designInfo2 = null;
				if (this._selectedDesign.HasValue)
				{
					designInfo2 = base.App.GameDatabase.GetDesignInfo(this._selectedDesign.Value);
				}
				if (designInfo2 != null)
				{
					base.App.UI.SetVisible("designNameTag", true);
					bool flag4 = designInfo2.GetRealShipClass().HasValue && designInfo2.GetRealShipClass().Value != RealShipClasses.Drone && designInfo2.GetRealShipClass().Value != RealShipClasses.AssaultShuttle;
					base.App.UI.SetVisible("gameRetrofitButton", flag3 && flag4);
					base.App.UI.SetEnabled("gameRetrofitButton", flag3 && !this._RetrofitMode && designInfo2.isPrototyped && designInfo2.DesignDate != base.App.GameDatabase.GetTurnCount() && flag4);
					if (this._RetrofitMode)
					{
                        base.App.UI.SetText("RetrofitCostPanel.game_designretrofitcost", Kerberos.Sots.StarFleet.StarFleet.CalculateRetrofitCost(base.App, designInfo2, this.SummarizeDesign(base.App.LocalPlayer.ID, false)).ToString());
					}
					base.App.UI.SetVisible("RetrofitCostPanel", this._RetrofitMode);
					if (this._RetrofitMode)
					{
						base.App.UI.SetText(base.App.UI.Path(new string[]
						{
							"gameCommitButton"
						}), App.Localize("@UI_DESIGN_RETROFIT_SUBMIT"));
					}
					else
					{
						base.App.UI.SetText(base.App.UI.Path(new string[]
						{
							"gameCommitButton"
						}), App.Localize("@UI_DESIGN_SUBMIT_DESIGN"));
					}
					if (designInfo2.isPrototyped)
					{
						base.App.UI.SetVisible("ShipCost", false);
						base.App.UI.SetVisible("ShipProductionCost", true);
						ShipDesignUI.SyncCost(base.App, "ShipProductionCost", designInfo);
						base.App.UI.SetText(base.App.UI.Path(new string[]
						{
							"ShipProductionCost",
							"gameShipsProduced"
						}), base.App.GameDatabase.GetNumShipsBuiltFromDesign(this._selectedDesign.Value).ToString());
						base.App.UI.SetText(base.App.UI.Path(new string[]
						{
							"ShipProductionCost",
							"gameShipsDestroyed"
						}), base.App.GameDatabase.GetNumShipsDestroyedFromDesign(this._selectedDesign.Value).ToString());
						base.App.UI.SetText(base.App.UI.Path(new string[]
						{
							"ShipProductionCost",
							"gameDesignComissionHeader"
						}), string.Format(App.Localize("@UI_DESIGN_DATE_HEADER"), designInfo2.DesignDate));
						base.App.UI.SetText(base.App.UI.Path(new string[]
						{
							"designNameTag"
						}), designInfo2.Name);
					}
					else
					{
						base.App.UI.SetVisible("ShipCost", true);
						base.App.UI.SetVisible("ShipProductionCost", false);
						ShipDesignUI.SyncCost(base.App, "ShipCost", designInfo);
						base.App.UI.SetText(base.App.UI.Path(new string[]
						{
							"designNameTag"
						}), designInfo2.Name);
					}
					if (designInfo2.isAttributesDiscovered)
					{
						IEnumerable<SectionEnumerations.DesignAttribute> designAttributesForDesign = base.App.GameDatabase.GetDesignAttributesForDesign(designInfo2.ID);
						if (designAttributesForDesign.Count<SectionEnumerations.DesignAttribute>() > 0)
						{
							base.App.UI.SetVisible("attributeNameTagPanel", true);
							base.App.UI.SetText("attributeNameTagPanel.attributeNameTag", App.Localize("@UI_" + designAttributesForDesign.First<SectionEnumerations.DesignAttribute>().ToString()));
							base.App.UI.SetTooltip("attributeNameTagPanel", App.Localize("@UI_" + designAttributesForDesign.First<SectionEnumerations.DesignAttribute>().ToString() + "_TOOLTIP"));
						}
						else
						{
							base.App.UI.SetVisible("attributeNameTagPanel", false);
						}
					}
					else
					{
						base.App.UI.SetVisible("attributeNameTagPanel", false);
					}
				}
				else
				{
					base.App.UI.SetVisible("RetrofitCostPanel", false);
					base.App.UI.SetVisible("designNameTag", false);
					base.App.UI.SetVisible("attributeNameTagPanel", false);
					base.App.UI.SetVisible("ShipCost", true);
					base.App.UI.SetVisible("ShipProductionCost", false);
					base.App.UI.SetVisible("gameRetrofitButton", false);
					base.App.UI.SetText(base.App.UI.Path(new string[]
					{
						"gameCommitButton"
					}), App.Localize("@UI_DESIGN_SUBMIT_DESIGN"));
					ShipDesignUI.SyncCost(base.App, "ShipCost", designInfo);
				}
				ShipDesignUI.SyncSpeed(base.App, designInfo);
				ShipDesignUI.SyncSupplies(base.App, designInfo);
				this.SyncSectionTechs(designInfo);
				DesignSectionInfo[] designSections = designInfo.DesignSections;
				DesignSectionInfo section;
				for (int i = 0; i < designSections.Length; i++)
				{
					section = designSections[i];
					ShipSectionAsset shipSectionAsset = base.App.AssetDatabase.ShipSections.First((ShipSectionAsset x) => x.FileName == section.FilePath);
					ShipSectionType sectionType = (shipSectionAsset != null) ? shipSectionAsset.Type : ShipSectionType.Mission;
					this.SetSelectedSection(shipSectionAsset, sectionType, string.Empty, false, designInfo);
					SectionShipData currentSectionData = this._selection.GetCurrentSectionData(sectionType);
					foreach (WeaponBankShipData current in currentSectionData.WeaponBanks)
					{
						current.Designs.Clear();
						current.Designs.AddRange(this.CollectWeaponDesigns(shipSectionAsset, current.Bank.TurretClass));
					}
				}
				designInfo = this.SummarizeDesign(base.App.LocalPlayer.ID, false);
			}
			if (this._targetArena != null)
			{
				RealShipClasses? currentClass = this._selection.GetCurrentClass();
				if (currentClass.HasValue)
				{
					this._targetArena.SetShipClass(currentClass.Value);
				}
			}
			if (!this._selection.IsCurrentShipDataValid())
			{
				DesignScreenState.Warn("Uninitialized data no ship switch can be completed.");
				return;
			}
			DesignScreenState.Trace("Loading ship start");
			this._builder.New(base.App.LocalPlayer, (
				from x in new ShipSectionAsset[]
				{
					this._selection.GetCurrentSection(ShipSectionType.Command),
					this._selection.GetCurrentSection(ShipSectionType.Mission),
					this._selection.GetCurrentSection(ShipSectionType.Engine)
				}
				where x != null
				select x).ToArray<ShipSectionAsset>(), base.App.AssetDatabase.TurretHousings, base.App.AssetDatabase.Weapons, Enumerable.Empty<LogicalWeapon>(), this._selection.GetWeaponAssignments().ToList<WeaponAssignment>(), base.App.AssetDatabase.Modules, base.App.AssetDatabase.ModulesToAssignByDefault, this._selection.GetModuleAssignments().ToList<ModuleAssignment>(), base.App.AssetDatabase.Psionics, designInfo.DesignSections, this._selection.GetCurrentFaction(), designInfo.Name, this._selectedDesignPW);
			DesignScreenState.Trace("Ship swap");
			if (this._builder.Ship != null)
			{
				this._builder.Ship.PostSetProp("StopAnims", new object[0]);
				this._builder.Ship.PostSetProp("SetValidateCurrentPosition", false);
				this._builder.Ship.Maneuvering.PostSetProp("CanAvoid", false);
				this._builder.Ship.PostSetProp("SetDisableLaunching", true);
				this._builder.Ship.PostSetProp("SetDisableAutoLaunching", true);
				this._input.SelectedID = this._builder.Ship.ObjectID;
			}
		}
		private static void Warn(string message)
		{
			App.Log.Warn(message, "design");
		}
		private static void Trace(string message)
		{
			App.Log.Trace(message, "design");
		}
		public bool OnKeyBindPressed(HotKeyManager.HotKeyActions action, string gamestates)
		{
			if (gamestates.Contains(base.Name))
			{
				switch (action)
				{
				case HotKeyManager.HotKeyActions.State_Starmap:
					base.App.UI.LockUI();
					base.App.SwitchGameState<StarMapState>(new object[0]);
					return true;
				case HotKeyManager.HotKeyActions.State_BuildScreen:
					return false;
				case HotKeyManager.HotKeyActions.State_DesignScreen:
					return false;
				case HotKeyManager.HotKeyActions.State_ResearchScreen:
					base.App.UI.LockUI();
					base.App.SwitchGameState<ResearchScreenState>(new object[0]);
					return true;
				case HotKeyManager.HotKeyActions.State_ComparativeAnalysysScreen:
					return false;
				case HotKeyManager.HotKeyActions.State_EmpireSummaryScreen:
					base.App.UI.LockUI();
					base.App.SwitchGameState<EmpireSummaryState>(new object[0]);
					return true;
				case HotKeyManager.HotKeyActions.State_SotspediaScreen:
					base.App.UI.LockUI();
					base.App.SwitchGameState<SotspediaState>(new object[0]);
					return true;
				case HotKeyManager.HotKeyActions.State_DiplomacyScreen:
					base.App.UI.LockUI();
					base.App.SwitchGameState<DiplomacyScreenState>(new object[0]);
					return true;
				}
			}
			return false;
		}
	}
}
