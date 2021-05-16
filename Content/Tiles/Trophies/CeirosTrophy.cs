using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Tiles.Trophies
{
    class CeirosTrophy : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.TrophyTile + name;
            return base.Autoload(ref name, ref texture);
        }

        public override void SetDefaults()
        {
            //this.QuickSetFurniture(3, 3, 7, SoundID.Dig, false, new Color(120, 85, 60), false, false, "Trophy");
            this.QuickSetPainting(3, 3, 7, new Color(120, 85, 60), "Trophy");
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY) =>
            Item.NewItem(new Vector2(i, j) * 16, ItemType<CeirosTrophyItem>());
    }

    class CeirosTrophyItem : QuickTileItem
    {
        public CeirosTrophyItem() : base("Ceiros Trophy", "", TileType<CeirosTrophy>(), 1, AssetDirectory.TrophyTile) { }
    }
}
