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
using Terraria.DataStructures;

namespace StarlightRiver.Content.Tiles.Underground
{
	class WitShrine : DummyTile
	{
		public override int DummyType => ModContent.ProjectileType<WitShrineDummy>();

		public override bool Autoload(ref string name, ref string texture)
		{
			texture = "StarlightRiver/Assets/Tiles/Underground/WitShrine";
			return true;
		}

		public override void SetDefaults()
		{
			QuickBlock.QuickSetFurniture(this, 3, 6, DustID.Stone, SoundID.Tink, false, new Color(100, 100, 100), false, false, "Mysterious Shrine");
		}

		public override bool NewRightClick(int i, int j)
		{
			var tile = (Tile)(Framing.GetTileSafely(i, j).Clone());

			if ((Dummy.modProjectile as WitShrineDummy).State == 0)
			{
				for (int x = 0; x < 3; x++)
					for (int y = 0; y < 6; y++)
					{
						int realX = x + i - tile.frameX / 18;
						int realY = y + j - tile.frameY / 18;

						Framing.GetTileSafely(realX, realY).frameX += 3 * 18;
					}

				(Dummy.modProjectile as WitShrineDummy).State = 1;
				return true;
			}

			return false;
		}
	}

	internal partial class WitShrineDummy : Dummy, IDrawAdditive
	{
		private runeState[,] gameBoard = new runeState[6, 6];
		private Point16 player = Point16.Zero;

		public ref float Timer => ref projectile.ai[0];
		public ref float State => ref projectile.ai[1];

		public float Windup => Math.Min(1, Timer / 120f);

		public enum runeState
		{
			Freindly,
			Hostile,
			HostileHidden,
			Goal
		}

		public WitShrineDummy() : base(ModContent.TileType<WitShrine>(), 3 * 16, 6 * 16) { }

		public override void Update()
		{
			var color = new Vector3(0.15f, 0.12f, 0.2f) * 3.4f;

			if (State == 0 && Parent.frameX > 3 * 18)
			{
				ResetBoard();

				for (int x = 0; x < 3; x++)
					for (int y = 0; y < 6; y++)
					{
						int realX = ParentX - 1 + x;
						int realY = ParentY - 3 + y;

						Framing.GetTileSafely(realX, realY).frameX -= 3 * 18;
					}
			}

			if (State != 0)
			{
				Dust.NewDustPerfect(projectile.Center + new Vector2(Main.rand.NextFloat(-24, 24), 28), ModContent.DustType<Dusts.Glow>(), Vector2.UnitY * -Main.rand.NextFloat(2), 0, new Color(30, 80, 255) * Windup, 0.2f);
				Timer++;
			}

			if(State == 1) //game 1
			{
				if (Timer == 1) //setup game board
				{
					for (int x = 0; x < gameBoard.GetLength(0); x++)
						for (int y = 0; y < gameBoard.GetLength(1); y++)
						{
							if(Main.rand.Next(2) == 0)
								gameBoard[x, y] = runeState.Hostile;
						}
				}
			}
		}

		private void ResetBoard()
		{
			for (int x = 0; x < gameBoard.GetLength(0); x++)
				for (int y = 0; y < gameBoard.GetLength(1); y++)
				{
					gameBoard[x, y] = runeState.Freindly;
				}
		}

		public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			var tex = ModContent.GetTexture("StarlightRiver/Assets/Tiles/Underground/WitPlayerTile");
			var basePos = projectile.Center + new Vector2(-48 * 3 + 24, -16 * 22) - Main.screenPosition;
			var targetPos = basePos + player.ToVector2() * 48;
			var source = new Rectangle(0, 0, 34, 34);

			spriteBatch.Draw(tex, targetPos, source, Color.White, 0, Vector2.One * 17, 1, 0, 0);
		}

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
			if (State != 0)
			{
				var tex = ModContent.GetTexture("StarlightRiver/Assets/Tiles/Moonstone/GlowSmall");
				var origin = new Vector2(tex.Width / 2, tex.Height);
				spriteBatch.Draw(tex, projectile.Center - Main.screenPosition + new Vector2(0, 60), default, GetBeamColor(StarlightWorld.rottime), 0, origin, 3.5f, 0, 0);
				spriteBatch.Draw(tex, projectile.Center - Main.screenPosition + new Vector2(10, 60), default, GetBeamColor(StarlightWorld.rottime + 2) * 0.8f, 0, origin, 2.5f, 0, 0);
				spriteBatch.Draw(tex, projectile.Center - Main.screenPosition + new Vector2(-10, 60), default, GetBeamColor(StarlightWorld.rottime + 4) * 0.8f, 0, origin, 3.2f, 0, 0);

				var runeTex = ModContent.GetTexture("StarlightRiver/Assets/Tiles/Underground/WitRune");
				var runeFrame = new Rectangle(0, 0, 22, 22);
				var basePos = projectile.Center + new Vector2(-48 * 3 + 24, -16 * 22) - Main.screenPosition;
				var parallaxPoint = projectile.Center + new Vector2(0, -16 * 22 + 152);// - new Vector2(Main.screenWidth, Main.screenHeight) / 2;
				var color = Color.White;
				var rand = new Random(1234758924);

				for (int x = 0; x < gameBoard.GetLength(0); x++)
					for (int y = 0; y < gameBoard.GetLength(1); y++)
					{
						if (player == new Point16(x, y))
							continue;

						var sin = 0.5f + (float)Math.Sin(StarlightWorld.rottime + x + y) * 0.5f;

						switch (gameBoard[x, y])
						{
							case runeState.Freindly:
								runeFrame.X = 22;
								runeFrame.Y = 22 * rand.Next(4);
								color = Color.Cyan;
								break;
							case runeState.Hostile:
								runeFrame.X = 0;
								runeFrame.Y = 22 * rand.Next(4);
								color = Color.Red;
								break;
							case runeState.HostileHidden:
								continue;
							case runeState.Goal:
								runeFrame.X = 44;
								runeFrame.Y = 0;
								color = Color.Yellow;
								break;
							default:
								continue;
						}

						for (int k = 0; k < 5; k++)
						{
							spriteBatch.Draw(runeTex, basePos + new Vector2(x * 48, y * 48) + (basePos + Main.screenPosition + new Vector2(x * 48, y * 48) - parallaxPoint) * (k / 4f * 0.12f) * sin, runeFrame, Color.White * (1f / k * 1.5f) * (0.7f + 0.3f * (1 - sin)), 0, Vector2.One * 11, 1 + (k / 4f * 0.5f), 0, 0);
						}

						Lighting.AddLight(basePos + Main.screenPosition + new Vector2(x * 48, y * 48), color.ToVector3() * 0.5f);
					}
			}
		}

		private Color GetBeamColor(float time)
		{
			var sin = (0.5f + (float)Math.Sin(time * 2 + 1) * 0.5f);
			var sin2 = (0.5f + (float)Math.Sin(time) * 0.5f);
			return new Color(20, 80 + (int)(50 * sin), 255) * sin2 * Windup;
		}
	}
}
