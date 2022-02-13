using Microsoft.Xna.Framework;
using StarlightRiver.Content.Abilities.ForbiddenWinds;
using Terraria;

namespace StarlightRiver.Content.Abilities
{
	//This class serves to simplify implementing ability interactions
	internal static class AbilityHelper
    {
        public static bool CheckDash(Player player, Rectangle hitbox)
        {
            if (!player.active) return false;
            return player.ActiveAbility<Dash>() && Collision.CheckAABBvAABBCollision(player.Hitbox.TopLeft(), player.Hitbox.Size(), hitbox.TopLeft(), hitbox.Size());
        }

        public static bool UsingAnyAbility(this Player player) => player.GetHandler().ActiveAbility != null;

        public static bool ActiveAbility<T>(this Player player) where T : Ability => player.GetHandler().ActiveAbility is T;

        public static AbilityHandler GetHandler(this Player player) => player.GetModPlayer<AbilityHandler>();
    }
}