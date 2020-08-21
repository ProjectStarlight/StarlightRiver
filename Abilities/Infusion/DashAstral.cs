using StarlightRiver.Abilities.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;

namespace StarlightRiver.Abilities.Infusion
{
    class DashAstral : InfusionItem<Dash>
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Comet Rush I");
            Tooltip.SetDefault("Forbidden Winds Infustion\nDash farther and faster");
        }

        public override void SetDefaults()
        {
            item.width = 20;
            item.height = 14;
            item.rare = ItemRarityID.Green;
        }

        public override void OnActivate()
        {       
            Ability.time = Dash.maxTime + 3;

            base.OnActivate();
        }

        public override void UpdateActiveEffects()
        {
            Main.NewText("SHIT!");
        }
    }
}
