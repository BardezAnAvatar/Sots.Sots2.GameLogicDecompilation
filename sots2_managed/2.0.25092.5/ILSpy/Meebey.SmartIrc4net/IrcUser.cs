using System;
using System.Collections.Specialized;
namespace Meebey.SmartIrc4net
{
	public class IrcUser
	{
		private IrcClient _IrcClient;
		private string _Nick;
		private string _Ident;
		private string _Host;
		private string _Realname;
		private bool _IsIrcOp;
		private bool _IsAway;
		private string _Server;
		private int _HopCount = -1;
		public string Nick
		{
			get
			{
				return this._Nick;
			}
			set
			{
				this._Nick = value;
			}
		}
		public string Ident
		{
			get
			{
				return this._Ident;
			}
			set
			{
				this._Ident = value;
			}
		}
		public string Host
		{
			get
			{
				return this._Host;
			}
			set
			{
				this._Host = value;
			}
		}
		public string Realname
		{
			get
			{
				return this._Realname;
			}
			set
			{
				this._Realname = value;
			}
		}
		public bool IsIrcOp
		{
			get
			{
				return this._IsIrcOp;
			}
			set
			{
				this._IsIrcOp = value;
			}
		}
		public bool IsAway
		{
			get
			{
				return this._IsAway;
			}
			set
			{
				this._IsAway = value;
			}
		}
		public string Server
		{
			get
			{
				return this._Server;
			}
			set
			{
				this._Server = value;
			}
		}
		public int HopCount
		{
			get
			{
				return this._HopCount;
			}
			set
			{
				this._HopCount = value;
			}
		}
		public string[] JoinedChannels
		{
			get
			{
				string[] channels = this._IrcClient.GetChannels();
				StringCollection stringCollection = new StringCollection();
				string[] array = channels;
				for (int i = 0; i < array.Length; i++)
				{
					string text = array[i];
					Channel channel = this._IrcClient.GetChannel(text);
					if (channel.UnsafeUsers.ContainsKey(this._Nick))
					{
						stringCollection.Add(text);
					}
				}
				string[] array2 = new string[stringCollection.Count];
				stringCollection.CopyTo(array2, 0);
				return array2;
			}
		}
		internal IrcUser(string nickname, IrcClient ircclient)
		{
			this._IrcClient = ircclient;
			this._Nick = nickname;
		}
	}
}
