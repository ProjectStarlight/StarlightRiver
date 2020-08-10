using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace StarlightRiver.NPCs.TownUpgrade
{
    class MerchantUpgrade : TownUpgrade
    {
        public MerchantUpgrade() : base("Merchant", "OnlyFans Account", "Help the merchant set up an OnlyFans!", "Nudes", "Stripper") { }

        public override void ClickButton()
        {
            Main.NewText("The merchant shit himself. Good job.", Color.Brown);
        }
    }
}
