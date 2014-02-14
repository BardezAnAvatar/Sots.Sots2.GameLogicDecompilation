using Kerberos.Sots.Engine;
using System;
namespace Kerberos.Sots.UI
{
	internal class ImageLabel : PanelBinding
	{
		private readonly Image _image;
		private readonly Label _label;
		public Image Image
		{
			get
			{
				return this._image;
			}
		}
		public Label Label
		{
			get
			{
				return this._label;
			}
		}
		public ImageLabel(UICommChannel ui, string id) : base(ui, id)
		{
			this._image = new Image(ui, base.UI.Path(new string[]
			{
				base.ID,
				"icon"
			}));
			this._label = new Label(ui, base.UI.Path(new string[]
			{
				base.ID,
				"label"
			}));
		}
	}
}
