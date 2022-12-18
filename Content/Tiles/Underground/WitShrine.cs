using StarlightRiver.Content.Tiles.Underground.WitShrineGames;
using StarlightRiver.Core.Systems.DummyTileSystem;
using System;
using Terraria.ID;

namespace StarlightRiver.Content.Tiles.Underground
{
	class WitShrine : DummyTile
	{
		public override int DummyType => ModContent.ProjectileType<WitShrineDummy>();

		public override string Texture => "StarlightRiver/Assets/Tiles/Underground/WitShrine";

		public override void SetStaticDefaults()
		{
			QuickBlock.QuickSetFurniture(this, 3, 6, DustID.Stone, SoundID.Tink, false, new Color(100, 100, 100), false, false, "Mysterious Shrine");
		}

		public override void SafeNearbyEffects(int i, int j, bool closer)
		{
			Tile tile = Framing.GetTileSafely(i, j);

			if (tile.TileFrameX == 0 && tile.TileFrameY == 0)
			{
				Projectile dummy = Dummy(i, j);

				if (dummy is null)
					return;

				if (((WitShrineDummy)dummy.ModProjectile).State == 0 && tile.TileFrameX > 36)
					tile.TileFrameX -= 3 * 18;
			}
		}

		public override bool RightClick(int i, int j)
		{
			var tile = (Tile)Framing.GetTileSafely(i, j).Clone();

			int x = i - tile.TileFrameX / 16;
			int y = j - tile.TileFrameY / 16;

			Projectile dummy = Dummy(x, y);

			if ((dummy.ModProjectile as WitShrineDummy).State == 0)
			{
				for (int x1 = 0; x1 < 3; x1++)
				{
					for (int y1 = 0; y1 < 6; y1++)
					{
						int realX = x1 + i - tile.TileFrameX / 18;
						int realY = y1 + j - tile.TileFrameY / 18;

						Framing.GetTileSafely(realX, realY).TileFrameX += 3 * 18;
					}
				}

				(dummy.ModProjectile as WitShrineDummy).State = 1;
				(dummy.ModProjectile as WitShrineDummy).Timer = 0;
				return true;
			}

			return false;
		}
	}

	public partial class WitShrineDummy : Dummy, IDrawAdditive
	{
		public enum runeState
		{
			None,
			Freindly,
			Hostile,
			HostileHidden,
			Goal
		}

		public runeState[,] gameBoard = new runeState[6, 6];
		public runeState[,] oldGameBoard = new runeState[6, 6];
		public WitShrineGame activeGame = null;

		public Vector2 Player = Vector2.Zero;
		public Vector2 oldPlayer = Vector2.Zero;
		public int PlayerTimer = 0;

		public ref float Timer => ref Projectile.ai[0];
		public ref float State => ref Projectile.ai[1];

		public float Windup => Math.Min(1, Timer / 120f);

		private Vector2 PlayerCenter => Projectile.Center + new Vector2(-48 * 3 + 24, -16 * 22) + Vector2.SmoothStep(Player * 48, oldPlayer * 48, PlayerTimer / 30f);

		public WitShrineDummy() : base(ModContent.TileType<WitShrine>(), 3 * 16, 6 * 16) { }

		public override void Update()
		{
			if (State == 0 && Parent.TileFrameX > 3 * 18)
			{
				ResetBoard();
				Player = Vector2.Zero;
				oldPlayer = Vector2.Zero;

				for (int x = 0; x < 3; x++)
				{
					for (int y = 0; y < 6; y++)
					{
						int realX = ParentX - 1 + x;
						int realY = ParentY - 3 + y;

						Framing.GetTileSafely(realX, realY).TileFrameX -= 3 * 18;
					}
				}
			}

			if (State != 0)
			{
				Dust.NewDustPerfect(Projectile.Center + new Vector2(Main.rand.NextFloat(-24, 24), 28), ModContent.DustType<Dusts.Glow>(), Vector2.UnitY * -Main.rand.NextFloat(2), 0, new Color(30, 80, 255) * Windup, 0.2f);
				Timer++;
			}

			if (State > 0)
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
				if (Main.mouseLeft && PlayerTimer == 0)
				{
					float xOff = Main.MouseWorld.X - PlayerCenter.X;
					float yOff = Main.MouseWorld.Y - PlayerCenter.Y;

					if (xOff > 0 && Math.Abs(xOff) > Math.Abs(yOff) && Player.X < 5)
						Player.X++;
					else if (yOff < 0 && Math.Abs(xOff) < Math.Abs(yOff) && Player.Y > 0)
						Player.Y--;
					else if (yOff > 0 && Math.Abs(xOff) < Math.Abs(yOff) && Player.Y < 5)
						Player.Y++;
					else if (xOff < 0 && Math.Abs(xOff) > Math.Abs(yOff) && Player.X > 0)
						Player.X--;

					PlayerTimer = 30;

					activeGame?.UpdatePlayer(Player, oldPlayer);
				}

				if (PlayerTimer > 0) //block is sch'moovin
				{
					PlayerTimer--;

					for (int k = 0; k < 4; k++)
					{
						if (Main.rand.NextBool(2))
						{
							Vector2 pos = PlayerCenter + new Vector2(15, 15).RotatedBy(k / 4f * 6.28f);
							Vector2 velocity = -Vector2.Normalize(Player - oldPlayer) * 0.2f;

							Dust.NewDustPerfect(pos, ModContent.DustType<Dusts.BlueStamina>(), velocity);
						}
					}
				}

				if (PlayerTimer == 1)
				{
					oldPlayer = Player;
					oldGameBoard = (runeState[,])gameBoard.Clone();
				}
			}
		}

