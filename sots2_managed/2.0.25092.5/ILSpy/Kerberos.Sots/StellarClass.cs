using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots
{
	public struct StellarClass : IComparable<StellarClass>, IEquatable<StellarClass>
	{
		private enum ParseMode
		{
			Normal,
			MinRange,
			MaxRange
		}
		public const int MinSubType = 0;
		public const int MaxSubType = 9;
		private byte _type;
		private byte _subtype;
		private byte _size;
		private static Random _rand = new Random((int)DateTime.UtcNow.Ticks);
		public static readonly StellarClass Default = new StellarClass("G2V");
		private static readonly IEnumerable<StellarSize> StellarSizeValues = Enum.GetValues(typeof(StellarSize)).Cast<StellarSize>();
		public StellarType Type
		{
			get
			{
				return (StellarType)this._type;
			}
			set
			{
				this._type = (byte)value;
			}
		}
		public int SubType
		{
			get
			{
				return (int)this._subtype;
			}
			set
			{
				if (value < 0 || value > 9)
				{
					throw new ArgumentOutOfRangeException("value", "Stellar sub-type must be 0-9.");
				}
				this._subtype = (byte)value;
			}
		}
		public StellarSize Size
		{
			get
			{
				return (StellarSize)this._size;
			}
			set
			{
				this._size = (byte)value;
			}
		}
		public static string Random(string Type = null, int? SubType = null, string Size = null)
		{
			string str = null;
			StellarType type = (StellarType)StellarClass._rand.Next(0, 7);
			StellarSize size = (StellarSize)StellarClass._rand.Next(0, 8);
			int num = StellarClass._rand.Next(0, 9);
			str = (Type ?? type.ToString()) + (SubType ?? num);
			return str + (Size ?? size.ToString());
		}
		public StellarClass(string str)
		{
			this = StellarClass.Parse(str);
		}
		public StellarClass(StellarType type, int subtype, StellarSize size)
		{
			this = default(StellarClass);
			this.Type = type;
			this.SubType = subtype;
			this.Size = size;
		}
		public static bool TryParseType(string str, out StellarType value)
		{
			return Enum.TryParse<StellarType>(str, out value);
		}
		public static bool TryParseSubType(string str, out int value)
		{
            if (!int.TryParse(str, out value))
            {
                return false;
            }
            return (value >= 0) && (value <= 9);
		}
		public static bool TryParseSize(string str, out StellarSize value)
		{
			return Enum.TryParse<StellarSize>(str, out value);
		}
		private static void Split(string str, out string type, out string subtype, out string size)
		{
			type = str.Substring(0, 1);
			if (str.Length > 1 && char.IsDigit(str[1]))
			{
				subtype = str.Substring(1, 1);
				size = ((str.Length > 2) ? str.Substring(2) : string.Empty);
				return;
			}
			subtype = string.Empty;
			size = ((str.Length > 1) ? str.Substring(1) : string.Empty);
		}
		private static StellarClass Parse(string str, StellarClass.ParseMode mode)
		{
			if (str == "Deepspace")
			{
				return new StellarClass(StellarType.MAX, 0, StellarSize.MAX);
			}
			StellarClass result;
			try
			{
				string value;
				string text;
				string value2;
				StellarClass.Split(str, out value, out text, out value2);
				switch (mode)
				{
				case StellarClass.ParseMode.Normal:
					if (string.IsNullOrEmpty(text))
					{
						text = 0.ToString();
					}
					break;
				case StellarClass.ParseMode.MinRange:
					if (string.IsNullOrEmpty(text))
					{
						text = 0.ToString();
					}
					if (string.IsNullOrEmpty(value2))
					{
						value2 = StellarClass.StellarSizeValues.First<StellarSize>().ToString();
					}
					break;
				case StellarClass.ParseMode.MaxRange:
					if (string.IsNullOrEmpty(text))
					{
						text = 9.ToString();
					}
					if (string.IsNullOrEmpty(value2))
					{
						value2 = StellarClass.StellarSizeValues.Last<StellarSize>().ToString();
					}
					break;
				default:
					throw new ArgumentOutOfRangeException("mode");
				}
				StellarType type = (StellarType)Enum.Parse(typeof(StellarType), value);
				int subtype = int.Parse(text);
				StellarSize size = (StellarSize)Enum.Parse(typeof(StellarSize), value2);
				result = new StellarClass(type, subtype, size);
			}
			catch (Exception innerException)
			{
				string message = string.Format("'{0}' is not a valid StellarClass.", str);
				throw new ArgumentOutOfRangeException(message, innerException);
			}
			return result;
		}
		public static StellarClass Parse(string str)
		{
			return StellarClass.Parse(str, StellarClass.ParseMode.Normal);
		}
		public static bool Parse(string str, out StellarClass output)
		{
			output = StellarClass.Parse(str);
			return true;
		}
		public static bool Contains(string minRange, string maxRange, StellarClass value)
		{
			StellarClass minRange2 = StellarClass.Parse(minRange, StellarClass.ParseMode.MinRange);
			StellarClass maxRange2 = StellarClass.Parse(maxRange, StellarClass.ParseMode.MaxRange);
			return StellarClass.Contains(minRange2, maxRange2, value);
		}
		public static bool Contains(StellarClass minRange, StellarClass maxRange, StellarClass value)
		{
			return value >= minRange && value <= maxRange;
		}
		public static bool operator <(StellarClass valueA, StellarClass valueB)
		{
			return StellarClass.Compare(valueA, valueB) < 0;
		}
		public static bool operator >(StellarClass valueA, StellarClass valueB)
		{
			return StellarClass.Compare(valueA, valueB) > 0;
		}
		public static bool operator <=(StellarClass valueA, StellarClass valueB)
		{
			return StellarClass.Compare(valueA, valueB) <= 0;
		}
		public static bool operator >=(StellarClass valueA, StellarClass valueB)
		{
			return StellarClass.Compare(valueA, valueB) >= 0;
		}
		public static bool operator ==(StellarClass valueA, StellarClass valueB)
		{
			return StellarClass.Compare(valueA, valueB) == 0;
		}
		public static bool operator !=(StellarClass valueA, StellarClass valueB)
		{
			return StellarClass.Compare(valueA, valueB) != 0;
		}
		public static bool operator <(string valueA, StellarClass valueB)
		{
			return StellarClass.Compare(new StellarClass(valueA), valueB) < 0;
		}
		public static bool operator >(string valueA, StellarClass valueB)
		{
			return StellarClass.Compare(new StellarClass(valueA), valueB) > 0;
		}
		public static bool operator <=(string valueA, StellarClass valueB)
		{
			return StellarClass.Compare(new StellarClass(valueA), valueB) <= 0;
		}
		public static bool operator >=(string valueA, StellarClass valueB)
		{
			return StellarClass.Compare(new StellarClass(valueA), valueB) >= 0;
		}
		public static bool operator ==(string valueA, StellarClass valueB)
		{
			return StellarClass.Compare(new StellarClass(valueA), valueB) == 0;
		}
		public static bool operator !=(string valueA, StellarClass valueB)
		{
			return StellarClass.Compare(new StellarClass(valueA), valueB) != 0;
		}
		public static bool operator <(StellarClass valueA, string valueB)
		{
			return StellarClass.Compare(valueA, new StellarClass(valueB)) < 0;
		}
		public static bool operator >(StellarClass valueA, string valueB)
		{
			return StellarClass.Compare(valueA, new StellarClass(valueB)) > 0;
		}
		public static bool operator <=(StellarClass valueA, string valueB)
		{
			return StellarClass.Compare(valueA, new StellarClass(valueB)) <= 0;
		}
		public static bool operator >=(StellarClass valueA, string valueB)
		{
			return StellarClass.Compare(valueA, new StellarClass(valueB)) >= 0;
		}
		public static bool operator ==(StellarClass valueA, string valueB)
		{
			return StellarClass.Compare(valueA, new StellarClass(valueB)) == 0;
		}
		public static bool operator !=(StellarClass valueA, string valueB)
		{
			return StellarClass.Compare(valueA, new StellarClass(valueB)) != 0;
		}
		bool IEquatable<StellarClass>.Equals(StellarClass other)
		{
			return StellarClass.Compare(this, other) == 0;
		}
		public bool Equals(StellarClass other)
		{
			return StellarClass.Compare(this, other) == 0;
		}
		public int CompareTo(StellarClass other)
		{
			return StellarClass.Compare(this, other);
		}
		public override bool Equals(object obj)
		{
			return obj != null && obj is StellarClass && StellarClass.Compare(this, (StellarClass)obj) == 0;
		}
		private static int Compare(StellarClass lhs, StellarClass rhs)
		{
			if (lhs._type != rhs._type)
			{
				return lhs._type.CompareTo(rhs._type);
			}
			if (lhs._subtype != rhs._subtype)
			{
				return lhs._subtype.CompareTo(rhs._subtype);
			}
			return lhs._size.CompareTo(rhs._size);
		}
		public override string ToString()
		{
			return string.Format("{0}{1}{2}", this.Type, this.SubType, this.Size);
		}
		public override int GetHashCode()
		{
			return (int)this._type | (int)this._subtype << 8 | (int)this._size << 16;
		}
		public string GetStellarActivity()
		{
			switch (this.Type)
			{
			case StellarType.O:
				return "7-9";
			case StellarType.B:
				return "6-8";
			case StellarType.A:
				return "5-8";
			case StellarType.F:
				return "4-8";
			case StellarType.G:
				return "3-7";
			case StellarType.K:
				return "1-5";
			case StellarType.M:
				return "0-4";
			default:
				return "0";
			}
		}
		public int GetInterference()
		{
			switch (this.Type)
			{
			case StellarType.O:
				return StellarClass._rand.Next(7, 9);
			case StellarType.B:
				return StellarClass._rand.Next(6, 8);
			case StellarType.A:
				return StellarClass._rand.Next(5, 8);
			case StellarType.F:
				return StellarClass._rand.Next(4, 8);
			case StellarType.G:
				return StellarClass._rand.Next(3, 7);
			case StellarType.K:
				return StellarClass._rand.Next(1, 5);
			case StellarType.M:
				return StellarClass._rand.Next(0, 4);
			default:
				return 0;
			}
		}
		public int GetAverageInterference()
		{
			switch (this.Type)
			{
			case StellarType.O:
				return 8;
			case StellarType.B:
				return 7;
			case StellarType.A:
				return 6;
			case StellarType.F:
				return 6;
			case StellarType.G:
				return 5;
			case StellarType.K:
				return 3;
			case StellarType.M:
				return 2;
			default:
				return 0;
			}
		}
	}
}
