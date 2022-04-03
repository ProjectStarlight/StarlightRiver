using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Bosses.GlassMiniboss
{
	class GlassHammer : ModProjectile
    {
        Vector2 origin;

        public NPC Parent => Main.npc[(int)Projectile.ai[1]];

        public override string Texture => "StarlightRiver/Assets/Bosses/GlassMiniboss/GlassHammer";

        public override void SetStaticDefaults() => DisplayName.SetDefault("Woven Hammer");

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.hostile = true;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = false;
            Projectile.aiStyle = -1;
            Projectile.penetrate = -1;
        }

        public override void AI()
        {
            if (Projectile.timeLeft == 60)
            {
                origin = Projectile.Center; //sets origin when spawned
                Projectile.NewProjectile(Parent.Center, Vector2.UnitX * 10, ProjectileType<Shockwave>(), 22, 0, Main.myPlayer); //Shockwave spawners
                Projectile.NewProjectile(Parent.Center, Vector2.UnitX * -10, ProjectileType<Shockwave>(), 22, 0, Main.myPlayer);
            }

            if (Projectile.timeLeft >= 30)
            {
                float radius = (60 - Projectile.timeLeft) * 2;
                float rotation = -(60 - Projectile.timeLeft) / 30f * 0.8f; //ai 0 is direction

                Projectile.Center = origin - Vector2.UnitY.RotatedBy(rotation * Projectile.ai[0]) * radius;
            }
            else if (Projectile.timeLeft >= 1)
            {
                float rotation = -0.8f + (60 - Projectile.timeLeft - 30) / 30f * ((float)Math.PI / 2 + 1.2f);

                Projectile.Center = origin - Vector2.UnitY.RotatedBy(rotation * Projectile.ai[0]) * 120;

                if (Projectile.timeLeft == 1)
                {
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Shatter, Projectile.Center);
                    Main.LocalPlayer.GetModPlayer<StarlightPlayer>().Shake += 15;

                    for (int k = 0; k < 30; k++)
                    {
                        Vector2 vector = Vector2.UnitY.RotatedByRandom((float)Math.PI / 2);
                        Dust.NewDustPerfect(Projectile.Center + vector * Main.rand.NextFloat(25), DustType<Dusts.Sand>(), vector * Main.rand.NextFloat(3, 5), 150, Color.White, 0.5f);
                    }
                }
            }
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            if (Projectile.timeLeft <= 30 && Projectile.timeLeft >= 20)
                target.AddBuff(BuffType<Buffs.Squash>(), 180);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Rectangle frame = new Rectangle(0, 166 * (int)((60 - Projectile.timeLeft) / 40f * 12), 214, 166);
            if (Projectile.timeLeft <= 20) frame.Y = 12 * 166;
            spriteBatch.Draw(Request<Texture2D>(Texture).Value, origin + new Vector2(-100, -130) - Main.screenPosition, frame, Color.White, 0, Vector2.Zero, 1, Projectile.ai[0] == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);

            return false;
        }
    }

    class Shockwave : ModProjectile
    {
        public override string Texture => AssetDirectory.Invisible;

        public override void SetStaticDefaults() => DisplayName.SetDefault("Shockwave");

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.timeLeft = 150;
            Projectile.aiStyle = -1;
            Projectile.penetrate = -1;
        }

        public override bool OnTileCollide(Vector2 oldVelocity) => false;

        public override void AI()
        {
            Tile tile = Framing.GetTileSafely((int)Projectile.Center.X / 16 + (Projectile.velocity.X > 0 ? 1 : -1), (int)Projectile.Center.Y / 16);
            //Main.NewText(tile.type);//debug

            if (Projectile.timeLeft < 140 && tile.type == Mod.TileType("VitricGlass"))
                Projectile.position.Y -= 128;

            if (Projectile.timeLeft < 150 && Projectile.velocity.Y == 0 && Projectile.timeLeft % 20 == 0)
                Projectile.NewProjectile(Projectile.Center + Vector2.UnitY * 16, Vector2.Zero, ProjectileType<ShockwaveSpike>(), Projectile.damage, 0, Projectile.owner);

            if (Projectile.velocity.X == 0)
                Projectile.Kill();

            Projectile.velocity.Y = 50;
        }
    }

    class ShockwaveSpike : ModProjectile, IDrawAdditive
    {
        public override string Texture => AssetDirectory.Invisible;

        public override void SetStaticDefaults() => DisplayName.SetDefault("Stone Pillar");

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 1;
            Projectile.hostile = true;
            Projectile.hide = true;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = false;
            Projectile.aiStyle = -1;
            Projectile.penetrate = -1;
        }

        public override void AI()
        {
            Projectile.ai[0]++; //ticks up the timer

            if (Projectile.ai[0] == 1) Projectile.ai[1] = Projectile.position.Y;

            if (Projectile.ai[0] == 50)
            {
                Projectile.hostile = true; //when this Projectile goes off

                for (int k = 0; k < 5; k++)
                {
                    Dust.NewDustPerfect(Projectile.Center + Vector2.UnitY * -8, DustType<Dusts.Stone>(), Vector2.UnitY.RotatedByRandom(1) * Main.rand.NextFloat(-4, -2));
                    Dust.NewDustPerfect(Projectile.Center + Vector2.UnitY * -8, DustType<Dusts.Glow>(), Vector2.UnitY.RotatedByRandom(1) * Main.rand.NextFloat(-2, 0), 0, new Color(255, 160, 80), 0.4f);
                }

                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item70, Projectile.Center);
            }

            if (Projectile.ai[0] >= 50)
            {
                int off = (int)(Helpers.Helper.SwoopEase((Projectile.ai[0] - 50) / 30f) * 170);

                if (Projectile.ai[0] > 80)
                    off = 170 - (int)((Projectile.ai[0] - 80) / 40f * 170);

                Projectile.position.Y = Projectile.ai[1] - off;
                Projectile.height = off;
            }
        }

		public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
		{
            drawCacheProjsBehindNPCsAndTiles.Add(index);
		}

		public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if (Projectile.ai[0] > 50)
            {
                Texture2D tex = Request<Texture2D>("StarlightRiver/Assets/Bosses/GlassMiniboss/Spike").Value;

                int off = (int)(Helpers.Helper.SwoopEase((Projectile.ai[0] - 50) / 30f) * 170);

                if (Projectile.ai[0] > 80)
                    off = 170 - (int)((Projectile.ai[0] - 80) / 40f * 170);

                Rectangle targetRect = new Rectangle((int)(Projectile.position.X - Main.screenPosition.X) + off / 4, (int)(Projectile.ai[1] - off + 26 - Main.screenPosition.Y), tex.Width, off);
                Rectangle sourceRect = new Rectangle(0, 0, tex.Width, off);
                spriteBatch.Draw(tex, targetRect, sourceRect, lightColor, 0, Vector2.Zero, 0, 0);


            }
        }

        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            if (Projectile.ai[0] <= 50)
            {
                Texture2D tex = Request<Texture2D>("StarlightRiver/Assets/Bosses/GlassMiniboss/SpikeTell").Value;
                float factor = 2 * Projectile.ai[0] / 25f - (float)Math.Pow(Projectile.ai[0], 2) / 625f;
                Color color = new Color(255, 180, 50) * factor;

                spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, color, 0, new Vector2(tex.Width / 2, tex.Height + 8), 2, 0, 0);
                spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White * factor, 0, new Vector2(tex.Width / 2, tex.Height + 8), 1, 0, 0);
                Lighting.AddLight(Projectile.Center, color.ToVector3() * 0.75f);
            }

        }
    }
}
