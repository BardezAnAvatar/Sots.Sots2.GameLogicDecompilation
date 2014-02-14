using Kerberos.Sots.Engine;
using System;
namespace Kerberos.Sots.GameStates
{
	[GameObjectType(InteropGameObjectType.IGOT_RESEARCHSTATE)]
	internal class ResearchState : GameObject, IActive
	{
		public bool Active
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				this.PostSetActive(true);
			}
		}
		public void NextTechFamily()
		{
			this.PostSetProp("SelectNextFamily", new object[0]);
		}
		public void PrevTechFamily()
		{
			this.PostSetProp("SelectPrevFamily", new object[0]);
		}
		public void RebindModels()
		{
			this.PostSetProp("RebindModels", new object[0]);
		}
		public void Clear()
		{
			this.PostSetProp("Clear", new object[0]);
		}
	}
}
