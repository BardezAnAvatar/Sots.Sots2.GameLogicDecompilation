using Kerberos.Sots.Data;
using Kerberos.Sots.Encounters;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using FixedData = Bardez.Projects.SwordOfTheStars.SotS2.Fixes.Sots.Data;

namespace Bardez.Projects.SwordOfTheStars.SotS2.Fixes.Sots.StarMapElements
{
    /// <summary>This class contains code fixes to general SotS2 logic in namespace Kerberos.Sots.StarMapElements.Starmap</summary>
	internal class StarMap
	{
        /// <summary>Improved performance for IsInRange in Kerberos.Sots.StarMapElements.Starmap</summary>
        /// <param name="db">Game database to query</param>
        /// <param name="playerId">Unique ID of the player whose range is being determined</param>
        /// <param name="loc">Vector location of range to determine</param>
        /// <param name="rangeMultiplier">Miltiplier for range</param>
        /// <param name="cachedFleetShips">Cache of fleet ships</param>
        /// <returns>A Flga indicating whether or not the player is in range of the target</returns>
        public static Boolean IsInRange(GameDatabase db, Int32 playerId, Vector3 loc, Single rangeMultiplier = 1f, Dictionary<Int32, List<ShipInfo>> cachedFleetShips = null)
        {
            Boolean inRange = false;

            if (Kerberos.Sots.StarMapElements.StarMap.AlwaysInRange)
            {
                inRange = true;
            }
            else
            {
                //List all players
                IEnumerable<PlayerInfo> players = db.GetPlayerInfos();
                PlayerInfo currentPlayer = null;
                foreach (PlayerInfo player in players)  //find the current player
                    if (player.ID == playerId)
                    {
                        currentPlayer = player;
                        break;
                    }

                //list all allies of the current player
                List<PlayerInfo> allies = new List<PlayerInfo>();
                foreach (PlayerInfo player in players)
	                if (player.ID != playerId && db.GetDiplomacyStateBetweenPlayers(player.ID, playerId) == DiplomacyState.ALLIED)
	                allies.Add(player);

                //get the list of player and its allies
                List<PlayerInfo> playerAndAllies = new List<PlayerInfo>(allies);
                playerAndAllies.Add(currentPlayer);

                //Get a list of all allied colonies
                List<Int32> alliedColonyIds = new List<Int32>();
                foreach (PlayerInfo player in playerAndAllies)
	                alliedColonyIds.AddRange(db.GetPlayerColonySystemIDs(player.ID));

                //Get a list of all systems in range of the player or allied colonies
                foreach (Int32 colonyId in alliedColonyIds)
                {
                    StarSystemInfo starSystemInfo = db.GetStarSystemInfo(colonyId);
                    if (starSystemInfo != null)
                    {
                        Single systemStratSensorRange = FixedData.GameDatabase.GetSystemStratSensorRange(db, colonyId, playerId, playerAndAllies);
                        Single length = (starSystemInfo.Origin - loc).Length;
                        if (length <= systemStratSensorRange * rangeMultiplier)
                        {
                            inRange = true;
                            break;
                        }
                    }
                }

                //Get a list of all fleets from the allies
                List<FleetInfo> alliedFleets = new List<FleetInfo>();
                foreach (PlayerInfo player in playerAndAllies)
                    alliedFleets.AddRange(db.GetFleetInfosByPlayerID(player.ID, FleetType.FL_NORMAL | FleetType.FL_DEFENSE | FleetType.FL_GATE | FleetType.FL_CARAVAN | FleetType.FL_ACCELERATOR));

                foreach (FleetInfo fleet in alliedFleets)
                {
                    List<ShipInfo> cachedShips = null;
                    if (cachedFleetShips != null && !cachedFleetShips.TryGetValue(fleet.ID, out cachedShips))
                        cachedShips = new List<ShipInfo>();

                    Single sensorRange = GameSession.GetFleetSensorRange(db.AssetDatabase, db, fleet, cachedShips);
                    if (sensorRange == 0f && db.GetShipsByFleetID(fleet.ID).Any())
                        sensorRange = db.AssetDatabase.DefaultStratSensorRange;

                    FleetLocation fleetLocation = db.GetFleetLocation(fleet.ID);
                    Vector3 coords = fleetLocation.Coords;
                    Single length2 = (coords - loc).Length;
                    if (length2 <= sensorRange * rangeMultiplier)
                    {
                        inRange = true;
                        break;
                    }
                }
            }

            return inRange;
        }
	}
}