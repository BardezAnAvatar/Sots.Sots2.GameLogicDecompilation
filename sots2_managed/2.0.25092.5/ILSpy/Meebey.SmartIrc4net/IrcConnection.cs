using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
namespace Meebey.SmartIrc4net
{
	public class IrcConnection
	{
		private class ReadThread
		{
			private IrcConnection _Connection;
			private Thread _Thread;
			private Queue _Queue = Queue.Synchronized(new Queue());
			public Queue Queue
			{
				get
				{
					return this._Queue;
				}
			}
			public ReadThread(IrcConnection connection)
			{
				this._Connection = connection;
			}
			public void Start()
			{
				this._Thread = new Thread(new ThreadStart(this._Worker));
				this._Thread.Name = string.Concat(new object[]
				{
					"ReadThread (",
					this._Connection.Address,
					":",
					this._Connection.Port,
					")"
				});
				this._Thread.IsBackground = true;
				this._Thread.Start();
			}
			public void Stop()
			{
				try
				{
					this._Thread.Abort();
				}
				catch (ThreadAbortException)
				{
				}
				this._Thread.Join();
				try
				{
					this._Connection._Reader.Close();
				}
				catch (ObjectDisposedException)
				{
				}
			}
			private void _Worker()
			{
				try
				{
					try
					{
						string obj;
						while (this._Connection.IsConnected && (obj = this._Connection._Reader.ReadLine()) != null)
						{
							this._Queue.Enqueue(obj);
						}
					}
					catch (IOException)
					{
					}
					finally
					{
						if (!this._Connection.IsDisconnecting)
						{
							this._Connection.IsConnectionError = true;
						}
					}
				}
				catch (ThreadAbortException)
				{
					Thread.ResetAbort();
				}
				catch (Exception)
				{
				}
			}
		}
		private class WriteThread
		{
			private IrcConnection _Connection;
			private Thread _Thread;
			private int _HighCount;
			private int _AboveMediumCount;
			private int _MediumCount;
			private int _BelowMediumCount;
			private int _LowCount;
			private int _AboveMediumSentCount;
			private int _MediumSentCount;
			private int _BelowMediumSentCount;
			private int _AboveMediumThresholdCount = 4;
			private int _MediumThresholdCount = 2;
			private int _BelowMediumThresholdCount = 1;
			private int _BurstCount;
			public WriteThread(IrcConnection connection)
			{
				this._Connection = connection;
			}
			public void Start()
			{
				this._Thread = new Thread(new ThreadStart(this._Worker));
				this._Thread.Name = string.Concat(new object[]
				{
					"WriteThread (",
					this._Connection.Address,
					":",
					this._Connection.Port,
					")"
				});
				this._Thread.IsBackground = true;
				this._Thread.Start();
			}
			public void Stop()
			{
				try
				{
					this._Thread.Abort();
				}
				catch (ThreadAbortException)
				{
				}
				this._Thread.Join();
				try
				{
					this._Connection._Writer.Close();
				}
				catch (ObjectDisposedException)
				{
				}
			}
			private void _Worker()
			{
				try
				{
					try
					{
						while (this._Connection.IsConnected)
						{
							this._CheckBuffer();
							Thread.Sleep(this._Connection._SendDelay);
						}
					}
					catch (IOException)
					{
					}
					finally
					{
						if (!this._Connection.IsDisconnecting)
						{
							this._Connection.IsConnectionError = true;
						}
					}
				}
				catch (ThreadAbortException)
				{
					Thread.ResetAbort();
				}
				catch (Exception)
				{
				}
			}
			private void _CheckBuffer()
			{
				if (!this._Connection._IsRegistered)
				{
					return;
				}
				this._HighCount = ((Queue)this._Connection._SendBuffer[Priority.High]).Count;
				this._AboveMediumCount = ((Queue)this._Connection._SendBuffer[Priority.AboveMedium]).Count;
				this._MediumCount = ((Queue)this._Connection._SendBuffer[Priority.Medium]).Count;
				this._BelowMediumCount = ((Queue)this._Connection._SendBuffer[Priority.BelowMedium]).Count;
				this._LowCount = ((Queue)this._Connection._SendBuffer[Priority.Low]).Count;
				if (this._CheckHighBuffer() && this._CheckAboveMediumBuffer() && this._CheckMediumBuffer() && this._CheckBelowMediumBuffer() && this._CheckLowBuffer())
				{
					this._AboveMediumSentCount = 0;
					this._MediumSentCount = 0;
					this._BelowMediumSentCount = 0;
					this._BurstCount = 0;
				}
				if (this._BurstCount < 3)
				{
					this._BurstCount++;
				}
			}
			private bool _CheckHighBuffer()
			{
				if (this._HighCount > 0)
				{
					string text = (string)((Queue)this._Connection._SendBuffer[Priority.High]).Dequeue();
					if (!this._Connection._WriteLine(text))
					{
						((Queue)this._Connection._SendBuffer[Priority.High]).Enqueue(text);
					}
					if (this._HighCount > 1)
					{
						return false;
					}
				}
				return true;
			}
			private bool _CheckAboveMediumBuffer()
			{
				if (this._AboveMediumCount > 0 && this._AboveMediumSentCount < this._AboveMediumThresholdCount)
				{
					string text = (string)((Queue)this._Connection._SendBuffer[Priority.AboveMedium]).Dequeue();
					if (!this._Connection._WriteLine(text))
					{
						((Queue)this._Connection._SendBuffer[Priority.AboveMedium]).Enqueue(text);
					}
					this._AboveMediumSentCount++;
					if (this._AboveMediumSentCount < this._AboveMediumThresholdCount)
					{
						return false;
					}
				}
				return true;
			}
			private bool _CheckMediumBuffer()
			{
				if (this._MediumCount > 0 && this._MediumSentCount < this._MediumThresholdCount)
				{
					string text = (string)((Queue)this._Connection._SendBuffer[Priority.Medium]).Dequeue();
					if (!this._Connection._WriteLine(text))
					{
						((Queue)this._Connection._SendBuffer[Priority.Medium]).Enqueue(text);
					}
					this._MediumSentCount++;
					if (this._MediumSentCount < this._MediumThresholdCount)
					{
						return false;
					}
				}
				return true;
			}
			private bool _CheckBelowMediumBuffer()
			{
				if (this._BelowMediumCount > 0 && this._BelowMediumSentCount < this._BelowMediumThresholdCount)
				{
					string text = (string)((Queue)this._Connection._SendBuffer[Priority.BelowMedium]).Dequeue();
					if (!this._Connection._WriteLine(text))
					{
						((Queue)this._Connection._SendBuffer[Priority.BelowMedium]).Enqueue(text);
					}
					this._BelowMediumSentCount++;
					if (this._BelowMediumSentCount < this._BelowMediumThresholdCount)
					{
						return false;
					}
				}
				return true;
			}
			private bool _CheckLowBuffer()
			{
				if (this._LowCount > 0)
				{
					if (this._HighCount > 0 || this._AboveMediumCount > 0 || this._MediumCount > 0 || this._BelowMediumCount > 0)
					{
						return true;
					}
					string text = (string)((Queue)this._Connection._SendBuffer[Priority.Low]).Dequeue();
					if (!this._Connection._WriteLine(text))
					{
						((Queue)this._Connection._SendBuffer[Priority.Low]).Enqueue(text);
					}
					if (this._LowCount > 1)
					{
						return false;
					}
				}
				return true;
			}
		}
		private class IdleWorkerThread
		{
			private IrcConnection _Connection;
			private Thread _Thread;
			public IdleWorkerThread(IrcConnection connection)
			{
				this._Connection = connection;
			}
			public void Start()
			{
				DateTime now = DateTime.Now;
				this._Connection._LastPingSent = now;
				this._Connection._LastPongReceived = now;
				this._Thread = new Thread(new ThreadStart(this._Worker));
				this._Thread.Name = string.Concat(new object[]
				{
					"IdleWorkerThread (",
					this._Connection.Address,
					":",
					this._Connection.Port,
					")"
				});
				this._Thread.IsBackground = true;
				this._Thread.Start();
			}
			public void Stop()
			{
				try
				{
					this._Thread.Abort();
				}
				catch (ThreadAbortException)
				{
				}
			}
			private void _Worker()
			{
				try
				{
					while (this._Connection.IsConnected)
					{
						Thread.Sleep(this._Connection._IdleWorkerInterval);
						if (this._Connection.IsRegistered)
						{
							DateTime now = DateTime.Now;
							int num = (int)(now - this._Connection._LastPingSent).TotalSeconds;
							int num2 = (int)(now - this._Connection._LastPongReceived).TotalSeconds;
							if (num < this._Connection._PingTimeout)
							{
								if (!(this._Connection._LastPingSent > this._Connection._LastPongReceived) && num2 > this._Connection._PingInterval)
								{
									this._Connection.WriteLine(Rfc2812.Ping(this._Connection.Address), Priority.Critical);
									this._Connection._LastPingSent = now;
								}
							}
							else
							{
								if (!this._Connection.IsDisconnecting)
								{
									this._Connection.IsConnectionError = true;
									break;
								}
								break;
							}
						}
					}
				}
				catch (ThreadAbortException)
				{
					Thread.ResetAbort();
				}
				catch (Exception)
				{
				}
			}
		}
		private string _VersionNumber;
		private string _VersionString;
		private string[] _AddressList = new string[]
		{
			"localhost"
		};
		private int _CurrentAddress;
		private int _Port;
		private StreamReader _Reader;
		private StreamWriter _Writer;
		private IrcConnection.ReadThread _ReadThread;
		private IrcConnection.WriteThread _WriteThread;
		private IrcConnection.IdleWorkerThread _IdleWorkerThread;
		private IrcTcpClient _TcpClient;
		private Hashtable _SendBuffer = Hashtable.Synchronized(new Hashtable());
		private int _SendDelay = 200;
		private bool _IsRegistered;
		private bool _IsConnected;
		private bool _IsConnectionError;
		private bool _IsDisconnecting;
		private int _ConnectTries;
		private bool _AutoRetry;
		private int _AutoRetryDelay = 30;
		private bool _AutoReconnect;
		private Encoding _Encoding = Encoding.Default;
		private int _SocketReceiveTimeout = 600;
		private int _SocketSendTimeout = 600;
		private int _IdleWorkerInterval = 60;
		private int _PingInterval = 60;
		private int _PingTimeout = 300;
		private DateTime _LastPingSent;
		private DateTime _LastPongReceived;
		private TimeSpan _Lag;
		public event ReadLineEventHandler OnReadLine;
		public event WriteLineEventHandler OnWriteLine;
		public event EventHandler OnConnecting;
		public event EventHandler OnConnected;
		public event EventHandler OnDisconnecting;
		public event EventHandler OnDisconnected;
		public event EventHandler OnConnectionError;
		public event AutoConnectErrorEventHandler OnAutoConnectError;
		protected bool IsConnectionError
		{
			get
			{
				bool flag = false;
				bool isConnectionError;
				try
				{
					Monitor.Enter(this, ref flag);
					isConnectionError = this._IsConnectionError;
				}
				finally
				{
					if (flag)
					{
						Monitor.Exit(this);
					}
				}
				return isConnectionError;
			}
			set
			{
				bool flag = false;
				try
				{
					Monitor.Enter(this, ref flag);
					this._IsConnectionError = value;
				}
				finally
				{
					if (flag)
					{
						Monitor.Exit(this);
					}
				}
			}
		}
		protected bool IsDisconnecting
		{
			get
			{
				bool flag = false;
				bool isDisconnecting;
				try
				{
					Monitor.Enter(this, ref flag);
					isDisconnecting = this._IsDisconnecting;
				}
				finally
				{
					if (flag)
					{
						Monitor.Exit(this);
					}
				}
				return isDisconnecting;
			}
			set
			{
				bool flag = false;
				try
				{
					Monitor.Enter(this, ref flag);
					this._IsDisconnecting = value;
				}
				finally
				{
					if (flag)
					{
						Monitor.Exit(this);
					}
				}
			}
		}
		public string Address
		{
			get
			{
				return this._AddressList[this._CurrentAddress];
			}
		}
		public string[] AddressList
		{
			get
			{
				return this._AddressList;
			}
		}
		public int Port
		{
			get
			{
				return this._Port;
			}
		}
		public bool AutoReconnect
		{
			get
			{
				return this._AutoReconnect;
			}
			set
			{
				this._AutoReconnect = value;
			}
		}
		public bool AutoRetry
		{
			get
			{
				return this._AutoRetry;
			}
			set
			{
				this._AutoRetry = value;
			}
		}
		public int AutoRetryDelay
		{
			get
			{
				return this._AutoRetryDelay;
			}
			set
			{
				this._AutoRetryDelay = value;
			}
		}
		public int SendDelay
		{
			get
			{
				return this._SendDelay;
			}
			set
			{
				this._SendDelay = value;
			}
		}
		public bool IsRegistered
		{
			get
			{
				return this._IsRegistered;
			}
		}
		public bool IsConnected
		{
			get
			{
				return this._IsConnected;
			}
		}
		public string VersionNumber
		{
			get
			{
				return this._VersionNumber;
			}
		}
		public string VersionString
		{
			get
			{
				return this._VersionString;
			}
		}
		public Encoding Encoding
		{
			get
			{
				return this._Encoding;
			}
			set
			{
				this._Encoding = value;
			}
		}
		public int SocketReceiveTimeout
		{
			get
			{
				return this._SocketReceiveTimeout;
			}
			set
			{
				this._SocketReceiveTimeout = value;
			}
		}
		public int SocketSendTimeout
		{
			get
			{
				return this._SocketSendTimeout;
			}
			set
			{
				this._SocketSendTimeout = value;
			}
		}
		public int IdleWorkerInterval
		{
			get
			{
				return this._IdleWorkerInterval;
			}
			set
			{
				this._IdleWorkerInterval = value;
			}
		}
		public int PingInterval
		{
			get
			{
				return this._PingInterval;
			}
			set
			{
				this._PingInterval = value;
			}
		}
		public int PingTimeout
		{
			get
			{
				return this._PingTimeout;
			}
			set
			{
				this._PingTimeout = value;
			}
		}
		public TimeSpan Lag
		{
			get
			{
				if (this._LastPingSent > this._LastPongReceived)
				{
					return DateTime.Now - this._LastPingSent;
				}
				return this._Lag;
			}
		}
		public IrcConnection()
		{
			this._SendBuffer[Priority.High] = Queue.Synchronized(new Queue());
			this._SendBuffer[Priority.AboveMedium] = Queue.Synchronized(new Queue());
			this._SendBuffer[Priority.Medium] = Queue.Synchronized(new Queue());
			this._SendBuffer[Priority.BelowMedium] = Queue.Synchronized(new Queue());
			this._SendBuffer[Priority.Low] = Queue.Synchronized(new Queue());
			this.OnReadLine += new ReadLineEventHandler(this._SimpleParser);
			this.OnConnectionError += new EventHandler(this._OnConnectionError);
			this._ReadThread = new IrcConnection.ReadThread(this);
			this._WriteThread = new IrcConnection.WriteThread(this);
			this._IdleWorkerThread = new IrcConnection.IdleWorkerThread(this);
			Assembly assembly = Assembly.GetAssembly(base.GetType());
			AssemblyName name = assembly.GetName(false);
			AssemblyProductAttribute assemblyProductAttribute = (AssemblyProductAttribute)assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false)[0];
			this._VersionNumber = name.Version.ToString();
			this._VersionString = assemblyProductAttribute.Product + " " + this._VersionNumber;
		}
		public void Connect(string[] addresslist, int port)
		{
			if (this._IsConnected)
			{
				throw new AlreadyConnectedException(string.Concat(new object[]
				{
					"Already connected to: ",
					this.Address,
					":",
					this.Port
				}));
			}
			this._ConnectTries++;
			this._AddressList = (string[])addresslist.Clone();
			this._Port = port;
			if (this.OnConnecting != null)
			{
				this.OnConnecting(this, EventArgs.Empty);
			}
			try
			{
				IPAddress address = Dns.GetHostEntry(this.Address).AddressList[0];
				this._TcpClient = new IrcTcpClient();
				this._TcpClient.NoDelay = true;
				this._TcpClient.Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, 1);
				this._TcpClient.ReceiveTimeout = this._SocketReceiveTimeout * 1000;
				this._TcpClient.SendTimeout = this._SocketSendTimeout * 1000;
				this._TcpClient.Connect(address, port);
				Stream stream = this._TcpClient.GetStream();
				this._Reader = new StreamReader(stream, this._Encoding);
				this._Writer = new StreamWriter(stream, this._Encoding);
				if (this._Encoding.GetPreamble().Length > 0)
				{
					this._Writer.WriteLine();
				}
				this._ConnectTries = 0;
				this.IsConnectionError = false;
				this._IsConnected = true;
				this._ReadThread.Start();
				this._WriteThread.Start();
				this._IdleWorkerThread.Start();
				if (this.OnConnected != null)
				{
					this.OnConnected(this, EventArgs.Empty);
				}
			}
			catch (Exception ex)
			{
				if (this._Reader != null)
				{
					try
					{
						this._Reader.Close();
					}
					catch (ObjectDisposedException)
					{
					}
				}
				if (this._Writer != null)
				{
					try
					{
						this._Writer.Close();
					}
					catch (ObjectDisposedException)
					{
					}
				}
				if (this._TcpClient != null)
				{
					this._TcpClient.Close();
				}
				this._IsConnected = false;
				this.IsConnectionError = true;
				if (!this._AutoRetry || this._ConnectTries > 3)
				{
					throw new CouldNotConnectException(string.Concat(new object[]
					{
						"Could not connect to: ",
						this.Address,
						":",
						this.Port,
						" ",
						ex.Message
					}), ex);
				}
				if (this.OnAutoConnectError != null)
				{
					this.OnAutoConnectError(this, new AutoConnectErrorEventArgs(this.Address, this.Port, ex));
				}
				Thread.Sleep(this._AutoRetryDelay * 1000);
				this._NextAddress();
				this.Connect(this._AddressList, this._Port);
			}
		}
		public void Connect(string address, int port)
		{
			this.Connect(new string[]
			{
				address
			}, port);
		}
		public void Reconnect()
		{
			this.Disconnect();
			this.Connect(this._AddressList, this._Port);
		}
		public void Disconnect()
		{
			bool arg_06_0 = this.IsConnected;
			if (this.OnDisconnecting != null)
			{
				this.OnDisconnecting(this, EventArgs.Empty);
			}
			this.IsDisconnecting = true;
			this._ReadThread.Stop();
			this._WriteThread.Stop();
			this._TcpClient.Close();
			this._IsConnected = false;
			this._IsRegistered = false;
			this.IsDisconnecting = false;
			if (this.OnDisconnected != null)
			{
				this.OnDisconnected(this, EventArgs.Empty);
			}
		}
		public void Listen(bool blocking)
		{
			if (blocking)
			{
				while (this.IsConnected)
				{
					this.ReadLine(true);
				}
				return;
			}
			while (this.ReadLine(false).Length > 0)
			{
			}
		}
		public void Listen()
		{
			this.Listen(true);
		}
		public void ListenOnce(bool blocking)
		{
			this.ReadLine(blocking);
		}
		public void ListenOnce()
		{
			this.ListenOnce(true);
		}
		public string ReadLine(bool blocking)
		{
			string text = "";
			if (blocking)
			{
				while (this.IsConnected && !this.IsConnectionError && this._ReadThread.Queue.Count == 0)
				{
					Thread.Sleep(10);
				}
			}
			if (this.IsConnected && this._ReadThread.Queue.Count > 0)
			{
				text = (string)this._ReadThread.Queue.Dequeue();
			}
			if (text != null && text.Length > 0 && this.OnReadLine != null)
			{
				this.OnReadLine(this, new ReadLineEventArgs(text));
			}
			if (this.IsConnectionError && !this.IsDisconnecting && this.OnConnectionError != null)
			{
				this.OnConnectionError(this, EventArgs.Empty);
			}
			return text;
		}
		public void WriteLine(string data, Priority priority)
		{
			if (priority != Priority.Critical)
			{
				((Queue)this._SendBuffer[priority]).Enqueue(data);
				return;
			}
			if (!this.IsConnected)
			{
				throw new NotConnectedException();
			}
			this._WriteLine(data);
		}
		public void WriteLine(string data)
		{
			this.WriteLine(data, Priority.Medium);
		}
		private bool _WriteLine(string data)
		{
			if (this.IsConnected)
			{
				try
				{
					this._Writer.Write(data + "\r\n");
					this._Writer.Flush();
				}
				catch (IOException)
				{
					this.IsConnectionError = true;
					bool result = false;
					return result;
				}
				catch (ObjectDisposedException)
				{
					this.IsConnectionError = true;
					bool result = false;
					return result;
				}
				if (this.OnWriteLine != null)
				{
					this.OnWriteLine(this, new WriteLineEventArgs(data));
				}
				return true;
			}
			return false;
		}
		private void _NextAddress()
		{
			this._CurrentAddress++;
			if (this._CurrentAddress >= this._AddressList.Length)
			{
				this._CurrentAddress = 0;
			}
		}
		private void _SimpleParser(object sender, ReadLineEventArgs args)
		{
			string line = args.Line;
			string[] array = line.Split(new char[]
			{
				' '
			});
			if (line[0] == ':')
			{
				string text = array[1];
				int num = 0;
				ReplyCode replyCode = ReplyCode.Null;
				if (int.TryParse(text, out num) && Enum.IsDefined(typeof(ReplyCode), num))
				{
					replyCode = (ReplyCode)num;
				}
				if (replyCode != ReplyCode.Null)
				{
					ReplyCode replyCode2 = replyCode;
					if (replyCode2 != ReplyCode.Welcome)
					{
						return;
					}
					this._IsRegistered = true;
					return;
				}
				else
				{
					string a;
					if ((a = array[1]) != null)
					{
						if (!(a == "PONG"))
						{
							return;
						}
						DateTime now = DateTime.Now;
						this._LastPongReceived = now;
						this._Lag = now - this._LastPingSent;
						return;
					}
				}
			}
			else
			{
				string text = array[0];
				string a2;
				if ((a2 = text) != null)
				{
					bool flag1 = a2 == "ERROR";
				}
			}
		}
		private void _OnConnectionError(object sender, EventArgs e)
		{
			try
			{
				if (this.AutoReconnect)
				{
					this.Reconnect();
				}
				else
				{
					this.Disconnect();
				}
			}
			catch (ConnectionException)
			{
			}
		}
	}
}
