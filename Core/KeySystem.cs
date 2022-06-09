using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using StarlightRiver.Keys;

namespace StarlightRiver.Core
{
	internal class KeySystem : ModSystem
	{
		public static List<Key> Keys = new List<Key>();

		public static List<Key> KeyInventory = new List<Key>();

		public override void PostUpdateWorld()
		{
			foreach (Key key in Keys)
			{
				key.Update();
			}
		}

		public override void SaveWorldData(TagCompound tag)
        {

        }

		public override void LoadWorldData(TagCompound tag)
		{
			foreach (Key key in KeyInventory)
			{
				Content.GUI.KeyInventory.keys.Add(new Content.GUI.KeyIcon(key, false));
			}
		}

		public override void Unload()
		{
			Keys = null;
			KeyInventory = null;	
		}
	}
}
