using StarlightRiver.Content.Keys;
using System.Collections.Generic;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Core.Systems.KeySystem
{
	internal class KeySystem : ModSystem
	{
		public static List<Key> Keys = new();

		public static List<Key> KeyInventory = new();

		public override void Load()
		{
			Terraria.On_Main.DrawItems += DrawKeys;
		}

		public override void Unload()
		{
			Keys = null;
			KeyInventory = null;
		}

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

		private void DrawKeys(Terraria.On_Main.orig_DrawItems orig, Main self)
		{
			foreach (Key key in Keys)
				key.Draw(Main.spriteBatch);

			orig(self);
		}
	}
}