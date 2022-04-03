using Microsoft.Xna.Framework;
using StarlightRiver.Content.Abilities.Faeflame;
using StarlightRiver.Content.Abilities.ForbiddenWinds;
using StarlightRiver.Content.Abilities.GaiasFist;
using Terraria;

namespace StarlightRiver.Content.Abilities
{
	//This class serves to simplify implementing ability interactions
	internal static class AbilityHelper
    {
        public static bool CheckDash(Player Player, Rectangle hitbox)
        {
            if (!Player.active) return false;
            return Player.ActiveAbility<Dash>() && Collision.CheckAABBvAABBCollision(Player.Hitbox.TopLeft(), Player.Hitbox.Size(), hitbox.TopLeft(), hitbox.Size());
        }

        public static bool CheckWisp(Player Player, Rectangle hitbox)
        {
            if (!Player.active) return false;
            return Player.ActiveAbility<Whip>() && Collision.CheckAABBvAABBCollision(Player.Hitbox.TopLeft(), Player.Hitbox.Size(), hitbox.TopLeft(), hitbox.Size());
        }

        public static bool CheckSmash(Player Player, Rectangle hitbox)
        {
            if (!Player.active) return false;
            return Player.ActiveAbility<Smash>() && Collision.CheckAABBvAABBCollision(Player.Hitbox.TopLeft(), Player.Hitbox.Size(), hitbox.TopLeft(), hitbox.Size());
        }

        public static bool UsingAnyAbility(this Player Player) => Player.GetHandler().ActiveAbility != null;

        public static bool ActiveAbility<T>(this Player Player) where T : Ability => Player.GetHandler().ActiveAbility is T;

        public static AbilityHandler GetHandler(this Player Player) => Player.GetModPlayer<AbilityHandler>();
    }
}