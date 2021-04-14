using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Tiles.Vitric
{
    internal class VitricMusicBox : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.VitricTile + name;
            return base.Autoload(ref name, ref texture);
        }

        public override void SetDefaults() => (this).QuickSetFurniture(2, 2, DustType<Content.Dusts.Air>(), SoundID.Shatter, false, new Color(200, 255, 255), false, false, "Music Box");
        public override void KillMultiTile(int i, int j, int frameX, int frameY) => Item.NewItem(new Vector2(i, j) * 16, ItemType<VitricMusicBoxItem>(), 12);
        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            player.noThrow = 2;
            player.showItemIcon = true;
            player.showItemIcon2 = ItemType<VitricMusicBoxItem>();
        }
    }
    internal class VitricMusicBoxItem : QuickTileItem { public VitricMusicBoxItem() : base("Music Box (Vitric Desert)", "", TileType<VitricMusicBox>(), 2, AssetDirectory.VitricTile) { } }
}