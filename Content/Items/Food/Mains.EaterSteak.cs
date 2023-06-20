using Terraria.GameContent.ItemDropRules;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Food
{
	internal class EaterSteak : Ingredient
	{
		public EaterSteak() : base("+10% damage reduction", 900, IngredientType.Main) { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.Blue;

			Item.value = Item.sellPrice(silver: 20);
		}

		public override void Load()
		{
			StarlightNPC.ModifyNPCLootEvent += LootEaterSteak;
		}

		public override void BuffEffects(Player Player, float multiplier)
		{
			Player.endurance += 0.1f;
		}

		private void LootEaterSteak(NPC NPC, NPCLoot npcloot)
		{
			if (NPC.type == NPCID.EaterofSouls)
				npcloot.Add(ItemDropRule.Common(ModContent.ItemType<EaterSteak>(), 8));
		}
	}
}