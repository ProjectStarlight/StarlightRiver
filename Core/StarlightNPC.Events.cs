using Terraria;
using Terraria.ModLoader;

using StarlightRiver.Core;

namespace StarlightRiver.Core
{
    public partial class StarlightNPC : GlobalNPC
    {
        public delegate void PostAIDelegate(NPC npc);
        public static event PostAIDelegate PostAIEvent;
        public override void PostAI(NPC npc)
        {
            PostAIEvent?.Invoke(npc);
        }

        public delegate void NPCLootDelegate(NPC npc);
        public static event NPCLootDelegate NPCLootEvent;
        public override void NPCLoot(NPC npc)
        {
            NPCLootEvent?.Invoke(npc);
        }

        public delegate void ModifyHitByItemDelegate(NPC npc, Player player, Item item, ref int damage, ref float knockback, ref bool crit);
        public static event ModifyHitByItemDelegate ModifyHitByItemEvent;
        public override void ModifyHitByItem(NPC npc, Player player, Item item, ref int damage, ref float knockback, ref bool crit)
        {
            ModifyHitByItemEvent?.Invoke(npc, player, item, ref damage, ref knockback, ref crit);
        }

        public delegate void ModifyHitByProjectileDelegate(NPC npc, Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection);
        public static event ModifyHitByProjectileDelegate ModifyHitByProjectileEvent;
        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            ModifyHitByProjectileEvent?.Invoke(npc, projectile, ref damage, ref knockback, ref crit, ref hitDirection);
        }

        public delegate void ModifyHitPlayerDelegate(NPC npc, Player target, ref int damage, ref bool crit);
        public static event ModifyHitPlayerDelegate ModifyHitPlayerEvent;
        public override void ModifyHitPlayer(NPC npc, Player target, ref int damage, ref bool crit)
		{
            ModifyHitPlayerEvent?.Invoke(npc, target, ref damage, ref crit);
        }

        public delegate void ResetEffectsDelegate(NPC npc);
        public static event ResetEffectsDelegate ResetEffectsEvent;
		public override void ResetEffects(NPC npc)
		{
            ResetEffectsEvent?.Invoke(npc);
		}
	}
}
