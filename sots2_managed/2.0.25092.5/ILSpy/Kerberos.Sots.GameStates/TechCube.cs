using Kerberos.Sots.Data;
using Kerberos.Sots.Data.TechnologyFramework;
using Kerberos.Sots.Engine;
using System;
using System.Linq;
namespace Kerberos.Sots.GameStates
{
	[GameObjectType(InteropGameObjectType.IGOT_TECHCUBE)]
	internal class TechCube : GameObject, IActive, IDisposable
	{
		private new App App;
		private bool _active;
		private float _spinSpeed;
		private string _familyTexture = "";
		private string _techTexture = "";
		public string FamilyTexture
		{
			get
			{
				return this._familyTexture;
			}
			set
			{
				if (this._familyTexture != value)
				{
					this._familyTexture = value;
					this.PostSetProp("SetFamilyTexture", value);
				}
			}
		}
		public string TechTexture
		{
			get
			{
				return this._techTexture;
			}
			set
			{
				if (this._techTexture != value)
				{
					this._techTexture = value;
					this.PostSetProp("SetTechTexture", value);
				}
			}
		}
		public float SpinSpeed
		{
			get
			{
				return this._spinSpeed;
			}
			set
			{
				this._spinSpeed = value;
				this.PostSetProp("SpinSpeed", value);
			}
		}
		public bool Active
		{
			get
			{
				return this._active;
			}
			set
			{
				this._active = value;
				this.PostSetActive(true);
				if (value)
				{
					this.App.UI.Send(new object[]
					{
						"SetGameObject",
						"researchCubeButton.idle",
						base.ObjectID
					});
					this.App.UI.Send(new object[]
					{
						"SetGameObject",
						"researchCubeButton.mouse_over",
						base.ObjectID
					});
					this.App.UI.Send(new object[]
					{
						"SetGameObject",
						"researchCubeButton.pressed",
						base.ObjectID
					});
				}
			}
		}
		public void UpdateResearchProgress()
		{
			int playerResearchingTechID = this.App.GameDatabase.GetPlayerResearchingTechID(this.App.LocalPlayer.ID);
			if (playerResearchingTechID != 0)
			{
				PlayerTechInfo playerTechInfo = this.App.GameDatabase.GetPlayerTechInfo(this.App.LocalPlayer.ID, playerResearchingTechID);
				float value = (float)playerTechInfo.Progress / (float)playerTechInfo.ResearchCost;
				this.PostSetProp("AmountResearched", value);
				return;
			}
			this.PostSetProp("AmountResearched", 0f);
		}
		public void RefreshResearchingTech()
		{
			string techID = this.App.GameDatabase.GetTechFileID(this.App.GameDatabase.GetPlayerResearchingTechID(this.App.LocalPlayer.ID));
			string techTexture = "Tech\\Icons\\question_mark.bmp";
			string familyTexture = "Tech\\Icons\\question_mark.bmp";
			if (techID != null)
			{
				Tech tech2 = this.App.AssetDatabase.MasterTechTree.Technologies.First((Tech tech) => tech.Id == techID);
				if (tech2 != null)
				{
					techTexture = tech2.GetProperIconPath();
					familyTexture = "Tech\\Icons\\Research_Icon.bmp";
				}
			}
			else
			{
				techID = this.App.GameDatabase.GetTechFileID(this.App.GameDatabase.GetPlayerFeasibilityStudyTechId(this.App.LocalPlayer.ID));
				if (techID != null)
				{
					Tech techno = this.App.AssetDatabase.MasterTechTree.Technologies.First((Tech tech) => tech.Id == techID);
					if (techno != null)
					{
						this.App.AssetDatabase.MasterTechTree.TechFamilies.First((TechFamily x) => x.Id == techno.Family);
						techTexture = techno.GetProperIconPath();
					}
				}
			}
			this.FamilyTexture = familyTexture;
			this.TechTexture = techTexture;
		}
		public TechCube(App game)
		{
			game.AddExistingObject(this, new object[0]);
			this.App = game;
		}
		public void Dispose()
		{
			if (this.App != null)
			{
				this.App.ReleaseObject(this);
			}
		}
	}
}
