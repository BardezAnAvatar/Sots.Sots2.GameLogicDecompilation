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
	internal class SpecterCombatAIControl : CombatAIController
	{
		private App m_Game;
		private Ship m_Spector;
		private IGameObject m_CurrentTarget;
		private int m_RefreshTarget;
		private SimpleAIStates m_State;
		public override Ship GetShip()
		{
			return this.m_Spector;
		}
		public override void SetTarget(IGameObject target)
		{
			this.m_CurrentTarget = target;
		}
		public override IGameObject GetTarget()
		{
			return this.m_CurrentTarget;
		}
		public SpecterCombatAIControl(App game, Ship ship)
		{
			this.m_Game = game;
			this.m_Spector = ship;
		}
		public override void Initialize()
		{
			this.m_CurrentTarget = null;
			this.m_RefreshTarget = 200;
			this.m_State = SimpleAIStates.SEEK;
			SpectreGlobalData globalSpectreData = this.m_Game.Game.AssetDatabase.GlobalSpectreData;
			SpectreGlobalData.SpectreSize spectreSize = SpectreGlobalData.SpectreSize.Small;
			if (this.m_Game.Game.ScriptModules.Spectre != null)
			{
				if (this.m_Spector.DesignID == this.m_Game.Game.ScriptModules.Spectre.SmallDesignId)
				{
					spectreSize = SpectreGlobalData.SpectreSize.Small;
				}
				else
				{
					if (this.m_Spector.DesignID == this.m_Game.Game.ScriptModules.Spectre.MediumDesignId)
					{
						spectreSize = SpectreGlobalData.SpectreSize.Medium;
					}
					else
					{
						if (this.m_Spector.DesignID == this.m_Game.Game.ScriptModules.Spectre.BigDesignId)
						{
							spectreSize = SpectreGlobalData.SpectreSize.Large;
						}
					}
				}
			}
			this.m_Spector.Maneuvering.PostSetProp("SetCombatAIDamage", new object[]
			{
				globalSpectreData.Damage[(int)spectreSize].Crew,
				globalSpectreData.Damage[(int)spectreSize].Population,
				globalSpectreData.Damage[(int)spectreSize].InfraDamage,
				globalSpectreData.Damage[(int)spectreSize].TeraDamage
			});
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
		}
		public override void OnThink()
		{
			if (this.m_Spector == null)
			{
				return;
			}
			switch (this.m_State)
			{
			case SimpleAIStates.SEEK:
				this.ThinkSeek();
				return;
			case SimpleAIStates.TRACK:
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
			return false;
		}
		public override bool RequestingNewTarget()
		{
			return this.m_State == SimpleAIStates.SEEK && this.m_CurrentTarget == null;
		}
		public override void FindNewTarget(IEnumerable<IGameObject> objs)
		{
			this.m_CurrentTarget = null;
			float num = 1E+15f;
			List<StellarBody> list = new List<StellarBody>();
			List<Ship> list2 = new List<Ship>();
			List<Ship> list3 = new List<Ship>();
			List<Ship> list4 = new List<Ship>();
			bool flag = this.HasTargetInRange(objs);
			foreach (IGameObject current in objs)
			{
				if (current != this.m_Spector)
				{
					if (current is Ship)
					{
						Ship ship = current as Ship;
						if (ship.Player.ID == this.m_Spector.Player.ID)
						{
							list4.Add(ship);
						}
						else
						{
							if (Ship.IsActiveShip(ship) && !Ship.IsBattleRiderSize(ship.RealShipClass) && !(ship.Faction.Name == "loa") && (!flag || ship.IsDetected(this.m_Spector.Player)))
							{
								list2.Add(ship);
							}
						}
					}
					else
					{
						if (current is StellarBody)
						{
							StellarBody stellarBody = current as StellarBody;
							if (stellarBody.Population > 0.0 && stellarBody.Parameters.ColonyPlayerID != this.m_Spector.Player.ID)
							{
								list.Add(stellarBody);
							}
						}
					}
				}
			}
			int num2 = 100;
			foreach (Ship targ in list2)
			{
				int numSpectersForTarget = SpecterCombatAIControl.GetNumSpectersForTarget(targ.ShipClass);
				int num3 = (
					from x in list4
					where x.Target == targ
					select x).Count<Ship>();
				if (num3 < numSpectersForTarget && num3 <= num2)
				{
					float lengthSquared = (this.m_Spector.Position - targ.Position).LengthSquared;
					if (lengthSquared < num || num3 < num2)
					{
						num = lengthSquared;
						this.m_CurrentTarget = targ;
						num2 = num3;
					}
				}
			}
			if (this.m_CurrentTarget == null)
			{
				num2 = 100;
				foreach (Ship targ in list3)
				{
					int numSpectersForTarget2 = SpecterCombatAIControl.GetNumSpectersForTarget(targ.ShipClass);
					int num4 = (
						from x in list4
						where x.Target == targ
						select x).Count<Ship>();
					if (num4 < numSpectersForTarget2 && num4 <= num2)
					{
						float lengthSquared2 = (this.m_Spector.Position - targ.Position).LengthSquared;
						if (lengthSquared2 < num || num4 < num2)
						{
							num = lengthSquared2;
							this.m_CurrentTarget = targ;
							num2 = num4;
						}
					}
				}
			}
			if (this.m_CurrentTarget == null)
			{
				foreach (StellarBody current2 in list)
				{
					float lengthSquared3 = (this.m_Spector.Position - current2.Parameters.Position).LengthSquared;
					if (lengthSquared3 < num)
					{
						num = lengthSquared3;
						this.m_CurrentTarget = current2;
					}
				}
			}
		}
		private bool HasTargetInRange(IEnumerable<IGameObject> objs)
		{
			bool result = false;
			foreach (IGameObject current in objs)
			{
				if (current != this.m_Spector)
				{
					if (current is Ship)
					{
						Ship ship = current as Ship;
						if (ship.Player != this.m_Spector.Player && Ship.IsActiveShip(ship) && !Ship.IsBattleRiderSize(ship.RealShipClass) && ship.IsDetected(this.m_Spector.Player))
						{
							result = true;
							break;
						}
					}
					else
					{
						if (current is StellarBody)
						{
							StellarBody stellarBody = current as StellarBody;
							if (stellarBody.Population > 0.0 && stellarBody.Parameters.ColonyPlayerID != this.m_Spector.Player.ID)
							{
								float num = stellarBody.Parameters.Radius + this.m_Spector.ShipSphere.radius + 15000f;
								if ((stellarBody.Parameters.Position - this.m_Spector.Position).LengthSquared < num * num)
								{
									result = true;
									break;
								}
							}
						}
					}
				}
			}
			return result;
		}
		public static int GetNumSpectersForTarget(ShipClass sc)
		{
			switch (sc)
			{
			case ShipClass.Cruiser:
				return 2;
			case ShipClass.Dreadnought:
				return 3;
			case ShipClass.Leviathan:
				return 4;
			case ShipClass.BattleRider:
				return 1;
			case ShipClass.Station:
				return 5;
			default:
				return 1;
			}
		}
		public override bool NeedsAParent()
		{
			return false;
		}
		public override void FindParent(IEnumerable<CombatAIController> controllers)
		{
		}
		private void ThinkSeek()
		{
			if (this.m_CurrentTarget != null)
			{
				if (this.m_CurrentTarget is Ship)
				{
					this.m_Spector.SetShipTarget(this.m_CurrentTarget.ObjectID, (this.m_CurrentTarget as Ship).ShipSphere.center, true, 0);
				}
				else
				{
					if (this.m_CurrentTarget is StellarBody)
					{
						this.m_Spector.SetShipTarget(this.m_CurrentTarget.ObjectID, Vector3.Zero, true, 0);
					}
				}
				this.m_RefreshTarget = 200;
				this.m_State = SimpleAIStates.TRACK;
			}
		}
		private void ThinkTrack()
		{
			if (this.m_CurrentTarget == null)
			{
				this.m_State = SimpleAIStates.SEEK;
				return;
			}
			bool flag = false;
			float num = 0f;
			if (this.m_CurrentTarget is Ship)
			{
				Ship ship = this.m_CurrentTarget as Ship;
				this.m_Spector.Maneuvering.PostAddGoal(ship.Position, -Vector3.UnitZ);
				if (!Ship.IsActiveShip(ship))
				{
					this.m_CurrentTarget = null;
				}
			}
			else
			{
				if (this.m_CurrentTarget is StellarBody)
				{
					this.m_Spector.Maneuvering.PostAddGoal((this.m_CurrentTarget as StellarBody).Parameters.Position, -Vector3.UnitZ);
					flag = true;
				}
			}
			if (num > 2.5E+07f || flag)
			{
				this.m_RefreshTarget--;
				if (this.m_RefreshTarget <= 0)
				{
					this.m_CurrentTarget = null;
				}
			}
		}
	}
}
