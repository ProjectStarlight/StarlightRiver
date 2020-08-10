using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Tiles.Vitric
{
    internal class VitricCrystalCollision : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = "StarlightRiver/Invisible";
            return true;
        }

        public override void SetDefaults()
        {
            QuickBlock.QuickSet(this, 0, DustType<Dusts.Air>(), SoundID.CoinPickup, new Color(115, 182, 158), -1);
            Main.tileBlockLight[Type] = false;
            Main.tileFrameImportant[Type] = true;
            TileID.Sets.DrawsWalls[Type] = true;
        }

        public override bool CanExplode(int i, int j)
        {
            return false;
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile t = Main.tile[i, j];
            if (t.frameX > 0)
            {
                Texture2D tex = GetTexture("StarlightRiver/Tiles/Vitric/LargeCrystal");
                spriteBatch.Draw(tex, ((new Vector2(i, j) + Helper.TileAdj) * 16) - Main.screenPosition, tex.Frame(2, 1, t.frameX - 1), Color.White, 0, new Vector2(80, 176), 1, 0, 0);
            }
        }
    }
}