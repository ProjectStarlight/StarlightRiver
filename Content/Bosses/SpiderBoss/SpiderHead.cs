using StarlightRiver.Core.Systems.ArmatureSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace StarlightRiver.Content.Bosses.SpiderBoss
{
	internal class SpiderHead : ModNPC
	{
		public Vector2 start;

		public float curveProgress;
		public int[] pathCoefficients = new int[4];

		public Arm[] legs = new Arm[44];

		public ref float Timer => ref NPC.ai[0];
		public ref float Phase => ref NPC.ai[1];
		public ref float AttackTimer => ref NPC.ai[2];
		public ref float AttackPhase => ref NPC.ai[3];

		public override string Texture => AssetDirectory.Invisible;

		public Vector2 PointOnPath(float progress)
		{
			float t = progress * 6.28f;
			float x = 500 * (float)Math.Sin(pathCoefficients[0] * t) + 500 * (float)Math.Sin(pathCoefficients[1] * t);
			float y = 500 * (float)Math.Sin(pathCoefficients[2] * t) + 500 * (float)Math.Sin(pathCoefficients[3] * t);

			return start + new Vector2(x, y);
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("[PH] Spider Head");
		}

		public override void SetDefaults()
		{
			for(int k = 0; k < 4; k++)
			{
				pathCoefficients[k] = Main.rand.Next(4, 10);
			}

			for(int k = 0; k < 44; k++)
			{
				legs[k] = new Arm(NPC.Center, 2, 16, ModContent.Request<Texture2D>(AssetDirectory.Debug).Value);
			}

			NPC.lifeMax = 10000;
			NPC.damage = 10;
			NPC.aiStyle = -1;
			NPC.noGravity = true;
			NPC.behindTiles = true;
			NPC.noTileCollide = true;
			NPC.width = 32;
			NPC.height = 32;
		}

		public override void AI()
		{
			Timer++;

			if (start == Vector2.Zero)
				start = NPC.Center;

			NPC.Center = PointOnPath(curveProgress);

			// Tunnel boring logic
			for(int x = -3; x <= 3; x++)
			{
				for(int y = -3; y <= 3; y++)
				{
					int checkX = (int)NPC.Center.X / 16 + x;
					int checkY = (int)NPC.Center.Y / 16 + y;

					if ((Math.Abs(x) > 2 || Math.Abs(y) > 2) && Framing.GetTileSafely(checkX, checkY).WallType != WallID.Wood)
					{
						WorldGen.PlaceTile(checkX, checkY, TileID.WoodBlock, true, true);
					}
					else
					{
						WorldGen.KillTile(checkX, checkY);
						Framing.GetTileSafely(checkX, checkY).WallType = WallID.Wood;
					}
				}
			}

			// Normalize crawl progress
			float dist = Vector2.Distance(NPC.Center, PointOnPath(curveProgress + 0.00025f));
			float progCoeff = Math.Min(1, 4 / dist);

			curveProgress += 0.00025f * progCoeff;

			// Aniamte legs
			int segments = 20;
			for (int k = 0; k < segments; k++)
			{
				var point = PointOnPath(curveProgress - k * 0.001f);
				float rot = point.DirectionTo(PointOnPath(curveProgress - k * 0.002f)).ToRotation();

				legs[k * 2].start = point + Vector2.UnitX.RotatedBy(rot + 1.57f) * 16;
				legs[k * 2 + 1].start = point + Vector2.UnitX.RotatedBy(rot - 1.57f) * 16;

				Vector2 offset = Vector2.UnitX.RotatedBy(Timer * 0.15f + k * 1.2f) * 12;

				legs[k * 2].IKToPoint(point + Vector2.UnitX.RotatedBy(rot + 1.57f) * 42 + offset);
				legs[k * 2 + 1].IKToPoint(point + Vector2.UnitX.RotatedBy(rot - 1.57f) * 42 + offset * -1);
			}

			foreach (Arm arm in legs)
			{
				arm.Update();
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			var tex = ModContent.Request<Texture2D>(AssetDirectory.Debug).Value;

			spriteBatch.Draw(tex, NPC.Center - screenPos, Color.Red);

			int segments = 20;
			for(int k = 0; k < segments; k++)
			{
				var point = PointOnPath(curveProgress - k * 0.001f);
				float rot = point.DirectionTo(PointOnPath(curveProgress - k * 0.002f)).ToRotation();
				spriteBatch.Draw(tex, point - screenPos, null, Color.White, rot, tex.Size() / 2f, 1, 0, 0);
			}

			foreach(Arm arm in legs)
			{
				arm.DrawArm(spriteBatch);
			}

			return true;
		}
	}
}
