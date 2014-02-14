using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.UI
{
	internal class ConsequenceEditDialog : Dialog
	{
		public const string DoneButton = "btnDone";
		public const string TypeList = "lstType";
		public const string ValueEditBox = "txtValue";
		private TreatyConsequenceInfo _editedConsequence;
		private Vector3 _panelColor;
		private ValueBoundSpinner _valueSpinner;
		public static Dictionary<ConsequenceType, SpinnerValueDescriptor> ConsequenceTypeSpinnerDescriptors = new Dictionary<ConsequenceType, SpinnerValueDescriptor>
		{

			{
				ConsequenceType.DiplomaticPointPenalty,
				new SpinnerValueDescriptor
				{
					min = 1.0,
					max = 100.0,
					rateOfChange = 1.0
				}
			},

			{
				ConsequenceType.Fine,
				new SpinnerValueDescriptor
				{
					min = 50000.0,
					max = 999999999999.0,
					rateOfChange = 50000.0
				}
			}
		};
		public ConsequenceEditDialog(App game, ref TreatyConsequenceInfo tci, Vector3 panelColor, string template = "TreatyConsequencePopup") : base(game, template)
		{
			this._editedConsequence = tci;
			this._panelColor = panelColor;
		}
		public override void Initialize()
		{
			DiplomacyUI.SyncPanelColor(this._app, base.ID, this._panelColor);
			this._app.UI.ClearItems("lstType");
			foreach (KeyValuePair<ConsequenceType, string> current in TreatyEditDialog.ConsequenceTypeLocMap)
			{
				this._app.UI.AddItem("lstType", string.Empty, (int)current.Key, App.Localize(current.Value));
			}
			this._app.UI.SetSelection("lstType", (int)this._editedConsequence.Type);
			this._app.UI.SetPropertyString("txtValue", "text", this._editedConsequence.ConsequenceValue.ToString());
			this._valueSpinner = new ValueBoundSpinner(base.UI, "spnValue", 0.0, 1.0, 1.0, 1.0);
			this._valueSpinner.ValueChanged += new ValueChangedEventHandler(this._valueSpinner_ValueChanged);
			if (ConsequenceEditDialog.ConsequenceTypeSpinnerDescriptors.ContainsKey(this._editedConsequence.Type))
			{
				this._valueSpinner.SetValueDescriptor(ConsequenceEditDialog.ConsequenceTypeSpinnerDescriptors[this._editedConsequence.Type]);
				this._editedConsequence.ConsequenceValue = (float)this._valueSpinner.Value;
				this._app.UI.SetPropertyString("txtValue", "text", this._editedConsequence.ConsequenceValue.ToString());
				this._valueSpinner.SetVisible(true);
				this._app.UI.SetVisible("txtValue", true);
				return;
			}
			this._valueSpinner.SetVisible(false);
			this._app.UI.SetVisible("txtValue", false);
		}
		private void _valueSpinner_ValueChanged(object sender, ValueChangedEventArgs e)
		{
			this._editedConsequence.ConsequenceValue = (float)e.NewValue;
			this._app.UI.SetText("txtValue", this._editedConsequence.ConsequenceValue.ToString());
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
					this._editedConsequence.Type = (ConsequenceType)int.Parse(msgParams[0]);
					if (ConsequenceEditDialog.ConsequenceTypeSpinnerDescriptors.ContainsKey(this._editedConsequence.Type))
					{
						this._valueSpinner.SetValueDescriptor(ConsequenceEditDialog.ConsequenceTypeSpinnerDescriptors[this._editedConsequence.Type]);
						this._editedConsequence.ConsequenceValue = (float)this._valueSpinner.Value;
						this._app.UI.SetPropertyString("txtValue", "text", this._editedConsequence.ConsequenceValue.ToString());
						this._valueSpinner.SetVisible(true);
						this._app.UI.SetVisible("txtValue", true);
						return;
					}
					this._valueSpinner.SetVisible(false);
					this._app.UI.SetVisible("txtValue", false);
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
						float consequenceValue = 0f;
						if (float.TryParse(msgParams[0], out consequenceValue))
						{
							this._editedConsequence.ConsequenceValue = consequenceValue;
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
