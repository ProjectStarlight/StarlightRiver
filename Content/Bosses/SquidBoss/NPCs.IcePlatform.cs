using StarlightRiver.Content.NPCs.BaseTypes;
using System.Linq;

namespace StarlightRiver.Content.Bosses.SquidBoss
{
	class IcePlatform : MovingPlatform
	{
		public int bobTime = 0;

		public override string Texture => AssetDirectory.SquidBoss + Name;

		public override void SafeSetDefaults()
		{
			NPC.width = 200;
			NPC.height = 32;
		}

		public override void SafeAI()
		{
			if (NPC.ai[2] == 0)
				NPC.ai[2] = NPC.position.Y;

			if (NPC.ai[3] != 0)
			{
				if (NPC.ai[3] > 360)
					NPC.position.Y += 8;

				if (NPC.ai[3] <= 90 && NPC.ai[3] > 0)
					NPC.position.Y -= 9;

				NPC.ai[3]--;

				return;
			}

			if (Main.npc.Any(n => n.active && n.type == ModContent.NPCType<ArenaActor>()))
			{
				var actor = Main.npc.FirstOrDefault(n => n.active && n.type == ModContent.NPCType<ArenaActor>()).ModNPC as ArenaActor;

				if (NPC.position.Y >= NPC.ai[2])
				{
					NPC.rotation = 0;
					NPC.ai[1] = 0;
				}

				if (NPC.position.Y + 18 >= actor.WaterLevelWorld)
					NPC.ai[1] = 1;

				if (NPC.ai[1] == 1 && (!Main.tile[(int)NPC.Center.X / 16, (int)NPC.Center.Y / 16 - 5].HasTile || actor.WaterLevelWorld - 18 > NPC.position.Y))
				{
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
	}

	class IcePlatformSmall : MovingPlatform, IUnderwater
	{
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
			if (NPC.ai[0] == 0)
				NPC.ai[0] = NPC.position.Y;

			if (beingStoodOn)
			{
				Dust.NewDust(NPC.position, NPC.width, NPC.height, Terraria.ID.DustID.Ice);
				NPC.ai[1]++;
			}
			else if (NPC.ai[1] > 0)
			{
				NPC.ai[1]--;
			}

			int maxStandTime = Main.masterMode ? 2 : 20;

			if (NPC.ai[1] >= maxStandTime)
				NPC.velocity.Y += 0.3f;
			else if (NPC.position.Y > NPC.ai[0])
				NPC.velocity.Y = -1;
			else
				NPC.velocity.Y = 0;
		}
	}

	class GoldPlatform : MovingPlatform, IUnderwater
	{
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
			if (NPC.ai[0] == 0)
				NPC.ai[0] = NPC.position.Y;

			if (beingStoodOn && StarlightWorld.HasFlag(WorldFlags.SquidBossOpen))
			{
				if (NPC.velocity.Y < 1.5f)
					NPC.velocity.Y += 0.02f;

				if (NPC.position.Y - NPC.ai[0] > 1600)
					NPC.velocity.Y = 0;
			}
			else if (NPC.position.Y > NPC.ai[0])
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
