using Kerberos.Sots.Engine;
using Kerberos.Sots.ShipFramework;
using System;
namespace Kerberos.Sots.GameObjects
{
	[GameObjectType(InteropGameObjectType.IGOT_SECTION)]
	internal class Section : GameObject, IDisposable
	{
		private bool _isAlive;
		public ShipSectionAsset ShipSectionAsset
		{
			get;
			set;
		}
		public bool IsAlive
		{
			get
			{
				return this._isAlive;
			}
			set
			{
				this._isAlive = value;
			}
		}
		public Section()
		{
			this._isAlive = true;
		}
		public void Dispose()
		{
			this.ShipSectionAsset = null;
		}
	}
}
