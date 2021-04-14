using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Tiles.Interactive
{
    internal class VoidDoorOn : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.InteractiveTile + name;
            return true;
        }

        public override void SetDefaults()
        {
            (this).QuickSet(int.MaxValue, DustType<Dusts.Void>(), SoundID.Drown, Color.Black, ItemType<VoidDoorItem>());
            Main.tileMerge[Type][TileType<VoidDoorOff>()] = true;
            animationFrameHeight = 88;
        }

        public override void AnimateTile(ref int frame, ref int frameCounter)
        {
            if (++frameCounter >= 5)
            {
                frameCounter = 0;
                if (++frame >= 3) frame = 0;
            }
        }
    }

    internal class VoidDoorOff : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.InteractiveTile + name;
            return true;
        }

        public override void SetDefaults()
        {
            drop = ItemType<VoidDoorItem>();
            dustType = DustType<Dusts.Void>();
            Main.tileMerge[Type][TileType<VoidDoorOn>()] = true;
        }
    }

    public class VoidDoorItem : QuickTileItem 
    { 
        public VoidDoorItem() : base("Void Barrier", "Dissappears when Purified", TileType<VoidDoorOn>(), 8, AssetDirectory.InteractiveTile) { } 
    }
}