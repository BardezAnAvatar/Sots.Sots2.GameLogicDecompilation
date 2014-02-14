using System;
namespace Kerberos.Sots.Engine
{
	internal interface IAttachable
	{
		IGameObject Parent
		{
			get;
			set;
		}
	}
}
