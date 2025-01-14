﻿using System.Linq;
using Terraria;
using Terraria.ID;

namespace StarlightRiver.Core.Systems.InstancedBuffSystem
{
	internal static class BuffInflictor
	{
		public static void InflictFromNet(Player player, int duration, string type)
		{
			Main.NewText($"InflictFromNet: {duration}, {type}");
			InstancedBuffPlayer mp = player.GetModPlayer<InstancedBuffPlayer>();

			if (InstancedBuff.TryGetPrototype(type, out InstancedBuff proto))
			{
				if (mp.buffInstances.Any(n => n.Name == proto.Name))
					mp.buffInstances.RemoveAll(n => n.Name == proto.Name);

				mp.buffInstances.Add(proto.Clone());
				player.AddBuff(proto.BackingType, duration);
			}
		}

		public static void InflictFromNet(NPC npc, int duration, string type)
		{
			Main.NewText($"InflictFromNet: {duration}, {type}");
			InstancedBuffNPC gn = npc.GetGlobalNPC<InstancedBuffNPC>();

			if (InstancedBuff.TryGetPrototype(type, out InstancedBuff proto))
			{
				if (gn.buffInstances.Any(n => n.Name == proto.Name))
					gn.buffInstances.RemoveAll(n => n.Name == proto.Name);

				gn.buffInstances.Add(proto.Clone());
				npc.AddBuff(proto.BackingType, duration);
			}
		}

		private static void InflictInner<T>(Player player, int duration, T premadeInstance = null) where T : InstancedBuff, new()
		{
			InstancedBuffPlayer mp = player.GetModPlayer<InstancedBuffPlayer>();

			if (premadeInstance is null) //Dont want to override the existing instance
			{
				if (mp.buffInstances.Any(n => n is T)) //If an instance already exists, let the backer get re-inflicted and stop
				{
					premadeInstance = InstancedBuffPlayer.GetInstance<T>(player);
					player.AddBuff(premadeInstance.BackingType, duration);
					premadeInstance.NetSync(player.whoAmI, true);
					return;
				}

				premadeInstance = new T(); //Else make a new instance and continue on
			}

			if (mp.buffInstances.Any(n => n is T)) //Remove any old instances if this is an override
				mp.buffInstances.RemoveAll(n => n is T);

			mp.buffInstances.Add(premadeInstance); //Add the new instance
			player.AddBuff(premadeInstance.BackingType, duration); //Inflict the backer
			premadeInstance.NetSync(player.whoAmI, true);
		}

		private static void InflictInner<T>(NPC npc, int duration, T premadeInstance = null) where T : InstancedBuff, new()
		{
			InstancedBuffNPC gn = npc.GetGlobalNPC<InstancedBuffNPC>();

			if (premadeInstance is null) //Dont want to override the existing instance
			{
				if (gn.buffInstances.Any(n => n is T)) //If an instance already exists, let the backer get re-inflicted and stop
				{
					premadeInstance = InstancedBuffNPC.GetInstance<T>(npc);
					npc.AddBuff(premadeInstance.BackingType, duration);
					premadeInstance.NetSync(npc.whoAmI, false);
					return;
				}

				premadeInstance = new T(); //Else make a new instance and continue on
			}

			if (gn.buffInstances.Any(n => n is T)) //Remove any old instances if this is an override
				gn.buffInstances.RemoveAll(n => n is T);

			gn.buffInstances.Add(premadeInstance); //Add the new instance
			npc.AddBuff(premadeInstance.BackingType, duration); //Inflict the backer
			premadeInstance.NetSync(npc.whoAmI, false);
		}

		private static void InflictStackInner<T>(NPC npc, int duration, BuffStack premadeStack) where T : InstancedBuff, new()
		{
			var instance = InstancedBuffNPC.GetInstance<T>(npc) as StackableBuff; //If possible, get the existing instancedBuff instance for this stackable buff

			//If we have too many stacks already, abort
			if (instance != null && instance.MaxStacks != -1 && instance.stacks.Count >= instance.MaxStacks)
				return;

			if (instance is null)
			{
				InflictInner<T>(npc, duration); //If not found, inflict it and get it
				instance = InstancedBuffNPC.GetInstance<T>(npc) as StackableBuff;
			}

			premadeStack ??= instance.GenerateDefaultStack(duration); //If we want to add a default stack
			premadeStack.duration = duration; //Override duration

			instance.stacks.Add(premadeStack); //Add our new stack

			int index = npc.FindBuffIndex(instance.BackingType); //Find the backer and update it's time to reflect that of the longest stack

			if (index != -1)
				npc.buffTime[index] = instance.GetDuration();

			instance.NetSync(npc.whoAmI, false);
		}

		private static void InflictStackInner<T>(Player player, int duration, BuffStack premadeStack) where T : InstancedBuff, new()
		{
			var instance = InstancedBuffPlayer.GetInstance<T>(player) as StackableBuff; //If possible, get the existing instancedBuff instance for this stackable buff

			//If we have too many stacks already, abort
			if (instance != null && instance.MaxStacks != -1 && instance.stacks.Count >= instance.MaxStacks)
				return;

			if (instance is null)
			{
				InflictInner<T>(player, duration); //If not found, inflict it and get it
				instance = InstancedBuffPlayer.GetInstance<T>(player) as StackableBuff;
			}

			premadeStack ??= instance.GenerateDefaultStack(duration);//If we want to add a default stack
			premadeStack.duration = duration; //Override duration

			instance.stacks.Add(premadeStack); //Add our new stack

			int index = player.FindBuffIndex(instance.BackingType); //Find the backer and update it's time to reflect that of the longest stack

			if (index != -1)
				player.buffTime[index] = instance.GetDuration();

			instance.NetSync(player.whoAmI, true);
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
		public static void InflictStack<A, B>(NPC npc, int duration, B premadeStack = null) where A : StackableBuff<B>, new() where B : BuffStack, new()
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
		public static void InflictStack<A, B>(Player player, int duration, B premadeStack = null) where A : StackableBuff<B>, new() where B : BuffStack, new()
		{
			InflictStackInner<A>(player, duration, premadeStack);
		}
	}
}