using Terraria.ID;
using Terraria.WorldBuilding;

namespace StarlightRiver.Content.Items.BuriedArtifacts
{
	public class PirateChestArtifactItem : ModItem
	{
		public override string Texture => AssetDirectory.Archaeology + "PirateChestArtifact";

		public override bool CanRightClick()
		{
			return true;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Pirate Chest");
			Tooltip.SetDefault("<right> to open\n'Sought after for centuries'");
		}

		public override void SetDefaults()
		{
			Item.width = 40;
			Item.height = 40;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Orange;
			Item.maxStack = 30;
			Item.autoReuse = true;
		}

		public override void RightClick(Player player)
		{
			int bar = (GenVars.gold == TileID.Gold) ? ItemID.PlatinumBar : ItemID.GoldBar;
			player.QuickSpawnItem(Item.GetSource_DropAsItem(), bar, Main.rand.Next(5,11));

			int[] gems = new int[]
			{
				ItemID.Topaz,
				ItemID.Amethyst,
				ItemID.Sapphire,
				ItemID.Emerald,
				ItemID.Diamond,
				ItemID.Ruby
			};
			int numGems = Main.rand.Next(15, 21);

			for (int i = 0; i < numGems; i++)
				player.QuickSpawnItem(Item.GetSource_DropAsItem(), gems[Main.rand.Next(gems.Length)]);

			player.QuickSpawnItem(Item.GetSource_DropAsItem(), ItemID.GoldCoin, Main.rand.Next(1, 5));
			player.QuickSpawnItem(Item.GetSource_DropAsItem(), ItemID.SilverCoin, Main.rand.Next(1, 99));
		}
	}
}