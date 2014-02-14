using Kerberos.Sots.Engine;
using System;
namespace Kerberos.Sots.UI
{
	internal class ImageButton : Button
	{
		public Image IdleImage
		{
			get;
			private set;
		}
		public Image MouseOverImage
		{
			get;
			private set;
		}
		public Image PressedImage
		{
			get;
			private set;
		}
		public Image DisabledImage
		{
			get;
			private set;
		}
		public void SetTexture(string textureAssetPath)
		{
			this.IdleImage.SetTexture(textureAssetPath);
			this.MouseOverImage.SetTexture(textureAssetPath);
			this.PressedImage.SetTexture(textureAssetPath);
			this.DisabledImage.SetTexture(textureAssetPath);
		}
		public void SetSprite(string spriteAssetPath)
		{
			this.IdleImage.SetSprite(spriteAssetPath);
			this.MouseOverImage.SetSprite(spriteAssetPath);
			this.PressedImage.SetSprite(spriteAssetPath);
			this.DisabledImage.SetSprite(spriteAssetPath);
		}
		public ImageButton(UICommChannel ui, string id, string createFromTemplateID = null) : base(ui, id, createFromTemplateID)
		{
			this.IdleImage = new Image(base.UI, base.UI.Path(new string[]
			{
				base.ID,
				"idle"
			}));
			this.MouseOverImage = new Image(base.UI, base.UI.Path(new string[]
			{
				base.ID,
				"mouse_over"
			}));
			this.PressedImage = new Image(base.UI, base.UI.Path(new string[]
			{
				base.ID,
				"pressed"
			}));
			this.DisabledImage = new Image(base.UI, base.UI.Path(new string[]
			{
				base.ID,
				"disabled"
			}));
		}
	}
}
