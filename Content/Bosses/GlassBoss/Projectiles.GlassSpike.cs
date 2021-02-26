using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Content.Items.Vitric;

namespace StarlightRiver.Content.Bosses.GlassBoss
{
    public class GlassSpike : ModProjectile, IDrawAdditive
    {
        Vector2 savedVelocity;

        public override string Texture => AssetDirectory.GlassBoss + Name;

        public override void SetDefaults()
        {
            projectile.hostile = true;
            projectile.width = 22;
            projectile.height = 22;
            projectile.penetrate = 1;
            projectile.timeLeft = 180;
            projectile.tileCollide = true;
            projectile.ignoreWater = true;
            projectile.damage = 5;
        }

        public override void SetStaticDefaults() => DisplayName.SetDefault("Glass Spike");

        public override void AI()
        {
            if (projectile.timeLeft == 180)
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
            for (int k = 0; k <= 10; k++)
            {
                Dust.NewDust(projectile.position, 22, 22, DustType<Dusts.GlassGravity>(), projectile.velocity.X * 0.5f, projectile.velocity.Y * 0.5f);
                Dust.NewDust(projectile.position, 22, 22, DustType<Content.Dusts.Air>());
            }
            Main.PlaySound(SoundID.Shatter, projectile.Center);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Color color = VitricSummonOrb.MoltenGlow(MathHelper.Min((200 - projectile.timeLeft), 120));

            spriteBatch.Draw(GetTexture(Texture), projectile.Center - Main.screenPosition, new Rectangle(0, 0, 22, 22), lightColor, projectile.rotation, Vector2.One * 11, projectile.scale, 0, 0);
            spriteBatch.Draw(GetTexture(Texture), projectile.Center - Main.screenPosition, new Rectangle(0, 22, 22, 22), color, projectile.rotation, Vector2.One * 11, projectile.scale, 0, 0);

            return false;
        }

        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            Texture2D tex = GetTexture(Texture + "Glow");
            Color color = VitricSummonOrb.MoltenGlow(MathHelper.Min((200 - projectile.timeLeft), 120));

            spriteBatch.Draw(tex, projectile.Center + Vector2.Normalize(projectile.velocity) * -40 - Main.screenPosition, tex.Frame(),
                color * (projectile.timeLeft / 140f), projectile.rotation + 3.14f, tex.Size() / 2, 1.8f, 0, 0);
        }
    }
}