using StarlightRiver.Core.Systems.PersistentDataSystem;
using System;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Content.PersistentData
{
	[Flags]
	internal enum BossrushUnlockFlag : int
	{
		Auroracle,
		Glassweaver,
		Ceiros
	}

	internal class BossRushDataStore : PersistentDataStore
	{
		BossrushUnlockFlag flags;
		int normalScore;
		int expertScore;
		int masterScore;

		/// <summary>
		/// Mark the given boss as downed for the purpose of unlocking boss rush
		/// </summary>
		/// <param name="flag"></param>
		public static void DefeatBoss(BossrushUnlockFlag flag)
		{
			PersistentDataStoreSystem.GetDataStore<BossRushDataStore>().flags |= flag;
		}

		public override void SaveGlobal(TagCompound tag)
		{
			tag["flags"] = (int)flags;
			tag["normal"] = normalScore;
			tag["expert"] = expertScore;
			tag["master"] = masterScore;

		}

		public override void LoadGlobal(TagCompound tag)
		{
			flags = (BossrushUnlockFlag)tag.GetInt("flags");
			normalScore = tag.GetInt("normal");
			expertScore = tag.GetInt("expert");
			masterScore = tag.GetInt("master");
		}
	}
}
