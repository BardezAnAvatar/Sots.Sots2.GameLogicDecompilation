using System;
namespace Kerberos.Sots.Engine
{
	internal interface IKeyBindListener
	{
		bool OnKeyBindPressed(HotKeyManager.HotKeyActions action, string gamestates);
	}
}
