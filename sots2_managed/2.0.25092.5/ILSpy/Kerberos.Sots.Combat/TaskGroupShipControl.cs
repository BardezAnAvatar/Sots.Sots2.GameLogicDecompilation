using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Combat
{
	internal class TaskGroupShipControl
	{
		protected App m_Game;
		protected CombatAI m_CommanderAI;
		private float m_SensorRange;
		protected bool m_CanMerge;
		protected Ship m_GroupPriorityTarget;
		protected List<Ship> m_Ships;
		public ShipFormation m_Formation;
		protected ShipControlType m_Type;
		public TacticalObjective m_TaskGroupObjective;
		public float SensorRange
		{
			get
			{
				return this.m_SensorRange;
			}
		}
		public bool CanMerge
		{
			get
			{
				return this.m_CanMerge;
			}
		}
		public Ship GroupPriorityTarget
		{
			get
			{
				return this.m_GroupPriorityTarget;
			}
		}
		public ShipControlType Type
		{
			get
			{
				return this.m_Type;
			}
		}
		public TaskGroupShipControl(App game, TacticalObjective to, CombatAI commanderAI)
		{
			this.m_Game = game;
			this.m_CommanderAI = commanderAI;
			this.m_Type = ShipControlType.None;
			this.m_Ships = new List<Ship>();
			this.m_TaskGroupObjective = to;
			this.m_Formation = new ShipFormation(game);
			this.m_Formation.BackLineOffsetDist = 500f;
			this.m_SensorRange = game.AssetDatabase.DefaultTacSensorRange;
			this.m_CanMerge = true;
			this.m_GroupPriorityTarget = null;
		}
		public void ShutDown()
		{
			if (this.m_Formation != null)
			{
				this.m_Formation.Destroy(this.m_Game);
			}
			foreach (Ship current in this.m_Ships)
			{
				this.SetNewTarget(current, null);
			}
		}
		public void AddShip(Ship ship, bool backLine)
		{
			this.m_Ships.Add(ship);
			if (backLine)
			{
				this.m_Formation.AddShipToBackLine(ship);
			}
			else
			{
				this.m_Formation.AddShip(ship);
			}
			this.m_SensorRange = Math.Max(this.m_SensorRange, ship.SensorRange);
		}
		public void RemoveShip(Ship ship)
		{
			this.m_Ships.Remove(ship);
			if (this.m_Formation != null)
			{
				this.m_Formation.RemoveShip(this.m_Game, ship);
			}
			this.m_SensorRange = this.m_Game.AssetDatabase.DefaultTacSensorRange;
			foreach (Ship current in this.m_Ships)
			{
				this.m_SensorRange = Math.Max(this.m_SensorRange, current.SensorRange);
			}
		}
		public void ClearPriorityTarget()
		{
			this.m_GroupPriorityTarget = null;
		}
		public void ShipRemoved(Ship ship)
		{
			this.m_Ships.Remove(ship);
			if (this.m_Formation != null)
			{
				this.m_Formation.RemoveShip(this.m_Game, ship);
			}
		}
		public bool ContainsShip(Ship ship)
		{
			return this.m_Ships.Contains(ship);
		}
		public int GetShipCount()
		{
			return this.m_Ships.Count;
		}
		public List<Ship> GetShips()
		{
			return this.m_Ships;
		}
		public Vector3 GetCurrentPosition()
		{
			return this.m_Formation.GetCurrentPosition();
		}
		public virtual void Update(int framesElapsed)
		{
		}
		public void SetFUP(Vector3 dest, Vector3 destFacing)
		{
			if (this.m_Formation != null && ((this.m_Formation.Destination - dest).LengthSquared > 10000f || Vector3.Dot(this.m_Formation.Facing, destFacing) < 0.75f || !this.m_Formation.DestinationSet))
			{
				Vector3 vector = dest;
				Vector3 facing = destFacing;
				if (!this.m_Formation.HasReceivedAnUpdate)
				{
					if (this.m_Ships.Count > 0)
					{
						vector = Vector3.Zero;
						foreach (Ship current in this.m_Ships)
						{
							vector += current.Position;
						}
						vector /= (float)this.m_Ships.Count;
						vector.Y = 0f;
						facing = -Vector3.Normalize(vector);
					}
				}
				else
				{
					vector = this.m_CommanderAI.GetSafeDestination(this.GetCurrentPosition(), dest);
				}
				this.m_Formation.SetDestination(this.m_Game, vector, facing, false);
			}
		}
		protected void FaceShipsToTarget()
		{
			foreach (Ship current in this.m_Formation.Ships)
			{
				if (current.Target != null)
				{
					this.FaceShipToTarget(current, current.Target);
				}
			}
		}
		public void SetNewTarget(Ship ship, IGameObject target)
		{
			if (ship.Target == target || ship.BlindFireActive || !this.m_CommanderAI.ShipCanChangeTarget(ship))
			{
				return;
			}
			bool flag = false;
			if (target is Ship)
			{
				Ship ship2 = target as Ship;
				if (ship2.CloakedState == CloakedState.Cloaking)
				{
					float cloakedDetectionPercent = this.m_CommanderAI.GetCloakedDetectionPercent(ship2);
					Vector3 v = default(Vector3);
					v.X = (this.m_CommanderAI.AIRandom.CoinToss(0.5) ? -1f : 1f) * this.m_CommanderAI.AIRandom.NextInclusive(0.0001f, 1f);
					v.Y = 0f;
					v.X = (this.m_CommanderAI.AIRandom.CoinToss(0.5) ? -1f : 1f) * this.m_CommanderAI.AIRandom.NextInclusive(0.0001f, 1f);
					v.Normalize();
					float s = this.m_CommanderAI.AIRandom.NextInclusive(0f, 200f * (1f - cloakedDetectionPercent));
					ship.SetBlindFireTarget(ship2.ShipSphere.center + ship2.Position + v * s, Vector3.Zero, ship2.ShipSphere.radius * 1.25f, 5f);
					flag = true;
				}
			}
			if (!flag)
			{
				int subTargetId = 0;
				if (target is Ship)
				{
					Ship ship3 = target as Ship;
					if (!ship3.MissionSection.IsAlive)
					{
						Section section = ship3.Sections.FirstOrDefault((Section x) => x.ShipSectionAsset.Type == ShipSectionType.Command);
						if (section != null)
						{
							subTargetId = section.ObjectID;
						}
					}
				}
				ship.SetShipTarget((target != null) ? target.ObjectID : 0, Vector3.Zero, false, subTargetId);
			}
		}
		public bool IsPlanetSeparatingTarget(Vector3 desiredPos, Vector3 targetsPos, float width)
		{
			if (this.m_CommanderAI == null)
			{
				return false;
			}
			Vector3 forward = targetsPos - desiredPos;
			forward.Y = 0f;
			float num = forward.Normalize();
			Matrix m = Matrix.CreateWorld(desiredPos, forward, Vector3.UnitY);
			Matrix mat = Matrix.Inverse(m);
			bool flag = false;
			foreach (StellarBody current in this.m_CommanderAI.PlanetsInSystem)
			{
				Vector3 vector = Vector3.Transform(current.Parameters.Position, mat);
				if (vector.Z + current.Parameters.Radius * 0.5f >= -num && vector.Z - current.Parameters.Radius * 0.5f <= 0f && Math.Abs(vector.X) < width + current.Parameters.Radius + 750f)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				foreach (StellarBody current2 in this.m_CommanderAI.PlanetsInSystem)
				{
					Vector3 vector2 = Vector3.Transform(current2.Parameters.Position, mat);
					if (vector2.Z + current2.Parameters.Radius * 0.5f >= -num && vector2.Z - current2.Parameters.Radius * 0.5f <= 0f && Math.Abs(vector2.X) < width + current2.Parameters.Radius + 7500f)
					{
						flag = true;
						break;
					}
				}
			}
			return flag;
		}
		protected void FaceShipToTarget(Ship ship, IGameObject target)
		{
			Vector3 v = Vector3.Zero;
			if (target is Ship)
			{
				Ship ship2 = target as Ship;
				v = ship2.Maneuvering.Position;
			}
			else
			{
				if (target is StellarBody)
				{
					StellarBody stellarBody = target as StellarBody;
					v = stellarBody.Parameters.Position;
				}
			}
			Vector3 look = v - ship.Maneuvering.Position;
			look.Y = 0f;
			look.Normalize();
			ship.Maneuvering.PostSetLook(look);
		}
		protected void FaceBroadsideRight(Ship ship, Ship target)
		{
			Vector3 look = target.Maneuvering.Position - ship.Maneuvering.Position;
			Vector3 vector = new Vector3(-look.Z, 0f, look.X);
			vector.Normalize();
			ship.Maneuvering.PostSetLook(look);
		}
		protected void FaceBroadSideLeft(Ship ship, Ship target)
		{
			Vector3 look = target.Maneuvering.Position - ship.Maneuvering.Position;
			Vector3 vector = new Vector3(look.Z, 0f, -look.X);
			vector.Normalize();
			ship.Maneuvering.PostSetLook(look);
		}
		protected void SetShipSpeed(Ship ship, ShipSpeedState sss)
		{
			if (ship != null)
			{
				ship.Maneuvering.SpeedState = sss;
			}
		}
	}
}
