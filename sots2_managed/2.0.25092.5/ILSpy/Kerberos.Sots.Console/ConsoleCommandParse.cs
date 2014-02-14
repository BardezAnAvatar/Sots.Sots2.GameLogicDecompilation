using Kerberos.Sots.Data;
using Kerberos.Sots.Encounters;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
namespace Kerberos.Sots.Console
{
	internal static class ConsoleCommandParse
	{
		private class CommandInfo
		{
			public string[] Aliases;
			public string Description;
			public int ParameterCount;
			public Action<App, IEnumerable<string>> Action;
		}
		private static readonly ConsoleCommandParse.CommandInfo[] _commandInfos;
		private static void Trace(string message)
		{
			App.Log.Trace(message, "con");
		}
		private static void Warn(string message)
		{
			App.Log.Warn(message, "con");
		}
		static ConsoleCommandParse()
		{
			ConsoleCommandParse.CommandInfo[] array = new ConsoleCommandParse.CommandInfo[69];
			ConsoleCommandParse.CommandInfo[] arg_6A_0 = array;
			int arg_6A_1 = 0;
			ConsoleCommandParse.CommandInfo commandInfo = new ConsoleCommandParse.CommandInfo();
			commandInfo.Aliases = new string[]
			{
				"help",
				"?"
			};
			commandInfo.Description = "Display available commands.";
			commandInfo.ParameterCount = 0;
			commandInfo.Action = delegate(App game, IEnumerable<string> parms)
			{
				StringBuilder stringBuilder = new StringBuilder();
				IOrderedEnumerable<string> orderedEnumerable = 
					from y in ConsoleCommandParse._commandInfos.SelectMany((ConsoleCommandParse.CommandInfo x) => x.Aliases).Distinct<string>()
					orderby y
					select y;
				string b = orderedEnumerable.Last<string>();
				foreach (string current in orderedEnumerable)
				{
					stringBuilder.Append(current);
					if (current != b)
					{
						stringBuilder.Append(", ");
					}
					stringBuilder.AppendLine();
				}
				ConsoleCommandParse.Trace(stringBuilder.ToString());
			};
			arg_6A_0[arg_6A_1] = commandInfo;
			ConsoleCommandParse.CommandInfo[] arg_CC_0 = array;
			int arg_CC_1 = 1;
			ConsoleCommandParse.CommandInfo commandInfo2 = new ConsoleCommandParse.CommandInfo();
			commandInfo2.Aliases = new string[]
			{
				"help",
				"?"
			};
			commandInfo2.Description = "Display information about a command.";
			commandInfo2.ParameterCount = 1;
			commandInfo2.Action = delegate(App game, IEnumerable<string> parms)
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (ConsoleCommandParse.CommandInfo current in 
					from x in ConsoleCommandParse._commandInfos
					where x.Aliases.Any((string y) => y.Equals(parms.First<string>(), StringComparison.InvariantCulture))
					select x)
				{
					string b = current.Aliases.Last<string>();
					string[] aliases = current.Aliases;
					for (int i = 0; i < aliases.Length; i++)
					{
						string text = aliases[i];
						stringBuilder.Append(text);
						if (text != b)
						{
							stringBuilder.Append(", ");
						}
					}
					stringBuilder.AppendLine(": (" + current.ParameterCount + ")");
					stringBuilder.AppendLine(current.Description);
					stringBuilder.AppendLine();
				}
				ConsoleCommandParse.Trace(stringBuilder.ToString());
			};
			arg_CC_0[arg_CC_1] = commandInfo2;
			ConsoleCommandParse.CommandInfo[] arg_12E_0 = array;
			int arg_12E_1 = 2;
			ConsoleCommandParse.CommandInfo commandInfo3 = new ConsoleCommandParse.CommandInfo();
			commandInfo3.Aliases = new string[]
			{
				"exit",
				"quit"
			};
			commandInfo3.Description = "Exit the game application.";
			commandInfo3.ParameterCount = 0;
			commandInfo3.Action = delegate(App game, IEnumerable<string> parms)
			{
				game.RequestExit();
			};
			arg_12E_0[arg_12E_1] = commandInfo3;
			ConsoleCommandParse.CommandInfo[] arg_187_0 = array;
			int arg_187_1 = 3;
			ConsoleCommandParse.CommandInfo commandInfo4 = new ConsoleCommandParse.CommandInfo();
			commandInfo4.Aliases = new string[]
			{
				"state"
			};
			commandInfo4.Description = "Display the current and pending game states.";
			commandInfo4.ParameterCount = 0;
			commandInfo4.Action = delegate(App game, IEnumerable<string> parms)
			{
				ConsoleCommandParse.Trace((" Current state: " + game.CurrentState) ?? "none");
				ConsoleCommandParse.Trace((" Pending state: " + game.PendingState) ?? "none");
			};
			arg_187_0[arg_187_1] = commandInfo4;
			ConsoleCommandParse.CommandInfo[] arg_1E6_0 = array;
			int arg_1E6_1 = 4;
			ConsoleCommandParse.CommandInfo commandInfo5 = new ConsoleCommandParse.CommandInfo();
			commandInfo5.Aliases = new string[]
			{
				"state"
			};
			commandInfo5.Description = "Switch to the specified game state.";
			commandInfo5.ParameterCount = 1;
			commandInfo5.Action = delegate(App game, IEnumerable<string> parms)
			{
				game.SwitchGameState(parms.First<string>());
			};
			arg_1E6_0[arg_1E6_1] = commandInfo5;
			ConsoleCommandParse.CommandInfo[] arg_245_0 = array;
			int arg_245_1 = 5;
			ConsoleCommandParse.CommandInfo commandInfo6 = new ConsoleCommandParse.CommandInfo();
			commandInfo6.Aliases = new string[]
			{
				"states"
			};
			commandInfo6.Description = "Display all game states.";
			commandInfo6.ParameterCount = 0;
			commandInfo6.Action = delegate(App game, IEnumerable<string> parms)
			{
				ConsoleCommandParse.Trace("Available game states:");
				foreach (GameState current in game.States)
				{
					ConsoleCommandParse.Trace(" " + current.Name);
				}
			};
			arg_245_0[arg_245_1] = commandInfo6;
			ConsoleCommandParse.CommandInfo[] arg_2A4_0 = array;
			int arg_2A4_1 = 6;
			ConsoleCommandParse.CommandInfo commandInfo7 = new ConsoleCommandParse.CommandInfo();
			commandInfo7.Aliases = new string[]
			{
				"msglog"
			};
			commandInfo7.Description = "Enable or disable the logging of engine messages.";
			commandInfo7.ParameterCount = 1;
			commandInfo7.Action = delegate(App game, IEnumerable<string> parms)
			{
				bool logEnable = bool.Parse(parms.First<string>());
				UICommChannel.LogEnable = logEnable;
				ScriptCommChannel.LogEnable = logEnable;
			};
			arg_2A4_0[arg_2A4_1] = commandInfo7;
			ConsoleCommandParse.CommandInfo[] arg_303_0 = array;
			int arg_303_1 = 7;
			ConsoleCommandParse.CommandInfo commandInfo8 = new ConsoleCommandParse.CommandInfo();
			commandInfo8.Aliases = new string[]
			{
				"load_tac_targeting"
			};
			commandInfo8.Description = "Loads combat with two hiver cruisers for target practice.";
			commandInfo8.ParameterCount = 0;
			commandInfo8.Action = delegate(App game, IEnumerable<string> parms)
			{
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(ConsoleResources.load_tac_targeting_config);
				CombatState gameState = game.GetGameState<CombatState>();
				int num = 0;
				game.SwitchGameState(gameState, new object[]
				{
					num,
					xmlDocument
				});
			};
			arg_303_0[arg_303_1] = commandInfo8;
			ConsoleCommandParse.CommandInfo[] arg_362_0 = array;
			int arg_362_1 = 8;
			ConsoleCommandParse.CommandInfo commandInfo9 = new ConsoleCommandParse.CommandInfo();
			commandInfo9.Aliases = new string[]
			{
				"toggleAI"
			};
			commandInfo9.Description = "Turn AI on/off.";
			commandInfo9.ParameterCount = 0;
			commandInfo9.Action = delegate(App game, IEnumerable<string> parms)
			{
				App.m_bAI_Enabled = !App.m_bAI_Enabled;
				if (App.m_bAI_Enabled)
				{
					ConsoleCommandParse.Trace("AI enabled");
					return;
				}
				ConsoleCommandParse.Trace("AI disabled");
			};
			arg_362_0[arg_362_1] = commandInfo9;
			ConsoleCommandParse.CommandInfo[] arg_3C2_0 = array;
			int arg_3C2_1 = 9;
			ConsoleCommandParse.CommandInfo commandInfo10 = new ConsoleCommandParse.CommandInfo();
			commandInfo10.Aliases = new string[]
			{
				"togglePlayerAI"
			};
			commandInfo10.Description = "Turn AI on/off for human player.";
			commandInfo10.ParameterCount = 0;
			commandInfo10.Action = delegate(App game, IEnumerable<string> parms)
			{
				App.m_bPlayerAI_Enabled = !App.m_bPlayerAI_Enabled;
				if (App.m_bPlayerAI_Enabled)
				{
					ConsoleCommandParse.Trace("Human Player's AI enabled");
					return;
				}
				ConsoleCommandParse.Trace("Human Player's AI disabled");
			};
			arg_3C2_0[arg_3C2_1] = commandInfo10;
			ConsoleCommandParse.CommandInfo[] arg_422_0 = array;
			int arg_422_1 = 10;
			ConsoleCommandParse.CommandInfo commandInfo11 = new ConsoleCommandParse.CommandInfo();
			commandInfo11.Aliases = new string[]
			{
				"debugFUP"
			};
			commandInfo11.Description = "Make all ships path to a Forming Up Point appropriate for the destination, rather than the destination itself.";
			commandInfo11.ParameterCount = 0;
			commandInfo11.Action = delegate(App game, IEnumerable<string> parms)
			{
				App.m_bDebugFup = !App.m_bDebugFup;
				if (App.m_bDebugFup)
				{
					ConsoleCommandParse.Trace("Form Up Point debugging enabled");
					return;
				}
				ConsoleCommandParse.Trace("Form Up Point debugging disabled");
			};
			arg_422_0[arg_422_1] = commandInfo11;
			ConsoleCommandParse.CommandInfo[] arg_482_0 = array;
			int arg_482_1 = 11;
			ConsoleCommandParse.CommandInfo commandInfo12 = new ConsoleCommandParse.CommandInfo();
			commandInfo12.Aliases = new string[]
			{
				"gamedbloc"
			};
			commandInfo12.Description = "Relocate the live game database to a file or :memory: depending on diagnostic needs.";
			commandInfo12.ParameterCount = 1;
			commandInfo12.Action = delegate(App game, IEnumerable<string> parms)
			{
				if (game.GameDatabase != null)
				{
					game.GameDatabase.ChangeLiveLocationAndOpen(parms.First<string>());
				}
			};
			arg_482_0[arg_482_1] = commandInfo12;
			ConsoleCommandParse.CommandInfo[] arg_4E2_0 = array;
			int arg_4E2_1 = 12;
			ConsoleCommandParse.CommandInfo commandInfo13 = new ConsoleCommandParse.CommandInfo();
			commandInfo13.Aliases = new string[]
			{
				"gamedbloc"
			};
			commandInfo13.Description = "Display the current location of the live game database.";
			commandInfo13.ParameterCount = 0;
			commandInfo13.Action = delegate(App game, IEnumerable<string> parms)
			{
				ConsoleCommandParse.Trace(("  " + game.GameDatabase != null) ? game.GameDatabase.LiveLocation : "(not available)");
			};
			arg_4E2_0[arg_4E2_1] = commandInfo13;
			ConsoleCommandParse.CommandInfo[] arg_542_0 = array;
			int arg_542_1 = 13;
			ConsoleCommandParse.CommandInfo commandInfo14 = new ConsoleCommandParse.CommandInfo();
			commandInfo14.Aliases = new string[]
			{
				"players"
			};
			commandInfo14.Description = "Display live game players.";
			commandInfo14.ParameterCount = 0;
			commandInfo14.Action = delegate(App game, IEnumerable<string> parms)
			{
				foreach (PlayerInfo current in game.GameDatabase.GetPlayerInfos())
				{
					string factionName = game.GameDatabase.GetFactionName(current.FactionID);
					ConsoleCommandParse.Trace(string.Format(" {0}: {1}, {2}, {3}", new object[]
					{
						current.ID,
						current.Name,
						factionName,
						game.Game.GetPlayerObject(current.ID).IsAI() ? "ai" : "NO ai"
					}));
				}
			};
			arg_542_0[arg_542_1] = commandInfo14;
			ConsoleCommandParse.CommandInfo[] arg_5A2_0 = array;
			int arg_5A2_1 = 14;
			ConsoleCommandParse.CommandInfo commandInfo15 = new ConsoleCommandParse.CommandInfo();
			commandInfo15.Aliases = new string[]
			{
				"addaiplayer"
			};
			commandInfo15.Description = "Add an AI player to the game with random starting conditions.";
			commandInfo15.ParameterCount = 0;
			commandInfo15.Action = delegate(App game, IEnumerable<string> parms)
			{
				Random random = new Random();
				HashSet<int> hashSet = new HashSet<int>(game.GameDatabase.GetStarSystemIDs());
				foreach (int current in game.GameDatabase.GetHomeWorldIDs())
				{
					hashSet.Remove(current);
				}
				int num = game.GameDatabase.InsertPlayer("AI Controlled " + Guid.NewGuid().ToString(), "human", new int?(hashSet.First<int>()), new Vector3(random.NextSingle(), random.NextSingle(), random.NextSingle()), new Vector3(1f, 1f, 1f), game.AssetDatabase.GetRandomBadgeTexture("human", random), game.AssetDatabase.GetRandomAvatarTexture("human", random), 100000.0, 0, true, false, false, 0, AIDifficulty.Normal);
				ResearchScreenState.BuildPlayerTechTree(game, game.AssetDatabase, game.GameDatabase, num);
				game.GameDatabase.InsertOrIgnoreAI(num, AIStance.EXPANDING);
			};
			arg_5A2_0[arg_5A2_1] = commandInfo15;
			ConsoleCommandParse.CommandInfo[] arg_602_0 = array;
			int arg_602_1 = 15;
			ConsoleCommandParse.CommandInfo commandInfo16 = new ConsoleCommandParse.CommandInfo();
			commandInfo16.Aliases = new string[]
			{
				"pcoverride"
			};
			commandInfo16.Description = "Override the existing primary and secondary colors for all players created after this point. (r1 g1 b1 r2 g2 b2)";
			commandInfo16.ParameterCount = 6;
			commandInfo16.Action = delegate(App game, IEnumerable<string> parms)
			{
				string[] array2 = parms.Take(6).ToArray<string>();
				Player.OverridePlayerColors(new Vector3(float.Parse(array2[0]), float.Parse(array2[1]), float.Parse(array2[2])), new Vector3(float.Parse(array2[3]), float.Parse(array2[4]), float.Parse(array2[5])));
			};
			arg_602_0[arg_602_1] = commandInfo16;
			ConsoleCommandParse.CommandInfo[] arg_662_0 = array;
			int arg_662_1 = 16;
			ConsoleCommandParse.CommandInfo commandInfo17 = new ConsoleCommandParse.CommandInfo();
			commandInfo17.Aliases = new string[]
			{
				"advmove"
			};
			commandInfo17.Description = "Set advanced formation movement enabled (true) or disabled (false).";
			commandInfo17.ParameterCount = 1;
			commandInfo17.Action = delegate(App game, IEnumerable<string> parms)
			{
				bool flag = bool.Parse(parms.First<string>());
				FormationDefinition.IsAdvancedFormationMovementEnabled = flag;
				if (flag)
				{
					ConsoleCommandParse.Trace("Advanced formation movement is ENABLED.");
					return;
				}
				ConsoleCommandParse.Trace("Advanced formation movement is DISABLED.");
			};
			arg_662_0[arg_662_1] = commandInfo17;
			ConsoleCommandParse.CommandInfo[] arg_6C2_0 = array;
			int arg_6C2_1 = 17;
			ConsoleCommandParse.CommandInfo commandInfo18 = new ConsoleCommandParse.CommandInfo();
			commandInfo18.Aliases = new string[]
			{
				"advmove"
			};
			commandInfo18.Description = "Displays the state of advanced formation movement.";
			commandInfo18.ParameterCount = 0;
			commandInfo18.Action = delegate(App game, IEnumerable<string> parms)
			{
				bool isAdvancedFormationMovementEnabled = FormationDefinition.IsAdvancedFormationMovementEnabled;
				if (isAdvancedFormationMovementEnabled)
				{
					ConsoleCommandParse.Trace("Advanced formation movement is ENABLED.");
					return;
				}
				ConsoleCommandParse.Trace("Advanced formation movement is DISABLED.");
			};
			arg_6C2_0[arg_6C2_1] = commandInfo18;
			ConsoleCommandParse.CommandInfo[] arg_722_0 = array;
			int arg_722_1 = 18;
			ConsoleCommandParse.CommandInfo commandInfo19 = new ConsoleCommandParse.CommandInfo();
			commandInfo19.Aliases = new string[]
			{
				"savedb"
			};
			commandInfo19.Description = "Saves the current game database to the given file.";
			commandInfo19.ParameterCount = 1;
			commandInfo19.Action = delegate(App game, IEnumerable<string> parms)
			{
				game.GameDatabase.Save(parms.First<string>());
			};
			arg_722_0[arg_722_1] = commandInfo19;
			ConsoleCommandParse.CommandInfo[] arg_782_0 = array;
			int arg_782_1 = 19;
			ConsoleCommandParse.CommandInfo commandInfo20 = new ConsoleCommandParse.CommandInfo();
			commandInfo20.Aliases = new string[]
			{
				"savecombat"
			};
			commandInfo20.Description = "Writes out a partial combat config file (xml) with the current positions of active ships. Uses default human cruiser ";
			commandInfo20.ParameterCount = 1;
			commandInfo20.Action = delegate(App game, IEnumerable<string> parms)
			{
				ConsoleCommandParse.Trace("Writing current combat configuration to '" + parms.First<string>() + "'...");
				if (!(game.CurrentState is CommonCombatState))
				{
					throw new InvalidOperationException("The current game state must be a CommonCombatState in order to write a CombatConfig file.");
				}
				CommonCombatState commonCombatState = game.CurrentState as CommonCombatState;
				commonCombatState.SaveCombatConfig(parms.First<string>());
				ConsoleCommandParse.Trace("OK.");
			};
			arg_782_0[arg_782_1] = commandInfo20;
			ConsoleCommandParse.CommandInfo[] arg_7E2_0 = array;
			int arg_7E2_1 = 20;
			ConsoleCommandParse.CommandInfo commandInfo21 = new ConsoleCommandParse.CommandInfo();
			commandInfo21.Aliases = new string[]
			{
				"endcombat"
			};
			commandInfo21.Description = "Forces an end condition in the running combat state and subsequent beaming of information back to the strat game.";
			commandInfo21.ParameterCount = 0;
			commandInfo21.Action = delegate(App game, IEnumerable<string> parms)
			{
				if (!(game.CurrentState is CommonCombatState))
				{
					throw new InvalidOperationException("Not in combat.");
				}
				CommonCombatState commonCombatState = game.CurrentState as CommonCombatState;
				if (!commonCombatState.EndCombat())
				{
					ConsoleCommandParse.Trace("Combat is already ending.");
					return;
				}
				ConsoleCommandParse.Trace("OK.");
			};
			arg_7E2_0[arg_7E2_1] = commandInfo21;
			ConsoleCommandParse.CommandInfo[] arg_842_0 = array;
			int arg_842_1 = 21;
			ConsoleCommandParse.CommandInfo commandInfo22 = new ConsoleCommandParse.CommandInfo();
			commandInfo22.Aliases = new string[]
			{
				"retain_combat_config"
			};
			commandInfo22.Description = "Specifies whether combat states should retain the configuration xml document for modification (true/false). Must be true before entering combat for 'savecombat' to work.";
			commandInfo22.ParameterCount = 1;
			commandInfo22.Action = delegate(App game, IEnumerable<string> parms)
			{
				CommonCombatState.RetainCombatConfig = bool.Parse(parms.First<string>());
				ConsoleCommandParse.Trace("CommonCombatState.RetainCombatConfig is now " + CommonCombatState.RetainCombatConfig.ToString().ToUpper() + ".");
			};
			arg_842_0[arg_842_1] = commandInfo22;
			ConsoleCommandParse.CommandInfo[] arg_8A2_0 = array;
			int arg_8A2_1 = 22;
			ConsoleCommandParse.CommandInfo commandInfo23 = new ConsoleCommandParse.CommandInfo();
			commandInfo23.Aliases = new string[]
			{
				"toggle_ai"
			};
			commandInfo23.Description = "Will enable Strategic AI control on the specified player.";
			commandInfo23.ParameterCount = 1;
			commandInfo23.Action = delegate(App game, IEnumerable<string> parms)
			{
				int num = int.Parse(parms.First<string>());
				Player player = game.GetPlayer(num);
				if (player != null)
				{
					ConsoleCommandParse.Trace(string.Concat(new object[]
					{
						"Toggling AI (",
						!player.IsAI(),
						") on player: ",
						num
					}));
					player.SetAI(!player.IsAI());
				}
			};
			arg_8A2_0[arg_8A2_1] = commandInfo23;
			ConsoleCommandParse.CommandInfo[] arg_902_0 = array;
			int arg_902_1 = 23;
			ConsoleCommandParse.CommandInfo commandInfo24 = new ConsoleCommandParse.CommandInfo();
			commandInfo24.Aliases = new string[]
			{
				"TechsDialog"
			};
			commandInfo24.Description = "Spawns dialog with all techs.";
			commandInfo24.ParameterCount = 0;
			commandInfo24.Action = delegate(App game, IEnumerable<string> parms)
			{
				game.UI.CreateDialog(new DebugTechDialog(game.Game, 0), null);
			};
			arg_902_0[arg_902_1] = commandInfo24;
			ConsoleCommandParse.CommandInfo[] arg_962_0 = array;
			int arg_962_1 = 24;
			ConsoleCommandParse.CommandInfo commandInfo25 = new ConsoleCommandParse.CommandInfo();
			commandInfo25.Aliases = new string[]
			{
				"possess_player"
			};
			commandInfo25.Description = "Allows the player to take control of another player.";
			commandInfo25.ParameterCount = 1;
			commandInfo25.Action = delegate(App game, IEnumerable<string> parms)
			{
				int num = int.Parse(parms.First<string>());
				Player player = game.GetPlayer(num);
				if (player != null)
				{
					ConsoleCommandParse.Trace("Possessing player ID: " + num);
					player.SetAI(false);
					game.Game.SetLocalPlayer(player);
					game.GetGameState<StarMapState>().RefreshStarmap(StarMapState.StarMapRefreshType.REFRESH_ALL);
				}
			};
			arg_962_0[arg_962_1] = commandInfo25;
			ConsoleCommandParse.CommandInfo[] arg_9C2_0 = array;
			int arg_9C2_1 = 25;
			ConsoleCommandParse.CommandInfo commandInfo26 = new ConsoleCommandParse.CommandInfo();
			commandInfo26.Aliases = new string[]
			{
				"add_demo_ships"
			};
			commandInfo26.Description = "This HACK adds demo ships to the first couple players (morrigi and zuul) at the first player's system.";
			commandInfo26.ParameterCount = 0;
			commandInfo26.Action = delegate(App game, IEnumerable<string> parms)
			{
				ConsoleCommandParse.Trace("Adding a bunch of new designs and ships and stuff...");
				GameDatabase gameDatabase = game.GameDatabase;
				int p1 = game.LocalPlayer.ID;
				int iD = game.GameDatabase.GetFleetInfosByPlayerID(p1, FleetType.FL_NORMAL).First<FleetInfo>().ID;
				int systemID = game.GameDatabase.GetHomeworlds().First((HomeworldInfo x) => x.PlayerID == p1).SystemID;
				DesignInfo design = new DesignInfo(p1, "WingStrike", new string[]
				{
					"factions\\morrigi\\sections\\cr_cmd.section",
					"factions\\morrigi\\sections\\cr_mis_armor.section",
					"factions\\morrigi\\sections\\cr_eng_fusion.section"
				});
				DesignInfo design2 = new DesignInfo(p1, "Darkness Shatters", new string[]
				{
					"factions\\morrigi\\sections\\cr_cmd.section",
					"factions\\morrigi\\sections\\cr_mis_carrier.section",
					"factions\\morrigi\\sections\\cr_eng_fusion.section"
				});
				DesignInfo design3 = new DesignInfo(p1, "War's End", new string[]
				{
					"factions\\morrigi\\sections\\cr_cmd_assault.section",
					"factions\\morrigi\\sections\\cr_mis_armor.section",
					"factions\\morrigi\\sections\\cr_eng_fusion.section"
				});
				DesignInfo design4 = new DesignInfo(p1, "Ke'Kona's Sorrow", new string[]
				{
					"factions\\morrigi\\sections\\cr_cmd.section",
					"factions\\morrigi\\sections\\cr_mis_barrage.section",
					"factions\\morrigi\\sections\\cr_eng_fusion.section"
				});
				DesignInfo design5 = new DesignInfo(p1, "Khan's Fist", new string[]
				{
					"factions\\morrigi\\sections\\cr_cmd.section",
					"factions\\morrigi\\sections\\cr_mis_cnc.section",
					"factions\\morrigi\\sections\\cr_eng_fusion.section"
				});
				DesignInfo design6 = new DesignInfo(p1, "Drone", new string[]
				{
					"factions\\morrigi\\sections\\br_drone.section"
				});
				DesignLab.SummarizeDesign(game.AssetDatabase, game.GameDatabase, design);
				DesignLab.SummarizeDesign(game.AssetDatabase, game.GameDatabase, design2);
				DesignLab.SummarizeDesign(game.AssetDatabase, game.GameDatabase, design3);
				DesignLab.SummarizeDesign(game.AssetDatabase, game.GameDatabase, design4);
				DesignLab.SummarizeDesign(game.AssetDatabase, game.GameDatabase, design5);
				DesignLab.SummarizeDesign(game.AssetDatabase, game.GameDatabase, design6);
				int designID = gameDatabase.InsertDesignByDesignInfo(design);
				int designID2 = gameDatabase.InsertDesignByDesignInfo(design2);
				int designID3 = gameDatabase.InsertDesignByDesignInfo(design3);
				int designID4 = gameDatabase.InsertDesignByDesignInfo(design4);
				int designID5 = gameDatabase.InsertDesignByDesignInfo(design5);
				int designID6 = gameDatabase.InsertDesignByDesignInfo(design6);
				gameDatabase.InsertShip(iD, designID, null, (ShipParams)0, null, 0);
				gameDatabase.InsertShip(iD, designID2, null, (ShipParams)0, null, 0);
				gameDatabase.InsertShip(iD, designID3, null, (ShipParams)0, null, 0);
				gameDatabase.InsertShip(iD, designID4, null, (ShipParams)0, null, 0);
				gameDatabase.InsertShip(iD, designID5, null, (ShipParams)0, null, 0);
				gameDatabase.InsertShip(iD, designID6, null, (ShipParams)0, null, 0);
				int iD2 = game.GameDatabase.GetPlayerInfos().First((PlayerInfo x) => game.Game.GetPlayerObject(x.ID).IsAI()).ID;
				int iD3 = game.GameDatabase.GetFleetInfosByPlayerID(iD2, FleetType.FL_NORMAL).First<FleetInfo>().ID;
				DesignInfo design7 = new DesignInfo(iD2, "The Pure", new string[]
				{
					"factions\\zuul\\sections\\cr_cmd.section",
					"factions\\zuul\\sections\\cr_mis_armor.section",
					"factions\\zuul\\sections\\cr_eng_fusion.section"
				});
				DesignInfo design8 = new DesignInfo(iD2, "The Divine", new string[]
				{
					"factions\\zuul\\sections\\cr_cmd.section",
					"factions\\zuul\\sections\\cr_mis_cnc.section",
					"factions\\zuul\\sections\\cr_eng_fusion.section"
				});
				DesignInfo design9 = new DesignInfo(iD2, "The Seekers", new string[]
				{
					"factions\\zuul\\sections\\cr_cmd_deepscan.section",
					"factions\\zuul\\sections\\cr_mis_armor.section",
					"factions\\zuul\\sections\\cr_eng_fusion.section"
				});
				DesignInfo design10 = new DesignInfo(iD2, "The First-Born", new string[]
				{
					"factions\\zuul\\sections\\cr_mis_rending_bore.section"
				});
				DesignLab.SummarizeDesign(game.AssetDatabase, game.GameDatabase, design7);
				DesignLab.SummarizeDesign(game.AssetDatabase, game.GameDatabase, design8);
				DesignLab.SummarizeDesign(game.AssetDatabase, game.GameDatabase, design9);
				DesignLab.SummarizeDesign(game.AssetDatabase, game.GameDatabase, design10);
				int designID7 = game.GameDatabase.InsertDesignByDesignInfo(design7);
				int designID8 = game.GameDatabase.InsertDesignByDesignInfo(design8);
				int designID9 = game.GameDatabase.InsertDesignByDesignInfo(design9);
				int designID10 = game.GameDatabase.InsertDesignByDesignInfo(design10);
				gameDatabase.InsertShip(iD3, designID7, null, (ShipParams)0, null, 0);
				gameDatabase.InsertShip(iD3, designID8, null, (ShipParams)0, null, 0);
				gameDatabase.InsertShip(iD3, designID9, null, (ShipParams)0, null, 0);
				gameDatabase.InsertShip(iD3, designID10, null, (ShipParams)0, null, 0);
				gameDatabase.UpdateFleetLocation(iD3, systemID, null);
				ConsoleCommandParse.Trace("OK.");
			};
			arg_9C2_0[arg_9C2_1] = commandInfo26;
			ConsoleCommandParse.CommandInfo[] arg_A22_0 = array;
			int arg_A22_1 = 26;
			ConsoleCommandParse.CommandInfo commandInfo27 = new ConsoleCommandParse.CommandInfo();
			commandInfo27.Aliases = new string[]
			{
				"show_research_debug"
			};
			commandInfo27.Description = "Causes the research screen debugging buttons to appear.";
			commandInfo27.ParameterCount = 0;
			commandInfo27.Action = delegate(App game, IEnumerable<string> parms)
			{
				game.GetGameState<ResearchScreenState>().ShowDebugControls();
			};
			arg_A22_0[arg_A22_1] = commandInfo27;
			ConsoleCommandParse.CommandInfo[] arg_A82_0 = array;
			int arg_A82_1 = 27;
			ConsoleCommandParse.CommandInfo commandInfo28 = new ConsoleCommandParse.CommandInfo();
			commandInfo28.Aliases = new string[]
			{
				"alltech"
			};
			commandInfo28.Description = "Gives player all technologies.";
			commandInfo28.ParameterCount = 0;
			commandInfo28.Action = delegate(App game, IEnumerable<string> parms)
			{
				ResearchScreenState.AcquireAllTechs(game.Game, game.LocalPlayer.ID);
				game.Game.CheckForNewEquipment(game.LocalPlayer.ID);
				game.Game.UpdateProfileTechs();
			};
			arg_A82_0[arg_A82_1] = commandInfo28;
			ConsoleCommandParse.CommandInfo[] arg_AE2_0 = array;
			int arg_AE2_1 = 28;
			ConsoleCommandParse.CommandInfo commandInfo29 = new ConsoleCommandParse.CommandInfo();
			commandInfo29.Aliases = new string[]
			{
				"debug_newseventart"
			};
			commandInfo29.Description = "Prints Debug feed for news event art.";
			commandInfo29.ParameterCount = 0;
			commandInfo29.Action = delegate(App game, IEnumerable<string> parms)
			{
				TurnEvent.DebugPrintNewsEventArt(game.Game);
			};
			arg_AE2_0[arg_AE2_1] = commandInfo29;
			ConsoleCommandParse.CommandInfo[] arg_B42_0 = array;
			int arg_B42_1 = 29;
			ConsoleCommandParse.CommandInfo commandInfo30 = new ConsoleCommandParse.CommandInfo();
			commandInfo30.Aliases = new string[]
			{
				"pimpedfleet"
			};
			commandInfo30.Description = "Gives you a pimped fleet";
			commandInfo30.ParameterCount = 0;
			commandInfo30.Action = delegate(App game, IEnumerable<string> parms)
			{
				int systemID = game.GameDatabase.GetPlayerHomeworld(game.LocalPlayer.ID).SystemID;
				int admiralID = game.GameDatabase.InsertAdmiral(game.LocalPlayer.ID, new int?(systemID), "Joe Kickass", "human", 10f, "male", 100f, 100f, 100);
				int fleetID = game.GameDatabase.InsertFleet(game.LocalPlayer.ID, admiralID, systemID, systemID, "1337 Fleet", FleetType.FL_NORMAL);
				DesignInfo designInfo = new DesignInfo();
				designInfo.PlayerID = game.LocalPlayer.ID;
				designInfo.Name = "Fit Shucker MKII";
				designInfo.Role = ShipRole.CARRIER;
				designInfo.DesignSections = new DesignSectionInfo[3];
				designInfo.DesignSections[0] = new DesignSectionInfo();
				designInfo.DesignSections[0].FilePath = "factions\\" + game.LocalPlayer.Faction.Name + "\\sections\\dn_eng_fusion.section";
				designInfo.DesignSections[1] = new DesignSectionInfo();
				designInfo.DesignSections[1].FilePath = "factions\\" + game.LocalPlayer.Faction.Name + "\\sections\\dn_mis_armor.section";
				designInfo.DesignSections[2] = new DesignSectionInfo();
				designInfo.DesignSections[2].FilePath = "factions\\" + game.LocalPlayer.Faction.Name + "\\sections\\dn_cmd_assault.section";
				DesignLab.SummarizeDesign(game.AssetDatabase, game.GameDatabase, designInfo);
				int designID = game.GameDatabase.InsertDesignByDesignInfo(designInfo);
				for (int i = 0; i < 3; i++)
				{
					game.GameDatabase.InsertShip(fleetID, designID, null, (ShipParams)0, null, 0);
				}
				new DesignInfo();
				designInfo.PlayerID = game.LocalPlayer.ID;
				designInfo.Name = "Little MEEP";
				designInfo.DesignSections = new DesignSectionInfo[2];
				designInfo.DesignSections[0] = new DesignSectionInfo();
				designInfo.DesignSections[0].FilePath = "factions\\" + game.LocalPlayer.Faction.Name + "\\sections\\br_eng_fusion.section";
				designInfo.DesignSections[1] = new DesignSectionInfo();
				designInfo.DesignSections[1].FilePath = "factions\\" + game.LocalPlayer.Faction.Name + "\\sections\\br_msn_spinal.section";
				DesignLab.SummarizeDesign(game.AssetDatabase, game.GameDatabase, designInfo);
				int designID2 = game.GameDatabase.InsertDesignByDesignInfo(designInfo);
				designInfo = new DesignInfo();
				designInfo.PlayerID = game.LocalPlayer.ID;
				designInfo.Name = "MEEP";
				designInfo.Role = ShipRole.CARRIER;
				designInfo.DesignSections = new DesignSectionInfo[3];
				designInfo.DesignSections[0] = new DesignSectionInfo();
				designInfo.DesignSections[0].FilePath = "factions\\" + game.LocalPlayer.Faction.Name + "\\sections\\cr_eng_fusion.section";
				designInfo.DesignSections[1] = new DesignSectionInfo();
				designInfo.DesignSections[1].FilePath = "factions\\" + game.LocalPlayer.Faction.Name + "\\sections\\cr_mis_brcarrier.section";
				designInfo.DesignSections[2] = new DesignSectionInfo();
				designInfo.DesignSections[2].FilePath = "factions\\" + game.LocalPlayer.Faction.Name + "\\sections\\cr_cmd_assault.section";
				designID = game.GameDatabase.InsertDesignByDesignInfo(designInfo);
				for (int j = 0; j < 8; j++)
				{
					int parentID = game.GameDatabase.InsertShip(fleetID, designID, null, (ShipParams)0, null, 0);
					for (int k = 0; k < 3; k++)
					{
						int shipID = game.GameDatabase.InsertShip(fleetID, designID2, null, (ShipParams)0, null, 0);
						game.GameDatabase.SetShipParent(shipID, parentID);
						game.GameDatabase.UpdateShipRiderIndex(shipID, k);
					}
				}
				designInfo = new DesignInfo();
				designInfo.PlayerID = game.LocalPlayer.ID;
				designInfo.Name = "Fun Master Flex";
				designInfo.Role = ShipRole.CARRIER;
				designInfo.Class = ShipClass.Leviathan;
				designInfo.DesignSections = new DesignSectionInfo[1];
				designInfo.DesignSections[0] = new DesignSectionInfo();
				designInfo.DesignSections[0].FilePath = "factions\\" + game.LocalPlayer.Faction.Name + "\\sections\\lv_carrier.section";
				DesignLab.SummarizeDesign(game.AssetDatabase, game.GameDatabase, designInfo);
				designID = game.GameDatabase.InsertDesignByDesignInfo(designInfo);
				game.GameDatabase.InsertShip(fleetID, designID, null, (ShipParams)0, null, 0);
			};
			arg_B42_0[arg_B42_1] = commandInfo30;
			ConsoleCommandParse.CommandInfo[] arg_BA2_0 = array;
			int arg_BA2_1 = 30;
			ConsoleCommandParse.CommandInfo commandInfo31 = new ConsoleCommandParse.CommandInfo();
			commandInfo31.Aliases = new string[]
			{
				"allencountered"
			};
			commandInfo31.Description = "Sets all players in the game to be already encountered.";
			commandInfo31.ParameterCount = 0;
			commandInfo31.Action = delegate(App Game, IEnumerable<string> parms)
			{
				List<int> list = Game.GameDatabase.GetStandardPlayerIDs().ToList<int>();
				foreach (int current in list)
				{
					foreach (int current2 in list)
					{
						if (current != current2)
						{
							DiplomacyInfo diplomacyInfo = Game.GameDatabase.GetDiplomacyInfo(current, current2);
							diplomacyInfo.isEncountered = true;
							Game.GameDatabase.UpdateDiplomacyInfo(diplomacyInfo);
						}
					}
				}
			};
			arg_BA2_0[arg_BA2_1] = commandInfo31;
			ConsoleCommandParse.CommandInfo[] arg_C02_0 = array;
			int arg_C02_1 = 31;
			ConsoleCommandParse.CommandInfo commandInfo32 = new ConsoleCommandParse.CommandInfo();
			commandInfo32.Aliases = new string[]
			{
				"adddiplomacypoints"
			};
			commandInfo32.Description = "Adds 100 points to diplomacy, generic diplomacy, intel, counter intel, and operations for the local player.";
			commandInfo32.ParameterCount = 0;
			commandInfo32.Action = delegate(App Game, IEnumerable<string> parms)
			{
				PlayerInfo playerInfo = Game.GameDatabase.GetPlayerInfo(Game.LocalPlayer.ID);
				foreach (int current in playerInfo.FactionDiplomacyPoints.Keys.ToList<int>())
				{
					Dictionary<int, int> factionDiplomacyPoints;
					int key;
					(factionDiplomacyPoints = playerInfo.FactionDiplomacyPoints)[key = current] = factionDiplomacyPoints[key] + 100;
					Game.GameDatabase.UpdateFactionDiplomacyPoints(playerInfo.ID, current, playerInfo.FactionDiplomacyPoints[current]);
				}
				playerInfo.GenericDiplomacyPoints += 100;
				playerInfo.IntelPoints += 100;
				playerInfo.CounterIntelPoints += 100;
				playerInfo.OperationsPoints += 100;
				Game.GameDatabase.UpdatePlayerIntelPoints(playerInfo.ID, playerInfo.IntelPoints);
				Game.GameDatabase.UpdatePlayerCounterintelPoints(playerInfo.ID, playerInfo.CounterIntelPoints);
				Game.GameDatabase.UpdatePlayerOperationsPoints(playerInfo.ID, playerInfo.OperationsPoints);
				Game.GameDatabase.UpdateGenericDiplomacyPoints(playerInfo.ID, playerInfo.GenericDiplomacyPoints);
			};
			arg_C02_0[arg_C02_1] = commandInfo32;
			ConsoleCommandParse.CommandInfo[] arg_C62_0 = array;
			int arg_C62_1 = 32;
			ConsoleCommandParse.CommandInfo commandInfo33 = new ConsoleCommandParse.CommandInfo();
			commandInfo33.Aliases = new string[]
			{
				"forcevn"
			};
			commandInfo33.Description = "Forces a von neumann collector attack on your homeworld";
			commandInfo33.ParameterCount = 0;
			commandInfo33.Action = delegate(App Game, IEnumerable<string> parms)
			{
				if (Game.Game.ScriptModules.VonNeumann != null)
				{
					Game.Game.ScriptModules.VonNeumann.ForceVonNeumannAttack = true;
				}
			};
			arg_C62_0[arg_C62_1] = commandInfo33;
			ConsoleCommandParse.CommandInfo[] arg_CC2_0 = array;
			int arg_CC2_1 = 33;
			ConsoleCommandParse.CommandInfo commandInfo34 = new ConsoleCommandParse.CommandInfo();
			commandInfo34.Aliases = new string[]
			{
				"forcevncycle"
			};
			commandInfo34.Description = "Gives vonneumann a lot of resources and targets your homeworld";
			commandInfo34.ParameterCount = 0;
			commandInfo34.Action = delegate(App Game, IEnumerable<string> parms)
			{
				if (Game.Game.ScriptModules.VonNeumann != null)
				{
					HomeworldInfo playerHomeworld = Game.GameDatabase.GetPlayerHomeworld(Game.LocalPlayer.ID);
					List<VonNeumannInfo> list = Game.GameDatabase.GetVonNeumannInfos().ToList<VonNeumannInfo>();
					if (list.Count > 0)
					{
						VonNeumannInfo vonNeumannInfo = list.First<VonNeumannInfo>();
						vonNeumannInfo.Resources += 200000;
						vonNeumannInfo.SystemId = playerHomeworld.SystemID;
						Game.GameDatabase.UpdateVonNeumannInfo(list.First<VonNeumannInfo>());
					}
					else
					{
						ConsoleCommandParse.Trace("Failed to create VN with top resources, try again next turn");
					}
					Game.Game.ScriptModules.VonNeumann.ForceVonNeumannAttackCycle = true;
				}
			};
			arg_CC2_0[arg_CC2_1] = commandInfo34;
			ConsoleCommandParse.CommandInfo[] arg_D22_0 = array;
			int arg_D22_1 = 34;
			ConsoleCommandParse.CommandInfo commandInfo35 = new ConsoleCommandParse.CommandInfo();
			commandInfo35.Aliases = new string[]
			{
				"forcemonitor"
			};
			commandInfo35.Description = "Spawns a monitor encounter at the closest available system";
			commandInfo35.ParameterCount = 0;
			commandInfo35.Action = delegate(App Game, IEnumerable<string> parms)
			{
				StarSystemInfo home = Game.GameDatabase.GetStarSystemInfo(Game.GameDatabase.GetOrbitalObjectInfo(Game.GameDatabase.GetPlayerInfo(Game.LocalPlayer.ID).Homeworld.Value).StarSystemID);
				List<StarSystemInfo> list = (
					from x in Game.GameDatabase.GetStarSystemInfos().ToList<StarSystemInfo>()
					orderby (x.Origin - home.Origin).Length
					select x).ToList<StarSystemInfo>();
				foreach (StarSystemInfo current in list)
				{
					List<ColonyInfo> list2 = Game.GameDatabase.GetColonyInfosForSystem(current.ID).ToList<ColonyInfo>();
					if (list2.Count == 0)
					{
						List<int> list3 = Game.GameDatabase.GetStarSystemOrbitalObjectIDs(current.ID).ToList<int>();
						if (list3.Count > 0 && Game.Game.ScriptModules.AsteroidMonitor != null)
						{
							Game.Game.ScriptModules.AsteroidMonitor.AddInstance(Game.GameDatabase, Game.AssetDatabase, current.ID, list3.First<int>());
						}
					}
				}
			};
			arg_D22_0[arg_D22_1] = commandInfo35;
			ConsoleCommandParse.CommandInfo[] arg_D82_0 = array;
			int arg_D82_1 = 35;
			ConsoleCommandParse.CommandInfo commandInfo36 = new ConsoleCommandParse.CommandInfo();
			commandInfo36.Aliases = new string[]
			{
				"surveyall"
			};
			commandInfo36.Description = "Know everything about every system";
			commandInfo36.ParameterCount = 0;
			commandInfo36.Action = delegate(App Game, IEnumerable<string> parms)
			{
				List<StarSystemInfo> list = Game.GameDatabase.GetStarSystemInfos().ToList<StarSystemInfo>();
				foreach (StarSystemInfo current in list)
				{
					Game.GameDatabase.InsertExploreRecord(current.ID, Game.LocalPlayer.ID, 1, true, true);
				}
			};
			arg_D82_0[arg_D82_1] = commandInfo36;
			ConsoleCommandParse.CommandInfo[] arg_DE2_0 = array;
			int arg_DE2_1 = 36;
			ConsoleCommandParse.CommandInfo commandInfo37 = new ConsoleCommandParse.CommandInfo();
			commandInfo37.Aliases = new string[]
			{
				"forceprotean"
			};
			commandInfo37.Description = "Spawns a protean at selected system";
			commandInfo37.ParameterCount = 0;
			commandInfo37.Action = delegate(App Game, IEnumerable<string> parms)
			{
				if (Game.CurrentState.GetType() == typeof(StarMapState))
				{
					StarMapState starMapState = (StarMapState)Game.CurrentState;
					int selectedSystem = starMapState.GetSelectedSystem();
					if (selectedSystem != 0)
					{
						Game.Game.ScriptModules.Gardeners.AddInstance(Game.GameDatabase, Game.AssetDatabase, selectedSystem);
					}
				}
			};
			arg_DE2_0[arg_DE2_1] = commandInfo37;
			ConsoleCommandParse.CommandInfo[] arg_E42_0 = array;
			int arg_E42_1 = 37;
			ConsoleCommandParse.CommandInfo commandInfo38 = new ConsoleCommandParse.CommandInfo();
			commandInfo38.Aliases = new string[]
			{
				"forcerelic"
			};
			commandInfo38.Description = "Spawns a relic encounter at the closest available system";
			commandInfo38.ParameterCount = 0;
			commandInfo38.Action = delegate(App Game, IEnumerable<string> parms)
			{
				StarSystemInfo home = Game.GameDatabase.GetStarSystemInfo(Game.GameDatabase.GetOrbitalObjectInfo(Game.GameDatabase.GetPlayerInfo(Game.LocalPlayer.ID).Homeworld.Value).StarSystemID);
				List<StarSystemInfo> list = (
					from x in Game.GameDatabase.GetStarSystemInfos().ToList<StarSystemInfo>()
					orderby (x.Origin - home.Origin).Length
					select x).ToList<StarSystemInfo>();
				foreach (StarSystemInfo current in list)
				{
					List<ColonyInfo> list2 = Game.GameDatabase.GetColonyInfosForSystem(current.ID).ToList<ColonyInfo>();
					if (list2.Count == 0)
					{
						List<int> list3 = Game.GameDatabase.GetStarSystemOrbitalObjectIDs(current.ID).ToList<int>();
						if (list3.Count > 0 && Game.Game.ScriptModules.MorrigiRelic != null)
						{
							Game.Game.ScriptModules.MorrigiRelic.AddInstance(Game.GameDatabase, Game.AssetDatabase, current.ID, list3.First<int>());
						}
					}
				}
			};
			arg_E42_0[arg_E42_1] = commandInfo38;
			ConsoleCommandParse.CommandInfo[] arg_EA2_0 = array;
			int arg_EA2_1 = 38;
			ConsoleCommandParse.CommandInfo commandInfo39 = new ConsoleCommandParse.CommandInfo();
			commandInfo39.Aliases = new string[]
			{
				"forcemeteor"
			};
			commandInfo39.Description = "Spawns a meteor encounter at the closest local players' available system";
			commandInfo39.ParameterCount = 1;
			commandInfo39.Action = delegate(App Game, IEnumerable<string> parms)
			{
				bool forceEncounter = bool.Parse(parms.First<string>());
				MeteorShower.ForceEncounter = forceEncounter;
				ConsoleCommandParse.Trace("MeteorShower.ForceEncounter: " + forceEncounter.ToString());
			};
			arg_EA2_0[arg_EA2_1] = commandInfo39;
			ConsoleCommandParse.CommandInfo[] arg_F02_0 = array;
			int arg_F02_1 = 39;
			ConsoleCommandParse.CommandInfo commandInfo40 = new ConsoleCommandParse.CommandInfo();
			commandInfo40.Aliases = new string[]
			{
				"forcecomet"
			};
			commandInfo40.Description = "Spawns a comet encounter at the closest local players' available system";
			commandInfo40.ParameterCount = 0;
			commandInfo40.Action = delegate(App Game, IEnumerable<string> parms)
			{
				if (Game.Game.ScriptModules.Comet != null)
				{
					Game.Game.ScriptModules.Comet.AddInstance(Game.GameDatabase, Game.AssetDatabase, new int?(Game.GameDatabase.GetOrbitalObjectInfo(Game.GameDatabase.GetPlayerInfo(Game.LocalPlayer.ID).Homeworld.Value).StarSystemID));
				}
				ConsoleCommandParse.Trace("Comet.ForceEncounter");
			};
			arg_F02_0[arg_F02_1] = commandInfo40;
			ConsoleCommandParse.CommandInfo[] arg_F62_0 = array;
			int arg_F62_1 = 40;
			ConsoleCommandParse.CommandInfo commandInfo41 = new ConsoleCommandParse.CommandInfo();
			commandInfo41.Aliases = new string[]
			{
				"forceghostship"
			};
			commandInfo41.Description = "Spawns a ghost ship at the closest local players' available system";
			commandInfo41.ParameterCount = 1;
			commandInfo41.Action = delegate(App Game, IEnumerable<string> parms)
			{
				bool forceEncounter = bool.Parse(parms.First<string>());
				GhostShip.ForceEncounter = forceEncounter;
				ConsoleCommandParse.Trace("GhostShip.ForceEncounter: " + forceEncounter.ToString());
			};
			arg_F62_0[arg_F62_1] = commandInfo41;
			ConsoleCommandParse.CommandInfo[] arg_FC2_0 = array;
			int arg_FC2_1 = 41;
			ConsoleCommandParse.CommandInfo commandInfo42 = new ConsoleCommandParse.CommandInfo();
			commandInfo42.Aliases = new string[]
			{
				"forcespectre"
			};
			commandInfo42.Description = "Spawns a spectre haunt at the targeted system.";
			commandInfo42.ParameterCount = 0;
			commandInfo42.Action = delegate(App Game, IEnumerable<string> parms)
			{
				int selectedSystem = Game.GetGameState<StarMapState>().GetSelectedSystem();
				if (selectedSystem != 0)
				{
					Game.Game.ScriptModules.Spectre.ExecuteEncounter(Game.Game, Game.LocalPlayer.PlayerInfo, selectedSystem, true);
				}
			};
			arg_FC2_0[arg_FC2_1] = commandInfo42;
			ConsoleCommandParse.CommandInfo[] arg_1022_0 = array;
			int arg_1022_1 = 42;
			ConsoleCommandParse.CommandInfo commandInfo43 = new ConsoleCommandParse.CommandInfo();
			commandInfo43.Aliases = new string[]
			{
				"forceslaver"
			};
			commandInfo43.Description = "Spawns a slaver band at the closest local players' available system";
			commandInfo43.ParameterCount = 1;
			commandInfo43.Action = delegate(App Game, IEnumerable<string> parms)
			{
				bool forceEncounter = bool.Parse(parms.First<string>());
				Slaver.ForceEncounter = forceEncounter;
				ConsoleCommandParse.Trace("Slaver.ForceEncounter: " + forceEncounter.ToString());
			};
			arg_1022_0[arg_1022_1] = commandInfo43;
			ConsoleCommandParse.CommandInfo[] arg_1082_0 = array;
			int arg_1082_1 = 43;
			ConsoleCommandParse.CommandInfo commandInfo44 = new ConsoleCommandParse.CommandInfo();
			commandInfo44.Aliases = new string[]
			{
				"forcepirates"
			};
			commandInfo44.Description = "Spawns pirates at every trading system";
			commandInfo44.ParameterCount = 1;
			commandInfo44.Action = delegate(App Game, IEnumerable<string> parms)
			{
				bool forceEncounter = bool.Parse(parms.First<string>());
				Pirates.ForceEncounter = forceEncounter;
				ConsoleCommandParse.Trace("Pirates.ForceEncounter: " + forceEncounter.ToString());
			};
			arg_1082_0[arg_1082_1] = commandInfo44;
			ConsoleCommandParse.CommandInfo[] arg_10E2_0 = array;
			int arg_10E2_1 = 44;
			ConsoleCommandParse.CommandInfo commandInfo45 = new ConsoleCommandParse.CommandInfo();
			commandInfo45.Aliases = new string[]
			{
				"forcelocust"
			};
			commandInfo45.Description = "Spawns a locust GM";
			commandInfo45.ParameterCount = 0;
			commandInfo45.Action = delegate(App Game, IEnumerable<string> parms)
			{
				if (Game.Game.ScriptModules.Locust != null)
				{
					int starSystemID = Game.GameDatabase.GetOrbitalObjectInfo(Game.GameDatabase.GetPlayerInfo(Game.LocalPlayer.ID).Homeworld.Value).StarSystemID;
					List<StarSystemInfo> list = (
						from x in EncounterTools.GetClosestStars(Game.GameDatabase, starSystemID)
						where Game.GameDatabase.GetColonyInfosForSystem(x.ID).Count<ColonyInfo>() == 0
						select x).ToList<StarSystemInfo>();
					if (list.Count > 0)
					{
						Game.Game.ScriptModules.Locust.AddInstance(Game.GameDatabase, Game.AssetDatabase, new int?(list.First<StarSystemInfo>().ID));
					}
				}
			};
			arg_10E2_0[arg_10E2_1] = commandInfo45;
			ConsoleCommandParse.CommandInfo[] arg_1142_0 = array;
			int arg_1142_1 = 45;
			ConsoleCommandParse.CommandInfo commandInfo46 = new ConsoleCommandParse.CommandInfo();
			commandInfo46.Aliases = new string[]
			{
				"forceneutronstar"
			};
			commandInfo46.Description = "Spawns a neutron star GM";
			commandInfo46.ParameterCount = 0;
			commandInfo46.Action = delegate(App Game, IEnumerable<string> parms)
			{
				if (Game.Game.ScriptModules.NeutronStar != null)
				{
					int num = 0;
					if (Game.Game.StarMapSelectedObject is StarSystemInfo)
					{
						num = (Game.Game.StarMapSelectedObject as StarSystemInfo).ID;
					}
					if (num == 0)
					{
						num = Game.GameDatabase.GetOrbitalObjectInfo(Game.GameDatabase.GetPlayerInfo(Game.LocalPlayer.ID).Homeworld.Value).StarSystemID;
					}
					Game.Game.ScriptModules.NeutronStar.AddInstance(Game.GameDatabase, Game.AssetDatabase, new int?(num));
				}
			};
			arg_1142_0[arg_1142_1] = commandInfo46;
			ConsoleCommandParse.CommandInfo[] arg_11A2_0 = array;
			int arg_11A2_1 = 46;
			ConsoleCommandParse.CommandInfo commandInfo47 = new ConsoleCommandParse.CommandInfo();
			commandInfo47.Aliases = new string[]
			{
				"forcegardener"
			};
			commandInfo47.Description = "Spawns a gardener GM";
			commandInfo47.ParameterCount = 0;
			commandInfo47.Action = delegate(App Game, IEnumerable<string> parms)
			{
				if (Game.Game.ScriptModules.Gardeners != null)
				{
					Game.Game.ScriptModules.Gardeners.AddInstance(Game.GameDatabase, Game.AssetDatabase, 0);
				}
			};
			arg_11A2_0[arg_11A2_1] = commandInfo47;
			ConsoleCommandParse.CommandInfo[] arg_1202_0 = array;
			int arg_1202_1 = 47;
			ConsoleCommandParse.CommandInfo commandInfo48 = new ConsoleCommandParse.CommandInfo();
			commandInfo48.Aliases = new string[]
			{
				"forcesupernova"
			};
			commandInfo48.Description = "Spawns a supernova GM";
			commandInfo48.ParameterCount = 0;
			commandInfo48.Action = delegate(App Game, IEnumerable<string> parms)
			{
				if (Game.Game.ScriptModules.SuperNova != null)
				{
					int starSystemID = Game.GameDatabase.GetOrbitalObjectInfo(Game.GameDatabase.GetPlayerInfo(Game.LocalPlayer.ID).Homeworld.Value).StarSystemID;
					List<StarSystemInfo> list = (
						from x in EncounterTools.GetClosestStars(Game.GameDatabase, starSystemID)
						where Game.GameDatabase.GetColonyInfosForSystem(x.ID).Count<ColonyInfo>() > 0
						select x).ToList<StarSystemInfo>();
					List<SuperNovaInfo> superNovas = Game.GameDatabase.GetSuperNovaInfos().ToList<SuperNovaInfo>();
					list.RemoveAll((StarSystemInfo x) => superNovas.Any((SuperNovaInfo y) => y.SystemId == x.ID));
					if (list.Count > 0)
					{
						Game.Game.ScriptModules.SuperNova.AddInstance(Game.GameDatabase, Game.AssetDatabase, list.First<StarSystemInfo>().ID);
					}
				}
			};
			arg_1202_0[arg_1202_1] = commandInfo48;
			ConsoleCommandParse.CommandInfo[] arg_1262_0 = array;
			int arg_1262_1 = 48;
			ConsoleCommandParse.CommandInfo commandInfo49 = new ConsoleCommandParse.CommandInfo();
			commandInfo49.Aliases = new string[]
			{
				"forcesk"
			};
			commandInfo49.Description = "Spawns a system killer GM";
			commandInfo49.ParameterCount = 0;
			commandInfo49.Action = delegate(App Game, IEnumerable<string> parms)
			{
				if (Game.Game.ScriptModules.SystemKiller != null)
				{
					Game.Game.ScriptModules.SystemKiller.AddInstance(Game.GameDatabase, Game.AssetDatabase, new int?(Game.GameDatabase.GetOrbitalObjectInfo(Game.GameDatabase.GetPlayerInfo(Game.LocalPlayer.ID).Homeworld.Value).StarSystemID));
				}
			};
			arg_1262_0[arg_1262_1] = commandInfo49;
			ConsoleCommandParse.CommandInfo[] arg_12C2_0 = array;
			int arg_12C2_1 = 49;
			ConsoleCommandParse.CommandInfo commandInfo50 = new ConsoleCommandParse.CommandInfo();
			commandInfo50.Aliases = new string[]
			{
				"colonize"
			};
			commandInfo50.Description = "Colonizes/develops planet with the given ID for the local player.";
			commandInfo50.ParameterCount = 1;
			commandInfo50.Action = delegate(App Game, IEnumerable<string> parms)
			{
				GameSession.MakeIdealColony(Game.GameDatabase, Game.AssetDatabase, int.Parse(parms.First<string>()), Game.LocalPlayer.ID, IdealColonyTypes.Secondary);
			};
			arg_12C2_0[arg_12C2_1] = commandInfo50;
			ConsoleCommandParse.CommandInfo[] arg_1322_0 = array;
			int arg_1322_1 = 50;
			ConsoleCommandParse.CommandInfo commandInfo51 = new ConsoleCommandParse.CommandInfo();
			commandInfo51.Aliases = new string[]
			{
				"skipcombat"
			};
			commandInfo51.Description = "Displays true/false depending on whether the skip combat hack flag is currently set.";
			commandInfo51.ParameterCount = 0;
			commandInfo51.Action = delegate(App Game, IEnumerable<string> parms)
			{
				ConsoleCommandParse.Trace("Skip combat: " + GameSession.SkipCombatHack.ToString());
			};
			arg_1322_0[arg_1322_1] = commandInfo51;
			ConsoleCommandParse.CommandInfo[] arg_1382_0 = array;
			int arg_1382_1 = 51;
			ConsoleCommandParse.CommandInfo commandInfo52 = new ConsoleCommandParse.CommandInfo();
			commandInfo52.Aliases = new string[]
			{
				"skipcombat"
			};
			commandInfo52.Description = "Sets the state of the skip combat hack flage true/false.";
			commandInfo52.ParameterCount = 1;
			commandInfo52.Action = delegate(App Game, IEnumerable<string> parms)
			{
				bool skipCombatHack = bool.Parse(parms.First<string>());
				GameSession.SkipCombatHack = skipCombatHack;
				ConsoleCommandParse.Trace("Skip combat: " + skipCombatHack.ToString());
			};
			arg_1382_0[arg_1382_1] = commandInfo52;
			ConsoleCommandParse.CommandInfo[] arg_13E2_0 = array;
			int arg_13E2_1 = 52;
			ConsoleCommandParse.CommandInfo commandInfo53 = new ConsoleCommandParse.CommandInfo();
			commandInfo53.Aliases = new string[]
			{
				"forcereaction"
			};
			commandInfo53.Description = "Sets the state of the force reaction (interception) hack flage true/false.";
			commandInfo53.ParameterCount = 1;
			commandInfo53.Action = delegate(App Game, IEnumerable<string> parms)
			{
				bool forceReactionHack = bool.Parse(parms.First<string>());
				GameSession.ForceReactionHack = forceReactionHack;
				ConsoleCommandParse.Trace("Force reaction: " + forceReactionHack.ToString());
			};
			arg_13E2_0[arg_13E2_1] = commandInfo53;
			ConsoleCommandParse.CommandInfo[] arg_1442_0 = array;
			int arg_1442_1 = 53;
			ConsoleCommandParse.CommandInfo commandInfo54 = new ConsoleCommandParse.CommandInfo();
			commandInfo54.Aliases = new string[]
			{
				"testcombat"
			};
			commandInfo54.Description = "From starmap enters debug combat state.";
			commandInfo54.ParameterCount = 0;
			commandInfo54.Action = delegate(App Game, IEnumerable<string> parms)
			{
				if (Game.Game.StarMapSelectedObject is StarSystemInfo)
				{
					StarSystemInfo starSystemInfo = Game.Game.StarMapSelectedObject as StarSystemInfo;
					Game.LaunchCombat(Game.Game, new PendingCombat
					{
						SystemID = starSystemInfo.ID
					}, true, false, true);
					return;
				}
				if (Game.Game.StarMapSelectedObject is FleetInfo)
				{
					FleetInfo fleetInfo = Game.Game.StarMapSelectedObject as FleetInfo;
					FleetLocation fl = Game.GameDatabase.GetFleetLocation(fleetInfo.ID, false);
					List<StarSystemInfo> source = Game.GameDatabase.GetDeepspaceStarSystemInfos().ToList<StarSystemInfo>();
					StarSystemInfo starSystemInfo2 = source.FirstOrDefault((StarSystemInfo x) => (x.Origin - fl.Coords).Length < 0.0001f);
					if (starSystemInfo2 == null)
					{
						starSystemInfo2.ID = Game.GameDatabase.InsertStarSystem(null, App.Localize("@UI_STARMAP_ENCOUNTER_DEEPSPACE"), null, "Deepspace", fl.Coords, false, false, null);
					}
					Game.GameDatabase.UpdateFleetLocation(fl.FleetID, starSystemInfo2.ID, null);
					Game.LaunchCombat(Game.Game, new PendingCombat
					{
						SystemID = starSystemInfo2.ID
					}, true, false, true);
				}
			};
			arg_1442_0[arg_1442_1] = commandInfo54;
			ConsoleCommandParse.CommandInfo[] arg_14A2_0 = array;
			int arg_14A2_1 = 54;
			ConsoleCommandParse.CommandInfo commandInfo55 = new ConsoleCommandParse.CommandInfo();
			commandInfo55.Aliases = new string[]
			{
				"extremestars"
			};
			commandInfo55.Description = "Replaces all stars in the active game with smallest/largest for UI testing.";
			commandInfo55.ParameterCount = 0;
			commandInfo55.Action = delegate(App Game, IEnumerable<string> parms)
			{
				Game.GameDatabase.ReplaceMapWithExtremeStars();
				if (Game.CurrentState == Game.GetGameState<StarMapState>())
				{
					Game.GetGameState<StarMapState>().RefreshStarmap(StarMapState.StarMapRefreshType.REFRESH_ALL);
				}
				ConsoleCommandParse.Trace("Stars have gone EXTREME!");
			};
			arg_14A2_0[arg_14A2_1] = commandInfo55;
			ConsoleCommandParse.CommandInfo[] arg_1502_0 = array;
			int arg_1502_1 = 55;
			ConsoleCommandParse.CommandInfo commandInfo56 = new ConsoleCommandParse.CommandInfo();
			commandInfo56.Aliases = new string[]
			{
				"nav"
			};
			commandInfo56.Description = "Navigates to the specified page in the encyclopedia.";
			commandInfo56.ParameterCount = 1;
			commandInfo56.Action = delegate(App Game, IEnumerable<string> parms)
			{
				SotspediaState.NavigateToLink(Game, parms.First<string>());
			};
			arg_1502_0[arg_1502_1] = commandInfo56;
			ConsoleCommandParse.CommandInfo[] arg_1562_0 = array;
			int arg_1562_1 = 56;
			ConsoleCommandParse.CommandInfo commandInfo57 = new ConsoleCommandParse.CommandInfo();
			commandInfo57.Aliases = new string[]
			{
				"techstyles"
			};
			commandInfo57.Description = "Tests generation of tech styles for a particular named faction. First param is the faction name, and second param is the number of iterations";
			commandInfo57.ParameterCount = 2;
			commandInfo57.Action = delegate(App Game, IEnumerable<string> parms)
			{
				string[] array2 = parms.ToArray<string>();
				if (Game.AssetDatabase != null)
				{
					Game.AssetDatabase.AIResearchFramework.TestTechStyleSelection(array2[0], int.Parse(array2[1]));
				}
			};
			arg_1562_0[arg_1562_1] = commandInfo57;
			ConsoleCommandParse.CommandInfo[] arg_15C2_0 = array;
			int arg_15C2_1 = 57;
			ConsoleCommandParse.CommandInfo commandInfo58 = new ConsoleCommandParse.CommandInfo();
			commandInfo58.Aliases = new string[]
			{
				"forceintelcritsuccess"
			};
			commandInfo58.Description = "Sets the state of the force intel critical success hack flage true/false.";
			commandInfo58.ParameterCount = 1;
			commandInfo58.Action = delegate(App Game, IEnumerable<string> parms)
			{
				bool forceIntelMissionCriticalSuccessHack = bool.Parse(parms.First<string>());
				GameSession.ForceIntelMissionCriticalSuccessHack = forceIntelMissionCriticalSuccessHack;
				ConsoleCommandParse.Trace("ForceIntelMissionCriticalSuccessHack: " + forceIntelMissionCriticalSuccessHack.ToString());
			};
			arg_15C2_0[arg_15C2_1] = commandInfo58;
			ConsoleCommandParse.CommandInfo[] arg_1622_0 = array;
			int arg_1622_1 = 58;
			ConsoleCommandParse.CommandInfo commandInfo59 = new ConsoleCommandParse.CommandInfo();
			commandInfo59.Aliases = new string[]
			{
				"instabuild"
			};
			commandInfo59.Description = "Accelerated construction true/false.";
			commandInfo59.ParameterCount = 1;
			commandInfo59.Action = delegate(App Game, IEnumerable<string> parms)
			{
				bool instaBuildHackEnabled = bool.Parse(parms.First<string>());
				GameSession.InstaBuildHackEnabled = instaBuildHackEnabled;
				ConsoleCommandParse.Trace("instabuild: " + instaBuildHackEnabled.ToString());
			};
			arg_1622_0[arg_1622_1] = commandInfo59;
			ConsoleCommandParse.CommandInfo[] arg_1682_0 = array;
			int arg_1682_1 = 59;
			ConsoleCommandParse.CommandInfo commandInfo60 = new ConsoleCommandParse.CommandInfo();
			commandInfo60.Aliases = new string[]
			{
				"print_designs"
			};
			commandInfo60.Description = "Prints out the list of ship designs for the given player number.";
			commandInfo60.ParameterCount = 1;
			commandInfo60.Action = delegate(App Game, IEnumerable<string> parms)
			{
				int playerid = int.Parse(parms.First<string>());
				StringBuilder stringBuilder = new StringBuilder();
				DesignLab.PrintPlayerDesignSummary(stringBuilder, Game, playerid, false);
				ConsoleCommandParse.Trace(stringBuilder.ToString());
			};
			arg_1682_0[arg_1682_1] = commandInfo60;
			ConsoleCommandParse.CommandInfo[] arg_16E2_0 = array;
			int arg_16E2_1 = 60;
			ConsoleCommandParse.CommandInfo commandInfo61 = new ConsoleCommandParse.CommandInfo();
			commandInfo61.Aliases = new string[]
			{
				"netplayers"
			};
			commandInfo61.Description = "Prints out a summary of player states belonging to the active network controller.";
			commandInfo61.ParameterCount = 0;
			commandInfo61.Action = delegate(App Game, IEnumerable<string> parms)
			{
				if (Game.Network == null)
				{
					ConsoleCommandParse.Warn("No network object available.");
					return;
				}
				Game.Network.PostLogPlayerInfo();
			};
			arg_16E2_0[arg_16E2_1] = commandInfo61;
			ConsoleCommandParse.CommandInfo[] arg_1742_0 = array;
			int arg_1742_1 = 61;
			ConsoleCommandParse.CommandInfo commandInfo62 = new ConsoleCommandParse.CommandInfo();
			commandInfo62.Aliases = new string[]
			{
				"buildcounterdesign"
			};
			commandInfo62.Description = "Prints out a summary of player states belonging to the active network controller.";
			commandInfo62.ParameterCount = 0;
			commandInfo62.Action = delegate(App Game, IEnumerable<string> parms)
			{
				int system = Game.GetGameState<StarMapState>().GetSelectedSystem();
				if (system != 0)
				{
					IEnumerable<FleetInfo> enumerable = 
						from x in Game.GameDatabase.GetFleetInfos(FleetType.FL_NORMAL)
						where x.SystemID == system
						select x;
					if (enumerable.Count<FleetInfo>() > 0)
					{
						int num = 0;
						StrategicAI.DesignConfigurationInfo designConfigurationInfo = default(StrategicAI.DesignConfigurationInfo);
						foreach (FleetInfo current in enumerable)
						{
							IEnumerable<ShipInfo> shipInfoByFleetID = Game.GameDatabase.GetShipInfoByFleetID(current.ID, true);
							foreach (ShipInfo current2 in shipInfoByFleetID)
							{
								StrategicAI.DesignConfigurationInfo designConfigurationInfo2 = StrategicAI.GetDesignConfigurationInfo(Game.Game, current2.DesignInfo);
								designConfigurationInfo += designConfigurationInfo2;
								num++;
							}
						}
						designConfigurationInfo.Average(num);
						DesignInfo design = DesignLab.CreateCounterDesign(Game.Game, ShipClass.Cruiser, Game.LocalPlayer.ID, designConfigurationInfo);
						Game.GameDatabase.InsertDesignByDesignInfo(design);
					}
				}
			};
			arg_1742_0[arg_1742_1] = commandInfo62;
			ConsoleCommandParse.CommandInfo[] arg_17A2_0 = array;
			int arg_17A2_1 = 62;
			ConsoleCommandParse.CommandInfo commandInfo63 = new ConsoleCommandParse.CommandInfo();
			commandInfo63.Aliases = new string[]
			{
				"addsuulka"
			};
			commandInfo63.Description = "Adds suulka to player.";
			commandInfo63.ParameterCount = 2;
			commandInfo63.Action = delegate(App Game, IEnumerable<string> parms)
			{
				int index = int.Parse(parms.ElementAt(0));
				int playerID = int.Parse(parms.ElementAt(1));
				List<SuulkaInfo> list = Game.GameDatabase.GetSuulkas().ToList<SuulkaInfo>();
				SuulkaInfo suulkaInfo = list[index];
				List<StationInfo> list2 = Game.GameDatabase.GetStationInfosByPlayerID(playerID).ToList<StationInfo>();
				StationInfo stationInfo = (list2.Count == 1) ? list2[0] : list2[1];
				Game.GameDatabase.UpdateSuulkaStation(suulkaInfo.ID, stationInfo.OrbitalObjectID);
				Game.GameDatabase.UpdateSuulkaArrivalTurns(suulkaInfo.ID, -1);
				Game.Game.InsertSuulkaFleet(playerID, suulkaInfo.ID);
			};
			arg_17A2_0[arg_17A2_1] = commandInfo63;
			ConsoleCommandParse.CommandInfo[] arg_1802_0 = array;
			int arg_1802_1 = 63;
			ConsoleCommandParse.CommandInfo commandInfo64 = new ConsoleCommandParse.CommandInfo();
			commandInfo64.Aliases = new string[]
			{
				"suulkaids"
			};
			commandInfo64.Description = "displays suulka indexes and names.";
			commandInfo64.ParameterCount = 0;
			commandInfo64.Action = delegate(App Game, IEnumerable<string> parms)
			{
				int num = 0;
				List<SuulkaInfo> list = Game.GameDatabase.GetSuulkas().ToList<SuulkaInfo>();
				foreach (SuulkaInfo current in list)
				{
					ShipInfo shipInfo = Game.GameDatabase.GetShipInfo(current.ShipID, false);
					ConsoleCommandParse.Trace(string.Concat(new object[]
					{
						"[",
						num,
						"] Suulka '",
						shipInfo.ShipName
					}));
					num++;
				}
			};
			arg_1802_0[arg_1802_1] = commandInfo64;
			ConsoleCommandParse.CommandInfo[] arg_1862_0 = array;
			int arg_1862_1 = 64;
			ConsoleCommandParse.CommandInfo commandInfo65 = new ConsoleCommandParse.CommandInfo();
			commandInfo65.Aliases = new string[]
			{
				"movefleet"
			};
			commandInfo65.Description = "Moves a player fleet to target system.";
			commandInfo65.ParameterCount = 0;
			commandInfo65.Action = delegate(App Game, IEnumerable<string> parms)
			{
				int selectedSystem = Game.GetGameState<StarMapState>().GetSelectedSystem();
				if (selectedSystem != 0)
				{
					IEnumerable<FleetInfo> source = 
						from x in Game.GameDatabase.GetFleetInfos(FleetType.FL_NORMAL)
						where x.PlayerID == Game.LocalPlayer.ID
						select x;
					FleetInfo fleetInfo = source.FirstOrDefault<FleetInfo>();
					if (fleetInfo != null)
					{
						Game.GameDatabase.UpdateFleetLocation(fleetInfo.ID, selectedSystem, null);
					}
				}
			};
			arg_1862_0[arg_1862_1] = commandInfo65;
			ConsoleCommandParse.CommandInfo[] arg_18C2_0 = array;
			int arg_18C2_1 = 65;
			ConsoleCommandParse.CommandInfo commandInfo66 = new ConsoleCommandParse.CommandInfo();
			commandInfo66.Aliases = new string[]
			{
				"comeatmebro"
			};
			commandInfo66.Description = "Declares war on everyone.";
			commandInfo66.ParameterCount = 0;
			commandInfo66.Action = delegate(App Game, IEnumerable<string> parms)
			{
				int iD = Game.LocalPlayer.ID;
				IEnumerable<PlayerInfo> playerInfos = Game.GameDatabase.GetPlayerInfos();
				foreach (PlayerInfo current in playerInfos)
				{
					if (current.ID != iD)
					{
						Game.GameDatabase.UpdateDiplomacyState(iD, current.ID, DiplomacyState.WAR, 0, true);
					}
				}
			};
			arg_18C2_0[arg_18C2_1] = commandInfo66;
			ConsoleCommandParse.CommandInfo[] arg_1922_0 = array;
			int arg_1922_1 = 66;
			ConsoleCommandParse.CommandInfo commandInfo67 = new ConsoleCommandParse.CommandInfo();
			commandInfo67.Aliases = new string[]
			{
				"addsavings"
			};
			commandInfo67.Description = "addsavings 'value' adds specified value to savings.";
			commandInfo67.ParameterCount = 1;
			commandInfo67.Action = delegate(App Game, IEnumerable<string> parms)
			{
				int num = int.Parse(parms.First<string>());
				Game.GameDatabase.UpdatePlayerSavings(Game.LocalPlayer.ID, Game.LocalPlayer.PlayerInfo.Savings += (double)num);
			};
			arg_1922_0[arg_1922_1] = commandInfo67;
			ConsoleCommandParse.CommandInfo[] arg_1982_0 = array;
			int arg_1982_1 = 67;
			ConsoleCommandParse.CommandInfo commandInfo68 = new ConsoleCommandParse.CommandInfo();
			commandInfo68.Aliases = new string[]
			{
				"simturns"
			};
			commandInfo68.Description = "sims a number of set turns";
			commandInfo68.ParameterCount = 1;
			commandInfo68.Action = delegate(App Game, IEnumerable<string> parms)
			{
				List<Player> list = (
					from x in Game.GameDatabase.GetStandardPlayerIDs()
					select Game.GetPlayer(x)).ToList<Player>();
				foreach (Player current in list)
				{
					current.SetAI(true);
				}
				int val = int.Parse(parms.First<string>());
				GameSession.SimAITurns = Math.Max(val, 0);
			};
			arg_1982_0[arg_1982_1] = commandInfo68;
			ConsoleCommandParse.CommandInfo[] arg_19E2_0 = array;
			int arg_19E2_1 = 68;
			ConsoleCommandParse.CommandInfo commandInfo69 = new ConsoleCommandParse.CommandInfo();
			commandInfo69.Aliases = new string[]
			{
				"stopsim"
			};
			commandInfo69.Description = "stops simturns";
			commandInfo69.ParameterCount = 0;
			commandInfo69.Action = delegate(App Game, IEnumerable<string> parms)
			{
				GameSession.SimAITurns = 0;
			};
			arg_19E2_0[arg_19E2_1] = commandInfo69;
			ConsoleCommandParse._commandInfos = array;
		}
		public static void Evaluate(App game, IEnumerable<string> cmds)
		{
			foreach (string current in 
				from x in cmds
				select x.Trim() into y
				where !y.StartsWith("//")
				select y)
			{
				ConsoleCommandParse.Trace("Console: " + current);
				string[] parts = current.Split(new char[]
				{
					' '
				});
				if (!string.IsNullOrWhiteSpace(parts[0]))
				{
					int paramCount = parts.Length - 1;
					ConsoleCommandParse.CommandInfo commandInfo = ConsoleCommandParse._commandInfos.FirstOrDefault((ConsoleCommandParse.CommandInfo x) => x.ParameterCount == paramCount && x.Aliases.Any((string y) => y.Equals(parts[0], StringComparison.InvariantCulture)));
					if (commandInfo == null)
					{
						ConsoleCommandParse.Warn(" Syntax error.");
					}
					else
					{
						commandInfo.Action(game, parts.Skip(1));
					}
				}
			}
		}
		public static void ProcessConsoleCommands(App game, ConsoleApplet applet)
		{
			if (applet != null)
			{
				ConsoleCommandParse.Evaluate(game, applet.FlushCommands());
			}
		}
	}
}
