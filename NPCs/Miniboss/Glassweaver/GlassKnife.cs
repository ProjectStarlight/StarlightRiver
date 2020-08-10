using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;

namespace StarlightRiver.NPCs.Miniboss.Glassweaver
{
    class GlassKnife : ModProjectile
    {
        public override void SetDefaults()
        {
            projectile.width = 24;
            projectile.height = 24;
            projectile.extraUpdates = 4;
        }

        public override void AI()
        {
            projectile.rotation = projectile.velocity.ToRotation() + (float)Math.PI / 4f;
            Dust.NewDustPerfect(projectile.Center, DustType<Dusts.Air>(), Vector2.Zero, 0, default, 0.5f);
        }
    }
}
