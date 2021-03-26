using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Content.Items.Vitric;
using System.Linq;

namespace StarlightRiver.Content.Bosses.GlassBoss
{
    class FinalFire : ModProjectile, IDrawAdditive
    {
        public VitricBoss parent;

        public override string Texture => AssetDirectory.GlassBoss + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Raging Fire");
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 80;
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            projectile.width = 32;
            projectile.height = 32;
            projectile.hostile = true;
            projectile.extraUpdates = 2;
        }

        public override void AI()
        {
            //Dust.NewDustPerfect(projectile.Center, DustID.Fire);

            if (projectile.ai[0] == 0)
            {
                projectile.velocity *= 1.05f;

                for (int k = 0; k < Main.maxProjectiles; k++)
                {
                    var proj = Main.projectile[k];
                    if (proj.type == ProjectileType<PlayerShield>() && Vector2.Distance(proj.Center, projectile.Center) <= 50) //change for rotational colission eventually
                    {
                        projectile.ai[0] = 1;
                        Main.PlaySound(SoundID.Shatter);
                        Main.player[proj.owner].GetModPlayer<StarlightPlayer>().Shake += 30;
                    }
                }
            }

            if(projectile.ai[0] == 1)
            {
                projectile.velocity = Vector2.UnitX.RotatedBy((projectile.Center - parent.npc.Center).ToRotation()) * -10;

                if (projectile.Hitbox.Intersects(parent.npc.Hitbox))
                {
                    parent.npc.StrikeNPC(100, 0, 0);
                    projectile.active = false;
                }
            }
        }

        public override void Kill(int timeLeft)
        {
            base.Kill(timeLeft);
        }

        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            for (int k = 0; k < projectile.oldPos.Length; k++)
            {
                Color color = (new Color(255, 200, 150) * ((projectile.oldPos.Length - k) / (float)projectile.oldPos.Length));
                if (k <= 4) color *= 1.2f;
                float scale = projectile.scale * (projectile.oldPos.Length - k) / projectile.oldPos.Length * 0.4f;
                Texture2D tex = GetTexture("StarlightRiver/Assets/Keys/GlowSoft");//TEXTURE PATH

                spriteBatch.Draw(tex, (projectile.oldPos[k] + projectile.Size / 2 + projectile.Center) * 0.5f - Main.screenPosition, null, color, 0, tex.Size() / 2, scale, default, default);
            }
        }
    }
}
