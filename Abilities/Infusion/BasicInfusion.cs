using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace StarlightRiver.Abilities.Infusion
{
    class BasicInfusion : InfusionItem
    {
        public override Type AbilityType => typeof(Content.Dash);

        public override void SetDefaults()
        {
            item.width = 20;
            item.height = 14;
            item.rare = ItemRarityID.Blue;
        }
    }
}
