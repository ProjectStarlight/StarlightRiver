using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Tiles.Permafrost
{
    class AuroraBrickWall : ModWall
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = "StarlightRiver/Assets/Tiles/Permafrost/AuroraBrickWall";
            return true;
        }

        public override void SetDefaults() => QuickBlock.QuickSetWall(this, DustID.Ice, SoundID.Tink, ItemType<AuroraBrickWallItem>(), true, new Color(33, 65, 94));
    }

    class AuroraBrickWallItem : QuickWallItem
    {
        public override string Texture => "StarlightRiver/Assets/Tiles/Permafrost/AuroraBrickWallItem";

        public AuroraBrickWallItem() : base("Aurora BrickWall", "Oooh... Preeetttyyy", WallType<AuroraBrickWall>(), ItemRarityID.White) { }
    }
}
