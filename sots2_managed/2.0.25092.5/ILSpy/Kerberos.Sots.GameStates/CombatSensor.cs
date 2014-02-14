using Kerberos.Sots.Engine;
using System;
namespace Kerberos.Sots.GameStates
{
	[GameObjectType(InteropGameObjectType.IGOT_COMBATSENSOR)]
	internal class CombatSensor : GameObject, IActive
	{
		private bool _active;
		private float _minDistFromCamera = 50000f;
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
		public float MinDistance
		{
			get
			{
				return this._minDistFromCamera;
			}
			set
			{
				if (value == this._minDistFromCamera)
				{
					return;
				}
				this._minDistFromCamera = value;
			}
		}
	}
}
