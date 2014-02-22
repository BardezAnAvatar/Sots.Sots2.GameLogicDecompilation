using mars.log;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
namespace Kerberos.Sots.Engine
{
	internal class MarsLogHost : ILogHost, IDisposable
	{
		private unsafe mars.log.ILogHost* logHost = logHost;
		private unsafe MarsLogListener* listener = null;
		private MessageLoggedEventHandler messageLoggedDelegate;
		public event MessageLoggedEventHandler MessageLogged
		{
			add
			{
				this.messageLoggedDelegate = (MessageLoggedEventHandler)Delegate.Combine(this.messageLoggedDelegate, value);
			}
			remove
			{
				this.messageLoggedDelegate = (MessageLoggedEventHandler)Delegate.Remove(this.messageLoggedDelegate, value);
			}
		}
		public unsafe string FilePath
		{
			get
			{
				mars.log.ILogHost* expr_06 = this.logHost;
				return new string(calli(System.Char modopt(System.Runtime.CompilerServices.IsConst)* modopt(System.Runtime.CompilerServices.CallConvThiscall)(System.IntPtr), expr_06, *(*(int*)expr_06 + 4)));
			}
		}
		public unsafe LogLevel Level
		{
			get
			{
				mars.log.ILogHost* expr_06 = this.logHost;
				return calli(mars.log.LogLevel modopt(System.Runtime.CompilerServices.CallConvThiscall)(System.IntPtr), expr_06, *(*(int*)expr_06 + 12));
			}
			set
			{
				mars.log.ILogHost* ptr = this.logHost;
				calli(System.Void modopt(System.Runtime.CompilerServices.CallConvThiscall)(System.IntPtr,mars.log.LogLevel), ptr, value, *(*(int*)ptr + 8));
			}
		}
		internal unsafe MarsLogHost(mars.log.ILogHost* logHost)
		{
			MarsLogListener* ptr = <Module>.@new(8u);
			MarsLogListener* ptr3;
			try
			{
				if (ptr != null)
				{
					*(int*)ptr = ref <Module>.??_7ILogListener@log@mars@@6B@;
					try
					{
						*(int*)ptr = ref <Module>.??_7MarsLogListener@Engine@Sots@Kerberos@@6B@;
						MarsLogListener* ptr2 = ptr + 4 / sizeof(MarsLogListener);
						<Module>.gcroot<Kerberos::Sots::Engine::MarsLogHost ^>.{ctor}(ptr2);
						try
						{
							<Module>.gcroot<Kerberos::Sots::Engine::MarsLogHost ^>.=(ptr2, this);
						}
						catch
						{
							<Module>.___CxxCallUnwindDtor(ldftn(gcroot<Kerberos::Sots::Engine::MarsLogHost ^>.{dtor}), (void*)(ptr + 4 / sizeof(MarsLogListener)));
							throw;
						}
					}
					catch
					{
						<Module>.___CxxCallUnwindDtor(ldftn(mars.log.ILogListener.{dtor}), (void*)ptr);
						throw;
					}
					ptr3 = ptr;
				}
				else
				{
					ptr3 = 0;
				}
			}
			catch
			{
				<Module>.delete((void*)ptr);
				throw;
			}
			this.listener = ptr3;
			if (!calli(System.Byte modopt(System.Runtime.CompilerServices.CompilerMarshalOverride) modopt(System.Runtime.CompilerServices.CallConvThiscall)(System.IntPtr,mars.log.ILogListener*), logHost, ptr3, *(*(int*)logHost + 16)))
			{
				throw new OutOfMemoryException("Failed to add log listener for script.");
			}
		}
		internal unsafe void RaiseMessageLogged(mars.log.LogMessageInfo* messageInfo)
		{
			if (this.messageLoggedDelegate != null)
			{
				LogMessageInfo messageInfo2 = new LogMessageInfo(*messageInfo, *(messageInfo + 4), *(messageInfo + 8), new string(*(messageInfo + 12)), new string(*(messageInfo + 16)), new string(*(messageInfo + 20)));
				this.messageLoggedDelegate(messageInfo2);
			}
		}
		private unsafe void ~MarsLogHost()
		{
			mars.log.ILogHost* ptr = this.logHost;
			object arg_19_0 = calli(System.Byte modopt(System.Runtime.CompilerServices.CompilerMarshalOverride) modopt(System.Runtime.CompilerServices.CallConvThiscall)(System.IntPtr,mars.log.ILogListener*), ptr, this.listener, *(*(int*)ptr + 20));
			MarsLogListener* ptr2 = this.listener;
			if (ptr2 != null)
			{
				object arg_2E_0 = calli(System.Void* modopt(System.Runtime.CompilerServices.CallConvThiscall)(System.IntPtr,System.UInt32), ptr2, 1, *(*(int*)ptr2));
			}
			this.listener = null;
			this.logHost = null;
		}
		public unsafe void LogMessage(LogLevel level, LogSeverity severity, string category, string message)
		{
			byte* ptr = category;
			if (ptr != null)
			{
				ptr = RuntimeHelpers.OffsetToStringData + ptr;
			}
			Char modopt(System.Runtime.CompilerServices.IsConst)& char modopt(System.Runtime.CompilerServices.IsConst)& = ptr;
			byte* ptr2 = message;
			if (ptr2 != null)
			{
				ptr2 = RuntimeHelpers.OffsetToStringData + ptr2;
			}
			Char modopt(System.Runtime.CompilerServices.IsConst)& char modopt(System.Runtime.CompilerServices.IsConst)&2 = ptr2;
			int num = *(int*)this.logHost + 24;
			calli(System.Void modopt(System.Runtime.CompilerServices.CallConvThiscall)(System.IntPtr,mars.log.LogLevel,mars.log.LogSeverity,System.Char modopt(System.Runtime.CompilerServices.IsConst)*,System.Char modopt(System.Runtime.CompilerServices.IsConst)*), this.logHost, level, severity, char modopt(System.Runtime.CompilerServices.IsConst)&, char modopt(System.Runtime.CompilerServices.IsConst)&2, *num);
		}
		protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool flag)
		{
			if (flag)
			{
				this.~MarsLogHost();
			}
			else
			{
				base.Finalize();
			}
		}
		public sealed override void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
