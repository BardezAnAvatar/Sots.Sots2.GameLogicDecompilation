using Kerberos.Sots.Framework;
using System;
namespace Kerberos.Sots.ShipFramework
{
	internal class LogicalMount
	{
		public LogicalBank Bank;
		public string TurretOverload;
		public string BarrelOverload;
		public string BaseOverload;
		public string NodeName;
		public string FireAnimName;
		public string ReloadAnimName;
		public MinMax Yaw = new MinMax
		{
			Min = -60f,
			Max = 60f
		};
		public MinMax Pitch = new MinMax
		{
			Min = -5f,
			Max = 60f
		};
		public override string ToString()
		{
			return this.NodeName ?? string.Empty;
		}
	}
}
