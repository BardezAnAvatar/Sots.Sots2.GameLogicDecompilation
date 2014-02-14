using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using System;
namespace Kerberos.Sots.GameObjects
{
	[GameObjectType(InteropGameObjectType.IGOT_STATICOMNILIGHT)]
	internal class StaticOmniLight : GameObject, IPosition, IActive, IAttachable
	{
		private Vector3 _pos;
		private float _radius = 100f;
		private float _specularPower = 50f;
		private Vector3 _intensity = new Vector3(100f, 100f, 100f);
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
		public Vector3 Intensity
		{
			get
			{
				return this._intensity;
			}
			set
			{
				if (value.X == this._intensity.X && value.Y == this._intensity.Y && value.Z == this._intensity.Z)
				{
					return;
				}
				this._intensity = value;
				this.PostSetProp("Intensity", this._intensity);
			}
		}
		public float SpecularPower
		{
			get
			{
				return this._specularPower;
			}
			set
			{
				if (value == this._specularPower)
				{
					return;
				}
				this._specularPower = value;
				this.PostSetProp("SpecularPower", this._specularPower);
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
