﻿using StarlightRiver.Content.Bosses.VitricBoss;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Content.Bosses.GlassMiniboss
{
	internal class GlassweaverSafetySystem : ModSystem
	{
		int scanTimer = 0;

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
			{
				// Skip these checks if the boss fight is active
				if (NPC.AnyNPCs(ModContent.NPCType<Glassweaver>()))
					return;

				// Otherwise try to spawn/correct him as needed
				var glassWeaver = Main.npc.FirstOrDefault(n => n.active && n.type == ModContent.NPCType<GlassweaverFriendly>());

				if (glassWeaver is null)
				{
					NPC.NewNPC(null, (int)IntendedGlassweaverLocation.X, (int)IntendedGlassweaverLocation.Y, ModContent.NPCType<GlassweaverFriendly>(), 0, 0, IntendedGlassweaverPhase);
				}
				else
				{
					if (glassWeaver.ModNPC is GlassweaverFriendly gw && gw.State != intendedGlassweaverPhase)
						gw.State = intendedGlassweaverPhase;

					if (Vector2.DistanceSquared(glassWeaver.Center, IntendedGlassweaverLocation) > 64)
						glassWeaver.Center = IntendedGlassweaverLocation;
				}
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
		}
	}
}
