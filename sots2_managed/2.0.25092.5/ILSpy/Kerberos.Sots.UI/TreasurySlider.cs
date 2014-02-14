using Kerberos.Sots.Engine;
using System;
namespace Kerberos.Sots.UI
{
	internal class TreasurySlider : ValueBoundSlider
	{
		public new void SetValue(int value)
		{
			base.SetValue(value);
		}
		protected override string OnFormatValueText(int value)
		{
			return base.Value.ToString("N0");
		}
		public TreasurySlider(UICommChannel ui, string id, int initialTreasury, int minTreasury, int maxTreasury) : base(ui, id)
		{
			base.Initialize(minTreasury, maxTreasury, initialTreasury);
		}
	}
}
