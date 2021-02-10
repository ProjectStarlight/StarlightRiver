using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Bosses.SquidBoss
{
    class DoorBomb : InteractiveProjectile
    {
        public override string Texture => AssetDirectory.SquidBoss + "SpewBlob";

        public override void GoodEffects() => StarlightWorld.Flag(WorldFlags.SquidBossOpen);

        public override void SetDefaults()
        {
            projectile.friendly = true;
            projectile.width = 32;
            projectile.height = 32;
            projectile.timeLeft = 176;
        }

        public override void AI()
        {
            if (projectile.timeLeft == 175) ValidPoints.Add(new Point16((int)projectile.Center.X / 16 + 11, (int)projectile.Center.Y / 16));

            projectile.ai[1] += 0.1f;
            projectile.rotation = projectile.velocity.ToRotation() + 1.57f;

            float sin = 1 + (float)Math.Sin(projectile.ai[1]);
            float cos = 1 + (float)Math.Cos(projectile.ai[1]);
            Color color = new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f);
            Lighting.AddLight(projectile.Center, color.ToVector3() * 0.6f);

            Dust d = Dust.NewDustPerfect(projectile.Center, 264, -projectile.velocity * 0.5f, 0, color, 1.4f);
            d.noGravity = true;
            d.rotation = Main.rand.NextFloat(6.28f);
        }

        public override void SafeKill(int timeLeft)
        {
            for (int k = 0; k < 20; k++)
            {
                Dust d = Dust.NewDustPerfect(projectile.Center, 264, Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(), 0, Color.White, 1.4f);
                d.noGravity = true;
                d.rotation = Main.rand.NextFloat(6.28f);
            }

            Main.PlaySound(SoundID.Item9, projectile.Center);
        }
    }
}
