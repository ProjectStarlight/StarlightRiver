using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Tiles.Underground.EvasionShrineBullets;

namespace StarlightRiver.Content.Tiles.Underground
{
	internal partial class EvasionShrineDummy : Dummy, IDrawAdditive
	{
		private void EndAttack()
		{
			Timer = 127;
			State++;
		}

		private void VerticalSawJaws(int timer)
		{
			if (timer == 0)
			{
				SpawnBlade(projectile.Center + new Vector2(130, 200), Vector2.UnitY * -8, 110);
				SpawnBlade(projectile.Center + new Vector2(-130, 200), Vector2.UnitY * -8, 110);
			}

			if (timer == 30)
			{
				SpawnBlade(projectile.Center + new Vector2(320, -300), Vector2.UnitY * 8, 90);
				SpawnBlade(projectile.Center + new Vector2(-320, -300), Vector2.UnitY * 8, 90);
				SpawnBlade(projectile.Center + new Vector2(0, -400), Vector2.UnitY * 8, 120);
			}

			if (timer == 120)
				EndAttack();
		}

		private void HorizontalSawJaws(int timer)
		{
			if (timer == 0)
			{
				SpawnBlade(projectile.Center + new Vector2(-500, -200), Vector2.UnitX * 9.5f, 120);
				SpawnBlade(projectile.Center + new Vector2(-500, 0), Vector2.UnitX * 9.5f, 120);
				SpawnBlade(projectile.Center + new Vector2(-500, 200), Vector2.UnitX * 9.5f, 120);
				SpawnBlade(projectile.Center + new Vector2(-500, 400), Vector2.UnitX * 9.5f, 120);

				SpawnBlade(projectile.Center + new Vector2(500, -300), Vector2.UnitX * -9.5f, 120);
				SpawnBlade(projectile.Center + new Vector2(500, -100), Vector2.UnitX * -9.5f, 120);
				SpawnBlade(projectile.Center + new Vector2(500, 100), Vector2.UnitX * -9.5f, 120);
				SpawnBlade(projectile.Center + new Vector2(500, 300), Vector2.UnitX * -9.5f, 120);
			}

			if (timer == 140)
				EndAttack();
		}

		private void DartBurst(int timer)
		{
			if(timer == 0)
			for (float k = 0; k <= 6.28f; k += 1.57f)
				SpawnDart(projectile.Center, projectile.Center + Vector2.One.RotatedBy(k) * 200, projectile.Center + Vector2.UnitX.RotatedBy(k) * 400, 120);

			if (timer == 140)
				EndAttack();
		}

		private void SpearsAndSwooshes(int timer)
		{
			if (timer == 0)
			{
				SpawnSpear(projectile.Center + new Vector2(-270, 100), projectile.Center + new Vector2(-275, 0), 60, 15, 30);
				SpawnSpear(projectile.Center + new Vector2(-220, 100), projectile.Center + new Vector2(-225, 0), 60, 15, 30);

				SpawnSpear(projectile.Center + new Vector2(270, 100), projectile.Center + new Vector2(275, 0), 60, 15, 30);
				SpawnSpear(projectile.Center + new Vector2(220, 100), projectile.Center + new Vector2(225, 0), 60, 15, 30);
			}

			if (timer == 20)
			{
				SpawnSpear(projectile.Center + new Vector2(-144, 140), projectile.Center + new Vector2(-149, 40), 60, 15, 30);
				SpawnSpear(projectile.Center + new Vector2(-94, 140), projectile.Center + new Vector2(-101, 40), 60, 15, 30);

				SpawnSpear(projectile.Center + new Vector2(144, 140), projectile.Center + new Vector2(149, 40), 60, 15, 30);
				SpawnSpear(projectile.Center + new Vector2(94, 140), projectile.Center + new Vector2(101, 40), 60, 15, 30);
			}

			if (timer == 40)
			{
				SpawnSpear(projectile.Center + new Vector2(-25, 55), projectile.Center + new Vector2(-30, -45), 60, 15, 30);
				SpawnSpear(projectile.Center + new Vector2(25, 55), projectile.Center + new Vector2(30, -45), 60, 15, 30);
			}

			if (timer == 120)
			{
				SpawnDart(projectile.Center + new Vector2(0, -450), projectile.Center + new Vector2(50, 0), projectile.Center + new Vector2(100, 450), 90);
				SpawnDart(projectile.Center + new Vector2(0, 450), projectile.Center + new Vector2(-50, 0), projectile.Center + new Vector2(-100, -450), 90);
			}

			if (timer == 130)
			{
				SpawnDart(projectile.Center + new Vector2(0, -450), projectile.Center + new Vector2(200, 0), projectile.Center + new Vector2(200, 450), 90);
				SpawnDart(projectile.Center + new Vector2(0, 450), projectile.Center + new Vector2(-200, 0), projectile.Center + new Vector2(-200, -450), 90);
			}

			if (timer == 140)
			{
				SpawnDart(projectile.Center + new Vector2(0, -450), projectile.Center + new Vector2(350, 0), projectile.Center + new Vector2(300, 450), 90);
				SpawnDart(projectile.Center + new Vector2(0, 450), projectile.Center + new Vector2(-350, 0), projectile.Center + new Vector2(-300, -450), 90);
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
						SpawnSpear(projectile.Center + new Vector2(-400 + k * 8, -200 - yOff), projectile.Center + new Vector2(-400 + k * 8, 0 - yOff), 30, 15, 15);
					}
				}
			}

