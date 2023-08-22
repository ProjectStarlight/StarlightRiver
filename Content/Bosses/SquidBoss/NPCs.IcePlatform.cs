﻿using StarlightRiver.Content.NPCs.BaseTypes;
using System.IO;
using System.Linq;

namespace StarlightRiver.Content.Bosses.SquidBoss
{
	class IcePlatform : MovingPlatform
	{
		public int bobTime = 0;
		public int index;

		public ref float Timer => ref NPC.ai[0];
		public ref float State => ref NPC.ai[1];
		public ref float HomeYPosition => ref NPC.ai[2];
		public ref float FallTime => ref NPC.ai[3];

		public override string Texture => AssetDirectory.SquidBoss + Name;

		public override void SafeSetDefaults()
		{
			NPC.width = 200;
			NPC.height = 32;
		}

		public override void SafeAI()
		{
			Timer++;

			if (Timer == 1)
				NPC.netUpdate = true;

			if (HomeYPosition == 0)
				HomeYPosition = NPC.position.Y;

			if (FallTime != 0)
			{
				if (FallTime > 360)
					NPC.position.Y += 8;

				if (FallTime <= 90 && FallTime > 0)
					NPC.position.Y -= 9;

				FallTime--;

				return;
			}

			if (Main.npc.Any(n => n.active && n.type == ModContent.NPCType<ArenaActor>()))
			{
				var actor = Main.npc.FirstOrDefault(n => n.active && n.type == ModContent.NPCType<ArenaActor>()).ModNPC as ArenaActor;

				if (NPC.position.Y >= HomeYPosition)
				{
					NPC.rotation = 0;
					State = 0;
					NPC.velocity *= 0;

					NPC.position.Y = HomeYPosition;
				}

				if (NPC.position.Y + 18 >= actor.WaterLevelWorld)
					State = 1;

				if (State == 1 && (!Main.tile[(int)NPC.Center.X / 16, (int)NPC.Center.Y / 16 - 5].HasTile || actor.WaterLevelWorld - 18 > NPC.position.Y))
				{
					bool blockedByTiles = false;

					for (int x = -1; x < NPC.width / 16 + 1; x++)
					{
						Tile tile = Framing.GetTileSafely((int)NPC.position.X / 16 + x, (int)NPC.position.Y / 16 - 2);

						if (tile.HasTile && Main.tileSolid[tile.TileType])
							blockedByTiles = true;
					}

					if (!blockedByTiles || actor.WaterGoingDown && (actor.WaterLevelWorld - 18) >= NPC.position.Y)
						NPC.position.Y = actor.WaterLevelWorld - 18;

					if ((beingStoodOn || bobTime > 0) && bobTime < 30)
						bobTime++;

					if (bobTime >= 30 && !beingStoodOn)
						bobTime = 0;

					NPC.rotation = (float)System.Math.Sin((Main.GameUpdateCount + NPC.Center.X) * 0.04f) * 0.05f;

					NPC.position.Y += (float)System.Math.Sin(bobTime / 30f * 6.28f * 2) * ((30 - bobTime) / 30f) * 4;
					NPC.rotation += (float)System.Math.Sin(bobTime / 30f * 6.28f * 2) * ((30 - bobTime) / 30f) * 0.05f;
				}
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			spriteBatch.Draw(tex, NPC.Center - Main.screenPosition, null, Lighting.GetColor((int)NPC.Center.X / 16, (int)NPC.Center.Y / 16) * 1.5f, NPC.rotation, tex.Size() / 2, 1, 0, 0);
			return false;
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write(index);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			index = reader.ReadInt32();
		}
	}

	class IcePlatformSmall : MovingPlatform, IUnderwater
	{
		public ref float HomeYPosition => ref NPC.ai[0];
		public ref float FallTimer => ref NPC.ai[1];

		public override string Texture => AssetDirectory.SquidBoss + Name;

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			return false;
		}

		public void DrawUnderWater(SpriteBatch spriteBatch, int NPCLayer)
		{
			spriteBatch.Draw(ModContent.Request<Texture2D>(Texture).Value, NPC.position - Main.screenPosition, Lighting.GetColor((int)NPC.Center.X / 16, (int)NPC.Center.Y / 16) * 1.5f);
		}

		public override void SafeSetDefaults()
		{
			NPC.width = 100;
			NPC.height = 20;
		}

		public override void SafeAI()
		{
			if (HomeYPosition == 0)
			{
				HomeYPosition = NPC.position.Y;
				NPC.netUpdate = true;
			}

			if (beingStoodOn)
			{
				Dust.NewDust(NPC.position, NPC.width, NPC.height, Terraria.ID.DustID.Ice);
				FallTimer++;

				NPC.netUpdate = true;
			}
			else if (FallTimer > 0)
			{
				FallTimer--;

				NPC.netUpdate = true;
			}

			int maxStandTime = Main.masterMode ? 2 : 20;

			if (FallTimer >= maxStandTime)
				NPC.velocity.Y += 0.3f;
			else if (NPC.position.Y > HomeYPosition)
				NPC.velocity.Y = -1;
			else
				NPC.velocity.Y = 0;
		}
	}

	class GoldPlatform : MovingPlatform, IUnderwater
	{
		public ref float HomeYPosition => ref NPC.ai[0];

		public override string Texture => AssetDirectory.SquidBoss + Name;

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			return false;
		}

		public void DrawUnderWater(SpriteBatch spriteBatch, int NPCLayer)
		{
			spriteBatch.Draw(ModContent.Request<Texture2D>(Texture).Value, NPC.position - Main.screenPosition, Lighting.GetColor((int)NPC.Center.X / 16, (int)NPC.Center.Y / 16));
		}

		public override void SafeSetDefaults()
		{
			NPC.width = 200;
			NPC.height = 20;
		}

		public override void SafeAI()
		{
			if (HomeYPosition == 0)
				HomeYPosition = NPC.position.Y;

			if (beingStoodOn && StarlightWorld.HasFlag(WorldFlags.SquidBossOpen))
			{
				if (NPC.velocity.Y < 1.5f)
					NPC.velocity.Y += 0.02f;

				if (NPC.position.Y - HomeYPosition > 1600)
					NPC.velocity.Y = 0;
			}
			else if (NPC.position.Y > HomeYPosition)
			{
				NPC.velocity.Y = -6;
			}
			else
			{
				NPC.velocity.Y = 0;
			}
		}
	}
}