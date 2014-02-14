using Kerberos.Sots.Engine;
using Kerberos.Sots.PlayerFramework;
using System;
namespace Kerberos.Sots.StarMapElements
{
	[GameObjectType(InteropGameObjectType.IGOT_STARMAPPROVINCE)]
	internal class StarMapProvince : StarMapObject
	{
		public StarMapProvince(App game)
		{
			game.AddExistingObject(this, new object[0]);
		}
		public void SetLabel(string value)
		{
			this.PostSetProp("Label", value);
		}
		public void SetCapital(StarMapSystem value)
		{
			this.PostSetProp("Capital", value);
		}
		public void SetPlayer(Player value)
		{
			this.PostSetProp("Player", value);
		}
	}
}
