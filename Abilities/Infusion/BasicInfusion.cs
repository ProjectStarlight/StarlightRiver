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
        public override Type AbilityType => null;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Basic Infusion I");
            Tooltip.SetDefault("Generic Infusion\nSlightly improves stamina regeneration\nUsed to create other infusions");
        }

        public override void SetDefaults()
        {
            item.width = 20;
            item.height = 14;
            item.rare = ItemRarityID.Blue;
        }

        public override void UpdateFixed()
        {
            Player.GetHandler().StaminaRegenRate += 0.2f;
        }
    }

    class TestInfusion : InfusionItem<Content.Pure>
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Test Infusion");
            Tooltip.SetDefault("IM GONNA SHIT MYSELF OH MY FUCKING GODDDD");
        }

        public override void SetDefaults()
        {
            item.width = 20;
            item.height = 14;
            item.rare = ItemRarityID.Blue;
        }
    }
}
