using StarlightRiver.Content.Tiles.Underground.EvasionShrineBullets;
using StarlightRiver.Core.Systems.DummyTileSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ID;

namespace StarlightRiver.Content.Tiles.Underground
{
	class EvasionShrine : DummyTile
	{
		public override int DummyType => ModContent.ProjectileType<EvasionShrineDummy>();

		public override string Texture => "StarlightRiver/Assets/Tiles/Underground/EvasionShrine";

		public override void SetStaticDefaults()
		{
			QuickBlock.QuickSetFurniture(this, 5, 6, DustID.Stone, SoundID.Tink, false, new Color(100, 100, 100), false, false, "Mysterious Shrine");
		}

		public override void SafeNearbyEffects(int i, int j, bool closer)
		{
			Tile tile = Framing.GetTileSafely(i, j);

			if (tile.TileFrameX == 0 && tile.TileFrameY == 0)
			{
				Projectile dummy = Dummy(i, j);

				if (dummy is null)
					return;

				if (((EvasionShrineDummy)dummy.ModProjectile).State == 0 && tile.TileFrameX >= 90)
				{
					tile.TileFrameX -= 5 * 18;
					dummy.ai[0] = 0;
				}
			}
		}

		public override bool RightClick(int i, int j)
		{
			var tile = (Tile)Framing.GetTileSafely(i, j).Clone();

			int x = i - tile.TileFrameX / 18;
			int y = j - tile.TileFrameY / 18;

			Projectile dummy = Dummy(x, y);

			if ((dummy.ModProjectile as EvasionShrineDummy).State == 0)
			{
				for (int x1 = 0; x1 < 5; x1++)
				{
					for (int y1 = 0; y1 < 6; y1++)
					{
						int realX = x1 + x;
						int realY = y1 + y;

						Framing.GetTileSafely(realX, realY).TileFrameX = (short)((5 + x1) * 18);
					}
				}

				(dummy.ModProjectile as EvasionShrineDummy).Timer = 0;
				(dummy.ModProjectile as EvasionShrineDummy).State = 1;
				(dummy.ModProjectile as EvasionShrineDummy).lives = 4;
				return true;
			}

			return false;
		}
	}

	internal partial class EvasionShrineDummy : Dummy, IDrawAdditive
	{
		public int maxAttacks = 15;
		public int lives;
		public List<int> attackOrder;

		public ref float Timer => ref Projectile.ai[0];
		public ref float State => ref Projectile.ai[1];

		public float Windup => Math.Min(1, Timer / 120f);

		public Rectangle Arena => new(ParentX * 16 - 25 * 16, ParentY * 16 - 20 * 16, 51 * 16, 30 * 16);

		public EvasionShrineDummy() : base(ModContent.TileType<EvasionShrine>(), 5 * 16, 6 * 16) { }

		public override void Update()
		{
			Vector3 color = new Vector3(0.15f, 0.12f, 0.2f) * 3.4f;

			Lighting.AddLight(Projectile.Center + new Vector2(240, 0), color);
			Lighting.AddLight(Projectile.Center + new Vector2(-240, 0), color);

			Lighting.AddLight(Projectile.Center + new Vector2(240, -50), color);
			Lighting.AddLight(Projectile.Center + new Vector2(-240, -50), color);

			Lighting.AddLight(Projectile.Center + new Vector2(240, -100), color);
			Lighting.AddLight(Projectile.Center + new Vector2(-240, -100), color);

			Lighting.AddLight(Projectile.Center + new Vector2(0, -230), color);

			if (State == 0 && Parent.TileFrameX > 5 * 18)
			{
				for (int x = 0; x < 5; x++)
				{
					for (int y = 0; y < 6; y++)
					{
						int realX = ParentX - 2 + x;
						int realY = ParentY - 3 + y;

						Framing.GetTileSafely(realX, realY).TileFrameX = (short)(x * 18);

						Main.NewText(Framing.GetTileSafely(realX, realY).ToString());
					}
				}

				Timer = 0;
			}

			if (State != 0)
			{
				(Mod as StarlightRiver).useIntenseMusic = true;
				Dust.NewDustPerfect(Projectile.Center + new Vector2(Main.rand.NextFloat(-24, 24), 28), ModContent.DustType<Dusts.Glow>(), Vector2.UnitY * -Main.rand.NextFloat(2), 0, new Color(150, 30, 205) * Windup, 0.2f);

				if (Main.rand.NextBool(2))
				{
					Dust.NewDustPerfect(Projectile.Center + new Vector2(-27 * 16 - 8 + 32, 96 + Main.rand.Next(-44, 44)), ModContent.DustType<Dusts.Glow>(), Vector2.UnitX * -Main.rand.NextFloat(2), 0, new Color(155, 40 + Main.rand.Next(50), 255) * Windup, 0.35f);
					Dust.NewDustPerfect(Projectile.Center + new Vector2(26 * 16, 96 + Main.rand.Next(-44, 44)), ModContent.DustType<Dusts.Glow>(), Vector2.UnitX * Main.rand.NextFloat(2), 0, new Color(155, 40 + Main.rand.Next(50), 255) * Windup, 0.35f);
				}

				if (State > 0)
				{
					Timer++;

					if (attackOrder is null)
					{
						attackOrder = new List<int>();

						for (int k = 0; k < 15; k++)
						{
							attackOrder.Add(k);
						}

						attackOrder = Helpers.Helper.RandomizeList<int>(attackOrder);
					}

					if (State > maxAttacks)
					{
						if (Timer > 600)
							State = -1;

						return;
					}

					SpawnObstacles((int)Timer - 128);
				}
			}

			if (State == -1 || lives <= 0 || !Main.player.Any(n => n.active && !n.dead && Vector2.Distance(n.Center, Projectile.Center) < 500)) //"fail" conditions, no living Players in radius or already failing
			{
				State = -1;

				if (Timer > 128)
					Timer = 128;

				Timer--;

				if (Timer <= 0)
				{
					State = 0;
					attackOrder = null;
				}

				return;
			}
		}

