using mars.ipc;
using System;
namespace Kerberos.Sots.Engine
{
	internal class ManagedMessageQueue : IMessageQueue
	{
		private unsafe mars.ipc.IMessageQueue* _q = queue;
		public unsafe int IncomingSize
		{
			get
			{
				mars.ipc.IMessageQueue* expr_06 = this._q;
				return calli(System.UInt32 modopt(System.Runtime.CompilerServices.CallConvThiscall)(System.IntPtr), expr_06, *(*(int*)expr_06 + 16));
			}
		}
		public unsafe int OutgoingSize
		{
			get
			{
				mars.ipc.IMessageQueue* expr_06 = this._q;
				return calli(System.UInt32 modopt(System.Runtime.CompilerServices.CallConvThiscall)(System.IntPtr), expr_06, *(*(int*)expr_06 + 12));
			}
		}
		public unsafe int IncomingCapacity
		{
			get
			{
				mars.ipc.IMessageQueue* expr_06 = this._q;
				return calli(System.UInt32 modopt(System.Runtime.CompilerServices.CallConvThiscall)(System.IntPtr), expr_06, *(*(int*)expr_06 + 8));
			}
		}
		public unsafe int OutgoingCapacity
		{
			get
			{
				mars.ipc.IMessageQueue* expr_06 = this._q;
				return calli(System.UInt32 modopt(System.Runtime.CompilerServices.CallConvThiscall)(System.IntPtr), expr_06, *(*(int*)expr_06 + 4));
			}
		}
		public unsafe void PrepareIncoming()
		{
			mars.ipc.IMessageQueue* expr_06 = this._q;
			calli(System.Void modopt(System.Runtime.CompilerServices.CallConvThiscall)(System.IntPtr), expr_06, *(*(int*)expr_06 + 20));
		}
		public unsafe void PrepareOutgoing()
		{
			mars.ipc.IMessageQueue* expr_06 = this._q;
			calli(System.Void modopt(System.Runtime.CompilerServices.CallConvThiscall)(System.IntPtr), expr_06, *(*(int*)expr_06 + 24));
		}
		public unsafe void Update()
		{
			mars.ipc.IMessageQueue* expr_06 = this._q;
			calli(System.Void modopt(System.Runtime.CompilerServices.CallConvThiscall)(System.IntPtr), expr_06, *(*(int*)expr_06 + 28));
		}
		public unsafe int GetNextMessage(byte[] data)
		{
			if (data != null && data.Length != 0)
			{
				int var_5_0E_cp_1 = 0;
				uint num = (uint)data.Length;
				mars.ipc.IMessageQueue* q = this._q;
				mars.ipc.IMessageQueue* expr_1F = q;
				uint num2 = calli(System.UInt32 modopt(System.Runtime.CompilerServices.CallConvThiscall)(System.IntPtr), expr_1F, *(*expr_1F + 32));
				uint num3;
				if (num < num2)
				{
					num3 = num;
				}
				else
				{
					num3 = num2;
				}
				return calli(System.UInt32 modopt(System.Runtime.CompilerServices.CallConvThiscall)(System.IntPtr,System.Void*,System.UInt32), q, ref data[var_5_0E_cp_1], num3, *(*q + 36));
			}
			throw new ArgumentNullException("data");
		}
		public unsafe int GetNextMessageSize()
		{
			mars.ipc.IMessageQueue* expr_06 = this._q;
			return calli(System.UInt32 modopt(System.Runtime.CompilerServices.CallConvThiscall)(System.IntPtr), expr_06, *(*(int*)expr_06 + 32));
		}
		public unsafe int GetNextMessageData(byte[] data, int size)
		{
			if (data != null && data.Length != 0)
			{
				int var_1_0E_cp_1 = 0;
				int num = *(int*)this._q + 36;
				return calli(System.UInt32 modopt(System.Runtime.CompilerServices.CallConvThiscall)(System.IntPtr,System.Void*,System.UInt32), this._q, ref data[var_1_0E_cp_1], size, *num);
			}
			throw new ArgumentNullException("data");
		}
		public unsafe void PutMessage(byte[] data, int count)
		{
			if (data == null || data.Length == 0)
			{
				throw new ArgumentNullException("data");
			}
			if (count <= 0)
			{
				throw new ArgumentOutOfRangeException("count");
			}
			int var_2_1D_cp_1 = 0;
			int num = *(int*)this._q + 40;
			mars.ipc.IMessageQueue.ErrorCode errorCode = calli(mars.ipc.IMessageQueue/ErrorCode modopt(System.Runtime.CompilerServices.CallConvThiscall)(System.IntPtr,System.Void modopt(System.Runtime.CompilerServices.IsConst)*,System.UInt32), this._q, ref data[var_2_1D_cp_1], count, *num);
			if (errorCode == (mars.ipc.IMessageQueue.ErrorCode)0)
			{
				return;
			}
			if (errorCode == (mars.ipc.IMessageQueue.ErrorCode)1)
			{
				throw new InvalidOperationException("Buffer is too full to accommodate message at this time.");
			}
			throw new InvalidOperationException("Message data is too large to fit in the outgoing buffer.");
		}
		public unsafe ManagedMessageQueue(mars.ipc.IMessageQueue* queue)
		{
		}
	}
}
