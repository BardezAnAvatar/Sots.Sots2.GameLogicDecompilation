using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using System;
namespace Kerberos.Sots.StarMapElements
{
	[GameObjectType(InteropGameObjectType.IGOT_STARMAPTERRAIN)]
	internal class StarMapTerrain : StarMapObject
	{
		public StarMapTerrain(App game, Vector3 origin, string label)
		{
			game.AddExistingObject(this, new object[0]);
			base.SetPosition(origin);
			this.PostSetProp("Label", label);
		}
	}
}
