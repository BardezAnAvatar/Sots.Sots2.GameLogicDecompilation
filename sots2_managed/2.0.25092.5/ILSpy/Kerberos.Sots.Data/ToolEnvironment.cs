using System;
namespace Kerberos.Sots.Data
{
	public static class ToolEnvironment
	{
		public static bool IsRunningInTool
		{
			get;
			set;
		}
		static ToolEnvironment()
		{
			ToolEnvironment.IsRunningInTool = false;
		}
	}
}
