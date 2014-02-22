using mars.script;
using System;
using System.Runtime.InteropServices;
namespace Kerberos.Sots.Engine
{
	internal class MarsEngine : IEngine
	{
		private unsafe IScriptEngine* engine;
		public unsafe string Version
		{
			get
			{
				IScriptEngine* expr_06 = this.engine;
				return new string(calli(System.Char modopt(System.Runtime.CompilerServices.IsConst)* modopt(System.Runtime.CompilerServices.CallConvThiscall)(System.IntPtr), expr_06, *(*(int*)expr_06 + 12)));
			}
		}
		public unsafe bool RenderingEnabled
		{
			[return: MarshalAs(UnmanagedType.U1)]
			get
			{
				IScriptEngine* expr_06 = this.engine;
				return calli(System.Byte modopt(System.Runtime.CompilerServices.CompilerMarshalOverride) modopt(System.Runtime.CompilerServices.CallConvThiscall)(System.IntPtr), expr_06, *(*(int*)expr_06 + 8));
			}
			[param: MarshalAs(UnmanagedType.U1)]
			set
			{
				IScriptEngine* ptr = this.engine;
				calli(System.Void modopt(System.Runtime.CompilerServices.CallConvThiscall)(System.IntPtr,System.Byte modopt(System.Runtime.CompilerServices.CompilerMarshalOverride)), ptr, value, *(*(int*)ptr + 4));
			}
		}
		internal unsafe MarsEngine(IScriptEngine* engine)
		{
			this.engine = engine;
		}
	}
}
