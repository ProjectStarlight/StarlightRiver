using Terraria;
using Terraria.ModLoader;

using StarlightRiver.Core;

namespace StarlightRiver.Core
{
    partial class StarlightProjectile : GlobalProjectile
    {
        public delegate void ModifyHitNPCDelegate(Projectile projectile, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection);
        public static event ModifyHitNPCDelegate ModifyHitNPCEvent;

        public void OnModifyHitNPCEvent(Projectile projectile, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        { ModifyHitNPCEvent?.Invoke(projectile, target, ref damage, ref knockback, ref crit, ref hitDirection); }

        public override void ModifyHitNPC(Projectile projectile, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        { OnModifyHitNPCEvent(projectile, target, ref damage, ref knockback, ref crit, ref hitDirection); }
    }
}
