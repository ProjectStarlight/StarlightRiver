using StarlightRiver.Core.Systems;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Core.Systems.DummyTileSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using StarlightRiver.Content.CustomHooks;
using StarlightRiver.Content.Items.Misc;

namespace StarlightRiver.Content.Tiles.Underground
{
	class CombatShrine : DummyTile, IHintable
	{
		public override int DummyType => ModContent.ProjectileType<CombatShrineDummy>();

		public override string Texture => "StarlightRiver/Assets/Tiles/Underground/CombatShrine";

		public override void SetStaticDefaults()
		{
			QuickBlock.QuickSetFurniture(this, 3, 6, DustID.Stone, SoundID.Tink, false, new Color(100, 100, 100), false, false, "Mysterious Shrine");
		}

		//public override void SafeNearbyEffects(int i, int j, bool closer)//does not do anything?
		//{
		//	Tile tile = Framing.GetTileSafely(i, j);

		//	if (tile.TileFrameX == 0 && tile.TileFrameY == 0)
		//	{
		//		Projectile dummy = Dummy(i, j);

		//		if (dummy is null)
		//			return;
		//	}
		//}

		public override bool SpawnConditions(int i, int j)//ensures the dummy can spawn if the tile gets stuck in the second frame
		{
			Tile tile = Main.tile[i, j];
			return (tile.TileFrameX == 0 || tile.TileFrameX == 3 * 18) && tile.TileFrameY == 0;
		}


		public override bool RightClick(int i, int j)
		{
			Tile tile = Framing.GetTileSafely(i, j);

			if (tile.TileFrameX == 3 * 18)//shrine is active
			{
				return false;
			}
			else if (tile.TileFrameX >= 6 * 18)//shrine is dormant
			{
				Main.NewText("The shrine has gone dormant...", Color.DarkSlateGray);
				return false;
			}

			int x = i - tile.TileFrameX / 18;
			int y = j - tile.TileFrameY / 18;

			Projectile dummy = Dummy(x, y);

			if (dummy is null)
				return false;

			if ((dummy.ModProjectile as CombatShrineDummy).State == 0)
			{
				for (int x1 = 0; x1 < 3; x1++)
				{
					for (int y1 = 0; y1 < 6; y1++)
					{
						int realX = x1 + x;
						int realY = y1 + y;

						Framing.GetTileSafely(realX, realY).TileFrameX = (short)((3 + x1) * 18);
					}
				}

				(dummy.ModProjectile as CombatShrineDummy).State = 1;
				return true;
			}

			return false;
		}
		public string GetHint()
		{
			return "A shrine - to which deity, you do not know, though it wields a blade. The statue's eyes seem to follow you, and strange runes dance across its pedestal.";
		}
	}

	[SLRDebug]
	class CombatShrineItem : QuickTileItem
	{
		public CombatShrineItem() : base("Combat shrine placer", "debug item", "CombatShrine") { }
	}

	class CombatShrineDummy : Dummy, IDrawAdditive
	{
		public List<NPC> minions = new();

		public int maxWaves = 6;
		private int waveTime = 0;

		public ref float Timer => ref Projectile.ai[0];
		public ref float State => ref Projectile.ai[1];

		public float Windup => Math.Min(1, Timer / 120f);

		const int ArenaOffsetX = -25;
		const int ArenaSizeX = 51;
		const int ArenaOffsetY = -19;
		const int ArenaSizeY = 26;

		public Rectangle ArenaPlayer => new((ParentX + ArenaOffsetX) * 16, (ParentY + ArenaOffsetY) * 16, ArenaSizeX * 16, ArenaSizeY * 16);
		public Rectangle ArenaTile => new(ParentX + ArenaOffsetX, ParentY + ArenaOffsetY, ArenaSizeX, ArenaSizeY);

		public CombatShrineDummy() : base(ModContent.TileType<CombatShrine>(), 3 * 16, 6 * 16) { }

		const float ShrineState_Idle = 0;
		//const float ShrineState_Active = 1;
		const float ShrineState_Failed = -1;
		const float ShrineState_Defeated = -2;

