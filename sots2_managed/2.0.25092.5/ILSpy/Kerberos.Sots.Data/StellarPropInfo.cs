using Kerberos.Sots.Framework;
using System;
namespace Kerberos.Sots.Data
{
	internal class StellarPropInfo : IIDProvider
	{
		public string AssetPath;
		public Matrix Transform;
		public int ID
		{
			get;
			set;
		}
	}
}
