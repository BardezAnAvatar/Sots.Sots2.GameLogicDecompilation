using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
namespace Kerberos.Sots.Data.Xml
{
	public static class XmlHelper
	{
		public static Exception NoXmlNameException = new Exception("The item you tried to save does not have a default XML name - Either provide one with the XmlName.get() function or use the save method that provides a node name.");
		public static void Load(this XmlDocument document, IFileSystem fileSystem, string path)
		{
			using (Stream stream = fileSystem.CreateStream(path))
			{
				document.Load(stream);
			}
		}
		public static int ExtractIntegerOrDefault(this XmlElement e, int defaultValue)
		{
			if (e == null)
			{
				return defaultValue;
			}
			return int.Parse(e.InnerText);
		}
		public static string ExtractStringOrDefault(this XmlElement e, string defaultValue)
		{
			if (e == null)
			{
				return defaultValue;
			}
			return e.InnerText;
		}
		public static float ExtractSingleOrDefault(this XmlElement e, float defaultValue)
		{
			if (e == null)
			{
				return defaultValue;
			}
			return float.Parse(e.InnerText);
		}
		public static double ExtractDoubleOrDefault(this XmlElement e, double defaultValue)
		{
			if (e == null)
			{
				return defaultValue;
			}
			return double.Parse(e.InnerText);
		}
		public static Vector2 ExtractVector2OrDefault(this XmlElement e, Vector2 defaultValue)
		{
			if (e == null)
			{
				return defaultValue;
			}
			return Vector2.Parse(e.InnerText);
		}
		public static Vector3 ExtractVector3OrDefault(this XmlElement e, Vector3 defaultValue)
		{
			if (e == null)
			{
				return defaultValue;
			}
			return Vector3.Parse(e.InnerText);
		}
		public static T GetDataOrDefault<T>(XmlElement node, T defaultValue)
		{
			if (node == null)
			{
				return defaultValue;
			}
			T result;
			try
			{
				if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
				{
					if (string.IsNullOrEmpty(node.InnerText))
					{
						result = default(T);
					}
					else
					{
						result = (T)((object)Convert.ChangeType(node.InnerText, Nullable.GetUnderlyingType(typeof(T))));
					}
				}
				else
				{
					result = (T)((object)Convert.ChangeType(node.InnerText, typeof(T)));
				}
			}
			catch
			{
				if (App.Log != null)
				{
					App.Log.Trace(string.Format("The node {0} could not be parsed in {1}.  The default '{2}' will be used for the variable.", node.Name, (node.ParentNode != null) ? node.ParentNode.Name : "<root>", defaultValue), "data");
				}
				result = defaultValue;
			}
			return result;
		}
		public static T GetData<T>(XmlElement node)
		{
			return XmlHelper.GetDataOrDefault<T>(node, default(T));
		}
		public static T GetData<T>(XmlElement rootNode, string nodeName)
		{
			return XmlHelper.GetDataOrDefault<T>(rootNode[nodeName], default(T));
		}
		public static T GetData<T>(XmlElement rootNode, Dictionary<string, Type> TypeMap)
		{
			if (!rootNode.HasChildNodes)
			{
				return default(T);
			}
			XmlElement xmlElement = (XmlElement)rootNode.FirstChild;
			if (TypeMap.ContainsKey(xmlElement.Name))
			{
				T t = (T)((object)Activator.CreateInstance(TypeMap[xmlElement.Name]));
				(t as IXmlLoadSave).LoadFromXmlNode(xmlElement);
				return t;
			}
			return default(T);
		}
		public static List<T> GetDataObjectCollection<T>(XmlElement rootNode, string nodeName, string memberName) where T : new()
		{
			List<T> list = new List<T>();
			if (rootNode[nodeName] != null)
			{
				foreach (XmlElement xmlElement in rootNode[nodeName].ChildNodes)
				{
					if (xmlElement.Name == memberName)
					{
						T t = (default(T) == null) ? Activator.CreateInstance<T>() : default(T);
						(t as IXmlLoadSave).LoadFromXmlNode(xmlElement);
						list.Add(t);
					}
				}
			}
			return list;
		}
		public static T GetDataObject<T>(XmlElement rootNode, string nodeName, Dictionary<string, Type> TypeMap) where T : new()
		{
			if (rootNode[nodeName] != null)
			{
				XmlElement xmlElement = (XmlElement)rootNode[nodeName].FirstChild;
				if (TypeMap.ContainsKey(xmlElement.Name))
				{
					T t = (T)((object)Activator.CreateInstance(TypeMap[xmlElement.Name]));
					(t as IXmlLoadSave).LoadFromXmlNode(xmlElement);
					return t;
				}
			}
			return default(T);
		}
		public static List<T> GetDataObjectCollection<T>(XmlElement rootNode, string nodeName, Dictionary<string, Type> TypeMap) where T : new()
		{
			List<T> list = new List<T>();
			if (rootNode[nodeName] != null)
			{
				foreach (XmlElement xmlElement in rootNode[nodeName].ChildNodes)
				{
					if (TypeMap.ContainsKey(xmlElement.Name))
					{
						T t = (T)((object)Activator.CreateInstance(TypeMap[xmlElement.Name]));
						(t as IXmlLoadSave).LoadFromXmlNode(xmlElement);
						list.Add(t);
					}
				}
			}
			return list;
		}
		public static List<T> GetDataCollection<T>(XmlElement rootNode, string nodeName, string memberName)
		{
			List<T> list = new List<T>();
			if (rootNode[nodeName] != null)
			{
				foreach (XmlElement xmlElement in rootNode[nodeName].ChildNodes)
				{
					if (xmlElement.Name == memberName)
					{
						list.Add(XmlHelper.GetData<T>(xmlElement));
					}
				}
			}
			return list;
		}
		public static void AddObjectNode(IXmlLoadSave item, string nodeName, ref XmlElement Root)
		{
			XmlElement xmlElement = Root.OwnerDocument.CreateElement(nodeName);
			XmlElement newChild = Root.OwnerDocument.CreateElement(item.XmlName);
			item.AttachToXmlNode(ref newChild);
			xmlElement.AppendChild(newChild);
			Root.AppendChild(xmlElement);
		}
		public static void AddObjectCollectionNode(IEnumerable<IXmlLoadSave> items, string nodeName, ref XmlElement Root)
		{
			XmlElement xmlElement = Root.OwnerDocument.CreateElement(nodeName);
			foreach (IXmlLoadSave current in items)
			{
				XmlElement newChild = Root.OwnerDocument.CreateElement(current.XmlName);
				current.AttachToXmlNode(ref newChild);
				xmlElement.AppendChild(newChild);
			}
			Root.AppendChild(xmlElement);
		}
		public static void AddObjectCollectionNode(IEnumerable<IXmlLoadSave> items, string nodeName, string memberName, ref XmlElement Root)
		{
			XmlElement xmlElement = Root.OwnerDocument.CreateElement(nodeName);
			foreach (IXmlLoadSave current in items)
			{
				XmlElement newChild = Root.OwnerDocument.CreateElement(memberName);
				current.AttachToXmlNode(ref newChild);
				xmlElement.AppendChild(newChild);
			}
			Root.AppendChild(xmlElement);
		}
		public static void AddCollectionNode<T>(IEnumerable<T> items, string nodeName, string memberName, ref XmlElement Root)
		{
			XmlElement newChild = Root.OwnerDocument.CreateElement(nodeName);
			foreach (T current in items)
			{
				XmlHelper.AddNode(current, memberName, ref newChild);
			}
			Root.AppendChild(newChild);
		}
		public static void AddNode(IXmlLoadSave Data, string Name, ref XmlElement Root)
		{
			if (Data != null)
			{
				XmlElement newChild = Root.OwnerDocument.CreateElement(Name);
				Data.AttachToXmlNode(ref newChild);
				Root.AppendChild(newChild);
			}
		}
		public static void AddNode(object Data, string Name, ref XmlElement Root)
		{
			XmlElement xmlElement = Root.OwnerDocument.CreateElement(Name);
			if (Data != null)
			{
				xmlElement.InnerText = Data.ToString();
			}
			Root.AppendChild(xmlElement);
		}
	}
}
