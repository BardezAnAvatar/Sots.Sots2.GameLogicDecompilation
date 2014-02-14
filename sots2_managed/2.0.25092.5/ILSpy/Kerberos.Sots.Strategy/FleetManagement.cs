using Kerberos.Sots.Data;
using Kerberos.Sots.StarFleet;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Strategy
{
	internal class FleetManagement
	{
		public FleetInfo Fleet;
		public MissionEstimate MissionTime;
		public FleetTypeFlags FleetTypes;
		public int Score;
		public int FleetStrength;
		public FleetManagement()
		{
			this.Fleet = null;
			this.MissionTime = null;
			this.Score = 0;
			this.FleetStrength = 0;
		}
		public static FleetTypeFlags GetFleetTypeFlags(App app, int fleetID, int playerID, bool isLoa)
		{
			AIFleetInfo aIFleetInfo = app.GameDatabase.GetAIFleetInfos(playerID).FirstOrDefault((AIFleetInfo x) => x.FleetID == fleetID);
			FleetTemplate fleetTemplate = null;
			string templateName = (aIFleetInfo != null) ? aIFleetInfo.FleetTemplate : DesignLab.DeduceFleetTemplate(app.GameDatabase, app.Game, fleetID);
			if (!string.IsNullOrEmpty(templateName))
			{
				fleetTemplate = app.GameDatabase.AssetDatabase.FleetTemplates.First((FleetTemplate x) => x.Name == templateName);
			}
			if (fleetTemplate == null)
			{
				fleetTemplate = app.GameDatabase.AssetDatabase.FleetTemplates.FirstOrDefault((FleetTemplate x) => x.Name == "DEFAULT_COMBAT");
			}
			return FleetManagement.GetFleetTypeFlags(fleetTemplate, isLoa);
		}
		public static FleetTypeFlags GetFleetTypeFlags(FleetTemplate template, bool isLoa)
		{
			if (template == null)
			{
				return FleetTypeFlags.UNKNOWN;
			}
			FleetTypeFlags fleetTypeFlags = FleetTypeFlags.UNKNOWN;
			if (isLoa && template.MissionTypes.Contains(MissionType.SURVEY))
			{
				fleetTypeFlags |= FleetTypeFlags.NPG;
			}
			using (List<MissionType>.Enumerator enumerator = template.MissionTypes.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					switch (enumerator.Current)
					{
					case MissionType.COLONIZATION:
						fleetTypeFlags |= FleetTypeFlags.COLONIZE;
						break;
					case MissionType.SURVEY:
						fleetTypeFlags |= FleetTypeFlags.SURVEY;
						break;
					case MissionType.CONSTRUCT_STN:
					case MissionType.UPGRADE_STN:
						fleetTypeFlags |= FleetTypeFlags.CONSTRUCTION;
						break;
					case MissionType.PATROL:
						fleetTypeFlags |= FleetTypeFlags.PATROL;
						break;
					case MissionType.STRIKE:
						fleetTypeFlags |= FleetTypeFlags.COMBAT;
						break;
					case MissionType.INVASION:
						fleetTypeFlags |= FleetTypeFlags.PLANETATTACK;
						break;
					case MissionType.GATE:
						fleetTypeFlags |= FleetTypeFlags.GATE;
						break;
					}
				}
			}
			return fleetTypeFlags;
		}
	}
}
