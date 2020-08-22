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
        public override string Texture => ModContent.GetInstance<Astral>().Texture;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Pulse I");
            Tooltip.SetDefault("Forbidden Winds Infusion\nMakes dashes shorter and more controlled\nShorter cooldown and decreases stamina usage by 0.2");
        }

        public override void SetDefaults()
        {
            item.width = 20;
            item.height = 14;
            item.rare = ItemRarityID.Green;
        }

        public override void OnActivate()
        {
            Ability.time -= 2;
            Ability.CooldownBonus -= 30;
            Ability.ActivationCostBonus -= 0.2f;
            base.OnActivate();
        }
    }
}
