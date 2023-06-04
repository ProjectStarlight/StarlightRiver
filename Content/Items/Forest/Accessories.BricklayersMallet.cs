using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core.Systems.ChestLootSystem;
using System;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Forest
{
	public class BricklayersMallet : SmartAccessory
	{
		public override string Texture => AssetDirectory.ForestItem + Name;

		public BricklayersMallet() : base("Bricklayer's Mallet", "Doubles block placement and tool range\nDecreases mining speed by 50% for blocks outside your original range") { }

		public override void Load()
		{
			StarlightPlayer.PostUpdateEquipsEvent += UpdateBricklayersMallet;
		}

		private void UpdateBricklayersMallet(StarlightPlayer player)
		{
			if (!Equipped(player.Player))
				return;

			Player Player = player.Player;
			Player.blockRange *= 2;

			if (Main.myPlayer == Player.whoAmI)
			{
				Vector2 target;

				if (Main.SmartCursorIsUsed)
					target = new Vector2(Main.SmartCursorX * 16, Main.SmartCursorY * 16);
				else
					target = Main.MouseWorld;

				bool outsideXRange = target.X < Player.Center.X ? Math.Abs(Player.Left.X - target.X) > (Player.tileRangeX - 1) * 16f : Math.Abs(Player.Right.X - target.X) > (Player.tileRangeX - 1) * 16f;
				bool outsideYRange = target.Y > Player.Center.Y ? Math.Abs(Player.Bottom.Y - target.Y) > (Player.tileRangeY - 2) * 16f : Math.Abs(Player.Top.Y - target.Y) > (Player.tileRangeY - 2) * 16f;

				if (outsideXRange || outsideYRange)
					Player.pickSpeed *= 1.5f;

				Player.tileRangeX *= 2;
				Player.tileRangeY *= 2;
			}
		}

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.Green;
			Item.value = Item.sellPrice(silver: 25);
		}
	}

	class BricklayersMalletPool : LootPool
	{
		public override void AddLoot()
		{
			AddItem(ModContent.ItemType<BricklayersMallet>(), ChestRegionFlags.Surface, 0.25f, 1, false, -1);
		}
	}
}