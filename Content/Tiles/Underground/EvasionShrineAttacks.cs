using System;

namespace StarlightRiver.Content.Tiles.Underground
{
	internal partial class EvasionShrineDummy : Dummy, IDrawAdditive
	{
		int free = 0; // tracks the free square for the spear squares attack

		private void EndAttack()
		{
			Timer = 127;
			State++;
		}

		private void VerticalSawJaws(int timer)
		{
			if (timer == 0)
			{
				SpawnBlade(Projectile.Center + new Vector2(130, 200), Vector2.UnitY * -8, 110);
				SpawnBlade(Projectile.Center + new Vector2(-130, 200), Vector2.UnitY * -8, 110);
			}

			if (timer == 30)
			{
				SpawnBlade(Projectile.Center + new Vector2(320, -300), Vector2.UnitY * 8, 90);
				SpawnBlade(Projectile.Center + new Vector2(-320, -300), Vector2.UnitY * 8, 90);
				SpawnBlade(Projectile.Center + new Vector2(0, -400), Vector2.UnitY * 8, 120);
			}

			if (timer == 120)
				EndAttack();
		}

		private void HorizontalSawJaws(int timer)
		{
			if (timer == 0)
			{
				SpawnBlade(Projectile.Center + new Vector2(-500, -200), Vector2.UnitX * 9.5f, 120);
				SpawnBlade(Projectile.Center + new Vector2(-500, 0), Vector2.UnitX * 9.5f, 120);
				SpawnBlade(Projectile.Center + new Vector2(-500, 200), Vector2.UnitX * 9.5f, 120);
				SpawnBlade(Projectile.Center + new Vector2(-500, 400), Vector2.UnitX * 9.5f, 120);

				SpawnBlade(Projectile.Center + new Vector2(500, -300), Vector2.UnitX * -9.5f, 120);
				SpawnBlade(Projectile.Center + new Vector2(500, -100), Vector2.UnitX * -9.5f, 120);
				SpawnBlade(Projectile.Center + new Vector2(500, 100), Vector2.UnitX * -9.5f, 120);
				SpawnBlade(Projectile.Center + new Vector2(500, 300), Vector2.UnitX * -9.5f, 120);
			}

			if (timer == 140)
				EndAttack();
		}

		private void DartBurst(int timer)
		{
			if (timer == 0)
			{
				for (float k = 0; k <= 6.28f; k += 1.57f)
				{
					SpawnDart(Projectile.Center, Projectile.Center + Vector2.One.RotatedBy(k) * 200, Projectile.Center + Vector2.UnitX.RotatedBy(k) * 400, 120);
				}
			}

			if (timer == 140)
				EndAttack();
		}

		private void DartBurst2(int timer)
		{
			if (timer == 0)
			{
				for (float k = 0; k <= 6.28f; k += 1.57f)
				{
					SpawnDart(Projectile.Center, Projectile.Center + Vector2.One.RotatedBy(k + 0.6f) * 200, Projectile.Center + Vector2.UnitX.RotatedBy(k + 0.6f) * 400, 120);
				}
			}

			if (timer == 140)
				EndAttack();
		}

		private void SpearsAndSwooshes(int timer)
		{
			if (timer == 0)
			{
				SpawnSpear(Projectile.Center + new Vector2(-270, 100), Projectile.Center + new Vector2(-275, 0), 60, 15, 30);
				SpawnSpear(Projectile.Center + new Vector2(-220, 100), Projectile.Center + new Vector2(-225, 0), 60, 15, 30);

				SpawnSpear(Projectile.Center + new Vector2(270, 100), Projectile.Center + new Vector2(275, 0), 60, 15, 30);
				SpawnSpear(Projectile.Center + new Vector2(220, 100), Projectile.Center + new Vector2(225, 0), 60, 15, 30);
			}

			if (timer == 20)
			{
				SpawnSpear(Projectile.Center + new Vector2(-144, 140), Projectile.Center + new Vector2(-149, 40), 60, 15, 30);
				SpawnSpear(Projectile.Center + new Vector2(-94, 140), Projectile.Center + new Vector2(-101, 40), 60, 15, 30);

				SpawnSpear(Projectile.Center + new Vector2(144, 140), Projectile.Center + new Vector2(149, 40), 60, 15, 30);
				SpawnSpear(Projectile.Center + new Vector2(94, 140), Projectile.Center + new Vector2(101, 40), 60, 15, 30);
			}

			if (timer == 40)
			{
				SpawnSpear(Projectile.Center + new Vector2(-25, 55), Projectile.Center + new Vector2(-30, -45), 60, 15, 30);
				SpawnSpear(Projectile.Center + new Vector2(25, 55), Projectile.Center + new Vector2(30, -45), 60, 15, 30);
			}

			if (timer == 120)
			{
				SpawnDart(Projectile.Center + new Vector2(0, -450), Projectile.Center + new Vector2(50, 0), Projectile.Center + new Vector2(100, 450), 90);
				SpawnDart(Projectile.Center + new Vector2(0, 450), Projectile.Center + new Vector2(-50, 0), Projectile.Center + new Vector2(-100, -450), 90);
			}

			if (timer == 130)
			{
				SpawnDart(Projectile.Center + new Vector2(0, -450), Projectile.Center + new Vector2(200, 0), Projectile.Center + new Vector2(200, 450), 90);
				SpawnDart(Projectile.Center + new Vector2(0, 450), Projectile.Center + new Vector2(-200, 0), Projectile.Center + new Vector2(-200, -450), 90);
			}

			if (timer == 140)
			{
				SpawnDart(Projectile.Center + new Vector2(0, -450), Projectile.Center + new Vector2(350, 0), Projectile.Center + new Vector2(300, 450), 90);
				SpawnDart(Projectile.Center + new Vector2(0, 450), Projectile.Center + new Vector2(-350, 0), Projectile.Center + new Vector2(-300, -450), 90);
			}

			if (timer == 240)
				EndAttack();
		}

