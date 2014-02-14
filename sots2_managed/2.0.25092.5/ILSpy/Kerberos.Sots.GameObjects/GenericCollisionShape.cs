using Kerberos.Sots.Engine;
using System;
namespace Kerberos.Sots.GameObjects
{
	[GameObjectType(InteropGameObjectType.IGOT_GENERICCOLLISIONSHAPE)]
	internal class GenericCollisionShape : GameObject
	{
		internal enum CollisionShapeType
		{
			SPHERE,
			CAPSULEX,
			CAPSULEY,
			CAPSULEZ,
			BOX
		}
	}
}
