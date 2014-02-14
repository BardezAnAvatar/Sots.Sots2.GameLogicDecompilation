using Kerberos.Sots.StarFleet;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.Strategy
{
	internal class FleetTemplate
	{
		public int TemplateID;
		public string Name;
		public string FleetName;
		public bool Initial;
		public List<MissionType> MissionTypes = new List<MissionType>();
		public List<ShipInclude> ShipIncludes = new List<ShipInclude>();
		public Dictionary<AIStance, int> StanceWeights = new Dictionary<AIStance, int>();
		public Dictionary<AIStance, int> MinFleetsForStance = new Dictionary<AIStance, int>();
		public List<string> AllowableFactions = new List<string>();
		public override string ToString()
		{
			return this.Name;
		}
		public bool CanFactionUse(string factionName)
		{
			return this.AllowableFactions.Count == 0 || this.AllowableFactions.Contains(factionName);
		}
	}
}
