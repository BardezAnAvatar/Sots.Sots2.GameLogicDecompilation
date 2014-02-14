using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using System;
namespace Kerberos.Sots.GameObjects
{
	[GameObjectType(InteropGameObjectType.IGOT_ORBITCAMERACONTROLLER)]
	internal class OrbitCameraController : GameObject, IActive, IDisposable
	{
		private bool _active;
		private bool _zoomEnabled = true;
		private int _targetID;
		private Vector3 _targetPosition = Vector3.Zero;
		private float _maxDistance;
		private float _minDistance;
		private float _desiredDistance;
		private float _desiredYaw;
		private float _minYaw;
		private float _maxYaw;
		private bool _yawEnabled = true;
		private float _desiredPitch;
		private bool _pitchEnabled;
		private float _minPitch = -90f;
		private float _maxPitch = 90f;
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
		public bool ZoomEnabled
		{
			get
			{
				return this._zoomEnabled;
			}
			set
			{
				if (this._zoomEnabled == value)
				{
					return;
				}
				this._zoomEnabled = value;
				this.PostSetProp("ZoomEnabled", value);
			}
		}
		public int TargetID
		{
			get
			{
				return this._targetID;
			}
			set
			{
				if (value == this._targetID)
				{
					return;
				}
				this._targetID = value;
				this.PostSetProp("TargetID", this._targetID);
			}
		}
		public Vector3 TargetPosition
		{
			get
			{
				return this._targetPosition;
			}
			set
			{
				if (value == this._targetPosition)
				{
					return;
				}
				this._targetPosition = value;
				this.PostSetProp("TargetPos", this._targetPosition);
			}
		}
		public float MaxDistance
		{
			get
			{
				return this._maxDistance;
			}
			set
			{
				if (value == this._maxDistance)
				{
					return;
				}
				this._maxDistance = value;
				this.PostSetProp("MaxDistance", this._maxDistance);
			}
		}
		public float MinDistance
		{
			get
			{
				return this._minDistance;
			}
			set
			{
				if (value == this._minDistance)
				{
					return;
				}
				this._minDistance = value;
				this.PostSetProp("MinDistance", this._minDistance);
			}
		}
		public float DesiredDistance
		{
			get
			{
				return this._desiredDistance;
			}
			set
			{
				this._desiredDistance = value;
				this.PostSetProp("DesiredDistance", this._desiredDistance);
			}
		}
		public float DesiredYaw
		{
			get
			{
				return this._desiredYaw;
			}
			set
			{
				if (value == this._desiredYaw)
				{
					return;
				}
				this._desiredYaw = value;
				this.PostSetProp("DesiredYaw", this._desiredYaw);
			}
		}
		public float MinYaw
		{
			get
			{
				return this._minYaw;
			}
			set
			{
				if (value == this._minYaw)
				{
					return;
				}
				this._minYaw = value;
				this.PostSetProp("MinYaw", this._minYaw);
			}
		}
		public float MaxYaw
		{
			get
			{
				return this._maxYaw;
			}
			set
			{
				if (value == this._maxYaw)
				{
					return;
				}
				this._maxYaw = value;
				this.PostSetProp("MaxYaw", this._maxYaw);
			}
		}
		public bool YawEnabled
		{
			get
			{
				return this._yawEnabled;
			}
			set
			{
				if (value == this._yawEnabled)
				{
					return;
				}
				this._yawEnabled = value;
				this.PostSetProp("YawEnabled", this._yawEnabled);
			}
		}
		public float DesiredPitch
		{
			get
			{
				return this._desiredPitch;
			}
			set
			{
				if (value == this._desiredPitch)
				{
					return;
				}
				this._desiredPitch = value;
				this.PostSetProp("DesiredPitch", this._desiredPitch);
			}
		}
		public bool PitchEnabled
		{
			get
			{
				return this._pitchEnabled;
			}
			set
			{
				if (value == this._pitchEnabled)
				{
					return;
				}
				this._pitchEnabled = value;
				this.PostSetProp("PitchEnabled", this._pitchEnabled);
			}
		}
		public float MinPitch
		{
			get
			{
				return this._minPitch;
			}
			set
			{
				if (value == this._minPitch)
				{
					return;
				}
				this._minPitch = value;
				this.PostSetProp("MinPitch", this._minPitch);
			}
		}
		public float MaxPitch
		{
			get
			{
				return this._maxPitch;
			}
			set
			{
				if (value == this._maxPitch)
				{
					return;
				}
				this._maxPitch = value;
				this.PostSetProp("MaxPitch", this._maxPitch);
			}
		}
		public void SnapToDesiredPosition()
		{
			this.PostSetProp("Snap", new object[0]);
		}
		public OrbitCameraController()
		{
			this._minDistance = 0.1f;
			this._maxDistance = 5000f;
			this._desiredDistance = 60f;
			this._pitchEnabled = true;
		}
		public OrbitCameraController(App game) : this()
		{
			game.AddExistingObject(this, new object[0]);
		}
		public void SetAttractMode(bool value)
		{
			this.PostSetProp("AttractMode", value);
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
