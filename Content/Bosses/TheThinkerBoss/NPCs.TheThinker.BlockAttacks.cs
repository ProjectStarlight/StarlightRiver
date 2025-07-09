using System;

namespace StarlightRiver.Content.Bosses.TheThinkerBoss
{
	internal partial class TheThinker
	{
		/// <summary>
		/// Spawns a cube traveling in the given direction and at the given offset (in cube sizes) from the center.
		/// </summary>
		/// <param name="direction">0 = left to right, 1 = right to left, 2 = top to bottom, 3 = bottom to top</param>
		/// <param name="offset">offset in cube sizes from the center, 0 is at the center</param>
		public void SpawnBlock(int direction = -1, int offset = -99, float speed = 3, int duration = 560)
		{
			int n;

			if (direction == -1 || direction > 3)
				direction = Main.rand.Next(4);

			if (offset == -99 || offset < -4 || offset > 4)
				offset = Main.rand.Next(-4, 5);

			int timer = 560 - duration;

			switch (direction)
			{
				case 0:
					n = NPC.NewNPC(NPC.GetSource_FromThis(), (int)home.X - 776, (int)home.Y + offset * 194 + 97, ModContent.NPCType<HallucinationBlock>(), 0, timer);
					Main.npc[n].velocity.X = speed;
					break;
				case 1:
					n = NPC.NewNPC(NPC.GetSource_FromThis(), (int)home.X + 776, (int)home.Y + offset * 194 + 97, ModContent.NPCType<HallucinationBlock>(), 0, timer);
					Main.npc[n].velocity.X = -speed;
					break;
				case 2:
					n = NPC.NewNPC(NPC.GetSource_FromThis(), (int)home.X + offset * 194, (int)home.Y - 776, ModContent.NPCType<HallucinationBlock>(), 0, timer);
					Main.npc[n].velocity.Y = speed;
					break;
				case 3:
					n = NPC.NewNPC(NPC.GetSource_FromThis(), (int)home.X + offset * 194, (int)home.Y + 776, ModContent.NPCType<HallucinationBlock>(), 0, timer);
					Main.npc[n].velocity.Y = -speed;
					break;
			}
		}

		public void SpawnProjectile(int direction = -1, int offset = -99, float speed = 5, int duration = 320)
		{
			if (direction == -1 || direction > 3)
				direction = Main.rand.Next(4);

			if (offset == -99 || offset < -4 || offset > 4)
				offset = Main.rand.Next(-3, 4);

			switch (direction)
			{
				case 0:
					Projectile.NewProjectile(NPC.GetSource_FromThis(), (int)home.X - 776, (int)home.Y + offset * 194, speed, 0, ModContent.ProjectileType<HallucinationHazard>(), 30, 1, Main.myPlayer, 0, duration);
					break;
				case 1:
					Projectile.NewProjectile(NPC.GetSource_FromThis(), (int)home.X + 776, (int)home.Y + offset * 194, -speed, 0, ModContent.ProjectileType<HallucinationHazard>(), 30, 1, Main.myPlayer, 0, duration);
					break;
				case 2:
					Projectile.NewProjectile(NPC.GetSource_FromThis(), (int)home.X + offset * 194, (int)home.Y - 800, 0, speed, ModContent.ProjectileType<HallucinationHazard>(), 30, 1, Main.myPlayer, 0, duration);
					break;
				case 3:
					Projectile.NewProjectile(NPC.GetSource_FromThis(), (int)home.X + offset * 194, (int)home.Y + 800, 0, -speed, ModContent.ProjectileType<HallucinationHazard>(), 30, 1, Main.myPlayer, 0, duration);
					break;
			}
		}

		public void AlternatingDirections()
		{
			if (Timer == 1)
			{
				SpawnBlock(0, -4, 4);
				SpawnBlock(1, -3, 4);
				SpawnBlock(0, -2, 4);
				SpawnBlock(1, -1, 4);
				SpawnBlock(1, 1, 4);
				SpawnBlock(0, 2, 4);
				SpawnBlock(1, 3, 4);
				SpawnBlock(0, 4, 4);
			}

			if (Timer == 300)
			{
				SpawnBlock(2, -4, 4);
				SpawnBlock(3, -3, 4);
				SpawnBlock(2, -2, 4);
				SpawnBlock(3, -1, 4);
				SpawnBlock(2, 1, 4);
				SpawnBlock(3, 2, 4);
				SpawnBlock(2, 3, 4);
				SpawnBlock(3, 4, 4);

				if (Main.expertMode)
				{
					for (int k = -4; k <= 4; k++)
					{
						SpawnProjectile(Math.Abs(k % 2), k);
					}
				}
			}

			if (Timer == 600)
			{
				SpawnBlock(0, -4, 4);
				SpawnBlock(1, -3, 4);
				SpawnBlock(0, -2, 4);
				SpawnBlock(1, -1, 4);
				SpawnBlock(1, 1, 4);
				SpawnBlock(0, 2, 4);
				SpawnBlock(1, 3, 4);
				SpawnBlock(0, 4, 4);

				if (Main.expertMode)
				{
					for (int k = -4; k <= 4; k++)
					{
						SpawnProjectile(2 + Math.Abs(k % 2), k);
					}
				}
			}

			if (Timer == 900)
			{
				SpawnBlock(2, -4, 4);
				SpawnBlock(3, -3, 4);
				SpawnBlock(2, -2, 4);
				SpawnBlock(3, -1, 4);
				SpawnBlock(2, 1, 4);
				SpawnBlock(3, 2, 4);
				SpawnBlock(2, 3, 4);
				SpawnBlock(3, 4, 4);

				if (Main.expertMode)
				{
					for (int k = -4; k <= 4; k++)
					{
						SpawnProjectile(Math.Abs(k % 2), k);
					}
				}
			}
		}

		public void FlappyBird()
		{
			if (Timer == 1)
			{
				SpawnBlock(3, 1, 1, 300);
				SpawnBlock(3, 2, 1, 300);
				SpawnBlock(3, 3, 1, 300);
			}

			if (Timer == 1 || Timer % 200 == 0)
			{
				int skip = Main.rand.Next(-2, 3);

				while (skip == 0)
					skip = Main.rand.Next(-2, 3);

				for (int k = -4; k < 5; k++)
				{
					if (k != skip && !(Timer % 400 == 0 && Main.expertMode && Math.Abs(skip - k) < 2))
					{
						SpawnBlock(0, k, 3);
					}

					if (k == skip && Main.expertMode && Timer % 400 == 0)
					{
						SpawnProjectile(0, k - 1, 3, 560);
						SpawnProjectile(0, k + 1, 3, 560);
					}
				}
			}
		}

		public void RainingHazzards()
		{
			if (Timer == 1)
			{
				SpawnBlock(3, -2, 0, 280);
				SpawnBlock(3, -1, 0, 280);
				SpawnBlock(3, 0, 0, 280);
				SpawnBlock(3, 1, 0, 280);
				SpawnBlock(3, 2, 0, 280);
			}

			if (Timer == 1 || Timer % 65 == 0)
			{
				int dir = (int)(Timer / 65) % 4;
				SpawnBlock(dir, Main.rand.Next(-3, 4), 3);
			}
		}
	}
}
