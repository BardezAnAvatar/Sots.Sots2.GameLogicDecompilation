using System;
using System.Collections;
using System.Collections.Generic;
namespace Kerberos.Sots.StarFleet
{
	public static class MissionTypeExtensions
	{
		public static string ToDisplayText(this MissionType missionType)
		{
			switch (missionType)
			{
			case MissionType.NO_MISSION:
				return App.Localize("@UI_MISSION_NO_MISSION");
			case MissionType.COLONIZATION:
				return App.Localize("@UI_MISSION_COLONIZATION");
			case MissionType.SUPPORT:
				return App.Localize("@UI_MISSION_SUPPORT");
			case MissionType.SURVEY:
				return App.Localize("@UI_MISSION_SURVEY");
			case MissionType.RELOCATION:
				return App.Localize("@UI_MISSION_RELOCATION");
			case MissionType.CONSTRUCT_STN:
				return App.Localize("@UI_MISSION_CONSTRUCT_STN");
			case MissionType.UPGRADE_STN:
				return App.Localize("@UI_MISSION_UPGRADE_STN");
			case MissionType.PATROL:
				return App.Localize("@UI_MISSION_PATROL");
			case MissionType.INTERDICTION:
				return App.Localize("@UI_MISSION_INTERDICTION");
			case MissionType.STRIKE:
				return App.Localize("@UI_MISSION_STRIKE");
			case MissionType.INVASION:
				return App.Localize("@UI_MISSION_INVASION");
			case MissionType.INTERCEPT:
				return App.Localize("@UI_MISSION_INTERCEPT");
			case MissionType.GATE:
				return App.Localize("@UI_MISSION_GATE");
			case MissionType.RETURN:
				return App.Localize("@UI_MISSION_RETURN");
			case MissionType.RETREAT:
				return App.Localize("@UI_MISSION_RETREAT");
			case MissionType.PIRACY:
				return App.Localize("@UI_MISSION_PIRACY");
			case MissionType.DEPLOY_NPG:
				return App.Localize("@UI_MISSION_NPG");
			case MissionType.EVACUATE:
				return App.Localize("@UI_MISSION_EVACUATION");
			case MissionType.SPECIAL_CONSTRUCT_STN:
				return App.Localize("@UI_MISSION_CONSTRUCT_STN");
			default:
				return "Unknown";
			}
		}
		public static int SerializeList(List<MissionType> missionTypes)
		{
			int num = 0;
			using (List<MissionType>.Enumerator enumerator = missionTypes.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					int current = (int)enumerator.Current;
					num |= num + (1 << current);
				}
			}
			return num;
		}
		public static List<MissionType> DeserializeList(int missionFlags)
		{
			List<MissionType> list = new List<MissionType>();
			BitArray bitArray = new BitArray(BitConverter.GetBytes(missionFlags));
			int num = 0;
			foreach (bool flag in bitArray)
			{
				if (flag)
				{
					list.Add((MissionType)num);
				}
				num++;
			}
			return list;
		}
	}
}
