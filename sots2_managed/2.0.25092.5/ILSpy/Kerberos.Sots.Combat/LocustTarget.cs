using Kerberos.Sots.GameObjects;
using System;
namespace Kerberos.Sots.Combat
{
	internal class LocustTarget
	{
		private int m_NumFightersOnTarget;
		private Ship m_Target;
		public int FightersOnTarget
		{
			get
			{
				return this.m_NumFightersOnTarget;
			}
		}
		public Ship Target
		{
			get
			{
				return this.m_Target;
			}
			set
			{
				this.m_Target = value;
			}
		}
		public LocustTarget()
		{
			this.m_NumFightersOnTarget = 0;
			this.m_Target = null;
		}
		public void IncFightersOnTarget()
		{
			this.m_NumFightersOnTarget++;
		}
		public void ClearNumTargets()
		{
			this.m_NumFightersOnTarget = 0;
		}
	}
}
