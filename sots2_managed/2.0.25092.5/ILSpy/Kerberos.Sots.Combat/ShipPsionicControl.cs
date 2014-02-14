using Kerberos.Sots.Data.ShipFramework;
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
	internal class ShipPsionicControl
	{
		private static int kMaxThinkDelay = 1800;
		private static int kMinThinkDelay = 600;
		private static int kSuulkaMaxThinkDelay = 300;
		private static int kSuulkaMinThinkDelay = 200;
		private App m_Game;
		private CombatAI m_CommanderAI;
		private PsionicControlState m_State;
		private Psionic m_HoldPsionic;
		private Ship m_Ship;
		private int m_CurrUpdateFrame;
		private int m_CurrMaxUpdateFrame;
		private int m_MinFrames;
		private int m_MaxFrames;
		public Ship ControlledShip
		{
			get
			{
				return this.m_Ship;
			}
		}
		public static float GetWeightForPsionic(CombatAI cmdAI, Ship ship, Psionic psi)
		{
			float num = 0f;
			if (psi != null && ship != null)
			{
				if (psi.IsActive)
				{
					return num;
				}
				switch (psi.Type)
				{
				case SectionEnumerations.PsionicAbility.TKFist:
					num = 50f;
					break;
				case SectionEnumerations.PsionicAbility.Hold:
					num = 15f;
					break;
				case SectionEnumerations.PsionicAbility.Crush:
					num = 10f;
					break;
				case SectionEnumerations.PsionicAbility.Reflector:
					num = 0f;
					break;
				case SectionEnumerations.PsionicAbility.Repair:
					num = 0f;
					break;
				case SectionEnumerations.PsionicAbility.Fear:
					num = 5f;
					break;
				case SectionEnumerations.PsionicAbility.PsiDrain:
					if (ship.CurrentPsiPower < (int)((float)ship.MaxPsiPower * 0.9f))
					{
						num = 10f;
						if (ship.CurrentPsiPower < ship.MaxPsiPower / 2)
						{
							num += 20f;
						}
					}
					else
					{
						num = 0f;
					}
					break;
				case SectionEnumerations.PsionicAbility.WildFire:
					num = 10f;
					break;
				case SectionEnumerations.PsionicAbility.Control:
					num = 5f;
					break;
				case SectionEnumerations.PsionicAbility.LifeDrain:
					num = 10f;
					break;
				}
			}
			return num;
		}
		public static bool IsHoldPsionic(Psionic psi)
		{
			if (psi != null)
			{
				switch (psi.Type)
				{
				case SectionEnumerations.PsionicAbility.TKFist:
				case SectionEnumerations.PsionicAbility.Hold:
				case SectionEnumerations.PsionicAbility.Crush:
				case SectionEnumerations.PsionicAbility.Repair:
				case SectionEnumerations.PsionicAbility.Fear:
				case SectionEnumerations.PsionicAbility.PsiDrain:
				case SectionEnumerations.PsionicAbility.Control:
				case SectionEnumerations.PsionicAbility.LifeDrain:
				case SectionEnumerations.PsionicAbility.Movement:
					return true;
				}
			}
			return false;
		}
		public static bool CanUsePsionic(Ship ship, Psionic psi, CombatAI cmdAI)
		{
			if (ship == null || psi == null)
			{
				return false;
			}
			if (psi != null)
			{
				switch (psi.Type)
				{
				case SectionEnumerations.PsionicAbility.TKFist:
				case SectionEnumerations.PsionicAbility.Hold:
				case SectionEnumerations.PsionicAbility.Crush:
				case SectionEnumerations.PsionicAbility.Reflector:
				case SectionEnumerations.PsionicAbility.Fear:
				case SectionEnumerations.PsionicAbility.WildFire:
				case SectionEnumerations.PsionicAbility.Control:
					if (ship.Target is Ship)
					{
						Ship ship2 = ship.Target as Ship;
						float num = ship.SensorRange + ship2.ShipSphere.radius;
						if ((ship.Position - ship2.Position).Length < num * num)
						{
							return true;
						}
					}
					break;
				case SectionEnumerations.PsionicAbility.Repair:
					return false;
				case SectionEnumerations.PsionicAbility.PsiDrain:
				case SectionEnumerations.PsionicAbility.LifeDrain:
					if (ship.Target != null)
					{
						if (ship.Target is Ship)
						{
							Ship ship3 = ship.Target as Ship;
							float num2 = ship.SensorRange + ship3.ShipSphere.radius;
							if ((ship.Position - ship3.Position).Length < num2 * num2)
							{
								return true;
							}
						}
						else
						{
							if (ship.Target is StellarBody)
							{
								StellarBody stellarBody = ship.Target as StellarBody;
								float num3 = ship.SensorRange + stellarBody.Parameters.Radius;
								if ((ship.Position - stellarBody.Parameters.Position).Length < num3 * num3)
								{
									return true;
								}
							}
						}
					}
					break;
				case SectionEnumerations.PsionicAbility.Movement:
					return true;
				}
			}
			return false;
		}
		public static bool PsionicsCanBeUsed(Ship ship, CombatAI cmdAI)
		{
			if (ship == null || ship.CurrentPsiPower <= 0 || ship.Psionics.Count<Psionic>() == 0)
			{
				return false;
			}
			bool result = false;
			foreach (Psionic current in ship.Psionics)
			{
				if (ShipPsionicControl.CanUsePsionic(ship, current, cmdAI))
				{
					result = true;
					break;
				}
			}
			return result;
		}
		public ShipPsionicControl(App game, CombatAI commanderAI, Ship ship)
		{
			this.m_Game = game;
			this.m_CommanderAI = commanderAI;
			this.m_Ship = ship;
			this.m_HoldPsionic = null;
			this.m_MinFrames = (ship.IsSuulka ? ShipPsionicControl.kSuulkaMinThinkDelay : ShipPsionicControl.kMinThinkDelay);
			this.m_MaxFrames = (ship.IsSuulka ? ShipPsionicControl.kSuulkaMaxThinkDelay : ShipPsionicControl.kMaxThinkDelay);
			this.m_CurrMaxUpdateFrame = commanderAI.AIRandom.NextInclusive(this.m_MinFrames, this.m_MaxFrames);
			this.m_CurrUpdateFrame = this.m_CurrMaxUpdateFrame;
			this.m_State = PsionicControlState.Think;
		}
		public virtual void Shutdown()
		{
		}
		public virtual void ObjectRemoved(IGameObject obj)
		{
			if (this.m_Ship == obj)
			{
				this.m_Ship = null;
			}
		}
		public virtual bool RemoveWeaponControl()
		{
			if (this.m_Ship == null)
			{
				return true;
			}
			this.m_CurrUpdateFrame--;
			return this.m_CurrUpdateFrame <= 0;
		}
		public bool CanChangeTarget()
		{
			return this.m_State != PsionicControlState.HoldPsionic;
		}
		public virtual void Update(int framesElapsed)
		{
			if (this.m_Ship == null)
			{
				return;
			}
			switch (this.m_State)
			{
			case PsionicControlState.Think:
				this.Think(framesElapsed);
				return;
			case PsionicControlState.ChoosePsionic:
				this.ChoosePsionic();
				return;
			case PsionicControlState.HoldPsionic:
				this.HoldPsionic();
				return;
			default:
				return;
			}
		}
		private void Think(int framesElapsed)
		{
			if (this.m_Ship.Target == null || !ShipPsionicControl.PsionicsCanBeUsed(this.m_Ship, this.m_CommanderAI))
			{
				this.m_CurrUpdateFrame = this.m_CurrMaxUpdateFrame;
				return;
			}
			if ((this.m_Ship.Target is Ship && !Ship.IsActiveShip(this.m_Ship.Target as Ship)) || (this.m_Ship.Target is StellarBody && (this.m_Ship.Target as StellarBody).Population <= 0.0))
			{
				this.m_CurrUpdateFrame = this.m_CurrMaxUpdateFrame;
				return;
			}
			this.m_CurrUpdateFrame -= framesElapsed;
			if (this.m_CurrUpdateFrame > 0)
			{
				return;
			}
			this.m_CurrMaxUpdateFrame = this.m_CommanderAI.AIRandom.NextInclusive(this.m_MinFrames, this.m_MaxFrames);
			this.m_CurrUpdateFrame = this.m_CurrMaxUpdateFrame;
			this.m_State = PsionicControlState.ChoosePsionic;
		}
		private void ChoosePsionic()
		{
			if (this.m_Ship == null || this.m_Ship.Target == null)
			{
				return;
			}
			this.m_HoldPsionic = null;
			Psionic psionic = null;
			float num = 0f;
			Dictionary<Psionic, float> dictionary = new Dictionary<Psionic, float>();
			foreach (Psionic current in this.m_Ship.Psionics)
			{
				float weightForPsionic = ShipPsionicControl.GetWeightForPsionic(this.m_CommanderAI, this.m_Ship, current);
				if (weightForPsionic > 0f)
				{
					dictionary.Add(current, weightForPsionic);
					num += weightForPsionic;
				}
			}
			if (dictionary.Count == 0)
			{
				this.m_State = PsionicControlState.Think;
				return;
			}
			float num2 = this.m_CommanderAI.AIRandom.NextInclusive(1f, num);
			foreach (KeyValuePair<Psionic, float> current2 in dictionary)
			{
				if (num2 <= current2.Value)
				{
					psionic = current2.Key;
					break;
				}
				num2 -= current2.Value;
			}
			if (psionic != null)
			{
				psionic.Activate();
				if (ShipPsionicControl.IsHoldPsionic(psionic))
				{
					this.m_State = PsionicControlState.HoldPsionic;
					this.m_HoldPsionic = psionic;
					return;
				}
				psionic.PostSetProp("SetTarget", this.m_Ship.Target.ObjectID);
				psionic.Deactivate();
				this.m_State = PsionicControlState.Think;
			}
		}
		private void HoldPsionic()
		{
			if (this.m_Ship == null || this.m_HoldPsionic == null)
			{
				return;
			}
			if (this.m_Ship.Target == null)
			{
				this.ClearHoldPsionic();
			}
			else
			{
				if (this.m_HoldPsionic.IsActive)
				{
					if (this.m_HoldPsionic.PercentConsumed > 0.95f)
					{
						this.ClearHoldPsionic();
					}
					else
					{
						if (this.m_HoldPsionic is PsiDrain && this.m_Ship.CurrentPsiPower >= this.m_Ship.MaxPsiPower)
						{
							this.ClearHoldPsionic();
						}
					}
				}
				else
				{
					this.ClearHoldPsionic();
				}
			}
			if (this.m_HoldPsionic == null)
			{
				this.m_State = PsionicControlState.Think;
			}
		}
		private void ClearHoldPsionic()
		{
			if (this.m_HoldPsionic != null)
			{
				this.m_HoldPsionic.Deactivate();
				this.m_HoldPsionic = null;
			}
		}
	}
}
