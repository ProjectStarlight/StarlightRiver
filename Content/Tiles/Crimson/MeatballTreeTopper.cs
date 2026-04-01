using StarlightRiver.Core.Systems.FoliageLayerSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Enums;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace StarlightRiver.Content.Tiles.Crimson
{
	internal class MeatballTreeTopper : ModTile
	{
		public override string Texture => "StarlightRiver/Assets/Tiles/Crimson/MeatballTreeTopper";

		public override void SetStaticDefaults()
		{
			Main.tileSolid[Type] = false;

			TileObjectData.newTile.AnchorBottom = new(AnchorType.AlternateTile | AnchorType.SolidTile, 1, 0);
			TileObjectData.newTile.AnchorAlternateTiles = [TileID.Trees, Type];
			QuickBlock.QuickSetFurniture(this, 1, 1, DustID.Blood, SoundID.Grass, Color.Red);
		}

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
		{
			Main.instance.TilesRenderer.AddSpecialPoint(i, j, TileDrawing.TileCounterType.CustomNonSolid);
			return false;
		}

		public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
		{
			if (StarlightRiver.debugMode && Main.LocalPlayer.controlHook)
				FoliageLayerSystem.overTilesData.Add(new(Terraria.GameContent.TextureAssets.MagicPixel.Value, new Rectangle(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y, 16, 16), Color.White));

			Texture2D tex = Assets.Tiles.Crimson.MeatballTreeTopper.Value;
			var worldPos = new Vector2(i, j) * 16;

				var frame = new Rectangle(2 * tex.Width / 4, 0, tex.Width / 4, tex.Height);
				FoliageLayerSystem.overTilesData.Add(new(tex, worldPos, frame, new Color(Lighting.GetSubLight(worldPos)), 0f, frame.Size() / 2f, 1f, 0, 0));
		}
	}
}
