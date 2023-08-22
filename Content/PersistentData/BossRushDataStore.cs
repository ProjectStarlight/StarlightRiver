using StarlightRiver.Core.Systems.PersistentDataSystem;
using System;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Content.PersistentData
{
	[Flags]
	internal enum BossrushUnlockFlag : int
	{
		Auroracle = 1,
		Glassweaver = 2,
		Ceiros = 4
	}

	internal class BossRushDataStore : PersistentDataStore
	{
		BossrushUnlockFlag flags;

		public int normalScore;
		public int expertScore;
		public int masterScore;

		/// <summary>
		/// If the boss rush should be unlocked or not
		/// </summary>
		public static bool UnlockedBossRush => ((int)PersistentDataStoreSystem.GetDataStore<BossRushDataStore>().flags & 0b111) == 0b111;

		/// <summary>
		/// Returns if a given boss is recorded as being beaten or not
		/// </summary>
		/// <param name="flag">The flag to check for</param>
		/// <returns>If that flag is true</returns>
		public static bool DownedBoss(BossrushUnlockFlag flag)
		{
			return (PersistentDataStoreSystem.GetDataStore<BossRushDataStore>().flags & flag) != 0;
		}

		/// <summary>
		/// Mark the given boss as downed for the purpose of unlocking boss rush
		/// </summary>
		/// <param name="flag"></param>
		public static void DefeatBoss(BossrushUnlockFlag flag)
		{
			PersistentDataStoreSystem.GetDataStore<BossRushDataStore>().flags |= flag;
			PersistentDataStoreSystem.GetDataStore<BossRushDataStore>().ForceSave();
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