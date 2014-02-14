using Kerberos.Sots.Engine;
using System;
namespace Kerberos.Sots.UI
{
	internal class ValueBoundSpinner : Spinner
	{
		public event ValueChangedEventHandler ValueChanged;
		public double Value
		{
			get;
			private set;
		}
		public double MinValue
		{
			get;
			private set;
		}
		public double MaxValue
		{
			get;
			private set;
		}
		public double RateOfChange
		{
			get;
			private set;
		}
		public string ValueText
		{
			get;
			private set;
		}
		public string ValuePath
		{
			get;
			private set;
		}
		private void PostValueChanged(double value)
		{
			this.Value = value;
			this.OnValueChanged(value);
			if (this.ValueChanged != null)
			{
				this.ValueChanged(this, new ValueChangedEventArgs(this.Value));
			}
			this.PostValueText(value);
		}
		public void SetValueDescriptor(SpinnerValueDescriptor svd)
		{
			this.MinValue = svd.min;
			this.MaxValue = svd.max;
			this.RateOfChange = svd.rateOfChange;
			if ((this.Value - svd.max) % svd.rateOfChange == 0.0)
			{
				this.Value = Math.Max(svd.min, Math.Min(svd.max, this.Value));
				return;
			}
			this.Value = svd.min;
		}
		public void SetValue(double value)
		{
			double num = Math.Max(Math.Min(value, this.MaxValue), this.MinValue);
			if (num != this.Value)
			{
				this.PostValueChanged(value);
			}
		}
		public void SetMin(double min)
		{
			this.MinValue = min;
		}
		public void SetMax(double max)
		{
			this.MaxValue = max;
		}
		public void SetRateOfChange(double rateOfChange)
		{
			this.RateOfChange = rateOfChange;
		}
		protected virtual string OnFormatValueText(double value)
		{
			return value.ToString();
		}
		private void PostValueText(double value)
		{
			this.ValueText = this.OnFormatValueText(value);
			base.UI.SetText(this.ValuePath, this.ValueText);
		}
		protected override void OnClick(Spinner.Direction direction)
		{
			base.OnClick(direction);
			switch (direction)
			{
			case Spinner.Direction.Up:
				this.SetValue(this.Value + this.RateOfChange);
				return;
			case Spinner.Direction.Down:
				this.SetValue(this.Value - this.RateOfChange);
				return;
			default:
				return;
			}
		}
		protected virtual void OnValueChanged(double newValue)
		{
		}
		public ValueBoundSpinner(UICommChannel ui, string id, SpinnerValueDescriptor svd) : this(ui, id, svd.min, svd.max, svd.min, svd.rateOfChange)
		{
		}
		public ValueBoundSpinner(UICommChannel ui, string id, SpinnerValueDescriptor svd, double initialValue) : this(ui, id, svd.min, svd.max, initialValue, svd.rateOfChange)
		{
		}
		public ValueBoundSpinner(UICommChannel ui, string id, double minValue, double maxValue, double initialValue, double rateOfChange = 1.0) : base(ui, id)
		{
			this.ValueText = string.Empty;
			this.ValuePath = base.UI.Path(new string[]
			{
				id,
				"parent",
				"value"
			});
			this.MinValue = minValue;
			this.MaxValue = Math.Max(minValue, maxValue);
			this.RateOfChange = rateOfChange;
			this.PostValueChanged(initialValue);
		}
	}
}
