using Terraria.GameContent.ItemDropRules;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Food
{
	internal class Gelatine : Ingredient
	{
		public Gelatine() : base("+6% damage reduction\n+3 defense", 1600, IngredientType.Main) { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.Blue;
		}

		public override void Load()
		{
			StarlightNPC.ModifyNPCLootEvent += LootGelatine;
		}

		public override void BuffEffects(Player Player, float multiplier)
		{
			Player.endurance += 0.06f;
			Player.statDefense += 3;
		}

		private void LootGelatine(NPC npc, NPCLoot npcloot)
		{
			if (npc.type == NPCID.KingSlime)
				npcloot.Add(ItemDropRule.Common(ModContent.ItemType<Gelatine>(), 1, 5, 25));
		}
	}
}