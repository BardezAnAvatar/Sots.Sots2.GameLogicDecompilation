using Kerberos.Sots.Engine;
using System;
namespace Kerberos.Sots.GameStates
{
	[GameObjectType(InteropGameObjectType.IGOT_COMBATGRID)]
	internal class CombatGrid : GameObject, IActive
	{
		private bool _active;
		private float _gridSize = 10f;
		private float _cellSize = 1f;
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
		public float GridSize
		{
			get
			{
				return this._gridSize;
			}
			set
			{
				if (value == this._gridSize)
				{
					return;
				}
				this._gridSize = value;
				this.PostSetProp("GridSize", this._gridSize);
			}
		}
		public float CellSize
		{
			get
			{
				return this._cellSize;
			}
			set
			{
				if (value == this._cellSize)
				{
					return;
				}
				this._cellSize = value;
				this.PostSetProp("CellSize", this._cellSize);
			}
		}
	}
}
