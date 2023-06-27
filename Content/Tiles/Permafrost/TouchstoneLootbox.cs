﻿using StarlightRiver.Content.Items.Permafrost;
using StarlightRiver.Core.Systems;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Permafrost
{
	class TouchstoneLootbox : LootChest
	{
		public override string Texture => AssetDirectory.PermafrostTile + Name;

		public override int HoverItemIcon => ModContent.ItemType<TouchstoneLootboxItem>();

		internal override List<Loot> GoldLootPool => new()
		{
				new Loot(ItemID.BlizzardinaBottle, 1),
				new Loot(ItemID.FlurryBoots, 1),
				new Loot(ItemType<BookOfFrost>(), 1)
			};

		internal override List<Loot> SmallLootPool => new()
		{
				new Loot(ItemID.HealingPotion, 2, 5),
				new Loot(ItemID.ManaPotion, 3, 6),
				new Loot(ItemID.FrostburnArrow, 40, 60),
				new Loot(ItemID.LifeCrystal, 1, 1),
				new Loot(ItemID.ManaCrystal, 2, 2),
				new Loot(ItemID.PlatinumBar, 5, 7),
				new Loot(ItemID.Diamond, 1, 1),
				new Loot(ItemID.IceBlock, 50, 100),
				new Loot(ItemType<AuroraIceBar>(), 2, 5),
				new Loot(ItemType<SquidBossSpawn>(), 1, 1)
			};

		public override void SafeSetDefaults()
		{
			TileObjectData.newTile.DrawYOffset = 2;
			QuickBlock.QuickSetFurniture(this, 2, 2, DustID.Stone, SoundID.Tink, false, new Color(151, 151, 151));
		}
	}

	[SLRDebug]
	class TouchstoneLootboxItem : QuickTileItem
	{
		public TouchstoneLootboxItem() : base("Touchstone Chest Placer", "", "TouchstoneLootbox", 0, AssetDirectory.PermafrostTile) { }
	}
}