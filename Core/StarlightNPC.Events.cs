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

        public delegate void NPCLootDelegate(NPC NPC);
        public static event NPCLootDelegate NPCLootEvent; //PORTTODO: Figure out how to work this with the new loot system
        public override void NPCLoot(NPC NPC)
        {
            NPCLootEvent?.Invoke(NPC);
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
    }
}
