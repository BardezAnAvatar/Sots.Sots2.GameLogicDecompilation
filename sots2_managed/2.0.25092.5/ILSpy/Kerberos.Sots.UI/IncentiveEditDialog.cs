using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.UI
{
	internal class IncentiveEditDialog : Dialog
	{
		public const string DoneButton = "btnDone";
		public const string TypeList = "lstType";
		public const string ValueEditBox = "txtValue";
		private TreatyIncentiveInfo _editedIncentive;
		private Vector3 _panelColor;
		private ValueBoundSpinner _valueSpinner;
		public static Dictionary<IncentiveType, SpinnerValueDescriptor> IncentiveTypeSpinnerDescriptors = new Dictionary<IncentiveType, SpinnerValueDescriptor>
		{

			{
				IncentiveType.ResearchPoints,
				new SpinnerValueDescriptor
				{
					min = 1.0,
					max = 100.0,
					rateOfChange = 1.0
				}
			},

			{
				IncentiveType.Savings,
				new SpinnerValueDescriptor
				{
					min = 50000.0,
					max = 999999999999.0,
					rateOfChange = 50000.0
				}
			}
		};
		public IncentiveEditDialog(App game, ref TreatyIncentiveInfo tci, Vector3 panelColor, string template = "TreatyConsequencePopup") : base(game, template)
		{
			this._editedIncentive = tci;
			this._panelColor = panelColor;
		}
		public override void Initialize()
		{
			DiplomacyUI.SyncPanelColor(this._app, base.ID, this._panelColor);
			this._app.UI.ClearItems("lstType");
			foreach (KeyValuePair<IncentiveType, string> current in TreatyEditDialog.IncentiveTypeLocMap)
			{
				this._app.UI.AddItem("lstType", string.Empty, (int)current.Key, App.Localize(current.Value));
			}
			this._app.UI.SetSelection("lstType", (int)this._editedIncentive.Type);
			this._app.UI.SetPropertyString("txtValue", "text", this._editedIncentive.IncentiveValue.ToString());
			this._valueSpinner = new ValueBoundSpinner(base.UI, "spnValue", IncentiveEditDialog.IncentiveTypeSpinnerDescriptors[this._editedIncentive.Type]);
			this._valueSpinner.ValueChanged += new ValueChangedEventHandler(this._valueSpinner_ValueChanged);
		}
		private void _valueSpinner_ValueChanged(object sender, ValueChangedEventArgs e)
		{
			this._editedIncentive.IncentiveValue = (float)e.NewValue;
			this._app.UI.SetText("txtValue", this._editedIncentive.IncentiveValue.ToString());
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (this._valueSpinner.TryPanelMessage(panelName, msgType, msgParams, PanelBinding.PanelMessageTargetFlags.Self | PanelBinding.PanelMessageTargetFlags.Recursive))
			{
				return;
			}
			if (msgType == "list_sel_changed")
			{
				if (panelName == "lstType")
				{
					this._editedIncentive.Type = (IncentiveType)int.Parse(msgParams[0]);
					this._valueSpinner.SetValueDescriptor(IncentiveEditDialog.IncentiveTypeSpinnerDescriptors[this._editedIncentive.Type]);
					this._editedIncentive.IncentiveValue = (float)this._valueSpinner.Value;
					this._app.UI.SetPropertyString("txtValue", "text", this._editedIncentive.IncentiveValue.ToString());
					return;
				}
			}
			else
			{
				if (msgType == "button_clicked")
				{
					if (panelName == "btnDone")
					{
						this._app.UI.CloseDialog(this, true);
						return;
					}
				}
				else
				{
					if (msgType == "text_changed")
					{
						float incentiveValue = 0f;
						if (float.TryParse(msgParams[0], out incentiveValue))
						{
							this._editedIncentive.IncentiveValue = incentiveValue;
						}
					}
				}
			}
		}
		public override string[] CloseDialog()
		{
			List<string> list = new List<string>();
			return list.ToArray();
		}
	}
}
