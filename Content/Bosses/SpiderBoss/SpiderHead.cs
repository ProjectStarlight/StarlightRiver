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
		public int[] pathCoefficients = new int[4];

		public ref float Timer => ref NPC.ai[0];
		public ref float Phase => ref NPC.ai[1];
		public ref float AttackTimer => ref NPC.ai[2];
		public ref float AttackPhase => ref NPC.ai[3];

		public override string Texture => AssetDirectory.Invisible;

		public Vector2 PointOnPath(float progress)
		{
			float t = progress * 6.28f;
			float x = 300 * (float)Math.Sin(pathCoefficients[0] * t) + 300 * (float)Math.Sin(pathCoefficients[1] * t);
			float y = 300 * (float)Math.Sin(pathCoefficients[2] * t) + 300 * (float)Math.Sin(pathCoefficients[3] * t);

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

			NPC.Center = PointOnPath(Timer % 600 / 600f);

			for(int x = -4; x <= 4; x++)
			{
				for(int y = -4; y <= 4; y++)
				{
					int checkX = (int)NPC.Center.X / 16 + x;
					int checkY = (int)NPC.Center.Y / 16 + y;



					if ((Math.Abs(x) > 2 || Math.Abs(y) > 2) && Framing.GetTileSafely(checkX, checkY).WallType != WallID.Wood)
					{
						WorldGen.PlaceTile(checkX, checkY, TileID.WoodBlock, false, true);
					}
					else
					{
						WorldGen.KillTile(checkX, checkY);
						Framing.GetTileSafely(checkX, checkY).WallType = WallID.Wood;
					}
				}
			}
		}
	}
}
