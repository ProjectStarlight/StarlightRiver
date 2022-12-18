using StarlightRiver.Content.Items.UndergroundTemple;
using System.Collections.Generic;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.UndergroundTemple
{
	class TempleLootBubble : LootBubble
	{
		public override string Texture => AssetDirectory.UndergroundTempleTile + Name;

		public override List<Loot> GoldLootPool => new()
		{
				new Loot(ItemType<RuneStaff>(), 1),
				new Loot(ItemType<TempleLens>(), 1)
			};

		public override void SetStaticDefaults()
		{
			MinPick = int.MaxValue;
			this.QuickSetFurniture(2, 2, DustType<Dusts.BlueStamina>(), SoundID.Drown, false, new Color(151, 151, 151));
		}
	}

	class TestBubble : QuickTileItem
	{
		public override string Texture => "StarlightRiver/Assets/Tiles/Bubble";

		public TestBubble() : base("Bubble", "Debug Item", "TempleLootBubble", 5, AssetDirectory.UndergroundTempleTile) { }
	}
}
