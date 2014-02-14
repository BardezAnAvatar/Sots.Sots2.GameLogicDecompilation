using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.PlayerFramework;
using System;
using System.IO;
using System.Linq;
namespace Kerberos.Sots.Strategy
{
	internal class PlayerSetup
	{
		private string _avatar;
		public string Name
		{
			get;
			set;
		}
		public string Faction
		{
			get;
			set;
		}
		public int SubfactionIndex
		{
			get;
			set;
		}
		public AIDifficulty AIDifficulty
		{
			get;
			set;
		}
		public Vector3 ShipColor
		{
			get;
			set;
		}
		public bool AI
		{
			get;
			set;
		}
		public bool Fixed
		{
			get;
			set;
		}
		public bool Locked
		{
			get;
			set;
		}
		public bool localPlayer
		{
			get;
			set;
		}
		public int InitialColonies
		{
			get;
			set;
		}
		public int InitialTechs
		{
			get;
			set;
		}
		public int InitialTreasury
		{
			get;
			set;
		}
		public int Team
		{
			get;
			set;
		}
		public int databaseId
		{
			get
			{
				return this.slot + 1;
			}
		}
		public int slot
		{
			get;
			set;
		}
		public int? EmpireColor
		{
			get;
			set;
		}
		public string Avatar
		{
			get
			{
				return this._avatar;
			}
			set
			{
				this._avatar = value;
			}
		}
		public string Badge
		{
			get;
			set;
		}
		public string EmpireName
		{
			get;
			set;
		}
		public bool Ready
		{
			get;
			set;
		}
		public NPlayerStatus Status
		{
			get;
			set;
		}
		public string GetBadgeTextureAssetPath(AssetDatabase assetdb)
		{
			if (string.IsNullOrEmpty(this.Badge))
			{
				return string.Empty;
			}
			if (string.IsNullOrEmpty(this.Faction))
			{
				return string.Empty;
			}
			Faction faction = assetdb.GetFaction(this.Faction);
			if (faction == null)
			{
				return string.Empty;
			}
			string text = faction.BadgeTexturePaths.FirstOrDefault((string x) => Path.GetFileNameWithoutExtension(x).ToLowerInvariant() == this.Badge.ToLowerInvariant());
			if (text == null)
			{
				return string.Empty;
			}
			return Path.Combine("factions", faction.Name, "badges", Path.GetFileNameWithoutExtension(text) + ".tga");
		}
		public string GetAvatarTextureAssetPath(AssetDatabase assetdb)
		{
			if (string.IsNullOrEmpty(this.Avatar))
			{
				return string.Empty;
			}
			if (string.IsNullOrEmpty(this.Faction))
			{
				return string.Empty;
			}
			Faction faction = assetdb.GetFaction(this.Faction);
			if (faction == null)
			{
				return string.Empty;
			}
			string text = faction.AvatarTexturePaths.FirstOrDefault((string x) => Path.GetFileNameWithoutExtension(x).ToLowerInvariant() == this.Avatar.ToLowerInvariant());
			if (text == null)
			{
				return string.Empty;
			}
			return Path.Combine("factions", faction.Name, "avatars", Path.GetFileNameWithoutExtension(text) + ".tga");
		}
		public PlayerSetup()
		{
			this.Name = string.Empty;
			this.EmpireName = string.Empty;
			this.ShipColor = Vector3.One;
			this.Avatar = null;
			this.Badge = null;
			this.AI = false;
			this.AIDifficulty = AIDifficulty.Normal;
			this.InitialTreasury = 500000;
			this.InitialColonies = 1;
			this.InitialTechs = 0;
			this.localPlayer = false;
			this.Ready = false;
			this.Locked = false;
			this.slot = 0;
			this.Team = 0;
		}
		public void FinalizeSetup(App game, AvailablePlayerFeatures availableFeatures)
		{
			if (string.IsNullOrEmpty(this.Faction))
			{
				this.Faction = App.GetSafeRandom().Choose(availableFeatures.Factions.Keys).Name;
			}
			Faction faction = game.AssetDatabase.GetFaction(this.Faction);
			if (string.IsNullOrEmpty(this.Name))
			{
				this.Name = "Player";
			}
			if (string.IsNullOrEmpty(this.EmpireName))
			{
				this.EmpireName = AssetDatabase.CommonStrings.Localize(faction.EmpireNames.GetNextStringID());
			}
			if (string.IsNullOrEmpty(this.Avatar))
			{
				this.Avatar = Path.GetFileNameWithoutExtension(availableFeatures.Factions[faction].Avatars.TakeRandom());
			}
			if (string.IsNullOrEmpty(this.Badge))
			{
				this.Badge = Path.GetFileNameWithoutExtension(availableFeatures.Factions[faction].Badges.TakeRandom());
			}
			if (!this.EmpireColor.HasValue)
			{
				this.EmpireColor = new int?(availableFeatures.EmpireColors.TakeRandom());
			}
		}
	}
}
