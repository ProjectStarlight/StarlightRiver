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
		/// Inflicts an instanced buff on a player
		/// </summary>
		/// <typeparam name="T">The type of instanced buff to inflict</typeparam>
		/// <param name="player">The player to inflict it on</param>
		public static void Inflict<T>(Player player, int duration, T premadeInstance = null) where T : InstancedBuff, new()
		{
			if (premadeInstance is null)
				premadeInstance = new T();

			player.GetModPlayer<InstancedBuffPlayer>().buffInstances.Add(premadeInstance);
			player.AddBuff(premadeInstance.backingType, duration);
		}

		/// <summary>
		/// Updates all instanced buffs in the standard update loop
		/// </summary>
		public override void PreUpdateBuffs()
		{
			buffInstances.ForEach(n => n.UpdatePlayer(Player));
		}
	}
}
