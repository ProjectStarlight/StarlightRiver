using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Core
{
	partial class StarlightProjectile : GlobalProjectile
    {
        public delegate void ModifyHitNPCDelegate(Projectile Projectile, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection);
        public static event ModifyHitNPCDelegate ModifyHitNPCEvent;

        public void OnModifyHitNPCEvent(Projectile Projectile, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        { ModifyHitNPCEvent?.Invoke(Projectile, target, ref damage, ref knockback, ref crit, ref hitDirection); }

        public override void ModifyHitNPC(Projectile Projectile, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        { OnModifyHitNPCEvent(Projectile, target, ref damage, ref knockback, ref crit, ref hitDirection); }
    }
}
