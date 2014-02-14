using Kerberos.Sots.Engine;
using System;
namespace Kerberos.Sots.Combat
{
	internal class SwarmerTarget
	{
		private int m_NumSwarmersOnTarget;
		private int m_NumGuardiansOnTarget;
		private IGameObject m_Target;
		public int SwarmersOnTarget
		{
			get
			{
				return this.m_NumSwarmersOnTarget;
			}
		}
		public int GuardiansOnTarget
		{
			get
			{
				return this.m_NumGuardiansOnTarget;
			}
		}
		public IGameObject Target
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
		public SwarmerTarget()
		{
			this.m_NumSwarmersOnTarget = 0;
			this.m_NumGuardiansOnTarget = 0;
			this.m_Target = null;
		}
		public void IncSwarmersOnTarget()
		{
			this.m_NumSwarmersOnTarget++;
		}
		public void IncGuardiansOnTarget()
		{
			this.m_NumGuardiansOnTarget++;
		}
		public void ClearNumTargets()
		{
			this.m_NumSwarmersOnTarget = 0;
			this.m_NumGuardiansOnTarget = 0;
		}
	}
}
