using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Projectiles.Dummies
{
    internal class HatchDummy : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("");
        }

        public override string Texture => "StarlightRiver/Invisible";

        public override void SetDefaults()
        {
            projectile.width = 22;
            projectile.height = 22;
            projectile.aiStyle = -1;
            projectile.timeLeft = 2;
            projectile.tileCollide = false;
        }

        public override void AI()
        {
            projectile.timeLeft = 2;
            if (Main.rand.Next(8) == 0)
            {
                float rot = Main.rand.NextFloat(-0.5f, 0.5f);
                Dust.NewDustPerfect(projectile.Center + new Vector2(0, 10).RotatedBy(rot), 244, new Vector2(0, 1).RotatedBy(rot), 0, default, 0.7f);
            }

            if (Main.tile[(int)projectile.Center.X / 16, (int)projectile.Center.Y / 16].type != TileType<Tiles.Overgrow.HatchOvergrow>())
            {
                projectile.timeLeft = 0;
            }
        }

        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
        {
            drawCacheProjsBehindNPCsAndTiles.Add(index);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Vector2 pos = projectile.Center - Main.screenPosition;
            Texture2D tex = GetTexture("StarlightRiver/Tiles/Overgrow/Shine");
            Color col = new Color(160, 160, 120);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.Additive);

            for (int k = 0; k <= 5; k++)
            {
                spriteBatch.Draw(tex, pos, tex.Frame(), col * 0.5f, (float)Math.Sin(StarlightWorld.rottime + k) * 0.5f, new Vector2(8, 0), 1, 0, 0);
                spriteBatch.Draw(tex, pos, tex.Frame(), col * 0.3f, (float)Math.Sin(StarlightWorld.rottime + k + 0.5f) * 0.5f, new Vector2(8, 0), 1.4f, 0, 0);
                spriteBatch.Draw(tex, pos, tex.Frame(), col * 0.6f, (float)Math.Sin(StarlightWorld.rottime + k + 0.7f) * 0.3f, new Vector2(8, 0), 1.2f, 0, 0);
            }

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend);
            return false;
        }
    }
}