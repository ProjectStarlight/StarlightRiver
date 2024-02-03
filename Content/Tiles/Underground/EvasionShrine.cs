using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.CustomHooks;
using StarlightRiver.Content.Items.Misc;
using StarlightRiver.Content.Tiles.Underground.EvasionShrineBullets;
using StarlightRiver.Core.Systems.DummyTileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Content.Tiles.Underground
{
	class EvasionShrine : ShrineTile
	{
		public const int EVASION_SHRINE_TILE_WIDTH = 5;
		public const int EVASION_SHRINE_TILE_HEIGHT = 6;

		public override int DummyType => DummySystem.DummyType<EvasionShrineDummy>();

		public override string Texture => "StarlightRiver/Assets/Tiles/Underground/EvasionShrine";

		public override int ShrineTileWidth => EvasionShrine.EVASION_SHRINE_TILE_WIDTH;

		public override int ShrineTileHeight => EvasionShrine.EVASION_SHRINE_TILE_HEIGHT;

		public override void AdditionalSetup(ShrineDummy shrineDummy)
		{
			(shrineDummy as EvasionShrineDummy).lives = 4;
		}

		public override string GetHint()
		{
			return "A shrine - to which deity, you do not know, though it wields a bow. The statue's eyes seem to follow you, and strange runes dance across its pedestal.";
		}
	}

	public partial class EvasionShrineDummy : ShrineDummy, IDrawAdditive
	{
		public int maxAttacks = 15;
		public int lives;
		public List<int> attackOrder;

		public float Windup => Math.Min(1, timer / 120f);

		public override int ArenaOffsetX => -27;
		public override int ArenaSizeX => 55;
		public override int ArenaOffsetY => -30;
		public override int ArenaSizeY => 49;

		public override int ShrineTileWidth => EvasionShrine.EVASION_SHRINE_TILE_WIDTH;
		public override int ShrineTileHeight => EvasionShrine.EVASION_SHRINE_TILE_HEIGHT;

		public EvasionShrineDummy() : base(ModContent.TileType<EvasionShrine>(), EvasionShrine.EVASION_SHRINE_TILE_WIDTH * 16, EvasionShrine.EVASION_SHRINE_TILE_HEIGHT * 16) { }

		public override void Update()
		{
			if (state == SHRINE_STATE_DEFEATED)//dont run anything if this is defeated
				return;

			//this check never succeeds since the tile does not spawn dummys on the 3rd frame
			if (Parent.TileFrameX >= 10 * 18)//check file frame for this being defeated
			{
				state = SHRINE_STATE_DEFEATED;
				return;//return here so defeated shrines never run the below code even when spawning a new dummy
			}

			bool anyPlayerInRange = false;

			foreach (Player player in Main.player)
			{
				bool thisPlayerInRange = player.active && !player.DeadOrGhost && ArenaPlayer.Intersects(player.Hitbox);

				if (thisPlayerInRange && state != SHRINE_STATE_IDLE)
					player.GetModPlayer<ShrinePlayer>().EvasionShrineActive = true;

				anyPlayerInRange = anyPlayerInRange || thisPlayerInRange;
			}

			Vector3 color = new Vector3(0.15f, 0.12f, 0.2f) * 3.4f;

			Lighting.AddLight(Center + new Vector2(240, 0), color);
			Lighting.AddLight(Center + new Vector2(-240, 0), color);

			Lighting.AddLight(Center + new Vector2(240, -50), color);
			Lighting.AddLight(Center + new Vector2(-240, -50), color);

			Lighting.AddLight(Center + new Vector2(240, -100), color);
			Lighting.AddLight(Center + new Vector2(-240, -100), color);

			Lighting.AddLight(Center + new Vector2(0, -230), color);

			if (state == SHRINE_STATE_IDLE && Parent.TileFrameX >= ShrineTileWidth * 18)//if idle and frame isnt default (happens when entity is despawned while active)
			{
				SetFrame(0);
				timer = 0;
			}

			if (state != SHRINE_STATE_IDLE)
			{
				ProtectionWorld.AddRegionBySource(new Point16(ParentX, ParentY), ArenaTile);//stop calling this and call RemoveRegionBySource() when shrine is completed

				StarlightRiver.Instance.useIntenseMusic = true;
				Dust.NewDustPerfect(Center + new Vector2(Main.rand.NextFloat(-24, 24), 28), ModContent.DustType<Dusts.Glow>(), Vector2.UnitY * -Main.rand.NextFloat(2), 0, new Color(150, 30, 205) * Windup, 0.2f);

				if (Main.rand.NextBool(2))
				{
					Dust.NewDustPerfect(Center + new Vector2(-27 * 16 - 8 + 32, 96 + Main.rand.Next(-44, 44)), ModContent.DustType<Dusts.Glow>(), Vector2.UnitX * -Main.rand.NextFloat(2), 0, new Color(155, 40 + Main.rand.Next(50), 255) * Windup, 0.35f);
					Dust.NewDustPerfect(Center + new Vector2(26 * 16, 96 + Main.rand.Next(-44, 44)), ModContent.DustType<Dusts.Glow>(), Vector2.UnitX * Main.rand.NextFloat(2), 0, new Color(155, 40 + Main.rand.Next(50), 255) * Windup, 0.35f);
				}

				if (state > SHRINE_STATE_IDLE)
				{
					timer++;

					if (attackOrder is null)
					{
						attackOrder = new List<int>();

						for (int k = 0; k < 15; k++)
						{
							attackOrder.Add(k);
						}

						attackOrder = Helpers.Helper.RandomizeList<int>(attackOrder);
					}

					if (state > maxAttacks) // --- !  WIN CONDITION  ! ---
					{
						if (timer > 600)
						{
							SpawnReward();
							state = SHRINE_STATE_DEFEATED;
							SetFrame(2);
							ProtectionWorld.RemoveRegionBySource(new Point16(ParentX, ParentY));
						}

						return;
					}

					if (Main.netMode != NetmodeID.MultiplayerClient)
						SpawnObstacles((int)timer - 128);
				}
			}

			if (state == SHRINE_STATE_FAILED || lives <= 0 || !anyPlayerInRange)//Main.player.Any(n => n.active && !n.dead && Vector2.Distance(n.Center, Center) < 500) //"fail" conditions, no living Players in radius or already failing
			{
				state = SHRINE_STATE_FAILED;

				if (timer > 128)
				{
					netUpdate = true;
					timer = 128;
				}

				timer--;

				if (timer <= 0)
				{
					state = SHRINE_STATE_IDLE;
					attackOrder = null;
					ProtectionWorld.RemoveRegionBySource(new Point16(ParentX, ParentY));
				}

				return;
			}
		}

		public void SpawnObstacles(int timer)
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;

			switch (attackOrder[(int)state - 1])
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
			SawbladeSmall.timeLeftToAssign = time;
			Projectile.NewProjectile(GetSource_FromThis(), start, vel, ModContent.ProjectileType<SawbladeSmall>(), 10, 0, Owner: -1, ai0: identity);
		}

		public void SpawnDart(Vector2 start, Vector2 mid, Vector2 end, int duration)
		{
			Dart.midPointToAssign = mid;
			Dart.endPointToAssign = end;
			Dart.durationToAssign = duration;
			Projectile.NewProjectile(GetSource_FromThis(), start, Vector2.Zero, ModContent.ProjectileType<Dart>(), 7, 0, Owner: -1, ai0: identity);
		}

		public void SpawnSpear(Vector2 start, Vector2 end, int teleTime, int riseTime, int retractTime, int holdTime = 0)
		{
			Spear.endPointToAssign = end;
			Spear.riseTimeToAssign = riseTime;
			Spear.retractTimeToAssign = retractTime;
			Spear.teleTimeToASsign = teleTime;
			Spear.holdTimeToAssign = holdTime;
			Projectile.NewProjectile(GetSource_FromThis(), start, Vector2.Zero, ModContent.ProjectileType<Spear>(), 15, 0, Owner: -1, ai0: identity);
		}

		private void SpawnReward()
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;

			switch (lives)
			{
				case 4:
					Item.NewItem(GetSource_FromAI(), Hitbox, ModContent.ItemType<TarnishedRing>());
					ShrineUtils.SimulateGoldChest(this, false);
					ShrineUtils.SimulateGoldChest(this, true);
					break;
				case 3:
					Item.NewItem(GetSource_FromAI(), Hitbox, ModContent.ItemType<TarnishedRing>());
					ShrineUtils.SimulateGoldChest(this, false);
					if (Main.rand.NextBool(4))
						ShrineUtils.SimulateGoldChest(this, false);

					break;
				case 2:
					Item.NewItem(GetSource_FromAI(), Hitbox, ModContent.ItemType<TarnishedRing>());
					ShrineUtils.SimulateGoldChest(this, false);
					if (Main.rand.NextBool(4))
						ShrineUtils.SimulateWoodenChest(this);

					break;
				case 1:
					Item.NewItem(GetSource_FromAI(), Hitbox, ModContent.ItemType<TarnishedRing>());
					ShrineUtils.SimulateGoldChest(this, false);
					break;
			}
		}

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
			if (state != SHRINE_STATE_IDLE && state != SHRINE_STATE_DEFEATED)
			{
				Texture2D tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Tiles/Moonstone/GlowSmall").Value;
				var origin = new Vector2(tex.Width / 2, tex.Height);
				spriteBatch.Draw(tex, Center - Main.screenPosition + new Vector2(0, 60), default, GetBeamColor(StarlightWorld.visualTimer), 0, origin, 3.5f, 0, 0);
				spriteBatch.Draw(tex, Center - Main.screenPosition + new Vector2(10, 60), default, GetBeamColor(StarlightWorld.visualTimer + 2) * 0.8f, 0, origin, 2.5f, 0, 0);
				spriteBatch.Draw(tex, Center - Main.screenPosition + new Vector2(-10, 60), default, GetBeamColor(StarlightWorld.visualTimer + 4) * 0.8f, 0, origin, 3.2f, 0, 0);

				if (state > SHRINE_STATE_IDLE)
				{
					Texture2D fireTex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Tiles/Underground/BrazierFlame").Value;
					var frame = new Rectangle(0, 32 * (int)(Main.GameUpdateCount / 6 % 6), 16, 32);

					Vector2 leftPos = Center - Main.screenPosition + new Vector2(-248, -220);
					Vector2 leftMidPos = Center - Main.screenPosition + new Vector2(-120, -140);
					Vector2 rightMidPos = Center - Main.screenPosition + new Vector2(120, -140);
					Vector2 rightPos = Center - Main.screenPosition + new Vector2(248, -220);

					if (state > maxAttacks)
					{
						if (timer > 300)
						{
							float progress = Math.Min(1, (timer - 300) / 240f);

							leftPos = Center - Main.screenPosition + Vector2.SmoothStep(new Vector2(-248, -220), Vector2.Zero, progress);
							leftMidPos = Center - Main.screenPosition + Vector2.SmoothStep(new Vector2(-120, -140), Vector2.Zero, progress);
							rightMidPos = Center - Main.screenPosition + Vector2.SmoothStep(new Vector2(120, -140), Vector2.Zero, progress);
							rightPos = Center - Main.screenPosition + Vector2.SmoothStep(new Vector2(248, -220), Vector2.Zero, progress);
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

					var targetRect = new Rectangle((int)(Center.X - Main.screenPosition.X) - 27 * 16 - 10, (int)(Center.Y - Main.screenPosition.Y) + 48, 32, 96);
					spriteBatch.Draw(barrier, targetRect, sourceRect, new Color(155, 100, 255) * 0.6f * Windup);
					spriteBatch.Draw(barrier, targetRect, sourceRect2, new Color(85, 50, 150) * 0.5f * Windup);
					targetRect.Inflate(-15, 0);
					targetRect.Offset(15, 0);
					spriteBatch.Draw(barrier, targetRect, sourceRect2, Color.White * Windup);

					targetRect = new Rectangle((int)(Center.X - Main.screenPosition.X) + 26 * 16 - 6, (int)(Center.Y - Main.screenPosition.Y) + 48, 32, 96);
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

		public override void SafeSendExtraAI(BinaryWriter writer)
		{
			writer.Write(lives);
			writer.Write(timer);
			writer.Write(state);
		}

		public override void SafeReceiveExtraAI(BinaryReader reader)
		{
			lives = reader.ReadInt32();
			timer = reader.ReadSingle();
			state = reader.ReadSingle();
		}
	}
}