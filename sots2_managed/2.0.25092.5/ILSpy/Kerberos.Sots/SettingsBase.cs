using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
namespace Kerberos.Sots
{
	internal abstract class SettingsBase
	{
		protected string _path;
		protected XmlDocument _doc = new XmlDocument();
		protected XmlElement _settingsElement;
		protected PropertyInfo[] _props;
		private XmlElement GetPropertyElement(PropertyInfo propInfo)
		{
			XmlElement xmlElement = this._settingsElement[propInfo.Name];
			if (xmlElement == null)
			{
				xmlElement = this._doc.CreateElement(propInfo.Name);
				xmlElement.InnerText = (string)Convert.ChangeType(propInfo.GetValue(this, null), typeof(string));
				this._settingsElement.AppendChild(xmlElement);
			}
			return xmlElement;
		}
		protected SettingsBase(string settingsDirectory)
		{
			this.RegisterProperties();
			this._path = Path.Combine(settingsDirectory, "settings.xml");
		}
		private void RegisterProperties()
		{
			PropertyInfo[] properties = base.GetType().GetProperties();
			List<PropertyInfo> list = new List<PropertyInfo>();
			PropertyInfo[] array = properties;
			for (int i = 0; i < array.Length; i++)
			{
				PropertyInfo propertyInfo = array[i];
				if (propertyInfo.CanRead && propertyInfo.CanWrite && propertyInfo.GetGetMethod().IsPublic && propertyInfo.GetSetMethod().IsPublic)
				{
					list.Add(propertyInfo);
				}
			}
			this._props = list.ToArray();
		}
		private void New()
		{
			this._doc = new XmlDocument();
			this._doc.AppendChild(this._doc.CreateElement("Settings"));
		}
		public void Load()
		{
			if (File.Exists(this._path))
			{
				this._doc = new XmlDocument();
				if (App.GetStreamForFile(this._path) == null)
				{
					this._doc.Load(this._path);
				}
				else
				{
					this._doc.Load(App.GetStreamForFile(this._path));
				}
			}
			else
			{
				this.New();
			}
			this._settingsElement = this._doc["Settings"];
			PropertyInfo[] props = this._props;
			for (int i = 0; i < props.Length; i++)
			{
				PropertyInfo propertyInfo = props[i];
				XmlElement propertyElement = this.GetPropertyElement(propertyInfo);
				try
				{
					propertyInfo.SetValue(this, Convert.ChangeType(propertyElement.InnerText, propertyInfo.PropertyType), null);
				}
				catch (FormatException)
				{
					App.Log.Warn(string.Format("Could not convert {0} value '{1}' to type {2}. Keeping existing value '{3}'.", new object[]
					{
						propertyInfo.Name,
						propertyElement.InnerText,
						propertyInfo.PropertyType.Name,
						propertyInfo.GetValue(this, null).ToString()
					}), "config");
				}
			}
		}
		public void Save()
		{
			PropertyInfo[] props = this._props;
			for (int i = 0; i < props.Length; i++)
			{
				PropertyInfo propertyInfo = props[i];
				this.GetPropertyElement(propertyInfo).InnerText = (string)Convert.ChangeType(propertyInfo.GetValue(this, null), typeof(string));
			}
			if (App.GetStreamForFile(this._path) == null)
			{
				this._doc.Save(this._path);
				return;
			}
			Stream streamForFile = App.GetStreamForFile(this._path);
			streamForFile.SetLength(0L);
			this._doc.Save(streamForFile);
		}
	}
}
