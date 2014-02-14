using Kerberos.Sots.Data;
using Kerberos.Sots.Data.ModuleFramework;
using Kerberos.Sots.Data.TechnologyFramework;
using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Data.Xml;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
namespace Kerberos.Sots
{
	internal class AssetDatabase : IDisposable
	{
		public enum MoraleModifierType
		{
			AllColonies,
			Province,
			System,
			Colony
		}
		public struct MoralModifier
		{
			public AssetDatabase.MoraleModifierType type;
			public int value;
		}
		public struct CritHitChances
		{
			public enum CritHitLocationTypes
			{
				CommandSection,
				MissionSection,
				EngineSection,
				Station,
				Monsters,
				LoaCmd,
				LoaMis,
				LoaEng,
				MaxLocations
			}
			public int[] Chances;
		}
		public class MiniMapData
		{
			public int ID = -1;
			public string Location = "";
		}
		public static Dictionary<GovernmentInfo.GovernmentType, Dictionary<MoralEvent, List<AssetDatabase.MoralModifier>>> MoralModifierMap = new Dictionary<GovernmentInfo.GovernmentType, Dictionary<MoralEvent, List<AssetDatabase.MoralModifier>>>
		{

			{
				GovernmentInfo.GovernmentType.Centrism,
				new Dictionary<MoralEvent, List<AssetDatabase.MoralModifier>>
				{

					{
						MoralEvent.ME_GHOSTSHIP_KILLED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 10
							}
						}
					},

					{
						MoralEvent.ME_1MILLION_DEBT,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -1
							}
						}
					},

