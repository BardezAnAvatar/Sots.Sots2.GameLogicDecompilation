using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Threading;
namespace Meebey.SmartIrc4net
{
	public class IrcClient : IrcCommands
	{
		private string _Nickname = string.Empty;
		private string[] _NicknameList;
		private int _CurrentNickname;
		private string _Realname = string.Empty;
		private string _Usermode = string.Empty;
		private int _IUsermode;
		private string _Username = string.Empty;
		private string _Password = string.Empty;
		private bool _IsAway;
		private string _CtcpVersion;
		private bool _ActiveChannelSyncing;
		private bool _PassiveChannelSyncing;
		private bool _AutoJoinOnInvite;
		private bool _AutoRejoin;
		private StringDictionary _AutoRejoinChannels = new StringDictionary();
		private bool _AutoRejoinChannelsWithKeys;
		private bool _AutoRejoinOnKick;
		private bool _AutoRelogin;
		private bool _AutoNickHandling = true;
		private bool _SupportNonRfc;
		private bool _SupportNonRfcLocked;
		private StringCollection _Motd = new StringCollection();
		private bool _MotdReceived;
		private Array _ReplyCodes = Enum.GetValues(typeof(ReplyCode));
		private StringCollection _JoinedChannels = new StringCollection();
		private Hashtable _Channels = Hashtable.Synchronized(new Hashtable(new CaseInsensitiveHashCodeProvider(), new CaseInsensitiveComparer()));
		private Hashtable _IrcUsers = Hashtable.Synchronized(new Hashtable(new CaseInsensitiveHashCodeProvider(), new CaseInsensitiveComparer()));
		private List<ChannelInfo> _ChannelList;
		private object _ChannelListSyncRoot = new object();
		private AutoResetEvent _ChannelListReceivedEvent;
		private List<WhoInfo> _WhoList;
		private object _WhoListSyncRoot = new object();
		private AutoResetEvent _WhoListReceivedEvent;
		private List<BanInfo> _BanList;
		private object _BanListSyncRoot = new object();
		private AutoResetEvent _BanListReceivedEvent;
		private static Regex _ReplyCodeRegex = new Regex("^:[^ ]+? ([0-9]{3}) .+$", RegexOptions.Compiled);
		private static Regex _PingRegex = new Regex("^PING :.*", RegexOptions.Compiled);
		private static Regex _ErrorRegex = new Regex("^ERROR :.*", RegexOptions.Compiled);
		private static Regex _ActionRegex = new Regex("^:.*? PRIVMSG (.).* :\u0001ACTION .*\u0001$", RegexOptions.Compiled);
		private static Regex _CtcpRequestRegex = new Regex("^:.*? PRIVMSG .* :\u0001.*\u0001$", RegexOptions.Compiled);
		private static Regex _MessageRegex = new Regex("^:.*? PRIVMSG (.).* :.*$", RegexOptions.Compiled);
		private static Regex _CtcpReplyRegex = new Regex("^:.*? NOTICE .* :\u0001.*\u0001$", RegexOptions.Compiled);
		private static Regex _NoticeRegex = new Regex("^:.*? NOTICE (.).* :.*$", RegexOptions.Compiled);
		private static Regex _InviteRegex = new Regex("^:.*? INVITE .* .*$", RegexOptions.Compiled);
		private static Regex _JoinRegex = new Regex("^:.*? JOIN .*$", RegexOptions.Compiled);
		private static Regex _TopicRegex = new Regex("^:.*? TOPIC .* :.*$", RegexOptions.Compiled);
		private static Regex _NickRegex = new Regex("^:.*? NICK .*$", RegexOptions.Compiled);
		private static Regex _KickRegex = new Regex("^:.*? KICK .* .*$", RegexOptions.Compiled);
		private static Regex _PartRegex = new Regex("^:.*? PART .*$", RegexOptions.Compiled);
		private static Regex _ModeRegex = new Regex("^:.*? MODE (.*) .*$", RegexOptions.Compiled);
		private static Regex _QuitRegex = new Regex("^:.*? QUIT :.*$", RegexOptions.Compiled);
		public event EventHandler OnRegistered;
		public event PingEventHandler OnPing;
		public event PongEventHandler OnPong;
		public event IrcEventHandler OnRawMessage;
		public event ErrorEventHandler OnError;
		public event IrcEventHandler OnErrorMessage;
		public event JoinEventHandler OnJoin;
		public event NamesEventHandler OnNames;
		public event ListEventHandler OnList;
		public event PartEventHandler OnPart;
		public event QuitEventHandler OnQuit;
		public event KickEventHandler OnKick;
		public event AwayEventHandler OnAway;
		public event IrcEventHandler OnUnAway;
		public event IrcEventHandler OnNowAway;
		public event InviteEventHandler OnInvite;
		public event BanEventHandler OnBan;
		public event UnbanEventHandler OnUnban;
		public event OpEventHandler OnOp;
		public event DeopEventHandler OnDeop;
		public event HalfopEventHandler OnHalfop;
		public event DehalfopEventHandler OnDehalfop;
		public event VoiceEventHandler OnVoice;
		public event DevoiceEventHandler OnDevoice;
		public event WhoEventHandler OnWho;
		public event MotdEventHandler OnMotd;
		public event TopicEventHandler OnTopic;
		public event TopicChangeEventHandler OnTopicChange;
		public event NickChangeEventHandler OnNickChange;
		public event IrcEventHandler OnModeChange;
		public event IrcEventHandler OnUserModeChange;
		public event IrcEventHandler OnChannelModeChange;
		public event IrcEventHandler OnChannelMessage;
		public event ActionEventHandler OnChannelAction;
		public event IrcEventHandler OnChannelNotice;
		public event IrcEventHandler OnChannelActiveSynced;
		public event IrcEventHandler OnChannelPassiveSynced;
		public event IrcEventHandler OnQueryMessage;
		public event ActionEventHandler OnQueryAction;
		public event IrcEventHandler OnQueryNotice;
		public event CtcpEventHandler OnCtcpRequest;
		public event CtcpEventHandler OnCtcpReply;
		public bool ActiveChannelSyncing
		{
			get
			{
				return this._ActiveChannelSyncing;
			}
			set
			{
				this._ActiveChannelSyncing = value;
			}
		}
		public bool PassiveChannelSyncing
		{
			get
			{
				return this._PassiveChannelSyncing;
			}
		}
		public string CtcpVersion
		{
			get
			{
				return this._CtcpVersion;
			}
			set
			{
				this._CtcpVersion = value;
			}
		}
		public bool AutoJoinOnInvite
		{
			get
			{
				return this._AutoJoinOnInvite;
			}
			set
			{
				this._AutoJoinOnInvite = value;
			}
		}
		public bool AutoRejoin
		{
			get
			{
				return this._AutoRejoin;
			}
			set
			{
				this._AutoRejoin = value;
			}
		}
		public bool AutoRejoinOnKick
		{
			get
			{
				return this._AutoRejoinOnKick;
			}
			set
			{
				this._AutoRejoinOnKick = value;
			}
		}
		public bool AutoRelogin
		{
			get
			{
				return this._AutoRelogin;
			}
			set
			{
				this._AutoRelogin = value;
			}
		}
		public bool AutoNickHandling
		{
			get
			{
				return this._AutoNickHandling;
			}
			set
			{
				this._AutoNickHandling = value;
			}
		}
		public bool SupportNonRfc
		{
			get
			{
				return this._SupportNonRfc;
			}
			set
			{
				if (this._SupportNonRfcLocked)
				{
					return;
				}
				this._SupportNonRfc = value;
			}
		}
		public string Nickname
		{
			get
			{
				return this._Nickname;
			}
		}
		public string[] NicknameList
		{
			get
			{
				return this._NicknameList;
			}
		}
		public string Realname
		{
			get
			{
				return this._Realname;
			}
		}
		public string Username
		{
			get
			{
				return this._Username;
			}
		}
		public string Usermode
		{
			get
			{
				return this._Usermode;
			}
		}
		public int IUsermode
		{
			get
			{
				return this._IUsermode;
			}
		}
		public bool IsAway
		{
			get
			{
				return this._IsAway;
			}
		}
		public string Password
		{
			get
			{
				return this._Password;
			}
		}
		public StringCollection JoinedChannels
		{
			get
			{
				return this._JoinedChannels;
			}
		}
		public StringCollection Motd
		{
			get
			{
				return this._Motd;
			}
		}
		public object BanListSyncRoot
		{
			get
			{
				return this._BanListSyncRoot;
			}
		}
		public IrcClient()
		{
			base.OnReadLine += new ReadLineEventHandler(this._Worker);
			base.OnDisconnected += new EventHandler(this._OnDisconnected);
			base.OnConnectionError += new EventHandler(this._OnConnectionError);
		}
		public new void Connect(string[] addresslist, int port)
		{
			this._SupportNonRfcLocked = true;
			base.Connect(addresslist, port);
		}
		public void Reconnect(bool login, bool channels)
		{
			if (channels)
			{
				this._StoreChannelsToRejoin();
			}
			base.Reconnect();
			if (login)
			{
				this._CurrentNickname = 0;
				this.Login(this._NicknameList, this.Realname, this.IUsermode, this.Username, this.Password);
			}
			if (channels)
			{
				this._RejoinChannels();
			}
		}
		public void Reconnect(bool login)
		{
			this.Reconnect(login, true);
		}
		public void Login(string[] nicklist, string realname, int usermode, string username, string password)
		{
			this._NicknameList = (string[])nicklist.Clone();
			this._Nickname = this._NicknameList[0].Replace(" ", "");
			this._Realname = realname;
			this._IUsermode = usermode;
			if (username != null && username.Length > 0)
			{
				this._Username = username.Replace(" ", "");
			}
			else
			{
				this._Username = Environment.UserName.Replace(" ", "");
			}
			if (password != null && password.Length > 0)
			{
				this._Password = password;
				base.RfcPass(this.Password, Priority.Critical);
			}
			base.RfcNick(this.Nickname, Priority.Critical);
			base.RfcUser(this.Username, this.IUsermode, this.Realname, Priority.Critical);
		}
		public void Login(string[] nicklist, string realname, int usermode, string username)
		{
			this.Login(nicklist, realname, usermode, username, "");
		}
		public void Login(string[] nicklist, string realname, int usermode)
		{
			this.Login(nicklist, realname, usermode, "", "");
		}
		public void Login(string[] nicklist, string realname)
		{
			this.Login(nicklist, realname, 0, "", "");
		}
		public void Login(string nick, string realname, int usermode, string username, string password)
		{
			this.Login(new string[]
			{
				nick,
				nick + "_",
				nick + "__"
			}, realname, usermode, username, password);
		}
		public void Login(string nick, string realname, int usermode, string username)
		{
			this.Login(new string[]
			{
				nick,
				nick + "_",
				nick + "__"
			}, realname, usermode, username, "");
		}
		public void Login(string nick, string realname, int usermode)
		{
			this.Login(new string[]
			{
				nick,
				nick + "_",
				nick + "__"
			}, realname, usermode, "", "");
		}
		public void Login(string nick, string realname)
		{
			this.Login(new string[]
			{
				nick,
				nick + "_",
				nick + "__"
			}, realname, 0, "", "");
		}
		public bool IsMe(string nickname)
		{
			return this.Nickname == nickname;
		}
		public bool IsJoined(string channelname)
		{
			return this.IsJoined(channelname, this.Nickname);
		}
		public bool IsJoined(string channelname, string nickname)
		{
			if (channelname == null)
			{
				throw new ArgumentNullException("channelname");
			}
			if (nickname == null)
			{
				throw new ArgumentNullException("nickname");
			}
			Channel channel = this.GetChannel(channelname);
			return channel != null && channel.UnsafeUsers != null && channel.UnsafeUsers.ContainsKey(nickname);
		}
		public IrcUser GetIrcUser(string nickname)
		{
			if (nickname == null)
			{
				throw new ArgumentNullException("nickname");
			}
			return (IrcUser)this._IrcUsers[nickname];
		}
		public ChannelUser GetChannelUser(string channelname, string nickname)
		{
			if (channelname == null)
			{
				throw new ArgumentNullException("channel");
			}
			if (nickname == null)
			{
				throw new ArgumentNullException("nickname");
			}
			Channel channel = this.GetChannel(channelname);
			if (channel != null)
			{
				return (ChannelUser)channel.UnsafeUsers[nickname];
			}
			return null;
		}
		public Channel GetChannel(string channelname)
		{
			if (channelname == null)
			{
				throw new ArgumentNullException("channelname");
			}
			return (Channel)this._Channels[channelname];
		}
		public string[] GetChannels()
		{
			string[] array = new string[this._Channels.Values.Count];
			int num = 0;
			foreach (Channel channel in this._Channels.Values)
			{
				array[num++] = channel.Name;
			}
			return array;
		}
		public IList<ChannelInfo> GetChannelList(string mask)
		{
			List<ChannelInfo> list = new List<ChannelInfo>();
			lock (this._ChannelListSyncRoot)
			{
				this._ChannelList = list;
				this._ChannelListReceivedEvent = new AutoResetEvent(false);
				base.RfcList(mask);
				this._ChannelListReceivedEvent.WaitOne();
				this._ChannelListReceivedEvent = null;
				this._ChannelList = null;
			}
			return list;
		}
		public IList<WhoInfo> GetWhoList(string mask)
		{
			List<WhoInfo> list = new List<WhoInfo>();
			lock (this._WhoListSyncRoot)
			{
				this._WhoList = list;
				this._WhoListReceivedEvent = new AutoResetEvent(false);
				base.RfcWho(mask);
				this._WhoListReceivedEvent.WaitOne();
				this._WhoListReceivedEvent = null;
				this._WhoList = null;
			}
			return list;
		}
		public IList<BanInfo> GetBanList(string channel)
		{
			List<BanInfo> list = new List<BanInfo>();
			lock (this._BanListSyncRoot)
			{
				this._BanList = list;
				this._BanListReceivedEvent = new AutoResetEvent(false);
				base.Ban(channel);
				this._BanListReceivedEvent.WaitOne();
				this._BanListReceivedEvent = null;
				this._BanList = null;
			}
			return list;
		}
		public IrcMessageData MessageParser(string rawline)
		{
			string nick = null;
			string ident = null;
			string host = null;
			string text = null;
			string message = null;
			string text2;
			if (rawline.Length > 0 && rawline[0] == ':')
			{
				text2 = rawline.Substring(1);
			}
			else
			{
				text2 = rawline;
			}
			string[] array = text2.Split(new char[]
			{
				' '
			});
			string text3 = array[0];
			string s = array[1];
			int num = text3.IndexOf("!");
			int num2 = text3.IndexOf("@");
			int num3 = text2.IndexOf(" :");
			if (num3 != -1)
			{
				num3++;
			}
			if (num != -1)
			{
				nick = text3.Substring(0, num);
			}
			if (num2 != -1 && num != -1)
			{
				ident = text3.Substring(num + 1, num2 - num - 1);
			}
			if (num2 != -1)
			{
				host = text3.Substring(num2 + 1);
			}
			int num4 = 0;
			ReplyCode replyCode = ReplyCode.Null;
			if (int.TryParse(s, out num4) && Enum.IsDefined(typeof(ReplyCode), num4))
			{
				replyCode = (ReplyCode)num4;
			}
			ReceiveType type = this._GetMessageType(rawline);
			if (num3 != -1)
			{
				message = text2.Substring(num3 + 1);
			}
			switch (type)
			{
			case ReceiveType.Join:
			case ReceiveType.Kick:
			case ReceiveType.Part:
			case ReceiveType.TopicChange:
			case ReceiveType.ChannelModeChange:
			case ReceiveType.ChannelMessage:
			case ReceiveType.ChannelAction:
			case ReceiveType.ChannelNotice:
				text = array[2];
				break;
			case ReceiveType.Invite:
			case ReceiveType.Who:
			case ReceiveType.Topic:
			case ReceiveType.BanList:
			case ReceiveType.ChannelMode:
				text = array[3];
				break;
			case ReceiveType.Name:
				text = array[4];
				break;
			}
			ReplyCode replyCode2 = replyCode;
			switch (replyCode2)
			{
			case ReplyCode.List:
			case ReplyCode.ListEnd:
				break;
			default:
				if (replyCode2 != ReplyCode.ErrorNoChannelModes)
				{
					goto IL_19D;
				}
				break;
			}
			text = array[3];
			IL_19D:
			if (text != null && text[0] == ':')
			{
				text = text.Substring(1);
			}
			return new IrcMessageData(this, text3, nick, ident, host, text, message, rawline, type, replyCode);
		}
		protected virtual IrcUser CreateIrcUser(string nickname)
		{
			return new IrcUser(nickname, this);
		}
		protected virtual Channel CreateChannel(string name)
		{
			if (this._SupportNonRfc)
			{
				return new NonRfcChannel(name);
			}
			return new Channel(name);
		}
		protected virtual ChannelUser CreateChannelUser(string channel, IrcUser ircUser)
		{
			if (this._SupportNonRfc)
			{
				return new NonRfcChannelUser(channel, ircUser);
			}
			return new ChannelUser(channel, ircUser);
		}
		private void _Worker(object sender, ReadLineEventArgs e)
		{
			this._HandleEvents(this.MessageParser(e.Line));
		}
		private void _OnDisconnected(object sender, EventArgs e)
		{
			if (this.AutoRejoin)
			{
				this._StoreChannelsToRejoin();
			}
			this._SyncingCleanup();
		}
		private void _OnConnectionError(object sender, EventArgs e)
		{
			try
			{
				if (base.AutoReconnect && this.AutoRelogin)
				{
					this.Login(this._NicknameList, this.Realname, this.IUsermode, this.Username, this.Password);
				}
				if (base.AutoReconnect && this.AutoRejoin)
				{
					this._RejoinChannels();
				}
			}
			catch (NotConnectedException)
			{
			}
		}
		private void _StoreChannelsToRejoin()
		{
			this._AutoRejoinChannels.Clear();
			if (this.ActiveChannelSyncing || this.PassiveChannelSyncing)
			{
				IEnumerator enumerator = this._Channels.Values.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						Channel channel = (Channel)enumerator.Current;
						if (channel.Key.Length > 0)
						{
							this._AutoRejoinChannels.Add(channel.Name, channel.Key);
							this._AutoRejoinChannelsWithKeys = true;
						}
						else
						{
							this._AutoRejoinChannels.Add(channel.Name, "nokey");
						}
					}
					return;
				}
				finally
				{
					IDisposable disposable = enumerator as IDisposable;
					if (disposable != null)
					{
						disposable.Dispose();
					}
				}
			}
			foreach (string current in this._JoinedChannels)
			{
				this._AutoRejoinChannels.Add(current, "nokey");
			}
		}
		private void _RejoinChannels()
		{
			int count = this._AutoRejoinChannels.Count;
			string[] array = new string[count];
			this._AutoRejoinChannels.Keys.CopyTo(array, 0);
			if (this._AutoRejoinChannelsWithKeys)
			{
				string[] array2 = new string[count];
				this._AutoRejoinChannels.Values.CopyTo(array2, 0);
				base.RfcJoin(array, array2, Priority.High);
			}
			else
			{
				base.RfcJoin(array, Priority.High);
			}
			this._AutoRejoinChannelsWithKeys = false;
			this._AutoRejoinChannels.Clear();
		}
		private void _SyncingCleanup()
		{
			this._JoinedChannels.Clear();
			if (this.ActiveChannelSyncing)
			{
				this._Channels.Clear();
				this._IrcUsers.Clear();
			}
			this._IsAway = false;
			this._MotdReceived = false;
			this._Motd.Clear();
		}
		private string _NextNickname()
		{
			this._CurrentNickname++;
			if (this._CurrentNickname >= this._NicknameList.Length)
			{
				this._CurrentNickname--;
			}
			return this.NicknameList[this._CurrentNickname];
		}
		private ReceiveType _GetMessageType(string rawline)
		{
			Match match = IrcClient._ReplyCodeRegex.Match(rawline);
			if (match.Success)
			{
				string value = match.Groups[1].Value;
				ReplyCode replyCode = (ReplyCode)int.Parse(value);
				if (Array.IndexOf(this._ReplyCodes, replyCode) == -1)
				{
					return ReceiveType.Unknown;
				}
				ReplyCode replyCode2 = replyCode;
				if (replyCode2 > ReplyCode.LuserMe)
				{
					switch (replyCode2)
					{
					case ReplyCode.WhoIsUser:
					case ReplyCode.WhoIsServer:
					case ReplyCode.WhoIsOperator:
					case ReplyCode.WhoIsIdle:
					case ReplyCode.EndOfWhoIs:
					case ReplyCode.WhoIsChannels:
						return ReceiveType.WhoIs;
					case ReplyCode.WhoWasUser:
						return ReceiveType.WhoWas;
					case ReplyCode.EndOfWho:
						break;
					case (ReplyCode)316:
					case (ReplyCode)320:
					case ReplyCode.UniqueOpIs:
					case (ReplyCode)326:
					case (ReplyCode)327:
					case (ReplyCode)328:
					case (ReplyCode)329:
					case (ReplyCode)330:
						goto IL_16F;
					case ReplyCode.ListStart:
					case ReplyCode.List:
					case ReplyCode.ListEnd:
						return ReceiveType.List;
					case ReplyCode.ChannelModeIs:
						return ReceiveType.ChannelMode;
					case ReplyCode.NoTopic:
					case ReplyCode.Topic:
						return ReceiveType.Topic;
					default:
						switch (replyCode2)
						{
						case ReplyCode.WhoReply:
							return ReceiveType.Who;
						case ReplyCode.NamesReply:
							break;
						default:
							switch (replyCode2)
							{
							case ReplyCode.EndOfNames:
								break;
							case ReplyCode.BanList:
							case ReplyCode.EndOfBanList:
								return ReceiveType.BanList;
							case ReplyCode.EndOfWhoWas:
								return ReceiveType.WhoWas;
							case (ReplyCode)370:
							case ReplyCode.Info:
							case (ReplyCode)373:
							case ReplyCode.EndOfInfo:
								goto IL_16F;
							case ReplyCode.Motd:
							case ReplyCode.MotdStart:
							case ReplyCode.EndOfMotd:
								return ReceiveType.Motd;
							default:
								goto IL_16F;
							}
							break;
						}
						return ReceiveType.Name;
					}
					return ReceiveType.Who;
				}
				switch (replyCode2)
				{
				case ReplyCode.Welcome:
				case ReplyCode.YourHost:
				case ReplyCode.Created:
				case ReplyCode.MyInfo:
				case ReplyCode.Bounce:
					return ReceiveType.Login;
				default:
					if (replyCode2 == ReplyCode.UserModeIs)
					{
						return ReceiveType.UserMode;
					}
					switch (replyCode2)
					{
					case ReplyCode.LuserClient:
					case ReplyCode.LuserOp:
					case ReplyCode.LuserUnknown:
					case ReplyCode.LuserChannels:
					case ReplyCode.LuserMe:
						return ReceiveType.Info;
					}
					break;
				}
				IL_16F:
				if (replyCode >= (ReplyCode)400 && replyCode <= (ReplyCode)599)
				{
					return ReceiveType.ErrorMessage;
				}
				return ReceiveType.Unknown;
			}
			else
			{
				match = IrcClient._PingRegex.Match(rawline);
				if (match.Success)
				{
					return ReceiveType.Unknown;
				}
				match = IrcClient._ErrorRegex.Match(rawline);
				if (match.Success)
				{
					return ReceiveType.Error;
				}
				match = IrcClient._ActionRegex.Match(rawline);
				if (match.Success)
				{
					string value2;
					if ((value2 = match.Groups[1].Value) != null && (value2 == "#" || value2 == "!" || value2 == "&" || value2 == "+"))
					{
						return ReceiveType.ChannelAction;
					}
					return ReceiveType.QueryAction;
				}
				else
				{
					match = IrcClient._CtcpRequestRegex.Match(rawline);
					if (match.Success)
					{
						return ReceiveType.CtcpRequest;
					}
					match = IrcClient._MessageRegex.Match(rawline);
					if (match.Success)
					{
						string value3;
						if ((value3 = match.Groups[1].Value) != null && (value3 == "#" || value3 == "!" || value3 == "&" || value3 == "+"))
						{
							return ReceiveType.ChannelMessage;
						}
						return ReceiveType.QueryMessage;
					}
					else
					{
						match = IrcClient._CtcpReplyRegex.Match(rawline);
						if (match.Success)
						{
							return ReceiveType.CtcpReply;
						}
						match = IrcClient._NoticeRegex.Match(rawline);
						if (match.Success)
						{
							string value4;
							if ((value4 = match.Groups[1].Value) != null && (value4 == "#" || value4 == "!" || value4 == "&" || value4 == "+"))
							{
								return ReceiveType.ChannelNotice;
							}
							return ReceiveType.QueryNotice;
						}
						else
						{
							match = IrcClient._InviteRegex.Match(rawline);
							if (match.Success)
							{
								return ReceiveType.Invite;
							}
							match = IrcClient._JoinRegex.Match(rawline);
							if (match.Success)
							{
								return ReceiveType.Join;
							}
							match = IrcClient._TopicRegex.Match(rawline);
							if (match.Success)
							{
								return ReceiveType.TopicChange;
							}
							match = IrcClient._NickRegex.Match(rawline);
							if (match.Success)
							{
								return ReceiveType.NickChange;
							}
							match = IrcClient._KickRegex.Match(rawline);
							if (match.Success)
							{
								return ReceiveType.Kick;
							}
							match = IrcClient._PartRegex.Match(rawline);
							if (match.Success)
							{
								return ReceiveType.Part;
							}
							match = IrcClient._ModeRegex.Match(rawline);
							if (match.Success)
							{
								if (match.Groups[1].Value == this._Nickname)
								{
									return ReceiveType.UserModeChange;
								}
								return ReceiveType.ChannelModeChange;
							}
							else
							{
								match = IrcClient._QuitRegex.Match(rawline);
								if (match.Success)
								{
									return ReceiveType.Quit;
								}
								return ReceiveType.Unknown;
							}
						}
					}
				}
			}
		}
		private void _HandleEvents(IrcMessageData ircdata)
		{
			if (this.OnRawMessage != null)
			{
				this.OnRawMessage(this, new IrcEventArgs(ircdata));
			}
			string text = ircdata.RawMessageArray[0];
			string a;
			if ((a = text) != null)
			{
				if (!(a == "PING"))
				{
					if (a == "ERROR")
					{
						this._Event_ERROR(ircdata);
					}
				}
				else
				{
					this._Event_PING(ircdata);
				}
			}
			text = ircdata.RawMessageArray[1];
			string key;
			switch (key = text)
			{
			case "PRIVMSG":
				this._Event_PRIVMSG(ircdata);
				break;
			case "NOTICE":
				this._Event_NOTICE(ircdata);
				break;
			case "JOIN":
				this._Event_JOIN(ircdata);
				break;
			case "PART":
				this._Event_PART(ircdata);
				break;
			case "KICK":
				this._Event_KICK(ircdata);
				break;
			case "QUIT":
				this._Event_QUIT(ircdata);
				break;
			case "TOPIC":
				this._Event_TOPIC(ircdata);
				break;
			case "NICK":
				this._Event_NICK(ircdata);
				break;
			case "INVITE":
				this._Event_INVITE(ircdata);
				break;
			case "MODE":
				this._Event_MODE(ircdata);
				break;
			case "PONG":
				this._Event_PONG(ircdata);
				break;
			}
			if (ircdata.ReplyCode != ReplyCode.Null)
			{
				ReplyCode replyCode = ircdata.ReplyCode;
				if (replyCode <= ReplyCode.ChannelModeIs)
				{
					if (replyCode <= ReplyCode.Away)
					{
						if (replyCode != ReplyCode.Welcome)
						{
							if (replyCode != ReplyCode.TryAgain)
							{
								if (replyCode == ReplyCode.Away)
								{
									this._Event_RPL_AWAY(ircdata);
								}
							}
							else
							{
								this._Event_RPL_TRYAGAIN(ircdata);
							}
						}
						else
						{
							this._Event_RPL_WELCOME(ircdata);
						}
					}
					else
					{
						switch (replyCode)
						{
						case ReplyCode.UnAway:
							this._Event_RPL_UNAWAY(ircdata);
							break;
						case ReplyCode.NowAway:
							this._Event_RPL_NOWAWAY(ircdata);
							break;
						default:
							if (replyCode != ReplyCode.EndOfWho)
							{
								switch (replyCode)
								{
								case ReplyCode.List:
									this._Event_RPL_LIST(ircdata);
									break;
								case ReplyCode.ListEnd:
									this._Event_RPL_LISTEND(ircdata);
									break;
								case ReplyCode.ChannelModeIs:
									this._Event_RPL_CHANNELMODEIS(ircdata);
									break;
								}
							}
							else
							{
								this._Event_RPL_ENDOFWHO(ircdata);
							}
							break;
						}
					}
				}
				else
				{
					if (replyCode <= ReplyCode.Motd)
					{
						switch (replyCode)
						{
						case ReplyCode.NoTopic:
							this._Event_RPL_NOTOPIC(ircdata);
							break;
						case ReplyCode.Topic:
							this._Event_RPL_TOPIC(ircdata);
							break;
						default:
							switch (replyCode)
							{
							case ReplyCode.WhoReply:
								this._Event_RPL_WHOREPLY(ircdata);
								break;
							case ReplyCode.NamesReply:
								this._Event_RPL_NAMREPLY(ircdata);
								break;
							default:
								switch (replyCode)
								{
								case ReplyCode.EndOfNames:
									this._Event_RPL_ENDOFNAMES(ircdata);
									break;
								case ReplyCode.BanList:
									this._Event_RPL_BANLIST(ircdata);
									break;
								case ReplyCode.EndOfBanList:
									this._Event_RPL_ENDOFBANLIST(ircdata);
									break;
								case ReplyCode.Motd:
									this._Event_RPL_MOTD(ircdata);
									break;
								}
								break;
							}
							break;
						}
					}
					else
					{
						if (replyCode != ReplyCode.EndOfMotd)
						{
							if (replyCode != ReplyCode.ErrorNicknameInUse)
							{
								if (replyCode == ReplyCode.ErrorNoChannelModes)
								{
									this._Event_ERR_NOCHANMODES(ircdata);
								}
							}
							else
							{
								this._Event_ERR_NICKNAMEINUSE(ircdata);
							}
						}
						else
						{
							this._Event_RPL_ENDOFMOTD(ircdata);
						}
					}
				}
			}
			if (ircdata.Type == ReceiveType.ErrorMessage)
			{
				this._Event_ERR(ircdata);
			}
		}
		private bool _RemoveIrcUser(string nickname)
		{
			if (this.GetIrcUser(nickname).JoinedChannels.Length == 0)
			{
				this._IrcUsers.Remove(nickname);
				return true;
			}
			return false;
		}
		private void _RemoveChannelUser(string channelname, string nickname)
		{
			Channel channel = this.GetChannel(channelname);
			channel.UnsafeUsers.Remove(nickname);
			channel.UnsafeOps.Remove(nickname);
			channel.UnsafeVoices.Remove(nickname);
			if (this.SupportNonRfc)
			{
				NonRfcChannel nonRfcChannel = (NonRfcChannel)channel;
				nonRfcChannel.UnsafeHalfops.Remove(nickname);
			}
		}
		private void _InterpretChannelMode(IrcMessageData ircdata, string mode, string parameter)
		{
			string[] array = parameter.Split(new char[]
			{
				' '
			});
			bool flag = false;
			bool flag2 = false;
			int length = mode.Length;
			Channel channel = null;
			if (this.ActiveChannelSyncing)
			{
				channel = this.GetChannel(ircdata.Channel);
			}
			IEnumerator enumerator = array.GetEnumerator();
			enumerator.MoveNext();
			int i = 0;
			while (i < length)
			{
				char c = mode[i];
				if (c <= 'b')
				{
					switch (c)
					{
					case '+':
						flag = true;
						flag2 = false;
						break;
					case ',':
						goto IL_52E;
					case '-':
						flag = false;
						flag2 = true;
						break;
					default:
					{
						if (c != 'b')
						{
							goto IL_52E;
						}
						string text = (string)enumerator.Current;
						enumerator.MoveNext();
						if (flag)
						{
							if (this.ActiveChannelSyncing)
							{
								try
								{
									channel.Bans.Add(text);
								}
								catch (ArgumentException)
								{
								}
							}
							if (this.OnBan != null)
							{
								this.OnBan(this, new BanEventArgs(ircdata, ircdata.Channel, ircdata.Nick, text));
							}
						}
						if (flag2)
						{
							if (this.ActiveChannelSyncing)
							{
								channel.Bans.Remove(text);
							}
							if (this.OnUnban != null)
							{
								this.OnUnban(this, new UnbanEventArgs(ircdata, ircdata.Channel, ircdata.Nick, text));
							}
						}
						break;
					}
					}
				}
				else
				{
					switch (c)
					{
					case 'h':
						if (this.SupportNonRfc)
						{
							string text = (string)enumerator.Current;
							enumerator.MoveNext();
							if (flag)
							{
								if (this.ActiveChannelSyncing && this.GetChannelUser(ircdata.Channel, text) != null)
								{
									try
									{
										((NonRfcChannel)channel).UnsafeHalfops.Add(text, this.GetIrcUser(text));
									}
									catch (ArgumentException)
									{
									}
									NonRfcChannelUser nonRfcChannelUser = (NonRfcChannelUser)this.GetChannelUser(ircdata.Channel, text);
									nonRfcChannelUser.IsHalfop = true;
								}
								if (this.OnHalfop != null)
								{
									this.OnHalfop(this, new HalfopEventArgs(ircdata, ircdata.Channel, ircdata.Nick, text));
								}
							}
							if (flag2)
							{
								if (this.ActiveChannelSyncing && this.GetChannelUser(ircdata.Channel, text) != null)
								{
									((NonRfcChannel)channel).UnsafeHalfops.Remove(text);
									NonRfcChannelUser nonRfcChannelUser2 = (NonRfcChannelUser)this.GetChannelUser(ircdata.Channel, text);
									nonRfcChannelUser2.IsHalfop = false;
								}
								if (this.OnDehalfop != null)
								{
									this.OnDehalfop(this, new DehalfopEventArgs(ircdata, ircdata.Channel, ircdata.Nick, text));
								}
							}
						}
						break;
					case 'i':
					case 'j':
						goto IL_52E;
					case 'k':
					{
						string text = (string)enumerator.Current;
						enumerator.MoveNext();
						if (flag && this.ActiveChannelSyncing)
						{
							channel.Key = text;
						}
						if (flag2 && this.ActiveChannelSyncing)
						{
							channel.Key = "";
						}
						break;
					}
					case 'l':
					{
						string text = (string)enumerator.Current;
						enumerator.MoveNext();
						if (flag && this.ActiveChannelSyncing)
						{
							try
							{
								channel.UserLimit = int.Parse(text);
							}
							catch (FormatException)
							{
							}
						}
						if (flag2 && this.ActiveChannelSyncing)
						{
							channel.UserLimit = 0;
						}
						break;
					}
					default:
						if (c != 'o')
						{
							if (c != 'v')
							{
								goto IL_52E;
							}
							string text = (string)enumerator.Current;
							enumerator.MoveNext();
							if (flag)
							{
								if (this.ActiveChannelSyncing && this.GetChannelUser(ircdata.Channel, text) != null)
								{
									try
									{
										channel.UnsafeVoices.Add(text, this.GetIrcUser(text));
									}
									catch (ArgumentException)
									{
									}
									ChannelUser channelUser = this.GetChannelUser(ircdata.Channel, text);
									channelUser.IsVoice = true;
								}
								if (this.OnVoice != null)
								{
									this.OnVoice(this, new VoiceEventArgs(ircdata, ircdata.Channel, ircdata.Nick, text));
								}
							}
							if (flag2)
							{
								if (this.ActiveChannelSyncing && this.GetChannelUser(ircdata.Channel, text) != null)
								{
									channel.UnsafeVoices.Remove(text);
									ChannelUser channelUser2 = this.GetChannelUser(ircdata.Channel, text);
									channelUser2.IsVoice = false;
								}
								if (this.OnDevoice != null)
								{
									this.OnDevoice(this, new DevoiceEventArgs(ircdata, ircdata.Channel, ircdata.Nick, text));
								}
							}
						}
						else
						{
							string text = (string)enumerator.Current;
							enumerator.MoveNext();
							if (flag)
							{
								if (this.ActiveChannelSyncing && this.GetChannelUser(ircdata.Channel, text) != null)
								{
									try
									{
										channel.UnsafeOps.Add(text, this.GetIrcUser(text));
									}
									catch (ArgumentException)
									{
									}
									ChannelUser channelUser3 = this.GetChannelUser(ircdata.Channel, text);
									channelUser3.IsOp = true;
								}
								if (this.OnOp != null)
								{
									this.OnOp(this, new OpEventArgs(ircdata, ircdata.Channel, ircdata.Nick, text));
								}
							}
							if (flag2)
							{
								if (this.ActiveChannelSyncing && this.GetChannelUser(ircdata.Channel, text) != null)
								{
									channel.UnsafeOps.Remove(text);
									ChannelUser channelUser4 = this.GetChannelUser(ircdata.Channel, text);
									channelUser4.IsOp = false;
								}
								if (this.OnDeop != null)
								{
									this.OnDeop(this, new DeopEventArgs(ircdata, ircdata.Channel, ircdata.Nick, text));
								}
							}
						}
						break;
					}
				}
				IL_5A3:
				i++;
				continue;
				IL_52E:
				if (flag && this.ActiveChannelSyncing && channel.Mode.IndexOf(mode[i]) == -1)
				{
					Channel expr_552 = channel;
					expr_552.Mode += mode[i];
				}
				if (flag2 && this.ActiveChannelSyncing)
				{
					channel.Mode = channel.Mode.Replace(mode[i].ToString(), string.Empty);
					goto IL_5A3;
				}
				goto IL_5A3;
			}
		}
		private void _Event_PING(IrcMessageData ircdata)
		{
			string text = ircdata.RawMessageArray[1].Substring(1);
			base.RfcPong(text, Priority.Critical);
			if (this.OnPing != null)
			{
				this.OnPing(this, new PingEventArgs(ircdata, text));
			}
		}
		private void _Event_PONG(IrcMessageData ircdata)
		{
			if (this.OnPong != null)
			{
				this.OnPong(this, new PongEventArgs(ircdata, ircdata.Irc.Lag));
			}
		}
		private void _Event_ERROR(IrcMessageData ircdata)
		{
			string message = ircdata.Message;
			if (this.OnError != null)
			{
				this.OnError(this, new ErrorEventArgs(ircdata, message));
			}
		}
		private void _Event_JOIN(IrcMessageData ircdata)
		{
			string nick = ircdata.Nick;
			string channel = ircdata.Channel;
			if (this.IsMe(nick))
			{
				this._JoinedChannels.Add(channel);
			}
			if (this.ActiveChannelSyncing)
			{
				Channel channel2;
				if (this.IsMe(nick))
				{
					channel2 = this.CreateChannel(channel);
					this._Channels.Add(channel, channel2);
					base.RfcMode(channel);
					base.RfcWho(channel);
					base.Ban(channel);
				}
				else
				{
					base.RfcWho(nick);
				}
				channel2 = this.GetChannel(channel);
				IrcUser ircUser = this.GetIrcUser(nick);
				if (ircUser == null)
				{
					ircUser = new IrcUser(nick, this);
					ircUser.Ident = ircdata.Ident;
					ircUser.Host = ircdata.Host;
					this._IrcUsers.Add(nick, ircUser);
				}
				ChannelUser value = this.CreateChannelUser(channel, ircUser);
				channel2.UnsafeUsers.Add(nick, value);
			}
			if (this.OnJoin != null)
			{
				this.OnJoin(this, new JoinEventArgs(ircdata, channel, nick));
			}
		}
		private void _Event_PART(IrcMessageData ircdata)
		{
			string nick = ircdata.Nick;
			string channel = ircdata.Channel;
			string message = ircdata.Message;
			if (this.IsMe(nick))
			{
				this._JoinedChannels.Remove(channel);
			}
			if (this.ActiveChannelSyncing)
			{
				if (this.IsMe(nick))
				{
					this._Channels.Remove(channel);
				}
				else
				{
					this._RemoveChannelUser(channel, nick);
					this._RemoveIrcUser(nick);
				}
			}
			if (this.OnPart != null)
			{
				this.OnPart(this, new PartEventArgs(ircdata, channel, nick, message));
			}
		}
		private void _Event_KICK(IrcMessageData ircdata)
		{
			string channel = ircdata.Channel;
			string nick = ircdata.Nick;
			string text = ircdata.RawMessageArray[3];
			string message = ircdata.Message;
			bool flag = this.IsMe(text);
			if (flag)
			{
				this._JoinedChannels.Remove(channel);
			}
			if (this.ActiveChannelSyncing)
			{
				if (flag)
				{
					Channel channel2 = this.GetChannel(channel);
					this._Channels.Remove(channel);
					if (this._AutoRejoinOnKick)
					{
						base.RfcJoin(channel2.Name, channel2.Key);
					}
				}
				else
				{
					this._RemoveChannelUser(channel, text);
					this._RemoveIrcUser(text);
				}
			}
			else
			{
				if (flag && this.AutoRejoinOnKick)
				{
					base.RfcJoin(channel);
				}
			}
			if (this.OnKick != null)
			{
				this.OnKick(this, new KickEventArgs(ircdata, channel, nick, text, message));
			}
		}
		private void _Event_QUIT(IrcMessageData ircdata)
		{
			string nick = ircdata.Nick;
			string message = ircdata.Message;
			if (this.ActiveChannelSyncing)
			{
				IrcUser ircUser = this.GetIrcUser(nick);
				if (ircUser != null)
				{
					string[] joinedChannels = ircUser.JoinedChannels;
					if (joinedChannels != null)
					{
						string[] array = joinedChannels;
						for (int i = 0; i < array.Length; i++)
						{
							string channelname = array[i];
							this._RemoveChannelUser(channelname, nick);
						}
						this._RemoveIrcUser(nick);
					}
				}
			}
			if (this.OnQuit != null)
			{
				this.OnQuit(this, new QuitEventArgs(ircdata, nick, message));
			}
		}
		private void _Event_PRIVMSG(IrcMessageData ircdata)
		{
			if (ircdata.Type == ReceiveType.CtcpRequest)
			{
				if (ircdata.Message.StartsWith("\u0001PING"))
				{
					if (ircdata.Message.Length > 7)
					{
						base.SendMessage(SendType.CtcpReply, ircdata.Nick, "PING " + ircdata.Message.Substring(6, ircdata.Message.Length - 7));
					}
					else
					{
						base.SendMessage(SendType.CtcpReply, ircdata.Nick, "PING");
					}
				}
				else
				{
					if (ircdata.Message.StartsWith("\u0001VERSION"))
					{
						string str;
						if (this._CtcpVersion == null)
						{
							str = base.VersionString;
						}
						else
						{
							str = this._CtcpVersion;
						}
						base.SendMessage(SendType.CtcpReply, ircdata.Nick, "VERSION " + str);
					}
					else
					{
						if (ircdata.Message.StartsWith("\u0001CLIENTINFO"))
						{
							base.SendMessage(SendType.CtcpReply, ircdata.Nick, "CLIENTINFO PING VERSION CLIENTINFO");
						}
					}
				}
			}
			switch (ircdata.Type)
			{
			case ReceiveType.ChannelMessage:
				if (this.OnChannelMessage != null)
				{
					this.OnChannelMessage(this, new IrcEventArgs(ircdata));
					return;
				}
				break;
			case ReceiveType.ChannelAction:
				if (this.OnChannelAction != null)
				{
					string actionmsg = ircdata.Message.Substring(8, ircdata.Message.Length - 9);
					this.OnChannelAction(this, new ActionEventArgs(ircdata, actionmsg));
					return;
				}
				break;
			case ReceiveType.ChannelNotice:
			case ReceiveType.QueryNotice:
			case ReceiveType.CtcpReply:
				break;
			case ReceiveType.QueryMessage:
				if (this.OnQueryMessage != null)
				{
					this.OnQueryMessage(this, new IrcEventArgs(ircdata));
					return;
				}
				break;
			case ReceiveType.QueryAction:
				if (this.OnQueryAction != null)
				{
					string actionmsg2 = ircdata.Message.Substring(8, ircdata.Message.Length - 9);
					this.OnQueryAction(this, new ActionEventArgs(ircdata, actionmsg2));
					return;
				}
				break;
			case ReceiveType.CtcpRequest:
				if (this.OnCtcpRequest != null)
				{
					int num = ircdata.Message.IndexOf(' ');
					string ctcpparam = "";
					string ctcpcmd;
					if (num != -1)
					{
						ctcpcmd = ircdata.Message.Substring(1, num - 1);
						ctcpparam = ircdata.Message.Substring(num + 1, ircdata.Message.Length - num - 2);
					}
					else
					{
						ctcpcmd = ircdata.Message.Substring(1, ircdata.Message.Length - 2);
					}
					this.OnCtcpRequest(this, new CtcpEventArgs(ircdata, ctcpcmd, ctcpparam));
				}
				break;
			default:
				return;
			}
		}
		private void _Event_NOTICE(IrcMessageData ircdata)
		{
			switch (ircdata.Type)
			{
			case ReceiveType.ChannelNotice:
				if (this.OnChannelNotice != null)
				{
					this.OnChannelNotice(this, new IrcEventArgs(ircdata));
					return;
				}
				break;
			case ReceiveType.QueryMessage:
			case ReceiveType.QueryAction:
				break;
			case ReceiveType.QueryNotice:
				if (this.OnQueryNotice != null)
				{
					this.OnQueryNotice(this, new IrcEventArgs(ircdata));
					return;
				}
				break;
			case ReceiveType.CtcpReply:
				if (this.OnCtcpReply != null)
				{
					int num = ircdata.Message.IndexOf(' ');
					string ctcpparam = "";
					string ctcpcmd;
					if (num != -1)
					{
						ctcpcmd = ircdata.Message.Substring(1, num - 1);
						ctcpparam = ircdata.Message.Substring(num + 1, ircdata.Message.Length - num - 2);
					}
					else
					{
						ctcpcmd = ircdata.Message.Substring(1, ircdata.Message.Length - 2);
					}
					this.OnCtcpReply(this, new CtcpEventArgs(ircdata, ctcpcmd, ctcpparam));
				}
				break;
			default:
				return;
			}
		}
		private void _Event_TOPIC(IrcMessageData ircdata)
		{
			string nick = ircdata.Nick;
			string channel = ircdata.Channel;
			string message = ircdata.Message;
			if (this.ActiveChannelSyncing && this.IsJoined(channel))
			{
				this.GetChannel(channel).Topic = message;
			}
			if (this.OnTopicChange != null)
			{
				this.OnTopicChange(this, new TopicChangeEventArgs(ircdata, channel, nick, message));
			}
		}
		private void _Event_NICK(IrcMessageData ircdata)
		{
			string nick = ircdata.Nick;
			string text = ircdata.RawMessageArray[2];
			if (text.StartsWith(":"))
			{
				text = text.Substring(1);
			}
			if (this.IsMe(ircdata.Nick))
			{
				this._Nickname = text;
			}
			if (this.ActiveChannelSyncing)
			{
				IrcUser ircUser = this.GetIrcUser(nick);
				if (ircUser != null)
				{
					string[] joinedChannels = ircUser.JoinedChannels;
					ircUser.Nick = text;
					this._IrcUsers.Remove(nick);
					this._IrcUsers.Add(text, ircUser);
					string[] array = joinedChannels;
					for (int i = 0; i < array.Length; i++)
					{
						string channelname = array[i];
						Channel channel = this.GetChannel(channelname);
						ChannelUser channelUser = this.GetChannelUser(channelname, nick);
						channel.UnsafeUsers.Remove(nick);
						channel.UnsafeUsers.Add(text, channelUser);
						if (channelUser.IsOp)
						{
							channel.UnsafeOps.Remove(nick);
							channel.UnsafeOps.Add(text, channelUser);
						}
						if (this.SupportNonRfc && ((NonRfcChannelUser)channelUser).IsHalfop)
						{
							NonRfcChannel nonRfcChannel = (NonRfcChannel)channel;
							nonRfcChannel.UnsafeHalfops.Remove(nick);
							nonRfcChannel.UnsafeHalfops.Add(text, channelUser);
						}
						if (channelUser.IsVoice)
						{
							channel.UnsafeVoices.Remove(nick);
							channel.UnsafeVoices.Add(text, channelUser);
						}
					}
				}
			}
			if (this.OnNickChange != null)
			{
				this.OnNickChange(this, new NickChangeEventArgs(ircdata, nick, text));
			}
		}
		private void _Event_INVITE(IrcMessageData ircdata)
		{
			string channel = ircdata.Channel;
			string nick = ircdata.Nick;
			if (this.AutoJoinOnInvite && channel.Trim() != "0")
			{
				base.RfcJoin(channel);
			}
			if (this.OnInvite != null)
			{
				this.OnInvite(this, new InviteEventArgs(ircdata, channel, nick));
			}
		}
		private void _Event_MODE(IrcMessageData ircdata)
		{
			if (this.IsMe(ircdata.RawMessageArray[2]))
			{
				this._Usermode = ircdata.RawMessageArray[3].Substring(1);
			}
			else
			{
				string mode = ircdata.RawMessageArray[3];
				string parameter = string.Join(" ", ircdata.RawMessageArray, 4, ircdata.RawMessageArray.Length - 4);
				this._InterpretChannelMode(ircdata, mode, parameter);
			}
			if (ircdata.Type == ReceiveType.UserModeChange && this.OnUserModeChange != null)
			{
				this.OnUserModeChange(this, new IrcEventArgs(ircdata));
			}
			if (ircdata.Type == ReceiveType.ChannelModeChange && this.OnChannelModeChange != null)
			{
				this.OnChannelModeChange(this, new IrcEventArgs(ircdata));
			}
			if (this.OnModeChange != null)
			{
				this.OnModeChange(this, new IrcEventArgs(ircdata));
			}
		}
		private void _Event_RPL_CHANNELMODEIS(IrcMessageData ircdata)
		{
			if (this.ActiveChannelSyncing && this.IsJoined(ircdata.Channel))
			{
				Channel channel = this.GetChannel(ircdata.Channel);
				channel.Mode = string.Empty;
				string mode = ircdata.RawMessageArray[4];
				string parameter = string.Join(" ", ircdata.RawMessageArray, 5, ircdata.RawMessageArray.Length - 5);
				this._InterpretChannelMode(ircdata, mode, parameter);
			}
		}
		private void _Event_RPL_WELCOME(IrcMessageData ircdata)
		{
			this._Nickname = ircdata.RawMessageArray[2];
			if (this.OnRegistered != null)
			{
				this.OnRegistered(this, EventArgs.Empty);
			}
		}
		private void _Event_RPL_TOPIC(IrcMessageData ircdata)
		{
			string message = ircdata.Message;
			string channel = ircdata.Channel;
			if (this.ActiveChannelSyncing && this.IsJoined(channel))
			{
				this.GetChannel(channel).Topic = message;
			}
			if (this.OnTopic != null)
			{
				this.OnTopic(this, new TopicEventArgs(ircdata, channel, message));
			}
		}
		private void _Event_RPL_NOTOPIC(IrcMessageData ircdata)
		{
			string channel = ircdata.Channel;
			if (this.ActiveChannelSyncing && this.IsJoined(channel))
			{
				this.GetChannel(channel).Topic = "";
			}
			if (this.OnTopic != null)
			{
				this.OnTopic(this, new TopicEventArgs(ircdata, channel, ""));
			}
		}
		private void _Event_RPL_NAMREPLY(IrcMessageData ircdata)
		{
			string channel = ircdata.Channel;
			string[] messageArray = ircdata.MessageArray;
			if (this.ActiveChannelSyncing && this.IsJoined(channel))
			{
				string[] array = messageArray;
				for (int i = 0; i < array.Length; i++)
				{
					string text = array[i];
					if (text.Length > 0)
					{
						bool flag = false;
						bool flag2 = false;
						bool flag3 = false;
						char c = text[0];
						string text2;
						if (c <= '+')
						{
							switch (c)
							{
							case '%':
								flag2 = true;
								text2 = text.Substring(1);
								break;
							case '&':
								text2 = text.Substring(1);
								break;
							default:
								if (c != '+')
								{
									goto IL_C3;
								}
								flag3 = true;
								text2 = text.Substring(1);
								break;
							}
						}
						else
						{
							if (c != '@')
							{
								if (c != '~')
								{
									goto IL_C3;
								}
								text2 = text.Substring(1);
							}
							else
							{
								flag = true;
								text2 = text.Substring(1);
							}
						}
						IL_C6:
						IrcUser ircUser = this.GetIrcUser(text2);
						ChannelUser channelUser = this.GetChannelUser(channel, text2);
						if (ircUser == null)
						{
							ircUser = new IrcUser(text2, this);
							this._IrcUsers.Add(text2, ircUser);
						}
						if (channelUser == null)
						{
							channelUser = this.CreateChannelUser(channel, ircUser);
							Channel channel2 = this.GetChannel(channel);
							channel2.UnsafeUsers.Add(text2, channelUser);
							if (flag)
							{
								channel2.UnsafeOps.Add(text2, channelUser);
							}
							if (this.SupportNonRfc && flag2)
							{
								((NonRfcChannel)channel2).UnsafeHalfops.Add(text2, channelUser);
							}
							if (flag3)
							{
								channel2.UnsafeVoices.Add(text2, channelUser);
							}
						}
						channelUser.IsOp = flag;
						channelUser.IsVoice = flag3;
						if (this.SupportNonRfc)
						{
							((NonRfcChannelUser)channelUser).IsHalfop = flag2;
							goto IL_187;
						}
						goto IL_187;
						IL_C3:
						text2 = text;
						goto IL_C6;
					}
					IL_187:;
				}
			}
			if (this.OnNames != null)
			{
				this.OnNames(this, new NamesEventArgs(ircdata, channel, messageArray));
			}
		}
		private void _Event_RPL_LIST(IrcMessageData ircdata)
		{
			string channel = ircdata.Channel;
			int userCount = int.Parse(ircdata.RawMessageArray[4]);
			string message = ircdata.Message;
			ChannelInfo channelInfo = null;
			if (this.OnList != null || this._ChannelList != null)
			{
				channelInfo = new ChannelInfo(channel, userCount, message);
			}
			if (this._ChannelList != null)
			{
				this._ChannelList.Add(channelInfo);
			}
			if (this.OnList != null)
			{
				this.OnList(this, new ListEventArgs(ircdata, channelInfo));
			}
		}
		private void _Event_RPL_LISTEND(IrcMessageData ircdata)
		{
			if (this._ChannelListReceivedEvent != null)
			{
				this._ChannelListReceivedEvent.Set();
			}
		}
		private void _Event_RPL_TRYAGAIN(IrcMessageData ircdata)
		{
			if (this._ChannelListReceivedEvent != null)
			{
				this._ChannelListReceivedEvent.Set();
			}
		}
		private void _Event_RPL_ENDOFNAMES(IrcMessageData ircdata)
		{
			string channelname = ircdata.RawMessageArray[3];
			if (this.ActiveChannelSyncing && this.IsJoined(channelname) && this.OnChannelPassiveSynced != null)
			{
				this.OnChannelPassiveSynced(this, new IrcEventArgs(ircdata));
			}
		}
		private void _Event_RPL_AWAY(IrcMessageData ircdata)
		{
			string text = ircdata.RawMessageArray[3];
			string message = ircdata.Message;
			if (this.ActiveChannelSyncing)
			{
				IrcUser ircUser = this.GetIrcUser(text);
				if (ircUser != null)
				{
					ircUser.IsAway = true;
				}
			}
			if (this.OnAway != null)
			{
				this.OnAway(this, new AwayEventArgs(ircdata, text, message));
			}
		}
		private void _Event_RPL_UNAWAY(IrcMessageData ircdata)
		{
			this._IsAway = false;
			if (this.OnUnAway != null)
			{
				this.OnUnAway(this, new IrcEventArgs(ircdata));
			}
		}
		private void _Event_RPL_NOWAWAY(IrcMessageData ircdata)
		{
			this._IsAway = true;
			if (this.OnNowAway != null)
			{
				this.OnNowAway(this, new IrcEventArgs(ircdata));
			}
		}
		private void _Event_RPL_WHOREPLY(IrcMessageData ircdata)
		{
			WhoInfo whoInfo = WhoInfo.Parse(ircdata);
			string channel = whoInfo.Channel;
			string nick = whoInfo.Nick;
			if (this._WhoList != null)
			{
				this._WhoList.Add(whoInfo);
			}
			if (this.ActiveChannelSyncing && this.IsJoined(channel))
			{
				IrcUser ircUser = this.GetIrcUser(nick);
				ChannelUser channelUser = this.GetChannelUser(channel, nick);
				if (ircUser != null)
				{
					ircUser.Ident = whoInfo.Ident;
					ircUser.Host = whoInfo.Host;
					ircUser.Server = whoInfo.Server;
					ircUser.Nick = whoInfo.Nick;
					ircUser.HopCount = whoInfo.HopCount;
					ircUser.Realname = whoInfo.Realname;
					ircUser.IsAway = whoInfo.IsAway;
					ircUser.IsIrcOp = whoInfo.IsIrcOp;
					char c = channel[0];
					switch (c)
					{
					case '!':
					case '#':
						break;
					case '"':
						goto IL_101;
					default:
						if (c != '&' && c != '+')
						{
							goto IL_101;
						}
						break;
					}
					if (channelUser != null)
					{
						channelUser.IsOp = whoInfo.IsOp;
						channelUser.IsVoice = whoInfo.IsVoice;
					}
				}
			}
			IL_101:
			if (this.OnWho != null)
			{
				this.OnWho(this, new WhoEventArgs(ircdata, whoInfo));
			}
		}
		private void _Event_RPL_ENDOFWHO(IrcMessageData ircdata)
		{
			if (this._WhoListReceivedEvent != null)
			{
				this._WhoListReceivedEvent.Set();
			}
		}
		private void _Event_RPL_MOTD(IrcMessageData ircdata)
		{
			if (!this._MotdReceived)
			{
				this._Motd.Add(ircdata.Message);
			}
			if (this.OnMotd != null)
			{
				this.OnMotd(this, new MotdEventArgs(ircdata, ircdata.Message));
			}
		}
		private void _Event_RPL_ENDOFMOTD(IrcMessageData ircdata)
		{
			this._MotdReceived = true;
		}
		private void _Event_RPL_BANLIST(IrcMessageData ircdata)
		{
			string channel = ircdata.Channel;
			BanInfo banInfo = BanInfo.Parse(ircdata);
			if (this._BanList != null)
			{
				this._BanList.Add(banInfo);
			}
			if (this.ActiveChannelSyncing && this.IsJoined(channel))
			{
				Channel channel2 = this.GetChannel(channel);
				if (channel2.IsSycned)
				{
					return;
				}
				channel2.Bans.Add(banInfo.Mask);
			}
		}
		private void _Event_RPL_ENDOFBANLIST(IrcMessageData ircdata)
		{
			string channel = ircdata.Channel;
			if (this._BanListReceivedEvent != null)
			{
				this._BanListReceivedEvent.Set();
			}
			if (this.ActiveChannelSyncing && this.IsJoined(channel))
			{
				Channel channel2 = this.GetChannel(channel);
				if (channel2.IsSycned)
				{
					return;
				}
				channel2.ActiveSyncStop = DateTime.Now;
				channel2.IsSycned = true;
				if (this.OnChannelActiveSynced != null)
				{
					this.OnChannelActiveSynced(this, new IrcEventArgs(ircdata));
				}
			}
		}
		private void _Event_ERR_NOCHANMODES(IrcMessageData ircdata)
		{
			string channelname = ircdata.RawMessageArray[3];
			if (this.ActiveChannelSyncing && this.IsJoined(channelname))
			{
				Channel channel = this.GetChannel(channelname);
				if (channel.IsSycned)
				{
					return;
				}
				channel.ActiveSyncStop = DateTime.Now;
				channel.IsSycned = true;
				if (this.OnChannelActiveSynced != null)
				{
					this.OnChannelActiveSynced(this, new IrcEventArgs(ircdata));
				}
			}
		}
		private void _Event_ERR(IrcMessageData ircdata)
		{
			if (this.OnErrorMessage != null)
			{
				this.OnErrorMessage(this, new IrcEventArgs(ircdata));
			}
		}
		private void _Event_ERR_NICKNAMEINUSE(IrcMessageData ircdata)
		{
			if (!this.AutoNickHandling)
			{
				return;
			}
			string newnickname;
			if (this._CurrentNickname == this.NicknameList.Length - 1)
			{
				Random random = new Random();
				int num = random.Next(999);
				if (this.Nickname.Length > 5)
				{
					newnickname = this.Nickname.Substring(0, 5) + num;
				}
				else
				{
					newnickname = this.Nickname.Substring(0, this.Nickname.Length - 1) + num;
				}
			}
			else
			{
				newnickname = this._NextNickname();
			}
			base.RfcNick(newnickname, Priority.Critical);
		}
	}
}
