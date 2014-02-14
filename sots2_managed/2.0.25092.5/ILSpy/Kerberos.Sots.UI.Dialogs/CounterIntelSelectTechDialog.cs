using Kerberos.Sots.Data;
using Kerberos.Sots.Data.TechnologyFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace Kerberos.Sots.UI.Dialogs
{
	internal class CounterIntelSelectTechDialog : Dialog
	{
		private readonly string LeftContentList = "CIleftContent";
		private readonly string RightContentList = "CIrightContent";
		private readonly string SystemButton = "systembtn";
		private readonly string ToggleButton = "techtoggle";
		private readonly string HeaderText = "headertxt";
		private readonly string DirectionTest = "dirtext";
		private int intelMissionID;
		private readonly GameSession _game;
		private IntelMissionInfo missioninfo;
		private Dictionary<int, string> _techFamilyPanels;
		private Dictionary<int, TechFamily> _FamilyID;
		private Dictionary<string, bool> _CheckedTechs;
		private Dictionary<string, string> _TechPanels;
		private Dictionary<string, string> _TechToggles;
		public CounterIntelSelectTechDialog(GameSession game, int intel_mission_id) : base(game.App, "counterIntel_Standard")
		{
			this.intelMissionID = intel_mission_id;
			this.missioninfo = game.GameDatabase.GetIntelInfo(this.intelMissionID);
			this._game = game;
		}
		public override void Initialize()
		{
			this._techFamilyPanels = new Dictionary<int, string>();
			this._FamilyID = new Dictionary<int, TechFamily>();
			this._CheckedTechs = new Dictionary<string, bool>();
			this._TechPanels = new Dictionary<string, string>();
			this._TechToggles = new Dictionary<string, string>();
			this._app.UI.SetEnabled(base.UI.Path(new string[]
			{
				base.ID,
				"okbtn"
			}), false);
			this.PopulateTechFamilyList(true);
			this._game.UI.SetText(base.UI.Path(new string[]
			{
				base.ID,
				this.HeaderText
			}), App.Localize("@UI_COUNTER_INTEL_TECH"));
			this._game.UI.SetText(base.UI.Path(new string[]
			{
				base.ID,
				this.DirectionTest
			}), App.Localize("@UI_COUNTER_INTEL_TECH_DIR"));
		}
		private void PopulateTechFamilyList(bool SelectFirst = false)
		{
			int num = 0;
			foreach (TechFamily techf in this._game.AssetDatabase.MasterTechTree.TechFamilies)
			{
				PlayerTechInfo playerTechInfo = this._game.GameDatabase.GetPlayerTechInfos(this._game.LocalPlayer.ID).FirstOrDefault((PlayerTechInfo x) => this._game.AssetDatabase.MasterTechTree.Technologies.FirstOrDefault((Tech y) => y.Id == x.TechFileID).Family == techf.Id);
				if (playerTechInfo != null)
				{
					this._game.UI.AddItem(this.LeftContentList, "", num, "", "FamilyTechCard");
					this._FamilyID.Add(num, techf);
					string itemGlobalID = this._game.UI.GetItemGlobalID(this.LeftContentList, "", num, "");
					this._techFamilyPanels.Add(num, itemGlobalID);
					this._game.UI.SetPropertyString(this._game.UI.Path(new string[]
					{
						itemGlobalID,
						this.SystemButton
					}), "id", this.SystemButton + "|" + num);
					this._game.UI.SetText(this._game.UI.Path(new string[]
					{
						itemGlobalID,
						"techLabel"
					}), techf.Name);
					this._game.UI.SetPropertyString(this._game.UI.Path(new string[]
					{
						itemGlobalID,
						"icon"
					}), "sprite", Path.GetFileNameWithoutExtension(techf.Icon));
					num++;
				}
			}
			if (SelectFirst && this._techFamilyPanels.Any<KeyValuePair<int, string>>())
			{
				this.SelectFamily(this._techFamilyPanels.First<KeyValuePair<int, string>>().Key);
			}
		}
		private void SelectFamily(int familypanelid)
		{
			this.SetSyncedTechFamily(this._FamilyID[familypanelid]);
			foreach (int current in this._techFamilyPanels.Keys)
			{
				base.UI.SetVisible(base.UI.Path(new string[]
				{
					this._techFamilyPanels[current],
					"selected"
				}), current == familypanelid);
			}
		}
		protected void SetSyncedTechFamily(TechFamily family)
		{
			this._game.UI.ClearItems(this.RightContentList);
			this._TechToggles.Clear();
			this._TechPanels.Clear();
			List<PlayerTechInfo> list = (
				from x in this._game.GameDatabase.GetPlayerTechInfos(this._game.LocalPlayer.ID)
				where this._game.AssetDatabase.MasterTechTree.Technologies.FirstOrDefault((Tech j) => j.Id == x.TechFileID).Family == family.Id
				select x).ToList<PlayerTechInfo>();
			foreach (PlayerTechInfo tech in list)
			{
				if (this._game.AssetDatabase.MasterTechTree.Technologies.FirstOrDefault((Tech x) => x.Id == tech.TechFileID).Allows.Any((Allowable x) => x.GetFactionProbabilityPercentage(this._game.LocalPlayer.Faction.Name.ToLower()) > 0f))
				{
					this._game.UI.AddItem(this.RightContentList, "", tech.TechID, "", "TechCard_Toggle");
					string itemGlobalID = this._game.UI.GetItemGlobalID(this.RightContentList, "", tech.TechID, "");
					this._TechPanels.Add(tech.TechFileID, itemGlobalID);
					this._game.UI.SetChecked(base.UI.Path(new string[]
					{
						itemGlobalID,
						"techtoggle"
					}), this._CheckedTechs.Any((KeyValuePair<string, bool> x) => x.Key == tech.TechFileID) && this._CheckedTechs[tech.TechFileID]);
					this._game.UI.SetVisible(base.UI.Path(new string[]
					{
						itemGlobalID,
						"contentSelected"
					}), this._CheckedTechs.Any((KeyValuePair<string, bool> x) => x.Key == tech.TechFileID) && this._CheckedTechs[tech.TechFileID]);
					this._game.UI.SetPropertyString(base.UI.Path(new string[]
					{
						itemGlobalID,
						"techtoggle"
					}), "id", "techtoggle|" + this._FamilyID.FirstOrDefault((KeyValuePair<int, TechFamily> x) => x.Value == family).Key.ToString() + "|" + tech.TechFileID);
					UICommChannel arg_35B_0 = this._game.UI;
					UICommChannel arg_356_0 = base.UI;
					string[] array = new string[2];
					array[0] = itemGlobalID;
					array[1] = "techtoggle|" + this._FamilyID.FirstOrDefault((KeyValuePair<int, TechFamily> x) => x.Value == family).Key.ToString() + "|" + tech.TechFileID;
					string globalID = arg_35B_0.GetGlobalID(arg_356_0.Path(array));
					this._TechToggles.Add(tech.TechFileID, globalID);
					this._game.UI.SetText(this._game.UI.Path(new string[]
					{
						itemGlobalID,
						"techLabel"
					}), this._game.AssetDatabase.MasterTechTree.Technologies.FirstOrDefault((Tech x) => x.Id == tech.TechFileID).Name);
					this._game.UI.SetPropertyString(this._game.UI.Path(new string[]
					{
						itemGlobalID,
						"icon"
					}), "sprite", Path.GetFileNameWithoutExtension(this._game.AssetDatabase.MasterTechTree.Technologies.FirstOrDefault((Tech x) => x.Id == tech.TechFileID).Icon));
				}
			}
		}
		private void SetChecked(int treeid, string techid, bool forcechecked = false)
		{
			foreach (string current in this._TechToggles.Keys.ToList<string>())
			{
				this._CheckedTechs[current] = (current == techid);
				this._game.UI.SetChecked(this._TechToggles[current], current == techid);
				this._game.UI.SetVisible(base.UI.Path(new string[]
				{
					this._TechPanels[current],
					"contentSelected"
				}), current == techid);
			}
			if (!this._CheckedTechs.Keys.Contains(techid))
			{
				this._CheckedTechs.Add(techid, true);
			}
			foreach (int current2 in this._techFamilyPanels.Keys)
			{
				this._game.UI.SetVisible(this._game.UI.Path(new string[]
				{
					this._techFamilyPanels[current2],
					"contentSelected"
				}), current2 == treeid);
			}
			this._app.UI.SetEnabled(base.UI.Path(new string[]
			{
				base.ID,
				"okbtn"
			}), this._CheckedTechs.Any((KeyValuePair<string, bool> x) => x.Value));
		}
		private void AutoSelect()
		{
			Random random = new Random();
			TechFamily fam = this._FamilyID.Values.ToArray<TechFamily>()[random.Next(0, this._FamilyID.Values.Count)];
			this.SetSyncedTechFamily(fam);
			this.SetChecked(this._FamilyID.FirstOrDefault((KeyValuePair<int, TechFamily> x) => x.Value == fam).Key, this._TechPanels.Keys.ToArray<string>()[random.Next(0, this._TechPanels.Keys.Count)], false);
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (panelName.StartsWith(this.SystemButton))
				{
					string[] array = panelName.Split(new char[]
					{
						'|'
					});
					int familypanelid = int.Parse(array[1]);
					this.SelectFamily(familypanelid);
				}
				else
				{
					if (panelName.StartsWith(this.ToggleButton))
					{
						string[] array2 = panelName.Split(new char[]
						{
							'|'
						});
						int treeid = int.Parse(array2[1]);
						string techid = array2[2];
						this.SetChecked(treeid, techid, false);
					}
					else
					{
						if (panelName == "autoselectbtn")
						{
							this.AutoSelect();
						}
						else
						{
							if (panelName == "okbtn")
							{
								this.DeployCounterIntel();
								this._game.UI.CloseDialog(this, true);
							}
						}
					}
				}
			}
            bool flag1 = msgType == "list_sel_changed";
		}
		private void DeployCounterIntel()
		{
			List<CounterIntelResponse> list = this._game.GameDatabase.GetCounterIntelResponses(this.intelMissionID).ToList<CounterIntelResponse>();
			foreach (CounterIntelResponse current in list)
			{
				this._app.GameDatabase.RemoveCounterIntelResponse(current.ID);
			}
			this._app.GameDatabase.InsertCounterIntelResponse(this.intelMissionID, false, this._CheckedTechs.First((KeyValuePair<string, bool> x) => x.Value).Key);
		}
		protected override void OnUpdate()
		{
		}
		public override string[] CloseDialog()
		{
			return null;
		}
	}
}
