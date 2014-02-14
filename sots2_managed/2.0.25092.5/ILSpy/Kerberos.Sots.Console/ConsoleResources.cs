using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;
namespace Kerberos.Sots.Console
{
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0"), DebuggerNonUserCode, CompilerGenerated]
	internal class ConsoleResources
	{
		private static ResourceManager resourceMan;
		private static CultureInfo resourceCulture;
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static ResourceManager ResourceManager
		{
			get
			{
				if (object.ReferenceEquals(ConsoleResources.resourceMan, null))
				{
					ResourceManager resourceManager = new ResourceManager("Kerberos.Sots.Console.ConsoleResources", typeof(ConsoleResources).Assembly);
					ConsoleResources.resourceMan = resourceManager;
				}
				return ConsoleResources.resourceMan;
			}
		}
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static CultureInfo Culture
		{
			get
			{
				return ConsoleResources.resourceCulture;
			}
			set
			{
				ConsoleResources.resourceCulture = value;
			}
		}
		internal static string load_tac_targeting_config
		{
			get
			{
				return ConsoleResources.ResourceManager.GetString("load_tac_targeting_config", ConsoleResources.resourceCulture);
			}
		}
		internal ConsoleResources()
		{
		}
	}
}
