using Kerberos.Sots.Framework;
using System;
namespace Kerberos.Sots.GameObjects
{
	internal class FormationPatternData
	{
		private bool _isLead;
		private Ship _ship;
		private Vector3 _position;
		public bool IsLead
		{
			get
			{
				return this._isLead;
			}
			set
			{
				this._isLead = value;
			}
		}
		public Ship Ship
		{
			get
			{
				return this._ship;
			}
			set
			{
				this._ship = value;
			}
		}
		public Vector3 Position
		{
			get
			{
				return this._position;
			}
			set
			{
				this._position = value;
			}
		}
	}
}
