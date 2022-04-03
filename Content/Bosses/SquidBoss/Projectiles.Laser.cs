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
                int y = (int)Projectile.Center.Y / 16 - 31;

                for (int k = 0; k < 58; k++)
                {
                    int x = (int)Projectile.Center.X / 16 + 22 + k;
                    ValidPoints.Add(new Point16(x, y));
                }
            }

            Projectile.ai[1]++;

            Projectile.Center = Main.npc.FirstOrDefault(n => n.ModNPC is SquidBoss).Center;

            //collision
            int height = 0;

            for (int k = 0; k < 45; k++)
            {
                Vector2 pos = Projectile.position + new Vector2(0, -16 * k);
                height += 16;

                if (Main.tile[(int)pos.X / 16 + 2, (int)pos.Y / 16 - 4].HasTile || Main.tile[(int)pos.X / 16 - 2, (int)pos.Y / 16 - 4].HasTile) break;
            }

            Rectangle rect = new Rectangle((int)Projectile.position.X, (int)Projectile.position.Y - height, Projectile.width, height);
            foreach (Player Player in Main.player.Where(n => n.active && n.Hitbox.Intersects(rect))) Player.Hurt(PlayerDeathReason.ByCustomReason(Player.name + " got lasered to death by a squid..."), 50, 0);
        }

        public void DrawUnderWater(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D tex2 = ModContent.Request<Texture2D>(Texture + "Glow").Value;

            for (int k = 0; k < 70; k++)
            {
                float sin = 1 + (float)Math.Sin(Projectile.ai[1] / 10f);
                float cos = 1 + (float)Math.Cos(Projectile.ai[1] / 10f);
                Color color = new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f) * 1.05f;

                Vector2 pos = Projectile.position + new Vector2(0, -16 * k);

                if (Main.tile[(int)pos.X / 16 + 2, (int)pos.Y / 16 + 1].HasTile || Main.tile[(int)pos.X / 16 - 2, (int)pos.Y / 16 + 1].HasTile)
                {
                    for (int n = 0; n < 20; n++)
                    {
                        Dust d = Dust.NewDustPerfect(pos + new Vector2(Main.rand.Next(0, 60), 32), 264, Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(3), 0, color, 2.2f);
                        d.noGravity = true;
                        d.rotation = Main.rand.NextFloat(6.28f);
                    }
                    break;
                }

                pos.Y -= Projectile.ai[1] % tex.Height;
                spriteBatch.Draw(tex, pos - Main.screenPosition, color);
                spriteBatch.Draw(tex2, pos - Main.screenPosition, Color.White);

                if (k % 10 == 0) Lighting.AddLight(pos, color.ToVector3() * 0.5f);
            }
        }
    }
}
