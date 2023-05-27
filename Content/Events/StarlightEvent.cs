using StarlightRiver.Content.Bosses.VitricBoss;
using StarlightRiver.Content.NPCs.Starlight;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Content.Events
{
	internal class StarlightEventSequenceSystem : ModSystem
	{
		public static int sequence = 0;
		public static bool willOccur = false;
		public static bool occuring = false;

		public static int fadeTimer;

		public static bool Active => !Main.dayTime && (occuring || fadeTimer > 0);

		public override void PostUpdateTime()
		{
			// Handles the fade effects
			if (occuring && fadeTimer < 300)
				fadeTimer++;

			if (!occuring && fadeTimer > 0)
				fadeTimer--;

			if (Main.dayTime && occuring)
			{
				occuring = false;
				willOccur = true;
			}

			// The event should trigger the next applicable night
			if (willOccur && !Main.dayTime && Main.time == 0)
			{
				occuring = true;
				willOccur = false;
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
				tileColor = Color.Lerp(tileColor, new Color(10, 200, 255) * 0.25f, fadeTimer / 300f);
				backgroundColor = Color.Lerp(backgroundColor, new Color(10, 80, 100) * 0.25f, fadeTimer / 300f);
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
		public override int Music => MusicLoader.GetMusicSlot("StarlightRiver/Sounds/Music/Moonstone");

		public override SceneEffectPriority Priority => SceneEffectPriority.BossMedium;

		public override void Load()
		{
			base.Load();
			StarlightNPC.OnKillEvent += TriggerEventActivation;
		}

		private void TriggerEventActivation(NPC NPC) //TODO: This might be worth moving elsewhere? This is a bit hidden away here
		{
			if (NPC.boss && StarlightEventSequenceSystem.sequence <= 0) //First visit is after any boss
				StarlightEventSequenceSystem.willOccur = true;

			if (NPC.type == ModContent.NPCType<VitricBoss>() && StarlightEventSequenceSystem.sequence == 1) //Second visit is after ceiros
				StarlightEventSequenceSystem.willOccur = true;
		}

		public override bool IsSceneEffectActive(Player player)
		{
			return StarlightEventSequenceSystem.Active;
		}
	}
}
