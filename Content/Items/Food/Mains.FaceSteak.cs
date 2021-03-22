using StarlightRiver.Core;
using Terraria;
using Terraria.ID;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Items.Food
{
    internal class FaceSteak : Ingredient
    {
        public FaceSteak() : base("+3% critical strike chance", 900, IngredientType.Main) { }

        public override void SafeSetDefaults() => item.rare = ItemRarityID.Blue;

        public override bool Autoload(ref string name)
        {
            StarlightNPC.NPCLootEvent += LootMonsterSteak;
            return true;
        }

        public override void BuffEffects(Player player, float multiplier)
        {
            player.meleeCrit += 3;
            player.rangedCrit += 3;
            player.magicCrit += 3;
        }

        private void LootMonsterSteak(NPC npc)
        {
            if (npc.type == NPCID.FaceMonster && Main.rand.Next(4) == 0)
                Item.NewItem(npc.Center, item.type);
        }
    }
}
