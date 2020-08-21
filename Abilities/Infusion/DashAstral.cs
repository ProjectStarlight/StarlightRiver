using StarlightRiver.Abilities.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;

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
            base.OnActivate();
       
            var b = 0.15f; // boost velocity when exiting dash
            Ability.vel = Ability.SignedLesserBound(Ability.GetDashBoost() * b, Ability.Player.velocity); // "conservation of momentum" (lol)
            Ability.time = 10;
            Ability.Speed = 28;

            Main.PlaySound(SoundID.Item45, Ability.Player.Center);
            Main.PlaySound(SoundID.Item104, Ability.Player.Center);
        }

        public override void UpdateActiveEffects()
        {
            Main.NewText("SHIT!");
        }
    }
}
