using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Tiles.Overgrow
{
    internal class BossWindowDummy : Dummy, IMoonlordLayerDrawable
    {
        private readonly ParticleSystem particles = new ParticleSystem("StarlightRiver/Assets/GUI/HolyBig", update);

        public BossWindowDummy() : base(TileType<BossWindow>(), 16, 16) { }

        public override void SafeSetDefaults() => projectile.hide = true;

        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
        {
            drawCacheProjsBehindNPCs.Add(index);
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Vector2 dpos = projectile.Center - Main.screenPosition + Vector2.UnitY * 16;

            Texture2D frametex = GetTexture(AssetDirectory.OvergrowTile + "WindowFrame");
            Texture2D glasstex = GetTexture(AssetDirectory.OvergrowTile + "WindowGlass");

            spriteBatch.Draw(glasstex, dpos, glasstex.Frame(), Color.White * 0.15f, 0, glasstex.Frame().Size() / 2, 1, 0, 0); //glass
            spriteBatch.Draw(frametex, dpos, frametex.Frame(), new Color(255, 255, 200), 0, frametex.Frame().Size() / 2, 1, 0, 0); //frame
        }

        private static ParticleSystem.Update update => UpdateWindowParticles;

        private static void UpdateWindowParticles(Particle particle)
        {
            particle.Color = Color.White * (particle.Timer / 600f) * 0.6f;
            particle.Scale = particle.Timer / 600f * 0.6f;
            particle.Position = particle.StoredPosition + FindOffset(particle.StoredPosition, 0.35f) + particle.Velocity * (600 - particle.Timer) - Main.screenPosition;
            particle.Rotation += 0.1f;

            particle.Timer--;
        }

        public override void Update()
        {
            //Dust
            if (projectile.ai[0] > 0 && projectile.ai[0] < 359) Dust.NewDustPerfect(projectile.Center + Vector2.One.RotatedByRandom(6.28f) * 412, DustType<Dusts.Stone>());

            if (Main.rand.Next(4) == 0 && projectile.ai[0] >= 360)
            {
                float rot = Main.rand.NextFloat(-1.5f, 1.5f);
                Dust.NewDustPerfect(projectile.Center + new Vector2(0, 1).RotatedBy(rot) * 500, DustType<Dusts.GoldSlowFade>(), (new Vector2(0, 1).RotatedBy(rot) + new Vector2(0, 1.6f)) * (0.1f + Math.Abs(rot / 5f)), 0, default, 0.23f + Math.Abs(rot / 5f));
            }

            //Screenshake
            if (projectile.ai[0] < 359 && projectile.ai[0] > 0) Main.LocalPlayer.GetModPlayer<StarlightPlayer>().Shake += (int)(359 - projectile.ai[0]) / 175;

            //Lighting
            for (float k = 0; k <= 6.28f; k += 0.2f)
            {
                Lighting.AddLight(projectile.Center + Vector2.One.RotatedBy(k) * 23 * 16, new Vector3(1, 1, 0.7f) * 0.8f);
            }

            for (int k = 0; k < 6; k++)
            {
                if (projectile.ai[0] > 0)
                {
                    float bright = projectile.ai[0] / 60f; if (bright > 1) bright = 1;
                    Lighting.AddLight(projectile.Center + new Vector2(560 + k * 35, 150 + k * 80), new Vector3(1, 1, 0.7f) * bright);
                    Lighting.AddLight(projectile.Center + new Vector2(-560 - k * 35, 150 + k * 80), new Vector3(1, 1, 0.7f) * bright);
                }

                if (projectile.ai[0] > 60)
                {
                    float bright = (projectile.ai[0] - 60) / 150f; if (bright > 1) bright = 1;
                    Lighting.AddLight(projectile.Center + new Vector2(450 + k * 15, 300 + k * 50), new Vector3(1, 1, 0.7f) * bright);
                    Lighting.AddLight(projectile.Center + new Vector2(-450 - k * 15, 300 + k * 50), new Vector3(1, 1, 0.7f) * bright);
                }

                if (projectile.ai[0] > 210)
                {
                    float bright = (projectile.ai[0] - 210) / 70f; if (bright > 1) bright = 1;
                    Lighting.AddLight(projectile.Center + new Vector2(250 + k * 5, 350 + k * 40), new Vector3(1, 1, 0.7f) * bright);
                    Lighting.AddLight(projectile.Center + new Vector2(-250 - k * 5, 350 + k * 40), new Vector3(1, 1, 0.7f) * bright);
                }

                if (projectile.ai[0] > 280)
                {
                    float bright = (projectile.ai[0] - 280) / 50f; if (bright > 1) bright = 1;
                    Lighting.AddLight(projectile.Center + new Vector2(40, 550 + k * 10), new Vector3(1, 1, 0.7f) * bright);
                    Lighting.AddLight(projectile.Center + new Vector2(-40, 550 + k * 10), new Vector3(1, 1, 0.7f) * bright);
                }
            }

            if (Main.player.Any(p => Vector2.Distance(p.Center, projectile.Center) < 2000) && !Main.npc.Any(n => n.active && n.type == NPCType<Bosses.OvergrowBoss.OvergrowBoss>()) && !StarlightWorld.HasFlag(WorldFlags.OvergrowBossFree))
            {
                NPC.NewNPC((int)projectile.Center.X, (int)projectile.Center.Y + 250, NPCType<Bosses.OvergrowBoss.OvergrowBoss>());

                NPC.NewNPC((int)projectile.Center.X - 790, (int)projectile.Center.Y + 450, NPCType<Bosses.OvergrowBoss.OvergrowBossAnchor>());
                NPC.NewNPC((int)projectile.Center.X + 790, (int)projectile.Center.Y + 450, NPCType<Bosses.OvergrowBoss.OvergrowBossAnchor>());
                NPC.NewNPC((int)projectile.Center.X - 300, (int)projectile.Center.Y + 600, NPCType<Bosses.OvergrowBoss.OvergrowBossAnchor>());
                NPC.NewNPC((int)projectile.Center.X + 300, (int)projectile.Center.Y + 600, NPCType<Bosses.OvergrowBoss.OvergrowBossAnchor>());
            }

            if (StarlightWorld.HasFlag(WorldFlags.OvergrowBossFree) && projectile.ai[0] <= 360) projectile.ai[0]++;
        }

        private static Vector2 FindOffset(Vector2 basepos, float factor, bool noVertical = false)
        {
            Vector2 origin = Main.screenPosition + new Vector2(Main.screenWidth / 2, Main.screenHeight / 2);
            float x = (origin.X - basepos.X) * factor;
            float y = (origin.Y - basepos.Y) * factor * 0.4f;
            return new Vector2(x, noVertical ? 0 : y);
        }

        private Rectangle GetSource(float offset, Texture2D tex)
        {
            int x = tex.Width / 2 - 564;
            int y = tex.Height / 2 - 564;
            Vector2 pos = new Vector2(x, y) - FindOffset(projectile.Center, offset);
            return new Rectangle((int)pos.X, (int)pos.Y + 160, 1128, 1128);
        }

        public void DrawMoonlordLayer(SpriteBatch spriteBatch)
        {
            Vector2 pos = projectile.Center;
            Vector2 dpos = pos - Main.screenPosition;
            Rectangle target = new Rectangle((int)dpos.X - 564, (int)dpos.Y - 564, 1128, 1128);

            if (!Helpers.Helper.OnScreen(target))
                return;

            //background
            Texture2D backtex1 = GetTexture(AssetDirectory.OvergrowTile + "Window4");
            spriteBatch.Draw(backtex1, target, GetSource(0, backtex1), Color.White, 0, Vector2.Zero, 0, 0);


            Texture2D backtex2 = GetTexture(AssetDirectory.OvergrowTile + "Window3");
            spriteBatch.Draw(backtex2, target, GetSource(0.4f, backtex2), Color.White, 0, Vector2.Zero, 0, 0);


            Texture2D backtex3 = GetTexture(AssetDirectory.OvergrowTile + "Window2");
            spriteBatch.Draw(backtex3, target, GetSource(0.3f, backtex3), Color.White, 0, Vector2.Zero, 0, 0);

            //godbeams
            spriteBatch.End();
            spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);

            // Update + draw dusts
            particles.DrawParticles(spriteBatch);
            if (Main.rand.Next(10) == 0) particles.AddParticle(new Particle(Vector2.Zero, new Vector2(0, Main.rand.NextFloat(0.6f, 1.8f)), 0, 1, Color.White, 600, projectile.Center + new Vector2(Main.rand.Next(-350, 350), -580)));

            for (int k = -2; k < 3; k++)
            {
                Texture2D tex2 = GetTexture(AssetDirectory.OvergrowTile + "PitGlow");
                float rot = (float)Main.time / 50 % 6.28f;
                float sin = (float)Math.Sin(rot + k);
                float sin2 = (float)Math.Sin(rot + k * 1.4f);
                float cos = (float)Math.Cos(rot + k * 1.8f);

                Vector2 beampos = dpos + FindOffset(pos, 0.4f + Math.Abs(k) * 0.05f, true) + new Vector2(k * 85 + (k % 2 == 0 ? sin : sin2) * 30, -220);
                Rectangle beamrect = new Rectangle((int)beampos.X - (int)(sin * 30), (int)beampos.Y + (int)(sin2 * 70), 90 + (int)(sin * 30), 700 + (int)(sin2 * 140));

                spriteBatch.Draw(tex2, beamrect, tex2.Frame(), new Color(255, 255, 200) * (1.4f + cos) * 0.9f, 0, tex2.Frame().Size() / 2, SpriteEffects.FlipVertically, 0);
            }

            spriteBatch.End();
            spriteBatch.Begin(default, default, SamplerState.PointWrap, default, default, default, Main.GameViewMatrix.TransformationMatrix);

            for (int k = -9; k < 8; k++)// small waterfalls
            {
                Texture2D watertex = GetTexture(AssetDirectory.OvergrowTile + "Waterfall");
                int frame = (int)Main.time % 16 / 2;
                spriteBatch.Draw(watertex, dpos + new Vector2(100, k * 64) + FindOffset(pos, 0.22f, true), new Rectangle(0, frame * 32, watertex.Width, 32), Color.White * 0.3f, 0, Vector2.Zero, 2, 0, 0);
            }

            //front row
            Texture2D backtex4 = GetTexture(AssetDirectory.OvergrowTile + "Window1");
            spriteBatch.Draw(backtex4, target, GetSource(0.2f, backtex4), Color.White, 0, Vector2.Zero, 0, 0);

            for (int k = -6; k < 6; k++) //big waterfall
            {
                Texture2D watertex = GetTexture(AssetDirectory.OvergrowTile + "Waterfall");
                int frame = (int)Main.time % 16 / 2;
                spriteBatch.Draw(watertex, dpos + new Vector2(300, k * 96) + FindOffset(pos, 0.1f, true), new Rectangle(0, frame * 32, watertex.Width, 32), Color.White * 0.3f, 0, Vector2.Zero, 3, 0, 0);
            }

            if (projectile.ai[0] <= 360) //wall
            {
                Texture2D walltex = GetTexture(AssetDirectory.OvergrowTile + "Dor2");
                Texture2D walltex2 = GetTexture(AssetDirectory.OvergrowTile + "Dor1");
                Rectangle sourceRect = new Rectangle(0, 0, walltex.Width, walltex.Height - (int)(projectile.ai[0] / 360 * 764));
                Rectangle sourceRect2 = new Rectangle(0, (int)(projectile.ai[0] / 360 * 564), walltex2.Width, walltex2.Height - (int)(projectile.ai[0] / 360 * 564));

                spriteBatch.Draw(walltex, dpos + new Vector2(0, 176 + projectile.ai[0] / 360 * 764), sourceRect, new Color(255, 255, 200), 0, walltex.Frame().Size() / 2, 1, 0, 0); //frame
                spriteBatch.Draw(walltex2, dpos + new Vector2(0, -282 - projectile.ai[0] / 360), sourceRect2, new Color(255, 255, 200), 0, walltex2.Frame().Size() / 2, 1, 0, 0); //frame
            }
        }
    }
}