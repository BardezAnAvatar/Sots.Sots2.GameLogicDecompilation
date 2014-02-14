using Kerberos.Sots.Engine;
using System;
using System.IO;
namespace Kerberos.Sots.GameStates
{
	internal class TestUIState : GameState
	{
		private GameObjectSet _crits;
		public override bool IsScreenState
		{
			get
			{
				return false;
			}
		}
		protected override void OnPrepare(GameState prev, object[] parms)
		{
			this._crits = new GameObjectSet(base.App);
		}
		protected override void UICommChannel_OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			base.UICommChannel_OnPanelMessage(panelName, msgType, msgParams);
			if (msgType == "button_clicked")
			{
				string text = "gameReloadScreen_";
				if (string.Compare(panelName, 0, text, 0, text.Length) == 0)
				{
					AssetDatabase.CommonStrings.Reload();
					string text2 = panelName.Substring(text.Length);
					base.App.UI.Send(new object[]
					{
						"ReloadScreen",
						text2
					});
					if (text2 == "TestDegrassi")
					{
						base.App.UI.AddItem("eventList", "", 0, "");
						string itemGlobalID = base.App.UI.GetItemGlobalID("eventList", "", 0, "");
						base.App.UI.AddItem(base.App.UI.Path(new string[]
						{
							itemGlobalID,
							"eventItemList"
						}), "", 0, "");
						string itemGlobalID2 = base.App.UI.GetItemGlobalID(base.App.UI.Path(new string[]
						{
							itemGlobalID,
							"eventItemList"
						}), "", 0, "");
						base.App.UI.SetPropertyString(base.App.UI.Path(new string[]
						{
							itemGlobalID2,
							"eventInfo"
						}), "text", "Probably should have just copied some generic placeholder text, this is getting a little bit ridiculous.Probably should have just copied some generic placeholder text, this is getting a little bit ridiculous. Probably should have just copied some generic placeholder text, this is getting a little bit ridiculous.Probably should have just copied some generic placeholder text, this is getting a little bit ridiculous. Probably should have just copied some generic placeholder text, this is getting a little bit ridiculous.Probably should have just copied some generic placeholder text, this is getting a little bit ridiculous. Probably should have just copied some generic placeholder text, this is getting a little bit ridiculous.Probably should have just copied some generic placeholder text, this is getting a little bit ridiculous. Probably should have just copied some generic placeholder text, this is getting a little bit ridiculous.Probably should have just copied some generic placeholder text, this is getting a little bit ridiculous.");
						base.App.UI.AddItem(base.App.UI.Path(new string[]
						{
							itemGlobalID,
							"eventItemList"
						}), "", 1, "");
						itemGlobalID2 = base.App.UI.GetItemGlobalID(base.App.UI.Path(new string[]
						{
							itemGlobalID,
							"eventItemList"
						}), "", 1, "");
						base.App.UI.SetPropertyString(base.App.UI.Path(new string[]
						{
							itemGlobalID2,
							"eventInfo"
						}), "text", "holder text, this is getting a little bit ridiculous.Probably should have just copied some generic placeholder text, this is getting a little bit ridiculous. Probably should have just copied some generic placeholder text, this is getting a little bit ridiculous.Probably should have just copied some generic placeholder text, this is getting a little bit ridiculous. Probably should have just copied some generic placeholder text, this is getting a little bit ridiculous.Probably should have just copied some generic placeholder text, this is getting a little bit ridiculous.");
					}
				}
			}
		}
		protected override void OnEnter()
		{
			string text = Path.Combine(base.App.GameRoot, "ui\\screens\\TestUI.xml");
			this._crits.Add<PanelReference>(new object[]
			{
				text,
				"gameDebugUIControls"
			});
		}
		protected override void OnExit(GameState next, ExitReason reason)
		{
			if (this._crits != null)
			{
				this._crits.Dispose();
			}
		}
		protected override void OnUpdate()
		{
		}
		public override bool IsReady()
		{
			return this._crits != null && this._crits.IsReady();
		}
		public override void OnEngineMessage(InteropMessageID messageID, ScriptMessageReader mr)
		{
		}
		public TestUIState(App game) : base(game)
		{
		}
	}
}
