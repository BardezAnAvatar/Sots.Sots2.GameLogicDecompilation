using Kerberos.Sots.Engine;
using System;
using System.Linq;
namespace Kerberos.Sots.GameObjects
{
	[GameObjectType(InteropGameObjectType.IGOT_SKY)]
	internal class Sky : AutoGameObject, IActive
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
				if (value == this._active)
				{
					return;
				}
				this._active = value;
				this.PostSetActive(this._active);
			}
		}
		public Sky(App game, SkyUsage usage, int instance)
		{
			instance = Math.Abs(instance);
			SkyDefinition[] array = (
				from def in game.AssetDatabase.SkyDefinitions
				where def.Usage == usage
				select def).ToArray<SkyDefinition>();
			string text = "Sky";
			if (array.Length > 0)
			{
				text = array[instance % array.Length].MaterialName;
			}
			game.AddExistingObject(this, new object[]
			{
				text
			});
		}
	}
}
