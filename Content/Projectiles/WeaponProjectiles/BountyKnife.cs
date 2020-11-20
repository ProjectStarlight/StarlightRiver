using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Projectiles.WeaponProjectiles
{
    internal class BountyKnife : ModProjectile
    {
        public override void SetDefaults()
        {
            projectile.width = 8;
            projectile.height = 8;
            projectile.friendly = true;
            projectile.penetrate = 1;
            projectile.timeLeft = 1000;
            projectile.tileCollide = true;
            projectile.ignoreWater = true;
            projectile.extraUpdates = 5;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Bounty Knife");
        }

        public override void AI()
        {
            Dust.NewDustPerfect(projectile.Center, DustType<Dusts.Starlight>(), Vector2.Zero);
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            if (!target.boss && !target.dontTakeDamage && !target.immortal && !target.friendly)
            {
                /*if (target.aiStyle == 1) NPC.NewNPC((int)target.position.X, (int)target.position.Y, ModContent.NPCType<NPCs.Beasts.SlimeBeast>());
                else if (!target.noGravity && !target.noTileCollide) NPC.NewNPC((int)target.position.X, (int)target.position.Y, ModContent.NPCType<NPCs.Beasts.GroundBeast>());
                else if (target.noGravity && !target.noTileCollide) NPC.NewNPC((int)target.position.X, (int)target.position.Y, ModContent.NPCType<NPCs.Beasts.AirBeast>());
                else if (target.noGravity && target.noTileCollide) NPC.NewNPC((int)target.position.X, (int)target.position.Y, ModContent.NPCType<NPCs.Beasts.PhaseBeast>());
                else NPC.NewNPC((int)target.position.X, (int)target.position.Y, ModContent.NPCType<NPCs.Beasts.FailsafeBeast>()); //probably stupid rare*/
            }
        }
    }
}