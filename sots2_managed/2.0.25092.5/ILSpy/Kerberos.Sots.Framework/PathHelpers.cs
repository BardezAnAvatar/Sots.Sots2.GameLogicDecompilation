using System;
using System.IO;
namespace Kerberos.Sots.Framework
{
	public class PathHelpers
	{
		public static string Combine(params string[] parts)
		{
			string text = null;
			for (int i = 0; i < parts.Length; i++)
			{
				string text2 = parts[i];
				if (text == null)
				{
					text = text2;
				}
				else
				{
					text = Path.Combine(text, text2);
				}
			}
			return text;
		}
		public static string FixSeparators(string path)
		{
			return path.Replace('/', '\\');
		}
	}
}
