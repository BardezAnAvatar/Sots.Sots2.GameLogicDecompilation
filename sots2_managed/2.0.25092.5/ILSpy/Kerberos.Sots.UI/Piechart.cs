using Kerberos.Sots.Engine;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.UI
{
	internal class Piechart : PanelBinding
	{
		public void SetSlices(params PiechartSlice[] values)
		{
			List<object> list = new List<object>();
			list.Add("SetSlices");
			list.Add(base.ID);
			list.Add(values.Length);
			for (int i = 0; i < values.Length; i++)
			{
				PiechartSlice piechartSlice = values[i];
				list.Add(piechartSlice.Color);
				list.Add((float)piechartSlice.Fraction * 6.28318548f);
			}
			base.UI.Send(list.ToArray());
		}
		public Piechart(UICommChannel ui, string id) : base(ui, id)
		{
		}
	}
}
