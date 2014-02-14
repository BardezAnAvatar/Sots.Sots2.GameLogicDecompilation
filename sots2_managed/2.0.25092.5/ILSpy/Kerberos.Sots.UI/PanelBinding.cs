using Kerberos.Sots.Engine;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.UI
{
	internal abstract class PanelBinding
	{
		[Flags]
		public enum PanelMessageTargetFlags
		{
			Self = 1,
			Recursive = 2
		}
		private readonly List<PanelBinding> _panels = new List<PanelBinding>();
		protected UICommChannel UI
		{
			get;
			private set;
		}
		public string ID
		{
			get;
			private set;
		}
		public string LocalID
		{
			get;
			private set;
		}
		protected void AddPanels(params PanelBinding[] range)
		{
			this._panels.AddRange(range);
		}
		protected internal void SetID(string value)
		{
			this.ID = value;
			int num = this.ID.LastIndexOf('.');
			if (num == -1)
			{
				this.LocalID = this.ID;
				return;
			}
			this.LocalID = this.ID.Substring(num + 1);
		}
		protected PanelBinding(UICommChannel ui, string id)
		{
			if (ui == null)
			{
				throw new ArgumentNullException("ui");
			}
			if (string.IsNullOrEmpty(id))
			{
				throw new ArgumentNullException("id");
			}
			this.UI = ui;
			this.SetID(id);
		}
		protected virtual void OnPanelMessage(string panelId, string msgType, string[] msgParams)
		{
		}
		public static bool TryPanelMessage(IEnumerable<PanelBinding> panels, string panelId, string msgType, string[] msgParams, PanelBinding.PanelMessageTargetFlags targetFlags = PanelBinding.PanelMessageTargetFlags.Self)
		{
			foreach (PanelBinding current in panels)
			{
				if (current.TryPanelMessage(panelId, msgType, msgParams, targetFlags))
				{
					return true;
				}
			}
			return false;
		}
		public bool TryPanelMessage(string panelId, string msgType, string[] msgParams, PanelBinding.PanelMessageTargetFlags targetFlags = PanelBinding.PanelMessageTargetFlags.Self)
		{
			if ((targetFlags & PanelBinding.PanelMessageTargetFlags.Self) != (PanelBinding.PanelMessageTargetFlags)0 && panelId == this.LocalID)
			{
				this.OnPanelMessage(panelId, msgType, msgParams);
				return true;
			}
			return (targetFlags & PanelBinding.PanelMessageTargetFlags.Recursive) != (PanelBinding.PanelMessageTargetFlags)0 && PanelBinding.TryPanelMessage(this._panels, panelId, msgType, msgParams, targetFlags | PanelBinding.PanelMessageTargetFlags.Self);
		}
		protected virtual void OnGameEvent(string eventName, string[] eventParams)
		{
		}
		public void TryGameEvent(string eventName, string[] eventParams)
		{
			this.OnGameEvent(eventName, eventParams);
		}
		public virtual void SetEnabled(bool value)
		{
			this.UI.SetEnabled(this.ID, value);
		}
		public void SetVisible(bool value)
		{
			this.UI.SetVisible(this.ID, value);
		}
		public override string ToString()
		{
			return this.ID + " (" + base.GetType().ToString() + ")";
		}
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}
