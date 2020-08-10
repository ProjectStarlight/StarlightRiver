using Microsoft.Xna.Framework;
using StarlightRiver.Items;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Tiles.Permafrost
{
    class AuroraBrickWall : ModWall
    {
        public override void SetDefaults() => QuickBlock.QuickSetWall(this, DustID.Ice, SoundID.Tink, ItemType<AuroraBrickWallItem>(), false, new Color(50, 155, 155));
    }

    class AuroraBrickWallItem : QuickWallItem
    {
        public AuroraBrickWallItem() : base("Aurora BrickWall", "Oooh... Preeetttyyy", WallType<AuroraBrickWall>(), ItemRarityID.White) { }
    }
}
