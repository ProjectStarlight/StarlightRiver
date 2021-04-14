using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Tiles.Misc
{
    class DisplayCaseFriendly : DisplayCase
    {
        public override bool NewRightClick(int i, int j)
        {
            Tile tile = Main.tile[i, j];


            int index = ModContent.GetInstance<DisplayCaseEntity>().Find(i - tile.frameX / 16, j - tile.frameY / 16);

            if (index == -1)
                return true;

            DisplayCaseEntity entity = (DisplayCaseEntity)TileEntity.ByID[index];

            if (entity.containedItem is null)
            {
                entity.containedItem = Main.LocalPlayer.HeldItem.Clone();
                Main.LocalPlayer.HeldItem.TurnToAir();
            }
            else
            {
                Helpers.Helper.NewItemSpecific(Main.LocalPlayer.Center, entity.containedItem.Clone());
                entity.containedItem = null;
            }

            return true;
        }
    }

    class DisplayCaseFriendlyItem : QuickTileItem
    {
        public DisplayCaseFriendlyItem() : base("Display Case", "Can hold an item for glamorous display", ModContent.TileType<DisplayCaseFriendly>(), 2, "StarlightRiver/Assets/Tiles/Misc/") { }
    }
}
