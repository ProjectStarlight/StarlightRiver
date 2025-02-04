﻿using System.Linq;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Content.Bosses.GlassMiniboss
{
	internal class GlassweaverSafetySystem : ModSystem
	{
		private static int scanTimer = 0;

		private static int intendedGlassweaverPhase;

		/// <summary>
		/// The highest state the glassweaver has reached in this world is the one he should spawn in
		/// </summary>
		public static int IntendedGlassweaverPhase
		{
			get => intendedGlassweaverPhase;

			set
			{
				if (value > intendedGlassweaverPhase)
					intendedGlassweaverPhase = value;
			}
		}

		/// <summary>
		/// The location in which the glassweaver should spawn/be
		/// </summary>
		public static Vector2 IntendedGlassweaverLocation => intendedGlassweaverPhase switch
		{
			0 => new Vector2((StarlightWorld.vitricBiome.X - 37 + 80) * 16, (StarlightWorld.vitricBiome.Center.Y + 20) * 16),
			1 or 2 or 3 or 4 or 5 or 6 => GlassweaverFriendly.ArenaPos,
			7 => StarlightWorld.vitricBiome.Center.ToVector2() * 16 + new Vector2(-86, 1200),
			_ => GlassweaverFriendly.ArenaPos,
		};

		public override void PostUpdateEverything()
		{
			scanTimer++;

			// Every minute we scan for a glassweaver. If there isnt one, or he is malformed, we spawn or correct him.
			if (scanTimer % 3600 == 0)
				DoGlassweaverScan();
		}

		/// <summary>
		/// Performs a safety check for the glassweaver to ensure he exists in the correct location and state,
		/// and if not attempts to correct him.
		/// </summary>
		public static void DoGlassweaverScan()
		{
			// Skip these checks if the boss fight is active
			if (NPC.AnyNPCs(ModContent.NPCType<Glassweaver>()))
				return;

			// Otherwise try to spawn/correct him as needed
			NPC glassWeaver = Main.npc.FirstOrDefault(n => n.active && n.type == ModContent.NPCType<GlassweaverFriendly>());

			if (glassWeaver is null)
			{
				NPC.NewNPC(null, (int)IntendedGlassweaverLocation.X, (int)IntendedGlassweaverLocation.Y, ModContent.NPCType<GlassweaverFriendly>(), 0, 0, IntendedGlassweaverPhase);
			}
			else if (glassWeaver.ModNPC is GlassweaverFriendly gw)
			{
				if (gw.State != intendedGlassweaverPhase)
					gw.State = intendedGlassweaverPhase;

				if (Vector2.DistanceSquared(glassWeaver.Center, IntendedGlassweaverLocation) > 64)
					glassWeaver.Center = IntendedGlassweaverLocation;
			}
			else
			{
				StarlightRiver.Instance.Logger.Warn("The glassweaver's ModNPC is not of the type for the glassweaver, something horrible has happened! Please report this to https://github.com/ProjectStarlight/StarlightRiver/issues");
				Main.NewText("The glassweaver's ModNPC is not of the type for the glassweaver, something horrible has happened! Please report this to https://github.com/ProjectStarlight/StarlightRiver/issues", Color.Red);

				glassWeaver.active = false;
				scanTimer = 3599;
			}
		}

		public override void ClearWorld()
		{
			intendedGlassweaverPhase = 0;
		}

		public override void SaveWorldData(TagCompound tag)
		{
			tag["IntendedPhase"] = intendedGlassweaverPhase;
		}

		public override void LoadWorldData(TagCompound tag)
		{
			intendedGlassweaverPhase = tag.GetInt("IntendedPhase");

			DoGlassweaverScan(); // Scan on load
		}
	}
}
