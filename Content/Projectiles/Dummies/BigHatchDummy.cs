using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Projectiles.Dummies
{
    internal class BigHatchDummy : Dummy, IDrawAdditive
    {
        public BigHatchDummy() : base(TileType<Tiles.Overgrow.BigHatchOvergrow>(), 16, 16) { }
        public override void Update()
        {
            projectile.ai[0] += 0.01f;
            if (projectile.ai[0] >= 6.28f) projectile.ai[0] = 0;
            if (Main.rand.Next(5) == 0)
            {
                float rot = Main.rand.NextFloat(-0.7f, 0.7f);
                Dust.NewDustPerfect(projectile.Center + new Vector2(24, -24), DustType<Dusts.Gold4>(),
                    new Vector2(0, 0.4f).RotatedBy(rot + 0.7f), 0, default, 0.4f - Math.Abs(rot) / 0.7f * 0.2f);
            }
        }
        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            Vector2 pos = projectile.Center + new Vector2(24, -32) - Main.screenPosition;
            Texture2D tex = GetTexture("StarlightRiver/Tiles/Overgrow/Shine");
            Color col = new Color(160, 160, 120);

            for (int k = 0; k <= 5; k++)
            {
                spriteBatch.Draw(tex, pos, tex.Frame(), col * 0.4f, (float)Math.Sin(projectile.ai[0] + k) * 0.5f + 0.7f, new Vector2(8, 0), 2.6f, 0, 0);
                spriteBatch.Draw(tex, pos, tex.Frame(), col * 0.3f, (float)Math.Sin(projectile.ai[0] + k + 0.5f) * 0.6f + 0.7f, new Vector2(8, 0), 4f, 0, 0);
                spriteBatch.Draw(tex, pos, tex.Frame(), col * 0.5f, (float)Math.Sin(projectile.ai[0] + k + 0.9f) * 0.3f + 0.7f, new Vector2(8, 0), 3.2f, 0, 0);
            }
        }
    }
}