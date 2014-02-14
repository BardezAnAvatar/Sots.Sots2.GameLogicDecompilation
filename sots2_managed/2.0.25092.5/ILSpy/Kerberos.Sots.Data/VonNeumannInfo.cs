using System;
using System.Collections.Generic;
namespace Kerberos.Sots.Data
{
	internal class VonNeumannInfo
	{
		public int Id;
		public int? FleetId;
		public int SystemId;
		public int OrbitalId;
		public int Resources;
		public int ResourcesCollectedLastTurn;
		public int ConstructionProgress;
		public int? ProjectDesignId;
		public int LastCollectionSystem;
		public int LastTargetSystem;
		public int LastCollectionTurn;
		public int LastTargetTurn;
		public List<VonNeumannTargetInfo> TargetInfos = new List<VonNeumannTargetInfo>();
	}
}
