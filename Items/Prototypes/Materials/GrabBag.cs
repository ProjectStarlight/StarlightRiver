using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Items.Prototypes.Materials
{
    class GrabBag : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Component Crate");
            Tooltip.SetDefault("Right click to open\nContains rare prototype parts");
        }

        public override void SetDefaults()
        {
            item.consumable = true;
            item.rare = ItemRarityID.LightRed;
        }

        public override bool CanRightClick() => true;

        public override void RightClick(Player player)
        {
            for (int k = 0; k < Main.rand.Next(2, 4); k++) //PORT: k < Main.MasterMode ? 3 : 2
            {
                int part = Main.rand.Next(4);
                switch (part)
                {
                    case 0: Item.NewItem(player.Center, ItemType<Tubes>()); break;
                    case 1: Item.NewItem(player.Center, ItemType<Wire>()); break;
                    case 2: Item.NewItem(player.Center, ItemType<Frame>()); break;
                    case 3: Item.NewItem(player.Center, ItemType<Board>()); break;
                }
            }
        }
    }
}
