using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using System;
namespace Kerberos.Sots.StarMapElements
{
	[GameObjectType(InteropGameObjectType.IGOT_STARMAPPROP)]
	internal class StarMapProp : StarMapObject
	{
		public StarMapProp(App game, string modelName, Vector3 position, Vector3 eulerRotation, float scale)
		{
			game.AddExistingObject(this, new object[]
			{
				modelName
			});
			this.PostSetPosition(position);
			this.PostSetRotation(Vector3.RadiansToDegrees(eulerRotation));
			this.PostSetScale(scale);
		}
	}
}