					{
						MoralEvent.ME_3MILLION_DEBT,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -2
							}
						}
					},

					{
						MoralEvent.ME_LOSE_WORLD_ENEMY,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -1
							},
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Province,
								value = -1
							}
						}
					},

					{
						MoralEvent.ME_LOSE_WORLD_RANDOM,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Province,
								value = -2
							}
						}
					},

					{
						MoralEvent.ME_LOSE_WORLD_GM,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -1
							}
						}
					},

					{
						MoralEvent.ME_LOSE_FORGE,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -3
							},
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Province,
								value = -2
							}
						}
					},

					{
						MoralEvent.ME_LOSE_GEM,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -5
							},
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Province,
								value = -2
							}
						}
					},

					{
						MoralEvent.ME_LOSE_PROVINCE,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -5
							}
						}
					},

					{
						MoralEvent.ME_LOSE_PROVINCE_CAPITAL,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -2
							}
						}
					},

					{
						MoralEvent.ME_LOSE_EMPIRE_CAPITAL,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -10
							}
						}
					},

					{
						MoralEvent.ME_LOSE_LEVIATHAN,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -2
							},
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.System,
								value = -3
							}
						}
					},

					{
						MoralEvent.ME_LOSE_FLAGSHIP,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -1
							},
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.System,
								value = -1
							}
						}
					},

					{
						MoralEvent.ME_LOSE_LVL5_STATION,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -1
							},
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.System,
								value = -2
							}
						}
					},

					{
						MoralEvent.ME_LOSE_STATION,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.System,
								value = -1
							}
						}
					},

					{
						MoralEvent.ME_ADMIRAL_MEDIA_HERO_WIN,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 1
							}
						}
					},

					{
						MoralEvent.ME_ADMIRAL_MEDIA_HERO_KILLED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -6
							}
						}
					},

					{
						MoralEvent.ME_ADMIRAL_KILLED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.System,
								value = -2
							}
						}
					},

					{
						MoralEvent.ME_ADMIRAL_CONVERTED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -1
							},
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Colony,
								value = -2
							}
						}
					},

					{
						MoralEvent.ME_TAX_INCREASED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -1
							}
						}
					},

					{
						MoralEvent.ME_TAX_DECREASED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 1
							}
						}
					},

					{
						MoralEvent.ME_ENEMY_IN_SYSTEM,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.System,
								value = -1
							}
						}
					},

					{
						MoralEvent.ME_WAR_DECLARED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -2
							}
						}
					},

					{
						MoralEvent.ME_BETRAYAL,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -5
							}
						}
					},

					{
						MoralEvent.ME_BETRAYED_LARGER,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -5
							}
						}
					},

					{
						MoralEvent.ME_BETRAYED_SMALLER,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 3
							}
						}
					},

					{
						MoralEvent.ME_WITHDRAW_FROM_ALLIANCE,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -2
							}
						}
					},

					{
						MoralEvent.ME_KICKED_FROM_ALLIANCE,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -1
							}
						}
					},

					{
						MoralEvent.ME_200MILLION_CIV_DEATHS,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -1
							},
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Colony,
								value = -4
							}
						}
					},

					{
						MoralEvent.ME_PLANET_PSI_DRAINED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Colony,
								value = -5
							}
						}
					},

					{
						MoralEvent.ME_PLANET_HEALTH_DRAINED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -1
							},
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Colony,
								value = -9
							}
						}
					},

					{
						MoralEvent.ME_PLAGUE_OUTBREAK,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Colony,
								value = -2
							}
						}
					},

					{
						MoralEvent.ME_ECONOMY_BELOW_15,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Colony,
								value = -1
							}
						}
					},

					{
						MoralEvent.ME_ECONOMY_0,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Colony,
								value = -3
							}
						}
					},

					{
						MoralEvent.ME_10MILLION_SAVINGS,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 1
							}
						}
					},

					{
						MoralEvent.ME_25MILLION_SAVINGS,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 2
							}
						}
					},

					{
						MoralEvent.ME_WORLD_COLONIZED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 1
							},
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Province,
								value = 1
							}
						}
					},

					{
						MoralEvent.ME_PROVINCE_FORMED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 3
							}
						}
					},

					{
						MoralEvent.ME_LVL5_STATION_BUILT,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 1
							},
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Colony,
								value = 4
							}
						}
					},

					{
						MoralEvent.ME_FLAGSHIP_BUILT,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 1
							},
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.System,
								value = 1
							}
						}
					},

					{
						MoralEvent.ME_LEVIATHAN_BUILT,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 2
							},
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.System,
								value = 3
							}
						}
					},

					{
						MoralEvent.ME_ENEMY_LEVIATHAN_DESTROYED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 2
							}
						}
					},

					{
						MoralEvent.ME_ENEMY_FLEET_DESTROYED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 1
							}
						}
					},

					{
						MoralEvent.ME_PERFECT_RANDOM_DEFEATED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.System,
								value = 1
							}
						}
					},

					{
						MoralEvent.ME_GM_DEFEATED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 3
							}
						}
					},

					{
						MoralEvent.ME_FRIENDLY_SUULKA_DEFEATED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -5
							}
						}
					},

					{
						MoralEvent.ME_FRIENDLY_SUULKA_SUMMONED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 5
							}
						}
					},

					{
						MoralEvent.ME_ENEMY_SUULKA_DEFEATED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 5
							}
						}
					},

					{
						MoralEvent.ME_ENEMY_SUULKA_SUMMONED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -5
							}
						}
					},

					{
						MoralEvent.ME_PIRATES_DEFEATED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.System,
								value = 1
							}
						}
					},

					{
						MoralEvent.ME_INDEPENDANT_INCORPORATED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 1
							}
						}
					},

					{
						MoralEvent.ME_INDEPENDANT_CONQUERED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 1
							}
						}
					},

					{
						MoralEvent.ME_INDEPENDANT_DESTROYED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 0
							}
						}
					},

					{
						MoralEvent.ME_ENEMY_PROVINCE_CAPTURED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 3
							}
						}
					},

					{
						MoralEvent.ME_ENEMY_EMPIRE_DESTROYED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 5
							}
						}
					},

					{
						MoralEvent.ME_ENEMY_EMPIRE_SURRENDER,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 10
							}
						}
					},

					{
						MoralEvent.ME_ENEMY_SYSTEM_CONQUERED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 2
							}
						}
					},

					{
						MoralEvent.ME_CIVILIAN_STATION_BUILT,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Province,
								value = 1
							}
						}
					},

					{
						MoralEvent.ME_FORM_PEACE_TREATY,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 2
							}
						}
					},

					{
						MoralEvent.ME_FORM_TRADE_TREATY,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 1
							}
						}
					},

					{
						MoralEvent.ME_GEM_WORLD_FORMED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 3
							},
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.System,
								value = 2
							}
						}
					},

					{
						MoralEvent.ME_FORGE_WORLD_FORMED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 2
							},
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.System,
								value = 1
							}
						}
					},

					{
						MoralEvent.ME_ENEMY_ADMIRAL_KILLED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 1
							}
						}
					},

					{
						MoralEvent.ME_ENEMY_ADMIRAL_CONVERTED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 2
							}
						}
					},

					{
						MoralEvent.ME_ECONOMY_ABOVE_85,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Colony,
								value = 1
							}
						}
					},

					{
						MoralEvent.ME_ECONOMY_100,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Colony,
								value = 2
							}
						}
					},

					{
						MoralEvent.ME_ASTEROID_STRIKE,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.System,
								value = -1
							}
						}
					},

					{
						MoralEvent.ME_OVERPOPULATION_PLAYER,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Colony,
								value = -2
							}
						}
					},

					{
						MoralEvent.ME_OVERPOPULATION_PLANET,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Colony,
								value = -1
							}
						}
					},

					{
						MoralEvent.ME_EVAC_OVERPOPULATION_PLANET,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Colony,
								value = -1
							}
						}
					},

					{
						MoralEvent.ME_REPLICANTS_ON,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Colony,
								value = -1
							}
						}
					},

					{
						MoralEvent.ME_SUPER_NOVA_RADIATION,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.System,
								value = -1
							}
						}
					},

					{
						MoralEvent.ME_ABANDON_COLONY,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Province,
								value = -10
							},
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -5
							}
						}
					},

					{
						MoralEvent.ME_SYSTEM_CLOSE,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.System,
								value = -15
							}
						}
					},

					{
						MoralEvent.ME_SYSTEM_OPEN,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.System,
								value = 10
							}
						}
					}
				}
			},

			{
				GovernmentInfo.GovernmentType.Junta,
				new Dictionary<MoralEvent, List<AssetDatabase.MoralModifier>>
				{

					{
						MoralEvent.ME_LOSE_WORLD_ENEMY,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -2
							},
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Province,
								value = 0
							}
						}
					},

					{
						MoralEvent.ME_LOSE_FORGE,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -3
							},
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Province,
								value = -2
							}
						}
					},

					{
						MoralEvent.ME_LOSE_PROVINCE,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -3
							}
						}
					},

					{
						MoralEvent.ME_LOSE_EMPIRE_CAPITAL,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -12
							}
						}
					},

					{
						MoralEvent.ME_LOSE_LEVIATHAN,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -6
							},
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.System,
								value = -4
							}
						}
					},

					{
						MoralEvent.ME_LOSE_FLAGSHIP,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -3
							},
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.System,
								value = -1
							}
						}
					},

					{
						MoralEvent.ME_LOSE_LVL5_STATION,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -2
							},
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.System,
								value = -2
							}
						}
					},

					{
						MoralEvent.ME_LOSE_STATION,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.System,
								value = -1
							}
						}
					},

					{
						MoralEvent.ME_ADMIRAL_CONVERTED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -3
							},
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Colony,
								value = -6
							}
						}
					},

					{
						MoralEvent.ME_ENEMY_IN_SYSTEM,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.System,
								value = 0
							}
						}
					},

					{
						MoralEvent.ME_WAR_DECLARED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 2
							}
						}
					},

					{
						MoralEvent.ME_BETRAYAL,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 1
							}
						}
					},

					{
						MoralEvent.ME_WITHDRAW_FROM_ALLIANCE,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -1
							}
						}
					},

					{
						MoralEvent.ME_KICKED_FROM_ALLIANCE,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -1
							}
						}
					},

					{
						MoralEvent.ME_INDEPENDANT_INCORPORATED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 0
							}
						}
					},

					{
						MoralEvent.ME_INDEPENDANT_DESTROYED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 0
							}
						}
					},

					{
						MoralEvent.ME_FORM_PEACE_TREATY,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 0
							}
						}
					},

					{
						MoralEvent.ME_FORM_TRADE_TREATY,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 1
							}
						}
					},

					{
						MoralEvent.ME_GEM_WORLD_FORMED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 2
							},
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.System,
								value = 2
							}
						}
					},

					{
						MoralEvent.ME_FORGE_WORLD_FORMED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 3
							},
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.System,
								value = 1
							}
						}
					},

					{
						MoralEvent.ME_ENEMY_ADMIRAL_KILLED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 2
							}
						}
					},

					{
						MoralEvent.ME_ENEMY_ADMIRAL_CONVERTED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 1
							}
						}
					}
				}
			},

			{
				GovernmentInfo.GovernmentType.Plutocracy,
				new Dictionary<MoralEvent, List<AssetDatabase.MoralModifier>>
				{

					{
						MoralEvent.ME_1MILLION_DEBT,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -2
							}
						}
					},

					{
						MoralEvent.ME_3MILLION_DEBT,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -4
							}
						}
					},

					{
						MoralEvent.ME_LOSE_WORLD_RANDOM,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Province,
								value = -4
							}
						}
					},

					{
						MoralEvent.ME_LOSE_WORLD_GM,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -2
							}
						}
					},

					{
						MoralEvent.ME_LOSE_FORGE,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -4
							},
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Province,
								value = -3
							}
						}
					},

					{
						MoralEvent.ME_ECONOMY_BELOW_15,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Colony,
								value = -2
							}
						}
					},

					{
						MoralEvent.ME_ECONOMY_0,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Colony,
								value = -5
							}
						}
					},

					{
						MoralEvent.ME_10MILLION_SAVINGS,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 2
							}
						}
					},

					{
						MoralEvent.ME_25MILLION_SAVINGS,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 3
							}
						}
					},

					{
						MoralEvent.ME_PERFECT_RANDOM_DEFEATED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.System,
								value = 2
							}
						}
					},

					{
						MoralEvent.ME_GM_DEFEATED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 5
							}
						}
					},

					{
						MoralEvent.ME_PIRATES_DEFEATED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.System,
								value = 2
							}
						}
					},

					{
						MoralEvent.ME_FORM_TRADE_TREATY,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 2
							}
						}
					},

					{
						MoralEvent.ME_FORGE_WORLD_FORMED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 3
							},
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Province,
								value = 1
							}
						}
					},

					{
						MoralEvent.ME_ECONOMY_ABOVE_85,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Colony,
								value = 2
							}
						}
					},

					{
						MoralEvent.ME_ECONOMY_100,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Colony,
								value = 3
							}
						}
					}
				}
			},

			{
				GovernmentInfo.GovernmentType.Mercantilism,
				new Dictionary<MoralEvent, List<AssetDatabase.MoralModifier>>
				{

					{
						MoralEvent.ME_1MILLION_DEBT,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -2
							}
						}
					},

					{
						MoralEvent.ME_3MILLION_DEBT,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -3
							}
						}
					},

					{
						MoralEvent.ME_LOSE_WORLD_ENEMY,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -2
							},
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Province,
								value = -1
							}
						}
					},

					{
						MoralEvent.ME_LOSE_WORLD_RANDOM,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Province,
								value = -3
							}
						}
					},

					{
						MoralEvent.ME_LOSE_WORLD_GM,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -2
							}
						}
					},

					{
						MoralEvent.ME_LOSE_FORGE,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -4
							},
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Province,
								value = -2
							}
						}
					},

					{
						MoralEvent.ME_LOSE_GEM,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -6
							},
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Province,
								value = -2
							}
						}
					},

					{
						MoralEvent.ME_LOSE_PROVINCE,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -7
							}
						}
					},

					{
						MoralEvent.ME_LOSE_PROVINCE_CAPITAL,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -3
							}
						}
					},

					{
						MoralEvent.ME_LOSE_EMPIRE_CAPITAL,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -15
							}
						}
					},

					{
						MoralEvent.ME_LOSE_LEVIATHAN,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -3
							},
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.System,
								value = -4
							}
						}
					},

					{
						MoralEvent.ME_LOSE_FLAGSHIP,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -2
							},
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.System,
								value = -1
							}
						}
					},

					{
						MoralEvent.ME_LOSE_LVL5_STATION,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -2
							},
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.System,
								value = -2
							}
						}
					},

					{
						MoralEvent.ME_LOSE_STATION,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.System,
								value = -2
							}
						}
					},

					{
						MoralEvent.ME_ADMIRAL_KILLED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.System,
								value = -3
							}
						}
					},

					{
						MoralEvent.ME_ADMIRAL_CONVERTED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -2
							},
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Colony,
								value = -3
							}
						}
					},

					{
						MoralEvent.ME_TAX_INCREASED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -2
							}
						}
					},

					{
						MoralEvent.ME_ENEMY_IN_SYSTEM,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.System,
								value = -2
							}
						}
					},

					{
						MoralEvent.ME_WAR_DECLARED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -3
							}
						}
					},

					{
						MoralEvent.ME_BETRAYAL,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -7
							}
						}
					},

					{
						MoralEvent.ME_WITHDRAW_FROM_ALLIANCE,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -3
							}
						}
					},

					{
						MoralEvent.ME_KICKED_FROM_ALLIANCE,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -2
							}
						}
					},

					{
						MoralEvent.ME_200MILLION_CIV_DEATHS,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -2
							},
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Colony,
								value = -4
							}
						}
					},

					{
						MoralEvent.ME_PLANET_PSI_DRAINED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Colony,
								value = -7
							}
						}
					},

					{
						MoralEvent.ME_PLANET_HEALTH_DRAINED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -2
							},
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Colony,
								value = -15
							}
						}
					},

					{
						MoralEvent.ME_PLAGUE_OUTBREAK,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Colony,
								value = -3
							}
						}
					},

					{
						MoralEvent.ME_ECONOMY_BELOW_15,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Colony,
								value = -2
							}
						}
					},

					{
						MoralEvent.ME_ECONOMY_0,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Colony,
								value = -4
							}
						}
					},

					{
						MoralEvent.ME_10MILLION_SAVINGS,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 2
							}
						}
					},

					{
						MoralEvent.ME_25MILLION_SAVINGS,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 4
							}
						}
					},

					{
						MoralEvent.ME_FORM_TRADE_TREATY,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 4
							}
						}
					},

					{
						MoralEvent.ME_ECONOMY_ABOVE_85,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Colony,
								value = 2
							}
						}
					},

					{
						MoralEvent.ME_ECONOMY_100,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Colony,
								value = 4
							}
						}
					}
				}
			},

			{
				GovernmentInfo.GovernmentType.Liberationism,
				new Dictionary<MoralEvent, List<AssetDatabase.MoralModifier>>
				{

					{
						MoralEvent.ME_1MILLION_DEBT,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -3
							}
						}
					},

					{
						MoralEvent.ME_3MILLION_DEBT,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -6
							}
						}
					},

					{
						MoralEvent.ME_LOSE_WORLD_ENEMY,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -2
							},
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Province,
								value = -1
							}
						}
					},

					{
						MoralEvent.ME_LOSE_FORGE,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -4
							},
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Province,
								value = -2
							}
						}
					},

					{
						MoralEvent.ME_LOSE_GEM,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -6
							},
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Province,
								value = -2
							}
						}
					},

					{
						MoralEvent.ME_LOSE_PROVINCE,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -7
							}
						}
					},

					{
						MoralEvent.ME_LOSE_PROVINCE_CAPITAL,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -3
							}
						}
					},

					{
						MoralEvent.ME_LOSE_EMPIRE_CAPITAL,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -13
							}
						}
					},

					{
						MoralEvent.ME_LOSE_LEVIATHAN,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -3
							},
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.System,
								value = -3
							}
						}
					},

					{
						MoralEvent.ME_LOSE_FLAGSHIP,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -2
							},
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.System,
								value = -1
							}
						}
					},

					{
						MoralEvent.ME_LOSE_LVL5_STATION,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -2
							},
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.System,
								value = -2
							}
						}
					},

					{
						MoralEvent.ME_LOSE_STATION,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.System,
								value = -1
							}
						}
					},

					{
						MoralEvent.ME_ADMIRAL_KILLED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.System,
								value = -3
							}
						}
					},

					{
						MoralEvent.ME_ADMIRAL_CONVERTED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -2
							},
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Colony,
								value = -3
							}
						}
					},

					{
						MoralEvent.ME_TAX_INCREASED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -3
							}
						}
					},

					{
						MoralEvent.ME_ENEMY_IN_SYSTEM,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.System,
								value = -2
							}
						}
					},

					{
						MoralEvent.ME_WAR_DECLARED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -4
							}
						}
					},

					{
						MoralEvent.ME_BETRAYAL,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -3
							}
						}
					},

					{
						MoralEvent.ME_WITHDRAW_FROM_ALLIANCE,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -4
							}
						}
					},

					{
						MoralEvent.ME_KICKED_FROM_ALLIANCE,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -2
							}
						}
					},

					{
						MoralEvent.ME_200MILLION_CIV_DEATHS,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -2
							},
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Colony,
								value = -5
							}
						}
					},

					{
						MoralEvent.ME_ECONOMY_BELOW_15,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Colony,
								value = -3
							}
						}
					},

					{
						MoralEvent.ME_ECONOMY_0,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Colony,
								value = -6
							}
						}
					},

					{
						MoralEvent.ME_10MILLION_SAVINGS,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 2
							}
						}
					},

					{
						MoralEvent.ME_25MILLION_SAVINGS,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 3
							}
						}
					},

					{
						MoralEvent.ME_WORLD_COLONIZED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 3
							},
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Province,
								value = 1
							}
						}
					},

					{
						MoralEvent.ME_PROVINCE_FORMED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 5
							}
						}
					},

					{
						MoralEvent.ME_FRIENDLY_SUULKA_DEFEATED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -10
							}
						}
					},

					{
						MoralEvent.ME_ENEMY_SUULKA_DEFEATED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 10
							}
						}
					},

					{
						MoralEvent.ME_PIRATES_DEFEATED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.System,
								value = 3
							}
						}
					},

					{
						MoralEvent.ME_INDEPENDANT_INCORPORATED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 1
							}
						}
					},

					{
						MoralEvent.ME_INDEPENDANT_CONQUERED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -3
							}
						}
					},

					{
						MoralEvent.ME_INDEPENDANT_DESTROYED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -3
							}
						}
					},

					{
						MoralEvent.ME_FORM_PEACE_TREATY,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 4
							}
						}
					},

					{
						MoralEvent.ME_FORM_TRADE_TREATY,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 4
							}
						}
					},

					{
						MoralEvent.ME_ECONOMY_ABOVE_85,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Colony,
								value = 3
							}
						}
					},

					{
						MoralEvent.ME_ECONOMY_100,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Colony,
								value = 6
							}
						}
					}
				}
			},

			{
				GovernmentInfo.GovernmentType.Anarchism,
				new Dictionary<MoralEvent, List<AssetDatabase.MoralModifier>>
				{

					{
						MoralEvent.ME_WAR_DECLARED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -3
							}
						}
					},

					{
						MoralEvent.ME_BETRAYAL,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -3
							}
						}
					},

					{
						MoralEvent.ME_WITHDRAW_FROM_ALLIANCE,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -4
							}
						}
					},

					{
						MoralEvent.ME_KICKED_FROM_ALLIANCE,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -2
							}
						}
					},

					{
						MoralEvent.ME_200MILLION_CIV_DEATHS,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -2
							},
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Colony,
								value = -5
							}
						}
					},

					{
						MoralEvent.ME_PLANET_PSI_DRAINED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Colony,
								value = -7
							}
						}
					},

					{
						MoralEvent.ME_WORLD_COLONIZED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 2
							},
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Province,
								value = 2
							}
						}
					},

					{
						MoralEvent.ME_PROVINCE_FORMED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 5
							}
						}
					},

					{
						MoralEvent.ME_PERFECT_RANDOM_DEFEATED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.System,
								value = 3
							}
						}
					},

					{
						MoralEvent.ME_GM_DEFEATED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 5
							}
						}
					},

					{
						MoralEvent.ME_FRIENDLY_SUULKA_DEFEATED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -10
							}
						}
					},

					{
						MoralEvent.ME_ENEMY_SUULKA_DEFEATED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 10
							}
						}
					},

					{
						MoralEvent.ME_PIRATES_DEFEATED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.System,
								value = 2
							}
						}
					},

					{
						MoralEvent.ME_INDEPENDANT_INCORPORATED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 3
							}
						}
					},

					{
						MoralEvent.ME_INDEPENDANT_CONQUERED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -1
							}
						}
					},

					{
						MoralEvent.ME_INDEPENDANT_DESTROYED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -2
							}
						}
					},

					{
						MoralEvent.ME_FORM_PEACE_TREATY,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 6
							}
						}
					},

					{
						MoralEvent.ME_FORM_TRADE_TREATY,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 2
							}
						}
					},

					{
						MoralEvent.ME_GEM_WORLD_FORMED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 5
							},
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.System,
								value = 2
							}
						}
					},

					{
						MoralEvent.ME_ENEMY_ADMIRAL_CONVERTED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 4
							}
						}
					}
				}
			},

			{
				GovernmentInfo.GovernmentType.Cooperativism,
				new Dictionary<MoralEvent, List<AssetDatabase.MoralModifier>>
				{

					{
						MoralEvent.ME_1MILLION_DEBT,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 0
							}
						}
					},

					{
						MoralEvent.ME_3MILLION_DEBT,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 0
							}
						}
					},

					{
						MoralEvent.ME_LOSE_WORLD_ENEMY,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -3
							},
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Province,
								value = -3
							}
						}
					},

					{
						MoralEvent.ME_LOSE_WORLD_RANDOM,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Province,
								value = -2
							}
						}
					},

					{
						MoralEvent.ME_TAX_INCREASED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 0
							}
						}
					},

					{
						MoralEvent.ME_WAR_DECLARED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -3
							}
						}
					},

					{
						MoralEvent.ME_WITHDRAW_FROM_ALLIANCE,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -3
							}
						}
					},

					{
						MoralEvent.ME_KICKED_FROM_ALLIANCE,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -2
							}
						}
					},

					{
						MoralEvent.ME_200MILLION_CIV_DEATHS,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -3
							},
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Colony,
								value = -7
							}
						}
					},

					{
						MoralEvent.ME_WORLD_COLONIZED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 1
							},
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Province,
								value = 2
							}
						}
					},

					{
						MoralEvent.ME_PROVINCE_FORMED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 4
							}
						}
					},

					{
						MoralEvent.ME_FRIENDLY_SUULKA_DEFEATED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -7
							}
						}
					},

					{
						MoralEvent.ME_ENEMY_SUULKA_DEFEATED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 7
							}
						}
					},

					{
						MoralEvent.ME_INDEPENDANT_INCORPORATED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 3
							}
						}
					},

					{
						MoralEvent.ME_INDEPENDANT_CONQUERED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -1
							}
						}
					},

					{
						MoralEvent.ME_INDEPENDANT_DESTROYED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -1
							}
						}
					},

					{
						MoralEvent.ME_FORM_PEACE_TREATY,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 3
							}
						}
					},

					{
						MoralEvent.ME_GEM_WORLD_FORMED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 5
							},
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.System,
								value = 2
							}
						}
					},

					{
						MoralEvent.ME_ENEMY_ADMIRAL_CONVERTED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 3
							}
						}
					}
				}
			},

			{
				GovernmentInfo.GovernmentType.Socialism,
				new Dictionary<MoralEvent, List<AssetDatabase.MoralModifier>>
				{

					{
						MoralEvent.ME_LOSE_WORLD_ENEMY,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -2
							},
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Province,
								value = -2
							}
						}
					},

					{
						MoralEvent.ME_LOSE_WORLD_RANDOM,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Province,
								value = -3
							}
						}
					},

					{
						MoralEvent.ME_LOSE_WORLD_GM,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -2
							}
						}
					},

					{
						MoralEvent.ME_LOSE_FORGE,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -4
							},
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Province,
								value = -3
							}
						}
					},

					{
						MoralEvent.ME_LOSE_GEM,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -7
							},
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Province,
								value = -10
							}
						}
					},

					{
						MoralEvent.ME_LOSE_PROVINCE,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -7
							}
						}
					},

					{
						MoralEvent.ME_LOSE_PROVINCE_CAPITAL,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -3
							}
						}
					},

					{
						MoralEvent.ME_LOSE_EMPIRE_CAPITAL,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -15
							}
						}
					},

					{
						MoralEvent.ME_200MILLION_CIV_DEATHS,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -2
							},
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Colony,
								value = -5
							}
						}
					},

					{
						MoralEvent.ME_PLANET_PSI_DRAINED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Colony,
								value = -6
							}
						}
					},

					{
						MoralEvent.ME_PLANET_HEALTH_DRAINED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -2
							},
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Colony,
								value = -11
							}
						}
					},

					{
						MoralEvent.ME_INDEPENDANT_INCORPORATED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 2
							}
						}
					},

					{
						MoralEvent.ME_INDEPENDANT_CONQUERED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 0
							}
						}
					},

					{
						MoralEvent.ME_INDEPENDANT_DESTROYED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -1
							}
						}
					},

					{
						MoralEvent.ME_FORM_PEACE_TREATY,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 4
							}
						}
					},

					{
						MoralEvent.ME_FORM_TRADE_TREATY,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 1
							}
						}
					}
				}
			},

			{
				GovernmentInfo.GovernmentType.Communalism,
				new Dictionary<MoralEvent, List<AssetDatabase.MoralModifier>>
				{

					{
						MoralEvent.ME_1MILLION_DEBT,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = 0
							}
						}
					},

					{
						MoralEvent.ME_3MILLION_DEBT,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -1
							}
						}
					},

					{
						MoralEvent.ME_LOSE_WORLD_ENEMY,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -1
							}
						}
					},

					{
						MoralEvent.ME_LOSE_FORGE,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -2
							},
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Province,
								value = -1
							}
						}
					},

					{
						MoralEvent.ME_LOSE_GEM,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -3
							},
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Province,
								value = -1
							}
						}
					},

					{
						MoralEvent.ME_LOSE_PROVINCE,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -2
							}
						}
					},

					{
						MoralEvent.ME_LOSE_PROVINCE_CAPITAL,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -1
							}
						}
					},

					{
						MoralEvent.ME_LOSE_EMPIRE_CAPITAL,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -5
							}
						}
					},

					{
						MoralEvent.ME_LOSE_LEVIATHAN,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -2
							}
						}
					},

					{
						MoralEvent.ME_LOSE_FLAGSHIP,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -1
							}
						}
					},

					{
						MoralEvent.ME_LOSE_LVL5_STATION,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -1
							}
						}
					},

					{
						MoralEvent.ME_LOSE_STATION,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.System,
								value = 0
							}
						}
					},

					{
						MoralEvent.ME_ADMIRAL_KILLED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.System,
								value = -1
							}
						}
					},

					{
						MoralEvent.ME_ADMIRAL_CONVERTED,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.AllColonies,
								value = -2
							},
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.Colony,
								value = -3
							}
						}
					},

					{
						MoralEvent.ME_ENEMY_IN_SYSTEM,
						new List<AssetDatabase.MoralModifier>
						{
							new AssetDatabase.MoralModifier
							{
								type = AssetDatabase.MoraleModifierType.System,
								value = 0
							}
						}
					}
				}
			}
		};
		public static Dictionary<ModuleEnums.StationModuleType, string> StationModuleTypeAssetMap = new Dictionary<ModuleEnums.StationModuleType, string>
		{

			{
				ModuleEnums.StationModuleType.Sensor,
				"factions\\{0}\\modules\\sn_sensor.module"
			},

			{
				ModuleEnums.StationModuleType.Customs,
				"factions\\{0}\\modules\\sn_customs.module"
			},

			{
				ModuleEnums.StationModuleType.Combat,
				"factions\\{0}\\modules\\sn_combat.module"
			},

			{
				ModuleEnums.StationModuleType.Repair,
				"factions\\{0}\\modules\\sn_repair.module"
			},

			{
				ModuleEnums.StationModuleType.Warehouse,
				"factions\\{0}\\modules\\sn_warehouse.module"
			},

			{
				ModuleEnums.StationModuleType.Command,
				"factions\\{0}\\modules\\sn_command.module"
			},

			{
				ModuleEnums.StationModuleType.Dock,
				"factions\\{0}\\modules\\sn_dock.module"
			},

			{
				ModuleEnums.StationModuleType.Terraform,
				"factions\\{0}\\modules\\sn_terraform.module"
			},

			{
				ModuleEnums.StationModuleType.Bastion,
				"factions\\{0}\\modules\\sn_bastion.module"
			},

			{
				ModuleEnums.StationModuleType.Amp,
				"factions\\{0}\\modules\\sn_gate_amplifier.module"
			},

			{
				ModuleEnums.StationModuleType.Defence,
				"factions\\{0}\\modules\\sn_defence.module"
			},

			{
				ModuleEnums.StationModuleType.AlienHabitation,
				"factions\\{0}\\modules\\sn_habitation_{0}.module"
			},

			{
				ModuleEnums.StationModuleType.Habitation,
				"factions\\{0}\\modules\\sn_habitation_{0}.module"
			},

			{
				ModuleEnums.StationModuleType.HumanHabitation,
				"factions\\human\\modules\\sn_habitation_human.module"
			},

			{
				ModuleEnums.StationModuleType.TarkasHabitation,
				"factions\\tarkas\\modules\\sn_habitation_tarkas.module"
			},

			{
				ModuleEnums.StationModuleType.LiirHabitation,
				"factions\\liir_zuul\\modules\\sn_habitation_liir_zuul.module"
			},

			{
				ModuleEnums.StationModuleType.HiverHabitation,
				"factions\\hiver\\modules\\sn_habitation_hiver.module"
			},

			{
				ModuleEnums.StationModuleType.MorrigiHabitation,
				"factions\\morrigi\\modules\\sn_habitation_morrigi.module"
			},

			{
				ModuleEnums.StationModuleType.ZuulHabitation,
				"factions\\zuul\\modules\\sn_habitation_zuul.module"
			},

			{
				ModuleEnums.StationModuleType.LoaHabitation,
				"factions\\loa\\modules\\sn_habitation_loa.module"
			},

			{
				ModuleEnums.StationModuleType.LargeHabitation,
				"factions\\{0}\\modules\\sn_large_habitation_{0}.module"
			},

			{
				ModuleEnums.StationModuleType.LargeAlienHabitation,
				"factions\\{0}\\modules\\sn_large_habitation_{0}.module"
			},

			{
				ModuleEnums.StationModuleType.HumanLargeHabitation,
				"factions\\human\\modules\\sn_large_habitation_human.module"
			},

			{
				ModuleEnums.StationModuleType.TarkasLargeHabitation,
				"factions\\tarkas\\modules\\sn_large_habitation_tarkas.module"
			},

			{
				ModuleEnums.StationModuleType.LiirLargeHabitation,
				"factions\\liir_zuul\\modules\\sn_large_habitation_liir_zuul.module"
			},

			{
				ModuleEnums.StationModuleType.HiverLargeHabitation,
				"factions\\hiver\\modules\\sn_large_habitation_hiver.module"
			},

			{
				ModuleEnums.StationModuleType.MorrigiLargeHabitation,
				"factions\\morrigi\\modules\\sn_large_habitation_morrigi.module"
			},

			{
				ModuleEnums.StationModuleType.ZuulLargeHabitation,
				"factions\\zuul\\modules\\sn_large_habitation_zuul.module"
			},

			{
				ModuleEnums.StationModuleType.LoaLargeHabitation,
				"factions\\loa\\modules\\sn_large_habitation_loa.module"
			},

			{
				ModuleEnums.StationModuleType.HumanHabitationForeign,
				"factions\\human\\modules\\sn_habitation_human.module"
			},

			{
				ModuleEnums.StationModuleType.TarkasHabitationForeign,
				"factions\\tarkas\\modules\\sn_habitation_tarkas.module"
			},

			{
				ModuleEnums.StationModuleType.LiirHabitationForeign,
				"factions\\liir_zuul\\modules\\sn_habitation_liir_zuul.module"
			},

			{
				ModuleEnums.StationModuleType.HiverHabitationForeign,
				"factions\\hiver\\modules\\sn_habitation_hiver.module"
			},

			{
				ModuleEnums.StationModuleType.MorrigiHabitationForeign,
				"factions\\morrigi\\modules\\sn_habitation_morrigi.module"
			},

			{
				ModuleEnums.StationModuleType.ZuulHabitationForeign,
				"factions\\zuul\\modules\\sn_habitation_zuul.module"
			},

			{
				ModuleEnums.StationModuleType.LoaHabitationForeign,
				"factions\\loa\\modules\\sn_habitation_loa.module"
			},

			{
				ModuleEnums.StationModuleType.HumanLargeHabitationForeign,
				"factions\\human\\modules\\sn_large_habitation_human.module"
			},

			{
				ModuleEnums.StationModuleType.TarkasLargeHabitationForeign,
				"factions\\tarkas\\modules\\sn_large_habitation_tarkas.module"
			},

			{
				ModuleEnums.StationModuleType.LiirLargeHabitationForeign,
				"factions\\liir_zuul\\modules\\sn_large_habitation_liir_zuul.module"
			},

			{
				ModuleEnums.StationModuleType.HiverLargeHabitationForeign,
				"factions\\hiver\\modules\\sn_large_habitation_hiver.module"
			},

			{
				ModuleEnums.StationModuleType.MorrigiLargeHabitationForeign,
				"factions\\morrigi\\modules\\sn_large_habitation_morrigi.module"
			},

			{
				ModuleEnums.StationModuleType.ZuulLargeHabitationForeign,
				"factions\\zuul\\modules\\sn_large_habitation_zuul.module"
			},

			{
				ModuleEnums.StationModuleType.LoaLargeHabitationForeign,
				"factions\\loa\\modules\\sn_large_habitation_loa.module"
			},

			{
				ModuleEnums.StationModuleType.GateLab,
				"factions\\{0}\\modules\\sn_gate_lab.module"
			},

			{
				ModuleEnums.StationModuleType.Lab,
				"factions\\{0}\\modules\\sn_lab.module"
			},

			{
				ModuleEnums.StationModuleType.EWPLab,
				"factions\\{0}\\modules\\sn_lab.module"
			},

			{
				ModuleEnums.StationModuleType.TRPLab,
				"factions\\{0}\\modules\\sn_lab.module"
			},

			{
				ModuleEnums.StationModuleType.NRGLab,
				"factions\\{0}\\modules\\sn_lab.module"
			},

			{
				ModuleEnums.StationModuleType.WARLab,
				"factions\\{0}\\modules\\sn_lab.module"
			},

			{
				ModuleEnums.StationModuleType.BALLab,
				"factions\\{0}\\modules\\sn_lab.module"
			},

			{
				ModuleEnums.StationModuleType.BIOLab,
				"factions\\{0}\\modules\\sn_lab.module"
			},

			{
				ModuleEnums.StationModuleType.INDLab,
				"factions\\{0}\\modules\\sn_lab.module"
			},

			{
				ModuleEnums.StationModuleType.CCCLab,
				"factions\\{0}\\modules\\sn_lab.module"
			},

			{
				ModuleEnums.StationModuleType.DRVLab,
				"factions\\{0}\\modules\\sn_lab.module"
			},

			{
				ModuleEnums.StationModuleType.POLLab,
				"factions\\{0}\\modules\\sn_lab.module"
			},

			{
				ModuleEnums.StationModuleType.PSILab,
				"factions\\{0}\\modules\\sn_lab.module"
			},

			{
				ModuleEnums.StationModuleType.ENGLab,
				"factions\\{0}\\modules\\sn_lab.module"
			},

			{
				ModuleEnums.StationModuleType.BRDLab,
				"factions\\{0}\\modules\\sn_lab.module"
			},

			{
				ModuleEnums.StationModuleType.SLDLab,
				"factions\\{0}\\modules\\sn_lab.module"
			},

			{
				ModuleEnums.StationModuleType.CYBLab,
				"factions\\{0}\\modules\\sn_lab.module"
			}
		};
		public static Dictionary<ModuleEnums.StationModuleType, ModuleEnums.ModuleSlotTypes> StationModuleTypeToMountTypeMap = new Dictionary<ModuleEnums.StationModuleType, ModuleEnums.ModuleSlotTypes>
		{

			{
				ModuleEnums.StationModuleType.Sensor,
				ModuleEnums.ModuleSlotTypes.Sensor
			},

			{
				ModuleEnums.StationModuleType.Customs,
				ModuleEnums.ModuleSlotTypes.Customs
			},

			{
				ModuleEnums.StationModuleType.Combat,
				ModuleEnums.ModuleSlotTypes.Combat
			},

			{
				ModuleEnums.StationModuleType.Repair,
				ModuleEnums.ModuleSlotTypes.Repair
			},

			{
				ModuleEnums.StationModuleType.Warehouse,
				ModuleEnums.ModuleSlotTypes.Warehouse
			},

			{
				ModuleEnums.StationModuleType.Command,
				ModuleEnums.ModuleSlotTypes.Command
			},

			{
				ModuleEnums.StationModuleType.Dock,
				ModuleEnums.ModuleSlotTypes.Dock
			},

			{
				ModuleEnums.StationModuleType.Terraform,
				ModuleEnums.ModuleSlotTypes.Terraform
			},

			{
				ModuleEnums.StationModuleType.Bastion,
				ModuleEnums.ModuleSlotTypes.Bastion
			},

			{
				ModuleEnums.StationModuleType.Amp,
				ModuleEnums.ModuleSlotTypes.Amp
			},

			{
				ModuleEnums.StationModuleType.Defence,
				ModuleEnums.ModuleSlotTypes.Defence
			},

			{
				ModuleEnums.StationModuleType.GateLab,
				ModuleEnums.ModuleSlotTypes.GateLab
			},

			{
				ModuleEnums.StationModuleType.AlienHabitation,
				ModuleEnums.ModuleSlotTypes.Habitation
			},

			{
				ModuleEnums.StationModuleType.Habitation,
				ModuleEnums.ModuleSlotTypes.Habitation
			},

			{
				ModuleEnums.StationModuleType.HumanHabitation,
				ModuleEnums.ModuleSlotTypes.Habitation
			},

			{
				ModuleEnums.StationModuleType.TarkasHabitation,
				ModuleEnums.ModuleSlotTypes.Habitation
			},

			{
				ModuleEnums.StationModuleType.LiirHabitation,
				ModuleEnums.ModuleSlotTypes.Habitation
			},

			{
				ModuleEnums.StationModuleType.HiverHabitation,
				ModuleEnums.ModuleSlotTypes.Habitation
			},

			{
				ModuleEnums.StationModuleType.MorrigiHabitation,
				ModuleEnums.ModuleSlotTypes.Habitation
			},

			{
				ModuleEnums.StationModuleType.ZuulHabitation,
				ModuleEnums.ModuleSlotTypes.Habitation
			},

			{
				ModuleEnums.StationModuleType.LoaHabitation,
				ModuleEnums.ModuleSlotTypes.Habitation
			},

			{
				ModuleEnums.StationModuleType.LargeHabitation,
				ModuleEnums.ModuleSlotTypes.LargeHabitation
			},

			{
				ModuleEnums.StationModuleType.LargeAlienHabitation,
				ModuleEnums.ModuleSlotTypes.LargeHabitation
			},

			{
				ModuleEnums.StationModuleType.HumanLargeHabitation,
				ModuleEnums.ModuleSlotTypes.LargeHabitation
			},

			{
				ModuleEnums.StationModuleType.TarkasLargeHabitation,
				ModuleEnums.ModuleSlotTypes.LargeHabitation
			},

			{
				ModuleEnums.StationModuleType.LiirLargeHabitation,
				ModuleEnums.ModuleSlotTypes.LargeHabitation
			},

			{
				ModuleEnums.StationModuleType.HiverLargeHabitation,
				ModuleEnums.ModuleSlotTypes.LargeHabitation
			},

			{
				ModuleEnums.StationModuleType.MorrigiLargeHabitation,
				ModuleEnums.ModuleSlotTypes.LargeHabitation
			},

			{
				ModuleEnums.StationModuleType.ZuulLargeHabitation,
				ModuleEnums.ModuleSlotTypes.LargeHabitation
			},

			{
				ModuleEnums.StationModuleType.LoaLargeHabitation,
				ModuleEnums.ModuleSlotTypes.LargeHabitation
			},

			{
				ModuleEnums.StationModuleType.HumanHabitationForeign,
				ModuleEnums.ModuleSlotTypes.AlienHabitation
			},

			{
				ModuleEnums.StationModuleType.TarkasHabitationForeign,
				ModuleEnums.ModuleSlotTypes.AlienHabitation
			},

			{
				ModuleEnums.StationModuleType.LiirHabitationForeign,
				ModuleEnums.ModuleSlotTypes.AlienHabitation
			},

			{
				ModuleEnums.StationModuleType.HiverHabitationForeign,
				ModuleEnums.ModuleSlotTypes.AlienHabitation
			},

			{
				ModuleEnums.StationModuleType.MorrigiHabitationForeign,
				ModuleEnums.ModuleSlotTypes.AlienHabitation
			},

			{
				ModuleEnums.StationModuleType.ZuulHabitationForeign,
				ModuleEnums.ModuleSlotTypes.AlienHabitation
			},

			{
				ModuleEnums.StationModuleType.LoaHabitationForeign,
				ModuleEnums.ModuleSlotTypes.AlienHabitation
			},

			{
				ModuleEnums.StationModuleType.HumanLargeHabitationForeign,
				ModuleEnums.ModuleSlotTypes.LargeAlienHabitation
			},

			{
				ModuleEnums.StationModuleType.TarkasLargeHabitationForeign,
				ModuleEnums.ModuleSlotTypes.LargeAlienHabitation
			},

			{
				ModuleEnums.StationModuleType.LiirLargeHabitationForeign,
				ModuleEnums.ModuleSlotTypes.LargeAlienHabitation
			},

			{
				ModuleEnums.StationModuleType.HiverLargeHabitationForeign,
				ModuleEnums.ModuleSlotTypes.LargeAlienHabitation
			},

			{
				ModuleEnums.StationModuleType.MorrigiLargeHabitationForeign,
				ModuleEnums.ModuleSlotTypes.LargeAlienHabitation
			},

			{
				ModuleEnums.StationModuleType.ZuulLargeHabitationForeign,
				ModuleEnums.ModuleSlotTypes.LargeAlienHabitation
			},

			{
				ModuleEnums.StationModuleType.LoaLargeHabitationForeign,
				ModuleEnums.ModuleSlotTypes.LargeAlienHabitation
			},

			{
				ModuleEnums.StationModuleType.Lab,
				ModuleEnums.ModuleSlotTypes.Lab
			},

			{
				ModuleEnums.StationModuleType.EWPLab,
				ModuleEnums.ModuleSlotTypes.Lab
			},

			{
				ModuleEnums.StationModuleType.TRPLab,
				ModuleEnums.ModuleSlotTypes.Lab
			},

			{
				ModuleEnums.StationModuleType.NRGLab,
				ModuleEnums.ModuleSlotTypes.Lab
			},

			{
				ModuleEnums.StationModuleType.WARLab,
				ModuleEnums.ModuleSlotTypes.Lab
			},

			{
				ModuleEnums.StationModuleType.BALLab,
				ModuleEnums.ModuleSlotTypes.Lab
			},

			{
				ModuleEnums.StationModuleType.BIOLab,
				ModuleEnums.ModuleSlotTypes.Lab
			},

			{
				ModuleEnums.StationModuleType.INDLab,
				ModuleEnums.ModuleSlotTypes.Lab
			},

			{
				ModuleEnums.StationModuleType.CCCLab,
				ModuleEnums.ModuleSlotTypes.Lab
			},

			{
				ModuleEnums.StationModuleType.DRVLab,
				ModuleEnums.ModuleSlotTypes.Lab
			},

			{
				ModuleEnums.StationModuleType.POLLab,
				ModuleEnums.ModuleSlotTypes.Lab
			},

			{
				ModuleEnums.StationModuleType.PSILab,
				ModuleEnums.ModuleSlotTypes.Lab
			},

			{
				ModuleEnums.StationModuleType.ENGLab,
				ModuleEnums.ModuleSlotTypes.Lab
			},

			{
				ModuleEnums.StationModuleType.BRDLab,
				ModuleEnums.ModuleSlotTypes.Lab
			},

			{
				ModuleEnums.StationModuleType.SLDLab,
				ModuleEnums.ModuleSlotTypes.Lab
			},

			{
				ModuleEnums.StationModuleType.CYBLab,
				ModuleEnums.ModuleSlotTypes.Lab
			}
		};
		private Dictionary<string, AssetDatabase.MiniMapData> _miniShipMap = new Dictionary<string, AssetDatabase.MiniMapData>();
		public Dictionary<int, Dictionary<ModuleEnums.StationModuleType, int>> DiplomaticStationUpgradeRequirements = new Dictionary<int, Dictionary<ModuleEnums.StationModuleType, int>>
		{

			{
				1,
				new Dictionary<ModuleEnums.StationModuleType, int>
				{

					{
						ModuleEnums.StationModuleType.Habitation,
						1
					},

					{
						ModuleEnums.StationModuleType.AlienHabitation,
						1
					},

					{
						ModuleEnums.StationModuleType.Customs,
						1
					}
				}
			},

			{
				2,
				new Dictionary<ModuleEnums.StationModuleType, int>
				{

					{
						ModuleEnums.StationModuleType.Habitation,
						2
					},

					{
						ModuleEnums.StationModuleType.AlienHabitation,
						1
					},

					{
						ModuleEnums.StationModuleType.Dock,
						1
					}
				}
			},

			{
				3,
				new Dictionary<ModuleEnums.StationModuleType, int>
				{

					{
						ModuleEnums.StationModuleType.Habitation,
						2
					},

					{
						ModuleEnums.StationModuleType.AlienHabitation,
						1
					},

					{
						ModuleEnums.StationModuleType.Dock,
						2
					},

					{
						ModuleEnums.StationModuleType.Sensor,
						1
					}
				}
			},

			{
				4,
				new Dictionary<ModuleEnums.StationModuleType, int>
				{

					{
						ModuleEnums.StationModuleType.Habitation,
						3
					},

					{
						ModuleEnums.StationModuleType.AlienHabitation,
						3
					},

					{
						ModuleEnums.StationModuleType.Dock,
						1
					},

					{
						ModuleEnums.StationModuleType.Sensor,
						1
					},

					{
						ModuleEnums.StationModuleType.LargeHabitation,
						1
					},

					{
						ModuleEnums.StationModuleType.LargeAlienHabitation,
						1
					}
				}
			}
		};
		public Dictionary<int, Dictionary<ModuleEnums.StationModuleType, int>> NavalStationUpgradeRequirements = new Dictionary<int, Dictionary<ModuleEnums.StationModuleType, int>>
		{

			{
				1,
				new Dictionary<ModuleEnums.StationModuleType, int>
				{

					{
						ModuleEnums.StationModuleType.Warehouse,
						1
					},

					{
						ModuleEnums.StationModuleType.Repair,
						1
					},

					{
						ModuleEnums.StationModuleType.Sensor,
						1
					},

					{
						ModuleEnums.StationModuleType.Command,
						1
					},

					{
						ModuleEnums.StationModuleType.Dock,
						1
					}
				}
			},

			{
				2,
				new Dictionary<ModuleEnums.StationModuleType, int>
				{

					{
						ModuleEnums.StationModuleType.Warehouse,
						3
					},

					{
						ModuleEnums.StationModuleType.Repair,
						1
					},

					{
						ModuleEnums.StationModuleType.Sensor,
						1
					},

					{
						ModuleEnums.StationModuleType.Command,
						1
					},

					{
						ModuleEnums.StationModuleType.Dock,
						2
					}
				}
			},

			{
				3,
				new Dictionary<ModuleEnums.StationModuleType, int>
				{

					{
						ModuleEnums.StationModuleType.Warehouse,
						5
					},

					{
						ModuleEnums.StationModuleType.Repair,
						3
					},

					{
						ModuleEnums.StationModuleType.Sensor,
						1
					},

					{
						ModuleEnums.StationModuleType.Command,
						3
					},

					{
						ModuleEnums.StationModuleType.Dock,
						3
					}
				}
			},

			{
				4,
				new Dictionary<ModuleEnums.StationModuleType, int>
				{

					{
						ModuleEnums.StationModuleType.Warehouse,
						3
					},

					{
						ModuleEnums.StationModuleType.Repair,
						3
					},

					{
						ModuleEnums.StationModuleType.Sensor,
						1
					},

					{
						ModuleEnums.StationModuleType.Command,
						3
					},

					{
						ModuleEnums.StationModuleType.Dock,
						5
					},

					{
						ModuleEnums.StationModuleType.Combat,
						3
					}
				}
			}
		};
		public Dictionary<int, Dictionary<ModuleEnums.StationModuleType, int>> ScienceStationUpgradeRequirements = new Dictionary<int, Dictionary<ModuleEnums.StationModuleType, int>>
		{

			{
				1,
				new Dictionary<ModuleEnums.StationModuleType, int>
				{

					{
						ModuleEnums.StationModuleType.Lab,
						1
					},

					{
						ModuleEnums.StationModuleType.Habitation,
						1
					},

					{
						ModuleEnums.StationModuleType.Sensor,
						1
					},

					{
						ModuleEnums.StationModuleType.Dock,
						1
					}
				}
			},

			{
				2,
				new Dictionary<ModuleEnums.StationModuleType, int>
				{

					{
						ModuleEnums.StationModuleType.Lab,
						2
					},

					{
						ModuleEnums.StationModuleType.Habitation,
						1
					},

					{
						ModuleEnums.StationModuleType.Sensor,
						1
					},

					{
						ModuleEnums.StationModuleType.Dock,
						1
					}
				}
			},

			{
				3,
				new Dictionary<ModuleEnums.StationModuleType, int>
				{

					{
						ModuleEnums.StationModuleType.Lab,
						3
					},

					{
						ModuleEnums.StationModuleType.Habitation,
						1
					},

					{
						ModuleEnums.StationModuleType.Dock,
						1
					},

					{
						ModuleEnums.StationModuleType.Warehouse,
						2
					}
				}
			},

			{
				4,
				new Dictionary<ModuleEnums.StationModuleType, int>
				{

					{
						ModuleEnums.StationModuleType.Lab,
						5
					},

					{
						ModuleEnums.StationModuleType.Habitation,
						1
					},

					{
						ModuleEnums.StationModuleType.Dock,
						1
					},

					{
						ModuleEnums.StationModuleType.Warehouse,
						2
					},

					{
						ModuleEnums.StationModuleType.Sensor,
						1
					}
				}
			}
		};
		public Dictionary<int, Dictionary<ModuleEnums.StationModuleType, int>> CivilianStationUpgradeRequirements = new Dictionary<int, Dictionary<ModuleEnums.StationModuleType, int>>
		{

			{
				1,
				new Dictionary<ModuleEnums.StationModuleType, int>
				{

					{
						ModuleEnums.StationModuleType.Habitation,
						1
					},

					{
						ModuleEnums.StationModuleType.Dock,
						1
					},

					{
						ModuleEnums.StationModuleType.Warehouse,
						1
					}
				}
			},

			{
				2,
				new Dictionary<ModuleEnums.StationModuleType, int>
				{

					{
						ModuleEnums.StationModuleType.Habitation,
						2
					},

					{
						ModuleEnums.StationModuleType.Dock,
						2
					},

					{
						ModuleEnums.StationModuleType.Warehouse,
						1
					}
				}
			},

			{
				3,
				new Dictionary<ModuleEnums.StationModuleType, int>
				{

					{
						ModuleEnums.StationModuleType.Habitation,
						2
					},

					{
						ModuleEnums.StationModuleType.Dock,
						2
					},

					{
						ModuleEnums.StationModuleType.Warehouse,
						1
					},

					{
						ModuleEnums.StationModuleType.AlienHabitation,
						1
					},

					{
						ModuleEnums.StationModuleType.Sensor,
						1
					}
				}
			},

			{
				4,
				new Dictionary<ModuleEnums.StationModuleType, int>
				{

					{
						ModuleEnums.StationModuleType.LargeHabitation,
						3
					},

					{
						ModuleEnums.StationModuleType.LargeAlienHabitation,
						1
					},

					{
						ModuleEnums.StationModuleType.Dock,
						2
					},

					{
						ModuleEnums.StationModuleType.Warehouse,
						2
					},

					{
						ModuleEnums.StationModuleType.Sensor,
						1
					}
				}
			}
		};
		public Dictionary<int, Dictionary<ModuleEnums.StationModuleType, int>> GateStationUpgradeRequirements = new Dictionary<int, Dictionary<ModuleEnums.StationModuleType, int>>
		{

			{
				1,
				new Dictionary<ModuleEnums.StationModuleType, int>
				{

					{
						ModuleEnums.StationModuleType.Habitation,
						1
					},

					{
						ModuleEnums.StationModuleType.Dock,
						1
					},

					{
						ModuleEnums.StationModuleType.Bastion,
						2
					},

					{
						ModuleEnums.StationModuleType.Amp,
						2
					},

					{
						ModuleEnums.StationModuleType.Sensor,
						1
					}
				}
			},

			{
				2,
				new Dictionary<ModuleEnums.StationModuleType, int>
				{

					{
						ModuleEnums.StationModuleType.Habitation,
						1
					},

					{
						ModuleEnums.StationModuleType.Amp,
						3
					},

					{
						ModuleEnums.StationModuleType.GateLab,
						1
					},

					{
						ModuleEnums.StationModuleType.Bastion,
						3
					},

					{
						ModuleEnums.StationModuleType.Sensor,
						2
					}
				}
			},

			{
				3,
				new Dictionary<ModuleEnums.StationModuleType, int>
				{

					{
						ModuleEnums.StationModuleType.Habitation,
						2
					},

					{
						ModuleEnums.StationModuleType.Dock,
						1
					},

					{
						ModuleEnums.StationModuleType.GateLab,
						2
					},

					{
						ModuleEnums.StationModuleType.Bastion,
						1
					},

					{
						ModuleEnums.StationModuleType.Sensor,
						1
					},

					{
						ModuleEnums.StationModuleType.Combat,
						2
					},

					{
						ModuleEnums.StationModuleType.Amp,
						3
					}
				}
			},

			{
				4,
				new Dictionary<ModuleEnums.StationModuleType, int>
				{

					{
						ModuleEnums.StationModuleType.Habitation,
						2
					},

					{
						ModuleEnums.StationModuleType.Dock,
						1
					},

					{
						ModuleEnums.StationModuleType.GateLab,
						3
					},

					{
						ModuleEnums.StationModuleType.Bastion,
						2
					},

					{
						ModuleEnums.StationModuleType.Sensor,
						2
					},

					{
						ModuleEnums.StationModuleType.Combat,
						2
					},

					{
						ModuleEnums.StationModuleType.Amp,
						4
					}
				}
			}
		};
		private Dictionary<RandomEncounter, int> _randomEncounterOdds = new Dictionary<RandomEncounter, int>();
		private Dictionary<EasterEgg, int> _easterEggOdds = new Dictionary<EasterEgg, int>();
		private Dictionary<EasterEgg, int> _gmOdds = new Dictionary<EasterEgg, int>();
		private GovernmentEffects _governmentEffects = new GovernmentEffects();
		private Dictionary<StratModifiers, object> _defaultStratModifiers = new Dictionary<StratModifiers, object>
		{

			{
				StratModifiers.StartProvincePlanets,
				3
			},

			{
				StratModifiers.MinProvincePlanets,
				3
			},

			{
				StratModifiers.MaxProvincePlanets,
				3
			},

			{
				StratModifiers.MaxProvincePlanetRange,
				8f
			},

			{
				StratModifiers.AllowPoliceInCombat,
				false
			},

			{
				StratModifiers.PoliceMoralBonus,
				0
			},

			{
				StratModifiers.AllowWorldSurrender,
				false
			},

			{
				StratModifiers.AllowProvinceSurrender,
				false
			},

			{
				StratModifiers.AllowEmpireSurrender,
				false
			},

			{
				StratModifiers.PrototypeConstructionCostModifierPF,
				3f
			},

			{
				StratModifiers.PrototypeSavingsCostModifierPF,
				3f
			},

			{
				StratModifiers.PrototypeConstructionCostModifierCR,
				2.5f
			},

			{
				StratModifiers.ConstructionCostModifierCR,
				1f
			},

			{
				StratModifiers.PrototypeConstructionCostModifierDN,
				2f
			},

			{
				StratModifiers.ConstructionCostModifierDN,
				1f
			},

			{
				StratModifiers.PrototypeConstructionCostModifierLV,
				1.5f
			},

			{
				StratModifiers.ConstructionCostModifierLV,
				1f
			},

			{
				StratModifiers.ConstructionCostModifierSN,
				1f
			},

			{
				StratModifiers.PrototypeSavingsCostModifierCR,
				4f
			},

			{
				StratModifiers.PrototypeSavingsCostModifierDN,
				3f
			},

			{
				StratModifiers.PrototypeSavingsCostModifierLV,
				2.5f
			},

			{
				StratModifiers.PrototypeTimeModifier,
				1f
			},

			{
				StratModifiers.ShowPrototypeDesignAttributes,
				false
			},

			{
				StratModifiers.BadDesignAttributePercent,
				10
			},

			{
				StratModifiers.GoodDesignAttributePercent,
				10
			},

			{
				StratModifiers.IndustrialOutputModifier,
				1f
			},

			{
				StratModifiers.PopulationGrowthModifier,
				1f
			},

			{
				StratModifiers.AdditionalMaxCivilianPopulation,
				0f
			},

			{
				StratModifiers.AdditionalMaxImperialPopulation,
				0f
			},

			{
				StratModifiers.SalvageModifier,
				0f
			},

			{
				StratModifiers.OddsOfRandomEncounter,
				0.1f
			},

			{
				StratModifiers.OverharvestModifier,
				10f
			},

			{
				StratModifiers.MinOverharvestRate,
				0f
			},

			{
				StratModifiers.OverharvestFromPopulationModifier,
				20f
			},

			{
				StratModifiers.OverPopulationPercentage,
				0.51f
			},

			{
				StratModifiers.TechFeasibilityDeviation,
				0.5f
			},

			{
				StratModifiers.SlaveDeathRateModifier,
				1f
			},

			{
				StratModifiers.NavyStationSensorCloakBonus,
				1f
			},

			{
				StratModifiers.ScienceStationSensorCloakBonus,
				1f
			},

			{
				StratModifiers.AllowPrivateerMission,
				false
			},

			{
				StratModifiers.StripMiningMaximum,
				0.1f
			},

			{
				StratModifiers.TerraformingModifier,
				1f
			},

			{
				StratModifiers.BiosphereDestructionModifier,
				1f
			},

			{
				StratModifiers.PlagueDeathModifier,
				1f
			},

			{
				StratModifiers.ColonySupportCostModifier,
				1f
			},

			{
				StratModifiers.AllowHardenedStructures,
				false
			},

			{
				StratModifiers.AllowPlanetBeam,
				false
			},

			{
				StratModifiers.AllowMirvPlanetaryMissiles,
				false
			},

			{
				StratModifiers.AllowDeepSpaceConstruction,
				false
			},

			{
				StratModifiers.SlaveProductionModifier,
				1f
			},

			{
				StratModifiers.AllowTradeEnclave,
				false
			},

			{
				StratModifiers.AllowProtectorate,
				false
			},

			{
				StratModifiers.AllowIncorporate,
				false
			},

			{
				StratModifiers.AllowAlienPopulations,
				false
			},

			{
				StratModifiers.AlienCivilianTaxRate,
				1f
			},

			{
				StratModifiers.ComparativeAnalysys,
				false
			},

			{
				StratModifiers.MassInductionProjectors,
				false
			},

			{
				StratModifiers.StandingNeutrinoWaves,
				false
			},

			{
				StratModifiers.AllowIdealAlienGrowthRate,
				false
			},

			{
				StratModifiers.AllowAlienImmigration,
				false
			},

			{
				StratModifiers.DiplomaticOfferingModifier,
				1f
			},

			{
				StratModifiers.AdmiralCareerModifier,
				1f
			},

			{
				StratModifiers.ChanceOfPirates,
				1f
			},

			{
				StratModifiers.AIBenefitBonus,
				1f
			},

			{
				StratModifiers.AIResearchBonus,
				0f
			},

			{
				StratModifiers.AIRevenueBonus,
				0f
			},

			{
				StratModifiers.AIProductionBonus,
				0f
			},

			{
				StratModifiers.ConstructionPointBonus,
				1f
			},

			{
				StratModifiers.AllowAIRebellion,
				false
			},

			{
				StratModifiers.ImmuneToSpectre,
				false
			},

			{
				StratModifiers.WarpDriveStratSignatureModifier,
				1f
			},

			{
				StratModifiers.UseFastestShipForFTLSpeed,
				false
			},

			{
				StratModifiers.AllowSuperWorlds,
				false
			},

			{
				StratModifiers.DiplomacyPointCostModifier,
				1f
			},

			{
				StratModifiers.NegativeRelationsModifier,
				1f
			},

			{
				StratModifiers.AllowOneFightRebellionEnding,
				false
			},

			{
				StratModifiers.DiplomaticReactionBonus,
				1f
			},

			{
				StratModifiers.MoralBonus,
				0
			},

			{
				StratModifiers.PsiResearchModifier,
				1f
			},

			{
				StratModifiers.ResearchModifier,
				1f
			},

			{
				StratModifiers.GlobalResearchModifier,
				1f
			},

			{
				StratModifiers.ResearchBreakthroughModifier,
				1f
			},

			{
				StratModifiers.AllowFarSense,
				false
			},

			{
				StratModifiers.C3ResearchModifier,
				1f
			},

			{
				StratModifiers.LeviathanResearchModifier,
				1f
			},

			{
				StratModifiers.AsteroidMonitorResearchModifier,
				1f
			},

			{
				StratModifiers.AdmiralReactionModifier,
				1f
			},

			{
				StratModifiers.GrandMenaceWarningTime,
				0
			},

			{
				StratModifiers.RandomEncounterWarningPercent,
				0f
			},

			{
				StratModifiers.SurveyTimeModifier,
				1f
			},

			{
				StratModifiers.DomeStageModifier,
				0
			},

			{
				StratModifiers.CavernDmodModifier,
				0f
			},

			{
				StratModifiers.IntelSuccessModifier,
				1f
			},

			{
				StratModifiers.EnemyIntelSuccessModifier,
				1f
			},

			{
				StratModifiers.EnemyOperationsSuccessModifier,
				1f
			},

			{
				StratModifiers.CounterIntelSuccessModifier,
				1f
			},

			{
				StratModifiers.ShipSupplyModifier,
				1f
			},

			{
				StratModifiers.WarehouseCapacityModifier,
				1f
			},

			{
				StratModifiers.ScrapShipModifier,
				1f
			},

			{
				StratModifiers.MaxColonizableHazard,
				650
			},

			{
				StratModifiers.MaxFlockBonusMod,
				1f
			},

			{
				StratModifiers.EnableTrade,
				false
			},

			{
				StratModifiers.TradeRevenue,
				1f
			},

			{
				StratModifiers.TaxRevenue,
				1f
			},

			{
				StratModifiers.IORevenue,
				1f
			},

			{
				StratModifiers.TradeRangeModifier,
				1f
			},

			{
				StratModifiers.GateCastDistance,
				0f
			},

			{
				StratModifiers.GateCastDeviation,
				0.05f
			},

			{
				StratModifiers.BoreSpeedModifier,
				0.66f
			},

			{
				StratModifiers.BoardingPartyModifier,
				1f
			},

			{
				StratModifiers.PhaseDislocationARBonus,
				0
			},

			{
				StratModifiers.PsiPotentialModifier,
				1f
			},

			{
				StratModifiers.PsiPotentialApplyModifier,
				100f
			},

			{
				StratModifiers.RequiresSterileEnvironment,
				false
			},

			{
				StratModifiers.MutableFleets,
				false
			},

			{
				StratModifiers.ColonyStarvation,
				true
			},

			{
				StratModifiers.DiplomacyReactionKillShips,
				-5
			},

			{
				StratModifiers.DiplomacyReactionKillColony,
				-75
			},

			{
				StratModifiers.DiplomacyReactionKillEnemy,
				4
			},

			{
				StratModifiers.DiplomacyReactionColonizeClaimedWorld,
				-20
			},

			{
				StratModifiers.DiplomacyReactionKillRaceWorld,
				-60
			},

			{
				StratModifiers.DiplomacyReactionKillGrandMenace,
				100
			},

			{
				StratModifiers.DiplomacyReactionInvadeIndependentWorld,
				-10
			},

			{
				StratModifiers.DiplomacyReactionSellSlaves,
				-75
			},

			{
				StratModifiers.DiplomacyReactionAIRebellion,
				-100
			},

			{
				StratModifiers.DiplomacyReactionKillSuulka,
				50
			},

			{
				StratModifiers.DiplomacyReactionMoney,
				20
			},

			{
				StratModifiers.DiplomacyReactionResearch,
				10
			},

			{
				StratModifiers.DiplomacyReactionSlave,
				0
			},

			{
				StratModifiers.DiplomacyReactionStarChamber,
				50
			},

			{
				StratModifiers.DiplomacyReactionDeclareWar,
				-2000
			},

			{
				StratModifiers.DiplomacyReactionPeaceTreaty,
				5
			},

			{
				StratModifiers.DiplomacyReactionBiggerEmpire,
				-3
			},

			{
				StratModifiers.DiplomacyReactionSmallerEmpire,
				0
			},

			{
				StratModifiers.DiplomacyReactionBetrayed,
				25
			},

			{
				StratModifiers.DiplomacyReactionBetrayal,
				-100
			},

			{
				StratModifiers.DiplomacyDemandWeight,
				1f
			},

			{
				StratModifiers.DiplomacyRequestWeight,
				1f
			},

			{
				StratModifiers.DiplomacyReactionRandomReductionHackMinimum,
				-20
			},

			{
				StratModifiers.DiplomacyReactionRandomReductionHackMaximum,
				-1
			}
		};
		private Dictionary<AIDifficulty, Dictionary<DifficultyModifiers, float>> AIDifficultyBonuses;
		private Dictionary<string, Dictionary<string, object>> _techBonuses;
		private Dictionary<string, GovActionValues> _govActionModifiers;
		private int _randomEncMinTurns;
		private int _randomEncTurnsToResetOdds;
		private float _randomEncMinOdds;
		private float _randomEncMaxOdds;
		private float _randomEncBaseOdds;
		private float _randomEncDecOddsCombat;
		private float _randomEncIncOddsIdle;
		private float _randomEncIncOddsRounds;
		private int _randomEncTurnsToExclude;
		private float _randomEncSinglePlayerOdds;
		private int _largeCombatThreshold;
		private float _infrastructureSupplyRatio;
		private float _populationNoise;
		private float _civilianPopulationGrowthRateMod;
		private float _civilianPopulationTriggerAmount;
		private float _civilianPopulationStartAmount;
		private int _civilianPopulationStartMoral;
		private int _diplomacyPointsPerProvince;
		private int[] _diplomacyPointsPerStation;
		private float _globalProductionModifier;
		private float _stationSupportRangeModifier;
		private float _colonySupportCostFactor;
		private float _baseCorruptionRate;
		private int _evacCivPerCol;
		private int _maxGovernmentShift;
		private float _defaultTacSensorRange;
		private float _defaultBRTacSensorRange;
		private float _defaultPlanetTacSensorRange;
		private float _defaultStratSensorRange;
		private float _policePatrolRadius;
		private int _grandMenaceMinTurn;
		private int _grandMenaceChance;
		private XmlElement _globals;
		private Dictionary<string, object> _cachedGlobals;
		private SwarmerGlobalData _globalSwarmerData;
		private MeteorShowerGlobalData _globalMeteorShowerData;
		private CometGlobalData _globalCometData;
		private NeutronStarGlobalData _globalNeutronStarData;
		private SuperNovaGlobalData _globalSuperNovaData;
		private SlaverGlobalData _globalSlaverData;
		private SpectreGlobalData _globalSpectreData;
		private AsteroidMonitorGlobalData _globalAsteroidMonitorData;
		private MorrigiRelicGlobalData _globalMorrigiRelicData;
		private GardenerGlobalData _globalGardenerData;
		private VonNeumannGlobalData _globalVonNeumannData;
		private LocustGlobalData _globalLocustData;
		private PiracyGlobalData _globalPiracyData;
		private GlobalSpotterRangeData _globalSpotterRanges;
		private float _aiRebellionChance;
		private float _aiRebellionColonyPercent;
		private float _encounterMinStartOffset;
		private float _encounterMaxStartOffset;
		private float _interceptThreshold;
		private float _tradePointPerFreighterFleet;
		private float _populationPerTradePoint;
		private float _incomePerInternationalTradePointMoved;
		private float _incomePerProvincialTradePointMoved;
		private float _incomePerGenericTradePointMoved;
		private float _taxDivider;
		private float _maxDebtMultiplier;
		private int _bankruptcyTurns;
		private float _provinceTradeModifier;
		private float _empireTradeModifier;
		private float _citizensPerImmigrationPoint;
		private int _moralBonusPerPoliceShip;
		private float _populationPerPlanetBeam;
		private float _populationPerPlanetMirv;
		private float _populationPerPlanetMissile;
		private float _populationPerHeavyPlanetMissile;
		private float _planetMissileLaunchDelay;
		private float _planetBeamLaunchDelay;
		private float _forgeWorldImpMaxBonus;
		private float _forgeWorldIOBonus;
		private float _gemWorldCivMaxBonus;
		private int _superWorldSizeConstraint;
		private float _superWorldModifier;
		private float _maxOverharvestRate;
		private float _randomEncOddsPerOrbital;
		private int _securityPointCost;
		private int _requiredIntelPointsForMission;
		private int _requiredCounterIntelPointsForMission;
		private int _colonyFleetSupportPoints;
		private int _stationLvl1FleetSupportPoints;
		private int _stationLvl2FleetSupportPoints;
		private int _stationLvl3FleetSupportPoints;
		private int _stationLvl4FleetSupportPoints;
		private int _stationLvl5FleetSupportPoints;
		private float _minSlaveDeathRate;
		private float _maxSlaveDeathRate;
		private float _imperialProductionMultiplier;
		private float _slaveProductionMultiplier;
		private float _civilianProductionMultiplier;
		private int _miningStationIOBonus;
		private float _flockMaxBonus;
		private float _flockBRBonus;
		private float _flockCRBonus;
		private float _flockDNBonus;
		private float _flockLVBonus;
		private int _flockBRCountBonus;
		private int _flockCRCountBonus;
		private int _flockDNCountBonus;
		private int _flockLVCountBonus;
		private int _declareWarPointCost;
		private int _requestResearchPointCost;
		private int _requestMilitaryAssistancePointCost;
		private int _requestGatePointCost;
		private int _requestEnclavePointCost;
		private int _requestSystemInfoPointCost;
		private int _requestSavingsPointCost;
		private int _demandSavingsPointCost;
		private int _demandResearchPointCost;
		private int _demandSystemInfoPointCost;
		private int _demandSlavesPointCost;
		private int _demandSystemPointCost;
		private int _demandProvincePointCost;
		private int _demandEmpirePointCost;
		private int _treatyArmisticeWarNeutralPointCost;
		private int _treatyArmisticeNeutralCeasefirePointCost;
		private int _treatyArmisticeNeutralNonAggroPointCost;
		private int _treatyArmisticeCeaseFireNonAggroPointCost;
		private int _treatyArmisticeCeaseFirePeacePointCost;
		private int _treatyArmisticeNonAggroPeaceCost;
		private int _treatyArmisticePeaceAlliancePointCost;
		private int _treatyTradePointCost;
		private int _treatyProtectoratePointCost;
		private int _treatyIncorporatePointCost;
		private int _treatyLimitationShipClassPointCost;
		private int _treatyLimitationFleetsPointCost;
		private int _treatyLimitationWeaponsPointCost;
		private int _treatyLimitationResearchTechPointCost;
		private int _treatyLimitationResearchTreePointCost;
		private int _treatyLimitationColoniesPointCost;
		private int _treatyLimitationForgeGemWorldsPointCost;
		private int _treatyLimitationStationType;
		private int _stimulusColonizationBonus;
		private int _stimulusMiningMin;
		private int _stimulusMiningMax;
		private int _stimulusColonizationMin;
		private int _stimulusColonizationMax;
		private int _stimulusTradeMin;
		private int _stimulusTradeMax;
		private int _StationsPerPopulation;
		private int _minLoaCubesOnBuild;
		private int _maxLoaCubesOnBuild;
		private int _LoaCostPerCube;
		private float _LoaDistanceBetweenGates;
		private int _LoaBaseMaxMass;
		private int _LoaMassInductionProjectorsMaxMass;
		private int _LoaMassStandingPulseWavesMaxMass;
		private float _LoaGateSystemMargin;
		private float _LoaTechModMod;
		private float _HomeworldTaxBonusMod;
		private Vector3 _pieChartColourShipMaintenance = default(Vector3);
		private Vector3 _pieChartColourPlanetaryDevelopment = default(Vector3);
		private Vector3 _pieChartColourDebtInterest = default(Vector3);
		private Vector3 _pieChartColourResearch = default(Vector3);
		private Vector3 _pieChartColourSecurity = default(Vector3);
		private Vector3 _pieChartColourStimulus = default(Vector3);
		private Vector3 _pieChartColourSavings = default(Vector3);
		private Vector3 _pieChartColourCorruption = default(Vector3);
		private int _accumulatedKnowledgeWeaponPerBattleMin = 5;
		private int _accumulatedKnowledgeWeaponPerBattleMax = 10;
		private int _upkeepBattleRider;
		private int _upkeepCruiser;
		private int _upkeepDreadnaught;
		private int _upkeepLeviathan;
		private int _upkeepDefensePlatform;
		private int[] _upkeepScienceStation = new int[5];
		private int[] _upkeepNavalStation = new int[5];
		private int[] _upkeepDiplomaticStation = new int[5];
		private int[] _upkeepCivilianStation = new int[5];
		private int[] _upkeepGateStation = new int[5];
		private float _eliteUpkeepCostScale = 1.5f;
		private float _starSystemEntryPointRange;
		private float _tacStealthArmorBonus;
		private float _slewModeMultiplier = 1f;
		private float _slewModeDecelMultiplier = 1f;
		private float _slewModeExitRange = 10000f;
		private float _slewModeEnterOffset = 2000f;
		private MineFieldParams _mineFieldParams = default(MineFieldParams);
		private SpecialProjectData _specialProjectData = default(SpecialProjectData);
		private DefenseManagerSettings _defenseManagerSettings = default(DefenseManagerSettings);
		public Vector3 RandomEncounterPrimaryColor = new Vector3(0.3f, 0.3f, 0.3f);
		private AssetDatabase.CritHitChances[] _critHitChances = new AssetDatabase.CritHitChances[8];
		private string[] _commonMaterialDictionaries;
		private SkyDefinition[] _skyDefinitions;
		private LogicalTurretHousing[] _turretHousings;
		private LogicalWeapon[] _weapons;
		private LogicalModule[] _modules;
		private LogicalModule[] _modulesToAssignByDefault;
		private LogicalPsionic[] _psionics;
		private SuulkaPsiBonus[] _suulkaPsiBonuses;
		private LogicalShield[] _shields;
		private LogicalShipSpark[] _shipSparks;
		private LogicalEffect _shipEMPEffect;
		private Faction[] _factions;
		private Race[] _races;
		private HashSet<ShipSectionAsset> _shipSections;
		private Dictionary<string, ShipSectionAsset> _shipSectionsByFilename;
		private TechTree _masterTechTree;
		private Kerberos.Sots.Data.TechnologyFramework.Tech[] _masterTechTreeRoots;
		private List<string> _techTreeModels;
		private List<string> _techTreeRoots;
		private static CommonStrings _commonStrings;
		private readonly PlanetGraphicsRules _planetgenrules;
		private readonly string[] _splashScreenImageNames;
		private readonly Random _random = new Random();
		private List<FleetTemplate> _fleetTemplates;
		public readonly Dictionary<DiplomacyStateChange, int> DiplomacyStateChangeMap;
		public GovernmentEffects GovEffects
		{
			get
			{
				return this._governmentEffects;
			}
		}
		public int RandomEncMinTurns
		{
			get
			{
				return this._randomEncMinTurns;
			}
		}
		public int RandomEncTurnsToResetOdds
		{
			get
			{
				return this._randomEncTurnsToResetOdds;
			}
		}
		public float RandomEncMinOdds
		{
			get
			{
				return this._randomEncMinOdds;
			}
		}
		public float RandomEncMaxOdds
		{
			get
			{
				return this._randomEncMaxOdds;
			}
		}
		public float RandomEncBaseOdds
		{
			get
			{
				return this._randomEncBaseOdds;
			}
		}
		public float RandomEncDecOddsCombat
		{
			get
			{
				return this._randomEncDecOddsCombat;
			}
		}
		public float RandomEncIncOddsIdle
		{
			get
			{
				return this._randomEncIncOddsIdle;
			}
		}
		public float RandomEncIncOddsRounds
		{
			get
			{
				return this._randomEncIncOddsRounds;
			}
		}
		public int RandomEncTurnsToExclude
		{
			get
			{
				return this._randomEncTurnsToExclude;
			}
		}
		public float RandomEncSinglePlayerOdds
		{
			get
			{
				return this._randomEncSinglePlayerOdds;
			}
		}
		public int LargeCombatThreshold
		{
			get
			{
				return this._largeCombatThreshold;
			}
		}
		public float InfrastructureSupplyRatio
		{
			get
			{
				return this._infrastructureSupplyRatio;
			}
		}
		public float PopulationNoise
		{
			get
			{
				return this._populationNoise;
			}
		}
		public float CivilianPopulationGrowthRateMod
		{
			get
			{
				return this._civilianPopulationGrowthRateMod;
			}
		}
		public float CivilianPopulationTriggerAmount
		{
			get
			{
				return this._civilianPopulationTriggerAmount;
			}
		}
		public float CivilianPopulationStartAmount
		{
			get
			{
				return this._civilianPopulationStartAmount;
			}
		}
		public int CivilianPopulationStartMoral
		{
			get
			{
				return this._civilianPopulationStartMoral;
			}
		}
		public int DiplomacyPointsPerProvince
		{
			get
			{
				return this._diplomacyPointsPerProvince;
			}
		}
		public int[] DiplomacyPointsPerStation
		{
			get
			{
				return this._diplomacyPointsPerStation;
			}
		}
		public float GlobalProductionModifier
		{
			get
			{
				return this._globalProductionModifier;
			}
		}
		public float StationSupportRangeModifier
		{
			get
			{
				return this._stationSupportRangeModifier;
			}
		}
		public float ColonySupportCostFactor
		{
			get
			{
				return this._colonySupportCostFactor;
			}
		}
		public float BaseCorruptionRate
		{
			get
			{
				return this._baseCorruptionRate;
			}
		}
		public int EvacCivPerCol
		{
			get
			{
				return this._evacCivPerCol;
			}
		}
		public int MaxGovernmentShift
		{
			get
			{
				return this._maxGovernmentShift;
			}
		}
		public float DefaultTacSensorRange
		{
			get
			{
				return this._defaultTacSensorRange;
			}
		}
		public float DefaultBRTacSensorRange
		{
			get
			{
				return this._defaultBRTacSensorRange;
			}
		}
		public float DefaultPlanetSensorRange
		{
			get
			{
				return this._defaultPlanetTacSensorRange;
			}
		}
		public float DefaultStratSensorRange
		{
			get
			{
				return this._defaultStratSensorRange;
			}
		}
		public float PolicePatrolRadius
		{
			get
			{
				return this._policePatrolRadius;
			}
		}
		public int GrandMenaceMinTurn
		{
			get
			{
				return this._grandMenaceMinTurn;
			}
		}
		public int GrandMenaceChance
		{
			get
			{
				return this._grandMenaceChance;
			}
		}
		public SwarmerGlobalData GlobalSwarmerData
		{
			get
			{
				return this._globalSwarmerData;
			}
		}
		public MeteorShowerGlobalData GlobalMeteorShowerData
		{
			get
			{
				return this._globalMeteorShowerData;
			}
		}
		public CometGlobalData GlobalCometData
		{
			get
			{
				return this._globalCometData;
			}
		}
		public NeutronStarGlobalData GlobalNeutronStarData
		{
			get
			{
				return this._globalNeutronStarData;
			}
		}
		public SuperNovaGlobalData GlobalSuperNovaData
		{
			get
			{
				return this._globalSuperNovaData;
			}
		}
		public SlaverGlobalData GlobalSlaverData
		{
			get
			{
				return this._globalSlaverData;
			}
		}
		public SpectreGlobalData GlobalSpectreData
		{
			get
			{
				return this._globalSpectreData;
			}
		}
		public AsteroidMonitorGlobalData GlobalAsteroidMonitorData
		{
			get
			{
				return this._globalAsteroidMonitorData;
			}
		}
		public MorrigiRelicGlobalData GlobalMorrigiRelicData
		{
			get
			{
				return this._globalMorrigiRelicData;
			}
		}
		public GardenerGlobalData GlobalGardenerData
		{
			get
			{
				return this._globalGardenerData;
			}
		}
		public VonNeumannGlobalData GlobalVonNeumannData
		{
			get
			{
				return this._globalVonNeumannData;
			}
		}
		public LocustGlobalData GlobalLocustData
		{
			get
			{
				return this._globalLocustData;
			}
		}
		public PiracyGlobalData GlobalPiracyData
		{
			get
			{
				return this._globalPiracyData;
			}
		}
		public GlobalSpotterRangeData GlobalSpotterRangeData
		{
			get
			{
				return this._globalSpotterRanges;
			}
		}
		public float AIRebellionChance
		{
			get
			{
				return this._aiRebellionChance;
			}
		}
		public float AIRebellionColonyPercent
		{
			get
			{
				return this._aiRebellionColonyPercent;
			}
		}
		public float MinEncounterStartPos
		{
			get
			{
				return this._encounterMinStartOffset;
			}
		}
		public float MaxEncounterStartPos
		{
			get
			{
				return this._encounterMaxStartOffset;
			}
		}
		public float InterceptThreshold
		{
			get
			{
				return this._interceptThreshold;
			}
		}
		public float TradePointPerFreightFleet
		{
			get
			{
				return this._tradePointPerFreighterFleet;
			}
		}
		public float PopulationPerTradePoint
		{
			get
			{
				return this._populationPerTradePoint;
			}
		}
		public float IncomePerInternationalTradePointMoved
		{
			get
			{
				return this._incomePerInternationalTradePointMoved;
			}
		}
		public float IncomePerProvincialTradePointMoved
		{
			get
			{
				return this._incomePerProvincialTradePointMoved;
			}
		}
		public float IncomePerGenericTradePointMoved
		{
			get
			{
				return this._incomePerGenericTradePointMoved;
			}
		}
		public float TaxDivider
		{
			get
			{
				return this._taxDivider;
			}
		}
		public float MaxDebtMultiplier
		{
			get
			{
				return this._maxDebtMultiplier;
			}
		}
		public int BankruptcyTurns
		{
			get
			{
				return this._bankruptcyTurns;
			}
		}
		public float ProvinceTradeModifier
		{
			get
			{
				return this._provinceTradeModifier;
			}
		}
		public float EmpireTradeModifier
		{
			get
			{
				return this._empireTradeModifier;
			}
		}
		public float CitizensPerImmigrationPoint
		{
			get
			{
				return this._citizensPerImmigrationPoint;
			}
		}
		public int MoralBonusPerPoliceShip
		{
			get
			{
				return this._moralBonusPerPoliceShip;
			}
		}
		public float PopulationPerPlanetBeam
		{
			get
			{
				return this._populationPerPlanetBeam;
			}
		}
		public float PopulationPerPlanetMirv
		{
			get
			{
				return this._populationPerPlanetMirv;
			}
		}
		public float PopulationPerPlanetMissile
		{
			get
			{
				return this._populationPerPlanetMissile;
			}
		}
		public float PopulationPerHeavyPlanetMissile
		{
			get
			{
				return this._populationPerHeavyPlanetMissile;
			}
		}
		public float PlanetMissileDelay
		{
			get
			{
				return this._planetMissileLaunchDelay;
			}
		}
		public float PlanetBeamDelay
		{
			get
			{
				return this._planetBeamLaunchDelay;
			}
		}
		public float ForgeWorldImpMaxBonus
		{
			get
			{
				return this._forgeWorldImpMaxBonus;
			}
		}
		public float ForgeWorldIOBonus
		{
			get
			{
				return this._forgeWorldIOBonus;
			}
		}
		public float GemWorldCivMaxBonus
		{
			get
			{
				return this._gemWorldCivMaxBonus;
			}
		}
		public int SuperWorldSizeConstraint
		{
			get
			{
				return this._superWorldSizeConstraint;
			}
		}
		public float SuperWorldModifier
		{
			get
			{
				return this._superWorldModifier;
			}
		}
		public float MaxOverharvestRate
		{
			get
			{
				return this._maxOverharvestRate;
			}
		}
		public float RandomEncOddsPerOrbital
		{
			get
			{
				return this._randomEncOddsPerOrbital;
			}
		}
		public int SecurityPointCost
		{
			get
			{
				return this._securityPointCost;
			}
		}
		public int RequiredIntelPointsForMission
		{
			get
			{
				return this._requiredIntelPointsForMission;
			}
		}
		public int RequiredCounterIntelPointsForMission
		{
			get
			{
				return this._requiredCounterIntelPointsForMission;
			}
		}
		public int ColonyFleetSupportPoints
		{
			get
			{
				return this._colonyFleetSupportPoints;
			}
		}
		public int StationLvl1FleetSupportPoints
		{
			get
			{
				return this._stationLvl1FleetSupportPoints;
			}
		}
		public int StationLvl2FleetSupportPoints
		{
			get
			{
				return this._stationLvl2FleetSupportPoints;
			}
		}
		public int StationLvl3FleetSupportPoints
		{
			get
			{
				return this._stationLvl3FleetSupportPoints;
			}
		}
		public int StationLvl4FleetSupportPoints
		{
			get
			{
				return this._stationLvl4FleetSupportPoints;
			}
		}
		public int StationLvl5FleetSupportPoints
		{
			get
			{
				return this._stationLvl5FleetSupportPoints;
			}
		}
		public float MinSlaveDeathRate
		{
			get
			{
				return this._minSlaveDeathRate;
			}
		}
		public float MaxSlaveDeathRate
		{
			get
			{
				return this._maxSlaveDeathRate;
			}
		}
		public float ImperialProductionMultiplier
		{
			get
			{
				return this._imperialProductionMultiplier;
			}
		}
		public float SlaveProductionMultiplier
		{
			get
			{
				return this._slaveProductionMultiplier;
			}
		}
		public float CivilianProductionMultiplier
		{
			get
			{
				return this._civilianProductionMultiplier;
			}
		}
		public int MiningStationIOBonus
		{
			get
			{
				return this._miningStationIOBonus;
			}
		}
		public float FlockMaxBonus
		{
			get
			{
				return this._flockMaxBonus;
			}
		}
		public float FlockBRBonus
		{
			get
			{
				return this._flockBRBonus;
			}
		}
		public float FlockCRBonus
		{
			get
			{
				return this._flockCRBonus;
			}
		}
		public float FlockDNBonus
		{
			get
			{
				return this._flockDNBonus;
			}
		}
		public float FlockLVBonus
		{
			get
			{
				return this._flockLVBonus;
			}
		}
		public int FlockBRCountBonus
		{
			get
			{
				return this._flockBRCountBonus;
			}
		}
		public int FlockCRCountBonus
		{
			get
			{
				return this._flockCRCountBonus;
			}
		}
		public int FlockDNCountBonus
		{
			get
			{
				return this._flockDNCountBonus;
			}
		}
		public int FlockLVCountBonus
		{
			get
			{
				return this._flockLVCountBonus;
			}
		}
		public int DeclareWarPointCost
		{
			get
			{
				return this._declareWarPointCost;
			}
		}
		public int RequestResearchPointCost
		{
			get
			{
				return this._requestResearchPointCost;
			}
		}
		public int RequestMilitaryAssistancePointCost
		{
			get
			{
				return this._requestMilitaryAssistancePointCost;
			}
		}
		public int RequestGatePointCost
		{
			get
			{
				return this._requestGatePointCost;
			}
		}
		public int RequestEnclavePointCost
		{
			get
			{
				return this._requestEnclavePointCost;
			}
		}
		public int RequestSystemInfoPointCost
		{
			get
			{
				return this._requestSystemInfoPointCost;
			}
		}
		public int RequestSavingsPointCost
		{
			get
			{
				return this._requestSavingsPointCost;
			}
		}
		public int DemandSavingsPointCost
		{
			get
			{
				return this._demandSavingsPointCost;
			}
		}
		public int DemandResearchPointCost
		{
			get
			{
				return this._demandResearchPointCost;
			}
		}
		public int DemandSystemInfoPointCost
		{
			get
			{
				return this._demandSystemInfoPointCost;
			}
		}
		public int DemandSlavesPointCost
		{
			get
			{
				return this._demandSlavesPointCost;
			}
		}
		public int DemandSystemPointCost
		{
			get
			{
				return this._demandSystemPointCost;
			}
		}
		public int DemandProvincePointCost
		{
			get
			{
				return this._demandProvincePointCost;
			}
		}
		public int DemandEmpirePointCost
		{
			get
			{
				return this._demandEmpirePointCost;
			}
		}
		public int TreatyArmisticeWarNeutralPointCost
		{
			get
			{
				return this._treatyArmisticeWarNeutralPointCost;
			}
		}
		public int TreatyArmisticeNeutralCeasefirePointCost
		{
			get
			{
				return this._treatyArmisticeNeutralCeasefirePointCost;
			}
		}
		public int TreatyArmisticeNeutralNonAggroPointCost
		{
			get
			{
				return this._treatyArmisticeNeutralNonAggroPointCost;
			}
		}
		public int TreatyArmisticeCeaseFireNonAggroPointCost
		{
			get
			{
				return this._treatyArmisticeCeaseFireNonAggroPointCost;
			}
		}
		public int TreatyArmisticeCeaseFirePeacePointCost
		{
			get
			{
				return this._treatyArmisticeCeaseFirePeacePointCost;
			}
		}
		public int TreatyArmisticeNonAggroPeaceCost
		{
			get
			{
				return this._treatyArmisticeNonAggroPeaceCost;
			}
		}
		public int TreatyArmisticePeaceAllianceCost
		{
			get
			{
				return this._treatyArmisticePeaceAlliancePointCost;
			}
		}
		public int TreatyTradePointCost
		{
			get
			{
				return this._treatyTradePointCost;
			}
		}
		public int TreatyIncorporatePointCost
		{
			get
			{
				return this._treatyIncorporatePointCost;
			}
		}
		public int TreatyProtectoratePointCost
		{
			get
			{
				return this._treatyProtectoratePointCost;
			}
		}
		public int TreatyLimitationShipClassPointCost
		{
			get
			{
				return this._treatyLimitationShipClassPointCost;
			}
		}
		public int TreatyLimitationFleetsPointCost
		{
			get
			{
				return this._treatyLimitationFleetsPointCost;
			}
		}
		public int TreatyLimitationWeaponsPointCost
		{
			get
			{
				return this._treatyLimitationWeaponsPointCost;
			}
		}
		public int TreatyLimitationResearchTechPointCost
		{
			get
			{
				return this._treatyLimitationResearchTechPointCost;
			}
		}
		public int TreatyLimitationResearchTreePointCost
		{
			get
			{
				return this._treatyLimitationResearchTreePointCost;
			}
		}
		public int TreatyLimitationColoniesPointCost
		{
			get
			{
				return this._treatyLimitationColoniesPointCost;
			}
		}
		public int TreatyLimitationForgeGemWorldsPointCost
		{
			get
			{
				return this._treatyLimitationForgeGemWorldsPointCost;
			}
		}
		public int TreatyLimitationStationType
		{
			get
			{
				return this._treatyLimitationStationType;
			}
		}
		public int StimulusColonizationBonus
		{
			get
			{
				return this._stimulusColonizationBonus;
			}
		}
		public int StimulusMiningMin
		{
			get
			{
				return this._stimulusMiningMin;
			}
		}
		public int StimulusMiningMax
		{
			get
			{
				return this._stimulusMiningMax;
			}
		}
		public int StimulusColonizationMin
		{
			get
			{
				return this._stimulusColonizationMin;
			}
		}
		public int StimulusColonizationMax
		{
			get
			{
				return this._stimulusColonizationMax;
			}
		}
		public int StimulusTradeMin
		{
			get
			{
				return this._stimulusTradeMin;
			}
		}
		public int StimulusTradeMax
		{
			get
			{
				return this._stimulusTradeMax;
			}
		}
		public int StationsPerPop
		{
			get
			{
				return this._StationsPerPopulation;
			}
		}
		public int MinLoaCubesOnBuild
		{
			get
			{
				return this._minLoaCubesOnBuild;
			}
		}
		public int MaxLoaCubesOnBuild
		{
			get
			{
				return this._maxLoaCubesOnBuild;
			}
		}
		public int LoaCostPerCube
		{
			get
			{
				return this._LoaCostPerCube;
			}
		}
		public float LoaDistanceBetweenGates
		{
			get
			{
				return this._LoaDistanceBetweenGates;
			}
		}
		public int LoaBaseMaxMass
		{
			get
			{
				return this._LoaBaseMaxMass;
			}
		}
		public int LoaMassInductionProjectorsMaxMass
		{
			get
			{
				return this._LoaMassInductionProjectorsMaxMass;
			}
		}
		public int LoaMassStandingPulseWavesMaxMass
		{
			get
			{
				return this._LoaMassStandingPulseWavesMaxMass;
			}
		}
		public float LoaGateSystemMargin
		{
			get
			{
				return this._LoaGateSystemMargin;
			}
		}
		public float LoaTechModMod
		{
			get
			{
				return this._LoaTechModMod;
			}
		}
		public float HomeworldTaxBonusMod
		{
			get
			{
				return this._HomeworldTaxBonusMod;
			}
		}
		public Vector3 PieChartColourShipMaintenance
		{
			get
			{
				return this._pieChartColourShipMaintenance;
			}
		}
		public Vector3 PieChartColourPlanetaryDevelopment
		{
			get
			{
				return this._pieChartColourPlanetaryDevelopment;
			}
		}
		public Vector3 PieChartColourDebtInterest
		{
			get
			{
				return this._pieChartColourDebtInterest;
			}
		}
		public Vector3 PieChartColourResearch
		{
			get
			{
				return this._pieChartColourResearch;
			}
		}
		public Vector3 PieChartColourSecurity
		{
			get
			{
				return this._pieChartColourSecurity;
			}
		}
		public Vector3 PieChartColourStimulus
		{
			get
			{
				return this._pieChartColourStimulus;
			}
		}
		public Vector3 PieChartColourSavings
		{
			get
			{
				return this._pieChartColourSavings;
			}
		}
		public Vector3 PieChartColourCorruption
		{
			get
			{
				return this._pieChartColourCorruption;
			}
		}
		public int AccumulatedKnowledgeWeaponPerBattleMin
		{
			get
			{
				return this._accumulatedKnowledgeWeaponPerBattleMin;
			}
		}
		public int AccumulatedKnowledgeWeaponPerBattleMax
		{
			get
			{
				return this._accumulatedKnowledgeWeaponPerBattleMax;
			}
		}
		public int UpkeepBattleRider
		{
			get
			{
				return this._upkeepBattleRider;
			}
		}
		public int UpkeepCruiser
		{
			get
			{
				return this._upkeepCruiser;
			}
		}
		public int UpkeepDreadnaught
		{
			get
			{
				return this._upkeepDreadnaught;
			}
		}
		public int UpkeepLeviathan
		{
			get
			{
				return this._upkeepLeviathan;
			}
		}
		public int UpkeepDefensePlatform
		{
			get
			{
				return this._upkeepDefensePlatform;
			}
		}
		public int[] UpkeepScienceStation
		{
			get
			{
				return this._upkeepScienceStation;
			}
		}
		public int[] UpkeepNavalStation
		{
			get
			{
				return this._upkeepNavalStation;
			}
		}
		public int[] UpkeepDiplomaticStation
		{
			get
			{
				return this._upkeepDiplomaticStation;
			}
		}
		public int[] UpkeepCivilianStation
		{
			get
			{
				return this._upkeepCivilianStation;
			}
		}
		public int[] UpkeepGateStation
		{
			get
			{
				return this._upkeepGateStation;
			}
		}
		public float EliteUpkeepCostScale
		{
			get
			{
				return this._eliteUpkeepCostScale;
			}
		}
		public float StarSystemEntryPointRange
		{
			get
			{
				return this._starSystemEntryPointRange;
			}
		}
		public float TacStealthArmorBonus
		{
			get
			{
				return this._tacStealthArmorBonus;
			}
		}
		public float SlewModeMultiplier
		{
			get
			{
				return this._slewModeMultiplier;
			}
		}
		public float SlewModeDecelMultiplier
		{
			get
			{
				return this._slewModeDecelMultiplier;
			}
		}
		public float SlewModeExitRange
		{
			get
			{
				return this._slewModeExitRange;
			}
		}
		public float SlewModeEnterOffset
		{
			get
			{
				return this._slewModeEnterOffset;
			}
		}
		public MineFieldParams MineFieldParams
		{
			get
			{
				return this._mineFieldParams;
			}
		}
		public SpecialProjectData SpecialProjectData
		{
			get
			{
				return this._specialProjectData;
			}
		}
		public DefenseManagerSettings DefenseManagerSettings
		{
			get
			{
				return this._defenseManagerSettings;
			}
		}
		public AssetDatabase.CritHitChances[] CriticalHitChances
		{
			get
			{
				return this._critHitChances;
			}
		}
		public IntelMissionDescMap IntelMissions
		{
			get;
			private set;
		}
		public IEnumerable<string> MaterialDictionaries
		{
			get
			{
				try
				{
					string[] commonMaterialDictionaries = this._commonMaterialDictionaries;
					for (int i = 0; i < commonMaterialDictionaries.Length; i++)
					{
						string text = commonMaterialDictionaries[i];
						yield return text;
					}
				}
				finally
				{
				}
				try
				{
					Faction[] factions = this._factions;
					for (int j = 0; j < factions.Length; j++)
					{
						Faction faction = factions[j];
						try
						{
							string[] materialDictionaries = faction.MaterialDictionaries;
							for (int k = 0; k < materialDictionaries.Length; k++)
							{
								string text2 = materialDictionaries[k];
								yield return text2;
							}
						}
						finally
						{
						}
					}
				}
				finally
				{
				}
				yield break;
			}
		}
		public Dictionary<StratModifiers, object> DefaultStratModifiers
		{
			get
			{
				return this._defaultStratModifiers;
			}
		}
		public Dictionary<RandomEncounter, int> RandomEncounterOdds
		{
			get
			{
				return this._randomEncounterOdds;
			}
		}
		public Dictionary<EasterEgg, int> EasterEggOdds
		{
			get
			{
				return this._easterEggOdds;
			}
		}
		public Dictionary<EasterEgg, int> GMOdds
		{
			get
			{
				return this._gmOdds;
			}
		}
		public IEnumerable<LogicalTurretHousing> TurretHousings
		{
			get
			{
				return this._turretHousings;
			}
		}
		public IEnumerable<LogicalWeapon> Weapons
		{
			get
			{
				return this._weapons;
			}
		}
		public IEnumerable<LogicalModule> Modules
		{
			get
			{
				return this._modules;
			}
		}
		public IEnumerable<LogicalModule> ModulesToAssignByDefault
		{
			get
			{
				return this._modulesToAssignByDefault;
			}
		}
		public IEnumerable<LogicalPsionic> Psionics
		{
			get
			{
				return this._psionics;
			}
		}
		public IEnumerable<SuulkaPsiBonus> SuulkaPsiBonuses
		{
			get
			{
				return this._suulkaPsiBonuses;
			}
		}
		public IEnumerable<LogicalShield> Shields
		{
			get
			{
				return this._shields;
			}
		}
		public IEnumerable<LogicalShipSpark> ShipSparks
		{
			get
			{
				return this._shipSparks;
			}
		}
		public LogicalEffect ShipEMPEffect
		{
			get
			{
				return this._shipEMPEffect;
			}
		}
		public IEnumerable<Faction> Factions
		{
			get
			{
				return this._factions;
			}
		}
		public IEnumerable<Race> Races
		{
			get
			{
				return this._races;
			}
		}
		public string[] SplashScreenImageNames
		{
			get
			{
				return this._splashScreenImageNames;
			}
		}
		public IList<SkyDefinition> SkyDefinitions
		{
			get
			{
				return this._skyDefinitions;
			}
		}
		public HashSet<ShipSectionAsset> ShipSections
		{
			get
			{
				return this._shipSections;
			}
		}
		public PlanetGraphicsRules PlanetGenerationRules
		{
			get
			{
				return this._planetgenrules;
			}
		}
		public TechTree MasterTechTree
		{
			get
			{
				return this._masterTechTree;
			}
		}
		public Kerberos.Sots.Data.TechnologyFramework.Tech[] MasterTechTreeRoots
		{
			get
			{
				return this._masterTechTreeRoots;
			}
		}
		public IEnumerable<string> TechTreeModels
		{
			get
			{
				return this._techTreeModels;
			}
		}
		public IEnumerable<string> TechTreeRoots
		{
			get
			{
				return this._techTreeRoots;
			}
		}
		public static CommonStrings CommonStrings
		{
			get
			{
				return AssetDatabase._commonStrings;
			}
		}
		public List<FleetTemplate> FleetTemplates
		{
			get
			{
				return this._fleetTemplates;
			}
		}
		public AIResearchFramework AIResearchFramework
		{
			get;
			private set;
		}
		private static ModuleEnums.ModuleSlotTypes? GetEquivalentModuleSlotType(ModuleEnums.StationModuleType value)
		{
			switch (value)
			{
			case ModuleEnums.StationModuleType.Sensor:
				return new ModuleEnums.ModuleSlotTypes?(ModuleEnums.ModuleSlotTypes.Sensor);
			case ModuleEnums.StationModuleType.Customs:
				return new ModuleEnums.ModuleSlotTypes?(ModuleEnums.ModuleSlotTypes.Customs);
			case ModuleEnums.StationModuleType.Combat:
				return new ModuleEnums.ModuleSlotTypes?(ModuleEnums.ModuleSlotTypes.Combat);
			case ModuleEnums.StationModuleType.Repair:
				return new ModuleEnums.ModuleSlotTypes?(ModuleEnums.ModuleSlotTypes.Repair);
			case ModuleEnums.StationModuleType.Warehouse:
				return new ModuleEnums.ModuleSlotTypes?(ModuleEnums.ModuleSlotTypes.Warehouse);
			case ModuleEnums.StationModuleType.Command:
				return new ModuleEnums.ModuleSlotTypes?(ModuleEnums.ModuleSlotTypes.Command);
			case ModuleEnums.StationModuleType.Dock:
				return new ModuleEnums.ModuleSlotTypes?(ModuleEnums.ModuleSlotTypes.Dock);
			case ModuleEnums.StationModuleType.Terraform:
				return new ModuleEnums.ModuleSlotTypes?(ModuleEnums.ModuleSlotTypes.Terraform);
			case ModuleEnums.StationModuleType.Bastion:
				return new ModuleEnums.ModuleSlotTypes?(ModuleEnums.ModuleSlotTypes.Bastion);
			case ModuleEnums.StationModuleType.Amp:
				return new ModuleEnums.ModuleSlotTypes?(ModuleEnums.ModuleSlotTypes.Amp);
			case ModuleEnums.StationModuleType.GateLab:
				return new ModuleEnums.ModuleSlotTypes?(ModuleEnums.ModuleSlotTypes.GateLab);
			case ModuleEnums.StationModuleType.Defence:
				return new ModuleEnums.ModuleSlotTypes?(ModuleEnums.ModuleSlotTypes.Defence);
			case ModuleEnums.StationModuleType.AlienHabitation:
				return new ModuleEnums.ModuleSlotTypes?(ModuleEnums.ModuleSlotTypes.AlienHabitation);
			case ModuleEnums.StationModuleType.Habitation:
				return new ModuleEnums.ModuleSlotTypes?(ModuleEnums.ModuleSlotTypes.Habitation);
			case ModuleEnums.StationModuleType.HumanHabitation:
			case ModuleEnums.StationModuleType.TarkasHabitation:
			case ModuleEnums.StationModuleType.LiirHabitation:
			case ModuleEnums.StationModuleType.HiverHabitation:
			case ModuleEnums.StationModuleType.MorrigiHabitation:
			case ModuleEnums.StationModuleType.ZuulHabitation:
			case ModuleEnums.StationModuleType.LoaHabitation:
			case ModuleEnums.StationModuleType.HumanHabitationForeign:
			case ModuleEnums.StationModuleType.TarkasHabitationForeign:
			case ModuleEnums.StationModuleType.LiirHabitationForeign:
			case ModuleEnums.StationModuleType.HiverHabitationForeign:
			case ModuleEnums.StationModuleType.MorrigiHabitationForeign:
			case ModuleEnums.StationModuleType.ZuulHabitationForeign:
			case ModuleEnums.StationModuleType.LoaHabitationForeign:
				break;
			case ModuleEnums.StationModuleType.LargeHabitation:
				return new ModuleEnums.ModuleSlotTypes?(ModuleEnums.ModuleSlotTypes.LargeHabitation);
			case ModuleEnums.StationModuleType.LargeAlienHabitation:
				return new ModuleEnums.ModuleSlotTypes?(ModuleEnums.ModuleSlotTypes.LargeAlienHabitation);
			default:
				if (value == ModuleEnums.StationModuleType.Lab)
				{
					return new ModuleEnums.ModuleSlotTypes?(ModuleEnums.ModuleSlotTypes.Lab);
				}
				break;
			}
			return null;
		}
		private static IEnumerable<ModuleEnums.StationModuleType> ResolveSpecificStationModuleTypes(ModuleEnums.StationModuleType value)
		{
			ModuleEnums.ModuleSlotTypes? slotType = AssetDatabase.GetEquivalentModuleSlotType(value);
			if (slotType.HasValue)
			{
				return 
					from x in AssetDatabase.StationModuleTypeToMountTypeMap
					where x.Value == slotType
					select x.Key;
			}
			return EmptyEnumerable<ModuleEnums.StationModuleType>.Default;
		}
		public static IEnumerable<ModuleEnums.StationModuleType> ResolveSpecificStationModuleTypes(string faction, ModuleEnums.StationModuleType value)
		{
			return AssetDatabase.ResolveSpecificStationModuleTypes(value);
		}
		public AssetDatabase.MiniMapData GetMiniShipDirectoryFromID(int id)
		{
			return this._miniShipMap.Values.FirstOrDefault((AssetDatabase.MiniMapData x) => x.ID == id);
		}
		public int GetNumMiniShips()
		{
			return this._miniShipMap.Values.Count;
		}
		public AssetDatabase.MiniMapData GetMiniShipDirectory(App game, string faction, FleetType ft, List<ShipInfo> fleetComposition)
		{
			string miniMapType = AssetDatabase.GetMiniMapType(game, faction, ft, fleetComposition);
			AssetDatabase.MiniMapData result;
			if (this._miniShipMap.TryGetValue(miniMapType, out result))
			{
				return result;
			}
			return new AssetDatabase.MiniMapData();
		}
		private static string GetMiniMapType(App game, string faction, FleetType ft, List<ShipInfo> fleetComposition)
		{
			if (fleetComposition.Any((ShipInfo x) => x.DesignID == game.Game.ScriptModules.Gardeners.GardenerDesignId))
			{
				return "gardener";
			}
			switch (faction)
			{
			case "human":
			case "hiver":
			case "liir_zuul":
			case "morrigi":
			case "tarkas":
			case "zuul":
			case "slavers":
			case "swarm":
			case "vonneumann":
				return faction;
			case "loa":
				if (ft == FleetType.FL_ACCELERATOR)
				{
					return "loa_gate";
				}
				return faction;
			case "independant_race_a":
				return "indy_a";
			case "independant_race_b":
				return "indy_b";
			case "grandmenaces":
				if (game.Game.ScriptModules.SystemKiller != null)
				{
					if (fleetComposition.Any((ShipInfo x) => x.DesignID == game.Game.ScriptModules.SystemKiller.SystemKillerDesignId))
					{
						return "systemkiller";
					}
				}
				if (game.Game.ScriptModules.NeutronStar != null)
				{
					if (fleetComposition.Any((ShipInfo x) => x.DesignID == game.Game.ScriptModules.NeutronStar.NeutronDesignId))
					{
						return "neutronstar";
					}
				}
				break;
			case "locusts":
				if (!fleetComposition.Any((ShipInfo x) => x.DesignID == game.Game.ScriptModules.Locust.WorldShipDesignId))
				{
					return "locust_moon";
				}
				return "locust_world";
			case "protean":
				return string.Empty;
			}
			return string.Empty;
		}
		public Dictionary<int, Dictionary<ModuleEnums.StationModuleType, int>> GetStationUpgradeRequirements(StationType type)
		{
			if (type == StationType.NAVAL)
			{
				return this.NavalStationUpgradeRequirements;
			}
			if (type == StationType.CIVILIAN)
			{
				return this.CivilianStationUpgradeRequirements;
			}
			if (type == StationType.DIPLOMATIC)
			{
				return this.DiplomaticStationUpgradeRequirements;
			}
			if (type == StationType.SCIENCE)
			{
				return this.ScienceStationUpgradeRequirements;
			}
			if (type == StationType.GATE)
			{
				return this.GateStationUpgradeRequirements;
			}
			return null;
		}
		private void LoadMoralEventModifiers(XmlDocument moralEvents)
		{
			if (moralEvents == null)
			{
				return;
			}
			XmlElement xmlElement = moralEvents["MoralEventModifiers"];
			if (xmlElement == null)
			{
				return;
			}
			foreach (GovernmentInfo.GovernmentType governmentType in Enum.GetValues(typeof(GovernmentInfo.GovernmentType)))
			{
				XmlElement xmlElement2 = xmlElement[governmentType.ToString()];
				if (xmlElement2 != null)
				{
					foreach (MoralEvent moralEvent in Enum.GetValues(typeof(MoralEvent)))
					{
						XmlElement xmlElement3 = xmlElement2[moralEvent.ToString()];
						if (xmlElement3 != null)
						{
							if (AssetDatabase.MoralModifierMap[governmentType].ContainsKey(moralEvent))
							{
								AssetDatabase.MoralModifierMap[governmentType][moralEvent].Clear();
							}
							else
							{
								AssetDatabase.MoralModifierMap[governmentType].Add(moralEvent, new List<AssetDatabase.MoralModifier>());
							}
							string attribute = xmlElement3.GetAttribute("ApplyMoralEventToAllColonies");
							if (!string.IsNullOrEmpty(attribute))
							{
								AssetDatabase.MoralModifierMap[governmentType][moralEvent].Add(new AssetDatabase.MoralModifier
								{
									type = AssetDatabase.MoraleModifierType.AllColonies,
									value = int.Parse(attribute)
								});
							}
							attribute = xmlElement3.GetAttribute("ApplyMoralEventToColony");
							if (!string.IsNullOrEmpty(attribute))
							{
								AssetDatabase.MoralModifierMap[governmentType][moralEvent].Add(new AssetDatabase.MoralModifier
								{
									type = AssetDatabase.MoraleModifierType.Colony,
									value = int.Parse(attribute)
								});
							}
							attribute = xmlElement3.GetAttribute("ApplyMoralEventToProvince");
							if (!string.IsNullOrEmpty(attribute))
							{
								AssetDatabase.MoralModifierMap[governmentType][moralEvent].Add(new AssetDatabase.MoralModifier
								{
									type = AssetDatabase.MoraleModifierType.Province,
									value = int.Parse(attribute)
								});
							}
							attribute = xmlElement3.GetAttribute("ApplyMoralEventToSystem");
							if (!string.IsNullOrEmpty(attribute))
							{
								AssetDatabase.MoralModifierMap[governmentType][moralEvent].Add(new AssetDatabase.MoralModifier
								{
									type = AssetDatabase.MoraleModifierType.System,
									value = int.Parse(attribute)
								});
							}
						}
					}
				}
			}
		}
		public void LoadDefaultStratModifiers(XmlDocument defaultStratMods)
		{
			if (defaultStratMods == null)
			{
				return;
			}
			XmlElement xmlElement = defaultStratMods["StratModifiers"];
			if (xmlElement == null)
			{
				return;
			}
			foreach (StratModifiers stratModifiers in Enum.GetValues(typeof(StratModifiers)))
			{
				if (xmlElement[stratModifiers.ToString()] != null)
				{
					if (this._defaultStratModifiers[stratModifiers] is bool)
					{
						this._defaultStratModifiers[stratModifiers] = XmlHelper.GetData<bool>(xmlElement, stratModifiers.ToString());
					}
					else
					{
						if (this._defaultStratModifiers[stratModifiers] is int)
						{
							this._defaultStratModifiers[stratModifiers] = XmlHelper.GetData<int>(xmlElement, stratModifiers.ToString());
						}
						else
						{
							if (this._defaultStratModifiers[stratModifiers] is float)
							{
								this._defaultStratModifiers[stratModifiers] = XmlHelper.GetData<float>(xmlElement, stratModifiers.ToString());
							}
							else
							{
								if (this._defaultStratModifiers[stratModifiers] is double)
								{
									this._defaultStratModifiers[stratModifiers] = XmlHelper.GetData<double>(xmlElement, stratModifiers.ToString());
								}
								else
								{
									this._defaultStratModifiers[stratModifiers] = XmlHelper.GetData<string>(xmlElement, stratModifiers.ToString());
								}
							}
						}
					}
				}
			}
		}
		public void LoadTechBonusValues(XmlDocument techbonuses)
		{
			if (techbonuses == null)
			{
				return;
			}
			XmlElement xmlElement = techbonuses["TechBonuses"];
			if (xmlElement == null)
			{
				return;
			}
			this._techBonuses = new Dictionary<string, Dictionary<string, object>>();
			foreach (XmlElement current in 
				from x in xmlElement.OfType<XmlElement>()
				where x.Name == "TechBonus"
				select x)
			{
				string attribute = current.GetAttribute("techID");
				if (!string.IsNullOrEmpty(attribute) && !this._techBonuses.ContainsKey(attribute))
				{
					Dictionary<string, object> dictionary = new Dictionary<string, object>();
					foreach (XmlAttribute xmlAttribute in current.Attributes)
					{
						if (xmlAttribute.Name != "techID")
						{
							dictionary.Add(xmlAttribute.Name, xmlAttribute.Value);
						}
					}
					this._techBonuses.Add(attribute, dictionary);
				}
			}
		}
		private void InitializeAIDifficultyBonuses()
		{
			this.AIDifficultyBonuses = new Dictionary<AIDifficulty, Dictionary<DifficultyModifiers, float>>();
			this.AIDifficultyBonuses[AIDifficulty.Easy] = new Dictionary<DifficultyModifiers, float>();
			this.AIDifficultyBonuses[AIDifficulty.Easy][DifficultyModifiers.ResearchBonus] = -0.25f;
			this.AIDifficultyBonuses[AIDifficulty.Easy][DifficultyModifiers.RevenueBonus] = -0.25f;
			this.AIDifficultyBonuses[AIDifficulty.Easy][DifficultyModifiers.ProductionBonus] = -0.25f;
			this.AIDifficultyBonuses[AIDifficulty.Easy][DifficultyModifiers.PopulationGrowthBonus] = -0.1f;
			this.AIDifficultyBonuses[AIDifficulty.Normal] = new Dictionary<DifficultyModifiers, float>();
			this.AIDifficultyBonuses[AIDifficulty.Normal][DifficultyModifiers.ResearchBonus] = 0f;
			this.AIDifficultyBonuses[AIDifficulty.Normal][DifficultyModifiers.RevenueBonus] = 0f;
			this.AIDifficultyBonuses[AIDifficulty.Normal][DifficultyModifiers.ProductionBonus] = 0f;
			this.AIDifficultyBonuses[AIDifficulty.Normal][DifficultyModifiers.PopulationGrowthBonus] = 0f;
			this.AIDifficultyBonuses[AIDifficulty.Hard] = new Dictionary<DifficultyModifiers, float>();
			this.AIDifficultyBonuses[AIDifficulty.Hard][DifficultyModifiers.ResearchBonus] = 0.25f;
			this.AIDifficultyBonuses[AIDifficulty.Hard][DifficultyModifiers.RevenueBonus] = 0.25f;
			this.AIDifficultyBonuses[AIDifficulty.Hard][DifficultyModifiers.ProductionBonus] = 0.25f;
			this.AIDifficultyBonuses[AIDifficulty.Hard][DifficultyModifiers.PopulationGrowthBonus] = 0.1f;
			this.AIDifficultyBonuses[AIDifficulty.VeryHard] = new Dictionary<DifficultyModifiers, float>();
			this.AIDifficultyBonuses[AIDifficulty.VeryHard][DifficultyModifiers.ResearchBonus] = 0.75f;
			this.AIDifficultyBonuses[AIDifficulty.VeryHard][DifficultyModifiers.RevenueBonus] = 0.75f;
			this.AIDifficultyBonuses[AIDifficulty.VeryHard][DifficultyModifiers.ProductionBonus] = 0.75f;
			this.AIDifficultyBonuses[AIDifficulty.VeryHard][DifficultyModifiers.PopulationGrowthBonus] = 0.25f;
		}
		public float GetAIModifier(App game, DifficultyModifiers dm, int playerId)
		{
			if (this.AIDifficultyBonuses == null)
			{
				return 0f;
			}
			Player player = game.GetPlayer(playerId);
			if (player == null || !player.IsAI() || !player.IsStandardPlayer || game.GameSetup == null)
			{
				return 0f;
			}
			return this.AIDifficultyBonuses[game.GameSetup.Players[player.ID - 1].AIDifficulty][dm];
		}
		public T GetTechBonus<T>(string techID, string bonusType)
		{
			T result = default(T);
			if (this._techBonuses == null)
			{
				App.Log.Warn("Tech bonuses not found, missing techbonuses.xml document", "data");
				return result;
			}
			Dictionary<string, object> dictionary;
			if (this._techBonuses.TryGetValue(techID, out dictionary))
			{
				object value;
				if (dictionary.TryGetValue(bonusType, out value))
				{
					result = (T)((object)Convert.ChangeType(value, typeof(T)));
				}
				else
				{
					App.Log.Warn("Did not find bonus " + bonusType + " in tech " + techID, "data");
				}
			}
			else
			{
				App.Log.Warn("Did not find tech " + techID, "data");
			}
			return result;
		}
		public void LoadGovernmentActionModifiers(XmlDocument govActions)
		{
			if (govActions == null)
			{
				return;
			}
			XmlElement xmlElement = govActions["GovernmentAction"];
			if (xmlElement == null)
			{
				return;
			}
			this._govActionModifiers = new Dictionary<string, GovActionValues>();
			foreach (XmlElement current in xmlElement.OfType<XmlElement>())
			{
				if (!this._govActionModifiers.ContainsKey(current.Name))
				{
					string attribute = current.GetAttribute("xchange");
					string attribute2 = current.GetAttribute("ychange");
					if (!string.IsNullOrEmpty(attribute) && !string.IsNullOrEmpty(attribute2))
					{
						GovActionValues govActionValues = new GovActionValues();
						govActionValues.XChange = int.Parse(attribute);
						govActionValues.YChange = int.Parse(attribute2);
						this._govActionModifiers.Add(current.Name, govActionValues);
					}
				}
			}
		}
		public GovActionValues GetGovActionValues(string ga)
		{
			GovActionValues result = null;
			if (this._govActionModifiers.TryGetValue(ga, out result))
			{
				return result;
			}
			App.Log.Warn("Did not find Government Action " + ga, "data");
			return null;
		}
		public string GetLocalizedTechnologyName(string techId)
		{
			return AssetDatabase.CommonStrings.Localize("@TECH_NAME_" + techId);
		}
		public string GetLocalizedStationTypeName(StationType value, bool useZuulStations)
		{
			if (value == StationType.INVALID_TYPE || value == StationType.NUM_TYPES)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			if (useZuulStations)
			{
				return App.Localize("@STATION_TYPE_" + value.ToString().ToUpperInvariant() + "_ZUUL");
			}
			return App.Localize("@STATION_TYPE_" + value.ToString().ToUpperInvariant());
		}
		public int GetDiplomaticRequestPointCost(RequestType rt)
		{
			switch (rt)
			{
			case RequestType.SavingsRequest:
				return this.RequestSavingsPointCost;
			case RequestType.SystemInfoRequest:
				return this.RequestSystemInfoPointCost;
			case RequestType.ResearchPointsRequest:
				return this.RequestResearchPointCost;
			case RequestType.MilitaryAssistanceRequest:
				return this.RequestMilitaryAssistancePointCost;
			case RequestType.GatePermissionRequest:
				return this.RequestGatePointCost;
			case RequestType.EstablishEnclaveRequest:
				return this.RequestEnclavePointCost;
			default:
				throw new ArgumentOutOfRangeException("rt");
			}
		}
		public int GetDiplomaticDemandPointCost(DemandType dt)
		{
			switch (dt)
			{
			case DemandType.SavingsDemand:
				return this.DemandSavingsPointCost;
			case DemandType.SystemInfoDemand:
				return this.DemandSystemInfoPointCost;
			case DemandType.ResearchPointsDemand:
				return this.DemandResearchPointCost;
			case DemandType.SlavesDemand:
				return this.DemandSlavesPointCost;
			case DemandType.WorldDemand:
				return this.DemandSystemPointCost;
			case DemandType.ProvinceDemand:
				return this.DemandProvincePointCost;
			case DemandType.SurrenderDemand:
				return this.DemandEmpirePointCost;
			default:
				throw new ArgumentOutOfRangeException("dt");
			}
		}
		public float GetPlagueInfectionRate(WeaponEnums.PlagueType pt)
		{
			switch (pt)
			{
			case WeaponEnums.PlagueType.BASIC:
				return 1.2f;
			case WeaponEnums.PlagueType.RETRO:
				return 1.6f;
			case WeaponEnums.PlagueType.BEAST:
				return 1.4f;
			case WeaponEnums.PlagueType.ASSIM:
				return 1.8f;
			case WeaponEnums.PlagueType.XOMBIE:
				return 1.8f;
			}
			return 0f;
		}
		public bool IsPotentialyHabitable(string planettype)
		{
			return planettype == "normal" || planettype == "pastoral" || planettype == "volcanic" || planettype == "cavernous" || planettype == "tempestuous" || planettype == "magnar" || planettype == "primordial";
		}
		public bool IsGasGiant(string planettype)
		{
			return planettype == "gaseous";
		}
		public bool IsMoon(string planettype)
		{
			return planettype == "barren";
		}
		public static string GetModuleFactionName(ModuleEnums.StationModuleType type)
		{
			string result = "";
			if (type.ToString().Contains("Morrigi"))
			{
				result = "morrigi";
			}
			if (type.ToString().Contains("Human"))
			{
				result = "human";
			}
			if (type.ToString().Contains("Tarkas"))
			{
				result = "tarkas";
			}
			if (type.ToString().Contains("Hiver"))
			{
				result = "hiver";
			}
			if (type.ToString().Contains("Liir"))
			{
				result = "liir_zuul";
			}
			if (type.ToString().Contains("Zuul"))
			{
				result = "zuul";
			}
			if (type.ToString().Contains("Loa"))
			{
				result = "loa";
			}
			return result;
		}
		public Faction GetFaction(string name)
		{
			Faction[] factions = this._factions;
			for (int i = 0; i < factions.Length; i++)
			{
				Faction faction = factions[i];
				if (faction.Name == name)
				{
					return faction;
				}
			}
			return null;
		}
		public Faction GetFaction(int factionId)
		{
			Faction[] factions = this._factions;
			for (int i = 0; i < factions.Length; i++)
			{
				Faction faction = factions[i];
				if (faction.ID == factionId)
				{
					return faction;
				}
			}
			return null;
		}
		public object GetFactionStratModifier(string name, string variable)
		{
			Faction faction = this.GetFaction(name);
			if (faction == null)
			{
				return null;
			}
			return faction.GetStratModifier(variable);
		}
		public object GetFactionStratModifier(int factionId, string variable)
		{
			Faction faction = this.GetFaction(factionId);
			if (faction == null)
			{
				return null;
			}
			return faction.GetStratModifier(variable);
		}
		public Race GetRace(string name)
		{
			Race[] races = this._races;
			for (int i = 0; i < races.Length; i++)
			{
				Race race = races[i];
				if (race.Name == name)
				{
					return race;
				}
			}
			return null;
		}
		public ShipSectionAsset GetShipSectionAsset(string filename)
		{
			ShipSectionAsset result;
			if (this._shipSectionsByFilename.TryGetValue(filename, out result))
			{
				return result;
			}
			return null;
		}
		public T GetGlobal<T>(string name)
		{
			object obj;
			if (!this._cachedGlobals.TryGetValue(name, out obj))
			{
				XmlElement xmlElement = this._globals[name];
				TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
				if (xmlElement == null || converter == null)
				{
					throw new Exception("Unable to retreive global value " + name + " or unable to convert to specified type.");
				}
				obj = converter.ConvertFromString(null, CultureInfo.InvariantCulture, xmlElement.InnerText);
				this._cachedGlobals[name] = obj;
			}
			return (T)((object)obj);
		}
		private void LoadGlobalsAndStratModifiers(XmlDocument d)
		{
			XmlElement xmlElement = d["CommonAssets"]["Globals"];
			this._globals = xmlElement;
			this._defaultTacSensorRange = XmlHelper.GetData<float>(xmlElement, "DefaultTacSensorRange");
			this._defaultBRTacSensorRange = XmlHelper.GetData<float>(xmlElement, "DefaultBRTacSensorRange");
			this._defaultPlanetTacSensorRange = XmlHelper.GetData<float>(xmlElement, "DefaultPlanetTacSensorRange");
			this._defaultStratSensorRange = XmlHelper.GetData<float>(xmlElement, "DefaultStratSensorRange");
			this._policePatrolRadius = XmlHelper.GetData<float>(xmlElement, "PolicePatrolRadius");
			this._grandMenaceMinTurn = XmlHelper.GetData<int>(xmlElement, "GrandMenaceMinTurn");
			this._grandMenaceChance = XmlHelper.GetData<int>(xmlElement, "GrandMenaceChance");
			this._randomEncMinTurns = XmlHelper.GetData<int>(xmlElement, "RandomEncounterMinTurns");
			this._randomEncTurnsToResetOdds = XmlHelper.GetData<int>(xmlElement, "RandomEncounterTurnsToResetOdds");
			this._randomEncMinOdds = XmlHelper.GetData<float>(xmlElement, "RandomEncounterMinOdds");
			this._randomEncMaxOdds = XmlHelper.GetData<float>(xmlElement, "RandomEncounterMaxOdds");
			this._randomEncBaseOdds = XmlHelper.GetData<float>(xmlElement, "RandomEncounterBaseOdds");
			this._randomEncDecOddsCombat = XmlHelper.GetData<float>(xmlElement, "RandomEncounterDecOddsCombat");
			this._randomEncIncOddsIdle = XmlHelper.GetData<float>(xmlElement, "RandomEncounterIncOddsIdle");
			this._randomEncIncOddsRounds = XmlHelper.GetData<float>(xmlElement, "RandomEncounterIncOddsRounds");
			this._randomEncTurnsToExclude = XmlHelper.GetData<int>(xmlElement, "RandomEncounterTurnsToExclude");
			this._randomEncSinglePlayerOdds = XmlHelper.GetData<float>(xmlElement, "RandomEncounterSinglePlayerOdds");
			this._largeCombatThreshold = XmlHelper.GetData<int>(xmlElement, "LargeCombatThreshold");
			this._infrastructureSupplyRatio = XmlHelper.GetData<float>(xmlElement, "InfrastructureSupplyRatio");
			this._populationNoise = XmlHelper.GetData<float>(xmlElement, "PopulationNoise");
			this._civilianPopulationGrowthRateMod = XmlHelper.GetData<float>(xmlElement, "CivilianPopulationGrowthRateMod");
			this._civilianPopulationTriggerAmount = XmlHelper.GetData<float>(xmlElement, "CivilianPopulationTriggerAmount");
			this._civilianPopulationStartAmount = XmlHelper.GetData<float>(xmlElement, "CivilianPopulationStartAmount");
			this._civilianPopulationStartMoral = XmlHelper.GetData<int>(xmlElement, "CivilianPopulationStartMoral");
			this._diplomacyPointsPerProvince = XmlHelper.GetData<int>(xmlElement, "DiplomacyPointsPerProvince");
			this._diplomacyPointsPerStation = new int[5];
			this._diplomacyPointsPerStation[0] = XmlHelper.GetData<int>(xmlElement, "DiplomacyPointsPerStationLevel1");
			this._diplomacyPointsPerStation[1] = XmlHelper.GetData<int>(xmlElement, "DiplomacyPointsPerStationLevel2");
			this._diplomacyPointsPerStation[2] = XmlHelper.GetData<int>(xmlElement, "DiplomacyPointsPerStationLevel3");
			this._diplomacyPointsPerStation[3] = XmlHelper.GetData<int>(xmlElement, "DiplomacyPointsPerStationLevel4");
			this._diplomacyPointsPerStation[4] = XmlHelper.GetData<int>(xmlElement, "DiplomacyPointsPerStationLevel5");
			this._globalProductionModifier = XmlHelper.GetData<float>(xmlElement, "GlobalProductionModifier");
			this._stationSupportRangeModifier = XmlHelper.GetData<float>(xmlElement, "StationSupportRangeModifier");
			this._colonySupportCostFactor = XmlHelper.GetData<float>(xmlElement, "ColonySupportCostFactor");
			this._baseCorruptionRate = XmlHelper.GetData<float>(xmlElement, "BaseCorruptionRate");
			this._evacCivPerCol = XmlHelper.GetData<int>(xmlElement, "EvacuationCivPerCol");
			this._maxGovernmentShift = XmlHelper.GetData<int>(xmlElement, "MaxGovernmentShift");
			XmlElement xmlElement2 = xmlElement["SwarmerData"];
			this._globalSwarmerData = new SwarmerGlobalData();
			if (xmlElement2 != null)
			{
				this._globalSwarmerData.GrowthRateLarvaSpawn = XmlHelper.GetData<int>(xmlElement2, "GrowthRateLarvaSpawn");
				this._globalSwarmerData.GrowthRateQueenSpawn = XmlHelper.GetData<int>(xmlElement2, "GrowthRateQueenSpawn");
				this._globalSwarmerData.NumHiveSwarmers = XmlHelper.GetData<int>(xmlElement2, "NumHiveSwarmers");
				this._globalSwarmerData.NumHiveGuardians = XmlHelper.GetData<int>(xmlElement2, "NumHiveGuardians");
				this._globalSwarmerData.NumQueenSwarmers = XmlHelper.GetData<int>(xmlElement2, "NumQueenSwarmers");
				this._globalSwarmerData.NumQueenGuardians = XmlHelper.GetData<int>(xmlElement2, "NumQueenGuardians");
			}
			xmlElement2 = xmlElement["MeteorShowerData"];
			this._globalMeteorShowerData = new MeteorShowerGlobalData();
			if (xmlElement2 != null)
			{
				this._globalMeteorShowerData.LargeMeteorChance = XmlHelper.GetData<int>(xmlElement2, "MeteorShowerLargeMeteorChance");
				this._globalMeteorShowerData.MinMeteors = XmlHelper.GetData<int>(xmlElement2, "MeteorShowerMinMeteors");
				this._globalMeteorShowerData.MaxMeteors = XmlHelper.GetData<int>(xmlElement2, "MeteorShowerMaxMeteors");
				this._globalMeteorShowerData.NumBreakoffMeteors = XmlHelper.GetData<int>(xmlElement2, "NumBreakoffMeteors");
				for (int i = 0; i < 3; i++)
				{
					this._globalMeteorShowerData.Damage[i] = default(CombatAIDamageData);
				}
				this._globalMeteorShowerData.Damage[0].SetDataFromElement(xmlElement2["SmallMeteorData"]);
				this._globalMeteorShowerData.Damage[1].SetDataFromElement(xmlElement2["MediumMeteorData"]);
				this._globalMeteorShowerData.Damage[2].SetDataFromElement(xmlElement2["LargeMeteorData"]);
				this._globalMeteorShowerData.ResourceBonuses[0] = ((xmlElement2["SmallMeteorData"] != null) ? int.Parse(xmlElement2["SmallMeteorData"].GetAttribute("resources")) : 0);
				this._globalMeteorShowerData.ResourceBonuses[1] = ((xmlElement2["MediumMeteorData"] != null) ? int.Parse(xmlElement2["MediumMeteorData"].GetAttribute("resources")) : 0);
				this._globalMeteorShowerData.ResourceBonuses[2] = ((xmlElement2["LargeMeteorData"] != null) ? int.Parse(xmlElement2["LargeMeteorData"].GetAttribute("resources")) : 0);
			}
			xmlElement2 = xmlElement["CometData"];
			this._globalCometData = new CometGlobalData();
			if (xmlElement2 != null)
			{
				this._globalCometData.Damage.SetDataFromElement(xmlElement2["CometDamage"]);
			}
			xmlElement2 = xmlElement["NeutronStarData"];
			this._globalNeutronStarData = new NeutronStarGlobalData();
			if (xmlElement2 != null)
			{
				this._globalNeutronStarData.Speed = XmlHelper.GetData<float>(xmlElement2, "StarSpeed");
				this._globalNeutronStarData.AffectRange = XmlHelper.GetData<float>(xmlElement2, "GravityAffectRange");
				this._globalNeutronStarData.MeteorRatio = XmlHelper.GetData<int>(xmlElement2, "MeteorRatio");
				this._globalNeutronStarData.CometRatio = Math.Max(100 - this._globalNeutronStarData.MeteorRatio, 0);
				this._globalNeutronStarData.MaxMeteorIntensity = Math.Max(XmlHelper.GetData<float>(xmlElement2, "MaxMeteorIntensity"), 1f);
			}
			xmlElement2 = xmlElement["SuperNovaData"];
			this._globalSuperNovaData = new SuperNovaGlobalData();
			if (xmlElement2 != null)
			{
				this._globalSuperNovaData.MinTurns = XmlHelper.GetData<int>(xmlElement2, "MinTurns");
				this._globalSuperNovaData.Chance = XmlHelper.GetData<int>(xmlElement2, "Chance");
				this._globalSuperNovaData.BlastRadius = XmlHelper.GetData<float>(xmlElement2, "BlastRadius");
				this._globalSuperNovaData.MinExplodeTurns = XmlHelper.GetData<int>(xmlElement2, "MinExplodeTurns");
				this._globalSuperNovaData.MaxExplodeTurns = XmlHelper.GetData<int>(xmlElement2, "MaxExplodeTurns");
				this._globalSuperNovaData.SystemInRangeBioReduction = XmlHelper.GetData<int>(xmlElement2, "SystemInRangeBioReduction");
				this._globalSuperNovaData.SystemInRangeMinHazard = XmlHelper.GetData<float>(xmlElement2, "SystemInRangeMinHazard");
				this._globalSuperNovaData.SystemInRangeMaxHazard = XmlHelper.GetData<float>(xmlElement2, "SystemInRangeMaxHazard");
			}
			xmlElement2 = xmlElement["SlaverData"];
			this._globalSlaverData = new SlaverGlobalData();
			if (xmlElement2 != null)
			{
				this._globalSlaverData.MinAbductors = XmlHelper.GetData<int>(xmlElement2, "SlaverMinAbductors");
				this._globalSlaverData.MaxAbductors = XmlHelper.GetData<int>(xmlElement2, "SlaverMaxAbductors");
				this._globalSlaverData.MinScavengers = XmlHelper.GetData<int>(xmlElement2, "SlaverMinScavengers");
				this._globalSlaverData.MaxScavengers = XmlHelper.GetData<int>(xmlElement2, "SlaverMaxScavengers");
			}
			xmlElement2 = xmlElement["SpectreData"];
			this._globalSpectreData = new SpectreGlobalData();
			if (xmlElement2 != null)
			{
				this._globalSpectreData.MinSpectres = XmlHelper.GetData<int>(xmlElement2, "SpectreMinSpectres");
				this._globalSpectreData.MaxSpectres = XmlHelper.GetData<int>(xmlElement2, "SpectreMaxSpectres");
				for (int j = 0; j < 3; j++)
				{
					this._globalSpectreData.Damage[j] = default(CombatAIDamageData);
				}
				this._globalSpectreData.Damage[0].SetDataFromElement(xmlElement2["SmallSpectreDamage"]);
				this._globalSpectreData.Damage[1].SetDataFromElement(xmlElement2["MediumSpectreDamage"]);
				this._globalSpectreData.Damage[2].SetDataFromElement(xmlElement2["LargeSpectreDamage"]);
			}
			xmlElement2 = xmlElement["AsteroidMonitorData"];
			this._globalAsteroidMonitorData = new AsteroidMonitorGlobalData();
			if (xmlElement2 != null)
			{
				this._globalAsteroidMonitorData.NumMonitors = XmlHelper.GetData<int>(xmlElement2, "AsteroidMonitorNumMonitors");
			}
			this._globalMorrigiRelicData = new MorrigiRelicGlobalData();
			xmlElement2 = xmlElement["MorrigiRelicData"];
			if (xmlElement2 != null)
			{
				this._globalMorrigiRelicData.NumFighters = XmlHelper.GetData<int>(xmlElement2, "NumCrowFighters");
				this._globalMorrigiRelicData.NumTombs = XmlHelper.GetData<int>(xmlElement2, "NumTombs");
				for (int k = 0; k < 2; k++)
				{
					this._globalMorrigiRelicData.ResearchBonus[k] = new ResearchBonusData();
				}
				this._globalMorrigiRelicData.ResearchBonus[0].SetDataFromElement(xmlElement2["CapturedResearchBonus"]);
				this._globalMorrigiRelicData.ResearchBonus[1].SetDataFromElement(xmlElement2["DestroyedResearchBonus"]);
				XmlElement xmlElement3 = xmlElement2["Rewards"];
				if (xmlElement3 != null)
				{
					for (int l = 0; l < 10; l++)
					{
						this._globalMorrigiRelicData.Rewards[l] = XmlHelper.GetData<int>(xmlElement3, ((MorrigiRelicGlobalData.RelicType)l).ToString());
					}
				}
			}
			xmlElement2 = xmlElement["GardenerData"];
			if (xmlElement2 != null)
			{
				this._globalGardenerData = new GardenerGlobalData();
				this._globalGardenerData.MinPlanets = XmlHelper.GetData<int>(xmlElement2, "ProteanMinPlanets");
				this._globalGardenerData.MinBiosphere = XmlHelper.GetData<int>(xmlElement2, "ProteanMinBiosphere");
				this._globalGardenerData.MaxBiosphere = XmlHelper.GetData<int>(xmlElement2, "ProteanMaxBiosphere");
				this._globalGardenerData.CatchUpDelay = XmlHelper.GetData<int>(xmlElement2, "ProteanCatchUpDelay");
				this._globalGardenerData.ProteanMobMin = XmlHelper.GetData<int>(xmlElement2, "ProteanMobMin");
				this._globalGardenerData.ProteanMobMax = XmlHelper.GetData<int>(xmlElement2, "ProteanMobMax");
				this._globalGardenerData.Terrforming = XmlHelper.GetData<float>(xmlElement2, "Terraform");
				this._globalGardenerData.BiosphereDamage = XmlHelper.GetData<float>(xmlElement2, "BiosphereDamage");
			}
			xmlElement2 = xmlElement["VonNeumannData"];
			this._globalVonNeumannData = new VonNeumannGlobalData();
			if (xmlElement2 != null)
			{
				this._globalVonNeumannData.StartingResources = XmlHelper.GetData<int>(xmlElement2, "VonNeumannStartingResources");
				this._globalVonNeumannData.BuildRate = XmlHelper.GetData<int>(xmlElement2, "VonNeumannBuildRate");
				this._globalVonNeumannData.SalvageCapacity = XmlHelper.GetData<int>(xmlElement2, "VonNeumannSalvageCapacity");
				this._globalVonNeumannData.SalvageCycle = XmlHelper.GetData<int>(xmlElement2, "VonNeumannSalvageCycle");
				this._globalVonNeumannData.TargetCycle = XmlHelper.GetData<int>(xmlElement2, "VonNeumannTargetCycle");
				this._globalVonNeumannData.MomRUCost = XmlHelper.GetData<int>(xmlElement2, "MomRUCost");
				this._globalVonNeumannData.BerserkerRUCost = XmlHelper.GetData<int>(xmlElement2, "BerserkerRUCost");
				this._globalVonNeumannData.ChildRUCost = XmlHelper.GetData<int>(xmlElement2, "ChildRUCost");
				this._globalVonNeumannData.ChildRUCarryCap = XmlHelper.GetData<int>(xmlElement2, "ChildRUCarryCap");
				this._globalVonNeumannData.MinChildrenToMaintain = XmlHelper.GetData<int>(xmlElement2, "MinChildrenToMaintain");
				this._globalVonNeumannData.NumSatelitesPerChild = XmlHelper.GetData<int>(xmlElement2, "NumSatelitesPerChild");
				this._globalVonNeumannData.NumShipsPerChild = XmlHelper.GetData<int>(xmlElement2, "NumShipsPerChild");
				this._globalVonNeumannData.ChildIntegrationTime = XmlHelper.GetData<float>(xmlElement2, "ChildIntegrationTime");
				this._globalVonNeumannData.RUTransferRateShip = XmlHelper.GetData<float>(xmlElement2, "RUTransferRateShip");
				this._globalVonNeumannData.RUTransferRatePlanet = XmlHelper.GetData<float>(xmlElement2, "RUTransferRatePlanet");
			}
			xmlElement2 = xmlElement["Locust"];
			this._globalLocustData = new LocustGlobalData();
			if (xmlElement2 != null)
			{
				this._globalLocustData.MaxDrones = XmlHelper.GetData<int>(xmlElement2, "MaxDrones");
				this._globalLocustData.MaxCombatDrones = XmlHelper.GetData<int>(xmlElement2, "MaxCombatDrones");
				this._globalLocustData.MaxMoonCombatDrones = XmlHelper.GetData<int>(xmlElement2, "MaxMoonCombatDrones");
				this._globalLocustData.DroneCost = XmlHelper.GetData<int>(xmlElement2, "DroneCost");
				this._globalLocustData.NumToLand = XmlHelper.GetData<int>(xmlElement2, "NumToLand");
				this._globalLocustData.MinResourceSpawnAmount = XmlHelper.GetData<int>(xmlElement2, "MinResourceSpawnAmount");
				this._globalLocustData.MaxSalvageRate = XmlHelper.GetData<int>(xmlElement2, "MaxSalvageRate");
				this._globalLocustData.InitialLocustScouts = XmlHelper.GetData<int>(xmlElement2, "InitialLocustScouts");
				this._globalLocustData.MinLocustScouts = XmlHelper.GetData<int>(xmlElement2, "MinLocustScouts");
				this._globalLocustData.LocustScoutCost = XmlHelper.GetData<int>(xmlElement2, "LocustScoutCost");
				this._globalLocustData.LocustMotherCost = XmlHelper.GetData<int>(xmlElement2, "LocustMotherCost");
			}
			this._aiRebellionChance = XmlHelper.GetData<float>(xmlElement, "AIRebellionChance");
			this._aiRebellionColonyPercent = XmlHelper.GetData<float>(xmlElement, "AIRebellionColonyPercent");
			this._encounterMinStartOffset = XmlHelper.GetData<float>(xmlElement, "EncounterMinStartDistance");
			this._encounterMaxStartOffset = XmlHelper.GetData<float>(xmlElement, "EncounterMaxStartDistance");
			this._interceptThreshold = XmlHelper.GetData<float>(xmlElement, "InterceptThreshold");
			this._globalPiracyData = new PiracyGlobalData();
			xmlElement2 = xmlElement["PiracyBounties"];
			if (xmlElement2 != null)
			{
				this._globalPiracyData.PiracyBaseOdds = XmlHelper.GetData<float>(xmlElement2, "PiracyBaseOdds");
				this._globalPiracyData.PiracyModPolice = XmlHelper.GetData<float>(xmlElement2, "PiracyModPolice");
				this._globalPiracyData.PiracyModNavalBase = XmlHelper.GetData<float>(xmlElement2, "PiracyModNavalBase");
				this._globalPiracyData.PiracyModNoNavalBase = XmlHelper.GetData<float>(xmlElement2, "PiracyModNoNavalBase");
				this._globalPiracyData.PiracyModZuulProximity = XmlHelper.GetData<float>(xmlElement2, "PiracyModZuulProximity");
				this._globalPiracyData.PiracyMinZuulProximity = XmlHelper.GetData<float>(xmlElement2, "PiracyMinZuulProximity");
				this._globalPiracyData.PiracyMinShips = XmlHelper.GetData<int>(xmlElement2, "PiracyMinShips");
				this._globalPiracyData.PiracyMaxShips = XmlHelper.GetData<int>(xmlElement2, "PiracyMaxShips");
				this._globalPiracyData.PiracyBaseMod = XmlHelper.GetData<float>(xmlElement2, "PiracyBaseMod");
				this._globalPiracyData.PiracyMinBaseShips = XmlHelper.GetData<int>(xmlElement2, "PiracyMinBaseShips");
				this._globalPiracyData.PiracyTotalMaxShips = XmlHelper.GetData<int>(xmlElement2, "PiracyTotalMaxShips");
				this._globalPiracyData.PiracyBaseRange = XmlHelper.GetData<int>(xmlElement2, "PiracyBaseRange");
				this._globalPiracyData.PiracyBaseShipBonus = XmlHelper.GetData<int>(xmlElement2, "PiracyBaseShipBonus");
				this._globalPiracyData.PiracyBaseTurnShipBonus = XmlHelper.GetData<int>(xmlElement2, "PiracyBaseTurnShipBonus");
				this._globalPiracyData.PiracyBaseTurnsPerUpdate = XmlHelper.GetData<int>(xmlElement2, "PiracyBaseTurnsPerUpdate");
				this._globalPiracyData.Bounties[1] = XmlHelper.GetData<int>(xmlElement2, "PirateShipDestroyed");
				this._globalPiracyData.Bounties[2] = XmlHelper.GetData<int>(xmlElement2, "FreighterDestroyed");
				this._globalPiracyData.Bounties[3] = XmlHelper.GetData<int>(xmlElement2, "FreighterCaptured");
				this._globalPiracyData.Bounties[0] = XmlHelper.GetData<int>(xmlElement2, "PirateBaseDestroyed");
				XmlElement xmlElement4 = xmlElement2["RelationBonusFromBase"];
				if (xmlElement4 != null)
				{
					foreach (XmlElement current in 
						from x in xmlElement4.OfType<XmlElement>()
						where x.Name == "Bonus"
						select x)
					{
						this._globalPiracyData.ReactionBonuses.Add(current.GetAttribute("faction"), int.Parse(current.GetAttribute("value")));
					}
				}
			}
			XmlElement xmlElement5 = xmlElement["BaseEmpireColors"];
			if (xmlElement5 != null)
			{
				List<Vector3> list = new List<Vector3>();
				foreach (XmlElement current2 in 
					from x in xmlElement5.OfType<XmlElement>()
					where x.Name == "Color"
					select x)
				{
					list.Add(Vector3.Parse(current2.GetAttribute("value")));
				}
				if (list.Count == 10)
				{
					Player.DefaultPrimaryPlayerColors.Clear();
					Player.DefaultPrimaryPlayerColors = list;
				}
			}
			this._tradePointPerFreighterFleet = XmlHelper.GetData<float>(xmlElement, "TradePointPerFreightFleet");
			this._populationPerTradePoint = XmlHelper.GetData<float>(xmlElement, "PopulationPerTradePoint");
			this._incomePerInternationalTradePointMoved = XmlHelper.GetData<float>(xmlElement, "IncomePerInternationalTradePointMoved");
			this._incomePerProvincialTradePointMoved = XmlHelper.GetData<float>(xmlElement, "IncomePerProvincialTradePointMoved");
			this._incomePerGenericTradePointMoved = XmlHelper.GetData<float>(xmlElement, "IncomePerGenericTradePointMoved");
			this._taxDivider = XmlHelper.GetData<float>(xmlElement, "TaxDivider");
			this._maxDebtMultiplier = XmlHelper.GetData<float>(xmlElement, "MaxDebtMultiplier");
			this._bankruptcyTurns = XmlHelper.GetData<int>(xmlElement, "BankruptcyTurns");
			this._provinceTradeModifier = XmlHelper.GetData<float>(xmlElement, "ProvinceTradeModifier");
			this._empireTradeModifier = XmlHelper.GetData<float>(xmlElement, "EmpireTradeModifier");
			this._citizensPerImmigrationPoint = XmlHelper.GetData<float>(xmlElement, "CitizensPerImmigrationPoint");
			this._moralBonusPerPoliceShip = XmlHelper.GetData<int>(xmlElement, "MoralBonusPerPoliceShip");
			this._populationPerPlanetBeam = XmlHelper.GetData<float>(xmlElement, "PopulationPerPlanetBeam");
			this._populationPerPlanetMirv = XmlHelper.GetData<float>(xmlElement, "PopulationPerPlanetMirv");
			this._populationPerPlanetMissile = XmlHelper.GetData<float>(xmlElement, "PopulationPerPlanetMissile");
			this._populationPerHeavyPlanetMissile = XmlHelper.GetData<float>(xmlElement, "PopulationPerHeavyPlanetMissile");
			this._planetMissileLaunchDelay = XmlHelper.GetData<float>(xmlElement, "PlanetMissileLaunchDelay");
			this._planetBeamLaunchDelay = XmlHelper.GetData<float>(xmlElement, "PlanetBeamLaunchDelay");
			this._forgeWorldImpMaxBonus = XmlHelper.GetData<float>(xmlElement, "ForgeWorldImpMaxBonus");
			this._forgeWorldIOBonus = XmlHelper.GetData<float>(xmlElement, "ForgeWorldIOBonus");
			this._gemWorldCivMaxBonus = XmlHelper.GetData<float>(xmlElement, "GemWorldCivMaxBonus");
			this._superWorldSizeConstraint = XmlHelper.GetData<int>(xmlElement, "SuperWorldSizeConstraint");
			this._superWorldModifier = XmlHelper.GetData<float>(xmlElement, "SuperWorldModifier");
			this._maxOverharvestRate = XmlHelper.GetData<float>(xmlElement, "MaxOverharvestRate");
			this._randomEncOddsPerOrbital = XmlHelper.GetData<float>(xmlElement, "RandomEncOddsPerOrbital");
			this._securityPointCost = XmlHelper.GetData<int>(xmlElement, "SecurityPointCost");
			this._requiredIntelPointsForMission = XmlHelper.GetData<int>(xmlElement, "RequiredIntelPointsForMission");
			this._requiredCounterIntelPointsForMission = XmlHelper.GetData<int>(xmlElement, "RequiredCounterIntelPointsForMission");
			this._colonyFleetSupportPoints = XmlHelper.GetData<int>(xmlElement, "ColonyFleetSupportPoints");
			this._stationLvl1FleetSupportPoints = XmlHelper.GetData<int>(xmlElement, "StationLvl1FleetSupportPoints");
			this._stationLvl2FleetSupportPoints = XmlHelper.GetData<int>(xmlElement, "StationLvl2FleetSupportPoints");
			this._stationLvl3FleetSupportPoints = XmlHelper.GetData<int>(xmlElement, "StationLvl3FleetSupportPoints");
			this._stationLvl4FleetSupportPoints = XmlHelper.GetData<int>(xmlElement, "StationLvl4FleetSupportPoints");
			this._stationLvl5FleetSupportPoints = XmlHelper.GetData<int>(xmlElement, "StationLvl5FleetSupportPoints");
			this._minSlaveDeathRate = XmlHelper.GetData<float>(xmlElement, "MinSlaveDeathRate");
			this._maxSlaveDeathRate = XmlHelper.GetData<float>(xmlElement, "MaxSlaveDeathRate");
			this._imperialProductionMultiplier = XmlHelper.GetData<float>(xmlElement, "ImperialProductionMultiplier");
			this._slaveProductionMultiplier = XmlHelper.GetData<float>(xmlElement, "SlaveProductionMultiplier");
			this._civilianProductionMultiplier = XmlHelper.GetData<float>(xmlElement, "CivilianProductionMultiplier");
			this._miningStationIOBonus = XmlHelper.GetData<int>(xmlElement, "MiningStationIOBonus");
			this._flockMaxBonus = XmlHelper.GetData<float>(xmlElement, "FlockMaxBonus");
			this._flockBRBonus = XmlHelper.GetData<float>(xmlElement, "FlockBRBonus");
			this._flockCRBonus = XmlHelper.GetData<float>(xmlElement, "FlockCRBonus");
			this._flockDNBonus = XmlHelper.GetData<float>(xmlElement, "FlockDNBonus");
			this._flockLVBonus = XmlHelper.GetData<float>(xmlElement, "FlockLVBonus");
			this._flockBRCountBonus = XmlHelper.GetData<int>(xmlElement, "FlockBRCountBonus");
			this._flockCRCountBonus = XmlHelper.GetData<int>(xmlElement, "FlockCRCountBonus");
			this._flockDNCountBonus = XmlHelper.GetData<int>(xmlElement, "FlockDNCountBonus");
			this._flockLVCountBonus = XmlHelper.GetData<int>(xmlElement, "FlockLVCountBonus");
			this._declareWarPointCost = XmlHelper.GetData<int>(xmlElement, "DeclareWarPointCost");
			this._requestResearchPointCost = XmlHelper.GetData<int>(xmlElement, "RequestResearchPointCost");
			this._requestMilitaryAssistancePointCost = XmlHelper.GetData<int>(xmlElement, "RequestMilitaryAssistancePointCost");
			this._requestGatePointCost = XmlHelper.GetData<int>(xmlElement, "RequestGatePointCost");
			this._requestEnclavePointCost = XmlHelper.GetData<int>(xmlElement, "RequestEnclavePointCost");
			this._requestSystemInfoPointCost = XmlHelper.GetData<int>(xmlElement, "RequestSystemInfoPointCost");
			this._requestSavingsPointCost = XmlHelper.GetData<int>(xmlElement, "RequestSavingsPointCost");
			this._demandSavingsPointCost = XmlHelper.GetData<int>(xmlElement, "DemandSavingsPointCost");
			this._demandResearchPointCost = XmlHelper.GetData<int>(xmlElement, "DemandResearchPointCost");
			this._demandSystemInfoPointCost = XmlHelper.GetData<int>(xmlElement, "DemandSystemInfoPointCost");
			this._demandSlavesPointCost = XmlHelper.GetData<int>(xmlElement, "DemandSlavesPointCost");
			this._demandSystemPointCost = XmlHelper.GetData<int>(xmlElement, "DemandSystemPointCost");
			this._demandProvincePointCost = XmlHelper.GetData<int>(xmlElement, "DemandProvincePointCost");
			this._demandEmpirePointCost = XmlHelper.GetData<int>(xmlElement, "DemandEmpirePointCost");
			this._treatyArmisticeWarNeutralPointCost = XmlHelper.GetData<int>(xmlElement, "TreatyArmisticeWarNeutralPointCost");
			this._treatyArmisticeNeutralCeasefirePointCost = XmlHelper.GetData<int>(xmlElement, "TreatyArmisticeNeutralCeasefirePointCost");
			this._treatyArmisticeNeutralNonAggroPointCost = XmlHelper.GetData<int>(xmlElement, "TreatyArmisticeNeutralNonAggroPointCost");
			this._treatyArmisticeCeaseFireNonAggroPointCost = XmlHelper.GetData<int>(xmlElement, "TreatyArmisticeCeaseFireNonAggroPointCost");
			this._treatyArmisticeCeaseFirePeacePointCost = XmlHelper.GetData<int>(xmlElement, "TreatyArmisticeCeaseFirePeacePointCost");
			this._treatyArmisticeNonAggroPeaceCost = XmlHelper.GetData<int>(xmlElement, "TreatyArmisticeNonAggroPeaceCost");
			this._treatyArmisticePeaceAlliancePointCost = XmlHelper.GetData<int>(xmlElement, "TreatyArmisticePeaceAlliancePointCost");
			this._treatyTradePointCost = XmlHelper.GetData<int>(xmlElement, "TreatyTradePointCost");
			this._treatyIncorporatePointCost = XmlHelper.GetData<int>(xmlElement, "TreatyIncorporatePointCost");
			this._treatyProtectoratePointCost = XmlHelper.GetData<int>(xmlElement, "TreatyProtectoratePointCost");
			this._treatyLimitationShipClassPointCost = XmlHelper.GetData<int>(xmlElement, "TreatyLimitationShipClassPointCost");
			this._treatyLimitationFleetsPointCost = XmlHelper.GetData<int>(xmlElement, "TreatyLimitationFleetsPointCost");
			this._treatyLimitationWeaponsPointCost = XmlHelper.GetData<int>(xmlElement, "TreatyLimitationWeaponsPointCost");
			this._treatyLimitationResearchTechPointCost = XmlHelper.GetData<int>(xmlElement, "TreatyLimitationResearchTechPointCost");
			this._treatyLimitationResearchTreePointCost = XmlHelper.GetData<int>(xmlElement, "TreatyLimitationResearchTreePointCost");
			this._treatyLimitationColoniesPointCost = XmlHelper.GetData<int>(xmlElement, "TreatyLimitationColoniesPointCost");
			this._treatyLimitationForgeGemWorldsPointCost = XmlHelper.GetData<int>(xmlElement, "TreatyLimitationForgeGemWorldsPointCost");
			this._treatyLimitationStationType = XmlHelper.GetData<int>(xmlElement, "TreatyLimitationStationType");
			this._stimulusColonizationBonus = XmlHelper.GetData<int>(xmlElement, "StimulusColonizationBonus");
			this._stimulusMiningMin = XmlHelper.GetData<int>(xmlElement, "StimulusMiningMin");
			this._stimulusMiningMax = XmlHelper.GetData<int>(xmlElement, "StimulusMiningMax");
			this._stimulusColonizationMin = XmlHelper.GetData<int>(xmlElement, "StimulusColonizationMin");
			this._stimulusColonizationMax = XmlHelper.GetData<int>(xmlElement, "StimulusColonizationMax");
			this._stimulusTradeMin = XmlHelper.GetData<int>(xmlElement, "StimulusTradeMin");
			this._stimulusTradeMax = XmlHelper.GetData<int>(xmlElement, "StimulusTradeMax");
			this._StationsPerPopulation = XmlHelper.GetData<int>(xmlElement, "PopulationPerStation");
			this._maxLoaCubesOnBuild = XmlHelper.GetData<int>(xmlElement, "MaxLoaCubesOnBuild");
			this._minLoaCubesOnBuild = XmlHelper.GetData<int>(xmlElement, "MinLoaCubesOnBuild");
			this._LoaCostPerCube = XmlHelper.GetData<int>(xmlElement, "LoaCubeCostPer");
			this._LoaDistanceBetweenGates = XmlHelper.GetData<float>(xmlElement, "LoaDistanceBetweenGates");
			this._LoaBaseMaxMass = XmlHelper.GetData<int>(xmlElement, "LoaBaseMaxMass");
			this._LoaMassInductionProjectorsMaxMass = XmlHelper.GetData<int>(xmlElement, "LoaMassInductionProjectorsMaxMass");
			this._LoaMassStandingPulseWavesMaxMass = XmlHelper.GetData<int>(xmlElement, "LoaMassStandingPulseWavesMaxMass");
			this._LoaGateSystemMargin = XmlHelper.GetData<float>(xmlElement, "LoaGateSystemMargin");
			this._LoaTechModMod = XmlHelper.GetData<float>(xmlElement, "LoaTechModMod");
			this._HomeworldTaxBonusMod = XmlHelper.GetData<float>(xmlElement, "HomeworldTaxBonusMod");
			this._pieChartColourShipMaintenance = Vector3.Parse(XmlHelper.GetData<string>(xmlElement, "PieChartColourShipMaintenance"));
			this._pieChartColourPlanetaryDevelopment = Vector3.Parse(XmlHelper.GetData<string>(xmlElement, "PieChartColourPlanetaryDevelopment"));
			this._pieChartColourDebtInterest = Vector3.Parse(XmlHelper.GetData<string>(xmlElement, "PieChartColourDebtInterest"));
			this._pieChartColourResearch = Vector3.Parse(XmlHelper.GetData<string>(xmlElement, "PieChartColourResearch"));
			this._pieChartColourSecurity = Vector3.Parse(XmlHelper.GetData<string>(xmlElement, "PieChartColourSecurity"));
			this._pieChartColourStimulus = Vector3.Parse(XmlHelper.GetData<string>(xmlElement, "PieChartColourStimulus"));
			this._pieChartColourSavings = Vector3.Parse(XmlHelper.GetData<string>(xmlElement, "PieChartColourSavings"));
			this._pieChartColourCorruption = Vector3.Parse(XmlHelper.GetData<string>(xmlElement, "PieChartColourCorruption"));
			this._accumulatedKnowledgeWeaponPerBattleMin = XmlHelper.GetData<int>(xmlElement, "WeaponAccumulatedKnowledgePerBattleMin");
			this._accumulatedKnowledgeWeaponPerBattleMax = XmlHelper.GetData<int>(xmlElement, "WeaponAccumulatedKnowledgePerBattleMax");
			this._upkeepBattleRider = XmlHelper.GetData<int>(xmlElement, "UpkeepBattleRider");
			this._upkeepCruiser = XmlHelper.GetData<int>(xmlElement, "UpkeepCruiser");
			this._upkeepDreadnaught = XmlHelper.GetData<int>(xmlElement, "UpkeepDreadnaught");
			this._upkeepLeviathan = XmlHelper.GetData<int>(xmlElement, "UpkeepLeviathan");
			this._upkeepDefensePlatform = XmlHelper.GetData<int>(xmlElement, "UpkeepDefensePlatform");
			for (int m = 0; m < 5; m++)
			{
				this._upkeepScienceStation[m] = XmlHelper.GetData<int>(xmlElement, "UpkeepScience" + (m + 1));
			}
			for (int n = 0; n < 5; n++)
			{
				this._upkeepNavalStation[n] = XmlHelper.GetData<int>(xmlElement, "UpkeepNaval" + (n + 1));
			}
			for (int num = 0; num < 5; num++)
			{
				this._upkeepCivilianStation[num] = XmlHelper.GetData<int>(xmlElement, "UpkeepCivilian" + (num + 1));
			}
			for (int num2 = 0; num2 < 5; num2++)
			{
				this._upkeepDiplomaticStation[num2] = XmlHelper.GetData<int>(xmlElement, "UpkeepDiplomatic" + (num2 + 1));
			}
			for (int num3 = 0; num3 < 5; num3++)
			{
				this._upkeepGateStation[num3] = XmlHelper.GetData<int>(xmlElement, "UpkeepGate" + (num3 + 1));
			}
			this._starSystemEntryPointRange = XmlHelper.GetData<float>(xmlElement, "StarSystemEntryPointRange");
			this._tacStealthArmorBonus = XmlHelper.GetData<float>(xmlElement, "TacStealthArmorSignature");
			this._slewModeMultiplier = XmlHelper.GetData<float>(xmlElement, "SlewModeMultiplier");
			this._slewModeDecelMultiplier = XmlHelper.GetData<float>(xmlElement, "SlewModeDecelMultiplier");
			this._slewModeExitRange = XmlHelper.GetData<float>(xmlElement, "SlewModeExitRange");
			this._slewModeEnterOffset = XmlHelper.GetData<float>(xmlElement, "SlewModeEnterOffset");
			this._globalSpotterRanges = new GlobalSpotterRangeData();
			XmlElement xmlElement6 = xmlElement["SpotterRangeTable"];
			if (xmlElement6 != null)
			{
				this._globalSpotterRanges.SpotterValues[0] = float.Parse(xmlElement6.GetAttribute("br"));
				this._globalSpotterRanges.SpotterValues[1] = float.Parse(xmlElement6.GetAttribute("cr"));
				this._globalSpotterRanges.SpotterValues[2] = float.Parse(xmlElement6.GetAttribute("dn"));
				this._globalSpotterRanges.SpotterValues[3] = float.Parse(xmlElement6.GetAttribute("lv"));
				this._globalSpotterRanges.SpotterValues[4] = float.Parse(xmlElement6.GetAttribute("sn"));
				this._globalSpotterRanges.StationLVLOffset = float.Parse(xmlElement6.GetAttribute("snlvloffset"));
			}
			XmlElement xmlElement7 = xmlElement["MineField"];
			if (xmlElement7 != null)
			{
				this._mineFieldParams.Width = XmlHelper.GetData<float>(xmlElement7, "Width");
				this._mineFieldParams.Length = XmlHelper.GetData<float>(xmlElement7, "Length");
				this._mineFieldParams.Height = XmlHelper.GetData<float>(xmlElement7, "Height");
				this._mineFieldParams.SpacingOffset = XmlHelper.GetData<float>(xmlElement7, "SpacingOffset");
			}
			else
			{
				this._mineFieldParams.Width = 1000f;
				this._mineFieldParams.Length = 1000f;
				this._mineFieldParams.Height = 0f;
				this._mineFieldParams.SpacingOffset = 100f;
			}
			XmlElement xmlElement8 = xmlElement["SpecialProjectData"];
			if (xmlElement8 != null)
			{
				this._specialProjectData.MinimumIndyInvestigate = XmlHelper.GetData<int>(xmlElement8, "MinimumIndyInvestigate");
				this._specialProjectData.MaximumIndyInvestigate = XmlHelper.GetData<int>(xmlElement8, "MaximumIndyInvestigate");
				this._specialProjectData.MinimumAsteroidMonitorStudy = XmlHelper.GetData<int>(xmlElement8, "MinimumAsteroidMonitorStudy");
				this._specialProjectData.MaximumAsteroidMonitorStudy = XmlHelper.GetData<int>(xmlElement8, "MaximumAsteroidMonitorStudy");
				this._specialProjectData.MinimumRadiationShieldingStudy = XmlHelper.GetData<int>(xmlElement8, "MinimumRadiationShieldingStudy");
				this._specialProjectData.MaximumRadiationShieldingStudy = XmlHelper.GetData<int>(xmlElement8, "MaximumRadiationShieldingStudy");
				this._specialProjectData.MinimumNeutronStarStudy = XmlHelper.GetData<int>(xmlElement8, "MaximumNeutronStarStudy");
				this._specialProjectData.MaximumNeutronStarStudy = XmlHelper.GetData<int>(xmlElement8, "MaximumNeutronStarStudy");
				this._specialProjectData.MinimumGardenerStudy = XmlHelper.GetData<int>(xmlElement8, "MaximumGardenerStudy");
				this._specialProjectData.MaximumGardenerStudy = XmlHelper.GetData<int>(xmlElement8, "MaximumGardenerStudy");
			}
			else
			{
				this._specialProjectData.MinimumIndyInvestigate = 40000;
				this._specialProjectData.MaximumIndyInvestigate = 40000;
				this._specialProjectData.MinimumAsteroidMonitorStudy = 40000;
				this._specialProjectData.MaximumAsteroidMonitorStudy = 40000;
			}
			XmlElement rootNode = xmlElement["DefenseManagerSettings"];
			if (xmlElement8 != null)
			{
				this._defenseManagerSettings.SDBCPCost = XmlHelper.GetData<int>(rootNode, "SDBCPCost");
				this._defenseManagerSettings.MineLayerCPCost = XmlHelper.GetData<int>(rootNode, "MineLayerCPCost");
				this._defenseManagerSettings.PoliceCPCost = XmlHelper.GetData<int>(rootNode, "PoliceCPCost");
				this._defenseManagerSettings.ScanSatCPCost = XmlHelper.GetData<int>(rootNode, "ScanSatCPCost");
				this._defenseManagerSettings.DroneSatCPCost = XmlHelper.GetData<int>(rootNode, "DroneSatCPCost");
				this._defenseManagerSettings.TorpSatCPCost = XmlHelper.GetData<int>(rootNode, "TorpSatCPCost");
				this._defenseManagerSettings.BRSatCPCost = XmlHelper.GetData<int>(rootNode, "BRSatCPCost");
				this._defenseManagerSettings.MonitorSatCPCost = XmlHelper.GetData<int>(rootNode, "MonitorSatCPCost");
				this._defenseManagerSettings.MissileSatCPCost = XmlHelper.GetData<int>(rootNode, "MissileSatCPCost");
			}
			else
			{
				this._defenseManagerSettings.SDBCPCost = 0;
				this._defenseManagerSettings.MineLayerCPCost = 0;
				this._defenseManagerSettings.PoliceCPCost = 0;
				this._defenseManagerSettings.ScanSatCPCost = 0;
				this._defenseManagerSettings.DroneSatCPCost = 0;
				this._defenseManagerSettings.TorpSatCPCost = 0;
				this._defenseManagerSettings.BRSatCPCost = 0;
				this._defenseManagerSettings.MonitorSatCPCost = 0;
				this._defenseManagerSettings.MissileSatCPCost = 0;
			}
			XmlElement xmlElement9 = xmlElement["MiniShips"];
			if (xmlElement9 != null)
			{
				int num4 = 0;
				foreach (XmlElement current3 in xmlElement9.OfType<XmlElement>())
				{
					this._miniShipMap.Add(current3.GetAttribute("type"), new AssetDatabase.MiniMapData
					{
						ID = num4,
						Location = current3.GetAttribute("location")
					});
					num4++;
				}
			}
			XmlElement xmlElement10 = xmlElement["CritHitPercentages"];
			if (xmlElement10 != null)
			{
				for (int num5 = 0; num5 < 8; num5++)
				{
					this._critHitChances[num5].Chances = new int[25];
				}
				foreach (XmlElement current4 in xmlElement10.OfType<XmlElement>())
				{
					int num6 = (int)((CritHitType)Enum.Parse(typeof(CritHitType), current4.GetAttribute("type")));
					this._critHitChances[0].Chances[num6] = int.Parse(current4.GetAttribute("cmd"));
					this._critHitChances[1].Chances[num6] = int.Parse(current4.GetAttribute("mis"));
					this._critHitChances[2].Chances[num6] = int.Parse(current4.GetAttribute("eng"));
					this._critHitChances[3].Chances[num6] = int.Parse(current4.GetAttribute("sn"));
					this._critHitChances[4].Chances[num6] = int.Parse(current4.GetAttribute("monster"));
					this._critHitChances[5].Chances[num6] = int.Parse(current4.GetAttribute("loa_cmd"));
					this._critHitChances[6].Chances[num6] = int.Parse(current4.GetAttribute("loa_mis"));
					this._critHitChances[7].Chances[num6] = int.Parse(current4.GetAttribute("loa_eng"));
				}
			}
		}
		private void LoadRandomEncounterOdds(XmlDocument d)
		{
			XmlElement xmlElement = d["CommonAssets"]["RandomEncounterOdds"];
			foreach (XmlElement xmlElement2 in xmlElement.ChildNodes)
			{
				RandomEncounter key;
				if (Enum.TryParse<RandomEncounter>(xmlElement2.Name, out key))
				{
					this._randomEncounterOdds.Add(key, int.Parse(xmlElement2.InnerText));
				}
			}
		}
		private void LoadEasterEggOdds(XmlDocument d)
		{
			XmlElement xmlElement = d["CommonAssets"]["EasterEggOdds"];
			foreach (XmlElement xmlElement2 in xmlElement.ChildNodes)
			{
				EasterEgg key;
				if (Enum.TryParse<EasterEgg>(xmlElement2.Name, out key))
				{
					this._easterEggOdds.Add(key, int.Parse(xmlElement2.InnerText));
				}
			}
		}
		private void LoadGMOdds(XmlDocument d)
		{
			XmlElement xmlElement = d["CommonAssets"]["GMOdds"];
			foreach (XmlElement xmlElement2 in xmlElement.ChildNodes)
			{
				EasterEgg key;
				if (Enum.TryParse<EasterEgg>(xmlElement2.Name, out key))
				{
					this._gmOdds.Add(key, int.Parse(xmlElement2.InnerText));
				}
			}
		}
		private static XmlDocument LoadFile(string file)
		{
			string[] array = ScriptHost.FileSystem.FindFiles(file);
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(ScriptHost.FileSystem, array[0]);
			for (int i = 1; i < array.Length; i++)
			{
				XmlDocument xmlDocument2 = new XmlDocument();
				xmlDocument2.Load(ScriptHost.FileSystem, array[i]);
				foreach (XmlNode current in xmlDocument2.DocumentElement.ChildNodes.OfType<XmlElement>().Cast<XmlElement>())
				{
					xmlDocument.DocumentElement.AppendChild(xmlDocument.ImportNode(current, true));
				}
			}
			return xmlDocument;
		}
		private void LoadFleetTemplates()
		{
			if (this._fleetTemplates == null)
			{
				this._fleetTemplates = new List<FleetTemplate>();
				Stream stream = ScriptHost.FileSystem.CreateStream("factions\\fleet_templates.xml");
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.Load(stream);
				int num = 1;
				foreach (XmlElement current in 
					from x in xmlDocument["FleetTemplates"].OfType<XmlElement>()
					where x.Name == "FleetTemplate"
					select x)
				{
					FleetTemplate fleetTemplate = new FleetTemplate();
					fleetTemplate.TemplateID = num;
					num++;
					fleetTemplate.Name = current.GetAttribute("name");
					fleetTemplate.FleetName = (current.GetAttribute("fleetName") ?? "Default Fleet");
					if (current.HasAttribute("initial"))
					{
						fleetTemplate.Initial = bool.Parse(current.GetAttribute("initial"));
					}
					else
					{
						fleetTemplate.Initial = false;
					}
					foreach (XmlElement current2 in current.ChildNodes.OfType<XmlElement>())
					{
						if (current2.Name == "StanceWeight")
						{
							fleetTemplate.StanceWeights[(AIStance)Enum.Parse(typeof(AIStance), current2.GetAttribute("stance"))] = int.Parse(current2.InnerText);
						}
						else
						{
							if (current2.Name == "Ship")
							{
								ShipInclude shipInclude = new ShipInclude();
								shipInclude.InclusionType = (ShipInclusionType)Enum.Parse(typeof(ShipInclusionType), current2.GetAttribute("inclusion"));
								shipInclude.Amount = (current2.HasAttribute("amount") ? int.Parse(current2.GetAttribute("amount")) : ((shipInclude.InclusionType == ShipInclusionType.FILL) ? 0 : 1));
								shipInclude.WeaponRole = (current2.HasAttribute("weaponRole") ? new WeaponRole?((WeaponRole)Enum.Parse(typeof(WeaponRole), current2.GetAttribute("weaponRole"))) : null);
								shipInclude.Faction = (current2.HasAttribute("faction") ? current2.GetAttribute("faction") : string.Empty);
								shipInclude.FactionExclusion = (current2.HasAttribute("nfaction") ? current2.GetAttribute("nfaction") : string.Empty);
								shipInclude.ShipRole = (ShipRole)Enum.Parse(typeof(ShipRole), current2.InnerText);
								fleetTemplate.ShipIncludes.Add(shipInclude);
							}
							else
							{
								if (current2.Name == "MissionType")
								{
									MissionType item = (MissionType)Enum.Parse(typeof(MissionType), current2.InnerText);
									fleetTemplate.MissionTypes.Add(item);
								}
								else
								{
									if (current2.Name == "MinToMaintain")
									{
										using (IEnumerator<XmlElement> enumerator3 = current2.ChildNodes.OfType<XmlElement>().GetEnumerator())
										{
											while (enumerator3.MoveNext())
											{
												XmlElement current3 = enumerator3.Current;
												if (current3.Name == "Amount")
												{
													fleetTemplate.MinFleetsForStance[(AIStance)Enum.Parse(typeof(AIStance), current3.GetAttribute("stance"))] = int.Parse(current3.InnerText);
												}
											}
											continue;
										}
									}
									if (current2.Name == "AllowableFactions")
									{
										foreach (XmlElement current4 in current2.ChildNodes.OfType<XmlElement>())
										{
											if (current4.Name == "Faction")
											{
												fleetTemplate.AllowableFactions.Add(current4.InnerText);
											}
										}
									}
								}
							}
						}
					}
					this._fleetTemplates.Add(fleetTemplate);
				}
				stream.Dispose();
			}
		}
		private static IEnumerable<string> EnumerateCommonMaterialDictionaries(XmlDocument d)
		{
			foreach (string current in 
				from x in d["CommonAssets"].OfType<XmlElement>()
				where x.Name.Equals("MaterialDictionary", StringComparison.InvariantCulture)
				select x.InnerText)
			{
				yield return current;
			}
			yield break;
		}
		private static IEnumerable<string> EnumerateSplashScreenImageNames(XmlElement node)
		{
			if (node != null)
			{
				foreach (XmlElement current in 
					from element in node.OfType<XmlElement>()
					where element.Name == "SplashScreen"
					select element)
				{
					string attribute = current.GetAttribute("image");
					if (!string.IsNullOrEmpty(attribute))
					{
						yield return attribute;
					}
				}
			}
			yield break;
		}
		private static string[] LoadSplashScreenImageNames(XmlDocument commonAssetsDoc, IEnumerable<Faction> factions)
		{
			List<string> list = new List<string>();
			list.AddRange(AssetDatabase.EnumerateSplashScreenImageNames(commonAssetsDoc["CommonAssets"]["SplashScreens"]));
			foreach (Faction current in factions)
			{
				XmlDocument xmlDocument = Faction.LoadMergedXMLDocument(current.FactionFileName);
				list.AddRange(AssetDatabase.EnumerateSplashScreenImageNames(xmlDocument["Faction"]["SplashScreens"]));
			}
			return list.ToArray();
		}
		public string GetRandomSplashScreenImageName()
		{
			int num = this._random.Next(this._splashScreenImageNames.Length);
			return this._splashScreenImageNames[num];
		}
		public static List<string> LoadTechTreeModels(XmlElement element)
		{
			List<string> list = new List<string>();
			if (element != null)
			{
				list.AddRange(
					from x in element.ChildNodes.OfType<XmlElement>()
					where x.Name == "Model"
					select x into y
					select y.InnerText);
			}
			return list;
		}
		public static List<string> LoadTechTreeRoots(XmlElement element)
		{
			List<string> list = new List<string>();
			if (element != null)
			{
				list.AddRange(
					from x in element.ChildNodes.OfType<XmlElement>()
					where x.Name == "RootNode"
					select x into y
					select y.InnerText);
			}
			return list;
		}
		private static List<string> LoadTechTreeModelsFromCommonAssetsXml(XmlDocument commonAssetsDoc)
		{
			return AssetDatabase.LoadTechTreeModels(commonAssetsDoc["CommonAssets"]["TechTree"]);
		}
		private static List<string> LoadTechTreeRootsFromCommonAssetsXml(XmlDocument commonAssetsDoc)
		{
			return AssetDatabase.LoadTechTreeRoots(commonAssetsDoc["CommonAssets"]["TechTree"]);
		}
		public AssetDatabase(App app)
		{
			XmlDocument xmlDocument = AssetDatabase.LoadFile("commonassets.xml");
			this._cachedGlobals = new Dictionary<string, object>();
			this.InitializeAIDifficultyBonuses();
			this.LoadFleetTemplates();
			this.LoadGlobalsAndStratModifiers(xmlDocument);
			this.LoadMoralEventModifiers(AssetDatabase.LoadFile("moralmodifiers.xml"));
			this.LoadDefaultStratModifiers(AssetDatabase.LoadFile("defaultstratmodifiers.xml"));
			this.LoadTechBonusValues(AssetDatabase.LoadFile("techbonuses.xml"));
			this.LoadGovernmentActionModifiers(AssetDatabase.LoadFile("govactionmodifiers.xml"));
			this.LoadRandomEncounterOdds(xmlDocument);
			this.LoadEasterEggOdds(xmlDocument);
			this.LoadGMOdds(xmlDocument);
			this._governmentEffects.LoadFromFile(AssetDatabase.LoadFile("goveffects.xml"));
			this._commonMaterialDictionaries = AssetDatabase.EnumerateCommonMaterialDictionaries(xmlDocument).ToArray<string>();
			this._turretHousings = TurretHousingLibrary.Enumerate().ToArray<LogicalTurretHousing>();
			this._weapons = WeaponLibrary.Enumerate(app).ToArray<LogicalWeapon>();
			this._modules = ModuleLibrary.Enumerate().ToArray<LogicalModule>();
			this._modulesToAssignByDefault = (
				from x in this._modules
				where x.AssignByDefault
				select x).ToArray<LogicalModule>();
			this._psionics = PsionicLibrary.Enumerate(xmlDocument).ToArray<LogicalPsionic>();
			this._suulkaPsiBonuses = SuulkaPsiBonusLibrary.Enumerate(xmlDocument).ToArray<SuulkaPsiBonus>();
			this._shields = ShieldLibrary.Enumerate(xmlDocument).ToArray<LogicalShield>();
			this._shipSparks = ShipSparksLibrary.Enumerate(xmlDocument).ToArray<LogicalShipSpark>();
			this._shipEMPEffect = new LogicalEffect();
			this._shipEMPEffect.Name = XmlHelper.GetData<string>(xmlDocument["CommonAssets"], "ShipEMPSpark");
			this._factions = FactionLibrary.Enumerate().ToArray<Faction>();
			this._races = RaceLibrary.Enumerate().ToArray<Race>();
			this._shipSections = new HashSet<ShipSectionAsset>(SectionLibrary.Enumerate(this));
			this._shipSectionsByFilename = new Dictionary<string, ShipSectionAsset>();
			foreach (ShipSectionAsset current in this._shipSections)
			{
				this._shipSectionsByFilename[current.FileName] = current;
			}
			this._planetgenrules = new PlanetGraphicsRules();
			this._skyDefinitions = Kerberos.Sots.GameObjects.SkyDefinitions.LoadFromXml();
			if (AssetDatabase._commonStrings != null)
			{
				throw new InvalidOperationException("CommonStrings already created.");
			}
			AssetDatabase._commonStrings = new CommonStrings(ScriptHost.TwoLetterISOLanguageName);
			this.IntelMissions = new IntelMissionDescMap();
			this._masterTechTree = new TechTree();
			TechnologyXmlUtility.LoadTechTreeFromXml("Tech\\techtree.techtree", ref this._masterTechTree);
			this._masterTechTreeRoots = this._masterTechTree.GetRoots();
			this._techTreeModels = AssetDatabase.LoadTechTreeModelsFromCommonAssetsXml(xmlDocument);
			this._techTreeRoots = AssetDatabase.LoadTechTreeRootsFromCommonAssetsXml(xmlDocument);
			this._splashScreenImageNames = AssetDatabase.LoadSplashScreenImageNames(xmlDocument, this._factions);
			this.AIResearchFramework = new AIResearchFramework();
			this.DiplomacyStateChangeMap = new Dictionary<DiplomacyStateChange, int>
			{

				{
					new DiplomacyStateChange
					{
						lower = DiplomacyState.WAR,
						upper = DiplomacyState.NEUTRAL
					},
					this.TreatyArmisticeWarNeutralPointCost
				},

				{
					new DiplomacyStateChange
					{
						lower = DiplomacyState.NEUTRAL,
						upper = DiplomacyState.CEASE_FIRE
					},
					this.TreatyArmisticeNeutralCeasefirePointCost
				},

				{
					new DiplomacyStateChange
					{
						lower = DiplomacyState.NEUTRAL,
						upper = DiplomacyState.NON_AGGRESSION
					},
					this.TreatyArmisticeNeutralNonAggroPointCost
				},

				{
					new DiplomacyStateChange
					{
						lower = DiplomacyState.CEASE_FIRE,
						upper = DiplomacyState.NON_AGGRESSION
					},
					this.TreatyArmisticeCeaseFireNonAggroPointCost
				},

				{
					new DiplomacyStateChange
					{
						lower = DiplomacyState.CEASE_FIRE,
						upper = DiplomacyState.PEACE
					},
					this.TreatyArmisticeCeaseFirePeacePointCost
				},

				{
					new DiplomacyStateChange
					{
						lower = DiplomacyState.NON_AGGRESSION,
						upper = DiplomacyState.PEACE
					},
					this.TreatyArmisticeNonAggroPeaceCost
				},

				{
					new DiplomacyStateChange
					{
						lower = DiplomacyState.PEACE,
						upper = DiplomacyState.ALLIED
					},
					this.TreatyArmisticePeaceAllianceCost
				}
			};
		}
		public string GetStationModuleAsset(ModuleEnums.StationModuleType type, string factionName)
		{
			string format = AssetDatabase.StationModuleTypeAssetMap[type];
			return string.Format(format, factionName);
		}
		public string GetRandomBadgeTexture(string faction, Random rng)
		{
			string[] badgeTexturePaths = this.Factions.First((Faction x) => x.Name.Equals(faction, StringComparison.InvariantCulture)).BadgeTexturePaths;
			if (badgeTexturePaths == null || badgeTexturePaths.Length == 0)
			{
				return string.Empty;
			}
			return badgeTexturePaths[rng.Next(badgeTexturePaths.Length)];
		}
		public string GetRandomAvatarTexture(string faction, Random rng)
		{
			string[] avatarTexturePaths = this.Factions.First((Faction x) => x.Name.Equals(faction, StringComparison.InvariantCulture)).AvatarTexturePaths;
			if (avatarTexturePaths == null || avatarTexturePaths.Length == 0)
			{
				return string.Empty;
			}
			return avatarTexturePaths[rng.Next(avatarTexturePaths.Length)];
		}
		public void Dispose()
		{
			LogicalWeapon[] weapons = this._weapons;
			for (int i = 0; i < weapons.Length; i++)
			{
				LogicalWeapon logicalWeapon = weapons[i];
				logicalWeapon.Dispose();
			}
		}
	}
}
