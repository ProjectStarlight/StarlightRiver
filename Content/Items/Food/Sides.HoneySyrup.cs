using System;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Food
{
	internal class HoneySyrup : Ingredient
	{
		public HoneySyrup() : base("Heal 100 life on use\n5% reduced movement speed", 3600, IngredientType.Side) { }

		public override void Load()
		{
			StarlightNPC.ModifyNPCLootEvent += DropHoney;
		}

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.Blue;

			Item.value = Item.sellPrice(silver: 10);
		}

		public override void OnUseEffects(Player player, float multiplier)
		{
			int heal = (int)(100 * multiplier);
			player.statLife += heal;
			player.HealEffect(heal);
		}

		public override void BuffEffects(Player Player, float multiplier)
		{
			Player.velocity.X *= 1 - 0.05f * multiplier;//todo: this isnt correct, and breaks running
		}

		private void DropHoney(NPC npc, NPCLoot npcloot)
		{
			if (npc.type == NPCID.QueenBee)
				npcloot.Add(ItemDropRule.Common(Type, 1, 5, 10));
		}
	}
}