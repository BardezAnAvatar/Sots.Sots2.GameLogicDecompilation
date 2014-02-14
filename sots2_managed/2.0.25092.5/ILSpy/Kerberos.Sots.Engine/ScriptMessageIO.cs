using System;
using System.IO;
using System.Text;
namespace Kerberos.Sots.Engine
{
	public abstract class ScriptMessageIO : IDisposable
	{
		private readonly MemoryStream _data;
		private readonly UnicodeEncoding _encoding = new UnicodeEncoding();
		protected MemoryStream Stream
		{
			get
			{
				return this._data;
			}
		}
		public ScriptMessageIO(MemoryStream stream)
		{
			this._data = (stream ?? new MemoryStream(4096));
		}
		public virtual void Dispose()
		{
			this._data.Dispose();
		}
		public virtual long GetSize()
		{
			return this._data.Length;
		}
		public virtual void SetSize(long size)
		{
			this._data.SetLength(size);
		}
		public virtual byte[] GetBuffer()
		{
			return this._data.GetBuffer();
		}
		public void Clear()
		{
			this._data.SetLength(0L);
		}
	}
}
