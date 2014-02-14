using System;
namespace Kerberos.Sots.Engine
{
	internal class GameObjectTypeAttribute : Attribute
	{
		public InteropGameObjectType Value
		{
			get;
			set;
		}
		public GameObjectTypeAttribute(InteropGameObjectType value)
		{
			this.Value = value;
		}
	}
}
