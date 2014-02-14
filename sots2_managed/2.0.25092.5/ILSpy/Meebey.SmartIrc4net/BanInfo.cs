using System;
namespace Meebey.SmartIrc4net
{
	public class BanInfo
	{
		private string f_Channel;
		private string f_Mask;
		public string Channel
		{
			get
			{
				return this.f_Channel;
			}
		}
		public string Mask
		{
			get
			{
				return this.f_Mask;
			}
		}
		private BanInfo()
		{
		}
		public static BanInfo Parse(IrcMessageData data)
		{
			return new BanInfo
			{
				f_Channel = data.RawMessageArray[3],
				f_Mask = data.RawMessageArray[4]
			};
		}
	}
}
