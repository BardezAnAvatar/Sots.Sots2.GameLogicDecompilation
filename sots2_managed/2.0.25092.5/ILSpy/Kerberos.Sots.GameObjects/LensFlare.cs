using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using System;
namespace Kerberos.Sots.GameObjects
{
	[GameObjectType(InteropGameObjectType.IGOT_LENSFLARE)]
	internal class LensFlare : GameObject, IPosition, IActive, IAttachable
	{
		private Vector3 _pos;
		private Vector3 _color = new Vector3(1f, 1f, 1f);
		private Vector2 _range = new Vector2(10f, 100f);
		private Vector2 _radius = new Vector2(0.2f, 0.2f);
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
		public Vector3 Color
		{
			get
			{
				return this._color;
			}
			set
			{
				if (value == this._color)
				{
					return;
				}
				this._color = value;
				this.PostSetProp("Color", this._color);
			}
		}
		public Vector2 Range
		{
			get
			{
				return this._range;
			}
			set
			{
				if (value == this._range)
				{
					return;
				}
				this._range = value;
				this.PostSetProp("Range", this._range);
			}
		}
		public Vector2 Radius
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
