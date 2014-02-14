using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Combat
{
	internal class ColonyTrapDroneControl : CombatAIController
	{
		private const int kHelpDelay = 200;
		private App m_Game;
		private Ship m_Drone;
		private NPCFactionCombatAI m_Controller;
		private Ship m_Target;
		private DroneTrapState m_State;
		private StellarBody m_TrapPlanet;
		private Vector3 m_DefaultTestPlanetPos;
		private float m_PrevDistFromPlanet;
		private float m_AttackRange;
		private bool m_RequiresHelp;
		private bool m_WeaponsDisabled;
		private int m_RequestHelpDelay;
		private int m_PlanetID;
		public DroneTrapState State
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
		public static int MinNumDronesPerShip(ShipClass sc)
		{
			switch (sc)
			{
			case ShipClass.Cruiser:
				return 1;
			case ShipClass.Dreadnought:
				return 3;
			case ShipClass.Leviathan:
				return 9;
			case ShipClass.BattleRider:
			case ShipClass.Station:
				return 0;
			default:
				return 0;
			}
		}
		public static int TrapPriorityValue(Ship ship)
		{
			if (ship == null)
			{
				return 0;
			}
			if (ship.ShipRole == ShipRole.COLONIZER)
			{
				return 10;
			}
			if (ship.ShipRole == ShipRole.COMMAND)
			{
				return 5;
			}
			switch (ship.ShipClass)
			{
			case ShipClass.Cruiser:
				return 5;
			case ShipClass.Dreadnought:
				return 4;
			case ShipClass.Leviathan:
				return 3;
			}
			return 0;
		}
		public override Ship GetShip()
		{
			return this.m_Drone;
		}
		public override void SetTarget(IGameObject target)
		{
			if (target == null || target is Ship)
			{
				this.m_Drone.SetShipTarget((target != null) ? target.ObjectID : 0, Vector3.Zero, true, 0);
				this.m_Target = ((target != null) ? (target as Ship) : null);
			}
		}
		public override IGameObject GetTarget()
		{
			return this.m_Target;
		}
		public ColonyTrapDroneControl(App game, Ship ship, NPCFactionCombatAI controller, int fleetID)
		{
			this.m_Game = game;
			this.m_Drone = ship;
			this.m_Controller = controller;
			this.m_DefaultTestPlanetPos = ship.Position;
			ColonyTrapInfo colonyTrapInfoByFleetID = game.GameDatabase.GetColonyTrapInfoByFleetID(fleetID);
			this.m_PlanetID = ((colonyTrapInfoByFleetID != null) ? colonyTrapInfoByFleetID.PlanetID : 0);
		}
		public override void Initialize()
		{
			this.m_Target = null;
			this.m_TrapPlanet = null;
			this.m_RequiresHelp = false;
			this.m_State = DroneTrapState.SEEK;
			this.m_RequestHelpDelay = 200;
			this.m_PrevDistFromPlanet = 0f;
			this.m_AttackRange = 0f;
			foreach (WeaponBank current in this.m_Drone.WeaponBanks.ToList<WeaponBank>())
			{
				current.PostSetProp("RequestFireStateChange", true);
				current.PostSetProp("DisableAllTurrets", true);
				this.m_AttackRange = Math.Max(current.Weapon.RangeTable.Effective.Range, this.m_AttackRange);
				this.m_WeaponsDisabled = true;
			}
			this.m_AttackRange = Math.Min(this.m_AttackRange, 1000f);
		}
		public override void Terminate()
		{
		}
		public override void ObjectRemoved(IGameObject obj)
		{
			if (this.m_Drone == obj)
			{
				this.m_Drone = null;
			}
			if (this.m_Target == obj)
			{
				this.SetTarget(null);
			}
		}
		public override void OnThink()
		{
			if (this.m_Drone == null)
			{
				return;
			}
			switch (this.m_State)
			{
			case DroneTrapState.SEEK:
				this.ThinkSeek();
				return;
			case DroneTrapState.TRACK:
				this.ThinkTrack();
				return;
			case DroneTrapState.DRAG:
				this.ThinkDrag();
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
			return (this.m_Target == null && this.m_State == DroneTrapState.SEEK) || this.m_TrapPlanet == null;
		}
		public override void FindNewTarget(IEnumerable<IGameObject> objs)
		{
			float num = 3.40282347E+38f;
			float num2 = 3.40282347E+38f;
			Ship ship = null;
			int num3 = -1;
			foreach (IGameObject current in objs)
			{
				if (current is Ship)
				{
					Ship ship2 = current as Ship;
					if (ship2.Player != this.m_Drone.Player && Ship.IsActiveShip(ship2) && ship2.IsDetected(this.m_Drone.Player))
					{
						int num4 = ColonyTrapDroneControl.TrapPriorityValue(ship2);
						float lengthSquared = (ship2.Position - this.m_Drone.Position).LengthSquared;
						if ((num4 == num3 && lengthSquared < num) || num4 > num3)
						{
							ship = ship2;
							num = lengthSquared;
							num3 = num4;
						}
					}
				}
				else
				{
					if (current is StellarBody && (this.m_TrapPlanet == null || this.m_TrapPlanet.PlanetInfo.ID != this.m_PlanetID))
					{
						StellarBody stellarBody = current as StellarBody;
						float lengthSquared2 = (stellarBody.Parameters.Position - this.m_Drone.Position).LengthSquared;
						if (lengthSquared2 < num2 || stellarBody.PlanetInfo.ID == this.m_PlanetID)
						{
							num2 = lengthSquared2;
							this.m_TrapPlanet = stellarBody;
						}
					}
				}
			}
			if (ship != null)
			{
				this.SetTarget(ship);
			}
		}
		public override bool NeedsAParent()
		{
			return false;
		}
		public override void FindParent(IEnumerable<CombatAIController> controllers)
		{
		}
		public bool RequestingHelpWithTarget()
		{
			return this.m_Target != null && this.m_State == DroneTrapState.DRAG && this.m_RequiresHelp;
		}
		public void CheckIfHelpIsRequired()
		{
			if (this.m_Controller != null && !this.RequestingHelpWithTarget())
			{
				List<ColonyTrapDroneControl> list = this.m_Controller.CombatAIControllers.OfType<ColonyTrapDroneControl>().ToList<ColonyTrapDroneControl>();
				foreach (ColonyTrapDroneControl current in list)
				{
					if (current.RequestingHelpWithTarget())
					{
						break;
					}
				}
			}
		}
		private void ThinkSeek()
		{
			if (this.m_Target != null)
			{
				this.m_State = DroneTrapState.TRACK;
				return;
			}
			Vector3 vector = (this.m_TrapPlanet != null) ? this.m_TrapPlanet.Parameters.Position : this.m_DefaultTestPlanetPos;
			if ((this.m_Drone.Maneuvering.Destination - vector).LengthSquared > 100f)
			{
				this.m_Drone.Maneuvering.PostAddGoal(vector, -Vector3.UnitZ);
			}
		}
		private void ThinkTrack()
		{
			if (this.m_Target == null)
			{
				this.m_Drone.SetShipTarget(0, Vector3.Zero, true, 0);
				this.m_State = DroneTrapState.SEEK;
				return;
			}
			if (this.m_Drone.ListenTurretFiring == Turret.FiringEnum.Firing)
			{
				this.m_State = DroneTrapState.DRAG;
				return;
			}
			bool flag = (this.m_Drone.Position - this.m_Target.Position).LengthSquared > this.m_AttackRange * this.m_AttackRange;
			if (this.m_WeaponsDisabled != flag)
			{
				foreach (WeaponBank current in this.m_Drone.WeaponBanks.ToList<WeaponBank>())
				{
					current.PostSetProp("DisableAllTurrets", flag);
				}
				this.m_WeaponsDisabled = flag;
			}
			Vector3 defaultTestPlanetPos = this.m_DefaultTestPlanetPos;
			Vector3 vector = this.m_Target.Position + Vector3.Normalize(defaultTestPlanetPos - this.m_Target.Position) * (this.m_AttackRange * 0.75f);
			Vector3 look = Vector3.Normalize(this.m_Target.Position - this.m_Drone.Position);
			if ((this.m_Drone.Maneuvering.Destination - vector).LengthSquared > 2500f)
			{
				this.m_Drone.Maneuvering.PostAddGoal(vector, look);
			}
		}
		private void ThinkDrag()
		{
			if (this.m_Target == null || this.m_Drone.ListenTurretFiring != Turret.FiringEnum.Firing)
			{
				this.SetTarget(null);
				this.m_State = DroneTrapState.SEEK;
				foreach (WeaponBank current in this.m_Drone.WeaponBanks.ToList<WeaponBank>())
				{
					current.PostSetProp("DisableAllTurrets", true);
				}
				this.m_WeaponsDisabled = true;
				return;
			}
			Vector3 defaultTestPlanetPos = this.m_DefaultTestPlanetPos;
			Vector3 look = defaultTestPlanetPos - this.m_Target.Position;
			look.Normalize();
			Vector3 vector = defaultTestPlanetPos;
			if ((this.m_Drone.Maneuvering.Destination - vector).LengthSquared > 2500f)
			{
				this.m_Drone.Maneuvering.PostAddGoal(vector, look);
			}
			float lengthSquared = (this.m_Target.Position - vector).LengthSquared;
			if (lengthSquared > this.m_PrevDistFromPlanet)
			{
				this.m_RequestHelpDelay--;
			}
			else
			{
				this.m_RequestHelpDelay = 200;
			}
			this.m_RequiresHelp = (this.m_RequestHelpDelay <= 0);
			float num = ((this.m_TrapPlanet != null) ? this.m_TrapPlanet.Parameters.Radius : 500f) + this.m_Target.ShipSphere.radius * 0.5f;
			if ((this.m_Target.Position - defaultTestPlanetPos).LengthSquared < num * num)
			{
				this.m_Target.KillShip(true);
			}
		}
	}
}
