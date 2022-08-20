using StarlightRiver.Core;
using Terraria.GameContent.ItemDropRules;

namespace StarlightRiver.Content.Items.Gravedigger
{
    public class LivingBlood : QuickMaterial
    {
        public override void Load()
        {
            StarlightNPC.ModifyGlobalLootEvent += AddItemToBloodMoonPool;
        }

        private void AddItemToBloodMoonPool(Terraria.ModLoader.GlobalLoot globalLoot)
        {
            globalLoot.Add(ItemDropRule.ByCondition(new Conditions.IsBloodMoonAndNotFromStatue(), Type, 10, 1, 2));
        }

        public LivingBlood() : base("Living Blood", "", 999, 50, 2, AssetDirectory.GravediggerItem) { }
    }
}