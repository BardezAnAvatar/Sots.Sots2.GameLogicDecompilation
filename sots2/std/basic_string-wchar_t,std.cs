using Microsoft.VisualC;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
namespace std
{
	[DebugInfoInPDB, MiscellaneousBits(64), NativeCppClass, UnsafeValueType]
	[StructLayout(LayoutKind.Sequential, Size = 28)]
	internal struct basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >
	{
		public unsafe static void <MarshalCopy>(basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >* ptr, basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >* right)
		{
			*(int*)(ptr + 20 / sizeof(basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >)) = 7;
			*(int*)(ptr + 16 / sizeof(basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >)) = 0;
			*(short*)ptr = 0;
			<Module>.std.basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >.assign(ptr, right, 0u, 4294967295u);
		}
		public unsafe static void <MarshalDestroy>(basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >* ptr)
		{
			if (8 <= *(int*)(ptr + 20 / sizeof(basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >)))
			{
				<Module>.delete(*(int*)ptr);
			}
			*(int*)(ptr + 20 / sizeof(basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >)) = 7;
			*(int*)(ptr + 16 / sizeof(basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >)) = 0;
			*(short*)ptr = 0;
		}
	}
}
