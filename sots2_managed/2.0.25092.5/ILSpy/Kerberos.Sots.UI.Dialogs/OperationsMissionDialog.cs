using System;
namespace Kerberos.Sots.UI.Dialogs
{
	internal class OperationsMissionDialog : Dialog
	{
		public OperationsMissionDialog(App game) : base(game, "dialogOperationsSummary")
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
