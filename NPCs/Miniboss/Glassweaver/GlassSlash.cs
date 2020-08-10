using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Terraria.ModLoader.ModContent;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.NPCs.Miniboss.Glassweaver
{
    class GlassSlash : ModProjectile
    {
        public override void SetStaticDefaults() => DisplayName.SetDefault("Woven Blade");

        public override void SetDefaults()
        {
            projectile.width = 48;
            projectile.height = 72;
            projectile.hostile = true;
            projectile.timeLeft = 80;
            projectile.tileCollide = false;
            projectile.aiStyle = -1;
            projectile.penetrate = -1;
        }

        public override void AI()
        {
            if(projectile.timeLeft >= 20 && projectile.timeLeft % 20 == 0) projectile.velocity.X += projectile.ai[0] == 0 ? -5 : 5; //burst forward with boss

            projectile.velocity.X *= 0.95f; //decelerate
        }
    }
}
