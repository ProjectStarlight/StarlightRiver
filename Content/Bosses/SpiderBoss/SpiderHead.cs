using StarlightRiver.Content.Tiles.Spider;
using StarlightRiver.Core.Systems.ArmatureSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace StarlightRiver.Content.Bosses.SpiderBoss
{
	internal struct TeleportPoint
	{
		public Vector2 pos;
		public float progress;

		public TeleportPoint(Vector2 pos, float progress)
		{
			this.pos = pos;
			this.progress = progress;
		}
	}

	internal class SpiderHead : ModNPC
	{
		public Vector2 start;

		public float curveProgress;
		public int[] pathCoefficients = new int[4];
		public List<TeleportPoint> teleportPoints = new(0);

		public TeleportPoint teleStart;
		public TeleportPoint teleEnd;
		public bool teleporting;

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
			NPCID.Sets.TrailCacheLength[NPC.type] = 200;
			NPCID.Sets.TrailingMode[NPC.type] = 1;
		}

		public override void SetDefaults()
		{
			for(int k = 0; k < 4; k++)
			{
				pathCoefficients[k] = Main.rand.Next(4, 10);
			}

			for(int k = 0; k < 44; k++)
			{
				legs[k] = new Arm(NPC.Center, 2, 24, ModContent.Request<Texture2D>(AssetDirectory.SpiderBoss + "SpiderLeg").Value);
			}

			NPC.lifeMax = 10000;
			NPC.damage = 10;
			NPC.aiStyle = -1;
			NPC.noGravity = true;
			NPC.behindTiles = true;
			NPC.noTileCollide = true;
			NPC.width = 32;
			NPC.height = 32;
			NPC.boss = true;
		}

		public override bool CheckActive()
		{
			return false;
		}

		public override void AI()
		{
			Timer++;
			AttackTimer++;

			if (start == Vector2.Zero)
			{
				// Set start point at spawn
				start = NPC.Center;

				// Set up teleport points every 1/20th of the way along the curve
				for (int k = 0; k < 20; k++)
				{
					float prog = k / 20f;
					teleportPoints.Add(new(PointOnPath(prog), prog));
				}

				// Eliminate super close teleport points, priority to higher indicies
				List<TeleportPoint> toRemove = new();

				foreach (TeleportPoint point in teleportPoints)
				{
					if (teleportPoints.Any(n => n.pos != point.pos && Vector2.Distance(n.pos, point.pos) < 32))
						toRemove.Add(point);
				}

				foreach (TeleportPoint point in toRemove)
				{
					teleportPoints.Remove(point);
				}
			}

			NPC.Center = PointOnPath(curveProgress);

			// Tunnel boring logic
			int tileType = Mod.Find<ModTile>("SpiderCave").Type;
			int wallType = ModContent.WallType<SpiderCaveWall>();

			for(int x = -3; x <= 3; x++)
			{
				for(int y = -3; y <= 3; y++)
				{
					int checkX = (int)NPC.Center.X / 16 + x;
					int checkY = (int)NPC.Center.Y / 16 + y;

					if ((Math.Abs(x) > 2 || Math.Abs(y) > 2) && Framing.GetTileSafely(checkX, checkY).WallType != wallType)
					{
						WorldGen.PlaceTile(checkX, checkY, tileType, true, true);
					}
					else
					{
						WorldGen.KillTile(checkX, checkY);
						Framing.GetTileSafely(checkX, checkY).WallType = (ushort)wallType;
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
				var point = NPC.oldPos[k * 10] + NPC.Size / 2f;

				if (point.X == 16)
					continue;

				var last = k == 0 ? NPC.Center : NPC.oldPos[(k - 1) * 10] + NPC.Size / 2f;
				float rot = point.DirectionTo(last).ToRotation();

				legs[k * 2].start = point + Vector2.UnitX.RotatedBy(rot + 1.57f) * 16;
				legs[k * 2 + 1].start = point + Vector2.UnitX.RotatedBy(rot - 1.57f) * 16;

				Vector2 offset = Vector2.UnitX.RotatedBy(Timer * 0.15f + k * 1.2f) * 18;
				Vector2 offset2 = Vector2.UnitX.RotatedBy(-Timer * 0.15f - (k + 10) * 1.2f) * 18;

				legs[k * 2].IKToPoint(point + Vector2.UnitX.RotatedBy(rot + 1.57f) * 52 + offset);
				legs[k * 2 + 1].IKToPoint(point + Vector2.UnitX.RotatedBy(rot - 1.57f) * 52 + offset2);
			}

			foreach (Arm arm in legs)
			{
				arm.Update();
			}

			// Teleport test
			if (curveProgress > 1)
			{
				foreach(TeleportPoint point in teleportPoints)
				{
					if (Vector2.Distance(NPC.Center, point.pos) < 16 && point.pos != teleEnd.pos)
					{
						teleporting = true;
						teleStart = point;
						teleEnd = teleportPoints[Main.rand.Next(teleportPoints.Count)];
						AttackTimer = 0;
					}
				}
			}

			// Teleport
			if (teleporting)
			{
				if (AttackTimer <= 30)
				{
					NPC.Center = teleStart.pos;
					curveProgress = 1 + teleStart.progress;
				}

				if (AttackTimer > 30)
				{
					NPC.Center = teleEnd.pos;
					curveProgress = 1 + teleEnd.progress;
				}

				if (AttackTimer == 60)
				{
					teleporting = false;
					AttackTimer = 0;
				}
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			var tex = ModContent.Request<Texture2D>(AssetDirectory.Debug).Value;

			foreach(TeleportPoint point in teleportPoints)
			{
				spriteBatch.Draw(tex, point.pos - screenPos, null, Color.Green, 0, tex.Size() / 2f, 1, 0, 0);
			}

			spriteBatch.Draw(tex, NPC.Center - screenPos, Color.Red);

			int segments = 20;
			for(int k = 0; k < segments; k++)
			{
				var point = NPC.oldPos[k * 10] + NPC.Size / 2f;

				if (point.X == 16)
					continue;

				var last = k == 0 ? NPC.Center : NPC.oldPos[(k - 1) * 10] + NPC.Size / 2f;
				float rot = point.DirectionTo(last).ToRotation();

				spriteBatch.Draw(tex, point - screenPos, null, Lighting.GetColor((point / 16).ToPoint()), rot, tex.Size() / 2f, 1, 0, 0);
			}

			foreach(Arm arm in legs)
			{
				arm.DrawArm(spriteBatch);
			}

			return true;
		}
	}
}
