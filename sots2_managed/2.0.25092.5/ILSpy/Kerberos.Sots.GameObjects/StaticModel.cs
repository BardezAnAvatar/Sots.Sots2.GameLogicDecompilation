using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using System;
namespace Kerberos.Sots.GameObjects
{
	[GameObjectType(InteropGameObjectType.IGOT_STATICMODEL)]
	internal class StaticModel : GameObject, IPosition, IOrientatable, IScalable, IActive, IAttachable
	{
		private Vector3 _pos;
		private Vector3 _rot;
		private float _scale = 1f;
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
				if (value.X == this._pos.X && value.Y == this._pos.Y && value.Z == this._pos.Z)
				{
					return;
				}
				this._pos = value;
				this.PostSetPosition(this._pos);
			}
		}
		public Vector3 Rotation
		{
			get
			{
				return this._rot;
			}
			set
			{
				if (value.X == this._rot.X && value.Y == this._rot.Y && value.Z == this._rot.Z)
				{
					return;
				}
				this._rot = value;
				this.PostSetRotation(this._rot);
			}
		}
		public float Scale
		{
			get
			{
				return this._scale;
			}
			set
			{
				if (value == this._scale)
				{
					return;
				}
				this._scale = value;
				this.PostSetScale(this._scale);
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
