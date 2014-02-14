using Kerberos.Sots.Engine;
using System;
namespace Kerberos.Sots.UI
{
	internal class Image : PanelBinding
	{
		public string TextureAssetPath
		{
			get;
			private set;
		}
		public string SpriteName
		{
			get;
			private set;
		}
		public void SetTexture(string textureAssetPath)
		{
			if (textureAssetPath == null)
			{
				textureAssetPath = string.Empty;
			}
			this.TextureAssetPath = textureAssetPath;
			this.SpriteName = null;
			base.UI.SetPropertyString(base.ID, "texture", textureAssetPath);
		}
		public void SetSprite(string spriteName)
		{
			if (spriteName == null)
			{
				spriteName = string.Empty;
			}
			this.TextureAssetPath = null;
			this.SpriteName = spriteName;
			base.UI.SetPropertyString(base.ID, "sprite", spriteName);
		}
		public Image(UICommChannel ui, string id) : base(ui, id)
		{
		}
	}
}
