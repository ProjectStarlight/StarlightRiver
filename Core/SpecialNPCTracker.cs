using Microsoft.Xna.Framework;
using StarlightRiver.Content.Bosses.VitricBoss;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Core
{
	class SpecialNPCTracker : GlobalNPC, ILoadable
	{
		//TODO: Implement some other way to efficiently track NPC presence perhaps.
		public static Dictionary<int, bool> Tracked = new Dictionary<int, bool>();

		public float Priority => 1;

		public void Load()
		{
			On.Terraria.Main.DoUpdate += ResetTracked;

			Tracked = new Dictionary<int, bool>(); //TODO: Find a way to use an attibute or smth to automatically add to what should be tracked. Hardcoding here is gross.
			Tracked.Add(ModContent.NPCType<VitricBoss>(), false);
		}

		private void ResetTracked(On.Terraria.Main.orig_DoUpdate orig, Main self, GameTime gameTime)
		{
			foreach (int n in Tracked.Keys)
				Tracked[n] = false;
		}

		public void Unload() 
		{
			Tracked = new Dictionary<int, bool>();
		}

		public override bool PreAI(NPC npc)
		{
			if (Tracked.ContainsKey(npc.type))
				Tracked[npc.type] = true;

			return base.PreAI(npc);
		}
	}
}
