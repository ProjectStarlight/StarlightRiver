using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.CustomHooks;
using StarlightRiver.Content.Items.Misc;
using StarlightRiver.Content.Tiles.Underground.EvasionShrineBullets;
using StarlightRiver.Core.Systems.DummyTileSystem;
using System;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Content.Tiles.Underground
{
	class EvasionShrine : DummyTile, IHintable
	{
		public override int DummyType => ModContent.ProjectileType<EvasionShrineDummy>();

		public override string Texture => "StarlightRiver/Assets/Tiles/Underground/EvasionShrine";

		public override void SetStaticDefaults()
		{
			QuickBlock.QuickSetFurniture(this, 5, 6, DustID.Stone, SoundID.Tink, false, new Color(100, 100, 100), false, false, "Mysterious Shrine");
			MinPick = int.MaxValue;
		}

		public override bool CanExplode(int i, int j)
		{
			return false;
		}

		public override void MouseOver(int i, int j)
		{
			Player Player = Main.LocalPlayer;
			Player.cursorItemIconID = ModContent.ItemType<Items.Hovers.GenericHover>();
			Player.noThrow = 2;
			Player.cursorItemIconEnabled = true;
		}

		public override bool SpawnConditions(int i, int j)//ensures the dummy can spawn if the tile gets stuck in the second frame
		{
			Tile tile = Main.tile[i, j];
			return (tile.TileFrameX == 0 || tile.TileFrameX == 5 * 18) && tile.TileFrameY == 0;
		}

		public override bool RightClick(int i, int j)
		{
			var tile = (Tile)Framing.GetTileSafely(i, j).Clone();

			if (tile.TileFrameX == 5 * 18)//shrine is active
			{
				return false;
			}
			else if (tile.TileFrameX >= 10 * 18)//shrine is dormant
			{
				Main.NewText("The shrine has gone dormant...", Color.DarkSlateGray);
				return false;
			}

			int x = i - tile.TileFrameX / 18;
			int y = j - tile.TileFrameY / 18;

			Projectile dummy = Dummy(x, y);

			if (dummy is null)
				return false;

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
		public string GetHint()
		{
			return "A shrine - to which deity, you do not know, though it wields a bow. The statue's eyes seem to follow you, and strange runes dance across its pedestal.";
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

		const int ArenaOffsetX = -27;
		const int ArenaSizeX = 55;
		const int ArenaOffsetY = -30;
		const int ArenaSizeY = 49;

		public Rectangle ArenaPlayer => new((ParentX + ArenaOffsetX) * 16, (ParentY + ArenaOffsetY) * 16, ArenaSizeX * 16, ArenaSizeY * 16);
		public Rectangle ArenaTile => new(ParentX + ArenaOffsetX, ParentY + ArenaOffsetY, ArenaSizeX, ArenaSizeY);

		public EvasionShrineDummy() : base(ModContent.TileType<EvasionShrine>(), 5 * 16, 6 * 16) { }

		const float ShrineState_Idle = 0;
		//const float ShrineState_Active = 1;
		const float ShrineState_Failed = -1;
		const float ShrineState_Defeated = -2;

		public override void Update()
		{
			if (State == ShrineState_Defeated)//dont run anything if this is defeated
				return;

			//this check never succeeds since the tile does not spawn dummys on the 3rd frame
			if (Parent.TileFrameX >= 10 * 18)//check file frame for this being defeated
			{
				State = ShrineState_Defeated;
				return;//return here so defeated shrines never run the below code even when spawning a new dummy
			}

			bool anyPlayerInRange = false;

			foreach (Player player in Main.player)
			{
				bool thisPlayerInRange = player.active && !player.dead && ArenaPlayer.Intersects(player.Hitbox);

				if (thisPlayerInRange && State != ShrineState_Idle)
					player.GetModPlayer<ShrinePlayer>().EvasionShrineActive = true;

				anyPlayerInRange = anyPlayerInRange || thisPlayerInRange;
			}

			Vector3 color = new Vector3(0.15f, 0.12f, 0.2f) * 3.4f;

			Lighting.AddLight(Projectile.Center + new Vector2(240, 0), color);
			Lighting.AddLight(Projectile.Center + new Vector2(-240, 0), color);

			Lighting.AddLight(Projectile.Center + new Vector2(240, -50), color);
			Lighting.AddLight(Projectile.Center + new Vector2(-240, -50), color);

			Lighting.AddLight(Projectile.Center + new Vector2(240, -100), color);
			Lighting.AddLight(Projectile.Center + new Vector2(-240, -100), color);

			Lighting.AddLight(Projectile.Center + new Vector2(0, -230), color);

			if (State == ShrineState_Idle && Parent.TileFrameX >= 5 * 18)//if idle and frame isnt default (happens when entity is despawned while active)
			{
				SetFrame(0);
				Timer = 0;
			}

			if (State != ShrineState_Idle)
			{
				ProtectionWorld.AddRegionBySource(new Point16(ParentX, ParentY), ArenaTile);//stop calling this and call RemoveRegionBySource() when shrine is completed

				(Mod as StarlightRiver).useIntenseMusic = true;
				Dust.NewDustPerfect(Projectile.Center + new Vector2(Main.rand.NextFloat(-24, 24), 28), ModContent.DustType<Dusts.Glow>(), Vector2.UnitY * -Main.rand.NextFloat(2), 0, new Color(150, 30, 205) * Windup, 0.2f);

				if (Main.rand.NextBool(2))
				{
					Dust.NewDustPerfect(Projectile.Center + new Vector2(-27 * 16 - 8 + 32, 96 + Main.rand.Next(-44, 44)), ModContent.DustType<Dusts.Glow>(), Vector2.UnitX * -Main.rand.NextFloat(2), 0, new Color(155, 40 + Main.rand.Next(50), 255) * Windup, 0.35f);
					Dust.NewDustPerfect(Projectile.Center + new Vector2(26 * 16, 96 + Main.rand.Next(-44, 44)), ModContent.DustType<Dusts.Glow>(), Vector2.UnitX * Main.rand.NextFloat(2), 0, new Color(155, 40 + Main.rand.Next(50), 255) * Windup, 0.35f);
				}

				if (State > ShrineState_Idle)
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

					if (State > maxAttacks) // --- !  WIN CONDITION  ! ---
					{
						if (Timer > 600)
						{
							SpawnReward();
							State = ShrineState_Defeated;
							SetFrame(2);
							ProtectionWorld.RemoveRegionBySource(new Point16(ParentX, ParentY));
						}

						return;
					}

					SpawnObstacles((int)Timer - 128);
				}
			}
			//else//renable this if there are issues with protection being left on
			//{	
			//	ProtectionWorld.RemoveRegionBySource(new Point16(ParentX, ParentY));
			//}

			if (State == ShrineState_Failed || lives <= 0 || !anyPlayerInRange)//Main.player.Any(n => n.active && !n.dead && Vector2.Distance(n.Center, Projectile.Center) < 500) //"fail" conditions, no living Players in radius or already failing
			{
				State = ShrineState_Failed;

				if (Timer > 128)
					Timer = 128;

				Timer--;

				if (Timer <= 0)
				{
					State = ShrineState_Idle;
					attackOrder = null;
					ProtectionWorld.RemoveRegionBySource(new Point16(ParentX, ParentY));
				}

				return;
			}
		}

		private void SetFrame(int frame)
		{
			const int tileWidth = 5;
			const int tileHeight = 6;
			for (int x = 0; x < tileWidth; x++)
			{
				for (int y = 0; y < tileHeight; y++)
				{
					int realX = ParentX - 2 + x;
					int realY = ParentY - 3 + y;

					Framing.GetTileSafely(realX, realY).TileFrameX = (short)((x + frame * tileWidth) * 18);
				}
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
					Item.NewItem(Projectile.GetSource_FromAI(), Projectile.getRect(), ModContent.ItemType<TarnishedRing>());
					ShrinePlayer.SimulateGoldChest(Projectile, false);
					ShrinePlayer.SimulateGoldChest(Projectile, true);
					break;
				case 3:
					Item.NewItem(Projectile.GetSource_FromAI(), Projectile.getRect(), ModContent.ItemType<TarnishedRing>());
					ShrinePlayer.SimulateGoldChest(Projectile, false);
					if (Main.rand.NextBool(4))
						ShrinePlayer.SimulateGoldChest(Projectile, false);

					break;
				case 2:
					Item.NewItem(Projectile.GetSource_FromAI(), Projectile.getRect(), ModContent.ItemType<TarnishedRing>());
					ShrinePlayer.SimulateGoldChest(Projectile, false);
					if (Main.rand.NextBool(4))
						ShrinePlayer.SimulateWoodenChest(Projectile);

					break;
				case 1:
					Item.NewItem(Projectile.GetSource_FromAI(), Projectile.getRect(), ModContent.ItemType<TarnishedRing>());
					ShrinePlayer.SimulateGoldChest(Projectile, false);
					break;
			}
		}

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
			if (State != ShrineState_Idle && State != ShrineState_Defeated)
			{
				Texture2D tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Tiles/Moonstone/GlowSmall").Value;
				var origin = new Vector2(tex.Width / 2, tex.Height);
				spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition + new Vector2(0, 60), default, GetBeamColor(StarlightWorld.visualTimer), 0, origin, 3.5f, 0, 0);
				spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition + new Vector2(10, 60), default, GetBeamColor(StarlightWorld.visualTimer + 2) * 0.8f, 0, origin, 2.5f, 0, 0);
				spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition + new Vector2(-10, 60), default, GetBeamColor(StarlightWorld.visualTimer + 4) * 0.8f, 0, origin, 3.2f, 0, 0);

				if (State > ShrineState_Idle)
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
}