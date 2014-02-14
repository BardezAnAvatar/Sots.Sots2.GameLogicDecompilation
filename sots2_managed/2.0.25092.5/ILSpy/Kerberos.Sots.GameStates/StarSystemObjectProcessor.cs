using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.GameStates
{
	internal class StarSystemObjectProcessor
	{
		public delegate void ProcessStar(StarSystemInfo starInfo);
		public delegate void ProcessPlanet(PlanetInfo planetInfo, Matrix world);
		public delegate void ProcessFleet(FleetInfo fleetInfo, Matrix world);
		public delegate void ProcessShip(FleetInfo fleetInfo, ShipInfo shipInfo, Matrix world);
		public event StarSystemObjectProcessor.ProcessStar OnStar;
		public event StarSystemObjectProcessor.ProcessPlanet OnPlanet;
		public event StarSystemObjectProcessor.ProcessFleet OnFleet;
		public event StarSystemObjectProcessor.ProcessShip OnShip;
		public void Process(App game, int systemId)
		{
			if (systemId == 0)
			{
				return;
			}
			GameDatabase gameDatabase = game.GameDatabase;
			StarSystemInfo starSystemInfo = gameDatabase.GetStarSystemInfo(systemId);
			gameDatabase.GetStarSystemOrbitalObjectInfos(systemId);
			if (this.OnStar != null)
			{
				this.OnStar(starSystemInfo);
			}
			if (this.OnPlanet != null)
			{
				PlanetInfo[] starSystemPlanetInfos = gameDatabase.GetStarSystemPlanetInfos(systemId);
				PlanetInfo[] array = starSystemPlanetInfos;
				for (int i = 0; i < array.Length; i++)
				{
					PlanetInfo planetInfo = array[i];
					Matrix orbitalTransform = gameDatabase.GetOrbitalTransform(planetInfo.ID);
					this.OnPlanet(planetInfo, orbitalTransform);
				}
			}
			if (this.OnFleet != null || this.OnShip != null)
			{
				IEnumerable<FleetInfo> fleetInfoBySystemID = gameDatabase.GetFleetInfoBySystemID(systemId, FleetType.FL_NORMAL);
				if (this.OnFleet != null)
				{
					foreach (FleetInfo current in fleetInfoBySystemID)
					{
						Matrix world = Matrix.CreateTranslation(this.GetSpawnPointForPlayer(current.PlayerID, systemId));
						this.OnFleet(current, world);
					}
				}
				if (this.OnShip != null)
				{
					Vector3 vector = new Vector3(200f, 0f, 0f);
					foreach (FleetInfo current2 in fleetInfoBySystemID)
					{
						Vector3 vector2 = this.GetSpawnPointForPlayer(current2.PlayerID, systemId);
						vector2 += vector * (float)current2.ID;
						List<ShipInfo> list = gameDatabase.GetShipInfoByFleetID(current2.ID, false).ToList<ShipInfo>();
						foreach (ShipInfo current3 in list)
						{
							Vector3 trans = vector2;
							vector2 += vector;
							Matrix world2 = Matrix.CreateTranslation(trans);
							this.OnShip(current2, current3, world2);
						}
					}
				}
			}
		}
		public Vector3 GetSpawnPointForPlayer(int playerID, int systemID)
		{
			return new Vector3(0f, 0f, 10000f * (float)(playerID - 1));
		}
	}
}
