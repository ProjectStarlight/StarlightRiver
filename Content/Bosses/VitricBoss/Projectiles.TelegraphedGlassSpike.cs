using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Items.Vitric;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Bosses.VitricBoss
{
    public class TelegraphedGlassSpike : ModProjectile, IDrawAdditive
    {
        Vector2 savedVelocity;

        public override string Texture => AssetDirectory.VitricBoss + "GlassSpike";

        public override void SetDefaults()
        {
            projectile.hostile = true;
            projectile.width = 22;
            projectile.height = 22;
            projectile.penetrate = 1;
            projectile.timeLeft = 240;
            projectile.tileCollide = true;
            projectile.ignoreWater = true;
            projectile.damage = 5;
        }

        public override void SetStaticDefaults() => DisplayName.SetDefault("Glass Spike");

        public override void AI()
        {
            if (projectile.timeLeft == 240)
            {
                savedVelocity = projectile.velocity;
                projectile.velocity *= 0;
            }

            if (projectile.timeLeft > 150)
                projectile.velocity = Vector2.SmoothStep(Vector2.Zero, savedVelocity, (30 - (projectile.timeLeft - 150)) / 30f);

            Color color = VitricSummonOrb.MoltenGlow(MathHelper.Min((200 - projectile.timeLeft), 120));

            for (int k = 0; k <= 1; k++)
            {
                Dust d = Dust.NewDustPerfect(projectile.Center + projectile.velocity, 264, (projectile.velocity * Main.rand.NextFloat(-0.25f, -0.1f)).RotatedBy(k == 0 ? 0.4f : -0.4f), 0, color, 1f);
                d.noGravity = true;
            }
            projectile.rotation = projectile.velocity.ToRotation() + 3.14f / 4;
        }

        public override void ModifyHitPlayer(Player target, ref int damage, ref bool crit) => target.AddBuff(BuffID.Bleeding, 300);

        public override void Kill(int timeLeft)
        {
            Color color = VitricSummonOrb.MoltenGlow(MathHelper.Min((200 - projectile.timeLeft), 120));

            for (int k = 0; k <= 10; k++)
            {
                Dust.NewDust(projectile.position, 22, 22, DustType<Dusts.GlassGravity>(), projectile.velocity.X * 0.5f, projectile.velocity.Y * 0.5f);
                Dust.NewDust(projectile.position, 22, 22, DustType<Dusts.Glow>(), 0, 0, 0, color, 0.3f);
            }
            Main.PlaySound(SoundID.Shatter, projectile.Center);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if (projectile.timeLeft > 180)
                return false;

            Color color = VitricSummonOrb.MoltenGlow(MathHelper.Min((200 - projectile.timeLeft), 120));

            spriteBatch.Draw(GetTexture(Texture), projectile.Center - Main.screenPosition, new Rectangle(0, 0, 22, 22), lightColor, projectile.rotation, Vector2.One * 11, projectile.scale, 0, 0);
            spriteBatch.Draw(GetTexture(Texture), projectile.Center - Main.screenPosition, new Rectangle(0, 22, 22, 22), color, projectile.rotation, Vector2.One * 11, projectile.scale, 0, 0);

            return false;
        }

        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            Texture2D tex = GetTexture(Texture + "Glow");
            float alpha = projectile.timeLeft > 160 ? 1 - (projectile.timeLeft - 160) / 20f : 1;
            Color color = VitricSummonOrb.MoltenGlow(MathHelper.Min((200 - projectile.timeLeft), 120)) * alpha;

            spriteBatch.Draw(tex, projectile.Center + Vector2.Normalize(projectile.velocity) * -40 - Main.screenPosition, tex.Frame(),
                color * (projectile.timeLeft / 140f), projectile.rotation + 3.14f, tex.Size() / 2, 1.8f, 0, 0);

            if(projectile.timeLeft > 180)
			{
                Texture2D tex2 = GetTexture(AssetDirectory.VitricBoss + "RoarLine");
                float alpha2 = (float)Math.Sin((projectile.timeLeft - 180) / 60f * 3.14f);
                Color color2 = new Color(255, 180, 80) * alpha2;
                var source = new Rectangle(0, tex2.Height / 2, tex2.Width, tex2.Height / 2);
                spriteBatch.Draw(tex2, projectile.Center - Main.screenPosition, source, color2, 0, new Vector2(tex2.Width / 2, 0), 6, 0, 0);
            }
        }
    }
}