using System;
namespace Kerberos.Sots.Engine
{
	internal static class FileSystemHelpers
	{
		public static string StripMountName(string path)
		{
			if (path.StartsWith("\\"))
			{
				int num = path.IndexOf("\\", 1);
				return path.Substring(num + 1);
			}
			return path;
		}
	}
}
