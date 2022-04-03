using StarlightRiver.Core;
using Terraria;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Food
{
	internal class FaceSteak : Ingredient
    {
        public FaceSteak() : base("+3% critical strike chance", 900, IngredientType.Main) { }

        public override void SafeSetDefaults() => Item.rare = ItemRarityID.Blue;

        public override void Load()
        {
            StarlightNPC.NPCLootEvent += LootMonsterSteak;
            return true;
        }

        public override void BuffEffects(Player Player, float multiplier)
        {
            Player.meleeCrit += 3;
            Player.rangedCrit += 3;
            Player.magicCrit += 3;
        }

        private void LootMonsterSteak(NPC NPC)
        {
            if (NPC.type == NPCID.FaceMonster && Main.rand.Next(4) == 0)
                Item.NewItem(NPC.Center, Item.type);
        }
    }
}
