using Kerberos.Sots.Engine;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
namespace Kerberos.Sots.Steam
{
	public class SteamAPI : ISteam, IDisposable
	{
		private static ILogHost _logHost;
		private bool _available = false;
		private unsafe APICallbackDelegator* _cbdelegator = null;
		private bool _storeStats = false;
		internal static SteamAPI Instance;
		public static int BaseAppID = 42990;
		public static int EoFAppID = 43000;
		private UserStatsReceivedEventHandler <backing_store>UserStatsReceived;
		private UserStatsStoredEventHandler <backing_store>UserStatsStored;
		private UserAchievementStoredEventHandler <backing_store>UserAchievementStored;
		private GlobalStatsReceivedEventHandler <backing_store>GlobalStatsReceived;
		public virtual event GlobalStatsReceivedEventHandler GlobalStatsReceived
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				this.<backing_store>GlobalStatsReceived = (GlobalStatsReceivedEventHandler)Delegate.Combine(this.<backing_store>GlobalStatsReceived, value);
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				this.<backing_store>GlobalStatsReceived = (GlobalStatsReceivedEventHandler)Delegate.Remove(this.<backing_store>GlobalStatsReceived, value);
			}
		}
		public virtual event UserAchievementStoredEventHandler UserAchievementStored
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				this.<backing_store>UserAchievementStored = (UserAchievementStoredEventHandler)Delegate.Combine(this.<backing_store>UserAchievementStored, value);
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				this.<backing_store>UserAchievementStored = (UserAchievementStoredEventHandler)Delegate.Remove(this.<backing_store>UserAchievementStored, value);
			}
		}
		public virtual event UserStatsStoredEventHandler UserStatsStored
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				this.<backing_store>UserStatsStored = (UserStatsStoredEventHandler)Delegate.Combine(this.<backing_store>UserStatsStored, value);
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				this.<backing_store>UserStatsStored = (UserStatsStoredEventHandler)Delegate.Remove(this.<backing_store>UserStatsStored, value);
			}
		}
		public virtual event UserStatsReceivedEventHandler UserStatsReceived
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				this.<backing_store>UserStatsReceived = (UserStatsReceivedEventHandler)Delegate.Combine(this.<backing_store>UserStatsReceived, value);
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				this.<backing_store>UserStatsReceived = (UserStatsReceivedEventHandler)Delegate.Remove(this.<backing_store>UserStatsReceived, value);
			}
		}
		public virtual bool IsAvailable
		{
			[return: MarshalAs(UnmanagedType.U1)]
			get
			{
				return this._available;
			}
		}
		private unsafe sbyte* GetGameOverlayName(GameOverlay overlay)
		{
			switch (overlay)
			{
			case GameOverlay.Friends:
				return (sbyte*)(&<Module>.??_C@_07OAMEEFKI@Friends?$AA@);
			case GameOverlay.Community:
				return (sbyte*)(&<Module>.??_C@_09NNGLBCPE@Community?$AA@);
			case GameOverlay.Players:
				return (sbyte*)(&<Module>.??_C@_07OLBIDKLK@Players?$AA@);
			case GameOverlay.Settings:
				return (sbyte*)(&<Module>.??_C@_08EEOHOBEO@Settings?$AA@);
			case GameOverlay.OfficialGameGroup:
				return (sbyte*)(&<Module>.??_C@_0BC@OEKJAOPK@OfficialGameGroup?$AA@);
			case GameOverlay.Achievements:
				return (sbyte*)(&<Module>.??_C@_0N@MEFHKGOC@Achievements?$AA@);
			default:
				throw new ArgumentOutOfRangeException("overlay");
			}
		}
		internal unsafe void OnUserStatsReceived(UserStatsReceived_t* pCallback)
		{
			this.raise_UserStatsReceived(new UserStatsReceivedData
			{
				GameID = *(ulong*)pCallback,
				Result = *(EResult*)(pCallback + 8 / sizeof(UserStatsReceived_t)),
				SteamIDUser = *(ulong*)(pCallback + 12 / sizeof(UserStatsReceived_t))
			});
		}
		internal unsafe void OnUserStatsStored(UserStatsStored_t* pCallback)
		{
			this.raise_UserStatsStored(new UserStatsStoredData
			{
				GameID = *(ulong*)pCallback,
				Result = *(EResult*)(pCallback + 8 / sizeof(UserStatsStored_t))
			});
		}
		internal unsafe void OnUserAchievementStored(UserAchievementStored_t* pCallback)
		{
			this.raise_UserAchievementStored(new UserAchievementStoredData
			{
				GameID = *(ulong*)pCallback,
				IsGroupAchievement = *(bool*)(pCallback + 8 / sizeof(UserAchievementStored_t)),
				AchievementName = new string((sbyte*)(pCallback + 9 / sizeof(UserAchievementStored_t))),
				CurProgress = *(uint*)(pCallback + 140 / sizeof(UserAchievementStored_t)),
				MaxProgress = *(uint*)(pCallback + 144 / sizeof(UserAchievementStored_t))
			});
		}
		internal unsafe void OnGlobalStatsReceived(GlobalStatsReceived_t* pCallback)
		{
			this.raise_GlobalStatsReceived(new GlobalStatsReceivedData
			{
				GameID = *(ulong*)pCallback
			});
		}
		internal static void Log(string @string)
		{
			SteamAPI._logHost.LogMessage(LogLevel.Normal, LogSeverity.Warn, "steam", @string);
		}
		internal SteamAPI(ILogHost logHost)
		{
			SteamAPI._logHost = logHost;
			if (SteamAPI.Instance != null)
			{
				throw new InvalidOperationException("Steam API instance already exists.");
			}
			SteamAPI.Instance = this;
		}
		private void ~SteamAPI()
		{
			SteamAPI.Instance = null;
		}
		public virtual int GetBaseAppID()
		{
			return 42990;
		}
		public virtual int GetEoFAppID()
		{
			return 43000;
		}
		protected virtual void raise_UserStatsReceived(UserStatsReceivedData value0)
		{
			UserStatsReceivedEventHandler userStatsReceivedEventHandler = this.<backing_store>UserStatsReceived;
			if (userStatsReceivedEventHandler != null)
			{
				userStatsReceivedEventHandler(value0);
			}
		}
		protected virtual void raise_UserStatsStored(UserStatsStoredData value0)
		{
			UserStatsStoredEventHandler userStatsStoredEventHandler = this.<backing_store>UserStatsStored;
			if (userStatsStoredEventHandler != null)
			{
				userStatsStoredEventHandler(value0);
			}
		}
		protected virtual void raise_UserAchievementStored(UserAchievementStoredData value0)
		{
			UserAchievementStoredEventHandler userAchievementStoredEventHandler = this.<backing_store>UserAchievementStored;
			if (userAchievementStoredEventHandler != null)
			{
				userAchievementStoredEventHandler(value0);
			}
		}
		protected virtual void raise_GlobalStatsReceived(GlobalStatsReceivedData value0)
		{
			GlobalStatsReceivedEventHandler globalStatsReceivedEventHandler = this.<backing_store>GlobalStatsReceived;
			if (globalStatsReceivedEventHandler != null)
			{
				globalStatsReceivedEventHandler(value0);
			}
		}
		[return: MarshalAs(UnmanagedType.U1)]
		public unsafe virtual bool Initialize()
		{
			if (this._available)
			{
				throw new SteamAPIException("Steam API already initialized.");
			}
			if (<Module>.SteamAPI_IsSteamRunning() != null)
			{
				APICallbackDelegator* ptr = <Module>.@new(80u);
				APICallbackDelegator* cbdelegator;
				try
				{
					if (ptr != null)
					{
						cbdelegator = <Module>.Kerberos.Sots.Steam.APICallbackDelegator.{ctor}(ptr);
					}
					else
					{
						cbdelegator = 0;
					}
				}
				catch
				{
					<Module>.delete((void*)ptr);
					throw;
				}
				this._cbdelegator = cbdelegator;
				this._available = true;
			}
			return true;
		}
		public unsafe virtual void Shutdown()
		{
			<Module>.SteamAPI_Shutdown();
			if (this._available)
			{
				this._available = false;
				APICallbackDelegator* cbdelegator = this._cbdelegator;
				if (cbdelegator != null)
				{
					APICallbackDelegator* ptr = cbdelegator;
					<Module>.Kerberos.Sots.Steam.APICallbackDelegator.{dtor}(ptr);
					<Module>.delete((void*)ptr);
				}
				this._cbdelegator = null;
			}
		}
		[return: MarshalAs(UnmanagedType.U1)]
		public unsafe virtual bool GetAchievement(string name)
		{
			bool result = false;
			ISteamUserStats* ptr = <Module>.SteamUserStats();
			int num = *(int*)ptr + 24;
			if (!calli(System.Byte modopt(System.Runtime.CompilerServices.CompilerMarshalOverride) modopt(System.Runtime.CompilerServices.CallConvThiscall)(System.IntPtr,System.SByte modopt(System.Runtime.CompilerServices.IsSignUnspecifiedByte) modopt(System.Runtime.CompilerServices.IsConst)*,System.Boolean*), ptr, <Module>.Kerberos.Sots.Steam.MarshalHelpers.ToANSI(name), ref result, *num))
			{
				throw new SteamAPIException("GetAchievement failed.");
			}
			return result;
		}
		public unsafe virtual void SetAchievement(string name)
		{
			ISteamUserStats* ptr = <Module>.SteamUserStats();
			int num = *(int*)ptr + 28;
			if (!calli(System.Byte modopt(System.Runtime.CompilerServices.CompilerMarshalOverride) modopt(System.Runtime.CompilerServices.CallConvThiscall)(System.IntPtr,System.SByte modopt(System.Runtime.CompilerServices.IsSignUnspecifiedByte) modopt(System.Runtime.CompilerServices.IsConst)*), ptr, <Module>.Kerberos.Sots.Steam.MarshalHelpers.ToANSI(name), *num))
			{
				throw new SteamAPIException("SetAchievement failed.");
			}
			ISteamUserStats* expr_2C = <Module>.SteamUserStats();
			object arg_37_0 = calli(System.Byte modopt(System.Runtime.CompilerServices.CompilerMarshalOverride) modopt(System.Runtime.CompilerServices.CallConvThiscall)(System.IntPtr), expr_2C, *(*expr_2C + 40));
		}
		public unsafe virtual void ClearAchievement(string name)
		{
			ISteamUserStats* ptr = <Module>.SteamUserStats();
			int num = *(int*)ptr + 32;
			if (!calli(System.Byte modopt(System.Runtime.CompilerServices.CompilerMarshalOverride) modopt(System.Runtime.CompilerServices.CallConvThiscall)(System.IntPtr,System.SByte modopt(System.Runtime.CompilerServices.IsSignUnspecifiedByte) modopt(System.Runtime.CompilerServices.IsConst)*), ptr, <Module>.Kerberos.Sots.Steam.MarshalHelpers.ToANSI(name), *num))
			{
				throw new SteamAPIException("ClearAchievement failed.");
			}
			ISteamUserStats* expr_2C = <Module>.SteamUserStats();
			object arg_37_0 = calli(System.Byte modopt(System.Runtime.CompilerServices.CompilerMarshalOverride) modopt(System.Runtime.CompilerServices.CallConvThiscall)(System.IntPtr), expr_2C, *(*expr_2C + 40));
		}
		[return: MarshalAs(UnmanagedType.U1)]
		public unsafe virtual bool StoreStats()
		{
			ISteamUserStats* expr_05 = <Module>.SteamUserStats();
			return calli(System.Byte modopt(System.Runtime.CompilerServices.CompilerMarshalOverride) modopt(System.Runtime.CompilerServices.CallConvThiscall)(System.IntPtr), expr_05, *(*expr_05 + 40));
		}
		[return: MarshalAs(UnmanagedType.U1)]
		public unsafe virtual bool BLoggedOn()
		{
			int num;
			if (this._available)
			{
				ISteamUser* expr_0D = <Module>.SteamUser();
				if (calli(System.Byte modopt(System.Runtime.CompilerServices.CompilerMarshalOverride) modopt(System.Runtime.CompilerServices.CallConvThiscall)(System.IntPtr), expr_0D, *(*expr_0D + 4)))
				{
					num = 1;
					goto IL_1F;
				}
			}
			num = 0;
			IL_1F:
			return (byte)num != 0;
		}
		public unsafe virtual void ActivateGameOverlay(GameOverlay overlay)
		{
			ISteamFriends* ptr = <Module>.SteamFriends();
			sbyte* ptr2;
			switch (overlay)
			{
			case GameOverlay.Friends:
				ptr2 = (sbyte*)(&<Module>.??_C@_07OAMEEFKI@Friends?$AA@);
				break;
			case GameOverlay.Community:
				ptr2 = (sbyte*)(&<Module>.??_C@_09NNGLBCPE@Community?$AA@);
				break;
			case GameOverlay.Players:
				ptr2 = (sbyte*)(&<Module>.??_C@_07OLBIDKLK@Players?$AA@);
				break;
			case GameOverlay.Settings:
				ptr2 = (sbyte*)(&<Module>.??_C@_08EEOHOBEO@Settings?$AA@);
				break;
			case GameOverlay.OfficialGameGroup:
				ptr2 = (sbyte*)(&<Module>.??_C@_0BC@OEKJAOPK@OfficialGameGroup?$AA@);
				break;
			case GameOverlay.Achievements:
				ptr2 = (sbyte*)(&<Module>.??_C@_0N@MEFHKGOC@Achievements?$AA@);
				break;
			default:
				throw new ArgumentOutOfRangeException("overlay");
			}
			calli(System.Void modopt(System.Runtime.CompilerServices.CallConvThiscall)(System.IntPtr,System.SByte modopt(System.Runtime.CompilerServices.IsSignUnspecifiedByte) modopt(System.Runtime.CompilerServices.IsConst)*), ptr, ptr2, *(*(int*)ptr + 84));
		}
		public unsafe virtual void RunCallbacks()
		{
			if (this._storeStats)
			{
				ISteamUserStats* expr_0D = <Module>.SteamUserStats();
				int storeStats = (calli(System.Byte modopt(System.Runtime.CompilerServices.CompilerMarshalOverride) modopt(System.Runtime.CompilerServices.CallConvThiscall)(System.IntPtr), expr_0D, *(*expr_0D + 40)) == 0) ? 1 : 0;
				this._storeStats = (storeStats != 0);
			}
			<Module>.SteamAPI_RunCallbacks();
		}
		[return: MarshalAs(UnmanagedType.U1)]
		public unsafe virtual bool RequestCurrentStats()
		{
			ISteamUserStats* expr_05 = <Module>.SteamUserStats();
			return calli(System.Byte modopt(System.Runtime.CompilerServices.CompilerMarshalOverride) modopt(System.Runtime.CompilerServices.CallConvThiscall)(System.IntPtr), expr_05, *(*expr_05));
		}
		[return: MarshalAs(UnmanagedType.U1)]
		public unsafe virtual bool RequestGlobalStats(int days)
		{
			ISteamUserStats* ptr = <Module>.SteamUserStats();
			byte result;
			if (calli(System.UInt64 modopt(System.Runtime.CompilerServices.CallConvThiscall)(System.IntPtr,System.Int32), ptr, days, *(*(int*)ptr + 152)) != 0L)
			{
				result = 1;
			}
			else
			{
				result = 0;
			}
			return result != 0;
		}
		public unsafe virtual ulong GetGameID()
		{
			ISteamUtils* ptr = <Module>.SteamUtils();
			CGameID cGameID = 0L;
			ISteamUtils* expr_11 = ptr;
			cGameID = (calli(System.UInt32 modopt(System.Runtime.CompilerServices.CallConvThiscall)(System.IntPtr), expr_11, *(*(int*)expr_11 + 36)) & 16777215);
			return cGameID;
		}
		[return: MarshalAs(UnmanagedType.U1)]
		public unsafe virtual bool HasDLC(int dlcID)
		{
			if (<Module>.SteamApps() == null)
			{
				return false;
			}
			ISteamApps* ptr = <Module>.SteamApps();
			return calli(System.Byte modopt(System.Runtime.CompilerServices.CompilerMarshalOverride) modopt(System.Runtime.CompilerServices.CallConvThiscall)(System.IntPtr,System.UInt32), ptr, dlcID, *(*(int*)ptr + 24));
		}
		public unsafe virtual string GetAchievementDisplayAttribute(string name, DisplayAttribute attribute)
		{
			sbyte* ptr;
			if (attribute != DisplayAttribute.Name)
			{
				if (attribute != DisplayAttribute.Description)
				{
					throw new ArgumentOutOfRangeException("attribute");
				}
				ptr = (sbyte*)(&<Module>.??_C@_04EBPADADD@desc?$AA@);
			}
			else
			{
				ptr = (sbyte*)(&<Module>.??_C@_04MEMAJGDJ@name?$AA@);
			}
			ISteamUserStats* ptr2 = <Module>.SteamUserStats();
			int num = *(int*)ptr2 + 48;
			return new string(calli(System.SByte modopt(System.Runtime.CompilerServices.IsSignUnspecifiedByte) modopt(System.Runtime.CompilerServices.IsConst)* modopt(System.Runtime.CompilerServices.CallConvThiscall)(System.IntPtr,System.SByte modopt(System.Runtime.CompilerServices.IsSignUnspecifiedByte) modopt(System.Runtime.CompilerServices.IsConst)*,System.SByte modopt(System.Runtime.CompilerServices.IsSignUnspecifiedByte) modopt(System.Runtime.CompilerServices.IsConst)*), ptr2, <Module>.Kerberos.Sots.Steam.MarshalHelpers.ToANSI(name), ptr, *num));
		}
		public unsafe virtual float GetStatSingle(string name)
		{
			ISteamUserStats* ptr = <Module>.SteamUserStats();
			int num = *(int*)ptr + 4;
			float result;
			if (!calli(System.Byte modopt(System.Runtime.CompilerServices.CompilerMarshalOverride) modopt(System.Runtime.CompilerServices.CallConvThiscall)(System.IntPtr,System.SByte modopt(System.Runtime.CompilerServices.IsSignUnspecifiedByte) modopt(System.Runtime.CompilerServices.IsConst)*,System.Single*), ptr, <Module>.Kerberos.Sots.Steam.MarshalHelpers.ToANSI(name), ref result, *num))
			{
				throw new SteamAPIException(string.Format("Steam API GetStat failed on: {0}", name));
			}
			return result;
		}
		public unsafe virtual int GetStat(string name)
		{
			ISteamUserStats* ptr = <Module>.SteamUserStats();
			int num = *(int*)ptr + 8;
			int result;
			if (!calli(System.Byte modopt(System.Runtime.CompilerServices.CompilerMarshalOverride) modopt(System.Runtime.CompilerServices.CallConvThiscall)(System.IntPtr,System.SByte modopt(System.Runtime.CompilerServices.IsSignUnspecifiedByte) modopt(System.Runtime.CompilerServices.IsConst)*,System.Int32*), ptr, <Module>.Kerberos.Sots.Steam.MarshalHelpers.ToANSI(name), ref result, *num))
			{
				throw new SteamAPIException(string.Format("GetStat failed on: {0}", name));
			}
			return result;
		}
		public unsafe virtual int GetGlobalStat(string name)
		{
			long num = 0L;
			ISteamUserStats* ptr = <Module>.SteamUserStats();
			int num2 = *(int*)ptr + 160;
			if (!calli(System.Byte modopt(System.Runtime.CompilerServices.CompilerMarshalOverride) modopt(System.Runtime.CompilerServices.CallConvThiscall)(System.IntPtr,System.SByte modopt(System.Runtime.CompilerServices.IsSignUnspecifiedByte) modopt(System.Runtime.CompilerServices.IsConst)*,System.Int64*), ptr, <Module>.Kerberos.Sots.Steam.MarshalHelpers.ToANSI(name), ref num, *num2))
			{
				string message = "Unable to retreive a global stat.";
				SteamAPI._logHost.LogMessage(LogLevel.Normal, LogSeverity.Warn, "steam", message);
			}
			return (int)num;
		}
		public unsafe virtual void SetStat(string name, int value)
		{
			ISteamUserStats* ptr = <Module>.SteamUserStats();
			int num = *(int*)ptr + 16;
			object arg_1B_0 = calli(System.Byte modopt(System.Runtime.CompilerServices.CompilerMarshalOverride) modopt(System.Runtime.CompilerServices.CallConvThiscall)(System.IntPtr,System.SByte modopt(System.Runtime.CompilerServices.IsSignUnspecifiedByte) modopt(System.Runtime.CompilerServices.IsConst)*,System.Int32), ptr, <Module>.Kerberos.Sots.Steam.MarshalHelpers.ToANSI(name), value, *num);
			ISteamUserStats* ptr2 = <Module>.SteamUserStats();
			int num2 = *(int*)ptr2 + 16;
			if (!calli(System.Byte modopt(System.Runtime.CompilerServices.CompilerMarshalOverride) modopt(System.Runtime.CompilerServices.CallConvThiscall)(System.IntPtr,System.SByte modopt(System.Runtime.CompilerServices.IsSignUnspecifiedByte) modopt(System.Runtime.CompilerServices.IsConst)*,System.Int32), ptr2, <Module>.Kerberos.Sots.Steam.MarshalHelpers.ToANSI(name), value, *num2))
			{
				throw new SteamAPIException(string.Format("SetStat failed on: {0} ({1})", name, value));
			}
			this._storeStats = true;
		}
		public unsafe virtual void SetStat(string name, float value)
		{
			ISteamUserStats* ptr = <Module>.SteamUserStats();
			int num = *(int*)ptr + 12;
			if (!calli(System.Byte modopt(System.Runtime.CompilerServices.CompilerMarshalOverride) modopt(System.Runtime.CompilerServices.CallConvThiscall)(System.IntPtr,System.SByte modopt(System.Runtime.CompilerServices.IsSignUnspecifiedByte) modopt(System.Runtime.CompilerServices.IsConst)*,System.Single), ptr, <Module>.Kerberos.Sots.Steam.MarshalHelpers.ToANSI(name), value, *num))
			{
				throw new SteamAPIException(string.Format("SetStat failed on: {0} ({1})", name, value));
			}
			this._storeStats = true;
		}
		public unsafe virtual void UpdateAvgRateStat(string name, float countThisSession, double sessionLength)
		{
			ISteamUserStats* ptr = <Module>.SteamUserStats();
			int num = *(int*)ptr + 20;
			if (!calli(System.Byte modopt(System.Runtime.CompilerServices.CompilerMarshalOverride) modopt(System.Runtime.CompilerServices.CallConvThiscall)(System.IntPtr,System.SByte modopt(System.Runtime.CompilerServices.IsSignUnspecifiedByte) modopt(System.Runtime.CompilerServices.IsConst)*,System.Single,System.Double), ptr, <Module>.Kerberos.Sots.Steam.MarshalHelpers.ToANSI(name), countThisSession, sessionLength, *num))
			{
				throw new SteamAPIException(string.Format("UpdateAvgRateStat failed on: {0} ({1},{2})", name, countThisSession, sessionLength));
			}
			this._storeStats = true;
		}
		public unsafe virtual void IndicateAchievementProgress(string name, uint currentProgress, uint nMaxProgress)
		{
			ISteamUserStats* ptr = <Module>.SteamUserStats();
			int num = *(int*)ptr + 52;
			if (!calli(System.Byte modopt(System.Runtime.CompilerServices.CompilerMarshalOverride) modopt(System.Runtime.CompilerServices.CallConvThiscall)(System.IntPtr,System.SByte modopt(System.Runtime.CompilerServices.IsSignUnspecifiedByte) modopt(System.Runtime.CompilerServices.IsConst)*,System.UInt32,System.UInt32), ptr, <Module>.Kerberos.Sots.Steam.MarshalHelpers.ToANSI(name), currentProgress, nMaxProgress, *num))
			{
				throw new SteamAPIException(string.Format("IndicateAchievementProgress failed on: {0} ({1},{2})", name, currentProgress, nMaxProgress));
			}
		}
		[return: MarshalAs(UnmanagedType.U1)]
		public unsafe virtual bool IsSubscribedApp()
		{
			ISteamUtils* ptr = <Module>.SteamUtils();
			ISteamApps* ptr2 = <Module>.SteamApps();
			int num = *(int*)ptr2 + 24;
			ISteamApps* arg_21_0 = ptr2;
			ISteamUtils* expr_14 = ptr;
			return calli(System.Byte modopt(System.Runtime.CompilerServices.CompilerMarshalOverride) modopt(System.Runtime.CompilerServices.CallConvThiscall)(System.IntPtr,System.UInt32), arg_21_0, calli(System.UInt32 modopt(System.Runtime.CompilerServices.CallConvThiscall)(System.IntPtr), expr_14, *(*(int*)expr_14 + 36)), *num) != 0;
		}
		protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool flag)
		{
			if (flag)
			{
				this.~SteamAPI();
			}
			else
			{
				base.Finalize();
			}
		}
		public sealed override void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
