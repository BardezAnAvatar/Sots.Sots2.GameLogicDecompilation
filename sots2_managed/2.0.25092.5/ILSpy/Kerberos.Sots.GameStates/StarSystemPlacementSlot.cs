using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using System;
namespace Kerberos.Sots.GameStates
{
	[GameObjectType(InteropGameObjectType.IGOT_STARSYSTEMPLACEMENTSLOT)]
	internal class StarSystemPlacementSlot : GameObject, IDisposable, IActive
	{
		public const string StationSlotType = "station";
		public const string SystemDefenceBoatSlotType = "sdb";
		public const string AsteroidMineSlotType = "astmine";
		public SlotData _slotData = new SlotData();
		private bool _active;
		public Vector3 Position
		{
			get
			{
				return this._slotData.Position;
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
				if (this._active == value)
				{
					return;
				}
				this.PostSetActive(value);
				this._active = value;
			}
		}
		public StarSystemPlacementSlot(App game, SlotData slotData)
		{
			this._slotData = slotData;
			game.AddExistingObject(this, new object[]
			{
				this._slotData.OccupantID,
				this._slotData.Parent,
				this._slotData.ParentDBID,
				(int)this._slotData.SupportedTypes
			});
		}
		public void Dispose()
		{
			base.App.ReleaseObject(this);
		}
		public void SetOccupant(IGameObject value)
		{
			this._slotData.OccupantID = ((value != null) ? value.ObjectID : 0);
			this.PostSetProp("Occupant", this._slotData.OccupantID);
		}
		public int GetOccupantID()
		{
			return this._slotData.OccupantID;
		}
		public void SetTransform(Vector3 position, float rotation)
		{
			this._slotData.Position = position;
			this._slotData.Rotation = rotation;
			this.PostSetProp("Transform", new object[]
			{
				position,
				rotation
			});
		}
	}
}