		public void SpawnObstacles(int timer)
		{
			switch (attackOrder[(int)State - 1])
			{
				case 0: VerticalSawJaws(timer); break;
				case 1: HorizontalSawJaws(timer); break;
				case 2: DartBurst(timer); break;
				case 3: SpearsAndSwooshes(timer); break;
				case 4: TopSpearsBottomDarts(timer); break;
				case 5: MiddleSqueeze(timer); break;
				case 6: ShooFromMiddle(timer); break;
				case 7: SideSqueeze(timer); break;
				case 8: CruelDarts(timer); break;
				case 9: SquareSpears(timer); break;
				case 10: DartBurst2(timer); break;
				default: EndAttack(); break;
			}
		}

		public void SpawnBlade(Vector2 start, Vector2 vel, int time)
		{
			int i = Projectile.NewProjectile(Projectile.GetSource_FromThis(), start, vel, ModContent.ProjectileType<SawbladeSmall>(), 10, 0, Main.myPlayer);
			var mp = Main.projectile[i].ModProjectile as SawbladeSmall;
			Main.projectile[i].timeLeft = time;
			mp.parent = this;
		}

		public void SpawnDart(Vector2 start, Vector2 mid, Vector2 end, int duration)
		{
			int i = Projectile.NewProjectile(Projectile.GetSource_FromThis(), start, Vector2.Zero, ModContent.ProjectileType<Dart>(), 7, 0, Main.myPlayer);
			var mp = Main.projectile[i].ModProjectile as Dart;
			mp.endPoint = end;
			mp.midPoint = mid;
			mp.duration = duration;
			mp.parent = this;
		}

		public void SpawnSpear(Vector2 start, Vector2 end, int teleTime, int riseTime, int retractTime, int holdTime = 0)
		{
			int i = Projectile.NewProjectile(Projectile.GetSource_FromThis(), start, Vector2.Zero, ModContent.ProjectileType<Spear>(), 15, 0, Main.myPlayer);
			var mp = Main.projectile[i].ModProjectile as Spear;
			mp.endPoint = end;
			mp.timeToRise = riseTime;
			mp.timeToRetract = retractTime;
			mp.teleTime = teleTime;
			mp.holdTime = holdTime;
			mp.parent = this;
		}

