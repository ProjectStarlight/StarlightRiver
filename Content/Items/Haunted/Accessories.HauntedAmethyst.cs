using StarlightRiver.Content.Items.BaseTypes;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Haunted
{
	public class HauntedAmethyst : SmartAccessory
	{
		internal bool summoned;

		public override string Texture => AssetDirectory.JungleItem + Name;

		public HauntedAmethyst() : base("Haunted Amethyst",
			"Increases your number of max minions by 1 when below 50% life\n" +
			"Automatically summons a minion if there is a valid weapon in your inventory")
		{ }

		public override void SafeSetDefaults()
		{
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Green;
		}

		public override void SafeUpdateEquip(Player player)
		{
			if (player.statLife < player.statLifeMax2 * 0.5f)
			{
				player.maxMinions += 1;

				if (!summoned)
				{
					Helpers.SummonerHelper.RespawnMinions(player, 1);
					summoned = true;
				}
			}
			else
			{
				summoned = false;
			}
		}

		public override void AddRecipes()
		{
			CreateRecipe().
				AddIngredient(ItemID.Amethyst, 5).
				AddTile(TileID.DemonAltar).
				Register();
		}
	}
}