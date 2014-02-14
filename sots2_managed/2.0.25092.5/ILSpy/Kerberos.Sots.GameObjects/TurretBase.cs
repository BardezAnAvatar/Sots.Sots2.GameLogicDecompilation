using Kerberos.Sots.Engine;
using System;
namespace Kerberos.Sots.GameObjects
{
	[GameObjectType(InteropGameObjectType.IGOT_TURRETBASE)]
	internal class TurretBase : AutoGameObject
	{
		private Section _section;
		private Module _module;
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
		public Module AttachedModule
		{
			get
			{
				return this._module;
			}
			set
			{
				this._module = value;
			}
		}
		public TurretBase(App game, string model, string damageModel, Section section, Module module)
		{
			game.AddExistingObject(this, new object[]
			{
				model,
				damageModel
			});
			this._section = section;
			this._module = module;
		}
		public override void Dispose()
		{
			base.Dispose();
			this._module = null;
			this._section = null;
		}
	}
}
