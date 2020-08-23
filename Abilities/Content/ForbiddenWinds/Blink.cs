using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Abilities.Content.ForbiddenWinds
{
    public class Blink : InfusionItem<Dash>
    {
        public override string Texture => ModContent.GetInstance<Astral>().Texture;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Blink");
            Tooltip.SetDefault("Forbidden Winds Infusion\nDash is replaced by a short-range teleport");
        }

        public override void SetDefaults()
        {
            item.width = 20;
            item.height = 14;
            item.rare = ItemRarityID.Green;
        }

        public override void OnActivate()
        {
            Ability.Time = 3;
        }

        public override void UpdateActive()
        {
            const int tpTime = 2;

            if (Ability.Time == tpTime)
            {
                Teleport();
            }

            if (Ability.Time <= 0)
                Ability.Deactivate();
        }

        private void Teleport()
        {
            Vector2 travelVector = Ability.Dir * Ability.Speed * Ability.Time;
            Vector2 collisionVector = Collision.AnyCollision(Player.position, travelVector, Player.width, Player.height);
            Player.oldPosition = Player.position;
            Player.position += collisionVector;

            if (Main.netMode != NetmodeID.Server)
            {
                TeleportFx(Player.oldPosition, Player.position);
            }
        }

        private void TeleportFx(Vector2 oldPosition, Vector2 newPosition)
        {
            // Animation plays at oldPosition and newPosition? Idk go wild.
        }

        public override void OnExit()
        {
            // DO NOT add fall damage reset!
            // This ability does nothing to your velocity. It shouldn't.
        }

        public override void UpdateActiveEffects()
        {
            // No visuals from the original.
        }
    }
}
