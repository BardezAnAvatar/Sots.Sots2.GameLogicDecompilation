using System;
namespace Kerberos.Sots.Engine
{
	internal static class GameObjectExtensions
	{
		public static int GetObjectID(this IGameObject value)
		{
			if (value != null)
			{
				return value.ObjectID;
			}
			return 0;
		}
	}
}
