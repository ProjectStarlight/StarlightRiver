using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Bosses.GlassBoss
{
	class FinalLaser : ModProjectile, IDrawAdditive
    {
        public VitricBoss parent;

        public override string Texture => AssetDirectory.GlassBoss + Name;

        public ref float Timer => ref projectile.ai[0];
        public ref float LaserRotation => ref projectile.ai[1];

        private float LaserTimer => (Timer - 120) % 400;

        public int direction = -1;
        public Vector2 endpoint = Vector2.Zero;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Raging Fire");
        }

        public override void SetDefaults()
        {
            projectile.width = 64;
            projectile.height = 64;
            projectile.hostile = true;
        }

        public override void AI()
        {
            Timer++;
            projectile.timeLeft = 2;

            if(Timer < 60)
			{
                var rot = Main.rand.NextFloat(6.28f);
                Dust.NewDustPerfect(projectile.Center + Vector2.One.RotatedBy(rot) * 100, DustType<Dusts.Glow>(), Vector2.One.RotatedBy(rot) * -3, 0, Color.Yellow, (1 - (Timer / 60f)));

                projectile.scale = Math.Min(1, (Timer / 60f));
            }

            if(Timer > 120)
			{
                if (LaserTimer == 1)
                    Helpers.Helper.PlayPitched("GlassBoss/lasercharge", 1, 0, projectile.Center);

                if (LaserTimer == 140)
                    direction = (Main.player[parent.npc.target].Center - projectile.Center).ToRotation() > LaserRotation ? 1 : -1;

                if(LaserTimer > 30 && LaserTimer <= 75)
				{
                    projectile.netUpdate = true;
                    LaserRotation = (Main.player[parent.npc.target].Center - projectile.Center).ToRotation();

                    var rot = Main.rand.NextFloat(6.28f);
                    Dust.NewDustPerfect(projectile.Center + Vector2.One.RotatedBy(rot) * 100, DustType<Dusts.Glow>(), Vector2.One.RotatedBy(rot) * -3, 0, Color.Yellow,  (LaserTimer - 30) / 45f);
                }

                if (LaserTimer == 150)
                    Main.PlaySound(SoundID.DD2_BetsyFlameBreath);

                if(LaserTimer > 150) //laser is actually active
				{
                    float laserSpeed = 0.008f;

                    for (int k = 0; k < 160; k++) //raycast to find the laser's endpoint
                    {
                        Vector2 posCheck = projectile.Center + Vector2.UnitX.RotatedBy(LaserRotation) * k * 8;

                        if (!parent.arena.Contains(posCheck.ToPoint()))
						{
                            endpoint = posCheck;
                            break;
						}
                    }

                    if (parent.shield != null && parent.shield.scale > 0.5f)
                    {
                        Vector2 shieldEdge1 = parent.shield.Center + Vector2.UnitX.RotatedBy(parent.shield.rotation + 1.57f) * 31 * parent.shield.scale;
                        Vector2 shieldEdge2 = parent.shield.Center + Vector2.UnitX.RotatedBy(parent.shield.rotation + 1.57f) * -31 * parent.shield.scale;

                        if (Helpers.Helper.LinesIntersect(projectile.Center, endpoint, shieldEdge1, shieldEdge2, out Vector2 intersection))
                        {
                            endpoint = intersection;
                            (parent.shield.modNPC as PlayerShieldNPC).startPoint = intersection;

                            laserSpeed = 0.002f;
                            Main.NewText("TESTIFICATE");
                        }
						else
						{
                            (parent.shield.modNPC as PlayerShieldNPC).startPoint = Vector2.Zero;
                        }
                    }

                    LaserRotation += laserSpeed * direction;

                    for (int k = 0; k < Main.maxPlayers; k++) //laser colission
					{
                        Vector2 point;
                        var player = Main.player[k];

                        if (player.active && !player.dead && Helpers.Helper.CheckLinearCollision(projectile.Center, endpoint, player.Hitbox, out point))
                        {
                            player.Hurt(Terraria.DataStructures.PlayerDeathReason.ByCustomReason(player.name + " was reduced to ash"), 10, 0, false, false, false, 5);
                            endpoint = point;
                            break;
                        }
                    }
				}

                else
                     (parent.shield.modNPC as PlayerShieldNPC).startPoint = Vector2.Zero;
            }

            if(Timer > 1320 || parent.Phase == (int)VitricBoss.AIStates.Dying || parent.Phase == (int)VitricBoss.AIStates.Leaving)
			{
                projectile.scale -= 0.05f;

                if (projectile.scale <= 0)
                    projectile.active = false;
			}

        }

        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            Texture2D texGlow = GetTexture("StarlightRiver/Assets/Keys/GlowSoft");

            int sin = (int)(Math.Sin(StarlightWorld.rottime * 3) * 40f);
            var color = new Color(255, 160 + sin, 40 + sin / 2);

            spriteBatch.Draw(texGlow, projectile.Center - Main.screenPosition, null, color * projectile.scale, 0, texGlow.Size() / 2, projectile.scale, default, default);

            if (LaserTimer > 30 && LaserTimer <= 120) //tell line
			{
                var texTell = GetTexture(AssetDirectory.MiscTextures + "DirectionalBeam");
                Vector2 origin = new Vector2(0, texTell.Height / 2);

                for (int k = 0; k < 40; k++)
                {
                    var pos = projectile.Center - Main.screenPosition + Vector2.UnitX.RotatedBy(LaserRotation) * k * 32;

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
                var texBeam = GetTexture(AssetDirectory.MiscTextures + "BeamCore");
                var texBeam2 = GetTexture(AssetDirectory.MiscTextures + "BeamTrail");
                var texDark = GetTexture(AssetDirectory.MiscTextures + "GradientBlack");

                Vector2 origin = new Vector2(0, texBeam.Height / 2);
                Vector2 origin2 = new Vector2(0, texBeam2.Height / 2);

                var effect = StarlightRiver.Instance.GetEffect("Effects/GlowingDust");

                effect.Parameters["uColor"].SetValue(color.ToVector3());

                spriteBatch.End();
                spriteBatch.Begin(default, default, default, default, default, effect, Main.GameViewMatrix.ZoomMatrix);

                float height = texBeam.Height / 2f;
                int width = (int)(projectile.Center - endpoint).Length();

                if (LaserTimer - 150 < 20)
                    height = texBeam.Height / 2f * (LaserTimer - 150) / 20f;

                if (LaserTimer - 150 > 230)
                    height = texBeam.Height / 2f * (1 - (LaserTimer - 380) / 20f);


                var pos = projectile.Center - Main.screenPosition;

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
                        Dust.NewDustPerfect(projectile.Center + Vector2.UnitX.RotatedBy(LaserRotation) * i, DustType<Dusts.Glow>(), Vector2.UnitY * Main.rand.NextFloat(-1.5f, -0.5f), 0, color, 0.4f);
                }

                var opacity = height / (texBeam.Height / 2f);

                spriteBatch.End();
                spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);

                if (parent.arena.Contains(Main.LocalPlayer.Center.ToPoint()))
                {
                    spriteBatch.Draw(texDark, projectile.Center + Vector2.UnitX.RotatedBy(LaserRotation + 1.57f) * 80 - Main.screenPosition, null, Color.White * opacity, LaserRotation, new Vector2(texDark.Width / 2, 0), 10, 0, 0);
                    spriteBatch.Draw(texDark, projectile.Center + Vector2.UnitX.RotatedBy(LaserRotation - 1.57f) * 80 - Main.screenPosition, null, Color.White * opacity, LaserRotation - 3.14f, new Vector2(texDark.Width / 2, 0), 10, 0, 0);
                }

                spriteBatch.End();
                spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);

                var impactTex = GetTexture(AssetDirectory.Assets + "Keys/GlowSoft");
                var impactTex2 = GetTexture(AssetDirectory.GUI + "ItemGlow");
                var glowTex = GetTexture(AssetDirectory.Assets + "GlowTrail");

                spriteBatch.Draw(glowTex, target, source, color * 0.95f, LaserRotation, new Vector2(0, glowTex.Height / 2), 0, 0);

                spriteBatch.Draw(impactTex, endpoint - Main.screenPosition, null, color * (height * 0.006f), 0, impactTex.Size() / 2, 6.4f, 0, 0);
                spriteBatch.Draw(impactTex2, endpoint - Main.screenPosition, null, color * (height * 0.01f), StarlightWorld.rottime * 2, impactTex2.Size() / 2, 0.75f, 0, 0);

                for (int k = 0; k < 4; k++)
                {
                    float rot = Main.rand.NextFloat(6.28f);
                    int variation = Main.rand.Next(30);

                    color.G -= (byte)variation;

                    Dust.NewDustPerfect(projectile.Center + Vector2.UnitX.RotatedBy(LaserRotation) * width + Vector2.One.RotatedBy(rot) * Main.rand.NextFloat(40), DustType<Dusts.Glow>(), Vector2.One.RotatedBy(rot) * 2, 0, color, 0.9f - (variation * 0.03f));
                }
            }

        }
    }
}
