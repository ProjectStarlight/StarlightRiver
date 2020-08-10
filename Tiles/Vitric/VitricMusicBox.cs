using Microsoft.Xna.Framework;
using StarlightRiver.Items;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Tiles.Vitric
{
    internal class VitricMusicBox : ModTile
    {
        public override void SetDefaults() => QuickBlock.QuickSetFurniture(this, 2, 2, DustType<Dusts.Air>(), SoundID.Shatter, false, new Color(200, 255, 255), false, false, "Music Box");
        public override void KillMultiTile(int i, int j, int frameX, int frameY) => Item.NewItem(new Vector2(i, j) * 16, ItemType<VitricMusicBoxItem>(), 12);
        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            player.noThrow = 2;
            player.showItemIcon = true;
            player.showItemIcon2 = ItemType<VitricMusicBoxItem>();
        }
    }
    internal class VitricMusicBoxItem : QuickTileItem { public VitricMusicBoxItem() : base("Music Box (Vitric Desert)", "", TileType<VitricMusicBox>(), 2) { } }
}