		private void SpawnReward()
		{
			switch (lives)
			{
				case 4:
					break;
				case 3:
					break;
				case 2:
					break;
				case 1:
					break;
				default:
					break;
			}
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

				if (State > 0)
				{
					Texture2D fireTex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Tiles/Underground/BrazierFlame").Value;
					var frame = new Rectangle(0, 32 * (int)(Main.GameUpdateCount / 6 % 6), 16, 32);

					Vector2 leftPos = Projectile.Center - Main.screenPosition + new Vector2(-248, -220);
					Vector2 leftMidPos = Projectile.Center - Main.screenPosition + new Vector2(-120, -140);
					Vector2 rightMidPos = Projectile.Center - Main.screenPosition + new Vector2(120, -140);
					Vector2 rightPos = Projectile.Center - Main.screenPosition + new Vector2(248, -220);

					if (State > maxAttacks)
					{
						if (Timer > 300)
						{
							float progress = Math.Min(1, (Timer - 300) / 240f);

							leftPos = Projectile.Center - Main.screenPosition + Vector2.SmoothStep(new Vector2(-248, -220), Vector2.Zero, progress);
							leftMidPos = Projectile.Center - Main.screenPosition + Vector2.SmoothStep(new Vector2(-120, -140), Vector2.Zero, progress);
							rightMidPos = Projectile.Center - Main.screenPosition + Vector2.SmoothStep(new Vector2(120, -140), Vector2.Zero, progress);
							rightPos = Projectile.Center - Main.screenPosition + Vector2.SmoothStep(new Vector2(248, -220), Vector2.Zero, progress);
						}
					}

					if (lives > 0) //kinda gross lol, should probably figure a better way of doing this
					{
						spriteBatch.Draw(fireTex, leftPos, frame, new Color(200, 100, 255), 0, fireTex.Size() / 2, 1, 0, 0);
						spriteBatch.Draw(fireTex, leftPos, frame, Color.White, 0, fireTex.Size() / 2, 0.95f, 0, 0);
					}

					if (lives > 1)
					{
						spriteBatch.Draw(fireTex, leftMidPos, frame, new Color(200, 100, 255), 0, fireTex.Size() / 2, 1, 0, 0);
						spriteBatch.Draw(fireTex, leftMidPos, frame, Color.White, 0, fireTex.Size() / 2, 0.95f, 0, 0);
					}

					if (lives > 2)
					{
						spriteBatch.Draw(fireTex, rightMidPos, frame, new Color(200, 100, 255), 0, fireTex.Size() / 2, 1, 0, 0);
						spriteBatch.Draw(fireTex, rightMidPos, frame, Color.White, 0, fireTex.Size() / 2, 0.95f, 0, 0);
					}

					if (lives > 3)
					{
						spriteBatch.Draw(fireTex, rightPos, frame, new Color(200, 100, 255), 0, fireTex.Size() / 2, 1, 0, 0);
						spriteBatch.Draw(fireTex, rightPos, frame, Color.White, 0, fireTex.Size() / 2, 0.95f, 0, 0);
					}

					Texture2D barrier = ModContent.Request<Texture2D>("StarlightRiver/Assets/MotionTrail").Value;
					var sourceRect = new Rectangle(0, (int)(Main.GameUpdateCount * 0.4f), barrier.Width, barrier.Height);
					var sourceRect2 = new Rectangle(0, (int)(Main.GameUpdateCount * -0.73f), barrier.Width, barrier.Height);

					var targetRect = new Rectangle((int)(Projectile.Center.X - Main.screenPosition.X) - 27 * 16 - 10, (int)(Projectile.Center.Y - Main.screenPosition.Y) + 48, 32, 96);
					spriteBatch.Draw(barrier, targetRect, sourceRect, new Color(155, 100, 255) * 0.6f * Windup);
					spriteBatch.Draw(barrier, targetRect, sourceRect2, new Color(85, 50, 150) * 0.5f * Windup);
					targetRect.Inflate(-15, 0);
					targetRect.Offset(15, 0);
					spriteBatch.Draw(barrier, targetRect, sourceRect2, Color.White * Windup);

					targetRect = new Rectangle((int)(Projectile.Center.X - Main.screenPosition.X) + 26 * 16 - 6, (int)(Projectile.Center.Y - Main.screenPosition.Y) + 48, 32, 96);
					spriteBatch.Draw(barrier, targetRect, sourceRect, new Color(155, 100, 255) * 0.6f * Windup, 0, default, SpriteEffects.FlipHorizontally, 0);
					spriteBatch.Draw(barrier, targetRect, sourceRect2, new Color(85, 50, 150) * 0.5f * Windup, 0, default, SpriteEffects.FlipHorizontally, 0);
					targetRect.Inflate(-15, 0);
					targetRect.Offset(-15, 0);
					spriteBatch.Draw(barrier, targetRect, sourceRect2, Color.White * Windup);
				}
			}
		}

		private Color GetBeamColor(float time)
		{
			float sin = 0.5f + (float)Math.Sin(time * 2 + 1) * 0.5f;
			float sin2 = 0.5f + (float)Math.Sin(time) * 0.5f;
			return new Color(80 + (int)(50 * sin), 60, 255) * sin2 * Windup;
		}
	}

	class EvasionShrineBiome : ModBiome
	{
		public override SceneEffectPriority Priority => SceneEffectPriority.BossLow;

		public override int Music => MusicLoader.GetMusicSlot("StarlightRiver/Sounds/Music/EvasionShrine");

		public override bool IsBiomeActive(Player player)
		{
			return Main.projectile.Any(
				n => n.active &&
				n.type == ModContent.ProjectileType<EvasionShrineDummy>() &&
				(n.ModProjectile as EvasionShrineDummy).Arena.Intersects(player.Hitbox) &&
				(n.ModProjectile as EvasionShrineDummy).State != 0);
		}
	}
}