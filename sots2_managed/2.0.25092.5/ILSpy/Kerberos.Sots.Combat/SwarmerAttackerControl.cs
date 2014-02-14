using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.Combat
{
	internal class SwarmerAttackerControl : CombatAIController
	{
		private App m_Game;
		private Ship m_Swarmer;
		private SwarmerSpawnerControl m_SwarmerParent;
		private IGameObject m_Target;
		private SwarmerAttackerStates m_State;
		private int m_UpdateRate;
		private SwarmerAttackerType m_Type;
		public SwarmerAttackerType Type
		{
			get
			{
				return this.m_Type;
			}
		}
		public SwarmerAttackerStates State
		{
			get
			{
				return this.m_State;
			}
			set
			{
				this.m_State = value;
			}
		}
		public static int NumSwarmersPerShip(ShipClass sc)
		{
			switch (sc)
			{
			case ShipClass.Cruiser:
				return 5;
			case ShipClass.Dreadnought:
				return 7;
			case ShipClass.Leviathan:
			case ShipClass.Station:
				return 10;
			case ShipClass.BattleRider:
				return 2;
			default:
				return 5;
			}
		}
		public static int NumGuardiansPerShip(ShipClass sc)
		{
			switch (sc)
			{
			case ShipClass.Cruiser:
				return 3;
			case ShipClass.Dreadnought:
				return 5;
			case ShipClass.Leviathan:
			case ShipClass.Station:
				return 7;
			case ShipClass.BattleRider:
				return 1;
			default:
				return 3;
			}
		}
		public override Ship GetShip()
		{
			return this.m_Swarmer;
		}
		public override void SetTarget(IGameObject target)
		{
			if (target != this.m_Target)
			{
				this.m_Target = target;
			}
		}
		public override IGameObject GetTarget()
		{
			return this.m_Target;
		}
		public SwarmerAttackerControl(App game, Ship ship, SwarmerAttackerType type)
		{
			this.m_Game = game;
			this.m_Swarmer = ship;
			this.m_Type = type;
		}
		public override void Initialize()
		{
			this.m_Target = null;
			this.m_SwarmerParent = null;
			this.m_State = SwarmerAttackerStates.SEEK;
			this.m_UpdateRate = 0;
			if (this.m_Swarmer != null)
			{
				foreach (WeaponBank current in this.m_Swarmer.WeaponBanks)
				{
					current.PostSetProp("IgnoreLineOfSight", true);
				}
			}
		}
		public override void Terminate()
		{
		}
		public override void ObjectRemoved(IGameObject obj)
		{
			if (this.m_SwarmerParent != null && this.m_SwarmerParent.GetShip() == obj)
			{
				this.m_SwarmerParent = null;
			}
			if (this.m_Target == obj)
			{
				this.m_Target = null;
			}
		}
		public override void OnThink()
		{
			if (this.m_Swarmer == null)
			{
				return;
			}
			switch (this.m_State)
			{
			case SwarmerAttackerStates.SEEK:
				this.ThinkSeek();
				return;
			case SwarmerAttackerStates.TRACK:
				this.ThinkTrack();
				return;
			default:
				return;
			}
		}
		public override void ForceFlee()
		{
		}
		public override bool VictoryConditionIsMet()
		{
			return false;
		}
		public override bool RequestingNewTarget()
		{
			return this.m_Swarmer != null && !this.m_Swarmer.DockedWithParent && this.m_Target == null && this.m_State == SwarmerAttackerStates.SEEK;
		}
		public override void FindNewTarget(IEnumerable<IGameObject> objs)
		{
			if (this.m_SwarmerParent == null)
			{
				float num = 3.40282347E+38f;
				this.m_Target = null;
				using (IEnumerator<IGameObject> enumerator = objs.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						IGameObject current = enumerator.Current;
						if (current is Ship)
						{
							Ship ship = current as Ship;
							if (ship.Player != this.m_Swarmer.Player && ship.Active && Ship.IsActiveShip(ship) && ship.IsDetected(this.m_Swarmer.Player))
							{
								float lengthSquared = (ship.Position - this.m_Swarmer.Position).LengthSquared;
								if (lengthSquared < num)
								{
									this.m_Target = ship;
									num = lengthSquared;
								}
							}
						}
					}
					return;
				}
			}
			this.m_SwarmerParent.RequestTargetFromParent(this);
		}
		public override bool NeedsAParent()
		{
			return this.m_SwarmerParent == null;
		}
		public override void FindParent(IEnumerable<CombatAIController> controllers)
		{
			foreach (CombatAIController current in controllers)
			{
				if (current is SwarmerSpawnerControl)
				{
					SwarmerSpawnerControl swarmerSpawnerControl = current as SwarmerSpawnerControl;
					if (swarmerSpawnerControl.IsThisMyParent(this.m_Swarmer))
					{
						swarmerSpawnerControl.AddChild(this);
						this.m_SwarmerParent = swarmerSpawnerControl;
						break;
					}
				}
			}
		}
		private void ThinkSeek()
		{
			if (this.m_Target != null)
			{
				if (this.m_Target is Ship)
				{
					this.m_Swarmer.SetShipTarget(this.m_Target.ObjectID, (this.m_Target as Ship).ShipSphere.center, true, 0);
				}
				else
				{
					this.m_Swarmer.SetShipTarget(this.m_Target.ObjectID, Vector3.Zero, true, 0);
				}
				this.m_State = SwarmerAttackerStates.TRACK;
				return;
			}
			if (this.m_SwarmerParent != null)
			{
				this.m_UpdateRate--;
				if (this.m_UpdateRate <= 0)
				{
					this.m_UpdateRate = 3;
					Vector3 vector = this.m_Swarmer.Position - this.m_SwarmerParent.GetShip().Position;
					vector.Normalize();
					Vector3 targetPos = this.m_SwarmerParent.GetShip().Position + vector * 1500f;
					this.m_Swarmer.Maneuvering.PostAddGoal(targetPos, vector);
				}
			}
		}
		private void ThinkTrack()
		{
			if (this.m_Target == null)
			{
				this.m_Swarmer.SetShipTarget(0, Vector3.Zero, true, 0);
				this.m_State = SwarmerAttackerStates.SEEK;
			}
		}
	}
}
