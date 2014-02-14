using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;
namespace Kerberos.Sots.Framework
{
	internal static class StringInjectExtension
	{
		public static string Inject(this string formatString, object injectionObject)
		{
			return formatString.Inject(StringInjectExtension.GetPropertyHash(injectionObject));
		}
		public static string Inject(this string formatString, IDictionary dictionary)
		{
			return formatString.Inject(new Hashtable(dictionary));
		}
		public static string Inject(this string formatString, Hashtable attributes)
		{
			string text = formatString;
			if (attributes == null || formatString == null)
			{
				return text;
			}
			foreach (string key in attributes.Keys)
			{
				text = text.InjectSingleValue(key, attributes[key]);
			}
			return text;
		}
		public static string InjectSingleValue(this string formatString, string key, object replacementValue)
		{
			string text = formatString;
			Regex regex = new Regex("{(" + key + ")(?:}|(?::(.[^}]*)}))");
			foreach (Match match in regex.Matches(formatString))
			{
				string newValue = match.ToString();
				if (match.Groups[2].Length > 0)
				{
					string format = string.Format(CultureInfo.InvariantCulture, "{{0:{0}}}", new object[]
					{
						match.Groups[2]
					});
					newValue = string.Format(CultureInfo.CurrentCulture, format, new object[]
					{
						replacementValue
					});
				}
				else
				{
					newValue = (replacementValue ?? string.Empty).ToString();
				}
				text = text.Replace(match.ToString(), newValue);
			}
			return text;
		}
		private static Hashtable GetPropertyHash(object properties)
		{
			Hashtable hashtable = null;
			if (properties != null)
			{
				hashtable = new Hashtable();
				PropertyDescriptorCollection properties2 = TypeDescriptor.GetProperties(properties);
				foreach (PropertyDescriptor propertyDescriptor in properties2)
				{
					hashtable.Add(propertyDescriptor.Name, propertyDescriptor.GetValue(properties));
				}
			}
			return hashtable;
		}
	}
}
