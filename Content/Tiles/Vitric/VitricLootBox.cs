using StarlightRiver.Content.Dusts;
using StarlightRiver.Core.Systems;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Vitric
{
	class VitricLootBox : LootChest
	{
		public override string Texture => AssetDirectory.VitricTile + Name;
		public override int HoverItemIcon => ModContent.ItemType<VitricLootBoxItem>();

		internal override List<Loot> GoldLootPool => new()
		{
				new Loot(ItemType<Items.Vitric.VitricBow>(), 1),
				new Loot(ItemType<Items.Vitric.VitricSword>(), 1)
			};

		internal override List<Loot> SmallLootPool => new()
		{
				new Loot(ItemID.LesserHealingPotion, 4, 8),
				new Loot(ItemID.LesserManaPotion, 3, 6),
				new Loot(ItemID.JestersArrow, 40, 60),
				new Loot(ItemID.SilverBullet, 20, 30),
				new Loot(ItemID.Dynamite, 2, 4),
				new Loot(ItemID.SpelunkerGlowstick, 15),
				new Loot(ItemType<Items.Vitric.SandstoneChunk>(), 3, 6),
			};

		public override void SafeSetDefaults()
		{
			TileObjectData.newTile.DrawYOffset = 2;
			this.QuickSetFurniture(2, 2, DustType<GlassGravity>(), SoundID.Tink, false, new Color(151, 151, 151));
		}
	}

	[SLRDebug]
	class VitricLootBoxItem : QuickTileItem
	{
		public VitricLootBoxItem() : base("Vitric Loot Box Item", "", "VitricLootBox", 1, AssetDirectory.VitricTile, false) { }
	}
}