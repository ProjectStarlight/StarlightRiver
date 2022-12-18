using StarlightRiver.Helpers;
using System;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Mushroom
{
	class Lumishroom : ModTile
	{
		public override string Texture => "StarlightRiver/Assets/Tiles/Mushroom/" + Name;

		public override void SetStaticDefaults()
		{
			Main.tileLighted[Type] = true;
			Main.tileCut[Type] = true;

			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidWithTop | AnchorType.SolidTile, 1, 0);
			TileObjectData.newTile.RandomStyleRange = 5;
			TileObjectData.newTile.StyleHorizontal = true;

			ItemDrop = ItemType<LumishroomItem>();

			this.QuickSetFurniture(1, 1, 61, SoundID.Grass, false, Color.Green);
			AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			(r, g, b) = (0.2f, 0.075f, 0.275f);
		}

		public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
		{
			Tile tile = Framing.GetTileSafely(i, j);
			Texture2D tex = Request<Texture2D>("StarlightRiver/Assets/Tiles/Mushroom/LumishroomGlow").Value;
			Vector2 pos = (new Vector2(i, j) + Helper.TileAdj) * 16 - Main.screenPosition;

			spriteBatch.Draw(tex, pos, new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16), Color.White * (float)(0.8f + Math.Sin(StarlightWorld.visualTimer + i) * 0.5f));
		}
	}

	class LumishroomItem : QuickTileItem
	{
		public LumishroomItem() : base("Lumishroom", "Glowy...", "Lumishroom", 1, "StarlightRiver/Assets/Tiles/Mushroom/") { }
	}
}