			if (timer == 60)
			{
				SpawnDart(projectile.Center + new Vector2(-400, 100), projectile.Center, projectile.Center + new Vector2(400, 100), 120);
				SpawnDart(projectile.Center + new Vector2(400, 100), projectile.Center + new Vector2(0, 170), projectile.Center + new Vector2(-400, 100), 120);
			}

			if (timer >= 240)
				EndAttack();
		}

		private void MiddleSqueeze(int timer)
		{
			if(timer <= 80)
			{
				for (int k = -400; k < 300; k += 60)
				{
					float off = 0;

					if (k < -200)
						off = (k + 200) * -0.9f;

					if (timer == (k + 400) / 10)
					{
						SpawnSpear(projectile.Center + new Vector2(-410 + off, k), projectile.Center + new Vector2(-100, k), 60, 45, 15, 420);
						SpawnSpear(projectile.Center + new Vector2(410 - off, k), projectile.Center + new Vector2(100, k), 60, 45, 15, 420);
					}
				}
			}

			if(timer >= 60 && timer % 40 == 0 && timer < 530) 
				SpawnBlade(projectile.Center + new Vector2(40 * (Main.rand.NextBool() ? 1 : -1), -400), Vector2.UnitY * 11, 90);

			if (timer >= 640)
				EndAttack();
		}

		private void ShooFromMiddle(int timer)
		{
			if(timer == 0)
			{
				SpawnBlade(projectile.Center + new Vector2(40, 300), Vector2.UnitY * -6, 180);
				SpawnBlade(projectile.Center + new Vector2(-40, 300), Vector2.UnitY * -6, 180);
			}

			if (timer == 120)
			{
				SpawnBlade(projectile.Center + new Vector2(360, -300), Vector2.UnitY * 6, 60);
				SpawnBlade(projectile.Center + new Vector2(-360, -300), Vector2.UnitY * 6, 60);
			}

			if (timer == 30)
			{
				SpawnSpear(projectile.Center + new Vector2(-140, -140), projectile.Center + new Vector2(-140, 40), 60, 15, 60);
				SpawnSpear(projectile.Center + new Vector2(-94, -140), projectile.Center + new Vector2(-94, 40), 60, 15, 60);

				SpawnSpear(projectile.Center + new Vector2(140, -140), projectile.Center + new Vector2(140, 40), 60, 15, 60);
				SpawnSpear(projectile.Center + new Vector2(94, -140), projectile.Center + new Vector2(94, 40), 60, 15, 60);

				SpawnSpear(projectile.Center + new Vector2(-140, -180), projectile.Center + new Vector2(-140, -320), 60, 15, 60);
				SpawnSpear(projectile.Center + new Vector2(-94, -180), projectile.Center + new Vector2(-94, -320), 60, 15, 60);

				SpawnSpear(projectile.Center + new Vector2(140, -180), projectile.Center + new Vector2(140, -320), 60, 15, 60);
				SpawnSpear(projectile.Center + new Vector2(94, -180), projectile.Center + new Vector2(94, -320), 60, 15, 60);
			}

			if (timer >= 180)
				EndAttack();
		}

		private void SideSqueeze(int timer)
		{
			for(int k = -200; k <= 200; k += 50)
			{
				if (timer == (k + 200) / 10)
				{
					bool swap = k % 200 == 0;
					SpawnSpear(projectile.Center + new Vector2(k, swap ? -440 : 200), projectile.Center + new Vector2(k, swap ? 200 : -440), 120, 45, 15, 320);
				}
			}

			if (timer > 60 && timer % 40 == 0 && timer <= 500)
			{
				int direction = (Main.rand.NextBool() ? 400 : Main.rand.NextBool() ? 320 : 240);
				SpawnBlade(projectile.Center + new Vector2(direction, 300), Vector2.UnitY * -4, 160);

				direction = (Main.rand.NextBool() ? 400 : Main.rand.NextBool() ? 320 : 240);
				SpawnBlade(projectile.Center + new Vector2(-direction, 300), Vector2.UnitY * -4, 160);
			}

			if (timer == 560)
				EndAttack();
		}
	}
}
