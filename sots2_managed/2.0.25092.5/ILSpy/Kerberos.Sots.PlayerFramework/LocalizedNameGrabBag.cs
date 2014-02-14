using System;
using System.Xml;
namespace Kerberos.Sots.PlayerFramework
{
	internal class LocalizedNameGrabBag
	{
		private int _next;
		public string Prefix
		{
			get;
			private set;
		}
		public int Count
		{
			get;
			private set;
		}
		private void Construct(string prefix, int count, Random random)
		{
			this.Prefix = prefix;
			this.Count = count;
			if (this.Count > 0)
			{
				this._next = random.Next(this.Count);
			}
		}
		public LocalizedNameGrabBag(string prefix, int count, Random random)
		{
			this.Construct(prefix, count, random);
		}
		public LocalizedNameGrabBag(XmlElement element, Random random)
		{
			if (element == null)
			{
				this.Construct(null, 0, null);
				return;
			}
			this.Construct(element.GetAttribute("prefix"), int.Parse(element.GetAttribute("count")), random);
		}
		public string GetNextStringID()
		{
			if (this.Count == 0)
			{
				return null;
			}
			this._next %= this.Count;
			string result = this.Prefix + (this._next + 1).ToString();
			this._next++;
			return result;
		}
	}
}
