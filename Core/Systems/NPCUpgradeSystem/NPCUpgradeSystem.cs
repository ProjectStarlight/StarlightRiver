using StarlightRiver.Content.NPCs.TownUpgrade;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Core.Systems.NPCUpgradeSystem
{
	internal class NPCUpgradeSystem : ModSystem
	{
		public static Dictionary<string, bool> townUpgrades = new();

		public override void NetSend(BinaryWriter writer)
		{
			writer.Write((short)townUpgrades.Count);
			foreach (KeyValuePair<string, bool> upgrade in townUpgrades)
			{
				writer.Write(upgrade.Key);
				writer.Write(upgrade.Value);
			}
		}

		public override void NetReceive(BinaryReader reader)
		{
			short recievedCount = reader.ReadInt16();
			for (int i = 0; i < recievedCount; i++)
				townUpgrades[reader.ReadString()] = reader.ReadBoolean();
		}

		public override void OnWorldLoad()
		{
			townUpgrades = new Dictionary<string, bool>();

			//Autoload NPC upgrades
			Mod Mod = StarlightRiver.Instance;

			if (Mod.Code != null)
			{
				foreach (Type type in Mod.Code.GetTypes().Where(t => t.IsSubclassOf(typeof(TownUpgrade))))
				{
					townUpgrades.Add(type.Name.Replace("Upgrade", ""), false);
				}
			}
		}

		public override void SaveWorldData(TagCompound tag)
		{
			var townTag = new TagCompound();
			foreach (KeyValuePair<string, bool> pair in townUpgrades)
				townTag.Add(pair.Key, pair.Value);

			tag[nameof(townUpgrades)] = townTag;
		}

		public override void LoadWorldData(TagCompound tag)
		{
			TagCompound tag1 = tag.GetCompound(nameof(townUpgrades));
			var targetDict = new Dictionary<string, bool>();

			foreach (KeyValuePair<string, object> pair in tag1)
				targetDict.Add(pair.Key, tag1.GetBool(pair.Key));

			townUpgrades = targetDict;
		}

		public override void Unload()
		{
			townUpgrades = null;
		}
	}
}
