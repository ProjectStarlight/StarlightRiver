using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Items.Permafrost;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Misc
{
    internal class AuroraDiscTile : DummyTile
    {
        public override int DummyType => ProjectileType<AuroraDiscTileDummy>();

        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.Invisible;
            return true;
        }

        public override void SetDefaults()
        {
            QuickBlock.QuickSetFurniture(this, 2, 2, DustType<Crystal>(), SoundID.Shatter, false, Color.White, false, false, "Mysterious Disc");
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            StarlightWorld.LearnRecipie("Aurora Disc");
            Item.NewItem(new Vector2(i, j) * 16, ItemType<AuroraDisc>());
        }
    }

    internal class AuroraDiscTileDummy : Dummy, IDrawAdditive
    {
        public AuroraDiscTileDummy() : base(TileType<AuroraDiscTile>(), 32, 32) { }

        public override void Update()
        {
            projectile.ai[0]++;

            for (int k = 0; k < 10; k++)
            {
                float radius = 60 + (float)Math.Sin(projectile.ai[0] / 20f) * 40;
                float angleOff = projectile.ai[0] / 40f;

                float angle = k / 10f * 6.28f + angleOff;

                var off = Vector2.One.RotatedBy(angle) * radius;

                float sin = 1 + (float)Math.Sin(-StarlightWorld.rottime + k / 10f * 6.28f);
                float cos = 1 + (float)Math.Cos(-StarlightWorld.rottime + k / 10f * 6.28f);
                Color color = new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f) * 1.1f;

                int alpha = (int)(255 * (0.5f + radius / 120f * 0.5f));

                Dust.NewDustPerfect(projectile.Center + off, DustType<Aurora>(), Vector2.Zero, alpha, color, 1);
            }
        }

        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            var tex = GetTexture("StarlightRiver/Assets/Keys/Glow");
            var texDisc = GetTexture("StarlightRiver/Assets/Items/Permafrost/AuroraDisc");

            float sin = 1 + (float)Math.Sin(-StarlightWorld.rottime);
            float cos = 1 + (float)Math.Cos(-StarlightWorld.rottime);
            Color color = new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f) * 1.1f;

            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, color * 0.75f, 0, tex.Size() / 2, 1, 0, 0);
            spriteBatch.Draw(texDisc, projectile.Center - Main.screenPosition, null, Lighting.GetColor((int)projectile.Center.X / 16, (int)projectile.Center.Y / 16), 0, texDisc.Size() / 2, 1, 0, 0);

            Lighting.AddLight(projectile.Center, color.ToVector3() * 0.5f);
        }
    }

    class DebugDisc : QuickTileItem
    {
        public override string Texture => AssetDirectory.Debug;

        public DebugDisc() : base("Debug Disc Placer", "Dragon Sex", TileType<AuroraDiscTile>(), 1) { }
    }
}