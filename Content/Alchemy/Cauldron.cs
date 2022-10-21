using StarlightRiver.Content.Dusts;
using Terraria.DataStructures;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Alchemy
{
	public class CauldronItem : ModItem
	{
		public override string Texture => AssetDirectory.Alchemy + Name;

		public override void SetStaticDefaults()
		{
			Tooltip.SetDefault("Alchemic Cauldron");
		}

		public override void SetDefaults()
		{
			Item.width = 26;
			Item.height = 22;
			Item.maxStack = 99;
			Item.useTurn = true;
			Item.autoReuse = true;
			Item.useAnimation = 15;
			Item.useTime = 15;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.consumable = true;
			Item.value = 500;
			Item.createTile = ModContent.TileType<CauldronTile>();
		}
	}

	internal class CauldronTile : DummyTile
	{
		public override int DummyType => ProjectileType<CauldronDummy>();

		public override string Texture => AssetDirectory.Alchemy + Name;

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			int item = Item.NewItem(new EntitySource_TileBreak(i, j), new Vector2(i, j) * 16, ItemType<CauldronItem>(), 1);

			// Sync the drop for multiPlayer
			if (Main.netMode == NetmodeID.MultiplayerClient && item >= 0)
				NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item, 1f);
		}

		public override void SafeNearbyEffects(int i, int j, bool closer)
		{
		}

		public override void SetStaticDefaults()
		{
			this.QuickSetFurniture(3, 2, DustType<Air>(), SoundID.Tink, true, new Color(50, 50, 50), false, false, "Alchemic Cauldron");
		}

		public override bool RightClick(int i, int j)
		{
			int x = i - Main.tile[i, j].TileFrameX / 16 % 3;
			int y = j - Main.tile[i, j].TileFrameY / 16 % 2;
			if (DummyExists(x, y, DummyType))
			{
				var cauldronDummy = (CauldronDummyAbstract)Dummy(x, y).ModProjectile;
				if (Main.LocalPlayer.HeldItem.type == ModContent.ItemType<MixingStick>())
				{
					cauldronDummy.AttemptStartCraft();
				}
				else
				{
					cauldronDummy.DumpIngredients();
				}
			}

			return false;
		}
	}

	public class CauldronDummy : CauldronDummyAbstract
	{
		public CauldronDummy() : base(TileType<CauldronTile>(), 48, 32) { }
	}
}
