using StarlightRiver.Content.Items.BaseTypes;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.UndergroundTemple;

class TempleWall : ModWall
{
	public override string Texture => AssetDirectory.UndergroundTempleTile + Name;

	public override void SetStaticDefaults() { this.QuickSetWall(DustID.Stone, SoundID.Dig, ItemType<TempleWallItem>(), true, new Color(20, 20, 20)); }
}

class TempleWallItem : BaseWallItem
{
	public TempleWallItem() : base("Ancient Temple Brick Wall", "", WallType<TempleWall>(), ItemRarityID.White, AssetDirectory.UndergroundTempleTile) { }
}

class TempleWallBig : ModWall
{
	public override string Texture => AssetDirectory.UndergroundTempleTile + Name;

	public override void SetStaticDefaults() { this.QuickSetWall(DustID.Stone, SoundID.Dig, ItemType<TempleWallBigItem>(), true, new Color(20, 20, 20)); }
}

class TempleWallBigItem : BaseWallItem
{
	public TempleWallBigItem() : base("Large Ancient Temple Brick Wall", "", WallType<TempleWallBig>(), ItemRarityID.White, AssetDirectory.UndergroundTempleTile) { }
}