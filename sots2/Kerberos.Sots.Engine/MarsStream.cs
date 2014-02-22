using mars.fs;
using System;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
namespace Kerberos.Sots.Engine
{
	internal class MarsStream : Stream
	{
		private bool disposed;
		private unsafe mars.fs.IFileSystem* fs;
		private unsafe IStream* stream;
		public unsafe sealed override long Position
		{
			get
			{
				this.VerifyObjectNotDisposed();
				IStream* expr_0C = this.stream;
				return calli(System.Int64 modopt(System.Runtime.CompilerServices.CallConvThiscall)(System.IntPtr), expr_0C, *(*(int*)expr_0C + 16));
			}
			set
			{
				this.VerifyObjectNotDisposed();
				IStream* ptr = this.stream;
				object arg_1B_0 = calli(System.Int64 modopt(System.Runtime.CompilerServices.CallConvThiscall)(System.IntPtr,System.Int64,mars.fs.SeekOrigin), ptr, value, 1, *(*(int*)ptr + 12));
			}
		}
		public unsafe sealed override long Length
		{
			get
			{
				this.VerifyObjectNotDisposed();
				IStream* expr_0C = this.stream;
				return calli(System.Int64 modopt(System.Runtime.CompilerServices.CallConvThiscall)(System.IntPtr), expr_0C, *(*(int*)expr_0C + 20));
			}
		}
		public sealed override bool CanWrite
		{
			[return: MarshalAs(UnmanagedType.U1)]
			get
			{
				return false;
			}
		}
		public sealed override bool CanSeek
		{
			[return: MarshalAs(UnmanagedType.U1)]
			get
			{
				return true;
			}
		}
		public sealed override bool CanRead
		{
			[return: MarshalAs(UnmanagedType.U1)]
			get
			{
				return true;
			}
		}
		private void VerifyObjectNotDisposed()
		{
			if (this.disposed)
			{
				throw new ObjectDisposedException("Methods were called after the stream was closed.");
			}
		}
		private void VerifyBufferIOParameters(byte[] buffer, int offset, int count)
		{
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}
			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException("offset", "offset is negative.");
			}
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count", "count is negative.");
			}
			if (offset + count > buffer.Length)
			{
				throw new ArgumentException("The sum of offset and count is larger than the buffer length.");
			}
		}
		internal unsafe MarsStream(mars.fs.IFileSystem* fs, IStream* stream)
		{
			try
			{
				if (fs == null)
				{
					throw new ArgumentNullException("fs");
				}
				if (stream == null)
				{
					throw new ArgumentNullException("stream");
				}
				this.fs = fs;
				this.stream = stream;
			}
			catch
			{
				base.Dispose(true);
				throw;
			}
		}
		private unsafe void ~MarsStream()
		{
			if (!this.disposed)
			{
				IStream* ptr = this.stream;
				if (ptr != null)
				{
					IStream* ptr2 = ptr;
					mars.fs.IFileSystem* ptr3 = this.fs;
					calli(System.Void modopt(System.Runtime.CompilerServices.CallConvThiscall)(System.IntPtr,mars.fs.IStream** modopt(System.Runtime.CompilerServices.IsImplicitlyDereferenced)), ptr3, ref ptr2, *(*(int*)ptr3 + 8));
					this.stream = null;
				}
				this.disposed = true;
			}
		}
		public sealed override void Flush()
		{
		}
		public unsafe sealed override int Read(byte[] buffer, int offset, int count)
		{
			this.VerifyBufferIOParameters(buffer, offset, count);
			this.VerifyObjectNotDisposed();
			int num = offset;
			int num2 = count;
			int num3 = 0;
			if (count > 0)
			{
				do
				{
					int var_1_22_cp_1 = num;
					try
					{
						int num4 = *(int*)this.stream + 4;
						int num5 = calli(System.Int32 modopt(System.Runtime.CompilerServices.CallConvThiscall)(System.IntPtr,System.Void*,System.Int32), this.stream, ref buffer[var_1_22_cp_1], num2, *num4);
						if (num5 == 0)
						{
							break;
						}
						num = num5 + num;
						num2 -= num5;
						num3 = num5 + num3;
					}
					catch
					{
						throw;
					}
				}
				while (num2 > 0);
			}
			return num3;
		}
		public unsafe sealed override long Seek(long offset, System.IO.SeekOrigin origin)
		{
			this.VerifyObjectNotDisposed();
			mars.fs.SeekOrigin seekOrigin;
			if (origin != System.IO.SeekOrigin.Begin)
			{
				if (origin != System.IO.SeekOrigin.Current)
				{
					if (origin != System.IO.SeekOrigin.End)
					{
						throw new ArgumentOutOfRangeException("origin");
					}
					seekOrigin = (mars.fs.SeekOrigin)2;
				}
				else
				{
					seekOrigin = (mars.fs.SeekOrigin)1;
				}
			}
			else
			{
				seekOrigin = (mars.fs.SeekOrigin)0;
			}
			IStream* ptr = this.stream;
			if (calli(System.Int64 modopt(System.Runtime.CompilerServices.CallConvThiscall)(System.IntPtr,System.Int64,mars.fs.SeekOrigin), ptr, offset, seekOrigin, *(*(int*)ptr + 12)) == 0L)
			{
				throw new IOException("Seek failed.");
			}
			return 0L;
		}
		public sealed override void SetLength(long value)
		{
			this.VerifyObjectNotDisposed();
			throw new NotSupportedException();
		}
		public sealed override void Write(byte[] buffer, int offset, int count)
		{
			this.VerifyBufferIOParameters(buffer, offset, count);
			this.VerifyObjectNotDisposed();
			throw new NotSupportedException();
		}
		[HandleProcessCorruptedStateExceptions]
		protected override void Dispose([MarshalAs(UnmanagedType.U1)] bool flag)
		{
			if (flag)
			{
				try
				{
					this.~MarsStream();
					return;
				}
				finally
				{
					base.Dispose(true);
				}
			}
			base.Dispose(false);
		}
	}
}
