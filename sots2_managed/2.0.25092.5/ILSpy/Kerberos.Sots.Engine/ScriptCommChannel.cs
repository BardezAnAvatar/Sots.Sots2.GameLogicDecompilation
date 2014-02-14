using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
namespace Kerberos.Sots.Engine
{
	internal class ScriptCommChannel
	{
		public static bool LogEnable;
		private UnicodeEncoding _stringcoder = new UnicodeEncoding();
		private IMessageQueue _messageQueue;
		private byte[] _messageBuffer;
		private uint _recvMessageCount;
		private uint _sendMessageCount;
		private ScriptMessageWriter _scriptMessageWriter = new ScriptMessageWriter();
		public ScriptCommChannel(IMessageQueue messageQueue)
		{
			this._messageQueue = messageQueue;
			this._messageBuffer = new byte[messageQueue.IncomingCapacity];
		}
		public void SendMessage(IEnumerable elements)
		{
			this._scriptMessageWriter.Clear();
			this._scriptMessageWriter.Write(elements);
			this._sendMessageCount += 1u;
			this._messageQueue.PutMessage(this._scriptMessageWriter.GetBuffer(), (int)this._scriptMessageWriter.GetSize());
		}
		public IEnumerable<ScriptMessageReader> PumpMessages()
		{
			this._messageQueue.Update();
			while (true)
			{
				int nextMessageSize = this._messageQueue.GetNextMessageSize();
				if (nextMessageSize == 0)
				{
					break;
				}
				ScriptMessageReader scriptMessageReader = new ScriptMessageReader();
				scriptMessageReader.SetSize((long)nextMessageSize);
				this._messageQueue.GetNextMessageData(scriptMessageReader.GetBuffer(), nextMessageSize);
				this._recvMessageCount += 1u;
				yield return scriptMessageReader;
			}
			yield break;
		}
	}
}
