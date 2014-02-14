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
	internal class VonNeumannChildControl : CombatAIController
	{
		private App m_Game;
		private Ship m_VonNeumannChild;
		private VonNeumannMomControl m_VonNeumannMom;
		private IGameObject m_Target;
		private VonNeumannChildStates m_State;
		private VonNeumannDisintegrationBeam m_Beam;
		private LogicalWeapon m_BeamWeapon;
		private float m_ApproachRange;
		private int m_RUStore;
		private bool m_Vanished;
		public bool Vanished
		{
			get
			{
				return this.m_Vanished;
			}
		}
		public VonNeumannMomControl VonNeumanMom
		{
			get
			{
				return this.m_VonNeumannMom;
			}
			set
			{
				this.m_VonNeumannMom = value;
			}
		}
		public VonNeumannChildStates State
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
		public override Ship GetShip()
		{
			return this.m_VonNeumannChild;
		}
		public override void SetTarget(IGameObject target)
		{
			if (target != this.m_Target)
			{
				this.m_Target = target;
				int num = (target != null) ? target.ObjectID : 0;
				CombatStance combatStance = (num != 0) ? CombatStance.PURSUE : CombatStance.NO_STANCE;
				if (this.m_VonNeumannChild.CombatStance != combatStance)
				{
					this.m_VonNeumannChild.SetCombatStance(combatStance);
				}
				this.m_VonNeumannChild.Maneuvering.PostSetProp("SetStanceTarget", new object[]
				{
					num,
					this.m_ApproachRange
				});
			}
		}
		public override IGameObject GetTarget()
		{
			return this.m_Target;
		}
		public VonNeumannChildControl(App game, Ship ship)
		{
			this.m_Game = game;
			this.m_VonNeumannChild = ship;
		}
		public override void Initialize()
		{
			this.m_Target = null;
			this.m_VonNeumannMom = null;
			this.m_State = VonNeumannChildStates.SEEK;
			this.m_Vanished = false;
			this.m_RUStore = 0;
			this.m_Beam = null;
			this.m_ApproachRange = 1000f;
			this.m_BeamWeapon = this.m_Game.AssetDatabase.Weapons.FirstOrDefault((LogicalWeapon x) => x.Traits.Any((WeaponEnums.WeaponTraits k) => k == WeaponEnums.WeaponTraits.Disintegrating));
			if (this.m_BeamWeapon != null)
			{
				this.m_BeamWeapon.AddGameObjectReference();
			}
		}
		public override void Terminate()
		{
			this.ClearBeam();
			if (this.m_BeamWeapon != null)
			{
				this.m_BeamWeapon.ReleaseGameObjectReference();
			}
		}
		public override void ObjectRemoved(IGameObject obj)
		{
			if (this.m_VonNeumannMom != null && this.m_VonNeumannMom.GetShip() == obj)
			{
				if (this.m_VonNeumannMom.GetShip().IsDestroyed && this.m_VonNeumannChild != null)
				{
					this.m_VonNeumannChild.KillShip(false);
				}
				this.m_VonNeumannMom = null;
			}
			if (this.m_Target == obj)
			{
				this.m_Target = null;
			}
		}
		public override void OnThink()
		{
			if (this.m_VonNeumannChild == null)
			{
				this.ClearBeam();
				return;
			}
			if (this.m_VonNeumannMom != null && this.m_VonNeumannMom.GetShip().IsDestroyed)
			{
				this.m_VonNeumannChild.KillShip(false);
				return;
			}
			switch (this.m_State)
			{
			case VonNeumannChildStates.SEEK:
				this.ThinkSeek();
				return;
			case VonNeumannChildStates.TRACK:
				this.ThinkTrack();
				return;
			case VonNeumannChildStates.INITCOLLECT:
				this.ThinkInitCollect();
				return;
			case VonNeumannChildStates.COLLECT:
				this.ThinkCollect();
				return;
			case VonNeumannChildStates.RETURN:
				this.ThinkReturn();
				return;
			case VonNeumannChildStates.EMIT:
				this.ThinkEmit();
				return;
			case VonNeumannChildStates.EMITTING:
				this.ThinkEmiting();
				return;
			case VonNeumannChildStates.INITFLEE:
				this.ThinkInitFlee();
				return;
			case VonNeumannChildStates.FLEE:
				this.ThinkFlee();
				return;
			case VonNeumannChildStates.VANISH:
				this.ThinkVanish();
				return;
			default:
				return;
			}
		}
		public override void ForceFlee()
		{
			if (this.m_State != VonNeumannChildStates.INITFLEE && this.m_State != VonNeumannChildStates.FLEE)
			{
				this.m_State = VonNeumannChildStates.INITFLEE;
			}
			this.ClearBeam();
		}
		public override bool VictoryConditionIsMet()
		{
			return this.m_Vanished;
		}
		public override bool RequestingNewTarget()
		{
			return this.m_Target == null && (this.m_State == VonNeumannChildStates.SEEK || this.m_State == VonNeumannChildStates.TRACK);
		}
		public override void FindNewTarget(IEnumerable<IGameObject> objs)
		{
			if (this.m_VonNeumannMom == null)
			{
				float num = 3.40282347E+38f;
				this.m_Target = null;
				foreach (IGameObject current in objs)
				{
					if (current is Ship)
					{
						Ship ship = current as Ship;
						if (ship.Player != this.m_VonNeumannChild.Player && ship.Active && Ship.IsActiveShip(ship) && ship.IsDetected(this.m_VonNeumannChild.Player))
						{
							float lengthSquared = (ship.Position - this.m_VonNeumannChild.Position).LengthSquared;
							if (lengthSquared < num)
							{
								this.m_Target = ship;
								num = lengthSquared;
							}
						}
					}
				}
			}
		}
		public override bool NeedsAParent()
		{
			return this.m_VonNeumannMom == null;
		}
		public override void FindParent(IEnumerable<CombatAIController> controllers)
		{
			foreach (CombatAIController current in controllers)
			{
				if (current is VonNeumannMomControl)
				{
					VonNeumannMomControl vonNeumannMomControl = current as VonNeumannMomControl;
					if (vonNeumannMomControl.IsThisMyMom(this.m_VonNeumannChild))
					{
						vonNeumannMomControl.AddChild(this);
						this.m_VonNeumannMom = vonNeumannMomControl;
						break;
					}
				}
			}
		}
		public int GetStoredResources()
		{
			return this.m_RUStore;
		}
		private void ThinkBirth()
		{
		}
		private void ThinkSeek()
		{
			if (this.m_Target == null)
			{
				if (this.m_VonNeumannMom == null)
				{
					this.m_State = VonNeumannChildStates.INITFLEE;
					return;
				}
				if (this.m_RUStore > 0)
				{
					this.m_State = VonNeumannChildStates.RETURN;
					return;
				}
			}
			else
			{
				this.m_State = VonNeumannChildStates.TRACK;
			}
		}
		private void ThinkTrack()
		{
			if (this.m_Target == null)
			{
				this.m_State = VonNeumannChildStates.INITFLEE;
				return;
			}
			float num = 0f;
			float val = 0f;
			Vector3 vector = Vector3.Zero;
			if (this.m_Target is Ship)
			{
				Ship ship = this.m_Target as Ship;
				vector = ship.Position;
				num = ship.ShipSphere.radius;
			}
			else
			{
				if (this.m_Target is StellarBody)
				{
					StellarBody stellarBody = this.m_Target as StellarBody;
					vector = stellarBody.Parameters.Position;
					num = stellarBody.Parameters.Radius;
					val = 750f;
				}
			}
			WeaponBank weaponBankWithWeaponTrait = this.m_VonNeumannChild.GetWeaponBankWithWeaponTrait(WeaponEnums.WeaponTraits.Disintegrating);
			if (weaponBankWithWeaponTrait != null)
			{
				this.m_ApproachRange = Math.Max(weaponBankWithWeaponTrait.Weapon.RangeTable.Effective.Range, 100f);
				weaponBankWithWeaponTrait.PostSetProp("DisableAllTurrets", true);
			}
			else
			{
				this.m_ApproachRange = Math.Max(100f, CombatAI.GetMinEffectiveWeaponRange(this.m_VonNeumannChild, false));
			}
			this.m_ApproachRange = Math.Max(this.m_ApproachRange, val);
			this.m_ApproachRange += num;
			Vector3 vector2 = vector;
			Vector3 v = this.m_VonNeumannChild.Position - vector2;
			v.Normalize();
			vector2 += v * this.m_ApproachRange;
			Vector3 look = v * -1f;
			this.m_VonNeumannChild.Maneuvering.PostAddGoal(vector2, look);
			float lengthSquared = (this.m_VonNeumannChild.Position - vector2).LengthSquared;
			float num2 = this.m_ApproachRange + 200f;
			if (lengthSquared < num2 * num2)
			{
				this.m_State = VonNeumannChildStates.INITCOLLECT;
			}
		}
		private void ThinkInitCollect()
		{
			this.SpawnBeam(this.m_Target, true);
			this.m_State = VonNeumannChildStates.COLLECT;
		}
		private void ThinkCollect()
		{
			if (this.m_Beam != null && !this.m_Beam.Active && this.m_Beam.ObjectStatus == GameObjectStatus.Ready)
			{
				this.m_Beam.Active = true;
			}
			if (this.m_Beam == null)
			{
				this.m_State = VonNeumannChildStates.SEEK;
				return;
			}
			if (this.m_Beam.Finished)
			{
				if (this.m_Beam.Succeeded)
				{
					this.m_RUStore += this.m_Beam.Resources;
				}
				if (this.m_Target is StellarBody)
				{
					PlanetInfo planetInfo = this.m_Game.GameDatabase.GetPlanetInfo((this.m_Target as StellarBody).Parameters.OrbitalID);
					if (planetInfo != null)
					{
						planetInfo.Resources = Math.Max(planetInfo.Resources - this.m_Beam.Resources, 0);
						this.m_Game.GameDatabase.UpdatePlanet(planetInfo);
					}
				}
				this.m_Target = null;
				if (this.m_VonNeumannChild.CombatStance != CombatStance.NO_STANCE)
				{
					this.m_VonNeumannChild.SetCombatStance(CombatStance.NO_STANCE);
				}
				this.m_VonNeumannChild.Maneuvering.PostSetProp("SetStanceTarget", new object[]
				{
					0,
					0f
				});
				if (this.m_RUStore < this.m_Game.AssetDatabase.GlobalVonNeumannData.ChildRUCarryCap)
				{
					this.m_State = VonNeumannChildStates.SEEK;
				}
				else
				{
					if (this.m_VonNeumannMom != null)
					{
						this.m_State = VonNeumannChildStates.RETURN;
					}
					else
					{
						this.m_State = VonNeumannChildStates.INITFLEE;
					}
				}
				this.ClearBeam();
			}
		}
		private void ThinkReturn()
		{
			if (this.m_VonNeumannMom != null)
			{
				Vector3 vector = this.m_VonNeumannChild.Position - this.m_VonNeumannMom.GetShip().Position;
				vector.Normalize();
				Vector3 vector2 = this.m_VonNeumannMom.GetShip().Position + vector * this.m_ApproachRange;
				this.m_VonNeumannChild.Maneuvering.PostAddGoal(vector2, -vector);
				if ((vector2 - this.m_VonNeumannChild.Position).LengthSquared < 40000f)
				{
					this.m_State = VonNeumannChildStates.EMIT;
					return;
				}
			}
			else
			{
				this.m_State = VonNeumannChildStates.INITFLEE;
			}
		}
		private void ThinkEmit()
		{
			this.SpawnBeam(this.m_VonNeumannMom.GetShip(), false);
			this.m_State = VonNeumannChildStates.EMITTING;
		}
		private void ThinkEmiting()
		{
			if (this.m_Beam != null && !this.m_Beam.Active && this.m_Beam.ObjectStatus == GameObjectStatus.Ready)
			{
				this.m_Beam.Active = true;
			}
			if (this.m_VonNeumannMom == null)
			{
				this.m_State = VonNeumannChildStates.INITFLEE;
				this.ClearBeam();
				return;
			}
			if (this.m_Beam.Finished)
			{
				this.ClearBeam();
				this.m_VonNeumannMom.SubmitResources(this.m_RUStore);
				this.m_RUStore = 0;
				this.m_State = VonNeumannChildStates.SEEK;
			}
		}
		private void ThinkInitFlee()
		{
			if (this.m_VonNeumannChild.CombatStance != CombatStance.NO_STANCE)
			{
				this.m_VonNeumannChild.SetCombatStance(CombatStance.NO_STANCE);
			}
			this.m_VonNeumannChild.Maneuvering.PostSetProp("SetStanceTarget", new object[]
			{
				0,
				0f
			});
			this.m_State = VonNeumannChildStates.FLEE;
		}
		private void ThinkFlee()
		{
			if (this.m_VonNeumannMom != null && !this.m_VonNeumannMom.Vanished)
			{
				if (this.m_VonNeumannChild.CombatStance != CombatStance.PURSUE)
				{
					this.m_VonNeumannChild.SetCombatStance(CombatStance.PURSUE);
				}
				this.m_VonNeumannChild.Maneuvering.PostSetProp("SetStanceTarget", new object[]
				{
					this.m_VonNeumannMom.GetShip().ObjectID,
					this.m_ApproachRange
				});
				return;
			}
			if (this.m_VonNeumannChild.CombatStance != CombatStance.RETREAT)
			{
				this.m_VonNeumannChild.SetCombatStance(CombatStance.RETREAT);
			}
			if (this.m_VonNeumannChild.HasRetreated)
			{
				this.m_State = VonNeumannChildStates.VANISH;
			}
		}
		private void ThinkVanish()
		{
			if (!this.m_Vanished)
			{
				this.m_VonNeumannChild.Active = false;
				this.m_Vanished = true;
			}
		}
		private void SpawnBeam(IGameObject target, bool absorbTarget)
		{
			this.ClearBeam();
			if (this.m_BeamWeapon == null || target == null)
			{
				return;
			}
			Turret turretWithWeaponTrait = this.m_VonNeumannChild.GetTurretWithWeaponTrait(WeaponEnums.WeaponTraits.Disintegrating);
			int objectID;
			if (turretWithWeaponTrait != null)
			{
				objectID = turretWithWeaponTrait.ObjectID;
			}
			else
			{
				objectID = this.m_VonNeumannChild.ObjectID;
			}
			int num = this.m_Game.AssetDatabase.GlobalVonNeumannData.ChildRUCarryCap;
			if (target is StellarBody)
			{
				PlanetInfo planetInfo = this.m_Game.GameDatabase.GetPlanetInfo((target as StellarBody).Parameters.OrbitalID);
				if (planetInfo != null)
				{
					num = Math.Min(num, planetInfo.Resources);
				}
			}
			List<object> list = new List<object>();
			list.Add(target.ObjectID);
			list.Add(this.m_BeamWeapon.GameObject.ObjectID);
			list.Add(this.m_VonNeumannChild.ObjectID);
			list.Add(this.m_VonNeumannChild.Player.ObjectID);
			list.Add(objectID);
			list.Add(absorbTarget);
			list.Add(num);
			list.Add(this.m_Game.AssetDatabase.GlobalVonNeumannData.RUTransferRateShip);
			list.Add(this.m_Game.AssetDatabase.GlobalVonNeumannData.RUTransferRatePlanet);
			list.Add(this.m_Game.AssetDatabase.GlobalVonNeumannData.ChildIntegrationTime);
			this.m_Beam = this.m_Game.AddObject<VonNeumannDisintegrationBeam>(list.ToArray());
		}
		private void ClearBeam()
		{
			if (this.m_Beam != null)
			{
				this.m_Game.ReleaseObject(this.m_Beam);
				this.m_Beam = null;
			}
		}
	}
}
