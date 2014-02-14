using System;
namespace Kerberos.Sots.UI
{
	internal class TestZoneMapDialog : Dialog
	{
		public TestZoneMapDialog(App game, string template = "dialogTestZoneMap") : base(game, template)
		{
		}
		public override void Initialize()
		{
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
		}
		public override string[] CloseDialog()
		{
			return null;
		}
	}
}
