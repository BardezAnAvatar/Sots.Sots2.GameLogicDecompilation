using Kerberos.Sots.Engine;
using Kerberos.Sots.GameObjects;
using System;
namespace Kerberos.Sots.Ships
{
	[GameObjectType(InteropGameObjectType.IGOT_BATTLERIDERSHIP)]
	internal class BattleRiderShip : Ship
	{
		public BattleRiderShip(App game, CreateShipParams createShipParams) : base(game, createShipParams)
		{
		}
	}
}
