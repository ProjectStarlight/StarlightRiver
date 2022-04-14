using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Bosses.SquidBoss
{
    class IcePlatformSpike : ModProjectile
    {
        public Tentacle Parent;

        public override string Texture => AssetDirectory.SquidBoss + Name;

        public override void SetDefaults()
        {
            Projectile.width = 198;
            Projectile.height = 24;
            Projectile.aiStyle = -1;
            Projectile.timeLeft = 300;
            Projectile.hostile = true;
            Projectile.damage = 15;
        }

        public override void AI()
        {
            Projectile.ai[0]++;

            if (Parent?.NPC.GetGlobalNPC<BarrierNPC>().Barrier <= 0)
            {
                Parent.NPC.dontTakeDamage = true;
                Projectile.active = false;
            }
        }
    }
}
