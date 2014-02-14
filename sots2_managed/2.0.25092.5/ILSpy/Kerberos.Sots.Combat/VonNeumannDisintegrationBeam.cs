using Kerberos.Sots.Engine;
using System;
namespace Kerberos.Sots.Combat
{
	[GameObjectType(InteropGameObjectType.IGOT_VONDISBEAM)]
	internal class VonNeumannDisintegrationBeam : GameObject, IActive
	{
		private bool m_Finished;
		private bool m_Succeeded;
		private int m_Resources;
		private bool _active;
		public bool Finished
		{
			get
			{
				return this.m_Finished;
			}
		}
		public bool Succeeded
		{
			get
			{
				return this.m_Succeeded;
			}
		}
		public int Resources
		{
			get
			{
				return this.m_Resources;
			}
		}
		public bool Active
		{
			get
			{
				return this._active;
			}
			set
			{
				if (value == this._active)
				{
					return;
				}
				this._active = value;
				this.PostSetActive(this._active);
			}
		}
		public VonNeumannDisintegrationBeam()
		{
			this.m_Finished = false;
			this.m_Succeeded = false;
			this.m_Resources = 0;
		}
		protected override GameObjectStatus OnCheckStatus()
		{
			GameObjectStatus gameObjectStatus = base.OnCheckStatus();
			if (gameObjectStatus != GameObjectStatus.Ready)
			{
				return gameObjectStatus;
			}
			return GameObjectStatus.Ready;
		}
		public override bool OnEngineMessage(InteropMessageID messageId, ScriptMessageReader message)
		{
			if (messageId == InteropMessageID.IMID_SCRIPT_OBJECT_SETPROP)
			{
				string a = message.ReadString();
				if (a == "BeamFinished")
				{
					this.m_Finished = true;
					this.m_Succeeded = (message.ReadInteger() == 1);
					this.m_Resources = message.ReadInteger();
				}
				return true;
			}
			App.Log.Warn("Unhandled message (id=" + messageId + ").", "combat");
			return base.OnEngineMessage(messageId, message);
		}
	}
}
