using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.GameObjects
{
	internal class ShipFormation
	{
		private FormationDefinition m_formation;
		private float m_BackLineOffsetDist;
		private List<Ship> m_ships;
		private List<Ship> m_shipsOnBackLine;
		private List<FormationPatternData> m_formationPattern;
		private bool m_destSet;
		private Vector3 m_destination;
		private Vector3 m_facing;
		private bool m_destroyed;
		private bool m_requiresReset;
		private bool m_isVFormation;
		public float BackLineOffsetDist
		{
			get
			{
				return this.m_BackLineOffsetDist;
			}
			set
			{
				this.m_BackLineOffsetDist = value;
			}
		}
		public List<Ship> Ships
		{
			get
			{
				return this.m_ships;
			}
		}
		public List<Ship> ShipsOnBackLine
		{
			get
			{
				return this.m_shipsOnBackLine;
			}
		}
		public bool DestinationSet
		{
			get
			{
				return this.m_destSet;
			}
		}
		public Vector3 Destination
		{
			get
			{
				return this.m_destination;
			}
		}
		public bool HasReceivedAnUpdate
		{
			get
			{
				return this.m_formation != null && this.m_formation.HasReceivedAnUpdate;
			}
		}
		public Vector3 Facing
		{
			get
			{
				return this.m_facing;
			}
		}
		public ShipFormation(App game)
		{
			this.m_facing = -Vector3.UnitZ;
			this.m_destination = Vector3.Zero;
			this.m_ships = new List<Ship>();
			this.m_shipsOnBackLine = new List<Ship>();
			this.m_formationPattern = new List<FormationPatternData>();
			this.m_destSet = false;
			this.m_requiresReset = false;
			this.m_isVFormation = false;
			this.m_formation = game.AddObject<FormationDefinition>(new object[0]);
			this.m_formation.ParentShipFormation = this;
			this.m_destroyed = false;
			this.m_BackLineOffsetDist = 750f;
		}
		public void AddShip(Ship ship)
		{
			if (!this.m_ships.Contains(ship))
			{
				this.m_ships.Add(ship);
				this.m_requiresReset = true;
				this.m_destSet = false;
			}
		}
		public void RemoveShip(int shipID)
		{
			Ship ship = this.m_ships.FirstOrDefault((Ship x) => x.ObjectID == shipID);
			if (ship != null)
			{
				this.RemoveShip(ship);
			}
		}
		public void RemoveShip(Ship ship)
		{
			this.m_ships.Remove(ship);
			this.m_shipsOnBackLine.Remove(ship);
			foreach (FormationPatternData current in this.m_formationPattern)
			{
				if (current.Ship == ship)
				{
					this.m_formationPattern.Remove(current);
					this.m_requiresReset = true;
					this.m_destSet = false;
					break;
				}
			}
		}
		public void AddShipToBackLine(Ship ship)
		{
			if (!this.m_shipsOnBackLine.Contains(ship))
			{
				this.m_shipsOnBackLine.Add(ship);
				this.m_requiresReset = true;
				this.m_destSet = false;
			}
		}
		public static Vector3 GetCenterOfMass(List<Ship> ships)
		{
			Vector3 vector = Vector3.Zero;
			if (ships.Count > 0)
			{
				foreach (Ship current in ships)
				{
					vector += current.Maneuvering.Position;
				}
				vector /= (float)ships.Count;
			}
			return vector;
		}
		public void CreateFormationPattern(Vector3 pos, Vector3 facing, bool vFormation)
		{
			if (facing.LengthSquared <= 1E-05f || !facing.IsFinite() || this.m_ships.Count == 0)
			{
				return;
			}
			Vector3 centerOfMass = ShipFormation.GetCenterOfMass(this.m_ships);
			Matrix formationMat = Matrix.CreateWorld(centerOfMass, facing, Vector3.UnitY);
			if (vFormation)
			{
				this.m_formationPattern = FormationPatternCreator.CreateVFormationPattern(this.m_ships, formationMat);
			}
			else
			{
				this.m_formationPattern = FormationPatternCreator.CreateLineAbreastPattern(this.m_ships, formationMat);
			}
			float num = 0f;
			foreach (FormationPatternData current in this.m_formationPattern)
			{
				num = Math.Max(current.Position.Z, num);
			}
			centerOfMass = ShipFormation.GetCenterOfMass(this.m_shipsOnBackLine);
			formationMat = Matrix.CreateWorld(centerOfMass, facing, Vector3.UnitY);
			List<FormationPatternData> list = FormationPatternCreator.CreateLineAbreastPattern(this.m_shipsOnBackLine, formationMat);
			foreach (FormationPatternData current2 in list)
			{
				Vector3 position = current2.Position;
				position.Z += this.m_BackLineOffsetDist + num;
				current2.IsLead = (current2.IsLead && this.m_formationPattern.Count == 0);
				current2.Position = position;
			}
			this.m_formationPattern.AddRange(list);
		}
		public Vector3 GetCurrentPosition()
		{
			if (this.m_formation != null)
			{
				return this.m_formation.GetFormationPosition();
			}
			return Vector3.Zero;
		}
		public void SetDestination(App game, Vector3 pos, Vector3 facing, bool vFormation)
		{
			if (this.m_destroyed)
			{
				return;
			}
			bool flag = false;
			if (this.m_requiresReset || Vector3.Dot(facing, this.m_facing) < 0.25f || this.m_isVFormation != vFormation)
			{
				this.CreateFormationPattern(pos, facing, vFormation);
				this.m_requiresReset = false;
				this.m_isVFormation = vFormation;
				flag = true;
			}
			if (this.m_formationPattern.Count > 0)
			{
				if (flag)
				{
					List<object> list = new List<object>();
					list.Add(pos);
					list.Add(facing);
					list.Add((this.m_formation != null) ? this.m_formation.ObjectID : 0);
					list.Add(this.m_formationPattern.Count);
					foreach (FormationPatternData current in this.m_formationPattern)
					{
						list.Add((current.Ship != null) ? current.Ship.ObjectID : 0);
						list.Add(current.Position);
						list.Add(current.IsLead);
					}
					game.PostApplyFormationPattern(list.ToArray());
				}
				else
				{
					this.m_formation.PostAddGoal(pos, facing);
				}
				this.m_destination = pos;
				this.m_facing = facing;
				this.m_destSet = true;
			}
		}
		public void RemoveShip(App game, Ship ship)
		{
			if (!this.m_ships.Contains(ship))
			{
				return;
			}
			game.PostRemoveShipsFromFormation(new List<object>
			{
				1,
				ship.ObjectID
			}.ToArray());
			this.RemoveShip(ship);
		}
		public void ClearShips(App game)
		{
			if (this.m_ships.Count > 0)
			{
				List<object> list = new List<object>();
				list.Add(this.m_ships.Count);
				foreach (Ship current in this.m_ships)
				{
					list.Add(current.ObjectID);
				}
				game.PostRemoveShipsFromFormation(list.ToArray());
				List<Ship> list2 = new List<Ship>();
				list2.AddRange(this.m_ships);
				foreach (Ship current2 in list2)
				{
					this.RemoveShip(current2);
				}
			}
		}
		public void Destroy(App game)
		{
			if (this.m_formation != null)
			{
				this.m_formation.ParentShipFormation = null;
				game.ReleaseObject(this.m_formation);
			}
			this.m_formation = null;
		}
	}
}
