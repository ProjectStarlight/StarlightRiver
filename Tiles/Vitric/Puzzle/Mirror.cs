using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Items;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Tiles.Vitric.Puzzle
{
    class Mirror : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = "StarlightRiver/Invisible";
            return true;
        }

        public override void SetDefaults() => QuickBlock.QuickSetFurniture(this, 1, 1, DustType<Dusts.Air>(), SoundID.Tink, false, new Color(0, 255, 255), false, true, "Mirror");

        public override bool NewRightClick(int i, int j)
        {
            Tile tile = Main.tile[i, j];

            //0 = topleft, 1 = topright, 2 = bottomright, 3 = bottomleft
            if (tile.frameX < 3) tile.frameX++; //x frame is rotation
            else tile.frameX = 0;

            Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/StoneSlide"), new Vector2(i, j) * 16);

            return true;
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Main.tile[i, j];
            Texture2D tex = GetTexture("StarlightRiver/Tiles/Vitric/Puzzle/MirrorOver");

            spriteBatch.Draw(tex, (new Vector2(i + 0.5f, j + 0.5f) + Helper.TileAdj) * 16 - Main.screenPosition, tex.Frame(), Lighting.GetColor(i, j), tile.frameX * 1.57f, tex.Size() / 2 + Vector2.One * 4, 1, 0, 0);
        }
    }

    class MirrorItem : QuickTileItem
    {
        public MirrorItem() : base("Light Mirror", "", TileType<Mirror>(), 1) { }
    }
}
