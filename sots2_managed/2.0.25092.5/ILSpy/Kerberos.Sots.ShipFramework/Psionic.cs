using Kerberos.Sots.Data.ShipFramework;
using Kerberos.Sots.Engine;
using System;
namespace Kerberos.Sots.ShipFramework
{
	[GameObjectType(InteropGameObjectType.IGOT_PSIONIC)]
	internal class Psionic : GameObject
	{
		private bool _isActive;
		private float _percentConsumed;
		public SectionEnumerations.PsionicAbility Type
		{
			get;
			set;
		}
		public bool IsActive
		{
			get
			{
				return this._isActive;
			}
		}
		public float PercentConsumed
		{
			get
			{
				return this._percentConsumed;
			}
		}
		public void Activate()
		{
			if (this._isActive)
			{
				return;
			}
			this.PostSetProp("ActivatePsionic", new object[0]);
		}
		public void Deactivate()
		{
			if (!this._isActive)
			{
				return;
			}
			this.PostSetProp("ReleasePsionic", new object[0]);
		}
		public override bool OnEngineMessage(InteropMessageID messageId, ScriptMessageReader message)
		{
			if (messageId == InteropMessageID.IMID_SCRIPT_OBJECT_SETPROP)
			{
				string a = message.ReadString();
				if (a == "Update")
				{
					this._percentConsumed = message.ReadSingle();
					return true;
				}
				if (a == "IsActive")
				{
					this._isActive = message.ReadBool();
					return true;
				}
			}
			return false;
		}
	}
}
