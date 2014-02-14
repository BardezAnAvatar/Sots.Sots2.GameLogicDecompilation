using Kerberos.Sots.Data;
using Kerberos.Sots.Strategy;
using System;
namespace Kerberos.Sots.UI.Dialogs
{
	internal class EspionagePlayerHeader : PanelBinding
	{
		private readonly GameSession _game;
		private readonly Label _playerNameLabel;
		private readonly Image _avatarImage;
		private readonly Image _badgeImage;
		private readonly Image _relationImage;
		public EspionagePlayerHeader(GameSession game, string dialogID) : base(game.UI, dialogID)
		{
			this._game = game;
			this._playerNameLabel = new Label(base.UI, base.UI.Path(new string[]
			{
				base.ID,
				"lblPlayerName"
			}));
			this._avatarImage = new Image(base.UI, base.UI.Path(new string[]
			{
				base.ID,
				"imgAvatar"
			}));
			this._badgeImage = new Image(base.UI, base.UI.Path(new string[]
			{
				base.ID,
				"imgBadge"
			}));
			this._relationImage = new Image(base.UI, base.UI.Path(new string[]
			{
				base.ID,
				"imgRelation"
			}));
			base.AddPanels(new PanelBinding[]
			{
				this._playerNameLabel,
				this._avatarImage,
				this._badgeImage,
				this._relationImage
			});
		}
		public void UpdateFromPlayerInfo(int localPlayerID, PlayerInfo targetPlayerInfo)
		{
			this._playerNameLabel.SetText(targetPlayerInfo.Name);
			this._avatarImage.SetTexture(targetPlayerInfo.AvatarAssetPath);
			this._badgeImage.SetTexture(targetPlayerInfo.BadgeAssetPath);
			DiplomacyInfo diplomacyInfo = this._game.GameDatabase.GetDiplomacyInfo(localPlayerID, targetPlayerInfo.ID);
			string text = null;
			if (diplomacyInfo != null)
			{
				text = diplomacyInfo.GetDiplomaticMoodSprite();
			}
			if (!string.IsNullOrEmpty(text))
			{
				this._relationImage.SetVisible(true);
				this._relationImage.SetSprite(text);
				return;
			}
			this._relationImage.SetVisible(false);
		}
	}
}
