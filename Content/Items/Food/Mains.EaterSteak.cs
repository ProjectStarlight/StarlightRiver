using StarlightRiver.Core;
using Terraria;
using Terraria.ID;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Items.Food
{
    internal class EaterSteak : Ingredient
    {
        public EaterSteak() : base("+3% damage reduction", 900, IngredientType.Main) { }

        public override void SafeSetDefaults() => item.rare = ItemRarityID.Blue;

        public override bool Autoload(ref string name)
        {
            StarlightNPC.NPCLootEvent += LootEaterSteak;
            return true;
        }

        public override void BuffEffects(Player player, float multiplier)
        {
            player.endurance += 0.03f;
        }

        private void LootEaterSteak(NPC npc)
        {
            if (npc.type == NPCID.EaterofSouls && Main.rand.Next(4) == 0)
                Item.NewItem(npc.Center, item.type);
        }
    }
}
