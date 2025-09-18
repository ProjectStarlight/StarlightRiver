using StarlightRiver.Content.Biomes;
using StarlightRiver.Content.Tiles.BaseTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Content.Tiles.Permafrost
{
	internal class AuroraPylon : ProximityActivatedPylon
	{
		public override string ID => "AuroraPylon";

		public override string Texture => AssetDirectory.PermafrostTile + Name;

		public override Asset<Texture2D> MapIcon => Assets.Tiles.Permafrost.AuroraPylon_MapIcon;

		public override Asset<Texture2D> CrystalTexture => Assets.Tiles.Permafrost.AuroraPylon_Crystal;

		public override Asset<Texture2D> CrystalHighlightTexture => Assets.Tiles.Permafrost.AuroraPylon_CrystalHighlight;

		public override Color PrimaryColor => new(120, 120, 255);

		public override Color SecondaryColor => new(220, 110, 255);

		public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
		{
			if (Main.LocalPlayer.InModBiome<PermafrostTempleBiome>())
				base.SpecialDraw(i, j, spriteBatch);
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			if (Main.LocalPlayer.InModBiome<PermafrostTempleBiome>())
				base.ModifyLight(i, j, ref r, ref g, ref b);
		}
	}

	internal class AuroraPylonItem : QuickTileItem
	{
		public AuroraPylonItem() : base("Aurora Pylon", "You shouldn't have this!", "AuroraPylon", 0, AssetDirectory.Debug, true, 0) { }
	}
}