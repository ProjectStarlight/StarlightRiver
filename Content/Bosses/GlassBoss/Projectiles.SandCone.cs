using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Helpers;

namespace StarlightRiver.Content.Bosses.GlassBoss
{
    internal class SandCone : ModProjectile, IDrawAdditive
    {
        public override string Texture => AssetDirectory.Invisible;

        public override void SetDefaults()
        {
            projectile.hostile = false;
            projectile.width = 1;
            projectile.height = 1;
            projectile.timeLeft = 2;
        }

        public override void AI()
        {
            projectile.timeLeft = 2;
            projectile.ai[0]++; //ticks up the timer

            if (projectile.ai[0] <= 30) //drawing in fire
            {
                var pos1 = projectile.Center + Vector2.UnitX.RotatedBy(projectile.rotation - 0.2f) * Main.rand.Next(-550, -450);
                var pos2 = projectile.Center + Vector2.UnitX.RotatedBy(projectile.rotation + 0.2f) * Main.rand.Next(-550, -450);
                var posRand = projectile.Center + Vector2.UnitX.RotatedBy(projectile.rotation + Main.rand.NextFloat(-0.2f, 0.2f)) * Main.rand.Next(-420, -380);

                Dust.NewDustPerfect(pos1, DustType<PowerupDust>(), (pos1 - projectile.Center) * -0.03f, 0, new Color(255, 240, 220), projectile.ai[0] / 25f);
                Dust.NewDustPerfect(pos2, DustType<PowerupDust>(), (pos2 - projectile.Center) * -0.03f, 0, new Color(255, 240, 220), projectile.ai[0] / 25f);
                Dust.NewDustPerfect(posRand, DustType<PowerupDust>(), (posRand - projectile.Center) * -0.03f, 0, new Color(255, 220, 100), projectile.ai[0] / 25f);

            }

            if (projectile.ai[0] >= 70) //when this projectile goes off
            {
                for (int k = 0; k < 4; k++)
                {
                    var rot = projectile.rotation + Main.rand.NextFloat(-0.2f, 0.2f);
                    Dust.NewDustPerfect(projectile.Center + Vector2.One.RotatedBy(rot - MathHelper.PiOver4) * -80, DustType<LavaSpew>(), -Vector2.UnitX.RotatedBy(rot), 0, default, Main.rand.NextFloat(0.8f, 1.2f));
                }

                foreach (Player player in Main.player.Where(n => Helper.CheckConicalCollision(projectile.Center, 700, projectile.rotation, 0.2f, n.Hitbox)))
                {
                    player.Hurt(PlayerDeathReason.ByCustomReason(player.name + " bit the dust..."), Main.expertMode ? 50 : 35, 0); //hurt em
                    player.AddBuff(BuffID.OnFire, 60); //burn the player
                }
                Main.PlaySound(SoundID.DD2_BetsyFireballShot, projectile.Center);
                projectile.Kill(); //self-destruct
            }
        }

        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            if (projectile.ai[0] <= 66) //draws the proejctile's tell ~1 second before it goes off
            {
                Texture2D tex = GetTexture("StarlightRiver/Assets/Bosses/GlassBoss/ConeTell");
                float alpha = (projectile.ai[0] * 2 / 33 - (float)Math.Pow(projectile.ai[0], 2) / 1086) * 0.5f;
                spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, tex.Frame(), new Color(255, 170, 100) * alpha, projectile.rotation - 1.57f, new Vector2(tex.Width / 2, tex.Height), 1, 0, 0);
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) => false;
    }
}