using Kerberos.Sots.Framework;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.GameStates
{
	internal class SpawnProfile
	{
		public int _fleetID;
		public int _playerID;
		public int _activeCommandShipID;
		public float _radius;
		public Vector3 _startingPosition;
		public Vector3 _spawnPosition;
		public Vector3 _spawnFacing;
		public Vector3 _retreatPosition;
		public List<int> _activeShips;
		public List<int> _reserveShips;
		public SpawnProfile()
		{
			this._activeShips = new List<int>();
			this._reserveShips = new List<int>();
			this._radius = 2000f;
		}
		public bool SpawnProfileOverlaps(SpawnProfile sp)
		{
			if (this._spawnPosition.Y != sp._spawnPosition.Y)
			{
				return false;
			}
			float num = this._radius + sp._radius;
			return (this._spawnPosition - sp._spawnPosition).LengthSquared < num * num;
		}
	}
}
