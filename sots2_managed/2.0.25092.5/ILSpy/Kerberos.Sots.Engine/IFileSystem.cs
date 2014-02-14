using System;
using System.IO;
namespace Kerberos.Sots.Engine
{
	public interface IFileSystem
	{
		Stream CreateStream(string path);
		void SplitBasePath(string path, out string mountName, out string suffix);
		bool IsBasePath(string path);
		bool IsRootedPath(string path);
		bool FileExists(string path);
		string[] FindFiles(string pattern);
		string[] FindDirectories(string pattern);
		bool TryResolveBaseFilePath(string path, out string result);
		bool TryResolveAbsoluteFilePath(string path, out string result);
	}
}
