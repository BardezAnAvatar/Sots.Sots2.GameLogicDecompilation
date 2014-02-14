using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.Combat
{
	internal class SwarmerQueenControl : SwarmerSpawnerControl
	{
		private Ship m_SwarmerHive;
		private Vector3 m_SpawnPos;
		private float m_SpawnRadius;
		private bool m_HasHadHive;
		private int m_AttemptsToFindHive;
		private int m_TrackRate;
		private int m_ResetIdleDirDelay;
		private int m_ResetTargetRate;
		private int m_ResetHoldPos;
		private int m_CurrMaxResetHoldPos;
		private int m_HoldPosDuration;
		private float m_RotDir;
		private float m_HoldDist;
		private IGameObject m_Target;
		public override void SetTarget(IGameObject target)
		{
			this.m_SwarmerSpawner.SetShipTarget((target != null) ? target.ObjectID : 0, Vector3.Zero, true, 0);
			this.m_Target = target;
		}
		public override IGameObject GetTarget()
		{
			return this.m_Target;
		}
		public SwarmerQueenControl(App game, Ship ship, int systemId) : base(game, ship, systemId)
		{
		}
		public override void Initialize()
		{
			base.Initialize();
			this.m_Target = null;
			this.m_TrackRate = 0;
			this.m_SwarmerHive = null;
			this.m_HasHadHive = false;
			this.m_AttemptsToFindHive = 60;
			this.m_ResetIdleDirDelay = 0;
			this.m_ResetTargetRate = 0;
			this.m_ResetHoldPos = 0;
			this.m_HoldPosDuration = 0;
			this.m_CurrMaxResetHoldPos = 0;
			this.m_RotDir = -1f;
			this.m_HoldDist = 0f;
			this.m_SpawnPos = this.m_SwarmerSpawner.Position;
			this.m_SpawnPos.Y = 0f;
			this.m_SpawnRadius = this.m_SpawnPos.Length;
		}
		public override void OnThink()
		{
			if (this.m_SwarmerSpawner == null)
			{
				return;
			}
			base.UpdateTargetList();
			if (this.m_TargetList.Count > 0)
			{
				base.MaintainMaxSwarmers();
			}
			switch (this.m_State)
			{
			case SwarmerSpawnerStates.IDLE:
				this.ThinkIdle();
				return;
			case SwarmerSpawnerStates.EMITSWARMER:
				this.ThinkEmitAttackSwarmer();
				return;
			case SwarmerSpawnerStates.INTEGRATESWARMER:
				this.ThinkIntegrateAttackSwarmer();
				return;
			case SwarmerSpawnerStates.ADDINGSWARMERS:
				this.ThinkAddingSwarmers();
				return;
			case SwarmerSpawnerStates.LAUNCHSWARMER:
				this.ThinkLaunch();
				return;
			case SwarmerSpawnerStates.WAITFORLAUNCH:
				this.ThinkWaitForLaunch();
				return;
			case SwarmerSpawnerStates.SEEK:
				this.ThinkSeek();
				return;
			case SwarmerSpawnerStates.TRACK:
				this.ThinkTrack();
				return;
			default:
				return;
			}
		}
		protected override void ThinkIdle()
		{
			base.ThinkIdle();
			if (this.m_Target == null)
			{
				this.m_ResetIdleDirDelay--;
				if (this.m_ResetIdleDirDelay <= 0)
				{
					this.PickIdleDestination();
					this.m_ResetIdleDirDelay = 200;
				}
				this.FindQueenTarget();
			}
			if (this.m_Target != null)
			{
				this.m_State = SwarmerSpawnerStates.SEEK;
			}
		}
		protected void ThinkSeek()
		{
			if (this.m_Target != null)
			{
				this.m_State = SwarmerSpawnerStates.TRACK;
				return;
			}
			this.m_State = SwarmerSpawnerStates.IDLE;
		}
		protected void ThinkTrack()
		{
			this.m_ResetTargetRate--;
			if (this.m_Target == null || this.m_ResetTargetRate <= 0)
			{
				if (this.m_Target == null)
				{
					this.m_HoldPosDuration = 0;
					this.m_ResetHoldPos = 0;
				}
				this.SetTarget(null);
				this.m_State = SwarmerSpawnerStates.SEEK;
				return;
			}
			this.m_TrackRate--;
			if (this.m_TrackRate <= 0)
			{
				this.m_TrackRate = 3;
				Vector3 vector = Vector3.Zero;
				if (this.m_Target is Ship)
				{
					vector = (this.m_Target as Ship).Position;
				}
				float num = Math.Max(1500f, CombatAI.GetMinEffectiveWeaponRange(this.m_SwarmerSpawner, false));
				Vector3 vector2 = vector;
				vector2.Y = 0f;
				Vector3 value = vector2 - this.m_SwarmerSpawner.Position;
				value.Y = 0f;
				Vector3 vector3 = Vector3.Normalize(value);
				Vector3 vector4 = vector3;
				float num2 = num + 500f;
				float num3 = 0f;
				float lengthSquared = value.LengthSquared;
				if (lengthSquared < num2 * num2)
				{
					if (this.m_HoldPosDuration <= 0)
					{
						if (lengthSquared > num3 * num3)
						{
							this.m_ResetHoldPos -= this.m_TrackRate;
							if (this.m_ResetHoldPos <= 0)
							{
								Random random = new Random();
								this.m_CurrMaxResetHoldPos = random.NextInclusive(600, 800);
								this.m_ResetHoldPos = this.m_CurrMaxResetHoldPos;
								this.m_HoldPosDuration = 600;
								this.m_RotDir = (random.CoinToss(0.5) ? -1f : 1f);
								this.m_HoldDist = value.Length * 0.9f;
							}
						}
						Matrix lhs = Matrix.CreateRotationY(this.m_RotDir * MathHelper.DegreesToRadians(30f));
						vector4 = (lhs * Matrix.CreateWorld(vector2, vector4, Vector3.UnitY)).Forward;
					}
				}
				else
				{
					this.m_ResetHoldPos = this.m_CurrMaxResetHoldPos;
					this.m_HoldPosDuration = 0;
				}
				if (this.m_HoldPosDuration > 0)
				{
					this.m_HoldPosDuration -= this.m_TrackRate;
					num = this.m_HoldDist;
				}
				vector2 -= vector4 * num;
				this.m_SwarmerSpawner.Maneuvering.PostAddGoal(vector2, vector3);
			}
		}
		private void PickIdleDestination()
		{
			Random random = new Random();
			float degrees = random.NextInclusive(-10f, 10f);
			Matrix lhs = Matrix.CreateRotationY(MathHelper.DegreesToRadians(degrees));
			Matrix rhs = Matrix.CreateWorld(Vector3.Zero, Vector3.Normalize(this.m_SpawnPos), Vector3.UnitY);
			rhs = lhs * rhs;
			rhs.Position = rhs.Forward * (this.m_SpawnRadius + random.NextInclusive(-2500f, 2500f));
			this.m_SwarmerSpawner.Maneuvering.PostAddGoal(rhs.Position, -rhs.Forward);
		}
		private void FindQueenTarget()
		{
			if (this.m_TargetList.Count == 0 || this.m_SwarmerSpawner == null)
			{
				return;
			}
			Vector3 v = this.HiveIsPresent() ? this.m_SwarmerHive.Position : this.m_SwarmerSpawner.Position;
			IGameObject target = null;
			ShipClass shipClass = ShipClass.BattleRider;
			float num = 3.40282347E+38f;
			foreach (SwarmerTarget current in this.m_TargetList)
			{
				if (current.Target is Ship)
				{
					Ship ship = current.Target as Ship;
					ShipClass shipClass2 = ship.ShipClass;
					if (Ship.IsShipClassBigger(shipClass2, shipClass, true))
					{
						float lengthSquared = (ship.Position - v).LengthSquared;
						if (lengthSquared < num || shipClass != shipClass2)
						{
							num = lengthSquared;
							target = current.Target;
						}
						shipClass = shipClass2;
					}
				}
			}
			this.SetTarget(target);
			this.m_ResetTargetRate = 30;
		}
		public override bool NeedsAParent()
		{
			return !this.m_HasHadHive && this.m_AttemptsToFindHive > 0;
		}
		public override void FindParent(IEnumerable<CombatAIController> controllers)
		{
			foreach (CombatAIController current in controllers)
			{
				if (current is SwarmerHiveControl)
				{
					this.m_HasHadHive = true;
					this.m_SwarmerHive = current.GetShip();
					break;
				}
			}
			this.m_AttemptsToFindHive--;
		}
		private bool HiveIsPresent()
		{
			return this.m_SwarmerHive != null && !this.m_SwarmerHive.IsDestroyed;
		}
	}
}
