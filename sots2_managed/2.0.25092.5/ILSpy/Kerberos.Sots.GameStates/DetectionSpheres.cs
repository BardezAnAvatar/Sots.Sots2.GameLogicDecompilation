using Kerberos.Sots.Framework;
using System;
namespace Kerberos.Sots.GameStates
{
	public class DetectionSpheres
	{
		public int playerID;
		public Vector3 center;
		public float minRadius;
		public float sensorRange;
		public float slewModeRange;
		public bool isPlanet;
		public bool ignoreNeutralPlanets;
		public DetectionSpheres(int p, Vector3 c)
		{
			this.playerID = p;
			this.center = c;
			this.minRadius = 0f;
			this.sensorRange = 0f;
			this.slewModeRange = 0f;
			this.isPlanet = false;
			this.ignoreNeutralPlanets = false;
		}
	}
}
