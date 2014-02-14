using Kerberos.Sots.Engine;
using System;
namespace Kerberos.Sots.GameObjects
{
	[GameObjectType(InteropGameObjectType.IGOT_DEFENSEMANAGER)]
	internal class DefenseManager : GameObject, IActive, IDisposable
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
		public DefenseManager(App game)
		{
			game.AddExistingObject(this, new object[0]);
		}
		public void Dispose()
		{
			if (base.App != null)
			{
				base.App.ReleaseObject(this);
			}
		}
	}
}
