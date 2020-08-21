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
            Tooltip.SetDefault("Forbidden Winds Infusion\nDash farther and carry more speed");
        }

        public override void SetDefaults()
        {
            item.width = 20;
            item.height = 14;
            item.rare = ItemRarityID.Green;
        }

        public override void OnActivate()
        {
            Ability.Speed *= 1.5f;
            Ability.Boost = 0.25f;

            base.OnActivate();
        }

        public override void UpdateActiveEffects()
        {
            Vector2 nextPos = Ability.Player.Center + Vector2.Normalize(Ability.Player.velocity) * Ability.Speed;
            for(float k = -2; k <= 2; k += 0.1f)
            {
                Vector2 pos = nextPos + Vector2.UnitX.RotatedBy(Ability.Player.velocity.ToRotation() + k) * 7 * (Dash.defaultTime - Ability.time);

                if (Ability.time == 0)
                {
                    //Vector2 pos2 = nextPos + Vector2.UnitX.RotatedBy(Ability.Player.velocity.ToRotation() + k) * 60;
                    //Dust.NewDustPerfect(pos2, DustType<Dusts.BlueStamina>(), Vector2.UnitY.RotatedBy(Ability.Player.velocity.ToRotation() + k + 1.57f) * Math.Abs(k), 0, default, 3 - Math.Abs(k));
                }
                Dust.NewDustPerfect(pos, DustType<Dusts.BlueStamina>(), Ability.Player.velocity * Main.rand.NextFloat(-0.4f, 0), 0, default, 1 - Ability.time / 10f);

                if(Math.Abs(k) >= 1.5f)
                {
                    Dust.NewDustPerfect(pos, DustType<Dusts.BlueStamina>(), Ability.Player.velocity * Main.rand.NextFloat(-0.6f, -0.4f), 0, default, 2.2f - Ability.time / 10f);
                }
            }
        }
    }
}
