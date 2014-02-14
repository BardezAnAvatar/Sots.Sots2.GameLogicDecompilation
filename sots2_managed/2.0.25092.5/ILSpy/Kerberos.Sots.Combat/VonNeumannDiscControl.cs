using Kerberos.Sots.Encounters;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Ships;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Combat
{
	internal class VonNeumannDiscControl : CombatAIController
	{
		private const int KUpdateRate = 60;
		public static readonly string[] _vonneumannDiscTypes = Enum.GetNames(typeof(VonNeumannDiscTypes));
		private App m_Game;
		private Ship m_Disc;
		private IGameObject m_Target;
		private VonNeumannNeoBerserkerControl m_ParentBerserker;
		private VonNeumannDiscTypes m_DiscType;
		private VonNeumannDiscStates m_State;
		private List<VonNeumannDiscControl> m_Discs;
		private List<Ship> m_PossessorBoardingPods;
		private int m_UpdateDelay;
		private bool m_PodsLaunched;
		public VonNeumannDiscTypes DiscType
		{
			get
			{
				return this.m_DiscType;
			}
		}
		public static bool DiscTypeIsAttacker(VonNeumannDiscTypes type)
		{
			switch (type)
			{
			case VonNeumannDiscTypes.IMPACTOR:
			case VonNeumannDiscTypes.POSSESSOR:
			case VonNeumannDiscTypes.DISINTEGRATOR:
			case VonNeumannDiscTypes.EMPULSER:
			case VonNeumannDiscTypes.EMITTER:
			case VonNeumannDiscTypes.ABSORBER:
			case VonNeumannDiscTypes.OPRESSOR:
				return true;
			default:
				return false;
			}
		}
		public static VonNeumannDiscTypes DiscTypeFromMissionSection(Section section)
		{
			VonNeumannDiscTypes result = VonNeumannDiscTypes.IMPACTOR;
			string text = section.ShipSectionAsset.SectionName.ToUpper();
			for (int i = 0; i < 10; i++)
			{
				if (text.Contains(VonNeumannDiscControl._vonneumannDiscTypes[i]))
				{
					result = (VonNeumannDiscTypes)i;
					break;
				}
			}
			return result;
		}
		public override Ship GetShip()
		{
			return this.m_Disc;
		}
		public override void SetTarget(IGameObject target)
		{
			this.m_Target = target;
			this.m_Disc.SetShipTarget((target != null) ? target.ObjectID : 0, Vector3.Zero, true, 0);
		}
		public override IGameObject GetTarget()
		{
			return this.m_Target;
		}
		public VonNeumannDiscControl(App game, Ship ship)
		{
			this.m_Game = game;
			this.m_Disc = ship;
			Section section = ship.Sections.FirstOrDefault((Section x) => x.ShipSectionAsset.Type == ShipSectionType.Mission);
			this.m_DiscType = VonNeumannDiscControl.DiscTypeFromMissionSection(section);
		}
		public override void Initialize()
		{
			this.m_Target = null;
			this.m_ParentBerserker = null;
			this.m_State = VonNeumannDiscStates.SEEK;
			this.m_PodsLaunched = false;
			this.m_Discs = new List<VonNeumannDiscControl>();
			this.m_PossessorBoardingPods = new List<Ship>();
			this.m_UpdateDelay = 0;
			if (this.m_DiscType == VonNeumannDiscTypes.POSSESSOR)
			{
				Matrix worldMat = Matrix.CreateRotationYPR(this.m_Disc.Maneuvering.Rotation);
				worldMat.Position = this.m_Disc.Maneuvering.Position;
				int num = this.m_Disc.BattleRiderMounts.Count<BattleRiderMount>();
				for (int i = 0; i < num; i++)
				{
					Ship ship = CombatAIController.CreateNewShip(this.m_Game.Game, worldMat, VonNeumann.StaticShipDesigns[VonNeumann.VonNeumannShipDesigns.BoardingPod].DesignId, this.m_Disc.ObjectID, this.m_Disc.InputID, this.m_Disc.Player.ObjectID);
					if (ship != null)
					{
						this.m_PossessorBoardingPods.Add(ship);
					}
				}
				this.m_State = VonNeumannDiscStates.ACTIVATEBOARDINGPODS;
			}
		}
		public override void Terminate()
		{
			if (this.m_DiscType == VonNeumannDiscTypes.SHIELDER)
			{
				this.ShieldDiscDestroyed();
			}
			if (this.m_DiscType == VonNeumannDiscTypes.CLOAKER)
			{
				this.CloakDiscDestroyed();
			}
		}
		public override void ObjectRemoved(IGameObject obj)
		{
			if (this.m_Target == obj)
			{
				this.SetTarget(null);
				if (this.m_Disc.CombatStance != CombatStance.NO_STANCE)
				{
					this.m_Disc.SetCombatStance(CombatStance.NO_STANCE);
				}
				this.m_Disc.Maneuvering.PostSetProp("SetStanceTarget", new object[]
				{
					0,
					0f
				});
			}
			if (this.m_ParentBerserker != null && this.m_ParentBerserker.GetShip() == obj)
			{
				this.m_ParentBerserker = null;
			}
			foreach (VonNeumannDiscControl current in this.m_Discs)
			{
				if (current.GetShip() == obj)
				{
					this.m_Discs.Remove(current);
					break;
				}
			}
			foreach (Ship current2 in this.m_PossessorBoardingPods)
			{
				if (current2 == obj)
				{
					this.m_PossessorBoardingPods.Remove(current2);
					break;
				}
			}
		}
		public override void OnThink()
		{
			if (this.m_Disc == null)
			{
				return;
			}
			VonNeumannDiscTypes discType = this.m_DiscType;
			if (discType != VonNeumannDiscTypes.POSSESSOR)
			{
				switch (discType)
				{
				case VonNeumannDiscTypes.SHIELDER:
					this.UpdateShieldDisc();
					break;
				case VonNeumannDiscTypes.CLOAKER:
					this.UpdateCloakDisc();
					break;
				}
			}
			else
			{
				this.UpdatePods();
			}
			switch (this.m_State)
			{
			case VonNeumannDiscStates.SEEK:
				this.ThinkSeek();
				return;
			case VonNeumannDiscStates.TRACK:
				this.ThinkTrack();
				return;
			case VonNeumannDiscStates.LAUNCHPODS:
				this.ThinkLaunchPods();
				return;
			case VonNeumannDiscStates.WAITFORLAUNCH:
				this.ThinkWaitForLaunch();
				return;
			case VonNeumannDiscStates.ACTIVATEBOARDINGPODS:
				this.ThinkActivateBoardingPods();
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
			return this.m_Target == null && this.m_State == VonNeumannDiscStates.SEEK;
		}
		public override void FindNewTarget(IEnumerable<IGameObject> objs)
		{
			if (this.m_ParentBerserker != null)
			{
				float num = 3.40282347E+38f;
				IGameObject target = null;
				foreach (IGameObject current in objs)
				{
					if (current is Ship)
					{
						Ship ship = current as Ship;
						if (VonNeumannDiscControl.DiscTypeIsAttacker(this.m_DiscType))
						{
							if (ship.Player == this.m_Disc.Player)
							{
								continue;
							}
						}
						else
						{
							if (ship.Player != this.m_Disc.Player)
							{
								continue;
							}
						}
						if (ship.Active && Ship.IsActiveShip(ship) && ship.IsDetected(this.m_Disc.Player))
						{
							float lengthSquared = (ship.Position - this.m_Disc.Position).LengthSquared;
							if (lengthSquared < num)
							{
								target = ship;
								num = lengthSquared;
							}
						}
					}
				}
				this.SetTarget(target);
			}
		}
		public override bool NeedsAParent()
		{
			return this.m_ParentBerserker == null;
		}
		public override void FindParent(IEnumerable<CombatAIController> controllers)
		{
			foreach (CombatAIController current in controllers)
			{
				if (current is VonNeumannNeoBerserkerControl)
				{
					VonNeumannNeoBerserkerControl vonNeumannNeoBerserkerControl = current as VonNeumannNeoBerserkerControl;
					if (vonNeumannNeoBerserkerControl.IsThisMyMom(this.m_Disc))
					{
						vonNeumannNeoBerserkerControl.AddChild(this);
						this.m_ParentBerserker = vonNeumannNeoBerserkerControl;
						break;
					}
				}
			}
		}
		private void ThinkSeek()
		{
			if (this.m_Target != null)
			{
				float num = VonNeumannDiscControl.DiscTypeIsAttacker(this.m_DiscType) ? Math.Max(100f, CombatAI.GetMinPointBlankWeaponRange(this.m_Disc, false)) : 400f;
				if (this.m_Disc.CombatStance != CombatStance.PURSUE)
				{
					this.m_Disc.SetCombatStance(CombatStance.PURSUE);
				}
				this.m_Disc.Maneuvering.PostSetProp("SetStanceTarget", new object[]
				{
					this.m_Target.ObjectID,
					num
				});
				this.m_State = VonNeumannDiscStates.TRACK;
			}
		}
		private void ThinkTrack()
		{
			if (this.m_Target == null)
			{
				this.m_State = VonNeumannDiscStates.SEEK;
				if (this.m_Disc.CombatStance != CombatStance.NO_STANCE)
				{
					this.m_Disc.SetCombatStance(CombatStance.NO_STANCE);
				}
				this.m_Disc.Maneuvering.PostSetProp("SetStanceTarget", new object[]
				{
					0,
					0f
				});
				return;
			}
			if (this.m_DiscType == VonNeumannDiscTypes.POSSESSOR && this.m_Target is Ship)
			{
				Ship ship = this.m_Target as Ship;
				if (!this.m_PodsLaunched && (this.m_Disc.Position - ship.Position).LengthSquared < 2250000f)
				{
					this.m_State = VonNeumannDiscStates.LAUNCHPODS;
				}
			}
		}
		private void ThinkLaunchPods()
		{
			bool flag = true;
			foreach (Ship current in this.m_PossessorBoardingPods)
			{
				if (!current.DockedWithParent)
				{
					flag = false;
				}
			}
			if (flag)
			{
				this.m_Disc.PostSetProp("LaunchBattleriders", new object[0]);
				this.m_State = VonNeumannDiscStates.WAITFORLAUNCH;
			}
		}
		private void ThinkWaitForLaunch()
		{
			bool flag = true;
			foreach (Ship current in this.m_PossessorBoardingPods)
			{
				if (current.DockedWithParent)
				{
					flag = false;
				}
			}
			if (flag)
			{
				this.m_PodsLaunched = true;
				this.m_State = VonNeumannDiscStates.SEEK;
			}
		}
		private void ThinkActivateBoardingPods()
		{
			bool flag = true;
			foreach (Ship current in this.m_PossessorBoardingPods)
			{
				if (current.ObjectStatus != GameObjectStatus.Ready)
				{
					flag = false;
				}
			}
			if (flag)
			{
				foreach (Ship current2 in this.m_PossessorBoardingPods)
				{
					BattleRiderSquad battleRiderSquad = this.m_Disc.AssignRiderToSquad(current2 as BattleRiderShip, current2.RiderIndex);
					if (battleRiderSquad != null)
					{
						current2.ParentID = this.m_Disc.ObjectID;
						current2.PostSetBattleRiderParent(battleRiderSquad.ObjectID);
						current2.Player = this.m_Disc.Player;
					}
					current2.Active = true;
					this.m_Game.CurrentState.AddGameObject(current2, false);
				}
				this.m_State = VonNeumannDiscStates.SEEK;
			}
		}
		public void SetListOfDiscs(List<VonNeumannDiscControl> discs)
		{
			foreach (VonNeumannDiscControl current in discs)
			{
				if (!this.m_Discs.Contains(current))
				{
					this.m_Discs.Add(current);
				}
			}
		}
		private void UpdatePods()
		{
			if (this.m_PodsLaunched)
			{
				this.m_PodsLaunched = false;
				foreach (Ship current in this.m_PossessorBoardingPods)
				{
					if (!current.DockedWithParent)
					{
						this.m_PodsLaunched = true;
						break;
					}
				}
			}
		}
		private void UpdateShieldDisc()
		{
			if (this.m_Disc == null || this.m_Disc.IsDestroyed)
			{
				return;
			}
			this.m_UpdateDelay--;
			if (this.m_UpdateDelay > 0)
			{
				return;
			}
			this.m_UpdateDelay = 60;
			foreach (VonNeumannDiscControl current in this.m_Discs)
			{
				if (current.GetShip().Shield != null)
				{
					if (current.GetShip() == this.m_Disc)
					{
						if (!current.GetShip().Shield.Active)
						{
							current.GetShip().Shield.Active = true;
						}
					}
					else
					{
						if ((current.GetShip().Position - this.m_Disc.Position).LengthSquared < 250000f)
						{
							if (!current.GetShip().Shield.Active)
							{
								current.GetShip().Shield.Active = true;
							}
						}
						else
						{
							if (current.GetShip().Shield.Active)
							{
								current.GetShip().Shield.Active = false;
							}
						}
					}
				}
			}
		}
		private void ShieldDiscDestroyed()
		{
			foreach (VonNeumannDiscControl current in this.m_Discs)
			{
				current.GetShip().Shield.Active = false;
			}
		}
		private void UpdateCloakDisc()
		{
			if (this.m_Disc == null || this.m_Disc.IsDestroyed)
			{
				return;
			}
			this.m_UpdateDelay--;
			if (this.m_UpdateDelay > 0)
			{
				return;
			}
			this.m_UpdateDelay = 60;
			foreach (VonNeumannDiscControl current in this.m_Discs)
			{
				if (current.GetShip() == this.m_Disc)
				{
					if (current.GetShip().CloakedState == CloakedState.None)
					{
						current.GetShip().SetCloaked(true);
					}
				}
				else
				{
					if ((current.GetShip().Position - this.m_Disc.Position).LengthSquared < 250000f)
					{
						if (current.GetShip().CloakedState == CloakedState.None)
						{
							current.GetShip().SetCloaked(true);
						}
					}
					else
					{
						if (current.GetShip().CloakedState != CloakedState.None)
						{
							current.GetShip().SetCloaked(false);
						}
					}
				}
			}
		}
		private void CloakDiscDestroyed()
		{
			foreach (VonNeumannDiscControl current in this.m_Discs)
			{
				current.GetShip().SetCloaked(false);
			}
		}
	}
}
