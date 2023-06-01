using StarlightRiver.Core.Systems;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Permafrost
{
	class AuroraBrickWall : ModWall
	{
		public override string Texture => "StarlightRiver/Assets/Tiles/Permafrost/AuroraBrickWall";

		public override void SetStaticDefaults()
		{
			QuickBlock.QuickSetWall(this, DustID.Ice, SoundID.Tink, ItemType<AuroraBrickWallItem>(), true, new Color(33, 65, 94));
		}
	}

	[SLRDebug]
	class AuroraBrickWallItem : QuickWallItem
	{
		public override string Texture => "StarlightRiver/Assets/Tiles/Permafrost/AuroraBrickWallItem";

		public AuroraBrickWallItem() : base("Aurora Brick Wall Dangerous", "Oooh... Preeetttyyy", WallType<AuroraBrickWall>(), ItemRarityID.White) { }
	}
}