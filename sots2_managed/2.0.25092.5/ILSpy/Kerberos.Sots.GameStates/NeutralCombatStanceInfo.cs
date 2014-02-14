using Kerberos.Sots.Framework;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.GameStates
{
	internal struct NeutralCombatStanceInfo
	{
		public NeutralCombatStance Stance;
		public Vector3 RandomSpawnCenter;
		public NeutralCombatStanceSpawnPosition FleetAPos;
		public NeutralCombatStanceSpawnPosition FleetBPos;
		public List<NeutralCombatStanceSpawnPosition> FleetAAllies;
		public List<NeutralCombatStanceSpawnPosition> FleetBAllies;
		public void InitData()
		{
			this.Stance = NeutralCombatStance.None;
			this.RandomSpawnCenter = Vector3.Zero;
			this.FleetAPos = new NeutralCombatStanceSpawnPosition();
			this.FleetAPos.SetInfo(0, Vector3.Zero, -Vector3.UnitZ);
			this.FleetBPos = new NeutralCombatStanceSpawnPosition();
			this.FleetBPos.SetInfo(0, Vector3.Zero, -Vector3.UnitZ);
			this.FleetAAllies = new List<NeutralCombatStanceSpawnPosition>();
			this.FleetBAllies = new List<NeutralCombatStanceSpawnPosition>();
		}
	}
}
