using StarlightRiver.Content.Bosses.GlassMiniboss;
using StarlightRiver.Core.Systems.CameraSystem;
using System;
using System.Linq;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.NPCs.Vitric.Gauntlet
{
	internal class JuggernautConstruct : VitricConstructNPC
	{
		private const int XFRAMES = 3; //TODO: Swap to using NPC.Frame
		private readonly float ACCELERATION = 0.2f;
		private readonly float MAX_SPEED = 2;

		private bool doingLaunchCombo = false;
		private NPC launchTarget = default;
		private int launchComboCooldown = 400;

		private int xFrame = 0;
		private int yFrame = 0;
		private int frameCounter = 0;

		private int savedDirection = 1;

		private bool attacking = false;
		private int attackCooldown = 0;
		private int spikeCounter = -1; //if its greater than 0 and divisible by 10, spawn a spike

		public override string Texture => AssetDirectory.GauntletNpc + "JuggernautConstruct";

		private Player Target => Main.player[NPC.target];

		public override Vector2 PreviewOffset => new(0, 6);

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Juggernaut Construct");
			Main.npcFrameCount[NPC.type] = 23;
		}

		public override void SetDefaults()
		{
			NPC.width = 104;
			NPC.height = 82;
			NPC.damage = 40;
			NPC.defense = 6;
			NPC.lifeMax = 350;
			NPC.value = 0f;
			NPC.knockBackResist = 0f;
			NPC.HitSound = new SoundStyle($"{nameof(StarlightRiver)}/Sounds/Impacts/IceHit") with { Pitch = -0.3f, PitchVariance = 0.3f };
			NPC.DeathSound = new SoundStyle($"{nameof(StarlightRiver)}/Sounds/Impacts/EnergyBreak") with { Pitch = -0.3f, PitchVariance = 0.3f };
		}

		public override void OnSpawn(IEntitySource source)
		{
			FindFrame(63);
		}

		public override void SafeAI()
		{
			NPC.TargetClosest(xFrame == 2);
			Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);
			attackCooldown--;

			if (ComboLogic())
				return;

			if (!attacking && attackCooldown <= 0 && Math.Abs(Target.Center.X - NPC.Center.X) < 200)
			{
				attacking = true;
				xFrame = 1;
				yFrame = 0;
				frameCounter = 0;
				savedDirection = NPC.spriteDirection;
			}

			if (attacking)
				AttackBehavior();
			else
				WalkingBehavior();
		}

		public override bool CanHitPlayer(Player target, ref int cooldownSlot)
		{
			return false;
		}

		public override void FindFrame(int frameHeight)
		{
			int frameWidth = 122;
			NPC.frame = new Rectangle(xFrame * frameWidth, yFrame * frameHeight, frameWidth, frameHeight);
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D mainTex = Request<Texture2D>(Texture).Value;
			Texture2D glowTex = Request<Texture2D>(Texture + "_Glow").Value;

			DrawConstruct(mainTex, glowTex, spriteBatch, screenPos, drawColor, NPC.IsABestiaryIconDummy ? new Vector2(8, 8) : Vector2.Zero, true);
			return false;
		}
		private void DrawConstruct(Texture2D mainTex, Texture2D glowTex, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor, Vector2 offset, bool drawGlowTex)
		{
			SpriteEffects effects = SpriteEffects.None;

			int frameHeight = mainTex.Height / Main.npcFrameCount[NPC.type];
			int frameWidth = mainTex.Width / XFRAMES;

			var origin = new Vector2(frameWidth * 0.25f, frameHeight * 0.5f + 3);

			if (NPC.spriteDirection != 1)
			{
				effects = SpriteEffects.FlipHorizontally;
				origin.X = frameWidth - origin.X;
			}

			var slopeOffset = new Vector2(0, NPC.gfxOffY);
			spriteBatch.Draw(mainTex, offset + slopeOffset + NPC.Center - screenPos, NPC.frame, drawColor, NPC.rotation, origin, NPC.scale * 2, effects, 0f);

			if (drawGlowTex)
				spriteBatch.Draw(glowTex, offset + slopeOffset + NPC.Center - screenPos, NPC.frame, Color.White, NPC.rotation, origin, NPC.scale * 2, effects, 0f);
		}

		public override void OnKill()
		{
			if (Main.netMode != NetmodeID.Server)
			{
				for (int i = 0; i < 9; i++)
					Dust.NewDustPerfect(NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), DustType<Dusts.Cinder>(), Main.rand.NextVector2Circular(3, 3), 0, new Color(255, 150, 50), Main.rand.NextFloat(0.75f, 1.25f)).noGravity = false;

				for (int k = 1; k <= 12; k++)
					Gore.NewGoreDirect(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), Main.rand.NextVector2Circular(3, 3), Mod.Find<ModGore>("ConstructGore" + k).Type);
				for (int j = 1; j <= 3; j++)
					Gore.NewGoreDirect(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), Main.rand.NextVector2Circular(3, 3), Mod.Find<ModGore>("GruntSwordGore" + j).Type);
			}
		}

		private void WalkingBehavior()
		{
			float xDir = Target.Center.X - NPC.Center.X;
			int xSign = Math.Sign(xDir);

			xFrame = 2;
			frameCounter++;

			if (frameCounter > 3)
			{
				frameCounter = 0;
				yFrame++;
				yFrame %= 12;
			}

			NPC.velocity.X += ACCELERATION * xSign;
			NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -MAX_SPEED, MAX_SPEED);

			if (NPC.collideX && NPC.velocity.Y == 0)
				NPC.velocity.Y = -8;
			NPC.spriteDirection = xSign;
		}

		private void AttackBehavior()
		{
			NPC.direction = NPC.spriteDirection = savedDirection;
			attackCooldown = 400;
			NPC.velocity.X *= 0.9f;
			xFrame = 1;
			frameCounter++;

			if (frameCounter > 4 && yFrame < 20 || frameCounter > 30)
			{
				frameCounter = 0;
				if (yFrame < 20)
				{
					yFrame++;
				}
				else
				{
					xFrame = 2;
					yFrame = 0;
					attacking = false;
				}

				if (yFrame == 2) //Spawn early since it spawns with a telegraph
					spikeCounter = 30;
				if (yFrame == 15)
					CameraSystem.shake += 8;
			}

			if (spikeCounter >= 0)
			{
				if (spikeCounter % 10 == 0)
				{
					SpawnSpike();
				}

				spikeCounter--;
			}
		}

		private bool ComboLogic()
		{
			if (!ableToDoCombo || attacking)
				return false;

			launchComboCooldown--;

			if (!doingLaunchCombo && launchComboCooldown < 0)
			{
				launchTarget = Main.npc.Where(n =>
				n.active &&
				n.type == ModContent.NPCType<GruntConstruct>() &&
				!(n.ModNPC as GruntConstruct).doingCombo &&
				!(n.ModNPC as GruntConstruct).doingJuggernautCombo &&
				Math.Abs(NPC.Center.X - n.Center.X) < 500).OrderBy(n => Math.Abs(NPC.Center.X - n.Center.X)).FirstOrDefault();
			}

			if (launchTarget != default && !doingLaunchCombo && launchComboCooldown < 0)
			{
				doingLaunchCombo = true;
				launchComboCooldown = 400;
				yFrame = 0;
				frameCounter = 0;
				savedDirection = NPC.spriteDirection;
				var launchTargetModNPC = launchTarget.ModNPC as GruntConstruct;

				launchTargetModNPC.doingJuggernautCombo = true;
				launchTargetModNPC.juggernautPartner = NPC;
			}

			if (doingLaunchCombo)
			{
				if (yFrame < 16 && (launchTarget == null || launchTarget == default || !launchTarget.active))
				{
					launchTarget = default;
					doingLaunchCombo = false;
					return false;
				}

				NPC.direction = NPC.spriteDirection = savedDirection;

				var launchTargetModNPC = launchTarget.ModNPC as GruntConstruct;

				//animation logic
				xFrame = 0;
				frameCounter++;

				if (frameCounter > 4 && yFrame < 22 || frameCounter > 30)
				{
					frameCounter = 0;

					if (yFrame < 22)
					{
						if (yFrame == 13)
						{
							if (Math.Abs(NPC.Center.X + NPC.direction * 60 - launchTarget.Center.X) < 40)
							{
								yFrame++;
							}
						}
						else
						{
							yFrame++;
						}

						if (yFrame == 16)
						{
							CameraSystem.shake += 8;
							launchTargetModNPC.juggernautComboLaunched = true;
							launchTarget.velocity.Y = -6;
							launchTarget.velocity.X = NPC.direction * 18;

							Vector2 ringVel = NPC.DirectionTo(launchTarget.Center);
							var ring = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center + ringVel * 35, ringVel, ProjectileType<Items.Vitric.IgnitionGauntlets.IgnitionGauntletsImpactRing>(), 0, 0, Target.whoAmI, Main.rand.Next(35, 45), ringVel.ToRotation());
							ring.extraUpdates = 0;
						}
					}
					else
					{
						xFrame = 2;
						yFrame = 0;
						doingLaunchCombo = false;
					}
				}

				return true;
			}

			return false;
		}

		private void SpawnSpike()
		{
			float endPositionX = NPC.Center.X + NPC.spriteDirection * 300;
			float startPositionX = NPC.Center.X + NPC.spriteDirection * 110;

			float spikePositionX = MathHelper.Lerp(endPositionX, startPositionX, spikeCounter / 30f);
			float spikePositionY = NPC.Bottom.Y + 16;
			int tries = 30;

			int i = (int)(spikePositionX / 16);
			int j = (int)(spikePositionY / 16);

			while (!Main.tile[i, j].HasTile || !Main.tileSolid[Main.tile[i, j].TileType]) //move down until on a solid tile
			{
				spikePositionY += 16;
				j = (int)(spikePositionY / 16);
				if (tries-- < 0)
					return;
			}

			tries = 20;
			while (Main.tile[i, j].HasTile && Main.tileSolid[Main.tile[i, j].TileType]) //move up until no longer on solid tile
			{
				spikePositionY -= 16;
				j = (int)(spikePositionY / 16);
				if (tries-- < 0)
					return;
			}

			spikePositionY += 32;

			var spikePos = new Vector2(spikePositionX, spikePositionY);
			var raise = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), spikePos, Vector2.Zero, ProjectileType<GlassRaiseSpike>(), (int)(NPC.damage * (Main.expertMode || Main.masterMode ? 0.3f : 1)), 1f, Main.myPlayer, -20, 1 - spikeCounter / 30f);
			raise.direction = NPC.spriteDirection;
			raise.scale = 0.65f;

			raise.position.X += (1 - raise.scale) * (raise.width / 2); //readjusting width to match scale
			raise.width = (int)(raise.width * raise.scale);
		}
		public override void DrawHealingGlow(SpriteBatch spriteBatch)
		{
			return;

			spriteBatch.End();
			spriteBatch.Begin(default, BlendState.Additive, default, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);

			Texture2D tex = Request<Texture2D>(Texture).Value;
			float sin = 0.5f + (float)Math.Sin(Main.timeForVisualEffects * 0.04f) * 0.5f;
			float distance = sin * 3 + 4;

			for (int i = 0; i < 8; i++)
			{
				float rad = i * 6.28f / 8;
				Vector2 offset = Vector2.UnitX.RotatedBy(rad) * distance;
				Color color = Color.OrangeRed * (1.75f - sin) * 0.7f;

				DrawConstruct(tex, null, spriteBatch, Main.screenPosition, color, offset, false);
			}

			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, default, Main.GameViewMatrix.TransformationMatrix);
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				Bestiary.SLRSpawnConditions.VitricDesert,
				new FlavorTextBestiaryInfoElement("One of the Glassweaver's constructs. This sturdy brute will often use its allies as projectiles when out of smashing range.")
			});
		}
	}
}