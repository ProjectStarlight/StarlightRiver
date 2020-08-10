using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace StarlightRiver.Core
{
    public partial class StarlightPlayer : ModPlayer
    {
        //for on-hit effects that require more specific effects, projectiles
        public delegate void ModifyHitByProjectileDelegate(Player player, Projectile proj, ref int damage, ref bool crit);
        public static event ModifyHitByProjectileDelegate ModifyHitByProjectileEvent;
        public override void ModifyHitByProjectile(Projectile proj, ref int damage, ref bool crit)
        {
            ModifyHitByProjectileEvent?.Invoke(player, proj, ref damage, ref crit);
        }

        //for on-hit effects that require more specific effects, contact damage
        public delegate void ModifyHitByNPCDelegate(Player player, NPC npc, ref int damage, ref bool crit);
        public static event ModifyHitByNPCDelegate ModifyHitByNPCEvent;
        public override void ModifyHitByNPC(NPC npc, ref int damage, ref bool crit)
        {
            ModifyHitByNPCEvent?.Invoke(player, npc, ref damage, ref crit);
        }

        //For stuff like fire gauntlet
        public delegate void ModifyHitNPCDelegate(Player player, Item item, NPC target, ref int damage, ref float knockback, ref bool crit);
        public static event ModifyHitNPCDelegate ModifyHitNPCEvent;
        public override void ModifyHitNPC(Item item, NPC target, ref int damage, ref float knockback, ref bool crit)
        {
            ModifyHitNPCEvent?.Invoke(player, item, target, ref damage, ref knockback, ref crit);
        }

        //this is the grossest one. I am sorry, little ones.
        public delegate bool PreHurtDelegate(Player player, bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource);
        /// <summary>
        /// If any PreHurtEvent returns false, the default behavior is overridden.
        /// </summary>
        public static event PreHurtDelegate PreHurtEvent;
        public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
        {
            if (PreHurtEvent != null)
            {
                bool result = true;
                foreach (PreHurtDelegate del in PreHurtEvent.GetInvocationList())
                {
                    result &= del(player, pvp, quiet, ref damage, ref hitDirection, ref crit, ref customDamage, ref playSound, ref genGore, ref damageSource);
                }
                return result;
            }
            return true;
        }
    }
}