		public void ResetBoard()
		{
			for (int x = 0; x < gameBoard.GetLength(0); x++)
			{
				for (int y = 0; y < gameBoard.GetLength(1); y++)
				{
					gameBoard[x, y] = runeState.None;
				}
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

		public override void PostDraw(Color lightColor)
		{
			Texture2D tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Tiles/Underground/WitPlayerTile").Value;
			Vector2 basePos = Projectile.Center + new Vector2(-48 * 3 + 24, -16 * 22) - Main.screenPosition;
			Vector2 targetPos = basePos + Vector2.SmoothStep(Player * 48, oldPlayer * 48, PlayerTimer / 30f) + Vector2.One;
			var source = new Rectangle(0, 0, 34, 34);

			Main.spriteBatch.Draw(tex, targetPos, source, Color.White, 0, Vector2.One * 17, 1, 0, 0);
		}

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
			if (State != 0)
			{
				Texture2D tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Tiles/Moonstone/GlowSmall").Value;
				var origin = new Vector2(tex.Width / 2, tex.Height);
				spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition + new Vector2(0, 60), default, GetBeamColor(StarlightWorld.visualTimer), 0, origin, 3.5f, 0, 0);
				spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition + new Vector2(10, 60), default, GetBeamColor(StarlightWorld.visualTimer + 2) * 0.8f, 0, origin, 2.5f, 0, 0);
				spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition + new Vector2(-10, 60), default, GetBeamColor(StarlightWorld.visualTimer + 4) * 0.8f, 0, origin, 3.2f, 0, 0);

				Texture2D runeTex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Tiles/Underground/WitRune").Value;
				var runeFrame = new Rectangle(0, 0, 22, 22);
				Vector2 basePos = Projectile.Center + new Vector2(-48 * 3 + 24, -16 * 22) - Main.screenPosition;
				Vector2 parallaxPoint = Projectile.Center + new Vector2(0, -16 * 22 + 152);// - new Vector2(Main.screenWidth, Main.screenHeight) / 2;
				Color color = Color.Transparent;
				Color color2 = Color.Transparent;
				var rand = new Random(1234758924);

				for (int x = 0; x < gameBoard.GetLength(0); x++)
				{
					for (int y = 0; y < gameBoard.GetLength(1); y++)
					{
						int random = rand.Next(4);

						float sin = 0.5f + (float)Math.Sin(StarlightWorld.visualTimer + x + y) * 0.5f;

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

						color2 = oldGameBoard[x, y] switch
						{
							runeState.None => Color.Transparent,
							runeState.Freindly => new Color(20, 50, 255),
							runeState.Hostile => new Color(255, 30, 10),
							runeState.HostileHidden => Color.Transparent,
							runeState.Goal => new Color(255, 200, 50),
							_ => Color.Transparent,
						};

						if (Player == new Vector2(x, y))
							color = Color.Transparent;

						if (oldPlayer == new Vector2(x, y))
							color2 = Color.Transparent;

						float combinedAlpha = Helpers.Helper.Lerp(color.A / 255f, color2.A / 255f, PlayerTimer / 30f);
						var colorCombined = Color.Lerp(color2, color, 1 - PlayerTimer / 30f);

						for (int k = 0; k < 5; k++)
						{
							Color finalColor = colorCombined * (2 - k / 5f * 2) * (0.7f + 0.3f * (1 - sin)) * combinedAlpha;

							Vector2 finalPos = basePos + new Vector2(x, y) * 48 + (basePos + Main.screenPosition + new Vector2(x, y) * 48 - parallaxPoint) * (k / 4f * 0.12f) * sin;
							spriteBatch.Draw(runeTex, finalPos, runeFrame, finalColor, 0, Vector2.One * 11, 1 + k / 4f * 0.5f, 0, 0);
						}

						Lighting.AddLight(basePos + Main.screenPosition + new Vector2(x * 48, y * 48), colorCombined.ToVector3() * 0.5f);
					}
				}
			}
		}

		private Color GetBeamColor(float time)
		{
			float sin = 0.5f + (float)Math.Sin(time * 2 + 1) * 0.5f;
			float sin2 = 0.5f + (float)Math.Sin(time) * 0.5f;
			return new Color(20, 80 + (int)(50 * sin), 255) * sin2 * Windup;
		}
	}
}
