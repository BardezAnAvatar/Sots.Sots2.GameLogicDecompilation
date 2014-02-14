using Kerberos.Sots.Engine;
using Kerberos.Sots.GameObjects;
using System;
namespace Kerberos.Sots.Ships
{
	[GameObjectType(InteropGameObjectType.IGOT_WRAITHABDUCTORSHIP)]
	internal class WraithAbductorShip : Ship
	{
		public WraithAbductorShip(App game, CreateShipParams createShipParams) : base(game, createShipParams)
		{
		}
	}
}
