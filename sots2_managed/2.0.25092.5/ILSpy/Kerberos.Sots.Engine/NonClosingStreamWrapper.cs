using System;
using System.IO;
using System.Runtime.Remoting;
namespace Kerberos.Sots.Engine
{
	public sealed class NonClosingStreamWrapper : Stream
	{
		private Stream stream;
		private bool closed;
		public Stream BaseStream
		{
			get
			{
				return this.stream;
			}
		}
		public override bool CanRead
		{
			get
			{
				return !this.closed && this.stream.CanRead;
			}
		}
		public override bool CanSeek
		{
			get
			{
				return !this.closed && this.stream.CanSeek;
			}
		}
		public override bool CanWrite
		{
			get
			{
				return !this.closed && this.stream.CanWrite;
			}
		}
		public override long Length
		{
			get
			{
				this.CheckClosed();
				return this.stream.Length;
			}
		}
		public override long Position
		{
			get
			{
				this.CheckClosed();
				return this.stream.Position;
			}
			set
			{
				this.CheckClosed();
				this.stream.Position = value;
			}
		}
		public NonClosingStreamWrapper(Stream stream)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}
			this.stream = stream;
		}
		private void CheckClosed()
		{
			if (this.closed)
			{
				throw new InvalidOperationException("Wrapper has been closed or disposed");
			}
		}
		public void CloseContainer()
		{
			base.Close();
			this.stream.Close();
		}
		public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			this.CheckClosed();
			return this.stream.BeginRead(buffer, offset, count, callback, state);
		}
		public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			this.CheckClosed();
			return this.stream.BeginWrite(buffer, offset, count, callback, state);
		}
		public override void Close()
		{
			if (!this.closed)
			{
				this.stream.Flush();
			}
			this.stream.Position = 0L;
		}
		public override ObjRef CreateObjRef(Type requestedType)
		{
			throw new NotSupportedException();
		}
		public override int EndRead(IAsyncResult asyncResult)
		{
			this.CheckClosed();
			return this.stream.EndRead(asyncResult);
		}
		public override void EndWrite(IAsyncResult asyncResult)
		{
			this.CheckClosed();
			this.stream.EndWrite(asyncResult);
		}
		public override void Flush()
		{
			this.CheckClosed();
			this.stream.Flush();
		}
		public override object InitializeLifetimeService()
		{
			throw new NotSupportedException();
		}
		public override int Read(byte[] buffer, int offset, int count)
		{
			this.CheckClosed();
			return this.stream.Read(buffer, offset, count);
		}
		public override int ReadByte()
		{
			this.CheckClosed();
			return this.stream.ReadByte();
		}
		public override long Seek(long offset, SeekOrigin origin)
		{
			this.CheckClosed();
			return this.stream.Seek(offset, origin);
		}
		public override void SetLength(long value)
		{
			this.CheckClosed();
			this.stream.SetLength(value);
		}
		public override void Write(byte[] buffer, int offset, int count)
		{
			this.CheckClosed();
			this.stream.Write(buffer, offset, count);
		}
		public override void WriteByte(byte value)
		{
			this.CheckClosed();
			this.stream.WriteByte(value);
		}
	}
}
