using Kerberos.Sots.Engine;
using System;
namespace Kerberos.Sots.UI
{
	internal class Spinner : PanelBinding
	{
		public enum Direction
		{
			Up,
			Down
		}
		private readonly Button _upButton;
		private readonly Button _downButton;
		public override void SetEnabled(bool value)
		{
			base.SetEnabled(value);
			this._upButton.SetEnabled(value);
			this._downButton.SetEnabled(value);
		}
		protected virtual void OnClick(Spinner.Direction direction)
		{
		}
		public Spinner(UICommChannel ui, string id) : base(ui, id)
		{
			this._upButton = new Button(base.UI, id + "_up", null);
			this._upButton.Clicked += new EventHandler(this.ButtonClicked);
			this._downButton = new Button(base.UI, id + "_down", null);
			this._downButton.Clicked += new EventHandler(this.ButtonClicked);
			base.AddPanels(new PanelBinding[]
			{
				this._upButton,
				this._downButton
			});
		}
		private void ButtonClicked(object sender, EventArgs e)
		{
			if (sender == this._upButton)
			{
				this.OnClick(Spinner.Direction.Up);
				return;
			}
			if (sender == this._downButton)
			{
				this.OnClick(Spinner.Direction.Down);
			}
		}
	}
}
