using static Terraria.ModLoader.ModContent;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;

namespace StarlightRiver.NPCs.Miniboss.Glassweaver
{
    class GlassUppercut : ModProjectile
    {
        private NPC parent => Main.npc[(int)projectile.ai[0]];

        public override string Texture => "StarlightRiver/Invisible";

        public override void SetStaticDefaults() => DisplayName.SetDefault("Woven Blade");

        public override void SetDefaults()
        {
            projectile.width = 80;
            projectile.height = 80;
            projectile.hostile = true;
            projectile.timeLeft = 20;
            projectile.tileCollide = false;
            projectile.aiStyle = -1;
            projectile.penetrate = -1;
        }

        public override void AI()
        {
            if (projectile.timeLeft == 20) Main.PlaySound(SoundID.Item65, projectile.Center);
            if (parent != null) projectile.Center = parent.Center + new Vector2(projectile.ai[1] == 0 ? -23 : 23, 40 - projectile.timeLeft * 2);
        }
    }
}
