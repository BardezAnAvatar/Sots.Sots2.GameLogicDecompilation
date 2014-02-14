using Kerberos.Sots.Framework;
using System;
namespace Kerberos.Sots
{
	internal class StarDisplayParams
	{
		public string AssetPath
		{
			get;
			set;
		}
		public Vector3 ImposterColor
		{
			get;
			set;
		}
		public StarDisplayParams()
		{
			this.AssetPath = string.Empty;
			this.ImposterColor = DefaultStarModelParameters.ImposterColor;
		}
	}
}
