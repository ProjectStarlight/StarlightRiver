using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Items;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Microsoft.Xna.Framework;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Tiles.Overgrow.Deco
{
    class Rock2x2 : ModTile
    {
        public override void SetDefaults()
        {
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.RandomStyleRange = 3;
            QuickBlock.QuickSetFurniture(this, 2, 2, DustID.Stone, SoundID.Tink, false, new Color(50, 50, 75));
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Framing.GetTileSafely(i, j);
            Texture2D tex = GetTexture("StarlightRiver/Tiles/Overgrow/Deco/Rock2x2Glow");
            Vector2 pos = (new Vector2(i, j) + Helper.TileAdj) * 16 - Main.screenPosition;

            spriteBatch.Draw(tex, pos, new Rectangle(tile.frameX, tile.frameY, 16, 16), Color.White);
            Lighting.AddLight(new Vector2(i, j) * 16, new Vector3(110, 200, 225) * 0.0015f);
        }
    }

    class Rock2x2Item : QuickTileItem
    {
        public override string Texture => "StarlightRiver/MarioCumming";

        public Rock2x2Item() : base("2x2 rock placer", "GOD FUCKIGN DAMNIT WHY CANT I JUST FUCK THE BIRD AAAAAAAA", TileType<Rock2x2>(), 7) { }
    }
}
