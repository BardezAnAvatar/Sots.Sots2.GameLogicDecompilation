using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.UI
{
	internal class DropDownList : PanelBinding
	{
		private readonly BidirMap<int, object> _items = new BidirMap<int, object>();
		private int _nextItemId;
		private object[] _selection = new object[0];
		public event EventHandler SelectionChanged;
		public object SelectedItem
		{
			get
			{
				if (this._selection.Length == 0)
				{
					return null;
				}
				return this._selection[0];
			}
		}
		protected virtual void OnSelectionChanged()
		{
		}
		public DropDownList(UICommChannel ui, string id) : base(ui, id)
		{
			base.UI.SetPropertyBool(base.ID, "only_user_events", true);
		}
		public bool AddItem(object item, string text)
		{
			if (item == null)
			{
				throw new ArgumentNullException("item");
			}
			if (text == null)
			{
				throw new ArgumentNullException("text");
			}
			if (this._items.Reverse.ContainsKey(item))
			{
				return false;
			}
			this._items.Insert(this._nextItemId, item);
			base.UI.AddItem(base.ID, string.Empty, this._nextItemId, text);
			this._nextItemId++;
			return true;
		}
		public int GetLastItemID()
		{
			if (this._items.Reverse.Count == 0)
			{
				return -1;
			}
			return this._items.Reverse.Last<KeyValuePair<object, int>>().Value;
		}
		public bool RemoveItem(object item)
		{
			if (item == null)
			{
				throw new ArgumentNullException("item");
			}
			int num;
			if (!this._items.Reverse.TryGetValue(item, out num))
			{
				return false;
			}
			this._items.Remove(num, item);
			base.UI.RemoveItems(base.ID, num);
			return true;
		}
		public void Clear()
		{
			this._items.Clear();
			base.UI.ClearItems(base.ID);
			this._nextItemId = 0;
		}
		public bool SetSelection(object item)
		{
			int userItemId;
			if (item == null)
			{
				this._selection = new object[0];
				userItemId = -1;
			}
			else
			{
				if (!this._items.Reverse.TryGetValue(item, out userItemId))
				{
					return false;
				}
				this._selection = new object[]
				{
					item
				};
			}
			base.UI.SetSelection(base.ID, userItemId);
			return true;
		}
		protected override void OnPanelMessage(string panelId, string msgType, string[] msgParams)
		{
			base.OnPanelMessage(panelId, msgType, msgParams);
			if (msgType == "list_sel_changed")
			{
				int key = int.Parse(msgParams[0]);
				object obj;
				if (this._items.Forward.TryGetValue(key, out obj))
				{
					this._selection = new object[]
					{
						obj
					};
				}
				else
				{
					this._selection = new object[0];
				}
				this.OnSelectionChanged();
				if (this.SelectionChanged != null)
				{
					this.SelectionChanged(this, EventArgs.Empty);
				}
			}
		}
	}
}
