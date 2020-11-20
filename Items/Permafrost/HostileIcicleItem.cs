using Terraria.ID;
using Terraria.ModLoader;
using StarlightRiver.Tiles.Permafrost;

namespace StarlightRiver.Items.Permafrost
{
	public class HostileIcicleItem : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Hostile Icicle");
		}


		public override void SetDefaults()
		{
			item.width = 64;
			item.height = 34;
			item.value = 150;

			item.maxStack = 99;

			item.useStyle = ItemUseStyleID.SwingThrow;
			item.useTime = 10;
			item.useAnimation = 15;

			item.useTurn = true;
			item.autoReuse = true;
			item.consumable = true;

			item.createTile = ModContent.TileType<HostileIcicle>();
		}

	}
}