using System;
namespace Kerberos.Sots.Engine
{
	public class UICommand
	{
		private class UIPollCommandState : IUIPollCommandState
		{
			internal UICommand Command
			{
				private get;
				set;
			}
			public bool? IsChecked
			{
				set
				{
					this.Command.IsChecked = value;
				}
			}
			public bool IsEnabled
			{
				set
				{
					this.Command.IsEnabled = value;
				}
			}
		}
		private bool _isEnabled = true;
		private bool? _isChecked = new bool?(false);
		private readonly Action _trigger;
		private readonly Action<IUIPollCommandState> _poll;
		public event Action<UICommand> IsEnabledChanged;
		public event Action<UICommand> IsCheckedChanged;
		public string Name
		{
			get;
			private set;
		}
		public bool IsEnabled
		{
			get
			{
				return this._isEnabled;
			}
			internal set
			{
				if (this._isEnabled == value)
				{
					return;
				}
				this._isEnabled = value;
				if (this.IsEnabledChanged != null)
				{
					this.IsEnabledChanged(this);
				}
			}
		}
		public bool? IsChecked
		{
			get
			{
				return this._isChecked;
			}
			internal set
			{
				if (this._isChecked == value)
				{
					return;
				}
				this._isChecked = value;
				if (this.IsCheckedChanged != null)
				{
					this.IsCheckedChanged(this);
				}
			}
		}
		internal void Poll()
		{
			this._poll(new UICommand.UIPollCommandState
			{
				Command = this
			});
		}
		public void Trigger()
		{
			this.Poll();
			if (this._isEnabled)
			{
				this._trigger();
			}
		}
		public UICommand(string name, Action trigger, Action<IUIPollCommandState> poll)
		{
			this.Name = name;
			this._trigger = trigger;
			this._poll = poll;
		}
	}
}
