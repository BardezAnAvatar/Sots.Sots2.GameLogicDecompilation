using Meebey.SmartIrc4net;
using System;
using System.Collections;
using System.Text;
namespace Kerberos.Sots.IRC
{
	internal class SotsIRC
	{
		public IrcClient irc;
		private App App;
		private string _ircNick;
		private int nicknum;
		private static readonly string server = "kerberos-productions.com";
		private static readonly string channel = "#sots2";
		public SotsIRC(App app)
		{
			this.App = app;
			this.irc = new IrcClient();
			this._ircNick = "Connecting";
		}
		public void OnQueryMessage(object sender, IrcEventArgs e)
		{
		}
		public void OnError(object sender, ErrorEventArgs e)
		{
		}
		private void irc_OnErrorMessage(object sender, IrcEventArgs e)
		{
			if (e.Data.ReplyCode == ReplyCode.ErrorNicknameInUse)
			{
				this.irc.RfcNick(this._ircNick + this.nicknum.ToString());
				this.nicknum++;
			}
		}
		public void OnRawMessage(object sender, IrcEventArgs e)
		{
		}
		public void OnChannelMessage(object sender, IrcEventArgs e)
		{
			if (e.Data != null && e.Data.Message != null)
			{
				ChannelUser channelUser = this.irc.GetChannelUser(SotsIRC.channel, e.Data.Nick);
				if (channelUser != null)
				{
					string text = channelUser.IsOp ? ("[b][" + e.Data.Nick + "]") : e.Data.Nick;
					text = (string.IsNullOrEmpty(text) ? "?" : text);
					this.App.Network.PostIRCChatMessage(text, e.Data.Message);
				}
			}
		}
		public void OnNickChanged(object sender, IrcEventArgs e)
		{
			this._ircNick = this.irc.Nickname;
			this.App.Network.PostIRCNick(this.irc.Nickname);
		}
		public void SetupIRCClient(string name)
		{
			if (this.irc.IsConnected)
			{
				this.SetNick(name);
				return;
			}
			this._SetupIRCClient(name);
		}
		private void _SetupIRCClient(string name)
		{
			if (!this.App.GameSettings.JoinGlobalChat)
			{
				this.App.Network.PostIRCChatMessage("", "Set Join Global Chat in Options to connect!");
				return;
			}
			this.irc.Encoding = Encoding.UTF8;
			this.irc.SendDelay = 200;
			this.irc.ActiveChannelSyncing = true;
			this.irc.OnQueryMessage += new IrcEventHandler(this.OnQueryMessage);
			this.irc.OnError += new ErrorEventHandler(this.OnError);
			this.irc.OnRawMessage += new IrcEventHandler(this.OnRawMessage);
			this.irc.OnChannelMessage += new IrcEventHandler(this.OnChannelMessage);
			this.irc.OnNickChange += new NickChangeEventHandler(this.OnNickChanged);
			this.irc.OnErrorMessage += new IrcEventHandler(this.irc_OnErrorMessage);
			this.irc.OnWho += new WhoEventHandler(this.irc_OnWho);
			this.irc.OnConnected += new EventHandler(this.irc_OnConnected);
			this.irc.OnConnecting += new EventHandler(this.irc_OnConnecting);
			this.irc.OnConnectionError += new EventHandler(this.irc_OnConnectionError);
			this.irc.OnMotd += new MotdEventHandler(this.irc_OnMotd);
			this.irc.OnTopic += new TopicEventHandler(this.irc_OnTopic);
			this.irc.OnDisconnected += new EventHandler(this.irc_OnDisconnected);
			string[] addresslist = new string[]
			{
				SotsIRC.server
			};
			int port = 6667;
			try
			{
				this.irc.Connect(addresslist, port);
			}
			catch (ConnectionException ex)
			{
				System.Console.WriteLine("couldn't connect! Reason: " + ex.Message);
			}
			if (this.irc.IsConnected)
			{
				this._ircNick = name.Replace(" ", "_");
				this.App.Network.PostIRCNick(this._ircNick);
				this.irc.Login(name, "Sots Client");
				this.irc.RfcJoin(SotsIRC.channel);
				return;
			}
			this.irc_OnDisconnected(null, null);
		}
		private void irc_OnDisconnected(object sender, EventArgs e)
		{
			this.irc.OnQueryMessage -= new IrcEventHandler(this.OnQueryMessage);
			this.irc.OnError -= new ErrorEventHandler(this.OnError);
			this.irc.OnRawMessage -= new IrcEventHandler(this.OnRawMessage);
			this.irc.OnChannelMessage -= new IrcEventHandler(this.OnChannelMessage);
			this.irc.OnNickChange -= new NickChangeEventHandler(this.OnNickChanged);
			this.irc.OnErrorMessage -= new IrcEventHandler(this.irc_OnErrorMessage);
			this.irc.OnWho -= new WhoEventHandler(this.irc_OnWho);
			this.irc.OnConnected -= new EventHandler(this.irc_OnConnected);
			this.irc.OnConnecting -= new EventHandler(this.irc_OnConnecting);
			this.irc.OnConnectionError -= new EventHandler(this.irc_OnConnectionError);
			this.irc.OnMotd -= new MotdEventHandler(this.irc_OnMotd);
			this.irc.OnTopic -= new TopicEventHandler(this.irc_OnTopic);
			this.irc.OnDisconnected -= new EventHandler(this.irc_OnDisconnected);
		}
		private void irc_OnTopic(object sender, TopicEventArgs e)
		{
		}
		private void irc_OnMotd(object sender, MotdEventArgs e)
		{
			if (e.Data != null && e.Data.Message != null)
			{
				ChannelUser channelUser = this.irc.GetChannelUser(SotsIRC.channel, e.Data.Nick);
				if (channelUser != null)
				{
					string text = "[b][MOTD:]";
					text = (string.IsNullOrEmpty(text) ? "?" : text);
					this.App.Network.PostIRCChatMessage(text, "[b][" + e.Data.Message + "]");
				}
			}
		}
		private void irc_OnConnectionError(object sender, EventArgs e)
		{
			this.App.Network.PostIRCChatMessage("", "*** ERROR CONNECTING TO CHAT SERVER!");
			this.irc.Disconnect();
		}
		private void irc_OnConnecting(object sender, EventArgs e)
		{
			this.App.Network.PostIRCChatMessage("", "*** Connecting to chat server...");
		}
		private void irc_OnConnected(object sender, EventArgs e)
		{
			this.App.Network.PostIRCChatMessage("", "*** Connected to chat server!");
		}
		public void Update()
		{
			if (this.irc.IsConnected)
			{
				this.irc.Listen(false);
			}
		}
		public void Disconnect()
		{
			if (this.irc.IsConnected)
			{
				this.irc.RfcQuit("Repensum est canicula");
			}
		}
		private void irc_OnWho(object sender, WhoEventArgs e)
		{
		}
		public void SetNick(string nick)
		{
			if (this._ircNick != nick.Replace(" ", "_"))
			{
				this._ircNick = nick.Replace(" ", "_");
				this.irc.RfcNick(nick);
			}
		}
		public void SendChatMessage(string msg)
		{
			if (msg == "/who")
			{
				Channel channel = this.irc.GetChannel(SotsIRC.channel);
				string text = "";
				foreach (DictionaryEntry dictionaryEntry in channel.Users)
				{
					string str = dictionaryEntry.Key.ToString();
					if (text == "")
					{
						text = channel.Users.Count.ToString() + " Connected Players. " + str;
					}
					else
					{
						text = text + ", " + str;
					}
				}
				this.App.Network.PostIRCChatMessage("", text);
				return;
			}
			if (this.irc.IsConnected)
			{
				this.irc.SendMessage(SendType.Message, SotsIRC.channel, msg);
			}
		}
		public void SendRawMessage(string cmd)
		{
			this.irc.WriteLine(cmd);
		}
	}
}
