using Kerberos.Sots.Engine;
using System;
namespace Kerberos.Sots.ShipFramework
{
	[GameObjectType(InteropGameObjectType.IGOT_MINEFIELD)]
	internal class MineField : GameObject, IActive
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
	}
}
