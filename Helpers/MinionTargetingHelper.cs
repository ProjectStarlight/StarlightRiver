using System.Collections.Generic;

namespace StarlightRiver.Helpers
{
	internal class MinionTargetingHelper
	{
		/// <summary>
		/// This retrieves a list of all valid targets for a minion, taking into account
		/// line of sight if appropriate and a maximum targeting radius.
		/// </summary>
		/// <param name="minion">The minion to find targets for</param>
		/// <param name="radius">The max range to check for targets in</param>
		/// <param name="los">If line of sight should be taken into account</param>
		/// <param name="originPlayer">If the radius should be checked against the projectile or it's owner</param>
		/// <returns></returns>
		public static List<NPC> FindTargets(Projectile minion, int radius, bool los, bool originPlayer)
		{
			List<NPC> targets = new();

			Player owner = Main.player[minion.owner];
			Vector2 origin = originPlayer ? owner.Center : minion.Center;

			// Check player's target first, and prioritize that if possible
			if (owner?.HasMinionAttackTargetNPC ?? false)
			{
				NPC npc = Main.npc[owner.MinionAttackTargetNPC];

				if (Vector2.Distance(origin, npc.Center) <= radius && Collision.CanHit(minion.Center, 1, 1, npc.Center, 1, 1))
				{
					targets.Add(npc);
					return targets;
				}
			}

			// Else, scan for other valid NPCs
			foreach (NPC npc in Main.npc)
			{
				if (!npc.active || npc.friendly || !npc.CanBeChasedBy(minion))
					continue;

				if (Vector2.Distance(origin, npc.Center) <= radius && (!los || Collision.CanHit(minion.Center, 1, 1, npc.Center, 1, 1)))
					targets.Add(npc);
			}

			return targets;
		}

		/// <summary>
		/// Finds the closest viable target for a given minion/sentry, taking into account a max radius and los
		/// </summary>
		/// <param name="minion">The minion to find the target for</param>
		/// <param name="radius">The maximum search distance for targets</param>
		/// <param name="los">If the search should obey line of sight</param>
		/// <param name="originPlayer">If the radius should be checked against the projectile or it's owner</param>
		/// <returns>The NPC to target</returns>
		public static NPC FindClosestTarget(Projectile minion, int radius, bool los, bool originPlayer)
		{
			List<NPC> possibles = FindTargets(minion, radius, los, originPlayer);

			if (possibles.Count == 0)
				return null;

			possibles.Sort((a, b) => Vector2.Distance(a.Center, minion.Center) > Vector2.Distance(b.Center, minion.Center) ? 1 : -1);

			return possibles[0];
		}

		/// <summary>
		/// Finds the farthest viable target for a given minion/sentry, taking into account a max radius and los
		/// </summary>
		/// <param name="minion">The minion to find the target for</param>
		/// <param name="radius">The maximum search distance for targets</param>
		/// <param name="los">If the search should obey line of sight</param>
		/// <param name="originPlayer">If the radius should be checked against the projectile or it's owner</param>
		/// <returns>The NPC to target</returns>
		public static NPC FindFarthestTarget(Projectile minion, int radius, bool los, bool originPlayer)
		{
			List<NPC> possibles = FindTargets(minion, radius, los, originPlayer);

			if (possibles.Count == 0)
				return null;

			possibles.Sort((a, b) => Vector2.Distance(a.Center, minion.Center) < Vector2.Distance(b.Center, minion.Center) ? 1 : -1);

			return possibles[0];
		}

		/// <summary>
		/// Finds the strongest (highest max HP) viable target for a given minion/sentry, taking into account a max radius and los
		/// </summary>
		/// <param name="minion">The minion to find the target for</param>
		/// <param name="radius">The maximum search distance for targets</param>
		/// <param name="los">If the search should obey line of sight</param>
		/// <param name="originPlayer">If the radius should be checked against the projectile or it's owner</param>
		/// <returns>The NPC to target</returns>
		public static NPC FindStrongestTarget(Projectile minion, int radius, bool los, bool originPlayer)
		{
			List<NPC> possibles = FindTargets(minion, radius, los, originPlayer);

			if (possibles.Count == 0)
				return null;

			possibles.Sort((a, b) => a.lifeMax > b.lifeMax ? 1 : -1);

			return possibles[0];
		}

		/// <summary>
		/// Finds the weakest (lowest max HP) viable target for a given minion/sentry, taking into account a max radius and los
		/// </summary>
		/// <param name="minion">The minion to find the target for</param>
		/// <param name="radius">The maximum search distance for targets</param>
		/// <param name="los">If the search should obey line of sight</param>
		/// <param name="originPlayer">If the radius should be checked against the projectile or it's owner</param>
		/// <returns>The NPC to target</returns>
		public static NPC FindWeakestTarget(Projectile minion, int radius, bool los, bool originPlayer)
		{
			List<NPC> possibles = FindTargets(minion, radius, los, originPlayer);

			if (possibles.Count == 0)
				return null;

			possibles.Sort((a, b) => a.lifeMax < b.lifeMax ? 1 : -1);

			return possibles[0];
		}

