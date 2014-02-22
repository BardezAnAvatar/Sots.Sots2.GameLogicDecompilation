using mars.fs;
using std;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
namespace Kerberos.Sots.Engine
{
	internal class MarsFileSystem : IFileSystem
	{
		private unsafe mars.fs.IFileSystem* fs;
		internal unsafe MarsFileSystem(mars.fs.IFileSystem* fs)
		{
			if (fs == null)
			{
				throw new ArgumentNullException("fs");
			}
			this.fs = fs;
		}
		public unsafe Stream CreateStream(string path)
		{
			if (path == null)
			{
				throw new ArgumentNullException("path");
			}
			byte* ptr = path;
			if (ptr != null)
			{
				ptr = RuntimeHelpers.OffsetToStringData + ptr;
			}
			Char modopt(System.Runtime.CompilerServices.IsConst)& char modopt(System.Runtime.CompilerServices.IsConst)& = ptr;
			IStream* ptr2;
			try
			{
				int num = *(int*)this.fs + 4;
				ptr2 = calli(mars.fs.IStream* modopt(System.Runtime.CompilerServices.CallConvThiscall)(System.IntPtr,System.Char modopt(System.Runtime.CompilerServices.IsConst)*), this.fs, char modopt(System.Runtime.CompilerServices.IsConst)&, *num);
			}
			catch
			{
				throw;
			}
			if (ptr2 == null)
			{
				throw new FileNotFoundException("Failed to create stream.", path);
			}
			return new MarsStream(this.fs, ptr2);
		}
		public unsafe void SplitBasePath(string path, out string mountName, out string suffix)
		{
			if (path == null)
			{
				throw new ArgumentNullException("path");
			}
			basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> > basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >;
			*(ref basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> > + 20) = 7;
			*(ref basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> > + 16) = 0;
			basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> > = 0;
			try
			{
				basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> > basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >2;
				*(ref basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >2 + 20) = 7;
				*(ref basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >2 + 16) = 0;
				basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >2 = 0;
				try
				{
					byte* ptr = path;
					if (ptr != null)
					{
						ptr = RuntimeHelpers.OffsetToStringData + ptr;
					}
					Char modopt(System.Runtime.CompilerServices.IsConst)& char modopt(System.Runtime.CompilerServices.IsConst)& = ptr;
					try
					{
						char* ptr2 = char modopt(System.Runtime.CompilerServices.IsConst)&;
						basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> > basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >3;
						*(ref basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >3 + 20) = 7;
						*(ref basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >3 + 16) = 0;
						basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >3 = 0;
						char* ptr3 = ptr2;
						if (*(short*)ptr2 != 0)
						{
							do
							{
								ptr3 += 2 / sizeof(char);
							}
							while (*(short*)ptr3 != 0);
						}
						uint count = ptr3 - ptr2 / sizeof(char) >> 1;
						<Module>.std.basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >.assign(ref basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >3, ptr2, count);
						try
						{
							mars.fs.IFileSystem* ptr4 = this.fs;
							calli(System.Void modopt(System.Runtime.CompilerServices.CallConvThiscall)(System.IntPtr,std.basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> > modopt(System.Runtime.CompilerServices.IsConst)* modopt(System.Runtime.CompilerServices.IsImplicitlyDereferenced),std.basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >* modopt(System.Runtime.CompilerServices.IsImplicitlyDereferenced),std.basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >* modopt(System.Runtime.CompilerServices.IsImplicitlyDereferenced)), ptr4, ref basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >3, ref basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >, ref basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >2, *(*(int*)ptr4 + 12));
						}
						catch
						{
							<Module>.___CxxCallUnwindDtor(ldftn(std.basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >.{dtor}), (void*)(&basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >3));
							throw;
						}
						if (8 <= *(ref basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >3 + 20))
						{
							<Module>.delete(basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >3);
						}
						*(ref basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >3 + 20) = 7;
						*(ref basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >3 + 16) = 0;
						basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >3 = 0;
					}
					catch
					{
						throw;
					}
					char* value;
					if (8 <= *(ref basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> > + 20))
					{
						value = basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >;
					}
					else
					{
						value = (char*)(&basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >);
					}
					mountName = new string((char*)value);
					char* value2;
					if (8 <= *(ref basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >2 + 20))
					{
						value2 = basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >2;
					}
					else
					{
						value2 = (char*)(&basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >2);
					}
					suffix = new string((char*)value2);
				}
				catch
				{
					<Module>.___CxxCallUnwindDtor(ldftn(std.basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >.{dtor}), (void*)(&basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >2));
					throw;
				}
				if (8 <= *(ref basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >2 + 20))
				{
					<Module>.delete(basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >2);
				}
				*(ref basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >2 + 20) = 7;
				*(ref basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >2 + 16) = 0;
				basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >2 = 0;
			}
			catch
			{
				<Module>.___CxxCallUnwindDtor(ldftn(std.basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >.{dtor}), (void*)(&basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >));
				throw;
			}
			if (8 <= *(ref basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> > + 20))
			{
				<Module>.delete(basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >);
			}
		}
		[return: MarshalAs(UnmanagedType.U1)]
		public unsafe bool IsBasePath(string path)
		{
			if (path == null)
			{
				throw new ArgumentNullException("path");
			}
			byte* ptr = path;
			if (ptr != null)
			{
				ptr = RuntimeHelpers.OffsetToStringData + ptr;
			}
			Char modopt(System.Runtime.CompilerServices.IsConst)& char modopt(System.Runtime.CompilerServices.IsConst)& = ptr;
			int num = *(int*)this.fs + 16;
			return calli(System.Byte modopt(System.Runtime.CompilerServices.CompilerMarshalOverride) modopt(System.Runtime.CompilerServices.CallConvThiscall)(System.IntPtr,System.Char modopt(System.Runtime.CompilerServices.IsConst)*), this.fs, char modopt(System.Runtime.CompilerServices.IsConst)&, *num);
		}
		[return: MarshalAs(UnmanagedType.U1)]
		public unsafe bool IsRootedPath(string path)
		{
			if (path == null)
			{
				throw new ArgumentNullException("path");
			}
			byte* ptr = path;
			if (ptr != null)
			{
				ptr = RuntimeHelpers.OffsetToStringData + ptr;
			}
			Char modopt(System.Runtime.CompilerServices.IsConst)& char modopt(System.Runtime.CompilerServices.IsConst)& = ptr;
			int num = *(int*)this.fs + 20;
			return calli(System.Byte modopt(System.Runtime.CompilerServices.CompilerMarshalOverride) modopt(System.Runtime.CompilerServices.CallConvThiscall)(System.IntPtr,System.Char modopt(System.Runtime.CompilerServices.IsConst)*), this.fs, char modopt(System.Runtime.CompilerServices.IsConst)&, *num);
		}
		[return: MarshalAs(UnmanagedType.U1)]
		public unsafe bool FileExists(string path)
		{
			if (path == null)
			{
				throw new ArgumentNullException("path");
			}
			byte* ptr = path;
			if (ptr != null)
			{
				ptr = RuntimeHelpers.OffsetToStringData + ptr;
			}
			Char modopt(System.Runtime.CompilerServices.IsConst)& char modopt(System.Runtime.CompilerServices.IsConst)& = ptr;
			int num = *(int*)this.fs + 24;
			return calli(System.Byte modopt(System.Runtime.CompilerServices.CompilerMarshalOverride) modopt(System.Runtime.CompilerServices.CallConvThiscall)(System.IntPtr,System.Char modopt(System.Runtime.CompilerServices.IsConst)*), this.fs, char modopt(System.Runtime.CompilerServices.IsConst)&, *num);
		}
		public unsafe string[] FindFiles(string pattern)
		{
			if (pattern == null)
			{
				throw new ArgumentNullException("pattern");
			}
			vector<std::basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >,std::allocator<std::basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> > > > vector<std::basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >,std::allocator<std::basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> > > > = 0;
			*(ref vector<std::basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >,std::allocator<std::basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> > > > + 4) = 0;
			*(ref vector<std::basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >,std::allocator<std::basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> > > > + 8) = 0;
			string[] array;
			try
			{
				<Module>.std.vector<std::basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >,std::allocator<std::basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> > > >.reserve(ref vector<std::basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >,std::allocator<std::basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> > > >, 20u);
				byte* ptr = pattern;
				if (ptr != null)
				{
					ptr = RuntimeHelpers.OffsetToStringData + ptr;
				}
				Char modopt(System.Runtime.CompilerServices.IsConst)& char modopt(System.Runtime.CompilerServices.IsConst)& = ptr;
				try
				{
					int num = *(int*)this.fs + 28;
					calli(System.Void modopt(System.Runtime.CompilerServices.CallConvThiscall)(System.IntPtr,System.Char modopt(System.Runtime.CompilerServices.IsConst)*,std.vector<std::basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >,std::allocator<std::basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> > > >* modopt(System.Runtime.CompilerServices.IsImplicitlyDereferenced)), this.fs, char modopt(System.Runtime.CompilerServices.IsConst)&, ref vector<std::basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >,std::allocator<std::basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> > > >, *num);
				}
				catch
				{
					throw;
				}
				array = new string[(*(ref vector<std::basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >,std::allocator<std::basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> > > > + 4) - vector<std::basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >,std::allocator<std::basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> > > >) / 28];
				int num2 = 0;
				if (0 < array.Length)
				{
					int num3 = 0;
					do
					{
						basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >* ptr2 = num3 + vector<std::basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >,std::allocator<std::basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> > > >;
						char* value;
						if (8 <= *(ptr2 + 20))
						{
							value = *ptr2;
						}
						else
						{
							value = ptr2;
						}
						array[num2] = new string((char*)value);
						num2++;
						num3 += 28;
					}
					while (num2 < array.Length);
				}
			}
			catch
			{
				<Module>.___CxxCallUnwindDtor(ldftn(std.vector<std::basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >,std::allocator<std::basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> > > >.{dtor}), (void*)(&vector<std::basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >,std::allocator<std::basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> > > >));
				throw;
			}
			<Module>.std.vector<std::basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >,std::allocator<std::basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> > > >._Tidy(ref vector<std::basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >,std::allocator<std::basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> > > >);
			return array;
		}
		public unsafe string[] FindDirectories(string pattern)
		{
			if (pattern == null)
			{
				throw new ArgumentNullException("pattern");
			}
			vector<std::basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >,std::allocator<std::basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> > > > vector<std::basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >,std::allocator<std::basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> > > > = 0;
			*(ref vector<std::basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >,std::allocator<std::basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> > > > + 4) = 0;
			*(ref vector<std::basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >,std::allocator<std::basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> > > > + 8) = 0;
			string[] array;
			try
			{
				<Module>.std.vector<std::basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >,std::allocator<std::basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> > > >.reserve(ref vector<std::basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >,std::allocator<std::basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> > > >, 20u);
				byte* ptr = pattern;
				if (ptr != null)
				{
					ptr = RuntimeHelpers.OffsetToStringData + ptr;
				}
				Char modopt(System.Runtime.CompilerServices.IsConst)& char modopt(System.Runtime.CompilerServices.IsConst)& = ptr;
				try
				{
					int num = *(int*)this.fs + 32;
					calli(System.Void modopt(System.Runtime.CompilerServices.CallConvThiscall)(System.IntPtr,System.Char modopt(System.Runtime.CompilerServices.IsConst)*,std.vector<std::basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >,std::allocator<std::basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> > > >* modopt(System.Runtime.CompilerServices.IsImplicitlyDereferenced)), this.fs, char modopt(System.Runtime.CompilerServices.IsConst)&, ref vector<std::basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >,std::allocator<std::basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> > > >, *num);
				}
				catch
				{
					throw;
				}
				array = new string[(*(ref vector<std::basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >,std::allocator<std::basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> > > > + 4) - vector<std::basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >,std::allocator<std::basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> > > >) / 28];
				int num2 = 0;
				if (0 < array.Length)
				{
					int num3 = 0;
					do
					{
						basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >* ptr2 = num3 + vector<std::basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >,std::allocator<std::basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> > > >;
						char* value;
						if (8 <= *(ptr2 + 20))
						{
							value = *ptr2;
						}
						else
						{
							value = ptr2;
						}
						array[num2] = new string((char*)value);
						num2++;
						num3 += 28;
					}
					while (num2 < array.Length);
				}
			}
			catch
			{
				<Module>.___CxxCallUnwindDtor(ldftn(std.vector<std::basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >,std::allocator<std::basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> > > >.{dtor}), (void*)(&vector<std::basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >,std::allocator<std::basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> > > >));
				throw;
			}
			<Module>.std.vector<std::basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >,std::allocator<std::basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> > > >._Tidy(ref vector<std::basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >,std::allocator<std::basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> > > >);
			return array;
		}
		[return: MarshalAs(UnmanagedType.U1)]
		public unsafe bool TryResolveBaseFilePath(string path, out string result)
		{
			result = null;
			if (path == null)
			{
				throw new ArgumentNullException("path");
			}
			basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> > basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >;
			*(ref basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> > + 20) = 7;
			*(ref basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> > + 16) = 0;
			basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> > = 0;
			bool flag;
			try
			{
				byte* ptr = path;
				if (ptr != null)
				{
					ptr = RuntimeHelpers.OffsetToStringData + ptr;
				}
				Char modopt(System.Runtime.CompilerServices.IsConst)& char modopt(System.Runtime.CompilerServices.IsConst)& = ptr;
				try
				{
					char* ptr2 = char modopt(System.Runtime.CompilerServices.IsConst)&;
					basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> > basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >2;
					*(ref basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >2 + 20) = 7;
					*(ref basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >2 + 16) = 0;
					basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >2 = 0;
					char* ptr3 = ptr2;
					if (*(short*)ptr2 != 0)
					{
						do
						{
							ptr3 += 2 / sizeof(char);
						}
						while (*(short*)ptr3 != 0);
					}
					uint count = ptr3 - ptr2 / sizeof(char) >> 1;
					<Module>.std.basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >.assign(ref basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >2, ptr2, count);
					try
					{
						mars.fs.IFileSystem* ptr4 = this.fs;
						flag = calli(System.Byte modopt(System.Runtime.CompilerServices.CompilerMarshalOverride) modopt(System.Runtime.CompilerServices.CallConvThiscall)(System.IntPtr,std.basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> > modopt(System.Runtime.CompilerServices.IsConst)* modopt(System.Runtime.CompilerServices.IsImplicitlyDereferenced),std.basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >* modopt(System.Runtime.CompilerServices.IsImplicitlyDereferenced)), ptr4, ref basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >2, ref basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >, *(*(int*)ptr4 + 36));
					}
					catch
					{
						<Module>.___CxxCallUnwindDtor(ldftn(std.basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >.{dtor}), (void*)(&basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >2));
						throw;
					}
					if (8 <= *(ref basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >2 + 20))
					{
						<Module>.delete(basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >2);
					}
					*(ref basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >2 + 20) = 7;
					*(ref basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >2 + 16) = 0;
					basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >2 = 0;
				}
				catch
				{
					throw;
				}
				if (flag)
				{
					char* value;
					if (8 <= *(ref basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> > + 20))
					{
						value = basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >;
					}
					else
					{
						value = (char*)(&basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >);
					}
					result = new string((char*)value);
				}
			}
			catch
			{
				<Module>.___CxxCallUnwindDtor(ldftn(std.basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >.{dtor}), (void*)(&basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >));
				throw;
			}
			if (8 <= *(ref basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> > + 20))
			{
				<Module>.delete(basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >);
			}
			return flag;
		}
		[return: MarshalAs(UnmanagedType.U1)]
		public unsafe bool TryResolveAbsoluteFilePath(string path, out string result)
		{
			result = null;
			if (path == null)
			{
				throw new ArgumentNullException("path");
			}
			basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> > basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >;
			*(ref basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> > + 20) = 7;
			*(ref basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> > + 16) = 0;
			basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> > = 0;
			bool flag;
			try
			{
				byte* ptr = path;
				if (ptr != null)
				{
					ptr = RuntimeHelpers.OffsetToStringData + ptr;
				}
				Char modopt(System.Runtime.CompilerServices.IsConst)& char modopt(System.Runtime.CompilerServices.IsConst)& = ptr;
				try
				{
					char* ptr2 = char modopt(System.Runtime.CompilerServices.IsConst)&;
					basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> > basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >2;
					*(ref basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >2 + 20) = 7;
					*(ref basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >2 + 16) = 0;
					basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >2 = 0;
					char* ptr3 = ptr2;
					if (*(short*)ptr2 != 0)
					{
						do
						{
							ptr3 += 2 / sizeof(char);
						}
						while (*(short*)ptr3 != 0);
					}
					uint count = ptr3 - ptr2 / sizeof(char) >> 1;
					<Module>.std.basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >.assign(ref basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >2, ptr2, count);
					try
					{
						mars.fs.IFileSystem* ptr4 = this.fs;
						flag = calli(System.Byte modopt(System.Runtime.CompilerServices.CompilerMarshalOverride) modopt(System.Runtime.CompilerServices.CallConvThiscall)(System.IntPtr,std.basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> > modopt(System.Runtime.CompilerServices.IsConst)* modopt(System.Runtime.CompilerServices.IsImplicitlyDereferenced),std.basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >* modopt(System.Runtime.CompilerServices.IsImplicitlyDereferenced)), ptr4, ref basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >2, ref basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >, *(*(int*)ptr4 + 40));
					}
					catch
					{
						<Module>.___CxxCallUnwindDtor(ldftn(std.basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >.{dtor}), (void*)(&basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >2));
						throw;
					}
					if (8 <= *(ref basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >2 + 20))
					{
						<Module>.delete(basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >2);
					}
					*(ref basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >2 + 20) = 7;
					*(ref basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >2 + 16) = 0;
					basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >2 = 0;
				}
				catch
				{
					throw;
				}
				if (flag)
				{
					char* value;
					if (8 <= *(ref basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> > + 20))
					{
						value = basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >;
					}
					else
					{
						value = (char*)(&basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >);
					}
					result = new string((char*)value);
				}
			}
			catch
			{
				<Module>.___CxxCallUnwindDtor(ldftn(std.basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >.{dtor}), (void*)(&basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >));
				throw;
			}
			if (8 <= *(ref basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> > + 20))
			{
				<Module>.delete(basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t> >);
			}
			return flag;
		}
	}
}