		public override void Update()
		{
			if (State == ShrineState_Defeated)//dont run anything if this is defeated
				return;

			//this check never succeeds since the tile does not spawn dummys on the 3rd frame
			if (Parent.TileFrameX >= 6 * 18)//check file frame for this being defeated
			{
				State = ShrineState_Defeated;
				return;//return here so defeated shrines never run the below code even when spawning a new dummy
			}

			bool anyPlayerInRange = false;

			foreach (Player player in Main.player)
			{
				bool thisPlayerInRange = player.active && !player.dead && ArenaPlayer.Intersects(player.Hitbox);

				if (thisPlayerInRange && State != ShrineState_Idle)
					player.GetModPlayer<ShrinePlayer>().CombatShrineActive = true;

				anyPlayerInRange = anyPlayerInRange || thisPlayerInRange;
			}

			if (State == ShrineState_Idle && Parent.TileFrameX >= 3 * 18)//if idle and frame isnt default (happens when entity is despawned while active)
			{
				SetFrame(0);
				return;
			}

			if (State != ShrineState_Idle)//this does not need a defeated check because of the above one
			{
				ProtectionWorld.AddRegionBySource(new Point16(ParentX, ParentY), ArenaTile);//stop calling this and call RemoveRegionBySource() when shrine is completed

				(Mod as StarlightRiver).useIntenseMusic = true;
				Dust.NewDustPerfect(Projectile.Center + new Vector2(Main.rand.NextFloat(-24, 24), 28), ModContent.DustType<Dusts.Glow>(), Vector2.UnitY * -Main.rand.NextFloat(2), 0, new Color(255, 40 + Main.rand.Next(50), 75) * Windup, 0.2f);

				if (Main.rand.NextBool(2))
				{
					Dust.NewDustPerfect(Projectile.Center + new Vector2(-25 * 16 - 8 + 32, 24 + Main.rand.Next(-40, 40)), ModContent.DustType<Dusts.Glow>(), Vector2.UnitX * -Main.rand.NextFloat(2), 0, new Color(255, 40 + Main.rand.Next(50), 75) * Windup, 0.35f);
					Dust.NewDustPerfect(Projectile.Center + new Vector2(24 * 16, 24 + Main.rand.Next(-40, 40)), ModContent.DustType<Dusts.Glow>(), Vector2.UnitX * Main.rand.NextFloat(2), 0, new Color(255, 40 + Main.rand.Next(50), 75) * Windup, 0.35f);
				}

				if (State == ShrineState_Failed || !anyPlayerInRange) //"fail" conditions, no living Players in radius or already failing
				{
					State = ShrineState_Failed;

					if (Timer > 128)
						Timer = 128;

					Timer--;

					if (Timer <= 0)
					{
						State = ShrineState_Idle;
						waveTime = 0;

						foreach (NPC NPC in minions)
							NPC.active = false;

						minions.Clear();
						ProtectionWorld.RemoveRegionBySource(new Point16(ParentX, ParentY));
					}

					return;
				}

				Timer++;

				if (State == maxWaves + 2)
				{
					if (Timer - waveTime >= 128)// --- !  WIN CONDITION  ! ---
					{
						for (int k = 0; k < 30; k++)
							Dust.NewDustPerfect(Projectile.Center + new Vector2(0, -32), ModContent.DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(5), 0, new Color(255, 100, 100), 0.6f);

						SpawnReward();
						State = ShrineState_Defeated;

						Timer = 0;
						waveTime = 0;
						SetFrame(2);
						ProtectionWorld.RemoveRegionBySource(new Point16(ParentX, ParentY));
					}

					return;
				}

				if (!minions.Any(n => n.active) && Timer - waveTime > 181) //advance the wave
				{
					SpawnWave();
					waveTime = (int)Timer;
					State++;
				}
			}
			//else//renable this if there are issues with protection being left on
			//{
			//	ProtectionWorld.RemoveRegionBySource(new Point16(ParentX, ParentY));
			//}
		}

		private void SetFrame(int frame)
		{
			const int tileWidth = 3;
			const int tileHeight = 6;
			int a = 3 / 2;
			int b = 5 / 2;
			for (int x = 0; x < tileWidth; x++)
			{
				for (int y = 0; y < tileHeight; y++)
				{
					int realX = ParentX - 1 + x;
					int realY = ParentY - 3 + y;

					Framing.GetTileSafely(realX, realY).TileFrameX = (short)((x + (frame * tileWidth)) * 18);
				}
			}
		}

