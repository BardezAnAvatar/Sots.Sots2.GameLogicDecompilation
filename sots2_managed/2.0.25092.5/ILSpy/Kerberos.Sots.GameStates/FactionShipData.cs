using Kerberos.Sots.PlayerFramework;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.GameStates
{
	internal class FactionShipData
	{
		public Faction Faction;
		public readonly List<ClassShipData> Classes = new List<ClassShipData>();
		private ClassShipData _selectedClass;
		public ClassShipData SelectedClass
		{
			get
			{
				ClassShipData arg_26_0;
				if ((arg_26_0 = this._selectedClass) == null)
				{
					if (this.Classes.Count <= 0)
					{
						return null;
					}
					arg_26_0 = this.Classes[0];
				}
				return arg_26_0;
			}
			set
			{
				this._selectedClass = value;
			}
		}
	}
}
