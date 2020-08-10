using Microsoft.Xna.Framework;
using Terraria;

namespace StarlightRiver.Abilities
{
    //This class serves to simplify implementing ability interactions
    internal static class AbilityHelper
    {
        public static bool CheckDash(Player player, Rectangle hitbox)
        {
            return Collision.CheckAABBvAABBCollision(player.Hitbox.TopLeft(), player.Hitbox.Size(), hitbox.TopLeft(), hitbox.Size()) && player.GetModPlayer<AbilityHandler>().dash.Active;
        }

        public static bool CheckWisp(Player player, Rectangle hitbox)
        {
            return Collision.CheckAABBvAABBCollision(player.Hitbox.TopLeft(), player.Hitbox.Size(), hitbox.TopLeft(), hitbox.Size()) && player.GetModPlayer<AbilityHandler>().wisp.Active;
        }

        public static bool CheckSmash(Player player, Rectangle hitbox)
        {
            return Collision.CheckAABBvAABBCollision(player.Hitbox.TopLeft(), player.Hitbox.Size(), hitbox.TopLeft(), hitbox.Size()) && player.GetModPlayer<AbilityHandler>().smash.Active;
        }
    }
}