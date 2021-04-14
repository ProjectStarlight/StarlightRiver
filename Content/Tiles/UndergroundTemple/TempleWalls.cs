using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Tiles.UndergroundTemple
{
    class TempleWall : ModWall
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.UndergroundTempleTile + name;
            return base.Autoload(ref name, ref texture);
        }

        public override void SetDefaults() { this.QuickSetWall(DustID.Stone, SoundID.Dig, ItemType<TempleWallItem>(), true, new Color(20, 20, 20)); } 
    }

    class TempleWallItem : QuickWallItem
    { 
        public TempleWallItem() : base("Ancient Temple Brick Wall", "", WallType<TempleWall>(), ItemRarityID.White, AssetDirectory.UndergroundTempleTile) { } 
    }

    class TempleWallBig : ModWall 
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.UndergroundTempleTile + name;
            return base.Autoload(ref name, ref texture);
        }

        public override void SetDefaults() { this.QuickSetWall(DustID.Stone, SoundID.Dig, ItemType<TempleWallBigItem>(), true, new Color(20, 20, 20)); }
    }

    class TempleWallBigItem : QuickWallItem 
    {
        public TempleWallBigItem() : base("Large Ancient Temple Brick Wall", "", WallType<TempleWallBig>(), ItemRarityID.White, AssetDirectory.UndergroundTempleTile) { } 
    }
}
