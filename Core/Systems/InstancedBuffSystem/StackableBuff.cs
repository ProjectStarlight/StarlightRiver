using System;
using System.Collections.Generic;

namespace StarlightRiver.Core.Systems.InstancedBuffSystem
{
	internal abstract class StackableBuff<T> : InstancedBuff where T : BuffStack
	{
		public List<T> stacks;

		/// <summary>
		/// Used to define the effects that should occur per-stack for NPCs
		/// </summary>
		/// <param name="npc">The NPC to effect</param>
		/// <param name="stack">The given stack of this buff</param>
		public virtual void PerStackEffectsNPC(NPC npc, T stack) { }

		/// <summary>
		/// Used to define the effects that should occur per-stack for Players
		/// </summary>
		/// <param name="player">The player to effect</param>
		/// <param name="stack">The given stack of this buff</param>
		public virtual void PerStackEffectsPlayer(Player player, T stack) { }

		/// <summary>
		/// What should happen to an NPC inflicted with this buff if they have any stacks. Applied after stacks take effect
		/// </summary>
		/// <param name="npc">The NPC to effect</param>
		public virtual void AnyStacksUpdateNPC(NPC npc) { }

		public sealed override void UpdateNPC(NPC npc)
		{
			stacks.ForEach(n => PerStackEffectsNPC(npc, n));
			AnyStacksUpdateNPC(npc);
		}

		/// <summary>
		/// What should happen to a player inflicted with this buff if they have any stacks. Applied after stacks take effect
		/// </summary>
		/// <param name="player">The player to effect</param>
		public virtual void AnyStacksUpdatePlayer(Player player) { }

		public sealed override void UpdatePlayer(Player player)
		{
			stacks.ForEach(n => PerStackEffectsPlayer(player, n));
			AnyStacksUpdatePlayer(player);
		}

		/// <summary>
		/// Helper for iterating all stacks, with built in safety checks for not being inflicted. to be used with StarlightNPC events.
		/// </summary>
		/// <param name="npc">The NPC to iterate the stacks of</param>
		/// <param name="iterator">The action to invoke for every stack</param>
		public void IterateStacksNPC(NPC npc, Action<NPC, T> iterator)
		{
			var instance = GetInstance(npc) as StackableBuff<T>;

			if (instance is null)
				return;

			foreach (T stack in instance.stacks)
			{
				iterator(npc, stack);
			}
		}

		/// <summary>
		/// Helper for iterating all stacks, with built in safety checks for not being inflicted. to be used with StarlightPlayer events.
		/// </summary>
		/// <param name="player">The player to iterate the stacks of</param>
		/// <param name="iterator">The action to invoke for every stack</param>
		public void IterateStacksPlayer(Player player, Action<Player, T> iterator)
		{
			var instance = GetInstance(player) as StackableBuff<T>;

			if (instance is null)
				return;

			foreach (T stack in instance.stacks)
			{
				iterator(player, stack);
			}
		}

		/// <summary>
		/// Inflicts with a stack or adds a new stack of a given buff to an NPC
		/// </summary>
		/// <typeparam name="A">The buff to inflict</typeparam>
		/// <typeparam name="B">That buffs type of stacks</typeparam>
		/// <param name="npc">The NPC to inflict this buff on</param>
		/// <param name="duration">The duration of the stack to inflict</param>
		/// <param name="premadeStack">If you wish to inflict a specific stack</param>
		public static void InflictStack<A, B>(NPC npc, int duration, B premadeStack = null) where A : StackableBuff<B>, new() where B : BuffStack, new()
		{
			A instance = InstancedBuffNPC.GetInstance<A>(npc);

			if (instance is null)
			{
				Inflict<A>(npc, duration);
				instance = InstancedBuffNPC.GetInstance<A>(npc);
			}

			if (premadeStack is null)
			{
				premadeStack = new B
				{
					duration = duration
				};
			}

			instance.stacks.Add(premadeStack);

			int index = npc.FindBuffIndex(instance.backingType);
			npc.buffTime[index] = instance.GetDuration();
		}

		/// <summary>
		/// Inflicts with a stack or adds a new stack of a given buff to a player
		/// </summary>
		/// <typeparam name="A">The buff to inflict</typeparam>
		/// <typeparam name="B">That buffs type of stacks</typeparam>
		/// <param name="player">The player to inflict this buff on</param>
		/// <param name="duration">The duration of the stack to inflict</param>
		/// <param name="premadeStack">If you wish to inflict a specific stack</param>
		public static void InflictStack<A, B>(Player player, int duration, B premadeStack = null) where A : StackableBuff<B>, new() where B : BuffStack, new()
		{
			A instance = InstancedBuffPlayer.GetInstance<A>(player);

			if (instance is null)
			{
				Inflict<A>(player, duration);
				instance = InstancedBuffPlayer.GetInstance<A>(player);
			}

			if (premadeStack is null)
			{
				premadeStack = new B
				{
					duration = duration
				};
			}

			instance.stacks.Add(premadeStack);

			int index = player.FindBuffIndex(instance.backingType);
			player.buffTime[index] = instance.GetDuration();
		}

		/// <summary>
		/// Gets the total duration of this stackable buff instance based on the longest stack in it's stacks.
		/// </summary>
		/// <returns></returns>
		public int GetDuration()
		{
			int duration = 0;

			foreach (T stack in stacks)
			{
				if (stack.duration > duration)
					duration = stack.duration;
			}

			return duration;
		}
	}

	internal class BuffStack
	{
		public int duration;
	}
}
