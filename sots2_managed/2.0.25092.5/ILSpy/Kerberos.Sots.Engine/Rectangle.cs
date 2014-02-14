using System;
using System.Text;
namespace Kerberos.Sots.Engine
{
	internal struct Rectangle
	{
		public float X;
		public float Y;
		public float W;
		public float H;
		public Rectangle(float x, float y, float w, float h)
		{
			this.X = x;
			this.Y = y;
			this.W = w;
			this.H = h;
		}
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("{");
			stringBuilder.Append(this.X);
			stringBuilder.Append(",");
			stringBuilder.Append(this.Y);
			stringBuilder.Append(",");
			stringBuilder.Append(this.W);
			stringBuilder.Append(",");
			stringBuilder.Append(this.H);
			stringBuilder.Append("}");
			return stringBuilder.ToString();
		}
		public bool IsIntersecting(float x, float y)
		{
			return x > this.X && x < this.X + this.W && y > this.Y && y < this.Y + this.H;
		}
		public static bool operator ==(Rectangle value1, Rectangle value2)
		{
			return value1.X == value2.X && value1.Y == value2.Y && value1.W == value2.W && value1.H == value2.H;
		}
		public static bool operator !=(Rectangle value1, Rectangle value2)
		{
			return !(value1 == value2);
		}
		public override bool Equals(object obj)
		{
			bool result;
			try
			{
				result = ((Rectangle)obj == this);
			}
			catch (InvalidCastException)
			{
				result = false;
			}
			return result;
		}
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}
