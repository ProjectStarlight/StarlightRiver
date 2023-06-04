using StarlightRiver.Content.Biomes;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.ID;

namespace StarlightRiver.Content.NPCs.Moonstone
{
	//bestiary needs to be done but there isnt a moonstone bestiary template thingy
	public class Dreamprism : ModNPC
	{
		private enum Phase
		{
			rising = 0,
			spinning = 1,
			slamming = 2,
			slammed = 3
		}

		private const float TRAIL_WIDTH = 0.7f;

		private List<Vector2> cache;
		private Trail trail;
		private Trail trail2;

		private int yFrame = 0;
		private float frameCounter = 0;

		private Phase phase = Phase.rising;

		private bool emerged = false;

		private float emergeSpeed = 1;

		private float bobCounter;
		private Vector2 posAbovePlayer = Vector2.Zero;
		private Vector2 risingPos = Vector2.Zero;

		private float rockRotation = 0f;
		private float rockRotationSpeed = 0.1f;

		private Vector2 rockPosition = Vector2.Zero;

		private int spinTimer = 0;

		private int slamTimer = 0;

		private Player Target => Main.player[NPC.target];

		public override string Texture => AssetDirectory.MoonstoneNPC + Name;

		public override void Load()
		{
			for (int i = 1; i < 5; i++)
			{
				GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, Texture + "_Rock" + i.ToString());
			}

