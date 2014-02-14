using System;
namespace Kerberos.Sots.Data
{
	internal class NamesFramework
	{
		public static NamesPool LoadFromXml(string filename)
		{
			return new NamesPool(filename);
		}
	}
}
