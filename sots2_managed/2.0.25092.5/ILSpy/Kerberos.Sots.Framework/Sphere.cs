using System;
namespace Kerberos.Sots.Framework
{
	internal struct Sphere
	{
		public int player_id;
		public Vector3 center;
		public float radius;
		public Sphere(int p, Vector3 c, float r)
		{
			this.player_id = p;
			this.center = c;
			this.radius = r;
		}
	}
}