		private void TopSpearsBottomDarts(int timer)
		{
			if (timer < 100)
			{
				for (int k = 0; k < 100; k += 10)
				{
					if (timer == k)
					{
						int yOff = (int)((float)Math.Sin(k / 100f * 3.14f) * 270);
						SpawnSpear(Projectile.Center + new Vector2(-400 + k * 8, -200 - yOff), Projectile.Center + new Vector2(-400 + k * 8, 0 - yOff), 30, 15, 25);
					}
				}
			}

			if (timer == 60)
			{
				SpawnDart(Projectile.Center + new Vector2(-400, 100), Projectile.Center, Projectile.Center + new Vector2(400, 100), 120);
				SpawnDart(Projectile.Center + new Vector2(400, 100), Projectile.Center + new Vector2(0, 170), Projectile.Center + new Vector2(-400, 100), 120);
			}

			if (timer >= 240)
				EndAttack();
		}

		private void MiddleSqueeze(int timer)
		{
			if (timer <= 80)
			{
				for (int k = -400; k < 300; k += 60)
				{
					float off = 0;

					if (k < -200)
						off = (k + 200) * -0.9f;

					if (timer == (k + 400) / 10)
					{
						SpawnSpear(Projectile.Center + new Vector2(-410 + off, k), Projectile.Center + new Vector2(-100, k), 60, 45, 15, 420);
						SpawnSpear(Projectile.Center + new Vector2(410 - off, k), Projectile.Center + new Vector2(100, k), 60, 45, 15, 420);
					}
				}
			}

			if (timer >= 100 && timer % 40 == 0 && timer < 530)
				SpawnBlade(Projectile.Center + new Vector2(40 * (Main.rand.NextBool() ? 1 : -1), -400), Vector2.UnitY * 11, 90);

			if (timer >= 640)
				EndAttack();
		}

		private void ShooFromMiddle(int timer)
		{
			if (timer == 0)
			{
				SpawnBlade(Projectile.Center + new Vector2(40, 300), Vector2.UnitY * -6, 180);
				SpawnBlade(Projectile.Center + new Vector2(-40, 300), Vector2.UnitY * -6, 180);
			}

			if (timer == 120)
			{
				SpawnBlade(Projectile.Center + new Vector2(360, -300), Vector2.UnitY * 6, 60);
				SpawnBlade(Projectile.Center + new Vector2(-360, -300), Vector2.UnitY * 6, 60);
			}

			if (timer == 30)
			{
				SpawnSpear(Projectile.Center + new Vector2(-140, -140), Projectile.Center + new Vector2(-140, 40), 60, 15, 60);
				SpawnSpear(Projectile.Center + new Vector2(-94, -140), Projectile.Center + new Vector2(-94, 40), 60, 15, 60);

				SpawnSpear(Projectile.Center + new Vector2(140, -140), Projectile.Center + new Vector2(140, 40), 60, 15, 60);
				SpawnSpear(Projectile.Center + new Vector2(94, -140), Projectile.Center + new Vector2(94, 40), 60, 15, 60);

				SpawnSpear(Projectile.Center + new Vector2(-140, -180), Projectile.Center + new Vector2(-140, -320), 60, 15, 60);
				SpawnSpear(Projectile.Center + new Vector2(-94, -180), Projectile.Center + new Vector2(-94, -320), 60, 15, 60);

				SpawnSpear(Projectile.Center + new Vector2(140, -180), Projectile.Center + new Vector2(140, -320), 60, 15, 60);
				SpawnSpear(Projectile.Center + new Vector2(94, -180), Projectile.Center + new Vector2(94, -320), 60, 15, 60);
			}

			if (timer >= 180)
				EndAttack();
		}

