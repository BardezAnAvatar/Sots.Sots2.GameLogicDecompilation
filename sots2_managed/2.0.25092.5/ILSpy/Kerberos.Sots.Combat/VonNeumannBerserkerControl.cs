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
	internal class VonNeumannBerserkerControl : VonNeumannNeoBerserkerControl
	{
		private const int MAX_PYRAMIDS = 14;
		private SpinningWheelFormation m_PyramidFormation;
		private List<Ship> m_LoadingPyramids;
		private List<VonNeumannPyramidControl> m_Pyramids;
		private bool m_FormationInitialized;
		private bool m_PyramidsLaunched;
		public VonNeumannBerserkerControl(App game, Ship ship) : base(game, ship)
		{
		}
		public override void Initialize()
		{
			base.Initialize();
			this.m_FormationInitialized = false;
			this.m_LoadingPyramids = new List<Ship>();
			this.m_Pyramids = new List<VonNeumannPyramidControl>();
			Matrix worldMat = Matrix.CreateRotationYPR(this.m_VonNeumannBerserker.Maneuvering.Rotation);
			worldMat.Position = this.m_VonNeumannBerserker.Maneuvering.Position;
			int designId = VonNeumann.StaticShipDesigns[VonNeumann.VonNeumannShipDesigns.Pyramid].DesignId;
			for (int i = 0; i < 14; i++)
			{
				Ship ship = CombatAIController.CreateNewShip(this.m_Game.Game, worldMat, designId, this.m_VonNeumannBerserker.ObjectID, this.m_VonNeumannBerserker.InputID, this.m_VonNeumannBerserker.Player.ObjectID);
				if (ship != null)
				{
					this.m_LoadingPyramids.Add(ship);
				}
			}
			this.m_PyramidsLaunched = false;
			this.m_State = VonNeumannBerserkerStates.INTEGRATECHILDS;
		}
		private void InitPyramidFormation()
		{
			if (this.m_FormationInitialized)
			{
				return;
			}
			List<object> list = new List<object>();
			list.Add(Vector3.Zero);
			list.Add(-Vector3.UnitZ);
			list.Add(this.m_Pyramids.Count);
			Matrix m = Matrix.CreateRotationYPR(this.m_VonNeumannBerserker.Maneuvering.Rotation);
			Vector3 pyramidsCOM = this.GetPyramidsCOM();
			m.Position = pyramidsCOM;
			Matrix mat = Matrix.Inverse(m);
			foreach (VonNeumannPyramidControl current in this.m_Pyramids)
			{
				Ship ship = current.GetShip();
				list.Add(ship.ObjectID);
				Vector3 vector = Vector3.Transform(ship.Position, mat);
				list.Add(vector);
			}
			this.m_PyramidFormation = this.m_Game.AddObject<SpinningWheelFormation>(list.ToArray());
			this.m_FormationInitialized = true;
		}
		private Vector3 GetPyramidsCOM()
		{
			if (this.m_Pyramids.Count == 0)
			{
				return this.m_VonNeumannBerserker.Maneuvering.Position;
			}
			Vector3 com = Vector3.Zero;
			this.m_Pyramids.ForEach(delegate(VonNeumannPyramidControl x)
			{
				com += x.GetShip().Position;
			});
			return com / (float)this.m_Pyramids.Count;
		}
		public override void Terminate()
		{
			if (this.m_PyramidFormation != null)
			{
				this.m_Game.ReleaseObject(this.m_PyramidFormation);
			}
		}
		public override void ObjectRemoved(IGameObject obj)
		{
			base.ObjectRemoved(obj);
			foreach (VonNeumannPyramidControl current in this.m_Pyramids)
			{
				if (current.GetShip() == obj)
				{
					this.m_Pyramids.Remove(current);
					break;
				}
			}
			foreach (Ship current2 in this.m_LoadingPyramids)
			{
				if (current2 == obj)
				{
					this.m_LoadingPyramids.Remove(current2);
					break;
				}
			}
		}
		public override bool IsThisMyMom(Ship ship)
		{
			return this.m_LoadingPyramids.Any((Ship x) => x == ship) || base.IsThisMyMom(ship);
		}
		public override void AddChild(CombatAIController child)
		{
			base.AddChild(child);
			if (child is VonNeumannPyramidControl)
			{
				foreach (Ship current in this.m_LoadingPyramids)
				{
					if (current == child.GetShip())
					{
						this.m_LoadingPyramids.Remove(current);
						break;
					}
				}
				this.m_Pyramids.Add(child as VonNeumannPyramidControl);
			}
		}
		public override void OnThink()
		{
			if (this.m_VonNeumannBerserker == null)
			{
				return;
			}
			switch (this.m_State)
			{
			case VonNeumannBerserkerStates.INTEGRATECHILDS:
				this.ThinkIntegrateChild();
				return;
			case VonNeumannBerserkerStates.SEEK:
				this.ThinkSeek();
				return;
			case VonNeumannBerserkerStates.TRACK:
				this.ThinkTrack();
				return;
			case VonNeumannBerserkerStates.BOMBARD:
				this.ThinkBombard();
				return;
			case VonNeumannBerserkerStates.LAUNCHING:
				this.ThinkLaunching();
				return;
			case VonNeumannBerserkerStates.ACTIVATINGDISCS:
				this.ThinkActivatingDiscs();
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
			return this.m_Target == null && this.m_State == VonNeumannBerserkerStates.SEEK;
		}
		public override void FindNewTarget(IEnumerable<IGameObject> objs)
		{
			base.FindNewTarget(objs);
			float num = 3.40282347E+38f;
			IGameObject target = null;
			foreach (IGameObject current in objs)
			{
				if (current is StellarBody)
				{
					StellarBody stellarBody = current as StellarBody;
					if (stellarBody.Parameters.ColonyPlayerID != this.m_VonNeumannBerserker.Player.ID)
					{
						float lengthSquared = (stellarBody.Parameters.Position - this.m_VonNeumannBerserker.Position).LengthSquared;
						if (lengthSquared < num)
						{
							target = stellarBody;
							num = lengthSquared;
						}
					}
				}
			}
			this.SetTarget(target);
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
			if (this.m_LoadingDiscs.Count == 0 && this.m_LoadingPyramids.Count == 0)
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
			foreach (Ship current2 in this.m_LoadingPyramids)
			{
				if (current2.ObjectStatus != GameObjectStatus.Ready)
				{
					flag = false;
				}
			}
			if (flag)
			{
				foreach (Ship current3 in this.m_LoadingDiscs)
				{
					current3.Player = this.m_VonNeumannBerserker.Player;
					current3.Active = false;
					this.m_Game.CurrentState.AddGameObject(current3, false);
				}
				foreach (Ship current4 in this.m_LoadingPyramids)
				{
					current4.Player = this.m_VonNeumannBerserker.Player;
					current4.Active = true;
					this.m_Game.CurrentState.AddGameObject(current4, false);
				}
				this.m_State = VonNeumannBerserkerStates.SEEK;
			}
		}
		protected override void ThinkTrack()
		{
			base.ThinkTrack();
			if (this.m_Target != null)
			{
				Vector3 vector = this.m_VonNeumannBerserker.Position - this.m_Target.Parameters.Position;
				vector.Normalize();
				Vector3 vector2 = this.m_Target.Parameters.Position + vector * (this.m_Target.Parameters.Radius + 750f + 1000f);
				this.m_VonNeumannBerserker.Maneuvering.PostAddGoal(vector2, -vector);
				if ((this.m_VonNeumannBerserker.Position - vector2).LengthSquared < 100f)
				{
					if (!this.m_DiscsActivated)
					{
						this.m_State = VonNeumannBerserkerStates.ACTIVATINGDISCS;
						return;
					}
					if (!this.m_PyramidsLaunched)
					{
						this.m_State = VonNeumannBerserkerStates.BOMBARD;
					}
				}
			}
		}
		private void ThinkBombard()
		{
			if (this.m_Target == null || this.m_Target == null)
			{
				this.m_State = VonNeumannBerserkerStates.SEEK;
				return;
			}
			this.m_VonNeumannBerserker.PostSetProp("LaunchBattleriders", new object[0]);
			this.m_State = VonNeumannBerserkerStates.LAUNCHING;
		}
		private void ThinkLaunching()
		{
			bool flag = true;
			foreach (VonNeumannPyramidControl current in this.m_Pyramids)
			{
				Ship ship = current.GetShip();
				if (ship.DockedWithParent)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				this.InitPyramidFormation();
				if (this.m_PyramidFormation.ObjectStatus == GameObjectStatus.Ready)
				{
					foreach (VonNeumannPyramidControl current2 in this.m_Pyramids)
					{
						current2.SetTarget(this.m_Target);
					}
					Vector3 facing = this.m_Target.Parameters.Position - this.m_VonNeumannBerserker.Position;
					facing.Normalize();
					this.m_PyramidFormation.PostFormationDefinition(this.GetPyramidsCOM(), facing, Vector3.Zero);
					this.m_PyramidFormation.PostSetActive(true);
					this.m_PyramidsLaunched = true;
					this.m_State = VonNeumannBerserkerStates.SEEK;
				}
			}
		}
	}
}
