using Microsoft.Xna.Framework;
using StarlightRiver.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;

namespace StarlightRiver.NPCs.TownUpgrade
{
    class GuideUpgrade : TownUpgrade
    {
        public GuideUpgrade() : base("Guide", "Chungus Study", "Help the guide study big chungus!", "Rift Crafting", "Scholar") { }

        public override List<Loot> Requirements => new List<Loot>()
        {
            new Loot(ItemID.DirtBlock, 20),
            new Loot(ItemID.Gel, 10),
            new Loot(ItemID.Wood, 50)
        };

        public override void ClickButton()
        {
            Main.NewText("The guide shit himself. Good job.", Color.Brown);
        }
    }
}
