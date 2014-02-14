using System;
namespace Kerberos.Sots.Engine
{
	internal abstract class AutoGameObject : GameObject, IDisposable
	{
		public virtual void Dispose()
		{
			if (base.App != null)
			{
				base.App.ReleaseObject(this);
			}
		}
	}
}
