using Kerberos.Sots.Engine;
using System;
namespace Kerberos.Sots.UI
{
	internal class ValueBoundSlider : Slider
	{
		public string ValueText
		{
			get;
			private set;
		}
		public string ValuePath
		{
			get;
			set;
		}
		protected virtual string OnFormatValueText(int value)
		{
			return value.ToString();
		}
		public override void SetEnabled(bool value)
		{
			base.SetEnabled(value);
			base.UI.SetVisible(base.ID, value);
		}
		private void PostValueText(int value)
		{
			this.ValueText = this.OnFormatValueText(value);
			base.UI.SetText(this.ValuePath, this.ValueText);
		}
		protected override void OnValueChanged(int newValue)
		{
			base.OnValueChanged(newValue);
			this.PostValueText(newValue);
		}
		protected override void OnInitialized()
		{
			base.OnInitialized();
			this.OnValueChanged(base.Value);
			this.PostValueText(base.Value);
		}
		protected ValueBoundSlider(UICommChannel ui, string id) : base(ui, id)
		{
			this.ValueText = string.Empty;
			this.ValuePath = base.UI.Path(new string[]
			{
				id,
				"parent",
				"value"
			});
		}
		public ValueBoundSlider(UICommChannel ui, string id, int minValue, int maxValue, int initialValue) : this(ui, id)
		{
			base.Initialize(minValue, maxValue, initialValue);
		}
	}
}
