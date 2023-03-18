using System.Linq;

namespace StarlightRiver.Core.Systems.InstancedBuffSystem
{
	internal static class BuffInflictor
	{
		private static void InflictInner<T>(Player player, int duration, T premadeInstance = null) where T : InstancedBuff, new()
		{
			InstancedBuffPlayer mp = player.GetModPlayer<InstancedBuffPlayer>();

			if (premadeInstance is null) //Dont want to override the existing instance
			{
				if (mp.buffInstances.Any(n => n is T)) //If an instance already exists, let the backer get re-inflicted and stop
				{
					player.AddBuff(premadeInstance.BackingType, duration);
					return;
				}

				premadeInstance = new T(); //Else make a new instance and continue on
			}

			if (mp.buffInstances.Any(n => n is T)) //Remove any old instances if this is an override
				mp.buffInstances.RemoveAll(n => n is T);

			mp.buffInstances.Add(premadeInstance); //Add the new instance
			player.AddBuff(premadeInstance.BackingType, duration); //Inflict the backer
		}

		private static void InflictInner<T>(NPC npc, int duration, T premadeInstance = null) where T : InstancedBuff, new()
		{
			InstancedBuffNPC gn = npc.GetGlobalNPC<InstancedBuffNPC>();

			if (premadeInstance is null) //Dont want to override the existing instance
			{
				if (gn.buffInstances.Any(n => n is T)) //If an instance already exists, let the backer get re-inflicted and stop
				{
					npc.AddBuff(premadeInstance.BackingType, duration);
					return;
				}

				premadeInstance = new T(); //Else make a new instance and continue on
			}

			if (gn.buffInstances.Any(n => n is T)) //Remove any old instances if this is an override
				gn.buffInstances.RemoveAll(n => n is T);

			gn.buffInstances.Add(premadeInstance); //Add the new instance
			npc.AddBuff(premadeInstance.BackingType, duration); //Inflict the backer
		}

		private static void InflictStackInner<T>(NPC npc, int duration, BuffStack premadeStack) where T : InstancedBuff, new()
		{
			var instance = InstancedBuffNPC.GetInstance<T>(npc) as StackableBuff; //If possible, get the existing instancedBuff instance for this stackable buff

			if (instance is null)
			{
				InflictInner<T>(npc, duration); //If not found, inflict it and get it
				instance = InstancedBuffNPC.GetInstance<T>(npc) as StackableBuff;
			}

			if (premadeStack is null) //If we want to add a default stack
				premadeStack = instance.GenerateDefaultStack(duration);

			instance.stacks.Add(premadeStack); //Add our new stack

			int index = npc.FindBuffIndex(instance.BackingType); //Find the backer and update it's time to reflect that of the longest stack
			npc.buffTime[index] = instance.GetDuration();
		}

		private static void InflictStackInner<T>(Player player, int duration, BuffStack premadeStack) where T : InstancedBuff, new()
		{
			var instance = InstancedBuffPlayer.GetInstance<T>(player) as StackableBuff; //If possible, get the existing instancedBuff instance for this stackable buff

			if (instance is null)
			{
				InflictInner<T>(player, duration); //If not found, inflict it and get it
				instance = InstancedBuffPlayer.GetInstance<T>(player) as StackableBuff;
			}

			if (premadeStack is null) //If we want to add a default stack
				premadeStack = instance.GenerateDefaultStack(duration);

			instance.stacks.Add(premadeStack); //Add our new stack

			int index = player.FindBuffIndex(instance.BackingType); //Find the backer and update it's time to reflect that of the longest stack
			player.buffTime[index] = instance.GetDuration();
		}

		/// <summary>
		/// Inflicts an instanced buff on a player, or replaces their istanced buff instance.
		/// </summary>
		/// <typeparam name="T">The type of instanced buff to inflict</typeparam>
		/// <param name="player">The player to inflict it on</param>
		/// <param name="duration">The duration of the buff</param>
		/// <param name="premadeInstance">An optional pre-made instance to add or override the existing instance with. If this is left as null the instance wont be overridden if it already exists.</param>
		public static void Inflict<T>(Player player, int duration, T premadeInstance = null) where T : InstancedBuff, new()
		{
			if (typeof(T).IsSubclassOf(typeof(StackableBuff)))
				InflictStackInner<T>(player, duration, null);
			else
				InflictInner<T>(player, duration, premadeInstance);
		}

		/// <summary>
		/// Inflicts an instanced buff on an NPC, or replaces their istanced buff instance.
		/// </summary>
		/// <typeparam name="T">The type of instanced buff to inflict</typeparam>
		/// <param name="npc">The NPC to inflict it on</param>
		/// <param name="duration">The duration of the buff</param>
		/// <param name="premadeInstance">An optional pre-made instance to add or override the existing instance with. If this is left as null the instance wont be overridden if it already exists.</param>
		public static void Inflict<T>(NPC npc, int duration, T premadeInstance = null) where T : InstancedBuff, new()
		{
			if (typeof(T).IsSubclassOf(typeof(StackableBuff)))
				InflictStackInner<T>(npc, duration, null);
			else
				InflictInner<T>(npc, duration, premadeInstance);
		}

		/// <summary>
		/// Inflicts with a stack or adds a new stack of a given buff to an NPC
		/// </summary>
		/// <typeparam name="A">The buff to inflict</typeparam>
		/// <typeparam name="B">That buffs type of stacks</typeparam>
		/// <param name="npc">The NPC to inflict this buff on</param>
		/// <param name="duration">The duration of the stack to inflict</param>
		/// <param name="premadeStack">If you wish to inflict a specific stack</param>
		public static void InflictStack<A, B>(NPC npc, int duration, B premadeStack = null) where A : StackableBuff<B>, new() where B : BuffStack
		{
			InflictStackInner<A>(npc, duration, premadeStack);
		}

		/// <summary>
		/// Inflicts with a stack or adds a new stack of a given buff to a player
		/// </summary>
		/// <typeparam name="A">The buff to inflict</typeparam>
		/// <typeparam name="B">That buffs type of stacks</typeparam>
		/// <param name="player">The player to inflict this buff on</param>
		/// <param name="duration">The duration of the stack to inflict</param>
		/// <param name="premadeStack">If you wish to inflict a specific stack</param>
		public static void InflictStack<A, B>(Player player, int duration, B premadeStack = null) where A : StackableBuff<B>, new() where B : BuffStack
		{
			InflictStackInner<A>(player, duration, premadeStack);
		}
	}
}
