using System.Collections.Generic;
using System.Linq;

namespace StarlightRiver.Core.Systems.InstancedBuffSystem
{
	internal class InstancedBuffPlayer : ModPlayer
	{
		/// <summary>
		/// The instanced buffs that exist on this NPC
		/// </summary>
		public readonly List<InstancedBuff> buffInstances = new();

		/// <summary>
		/// Gets the instance of a given instanced buff inflicted on a player
		/// </summary>
		/// <typeparam name="T">The type of the instanced buff to get</typeparam>
		/// <param name="player">The player to check for this buff on</param>
		/// <returns>The inflicted instance, or null if one does not exist</returns>
		public static T? GetInstance<T>(Player player) where T : InstancedBuff
		{
			return player.GetModPlayer<InstancedBuffPlayer>().buffInstances.FirstOrDefault(n => n is T) as T;
		}

		/// <summary>
		/// Gets the instanced buff with the given internal name on a player
		/// </summary>
		/// <param name="player">the player to check for this buff on</param>
		/// <param name="name">the internal name of the buff to check for</param>
		/// <returns>The inflicted instance, or null if one does not exist</returns>
		public static InstancedBuff? GetInstance(Player player, string name)
		{
			return player.GetModPlayer<InstancedBuffPlayer>().buffInstances.FirstOrDefault(n => n.Name == name);
		}

		/// <summary>
		/// Updates all instanced buffs in the standard update loop
		/// </summary>
		public override void PreUpdateBuffs()
		{
			buffInstances.ForEach(n => n.UpdatePlayer(Player));
		}

		/// <summary>
		/// Handles removing all expired buffs
		/// </summary>
		public override void PostUpdateBuffs()
		{
			buffInstances.RemoveAll(n => !Player.HasBuff(n.BackingType));
		}
	}
}