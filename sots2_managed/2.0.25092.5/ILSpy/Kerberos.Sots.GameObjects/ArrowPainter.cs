using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.GameObjects
{
	[GameObjectType(InteropGameObjectType.IGOT_ARROWPAINTER)]
	internal class ArrowPainter : GameObject, IActive, IDisposable
	{
		private bool _active;
		private APStyle _style;
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
		public APStyle Style
		{
			get
			{
				return this._style;
			}
			set
			{
				if (value == this._style)
				{
					return;
				}
				this._style = value;
				this.PostSetProp("SetStyle", new object[]
				{
					this._style
				});
			}
		}
		public void AddSection(List<Vector3> path, APStyle style, int special = 0, Vector3? Color = null)
		{
			List<object> list = new List<object>();
			list.Add(path.Count<Vector3>());
			foreach (Vector3 current in path)
			{
				list.Add(current);
			}
			list.Add((int)style);
			list.Add(special);
			Color = (Color.HasValue ? Color : new Vector3?(Vector3.Zero));
			list.Add(Color);
			this.PostSetProp("AddSection", list.ToArray());
		}
		public void ClearSections()
		{
			this.PostSetProp("ClearSections", new object[0]);
		}
		public ArrowPainter(App game)
		{
			game.AddExistingObject(this, new object[0]);
		}
		public void Dispose()
		{
			if (base.App != null)
			{
				base.App.ReleaseObject(this);
			}
		}
	}
}
