using StarlightRiver.Content.NPCs.Starlight;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Content.Events
{
	internal class StarlightEventSequenceSystem : ModSystem
	{
		public static int sequence = 0;
		public static bool willOccur = false;
		public static bool occuring = false;

		public static bool Active => !Main.dayTime && occuring;

		public override void PostUpdateTime()
		{
			// The event should trigger the next applicable night
			if (willOccur && !Main.dayTime && Main.time == 0)
			{
				occuring = true;
				Main.NewText("A strange traveler has arrived...", new Color(150, 200, 255));
				NPC.NewNPC(null, Main.spawnTileX * 16, Main.spawnTileY * 16 - 120, ModContent.NPCType<Crow>());
			}

			if (Active && Main.time > 32400 / 2)
				Main.time = 32400 / 2;
		}

		public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)
		{
			if (Active)
			{
				tileColor = new Color(10, 200, 255) * 0.25f;
				backgroundColor = new Color(10, 80, 100) * 0.25f;
			}
		}

		public override void SaveWorldData(TagCompound tag)
		{
			tag["sequence"] = sequence;
			tag["willOccur"] = willOccur;
		}

		public override void LoadWorldData(TagCompound tag)
		{
			sequence = tag.GetInt("sequence");
			willOccur = tag.GetBool("willOccur");
		}
	}

	internal class StarlightEvent : ModSceneEffect
	{
		public override int Music => MusicLoader.GetMusicSlot(Mod, "Sounds/Music/VoidPre");

		public override void Load()
		{
			base.Load();
			StarlightNPC.OnKillEvent += trackEventActive;
		}

		private void trackEventActive(NPC NPC)
		{
			if (NPC.boss && StarlightEventSequenceSystem.sequence <= 0)
				StarlightEventSequenceSystem.willOccur = true;
		}

		public override bool IsSceneEffectActive(Player player)
		{
			return !Main.dayTime && StarlightEventSequenceSystem.willOccur;
		}
	}
}
