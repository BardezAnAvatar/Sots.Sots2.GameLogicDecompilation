using System;
namespace Kerberos.Sots.Engine
{
	internal interface IGameObject
	{
		int ObjectID
		{
			get;
		}
		GameObjectStatus ObjectStatus
		{
			get;
		}
		App App
		{
			get;
		}
		bool OnEngineMessage(InteropMessageID messageId, ScriptMessageReader message);
	}
}
