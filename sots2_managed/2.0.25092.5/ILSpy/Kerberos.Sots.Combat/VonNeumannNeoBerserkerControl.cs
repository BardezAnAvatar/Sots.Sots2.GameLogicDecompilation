using Kerberos.Sots.Encounters;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Combat
{
	internal class VonNeumannNeoBerserkerControl : CombatAIController
	{
		protected const int MAX_DISCS = 10;
		protected App m_Game;
		protected Ship m_VonNeumannBerserker;
		protected List<Ship> m_LoadingDiscs;
		protected List<VonNeumannDiscControl> m_Discs;
		protected StellarBody m_Target;
		protected VonNeumannBerserkerStates m_State;
		protected bool m_DiscsActivated;
		public override Ship GetShip()
		{
			return this.m_VonNeumannBerserker;
		}
		public override void SetTarget(IGameObject target)
		{
			if (target is StellarBody && this.m_VonNeumannBerserker != null)
			{
				this.m_Target = (target as StellarBody);
				this.m_VonNeumannBerserker.SetShipTarget((this.m_Target != null) ? this.m_Target.ObjectID : 0, Vector3.Zero, true, 0);
			}
		}
		public override IGameObject GetTarget()
		{
			return this.m_Target;
		}
		public VonNeumannNeoBerserkerControl(App game, Ship ship)
		{
			this.m_Game = game;
			this.m_VonNeumannBerserker = ship;
		}
		public override void Initialize()
		{
			this.m_LoadingDiscs = new List<Ship>();
			this.m_Discs = new List<VonNeumannDiscControl>();
			this.m_DiscsActivated = false;
			Matrix rhs = Matrix.CreateRotationYPR(this.m_VonNeumannBerserker.Maneuvering.Rotation);
			rhs.Position = this.m_VonNeumannBerserker.Maneuvering.Position;
			float num = MathHelper.DegreesToRadians(36f);
			for (int i = 0; i < 10; i++)
			{
				Matrix lhs = Matrix.CreateRotationY((float)i * num);
				Matrix worldMat = lhs * rhs;
				worldMat.Position += worldMat.Forward * 500f;
				int key = 5 + i;
				Ship ship = CombatAIController.CreateNewShip(this.m_Game.Game, worldMat, VonNeumann.StaticShipDesigns[(VonNeumann.VonNeumannShipDesigns)key].DesignId, this.m_VonNeumannBerserker.ObjectID, this.m_VonNeumannBerserker.InputID, this.m_VonNeumannBerserker.Player.ObjectID);
				if (ship != null)
				{
					this.m_LoadingDiscs.Add(ship);
				}
			}
			this.m_State = VonNeumannBerserkerStates.INTEGRATECHILDS;
		}
		public override void Terminate()
		{
		}
		public override void ObjectRemoved(IGameObject obj)
		{
			foreach (VonNeumannDiscControl current in this.m_Discs)
			{
				if (current.GetShip() == obj)
				{
					this.m_Discs.Remove(current);
					break;
				}
			}
			foreach (Ship current2 in this.m_LoadingDiscs)
			{
				if (current2 == obj)
				{
					this.m_LoadingDiscs.Remove(current2);
					break;
				}
			}
			if (obj == this.m_VonNeumannBerserker)
			{
				if (this.m_VonNeumannBerserker != null && this.m_VonNeumannBerserker.IsDestroyed && !this.m_DiscsActivated)
				{
					foreach (VonNeumannDiscControl current3 in this.m_Discs)
					{
						if (current3.GetShip() != null)
						{
							current3.GetShip().KillShip(false);
						}
					}
				}
				this.m_VonNeumannBerserker = null;
			}
		}
		public virtual bool IsThisMyMom(Ship ship)
		{
			return this.m_LoadingDiscs.Any((Ship x) => x == ship);
		}
		public virtual void AddChild(CombatAIController child)
		{
			if (child is VonNeumannDiscControl)
			{
				foreach (Ship current in this.m_LoadingDiscs)
				{
					if (current == child.GetShip())
					{
						current.Active = this.m_DiscsActivated;
						this.m_LoadingDiscs.Remove(current);
						break;
					}
				}
				this.m_Discs.Add(child as VonNeumannDiscControl);
			}
		}
		public override void OnThink()
		{
			if (this.m_VonNeumannBerserker == null)
			{
				return;
			}
			if (this.m_VonNeumannBerserker.IsDestroyed && !this.m_DiscsActivated)
			{
				foreach (VonNeumannDiscControl current in this.m_Discs)
				{
					if (current.GetShip() != null)
					{
						current.GetShip().KillShip(false);
					}
				}
				return;
			}
			switch (this.m_State)
			{
			case VonNeumannBerserkerStates.INTEGRATECHILDS:
				this.ThinkIntegrateChilds();
				return;
			case VonNeumannBerserkerStates.SEEK:
				this.ThinkSeek();
				return;
			case VonNeumannBerserkerStates.TRACK:
				this.ThinkTrack();
				return;
			case VonNeumannBerserkerStates.BOMBARD:
			case VonNeumannBerserkerStates.LAUNCHING:
				break;
			case VonNeumannBerserkerStates.ACTIVATINGDISCS:
				this.ThinkActivatingDiscs();
				break;
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
			if (this.m_LoadingDiscs.Count > 0)
			{
				return false;
			}
			bool flag = this.m_State == VonNeumannBerserkerStates.SEEK;
			foreach (VonNeumannDiscControl current in this.m_Discs)
			{
				flag = (flag || current.RequestingNewTarget());
				if (flag)
				{
					break;
				}
			}
			return flag || !this.m_DiscsActivated;
		}
		public override void FindNewTarget(IEnumerable<IGameObject> objs)
		{
			List<Ship> list = new List<Ship>();
			List<Ship> list2 = new List<Ship>();
			foreach (IGameObject current in objs)
			{
				if (current is Ship)
				{
					Ship ship = current as Ship;
					if (ship != this.m_VonNeumannBerserker && Ship.IsActiveShip(ship) && ship.IsDetected(this.m_VonNeumannBerserker.Player) && ship.Player != this.m_VonNeumannBerserker.Player)
					{
						list.Add(ship);
					}
				}
			}
			foreach (VonNeumannDiscControl current2 in this.m_Discs)
			{
				if (VonNeumannDiscControl.DiscTypeIsAttacker(current2.DiscType))
				{
					list2.Add(current2.GetShip());
				}
			}
			if (list.Count == 0)
			{
				return;
			}
			if (!this.m_DiscsActivated)
			{
				this.m_State = VonNeumannBerserkerStates.ACTIVATINGDISCS;
			}
			foreach (VonNeumannDiscControl current3 in this.m_Discs)
			{
				if (Ship.IsActiveShip(current3.GetShip()) && current3.RequestingNewTarget())
				{
					Vector3 position = current3.GetShip().Position;
					float num = 3.40282347E+38f;
					Ship ship2 = null;
					List<Ship> list3 = VonNeumannDiscControl.DiscTypeIsAttacker(current3.DiscType) ? list : list2;
					foreach (Ship current4 in list3)
					{
						if (current4 != current3.GetShip())
						{
							float lengthSquared = (current4.Position - position).LengthSquared;
							if (lengthSquared < num)
							{
								ship2 = current4;
								num = lengthSquared;
							}
						}
					}
					if (ship2 != null)
					{
						current3.SetTarget(ship2);
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
		private void ThinkIntegrateChilds()
		{
			if (this.m_LoadingDiscs.Count == 0)
			{
				this.m_State = VonNeumannBerserkerStates.SEEK;
				return;
			}
			bool flag = true;
			foreach (Ship current in this.m_LoadingDiscs)
			{
				if (current.ObjectStatus != GameObjectStatus.Ready)
				{
					flag = false;
				}
			}
			if (flag)
			{
				foreach (Ship current2 in this.m_LoadingDiscs)
				{
					current2.Player = this.m_VonNeumannBerserker.Player;
					current2.Active = false;
					this.m_Game.CurrentState.AddGameObject(current2, false);
				}
				this.m_State = VonNeumannBerserkerStates.SEEK;
			}
		}
		protected virtual void ThinkSeek()
		{
			if (this.m_Target != null)
			{
				this.m_State = VonNeumannBerserkerStates.TRACK;
			}
			if (!this.m_DiscsActivated && !(this is VonNeumannBerserkerControl))
			{
				this.m_State = VonNeumannBerserkerStates.ACTIVATINGDISCS;
			}
		}
		protected virtual void ThinkTrack()
		{
			if (this.m_Target == null)
			{
				this.m_State = VonNeumannBerserkerStates.SEEK;
			}
		}
		protected virtual void ThinkActivatingDiscs()
		{
			Matrix rhs = Matrix.CreateRotationYPR(this.m_VonNeumannBerserker.Maneuvering.Rotation);
			rhs.Position = this.m_VonNeumannBerserker.Maneuvering.Position;
			float num = MathHelper.DegreesToRadians(36f);
			int num2 = 0;
			foreach (VonNeumannDiscControl current in this.m_Discs)
			{
				Ship ship = current.GetShip();
				Matrix lhs = Matrix.CreateRotationY((float)num2 * num);
				Matrix matrix = lhs * rhs;
				ship.Position = matrix.Position + matrix.Forward * 500f;
				ship.Active = true;
				if (ship.Shield != null)
				{
					ship.Shield.Active = false;
				}
				current.SetListOfDiscs(this.m_Discs);
				num2++;
			}
			this.m_DiscsActivated = true;
			this.m_State = VonNeumannBerserkerStates.SEEK;
		}
	}
}
