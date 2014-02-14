using System;
namespace Kerberos.Sots.ShipFramework
{
	internal class LogicalShipSpark
	{
		public enum ShipSparkType
		{
			SMALL_SPARK,
			LARGE_SPARK,
			FIRE
		}
		public LogicalShipSpark.ShipSparkType Type;
		public LogicalEffect SparkEffect;
	}
}
