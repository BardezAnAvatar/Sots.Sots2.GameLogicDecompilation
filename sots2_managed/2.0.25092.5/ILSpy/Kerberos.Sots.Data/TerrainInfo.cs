using Kerberos.Sots.Framework;
using System;
namespace Kerberos.Sots.Data
{
	internal class TerrainInfo : IIDProvider
	{
		public string Name;
		public Vector3 Origin;
		public int ID
		{
			get;
			set;
		}
	}
}
