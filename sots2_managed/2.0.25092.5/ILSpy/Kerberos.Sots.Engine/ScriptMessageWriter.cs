using Kerberos.Sots.Framework;
using Kerberos.Sots.ShipFramework;
using System;
using System.Collections;
using System.IO;
using System.Text;
namespace Kerberos.Sots.Engine
{
	internal sealed class ScriptMessageWriter : ScriptMessageIO
	{
		private readonly SotsBinaryWriter _writer;
		private bool noDebugging;
		public ScriptMessageWriter() : this(false, null)
		{
		}
		public ScriptMessageWriter(bool noDebugging, MemoryStream stream) : base(stream)
		{
			this.noDebugging = noDebugging;
			this._writer = new SotsBinaryWriter(base.Stream, Encoding.UTF8);
		}
		public override void Dispose()
		{
			base.Dispose();
			this._writer.Dispose();
		}
		public new long GetSize()
		{
			this._writer.Flush();
			return base.GetSize();
		}
		public new byte[] GetBuffer()
		{
			this._writer.Flush();
			return base.GetBuffer();
		}
		public void WriteString(string value)
		{
			this._writer.Write(value);
		}
		public void WriteSingle(float value)
		{
			this._writer.Write(value);
		}
		public void WriteInteger(int value)
		{
			this._writer.Write7BitEncodedInt(value);
		}
		public void WriteDouble(double value)
		{
			this._writer.Write(value);
		}
		private void WriteByte(char b)
		{
			this._writer.Write(b);
		}
		public void Write(object value)
		{
			Type type = value.GetType();
			if (type.IsEnum)
			{
				this.WriteInteger((int)value);
				return;
			}
			if (type == typeof(bool))
			{
				this.WriteInteger(((bool)value) ? 1 : 0);
				return;
			}
			if (type == typeof(float))
			{
				this.WriteSingle((float)value);
				return;
			}
			if (type == typeof(int))
			{
				this.WriteInteger((int)value);
				return;
			}
			if (type == typeof(double))
			{
				this.WriteDouble((double)value);
				return;
			}
			if (type == typeof(string))
			{
				this.WriteString((string)value);
				return;
			}
			if (type == typeof(Matrix))
			{
				Matrix matrix = (Matrix)value;
				this.WriteSingle(matrix.M11);
				this.WriteSingle(matrix.M12);
				this.WriteSingle(matrix.M13);
				this.WriteSingle(matrix.M14);
				this.WriteSingle(matrix.M21);
				this.WriteSingle(matrix.M22);
				this.WriteSingle(matrix.M23);
				this.WriteSingle(matrix.M24);
				this.WriteSingle(matrix.M31);
				this.WriteSingle(matrix.M32);
				this.WriteSingle(matrix.M33);
				this.WriteSingle(matrix.M34);
				this.WriteSingle(matrix.M41);
				this.WriteSingle(matrix.M42);
				this.WriteSingle(matrix.M43);
				this.WriteSingle(matrix.M44);
				return;
			}
			if (type == typeof(Vector4))
			{
				Vector4 vector = (Vector4)value;
				this.WriteSingle(vector.X);
				this.WriteSingle(vector.Y);
				this.WriteSingle(vector.Z);
				this.WriteSingle(vector.W);
				return;
			}
			if (type == typeof(Vector3))
			{
				Vector3 vector2 = (Vector3)value;
				this.WriteSingle(vector2.X);
				this.WriteSingle(vector2.Y);
				this.WriteSingle(vector2.Z);
				return;
			}
			if (type == typeof(Vector2))
			{
				Vector2 vector3 = (Vector2)value;
				this.WriteSingle(vector3.X);
				this.WriteSingle(vector3.Y);
				return;
			}
			if (type == typeof(DamagePattern))
			{
				((DamagePattern)value).Write(this);
				return;
			}
			if (value is IGameObject)
			{
				this.WriteInteger((value as IGameObject).ObjectID);
				return;
			}
			throw new ArgumentException("Disallowed ScriptMessage element type: " + type);
		}
		public void Write(IEnumerable values)
		{
			foreach (object current in values)
			{
				this.Write(current);
			}
		}
	}
}
