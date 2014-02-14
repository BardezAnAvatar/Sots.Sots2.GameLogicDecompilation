using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using System;
namespace Kerberos.Sots.GameObjects
{
	[GameObjectType(InteropGameObjectType.IGOT_POINT_OF_INTEREST)]
	internal class PointOfInterest : GameObject, IActive, IPosition
	{
		public const string POIEffectFilePath = "effects\\ui\\Point_of_Interest.effect";
		private int _targetID;
		private float _radius;
		private bool _hasBeenSeen;
		private bool _active;
		private Vector3 _position;
		public int TargetID
		{
			get
			{
				return this._targetID;
			}
			set
			{
				this._targetID = value;
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
				this._radius = value;
			}
		}
		public bool HasBeenSeen
		{
			get
			{
				return this._hasBeenSeen;
			}
			set
			{
				if (value == this._hasBeenSeen)
				{
					return;
				}
				this._hasBeenSeen = value;
				this.PostSetProp("HasBeenSeen", value);
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
				bool flag = !this._hasBeenSeen && value;
				this._active = flag;
				this.PostSetActive(flag);
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
				Vector3 position = value;
				position.Y = 0f;
				this._position = position;
				this.PostSetProp("SetPosition", value);
			}
		}
		public override bool OnEngineMessage(InteropMessageID messageId, ScriptMessageReader message)
		{
			if (messageId == InteropMessageID.IMID_SCRIPT_OBJECT_SETPROP)
			{
				string a = message.ReadString();
				if (a == "Position")
				{
					Vector3 position = new Vector3(message.ReadSingle(), message.ReadSingle(), message.ReadSingle());
					this._position = position;
					return true;
				}
			}
			return false;
		}
	}
}
