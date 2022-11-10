using StarlightRiver.Content.Items.Vitric;
using System.Collections.Generic;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Dungeon
{
	public class StarlightPendant : ModItem
	{
		private int currentMana = -1;

		private readonly List<int> manaConsumed = new();

		public override string Texture => AssetDirectory.DungeonItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Starlight Pendant");
			Tooltip.SetDefault("Barrier boosts mana regen by 1% per point of currently active barrier \n" +
				"Consuming mana boosts barrier regen by 1% per 4 points of mana consumed in the past 5 seconds \n" +
				"-10 barrier");
		}

		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 28;
			Item.rare = ItemRarityID.Green;
			Item.value = Item.buyPrice(0, 5, 0, 0);
			Item.accessory = true;
		}

		public override void UpdateAccessory(Player Player, bool hideVisual)
		{
			Player.GetModPlayer<BarrierPlayer>().MaxBarrier = (int)MathHelper.Max(0, Player.GetModPlayer<BarrierPlayer>().MaxBarrier - 10);

			if (currentMana != Player.statMana || Player.itemTime >= 2)
			{
				if (currentMana > Player.statMana)
					manaConsumed.Add(currentMana - Player.statMana);
				else if (Player.itemTime >= 2)
					manaConsumed.Add(0);
				currentMana = Player.statMana;
			}

			while (manaConsumed.Count > 300)
				manaConsumed.RemoveAt(0);

			int manaConsumedTotal = 0;
			foreach (int i in manaConsumed)
				manaConsumedTotal += i;
			float manaBonus = 1 + Player.GetModPlayer<BarrierPlayer>().Barrier * 0.01f;

			/*if (manaBonus > 1)
				Main.NewText(manaBonus.ToString());*/
			Player.manaRegen = (int)(Player.manaRegen * manaBonus);

			Player.GetModPlayer<BarrierPlayer>().RechargeRate = (int)(Player.GetModPlayer<BarrierPlayer>().RechargeRate * 1 + manaConsumedTotal * 0.025f);
			Player.GetModPlayer<BarrierPlayer>().RechargeDelay = (int)(Player.GetModPlayer<BarrierPlayer>().RechargeDelay / (1 + manaConsumedTotal * 0.025f));
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<SandstoneChunk>(), 4);
			recipe.AddIngredient(ModContent.ItemType<AquaSapphire>(), 1);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
}