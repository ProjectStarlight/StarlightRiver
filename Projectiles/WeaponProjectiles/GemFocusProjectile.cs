using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Projectiles.WeaponProjectiles
{
    internal class GemFocusProjectile : ModProjectile
    {
        public override string Texture => "StarlightRiver/Invisible";

        public override void SetDefaults()
        {
            projectile.friendly = true;
            projectile.aiStyle = -1;
            projectile.timeLeft = 30;
            projectile.width = 42;
            projectile.height = 42;
            projectile.tileCollide = true;
            projectile.penetrate = -1;
            projectile.netSpam = 1;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            return false;
        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            base.ModifyHitNPC(target, ref damage, ref knockback, ref crit, ref hitDirection);
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            if (projectile.ai[1] == 0)
            {
                projectile.ai[1] = 15;
                Main.PlaySound(SoundID.DD2_WitherBeastAuraPulse, projectile.Center);
            }
        }

        public override void AI()
        {
            if (projectile.ai[1] > 0) projectile.ai[1]--;
            if (Main.player[projectile.owner].channel && Main.player[projectile.owner].statMana > 0)
            {
                projectile.timeLeft = 30;
                if (projectile.ai[0] < 30) projectile.ai[0]++;
                projectile.velocity += Vector2.Normalize(Main.MouseWorld - projectile.Center) * 0.3f;
                if (projectile.velocity.Length() > 5) projectile.velocity = Vector2.Normalize(projectile.velocity) * 5;

                projectile.rotation = projectile.velocity.X * 0.1f;
            }
            else
            {
                projectile.ai[0]--;
                projectile.velocity *= 0;
            }

            projectile.alpha = (int)(projectile.ai[0] / 30f * 255);

            for (int k = 0; k < 6; k++)
            {
                Color color = Color.White;
                switch (k)
                {
                    case 0: color = new Color(255, 150, 150); break;
                    case 1: color = new Color(150, 255, 150); break;
                    case 2: color = new Color(150, 150, 255); break;
                    case 3: color = new Color(255, 240, 150); break;
                    case 4: color = new Color(230, 150, 255); break;
                    case 5: color = new Color(255, 255, 255); break;
                }

                float x = (float)Math.Cos(StarlightWorld.rottime + k) * projectile.ai[0] / 30f * 40;
                float y = (float)Math.Sin(StarlightWorld.rottime + k) * projectile.ai[0] / 30f * 10;
                Vector2 pos = (new Vector2(x, y)).RotatedBy(k / 12f * 6.28f);

                Dust d = Dust.NewDustPerfect(projectile.Center, DustType<Dusts.GemFocusDust>(), pos, 0, color, 1f);
                d.customData = projectile;
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D over = GetTexture("StarlightRiver/Items/Misc/GemFocusOver");
            Texture2D under = GetTexture("StarlightRiver/Items/Misc/GemFocusUnder");
            Texture2D glow = GetTexture("StarlightRiver/RiftCrafting/Glow0");

            Vector2 position = projectile.position - Main.screenPosition;
            float scale = projectile.scale;
            float fade = (projectile.alpha / 255f);
            float pulse = 1 - projectile.ai[1] / 15f;
            //Rectangle frame = under.Frame();

            spriteBatch.Draw(under, position + projectile.Size / 2 * scale, under.Frame(), Color.White * fade, projectile.rotation, under.Size() / 2, scale, 0, 0);

            float timer = (float)Math.Sin(StarlightWorld.rottime) * 0.1f;
            spriteBatch.Draw(under, position + projectile.Size / 2 * scale, under.Frame(), Main.DiscoColor * (0.4f + timer) * fade, projectile.rotation, under.Size() / 2, scale * 1.3f + timer, 0, 0);

            spriteBatch.Draw(over, position + projectile.Size / 2 * scale, over.Frame(), lightColor * fade, projectile.rotation, over.Size() / 2, scale, 0, 0);

            spriteBatch.End();
            spriteBatch.Begin(default, BlendState.Additive);

            spriteBatch.Draw(glow, position + projectile.Size / 2 * scale, glow.Frame(), Main.DiscoColor * 0.5f * fade, projectile.rotation, glow.Size() / 2, scale, 0, 0);

            if (projectile.ai[1] > 0)
                spriteBatch.Draw(glow, position + projectile.Size / 2 * scale, glow.Frame(), Main.DiscoColor * (1 - pulse) * fade, projectile.rotation, glow.Size() / 2, scale * (1 + pulse * 2), 0, 0);

            spriteBatch.End();
            spriteBatch.Begin();

            Lighting.AddLight(projectile.Center, Main.DiscoColor.ToVector3() * 0.5f * fade);
            return false;
        }
    }
}