using Kerberos.Sots.Engine;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.GameStates
{
	internal class SotspediaState : GameState, IKeyBindListener
	{
		private GameObjectSet _crits;
		private bool _searchMode;
		private string _initialPage;
		private static readonly string UIHumanTOC = "gameHumanTOC";
		private static readonly string UIHiverTOC = "gameHiverTOC";
		private static readonly string UITarkasTOC = "gameTarkasTOC";
		private static readonly string UILiirTOC = "gameLiirTOC";
		private static readonly string UIMorrigiTOC = "gameMorrigiTOC";
		private static readonly string UIZuulTOC = "gameZuulTOC";
		private static readonly string UITechTOC = "gameTechTOC";
		private static readonly string UIHumanButton = "gameHumanButton";
		private static readonly string UIHiverButton = "gameHiverButton";
		private static readonly string UITarkasButton = "gameTarkasButton";
		private static readonly string UILiirButton = "gameLiirButton";
		private static readonly string UIMorrigiButton = "gameMorrigiButton";
		private static readonly string UIZuulButton = "gameZuulButton";
		private static readonly string UITechButton = "gameTechButton";
		private static readonly string UIHumanContent = "gameHumanContent";
		private static readonly string UIHiverContent = "gameHiverContent";
		private static readonly string UITarkasContent = "gameTarkasContent";
		private static readonly string UILiirContent = "gameLiirContent";
		private static readonly string UIMorrigiContent = "gameMorrigiContent";
		private static readonly string UIZuulContent = "gameZuulContent";
		private static readonly string UITechContent = "gameTechContent";
		private static readonly string UIBackButton = "gameExitButton";
		private string _currentCategory;
		private string UITableOfContents
		{
			get
			{
				return string.Format("game{0}TOC", this._currentCategory);
			}
		}
		private string UIArticlePage
		{
			get
			{
				return string.Format("game{0}Page", this._currentCategory);
			}
		}
		public static void NavigateToLink(App app, string name)
		{
			if (app.CurrentState == app.GetGameState<SotspediaState>())
			{
				app.GetGameState<SotspediaState>().NavigateToLink(name);
				return;
			}
			app.SwitchGameState(app.GetGameState<SotspediaState>(), new object[]
			{
				name
			});
		}
		public SotspediaState(App game) : base(game)
		{
		}
		protected override void OnPrepare(GameState prev, object[] parms)
		{
			this._crits = new GameObjectSet(base.App);
			base.App.UI.LoadScreen("Sotspedia");
			this._initialPage = null;
			if (parms != null && parms.Length > 0)
			{
				this._initialPage = (parms[0] as string);
			}
		}
		private void UICommChannel_GameEvent(string eventName, string[] eventParams)
		{
			if (eventName != null)
			{
				if (eventName == "LinkClicked")
				{
					this.ProcessGameEvent_LinkClicked(eventParams);
					return;
				}
				if (!(eventName == "SearchMode"))
				{
					return;
				}
				if (eventParams[0] == "true")
				{
					this._searchMode = true;
					return;
				}
				this._searchMode = false;
			}
		}
		private string InferCategory(string pageFile)
		{
			if (pageFile.Contains("Hiver"))
			{
				return "Hiver";
			}
			if (pageFile.Contains("Human"))
			{
				return "Human";
			}
			if (pageFile.Contains("Tarka"))
			{
				return "Tarkas";
			}
			if (pageFile.Contains("Zuul"))
			{
				return "Zuul";
			}
			if (pageFile.Contains("Liir"))
			{
				return "Liir";
			}
			if (pageFile.Contains("Morrigi"))
			{
				return "Morrigi";
			}
			if (pageFile.Contains("Techs"))
			{
				return "Tech";
			}
			return this._currentCategory;
		}
		private void NavigateToLink(string name)
		{
			if (!this._searchMode)
			{
				string currentCategory = this.InferCategory(name);
				this.SetCurrentCategory(currentCategory);
			}
			base.App.UI.SetPropertyString(this.UIArticlePage, "link", name);
		}
		private void ProcessGameEvent_LinkClicked(string[] eventParams)
		{
			this.NavigateToLink(eventParams[0]);
		}
		private void SetCurrentCategory(string category)
		{
			this._currentCategory = category;
			base.App.UI.SetVisible(SotspediaState.UIHumanContent, this._currentCategory == "Human");
			base.App.UI.SetVisible(SotspediaState.UIHiverContent, this._currentCategory == "Hiver");
			base.App.UI.SetVisible(SotspediaState.UITarkasContent, this._currentCategory == "Tarkas");
			base.App.UI.SetVisible(SotspediaState.UILiirContent, this._currentCategory == "Liir");
			base.App.UI.SetVisible(SotspediaState.UIMorrigiContent, this._currentCategory == "Morrigi");
			base.App.UI.SetVisible(SotspediaState.UIZuulContent, this._currentCategory == "Zuul");
			base.App.UI.SetVisible(SotspediaState.UITechContent, this._currentCategory == "Tech");
		}
		private string GetPanelFromCurrentCategory()
		{
			string currentCategory;
			switch (currentCategory = this._currentCategory)
			{
			case "Human":
				return SotspediaState.UIHumanTOC;
			case "Hiver":
				return SotspediaState.UIHiverTOC;
			case "Tarkas":
				return SotspediaState.UITarkasTOC;
			case "Liir":
				return SotspediaState.UILiirTOC;
			case "Morrigi":
				return SotspediaState.UIMorrigiTOC;
			case "Zuul":
				return SotspediaState.UIZuulTOC;
			case "Tech":
				return SotspediaState.UITechTOC;
			}
			return "";
		}
		protected override void UICommChannel_OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (panelName == SotspediaState.UIHumanButton)
				{
					this.SetCurrentCategory("Human");
					return;
				}
				if (panelName == SotspediaState.UIHiverButton)
				{
					this.SetCurrentCategory("Hiver");
					return;
				}
				if (panelName == SotspediaState.UITarkasButton)
				{
					this.SetCurrentCategory("Tarkas");
					return;
				}
				if (panelName == SotspediaState.UILiirButton)
				{
					this.SetCurrentCategory("Liir");
					return;
				}
				if (panelName == SotspediaState.UIMorrigiButton)
				{
					this.SetCurrentCategory("Morrigi");
					return;
				}
				if (panelName == SotspediaState.UIZuulButton)
				{
					this.SetCurrentCategory("Zuul");
					return;
				}
				if (panelName == SotspediaState.UITechButton)
				{
					this.SetCurrentCategory("Tech");
					return;
				}
				if (panelName == SotspediaState.UIBackButton)
				{
					if (base.App.PreviousState != null && (base.App.PreviousState is StarMapState || base.App.PreviousState is ResearchScreenState))
					{
						base.App.SwitchGameState(base.App.PreviousState, new object[0]);
						return;
					}
					if (base.App.PreviousState != null && base.App.PreviousState is DesignScreenState)
					{
						base.App.SwitchGameState<StarMapState>(new object[0]);
						return;
					}
					base.App.SwitchGameStateViaLoadingScreen(null, null, base.App.PreviousState ?? base.App.GetGameState<MainMenuState>(), new object[0]);
					return;
				}
			}
			else
			{
				if (msgType == "text_changed" && panelName.StartsWith("searchBox"))
				{
					base.App.UI.Send(new object[]
					{
						"UpdateSearchTerms",
						this.GetPanelFromCurrentCategory(),
						msgParams[0]
					});
				}
			}
		}
		protected override void OnEnter()
		{
			base.App.UI.SetScreen("Sotspedia");
			base.App.UI.GameEvent += new UIEventGameEvent(this.UICommChannel_GameEvent);
			base.App.UI.Send(new object[]
			{
				"SetCategory",
				SotspediaState.UIHumanTOC,
				"Human"
			});
			base.App.UI.Send(new object[]
			{
				"SetCategory",
				SotspediaState.UIHiverTOC,
				"Hiver"
			});
			base.App.UI.Send(new object[]
			{
				"SetCategory",
				SotspediaState.UITarkasTOC,
				"Tarkas"
			});
			base.App.UI.Send(new object[]
			{
				"SetCategory",
				SotspediaState.UILiirTOC,
				"Liir"
			});
			base.App.UI.Send(new object[]
			{
				"SetCategory",
				SotspediaState.UIMorrigiTOC,
				"Morrigi"
			});
			base.App.UI.Send(new object[]
			{
				"SetCategory",
				SotspediaState.UIZuulTOC,
				"Zuul"
			});
			base.App.UI.Send(new object[]
			{
				"SetCategory",
				SotspediaState.UITechTOC,
				"Tech"
			});
			this.SetCurrentCategory("Tarkas");
			List<object> list = new List<object>();
			List<string> researchedTechs = base.App.UserProfile.ResearchedTechs;
			list.Add("SotspediaSetUnlockKeys");
			list.Add(SotspediaState.UIHumanTOC);
			list.Add(researchedTechs.Count);
			foreach (string current in researchedTechs)
			{
				list.Add(current);
			}
			base.App.UI.Send(list.ToArray());
			if (this._initialPage != null)
			{
				this.NavigateToLink(this._initialPage);
			}
			base.App.HotKeyManager.AddListener(this);
		}
		protected override void OnExit(GameState prev, ExitReason reason)
		{
			base.App.HotKeyManager.RemoveListener(this);
		}
		protected override void OnUpdate()
		{
		}
		public override bool IsReady()
		{
			return this._crits != null && this._crits.IsReady() && base.IsReady();
		}
		public override void OnEngineMessage(InteropMessageID messageID, ScriptMessageReader mr)
		{
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
					base.App.UI.LockUI();
					base.App.SwitchGameState<DesignScreenState>(new object[]
					{
						false,
						base.Name
					});
					return true;
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
					return false;
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
