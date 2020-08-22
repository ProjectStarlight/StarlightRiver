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
            Vector2 vel = Vector2.Normalize(Player.velocity) * -1;

            if (Ability.Time > 0)
                for (float k = 0; k < 6.28f; k += 0.02f)
                {
                    float factor = Ability.Time / 10f;
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
