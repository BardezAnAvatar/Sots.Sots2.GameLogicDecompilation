using Kerberos.Sots.Data;
using Kerberos.Sots.Data.WeaponFramework;
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
	internal class SystemKillerCombatAIControl : CombatAIController
	{
		protected App m_Game;
		protected Ship m_SystemKiller;
		protected WeaponBank m_BeamBank;
		protected bool m_VictoryConditionsMet;
		protected bool m_SpaceBattle;
		protected List<StellarBody> m_Planets;
		protected List<MoonData> m_Moons;
		protected List<StarModel> m_Stars;
		protected IGameObject m_CurrentTarget;
		protected Vector3 m_TargetCenter;
		protected Vector3 m_TargetLook;
		protected int m_TrackUpdateRate;
		protected float m_PlanetOffsetDist;
		protected SystemKillerStates m_State;
		public override Ship GetShip()
		{
			return this.m_SystemKiller;
		}
		public override void SetTarget(IGameObject target)
		{
			this.m_CurrentTarget = target;
		}
		public override IGameObject GetTarget()
		{
			return this.m_CurrentTarget;
		}
		public SystemKillerCombatAIControl(App game, Ship ship)
		{
			this.m_Game = game;
			this.m_SystemKiller = ship;
		}
		public override void Initialize()
		{
			this.m_Planets = new List<StellarBody>();
			this.m_Moons = new List<MoonData>();
			this.m_Stars = new List<StarModel>();
			this.m_VictoryConditionsMet = false;
			this.m_SpaceBattle = false;
			this.m_TrackUpdateRate = 0;
			this.m_PlanetOffsetDist = 0f;
			this.m_BeamBank = this.m_SystemKiller.GetWeaponBankWithWeaponTrait(WeaponEnums.WeaponTraits.PlanetKilling);
			if (this.m_BeamBank != null)
			{
				this.m_BeamBank.PostSetProp("RequestFireStateChange", true);
				this.m_BeamBank.PostSetProp("DisableAllTurrets", true);
			}
		}
		public override void Terminate()
		{
		}
		public override void ObjectRemoved(IGameObject obj)
		{
			if (obj == this.m_CurrentTarget)
			{
				this.m_CurrentTarget = null;
			}
			if (this.m_BeamBank == obj)
			{
				this.m_BeamBank = null;
			}
			if (obj is StellarBody)
			{
				StellarBody stellarBody = obj as StellarBody;
				this.m_Planets.Remove(stellarBody);
				List<MoonData> list = new List<MoonData>();
				foreach (MoonData current in this.m_Moons)
				{
					if (current.Moon == stellarBody)
					{
						list.Add(current);
					}
					else
					{
						if (current.ParentID == stellarBody.Parameters.OrbitalID)
						{
							list.Add(current);
						}
					}
				}
				foreach (MoonData current2 in list)
				{
					this.m_Moons.Remove(current2);
				}
			}
			if (obj is StarModel)
			{
				this.m_Stars.Remove(obj as StarModel);
			}
		}
		public override void OnThink()
		{
			if (this.m_SystemKiller == null)
			{
				return;
			}
			switch (this.m_State)
			{
			case SystemKillerStates.SEEK:
				this.ThinkSeek();
				return;
			case SystemKillerStates.TRACK:
				this.ThinkTrack();
				return;
			case SystemKillerStates.FIREBEAM:
				this.ThinkFireBeam();
				return;
			case SystemKillerStates.FIRINGBEAM:
				this.ThinkFiringBeam();
				return;
			case SystemKillerStates.SPACEBATTLE:
				this.ThinkSpaceBattle();
				return;
			case SystemKillerStates.VICTORY:
				this.ThinkVictory();
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
			return this.m_VictoryConditionsMet;
		}
		public override bool RequestingNewTarget()
		{
			return this.m_State == SystemKillerStates.SEEK && this.m_Planets.Count == 0 && this.m_Stars.Count == 0 && !this.m_SpaceBattle;
		}
		public override void FindNewTarget(IEnumerable<IGameObject> objs)
		{
			this.m_SpaceBattle = true;
			List<StellarBody> list = new List<StellarBody>();
			List<MoonData> list2 = new List<MoonData>();
			List<StarModel> list3 = new List<StarModel>();
			foreach (IGameObject current in objs)
			{
				if (current is StellarBody)
				{
					StellarBody item = current as StellarBody;
					list.Add(item);
					this.m_SpaceBattle = false;
				}
				else
				{
					if (current is StarModel)
					{
						StarModel item2 = current as StarModel;
						list3.Add(item2);
						this.m_SpaceBattle = false;
					}
				}
			}
			foreach (StellarBody current2 in list)
			{
				IEnumerable<OrbitalObjectInfo> moons = this.m_Game.GameDatabase.GetMoons(current2.Parameters.OrbitalID);
				foreach (OrbitalObjectInfo ooi in moons)
				{
					MoonData moonData = new MoonData();
					moonData.ParentID = ooi.ParentID.Value;
					moonData.Moon = list.First((StellarBody x) => x.Parameters.OrbitalID == ooi.ID);
					list2.Add(moonData);
				}
			}
			foreach (MoonData current3 in list2)
			{
				list.Remove(current3.Moon);
			}
			this.m_Planets = list;
			this.m_Moons = list2;
			this.m_Stars = list3;
			if (this.m_SpaceBattle)
			{
				this.m_State = SystemKillerStates.SPACEBATTLE;
			}
		}
		public override bool NeedsAParent()
		{
			return false;
		}
		public override void FindParent(IEnumerable<CombatAIController> controllers)
		{
		}
		protected virtual void ThinkSeek()
		{
			if (this.m_CurrentTarget == null)
			{
				this.FindCurrentTarget(false);
				return;
			}
			this.m_State = SystemKillerStates.TRACK;
		}
		protected virtual void ThinkTrack()
		{
			if (this.m_CurrentTarget == null)
			{
				this.m_State = SystemKillerStates.SEEK;
				return;
			}
			this.m_TrackUpdateRate--;
			if (this.m_TrackUpdateRate <= 0)
			{
				this.m_TrackUpdateRate = 10;
				Vector3 vector = this.m_SystemKiller.Position - this.m_TargetCenter;
				vector.Normalize();
				Vector3 vector2 = this.m_TargetCenter + vector * this.m_PlanetOffsetDist;
				this.m_TargetLook = -vector;
				this.m_SystemKiller.Maneuvering.PostAddGoal(vector2, this.m_TargetLook);
				Matrix matrix = Matrix.CreateRotationYPR(this.m_SystemKiller.Rotation);
				if (this.m_SystemKiller.Target != null && (this.m_SystemKiller.Position - vector2).LengthSquared <= 9000f && Vector3.Dot(matrix.Forward, this.m_TargetLook) > 0.8f)
				{
					if (this.m_BeamBank != null)
					{
						this.m_BeamBank.PostSetProp("DisableAllTurrets", false);
					}
					this.m_State = SystemKillerStates.FIREBEAM;
				}
			}
		}
		private void ThinkFireBeam()
		{
			if (this.m_BeamBank == null || this.m_CurrentTarget == null)
			{
				this.m_State = SystemKillerStates.SEEK;
				return;
			}
			if (this.m_SystemKiller.ListenTurretFiring == Turret.FiringEnum.Firing)
			{
				this.m_State = SystemKillerStates.FIRINGBEAM;
			}
		}
		private void ThinkFiringBeam()
		{
			if (this.m_BeamBank == null || this.m_CurrentTarget == null)
			{
				this.m_State = SystemKillerStates.SEEK;
				return;
			}
			if (this.m_SystemKiller.ListenTurretFiring != Turret.FiringEnum.Firing)
			{
				if (this.m_SystemKiller.ListenTurretFiring == Turret.FiringEnum.Completed)
				{
					this.m_SystemKiller.PostSetProp("FullyHealShip", new object[0]);
					if (this.m_CurrentTarget is StellarBody)
					{
						this.m_Planets.Remove(this.m_CurrentTarget as StellarBody);
					}
					else
					{
						if (this.m_CurrentTarget is StarModel)
						{
							this.m_Stars.Remove(this.m_CurrentTarget as StarModel);
						}
					}
					this.m_CurrentTarget = null;
					this.m_VictoryConditionsMet = true;
				}
				this.m_State = SystemKillerStates.SEEK;
			}
		}
		private void ThinkSpaceBattle()
		{
			this.m_TargetCenter = Matrix.CreateRotationYPR(this.m_SystemKiller.Rotation).Forward * (Kerberos.Sots.GameStates.StarSystem.CombatZoneMapRadii.Last<float>() * 5700f);
			this.m_TargetLook = this.m_SystemKiller.Position - this.m_TargetCenter;
			this.m_TargetLook.Y = 0f;
			this.m_TargetLook.Normalize();
			Vector3 vector = this.m_TargetCenter + this.m_TargetLook * this.m_PlanetOffsetDist;
			this.m_SystemKiller.Maneuvering.PostAddGoal(vector, this.m_TargetLook);
			if ((this.m_SystemKiller.Position - vector).LengthSquared <= 90000f)
			{
				this.m_VictoryConditionsMet = true;
				this.m_State = SystemKillerStates.SEEK;
			}
		}
		private void ThinkVictory()
		{
		}
		protected void FindCurrentTarget(bool needColony = false)
		{
			if (this.m_SpaceBattle || this.m_SystemKiller == null)
			{
				return;
			}
			float num = 0f;
			float num2 = 5000f;
			Vector3 vector = default(Vector3);
			int orbitalObjectID = 0;
			foreach (StellarBody current in this.m_Planets)
			{
				if (!needColony || current.Population != 0.0)
				{
					Vector3 position = current.Parameters.Position;
					float lengthSquared = position.LengthSquared;
					if (lengthSquared > num)
					{
						num = lengthSquared;
						this.m_CurrentTarget = current;
						orbitalObjectID = current.Parameters.OrbitalID;
						vector = current.Parameters.Position;
						num2 = current.Parameters.Radius + 750f;
					}
				}
			}
			if (!needColony)
			{
				if (this.m_CurrentTarget != null)
				{
					num = 0f;
					IGameObject gameObject = null;
					foreach (MoonData current2 in 
						from x in this.m_Moons
						where x.ParentID == orbitalObjectID
						select x)
					{
						Vector3 position2 = current2.Moon.Parameters.Position;
						float lengthSquared2 = position2.LengthSquared;
						if (lengthSquared2 > num)
						{
							num = lengthSquared2;
							gameObject = current2.Moon;
							vector = current2.Moon.Parameters.Position;
							num2 = current2.Moon.Parameters.Radius + 750f;
						}
					}
					if (gameObject != null)
					{
						this.m_CurrentTarget = gameObject;
					}
				}
				if (this.m_CurrentTarget == null)
				{
					foreach (StarModel current3 in this.m_Stars)
					{
						float lengthSquared3 = current3.Position.LengthSquared;
						if (lengthSquared3 <= num)
						{
							num = lengthSquared3;
							this.m_CurrentTarget = current3;
							vector = current3.Position;
							num2 = current3.Radius + 7500f;
						}
					}
				}
			}
			if (this.m_CurrentTarget != null)
			{
				Vector3 v = this.m_SystemKiller.Position - vector;
				v.Normalize();
				this.m_PlanetOffsetDist = num2 + this.m_SystemKiller.ShipSphere.radius + 500f;
				this.m_TargetCenter = vector;
				this.m_TargetLook = -v;
			}
			else
			{
				if (!needColony)
				{
					this.m_VictoryConditionsMet = true;
				}
			}
			this.m_SystemKiller.Target = this.m_CurrentTarget;
			if (this.m_CurrentTarget != null && this.m_BeamBank != null)
			{
				this.m_BeamBank.PostSetProp("SetTarget", new object[]
				{
					this.m_CurrentTarget.ObjectID,
					Vector3.Zero,
					true
				});
			}
		}
	}
}
