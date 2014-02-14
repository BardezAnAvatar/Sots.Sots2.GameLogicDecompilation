using System;
namespace Kerberos.Sots.StarSystemPathing
{
	internal class LinkNodeChild
	{
		public LinkNodeChild ParentLink;
		public int SystemId;
		public int NodeId;
		public float Distance;
		public float TotalDistance;
		public bool HasBeenChecked;
	}
}
