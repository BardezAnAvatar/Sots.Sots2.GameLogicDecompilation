using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using System;
namespace Kerberos.Sots.GameStates
{
	[GameObjectType(InteropGameObjectType.IGOT_LARGEASTEROID)]
	internal class LargeAsteroid : GameObject, IActive, IDisposable
	{
		private bool _active;
		private Matrix _worldTransform;
		private int _id;
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
		public Matrix WorldTransform
		{
			get
			{
				return this._worldTransform;
			}
			set
			{
				this._worldTransform = value;
			}
		}
		public int ID
		{
			get
			{
				return this._id;
			}
			set
			{
				this._id = value;
			}
		}
		public LargeAsteroid(App g, Vector3 position)
		{
			g.AddExistingObject(this, new object[]
			{
				position
			});
		}
		public void Dispose()
		{
			base.App.ReleaseObject(this);
		}
	}
}
