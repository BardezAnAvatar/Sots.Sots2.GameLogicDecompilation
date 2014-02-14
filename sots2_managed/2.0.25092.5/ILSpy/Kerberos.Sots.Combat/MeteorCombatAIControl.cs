using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Combat
{
	internal class MeteorCombatAIControl : CombatAIController
	{
		private App m_Game;
		private Ship m_Meteor;
		private bool m_VictoryConditionsMet;
		private bool m_FailureConditionMet;
		internal int m_AddedResources;
		internal bool m_StruckPlanet;
		private bool m_CanSubDivide;
		private bool m_CanApplyResources;
		private int m_UpdateRate;
		private MeteorShowerGlobalData.MeteorSizes m_Size;
		private StellarBody m_Target;
		private Vector3 m_TargetPosition;
		private Vector3 m_TargetFacing;
		private List<StellarBody> m_Planets;
		private SimpleAIStates m_State;
		public static MeteorShowerGlobalData.MeteorSizes GetMeteorSize(App game, int designID)
		{
			if (game.Game.ScriptModules.MeteorShower == null)
			{
				return MeteorShowerGlobalData.MeteorSizes.Small;
			}
			DesignInfo designInfo = game.GameDatabase.GetDesignInfo(designID);
			if (designInfo != null)
			{
				DesignSectionInfo designSectionInfo = designInfo.DesignSections.FirstOrDefault<DesignSectionInfo>();
				if (designSectionInfo != null)
				{
					if (designSectionInfo.ShipSectionAsset.FileName.Contains("Medium"))
					{
						return MeteorShowerGlobalData.MeteorSizes.Medium;
					}
					if (designSectionInfo.ShipSectionAsset.FileName.Contains("Large"))
					{
						return MeteorShowerGlobalData.MeteorSizes.Large;
					}
				}
			}
			return MeteorShowerGlobalData.MeteorSizes.Small;
		}
		public static bool CanSubDivide(MeteorShowerGlobalData.MeteorSizes meteorSize)
		{
			return meteorSize != MeteorShowerGlobalData.MeteorSizes.Small;
		}
		public static List<int> GetAvailableSubMeteorDesignIDs(App game, MeteorShowerGlobalData.MeteorSizes meteorSize)
		{
			List<int> list = new List<int>();
			if (game.Game.ScriptModules.MeteorShower == null)
			{
				return list;
			}
			int[] meteorDesignIds = game.Game.ScriptModules.MeteorShower.MeteorDesignIds;
			int num = 0;
			switch (meteorSize)
			{
			case MeteorShowerGlobalData.MeteorSizes.Medium:
				num = meteorDesignIds.Length - 7;
				break;
			case MeteorShowerGlobalData.MeteorSizes.Large:
				num = meteorDesignIds.Length - 5;
				break;
			}
			int num2 = 0;
			while (num2 < meteorDesignIds.Length && num2 < num)
			{
				list.Add(meteorDesignIds[num2]);
				num2++;
			}
			return list;
		}
		public override Ship GetShip()
		{
			return this.m_Meteor;
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
		public MeteorCombatAIControl(App game, Ship ship)
		{
			this.m_Game = game;
			this.m_Meteor = ship;
		}
		public override void Initialize()
		{
			this.m_State = SimpleAIStates.SEEK;
			this.m_VictoryConditionsMet = false;
			this.m_FailureConditionMet = false;
			this.m_Planets = new List<StellarBody>();
			this.m_UpdateRate = 0;
			this.m_Size = MeteorCombatAIControl.GetMeteorSize(this.m_Game, this.m_Meteor.DesignID);
			this.m_CanSubDivide = MeteorCombatAIControl.CanSubDivide(this.m_Size);
			this.m_CanApplyResources = true;
			this.m_Target = null;
			this.m_TargetFacing = -Vector3.UnitZ;
			this.m_TargetPosition = Vector3.Zero;
			MeteorShowerGlobalData globalMeteorShowerData = this.m_Game.Game.AssetDatabase.GlobalMeteorShowerData;
			this.m_Meteor.Maneuvering.PostSetProp("SetCombatAIDamage", new object[]
			{
				globalMeteorShowerData.Damage[(int)this.m_Size].Crew,
				globalMeteorShowerData.Damage[(int)this.m_Size].Population,
				globalMeteorShowerData.Damage[(int)this.m_Size].InfraDamage,
				globalMeteorShowerData.Damage[(int)this.m_Size].TeraDamage
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
			if (obj == this.m_Meteor)
			{
				if (this.m_Meteor.IsDestroyed)
				{
					this.SpawnSmallerMeteors();
					this.ApplyResourcesToPlanet();
				}
				this.m_Meteor = null;
			}
		}
		public override void OnThink()
		{
			if (this.m_Meteor == null)
			{
				return;
			}
			if (this.m_Meteor.IsDestroyed)
			{
				this.ApplyResourcesToPlanet();
				this.SpawnSmallerMeteors();
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
			return this.m_VictoryConditionsMet || this.m_FailureConditionMet;
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
				this.m_Meteor.Maneuvering.PostAddGoal(this.m_TargetPosition, this.m_TargetFacing);
				this.m_State = SimpleAIStates.TRACK;
			}
		}
		private void ThinkTrack()
		{
			if (this.m_FailureConditionMet)
			{
				return;
			}
			this.m_UpdateRate--;
			if (this.m_UpdateRate <= 0)
			{
				this.m_UpdateRate = 15;
				this.m_Meteor.Maneuvering.PostAddGoal(this.m_TargetPosition, this.m_TargetFacing);
				this.CheckForPlanetsInPath();
			}
		}
		private void ObtainPositionAndFacing()
		{
			this.m_TargetFacing = Matrix.CreateRotationYPR(this.m_Meteor.Maneuvering.Rotation).Forward;
			this.m_TargetPosition = this.m_Meteor.Maneuvering.Position + this.m_TargetFacing * 500000f;
		}
		private void CheckForPlanetsInPath()
		{
			Matrix m = Matrix.CreateRotationYPR(this.m_Meteor.Maneuvering.Rotation);
			m.Position = this.m_Meteor.Maneuvering.Position;
			Matrix mat = Matrix.Inverse(m);
			float radius = this.m_Meteor.ShipSphere.radius;
			bool flag = false;
			float num = 3.40282347E+38f;
			StellarBody stellarBody = null;
			foreach (StellarBody current in this.m_Planets)
			{
				ColonyInfo colonyInfoForPlanet = this.m_Game.GameDatabase.GetColonyInfoForPlanet(current.Parameters.OrbitalID);
				if (colonyInfoForPlanet != null)
				{
					Vector3 vector = Vector3.Transform(current.Parameters.Position, mat);
					if (Math.Abs(vector.X) < radius + current.Parameters.Radius && Math.Abs(vector.Y) < radius + current.Parameters.Radius && vector.Z < 0f)
					{
						flag = true;
						float num2 = -vector.Z - current.Parameters.Radius;
						if (num2 < num)
						{
							num = num2;
							stellarBody = current;
						}
					}
				}
			}
			if (stellarBody != this.m_Target)
			{
				int targetId = (stellarBody != null) ? stellarBody.ObjectID : 0;
				this.m_Meteor.SetShipTarget(targetId, Vector3.Zero, true, 0);
				this.SetTarget(stellarBody);
			}
			if (!flag)
			{
				this.m_FailureConditionMet = true;
			}
		}
		private void SpawnSmallerMeteors()
		{
			if (!this.m_CanSubDivide || this.m_Meteor.InstantlyKilled)
			{
				return;
			}
			this.m_CanSubDivide = false;
			Random random = new Random();
			Vector3 forward = (this.m_Target != null) ? (this.m_Target.Parameters.Position - this.m_Meteor.Maneuvering.Position) : (this.m_TargetPosition - this.m_Meteor.Maneuvering.Position);
			forward.Normalize();
			Matrix rhs = Matrix.CreateWorld(this.m_Meteor.Maneuvering.Position, forward, Vector3.UnitY);
			Sphere shipSphere = this.m_Meteor.ShipSphere;
			int numBreakoffMeteors = this.m_Game.AssetDatabase.GlobalMeteorShowerData.NumBreakoffMeteors;
			float maxAngle = MathHelper.DegreesToRadians(30f);
			List<int> availableSubMeteorDesignIDs = MeteorCombatAIControl.GetAvailableSubMeteorDesignIDs(this.m_Game, this.m_Size);
			if (availableSubMeteorDesignIDs.Count > 0)
			{
				for (int i = 0; i < numBreakoffMeteors; i++)
				{
					Vector3 vector = default(Vector3);
					vector.X = (random.CoinToss(0.5) ? -1f : 1f) * random.NextInclusive(10f, 85f);
					vector.Y = (random.CoinToss(0.5) ? -1f : 1f) * random.NextInclusive(10f, 85f);
					vector.Z = (random.CoinToss(0.5) ? -1f : 1f) * random.NextInclusive(10f, 85f);
					vector.Normalize();
					vector *= random.NextInclusive(shipSphere.radius * 0.1f, shipSphere.radius);
					Matrix matrix = Matrix.PolarDeviation(random, maxAngle);
					matrix.Position = vector;
					matrix *= rhs;
					int designId = availableSubMeteorDesignIDs[random.NextInclusive(0, availableSubMeteorDesignIDs.Count - 1)];
					this.m_Game.CurrentState.AddGameObject(CombatAIController.CreateNewShip(this.m_Game.Game, matrix, designId, 0, this.m_Meteor.InputID, this.m_Meteor.Player.ObjectID), true);
				}
			}
		}
		private void ApplyResourcesToPlanet()
		{
			if (this.m_Target == null || this.m_Target == null || !this.m_CanApplyResources)
			{
				return;
			}
			this.m_CanApplyResources = false;
			this.m_StruckPlanet = this.m_Meteor.InstantlyKilled;
			float num = this.m_Meteor.InstantlyKilled ? 0.1f : 1f;
			int num2 = (int)((float)this.m_Game.AssetDatabase.GlobalMeteorShowerData.ResourceBonuses[(int)this.m_Size] * num);
			StellarBody target = this.m_Target;
			PlanetInfo planetInfo = this.m_Game.GameDatabase.GetPlanetInfo(target.Parameters.OrbitalID);
			if (planetInfo != null)
			{
				planetInfo.Resources += num2;
				this.m_Game.GameDatabase.UpdatePlanet(planetInfo);
				this.m_AddedResources = num2;
			}
		}
	}
}
