using Microsoft.VisualC;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
namespace std
{
	[DebugInfoInPDB, MiscellaneousBits(64), NativeCppClass]
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	internal struct allocator<char>
	{
		[DebugInfoInPDB, MiscellaneousBits(65), CLSCompliant(false), NativeCppClass]
		[StructLayout(LayoutKind.Sequential, Size = 1)]
		public struct rebind<char>
		{
		}
		public unsafe static void <MarshalCopy>(allocator<char>* ptr, allocator<char>* ptr2)
		{
		}
	}
}
