using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Strategy
{
	public class OpenCloseSystemToggleData
	{
		private List<OpenCloseSystemInfo> Systems;
		public List<OpenCloseSystemInfo> ToggledSystems
		{
			get
			{
				return this.Systems;
			}
		}
		public OpenCloseSystemToggleData()
		{
			this.Systems = new List<OpenCloseSystemInfo>();
		}
		public void ClearData()
		{
			this.Systems.Clear();
		}
		public void SystemToggled(int playerId, int systemId, bool isOpen)
		{
			OpenCloseSystemInfo openCloseSystemInfo = this.Systems.FirstOrDefault((OpenCloseSystemInfo x) => x.SystemID == systemId);
			if (openCloseSystemInfo != null)
			{
				this.Systems.Remove(openCloseSystemInfo);
				return;
			}
			OpenCloseSystemInfo openCloseSystemInfo2 = new OpenCloseSystemInfo();
			openCloseSystemInfo2.PlayerID = playerId;
			openCloseSystemInfo2.SystemID = systemId;
			openCloseSystemInfo2.IsOpen = isOpen;
			this.Systems.Add(openCloseSystemInfo2);
		}
	}
}
