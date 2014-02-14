using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using System;
namespace Kerberos.Sots.GameObjects
{
	[GameObjectType(InteropGameObjectType.IGOT_STATICSPOTLIGHT)]
	internal class StaticSpotLight : GameObject, IPosition, IActive, IAttachable
	{
		private Vector3 _pos;
		private Vector3 _rot;
		private float _radius = 100f;
		private float _specularPower = 50f;
		private Vector3 _intensity = new Vector3(100f, 100f, 100f);
		private float _conePower = 10f;
		private float _coneScale = 10f;
		private float _coneFov = 90f;
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
		public float ConePower
		{
			get
			{
				return this._conePower;
			}
			set
			{
				if (value == this._conePower)
				{
					return;
				}
				this._conePower = value;
				this.PostSetProp("ConePower", this._conePower);
			}
		}
		public float ConeScale
		{
			get
			{
				return this._coneScale;
			}
			set
			{
				if (value == this._coneScale)
				{
					return;
				}
				this._coneScale = value;
				this.PostSetProp("ConeScale", this._coneScale);
			}
		}
		public float ConeFov
		{
			get
			{
				return this._coneFov;
			}
			set
			{
				if (value == this._coneFov)
				{
					return;
				}
				this._coneFov = value;
				this.PostSetProp("ConeFov", this._coneFov);
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
