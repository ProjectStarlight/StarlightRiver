﻿using Terraria.GameContent.ItemDropRules;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Food
{
	internal class Rabbit : Ingredient
	{
		public Rabbit() : base("+5% melee damage", 600, IngredientType.Main) { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.White;

			Item.value = Item.sellPrice(silver: 10);
		}

		public override void Load()
		{
			StarlightNPC.ModifyNPCLootEvent += LootRabbit;
		}

		public override void BuffEffects(Player Player, float multiplier)
		{
			Player.GetDamage(DamageClass.Melee) += 0.05f * multiplier;
		}

		private void LootRabbit(NPC NPC, NPCLoot npcloot)
		{
			if (NPC.type == NPCID.Bunny)
				npcloot.Add(ItemDropRule.Common(ModContent.ItemType<Rabbit>(), 4));
		}
	}
}