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
            Projectile.timeLeft = Main.expertMode ? 450 : 600;
            Projectile.aiStyle = -1;
        }

        public override void AI()
        {
            if (Projectile.timeLeft == 599 || Main.expertMode && Projectile.timeLeft == 449)
            {
                int y = (int)Projectile.Center.Y / 16 - 28;

                int xOff = (Parent.ModNPC as SquidBoss).variantAttack ? 22 : -78;

                for (int k = 0; k < 58; k++)
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
                Vector2 pos = Projectile.position + new Vector2(0, -16 * k);
                height += 16;

                if (Main.tile[(int)pos.X / 16 + 2, (int)pos.Y / 16].HasTile || Main.tile[(int)pos.X / 16 - 2, (int)pos.Y / 16].HasTile)
                    break;
            }

            Height = height;

            Rectangle rect = new Rectangle((int)Projectile.position.X, (int)Projectile.position.Y - height, Projectile.width, height);

            float sin = 1 + (float)Math.Sin(Projectile.ai[1] / 10f);
            float cos = 1 + (float)Math.Cos(Projectile.ai[1] / 10f);
            Color color = new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f);

            Dust.NewDust(rect.TopLeft(), rect.Width, rect.Height, ModContent.DustType<Dusts.Glow>(), 1, -6, 0, color, Main.rand.NextFloat(0.4f, 0.6f));

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
            int width = Height;

            for (int k = 0; k <= Height; k += 500)
            {
                var pos = Projectile.Center + Vector2.UnitY * -k - Main.screenPosition;
                var thisHeight = k > (Height - 500) ? (Height % 500) : 500;

                var source = new Rectangle((int)(Projectile.ai[1] * 0.01f * -texBeam.Width), 0, (int)(texBeam.Width * thisHeight / 500f), texBeam.Height);
                var source1 = new Rectangle((int)(Projectile.ai[1] * 0.023f * -texBeam.Width), 0, (int)(texBeam.Width * thisHeight / 500f), texBeam.Height);
                var source2 = new Rectangle((int)(Projectile.ai[1] * 0.01f * -texBeam2.Width), 0, (int)(texBeam2.Width * thisHeight / 500f), texBeam2.Height);

                var target = new Rectangle((int)pos.X, (int)pos.Y, thisHeight, (int)(height * 1.5f));
                var target2 = new Rectangle((int)pos.X, (int)pos.Y, thisHeight, (int)height);
                var target3 = new Rectangle((int)pos.X, (int)pos.Y, thisHeight, 50);

                spriteBatch.Draw(texBeam, target, source, color * 0.65f, -1.57f, origin, 0, 0);
                spriteBatch.Draw(texBeam, target, source1, color * 0.45f, -1.57f, origin, 0, 0);
                spriteBatch.Draw(texBeam2, target2, source2, color * 0.65f, -1.57f, origin2, 0, 0);
                spriteBatch.Draw(texBeam2, target3, source2, color * 1.1f, -1.57f, origin2, 0, 0);

                Main.NewText(thisHeight);
            }

            spriteBatch.Draw(texStar, Projectile.Center - Vector2.UnitY * Height - Main.screenPosition, null, color * 1.1f, 0, texStar.Size() / 2, 1, 0, 0);

            var opacity = height / (texBeam.Height / 2f);

            spriteBatch.End();
            spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);

        }
    }
}
