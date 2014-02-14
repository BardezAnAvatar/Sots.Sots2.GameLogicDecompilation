using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Strategy;
using System;
using System.IO;
namespace Kerberos.Sots.UI
{
	internal class DiplomacyCard : PanelBinding
	{
		private App App;
		private PlayerInfo Player;
		private static string UIsecondaryColor = "secondaryColor";
		private static string UIempireColor = "empireColor";
		private static string UIavatar = "avatar_doodle";
		private static string UIbadge = "badge";
		private static string UIpeace = "peace";
		private static string UIwar = "war";
		private static string UImoodHatred = "moodHatred";
		private static string UImoodHostile = "moodHostile";
		private static string UImoodDistrust = "moodDistrust";
		private static string UImoodIndifferent = "moodIndifferent";
		private static string UImoodTrust = "moodTrust";
		private static string UImoodFriend = "moodFriend";
		private static string UImoodLove = "moodLove";
		public DiplomacyCard(App game, int playerid, UICommChannel ui, string id) : base(ui, id)
		{
			this.App = game;
			this.Player = this.App.GameDatabase.GetPlayerInfo(playerid);
		}
		protected override void OnPanelMessage(string panelId, string msgType, string[] msgParams)
		{
			base.OnPanelMessage(panelId, msgType, msgParams);
		}
		public void Initialize()
		{
			this.App.UI.SetPropertyColor(base.UI.Path(new string[]
			{
				base.ID,
				DiplomacyCard.UIsecondaryColor
			}), "color", this.Player.SecondaryColor * 255f);
			this.App.UI.SetPropertyColor(base.UI.Path(new string[]
			{
				base.ID,
				DiplomacyCard.UIempireColor
			}), "color", this.Player.PrimaryColor * 255f);
			this.App.UI.SetPropertyString(base.UI.Path(new string[]
			{
				base.ID,
				DiplomacyCard.UIavatar
			}), "sprite", Path.GetFileNameWithoutExtension(this.Player.AvatarAssetPath));
			this.App.UI.SetPropertyString(base.UI.Path(new string[]
			{
				base.ID,
				DiplomacyCard.UIbadge
			}), "sprite", Path.GetFileNameWithoutExtension(this.Player.BadgeAssetPath));
			if (this.App.GameDatabase.GetDiplomacyStateBetweenPlayers(this.App.LocalPlayer.ID, this.Player.ID) == DiplomacyState.WAR)
			{
				this.App.UI.SetVisible(base.UI.Path(new string[]
				{
					base.ID,
					DiplomacyCard.UIpeace
				}), false);
				this.App.UI.SetVisible(base.UI.Path(new string[]
				{
					base.ID,
					DiplomacyCard.UIwar
				}), true);
			}
			else
			{
				this.App.UI.SetVisible(base.UI.Path(new string[]
				{
					base.ID,
					DiplomacyCard.UIpeace
				}), true);
				this.App.UI.SetVisible(base.UI.Path(new string[]
				{
					base.ID,
					DiplomacyCard.UIwar
				}), false);
			}
			DiplomacyInfo diplomacyInfo = this.App.GameDatabase.GetDiplomacyInfo(this.Player.ID, this.App.LocalPlayer.ID);
			this.App.UI.SetVisible(base.UI.Path(new string[]
			{
				base.ID,
				DiplomacyCard.UImoodHatred
			}), false);
			this.App.UI.SetVisible(base.UI.Path(new string[]
			{
				base.ID,
				DiplomacyCard.UImoodHostile
			}), false);
			this.App.UI.SetVisible(base.UI.Path(new string[]
			{
				base.ID,
				DiplomacyCard.UImoodDistrust
			}), false);
			this.App.UI.SetVisible(base.UI.Path(new string[]
			{
				base.ID,
				DiplomacyCard.UImoodIndifferent
			}), false);
			this.App.UI.SetVisible(base.UI.Path(new string[]
			{
				base.ID,
				DiplomacyCard.UImoodTrust
			}), false);
			this.App.UI.SetVisible(base.UI.Path(new string[]
			{
				base.ID,
				DiplomacyCard.UImoodFriend
			}), false);
			this.App.UI.SetVisible(base.UI.Path(new string[]
			{
				base.ID,
				DiplomacyCard.UImoodLove
			}), false);
			switch (diplomacyInfo.GetDiplomaticMood())
			{
			case DiplomaticMood.Hatred:
				this.App.UI.SetVisible(base.UI.Path(new string[]
				{
					base.ID,
					DiplomacyCard.UImoodHatred
				}), true);
				return;
			case DiplomaticMood.Hostility:
				this.App.UI.SetVisible(base.UI.Path(new string[]
				{
					base.ID,
					DiplomacyCard.UImoodHostile
				}), true);
				return;
			case DiplomaticMood.Distrust:
				this.App.UI.SetVisible(base.UI.Path(new string[]
				{
					base.ID,
					DiplomacyCard.UImoodDistrust
				}), true);
				return;
			case DiplomaticMood.Indifference:
				this.App.UI.SetVisible(base.UI.Path(new string[]
				{
					base.ID,
					DiplomacyCard.UImoodIndifferent
				}), true);
				return;
			case DiplomaticMood.Trust:
				this.App.UI.SetVisible(base.UI.Path(new string[]
				{
					base.ID,
					DiplomacyCard.UImoodTrust
				}), true);
				return;
			case DiplomaticMood.Friendship:
				this.App.UI.SetVisible(base.UI.Path(new string[]
				{
					base.ID,
					DiplomacyCard.UImoodFriend
				}), true);
				return;
			case DiplomaticMood.Love:
				this.App.UI.SetVisible(base.UI.Path(new string[]
				{
					base.ID,
					DiplomacyCard.UImoodLove
				}), true);
				return;
			default:
				return;
			}
		}
	}
}
