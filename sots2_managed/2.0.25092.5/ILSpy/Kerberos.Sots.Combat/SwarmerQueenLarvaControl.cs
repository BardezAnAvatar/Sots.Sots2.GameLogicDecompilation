using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.Combat
{
	internal class SwarmerQueenLarvaControl : CombatAIController
	{
		private App m_Game;
		private Ship m_SwarmerQueenLarva;
		private Ship m_SwarmerHive;
		private List<Ship> m_Enemies;
		private SwarmerQueenLarvaStates m_State;
		private IGameObject m_Target;
		private int m_EnemyUpdateRate;
		private int m_UpdateRate;
		private bool m_HasHadHive;
		private float m_BeltRadius;
		private float m_BeltThickness;
		private int m_CurrEvadeNodeIndex;
		private int m_NumEvadeNodes;
		private Vector3[] m_EvadeLocations;
		public SwarmerQueenLarvaStates State
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
		public override Ship GetShip()
		{
			return this.m_SwarmerQueenLarva;
		}
		public override void SetTarget(IGameObject target)
		{
			this.m_SwarmerQueenLarva.SetShipTarget((this.m_Target != null) ? this.m_Target.ObjectID : 0, Vector3.Zero, true, 0);
			this.m_Target = target;
		}
		public override IGameObject GetTarget()
		{
			return this.m_Target;
		}
		public SwarmerQueenLarvaControl(App game, Ship ship)
		{
			this.m_Game = game;
			this.m_SwarmerQueenLarva = ship;
		}
		public override void Initialize()
		{
			this.m_Enemies = new List<Ship>();
			this.m_State = SwarmerQueenLarvaStates.SEEK;
			this.m_EnemyUpdateRate = 0;
			this.m_UpdateRate = 0;
			this.m_SwarmerHive = null;
			this.m_Target = null;
			this.m_HasHadHive = false;
			this.m_BeltRadius = this.m_SwarmerQueenLarva.Position.Length;
			this.m_BeltThickness = 1000f;
			this.m_NumEvadeNodes = 10;
			this.m_CurrEvadeNodeIndex = 0;
			float num = MathHelper.DegreesToRadians(360f / (float)this.m_NumEvadeNodes);
			this.m_EvadeLocations = new Vector3[this.m_NumEvadeNodes];
			for (int i = 0; i < this.m_NumEvadeNodes; i++)
			{
				this.m_EvadeLocations[i] = new Vector3((float)Math.Sin((double)(num * (float)i)), 0f, (float)Math.Cos((double)(num * (float)i))) * this.m_BeltRadius;
			}
		}
		public override void Terminate()
		{
			this.m_SwarmerHive = null;
			this.SetTarget(null);
		}
		public override void ObjectRemoved(IGameObject obj)
		{
			foreach (Ship current in this.m_Enemies)
			{
				if (current == obj)
				{
					this.m_Enemies.Remove(current);
					break;
				}
			}
		}
		public override void OnThink()
		{
			if (this.m_SwarmerQueenLarva == null)
			{
				return;
			}
			switch (this.m_State)
			{
			case SwarmerQueenLarvaStates.SEEK:
				this.ThinkSeek();
				return;
			case SwarmerQueenLarvaStates.TRACK:
				this.ThinkTrack();
				return;
			case SwarmerQueenLarvaStates.EVADE:
				this.ThinkEvade();
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
			return this.m_State == SwarmerQueenLarvaStates.SEEK;
		}
		public override void FindNewTarget(IEnumerable<IGameObject> objs)
		{
			this.m_Enemies.Clear();
			foreach (IGameObject current in objs)
			{
				if (current is Ship)
				{
					Ship ship = current as Ship;
					if (ship.Player != this.m_SwarmerQueenLarva.Player && ship.Active && Ship.IsActiveShip(ship) && ship.IsDetected(this.m_SwarmerQueenLarva.Player))
					{
						this.m_Enemies.Add(ship);
					}
				}
			}
			this.m_EnemyUpdateRate = 30;
		}
		private void FindTarget()
		{
			if (this.m_SwarmerHive == null)
			{
				return;
			}
			Ship target = null;
			float num = 3.40282347E+38f;
			foreach (Ship current in this.m_Enemies)
			{
				float lengthSquared = (this.m_SwarmerHive.Position - current.Position).LengthSquared;
				if (lengthSquared < num)
				{
					target = current;
					num = lengthSquared;
				}
			}
			this.SetTarget(target);
		}
		public override bool NeedsAParent()
		{
			return !this.m_HasHadHive;
		}
		public override void FindParent(IEnumerable<CombatAIController> controllers)
		{
			foreach (CombatAIController current in controllers)
			{
				if (current is SwarmerHiveControl)
				{
					this.m_HasHadHive = true;
					this.m_SwarmerHive = current.GetShip();
					break;
				}
			}
		}
		private bool HiveIsPresent()
		{
			return this.m_SwarmerHive != null && !this.m_SwarmerHive.IsDestroyed;
		}
		private void ThinkSeek()
		{
			if (this.m_Enemies.Count > 0)
			{
				if (this.HiveIsPresent())
				{
					this.FindTarget();
					this.m_State = SwarmerQueenLarvaStates.TRACK;
					return;
				}
				this.FindInitialNodeIndex();
				this.m_State = SwarmerQueenLarvaStates.EVADE;
			}
		}
		private void ThinkTrack()
		{
			this.m_EnemyUpdateRate--;
			if (this.m_Target == null || !this.HiveIsPresent() || this.m_EnemyUpdateRate <= 0)
			{
				this.m_State = SwarmerQueenLarvaStates.SEEK;
				return;
			}
			this.m_UpdateRate--;
			if (this.m_UpdateRate <= 0)
			{
				this.m_UpdateRate = 5;
				if (!(this.m_Target is Ship))
				{
					return;
				}
				Ship ship = this.m_Target as Ship;
				Vector3 vector = ship.Position - this.m_SwarmerHive.Position;
				Vector3 vector2 = ship.Maneuvering.Destination - this.m_SwarmerHive.Position;
				Vector3 v = (vector2.LengthSquared < vector.LengthSquared) ? ship.Maneuvering.Destination : ship.Position;
				Vector3 vector3 = default(Vector3);
				if (vector2.LengthSquared < vector.LengthSquared)
				{
					vector3 = vector2;
				}
				else
				{
					vector3 = vector;
				}
				vector3.Y = 0f;
				float length = vector3.Length;
				vector3 /= length;
				Vector3 vector4 = this.m_SwarmerHive.Position;
				if (length > 3000f)
				{
					vector4 += vector3 * Math.Min(length * 0.5f, 3000f);
				}
				else
				{
					float num = Math.Min((v - this.m_SwarmerQueenLarva.Position).Length, 3000f);
					if (num > 500f)
					{
						vector4 = v - vector3 * num;
					}
					else
					{
						vector4 = this.m_SwarmerQueenLarva.Position;
					}
				}
				this.m_SwarmerQueenLarva.Maneuvering.PostAddGoal(vector4, vector3);
			}
		}
		private void ThinkEvade()
		{
			if (this.m_Enemies.Count == 0)
			{
				this.m_State = SwarmerQueenLarvaStates.SEEK;
				return;
			}
			this.m_UpdateRate--;
			if (this.m_UpdateRate <= 0)
			{
				this.m_UpdateRate = 30;
				int currEvadeNodeIndex = this.m_CurrEvadeNodeIndex;
				this.FindSafestNodeIndex();
				if (currEvadeNodeIndex != this.m_CurrEvadeNodeIndex)
				{
					Random random = new Random();
					float s = random.NextInclusive(-this.m_BeltThickness, this.m_BeltThickness);
					Vector3 v = this.m_EvadeLocations[this.m_CurrEvadeNodeIndex];
					Vector3 vector = v - this.m_SwarmerQueenLarva.Position;
					vector.Y = 0f;
					vector.Normalize();
					Vector3 v2 = Vector3.Cross(vector, Vector3.UnitY);
					this.m_SwarmerQueenLarva.Maneuvering.PostAddGoal(v + v2 * s, vector);
				}
			}
		}
		private void FindInitialNodeIndex()
		{
			this.m_CurrEvadeNodeIndex = 0;
			float num = 3.40282347E+38f;
			for (int i = 0; i < this.m_NumEvadeNodes; i++)
			{
				float lengthSquared = (this.m_EvadeLocations[i] - this.m_SwarmerQueenLarva.Position).LengthSquared;
				if (lengthSquared < num)
				{
					num = lengthSquared;
					this.m_CurrEvadeNodeIndex = i;
				}
			}
		}
		private void FindSafestNodeIndex()
		{
			int[] array = new int[this.m_NumEvadeNodes];
			for (int i = 0; i < this.m_NumEvadeNodes; i++)
			{
				foreach (Ship current in this.m_Enemies)
				{
					if ((current.Position - this.m_EvadeLocations[i]).LengthSquared < 2.5E+07f)
					{
						array[i]++;
					}
				}
			}
			if (array[this.m_CurrEvadeNodeIndex] == 0)
			{
				return;
			}
			Random random = new Random();
			int num = this.m_CurrEvadeNodeIndex - 1;
			int num2 = this.m_CurrEvadeNodeIndex + 1;
			int num3 = this.m_NumEvadeNodes / 2;
			bool flag = false;
			for (int j = 0; j < num3; j++)
			{
				if (num < 0)
				{
					num = this.m_NumEvadeNodes - 1;
				}
				if (num2 >= this.m_NumEvadeNodes)
				{
					num2 = 0;
				}
				if (array[num] == 0 && array[num2] == 0)
				{
					this.m_CurrEvadeNodeIndex = (random.CoinToss(0.5) ? num : num2);
					flag = true;
					break;
				}
				if (array[num] == 0)
				{
					this.m_CurrEvadeNodeIndex = num;
					flag = true;
					break;
				}
				if (array[num2] == 0)
				{
					this.m_CurrEvadeNodeIndex = num2;
					flag = true;
					break;
				}
				num2++;
				num--;
			}
			if (!flag)
			{
				int num4 = 500;
				float num5 = 3.40282347E+38f;
				for (int k = 0; k < this.m_NumEvadeNodes; k++)
				{
					float lengthSquared = (this.m_EvadeLocations[k] - this.m_SwarmerQueenLarva.Position).LengthSquared;
					if ((num4 == array[k] && lengthSquared < num5) || num4 < array[k])
					{
						num5 = lengthSquared;
						num4 = array[k];
						this.m_CurrEvadeNodeIndex = k;
					}
				}
			}
		}
	}
}
