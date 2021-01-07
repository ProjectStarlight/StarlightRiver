using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Bosses.GlassMiniboss;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Helpers;

namespace StarlightRiver.Content.Tiles.Vitric
{
    class ForgeActor : ModTile
    {
        public static GlassMiniboss boss;

        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.Invisible;
            return true;
        }

        public override void SetDefaults() => (this).QuickSetFurniture(1, 1, DustType<Content.Dusts.Air>(), SoundID.Shatter, false, Color.Black);

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Vector2 pos = (new Vector2(i - 9, j + 3) + Helper.TileAdj) * 16 + new Vector2(-464, -336) - Main.screenPosition;
            Texture2D backdrop = GetTexture(AssetDirectory.GlassMiniboss + "Backdrop");
            var frame = new Rectangle(0, (backdrop.Height / 3) * (int)(Main.GameUpdateCount / 2 % 3), backdrop.Width, backdrop.Height / 3);

            spriteBatch.Draw(backdrop, pos, frame, Color.White);
        }
    }
}
