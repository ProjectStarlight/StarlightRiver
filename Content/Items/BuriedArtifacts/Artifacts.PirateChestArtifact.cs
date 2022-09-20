using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.BuriedArtifacts
{
    public class PirateChestArtifactItem : ModItem
    {
        public override string Texture => AssetDirectory.Archaeology + "PirateChestArtifact";

		public override bool CanRightClick() => true;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Pirate Chest");
			Tooltip.SetDefault("Right click to open\n'Sought after for centuries'");
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
			int bar = (WorldGen.gold == TileID.Gold) ? ItemID.PlatinumBar : ItemID.GoldBar;
			Item.NewItem(Item.GetSource_DropAsItem(), player.Hitbox, bar, Main.rand.Next(5,11));

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
				Item.NewItem(Item.GetSource_DropAsItem(), player.Hitbox, gems[Main.rand.Next(gems.Length)]);

			Item.NewItem(Item.GetSource_DropAsItem(), player.Hitbox, ItemID.GoldCoin, Main.rand.Next(1, 5));
			Item.NewItem(Item.GetSource_DropAsItem(), player.Hitbox, ItemID.SilverCoin, Main.rand.Next(1, 99));
		}
	}
}