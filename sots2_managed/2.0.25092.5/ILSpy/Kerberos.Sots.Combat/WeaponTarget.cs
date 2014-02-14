using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using System;
namespace Kerberos.Sots.Combat
{
	internal class WeaponTarget
	{
		public IGameObject Target;
		public Vector3 Position;
		private WeaponTarget()
		{
			this.Target = null;
			this.Position = Vector3.Zero;
		}
	}
}
