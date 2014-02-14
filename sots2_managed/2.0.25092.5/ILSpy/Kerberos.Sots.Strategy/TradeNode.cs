using System;
namespace Kerberos.Sots.Strategy
{
	public class TradeNode
	{
		public int Produced;
		public int ProductionCapacity;
		public int Consumption;
		public int Freighters;
		public int DockCapacity;
		public int DockExportCapacity;
		public int ExportInt;
		public int ExportProv;
		public int ExportLoc;
		public int ImportInt;
		public int ImportProv;
		public int ImportLoc;
		public float Range;
		public int Turn;
		public int GetTotalImportsAndExports()
		{
			return this.ImportInt + this.ImportProv + this.ImportLoc + this.ExportInt + this.ExportProv + this.ExportLoc;
		}
	}
}
