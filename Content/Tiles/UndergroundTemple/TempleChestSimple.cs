using StarlightRiver.Content.Items.Potions;
using StarlightRiver.Core.Systems;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.UndergroundTemple
{
	class TempleChestSimple : LootChest
	{
		public override string Texture => AssetDirectory.UndergroundTempleTile + Name;

		public override int HoverItemIcon => ModContent.ItemType<TempleChestPlacer>();

		internal override List<Loot> GoldLootPool => new()
		{
				new Loot(ItemType<Items.UndergroundTemple.TemplePick>(), 1),
				new Loot(ItemType<Items.UndergroundTemple.TempleSpear>(), 1),
				new Loot(ItemType<Items.UndergroundTemple.RuneStaff>(), 1),
				new Loot(ItemType<Items.UndergroundTemple.TempleLens>(), 1),
				new Loot(ItemType<Items.UndergroundTemple.TempleRune>(), 1)
			};

		internal override List<Loot> SmallLootPool => new()
		{
				new Loot(ItemID.LesserHealingPotion, 4, 8),
				new Loot(ItemID.LesserManaPotion, 3, 6),
				new Loot(ItemID.JestersArrow, 40, 60),
				new Loot(ItemID.SilverBullet, 20, 30),
				new Loot(ItemID.Dynamite, 2, 4),
				new Loot(ItemID.SpelunkerGlowstick, 15),
				new Loot(ItemType<LesserBarrierPotion>(), 4, 8)
			};

		public override void SafeSetDefaults()
		{
			TileObjectData.newTile.DrawYOffset = 2;
			this.QuickSetFurniture(2, 2, DustID.Stone, SoundID.Tink, false, new Color(151, 151, 151));
		}
	}

	[SLRDebug]
	class TempleChestPlacer : QuickTileItem
	{
		public TempleChestPlacer() : base("Temple Chest Placer", "", "TempleChestSimple", 0, AssetDirectory.UndergroundTempleTile) { }
	}
}