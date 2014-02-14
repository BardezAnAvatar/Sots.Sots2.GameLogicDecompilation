using Kerberos.Sots.Steam;
using System;
namespace Kerberos.Sots.Engine
{
	public struct ScriptHostParams
	{
		public string AssetDir;
		public string LogDir;
		public string InitialStateName;
		public string ConsoleScriptFileName;
		public IMessageQueue UIMessageQueue;
		public IMessageQueue ScriptMessageQueue;
		public IFileSystem FileSystem;
		public ILogHost LogHost;
		public IEngine Engine;
		public bool AllowConsole;
		public ISteam SteamAPI;
		public string TwoLetterISOLanguageName;
	}
}
