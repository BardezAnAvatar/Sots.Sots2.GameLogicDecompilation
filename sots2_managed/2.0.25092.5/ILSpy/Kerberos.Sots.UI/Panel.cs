using Kerberos.Sots.Engine;
using System;
namespace Kerberos.Sots.UI
{
	internal abstract class Panel : PanelBinding, IDisposable
	{
		private bool _disposed;
		private bool _needsDispose;
		protected Panel(UICommChannel ui, string id) : this(ui, id, null)
		{
		}
		protected Panel(UICommChannel ui, string id, string createFromTemplateID) : base(ui, id)
		{
			if (createFromTemplateID != null)
			{
				if (createFromTemplateID == string.Empty)
				{
					throw new ArgumentNullException("createFromTemplateID");
				}
				base.SetID(ui.CreatePanelFromTemplate(createFromTemplateID, id));
				this._needsDispose = true;
				return;
			}
			else
			{
				if (string.IsNullOrEmpty(id))
				{
					throw new ArgumentNullException("id", "If no panel template is specified, a valid panel ID must be provided");
				}
				return;
			}
		}
		protected virtual void OnDisposing()
		{
		}
		public void Dispose()
		{
			if (!this._needsDispose)
			{
				App.Log.Warn("Tried to dispose panel '" + this.ToString() + "' that doesn't have ownership of the underlying object.", "gui");
				return;
			}
			if (this._disposed)
			{
				App.Log.Warn("Panel '" + this.ToString() + "' has already been disposed.", "gui");
				return;
			}
			try
			{
				this.OnDisposing();
			}
			finally
			{
				base.UI.DestroyPanel(base.ID);
				this._disposed = true;
			}
		}
	}
}
