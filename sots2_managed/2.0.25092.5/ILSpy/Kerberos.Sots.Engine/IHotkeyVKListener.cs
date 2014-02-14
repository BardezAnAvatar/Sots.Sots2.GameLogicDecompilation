using System;
using System.Windows.Forms;
namespace Kerberos.Sots.Engine
{
	internal interface IHotkeyVKListener
	{
		bool OnVKReported(Keys key, bool shift, bool ctrl, bool alt);
	}
}
