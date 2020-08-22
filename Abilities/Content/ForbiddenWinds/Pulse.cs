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
            Tooltip.SetDefault("Forbidden Winds Infusion\nMakes dashes shorter and more controlled\n reduces stamina cost by 0.2");
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
            Ability.Speed -= 5;
            Ability.CooldownBonus -= 15;
            Ability.ActivationCostBonus -= 0.2f;
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
                Tooltip.SetDefault("Forbidden Winds Infusion\nMakes dashes even shorter and even more frequent\nDecreases stamina usage by 0.35");
            }

            public override void OnActivate()
            {
                Ability.Time -= 1;
                Ability.Speed -= 3;
                Ability.CooldownBonus -= 15;
                Ability.ActivationCostBonus -= 0.15f;
                Ability.OnActivate();
            }
        }

        class Pulse3 : Pulse
        {
            public override void SetStaticDefaults()
            {
                DisplayName.SetDefault("Pulse III");
                Tooltip.SetDefault("Forbidden Winds Infusion\nMakes dashes super short and frequent\nDecreases stamina usage by 0.5");
            }

            public override void OnActivate()
            {
                Ability.Time -= 1;
                Ability.Speed -= 3;
                Ability.CooldownBonus -= 15;
                Ability.ActivationCostBonus -= 0.15f;
                Ability.OnActivate();
            }
        }
    }
}
