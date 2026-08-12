using StarlightRiver.Content.Biomes;
using StarlightRiver.Content.Bosses.GlassMiniboss;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Content.NPCs.Starlight
{
	internal class AlicanSafetySystem : ModSystem
	{
		private static int scanTimer;
		private static int intendedAlicanPhase;

		public static int IntendedAlicanPhase
		{
			get => intendedAlicanPhase;

			set
			{
				if (value > intendedAlicanPhase)
					intendedAlicanPhase = value;
			}
		}

		public static Vector2 IntendedAlicanLocation => IntendedAlicanPhase switch
		{
			_ => ObservatorySystem.ObservatoryRoomWorld.TopLeft() + new Vector2(11, 4) * 16
		};

		public override void PostUpdateEverything()
		{
			scanTimer++;

			if (scanTimer % 3600 == 0)
				DoAlicanScan();
		}

		public static void DoAlicanScan()
		{
			// Skip if the MP client, the server takes care of this
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;

			NPC alican = Main.npc.FirstOrDefault(n => n.active && n.type == ModContent.NPCType<Alican>());

			if (alican is null)
			{
				int i = NPC.NewNPC(null, (int)IntendedAlicanLocation.X, (int)IntendedAlicanLocation.Y, ModContent.NPCType<Alican>(), 0, 0, IntendedAlicanPhase);
				Main.npc[i].netUpdate = true;
			}
			else if (alican.ModNPC is Alican thisAlican)
			{
				if (thisAlican.State != intendedAlicanPhase)
					thisAlican.State = intendedAlicanPhase;

				if (Vector2.DistanceSquared(alican.Center, IntendedAlicanLocation) > 64)
					alican.Center = IntendedAlicanLocation;

				alican.netUpdate = true;
			}
			else
			{
				StarlightRiver.Instance.Logger.Warn("Alican's ModNPC is not of the type for the alican, something horrible has happened! Please report this to https://github.com/ProjectStarlight/StarlightRiver/issues");
				Main.NewText("Alican's ModNPC is not of the type for the alican, something horrible has happened! Please report this to https://github.com/ProjectStarlight/StarlightRiver/issues", Color.Red);

				alican.active = false;
				scanTimer = 3599;
			}
		}

		public static void DebugForceState(int stateToForce)
		{
			intendedAlicanPhase = stateToForce;

			NPC alican = Main.npc.FirstOrDefault(n => n.active && n.type == ModContent.NPCType<Alican>());

			if (alican.ModNPC is Alican thisAlican)
				thisAlican.State = stateToForce;
		}

		public override void ClearWorld()
		{
			intendedAlicanPhase = 0;
		}

		public override void SaveWorldData(TagCompound tag)
		{
			tag["IntendedPhase"] = intendedAlicanPhase;
		}

		public override void LoadWorldData(TagCompound tag)
		{
			intendedAlicanPhase = tag.GetInt("IntendedPhase");

			DoAlicanScan(); // Scan on load
		}
	}
}