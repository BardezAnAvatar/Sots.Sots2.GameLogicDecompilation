using System;
using System.IO;
using System.Text;
namespace Kerberos.Sots.Engine
{
	public sealed class ScriptMessageReader : ScriptMessageIO
	{
		private readonly SotsBinaryReader _reader;
		private bool noDebugging;
		public ScriptMessageReader() : this(false, null)
		{
		}
		public ScriptMessageReader(bool noDebugging, MemoryStream stream) : base(stream)
		{
			this.noDebugging = noDebugging;
			this._reader = new SotsBinaryReader(base.Stream, Encoding.UTF8);
		}
		public override void Dispose()
		{
			base.Dispose();
			this._reader.Dispose();
		}
		public string ReadString()
		{
			return this._reader.ReadString();
		}
		public float ReadSingle()
		{
			return this._reader.ReadSingle();
		}
		public int ReadInteger()
		{
			return this._reader.Read7BitEncodedInt();
		}
		public bool ReadBool()
		{
			return this.ReadInteger() != 0;
		}
		public double ReadDouble()
		{
			return this._reader.ReadDouble();
		}
		public char ReadByte()
		{
			return (char)this._reader.ReadByte();
		}
	}
}
