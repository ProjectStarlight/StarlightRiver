using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;

namespace StarlightRiver.Items.Banners
{
	public class VitricBatBanner : Banner { public VitricBatBanner() : base(0, "Vitric Bat Banner") { } }
	public class VitricSlimeBanner : Banner { public VitricSlimeBanner() : base(1, "Vitric Slime Banner") { }}
	public class OvergrowthNightmareBanner : Banner { public OvergrowthNightmareBanner() : base(2, "Overgrowth Nightmare Banner") { } }
	public class CorruptHornetBanner : Banner { public CorruptHornetBanner() : base(3, "Corrupt Hornet Banner") { } }
	public class OvergrowthSkeletonBanner : Banner { public OvergrowthSkeletonBanner() : base(4, "Overgrowth Skeleton Banner") { } }


	public abstract class Banner : ModItem
	{
		private readonly string ItemName;
		private readonly int BannerIndex;
		protected Banner(int index, string name)
		{
			ItemName = name;
			BannerIndex = index;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault(ItemName);
		}

		public override void SetDefaults() {
			item.width = 10;
			item.height = 24;
			item.maxStack = 99;
			item.useTurn = true;
			item.autoReuse = true;
			item.useAnimation = 15;
			item.useTime = 10;
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.consumable = true;
			item.rare = ItemRarityID.Blue;//maybe
			item.value = Item.buyPrice(0, 0, 10, 0);//maybe
			item.createTile = TileType<Tiles.Banners.MonsterBanner>();
			item.placeStyle = BannerIndex;
		}
	}
}
