using Kerberos.Sots.Framework;
using System;
using System.Globalization;
using System.Threading;
namespace Kerberos.Sots.Engine
{
	public static class ScriptHost
	{
		private static Log _log;
		private static bool _allowConsole;
		private static object _host;
		private static Thread _thread;
		private static bool _quit;
		private static readonly AutoResetEvent _kickUpdate;
		private static readonly ManualResetEvent _readyForNextUpdate;
		internal static Log Log
		{
			get
			{
				return ScriptHost._log;
			}
		}
		public static bool AllowConsole
		{
			get
			{
				return ScriptHost._allowConsole;
			}
		}
		public static IFileSystem FileSystem
		{
			get;
			private set;
		}
		public static IEngine Engine
		{
			get;
			private set;
		}
		public static string TwoLetterISOLanguageName
		{
			get;
			private set;
		}
		private static void ConditionThread(Thread value)
		{
			value.CurrentUICulture = value.CurrentCulture;
			value.CurrentCulture = CultureInfo.InvariantCulture;
		}
		internal static Thread CreateThread(ThreadStart start)
		{
			Thread thread = new Thread(start);
			ScriptHost.ConditionThread(thread);
			return thread;
		}
		internal static Thread CreateThread(ParameterizedThreadStart start)
		{
			Thread thread = new Thread(start);
			ScriptHost.ConditionThread(thread);
			return thread;
		}
		static ScriptHost()
		{
			ScriptHost._host = new object();
			ScriptHost._thread = ScriptHost.CreateThread(new ParameterizedThreadStart(ScriptHost.ThreadProc));
			ScriptHost._kickUpdate = new AutoResetEvent(false);
			ScriptHost._readyForNextUpdate = new ManualResetEvent(true);
			ScriptHost.ConditionThread(Thread.CurrentThread);
		}
		private static void ThreadProc(object scriptHostParams)
		{
			ScriptHostParams scriptHostParams2 = (ScriptHostParams)scriptHostParams;
			ScriptHost.FileSystem = scriptHostParams2.FileSystem;
			ScriptHost.Engine = scriptHostParams2.Engine;
			ScriptHost.TwoLetterISOLanguageName = scriptHostParams2.TwoLetterISOLanguageName;
			ScriptHost._thread.Priority = ThreadPriority.Normal;
			ScriptHost._thread.Name = "SotsScript";
			App app = new App(scriptHostParams2);
			bool flag = false;
			while (!flag)
			{
				lock (ScriptHost._host)
				{
					if (ScriptHost._quit)
					{
						flag = true;
					}
				}
				ScriptHost._readyForNextUpdate.Set();
				if (!flag)
				{
					ScriptHost._kickUpdate.WaitOne();
					ScriptHost._readyForNextUpdate.Reset();
					app.Update();
				}
			}
			app.Exiting();
		}
		public static bool Localize(string text, out string localized)
		{
			return AssetDatabase.CommonStrings.Localize(text, out localized);
		}
		public static void Load(ScriptHostParams p)
		{
			ScriptHost._log = new Log(p.LogHost);
			ScriptHost._allowConsole = p.AllowConsole;
			if (ScriptHost._thread.IsAlive)
			{
				throw new InvalidOperationException("Previous thread is still alive.");
			}
			ScriptHost._thread.Start(p);
		}
		public static void Update(bool waitForCompletion)
		{
			ScriptHost._readyForNextUpdate.WaitOne();
			ScriptHost._kickUpdate.Set();
			if (waitForCompletion)
			{
				ScriptHost._readyForNextUpdate.WaitOne();
			}
		}
		public static void Exit()
		{
			lock (ScriptHost._host)
			{
				ScriptHost._quit = true;
			}
			ScriptHost._readyForNextUpdate.WaitOne();
			ScriptHost._kickUpdate.Set();
		}
		public static bool Exited()
		{
			return !ScriptHost._thread.IsAlive;
		}
	}
}
