using System;
namespace Kerberos.Sots.Engine
{
	public interface IEngine
	{
		bool RenderingEnabled
		{
			get;
			set;
		}
		string Version
		{
			get;
		}
	}
}
