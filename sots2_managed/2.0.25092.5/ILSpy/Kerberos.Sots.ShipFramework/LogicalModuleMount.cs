using System;
namespace Kerberos.Sots.ShipFramework
{
	internal class LogicalModuleMount
	{
		public ShipSectionAsset Section;
		public string AssignedModuleName = "";
		public string ModuleType = "";
		public string NodeName = "";
		public int FrameX;
		public int FrameY;
		public override string ToString()
		{
			return string.Concat(new string[]
			{
				this.Section.FileName,
				",",
				this.NodeName,
				",",
				this.ModuleType.ToString()
			});
		}
	}
}
