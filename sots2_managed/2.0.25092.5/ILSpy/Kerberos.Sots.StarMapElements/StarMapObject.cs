using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using System;
namespace Kerberos.Sots.StarMapElements
{
	internal abstract class StarMapObject : AutoGameObject
	{
		public Vector3 Position;
		public float SensorRange;
		public void SetPosition(Vector3 value)
		{
			this.PostSetPosition(value);
			this.Position = value;
		}
		public void SetSensorRange(float value)
		{
			this.PostSetProp("SensorRange", value);
			this.SensorRange = value;
		}
		public void SetIsSelectable(bool value)
		{
			this.PostSetProp("Selectable", value);
		}
	}
}
