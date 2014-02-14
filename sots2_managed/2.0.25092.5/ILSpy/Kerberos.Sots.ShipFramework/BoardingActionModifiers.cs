using System;
namespace Kerberos.Sots.ShipFramework
{
	internal struct BoardingActionModifiers
	{
		public float FreshAgentStrength;
		public float TiredAgentStrength;
		public float ExhaustedAgentStrength;
		public AgentLocationStrength LocationStrength;
		public AgentEfficiencyVsBoarding EfficiencyVSBoarding;
	}
}
