using StarlightRiver.Core;
using Terraria.GameContent.ItemDropRules;

namespace StarlightRiver.Content.Items.Gravedigger
{
    public class LivingBlood : QuickMaterial
    {
        public override void Load()
        {
            StarlightNPC.ModifyNPCLootEvent += AddItemToBloodMoonPool;
        }

        private void AddItemToBloodMoonPool(Terraria.NPC npc, Terraria.ModLoader.NPCLoot npcloot)
        {
            if (npc.damage > 0 && npc.HasPlayerTarget)
                npcloot.Add(ItemDropRule.ByCondition(new Conditions.IsBloodMoonAndNotFromStatue(), Type, 20, 1, 2));
        }

        public LivingBlood() : base("Living Blood", "", 999, 50, 2, AssetDirectory.GravediggerItem) { }
    }
}