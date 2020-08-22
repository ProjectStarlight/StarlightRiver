using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Abilities.Content.ForbiddenWinds
{
    class Astral : InfusionItem<Dash>
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Comet Rush I");
            Tooltip.SetDefault("Forbidden Winds Infusion\nDash farther and carry more speed\nIncreases stamina cost to 1.3");
        }

        public override void SetDefaults()
        {
            item.width = 20;
            item.height = 14;
            item.rare = ItemRarityID.Green;
        }

        public override void OnActivate()
        {
            Ability.Speed *= 0.6f;
            Ability.Boost = 0.5f;
            Ability.ActivationCostBonus += 0.3f;

            base.OnActivate();
            Main.PlaySound(SoundID.Item96, Player.Center);

            Ability.time = 10;
        }

        public override void UpdateActiveEffects()
        {
            Vector2 nextPos = Player.Center + Vector2.Normalize(Player.velocity) * Ability.Speed;
            for (float k = -2; k <= 2; k += 0.1f)
            {
                Vector2 pos = nextPos + Vector2.UnitX.RotatedBy(Player.velocity.ToRotation() + k) * 7 * (Dash.defaultTime - 3 - Ability.time);

                if (Ability.time == 0)
                {
                    //Vector2 pos2 = nextPos + Vector2.UnitX.RotatedBy(Ability.Player.velocity.ToRotation() + k) * 60;
                    //Dust.NewDustPerfect(pos2, DustType<Dusts.BlueStamina>(), Vector2.UnitY.RotatedBy(Ability.Player.velocity.ToRotation() + k + 1.57f) * Math.Abs(k), 0, default, 3 - Math.Abs(k));
                }
                Dust.NewDustPerfect(pos, DustType<Dusts.BlueStamina>(), Player.velocity * Main.rand.NextFloat(-0.4f, 0), 0, default, 1 - Ability.time / 10f);

                if (Math.Abs(k) >= 1.5f)
                    Dust.NewDustPerfect(pos, DustType<Dusts.BlueStamina>(), Player.velocity * Main.rand.NextFloat(-0.6f, -0.4f), 0, default, 2.2f - Ability.time / 10f);
            }
        }
    }

    class Astral2 : Astral
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Comet Rush II");
            Tooltip.SetDefault("Forbidden Winds Infusion\nDash farther and carry even more speed\nIncreases stamina cost to 1.6");
        }

        public override void OnActivate()
        {
            Ability.ActivationCostBonus += 0.3f;
            base.OnActivate();
            Ability.Speed *= 1.1f;
        }
    }

    class Astral3 : Astral
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Comet Rush III");
            Tooltip.SetDefault("Forbidden Winds Infusion\nDash farther and carry the most speed\nIncreases stamina cost to 2");
        }

        public override void OnActivate()
        {
            Ability.ActivationCostBonus += 0.7f;
            base.OnActivate();
            Ability.Speed *= 1.2f;
        }
    }
}
