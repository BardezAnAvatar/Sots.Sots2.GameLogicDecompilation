using Kerberos.Sots.Engine;
using System;
namespace Kerberos.Sots.UI
{
	internal class TimeSlider : ValueBoundSlider
	{
		public bool SupportsInfinity
		{
			get;
			private set;
		}
		public float MinTimeInMinutes
		{
			get;
			private set;
		}
		public float MaxTimeInMinutes
		{
			get;
			private set;
		}
		public float TimeInMinutesGranularity
		{
			get;
			private set;
		}
		public float TimeInMinutes
		{
			get;
			private set;
		}
		protected override string OnFormatValueText(int value)
		{
			if (this.TimeInMinutes == 3.40282347E+38f)
			{
				return "∞";
			}
			TimeSpan timeSpan = TimeSpan.FromMinutes((double)this.TimeInMinutes);
			return timeSpan.Minutes + ":" + timeSpan.Seconds.ToString("00");
		}
		private int GetMaxOrInfinityValue()
		{
			return (int)((this.MaxTimeInMinutes - this.MinTimeInMinutes) / this.TimeInMinutesGranularity) + (this.SupportsInfinity ? 1 : 0);
		}
		private int TimeInMinutesToValue(float value)
		{
			if (this.SupportsInfinity && value == 3.40282347E+38f)
			{
				return this.GetMaxOrInfinityValue();
			}
			return (int)((value - this.MinTimeInMinutes) / this.TimeInMinutesGranularity);
		}
		private float ValueToTimeInMinutes(int value)
		{
			if (this.SupportsInfinity && value == this.GetMaxOrInfinityValue())
			{
				return 3.40282347E+38f;
			}
			return this.TimeInMinutesGranularity * (float)value + this.MinTimeInMinutes;
		}
		protected override void OnValueChanged(int newValue)
		{
			this.TimeInMinutes = this.ValueToTimeInMinutes(newValue);
			base.OnValueChanged(newValue);
		}
		public TimeSlider(UICommChannel ui, string id, float initialTimeInMinutes, float minTimeInMinutes, float maxTimeInMinutes, float granularityInMinutes, bool supportsInfinity) : base(ui, id)
		{
			this.TimeInMinutesGranularity = granularityInMinutes;
			this.MinTimeInMinutes = minTimeInMinutes;
			this.MaxTimeInMinutes = maxTimeInMinutes;
			this.SupportsInfinity = supportsInfinity;
			base.Initialize(this.TimeInMinutesToValue(minTimeInMinutes), this.GetMaxOrInfinityValue(), this.TimeInMinutesToValue(initialTimeInMinutes));
			this.TimeInMinutes = initialTimeInMinutes;
		}
	}
}
