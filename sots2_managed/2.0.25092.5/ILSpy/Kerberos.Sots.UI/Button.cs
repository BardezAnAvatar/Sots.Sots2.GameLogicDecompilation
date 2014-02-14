using Kerberos.Sots.Engine;
using System;
namespace Kerberos.Sots.UI
{
	internal class Button : Panel
	{
		public event EventHandler Clicked;
		protected virtual void OnClicked()
		{
		}
		public Button(UICommChannel ui, string id, string createFromTemplateID = null) : base(ui, id, createFromTemplateID)
		{
		}
		protected override void OnPanelMessage(string panelId, string msgType, string[] msgParams)
		{
			base.OnPanelMessage(panelId, msgType, msgParams);
			if (msgType == "button_clicked")
			{
				this.OnClicked();
				if (this.Clicked != null)
				{
					this.Clicked(this, EventArgs.Empty);
				}
			}
		}
	}
}
