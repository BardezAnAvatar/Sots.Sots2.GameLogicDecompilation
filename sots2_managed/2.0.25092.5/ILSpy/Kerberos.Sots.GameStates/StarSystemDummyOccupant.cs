using Kerberos.Sots.Engine;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
namespace Kerberos.Sots.GameStates
{
	[GameObjectType(InteropGameObjectType.IGOT_STARSYSTEMDUMMYOCCUPANT)]
	internal class StarSystemDummyOccupant : GameObject, IDisposable, IActive
	{
		private bool _active;
		public bool Active
		{
			get
			{
				return this._active;
			}
			set
			{
				if (this._active == value)
				{
					return;
				}
				this.PostSetActive(value);
				this._active = value;
			}
		}
		public StarSystemDummyOccupant(App game, string modelName, StationType type)
		{
			game.AddExistingObject(this, new object[]
			{
				modelName,
				type.ToFlags()
			});
		}
		public void Dispose()
		{
			base.App.ReleaseObject(this);
		}
	}
}
