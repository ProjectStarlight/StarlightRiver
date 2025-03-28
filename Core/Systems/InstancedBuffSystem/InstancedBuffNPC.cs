﻿using System.Collections.Generic;
using System.Linq;
using Terraria.ID;

namespace StarlightRiver.Core.Systems.InstancedBuffSystem
{
	internal class InstancedBuffNPC : GlobalNPC
	{
		/// <summary>
		/// The instanced buffs that exist on this NPC
		/// </summary>
		public readonly List<InstancedBuff> buffInstances = [];

		public override bool InstancePerEntity => true;

		public override void Load()
		{
			On_NPC.UpdateNPC_BuffSetFlags += UpdateInstancedBuffs;
		}

		/// <summary>
		/// Gets the instance of a given instanced buff inflicted on an NPC
		/// </summary>
		/// <typeparam name="T">The type of the instanced buff to get</typeparam>
		/// <param name="npc">The NPC to check for this buff on</param>
		/// <returns>The inflicted instance, or null if one does not exist</returns>
		public static T GetInstance<T>(NPC npc) where T : InstancedBuff
		{
			npc.TryGetGlobalNPC(out InstancedBuffNPC instancedBuffNPC);

			if (instancedBuffNPC != null)
				return instancedBuffNPC.buffInstances.FirstOrDefault(n => n is T) as T;
			else
				return null;
		}

		/// <summary>
		/// Gets the instanced buff with the given internal name on an NPC
		/// </summary>
		/// <param name="npc">the NPC to check for this buff on</param>
		/// <param name="name">the internal name of the buff to check for</param>
		/// <returns>The inflicted instance, or null if one does not exist</returns>
		public static InstancedBuff GetInstance(NPC npc, string name)
		{
			npc.TryGetGlobalNPC(out InstancedBuffNPC instancedBuffNPC);

			if (instancedBuffNPC != null)
				return instancedBuffNPC.buffInstances.FirstOrDefault(n => n.Name == name);
			else
				return null;
		}

		/// <summary>
		/// Updates the standard update loop for instanced buffs
		/// </summary>
		/// <param name="orig"></param>
		/// <param name="self"></param>
		/// <param name="lowerBuffTime"></param>
		private void UpdateInstancedBuffs(On_NPC.orig_UpdateNPC_BuffSetFlags orig, NPC self, bool lowerBuffTime)
		{
			orig(self, lowerBuffTime);

			if (self.type > NPCID.None && self.active)
				self.GetGlobalNPC<InstancedBuffNPC>().buffInstances.ForEach(n => n.UpdateNPC(self));
		}

		/// <summary>
		/// Handles removing all expired buff instances
		/// </summary>
		/// <param name="npc"></param>
		public override void PostAI(NPC npc)
		{
			buffInstances.RemoveAll(n => !npc.HasBuff(n.BackingType));
		}
	}
}