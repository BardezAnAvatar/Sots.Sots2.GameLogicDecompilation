using System;
namespace Kerberos.Sots.Steam
{
	public struct UserAchievementStoredData
	{
		public ulong GameID;
		public bool IsGroupAchievement;
		public string AchievementName;
		public uint CurProgress;
		public uint MaxProgress;
	}
}
