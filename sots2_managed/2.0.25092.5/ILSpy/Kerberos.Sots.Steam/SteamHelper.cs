using System;
using System.Collections.Generic;
namespace Kerberos.Sots.Steam
{
	public class SteamHelper
	{
		private ulong _appID;
		private ISteam _steam;
		private readonly List<UserAchievementStoredData> _achievements = new List<UserAchievementStoredData>();
		private bool _initialized;
		public bool Initialized
		{
			get
			{
				return this._initialized;
			}
		}
		private void GlobalStatsReceivedEventHandler(GlobalStatsReceivedData data)
		{
			App.Log.Warn("Global stats received.", "steam");
		}
		private void UserStatsReceivedEventHandler(UserStatsReceivedData data)
		{
			App.Log.Warn("Received user stats data.", "steam");
			if (this._steam.IsAvailable)
			{
				this._steam.SetStat("UNIQUE_PROFILES", 1);
			}
			int globalStat = this._steam.GetGlobalStat("UNIQUE_PROFILES");
			App.Log.Warn("Steam profile status: " + globalStat.ToString(), "steam");
		}
		private void UserStatsStoredEventHandler(UserStatsStoredData data)
		{
			if (data.GameID == this._appID && data.Result == EResult.k_EResultOK)
			{
				App.Log.Warn("Received user stats stored.", "steam");
				this._initialized = true;
			}
		}
		private void UserAchievementStoredEventHandler(UserAchievementStoredData data)
		{
			if (data.GameID != this._appID && !this._achievements.Contains(data))
			{
				this._achievements.Add(data);
			}
		}
		public SteamHelper(ISteam steam)
		{
			this._initialized = steam.IsAvailable;
			this._steam = steam;
			this._steam.UserStatsStored += new UserStatsStoredEventHandler(this.UserStatsStoredEventHandler);
			this._steam.UserStatsReceived += new UserStatsReceivedEventHandler(this.UserStatsReceivedEventHandler);
			this._steam.UserAchievementStored += new UserAchievementStoredEventHandler(this.UserAchievementStoredEventHandler);
			this._steam.GlobalStatsReceived += new GlobalStatsReceivedEventHandler(this.GlobalStatsReceivedEventHandler);
			if (this._initialized)
			{
				this._appID = this._steam.GetGameID();
				this.RequestStats();
			}
		}
		public void RequestStats()
		{
			if (this._steam.IsAvailable && this._steam.BLoggedOn())
			{
				App.Log.Warn("Requesting steam stats.", "steam");
				this._steam.RequestCurrentStats();
				this._steam.RequestGlobalStats(60);
				return;
			}
			App.Log.Warn("Steam not available.", "steam");
		}
		public void DoAchievement(AchievementType cheevo)
		{
			if (this._initialized && !this.HasAchievement(cheevo))
			{
				this.AddAchievement(cheevo);
			}
		}
		private void AddAchievement(AchievementType cheevo)
		{
			try
			{
				this._steam.SetAchievement(cheevo.ToString());
			}
			catch (Exception)
			{
				App.Log.Warn("SetAchievement: " + cheevo.ToString() + " failed.", "steam");
			}
		}
		private bool HasAchievement(AchievementType cheevo)
		{
			bool result = false;
			try
			{
				result = this._steam.GetAchievement(cheevo.ToString());
			}
			catch (Exception)
			{
				App.Log.Warn("GetAchievement " + cheevo.ToString() + " failed.", "steam");
			}
			return result;
		}
	}
}
