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
			var worldPos = new Vector2(i, j) * 16 + new Vector2(8, 42);
			var frame = new Rectangle(2 * tex.Width / 4, 0, tex.Width / 4, tex.Height);
			var rot = MathF.Sin(Main.windCounter * 0.05f + i * 0.1f) * Main.windSpeedCurrent * 0.05f + Main.windSpeedCurrent * 0.1f;

			FoliageLayerSystem.overTilesData.Add(new(tex, worldPos, frame, new Color(Lighting.GetSubLight(worldPos)), rot, new Vector2(frame.Width / 2f, frame.Height), 1f, 0, 0));
		}

		public override void NearbyEffects(int i, int j, bool closer)
		{
			// This is very annoying, trees dont properly call TileFrame when changed or destroyed for all adjacent tiles so we
			// need to check seperate for if this should still exist.

			if (!WorldGenHelper.ScanForTypeDown(i, j, TileID.CrimsonGrass))
				WorldGen.KillTile(i, j);

			var down = Framing.GetTileSafely(i, j + 1);

			if (!down.HasTile || down.TileType != TileID.Trees)
				WorldGen.KillTile(i, j);
		}

		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
		{
			// We also still need the proper checks here for world generation logic. 

			if (!WorldGenHelper.ScanForTypeDown(i, j, TileID.CrimsonGrass))
				WorldGen.KillTile(i, j);

			var down = Framing.GetTileSafely(i, j + 1);

			if (!down.HasTile || down.TileType != TileID.Trees)
				WorldGen.KillTile(i, j);

			return true;
		}
	}

	internal class NoVanillaTopper : GlobalTile
	{
		public override bool TileFrame(int i, int j, int type, ref bool resetFrame, ref bool noBreak)
		{
			if (type == TileID.Trees)
			{
				var up = Framing.GetTileSafely(i, j - 1);

				if (up.HasTile && up.TileType == ModContent.TileType<MeatballTreeTopper>())
				{
					Framing.GetTileSafely(i, j).TileFrameX = 0;
					Framing.GetTileSafely(i, j).TileFrameY = 0;

					WorldGen.TileFrame(i, j - 1);

					return false;
				}
			}

			return true;
		}
	}
}
