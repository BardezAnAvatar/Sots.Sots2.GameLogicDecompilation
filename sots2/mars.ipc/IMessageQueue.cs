using Microsoft.VisualC;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
namespace mars.ipc
{
	[DebugInfoInPDB, MiscellaneousBits(64), NativeCppClass]
	[StructLayout(LayoutKind.Sequential, Size = 4)]
	internal static struct IMessageQueue
	{
		[DebugInfoInPDB, MiscellaneousBits(64), CLSCompliant(false), NativeCppClass]
		public enum ErrorCode
		{

		}
	}
}
