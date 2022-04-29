using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.NPCs.BaseTypes;
using StarlightRiver.Core;
using System;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Bosses.SquidBoss
{
    class Laser : InteractiveProjectile, IUnderwater
    {
        public NPC Parent;

        public int Height;

        public override string Texture => AssetDirectory.SquidBoss + Name;

        public override bool OnTileCollide(Vector2 oldVelocity) => false;

        public override bool PreDraw(ref Color drawColor) => false;

        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 1;
            Projectile.damage = 50;
            Projectile.hostile = true;
            Projectile.timeLeft = Main.expertMode ? 510 : 660;
            Projectile.aiStyle = -1;
        }

        public override void AI()
        {
            if (Projectile.timeLeft == 659 || Main.expertMode && Projectile.timeLeft == 509)
            {
                int y = (int)Projectile.Center.Y / 16 - 28;

                int xOff = (Parent.ModNPC as SquidBoss).variantAttack ? 18 : -76;

                for (int k = 0; k < 59; k++)
                {
                    int x = (int)Projectile.Center.X / 16 + xOff + k;
                    ValidPoints.Add(new Point16(x, y));
                }
            }

            Projectile.ai[1]++;

            Projectile.Center = Parent.Center;

            //collision
            int height = 0;

            for (int k = 0; k < 200; k++)
            {
                Vector2 pos = Projectile.Center + new Vector2(0, -16 * k);
                height += 16;

                for (int i = -2; i <= 2; i++)
                {
                    if (Main.tile[(int)pos.X / 16 + i, (int)pos.Y / 16].HasTile)
                        k = 200;
                }
            }

            Height = height;

            Rectangle rect = new Rectangle((int)Projectile.position.X, (int)Projectile.position.Y - height + 16, Projectile.width, height - 16);

            float sin = 1 + (float)Math.Sin(Projectile.ai[1] / 10f);
            float cos = 1 + (float)Math.Cos(Projectile.ai[1] / 10f);
            Color color = new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f);

            for (int k = 0; k < rect.Height; k += 500)
            {
                int i = Dust.NewDust(rect.TopLeft() + Vector2.UnitY * k, rect.Width, rect.Height - k, ModContent.DustType<Dusts.Glow>(), 0, -6, 0, color, Main.rand.NextFloat(0.4f, 0.6f));
                Main.dust[i].noLight = true;
            }

            if (Projectile.timeLeft > 30)
            {
                var endPos = Projectile.Center - Vector2.UnitY * (height - 84);

                for (int k = 0; k < 5; k++)
                {
                    var vel = Vector2.UnitY.RotatedByRandom(2f) * Main.rand.NextFloat(15);
                    Dust.NewDustPerfect(endPos, ModContent.DustType<Dusts.ColoredSpark>(), vel, 0, color, Main.rand.NextFloat(1.2f, 2.6f));
                }

                Main.LocalPlayer.GetModPlayer<StarlightPlayer>().Shake += (int)Math.Max(0, 3 - Vector2.Distance(Main.LocalPlayer.Center, endPos) * 0.005f);
            }

            foreach (Player Player in Main.player.Where(n => n.active && n.Hitbox.Intersects(rect)))
            {
                Player.Hurt(PlayerDeathReason.ByCustomReason(Player.name + " got lasered to death by a squid..."), 50, 0);
            }
        }

        public void DrawUnderWater(SpriteBatch spriteBatch, int NPCLayer)
        {
            float sin = 1 + (float)Math.Sin(Projectile.ai[1] / 10f);
            float cos = 1 + (float)Math.Cos(Projectile.ai[1] / 10f);
            Color color = new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f) * 1.05f;

            float alpha = Projectile.timeLeft > (Main.expertMode ? 480 : 630) ? 1 - (Projectile.timeLeft - (Main.expertMode ? 480 : 630)) / 30f : Projectile.timeLeft < 30 ? Projectile.timeLeft / 30f : 1;
            color = color * alpha;

            var texBeam = ModContent.Request<Texture2D>("StarlightRiver/Assets/ShadowTrail").Value;
            var texBeam2 = ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrail").Value;
            var texStar = ModContent.Request<Texture2D>(AssetDirectory.GUI + "ItemGlow").Value;

            Vector2 origin = new Vector2(0, texBeam.Height / 2);
            Vector2 origin2 = new Vector2(0, texBeam2.Height / 2);

            var effect = StarlightRiver.Instance.Assets.Request<Effect>("Effects/GlowingDust").Value;

            effect.Parameters["uColor"].SetValue(color.ToVector3());

            spriteBatch.End();
            spriteBatch.Begin(default, BlendState.Additive, SamplerState.PointWrap, default, default, default, Main.GameViewMatrix.ZoomMatrix);

            float height = texBeam2.Height / 2f * 1.5f;
            int adjustedLaserHeight = Height - 32;

            for (int k = 0; k <= adjustedLaserHeight; k += 500)
            {
                if (k > (adjustedLaserHeight - 500)) //Change to end for the last segment
                    texBeam2 = ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrailOneEnd").Value;

                var pos = Projectile.Center + Vector2.UnitY * -k - Main.screenPosition;
                var thisHeight = k > (adjustedLaserHeight - 500) ? (adjustedLaserHeight % 500) : 500;

                var source = new Rectangle((int)(Projectile.ai[1] * 0.01f * -texBeam.Width), 0, (int)(texBeam.Width * thisHeight / 500f), texBeam.Height);
                var source1 = new Rectangle((int)(Projectile.ai[1] * 0.023f * -texBeam.Width), 0, (int)(texBeam.Width * thisHeight / 500f), texBeam.Height);
                var source2 = new Rectangle(0, 0, (int)(texBeam2.Width * thisHeight / 500f), texBeam2.Height);

                var target = new Rectangle((int)pos.X, (int)pos.Y, thisHeight, (int)(height * 1.25f * alpha));
                var target2 = new Rectangle((int)pos.X, (int)pos.Y, thisHeight, (int)(height * 2.8f * alpha));
                var target3 = new Rectangle((int)pos.X, (int)pos.Y, thisHeight, (int)(50 * alpha));

                spriteBatch.Draw(texBeam, target, source, color * 0.65f, -1.57f, origin, 0, 0);
                spriteBatch.Draw(texBeam, target, source1, color * 0.45f, -1.57f, origin, 0, 0);
                spriteBatch.Draw(texBeam2, target2, source2, color * 0.65f, -1.57f, origin2, 0, 0);
                spriteBatch.Draw(texBeam2, target3, source2, color * 1.1f, -1.57f, origin2, 0, 0);

                Main.NewText(thisHeight);
            }

            spriteBatch.Draw(texStar, Projectile.Center - Vector2.UnitY * (Height - 16) - Main.screenPosition, null, color * 1.1f, Projectile.ai[1] * 0.025f, texStar.Size() / 2, 1, 0, 0);
            spriteBatch.Draw(texStar, Projectile.Center - Vector2.UnitY * (Height - 16) - Main.screenPosition, null, color * 1.1f, Projectile.ai[1] * -0.045f, texStar.Size() / 2, 0.65f, 0, 0);

            spriteBatch.End();
            spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);

        }
    }
}
