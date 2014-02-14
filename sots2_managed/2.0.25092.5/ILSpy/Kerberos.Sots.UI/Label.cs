using Kerberos.Sots.Engine;
using System;
namespace Kerberos.Sots.UI
{
	internal class Label : PanelBinding
	{
		public void SetText(string text)
		{
			base.UI.SetText(base.ID, text);
		}
		public Label(UICommChannel ui, string id) : base(ui, id)
		{
		}
	}
}
