using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.Combat
{
	internal class VonNeumannPlanetKillerCombatAIControl : SystemKillerCombatAIControl
	{
		private int m_SystemId;
		public VonNeumannPlanetKillerCombatAIControl(App game, Ship ship, int systemId) : base(game, ship)
		{
			this.m_SystemId = systemId;
		}
		public override bool RequestingNewTarget()
		{
			return base.RequestingNewTarget() || (this.m_State == SystemKillerStates.SEEK && this.m_CurrentTarget == null);
		}
		public override void FindNewTarget(IEnumerable<IGameObject> objs)
		{
			if (this.m_SystemKiller == null)
			{
				return;
			}
			if (!this.m_SpaceBattle && this.m_Planets.Count + this.m_Stars.Count + this.m_Moons.Count == 0)
			{
				base.FindNewTarget(objs);
			}
			if (this.m_Game.Game.ScriptModules.VonNeumann != null && this.m_Game.Game.ScriptModules.VonNeumann.HomeWorldSystemID == this.m_SystemId)
			{
				float num = 3.40282347E+38f;
				using (IEnumerator<IGameObject> enumerator = objs.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						IGameObject current = enumerator.Current;
						if (current is Ship)
						{
							Ship ship = current as Ship;
							if (ship.Player != this.m_SystemKiller.Player && Ship.IsActiveShip(ship) && ship.IsDetected(this.m_SystemKiller.Player))
							{
								float lengthSquared = (ship.Position - this.m_SystemKiller.Position).LengthSquared;
								if (lengthSquared < num)
								{
									this.m_CurrentTarget = ship;
									this.m_PlanetOffsetDist = ship.ShipSphere.radius + 1000f;
									num = lengthSquared;
								}
							}
						}
					}
					return;
				}
			}
			base.FindCurrentTarget(true);
		}
		public override void FindParent(IEnumerable<CombatAIController> controllers)
		{
		}
		protected override void ThinkSeek()
		{
			if (this.m_CurrentTarget == null)
			{
				return;
			}
			this.m_State = SystemKillerStates.TRACK;
		}
		protected override void ThinkTrack()
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
				if (this.m_CurrentTarget is Ship)
				{
					this.m_TargetCenter = (this.m_CurrentTarget as Ship).Position;
					this.m_TargetLook = this.m_TargetCenter - this.m_SystemKiller.Position;
					this.m_TargetLook.Y = 0f;
					this.m_TargetLook.Normalize();
				}
				Vector3 vector2 = this.m_TargetCenter + vector * this.m_PlanetOffsetDist;
				this.m_TargetLook = -vector;
				this.m_SystemKiller.Maneuvering.PostAddGoal(vector2, this.m_TargetLook);
				if (this.m_CurrentTarget is StellarBody)
				{
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
		}
	}
}
