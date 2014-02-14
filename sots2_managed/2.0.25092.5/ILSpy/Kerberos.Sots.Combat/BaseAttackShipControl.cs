using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Combat
{
	internal class BaseAttackShipControl : TaskGroupShipControl
	{
		private static int kUpdateTargetSelection = 180;
		private static int kUpdateClearTarget = 3;
		private int m_UpdateTargetSelection;
		private int m_ClearTargetsUpdate;
		public BaseAttackShipControl(App game, TacticalObjective to, CombatAI commanderAI) : base(game, to, commanderAI)
		{
			this.m_ClearTargetsUpdate = 0;
			this.m_UpdateTargetSelection = 0;
			this.m_CanMerge = false;
		}
		public override void Update(int framesElapsed)
		{
			this.m_UpdateTargetSelection -= framesElapsed;
			if (this.m_UpdateTargetSelection <= 0)
			{
				this.UpdateTargetSelection();
				this.m_UpdateTargetSelection = BaseAttackShipControl.kUpdateTargetSelection;
			}
			bool flag = true;
			if (this.m_TaskGroupObjective is AttackGroupObjective && this.m_TaskGroupObjective.m_TargetEnemyGroup != null)
			{
				float num = TaskGroup.ATTACK_GROUP_RANGE + this.m_TaskGroupObjective.m_TargetEnemyGroup.GetGroupRadius();
				Vector3 vector = base.GetCurrentPosition() - this.m_TaskGroupObjective.m_TargetEnemyGroup.m_LastKnownPosition;
				float length = vector.Length;
				if (length > 0f && this.m_Ships.Count > 0)
				{
					vector /= length;
					Vector3 value = this.m_TaskGroupObjective.m_TargetEnemyGroup.m_LastKnownDestination - this.m_TaskGroupObjective.m_TargetEnemyGroup.m_LastKnownPosition;
					if (length > num && value.LengthSquared > 2.5E+07f && Vector3.Dot(vector, Vector3.Normalize(value)) < 0.9f)
					{
						float num2 = 0f;
						foreach (Ship current in this.m_Ships)
						{
							num2 += current.Maneuvering.MaxShipSpeed;
						}
						num2 /= (float)this.m_Ships.Count;
						float num3 = num2 * 0.8f;
						float s = (this.m_TaskGroupObjective.m_TargetEnemyGroup.m_LastKnownHeading.LengthSquared > num3 * num3) ? 0.75f : 0.5f;
						Vector3 vector2 = (this.m_TaskGroupObjective.m_TargetEnemyGroup.m_LastKnownPosition + this.m_TaskGroupObjective.m_TargetEnemyGroup.m_LastKnownDestination) * s;
						vector2.Y = 0f;
						Vector3 destFacing = vector2 - base.GetCurrentPosition();
						destFacing.Y = 0f;
						destFacing.Normalize();
						foreach (Ship current2 in this.m_Ships)
						{
							float lengthSquared = (current2.Position - this.m_TaskGroupObjective.m_TargetEnemyGroup.m_LastKnownPosition).LengthSquared;
							float num4 = 16000f;
							if (current2.Maneuvering.SpeedState == ShipSpeedState.Overthrust)
							{
								num4 -= 14000f;
								if (lengthSquared < num4 * num4)
								{
									base.SetShipSpeed(current2, ShipSpeedState.Normal);
								}
							}
							else
							{
								if (lengthSquared > num4 * num4)
								{
									base.SetShipSpeed(current2, ShipSpeedState.Overthrust);
								}
							}
						}
						base.SetFUP(vector2, destFacing);
						flag = false;
					}
					else
					{
						foreach (Ship current3 in this.m_Ships)
						{
							base.SetShipSpeed(current3, ShipSpeedState.Normal);
						}
					}
				}
			}
			foreach (Ship current4 in this.m_Ships)
			{
				if (current4.WeaponControlsIsInitilized)
				{
					if (current4.WeaponControls.Any((SpecWeaponControl x) => x.RequestHoldShip()))
					{
						flag = false;
						break;
					}
				}
			}
			if (flag)
			{
				this.OnAttackUpdate(framesElapsed);
			}
		}
		private void UpdateTargetSelection()
		{
			List<Ship> list = new List<Ship>();
			if (this.m_TaskGroupObjective.m_TargetEnemyGroup != null)
			{
				foreach (Ship current in this.m_TaskGroupObjective.m_TargetEnemyGroup.m_Ships)
				{
					if (Ship.IsActiveShip(current) && current.IsDetected(this.m_CommanderAI.m_Player))
					{
						list.Add(current);
					}
				}
			}
			bool flag = false;
			if (list.Count > 0)
			{
				this.m_ClearTargetsUpdate--;
				if (this.m_ClearTargetsUpdate <= 0)
				{
					flag = true;
					this.m_ClearTargetsUpdate = BaseAttackShipControl.kUpdateClearTarget;
				}
			}
			else
			{
				this.m_ClearTargetsUpdate = BaseAttackShipControl.kUpdateClearTarget;
			}
			List<Ship> list2 = new List<Ship>();
			foreach (Ship current2 in this.m_Ships)
			{
				list2.Add(current2);
				if (current2.Target != null && current2.Target is Ship)
				{
					Ship s = current2.Target as Ship;
					if (!Ship.IsActiveShip(s) || !list.Any((Ship x) => x == s) || flag)
					{
						base.SetNewTarget(current2, null);
					}
				}
			}
			if (list2.Count == 0 || list.Count == 0)
			{
				return;
			}
			Vector3 currentPosition = base.GetCurrentPosition();
			ShipTargetComparision comparer = new ShipTargetComparision(this.m_CommanderAI, currentPosition);
			list.Sort(comparer);
			this.m_GroupPriorityTarget = null;
			foreach (Ship e in list)
			{
				if (!this.m_CommanderAI.GetTaskGroups().Any((TaskGroup x) => x.IsDesiredGroupTargetTaken(this, e)))
				{
					this.m_GroupPriorityTarget = e;
					break;
				}
			}
			if (this.m_GroupPriorityTarget == null)
			{
				this.m_GroupPriorityTarget = list.First<Ship>();
			}
			int num = this.m_CommanderAI.GetTargetShipScore(this.m_GroupPriorityTarget);
			while (num > 0 && list2.Count > 0)
			{
				Ship ship = null;
				int num2 = 0;
				float num3 = 3.40282347E+38f;
				foreach (Ship current3 in list2)
				{
					float lengthSquared = (current3.Maneuvering.Position - this.m_GroupPriorityTarget.Maneuvering.Position).LengthSquared;
					if (lengthSquared < num3)
					{
						num2 = CombatAI.GetShipStrength(this.m_GroupPriorityTarget);
						num3 = lengthSquared;
						ship = current3;
					}
				}
				if (ship == null)
				{
					break;
				}
				base.SetNewTarget(ship, this.m_GroupPriorityTarget);
				num -= num2;
				list2.Remove(ship);
			}
			if (list2.Count > 0)
			{
				foreach (Ship current4 in list2)
				{
					Ship ship2 = null;
					float num4 = 3.40282347E+38f;
					foreach (Ship current5 in list)
					{
						if (current5 != this.m_GroupPriorityTarget)
						{
							float lengthSquared2 = (current4.Maneuvering.Position - current5.Maneuvering.Position).LengthSquared;
							if (lengthSquared2 < num4)
							{
								num4 = lengthSquared2;
								ship2 = current5;
							}
						}
					}
					if (ship2 != null)
					{
						base.SetNewTarget(current4, ship2);
					}
					else
					{
						base.SetNewTarget(current4, this.m_GroupPriorityTarget);
					}
				}
			}
		}
		protected virtual void OnAttackUpdate(int framesElapsed)
		{
		}
		protected Vector3 GetFlyByPosition(Vector3 currentPos, Vector3 flybyCenter, float offsetDist, float deviationAngle = 0f)
		{
			Vector3 vector = flybyCenter - currentPos;
			if (vector.LengthSquared <= 1E-06f)
			{
				return currentPos;
			}
			vector.Y = 0f;
			vector.Normalize();
			Vector3 vector2 = flybyCenter + vector * offsetDist;
			StellarBody planetContainingPosition = this.m_CommanderAI.GetPlanetContainingPosition(vector2);
			if (planetContainingPosition == null)
			{
				return vector2;
			}
			Matrix lhs = Matrix.CreateRotationYPR(MathHelper.DegreesToRadians(deviationAngle), 0f, 0f);
			Matrix matrix = Matrix.CreateWorld(flybyCenter, vector, Vector3.UnitZ);
			matrix = lhs * matrix;
			Matrix mat = Matrix.Inverse(matrix);
			Vector3 vector3 = Vector3.Transform(planetContainingPosition.Parameters.Position, mat);
			Vector3 vector4 = vector3;
			vector4.X += ((vector4.X < 0f) ? 1f : -1f) * (planetContainingPosition.Parameters.Radius + 750f + 500f);
			vector4 = Vector3.Transform(vector4, matrix);
			vector = vector4 - flybyCenter;
			vector.Y = 0f;
			vector.Normalize();
			return flybyCenter + vector * offsetDist;
		}
		public float GetAddedFlyByAngle(Matrix currentAttackMatrix, Vector3 attackCenter, float offsetDist)
		{
			float num = 0f;
			Vector3 vector = attackCenter + currentAttackMatrix.Forward * offsetDist;
			StellarBody planetContainingPosition = this.m_CommanderAI.GetPlanetContainingPosition(vector);
			if (planetContainingPosition == null)
			{
				return num;
			}
			Matrix mat = Matrix.Inverse(currentAttackMatrix);
			float num2 = (Vector3.Transform(planetContainingPosition.Parameters.Position, mat).X < 0f) ? -1f : 1f;
			float num3 = MathHelper.DegreesToRadians(30f);
			float num4 = planetContainingPosition.Parameters.Radius + 750f + 500f;
			bool flag = false;
			while (!flag)
			{
				num += num2 * num3;
				vector = attackCenter + Matrix.CreateRotationYPR(currentAttackMatrix.EulerAngles.X + num, 0f, 0f).Forward * offsetDist;
				if ((vector - planetContainingPosition.Parameters.Position).LengthSquared > num4 * num4)
				{
					break;
				}
				flag = false;
			}
			return num;
		}
	}
}