			for (int i = 1; i < 6; i++)
			{
				GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, Texture + "_Gore" + i.ToString());
			}
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Dreamprism");
			NPCID.Sets.TrailCacheLength[Type] = 10;
			NPCID.Sets.TrailingMode[Type] = 0;
			Main.npcFrameCount[NPC.type] = 16;
		}

		public override void SetDefaults()
		{
			NPC.width = 30;
			NPC.height = 30;
			NPC.knockBackResist = 0.2f;
			NPC.lifeMax = 150;
			NPC.noGravity = true;
			NPC.noTileCollide = false;
			NPC.damage = 35;
			NPC.aiStyle = -1;
			NPC.friendly = false;

			NPC.HitSound = SoundID.Item27 with
			{
				Pitch = -0.3f
			};
			NPC.DeathSound = SoundID.Shatter;

			NPC.value = Item.buyPrice(silver: 3, copper: 15);
			NPC.behindTiles = true;
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
			{
				Bestiary.SLRSpawnConditions.Moonstone,
				new FlavorTextBestiaryInfoElement("These anomalous beings are a collection of previously lifeless rocks possessed by the hopes and dreams of the collective subconscious.")
			});
		}

		public override void OnSpawn(IEntitySource source)
		{
			rockPosition = NPC.Center;
		}

		public override bool CanHitPlayer(Player target, ref int cooldownSlot)
		{
			return phase == Phase.slamming;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			if (phase == Phase.slamming || phase == Phase.slammed)
				DrawTrail();

			if (NPC.IsABestiaryIconDummy)
				drawColor = NPC.GetBestiaryEntryColor();

			rockPosition = Vector2.Lerp(rockPosition, NPC.Center, 0.45f);
			Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
			Texture2D glowTexture = ModContent.Request<Texture2D>(Texture + "_Glow").Value;

			SpriteEffects effects = SpriteEffects.None;
			Vector2 origin = NPC.frame.Size() / 2;

			if (NPC.spriteDirection != 1)
				effects = SpriteEffects.FlipHorizontally;

			var slopeOffset = new Vector2(0, NPC.gfxOffY);

			DrawRocks(spriteBatch, screenPos, drawColor, true);

			spriteBatch.Draw(texture, slopeOffset + NPC.Center - screenPos, NPC.frame, drawColor, NPC.rotation, origin, NPC.scale, effects, 0f);

			if (!NPC.IsABestiaryIconDummy)
				spriteBatch.Draw(glowTexture, slopeOffset + NPC.Center - screenPos, NPC.frame, Color.White, NPC.rotation, origin, NPC.scale, effects, 0f);

			DrawRocks(spriteBatch, screenPos, drawColor, false);
			return false;
		}

		public override void AI()
		{
			Lighting.AddLight(NPC.Center, Color.Cyan.ToVector3() * 1.2f);
			NPC.TargetClosest(false);

			switch (phase)
			{
				case Phase.rising:
					RisingBehavior();
					break;

				case Phase.spinning:
					SpinBehavior();
					break;

				case Phase.slamming:
					SlamBehavior();
					break;

				case Phase.slammed:
					SlammedBehavior();
					break;
			}
		}

		public override void FindFrame(int frameHeight)
		{
			rockRotation += rockRotationSpeed;
			frameCounter += rockRotationSpeed;

			if (frameCounter >= 1)
			{
				yFrame++;
				frameCounter = 0;
			}

			yFrame %= Main.npcFrameCount[NPC.type];
			NPC.frame = new Rectangle(0, yFrame * frameHeight, NPC.width, frameHeight);
		}

		public override void OnKill()
		{
			for (int i = 1; i < 5; i++)
			{
				float angle = i / 4f * 6.28f + rockRotation;
				Vector2 offset = (angle.ToRotationVector2() * new Vector2(30, 10)).RotatedBy(0.3f * Math.Sin(rockRotation * 0.2f));

				Gore.NewGoreDirect(NPC.GetSource_FromThis(), NPC.Center + offset, Main.rand.NextVector2Circular(2, 2), Mod.Find<ModGore>("Dreamprism_Rock" + i.ToString()).Type);
			}

			for (int j = 1; j < 6; j++)
			{
				Gore.NewGoreDirect(NPC.GetSource_FromThis(), NPC.Center + Main.rand.NextVector2Circular(10, 30), Main.rand.NextVector2Circular(3, 3), Mod.Find<ModGore>("Dreamprism_Gore" + j.ToString()).Type);
			}
		}

		#region trail stuff
		private void DrawTrail()
		{
			Main.spriteBatch.End();

			Effect effect = Terraria.Graphics.Effects.Filters.Scene["DatsuzeiTrail"].GetShader().Shader;

			var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.TransformationMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.02f);
			effect.Parameters["repeats"].SetValue(8f);
			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrail").Value);
			effect.Parameters["sampleTexture2"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/Items/Moonstone/DatsuzeiFlameMap2").Value);

			trail?.Render(effect);

			effect.Parameters["sampleTexture2"].SetValue(Terraria.GameContent.TextureAssets.MagicPixel.Value);

			trail2?.Render(effect);

			Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
		}

		private void ManageCaches()
		{
			if (cache == null)
			{
				cache = new List<Vector2>();

				for (int i = 0; i < 15; i++)
				{
					cache.Add(NPC.Center);
				}
			}

			cache.Add(NPC.Center);

			while (cache.Count > 15)
			{
				cache.RemoveAt(0);
			}
		}

		private void ManageTrail()
		{
			trail ??= new Trail(Main.instance.GraphicsDevice, 15, new TriangularTip(1), factor => factor * 25 * TRAIL_WIDTH, factor => new Color(120, 20 + (int)(100 * factor.X), 255) * factor.X);

			trail2 ??= new Trail(Main.instance.GraphicsDevice, 15, new TriangularTip(1), factor => (50 + 0 + factor * 0) * TRAIL_WIDTH, factor => new Color(100, 20 + (int)(60 * factor.X), 255) * factor.X * 0.15f);

			if (cache != null)
			{
				trail2.Positions = cache.ToArray();
				trail.Positions = cache.ToArray();
			}

			trail.NextPosition = NPC.Center + NPC.velocity;
			trail2.NextPosition = NPC.Center + NPC.velocity;
		}

		#endregion

		private void DrawRocks(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor, bool behind)
		{
			for (int i = 1; i < 5; i++)
			{
				Texture2D rockTex = ModContent.Request<Texture2D>(Texture + "_Rock" + i.ToString()).Value;
				Texture2D glowTex = ModContent.Request<Texture2D>(Texture + "_Rock" + i.ToString() + "_Glow").Value;
				float angle = i / 4f * 6.28f + rockRotation;

				Vector2 offset = (angle.ToRotationVector2() * new Vector2(30, 10)).RotatedBy(0.3f * Math.Sin(rockRotation * 0.2f));

				if (behind && offset.Y < 0 || !behind && offset.Y >= 0)
				{
					spriteBatch.Draw(rockTex, offset + rockPosition - screenPos, null, drawColor, NPC.rotation, rockTex.Size() / 2, NPC.scale + offset.Y * 0.02f, SpriteEffects.None, 0f);
					if (!NPC.IsABestiaryIconDummy)
						spriteBatch.Draw(glowTex, offset + rockPosition - screenPos, null, Color.White, NPC.rotation, rockTex.Size() / 2, NPC.scale + offset.Y * 0.02f, SpriteEffects.None, 0f);
				}
			}
		}

		private void RisingBehavior()
		{
			bobCounter += 0.02f;

			if (posAbovePlayer == Vector2.Zero)
			{
				risingPos = NPC.Center;
				posAbovePlayer = Target.Center - new Vector2(0, Main.rand.Next(300, 400));
			}

			float distance = posAbovePlayer.Y - risingPos.Y;
			float progress = (NPC.Center.Y - risingPos.Y) / distance;

			if (progress < 0)
			{
				progress = MathHelper.Clamp(progress, 0, 1);
				risingPos = NPC.Center;
			}

			if (progress < 0.85f)
				posAbovePlayer.X = Target.Center.X;

			rockRotationSpeed = 0.35f * progress;

			Vector2 dir = NPC.DirectionTo(posAbovePlayer);
			NPC.velocity = dir * ((float)Math.Sin(progress * 3.14f) + 0.3f) * 5;
			NPC.velocity.Y += (float)Math.Cos(bobCounter) * 0.15f;

			if (!emerged)
			{
				NPC.velocity *= 0.1f;
				NPC.rotation += Main.rand.NextFloat(-0.05f, 0.05f);
				NPC.rotation *= 0.95f;
				Tile tile = Main.tile[(int)NPC.Center.X / 16, (int)((NPC.Center.Y - 12) / 16) + 2];

				if (!tile.HasTile || !Main.tileSolid[tile.TileType])
				{
					NPC.rotation = 0;
					emerged = true;

					for (int k = 0; k < 5; k++)
					{
						Dust.NewDustPerfect(NPC.Center + new Vector2(0, 20), ModContent.DustType<Dusts.Stone>(), new Vector2(0, -1).RotatedByRandom(1) * Main.rand.NextFloat(2, 5));
					}
				}
			}
			else
			{
				emergeSpeed += 0.8f;

				if (emergeSpeed < 3.14f)
					NPC.velocity.Y += (float)Math.Cos(emergeSpeed) * 15;
			}

			if ((NPC.Center - posAbovePlayer).Length() < 4)
			{
				emergeSpeed = 1;
				emerged = false;
				bobCounter = 0;
				phase = Phase.spinning;
				risingPos = Vector2.Zero;
				posAbovePlayer = Vector2.Zero;
			}
		}

		private void SpinBehavior()
		{
			rockRotationSpeed = 0.35f;
			spinTimer++;

			NPC.velocity.Y = (float)-Math.Sin(spinTimer / 30f * 3.14f) * 2f;

			if (spinTimer > 30)
			{
				NPC.velocity.Y = 20;
				spinTimer = 0;
				phase = Phase.slamming;
				NPC.noTileCollide = false;
			}
		}

		private void SlamBehavior()
		{
			if (!Main.dedServ)
			{
				ManageCaches();
				ManageTrail();
			}

			Dust.NewDustPerfect(NPC.Center + Main.rand.NextVector2Circular(15, 15), ModContent.DustType<Dusts.Glow>(), Main.rand.NextVector2Circular(1, 1), 0, Color.Lerp(new Color(120, 80, 255), Color.White, Main.rand.NextFloat()), Main.rand.NextFloat(0.35f, 0.65f));
			Dust.NewDustPerfect(NPC.Center + Main.rand.NextVector2Circular(15, 15), ModContent.DustType<Dusts.Aurora>(), Main.rand.NextVector2Circular(1, 1), 0, Color.Lerp(new Color(120, 80, 255), Color.White, Main.rand.NextFloat()), Main.rand.NextFloat(0.35f, 0.65f));

			rockRotationSpeed = 0.35f;

			if (NPC.velocity.Y < 30)
				NPC.velocity.Y += 1f;

			slamTimer++;

			if (NPC.collideY || slamTimer > 60)
			{
				Helper.PlayPitched("GlassMiniboss/GlassSmash", 1f, 0.3f, NPC.Center);

				for (int k = 0; k < 16; k++)
				{
					Dust.NewDustPerfect(NPC.Center, ModContent.DustType<Dusts.Stone>(), new Vector2(0, -1).RotatedByRandom(1) * Main.rand.NextFloat(3, 7));
					//target.PickTile((int)(NPC.Center.X / 16) + k - 1, (int)((NPC.Bottom.Y - 12) / 16), 0);
					//target.PickTile((int)(NPC.Center.X / 16) + k - 1, (int)((NPC.Bottom.Y - 12) / 16) + 1, 0);
				}

				if (Target == Main.LocalPlayer)
					CameraSystem.shake += 8;

				NPC.Center += NPC.velocity / 3;
				Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), NPC.Center + new Vector2(0, 10), Vector2.Zero, ModContent.ProjectileType<DreamprismSlam>(), (int)(NPC.damage * (Main.expertMode ? 0.5f : 1)), 0).timeLeft = 294;
				slamTimer = 0;
				phase = Phase.slammed;
				NPC.velocity = Vector2.Zero;
			}
		}

		private void SlammedBehavior()
		{
			if (!Main.dedServ)
			{
				ManageCaches();
				ManageTrail();
			}

			NPC.velocity = Vector2.Zero;
			rockRotationSpeed = 0;
			slamTimer++;

			if (slamTimer > 90)
			{
				Helper.PlayPitched("StoneSlide", 1f, -1f, NPC.Center);
				risingPos = NPC.Center;
				posAbovePlayer = Target.Center - new Vector2(0, Main.rand.Next(300, 400));
				phase = Phase.rising;
				slamTimer = 0;
				cache = null;
				NPC.noTileCollide = true;
			}
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo)
		{
			return spawnInfo.Player.InModBiome(ModContent.GetInstance<MoonstoneBiome>()) ? 10 : 0;
		}
	}

	public class DreamprismSlam : ModProjectile, IDrawOverTiles
	{
		float counter = 0;

		int frameCounter = 0;

		public override string Texture => AssetDirectory.MoonstoneNPC + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Slam");
		}

		public override void SetDefaults()
		{
			Projectile.friendly = false;
			Projectile.hostile = true;
			Projectile.tileCollide = false;
			Projectile.Size = new Vector2(96, 96);
			Projectile.penetrate = -1;
			Projectile.timeLeft = 200;
			Projectile.rotation = Main.rand.NextFloat(6.28f);
		}

		public override void AI()
		{
			frameCounter += 4;

			if (counter > 0)
				Projectile.hostile = false;

			counter += (float)(Math.PI / 2f) / 200;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			return false;
		}

		public void DrawOverTiles(SpriteBatch spriteBatch)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			int frameSize = (int)MathHelper.Min(frameCounter, tex.Height / 2);
			var frame = new Rectangle(tex.Width / 2 - frameSize, tex.Height / 2 - frameSize, frameSize * 2, frameSize * 2);
			var color = new Color(99, 71, 255, 0);
			color *= (float)Math.Cos(counter);

			for (int i = 0; i < 2; i++)
			{
				spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, frame, color, Projectile.rotation, new Vector2(frameSize, frameSize), Projectile.scale, SpriteEffects.None, 0);
			}
		}
	}
}