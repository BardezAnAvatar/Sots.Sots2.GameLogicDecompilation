using Kerberos.Sots.Data;
using System;
namespace Kerberos.Sots.UI
{
	internal class DialogLoaShipTransfer : Dialog
	{
		private const string PointEditBox = "LoaPointValue";
		private static string PointSlider = "LoaPointSlider";
		private static string dialogok = "loa_dialog_ok";
		private static string dialogeventtext = "event_text";
		private FleetInfo _targetFleet;
		private FleetInfo _sourceFleet;
		private ShipInfo _ship;
		private int _defaulttransfer;
		private int _numtotransfer;
		public DialogLoaShipTransfer(App app, int targetFleet, int sourceFleet, int shipid, int DefaultTransfer = 1) : base(app, "LoaCubeTransfer")
		{
			this._targetFleet = app.GameDatabase.GetFleetInfo(targetFleet);
			this._sourceFleet = app.GameDatabase.GetFleetInfo(sourceFleet);
			this._ship = app.GameDatabase.GetShipInfo(shipid, false);
			this._defaulttransfer = DefaultTransfer;
		}
		public override void Initialize()
		{
			this._app.UI.SetSliderRange(this._app.UI.Path(new string[]
			{
				base.ID,
				DialogLoaShipTransfer.PointSlider
			}), this._defaulttransfer, this._ship.LoaCubes);
			this._app.UI.SetText(this._app.UI.Path(new string[]
			{
				base.ID,
				DialogLoaShipTransfer.dialogeventtext
			}), string.Format("Transfer cubes to {0} from {1}", this._targetFleet.Name, this._sourceFleet.Name));
			this._app.UI.SetSliderValue(this._app.UI.Path(new string[]
			{
				base.ID,
				DialogLoaShipTransfer.PointSlider
			}), this._defaulttransfer);
			this._app.UI.SetText(this._app.UI.Path(new string[]
			{
				base.ID,
				"LoaPointValue"
			}), this._defaulttransfer.ToString());
			this._numtotransfer = 1;
			this.UpdateSlider();
		}
		private void UpdateSlider()
		{
			this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
			{
				base.ID,
				DialogLoaShipTransfer.PointSlider,
				"right_label"
			}), "text", this._numtotransfer.ToString());
		}
		protected override void OnUpdate()
		{
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "list_item_dblclk")
			{
				return;
			}
			if (msgType == "slider_value")
			{
				if (panelName == DialogLoaShipTransfer.PointSlider)
				{
					this._numtotransfer = (int)float.Parse(msgParams[0]);
					this._app.UI.SetText(this._app.UI.Path(new string[]
					{
						base.ID,
						"LoaPointValue"
					}), msgParams[0]);
					this.UpdateSlider();
					return;
				}
			}
			else
			{
				if (msgType == "text_changed")
				{
					int val;
					if (int.TryParse(msgParams[0], out val))
					{
						int num = Math.Max(this._defaulttransfer, Math.Min(val, this._ship.LoaCubes));
						this._app.UI.SetSliderValue(this._app.UI.Path(new string[]
						{
							base.ID,
							DialogLoaShipTransfer.PointSlider
						}), num);
						this._numtotransfer = num;
					}
					else
					{
						if (msgParams[0] == string.Empty)
						{
							int defaulttransfer = this._defaulttransfer;
							this._app.UI.SetSliderValue(this._app.UI.Path(new string[]
							{
								base.ID,
								"LoaPointValue"
							}), defaulttransfer);
							this._numtotransfer = defaulttransfer;
						}
					}
					this.UpdateSlider();
					return;
				}
				if (msgType == "list_sel_changed")
				{
					return;
				}
				if (msgType == "button_clicked")
				{
					if (panelName == DialogLoaShipTransfer.dialogok)
					{
						this._app.UI.CloseDialog(this, true);
						return;
					}
				}
				else
                {
                    bool flag1 = msgType == "dialog_closed";
				}
			}
		}
		public override string[] CloseDialog()
		{
			return new string[]
			{
				this._targetFleet.ID.ToString(),
				this._sourceFleet.ID.ToString(),
				this._ship.ID.ToString(),
				this._numtotransfer.ToString()
			};
		}
	}
}