		private void SpawnWave()
		{
			for (int k = 0; k < 20; k++)
				Dust.NewDustPerfect(Projectile.Center + new Vector2(Main.rand.NextFloat(-24, 24), 28), ModContent.DustType<Dusts.Glow>(), Vector2.UnitY * -Main.rand.NextFloat(5), 0, new Color(255, Main.rand.Next(50), 0), 0.5f);

			if (State == 1)
			{
				SpawnNPC(Projectile.Center + new Vector2(130, 50), NPCID.RedSlime, 20);
				SpawnNPC(Projectile.Center + new Vector2(-130, 50), NPCID.RedSlime, 20);
				SpawnNPC(Projectile.Center + new Vector2(267, -40), NPCID.RedSlime, 20);
				SpawnNPC(Projectile.Center + new Vector2(-267, -40), NPCID.RedSlime, 20);
			}

			if (State == 2)
			{
				SpawnNPC(Projectile.Center + new Vector2(110, 50), NPCID.RedSlime, 20);
				SpawnNPC(Projectile.Center + new Vector2(-110, 50), NPCID.RedSlime, 20);
				SpawnNPC(Projectile.Center + new Vector2(240, 40), NPCID.Skeleton, 20);
				SpawnNPC(Projectile.Center + new Vector2(-240, 40), NPCID.Skeleton, 20);
				SpawnNPC(Projectile.Center + new Vector2(0, -150), NPCID.CaveBat, 20);
			}

			if (State == 3)
			{
				SpawnNPC(Projectile.Center + new Vector2(130, 40), NPCID.SkeletonArcher, 20, hpOverride: 0.2f, damageOverride: 0.2f, defenseOverride: 0f);
				SpawnNPC(Projectile.Center + new Vector2(-130, 40), NPCID.SkeletonArcher, 20, hpOverride: 0.2f, damageOverride: 0.2f, defenseOverride: 0f);
				SpawnNPC(Projectile.Center + new Vector2(140, -140), NPCID.CaveBat, 20);
				SpawnNPC(Projectile.Center + new Vector2(-140, -140), NPCID.CaveBat, 20);
			}

			if (State == 4)
			{
				SpawnNPC(Projectile.Center + new Vector2(130, 50), NPCID.Skeleton, 20);
				SpawnNPC(Projectile.Center + new Vector2(-130, 50), NPCID.Skeleton, 20);

				SpawnNPC(Projectile.Center + new Vector2(267, -40), NPCID.SkeletonArcher, 20, hpOverride: 0.2f, damageOverride: 0.2f, defenseOverride: 0f);
				SpawnNPC(Projectile.Center + new Vector2(-267, -40), NPCID.SkeletonArcher, 20, hpOverride: 0.2f, damageOverride: 0.2f, defenseOverride: 0f);

				SpawnNPC(Projectile.Center + new Vector2(70, -140), NPCID.CaveBat, 20);
				SpawnNPC(Projectile.Center + new Vector2(-70, -140), NPCID.CaveBat, 20);
			}

			if (State == 5)
			{
				SpawnNPC(Projectile.Center + new Vector2(130, 50), NPCID.SkeletonArcher, 20, hpOverride: 0.2f, damageOverride: 0.2f, defenseOverride: 0f);
				SpawnNPC(Projectile.Center + new Vector2(-130, 50), NPCID.SkeletonArcher, 20, hpOverride: 0.2f, damageOverride: 0.2f, defenseOverride: 0f);

				SpawnNPC(Projectile.Center + new Vector2(120, -160), NPCID.CaveBat, 20);
				SpawnNPC(Projectile.Center + new Vector2(-120, -160), NPCID.CaveBat, 20);

				SpawnNPC(Projectile.Center + new Vector2(220, -110), NPCID.CaveBat, 20);
				SpawnNPC(Projectile.Center + new Vector2(-220, -110), NPCID.CaveBat, 20);
			}

			if (State == 6)
			{
				SpawnNPC(Projectile.Center + new Vector2(130, 50), NPCID.Skeleton, 20);
				SpawnNPC(Projectile.Center + new Vector2(-130, 50), NPCID.Skeleton, 20);

				SpawnNPC(Projectile.Center + new Vector2(267, -50), NPCID.Skeleton, 20);
				SpawnNPC(Projectile.Center + new Vector2(-267, -50), NPCID.Skeleton, 20);

				SpawnNPC(Projectile.Center + new Vector2(0, -170), NPCID.Demon, 40, hpOverride: 2f, scale: 1.5f);
			}
		}

