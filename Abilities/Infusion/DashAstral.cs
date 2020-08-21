using Microsoft.Xna.Framework;
using StarlightRiver.Abilities.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

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
            Vector2 prevPos = Ability.Player.Center + Vector2.Normalize(Ability.Player.velocity) * 10;
            int direction = Ability.time % 2 == 0 ? -1 : 1;

            for (int k = 0; k < 60; k++)
            {
                float rot = (0.1f * k) * direction;
                Dust dus = Dust.NewDustPerfect(prevPos + Vector2.Normalize(Ability.Player.velocity).RotatedBy(rot) * (k / 2) * (0.5f + Ability.time / 8f), DustType<Dusts.Starlight>());
                dus.fadeIn = k - Ability.time * 3;
            }
        }
    }
}
