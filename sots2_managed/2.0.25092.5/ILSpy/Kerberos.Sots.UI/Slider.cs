using Kerberos.Sots.Engine;
using System;
namespace Kerberos.Sots.UI
{
	internal class Slider : PanelBinding
	{
		public event ValueChangedEventHandler ValueChanged;
		public int Value
		{
			get;
			private set;
		}
		public int MinValue
		{
			get;
			private set;
		}
		public int MaxValue
		{
			get;
			private set;
		}
		protected void SetValue(int value)
		{
			value = Math.Max(Math.Min(value, this.MaxValue), this.MinValue);
			if (value != this.Value)
			{
				base.UI.SetSliderValue(base.ID, value);
				this.PostValueChanged(value);
			}
		}
		protected virtual void OnValueChanged(int newValue)
		{
		}
		private void PostValueChanged(int value)
		{
			this.Value = value;
			this.OnValueChanged(this.Value);
			if (this.ValueChanged != null)
			{
				this.ValueChanged(this, new ValueChangedEventArgs((double)this.Value));
			}
		}
		protected override void OnPanelMessage(string panelId, string msgType, string[] msgParams)
		{
			base.OnPanelMessage(panelId, msgType, msgParams);
			if (msgType == "slider_value")
			{
				this.PostValueChanged(int.Parse(msgParams[0]));
			}
		}
		protected virtual void OnInitialized()
		{
		}
		protected void Initialize(int minValue, int maxValue, int initialValue)
		{
			base.UI.InitializeSlider(base.ID, minValue, maxValue, initialValue);
			this.MinValue = minValue;
			this.MaxValue = maxValue;
			this.Value = initialValue;
			this.OnInitialized();
		}
		public Slider(UICommChannel ui, string id) : base(ui, id)
		{
		}
	}
}
