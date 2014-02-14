using System;
namespace Kerberos.Sots.Data
{
	internal class NodeLineInfo : IIDProvider
	{
		public int System1ID;
		public int System2ID;
		public int Health;
		public bool IsLoaLine;
		public int ID
		{
			get;
			set;
		}
		public bool IsPermenant
		{
			get
			{
				return this.Health == -1;
			}
		}
	}
}
