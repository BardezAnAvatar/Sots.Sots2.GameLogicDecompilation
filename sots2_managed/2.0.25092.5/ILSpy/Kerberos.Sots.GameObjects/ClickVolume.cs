using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using System;
namespace Kerberos.Sots.GameObjects
{
	[GameObjectType(InteropGameObjectType.IGOT_CLICKVOLUME)]
	internal class ClickVolume : GameObject, IPosition, IActive, IAttachable
	{
		private Vector3 _pos;
		private float _radius = 1f;
		private bool _active;
		private IGameObject _parent;
		public IGameObject Parent
		{
			get
			{
				return this._parent;
			}
			set
			{
				if (this._parent == value)
				{
					return;
				}
				this._parent = value;
				this.PostSetParent(value);
			}
		}
		public Vector3 Position
		{
			get
			{
				return this._pos;
			}
			set
			{
				if (value == this._pos)
				{
					return;
				}
				this._pos = value;
				this.PostSetPosition(this._pos);
			}
		}
		public float Radius
		{
			get
			{
				return this._radius;
			}
			set
			{
				if (value == this._radius)
				{
					return;
				}
				this._radius = value;
				this.PostSetProp("Radius", this._radius);
			}
		}
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
