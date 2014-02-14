using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using System;
namespace Kerberos.Sots.GameStates
{
	[GameObjectType(InteropGameObjectType.IGOT_ASTEROIDBELT)]
	internal class AsteroidBelt : GameObject, IActive, IDisposable
	{
		private bool _active;
		public bool Active
		{
			get
			{
				return this._active;
			}
			set
			{
				if (value == this._active)
				{
					return;
				}
				this._active = value;
				this.PostSetActive(this._active);
			}
		}
		public AsteroidBelt(App g, int randomSeed, Vector3 center, float innerRadius, float outterRadius, float minHeight, float maxHeight, int numAsteroids)
		{
			g.AddExistingObject(this, new object[]
			{
				randomSeed,
				center,
				innerRadius,
				outterRadius,
				minHeight,
				maxHeight,
				numAsteroids
			});
		}
		public void Dispose()
		{
			base.App.ReleaseObject(this);
		}
	}
}
