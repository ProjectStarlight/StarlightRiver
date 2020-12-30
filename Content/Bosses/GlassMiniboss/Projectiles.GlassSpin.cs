using Terraria;
using Terraria.ModLoader;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Bosses.GlassMiniboss
{
    class GlassSpin : ModProjectile
    {
        private NPC parent => Main.npc[(int)projectile.ai[0]];

        public override string Texture => AssetDirectory.Invisible;

        public override void SetStaticDefaults() => DisplayName.SetDefault("Woven Blade");

        public override void SetDefaults()
        {
            projectile.width = 200;
            projectile.height = 30;
            projectile.hostile = true;
            projectile.timeLeft = 150;
            projectile.tileCollide = false;
            projectile.aiStyle = -1;
            projectile.penetrate = -1;
        }

        public override void AI()
        {
            if (parent != null) projectile.Center = parent.Center;
        }
    }
}
