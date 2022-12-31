using System;

namespace StarlightRiver.Content.NPCs.Vitric.Gauntlet
{
	internal class GauntletSpawner : ConstructSpawner
	{
		public Vector2 targetPos;
		public Vector2 startPos;

		public int moveTimer;
		private float rand;

		public override bool PreAI()
		{
			moveTimer++;

			gravity = false;
			Projectile.tileCollide = false;

			if (moveTimer == 1)
				rand = Main.rand.NextFloat(6.28f);

			if (moveTimer < 60)
			{
				float offset = (1 - moveTimer / 60f) * ((float)Math.Cos(moveTimer / 8f * 3.14f + rand) * 30 + (float)Math.Sin(moveTimer / 14f * 3.14f + rand) * 20);
				Projectile.Center = Vector2.SmoothStep(startPos, targetPos, moveTimer / 60f) + new Vector2(0, 32 - (float)Math.Sin(moveTimer / 60f * 3.14f) * (180 + offset));
			}
			else if (moveTimer == 60)
			{
				Timer = 2;
			}

			return true;
		}
	}
}
