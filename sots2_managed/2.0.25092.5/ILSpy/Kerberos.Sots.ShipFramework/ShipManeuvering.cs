using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using System;
namespace Kerberos.Sots.ShipFramework
{
	[GameObjectType(InteropGameObjectType.IGOT_SHIPMANEUVERING)]
	internal class ShipManeuvering : GameObject
	{
		private Vector3 _position;
		private Vector3 _rotation;
		private Vector3 _destination;
		private Vector3 _velocity;
		private Vector3 _retreatDestination;
		private RetreatData _retreatData;
		private ShipSpeedState _shipSpeedState;
		private TargetFacingAngle _targetFacingAngle;
		private float _maxShipSpeed;
		public ShipSpeedState SpeedState
		{
			get
			{
				return this._shipSpeedState;
			}
			set
			{
				if (value == this._shipSpeedState)
				{
					return;
				}
				this._shipSpeedState = value;
				this.PostSetProp("SetSpeedState", (int)value);
			}
		}
		public TargetFacingAngle TargetFacingAngle
		{
			get
			{
				return this._targetFacingAngle;
			}
			set
			{
				if (value == this._targetFacingAngle)
				{
					return;
				}
				this._targetFacingAngle = value;
				this.PostSetProp("TargetFacingAngle", (int)value);
			}
		}
		public Vector3 Position
		{
			get
			{
				return this._position;
			}
			set
			{
				this._position = value;
			}
		}
		public Vector3 Rotation
		{
			get
			{
				return this._rotation;
			}
			set
			{
				this._rotation = value;
			}
		}
		public Vector3 Velocity
		{
			get
			{
				return this._velocity;
			}
			set
			{
				this._velocity = value;
			}
		}
		public Vector3 Destination
		{
			get
			{
				return this._destination;
			}
			set
			{
				this._destination = value;
			}
		}
		public float MaxShipSpeed
		{
			get
			{
				return this._maxShipSpeed;
			}
			set
			{
				this._maxShipSpeed = value;
			}
		}
		public RetreatData RetreatData
		{
			get
			{
				return this._retreatData;
			}
			set
			{
				this._retreatData = value;
				this.RetreatDestination = value.DefaultDestination;
			}
		}
		public Vector3 RetreatDestination
		{
			get
			{
				return this._retreatDestination;
			}
			set
			{
				this._retreatDestination = value;
				this.PostSetProp("RetreatDest", value);
			}
		}
		public ShipManeuvering()
		{
			this._shipSpeedState = ShipSpeedState.Normal;
			this._retreatData = new RetreatData();
		}
		public override bool OnEngineMessage(InteropMessageID messageId, ScriptMessageReader message)
		{
			if (messageId != InteropMessageID.IMID_SCRIPT_OBJECT_SETPROP)
			{
				if (messageId == InteropMessageID.IMID_SCRIPT_MANEUVER_INFO)
				{
					Vector3 position = new Vector3(message.ReadSingle(), message.ReadSingle(), message.ReadSingle());
					this._position = position;
					Vector3 rotation = new Vector3(message.ReadSingle(), message.ReadSingle(), message.ReadSingle());
					this._rotation = rotation;
					Vector3 velocity = new Vector3(message.ReadSingle(), message.ReadSingle(), message.ReadSingle());
					this._velocity = velocity;
					Vector3 destination = new Vector3(message.ReadSingle(), message.ReadSingle(), message.ReadSingle());
					this._destination = destination;
					return true;
				}
				App.Log.Warn("Unhandled message (id=" + messageId + ").", "combat");
			}
			else
			{
				string a = message.ReadString();
				if (a == "Position")
				{
					Vector3 position2 = new Vector3(message.ReadSingle(), message.ReadSingle(), message.ReadSingle());
					this._position = position2;
					return true;
				}
				if (a == "ShipSpeedScale")
				{
					this._shipSpeedState = (ShipSpeedState)message.ReadInteger();
					return true;
				}
				if (a == "TargetFacingAngle")
				{
					this._targetFacingAngle = (TargetFacingAngle)message.ReadInteger();
					return true;
				}
			}
			return false;
		}
	}
}
