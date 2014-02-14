using System;
namespace Kerberos.Sots.Data
{
	internal delegate void RowObjectDirtiedEventHandler<TRowKey>(object sender, TRowKey key);
}
