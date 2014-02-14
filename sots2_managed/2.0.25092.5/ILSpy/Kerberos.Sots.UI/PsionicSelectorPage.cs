using Kerberos.Sots.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.UI
{
	internal class PsionicSelectorPage : PanelBinding
	{
		private readonly string _itemsPanelID;
		private readonly List<PsionicSelectorItem> _items = new List<PsionicSelectorItem>();
		public PsionicSelectorPage(UICommChannel ui, string id) : base(ui, id)
		{
			this._itemsPanelID = base.UI.Path(new string[]
			{
				base.ID,
				"items"
			});
		}
		public void DetachItems()
		{
			if (this._items.Count > 0)
			{
				this._items.Clear();
			}
			base.UI.Send(new object[]
			{
				"DetachItems",
				this._itemsPanelID
			});
		}
		public void ReplaceItems(IEnumerable<PsionicSelectorItem> range, bool detach = true)
		{
			if (detach)
			{
				this.DetachItems();
			}
			this._items.Clear();
			this._items.AddRange(range);
			List<object> list = new List<object>();
			list.Add("AttachItems");
			list.Add(this._itemsPanelID);
			list.Add(this._items.Count);
			list.AddRange(
				from x in this._items
				select x.ID);
			base.UI.Send(list.ToArray());
		}
	}
}