		/// <summary>
		/// Finds a random valid target for a given minion/sentry, taking into account a max radius and los.
		/// THIS WILL NOT BE SYNCED! Be sure to sync it yourself!
		/// </summary>
		/// <param name="minion">The minion to find the target for</param>
		/// <param name="radius">The maximum search distance for targets</param>
		/// <param name="los">If the search should obey line of sight</param>
		/// <param name="originPlayer">If the radius should be checked against the projectile or it's owner</param>
		/// <returns>The NPC to target</returns>
		public static NPC FindRandomTarget(Projectile minion, int radius, bool los, bool originPlayer)
		{
			List<NPC> possibles = FindTargets(minion, radius, los, originPlayer);

			if (possibles.Count == 0)
				return null;

			Helper.RandomizeList(possibles);

			return possibles[0];
		}
	}

	/// <summary>
	/// Extension methods to ease the usage of targeting helpers
	/// </summary>
	internal static class MinionTargetingExtensions
	{
		/// <summary>
		/// Finds the closest viable target for this minion/sentry, taking into account a max radius and los
		/// </summary>
		/// <param name="projectile">The minion to find the target for</param>
		/// <param name="radius">The maximum search distance for targets</param>
		/// <param name="los">If the search should obey line of sight</param>
		/// <param name="originPlayer">If the radius should be checked against the projectile or it's owner</param>
		/// <returns>The NPC to target</returns>
		public static NPC TargetClosestNPC(this Projectile projectile, int radius, bool los, bool originPlayer)
		{
			return MinionTargetingHelper.FindClosestTarget(projectile, radius, los, originPlayer);
		}

		/// <summary>
		/// Finds the farthest viable target for this minion/sentry, taking into account a max radius and los
		/// </summary>
		/// <param name="projectile">The minion to find the target for</param>
		/// <param name="radius">The maximum search distance for targets</param>
		/// <param name="los">If the search should obey line of sight</param>
		/// <param name="originPlayer">If the radius should be checked against the projectile or it's owner</param>
		/// <returns>The NPC to target</returns>
		public static NPC TargetFarthestNPC(this Projectile projectile, int radius, bool los, bool originPlayer)
		{
			return MinionTargetingHelper.FindFarthestTarget(projectile, radius, los, originPlayer);
		}

		/// <summary>
		/// Finds the strongest (highest max HP) viable target for this minion/sentry, taking into account a max radius and los
		/// </summary>
		/// <param name="projectile">The minion to find the target for</param>
		/// <param name="radius">The maximum search distance for targets</param>
		/// <param name="los">If the search should obey line of sight</param>
		/// <param name="originPlayer">If the radius should be checked against the projectile or it's owner</param>
		/// <returns>The NPC to target</returns>
		public static NPC TargetStrongestNPC(this Projectile projectile, int radius, bool los, bool originPlayer)
		{
			return MinionTargetingHelper.FindStrongestTarget(projectile, radius, los, originPlayer);
		}

		/// <summary>
		/// Finds the weakest (lowest max HP) viable target for this minion/sentry, taking into account a max radius and los
		/// </summary>
		/// <param name="projectile">The minion to find the target for</param>
		/// <param name="radius">The maximum search distance for targets</param>
		/// <param name="los">If the search should obey line of sight</param>
		/// <param name="originPlayer">If the radius should be checked against the projectile or it's owner</param>
		/// <returns>The NPC to target</returns>
		public static NPC TargetWeakestNPC(this Projectile projectile, int radius, bool los, bool originPlayer)
		{
			return MinionTargetingHelper.FindWeakestTarget(projectile, radius, los, originPlayer);
		}

		/// <summary>
		/// Finds a random valid target for this minion/sentry, taking into account a max radius and los.
		/// THIS WILL NOT BE SYNCED! Be sure to sync it yourself!
		/// </summary>
		/// <param name="projectile">The minion to find the target for</param>
		/// <param name="radius">The maximum search distance for targets</param>
		/// <param name="los">If the search should obey line of sight</param>
		/// <param name="originPlayer">If the radius should be checked against the projectile or it's owner</param>
		/// <returns>The NPC to target</returns>
		public static NPC TargetRandomNPC(this Projectile projectile, int radius, bool los, bool originPlayer)
		{
			return MinionTargetingHelper.FindRandomTarget(projectile, radius, los, originPlayer);
		}

		/// <summary>
		/// Checks if a target is still valid, taking a max radius and los into account
		/// </summary>
		/// <param name="projectile"></param>
		/// <param name="target"></param>
		/// <param name="los"></param>
		/// <param name="target"></param>
		/// <param name="originPlayer">If the radius should be checked against the projectile or it's owner</param>
		/// <returns>If the target is alive and within range</returns>
		public static bool TargetValid(this Projectile projectile, int radius, bool los, bool originPlayer, NPC target)
		{
			if (target is null || !target.active || target.friendly || !target.CanBeChasedBy(projectile))
				return false;

			Vector2 origin = originPlayer ? Main.player[projectile.owner].Center : projectile.Center;

			if (Vector2.Distance(origin, target.Center) > radius)
				return false;

			if (los && !Collision.CanHit(projectile.Center, 1, 1, target.Center, 1, 1))
				return false;

			return true;
		}
	}
}