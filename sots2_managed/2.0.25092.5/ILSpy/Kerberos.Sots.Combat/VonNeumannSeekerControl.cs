using Kerberos.Sots.Encounters;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.Combat
{
	internal class VonNeumannSeekerControl : CombatAIController
	{
		private const int MAX_SEEKER_PODS = 12;
		private App m_Game;
		private Ship m_VonNeumannSeeker;
		private List<Ship> m_LoadingPods;
		private List<Ship> m_VonNeumannSeekerPods;
		private List<PlanetTarget> m_AvailableTargets;
		private StellarBody m_CurrentTarget;
		private VonNeumannSeekerStates m_State;
		private int m_NumPodsRemain;
		private bool m_Vanished;
		public bool Vanished
		{
			get
			{
				return this.m_Vanished;
			}
		}
		public override Ship GetShip()
		{
			return this.m_VonNeumannSeeker;
		}
		public override void SetTarget(IGameObject target)
		{
			if (target == null)
			{
				this.m_CurrentTarget = null;
				return;
			}
			if (target is StellarBody)
			{
				this.m_CurrentTarget = (target as StellarBody);
			}
		}
		public override IGameObject GetTarget()
		{
			return this.m_CurrentTarget;
		}
		public VonNeumannSeekerControl(App game, Ship ship)
		{
			this.m_Game = game;
			this.m_VonNeumannSeeker = ship;
			this.m_NumPodsRemain = 0;
		}
		public override void Initialize()
		{
			this.m_VonNeumannSeekerPods = new List<Ship>();
			this.m_LoadingPods = new List<Ship>();
			this.m_AvailableTargets = new List<PlanetTarget>();
			this.m_CurrentTarget = null;
			this.m_State = VonNeumannSeekerStates.SEEK;
			this.m_Vanished = false;
			Matrix worldMat = Matrix.CreateRotationYPR(this.m_VonNeumannSeeker.Maneuvering.Rotation);
			worldMat.Position = this.m_VonNeumannSeeker.Maneuvering.Position;
			int designId = VonNeumann.StaticShipDesigns[VonNeumann.VonNeumannShipDesigns.SeekerProbe].DesignId;
			for (int i = 0; i < 12; i++)
			{
				Ship ship = CombatAIController.CreateNewShip(this.m_Game.Game, worldMat, designId, this.m_VonNeumannSeeker.ObjectID, this.m_VonNeumannSeeker.InputID, this.m_VonNeumannSeeker.Player.ObjectID);
				if (ship != null)
				{
					this.m_LoadingPods.Add(ship);
					this.m_NumPodsRemain++;
				}
			}
			this.m_State = VonNeumannSeekerStates.INTEGRATECHILD;
		}
		public override void Terminate()
		{
		}
		public override void ObjectRemoved(IGameObject obj)
		{
			foreach (Ship current in this.m_VonNeumannSeekerPods)
			{
				if (current == obj)
				{
					this.m_NumPodsRemain--;
					this.m_VonNeumannSeekerPods.Remove(current);
					break;
				}
			}
			foreach (PlanetTarget current2 in this.m_AvailableTargets)
			{
				if (current2.Planet == obj)
				{
					this.m_AvailableTargets.Remove(current2);
					break;
				}
			}
			if (obj == this.m_CurrentTarget)
			{
				this.m_CurrentTarget = null;
			}
		}
		public override void OnThink()
		{
			if (this.m_VonNeumannSeeker == null)
			{
				return;
			}
			switch (this.m_State)
			{
			case VonNeumannSeekerStates.INTEGRATECHILD:
				this.ThinkIntegrateChild();
				return;
			case VonNeumannSeekerStates.SEEK:
				this.ThinkSeek();
				return;
			case VonNeumannSeekerStates.TRACK:
				this.ThinkTrack();
				return;
			case VonNeumannSeekerStates.BOMBARD:
				this.ThinkBombard();
				return;
			case VonNeumannSeekerStates.LAUNCHING:
				this.ThinkLaunching();
				return;
			case VonNeumannSeekerStates.WAIT:
				this.ThinkWait();
				return;
			case VonNeumannSeekerStates.INITFLEE:
				this.ThinkInitFlee();
				return;
			case VonNeumannSeekerStates.FLEE:
				this.ThinkFlee();
				return;
			case VonNeumannSeekerStates.VANISH:
				this.ThinkVanish();
				return;
			default:
				return;
			}
		}
		public override void ForceFlee()
		{
			if (this.m_State != VonNeumannSeekerStates.INITFLEE && this.m_State != VonNeumannSeekerStates.FLEE)
			{
				this.m_State = VonNeumannSeekerStates.INITFLEE;
			}
		}
		public override bool VictoryConditionIsMet()
		{
			return this.m_Vanished;
		}
		public override bool RequestingNewTarget()
		{
			return this.m_State == VonNeumannSeekerStates.SEEK && this.m_AvailableTargets.Count == 0;
		}
		public override void FindNewTarget(IEnumerable<IGameObject> objs)
		{
			this.m_AvailableTargets.Clear();
			foreach (IGameObject current in objs)
			{
				if (current is StellarBody)
				{
					StellarBody stellarBody = current as StellarBody;
					if (stellarBody.Population > 0.0)
					{
						this.m_AvailableTargets.Add(new PlanetTarget(stellarBody));
					}
				}
			}
		}
		public override bool NeedsAParent()
		{
			return false;
		}
		public override void FindParent(IEnumerable<CombatAIController> controllers)
		{
		}
		private void ThinkIntegrateChild()
		{
			if (this.m_LoadingPods.Count == 0)
			{
				this.m_State = VonNeumannSeekerStates.SEEK;
				return;
			}
			bool flag = true;
			foreach (Ship current in this.m_LoadingPods)
			{
				if (current.ObjectStatus != GameObjectStatus.Ready)
				{
					flag = false;
				}
			}
			if (flag)
			{
				foreach (Ship current2 in this.m_LoadingPods)
				{
					current2.Player = this.m_VonNeumannSeeker.Player;
					current2.Visible = this.m_VonNeumannSeeker.Visible;
					current2.Active = true;
					this.m_Game.CurrentState.AddGameObject(current2, false);
					this.m_VonNeumannSeekerPods.Add(current2);
				}
				this.m_LoadingPods.Clear();
				this.m_State = VonNeumannSeekerStates.SEEK;
			}
		}
		private void ThinkSeek()
		{
			if (this.m_NumPodsRemain == 0)
			{
				this.m_State = VonNeumannSeekerStates.INITFLEE;
				return;
			}
			if (this.m_CurrentTarget == null)
			{
				float num = 3.40282347E+38f;
				foreach (PlanetTarget current in this.m_AvailableTargets)
				{
					if (!current.HasBeenVisted)
					{
						float lengthSquared = (current.Planet.Parameters.Position - this.m_VonNeumannSeeker.Position).LengthSquared;
						if (lengthSquared < num)
						{
							num = lengthSquared;
							this.m_CurrentTarget = current.Planet;
						}
					}
				}
				this.m_VonNeumannSeeker.SetShipTarget((this.m_CurrentTarget != null) ? this.m_CurrentTarget.ObjectID : 0, Vector3.Zero, true, 0);
				if (this.m_CurrentTarget == null)
				{
					this.m_State = VonNeumannSeekerStates.INITFLEE;
					return;
				}
			}
			else
			{
				this.m_State = VonNeumannSeekerStates.TRACK;
			}
		}
		private void ThinkTrack()
		{
			if (this.m_CurrentTarget == null)
			{
				this.m_State = VonNeumannSeekerStates.SEEK;
				return;
			}
			Vector3 position = this.m_CurrentTarget.Parameters.Position;
			float num = this.m_CurrentTarget.Parameters.Radius + 750f + 500f;
			Vector3 vector = position;
			Vector3 v = this.m_VonNeumannSeeker.Position - vector;
			v.Y = 0f;
			v.Normalize();
			vector += v * (num - 100f);
			Vector3 look = v * -1f;
			this.m_VonNeumannSeeker.Maneuvering.PostAddGoal(vector, look);
			float lengthSquared = (this.m_VonNeumannSeeker.Position - vector).LengthSquared;
			if (lengthSquared < num * num)
			{
				this.m_State = VonNeumannSeekerStates.BOMBARD;
			}
		}
		private void ThinkBombard()
		{
			if (this.m_CurrentTarget == null)
			{
				this.m_State = VonNeumannSeekerStates.SEEK;
				return;
			}
			this.m_VonNeumannSeeker.PostSetProp("LaunchBattleriders", new object[0]);
			this.m_State = VonNeumannSeekerStates.LAUNCHING;
		}
		private void ThinkLaunching()
		{
			bool flag = true;
			foreach (Ship current in this.m_VonNeumannSeekerPods)
			{
				if (current.DockedWithParent)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				this.m_State = VonNeumannSeekerStates.WAIT;
			}
		}
		private void ThinkWait()
		{
			bool flag = true;
			foreach (Ship current in this.m_VonNeumannSeekerPods)
			{
				if (!current.DockedWithParent)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				foreach (PlanetTarget current2 in this.m_AvailableTargets)
				{
					if (current2.Planet == this.m_CurrentTarget)
					{
						current2.HasBeenVisted = true;
						break;
					}
				}
				this.m_CurrentTarget = null;
				this.m_State = VonNeumannSeekerStates.SEEK;
			}
		}
		private void ThinkInitFlee()
		{
			this.m_State = VonNeumannSeekerStates.FLEE;
			this.m_VonNeumannSeeker.PostSetProp("RecoverBattleriders", new object[0]);
		}
		private void ThinkFlee()
		{
			if (this.m_VonNeumannSeeker.CombatStance != CombatStance.RETREAT)
			{
				this.m_VonNeumannSeeker.SetCombatStance(CombatStance.RETREAT);
			}
			if (this.m_VonNeumannSeeker.HasRetreated)
			{
				this.m_State = VonNeumannSeekerStates.VANISH;
			}
		}
		private void ThinkVanish()
		{
			if (!this.m_Vanished)
			{
				this.m_VonNeumannSeeker.Active = false;
				this.m_Vanished = true;
			}
		}
	}
}
