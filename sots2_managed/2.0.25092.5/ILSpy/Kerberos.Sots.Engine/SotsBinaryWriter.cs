using System;
using System.IO;
using System.Text;
namespace Kerberos.Sots.Engine
{
	internal sealed class SotsBinaryWriter : BinaryWriter
	{
		public SotsBinaryWriter(Stream input, Encoding encoding) : base(input, encoding)
		{
		}
		public new void Write7BitEncodedInt(int value)
		{
			base.Write7BitEncodedInt(value);
		}
	}
}
