using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Abilities.ForbiddenWinds;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using Terraria.Graphics.Effects;

namespace StarlightRiver.Content.NPCs.Vitric.Gauntlet
{
    public class GauntletSpawner : ModProjectile
    {
        private List<Vector2> cache;
        private Trail trail;

        public ref float NPCType => ref Projectile.ai[0];
        public ref float Timer => ref Projectile.ai[1];

        public override string Texture => AssetDirectory.Invisible;

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.aiStyle = -1;
        }

        public override void AI()
        {
            ManageCaches();
            ManageTrail();

            if (Timer > 1)
                Timer++;
            else //While being thrown out
            {
                Projectile.velocity.Y += 0.1f;

                if(Main.rand.NextBool(2))
                    Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(10, 10), ModContent.DustType<Cinder>(), Vector2.Zero, 0, new Color(255, 170, 100), 0.65f);

                return;
            }

            if (Timer < 50)
            {
                Vector2 cinderPos = Projectile.Top + Main.rand.NextVector2Circular(40, 40);
                var vel = -Vector2.UnitY.RotatedBy(cinderPos.AngleTo(Projectile.Center)) * Main.rand.NextFloat(-2, 2);
                Dust cinder = Dust.NewDustPerfect(cinderPos, ModContent.DustType<Cinder>(), vel, 0, Bosses.GlassMiniboss.Glassweaver.GlowDustOrange, 0.8f);
                cinder.customData = Projectile.Center + Main.rand.NextVector2Circular(40, 40);
            }

            if (Timer > 70)
            {
                //NPC.NewNPC(Entity.GetSource_Misc("SLR:GlassGauntlet"), (int)Projectile.Center.X, (int)Projectile.Center.Y, (int)NPCType);
                Projectile.Kill();
            }
        }

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
            Projectile.velocity *= 0;
            Timer = 2;
            return false;
		}

		public override bool PreDraw(ref Color lightColor)
        {
            var tex = Terraria.GameContent.TextureAssets.Npc[(int)NPCType].Value;
            var fakeNPC = new NPC();
            fakeNPC.SetDefaults((int)NPCType);
            fakeNPC.FindFrame();

            Effect trailEffect = Filters.Scene["CeirosRing"].GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            trailEffect.Parameters["time"].SetValue(Projectile.timeLeft * -0.04f);
            trailEffect.Parameters["repeats"].SetValue(1);
            trailEffect.Parameters["transformMatrix"].SetValue(world * view * projection);
            trailEffect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/EnergyTrail").Value);

            trail?.Render(trailEffect);

            var glowTex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Keys/GlowAlpha").Value;
            var color = new Color(255, 160, 100);
            color.A = 0;
            Main.spriteBatch.Draw(glowTex, Projectile.Center - Main.screenPosition, null, color * (1 - (Timer / 25f)), 0, glowTex.Size() / 2, Timer * 0.1f, 0, 0);
            Main.spriteBatch.Draw(glowTex, Projectile.Center - Main.screenPosition, null, color * (1 - (Timer / 25f)), 0, glowTex.Size() / 2, Timer * 0.05f, 0, 0);

            Main.spriteBatch.Draw(glowTex, Projectile.Center - Main.screenPosition, null, color * ((25 - Timer) / 25f), 0, glowTex.Size() / 2, 0.75f, 0, 0);
            Main.spriteBatch.Draw(glowTex, Projectile.Center - Main.screenPosition, null, color * 2 * ((25 - Timer) / 25f), 0, glowTex.Size() / 2, 0.25f, 0, 0);

            var effect = Filters.Scene["MoltenForm"].GetShader().Shader;
            effect.Parameters["sampleTexture2"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/Bosses/VitricBoss/ShieldMap").Value);
            effect.Parameters["uTime"].SetValue(Timer / 70f * 2);
            effect.Parameters["sourceFrame"].SetValue(new Vector4(0, 0, fakeNPC.frame.Width, fakeNPC.frame.Height));
            effect.Parameters["texSize"].SetValue(tex.Size());

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(default, BlendState.NonPremultiplied, default, default, default, effect, Main.GameViewMatrix.ZoomMatrix);

            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, fakeNPC.frame, Color.White, 0, fakeNPC.frame.Size() / 2, 1, 0, 0);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);

            return false;
        }

        private void ManageCaches()
        {
            if (cache is null)
            {
                cache = new List<Vector2>();
                for (int i = 0; i < 30; i++)
                {
                    cache.Add(Projectile.Center);
                }
            }

            cache.Add(Projectile.Center);

            while (cache.Count > 30)
            {
                cache.RemoveAt(0);
            }

        }

        private void ManageTrail()
        {
            trail ??= new Trail(Main.instance.GraphicsDevice, 30, new TriangularTip(4), factor => 24, factor =>
            {
                return new Color(255, 200, 165) * factor.X;
            });

            trail.Positions = cache.ToArray();
            trail.NextPosition = Projectile.Center + Projectile.velocity;
        }
    }
}
