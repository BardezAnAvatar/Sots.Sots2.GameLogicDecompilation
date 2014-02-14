using Kerberos.Sots.Engine;
using System;
using System.IO;
namespace Kerberos.Sots.ShipFramework
{
	internal class DamagePattern
	{
		public static readonly DamagePattern Empty = new DamagePattern(1, 1, 0, 0, new byte[]
		{
			255,
			255,
			255,
			255
		});
		private byte[] _data;
		private int _originX;
		private int _originY;
		private int _width;
		private int _height;
		public byte[] Data
		{
			get
			{
				return this._data;
			}
		}
		public int Width
		{
			get
			{
				return this._width;
			}
		}
		public int Height
		{
			get
			{
				return this._height;
			}
		}
		private static int GetStride(int width)
		{
			return (width + 31) / 32 * 4;
		}
		private static int GetSize(int stride, int height)
		{
			return stride * height;
		}
		public void SetValue(int x, int y, bool value)
		{
			int num = x / 8;
			int num2 = x % 8;
			int num3 = y * DamagePattern.GetStride(this._width) + num;
			byte b = this._data[num3];
			byte b2 = (byte)(1 << num2);
			if (value)
			{
				b |= b2;
			}
			else
			{
				b &= (Byte)~b2;
			}
			this._data[num3] = b;
		}
		public bool GetValue(int x, int y)
		{
			int num = x / 8;
			int num2 = x % 8;
			int num3 = y * DamagePattern.GetStride(this._width) + num;
			byte b = this._data[num3];
			return ((int)b & 1 << num2) != 0;
		}
		public int GetTotalFilled()
		{
			int num = 0;
			for (int i = 0; i < this._width; i++)
			{
				for (int j = 0; j < this._height; j++)
				{
					if (!this.GetValue(i, j))
					{
						num++;
					}
				}
			}
			return num;
		}
		public DamagePattern(int width, int height) : this(width, height, 0, 0, (byte[])null)
		{
		}
		private static byte[] GetDataFromText(int w, int h, string text)
		{
			if (text == null || w == 0 || h == 0)
			{
				return null;
			}
			int stride = DamagePattern.GetStride(w);
			int size = DamagePattern.GetSize(stride, h);
			byte[] array = new byte[size];
			int num = 0;
			for (int i = 0; i < h; i++)
			{
				for (int j = 0; j < w; j++)
				{
					if (text[num] != '0')
					{
						int num2 = i * stride + j / 8;
						byte[] expr_4B_cp_0 = array;
						int expr_4B_cp_1 = num2;
						expr_4B_cp_0[expr_4B_cp_1] |= (byte)(1 << j % 8);
					}
					num++;
				}
				num++;
			}
			return array;
		}
		public string ToDatabaseString()
		{
			string result;
			using (ScriptMessageWriter scriptMessageWriter = new ScriptMessageWriter(true, null))
			{
				this.Write(scriptMessageWriter);
				result = Convert.ToBase64String(scriptMessageWriter.GetBuffer(), 0, (int)scriptMessageWriter.GetSize(), Base64FormattingOptions.None);
			}
			return result;
		}
		public static DamagePattern FromDatabaseString(string base64String)
		{
			byte[] buffer = Convert.FromBase64String(base64String);
			DamagePattern result;
			using (MemoryStream memoryStream = new MemoryStream(buffer))
			{
				using (ScriptMessageReader scriptMessageReader = new ScriptMessageReader(true, memoryStream))
				{
					result = DamagePattern.Read(scriptMessageReader);
				}
			}
			return result;
		}
		public int GetTotalPoints()
		{
			return this._width * this._height;
		}
		public int GetNumOfType(bool value)
		{
			int num = 0;
			for (int i = 0; i < this._height; i++)
			{
				for (int j = 0; j < this._width; j++)
				{
					if (this.GetValue(j, i) == value)
					{
						num++;
					}
				}
			}
			return num;
		}
		public DamagePattern(int width, int height, int originX, int originY, string initialDataText) : this(width, height, originX, originY, DamagePattern.GetDataFromText(width, height, initialDataText))
		{
		}
		public DamagePattern(int width, int height, int originX, int originY, byte[] initialData)
		{
			this._data = initialData;
			this._width = width;
			this._height = height;
			this._originX = originX;
			this._originY = originY;
			if (this._data == null)
			{
				int stride = DamagePattern.GetStride(this._width);
				int size = DamagePattern.GetSize(stride, this._height);
				this._data = new byte[size];
			}
		}
		public static void FourCCToBytes(uint value, out byte b1, out byte b2, out byte b3, out byte b4)
		{
			b1 = (byte)(value & 255u);
			b2 = (byte)((value & 65280u) >> 8);
			b3 = (byte)((value & 16711680u) >> 16);
			b4 = (byte)((value & 4278190080u) >> 24);
		}
		public static uint BytesToFourCC(byte b1, byte b2, byte b3, byte b4)
		{
			return (uint)((int)b1 | (int)b2 << 8 | (int)b3 << 16 | (int)b4 << 24);
		}
		public static DamagePattern Read(ScriptMessageReader r)
		{
			int width = r.ReadInteger();
			int height = r.ReadInteger();
			int originX = r.ReadInteger();
			int originY = r.ReadInteger();
			int stride = DamagePattern.GetStride(width);
			int size = DamagePattern.GetSize(stride, height);
			byte[] array = new byte[size];
			for (int i = 0; i < size; i += 4)
			{
				DamagePattern.FourCCToBytes((uint)r.ReadInteger(), out array[i], out array[i + 1], out array[i + 2], out array[i + 3]);
			}
			return new DamagePattern(width, height, originX, originY, array);
		}
		public void Write(ScriptMessageWriter m)
		{
			m.WriteInteger(this._width);
			m.WriteInteger(this._height);
			m.WriteInteger(this._originX);
			m.WriteInteger(this._originY);
			int stride = DamagePattern.GetStride(this._width);
			int size = DamagePattern.GetSize(stride, this._height);
			for (int i = 0; i < size; i += 4)
			{
				m.WriteInteger((int)DamagePattern.BytesToFourCC(this._data[i], this._data[i + 1], this._data[i + 2], this._data[i + 3]));
			}
		}
	}
}
