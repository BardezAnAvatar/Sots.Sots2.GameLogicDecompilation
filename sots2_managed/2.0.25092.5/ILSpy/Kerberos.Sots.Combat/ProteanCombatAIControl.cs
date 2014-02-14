using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Combat
{
	internal class ProteanCombatAIControl : CombatAIController
	{
		private App m_Game;
		private Ship m_Protean;
		private Ship m_Gardener;
		private StellarBody m_Home;
		private IGameObject m_CurrentTarget;
		private int m_FindNewTargDelay;
		private ProteanCombatStates m_State;
		private Vector3 m_LastGardenerPos;
		private float m_GardenerRadius;
		private bool m_HasGardener;
		private bool m_Victory;
		public override Ship GetShip()
		{
			return this.m_Protean;
		}
		public override void SetTarget(IGameObject target)
		{
			if (target != this.m_CurrentTarget)
			{
				int num = 0;
				CombatStance combatStance = CombatStance.NO_STANCE;
				float num2 = 0f;
				if (target != null)
				{
					num = target.ObjectID;
					combatStance = CombatStance.PURSUE;
					num2 = Math.Max(CombatAI.GetMinEffectiveWeaponRange(this.m_Protean, false), 500f);
				}
				this.m_Protean.SetCombatStance(combatStance);
				this.m_Protean.SetShipTarget(num, Vector3.Zero, true, 0);
				this.m_Protean.Maneuvering.PostSetProp("SetStanceTarget", new object[]
				{
					num,
					num2
				});
			}
			this.m_CurrentTarget = target;
		}
		public override IGameObject GetTarget()
		{
			return this.m_CurrentTarget;
		}
		public ProteanCombatAIControl(App game, Ship ship)
		{
			this.m_Game = game;
			this.m_Protean = ship;
		}
		public override void Initialize()
		{
			this.m_CurrentTarget = null;
			this.m_Gardener = null;
			this.m_State = ProteanCombatStates.INIT;
			this.m_FindNewTargDelay = 300;
			this.m_Gardener = null;
			this.m_HasGardener = false;
			this.m_LastGardenerPos = Vector3.Zero;
			this.m_GardenerRadius = 0f;
			this.m_Victory = false;
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
			if (obj == this.m_Home)
			{
				this.m_Home = null;
			}
			if (obj == this.m_Gardener)
			{
				this.m_Gardener = null;
			}
		}
		public override void OnThink()
		{
			if (this.m_Protean == null)
			{
				return;
			}
			switch (this.m_State)
			{
			case ProteanCombatStates.INIT:
				this.ThinkInit();
				return;
			case ProteanCombatStates.SEEK:
				this.ThinkSeek();
				return;
			case ProteanCombatStates.TRACK:
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
			return this.m_Victory;
		}
		public override bool RequestingNewTarget()
		{
			return this.m_State == ProteanCombatStates.INIT || this.m_CurrentTarget == null;
		}
		public override void FindNewTarget(IEnumerable<IGameObject> objs)
		{
			if (this.m_Protean == null)
			{
				return;
			}
			float num = this.m_Protean.SensorRange + 10000f;
			if (this.m_Home != null)
			{
				num += this.m_Home.Parameters.Radius;
			}
			IGameObject gameObject = null;
			List<StellarBody> list = new List<StellarBody>();
			List<Ship> list2 = new List<Ship>();
			List<Ship> list3 = new List<Ship>();
			bool flag = false;
			foreach (IGameObject current in objs)
			{
				if (current is StellarBody)
				{
					StellarBody stellarBody = current as StellarBody;
					list.Add(stellarBody);
					if (stellarBody.Population > 0.0 && stellarBody.Parameters.ColonyPlayerID != this.m_Protean.Player.ID)
					{
						flag = true;
					}
				}
				else
				{
					if (current is Kerberos.Sots.GameStates.StarSystem)
					{
						Kerberos.Sots.GameStates.StarSystem starSystem = current as Kerberos.Sots.GameStates.StarSystem;
						using (IEnumerator<IGameObject> enumerator2 = starSystem.Crits.Objects.GetEnumerator())
						{
							while (enumerator2.MoveNext())
							{
								IGameObject current2 = enumerator2.Current;
								if (current2 is StellarBody)
								{
									list.Add(current2 as StellarBody);
								}
							}
							continue;
						}
					}
					if (current is Ship)
					{
						Ship ship = current as Ship;
						if (this.m_Gardener == null && ship.IsGardener)
						{
							this.m_Gardener = ship;
							this.m_GardenerRadius = ship.ShipSphere.radius;
							this.m_HasGardener = true;
						}
						if ((this.m_Home != null || this.m_Gardener != null || this.m_HasGardener) && ship != this.m_Protean && Ship.IsActiveShip(ship))
						{
							if (this.m_Gardener == ship)
							{
								this.m_LastGardenerPos = ship.Position;
							}
							else
							{
								if (ship.Player == this.m_Protean.Player)
								{
									list3.Add(ship);
								}
								else
								{
									flag = true;
									if (ship.IsDetected(this.m_Protean.Player))
									{
										Vector3 v = this.m_HasGardener ? this.m_LastGardenerPos : this.m_Home.Parameters.Position;
										float lengthSquared = (v - ship.Position).LengthSquared;
										if (lengthSquared <= num * num)
										{
											list2.Add(ship);
										}
									}
								}
							}
						}
					}
				}
			}
			if (flag)
			{
				float num2 = 3.40282347E+38f;
				foreach (Ship enemy in list2)
				{
					if (!list3.Any((Ship x) => x.Target == enemy))
					{
						float lengthSquared2 = (this.m_Protean.Position - enemy.Position).LengthSquared;
						if (lengthSquared2 < num2)
						{
							num2 = lengthSquared2;
							gameObject = enemy;
						}
					}
				}
				if (gameObject == null)
				{
					int num3 = -1;
					num2 = 3.40282347E+38f;
					foreach (Ship enemy in list2)
					{
						List<Ship> list4 = (
							from x in list3
							where x.Target == enemy
							select x).ToList<Ship>();
						float lengthSquared3 = (this.m_Protean.Position - enemy.Position).LengthSquared;
						if ((lengthSquared3 < num2 && list4.Count == num3) || list4.Count < num3 || num3 < 0)
						{
							num2 = lengthSquared3;
							gameObject = enemy;
							num3 = list4.Count;
						}
					}
				}
				if (gameObject != null || !this.m_HasGardener)
				{
					goto IL_475;
				}
				num2 = 3.40282347E+38f;
				using (List<StellarBody>.Enumerator enumerator5 = list.GetEnumerator())
				{
					while (enumerator5.MoveNext())
					{
						StellarBody current3 = enumerator5.Current;
						if (current3.Population > 0.0 && current3.Parameters.ColonyPlayerID != this.m_Protean.Player.ID)
						{
							float lengthSquared4 = (this.m_Protean.Position - current3.Parameters.Position).LengthSquared;
							if (lengthSquared4 < num2)
							{
								num2 = lengthSquared4;
								gameObject = current3;
							}
						}
					}
					goto IL_475;
				}
			}
			if (this.m_HasGardener)
			{
				this.m_Victory = true;
			}
			IL_475:
			if (this.m_Home == null && !this.m_HasGardener)
			{
				float num4 = 3.40282347E+38f;
				foreach (StellarBody current4 in list)
				{
					float lengthSquared5 = (this.m_Protean.Position - current4.Parameters.Position).LengthSquared;
					if (lengthSquared5 < num4)
					{
						num4 = lengthSquared5;
						this.m_Home = current4;
					}
				}
				if (this.m_Home != null)
				{
					this.m_Protean.Maneuvering.PostAddGoal(this.m_Home.Parameters.Position, -Vector3.UnitZ);
				}
			}
			this.SetTarget(gameObject);
		}
		public override bool NeedsAParent()
		{
			return false;
		}
		public override void FindParent(IEnumerable<CombatAIController> controllers)
		{
		}
		private void ThinkInit()
		{
			if (this.m_Home != null)
			{
				this.m_Protean.Maneuvering.PostAddGoal(this.m_Home.Parameters.Position, -Vector3.UnitZ);
				this.m_State = ProteanCombatStates.SEEK;
			}
		}
		private void ThinkSeek()
		{
			if (this.m_CurrentTarget != null)
			{
				this.m_FindNewTargDelay = 300;
				this.m_State = ProteanCombatStates.TRACK;
			}
		}
		private void ThinkTrack()
		{
			this.m_FindNewTargDelay--;
			if (this.m_CurrentTarget == null || (this.m_Home == null && (this.m_Gardener == null || !Ship.IsActiveShip(this.m_Gardener))) || this.m_FindNewTargDelay <= 0)
			{
				this.SetTarget(null);
				this.m_State = ProteanCombatStates.SEEK;
				return;
			}
			Ship ship = this.m_CurrentTarget as Ship;
			if (ship != null)
			{
				Vector3 v = this.m_HasGardener ? this.m_LastGardenerPos : this.m_Home.Parameters.Position;
				float num = this.m_HasGardener ? this.m_GardenerRadius : this.m_Home.Parameters.Radius;
				float lengthSquared = (v - ship.Position).LengthSquared;
				float num2 = num + this.m_Protean.SensorRange + 20000f;
				if (!Ship.IsActiveShip(ship) || lengthSquared > num2 * num2)
				{
					this.SetTarget(null);
					this.m_State = ProteanCombatStates.SEEK;
					this.m_Protean.Maneuvering.PostAddGoal(this.m_Home.Parameters.Position, -Vector3.UnitZ);
				}
			}
		}
	}
}
