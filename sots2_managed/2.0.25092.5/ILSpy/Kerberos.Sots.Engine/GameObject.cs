using System;
namespace Kerberos.Sots.Engine
{
	internal abstract class GameObject : IGameObject
	{
		private GameObjectStatus _engineObjectStatus;
		public int ObjectID
		{
			get;
			set;
		}
		public GameObjectStatus ObjectStatus
		{
			get
			{
				return this.CheckStatus();
			}
		}
		public App App
		{
			get;
			set;
		}
		internal void PromoteEngineObjectStatus(GameObjectStatus value)
		{
			if (value == GameObjectStatus.Pending)
			{
				throw new ArgumentOutOfRangeException("value", "Can not revert to pending status. Create a new object instead.");
			}
			this._engineObjectStatus = value;
		}
		protected virtual GameObjectStatus OnCheckStatus()
		{
			return GameObjectStatus.Ready;
		}
		private GameObjectStatus CheckStatus()
		{
			if (this._engineObjectStatus != GameObjectStatus.Ready)
			{
				return this._engineObjectStatus;
			}
			return this.OnCheckStatus();
		}
		public virtual bool OnEngineMessage(InteropMessageID messageId, ScriptMessageReader message)
		{
			return false;
		}
	}
}
