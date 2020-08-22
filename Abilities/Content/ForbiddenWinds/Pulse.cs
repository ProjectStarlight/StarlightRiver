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
        protected float sizeMult;

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
            Ability.Time = 4;
            Ability.Speed -= 3;
            Ability.CooldownBonus -= 10;
            Ability.ActivationCostBonus -= 0.15f;
            Ability.Boost += 0.2f;
            base.OnActivate();
            sizeMult = 0.33f;
        }

        public override void UpdateActiveEffects()
        {
            if (Ability.Time > 0)
                return;

            Vector2 vel = Vector2.Normalize(Player.velocity) * -1;

            float maxSize = 1.4f;
            float numCircles = 10 * sizeMult;

            for (var size = maxSize; size > 0; size -= maxSize / numCircles)
                for (float k = 0; k < 6.28f; k += 0.02f)
                {
                    float ovalScale = size / (1 + Ability.Time) * sizeMult;
                    float offset = (size / maxSize) * 30 + 10;
                    Vector2 pos = Player.Center + vel * offset + (new Vector2((float)Math.Cos(k) * 20, (float)Math.Sin(k) * 40) * ovalScale).RotatedBy(Player.velocity.ToRotation());

                    Dust d = Dust.NewDustPerfect(pos, 264, vel * (size / maxSize) * 10, 0, new Color(220, 20, 50), 0.7f);
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
                Ability.Time -= 2;
                Ability.CooldownBonus -= 10;
                Ability.ActivationCostBonus -= 0.1f;
                Ability.Boost += 0.15f;
                base.OnActivate();
                sizeMult = 0.5f;
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
                sizeMult = 0.66f;
            }
        }
    }
}
