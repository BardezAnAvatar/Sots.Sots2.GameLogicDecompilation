using Kerberos.Sots.Engine;
using Kerberos.Sots.ShipFramework;
using System;
namespace Kerberos.Sots.GameObjects
{
	[GameObjectType(InteropGameObjectType.IGOT_MODULE)]
	internal class Module : GameObject, IDisposable
	{
		public LogicalModuleMount _attachment;
		private Section _section;
		private bool _isAlive;
		private int _destroyedByPlayer;
		public LogicalModule _module;
		public LogicalModuleMount Attachment
		{
			get
			{
				return this._attachment;
			}
			set
			{
				if (this._attachment != null)
				{
					throw new InvalidOperationException("Cannot change a module's value once it has been set.");
				}
				this._attachment = value;
			}
		}
		public Section AttachedSection
		{
			get
			{
				return this._section;
			}
			set
			{
				this._section = value;
			}
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
		public int DestroyedByPlayer
		{
			get
			{
				return this._destroyedByPlayer;
			}
			set
			{
				this._destroyedByPlayer = value;
			}
		}
		public LogicalModule LogicalModule
		{
			get
			{
				return this._module;
			}
			set
			{
				if (this._module != null)
				{
					throw new InvalidOperationException("Cannot change a module's value once it has been set.");
				}
				this._module = value;
			}
		}
		public Module()
		{
			this._isAlive = true;
			this._destroyedByPlayer = 0;
		}
		public void Dispose()
		{
			this._attachment = null;
			this._module = null;
			this._section = null;
		}
	}
}
