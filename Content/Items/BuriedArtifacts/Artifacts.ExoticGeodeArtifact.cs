using StarlightRiver.Content.Items.SteampunkSet;
using Terraria.ID;

namespace StarlightRiver.Content.Items.BuriedArtifacts
{
	public class ExoticGeodeArtifactItem : ModItem
	{
		public override string Texture => AssetDirectory.Archaeology + "ExoticGeodeArtifact";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Exotic Geode");
			Tooltip.SetDefault("'Incredibly shiny'\nCan be used at an extractinator");
			ItemID.Sets.ExtractinatorMode[Item.type] = Item.type;
		}

		public override void SetDefaults()
		{
			Item.width = 40;
			Item.height = 40;
			Item.value = Item.sellPrice(silver: 10);
			Item.rare = ItemRarityID.Blue;
			Item.maxStack = 30;
			Item.autoReuse = true;
			Item.consumable = true;
			Item.useAnimation = 15;
			Item.useTime = 10;
			Item.useStyle = ItemUseStyleID.Swing;
		}

		public override void ExtractinatorUse(int extractinatorBlockType, ref int resultType, ref int resultStack)
		{
			if (Main.rand.NextBool(2))
			{
				resultStack = Main.rand.Next(2, 3);
				resultType = ModContent.ItemType<AncientGear>();
				return;
			}

			int[] gems = new int[]
			{
				ItemID.Topaz,
				ItemID.Amethyst,
				ItemID.Sapphire,
				ItemID.Emerald,
				ItemID.Diamond,
				ItemID.Ruby
			};
			resultStack = Main.rand.Next(7, 15);
			resultType = Main.rand.Next(gems);
		}
	}
}