using Kerberos.Sots.Data;
using System;
using System.IO;
namespace Kerberos.Sots.UI
{
	internal class StarmapUI
	{
		public static readonly string UIPlayerCardName = "lblPlayerName";
		public static readonly string UIPlayerCardAvatar = "imgAvatar";
		public static readonly string UIPlayerCardBadge = "imgBadge";
		public static readonly string UIPlayerCardRelation = "imgRelation";
		public static void SyncPlayerCard(App game, string panelName, int playerId)
		{
			StarmapUI.SyncPlayerCard(game, panelName, game.GameDatabase.GetPlayerInfo(playerId));
		}
		public static void SyncPlayerCard(App game, string panelName, PlayerInfo pi)
		{
			if (pi == null)
			{
				return;
			}
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelName,
				StarmapUI.UIPlayerCardName
			}), "text", pi.Name);
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelName,
				StarmapUI.UIPlayerCardAvatar
			}), "sprite", Path.GetFileNameWithoutExtension(pi.AvatarAssetPath));
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelName,
				StarmapUI.UIPlayerCardBadge
			}), "sprite", Path.GetFileNameWithoutExtension(pi.BadgeAssetPath));
			DiplomacyInfo diplomacyInfo = game.GameDatabase.GetDiplomacyInfo(game.Game.LocalPlayer.ID, pi.ID);
			DiplomaticMood diplomaticMood = diplomacyInfo.GetDiplomaticMood();
			game.UI.SetVisible(game.UI.Path(new string[]
			{
				panelName,
				StarmapUI.UIPlayerCardRelation
			}), true);
			if (diplomaticMood == DiplomaticMood.Love && pi.ID != game.LocalPlayer.ID)
			{
				game.UI.SetPropertyString(game.UI.Path(new string[]
				{
					panelName,
					StarmapUI.UIPlayerCardRelation
				}), "sprite", "Love");
				return;
			}
			if (diplomaticMood == DiplomaticMood.Hatred && pi.ID != game.LocalPlayer.ID)
			{
				game.UI.SetPropertyString(game.UI.Path(new string[]
				{
					panelName,
					StarmapUI.UIPlayerCardRelation
				}), "sprite", "Hate");
				return;
			}
			game.UI.SetVisible(game.UI.Path(new string[]
			{
				panelName,
				StarmapUI.UIPlayerCardRelation
			}), false);
		}
		public static void SyncPlayerCard(App game, string panelName, string playerName, string avatarSprite, string badgeSprite, string relationSprite)
		{
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelName,
				StarmapUI.UIPlayerCardName
			}), "text", playerName);
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelName,
				StarmapUI.UIPlayerCardAvatar
			}), "sprite", avatarSprite);
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelName,
				StarmapUI.UIPlayerCardBadge
			}), "sprite", badgeSprite);
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelName,
				StarmapUI.UIPlayerCardRelation
			}), "sprite", relationSprite);
		}
	}
}
