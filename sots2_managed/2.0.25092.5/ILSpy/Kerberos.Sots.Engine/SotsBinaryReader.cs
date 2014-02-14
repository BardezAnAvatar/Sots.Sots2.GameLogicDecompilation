using System;
using System.IO;
using System.Text;
namespace Kerberos.Sots.Engine
{
	internal sealed class SotsBinaryReader : BinaryReader
	{
		public SotsBinaryReader(Stream input, Encoding encoding) : base(input, encoding)
		{
		}
		public new int Read7BitEncodedInt()
		{
			return base.Read7BitEncodedInt();
		}
	}
}
