using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.Combat
{
	internal class CometCombatAIControl : CombatAIController
	{
		private App m_Game;
		private Ship m_Comet;
		private bool m_VictoryConditionsMet;
		private int m_UpdateRate;
		private StellarBody m_Target;
		private Vector3 m_TargetPosition;
		private Vector3 m_TargetFacing;
		private List<StellarBody> m_Planets;
		private SimpleAIStates m_State;
		public override Ship GetShip()
		{
			return this.m_Comet;
		}
		public override void SetTarget(IGameObject target)
		{
			if (target is StellarBody)
			{
				this.m_Target = (target as StellarBody);
			}
		}
		public override IGameObject GetTarget()
		{
			return this.m_Target;
		}
		public CometCombatAIControl(App game, Ship ship)
		{
			this.m_Game = game;
			this.m_Comet = ship;
		}
		public override void Initialize()
		{
			this.m_State = SimpleAIStates.SEEK;
			this.m_VictoryConditionsMet = false;
			this.m_Planets = new List<StellarBody>();
			this.m_UpdateRate = 0;
			this.m_Target = null;
			this.m_TargetFacing = -Vector3.UnitZ;
			this.m_TargetPosition = Vector3.Zero;
			CometGlobalData globalCometData = this.m_Game.Game.AssetDatabase.GlobalCometData;
			this.m_Comet.Maneuvering.PostSetProp("SetCombatAIDamage", new object[]
			{
				globalCometData.Damage.Crew,
				globalCometData.Damage.Population,
				globalCometData.Damage.InfraDamage,
				globalCometData.Damage.TeraDamage
			});
		}
		public override void Terminate()
		{
		}
		public override void ObjectRemoved(IGameObject obj)
		{
			foreach (StellarBody current in this.m_Planets)
			{
				if (current == obj)
				{
					this.m_Planets.Remove(current);
					break;
				}
			}
			if (obj == this.m_Comet)
			{
				this.m_Comet = null;
			}
		}
		public override void OnThink()
		{
			if (this.m_Comet == null)
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
			return this.m_VictoryConditionsMet;
		}
		public override bool RequestingNewTarget()
		{
			return this.m_State == SimpleAIStates.SEEK && this.m_Planets.Count == 0;
		}
		public override void FindNewTarget(IEnumerable<IGameObject> objs)
		{
			foreach (IGameObject current in objs)
			{
				if (current is StellarBody)
				{
					this.m_Planets.Add(current as StellarBody);
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
		private void ThinkSeek()
		{
			if (this.m_Planets.Count > 0)
			{
				this.ObtainPositionAndFacing();
				this.m_Comet.Maneuvering.PostAddGoal(this.m_TargetPosition, this.m_TargetFacing);
				this.m_State = SimpleAIStates.TRACK;
			}
		}
		private void ThinkTrack()
		{
			this.m_UpdateRate--;
			if (this.m_UpdateRate <= 0)
			{
				this.m_UpdateRate = 15;
				this.m_Comet.Maneuvering.PostAddGoal(this.m_TargetPosition, this.m_TargetFacing);
			}
		}
		private void ObtainPositionAndFacing()
		{
			this.m_TargetFacing = Matrix.CreateRotationYPR(this.m_Comet.Maneuvering.Rotation).Forward;
			this.m_TargetPosition = this.m_Comet.Maneuvering.Position + this.m_TargetFacing * 500000f;
		}
	}
}
