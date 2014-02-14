using Kerberos.Sots.Engine;
using System;
namespace Kerberos.Sots.UI
{
	internal class PercentageSlider : ValueBoundSlider
	{
		protected override string OnFormatValueText(int value)
		{
			return base.Value.ToString() + "%";
		}
		public PercentageSlider(UICommChannel ui, string id, int initialPercentage, int minPercentage, int maxPercentage) : base(ui, id)
		{
			base.Initialize(minPercentage, maxPercentage, initialPercentage);
		}
	}
}
