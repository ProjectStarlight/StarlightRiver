﻿using StarlightRiver.Content.Abilities;
using System.Linq;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.NPCs.Corruption
{
	class Stalker : ModNPC
	{
		public override string Texture => AssetDirectory.Assets + "NPCs/Corruption/" + Name;

		public ref float Timer => ref NPC.ai[0];
		public ref float State => ref NPC.ai[1];

		enum States
		{
			Waiting,
			Fleeing
		}

		public override void SetDefaults()
		{
			NPC.noGravity = true;
			NPC.width = 36;
			NPC.height = 28;
			NPC.immortal = true;
			NPC.lifeMax = 10000;
			NPC.alpha = 255;
		}

		public override void AI()
		{
			Timer++;

			if (Timer % 240 <= 30)
				NPC.frame = new Rectangle(0, NPC.height * (int)(Timer % 240 / 30f * 8), NPC.width, NPC.height);
			else
				NPC.frame = new Rectangle(0, 0, NPC.width, NPC.height);

			if (State == 0)
			{
				NPC.TargetClosest();

				if (Vector2.Distance(Main.player[NPC.target].Center, NPC.Center) < 360 ||
					Main.projectile.Any(n => n.active && n.friendly && Vector2.Distance(n.Center, NPC.Center) < 360) ||
					NearLight()
					)
				{
					State = (int)States.Fleeing;
				}
			}

			if (State == 1)
			{
				NPC.alpha -= 25;

				if (NPC.alpha < 50)
					NPC.active = false;
			}
		}

		public bool NearLight()
		{
			for (int x = -5; x < 5; x++)
			{
				for (int y = -5; y < 5; y++)
				{
					int xPos = (int)(NPC.Center.X / 16) + x;
					int yPos = (int)(NPC.Center.Y / 16) + y;

					if (Lighting.Brightness(xPos, yPos) > 0.15f)
						return true;
				}
			}

			return false;
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo)
		{
			// Server does not initialize lighting engine so stalkers won't function there without making them clientside entirely
			// Therefore stalkers are disabled in multiplayer until someone wants to come here and completely rewrite them
			if (Main.netMode == NetmodeID.Server)
				return 0;

			Tile tile = Framing.GetTileSafely(spawnInfo.SpawnTileX, spawnInfo.SpawnTileY);
			Vector2 spawnPoint = new Vector2(spawnInfo.SpawnTileX, spawnInfo.SpawnTileY) * 16;

			return spawnInfo.Player.ZoneCorrupt &&
				tile.WallType == WallID.EbonstoneUnsafe &&
				tile.LiquidAmount == 0 &&
				Lighting.Brightness(spawnInfo.SpawnTileX, spawnInfo.SpawnTileY) <= 0.1f &&
				!Main.npc.Any(n => n.type == NPCType<Stalker>() && Vector2.Distance(n.Center, spawnPoint) < 320) ? 1 : 0;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D tex = Request<Texture2D>(Texture).Value;
			spriteBatch.Draw(tex, NPC.Center - screenPos, NPC.frame, Color.White * (NPC.alpha / 255f), 0, NPC.Size / 2, 1, 0, 0);

			return false;
		}
	}
}