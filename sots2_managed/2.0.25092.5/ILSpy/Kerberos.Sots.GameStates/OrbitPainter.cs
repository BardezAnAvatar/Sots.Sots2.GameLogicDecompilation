using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using System;
namespace Kerberos.Sots.GameStates
{
	[GameObjectType(InteropGameObjectType.IGOT_ORBITPAINTER)]
	internal class OrbitPainter : GameObject, IActive
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
		public void Add(Matrix orbitTransform)
		{
			base.App.PostEngineMessage(new object[]
			{
				InteropMessageID.IMID_ENGINE_ADD_ORBIT,
				base.ObjectID,
				orbitTransform.M11,
				orbitTransform.M12,
				orbitTransform.M13,
				orbitTransform.M14,
				orbitTransform.M21,
				orbitTransform.M22,
				orbitTransform.M23,
				orbitTransform.M24,
				orbitTransform.M31,
				orbitTransform.M32,
				orbitTransform.M33,
				orbitTransform.M34,
				orbitTransform.M41,
				orbitTransform.M42,
				orbitTransform.M43,
				orbitTransform.M44
			});
		}
	}
}
