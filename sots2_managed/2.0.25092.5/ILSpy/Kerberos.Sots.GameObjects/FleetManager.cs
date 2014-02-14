using Kerberos.Sots.Engine;
using System;
using System.Linq;
namespace Kerberos.Sots.GameObjects
{
	[GameObjectType(InteropGameObjectType.IGOT_FLEETMANAGER)]
	internal class FleetManager : GameObject, IActive, IDisposable
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
		public FleetManager(App game)
		{
			game.AddExistingObject(this, new object[0]);
			game.UI.PanelMessage += new UIEventPanelMessage(this.UICommChannel_OnPanelMessage);
		}
		protected void UICommChannel_OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked" && panelName.StartsWith("shipbutton"))
			{
				string[] array = panelName.Split(new char[]
				{
					'|'
				});
				if (array.Count<string>() > 1)
				{
					int value = int.Parse(array[1]);
					this.PostSetProp("SelectShip", value);
				}
			}
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
