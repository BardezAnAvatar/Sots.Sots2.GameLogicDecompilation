using System;
namespace Kerberos.Sots.Data
{
	public class MoraleEventHistory
	{
		public int id;
		public int turn;
		public MoralEvent moraleEvent;
		public int playerId;
		public int value;
		public int? colonyId;
		public int? systemId;
		public int? provinceId;
	}
}
