using Kerberos.Sots.Engine;
using Kerberos.Sots.Strategy;
using System;
using System.IO;
using System.Linq;
using System.Xml;
namespace Kerberos.Sots.GameStates
{
	internal class TestLoadCombatState : GameState
	{
		private string[] _availableConfigFiles;
		private string _selectedConfigFile;
		public TestLoadCombatState(App game) : base(game)
		{
		}
		protected override void OnPrepare(GameState prev, object[] parms)
		{
			string searchPattern = "*.xml";
			this._availableConfigFiles = Directory.EnumerateFiles(Path.Combine(base.App.GameRoot, "data"), searchPattern, SearchOption.TopDirectoryOnly).ToArray<string>();
			this._selectedConfigFile = null;
			base.App.UI.LoadScreen("TestLoadCombat");
		}
		protected override void OnEnter()
		{
			if (base.App.LocalPlayer == null)
			{
				base.App.NewGame();
			}
			base.App.UI.SetScreen("TestLoadCombat");
			base.App.UI.ClearItems("combatConfigList");
			for (int i = 0; i < this._availableConfigFiles.Length; i++)
			{
				base.App.UI.AddItem("combatConfigList", string.Empty, i, Path.GetFileNameWithoutExtension(this._availableConfigFiles[i]));
			}
			base.App.UI.SetEnabled("gameNextButton", false);
		}
		protected override void OnExit(GameState prev, ExitReason reason)
		{
			this._availableConfigFiles = null;
			this._selectedConfigFile = null;
		}
		protected override void UICommChannel_OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "list_sel_changed")
			{
				if (panelName == "combatConfigList")
				{
					if (!string.IsNullOrEmpty(msgParams[0]))
					{
						if (this._selectedConfigFile == null)
						{
							base.App.UI.SetEnabled("gameNextButton", true);
						}
						this._selectedConfigFile = this._availableConfigFiles[int.Parse(msgParams[0])];
						return;
					}
					if (this._selectedConfigFile != null)
					{
						base.App.UI.SetEnabled("gameNextButton", false);
					}
					this._selectedConfigFile = null;
					return;
				}
			}
			else
			{
				if (msgType == "button_clicked")
				{
					if (panelName == "gameExitButton")
					{
						this.GoBack();
						return;
					}
					if (panelName == "gameNextButton")
					{
						this.GoNext();
					}
				}
			}
		}
		private void GoBack()
		{
			base.App.SwitchGameState("MainMenuState");
		}
		private void GoNext()
		{
			try
			{
				string selectedConfigFile = this._selectedConfigFile;
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.Load(selectedConfigFile);
				base.App.SwitchGameState<CombatState>(new object[]
				{
					new PendingCombat(),
					xmlDocument,
					true
				});
			}
			catch (Exception)
			{
				base.App.UI.SetEnabled("gameNextButton", false);
				throw;
			}
		}
		protected override void OnUpdate()
		{
		}
		public override void OnEngineMessage(InteropMessageID messageID, ScriptMessageReader mr)
		{
		}
	}
}
