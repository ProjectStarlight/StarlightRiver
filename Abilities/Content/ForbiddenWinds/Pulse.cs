using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Abilities.Content.ForbiddenWinds
{
    public class Pulse : InfusionItem<Dash>
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Pulse I");
            Tooltip.SetDefault("Forbidden Winds Infusion\nDashes become short, frequent, and potent pulses\nDecreases Dash stamina cost by 0.15");
        }

        public override void SetDefaults()
        {
            item.width = 20;
            item.height = 14;
            item.rare = ItemRarityID.Green;
        }

        public override void OnActivate()
        {
            Ability.Time -= 4;
            Ability.CooldownBonus -= 15;
            Ability.ActivationCostBonus -= 0.15f;
            Ability.Boost += 0.15f;
            base.OnActivate();
        }

        public override void UpdateActiveEffects()
        {
            // TODO visuals
            base.UpdateActiveEffects();
        }

        class Pulse2 : Pulse
        {
            public override void SetStaticDefaults()
            {
                DisplayName.SetDefault("Pulse II");
                Tooltip.SetDefault("Forbidden Winds Infusion\nDashes become even shorter, more frequent, and more potent pulses\nDecreases Dash stamina cost by 0.25");
            }

            public override void OnActivate()
            {
                Ability.Time -= 1;
                Ability.CooldownBonus -= 10;
                Ability.ActivationCostBonus -= 0.1f;
                Ability.Boost += 0.15f;
                base.OnActivate();
            }
        }

        class Pulse3 : Pulse2
        {
            public override void SetStaticDefaults()
            {
                DisplayName.SetDefault("Pulse III");
                Tooltip.SetDefault("Forbidden Winds Infusion\nDashes become strong rapid-fire pulses\nDecreases Dash stamina cost by 0.5");
            }

            public override void OnActivate()
            {
                Ability.Time -= 1;
                Ability.CooldownBonus -= 10;
                Ability.ActivationCostBonus -= 0.25f;
                Ability.Boost += 0.15f;
                base.OnActivate();
            }
        }
    }
}
