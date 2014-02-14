using System;
namespace Kerberos.Sots.UI
{
	public class ValueChangedEventArgs : EventArgs
	{
		public double NewValue
		{
			get;
			private set;
		}
		public ValueChangedEventArgs(double newValue)
		{
			this.NewValue = newValue;
		}
	}
}
