using StarlightRiver.Core;
using Terraria;
using Terraria.ID;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Items.Food
{
    internal class Rabbit : Ingredient
    {
        public Rabbit() : base("+5% melee damage", 600, IngredientType.Main) { }

        public override void SafeSetDefaults() => item.rare = ItemRarityID.White;

        public override bool Autoload(ref string name)
        {
            StarlightNPC.NPCLootEvent += LootRabbit;
            return true;
        }

        public override void BuffEffects(Player player, float multiplier)
        {
            player.meleeDamageMult += 0.05f * multiplier;
        }

        private void LootRabbit(NPC npc)
        {
            if (npc.type == NPCID.Bunny && Main.rand.Next(4) == 0)
                Item.NewItem(npc.Center, item.type);
        }
    }
}