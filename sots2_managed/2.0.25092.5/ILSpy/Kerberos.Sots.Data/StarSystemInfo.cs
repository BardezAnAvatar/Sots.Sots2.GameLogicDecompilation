using System;
using System.Collections.Generic;
namespace Kerberos.Sots.Data
{
	internal class StarSystemInfo : StellarInfo
	{
		public string Name;
		public string StellarClass;
		public int? ProvinceID;
		public bool IsVisible;
		public int? TerrainID;
		public List<int> ControlZones;
		public bool IsOpen = true;
		public bool IsDeepSpace
		{
			get
			{
				return this.StellarClass == "Deepspace";
			}
		}
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
		public override string ToString()
		{
			return string.Format("ID={0},Name={1}", base.ID, this.Name);
		}

		public static bool operator ==(StarSystemInfo s1, StarSystemInfo s2)
		{
            //Bardez fixing reflected output
            Boolean equal = false;

            if ((Object)s1 == (Object)s2)   //both null or both non-null
                equal = ((Object)s1 == null) ? true : s1.ID == s2.ID;

            return equal;
		}

		public static bool operator !=(StarSystemInfo s1, StarSystemInfo s2)
        {
            return !(s1 == s2);
		}

		public override bool Equals(object obj)
		{
            //Bardez fixing reflected output
            Boolean equal = false;

            if (obj is StarSystemInfo)
            {
                StarSystemInfo compare = obj as StarSystemInfo;
                equal = (base.ID == compare.ID);
            }

            return equal;
		}
	}
}
