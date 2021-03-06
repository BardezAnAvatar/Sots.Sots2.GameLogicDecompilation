using System;
namespace Kerberos.Sots
{
	public enum TurnEventMessage
	{
		EM_SYSTEM_SURVEYED,
		EM_SHIPS_BUILT_SINGLE,
		EM_SHIPS_BUILT_MULTIPLE,
		EM_PROTOTYPE_COMPLETE,
		EM_NO_RESEARCH,
		EM_RESEARCH_COMPLETE,
		EM_FEASIBILITY_STUDY_COMPLETE_VERY_BAD,
		EM_FEASIBILITY_STUDY_COMPLETE_BAD,
		EM_FEASIBILITY_STUDY_COMPLETE_GOOD,
		EM_FEASIBILITY_STUDY_COMPLETE_VERY_GOOD,
		EM_GATE_SHIP_DEPLOYED,
		EM_GATE_STATION_DEPLOYED,
		EM_COLONY_ESTABLISHED,
		EM_COLONY_SUPPORT,
		EM_COLONY_SUPPORT_COMPLETE,
		EM_EVACUATION_COMPLETE,
		EM_COLONY_SELF_SUFFICIENT,
		EM_COLONY_ABANDONED,
		EM_STATION_BUILT,
		EM_STATION_UPGRADED,
		EM_SUPERWORLD_COMPLETE,
		EM_COMBAT_DRAW,
		EM_COMBAT_WIN,
		EM_COMBAT_LOSS,
		EM_BETRAYAL,
		EM_BETRAYED,
		EM_RELOCATION_COMPLETE,
		EM_OVERHARVEST_WARNING,
		EM_REBELLION_STARTING,
		EM_REBELLION_STARTED,
		EM_REBELLION_ONGOING,
		EM_REBELLION_ENDED_WIN,
		EM_REBELLION_ENDED_LOSS,
		EM_GOVERNMENT_TYPE_CHANGED,
		EM_INTEL_MISSION_FAILED,
		EM_RESEARCH_NEVER_COMPLETE,
		EM_GATE_CAPACITY_REACHED,
		EM_NODE_LINE_COLLAPSE,
		EM_NODE_LINE_COLLAPSE_FLEET_LOSS,
		EM_SUULKA_ARRIVES,
		EM_SUULKA_LEAVES,
		EM_ADMIRAL_PROMOTED,
		EM_POST_COMBAT,
		EM_TREATY_REQUESTED,
		EM_TREATY_ACCEPTED,
		EM_TREATY_DECLINED,
		EM_TREATY_EXPIRED,
		EM_TREATY_BROKEN_OFFENDER,
		EM_TREATY_BROKEN_VICTIM,
		EM_REQUEST_REQUESTED,
		EM_REQUEST_DECLINED,
		EM_REQUEST_ACCEPTED,
		EM_DEMAND_REQUESTED,
		EM_DEMAND_DECLINED,
		EM_DEMAND_ACCEPTED,
		EM_PLAGUE_STARTED,
		EM_PLAGUE_ENDED,
		EM_PLAGUE_DAMAGE_POP,
		EM_PLAGUE_DAMAGE_STRUCT,
		EM_PLAGUE_DAMAGE_POP_STRUCT,
		EM_ADMIRAL_DEAD,
		EM_ADMIRAL_RETIRED,
		EM_SALVAGE_PROJECT_COMPLETE,
		EM_NEW_SALVAGE_PROJECT,
		EM_NEW_SPECIAL_PROJECT,
		EM_MONITOR_PROJECT_COMPLETE,
		EM_RADIATION_SHIELDING_PROJECT_COMPLETE,
		EM_NEUTRONSTAR_PROJECT_COMPLETE,
		EM_GARDENER_PROJECT_COMPLETE,
		EM_INTEL_MISSION_CRITICAL_FAILED,
		EM_INTEL_MISSION_CRITICAL_FAILED_LEAK,
		EM_INTEL_MISSION_NO_RANDOM_SYSTEM,
		EM_INTEL_MISSION_RANDOM_SYSTEM,
		EM_INTEL_MISSION_NO_HIGHEST_TRADE_SYSTEM,
		EM_INTEL_MISSION_HIGHEST_TRADE_SYSTEM,
		EM_INTEL_MISSION_NO_NEWEST_COLONY_SYSTEM,
		EM_INTEL_MISSION_NEWEST_COLONY_SYSTEM,
		EM_INTEL_MISSION_CURRENT_TECH,
		EM_INTEL_MISSION_NO_COMPLETE_TECHS,
		EM_INTEL_MISSION_RECENT_TECH,
		EM_INTEL_MISSION_CRITICAL_SUCCESS,
		EM_WAR_DECLARED_DEFENDER,
		EM_WAR_DECLARED_AGGRESSOR,
		EM_ADMIRAL_CAPTURED,
		EM_ADMIRAL_ESCAPES,
		EM_ADMIRAL_DEFECTS,
		EM_ASTEROID_STORM,
		EM_ATTRIBUTES_DISCOVERED,
		EM_GARDENER_SYSTEM_FOUND,
		EM_INCOMING_ALIEN_FLEET,
		EM_INDY_ASSIMILATED,
		EM_INDY_PROTECTORATE,
		EM_LOCUST_INFESTATION_DEFEATED,
		EM_LOCUST_SHIP_DESTROYED,
		EM_LOCUST_INCOMING,
		EM_LOCUST_SPOTTED,
		EM_MONITOR_FOUND,
		EM_MONITOR_PROJECT_AVAILABLE,
		EM_MONITOR_CAPTURED,
		EM_GARDENER_CAPTURED,
		EM_NEUTRONSTAR_DESTROYED,
		EM_PIRATE_RAID,
		EM_PIRATE_BASE_DESTROYED,
		EM_PROTEANS_REMOVED,
		EM_RESEARCH_FAILED,
		EM_RETROFIT_COMPLETE_SINGLE,
		EM_RETROFIT_COMPLETE_MULTI,
		EM_RETROFIT_COMPLETE_STATION,
		EM_SHIPS_SCATTERED_NODE_CANNON,
		EM_SHIPS_RECYCLED,
		EM_SLAVER_ATTACK,
		EM_SLAVES_DEAD,
		EM_SPECTRE_ATTACK,
		EM_SWARM_DESTROYED,
		EM_SWARM_ENCOUNTERED,
		EM_SWARM_INFESTATION,
		EM_SWARM_QUEEN_DESTROYED,
		EM_SWARM_QUEEN_INCOMING,
		EM_SWARM_QUEEN_SPOTTED,
		EM_SYS_KILLER_DESTROYED,
		EM_SYS_KILLER_INCOMING,
		EM_SYS_KILLER_LEAVING,
		EM_SYS_KILLER_SPOTTED,
		EM_NEUTRON_STAR_DESTROYED_SYSTEM,
		EM_NEUTRON_STAR_NEARBY,
		EM_SUPER_NOVA_TURN,
		EM_SUPER_NOVA_DESTROYED_SYSTEM,
		EM_TOMB_DEFENDERS_DESTROYED,
		EM_TOMB_DESTROYED,
		EM_TOMB_DISCOVERED,
		EM_VN_COLLECTOR_ATTACK,
		EM_VN_SEEKER_ATTACK,
		EM_VN_BERSERKER_ATTACK,
		EM_VN_SYS_KILLER_ATTACK,
		EM_VN_HW_DEFEATED,
		EM_SUULKA_DIES,
		EM_MISSION_COMPLETE,
		EM_AI_REBELLION_END,
		EM_AI_REBELLION_START,
		EM_SLAVES_DELIVERED,
		EM_ALLIANCE_CREATED,
		EM_ALLIANCE_DISSOLVED,
		EM_ASSIMILATION_PLAGUE_PLANET_GAINED,
		EM_ASSIMILATION_PLAGUE_PLANET_LOST,
		EM_BANKRUPTCY_IMMINENT,
		EM_BANKRUPTCY_AVOIDED,
		EM_BANKRUPTCY_COLONY_LOST,
		EM_BIOWEAPON_STRIKE,
		EM_INVOICES_COMPLETE,
		EM_COLONY_ACQUIRED,
		EM_EMPIRE_DESTROYED,
		EM_EMPIRE_ENCOUNTERED,
		EM_FARCAST_FAILED,
		EM_FARCAST_SUCCESS,
		EM_FLEET_DESTROYED,
		EM_FREIGHTER_BUILT,
		EM_FREIGHTERS_BUILT,
		EM_GATE_DESTROYED,
		EM_GATE_JUMP_ABORTED,
		EM_TRADE_STIMULUS,
		EM_COLONY_STIMULUS,
		EM_MINING_STIMULUS,
		EM_LEFT_ALLIANCE,
		EM_PLANET_DESTROYED,
		EM_PLANET_LIFE_DRAINED,
		EM_PLANET_PSI_DRAINED,
		EM_PLANET_NO_RESOURCES,
		EM_PLAYER_SURRENDERED_TO_YOU,
		EM_YOU_SURRENDER,
		EM_PROVINCE_SURRENDERED_TO_YOU,
		EM_SYSTEM_SURRENDERED_TO_YOU,
		EM_SAVINGS_SURRENDERED_TO_YOU,
		EM_STATION_DESTROYED,
		EM_STATION_UPGRADABLE,
		EM_SDB_BUILT,
		EM_SDBS_BUILT,
		EM_COMBAT_DETECTED,
		EM_FLEET_DISBANDED,
		EM_FLEET_REDIRECTED,
		EM_HOMEWORLD_REESTABLISHED,
		EM_COLONY_DESTROYED,
		EM_SURVEY_INDEPENDENT_RACE_FOUND,
		EM_ADMIRAL_INTEL_LEAK_GIVE,
		EM_ADMIRAL_INTEL_LEAK_TAKE,
		EM_COUNTER_INTEL_SUCCESS,
		EM_COUNTER_INTEL_CRITICAL_SUCCESS,
		EM_COUNTER_INTEL_FAILURE,
		EM_COUNTER_INTEL_CRITICAL_FAILURE,
		EM_INCOMING_ASTEROID_SHOWER,
		EM_INCOMING_SPECTORS,
		EM_INCOMING_GHOST_SHIP,
		EM_INCOMING_SLAVERS,
		EM_INCOMING_LOCUST,
		EM_INCOMING_COMET,
		EM_INCOMING_SYSTEMKILLER,
		EM_BOOSTFAILED_1,
		EM_BOOSTFAILED_2,
		EM_BOOSTFAILED_3,
		EM_RESEARCH_PLAGUE_DISASTER,
		EM_RESEARCH_AI_DISASTER,
		EM_INCOMING_NEUTRON_STAR,
		EM_INCOMING_SUPER_NOVA,
		EM_INCOMING_GARDENER,
		EM_RECEIVED_MONEY,
		EM_RECEIVED_RESEARCH_MONEY,
		EM_LOA_CUBES_ABANDONED,
		EM_LOA_CUBES_ABANDONED_DEEPSPACE
	}
}
