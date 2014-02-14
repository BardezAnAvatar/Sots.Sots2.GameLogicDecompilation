using Kerberos.Sots.Framework;
using System;
namespace Kerberos.Sots.Engine
{
	internal interface IForceable
	{
		Vector3 Linear
		{
			get;
			set;
		}
		Vector3 Rotational
		{
			get;
			set;
		}
		float DurationInSeconds
		{
			get;
			set;
		}
	}
}
