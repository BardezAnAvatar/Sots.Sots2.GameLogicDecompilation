using System;
namespace Kerberos.Sots.Engine
{
	public interface IUIPollCommandState
	{
		bool? IsChecked
		{
			set;
		}
		bool IsEnabled
		{
			set;
		}
	}
}