		private void SideSqueeze(int timer)
		{
			for (int k = -200; k <= 200; k += 50)
			{
				if (timer == (k + 200) / 10)
				{
					bool swap = k % 100 == 0;
					SpawnSpear(Projectile.Center + new Vector2(k, swap ? -440 : 200), Projectile.Center + new Vector2(k, swap ? 200 : -440), 120, 45, 30, 320);
				}
			}

			if (timer > 60 && timer % 40 == 0 && timer <= 500)
			{
				int direction = Main.rand.NextBool() ? 400 : Main.rand.NextBool() ? 320 : 240;
				SpawnBlade(Projectile.Center + new Vector2(direction, 300), Vector2.UnitY * -4, 160);

				direction = Main.rand.NextBool() ? 400 : Main.rand.NextBool() ? 320 : 240;
				SpawnBlade(Projectile.Center + new Vector2(-direction, 300), Vector2.UnitY * -4, 160);
			}

			if (timer == 560)
				EndAttack();
		}

		private void CruelDarts(int timer)
		{
			if (timer == 0)
			{
				SpawnDart(Projectile.Center + new Vector2(-400, -350), Projectile.Center + new Vector2(0, -375), Projectile.Center + new Vector2(400, 350), 120);
				SpawnDart(Projectile.Center + new Vector2(400, 350), Projectile.Center + new Vector2(0, 375), Projectile.Center + new Vector2(-400, -350), 120);
			}

			if (timer == 15)
			{
				SpawnDart(Projectile.Center + new Vector2(-400, -200), Projectile.Center + new Vector2(0, -225), Projectile.Center + new Vector2(400, 200), 120);
				SpawnDart(Projectile.Center + new Vector2(400, 200), Projectile.Center + new Vector2(0, 225), Projectile.Center + new Vector2(-400, -200), 120);
			}

			if (timer == 30)
			{
				SpawnDart(Projectile.Center + new Vector2(-400, -50), Projectile.Center + new Vector2(0, -75), Projectile.Center + new Vector2(400, 50), 120);
				SpawnDart(Projectile.Center + new Vector2(400, 50), Projectile.Center + new Vector2(0, 75), Projectile.Center + new Vector2(-400, -50), 120);
			}

			if (timer == 90)
			{
				SpawnDart(Projectile.Center + new Vector2(400, -300), Projectile.Center + new Vector2(0, -325), Projectile.Center + new Vector2(-400, 300), 120);
				SpawnDart(Projectile.Center + new Vector2(-400, 300), Projectile.Center + new Vector2(0, 325), Projectile.Center + new Vector2(400, -300), 120);
			}

			if (timer == 105)
			{
				SpawnDart(Projectile.Center + new Vector2(400, -150), Projectile.Center + new Vector2(0, -175), Projectile.Center + new Vector2(-400, 150), 120);
				SpawnDart(Projectile.Center + new Vector2(-400, 150), Projectile.Center + new Vector2(0, 175), Projectile.Center + new Vector2(400, -150), 120);
			}

			if (timer == 120)
			{
				SpawnDart(Projectile.Center + new Vector2(400, 0), Projectile.Center + new Vector2(0, -25), Projectile.Center + new Vector2(-400, 0), 120);
				SpawnDart(Projectile.Center + new Vector2(-400, 0), Projectile.Center + new Vector2(0, 25), Projectile.Center + new Vector2(400, 0), 120);
			}

			if (timer == 240)
				EndAttack();
		}

		private void SquareSpears(int timer)
		{
			if (timer == 0)
				free = Main.rand.Next(4);

			if (timer % 240 == 0 && timer < 720)
			{
				free++;

				if (free % 4 != 0)
				{
					for (int k = 0; k < 50; k += 5)
					{
						int yOff = (int)((float)Math.Sin(k / 100f * 3.14f) * 270);
						SpawnSpear(Projectile.Center + new Vector2(-380 + k * 8, -200 - yOff), Projectile.Center + new Vector2(-380 + k * 8, -60), 160 - k, 30, 30, 30);
					}
				}

				if (free % 4 != 1)
				{
					for (int k = 0; k < 50; k += 5)
					{
						int yOff = (int)((float)Math.Sin(k / 100f * 3.14f) * 270);
						SpawnSpear(Projectile.Center + new Vector2(380 - k * 8, -200 - yOff), Projectile.Center + new Vector2(380 - k * 8, -60), 160 - k, 30, 30, 30);
					}
				}

				if (free % 4 != 2)
				{
					for (int k = 0; k < 50; k += 5)
					{
						SpawnSpear(Projectile.Center + new Vector2(-380 + k * 8, 300), Projectile.Center + new Vector2(-380 + k * 8, 40), 160 - k, 30, 30, 30);
					}
				}

				if (free % 4 != 3)
				{
					for (int k = 0; k < 50; k += 5)
					{
						SpawnSpear(Projectile.Center + new Vector2(380 - k * 8, 300), Projectile.Center + new Vector2(380 - k * 8, 40), 160 - k, 30, 30, 30);
					}
				}
			}

			if (timer >= 720)
				EndAttack();
		}
	}
}
