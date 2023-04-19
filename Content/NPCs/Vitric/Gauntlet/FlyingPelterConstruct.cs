using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Items.Misc;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.NPCs.Vitric.Gauntlet
{
	internal class FlyingPelterConstruct : VitricConstructNPC
	{
		private const int BOWFRAMES = 4;
		private const int XFRAMES = 1;

		private int bowFrame = 0;
		private int bowFrameCounter = 0;

		private int bodyFrame;
		private int frameCounter;

		float bowRotation = 0;
		float bowArmRotation = 0;

		float headRotation = 0f;

		private readonly int XFrame = 0;

		public Vector2 posToBe = Vector2.Zero;

		public Vector2 oldPos = Vector2.Zero;

		public bool attacking = false;

		public bool doingCombo = false;
		public NPC comboPartner = default;
		public bool empowered = false;

		private int arrowsShot = 0;

		private float bobCounter = 0f;

		public bool stayInPlace = false;

		public NPC pairedGrunt = default;

		private float glowCounter = 0;

		private float predictorLength = 1f;

		private Vector2 knockbackVel = Vector2.Zero;

		private Player Target => Main.player[NPC.target];

		private Vector2 BowArmPos => NPC.Center + new Vector2(16 * NPC.spriteDirection, -4).RotatedBy(NPC.rotation);
		private Vector2 BackArmPos => NPC.Center + new Vector2(5 * NPC.spriteDirection, -4).RotatedBy(NPC.rotation);

		private Vector2 HeadPos => NPC.Center + new Vector2(12 * NPC.spriteDirection, -8).RotatedBy(NPC.rotation);

		private Vector2 BowPos => BowArmPos + ((16 + (float)Math.Abs(Math.Sin(bowArmRotation)) * 3) * bowArmRotation.ToRotationVector2()).RotatedBy(NPC.rotation);

		float BackArmRotation => BackArmPos.DirectionTo(BowPos).ToRotation();

		public override string Texture => AssetDirectory.GauntletNpc + "FlyingPelterConstruct";

		public override Vector2 PreviewOffset => new(0, 0);

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Flying Pelter Construct");
			Main.npcFrameCount[NPC.type] = 5;
		}

		public override void SetDefaults()
		{
			NPC.width = 30;
			NPC.height = 48;
			NPC.damage = 25;
			NPC.defense = 3;
			NPC.lifeMax = 100;
			NPC.value = 0f;
			NPC.knockBackResist = 0.6f;
			NPC.HitSound = SoundID.Item27 with
			{
				Pitch = -0.3f
			};
			NPC.DeathSound = SoundID.Shatter;
			NPC.noGravity = true;
			NPC.behindTiles = true;
		}

		public override void OnSpawn(IEntitySource source)
		{
			posToBe = NPC.Center;
			oldPos = NPC.Center;
		}

		public override void SafeAI()
		{
			knockbackVel *= 0.87f;

			if (pairedGrunt != default)
			{
				if (!pairedGrunt.active)
					pairedGrunt = default;
			}

			if (empowered)
			{
				glowCounter += 0.1f;

				if (!comboPartner.active)
					empowered = false;
			}

			if (doingCombo && ableToDoCombo)
			{
				if (!comboPartner.active)
					doingCombo = false;

				posToBe = new Vector2(MathHelper.Lerp(comboPartner.Center.X, Target.Center.X, 0.5f), comboPartner.Center.Y - 100);

				if (stayInPlace)
					posToBe = NPC.Center;
			}

			bobCounter += 0.02f;
			NPC.TargetClosest(true);

			frameCounter++;

			if (frameCounter > 4)
			{
				frameCounter = 0;

				bodyFrame++;
				bodyFrame %= Main.npcFrameCount[NPC.type];
			}

			Vector2 direction = BowArmPos.DirectionTo(Target.Center);

			if (!empowered)
				direction = direction.RotatedBy((Target.Center.X - NPC.Center.X) * -0.0003f);

			float rotDifference = Helper.RotationDifference(direction.ToRotation(), bowArmRotation);

			if (!empowered || bowFrameCounter < 75)
				bowArmRotation = MathHelper.Lerp(bowArmRotation, bowArmRotation + rotDifference, 0.1f);

			bowRotation = BackArmPos.DirectionTo(BowPos).ToRotation();

			NPC.spriteDirection = Math.Sign(NPC.Center.DirectionTo(Target.Center).X);

			if (NPC.spriteDirection == 1)
				headRotation = bowRotation / 2;
			else
				headRotation = Helper.RotationDifference(bowRotation, 3.14f) / 2;

			float distance = posToBe.X - oldPos.X;
			float progress = (NPC.Center.X - oldPos.X) / distance;

			if (progress < 0)
			{
				progress = MathHelper.Clamp(progress, 0, 1);
				oldPos = NPC.Center;
			}

			Vector2 dir = NPC.DirectionTo(posToBe);

			if (NPC.Distance(posToBe) < 30 || attacking || NPC.collideX || NPC.collideY)
			{
				NPC.velocity.Y = (float)Math.Cos(bobCounter) * 0.15f;
				attacking = true;
			}
			else
			{
				NPC.velocity = dir * ((float)Math.Sin(progress * 3.14f) + 0.1f) * 3;
				NPC.velocity.Y += (float)Math.Cos(bobCounter) * 0.15f;
				attacking = false;
			}

			if (attacking)
			{
				ShootArrows();
			}
			else
			{
				attacking = false;
				bowFrame = 0;
				bowFrameCounter = 0;
			}

			NPC.velocity += knockbackVel;
		}

		public override bool CanHitPlayer(Player target, ref int cooldownSlot)
		{
			return false;
		}

		public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
		{
			position.X += 8 * NPC.spriteDirection;
			position.Y -= 5;
			return true;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			if (NPC.IsABestiaryIconDummy)
			{
				DrawBestiary(spriteBatch, screenPos, Color.White);
				return false;
			}

			if (empowered)
			{
				Main.spriteBatch.End();
				Main.spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);

				if (bowFrame == 0)
					DrawPredictor(screenPos);

				float sin = 0.5f + (float)Math.Sin(glowCounter) * 0.5f;
				float distance = sin * 4 + 2;

				for (int i = 0; i < 8; i++)
				{
					float rad = i * 6.28f / 8;
					Vector2 offset = Vector2.UnitX.RotatedBy(rad) * distance;
					Color color = Color.OrangeRed * (1.5f - sin) * 0.7f;
					DrawComponents(true, screenPos, color, offset);
				}

				Main.spriteBatch.End();
				Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);

				if (bowFrame == 0)
					DrawLaserArrow(screenPos);
			}

			DrawComponents(false, screenPos, drawColor, Vector2.Zero);
			return false;
		}

		private void DrawBestiary(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D tex = Request<Texture2D>(PreviewTexturePath).Value;
			spriteBatch.Draw(tex, NPC.Center - screenPos, null, drawColor, NPC.rotation, tex.Size() / 2, NPC.scale, NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
		}

		public override void FindFrame(int frameHeight)
		{
			int frameWidth = 46;
			NPC.frame = new Rectangle(frameWidth * XFrame, bodyFrame * frameHeight, frameWidth, frameHeight);
		}

		private void DrawComponents(bool glow, Vector2 screenPos, Color drawColor, Vector2 offset) //TODO: Potentially have glow use a preview sprite instead of repeating this logic multiple times.
		{
			SpriteEffects effects = SpriteEffects.None;
			SpriteEffects bowEffects = SpriteEffects.None;

			string glowTag = glow ? "_White" : "";

			Texture2D mainTex = Request<Texture2D>(Texture + glowTag).Value;
			Texture2D glowTex = Request<Texture2D>(Texture + "_Glow").Value;

			Texture2D armTex = Request<Texture2D>(Texture + "_Arms" + glowTag).Value;
			Texture2D armGlowTex = Request<Texture2D>(Texture + "_Arms_Glow").Value;

			Texture2D headTex = Request<Texture2D>(Texture + "_Head" + glowTag).Value;

			Texture2D bowTex = Request<Texture2D>(Texture + "_Bow" + glowTag).Value;

			int armFrameSize = armTex.Height / 2;
			var frontFrame = new Rectangle(0, 0, armTex.Width, armFrameSize);
			var backFrame = new Rectangle(0, armFrameSize, armTex.Width, armFrameSize);

			int bowFrameHeight = bowTex.Height / BOWFRAMES;
			var bowFrameBox = new Rectangle(0, bowFrame * bowFrameHeight, bowTex.Width, bowFrameHeight);

			int mainFrameHeight = mainTex.Height / Main.npcFrameCount[NPC.type];
			int mainFrameWidth = mainTex.Width / XFRAMES;

			var backArmOrigin = new Vector2(3, 7);
			var bowArmOrigin = new Vector2(1, 5);
			var bowOrigin = new Vector2(18, 20);
			var headOrigin = new Vector2(headTex.Width / 2, headTex.Height);

			if (NPC.spriteDirection != 1)
			{
				effects = SpriteEffects.FlipHorizontally;
				bowEffects = SpriteEffects.FlipVertically;

				bowOrigin = new Vector2(bowOrigin.X, bowFrameHeight - bowOrigin.Y);
				backArmOrigin = new Vector2(backArmOrigin.X, armFrameSize - backArmOrigin.Y);
				bowArmOrigin = new Vector2(bowArmOrigin.X, armFrameSize - bowArmOrigin.Y);
				//bowOrigin = new Vector2(bowTex.Width - bowOrigin.X, bowOrigin.Y);
			}

			var slopeOffset = new Vector2(0, NPC.gfxOffY);
			Main.spriteBatch.Draw(mainTex, offset + NPC.Center + slopeOffset - screenPos, NPC.frame, drawColor, NPC.rotation, NPC.frame.Size() / 2, NPC.scale, effects, 0f);

			if (!glow)
				Main.spriteBatch.Draw(glowTex, offset + NPC.Center + slopeOffset - screenPos, NPC.frame, Color.White, NPC.rotation, NPC.frame.Size() / 2, NPC.scale, effects, 0f);

			Main.spriteBatch.Draw(headTex, offset + HeadPos + slopeOffset - screenPos, null, drawColor, headRotation + NPC.rotation, headOrigin, NPC.scale, effects, 0f);
			Main.spriteBatch.Draw(armTex, offset + BowArmPos + slopeOffset - screenPos, backFrame, drawColor, bowArmRotation + NPC.rotation, bowArmOrigin, NPC.scale, bowEffects, 0f);

			if (!glow)
				Main.spriteBatch.Draw(armGlowTex, offset + BowArmPos + slopeOffset - screenPos, backFrame, Color.White, bowArmRotation + NPC.rotation, bowArmOrigin, NPC.scale, bowEffects, 0f);

			Main.spriteBatch.Draw(bowTex, offset + BowPos + slopeOffset - screenPos, bowFrameBox, drawColor, bowRotation + NPC.rotation, bowOrigin, NPC.scale, bowEffects, 0f);
			Main.spriteBatch.Draw(armTex, offset + BackArmPos + slopeOffset - screenPos, frontFrame, drawColor, BackArmRotation + NPC.rotation, backArmOrigin, NPC.scale, bowEffects, 0f);

			if (!glow)
				Main.spriteBatch.Draw(armGlowTex, offset + BackArmPos + slopeOffset - screenPos, frontFrame, Color.White, BackArmRotation + NPC.rotation, backArmOrigin, NPC.scale, bowEffects, 0f);
		}

		private void DrawPredictor(Vector2 screenPos)
		{
			Texture2D predictorTex = Request<Texture2D>(AssetDirectory.Keys + "Shine").Value;
			float rot = bowArmRotation + 1.57f;

			float charge = EaseFunction.EaseQuadInOut.Ease(MathHelper.Clamp(bowFrameCounter / 100f, 0, 1));
			float opacity = (float)Math.Sqrt(charge);

			if (bowFrameCounter > 100)
				opacity *= 1 - (bowFrameCounter - 100) / 10f;

			var scale = new Vector2((0.1f + (1 - charge)) * 0.3f, predictorLength);
			var origin = new Vector2(predictorTex.Width / 2, predictorTex.Height);
			var slopeOffset = new Vector2(0, NPC.gfxOffY);

			Main.spriteBatch.Draw(predictorTex, slopeOffset + BowPos - screenPos, null, Color.Orange * opacity, rot, origin, scale, SpriteEffects.None, 0f);
		}

		private void DrawLaserArrow(Vector2 screenPos)
		{
			Texture2D arrowTex = Request<Texture2D>(AssetDirectory.GauntletNpc + "PelterConstructArrowLarge").Value;

			float rot = bowArmRotation;
			Vector2 pos = BowPos + bowArmRotation.ToRotationVector2() * 25 - screenPos;
			Vector2 origin = arrowTex.Size() / 2;

			float charge = 1 - MathHelper.Clamp(bowFrameCounter / 100f, 0, 1);
			float distance = charge * 8;

			for (int i = 0; i < 8; i++)
			{
				float rad = i * 6.28f / 8;
				Vector2 offset = Vector2.UnitX.RotatedBy(rad) * distance;
				Color color = Color.White * (1 - charge) * 0.3f;
				Main.spriteBatch.Draw(arrowTex, pos + offset, null, color, rot + 1.57f, origin, 1, SpriteEffects.None, 0f);
			}
		}

		public override void HitEffect(NPC.HitInfo hit)
		{
			if (pairedGrunt != default)
				(pairedGrunt.ModNPC as FlyingGruntConstruct).attacking = true;
		}

		public override void OnKill()
		{
			if (Main.netMode != NetmodeID.Server)
			{
				for (int i = 0; i < 9; i++)
					Dust.NewDustPerfect(NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), DustType<Cinder>(), Main.rand.NextVector2Circular(3, 3), 0, new Color(255, 150, 50), Main.rand.NextFloat(0.75f, 1.25f)).noGravity = false;

				for (int k = 1; k <= 12; k++)
					Gore.NewGoreDirect(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), Main.rand.NextVector2Circular(3, 3), Mod.Find<ModGore>("ConstructGore" + k).Type);
			}
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				Bestiary.SLRSpawnConditions.VitricDesert,
				new FlavorTextBestiaryInfoElement("One of the Glassweaver's constructs. Shares its ground variant's fragility, but it's wings grant it unparalleled vantage.")
			});
		}

		private void ShootArrows()
		{
			int arrowsToShoot = 3;
			int timeToShoot = 75;
			int timeToCharge = 4;

			if (empowered)
			{
				arrowsToShoot = 2;
				timeToShoot = 110;
				timeToCharge = 5;
			}

			bowFrameCounter++;
			if (bowFrame == 0)
			{
				if (bowFrameCounter < 75)
					predictorLength = 0.15f;

				if (bowFrameCounter > timeToShoot)
				{
					arrowsShot++;
					if (arrowsShot > arrowsToShoot)
					{
						arrowsShot = 0;
						attacking = false;
						posToBe = Target.Center + new Vector2(Main.rand.Next(-500, -100) * Math.Sign(Target.Center.X - NPC.Center.X), Main.rand.Next(-200, -70));
						oldPos = NPC.Center;
					}

					SoundEngine.PlaySound(SoundID.Item5, NPC.Center);

					if (!empowered)
					{
						Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), BowPos, BowPos.DirectionTo(Target.Center).RotatedBy((Target.Center.X - NPC.Center.X) * -0.0003f) * 10, ProjectileType<PelterConstructArrow>(), (int)(NPC.damage * (Main.expertMode || Main.masterMode ? 0.3f : 1)), NPC.knockBackResist);
					}
					else
					{
						var proj = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), BowPos + bowArmRotation.ToRotationVector2() * 5, bowArmRotation.ToRotationVector2() * 50, ProjectileType<PelterConstructArrowLarge>(), (int)(NPC.damage * (Main.expertMode || Main.masterMode ? 0.3f : 1)), NPC.knockBackResist);
						proj.rotation = bowArmRotation + 1.57f;
						proj.ai[0] = proj.Distance(Target.Center) / 5;

						knockbackVel = bowArmRotation.ToRotationVector2() * -5;

						for (int i = 0; i < 15; i++)
						{
							Vector2 dustPos = BowPos + Main.rand.NextVector2Circular(10, 10);
							Dust.NewDustPerfect(dustPos, DustType<Dusts.Glow>(), bowArmRotation.ToRotationVector2().RotatedByRandom(0.7f) * Main.rand.NextFloat(0.1f, 1f) * 4f, 0, new Color(255, 150, 50), Main.rand.NextFloat(0.75f, 1.25f)).noGravity = true;
						}
					}

					bowFrameCounter = 0;
					bowFrame++;
				}
			}
			else if (bowFrameCounter > timeToCharge)
			{
				bowFrameCounter = 0;
				bowFrame++;
			}

			bowFrame %= BOWFRAMES;
			NPC.spriteDirection = Math.Sign(NPC.Center.DirectionTo(Target.Center).X);

			NPC.velocity.X = 0;
		}

		public override void DrawHealingGlow(SpriteBatch spriteBatch)
		{
			spriteBatch.End();
			spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);

			float sin = 0.5f + (float)Math.Sin(Main.timeForVisualEffects * 0.04f) * 0.5f;
			float distance = sin * 3 + 2;

			for (int i = 0; i < 8; i++)
			{
				float rad = i * 6.28f / 8;
				Vector2 offset = Vector2.UnitX.RotatedBy(rad) * distance;
				Color color = Color.OrangeRed * (1.75f - sin) * 0.7f;
				DrawComponents(false, Main.screenPosition, color, offset);
			}

			spriteBatch.End();
			spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
		}
	}

	internal class PelterConstructArrowLarge : ModProjectile, IDrawAdditive
	{
		private List<Vector2> cache;
		private Trail trail;

		private float fade = 1;

		private Vector2 firstPos = Vector2.Zero;

		public override string Texture => AssetDirectory.GauntletNpc + Name;

		public override void SetDefaults()
		{
			Projectile.width = 12;
			Projectile.height = 12;
			Projectile.friendly = false;
			Projectile.hostile = true;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 270;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = false;
			Projectile.extraUpdates = 20;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Glass Arrow");
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Main.spriteBatch.End();
			Effect effect = Terraria.Graphics.Effects.Filters.Scene["CeirosRing"].GetShader().Shader;

			var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.ZoomMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["time"].SetValue(Projectile.timeLeft * -0.04f);
			effect.Parameters["repeats"].SetValue((int)Projectile.ai[0] / 5);
			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(Request<Texture2D>("StarlightRiver/Assets/EnergyTrail").Value);

			trail?.Render(effect);

			effect.Parameters["sampleTexture"].SetValue(Request<Texture2D>("StarlightRiver/Assets/FireTrail").Value);

			trail?.Render(effect);
			Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);

			Texture2D flash = Request<Texture2D>(Texture + "_Flare").Value;
			Color flashFade = Color.OrangeRed * fade * fade;
			flashFade.A = 0;
			Main.EntitySpriteDraw(flash, firstPos - Main.screenPosition, null, flashFade, 0, flash.Size() / 2, 2.5f * MathHelper.Lerp(0.5f, 1, fade * fade), SpriteEffects.None, 0);

			Texture2D tex = Request<Texture2D>(Texture).Value;
			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White * fade, Projectile.rotation, tex.Size() / 2, Projectile.scale, SpriteEffects.None, 0f);
			return false;
		}

		public override void AI()
		{
			if (firstPos == Vector2.Zero)
				firstPos = Projectile.Center;

			if (!Main.dedServ)
			{
				if (Projectile.extraUpdates > 0)
					ManageCaches();

				ManageTrail();
			}

			if (Projectile.extraUpdates == 0)
			{
				Projectile.timeLeft++;
				fade -= 0.025f;

				if (fade <= 0)
					Projectile.active = false;
			}
			else
			{
				Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6, 6), 6, null, 0, default, 1.1f);
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (Projectile.extraUpdates > 0)
			{
				Projectile.extraUpdates = 0;
				Projectile.position += oldVelocity;

				if (!Main.dedServ)
					ManageCaches();

				Projectile.velocity = Vector2.Zero;

				SoundEngine.PlaySound(new SoundStyle($"{nameof(StarlightRiver)}/Sounds/Magic/FireHit"), Projectile.Center);
				Helper.PlayPitched("Impacts/AirstrikeImpact", 0.4f, Main.rand.NextFloat(-0.1f, 0.1f));

				SpawnParticles();
			}

			return false;
		}

		private void ManageCaches()
		{
			if (cache == null)
			{
				cache = new List<Vector2>();
				for (int i = 0; i < (int)Projectile.ai[0]; i++)
				{
					cache.Add(Projectile.Center);
				}
			}

			cache.Add(Projectile.Center);

			while (cache.Count > (int)Projectile.ai[0])
			{
				cache.RemoveAt(0);
			}
		}

		private void ManageTrail()
		{
			trail ??= new Trail(Main.instance.GraphicsDevice, (int)Projectile.ai[0], new TriangularTip(4), factor => 14 * fade, factor => new Color(255, 100, 65) * 0.5f * (float)Math.Sqrt(factor.X));

			trail.Positions = cache.ToArray();
			trail.NextPosition = Projectile.Center + Projectile.velocity;
		}

		public void DrawAdditive(SpriteBatch sb)
		{
			Texture2D tex = Request<Texture2D>(AssetDirectory.Assets + "Keys/GlowSoft").Value;

			Color color = Color.OrangeRed;
			for (int i = 0; i < 6; i++)
				sb.Draw(tex, firstPos - Main.screenPosition, null, color, 0, tex.Size() / 2, 1.25f * fade, SpriteEffects.None, 0f);
		}

		private void SpawnParticles()
		{
			for (int i = 0; i < 6; i++) //TODO: Perhaps make spawning this its own method?
			{
				var dust = Dust.NewDustDirect(Projectile.Center - new Vector2(16, 16), 0, 0, DustType<CoachGunDust>());
				dust.velocity = Main.rand.NextVector2Circular(3, 3);
				dust.scale = Main.rand.NextFloat(0.8f, 1.4f);
				dust.alpha = 70 + Main.rand.Next(60);
				dust.rotation = Main.rand.NextFloat(6.28f);
			}

			for (int i = 0; i < 6; i++)
			{
				var dust = Dust.NewDustDirect(Projectile.Center - new Vector2(16, 16), 0, 0, DustType<CoachGunDustTwo>());
				dust.velocity = Main.rand.NextVector2Circular(3, 3);
				dust.scale = Main.rand.NextFloat(0.8f, 1.4f);
				dust.alpha = Main.rand.Next(80) + 40;
				dust.rotation = Main.rand.NextFloat(6.28f);

				Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(25, 25), DustType<CoachGunDustGlow>()).scale = 0.9f;
			}

			for (int i = 0; i < 3; i++)
			{
				Vector2 velocity = Main.rand.NextFloat(6.28f).ToRotationVector2() * Main.rand.NextFloat(2, 3);
				var proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ProjectileType<CoachGunEmber>(), 0, 0, 255);
				proj.friendly = false;
				proj.hostile = true;
				proj.scale = Main.rand.NextFloat(0.85f, 1.15f);
			}

			var proj2 = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ProjectileType<ConstructRing>(), (int)(Projectile.damage * (Main.expertMode || Main.masterMode ? 0.3f : 1)), 0);
			(proj2.ModProjectile as ConstructRing).finalRadius = 60;

			for (int i = 0; i < 6; i++)
			{
				Vector2 vel = Main.rand.NextFloat(6.28f).ToRotationVector2();
				var dust = Dust.NewDustDirect(Projectile.Center - new Vector2(16, 16) + vel * Main.rand.Next(70), 0, 0, DustType<CoachGunDustFive>());
				dust.velocity = vel * Main.rand.Next(4);
				dust.scale = Main.rand.NextFloat(0.25f, 0.5f);
				dust.alpha = 70 + Main.rand.Next(60);
				dust.rotation = Main.rand.NextFloat(6.28f);
			}

			for (int i = 0; i < 3; i++)
			{
				Vector2 dir = -(Projectile.rotation - 1.57f).ToRotationVector2().RotatedByRandom(1.57f) * Main.rand.NextFloat(5);
				/*int dustID = Dust.NewDust(Projectile.Center, 2, 2, ModContent.DustType<MagmaGunDust>(), dir.X, dir.Y);
                Main.dust[dustID].noGravity = false;*/

				Gore.NewGoreDirect(Projectile.GetSource_FromThis(), Projectile.Center - Projectile.velocity, dir, StarlightRiver.Instance.Find<ModGore>("MagmiteGore").Type, Main.rand.NextFloat(0.5f, 0.7f));
			}
		}
	}

	internal class ConstructRing : ModProjectile
	{
		public override string Texture => AssetDirectory.BreacherItem + "OrbitalStrike";

		public int finalRadius = 60;

		private float Progress => 1 - Projectile.timeLeft / 5f;
		private float Radius => finalRadius * (float)Math.Sqrt(Math.Sqrt(Progress));

		public override void SetDefaults()
		{
			Projectile.width = 80;
			Projectile.height = 80;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 5;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Explosion");
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			Vector2 line = targetHitbox.Center.ToVector2() - Projectile.Center;
			line.Normalize();
			line *= Radius;

			if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + line))
				return true;

			return false;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			return false;
		}
	}
}