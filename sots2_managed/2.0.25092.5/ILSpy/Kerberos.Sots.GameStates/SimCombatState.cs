using Kerberos.Sots.Combat;
using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.PlayerFramework;
using System;
using System.IO;
namespace Kerberos.Sots.GameStates
{
	internal class SimCombatState : CommonCombatState
	{
		public SimCombatState(App game) : base(game)
		{
		}
		protected override GameState GetExitState()
		{
			return base.App.GetGameState<StarMapState>();
		}
		protected override void UICommChannel_OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
            bool flag1 = msgType == "button_clicked";
		}
		protected override void OnCombatEnding()
		{
		}
		protected override void OnPrepare(GameState prev, object[] stateParams)
		{
			base.SimMode = true;
			base.OnPrepare(prev, stateParams);
			base.App.UI.LoadScreen("SimCombat");
		}
		protected override void OnEnter()
		{
			base.OnEnter();
			base.App.UI.SetScreen("SimCombat");
			base.App.PostEngineMessage(new object[]
			{
				InteropMessageID.IMID_ENGINE_COMBAT_CONNECT_UI,
				base.Combat.ObjectID,
				"SimCombat"
			});
			this.BuildAvatarList("avatarList");
		}
		protected void BuildAvatarList(string panelName)
		{
			base.App.UI.ClearItems(panelName);
			int num = 0;
			foreach (Player current in base.PlayersInCombat)
			{
				base.App.UI.AddItem(panelName, string.Empty, num, string.Empty);
				string itemGlobalID = base.App.UI.GetItemGlobalID(panelName, string.Empty, num, string.Empty);
				PlayerInfo playerInfo = base.App.GameDatabase.GetPlayerInfo(current.ID);
				base.App.UI.SetPropertyString(base.App.UI.Path(new string[]
				{
					itemGlobalID,
					"playeravatar"
				}), "sprite", Path.GetFileNameWithoutExtension(playerInfo.AvatarAssetPath));
				base.App.UI.SetPropertyString(base.App.UI.Path(new string[]
				{
					itemGlobalID,
					"badge"
				}), "sprite", Path.GetFileNameWithoutExtension(playerInfo.BadgeAssetPath));
				base.App.UI.SetPropertyColor(base.App.UI.Path(new string[]
				{
					itemGlobalID,
					"primaryColor"
				}), "color", playerInfo.PrimaryColor * 255f);
				base.App.UI.SetPropertyColor(base.App.UI.Path(new string[]
				{
					itemGlobalID,
					"secondaryColor"
				}), "color", playerInfo.SecondaryColor * 255f);
				base.App.UI.SetPropertyString(base.App.UI.Path(new string[]
				{
					itemGlobalID,
					"name"
				}), "text", playerInfo.Name);
				num++;
			}
		}
		protected override void OnExit(GameState prev, ExitReason reason)
		{
			base.OnExit(prev, reason);
		}
		protected override void SyncPlayerList()
		{
		}
		public override void OnEngineMessage(InteropMessageID messageID, ScriptMessageReader mr)
		{
			if (messageID == InteropMessageID.IMID_SCRIPT_MOVE_ORDER)
			{
				int playerID = mr.ReadInteger();
				CombatAI commanderForPlayerID = base.GetCommanderForPlayerID(playerID);
				if (commanderForPlayerID != null)
				{
					return;
				}
			}
			else
			{
				base.OnEngineMessage(messageID, mr);
			}
		}
	}
}
