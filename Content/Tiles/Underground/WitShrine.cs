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
using StarlightRiver.Content.Tiles.Underground.WitShrineGames;

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

		public override void SafeNearbyEffects(int i, int j, bool closer)
		{
			var tile = Framing.GetTileSafely(i, j);

			if (Dummy is null)
				return;

			if (((WitShrineDummy)Dummy.modProjectile).State == 0 && tile.frameX > 36)
				tile.frameX -= 3 * 18;
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
				(Dummy.modProjectile as WitShrineDummy).Timer = 0;
				return true;
			}

			return false;
		}
	}

	public partial class WitShrineDummy : Dummy, IDrawAdditive
	{
		public runeState[,] gameBoard = new runeState[6, 6];
		public runeState[,] oldGameBoard = new runeState[6, 6];
		public WitShrineGame activeGame = null;

		public Vector2 player = Vector2.Zero;
		public Vector2 oldPlayer = Vector2.Zero;
		public int playerTimer = 0;

		public ref float Timer => ref projectile.ai[0];
		public ref float State => ref projectile.ai[1];

		public float Windup => Math.Min(1, Timer / 120f);

		private Vector2 playerCenter => projectile.Center + new Vector2(-48 * 3 + 24, -16 * 22) + Vector2.SmoothStep(player * 48, oldPlayer * 48, playerTimer / 30f);

		public enum runeState
		{
			None,
			Freindly,
			Hostile,
			HostileHidden,
			Goal
		}

		public WitShrineDummy() : base(ModContent.TileType<WitShrine>(), 3 * 16, 6 * 16) { }

		public override void Update()
		{
			if (State == 0 && Parent.frameX > 3 * 18)
			{
				ResetBoard();
				player = Vector2.Zero;
				oldPlayer = Vector2.Zero;

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

			if(State > 0)
			{
				if (Timer == 1) //setup game board
				{
					switch (State)
					{
						case 1: activeGame = new MazeGame(this); break;
						case 2: activeGame = new WinGame(this); break;
						case 3: activeGame = new MazeGame(this); break;

						case 99: activeGame = new LoseGame(this); break;

					}
					activeGame?.SetupBoard();
				}

				activeGame?.UpdateBoard();

				//temporary thing to test input? might be hard to make this feel good
				if(Main.mouseLeft && playerTimer == 0)
				{
					float xOff = Main.MouseWorld.X - playerCenter.X;
					float yOff = Main.MouseWorld.Y -playerCenter.Y;

					if (xOff > 0 && Math.Abs(xOff) > Math.Abs(yOff) && player.X < 5)
						player.X++;
					else if (yOff < 0 && Math.Abs(xOff) < Math.Abs(yOff) && player.Y > 0)
						player.Y--;
					else if (yOff > 0 && Math.Abs(xOff) < Math.Abs(yOff) && player.Y < 5)
						player.Y++;
					else if (xOff < 0 && Math.Abs(xOff) > Math.Abs(yOff) && player.X > 0)
						player.X--;

					playerTimer = 30;

					activeGame?.UpdatePlayer(player, oldPlayer);
				}

				if (playerTimer > 0) //block is sch'moovin
				{
					playerTimer--;

					for (int k = 0; k < 4; k++)
					{
						if (Main.rand.Next(2) == 0)
						{
							Vector2 pos = playerCenter + new Vector2(15, 15).RotatedBy(k / 4f * 6.28f);
							Vector2 velocity = -Vector2.Normalize(player - oldPlayer) * 0.2f;

							Dust.NewDustPerfect(pos, ModContent.DustType<Dusts.BlueStamina>(), velocity);
						}
					}
				}

				if (playerTimer == 1)
				{
					oldPlayer = player;
					oldGameBoard = (runeState[,])gameBoard.Clone();
				}
			}
		}

		public void ResetBoard()
		{
			for (int x = 0; x < gameBoard.GetLength(0); x++)
				for (int y = 0; y < gameBoard.GetLength(1); y++)
				{
					gameBoard[x, y] = runeState.None;
				}
		}

		public void WinGame()
		{
			State++;
			Timer = 0;
		}

		public void LoseGame()
		{
			State = 99;
			Timer = 0;
		}

		public void GotoGame(int index)
		{
			State = index;
			Timer = 0;
		}

		public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			var tex = ModContent.GetTexture("StarlightRiver/Assets/Tiles/Underground/WitPlayerTile");
			var basePos = projectile.Center + new Vector2(-48 * 3 + 24, -16 * 22) - Main.screenPosition;
			var targetPos = basePos + Vector2.SmoothStep(player * 48, oldPlayer * 48, playerTimer / 30f) + Vector2.One;
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
				var color = Color.Transparent;
				var color2 = Color.Transparent;
				var rand = new Random(1234758924);

				for (int x = 0; x < gameBoard.GetLength(0); x++)
					for (int y = 0; y < gameBoard.GetLength(1); y++)
					{
						int random = rand.Next(4);

						var sin = 0.5f + (float)Math.Sin(StarlightWorld.rottime + x + y) * 0.5f;

						switch (gameBoard[x, y])
						{
							case runeState.None:
								color = Color.Transparent;
								break;
							case runeState.Freindly:
								runeFrame.X = 22;
								runeFrame.Y = 22 * random;
								color = new Color(20, 50, 255);
								break;
							case runeState.Hostile:
								runeFrame.X = 0;
								runeFrame.Y = 22 * random;
								color = new Color(255, 30, 10);
								break;
							case runeState.HostileHidden:
								color = Color.Transparent;
								break;
							case runeState.Goal:
								runeFrame.X = 44;
								runeFrame.Y = 0;
								color = new Color(255, 200, 50);
								break;
							default:
								color = Color.Transparent;
								break;
						}

						switch (oldGameBoard[x, y])
						{
							case runeState.None:
								color2 = Color.Transparent;
								break;
							case runeState.Freindly:
								color2 = new Color(20, 50, 255);
								break;
							case runeState.Hostile:
								color2 = new Color(255, 30, 10);
								break;
							case runeState.HostileHidden:
								color2 = Color.Transparent;
								break;
							case runeState.Goal:
								color2 = new Color(255, 200, 50);
								break;
							default:
								color2 = Color.Transparent;
								break;
						}

						if (player == new Vector2(x, y))
							color = Color.Transparent;

						if (oldPlayer == new Vector2(x, y))
							color2 = Color.Transparent;


						float combinedAlpha = Helpers.Helper.Lerp(color.A / 255f, color2.A / 255f, playerTimer/30f);
						var colorCombined = Color.Lerp(color2, color, 1 - playerTimer / 30f);

						for (int k = 0; k < 5; k++)
						{
							var finalColor = colorCombined * (2 - k / 5f * 2) * (0.7f + 0.3f * (1 - sin)) * combinedAlpha;

							var finalPos = basePos + new Vector2(x, y) * 48 + (basePos + Main.screenPosition + new Vector2(x, y) * 48 - parallaxPoint) * (k / 4f * 0.12f) * sin;
							spriteBatch.Draw(runeTex, finalPos, runeFrame, finalColor, 0, Vector2.One * 11, 1 + (k / 4f * 0.5f), 0, 0);
						}

						Lighting.AddLight(basePos + Main.screenPosition + new Vector2(x * 48, y * 48), colorCombined.ToVector3() * 0.5f);
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
