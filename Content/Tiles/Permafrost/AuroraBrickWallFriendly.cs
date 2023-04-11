using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Permafrost
{
	class AuroraBrickWallFriendly : ModWall
	{
		public override string Texture => "StarlightRiver/Assets/Tiles/Permafrost/AuroraBrickWall";

		public override void SetStaticDefaults()
		{
			QuickBlock.QuickSetWall(this, DustID.Ice, SoundID.Tink, ItemType<AuroraBrickWallFriendlyItem>(), true, new Color(33, 65, 94));
		}
	}

	class AuroraBrickWallFriendlyItem : QuickWallItem
	{
		public override string Texture => "StarlightRiver/Assets/Tiles/Permafrost/AuroraBrickWallItem";

		public AuroraBrickWallFriendlyItem() : base("Aurora BrickWall", "Oooh... Preeetttyyy", WallType<AuroraBrickWallFriendly>(), ItemRarityID.White) { }
	}
}