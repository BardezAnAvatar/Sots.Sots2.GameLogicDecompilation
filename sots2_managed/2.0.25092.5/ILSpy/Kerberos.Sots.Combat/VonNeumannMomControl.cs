using Kerberos.Sots.Data;
using Kerberos.Sots.Encounters;
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
	internal class VonNeumannMomControl : CombatAIController
	{
		private App m_Game;
		private Ship m_VonNeumannMom;
		private List<Ship> m_LoadingChilds;
		private List<VonNeumannChildControl> m_SpawnedChilds;
		private List<VonNeumannMomControl> m_SpawnedMoms;
		private List<VonNeumannChildSpawnLocations> m_SpawnLocations;
		private List<Ship> m_SpawnedDefensePlatforms;
		private List<Ship> m_SpawnedEnemies;
		private Ship m_LoadingMom;
		private IGameObject m_Target;
		private VonNeumannOrders m_Orders;
		private VonNeumannMomStates m_State;
		private int m_NumChildrenToMaintain;
		private int m_VonNeumannID;
		private int m_FleetID;
		private int m_RUStore;
		private bool m_Vanished;
		private bool m_TryFindParent;
		public bool Vanished
		{
			get
			{
				return this.m_Vanished;
			}
		}
		public VonNeumannMomStates State
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
		public static int CalcNumChildren(VonNeumannGlobalData vgd, int numSats, int numShips)
		{
			if (vgd == null || vgd.NumShipsPerChild <= 0 || vgd.NumSatelitesPerChild <= 0)
			{
				return 0;
			}
			return numSats / vgd.NumSatelitesPerChild + numShips / vgd.NumShipsPerChild;
		}
		public override Ship GetShip()
		{
			return this.m_VonNeumannMom;
		}
		public override void SetTarget(IGameObject target)
		{
			this.m_Target = target;
		}
		public override IGameObject GetTarget()
		{
			return this.m_Target;
		}
		public VonNeumannMomControl(App game, Ship ship, int fleetid)
		{
			this.m_Game = game;
			this.m_VonNeumannMom = ship;
			this.m_FleetID = fleetid;
		}
		public override void Initialize()
		{
			this.m_LoadingMom = null;
			this.m_LoadingChilds = new List<Ship>();
			this.m_SpawnedChilds = new List<VonNeumannChildControl>();
			this.m_SpawnedMoms = new List<VonNeumannMomControl>();
			this.m_SpawnLocations = new List<VonNeumannChildSpawnLocations>();
			this.m_SpawnedDefensePlatforms = new List<Ship>();
			this.m_SpawnedEnemies = new List<Ship>();
			this.m_State = VonNeumannMomStates.COLLECT;
			this.m_Orders = VonNeumannOrders.COLLECT;
			this.m_NumChildrenToMaintain = 10;
			this.m_Vanished = false;
			this.m_TryFindParent = true;
			this.m_VonNeumannID = 0;
			List<VonNeumannInfo> source = this.m_Game.GameDatabase.GetVonNeumannInfos().ToList<VonNeumannInfo>();
			VonNeumannInfo vonNeumannInfo = source.FirstOrDefault((VonNeumannInfo x) => x.FleetId == this.m_FleetID);
			if (vonNeumannInfo != null)
			{
				this.m_VonNeumannID = vonNeumannInfo.Id;
			}
			Matrix matrix = Matrix.CreateRotationYPR(this.m_VonNeumannMom.Maneuvering.Rotation);
			matrix.Position = this.m_VonNeumannMom.Maneuvering.Position + matrix.Forward * 500f;
			Vector3 arg_F6_0 = matrix.Right;
			int designId = VonNeumann.StaticShipDesigns[VonNeumann.VonNeumannShipDesigns.CollectorProbe].DesignId;
			int numChildrenToMaintain = this.m_NumChildrenToMaintain;
			float num = MathHelper.DegreesToRadians(360f / (float)numChildrenToMaintain);
			float num2 = num * 0.5f;
			float num3 = 1f;
			for (int i = 0; i < numChildrenToMaintain; i++)
			{
				float num4 = num * (float)((i % numChildrenToMaintain + 1) / 2) * num3 + num2;
				Vector3 dir = new Vector3((float)Math.Sin((double)num4), 0f, -(float)Math.Cos((double)num4));
				VonNeumannChildSpawnLocations item = new VonNeumannChildSpawnLocations(this.m_VonNeumannMom, dir, 300f);
				this.m_SpawnLocations.Add(item);
				num3 *= -1f;
			}
			int minChildrenToMaintain = this.m_Game.AssetDatabase.GlobalVonNeumannData.MinChildrenToMaintain;
			for (int j = 0; j < minChildrenToMaintain; j++)
			{
				VonNeumannChildSpawnLocations vonNeumannChildSpawnLocations = this.m_SpawnLocations.FirstOrDefault((VonNeumannChildSpawnLocations x) => x.CanSpawnAtLocation());
				if (vonNeumannChildSpawnLocations != null)
				{
					Matrix worldMat = matrix;
					worldMat.Position = vonNeumannChildSpawnLocations.GetSpawnLocation();
					Ship ship = CombatAIController.CreateNewShip(this.m_Game.Game, worldMat, designId, 0, this.m_VonNeumannMom.InputID, this.m_VonNeumannMom.Player.ObjectID);
					if (ship != null)
					{
						this.m_LoadingChilds.Add(ship);
						vonNeumannChildSpawnLocations.SpawnAtLocation(ship);
					}
				}
			}
			if (this.m_LoadingChilds.Count > 0)
			{
				this.m_State = VonNeumannMomStates.INTEGRATECHILD;
			}
		}
		public override void Terminate()
		{
			if (this.m_VonNeumannMom != null && !this.m_VonNeumannMom.IsDestroyed)
			{
				this.ThinkVanish();
			}
			this.m_SpawnedDefensePlatforms.Clear();
			this.m_SpawnedEnemies.Clear();
		}
		public override void ObjectRemoved(IGameObject obj)
		{
			foreach (VonNeumannMomControl current in this.m_SpawnedMoms)
			{
				if (current.GetShip() == obj)
				{
					this.m_SpawnedMoms.Remove(current);
					break;
				}
			}
			foreach (VonNeumannChildControl current2 in this.m_SpawnedChilds)
			{
				if (current2.GetShip() == obj)
				{
					this.m_SpawnedChilds.Remove(current2);
					break;
				}
			}
			if (this.m_Target == obj)
			{
				this.m_Target = null;
			}
			foreach (VonNeumannChildSpawnLocations current3 in this.m_SpawnLocations)
			{
				if (current3.GetChildInSpace() == obj)
				{
					current3.Clear();
				}
			}
			List<Ship> list = new List<Ship>();
			foreach (Ship current4 in this.m_SpawnedDefensePlatforms)
			{
				if (current4 == obj)
				{
					list.Add(current4);
				}
			}
			foreach (Ship current5 in this.m_SpawnedEnemies)
			{
				if (current5 == obj)
				{
					list.Add(current5);
				}
			}
			foreach (Ship current6 in list)
			{
				this.m_SpawnedDefensePlatforms.Remove(current6);
				this.m_SpawnedEnemies.Remove(current6);
			}
		}
		public void SubmitResources(int amount)
		{
			if (amount > 0)
			{
				this.m_RUStore += amount;
			}
		}
		public void AddChild(CombatAIController child)
		{
			if (child is VonNeumannChildControl)
			{
				foreach (Ship current in this.m_LoadingChilds)
				{
					if (current == child.GetShip())
					{
						this.m_LoadingChilds.Remove(current);
						break;
					}
				}
				this.m_SpawnedChilds.Add(child as VonNeumannChildControl);
			}
			else
			{
				if (child is VonNeumannMomControl)
				{
					if (child.GetShip() == this.m_LoadingMom)
					{
						this.m_LoadingMom = null;
					}
					this.m_SpawnedMoms.Add(child as VonNeumannMomControl);
				}
			}
			switch (this.m_State)
			{
			case VonNeumannMomStates.INITFLEE:
			case VonNeumannMomStates.FLEE:
			case VonNeumannMomStates.VANISH:
				child.ForceFlee();
				return;
			default:
				return;
			}
		}
		public bool IsThisMyMom(Ship ship)
		{
			return this.m_LoadingChilds.Any((Ship x) => x == ship) || this.m_LoadingMom == ship;
		}
		public override void OnThink()
		{
			if (this.m_VonNeumannMom == null)
			{
				return;
			}
			foreach (VonNeumannChildSpawnLocations current in this.m_SpawnLocations)
			{
				current.Update();
			}
			if (this.m_VonNeumannMom.IsDestroyed)
			{
				this.m_State = VonNeumannMomStates.INITFLEE;
			}
			else
			{
				this.ValidateChildren();
			}
			switch (this.m_State)
			{
			case VonNeumannMomStates.COLLECT:
				this.ThinkCollect();
				return;
			case VonNeumannMomStates.EMITCHILD:
				this.ThinkEmitChild();
				return;
			case VonNeumannMomStates.INTEGRATECHILD:
				this.ThinkIntegrateChild();
				return;
			case VonNeumannMomStates.EMITMOM:
				this.ThinkEmitMom();
				return;
			case VonNeumannMomStates.INTEGRATEMOM:
				this.ThinkIntegrateMom();
				return;
			case VonNeumannMomStates.INITFLEE:
				this.ThinkInitFlee();
				return;
			case VonNeumannMomStates.FLEE:
				this.ThinkFlee();
				return;
			case VonNeumannMomStates.VANISH:
				this.ThinkVanish();
				return;
			default:
				return;
			}
		}
		public override void ForceFlee()
		{
			if (this.m_State != VonNeumannMomStates.INITFLEE && this.m_State != VonNeumannMomStates.FLEE)
			{
				this.m_State = VonNeumannMomStates.INITFLEE;
			}
		}
		public override bool VictoryConditionIsMet()
		{
			return this.m_Vanished;
		}
		public override bool RequestingNewTarget()
		{
			bool result = false;
			if (this.m_State == VonNeumannMomStates.COLLECT)
			{
				foreach (VonNeumannChildControl current in this.m_SpawnedChilds)
				{
					if (current.RequestingNewTarget())
					{
						result = true;
						break;
					}
				}
			}
			return result;
		}
		public override void FindNewTarget(IEnumerable<IGameObject> objs)
		{
			this.m_SpawnedDefensePlatforms.Clear();
			this.m_SpawnedEnemies.Clear();
			List<Ship> list = new List<Ship>();
			List<StellarBody> list2 = new List<StellarBody>();
			foreach (IGameObject current in objs)
			{
				if (current is Ship)
				{
					Ship ship = current as Ship;
					if (ship.Active && Ship.IsActiveShip(ship) && ship.Player != this.m_VonNeumannMom.Player)
					{
						if (ship.RealShipClass == RealShipClasses.Platform)
						{
							this.m_SpawnedDefensePlatforms.Add(ship);
						}
						else
						{
							this.m_SpawnedEnemies.Add(ship);
						}
						bool flag = ship.IsDetected(this.m_VonNeumannMom.Player);
						bool flag2 = false;
						foreach (VonNeumannChildControl current2 in this.m_SpawnedChilds)
						{
							if (ship == current2.GetTarget())
							{
								flag2 = true;
								if (!flag)
								{
									current2.SetTarget(null);
									break;
								}
								break;
							}
						}
						if (flag && !flag2)
						{
							list.Add(ship);
						}
					}
				}
				else
				{
					if (current is StellarBody)
					{
						StellarBody stellarBody = current as StellarBody;
						if (stellarBody.Parameters.ColonyPlayerID != this.m_VonNeumannMom.Player.ID && stellarBody.Population > 0.0)
						{
							list2.Add(stellarBody);
						}
					}
				}
			}
			bool flag3 = list.Count == 0;
			foreach (VonNeumannChildControl current3 in this.m_SpawnedChilds)
			{
				if (current3.RequestingNewTarget())
				{
					Vector3 position = current3.GetShip().Position;
					float num = 3.40282347E+38f;
					if (list.Count > 0)
					{
						Ship ship2 = null;
						foreach (Ship current4 in list)
						{
							float lengthSquared = (current4.Position - position).LengthSquared;
							if (lengthSquared < num)
							{
								ship2 = current4;
								num = lengthSquared;
							}
						}
						if (ship2 != null)
						{
							current3.SetTarget(ship2);
							list.Remove(ship2);
						}
					}
					flag3 = (list.Count == 0);
				}
				if (flag3)
				{
					break;
				}
			}
			if (flag3)
			{
				StellarBody stellarBody2 = null;
				float num2 = 3.40282347E+38f;
				foreach (StellarBody current5 in list2)
				{
					float lengthSquared2 = (current5.Parameters.Position - this.m_VonNeumannMom.Position).LengthSquared;
					if (lengthSquared2 < num2)
					{
						stellarBody2 = current5;
						num2 = lengthSquared2;
					}
				}
				if (stellarBody2 != null)
				{
					foreach (VonNeumannChildControl current6 in this.m_SpawnedChilds)
					{
						if (current6.RequestingNewTarget())
						{
							current6.SetTarget(stellarBody2);
						}
					}
				}
			}
		}
		public override bool NeedsAParent()
		{
			return this.m_TryFindParent;
		}
		public override void FindParent(IEnumerable<CombatAIController> controllers)
		{
			foreach (CombatAIController current in controllers)
			{
				if (current is VonNeumannMomControl)
				{
					VonNeumannMomControl vonNeumannMomControl = current as VonNeumannMomControl;
					if (vonNeumannMomControl.IsThisMyMom(this.m_VonNeumannMom))
					{
						vonNeumannMomControl.AddChild(this);
						break;
					}
				}
			}
			this.m_TryFindParent = false;
		}
		public int GetStoredResources()
		{
			return this.m_RUStore;
		}
		private int GetNumChildrenToMaintain()
		{
			int count = this.m_SpawnedDefensePlatforms.Count;
			int count2 = this.m_SpawnedEnemies.Count;
			return VonNeumannMomControl.CalcNumChildren(this.m_Game.AssetDatabase.GlobalVonNeumannData, count, count2);
		}
		private void ValidateChildren()
		{
			List<VonNeumannChildControl> list = new List<VonNeumannChildControl>();
			foreach (VonNeumannChildControl current in this.m_SpawnedChilds)
			{
				if (current.GetShip() == null || current.GetShip().IsDestroyed)
				{
					list.Add(current);
				}
			}
			foreach (VonNeumannChildControl current2 in list)
			{
				this.m_SpawnedChilds.Remove(current2);
			}
		}
		private void ThinkCollect()
		{
			if (this.m_RUStore >= this.m_Game.AssetDatabase.GlobalVonNeumannData.MomRUCost)
			{
				switch (this.m_Orders)
				{
				case VonNeumannOrders.COLLECT:
					this.m_State = VonNeumannMomStates.INITFLEE;
					return;
				}
				this.m_State = VonNeumannMomStates.EMITMOM;
				return;
			}
			if (this.m_RUStore >= this.m_Game.AssetDatabase.GlobalVonNeumannData.ChildRUCost)
			{
				if (this.m_SpawnedChilds.Count < this.m_NumChildrenToMaintain)
				{
					this.m_State = VonNeumannMomStates.EMITCHILD;
					return;
				}
			}
			else
			{
				if (this.m_SpawnedChilds.Count < this.m_Game.AssetDatabase.GlobalVonNeumannData.MinChildrenToMaintain && this.m_LoadingChilds.Count == 0)
				{
					this.m_RUStore += this.m_Game.AssetDatabase.GlobalVonNeumannData.ChildRUCost;
					this.m_State = VonNeumannMomStates.EMITCHILD;
				}
			}
		}
		private void ThinkEmitChild()
		{
			VonNeumannChildSpawnLocations vonNeumannChildSpawnLocations = this.m_SpawnLocations.FirstOrDefault((VonNeumannChildSpawnLocations x) => x.CanSpawnAtLocation());
			if (vonNeumannChildSpawnLocations == null)
			{
				return;
			}
			Matrix worldMat = Matrix.CreateRotationYPR(this.m_VonNeumannMom.Maneuvering.Rotation);
			worldMat.Position = vonNeumannChildSpawnLocations.GetSpawnLocation();
			Ship ship = CombatAIController.CreateNewShip(this.m_Game.Game, worldMat, VonNeumann.StaticShipDesigns[VonNeumann.VonNeumannShipDesigns.CollectorProbe].DesignId, 0, this.m_VonNeumannMom.InputID, this.m_VonNeumannMom.Player.ObjectID);
			ship.Maneuvering.RetreatDestination = this.m_VonNeumannMom.Maneuvering.RetreatDestination;
			if (ship == null)
			{
				this.m_State = VonNeumannMomStates.COLLECT;
				return;
			}
			vonNeumannChildSpawnLocations.SpawnAtLocation(ship);
			this.m_RUStore -= this.m_Game.AssetDatabase.GlobalVonNeumannData.ChildRUCost;
			this.m_State = VonNeumannMomStates.INTEGRATECHILD;
			this.m_LoadingChilds.Add(ship);
		}
		private void ThinkIntegrateChild()
		{
			if (this.m_LoadingChilds.Count == 0)
			{
				this.m_State = VonNeumannMomStates.COLLECT;
				return;
			}
			bool flag = true;
			foreach (Ship current in this.m_LoadingChilds)
			{
				if (current.ObjectStatus != GameObjectStatus.Ready)
				{
					flag = false;
				}
			}
			if (flag)
			{
				foreach (Ship current2 in this.m_LoadingChilds)
				{
					current2.Player = this.m_VonNeumannMom.Player;
					current2.Active = true;
					this.m_Game.CurrentState.AddGameObject(current2, false);
				}
				this.m_State = VonNeumannMomStates.COLLECT;
			}
		}
		private void ThinkEmitMom()
		{
			this.m_RUStore -= this.m_Game.AssetDatabase.GlobalVonNeumannData.MomRUCost;
			Matrix worldMat = Matrix.CreateRotationYPR(this.m_VonNeumannMom.Maneuvering.Rotation);
			worldMat.Position = this.m_VonNeumannMom.Maneuvering.Position + worldMat.Forward * 500f;
			Ship ship = CombatAIController.CreateNewShip(this.m_Game.Game, worldMat, VonNeumann.StaticShipDesigns[VonNeumann.VonNeumannShipDesigns.CollectorMothership].DesignId, 0, this.m_VonNeumannMom.InputID, this.m_VonNeumannMom.Player.ObjectID);
			if (ship == null)
			{
				this.m_State = VonNeumannMomStates.COLLECT;
				return;
			}
			this.m_RUStore -= this.m_Game.AssetDatabase.GlobalVonNeumannData.ChildRUCost;
			this.m_State = VonNeumannMomStates.INTEGRATEMOM;
			this.m_LoadingMom = ship;
		}
		private void ThinkIntegrateMom()
		{
			if (this.m_LoadingMom == null)
			{
				this.m_State = VonNeumannMomStates.COLLECT;
				return;
			}
			if (this.m_LoadingMom.ObjectStatus == GameObjectStatus.Ready)
			{
				this.m_LoadingMom.Active = true;
				this.m_LoadingMom.Player = this.m_VonNeumannMom.Player;
				this.m_Game.CurrentState.AddGameObject(this.m_LoadingMom, false);
				this.m_State = VonNeumannMomStates.INITFLEE;
			}
		}
		private void ThinkInitFlee()
		{
			this.m_State = VonNeumannMomStates.FLEE;
			foreach (VonNeumannChildControl current in this.m_SpawnedChilds)
			{
				current.ForceFlee();
			}
			foreach (VonNeumannMomControl current2 in this.m_SpawnedMoms)
			{
				current2.ForceFlee();
			}
		}
		private void ThinkFlee()
		{
			if (this.m_VonNeumannMom.CombatStance != CombatStance.RETREAT)
			{
				this.m_VonNeumannMom.SetCombatStance(CombatStance.RETREAT);
			}
			if (this.m_VonNeumannMom.HasRetreated)
			{
				this.m_State = VonNeumannMomStates.VANISH;
			}
		}
		private void ThinkVanish()
		{
			if (!this.m_Vanished)
			{
				int num = this.m_RUStore;
				foreach (VonNeumannChildControl current in this.m_SpawnedChilds)
				{
					num += current.GetStoredResources();
				}
				VonNeumann.HandleMomRetreated(this.m_Game.Game, this.m_VonNeumannID, num);
				this.m_VonNeumannMom.Active = false;
				this.m_Vanished = true;
			}
		}
	}
}
