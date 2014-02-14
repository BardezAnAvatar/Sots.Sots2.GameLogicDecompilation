using System;
namespace Kerberos.Sots.Steam
{
	public interface ISteam
	{
		event UserStatsReceivedEventHandler UserStatsReceived;
		event UserStatsStoredEventHandler UserStatsStored;
		event UserAchievementStoredEventHandler UserAchievementStored;
		event GlobalStatsReceivedEventHandler GlobalStatsReceived;
		bool IsAvailable
		{
			get;
		}
		bool Initialize();
		void Shutdown();
		bool GetAchievement(string name);
		void SetAchievement(string name);
		void ClearAchievement(string name);
		bool StoreStats();
		bool BLoggedOn();
		void RunCallbacks();
		bool RequestCurrentStats();
		bool RequestGlobalStats(int days);
		ulong GetGameID();
		bool HasDLC(int dlcID);
		void ActivateGameOverlay(GameOverlay overlay);
		string GetAchievementDisplayAttribute(string name, DisplayAttribute attribute);
		float GetStatSingle(string name);
		int GetStat(string name);
		int GetGlobalStat(string name);
		void SetStat(string name, float value);
		void SetStat(string name, int value);
		void UpdateAvgRateStat(string name, float countThisSession, double sessionLength);
		void IndicateAchievementProgress(string name, uint currentProgress, uint nMaxProgress);
		bool IsSubscribedApp();
		int GetBaseAppID();
		int GetEoFAppID();
	}
}