		private void SpawnNPC(Vector2 pos, int type, int dustAmount, float hpOverride = -1, float damageOverride = -1, float defenseOverride = -1, float scale = 1)
		{
			int i = Projectile.NewProjectile(new EntitySource_WorldEvent(), pos, Vector2.Zero, ModContent.ProjectileType<SpawnEgg>(), 0, 0, Main.myPlayer, type, dustAmount);
			(Main.projectile[i].ModProjectile as SpawnEgg).parent = this;
			(Main.projectile[i].ModProjectile as SpawnEgg).hpOverride = hpOverride;
			(Main.projectile[i].ModProjectile as SpawnEgg).damageOverride = damageOverride;
			(Main.projectile[i].ModProjectile as SpawnEgg).defenseOverride = defenseOverride;
			Main.projectile[i].scale = scale;
		}

		private void SpawnReward()
		{
			Item.NewItem(Projectile.GetSource_FromAI(), Projectile.getRect(), ModContent.ItemType<DullBlade>());
			ShrinePlayer.SimulateGoldChest(Projectile, false);//todo: maybe this should be a relic on no-hit
			ShrinePlayer.SimulateWoodenChest(Projectile);
		}

		public override void PostDraw(Color lightColor)
		{
			SpriteBatch spriteBatch = Main.spriteBatch;

			for (int k = 0; k < minions.Count; k++)
			{
				NPC target = minions[k];

				if (!target.active)
					continue;

				if (Main.rand.NextBool(2))
					Dust.NewDustPerfect(target.position + new Vector2(Main.rand.Next(target.width), Main.rand.Next(target.height)), ModContent.DustType<Dusts.Shadow>(), new Vector2(0, -Main.rand.NextFloat()), 0, Color.Black, Main.rand.NextFloat());

				Effect effect = Terraria.Graphics.Effects.Filters.Scene["Whitewash"].GetShader().Shader;

				spriteBatch.End();
				spriteBatch.Begin(default, default, default, default, default, effect, Main.GameViewMatrix.ZoomMatrix);

				spriteBatch.Draw(TextureAssets.Npc[target.type].Value, target.Center + Vector2.UnitX * 2 - Main.screenPosition, target.frame, Color.White, target.rotation, target.frame.Size() / 2, target.scale, target.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
				spriteBatch.Draw(TextureAssets.Npc[target.type].Value, target.Center + Vector2.UnitX * -2 - Main.screenPosition, target.frame, Color.White, target.rotation, target.frame.Size() / 2, target.scale, target.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
				spriteBatch.Draw(TextureAssets.Npc[target.type].Value, target.Center + Vector2.UnitY * 2 - Main.screenPosition, target.frame, Color.White, target.rotation, target.frame.Size() / 2, target.scale, target.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
				spriteBatch.Draw(TextureAssets.Npc[target.type].Value, target.Center + Vector2.UnitY * -2 - Main.screenPosition, target.frame, Color.White, target.rotation, target.frame.Size() / 2, target.scale, target.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);

				spriteBatch.End();
				spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);

				spriteBatch.Draw(TextureAssets.Npc[target.type].Value, target.Center - Main.screenPosition, target.frame, Color.Black, target.rotation, target.frame.Size() / 2, target.scale, target.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
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

				float rad = -32;

				if (State >= maxWaves + 2)
					rad += Helpers.Helper.BezierEase((Timer - waveTime) / 128f) * 32;

				for (int k = 0; k < Math.Min(State - 2, maxWaves - 1); k++)
				{
					Texture2D tex2 = ModContent.Request<Texture2D>("StarlightRiver/Assets/Keys/GlowSoft").Value;
					spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition + new Vector2(0, -44) + Vector2.UnitX.RotatedBy(k / (float)(maxWaves - 2) * 3.14f) * rad, default, new Color(255, 100, 100), 0, tex.Size() / 2, 0.3f, 0, 0);
					spriteBatch.Draw(tex2, Projectile.Center - Main.screenPosition + new Vector2(0, -32) + Vector2.UnitX.RotatedBy(k / (float)(maxWaves - 2) * 3.14f) * rad, default, new Color(255, 100, 100), 0, tex2.Size() / 2, 0.3f, 0, 0);
					spriteBatch.Draw(tex2, Projectile.Center - Main.screenPosition + new Vector2(0, -32) + Vector2.UnitX.RotatedBy(k / (float)(maxWaves - 2) * 3.14f) * rad, default, Color.White, 0, tex2.Size() / 2, 0.1f, 0, 0);
				}

				Texture2D barrier = ModContent.Request<Texture2D>("StarlightRiver/Assets/MotionTrail").Value;
				var sourceRect = new Rectangle(0, (int)(Main.GameUpdateCount * 0.4f), barrier.Width, barrier.Height);
				var sourceRect2 = new Rectangle(0, (int)(Main.GameUpdateCount * -0.73f), barrier.Width, barrier.Height);

				var targetRect = new Rectangle((int)(Projectile.Center.X - Main.screenPosition.X) - 25 * 16 - 10, (int)(Projectile.Center.Y - Main.screenPosition.Y) - 16, 32, 80);
				spriteBatch.Draw(barrier, targetRect, sourceRect, new Color(255, 100, 100) * 0.6f * Windup);
				spriteBatch.Draw(barrier, targetRect, sourceRect2, new Color(255, 50, 50) * 0.5f * Windup);
				targetRect.Inflate(-15, 0);
				targetRect.Offset(15, 0);
				spriteBatch.Draw(barrier, targetRect, sourceRect2, Color.White * Windup);

				targetRect = new Rectangle((int)(Projectile.Center.X - Main.screenPosition.X) + 24 * 16 - 6, (int)(Projectile.Center.Y - Main.screenPosition.Y) - 16, 32, 80);
				spriteBatch.Draw(barrier, targetRect, sourceRect, new Color(255, 100, 100) * 0.6f * Windup, 0, default, SpriteEffects.FlipHorizontally, 0);
				spriteBatch.Draw(barrier, targetRect, sourceRect2, new Color(255, 50, 50) * 0.5f * Windup, 0, default, SpriteEffects.FlipHorizontally, 0);
				targetRect.Inflate(-15, 0);
				targetRect.Offset(-15, 0);
				spriteBatch.Draw(barrier, targetRect, sourceRect2, Color.White * Windup);
			}
		}

		private Color GetBeamColor(float time)
		{
			float sin = 0.5f + (float)Math.Sin(time * 2 + 1) * 0.5f;
			float sin2 = 0.5f + (float)Math.Sin(time) * 0.5f;
			return new Color(255, (int)(50 * sin), 0) * sin2 * Windup;
		}
	}

	class SpawnEgg : ModProjectile, IDrawAdditive
	{
		public float hpOverride = -1;
		public float damageOverride = -1;
		public float defenseOverride = -1;

		public ref float SpawnType => ref Projectile.ai[0];
		public ref float DustCount => ref Projectile.ai[1];

		public CombatShrineDummy parent = null;

		public override string Texture => AssetDirectory.Invisible;

		public override void SetDefaults()
		{
			Projectile.width = 32;
			Projectile.height = 32;
			Projectile.timeLeft = 120;
			Projectile.tileCollide = false;
			Projectile.friendly = true;
		}

		public override void AI()
		{
			if (Projectile.timeLeft == 70)
				Helpers.Helper.PlayPitched("ShadowSpawn", 1, 1, Projectile.Center);

			if (Projectile.timeLeft == 30)
			{
				int i = Terraria.NPC.NewNPC(Projectile.GetSource_FromThis(), (int)Projectile.Center.X, (int)Projectile.Center.Y, (int)SpawnType);
				NPC NPC = Main.npc[i];
				NPC.alpha = 255;
				NPC.GivenName = "Shadow";
				NPC.lavaImmune = true;
				NPC.trapImmune = true;
				NPC.HitSound = SoundID.NPCHit7;
				NPC.DeathSound = new SoundStyle($"{nameof(StarlightRiver)}/Sounds/ShadowDeath");
				NPC.GetGlobalNPC<StarlightNPC>().dontDropItems = true;

				if (hpOverride != -1)
				{
					NPC.lifeMax = (int)(NPC.lifeMax * hpOverride);
					NPC.life = (int)(NPC.life * hpOverride);
				}

				if (damageOverride != -1)
					NPC.damage = (int)(NPC.damage * damageOverride);

				if (defenseOverride != -1)
					NPC.defense = (int)(NPC.defense * defenseOverride);

				//Helpers.Helper.PlayPitched("Magic/Shadow2", 1.1f, 1, Projectile.Center);

				for (int k = 0; k < DustCount; k++)
				{
					Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(1.5f, 2), 0, new Color(255, 100, 100), 0.2f);
				}

				parent?.minions.Add(NPC);
			}
		}

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
			Texture2D tex = ModContent.Request<Texture2D>(AssetDirectory.GUI + "ItemGlow").Value;
			Texture2D texRing = ModContent.Request<Texture2D>(AssetDirectory.GUI + "RingGlow").Value;

			float bright = Helpers.Helper.BezierEase(1 - (Projectile.timeLeft - 60) / 120f);

			if (Projectile.timeLeft < 20)
				bright = Projectile.timeLeft / 20f;

			float starScale = Helpers.Helper.BezierEase(1 - (Projectile.timeLeft - 90) / 30f);

			if (Projectile.timeLeft <= 90)
				starScale = 0.3f + Projectile.timeLeft / 90f * 0.7f;

			spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.Red * bright, Helpers.Helper.BezierEase(Projectile.timeLeft / 160f) * 6.28f, tex.Size() / 2, starScale * 0.3f * Projectile.scale, 0, 0);
			spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White * bright, Helpers.Helper.BezierEase(Projectile.timeLeft / 160f) * 6.28f, tex.Size() / 2, starScale * 0.2f * Projectile.scale, 0, 0);

