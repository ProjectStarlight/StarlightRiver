using Microsoft.Xna.Framework;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Bosses.GlassMiniboss
{
    class ChargeShard : ModProjectile
    {
        public Vector2 target;
        private Vector2 origin;

        public override string Texture => "StarlightRiver/Assets/Bosses/GlassMiniboss/ChargeShard";

        public override void SetStaticDefaults() => DisplayName.SetDefault("shard");

        public override void SetDefaults()
        {
            projectile.width = 16;
            projectile.height = 16;
            projectile.timeLeft = 40;
            projectile.tileCollide = false;
            projectile.aiStyle = -1;
            projectile.penetrate = -1;
        }

        public override void AI()
        {
            if (projectile.timeLeft == 40)
                origin = projectile.Center;

            projectile.Center = Vector2.SmoothStep(origin, target, 1 - projectile.timeLeft / 40f);

            projectile.rotation += 0.2f;
        }

        public override void Kill(int timeLeft)
        {
            for (int k = 0; k < 20; k++)
                Dust.NewDust(projectile.position, 16, 16, DustID.Fire);

            Main.PlaySound(SoundID.Item73, projectile.Center);

            if (Main.npc.Any(n => n.modNPC is GlassMiniboss))
            {
                var parent = Main.npc.First(n => n.modNPC is GlassMiniboss).modNPC as GlassMiniboss;

                if (target.X > GlassMiniboss.spawnPos.X)
                    if (parent.rightForgeCharge < 15) parent.rightForgeCharge++;
                else
                    if (parent.leftForgeCharge < 15) parent.leftForgeCharge++;
            }
        }
    }
}
