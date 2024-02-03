using System;
using System.Collections.Generic;

namespace StarlightRiver.Core.Systems.InstancedBuffSystem
{
	/// <summary>
	/// This class is to be used for buffs which require handling stacks with seperate data, such as seperate durations or magnitudes.
	/// To inflict an instanced buff, call BuffInflictor.Inflict or BuffInflictor.InflictStack.
	/// </summary>
	internal abstract class StackableBuff : InstancedBuff
	{
		/// <summary>
		/// A list of instances of stacks for this buff
		/// </summary>
		public List<BuffStack> stacks = new();

		public virtual void PerStackEffectsNPC(NPC npc, BuffStack stack) { }

		public virtual void PerStackEffectsPlayer(Player player, BuffStack stack) { }

		/// <summary>
		/// What should happen to an NPC inflicted with this buff if they have any stacks. Applied after stacks take effect
		/// </summary>
		/// <param name="npc">The NPC to effect</param>
		public virtual void AnyStacksUpdateNPC(NPC npc) { }

		public sealed override void UpdateNPC(NPC npc)
		{
			stacks.ForEach(n => PerStackEffectsNPC(npc, n)); //Apply the effects of all stacks

			stacks.ForEach(n => n.duration--); //Handle stack durations automatically
			stacks.RemoveAll(n => n.duration <= 0);

			AnyStacksUpdateNPC(npc);
		}

		/// <summary>
		/// What should happen to a player inflicted with this buff if they have any stacks. Applied after stacks take effect
		/// </summary>
		/// <param name="player">The player to effect</param>
		public virtual void AnyStacksUpdatePlayer(Player player) { }

		public sealed override void UpdatePlayer(Player player)
		{
			stacks.ForEach(n => PerStackEffectsPlayer(player, n)); //Apply the effects of all stacks

			stacks.ForEach(n => n.duration--); //Handle stack durations automatically
			stacks.RemoveAll(n => n.duration <= 0);

			AnyStacksUpdatePlayer(player);
		}

		/// <summary>
		/// Helper for iterating all stacks, with built in safety checks for not being inflicted. to be used with StarlightNPC events.
		/// </summary>
		/// <param name="npc">The NPC to iterate the stacks of</param>
		/// <param name="iterator">The action to invoke for every stack</param>
		public void IterateStacksNPC(NPC npc, Action<NPC, BuffStack> iterator)
		{
			var instance = GetInstance(npc) as StackableBuff<BuffStack>;

			if (instance is null)
				return;

			foreach (BuffStack stack in instance.stacks)
			{
				iterator(npc, stack);
			}
		}

		/// <summary>
		/// Helper for iterating all stacks, with built in safety checks for not being inflicted. to be used with StarlightPlayer events.
		/// </summary>
		/// <param name="player">The player to iterate the stacks of</param>
		/// <param name="iterator">The action to invoke for every stack</param>
		public void IterateStacksPlayer(Player player, Action<Player, BuffStack> iterator)
		{
			var instance = GetInstance(player) as StackableBuff<BuffStack>;

			if (instance is null)
				return;

			foreach (BuffStack stack in instance.stacks)
			{
				iterator(player, stack);
			}
		}

		/// <summary>
		/// Gets the total duration of this stackable buff instance based on the longest stack in it's stacks.
		/// </summary>
		/// <returns></returns>
		public int GetDuration()
		{
			int duration = 0;

			foreach (BuffStack stack in stacks)
			{
				if (stack.duration > duration)
					duration = stack.duration;
			}

			return duration;
		}

		public abstract BuffStack GenerateDefaultStack(int duration);
	}

	/// <summary>
	/// This class is to be used for buffs which require handling stacks with seperate data, such as seperate durations or magnitudes. Use this when you need a stack type other than BuffStack.
	/// To inflict an instanced buff, call BuffInflictor.Inflict or BuffInflictor.InflictStack.
	/// </summary>
	/// <typeparam name="T">The type of stack this buff uses. The default is the parent class BuffStack, which contains only a duration. You can create custom stack types by extending this class.</typeparam>
	internal abstract class StackableBuff<T> : StackableBuff where T : BuffStack
	{
		/// <summary>
		/// Used to define the effects that should occur per-stack for NPCs
		/// </summary>
		/// <param name="npc">The NPC to effect</param>
		/// <param name="stack">The given stack of this buff</param>
		public virtual void PerStackEffectsNPC(NPC npc, T stack) { }

		public sealed override void PerStackEffectsNPC(NPC npc, BuffStack stack)
		{
			PerStackEffectsNPC(npc, (T)stack);
		}

		/// <summary>
		/// Used to define the effects that should occur per-stack for Players
		/// </summary>
		/// <param name="player">The player to effect</param>
		/// <param name="stack">The given stack of this buff</param>
		public virtual void PerStackEffectsPlayer(Player player, T stack) { }

		public sealed override void PerStackEffectsPlayer(Player player, BuffStack stack)
		{
			PerStackEffectsPlayer(player, (T)stack);
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
		/// The default stack to generate for this stackable buff, for when its inflicted without a stack instance passed to it.
		/// </summary>
		/// <returns>The default stack to add to the stacks</returns>
		public abstract T GenerateDefaultStackTyped(int duration);

		public sealed override BuffStack GenerateDefaultStack(int duration)
		{
			return GenerateDefaultStackTyped(duration);
		}
	}

	/// <summary>
	/// A class to represent a stack of a stackable buff. These generally exist soely to hold data and are classes only because struct inheritence dosent play nice with generics.
	/// </summary>
	public class BuffStack
	{
		public int duration;
	}
}