using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Tiles.Misc
{
    internal class SandscriptTile : ModTile
    {
        public override void SetDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            drop = ItemType<Items.Misc.Sandscript>();
            dustType = DustID.Gold;
        }

        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref Color drawColor, ref int nextSpecialDrawIndex)
        {
            if (Main.rand.Next(2) == 0) Dust.NewDustPerfect(new Vector2(i + Main.rand.NextFloat(), j + Main.rand.NextFloat()) * 16, 204, new Vector2(0, Main.rand.NextFloat(1, 1.6f)), 0, default, 0.5f);
        }
    }
}