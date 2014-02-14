using System;
using System.Diagnostics;
namespace Kerberos.Sots.Framework
{
	internal static class ShellHelper
	{
		public static void ShellOpen(string pathname)
		{
			Process.Start(new ProcessStartInfo(pathname)
			{
				UseShellExecute = true,
				Verb = "open"
			});
		}
		public static void ShellExplore(string filename)
		{
			Process.Start("explorer.exe", "/select," + filename);
		}
	}
}
