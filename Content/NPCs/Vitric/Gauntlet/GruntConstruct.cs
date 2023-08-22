using StarlightRiver.Content.Dusts;
using StarlightRiver.Helpers;
using System;
using System.Linq;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.NPCs.Vitric.Gauntlet
{
	internal class GruntConstruct : VitricConstructNPC
	{
		private const int XFRAMES = 4; //TODO: Swap to using NPC.Frame

		private int xFrame = 0;
		private int yFrame = 0;
		private int frameCounter = 0;

		private int savedDirection = 0;

		private int xPosToBe = 0;

		private bool attacking = false;
		private int attackCooldown = 0;

		private bool idling = false;

		public bool doingCombo = false;
		private bool comboJumped = false;
		private bool comboJumpedTwice = false;
		private NPC partner = default;
		private int comboDirection = 0;

		private float unboundRotation;

		private float unboundRollRotation = 6.28f;

		private int cooldownDuration = 80;
		private float maxSpeed = 5;
		private float acceleration = 0.3f;

		public bool doingJuggernautCombo = false;
		public bool juggernautComboLaunched = false;
		public NPC juggernautPartner = default;

		private Player Target => Main.player[NPC.target];

		public override string Texture => AssetDirectory.GauntletNpc + "GruntConstruct";

		public override Vector2 PreviewOffset => new(8, 6);

		public override void Load()
		{
			for (int k = 1; k <= 17; k++)
			{
				GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, AssetDirectory.VitricNpc + "Gore/ConstructGore" + k);
			}

			for (int j = 1; j <= 3; j++)
			{
				GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, AssetDirectory.VitricNpc + "Gore/GruntSwordGore" + j);
			}
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Grunt Construct");
			Main.npcFrameCount[NPC.type] = 15;
		}

		public override void SetDefaults()
		{
			NPC.width = 30;
			NPC.height = 48;
			NPC.damage = 30;
			NPC.defense = 3;
			NPC.lifeMax = 150;
			NPC.value = 0f;
			NPC.knockBackResist = 0.2f;
			NPC.HitSound = new SoundStyle($"{nameof(StarlightRiver)}/Sounds/Impacts/IceHit") with { PitchVariance = 0.3f };
			NPC.DeathSound = new SoundStyle($"{nameof(StarlightRiver)}/Sounds/Impacts/EnergyBreak") with { PitchVariance = 0.3f };
			cooldownDuration = Main.rand.Next(65, 90); //TODO: wtf is this
			maxSpeed = Main.rand.NextFloat(4.5f, 5.5f);
			acceleration = Main.rand.NextFloat(0.22f, 0.35f);
		}

		public override void OnSpawn(IEntitySource source)
		{
			FindFrame(82);
		}

		public override void FindFrame(int frameHeight)
		{
			int frameWidth = 116;
			NPC.frame = new Rectangle(xFrame * frameWidth, yFrame * frameHeight + 2, frameWidth, frameHeight);
		}

		public override void SafeAI() //TODO: Document snippets with their intended behavior
		{
			if (xFrame == 2 && yFrame == 6 && frameCounter == 1 && Main.netMode != NetmodeID.Server) //Dust when the enemy swings it's sword
			{
				for (int i = 0; i < 15; i++)
				{
					Vector2 dustPos = NPC.Center + new Vector2(NPC.spriteDirection * 40, 0) + Main.rand.NextVector2Circular(20, 20);
					Dust.NewDustPerfect(dustPos, DustType<Cinder>(), Vector2.Normalize(NPC.velocity).RotatedByRandom(0.2f) * Main.rand.NextFloat(0.5f, 1f) * 12f, 0, new Color(255, 150, 50), Main.rand.NextFloat(0.75f, 1.25f)).noGravity = false;
				}
			}

			NPC.TargetClosest(false);
			Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);
			attackCooldown--;

			unboundRotation *= 0.9f;

			if (Math.Abs(unboundRotation) < 0.4f)
				unboundRotation = 0;

			if (!juggernautComboLaunched || NPC.velocity.Y > 0)
				unboundRollRotation = MathHelper.Lerp(unboundRollRotation, 6.28f, 0.2f);

			NPC.rotation = unboundRotation + unboundRollRotation * Math.Sign(NPC.velocity.X);

			if (ComboBehavior() || JuggernautComboBehavior())
				return;

			if (Math.Abs(Target.Center.X - NPC.Center.X) < 600 && !idling)
			{
				if (Math.Abs(Target.Center.X - NPC.Center.X) < 100 || attacking)
				{
					if (attackCooldown < 0)
					{
						attacking = true;
						AttackBehavior();
						return;
					}
				}

				NPC closestPelter = Main.npc.Where(x => //TODO: Same as shielder combo, cache partner
				x.active &&
				x.type == NPCType<PelterConstruct>() &&
				NPC.Distance(x.Center) < 600).OrderBy(x => NPC.Distance(x.Center)).FirstOrDefault();

				if (closestPelter != default && !attacking)
				{
					xPosToBe = (int)MathHelper.Lerp(closestPelter.Center.X, Target.Center.X, 0.8f);

					if (Math.Abs(xPosToBe - NPC.Center.X) < 25 || idling)
					{
						idling = true;
						IdleBehavior();
						return;
					}
				}
				else
				{
					xPosToBe = (int)Target.Center.X;
				}

				float xDir = xPosToBe - NPC.Center.X;
				int xSign = Math.Sign(xDir);

				if (xFrame != 1)
				{
					frameCounter = 0;
					yFrame = 0;
					xFrame = 1;
				}

				frameCounter++;

				if (frameCounter > 3)
				{
					frameCounter = 0;
					yFrame++;
					yFrame %= 8;
				}

				NPC.velocity.X += acceleration * xSign;
				NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -maxSpeed, maxSpeed);

				if (NPC.collideX && NPC.velocity.Y == 0)
					NPC.velocity.Y = -8;

				NPC.spriteDirection = xSign;

			}
			else
			{
				IdleBehavior();
			}
		}

		public override void HitEffect(NPC.HitInfo hit)
		{
			if (NPC.life <= 0 && Main.netMode != NetmodeID.Server)
			{
				for (int i = 0; i < 9; i++)
				{
					Dust.NewDustPerfect(NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), DustType<Dusts.Cinder>(), Main.rand.NextVector2Circular(3, 3), 0, new Color(255, 150, 50), Main.rand.NextFloat(0.75f, 1.25f)).noGravity = false;
				}

				for (int k = 1; k <= 12; k++)
				{
					Gore.NewGoreDirect(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), Main.rand.NextVector2Circular(3, 3), Mod.Find<ModGore>("ConstructGore" + k).Type);
				}

				for (int j = 1; j <= 3; j++)
				{
					Gore.NewGoreDirect(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), Main.rand.NextVector2Circular(3, 3), Mod.Find<ModGore>("GruntSwordGore" + j).Type);
				}
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D mainTex = Request<Texture2D>(Texture).Value;
			Texture2D glowTex = Request<Texture2D>(Texture + "_Glow").Value;

			DrawConstruct(mainTex, glowTex, spriteBatch, screenPos, drawColor, Vector2.Zero, true);
			return false;
		}

		private void DrawConstruct(Texture2D mainTex, Texture2D glowTex, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor, Vector2 offset, bool drawGlowTex)
		{
			int frameWidth = mainTex.Width / XFRAMES;
			int frameHeight = mainTex.Height / Main.npcFrameCount[NPC.type];

			SpriteEffects effects = SpriteEffects.None;
			var origin = new Vector2(frameWidth / 4, frameHeight * 0.75f - 8);

			if (xFrame == 2)
				origin.Y -= 2;

			if (xFrame == 0)
				origin.Y += 2;

			if (NPC.spriteDirection != 1)
			{
				effects = SpriteEffects.FlipHorizontally;
				origin.X = frameWidth - origin.X;
			}

			var slopeOffset = new Vector2(0, NPC.gfxOffY);
			spriteBatch.Draw(mainTex, offset + slopeOffset + NPC.Center - screenPos, NPC.frame, drawColor, NPC.rotation, origin, NPC.scale, effects, 0f);

			if (drawGlowTex)
				spriteBatch.Draw(glowTex, offset + slopeOffset + NPC.Center - screenPos, NPC.frame, Color.White, NPC.rotation, origin, NPC.scale, effects, 0f);
		}

		public override bool CanHitPlayer(Player target, ref int cooldownSlot)
		{
			if (xFrame == 2 && yFrame >= 6) //TODO: Change to being based off of state directly
				return base.CanHitPlayer(target, ref cooldownSlot);

			if (xFrame == 3)
				return base.CanHitPlayer(target, ref cooldownSlot);

			return false;
		}

		private void IdleBehavior()
		{
			NPC.velocity.X *= 0.9f;

			if (xFrame != 0)
			{
				frameCounter = 0;
				yFrame = 0;
				xFrame = 0;
			}

			frameCounter++;
			if (frameCounter > 3)
			{
				frameCounter = 0;
				if (yFrame < 14)
					yFrame++;
				else
					idling = false;
			}
		}

		private void AttackBehavior()
		{
			if (xFrame != 2)
			{
				NPC.spriteDirection = Math.Sign(Target.Center.X - NPC.Center.X);
				frameCounter = 0;
				yFrame = 0;
				xFrame = 2;
			}

			if (yFrame >= 6)
				NPC.spriteDirection = Math.Sign(NPC.velocity.X);

			frameCounter++;

			if (yFrame < 6)
				NPC.velocity.X *= 0.9f;
			else
				NPC.velocity.X *= 0.96f;

			if (frameCounter > 3)
			{
				frameCounter = 0;

				if (yFrame < 13)
				{
					yFrame++;
				}
				else
				{
					attackCooldown = cooldownDuration;
					attacking = false;
					yFrame = 0;
					xFrame = 1;
				}

				if (yFrame == 6)
				{
					NPC.velocity.X = NPC.spriteDirection * 17;
					NPC.velocity.Y = -3;
					Helper.PlayPitched("Effects/HeavyWhooshShort", Main.rand.NextFloat(0.2f, 0.3f), Main.rand.NextFloat(0.5f, 0.8f), NPC.Center);
				}
			}
		}

		private bool ComboBehavior() //returns true if combo is being done
		{
			if (!ableToDoCombo)
				return false;

			if (partner == default || !SuitablePartner(partner))
			{
				NPC tempPartner = Main.npc.Where(SuitablePartner).OrderBy(x => NPC.Distance(x.Center)).FirstOrDefault();

				if (tempPartner != default && !doingCombo)
				{
					doingCombo = true;
					partner = tempPartner;
					(partner.ModNPC as ShieldConstruct).bounceCooldown = 300;
				}
			}

			if (doingCombo)
			{
				if (partner.active && (partner.ModNPC as ShieldConstruct).Guarding)
				{
					if (!comboJumpedTwice)
					{
						if (xFrame != 1)
						{
							frameCounter = 0;
							yFrame = 0;
							xFrame = 1;
						}

						frameCounter++;

						if (frameCounter > 3)
						{
							frameCounter = 0;
							yFrame++;
							yFrame %= 8;
						}
					}
					else
					{
						if (xFrame != 2)
						{
							frameCounter = 0;
							yFrame = 3;
							xFrame = 2;
						}

						if (NPC.velocity.Y > 0)
						{
							frameCounter++;

							if (frameCounter > 3)
							{
								frameCounter = 0;

								if (yFrame < 13)
									yFrame++;
							}
						}
					}

					if (Math.Abs(NPC.Center.X - partner.Center.X) < 110 && !comboJumped)
					{
						NPC.velocity = ArcVelocityHelper.GetArcVel(NPC.Bottom, partner.Top + new Vector2(partner.spriteDirection * 15, 0), 0.1f, 120, 350);
						comboJumped = true;

					}

					if (comboJumped)
					{
						NPC.velocity.X *= 1.05f;

						if (NPC.collideY && NPC.velocity.Y == 0)
						{
							comboJumped = false;
							comboJumpedTwice = false;
							doingCombo = false;
						}
						else
						{
							if (NPC.velocity.Y > 0 && NPC.Center.Y > (partner.Top.Y + 5) && !comboJumpedTwice)
							{
								comboDirection = NPC.spriteDirection;
								partner.velocity.X = -1 * comboDirection;
								NPC.velocity = ArcVelocityHelper.GetArcVel(NPC.Center, Target.Center + new Vector2(NPC.spriteDirection * 15, 0), 0.2f, 120, 250);
								NPC.velocity.X *= 2f;
								unboundRotation = -6.28f * NPC.spriteDirection * 0.95f;
								comboJumpedTwice = true;

								//TODO: check if this is going too fast in multiplayer cause of the extraupdates non sync
								if (Main.netMode != NetmodeID.MultiplayerClient)
								{
									var ring = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Bottom, NPC.Bottom.DirectionTo(partner.Center), ProjectileType<Items.Vitric.IgnitionGauntlets.IgnitionGauntletsImpactRing>(), 0, 0, Target.whoAmI, Main.rand.Next(25, 35), NPC.Center.DirectionTo(partner.Center).ToRotation());
									ring.extraUpdates = 0;
								}
							}
						}
					}

					if (comboJumpedTwice)
					{
						NPC.spriteDirection = comboDirection;
					}
					else
					{
						NPC.velocity.X += NPC.spriteDirection * 0.5f;
						NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -maxSpeed, maxSpeed);
					}
				}
				else
				{
					comboJumped = false;
					comboJumpedTwice = false;
					doingCombo = false;
				}

				return true;
			}

			return false;
		}

		private bool JuggernautComboBehavior()
		{
			if (!ableToDoCombo)
				return false;

			if (doingJuggernautCombo)
			{
				if (!juggernautComboLaunched)
				{
					savedDirection = NPC.spriteDirection;

					if (juggernautPartner == null || juggernautPartner == default || !juggernautPartner.active)
					{
						juggernautPartner = default;
						doingJuggernautCombo = false;
						return false;
					}

					if (Math.Abs(juggernautPartner.Center.X + juggernautPartner.direction * 60 - NPC.Center.X) > 20) //Run to partner
					{
						NPC.direction = NPC.spriteDirection = Math.Sign(juggernautPartner.Center.X + juggernautPartner.direction * 80 - NPC.Center.X);

						if (xFrame != 1)
						{
							frameCounter = 0;
							yFrame = 0;
							xFrame = 1;
						}

						frameCounter++;

						if (frameCounter > 3)
						{
							frameCounter = 0;
							yFrame++;
							yFrame %= 8;
						}

						NPC.velocity.X += NPC.spriteDirection * 0.5f;
						NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -maxSpeed, maxSpeed);

						if (NPC.collideX && NPC.velocity.Y == 0)
							NPC.velocity.Y = -8;
					}
					else //Idle in front of partner
					{
						NPC.velocity.X *= 0.9f;

						if (xFrame != 0)
						{
							frameCounter = 0;
							yFrame = 0;
							xFrame = 0;
						}

						frameCounter++;

						if (frameCounter > 3)
						{
							frameCounter = 0;

							if (yFrame < 14)
								yFrame++;
						}
					}
				}
				else //When launched
				{
					NPC.direction = NPC.spriteDirection = savedDirection;
					NPC.velocity.X *= 1.05f;

					if (NPC.velocity.Y < -1 && frameCounter == 0 && Math.Abs(Target.Center.X - NPC.Center.X) > 100) //In ball form
					{
						yFrame = 0;
						xFrame = 3;
						unboundRollRotation += 0.5f;
					}
					else //Slashing
					{
						if (xFrame != 2)
						{
							frameCounter = 0;
							yFrame = 5;
							xFrame = 2;
							unboundRollRotation %= 6.28f;
						}

						frameCounter++;

						if (frameCounter > 3)
						{
							frameCounter = 0;

							if (yFrame < 13)
								yFrame++;
						}

						if (NPC.collideY && yFrame > 5)
						{
							unboundRollRotation %= 6.28f;
							frameCounter = 0;
							xFrame = 1;
							yFrame = 0;
							juggernautPartner = default;
							doingJuggernautCombo = false;
							juggernautComboLaunched = false;
						}
					}
				}

				return true;
			}

			return false;
		}

		private bool SuitablePartner(NPC potentialPartner)
		{
			return potentialPartner.active &&
			potentialPartner.type == NPCType<ShieldConstruct>() &&
			(potentialPartner.ModNPC as ShieldConstruct).Guarding &&
			!(potentialPartner.ModNPC as ShieldConstruct).jumpingUp &&
			!(potentialPartner.ModNPC as ShieldConstruct).stacked &&
			(potentialPartner.ModNPC as ShieldConstruct).bounceCooldown <= 0 &&
			potentialPartner.spriteDirection == NPC.spriteDirection &&
			NPC.Distance(potentialPartner.Center) > 50 &&
			NPC.Distance(potentialPartner.Center) < 600 &&
			Math.Sign(potentialPartner.Center.X - NPC.Center.X) == NPC.spriteDirection;
		}

		public override void DrawHealingGlow(SpriteBatch spriteBatch)
		{
			spriteBatch.End();
			spriteBatch.Begin(default, BlendState.Additive, default, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);

			Texture2D tex = Request<Texture2D>(Texture).Value;
			float sin = 0.5f + (float)Math.Sin(Main.timeForVisualEffects * 0.04f) * 0.5f;
			float distance = sin * 3 + 4;

			for (int i = 0; i < 8; i++)
			{
				float rad = i * 6.28f / 8;
				Vector2 offset = Vector2.UnitX.RotatedBy(rad) * distance + NPC.netOffset;
				Color color = Color.OrangeRed * (1.75f - sin) * 0.7f;

				DrawConstruct(tex, null, spriteBatch, Main.screenPosition, color, offset, false);
			}

			spriteBatch.End();
			spriteBatch.Begin(default, default, default, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				Bestiary.SLRSpawnConditions.VitricDesert,
				new FlavorTextBestiaryInfoElement("One of the Glassweaver's constructs. Uses its small stature and curved blade to menace challengers up close.")
			});
		}
	}
}