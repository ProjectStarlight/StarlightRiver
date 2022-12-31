using StarlightRiver.Content.Abilities.Faewhip;
using StarlightRiver.Content.Abilities.ForbiddenWinds;

namespace StarlightRiver.Content.Abilities
{
	//This class serves to simplify implementing ability interactions
	internal static class AbilityHelper
	{
		public static bool CheckDash(Player Player, Rectangle hitbox)
		{
			if (!Player.active)
				return false;
			return Player.ActiveAbility<Dash>() && Collision.CheckAABBvAABBCollision(Player.Hitbox.TopLeft(), Player.Hitbox.Size(), hitbox.TopLeft(), hitbox.Size());
		}

		public static bool CheckWisp(Player Player, Rectangle hitbox)
		{
			if (!Player.active)
				return false;
			return Player.ActiveAbility<Whip>() && Collision.CheckAABBvAABBCollision(Player.Hitbox.TopLeft(), Player.Hitbox.Size(), hitbox.TopLeft(), hitbox.Size());
		}

		public static bool UsingAnyAbility(this Player Player)
		{
			return Player.GetHandler().ActiveAbility != null;
		}

		public static bool ActiveAbility<T>(this Player Player) where T : Ability
		{
			return Player.GetHandler().ActiveAbility is T;
		}

		public static AbilityHandler GetHandler(this Player Player)
		{
			return Player.GetModPlayer<AbilityHandler>();
		}
	}
}