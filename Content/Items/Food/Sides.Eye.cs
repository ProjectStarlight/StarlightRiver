using Terraria.GameContent.ItemDropRules;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Food
{
	internal class Eye : Ingredient
	{
		public Eye() : base("+10% critical strike chance", 900, IngredientType.Side) { }

		public override void Load()
		{
			StarlightNPC.ModifyNPCLootEvent += DropEye;
		}

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.White;

			Item.value = Item.sellPrice(silver: 5);
		}

		public override void BuffEffects(Player Player, float multiplier)
		{
			Player.GetCritChance(DamageClass.Generic) += 0.1f * multiplier;
		}

		private void DropEye(NPC npc, NPCLoot npcloot)
		{
			if (npc.type == NPCID.DemonEye || npc.type == NPCID.DemonEye2 || npc.type == NPCID.WanderingEye)
				npcloot.Add(ItemDropRule.Common(Type, 4, 1, 1));
		}
	}
}