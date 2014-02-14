using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using System;
namespace Kerberos.Sots.StarMapElements
{
	[GameObjectType(InteropGameObjectType.IGOT_STARMAPSERVERNAME)]
	internal class StarMapServerName : StarMapObject
	{
		public StarMapServerName(App game, Vector3 origin, string label)
		{
			game.AddExistingObject(this, new object[0]);
			base.SetPosition(origin);
			this.PostSetProp("Label", label);
		}
	}
}
