using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Core
{
    public partial class StarlightNPC : GlobalNPC
    {
        public delegate void PostAIDelegate(NPC NPC);
        public static event PostAIDelegate PostAIEvent;
        public override void PostAI(NPC NPC)
        {
            PostAIEvent?.Invoke(NPC);
        }

        public delegate void OnKillDelegate(NPC NPC);
        public static event OnKillDelegate OnKillEvent;
        public override void OnKill(NPC NPC)
        {
            OnKillEvent?.Invoke(NPC);
        }

        //these modify hit bys should only be used for editting the ref variables if you want them changed in a way that happens BEFORE Player on hit effects run. no extra effects will be synced in multiPlayer outside of the ref variables
        public delegate void ModifyHitByItemDelegate(NPC NPC, Player Player, Item Item, ref int damage, ref float knockback, ref bool crit);
        public static event ModifyHitByItemDelegate ModifyHitByItemEvent;
        public override void ModifyHitByItem(NPC NPC, Player Player, Item Item, ref int damage, ref float knockback, ref bool crit)
        {
            ModifyHitByItemEvent?.Invoke(NPC, Player, Item, ref damage, ref knockback, ref crit);
        }

        public delegate void ModifyHitByProjectileDelegate(NPC NPC, Projectile Projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection);
        public static event ModifyHitByProjectileDelegate ModifyHitByProjectileEvent;
        public override void ModifyHitByProjectile(NPC NPC, Projectile Projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            ModifyHitByProjectileEvent?.Invoke(NPC, Projectile, ref damage, ref knockback, ref crit, ref hitDirection);
        }

        public delegate void ModifyHitPlayerDelegate(NPC NPC, Player target, ref int damage, ref bool crit);
        public static event ModifyHitPlayerDelegate ModifyHitPlayerEvent;
        public override void ModifyHitPlayer(NPC NPC, Player target, ref int damage, ref bool crit)
        {
            ModifyHitPlayerEvent?.Invoke(NPC, target, ref damage, ref crit);
        }

        public delegate void ResetEffectsDelegate(NPC NPC);
        public static event ResetEffectsDelegate ResetEffectsEvent;
        public override void ResetEffects(NPC NPC)
        {
            ResetEffectsEvent?.Invoke(NPC);
        }

        public delegate void ModifyNPCLootDelegate(NPC npc, NPCLoot npcloot);
        public static event ModifyNPCLootDelegate ModifyNPCLootEvent;   
		public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
		{
            ModifyNPCLootEvent?.Invoke(npc, npcLoot);
		}

        public delegate void ModifyGlobalLootDelegate(GlobalLoot globalLoot);
        public static event ModifyGlobalLootDelegate ModifyGlobalLootEvent;
        public override void ModifyGlobalLoot(GlobalLoot globalLoot)
        {
            ModifyGlobalLootEvent?.Invoke(globalLoot);
        }

        public override void Unload()
		{
            PostAIEvent = null;
            OnKillEvent = null;
            ModifyHitByItemEvent = null;
            ModifyHitByProjectileEvent = null;
            ModifyHitPlayerEvent = null;
            ResetEffectsEvent = null;
            ModifyNPCLootEvent = null;
            ModifyGlobalLootEvent = null;
		}
	}
}
