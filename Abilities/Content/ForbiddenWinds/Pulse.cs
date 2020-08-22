using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;

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
            Ability.Boost += 0.1f;
            base.OnActivate();
        }

        public override void UpdateActiveEffects()
        {
            Vector2 vel = Vector2.Normalize(Player.velocity) * -1;

            if (Ability.Time > 0)
                for (float k = 0; k < 6.28f; k += 0.02f)
                {
                    float factor = Ability.Time / 3f;
                    Vector2 pos = Player.Center + (new Vector2((float)Math.Cos(k) * 20, (float)Math.Sin(k) * 40) * factor).RotatedBy(Player.velocity.ToRotation());

                    Dust d = Dust.NewDustPerfect(pos, 264, vel * 10, 0, new Color(220, 20, 50), 0.5f);
                    d.noGravity = true;
                    d.noLight = true;
                }
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
                //Ability.Boost += 0.15f;
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
                //Ability.Boost += 0.15f;
                base.OnActivate();
            }
        }
    }
}
