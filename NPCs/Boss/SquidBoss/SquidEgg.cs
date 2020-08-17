using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.NPCs.Boss.SquidBoss
{
    class SquidEgg : ModProjectile
    {
        public override void SetDefaults()
        {
            projectile.width = 62;
            projectile.height = 34;
            projectile.aiStyle = -1;
            projectile.timeLeft = 300;
            projectile.hostile = true;
            projectile.damage = 15;
        }

        public override void AI()
        {
            projectile.ai[0]++;

            if (projectile.ai[0] % 100 == 0)
            {
                NPC.NewNPC((int)projectile.Center.X, (int)projectile.Center.Y, NPCType<Auroraling>());
                Main.PlaySound(SoundID.Item86, projectile.Center);
            }
        }

        public override void Kill(int timeLeft)
        {
            NPC.NewNPC((int)projectile.Center.X, (int)projectile.Center.Y, Main.expertMode ? NPCType<Auroraborn>() : NPCType<Auroraling>());
        }
    }
}
