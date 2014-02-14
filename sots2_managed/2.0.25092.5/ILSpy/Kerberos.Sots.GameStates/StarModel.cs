using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using System;
namespace Kerberos.Sots.GameStates
{
	[GameObjectType(InteropGameObjectType.IGOT_STARMODEL)]
	internal class StarModel : AutoGameObject, IActive
	{
		private bool _active;
		public int StarSystemDatabaseID
		{
			get;
			set;
		}
		public float Radius
		{
			get;
			private set;
		}
		public Vector3 Position
		{
			get;
			private set;
		}
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
		public StarModel(App app, string modelName, Vector3 position, float radius, bool isInCombat) : this(app, modelName, position, radius, isInCombat, string.Empty, Vector2.Zero, Vector2.Zero, Vector3.Zero, false, string.Empty)
		{
		}
		public StarModel(App app, string modelName, Vector3 position, float radius, bool isInCombat, string impostorMaterialName, Vector2 impostorSpriteScale, Vector2 impostorRange, Vector3 impostorVertexColor, bool impostorEnabled, string name)
		{
			this.Radius = radius;
			this.Position = position;
			app.AddExistingObject(this, new object[]
			{
				modelName,
				position,
				radius,
				isInCombat,
				impostorMaterialName,
				impostorSpriteScale,
				impostorRange,
				impostorVertexColor,
				impostorEnabled,
				name
			});
		}
	}
}
