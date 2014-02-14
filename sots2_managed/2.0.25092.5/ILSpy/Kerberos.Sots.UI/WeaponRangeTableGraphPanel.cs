using Kerberos.Sots.Engine;
using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.UI
{
	internal abstract class WeaponRangeTableGraphPanel : GraphPanel
	{
		public WeaponRangeTable RangeTable
		{
			get;
			private set;
		}
		public void SetRangeTables(WeaponRangeTable primary, WeaponRangeTable comparative)
		{
			if (primary == null)
			{
				return;
			}
			List<object> list = new List<object>();
			list.Add("SetRangeTables");
			list.Add(base.ID);
			int num;
			if (comparative != null)
			{
				num = 2;
			}
			else
			{
				num = 1;
			}
			list.Add(num);
			list.AddRange(primary.EnumerateScriptMessageParams());
			if (comparative != null)
			{
				list.AddRange(comparative.EnumerateScriptMessageParams());
			}
			base.UI.Send(list.ToArray());
		}
		public WeaponRangeTableGraphPanel(UICommChannel ui, string id) : base(ui, id)
		{
		}
	}
}
