using Kerberos.Sots.GameStates;
using System;
namespace Kerberos.Sots.Combat
{
	internal class PlanetTarget
	{
		private StellarBody _planet;
		private bool _hasBeenVisited;
		public StellarBody Planet
		{
			get
			{
				return this._planet;
			}
		}
		public bool HasBeenVisted
		{
			get
			{
				return this._hasBeenVisited;
			}
			set
			{
				this._hasBeenVisited = value;
			}
		}
		public PlanetTarget(StellarBody planet)
		{
			this._planet = planet;
			this._hasBeenVisited = false;
		}
	}
}
