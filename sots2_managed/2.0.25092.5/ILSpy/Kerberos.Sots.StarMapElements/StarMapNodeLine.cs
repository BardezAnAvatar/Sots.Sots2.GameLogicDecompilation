using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using System;
namespace Kerberos.Sots.StarMapElements
{
	[GameObjectType(InteropGameObjectType.IGOT_STARMAPNODELINE)]
	internal class StarMapNodeLine : StarMapObject
	{
		public StarMapNodeLine(App game, Vector3 from, Vector3 to)
		{
			game.AddExistingObject(this, new object[0]);
			this.PostSetProp("Points", new object[]
			{
				from,
				to
			});
		}
	}
}
