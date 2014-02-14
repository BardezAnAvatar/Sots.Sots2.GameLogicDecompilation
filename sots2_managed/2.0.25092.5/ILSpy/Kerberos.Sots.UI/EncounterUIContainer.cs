using Kerberos.Sots.Strategy;
using System;
namespace Kerberos.Sots.UI
{
	internal struct EncounterUIContainer
	{
		public int NumEnounters;
		public PanelBinding[] _panels;
		public PendingCombat _combat;
		public string CombatListID;
		public string SystemItemID;
		public string InstancePanel;
		public string SystemButtonID;
	}
}
