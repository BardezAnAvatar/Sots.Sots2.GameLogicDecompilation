using System;
namespace Meebey.SmartIrc4net
{
	public class WhoInfo
	{
		private string f_Channel;
		private string f_Ident;
		private string f_Host;
		private string f_Server;
		private string f_Nick;
		private int f_HopCount;
		private string f_Realname;
		private bool f_IsAway;
		private bool f_IsOp;
		private bool f_IsVoice;
		private bool f_IsIrcOp;
		public string Channel
		{
			get
			{
				return this.f_Channel;
			}
		}
		public string Ident
		{
			get
			{
				return this.f_Ident;
			}
		}
		public string Host
		{
			get
			{
				return this.f_Host;
			}
		}
		public string Server
		{
			get
			{
				return this.f_Server;
			}
		}
		public string Nick
		{
			get
			{
				return this.f_Nick;
			}
		}
		public int HopCount
		{
			get
			{
				return this.f_HopCount;
			}
		}
		public string Realname
		{
			get
			{
				return this.f_Realname;
			}
		}
		public bool IsAway
		{
			get
			{
				return this.f_IsAway;
			}
		}
		public bool IsOp
		{
			get
			{
				return this.f_IsOp;
			}
		}
		public bool IsVoice
		{
			get
			{
				return this.f_IsVoice;
			}
		}
		public bool IsIrcOp
		{
			get
			{
				return this.f_IsIrcOp;
			}
		}
		private WhoInfo()
		{
		}
		public static WhoInfo Parse(IrcMessageData data)
		{
			WhoInfo whoInfo = new WhoInfo();
			whoInfo.f_Channel = data.RawMessageArray[3];
			whoInfo.f_Ident = data.RawMessageArray[4];
			whoInfo.f_Host = data.RawMessageArray[5];
			whoInfo.f_Server = data.RawMessageArray[6];
			whoInfo.f_Nick = data.RawMessageArray[7];
			whoInfo.f_Realname = string.Join(" ", data.MessageArray, 1, data.MessageArray.Length - 1);
			string s = data.MessageArray[0];
			try
			{
				int.Parse(s);
			}
			catch (FormatException)
			{
			}
			string text = data.RawMessageArray[8];
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			bool flag4 = false;
			int length = text.Length;
			for (int i = 0; i < length; i++)
			{
				char c = text[i];
				switch (c)
				{
				case '*':
					flag3 = true;
					break;
				case '+':
					flag2 = true;
					break;
				default:
					if (c != '@')
					{
						switch (c)
						{
						case 'G':
							flag4 = true;
							break;
						case 'H':
							flag4 = false;
							break;
						}
					}
					else
					{
						flag = true;
					}
					break;
				}
			}
			whoInfo.f_IsAway = flag4;
			whoInfo.f_IsOp = flag;
			whoInfo.f_IsVoice = flag2;
			whoInfo.f_IsIrcOp = flag3;
			return whoInfo;
		}
	}
}
