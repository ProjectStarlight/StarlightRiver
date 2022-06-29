using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Bosses.VitricBoss
{
	class FinalLaser : ModProjectile, IDrawAdditive
    {
        public VitricBoss parent;

        public override string Texture => AssetDirectory.VitricBoss + Name;

        public ref float Timer => ref Projectile.ai[0];
        public ref float LaserRotation => ref Projectile.ai[1];

        private float LaserTimer => (Timer - 120) % 400;

        public int direction = -1;
        public Vector2 endpoint = Vector2.Zero;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Raging Fire");
        }

        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 64;
            Projectile.hostile = true;
        }

		public override bool PreDraw(ref Color lightColor)
		{
            return false;
		}

		public void findParent()
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC NPC = Main.npc[i];
                if (NPC.active && NPC.type == ModContent.NPCType<VitricBoss>())
                {
                    parent = NPC.ModNPC as VitricBoss;
                    return;
                }
            }
            return;
        }

        public override void AI()
        {
            if (parent is null)
                findParent();

            if (parent is null)
                return;

            Timer++;
            Projectile.timeLeft = 2;

            Projectile.Center = parent.NPC.Center + new Vector2(4, -4);

            if (Timer < 120 && Main.masterMode)
                Projectile.extraUpdates = 2;
            else
                Projectile.extraUpdates = 0;

            if (Timer < 60)
			{
                for (int k = 0; k < 3; k++)
                {
                    var rot = Main.rand.NextFloat(6.28f);
                    Dust.NewDustPerfect(Projectile.Center + Vector2.One.RotatedBy(rot) * 100, DustType<Dusts.Glow>(), Vector2.One.RotatedBy(rot) * -3, 0, new Color(255, Main.rand.Next(200, 255), 100), (1 - (Timer / 60f)));
                }

                Projectile.scale = Math.Min(1, (Timer / 60f));
            }

            if (Timer > 120)
            {
                //if (LaserTimer == 1)
                //    Helpers.Helper.PlayPitched("VitricBoss/CeirosLaser", 1, 0, Projectile.Center);

                if (LaserTimer == 140)
                    direction = (Main.player[parent.NPC.target].Center - Projectile.Center).ToRotation() > LaserRotation ? 1 : -1;

                if (LaserTimer > 30 && LaserTimer <= 75)
                {
                    Projectile.netUpdate = true;
                    LaserRotation = (Main.player[parent.NPC.target].Center - Projectile.Center).ToRotation();

                    var rot = Main.rand.NextFloat(6.28f);
                    Dust.NewDustPerfect(Projectile.Center + Vector2.One.RotatedBy(rot) * 100, DustType<Dusts.Glow>(), Vector2.One.RotatedBy(rot) * -3, 0, Color.Yellow, (LaserTimer - 30) / 45f);
                }

                if (LaserTimer == 135)
                    Helpers.Helper.PlayPitched("VitricBoss/LaserFire", 1.0f, 0, Projectile.Center);

                //if (LaserTimer == 150)
                //Terraria.Audio.SoundEngine.PlaySound(SoundID.DD2_BetsyFlameBreath);

                if (LaserTimer > 150) //laser is actually active
                {
                    parent.NPC.position += Vector2.Normalize(endpoint - Projectile.Center) * -4.0f;

                    float laserSpeed = Main.masterMode ? 0.019f : Main.expertMode ? 0.017f : 0.014f;

                    for (int k = 0; k < 160; k++) //raycast to find the laser's endpoint
                    {
                        Vector2 posCheck = Projectile.Center + Vector2.UnitX.RotatedBy(LaserRotation) * k * 8;

                        if (!parent.arena.Contains(posCheck.ToPoint()))
                        {
                            endpoint = posCheck;
                            break;
                        }
                    }

                    LaserRotation += laserSpeed * direction;

                    for (int k = 0; k < Main.maxPlayers; k++) //laser colission
                    {
                        Vector2 point;
                        var Player = Main.player[k];

                        if (Player.active && !Player.dead && Helpers.Helper.CheckLinearCollision(Projectile.Center, endpoint, Player.Hitbox, out point))
                        {
                            Player.Hurt(Terraria.DataStructures.PlayerDeathReason.ByCustomReason(Player.name + " was reduced to ash"), Main.masterMode ? 9999999 : Main.expertMode ? 65 : 45, 0, false, false, false, 5);
                            endpoint = point;
                            break;
                        }
                    }
                }
            }

            if(Timer > 500 || parent.Phase == (int)VitricBoss.AIStates.Dying || parent.Phase == (int)VitricBoss.AIStates.Leaving)
			{
                Projectile.scale -= 0.05f;

                if (Projectile.scale <= 0)
                    Projectile.active = false;
			}

        }

        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            if (parent is null)
                return;

            Texture2D texGlow = Request<Texture2D>("StarlightRiver/Assets/Keys/Glow").Value;

            int sin = (int)(Math.Sin(StarlightWorld.rottime * 3) * 40f);
            var color = new Color(255, 160 + sin, 40 + sin / 2);

            spriteBatch.Draw(texGlow, Projectile.Center - Main.screenPosition, null, color * Projectile.scale, 0, texGlow.Size() / 2, Projectile.scale * 1.0f, default, default);
            spriteBatch.Draw(texGlow, Projectile.Center - Main.screenPosition, null, color * Projectile.scale * 1.2f, 0, texGlow.Size() / 2, Projectile.scale * 1.6f, default, default);

            var effect1 = Terraria.Graphics.Effects.Filters.Scene["SunPlasma"].GetShader().Shader;
            effect1.Parameters["sampleTexture2"].SetValue(Request<Texture2D>("StarlightRiver/Assets/Bosses/VitricBoss/LaserBallMap").Value);
            effect1.Parameters["sampleTexture3"].SetValue(Request<Texture2D>("StarlightRiver/Assets/Bosses/VitricBoss/LaserBallDistort").Value);
            effect1.Parameters["uTime"].SetValue(Main.GameUpdateCount * 0.01f);

            spriteBatch.End();
            spriteBatch.Begin(default, BlendState.NonPremultiplied, default, default, default, effect1, Main.GameViewMatrix.ZoomMatrix);

            spriteBatch.Draw(Request<Texture2D>(AssetDirectory.VitricBoss + Name).Value, Projectile.Center - Main.screenPosition, null, Color.White * Projectile.scale, 0, Projectile.Size / 2, Projectile.scale * 1.7f, 0, 0);

            spriteBatch.End();
            spriteBatch.Begin(default, BlendState.Additive, SamplerState.PointWrap, default, default, default, Main.GameViewMatrix.ZoomMatrix);

            if (LaserTimer > 30 && LaserTimer <= 120) //tell line
			{
                var texTell = Request<Texture2D>(AssetDirectory.MiscTextures + "DirectionalBeam").Value;
                Vector2 origin = new Vector2(0, texTell.Height / 2);

                for (int k = 0; k < 40; k++)
                {
                    var pos = Projectile.Center - Main.screenPosition + Vector2.UnitX.RotatedBy(LaserRotation) * k * 32;

                    if (!parent.arena.Contains((pos + Main.screenPosition).ToPoint()))
                        break;

                    var colorTell = new Color(255, (int)(185 * (k / 10f)), 50);
                    var colorMult = (k / 10f) * (float)(Math.Sin((LaserTimer - 30) / 90f * 3.14f));
                    var source = new Rectangle((int)(((LaserTimer - 30) / 15f) * -texTell.Width), 0, texTell.Width, texTell.Height);

                    spriteBatch.Draw(texTell, pos, source, colorTell * colorMult, LaserRotation, origin, 1, 0, 0);
                }
            }

            if (LaserTimer > 150) //the actual laser
            {
                var texBeam = Request<Texture2D>(AssetDirectory.MiscTextures + "BeamCore").Value;
                var texBeam2 = Request<Texture2D>(AssetDirectory.MiscTextures + "BeamTrail").Value;
                var texDark = Request<Texture2D>(AssetDirectory.MiscTextures + "GradientBlack").Value;

                Vector2 origin = new Vector2(0, texBeam.Height / 2);
                Vector2 origin2 = new Vector2(0, texBeam2.Height / 2);

                var effect = StarlightRiver.Instance.Assets.Request<Effect>("Effects/GlowingDust").Value;

                effect.Parameters["uColor"].SetValue(color.ToVector3());

                spriteBatch.End();
                spriteBatch.Begin(default, default, default, default, default, effect, Main.GameViewMatrix.ZoomMatrix);

                float height = texBeam.Height / 2f;
                int width = (int)(Projectile.Center - endpoint).Length();

                if (LaserTimer - 150 < 20)
                    height = texBeam.Height / 2f * (LaserTimer - 150) / 20f;

                if (LaserTimer - 150 > 230)
                    height = texBeam.Height / 2f * (1 - (LaserTimer - 380) / 20f);


                var pos = Projectile.Center - Main.screenPosition;

                var target = new Rectangle((int)pos.X, (int)pos.Y, width, (int)(height * 1.2f));
                var target2 = new Rectangle((int)pos.X, (int)pos.Y, width, (int)height);

                var source = new Rectangle((int)(((LaserTimer - 150) / 20f) * -texBeam.Width), 0, texBeam.Width, texBeam.Height);
                var source2 = new Rectangle((int)(((LaserTimer - 150) / 45f) * -texBeam2.Width), 0, texBeam2.Width, texBeam2.Height);

                spriteBatch.Draw(texBeam, target, source, color, LaserRotation, origin, 0, 0);
                spriteBatch.Draw(texBeam2, target2, source2, color * 0.5f, LaserRotation, origin2, 0, 0);

                for (int i = 0; i < width; i += 10)
                {
                    Lighting.AddLight(pos + Vector2.UnitX.RotatedBy(LaserRotation) * i + Main.screenPosition, color.ToVector3() * height * 0.010f);

                    if(Main.rand.Next(20) == 0)
                        Dust.NewDustPerfect(Projectile.Center + Vector2.UnitX.RotatedBy(LaserRotation) * i, DustType<Dusts.Glow>(), Vector2.UnitY * Main.rand.NextFloat(-1.5f, -0.5f), 0, color, 0.4f);
                }

                var opacity = height / (texBeam.Height / 2f);

                spriteBatch.End();
                spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);

                if (parent.arena.Contains(Main.LocalPlayer.Center.ToPoint()))
                {
                    spriteBatch.Draw(texDark, Projectile.Center + Vector2.UnitX.RotatedBy(LaserRotation + 1.57f) * 80 - Main.screenPosition, null, Color.White * opacity, LaserRotation, new Vector2(texDark.Width / 2, 0), 10, 0, 0);
                    spriteBatch.Draw(texDark, Projectile.Center + Vector2.UnitX.RotatedBy(LaserRotation - 1.57f) * 80 - Main.screenPosition, null, Color.White * opacity, LaserRotation - 3.14f, new Vector2(texDark.Width / 2, 0), 10, 0, 0);
                }

                spriteBatch.End();
                spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);

                var impactTex = Request<Texture2D>(AssetDirectory.Assets + "Keys/GlowSoft").Value;
                var impactTex2 = Request<Texture2D>(AssetDirectory.GUI + "ItemGlow").Value;
                var glowTex = Request<Texture2D>(AssetDirectory.Assets + "GlowTrail").Value;

                spriteBatch.Draw(glowTex, target, source, color * 0.95f, LaserRotation, new Vector2(0, glowTex.Height / 2), 0, 0);

                spriteBatch.Draw(impactTex, endpoint - Main.screenPosition, null, color * (height * 0.006f), 0, impactTex.Size() / 2, 6.4f, 0, 0);
                spriteBatch.Draw(impactTex2, endpoint - Main.screenPosition, null, color * (height * 0.01f), StarlightWorld.rottime * 2, impactTex2.Size() / 2, 0.75f, 0, 0);

                for (int k = 0; k < 4; k++)
                {
                    float rot = Main.rand.NextFloat(6.28f);
                    int variation = Main.rand.Next(30);

                    color.G -= (byte)variation;

                    Dust.NewDustPerfect(Projectile.Center + Vector2.UnitX.RotatedBy(LaserRotation) * width + Vector2.One.RotatedBy(rot) * Main.rand.NextFloat(40), DustType<Dusts.Glow>(), Vector2.One.RotatedBy(rot) * 2, 0, color, 0.9f - (variation * 0.03f));
                }
            }

        }
    }
}
