using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Food
{
	internal class Rabbit : Ingredient
    {
        public Rabbit() : base("+5% melee damage", 600, IngredientType.Main) { }

        public override void SafeSetDefaults() => Item.rare = ItemRarityID.White;

        public override void Load()
        {
            StarlightNPC.OnKillEvent += LootRabbit;
        }

        public override void BuffEffects(Player Player, float multiplier)
        {
            Player.GetDamage(DamageClass.Melee) += 0.05f * multiplier;
        }

        private void LootRabbit(NPC NPC)
        {
            if (NPC.type == NPCID.Bunny && Main.rand.Next(4) == 0)
                Item.NewItem(NPC.GetSource_Loot(), NPC.Center, Item.type);
        }
    }
}