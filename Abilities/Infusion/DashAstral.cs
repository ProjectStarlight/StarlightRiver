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
            Ability.time = Dash.maxTime + 5;
            Ability.Speed *= 1.5f;
            Ability.Boost = 0.25f;

            base.OnActivate();
        }

        public override void UpdateActiveEffects()
        {
            Vector2 prevPos = Ability.Player.Center + Vector2.Normalize(Ability.Player.velocity) * 10;
            int direction = Ability.time % 2 == 0 ? -1 : 1;

            for (int k = 0; k < 60; k++)
            {
                Vector2 off = Vector2.Normalize(Ability.Player.velocity).RotatedBy(k % 2 == 0 ? 0.2f : -0.2f) * -k * (1 - Ability.time / 10f);
                Dust dus = Dust.NewDustPerfect(prevPos + off, DustType<Dusts.Starlight>(), off, 0, default, 1 - Ability.time / 10f);
                dus.fadeIn = k - Ability.time * 3;
            }
        }
    }
}
