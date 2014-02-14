using Kerberos.Sots.Strategy;
using System;
namespace Kerberos.Sots.UI
{
	internal class TurnEventUI
	{
		public const string UITurnEventImage = "turnEventImage";
		public const string UITurnEventMessage = "turnEventMessage";
		public const string UITurnEventNext = "turnEventNext";
		public const string UITurnEventPrevious = "turnEventPrevious";
		internal static void SyncTurnEventWidget(GameSession game, string panelName, TurnEvent turnEvent)
		{
			if (turnEvent == null)
			{
				game.UI.SetPropertyString("turnEventMessage", "text", "");
				game.UI.SetPropertyString("turnEventImage", "sprite", "");
				game.UI.SetVisible("turnEventNext", false);
				game.UI.SetVisible("turnEventPrevious", false);
				return;
			}
			game.UI.SetPropertyString("turnEventMessage", "text", turnEvent.GetEventMessage(game));
			game.UI.SetPropertyString("turnEventImage", "texture", TurnEvent.GetTurnEventSprite(game, turnEvent));
			game.UI.SetVisible("turnEventNext", true);
			game.UI.SetVisible("turnEventPrevious", true);
		}
		internal static void SyncTurnEventTicker(GameSession game, string panelName)
		{
			game.UI.ClearItems(panelName);
			game.UI.AddItem(panelName, "", 8000000, "", "tickerEvent_Spacer");
			game.UI.AddItem(panelName, "", 8000001, "", "tickerEvent_Item");
			string itemGlobalID = game.UI.GetItemGlobalID(panelName, "", 8000001, "");
			game.UI.SetText(game.UI.Path(new string[]
			{
				itemGlobalID,
				"tickerEventButton",
				"idle"
			}), string.Concat(new string[]
			{
				App.Localize("@TURN"),
				" ",
				game.GameDatabase.GetTurnCount().ToString(),
				" ",
				App.Localize("@UI_EVENTS")
			}));
			game.UI.SetText(game.UI.Path(new string[]
			{
				itemGlobalID,
				"tickerEventButton",
				"mouse_over"
			}), string.Concat(new string[]
			{
				App.Localize("@TURN"),
				" ",
				game.GameDatabase.GetTurnCount().ToString(),
				" ",
				App.Localize("@UI_EVENTS")
			}));
			game.UI.SetText(game.UI.Path(new string[]
			{
				itemGlobalID,
				"tickerEventButton",
				"pressed"
			}), string.Concat(new string[]
			{
				App.Localize("@TURN"),
				" ",
				game.GameDatabase.GetTurnCount().ToString(),
				" ",
				App.Localize("@UI_EVENTS")
			}));
			game.UI.SetText(game.UI.Path(new string[]
			{
				itemGlobalID,
				"tickerEventButton",
				"disabled"
			}), string.Concat(new string[]
			{
				App.Localize("@TURN"),
				" ",
				game.GameDatabase.GetTurnCount().ToString(),
				" ",
				App.Localize("@UI_EVENTS")
			}));
			foreach (TurnEvent current in game.TurnEvents)
			{
				game.UI.AddItem(panelName, "", current.ID, "", "tickerEvent_Item");
				itemGlobalID = game.UI.GetItemGlobalID(panelName, "", current.ID, "");
				game.UI.SetText(game.UI.Path(new string[]
				{
					itemGlobalID,
					"tickerEventButton",
					"idle"
				}), current.GetEventName(game));
				game.UI.SetText(game.UI.Path(new string[]
				{
					itemGlobalID,
					"tickerEventButton",
					"mouse_over"
				}), current.GetEventName(game));
				game.UI.SetText(game.UI.Path(new string[]
				{
					itemGlobalID,
					"tickerEventButton",
					"pressed"
				}), current.GetEventName(game));
				game.UI.SetText(game.UI.Path(new string[]
				{
					itemGlobalID,
					"tickerEventButton",
					"disabled"
				}), current.GetEventName(game));
				game.UI.SetPropertyString(game.UI.Path(new string[]
				{
					itemGlobalID,
					"tickerEventButton"
				}), "id", "tickerEventButton|" + current.ID.ToString());
			}
			game.UI.AddItem(panelName, "", 9000000, "", "tickerEvent_Spacer");
		}
	}
}
