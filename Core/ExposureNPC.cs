using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Core
{
    public class ExposureNPC : GlobalNPC
    {
        //Maybe make this use a dictionary system in the future for some compatibility etc
        public int ExposureAddAll;
        public int ExposureAddMelee;
        public int ExposureAddRanged;
        public int ExposureAddMagic;
        public int ExposureAddSummon; //literally just summon tag without the requirement of being the players minion target

        public float ExposureMultAll;
        public float ExposureMultMelee;
        public float ExposureMultRanged;
        public float ExposureMultMagic;
        public float ExposureMultSummon;

        public override bool InstancePerEntity => true;
        public override void ResetEffects(NPC npc)
        {
            ExposureAddAll = 0;
            ExposureAddMelee = 0;
            ExposureAddRanged = 0;
            ExposureAddMagic = 0;
            ExposureAddSummon = 0;

            ExposureMultAll = 0f;
            ExposureMultMelee = 0f;
            ExposureMultRanged = 0f;
            ExposureMultMagic = 0f;
            ExposureMultSummon = 0f;
        }

        public override void ModifyHitByItem(NPC npc, Player player, Item item, ref int damage, ref float knockback, ref bool crit)
        {
            if (item.CountsAsClass(DamageClass.Melee))
                damage = (int)(damage * (1f + ExposureMultMelee)) + ExposureAddMelee;

            if (item.CountsAsClass(DamageClass.Ranged))
                damage = (int)(damage * (1f + ExposureMultRanged)) + ExposureAddRanged;

            if (item.CountsAsClass(DamageClass.Magic))
                damage = (int)(damage * (1f + ExposureMultMagic)) + ExposureAddMagic;

            if (item.CountsAsClass(DamageClass.Summon))
                damage = (int)(damage * (1f + ExposureMultSummon)) + ExposureAddSummon;

            damage = (int)(damage * (1f + ExposureMultAll)) + ExposureAddAll;
        }

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            if (projectile.CountsAsClass(DamageClass.Melee))
                damage = (int)(damage * (1f + ExposureMultMelee)) + ExposureAddMelee;

            if (projectile.CountsAsClass(DamageClass.Ranged))
                damage = (int)(damage * (1f + ExposureMultRanged)) + ExposureAddRanged;

            if (projectile.CountsAsClass(DamageClass.Magic))
                damage = (int)(damage * (1f + ExposureMultMagic)) + ExposureAddMagic;

            if (projectile.CountsAsClass(DamageClass.Summon))
                damage = (int)(damage * (1f + ExposureMultSummon)) + ExposureAddSummon;

            damage = (int)(damage * (1f + ExposureMultAll)) + ExposureAddAll;
        }
    }
}
