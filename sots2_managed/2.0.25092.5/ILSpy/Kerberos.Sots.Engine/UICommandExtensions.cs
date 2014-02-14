using System;
using System.Collections.Generic;
namespace Kerberos.Sots.Engine
{
	public static class UICommandExtensions
	{
		public static void Poll(this IEnumerable<UICommand> commands)
		{
			foreach (UICommand current in commands)
			{
				current.Poll();
			}
		}
	}
}
