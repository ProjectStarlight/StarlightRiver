using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

using StarlightRiver.Core;
using StarlightRiver.Content.NPCs.BaseTypes;

namespace StarlightRiver.Content.Bosses.SquidBoss
{
    class Laser : InteractiveProjectile, IUnderwater
    {
        public override string Texture => AssetDirectory.SquidBoss + Name;
        public override bool OnTileCollide(Vector2 oldVelocity) => false;

        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor) => false;

        public override void SetDefaults()
        {
            projectile.width = 60;
            projectile.height = 1;
            projectile.damage = 50;
            projectile.hostile = true;
            projectile.timeLeft = Main.expertMode ? 450 : 600;
            projectile.aiStyle = -1;
        }

        public override void AI()
        {
            if (projectile.timeLeft == 599 || Main.expertMode && projectile.timeLeft == 449)
            {
                int y = (int)projectile.Center.Y / 16 - 31;

                for (int k = 0; k < 58; k++)
                {
                    int x = (int)projectile.Center.X / 16 + 22 + k;
                    ValidPoints.Add(new Point16(x, y));
                }
            }

            projectile.ai[1]++;

            projectile.Center = Main.npc.FirstOrDefault(n => n.modNPC is SquidBoss).Center;

            //collision
            int height = 0;

            for (int k = 0; k < 45; k++)
            {
                Vector2 pos = projectile.position + new Vector2(0, -16 * k);
                height += 16;

                if (Main.tile[(int)pos.X / 16 + 2, (int)pos.Y / 16 - 4].active() || Main.tile[(int)pos.X / 16 - 2, (int)pos.Y / 16 - 4].active()) break;
            }

            Rectangle rect = new Rectangle((int)projectile.position.X, (int)projectile.position.Y - height, projectile.width, height);
            foreach (Player player in Main.player.Where(n => n.active && n.Hitbox.Intersects(rect))) player.Hurt(PlayerDeathReason.ByCustomReason(player.name + " got lasered to death by a squid..."), 50, 0);
        }

        public void DrawUnderWater(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.GetTexture(Texture);
            Texture2D tex2 = ModContent.GetTexture(Texture + "Glow");

            for (int k = 0; k < 70; k++)
            {
                float sin = 1 + (float)Math.Sin(projectile.ai[1] / 10f);
                float cos = 1 + (float)Math.Cos(projectile.ai[1] / 10f);
                Color color = new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f) * 1.05f;

                Vector2 pos = projectile.position + new Vector2(0, -16 * k);

                if (Main.tile[(int)pos.X / 16 + 2, (int)pos.Y / 16 + 1].active() || Main.tile[(int)pos.X / 16 - 2, (int)pos.Y / 16 + 1].active())
                {
                    for (int n = 0; n < 20; n++)
                    {
                        Dust d = Dust.NewDustPerfect(pos + new Vector2(Main.rand.Next(0, 60), 32), 264, Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(3), 0, color, 2.2f);
                        d.noGravity = true;
                        d.rotation = Main.rand.NextFloat(6.28f);
                    }
                    break;
                }

                pos.Y -= projectile.ai[1] % tex.Height;
                spriteBatch.Draw(tex, pos - Main.screenPosition, color);
                spriteBatch.Draw(tex2, pos - Main.screenPosition, Color.White);

                if (k % 10 == 0) Lighting.AddLight(pos, color.ToVector3() * 0.5f);
            }
        }
    }
}
