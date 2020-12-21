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
            texture = Directory.Invisible;
            return true;
        }

        public override void SetDefaults() => (this).QuickSetFurniture(1, 1, DustType<Content.Dusts.Air>(), SoundID.Shatter, false, Color.Black);

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Vector2 pos = (new Vector2(i + 3, j + 3) + Helper.TileAdj) * 16 + new Vector2(-464, -336) - Main.screenPosition;
            Texture2D backdrop = GetTexture(Directory.GlassMiniboss + "Backdrop");

            spriteBatch.Draw(backdrop, pos, Color.White);

            if (boss is null || !boss.npc.active)
            {
                Vector2 forgeOff1 = new Vector2(36, 324);
                Vector2 forgeOff2 = new Vector2(736, 324);
                var tex = GetTexture(Directory.GlassMiniboss + "Forge");

                spriteBatch.Draw(tex, pos + forgeOff1, Color.White);
                spriteBatch.Draw(tex, pos + forgeOff2, Color.White);
            }
        }
    }
}
