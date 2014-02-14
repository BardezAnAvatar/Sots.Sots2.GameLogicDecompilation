using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.UI
{
	internal class EventHistoryDialog : Dialog
	{
		public const string OKButton = "buttonOK";
		public const string EventList = "eventList";
		public const string EventItemList = "eventItemList";
		public const string EventTurnList = "turnList";
		public EventHistoryDialog(App game) : base(game, "dialogEventHistory")
		{
		}
		public override void Initialize()
		{
			base.Initialize();
			IEnumerable<TurnEvent> turnEventsByPlayerID = this._app.GameDatabase.GetTurnEventsByPlayerID(this._app.LocalPlayer.ID);
			if (turnEventsByPlayerID.Count<TurnEvent>() == 0)
			{
				return;
			}
			int turnCount = this._app.GameDatabase.GetTurnCount();
			for (int i = turnCount; i > 0; i -= 25)
			{
				int num = i - 25;
				if (num < 0)
				{
					num = 0;
				}
				this._app.UI.AddItem(this._app.UI.Path(new string[]
				{
					base.ID,
					"turnList"
				}), "", i, string.Concat(new string[]
				{
					App.Localize("@UI_GENERAL_TURN"),
					" ",
					(num + 1).ToString(),
					" - ",
					i.ToString()
				}));
			}
			this.SyncTurns(turnCount - 25, turnCount);
		}
		private void SyncTurns(int from, int to)
		{
			int num = 0;
			int num2 = -1;
			string text = "";
			this._app.UI.ClearItems(this._app.UI.Path(new string[]
			{
				base.ID,
				"eventList"
			}));
			IEnumerable<TurnEvent> source = 
				from x in this._app.GameDatabase.GetTurnEventsByPlayerID(this._app.LocalPlayer.ID)
				where x.TurnNumber >= @from && x.TurnNumber <= to
				select x;
			if (source.Count<TurnEvent>() == 0)
			{
				return;
			}
			foreach (TurnEvent current in source.Reverse<TurnEvent>())
			{
				if (num2 != current.TurnNumber)
				{
					this._app.UI.AddItem(this._app.UI.Path(new string[]
					{
						base.ID,
						"eventList"
					}), "", current.TurnNumber, "");
					text = this._app.UI.GetItemGlobalID(this._app.UI.Path(new string[]
					{
						base.ID,
						"eventList"
					}), "", current.TurnNumber, "");
					this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
					{
						text,
						"turnNumber"
					}), "text", App.Localize("@TURN") + " " + current.TurnNumber.ToString());
					num2 = current.TurnNumber;
				}
				string eventMessage = current.GetEventMessage(this._app.Game);
				this._app.UI.AddItem(this._app.UI.Path(new string[]
				{
					text,
					"eventItemList"
				}), "", num, "");
				string itemGlobalID = this._app.UI.GetItemGlobalID(this._app.UI.Path(new string[]
				{
					text,
					"eventItemList"
				}), "", num, "");
				this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"eventInfo"
				}), "text", eventMessage);
				this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"eventImage"
				}), "texture", TurnEvent.GetTurnEventSprite(this._app.Game, current));
				num++;
			}
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked" && panelName == "buttonOK")
			{
				this._app.UI.CloseDialog(this, true);
			}
			if (msgType == "list_sel_changed" && panelName == "turnList")
			{
				int num = int.Parse(msgParams[0]);
				this.SyncTurns(num - 25, num);
			}
		}
		public override string[] CloseDialog()
		{
			return null;
		}
	}
}
