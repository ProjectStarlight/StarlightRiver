using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Food
{
	internal class FaceSteak : Ingredient
    {
        public FaceSteak() : base("+3% critical strike chance", 900, IngredientType.Main) { }

        public override void SafeSetDefaults() => Item.rare = ItemRarityID.Blue;

        public override void Load()
        {
            StarlightNPC.OnKillEvent += LootMonsterSteak;
        }

        public override void BuffEffects(Player Player, float multiplier)
        {
            Player.GetCritChance(DamageClass.Melee) += 3;
            Player.GetCritChance(DamageClass.Ranged) += 3;
            Player.GetCritChance(DamageClass.Magic) += 3;
        }

        private void LootMonsterSteak(NPC NPC)
        {
            if (NPC.type == NPCID.FaceMonster && Main.rand.Next(4) == 0)
                Item.NewItem(NPC.GetItemSource_Loot(), NPC.Center, Item.type);
        }
    }
}
