using Kerberos.Sots.Framework;
using System;
namespace Kerberos.Sots.GameStates
{
	internal class NeighboringSystemInfo
	{
		public string Name = "";
		public int SystemID;
		public Vector3 Location = default(Vector3);
		public Vector3 BaseOffsetLocation = default(Vector3);
		public Vector3 DirFromSystem = default(Vector3);
	}
}
