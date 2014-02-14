using Kerberos.Sots.Data;
using Kerberos.Sots.StarFleet;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Strategy
{
	internal class FleetTemplateComparision : IComparer<FleetTemplate>
	{
		private AIStance _stance;
		private List<AIFleetInfo> _nonDefendAIFleets;
		public FleetTemplateComparision(AIStance stance, List<AIFleetInfo> nonDefendAIFleets)
		{
			this._stance = stance;
			this._nonDefendAIFleets = nonDefendAIFleets;
		}
		public int Compare(FleetTemplate a, FleetTemplate b)
		{
			if (a == b)
			{
				return 0;
			}
			int num = this._nonDefendAIFleets.Count((AIFleetInfo x) => x.FleetTemplate == a.Name);
			int num2 = this._nonDefendAIFleets.Count((AIFleetInfo x) => x.FleetTemplate == b.Name);
			bool flag = FleetTemplateComparision.MustHaveTemplate(a);
			bool flag2 = FleetTemplateComparision.MustHaveTemplate(b);
			if (num == 0 && flag && (!flag2 || num2 > 0))
			{
				return -1;
			}
			if ((num > 0 || !flag) && flag2 && num2 == 0)
			{
				return 1;
			}
			int num3 = (int)Math.Floor((double)((float)num / (float)a.MinFleetsForStance[this._stance] * 100f));
			int num4 = (int)Math.Floor((double)((float)num2 / (float)b.MinFleetsForStance[this._stance] * 100f));
			if (num3 < num4)
			{
				return -1;
			}
			if (num3 > num4)
			{
				return 1;
			}
			int num5 = a.MinFleetsForStance[this._stance] - num;
			int num6 = b.MinFleetsForStance[this._stance] - num2;
			if (num5 > num6)
			{
				return -1;
			}
			if (num5 < num6)
			{
				return 1;
			}
			if (a.MinFleetsForStance[this._stance] > b.MinFleetsForStance[this._stance])
			{
				return -1;
			}
			if (a.MinFleetsForStance[this._stance] < b.MinFleetsForStance[this._stance])
			{
				return 1;
			}
			int missionPriority = this.GetMissionPriority(a.MissionTypes);
			int missionPriority2 = this.GetMissionPriority(b.MissionTypes);
			if (missionPriority > missionPriority2)
			{
				return -1;
			}
			if (missionPriority < missionPriority2)
			{
				return 1;
			}
			if (a.TemplateID < b.TemplateID)
			{
				return -1;
			}
			if (a.TemplateID > b.TemplateID)
			{
				return 1;
			}
			return 0;
		}
		private int GetMissionPriority(List<MissionType> missions)
		{
			int num = 0;
			using (List<MissionType>.Enumerator enumerator = missions.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					switch (enumerator.Current)
					{
					case MissionType.COLONIZATION:
					case MissionType.SUPPORT:
						num += 3;
						break;
					case MissionType.SURVEY:
						num += 7;
						break;
					case MissionType.CONSTRUCT_STN:
					case MissionType.UPGRADE_STN:
						num += 2;
						break;
					case MissionType.PATROL:
					case MissionType.INTERDICTION:
					case MissionType.STRIKE:
					case MissionType.INVASION:
						num++;
						break;
					case MissionType.GATE:
						num += 9;
						break;
					case MissionType.DEPLOY_NPG:
						num += 8;
						break;
					}
				}
			}
			return num;
		}
		public static bool MustHaveTemplate(FleetTemplate template)
		{
			foreach (MissionType current in template.MissionTypes)
			{
				MissionType missionType = current;
				switch (missionType)
				{
				case MissionType.COLONIZATION:
				case MissionType.SUPPORT:
				case MissionType.SURVEY:
				case MissionType.CONSTRUCT_STN:
				case MissionType.UPGRADE_STN:
					break;
				case MissionType.RELOCATION:
					continue;
				default:
					if (missionType != MissionType.GATE && missionType != MissionType.DEPLOY_NPG)
					{
						continue;
					}
					break;
				}
				return true;
			}
			return false;
		}
	}
}