			float ringBright = 1;
			if (Projectile.timeLeft > 100)
				ringBright = 1 - (Projectile.timeLeft - 100) / 20f;

			float ringScale = 1 + (Projectile.timeLeft - 50) / 70f * 0.3f;

			if (Projectile.timeLeft <= 50)
				ringScale = Helpers.Helper.BezierEase((Projectile.timeLeft - 20) / 30f);

			spriteBatch.Draw(texRing, Projectile.Center - Main.screenPosition, null, Color.Red * ringBright * 0.8f, Projectile.timeLeft / 60f * 6.28f, texRing.Size() / 2, ringScale * 0.2f * Projectile.scale, 0, 0);
			spriteBatch.Draw(texRing, Projectile.Center - Main.screenPosition, null, Color.White * ringBright * 0.5f, Projectile.timeLeft / 60f * 6.28f, texRing.Size() / 2, ringScale * 0.195f * Projectile.scale, 0, 0);

			if (Projectile.timeLeft < 30)
			{
				Texture2D tex2 = ModContent.Request<Texture2D>("StarlightRiver/Assets/Keys/GlowSoft").Value;
				spriteBatch.Draw(tex2, Projectile.Center - Main.screenPosition, null, new Color(255, 50, 50) * (Projectile.timeLeft / 30f), 0, tex2.Size() / 2, (1 - Projectile.timeLeft / 30f) * 7 * Projectile.scale, 0, 0);
				spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, new Color(255, 150, 150) * (Projectile.timeLeft / 30f), 0, tex.Size() / 2, (1 - Projectile.timeLeft / 30f) * 1 * Projectile.scale, 0, 0);

				if (Projectile.timeLeft > 15)
					spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, new Color(255, 100, 100) * ((Projectile.timeLeft - 15) / 15f), 1.57f / 4, tex.Size() / 2, (1 - (Projectile.timeLeft - 15) / 15f) * 2 * Projectile.scale, 0, 0);
			}
		}
	}
}