using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.NPCs.Vitric.Gauntlet
{
	internal class GruntConstruct : ModNPC, IGauntletNPC
	{
		public override string Texture => AssetDirectory.GauntletNpc + "GruntConstruct";

		private Player target => Main.player[NPC.target];

		private int XFRAMES = 3;
		private int xFrame = 0;
		private int yFrame = 0;
		private int frameCounter = 0;

		private int xPosToBe = 0;

		private bool attacking = false;
		private int attackCooldown = 0;

		private bool idling = false;

		private bool doingCombo = false;
		private bool comboJumped = false;
		private bool comboJumpedTwice = false;
		private NPC partner = default;
		private int comboDirection = 0;

		private float enemyRotation;

		private int cooldownDuration = 80;
		private float maxSpeed = 5;
		private float acceleration = 0.3f;
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Grunt Construct");
			Main.npcFrameCount[NPC.type] = 15;
		}

		public override void Load()
		{
			for (int k = 1; k <= 17; k++)
				GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, AssetDirectory.VitricNpc + "Gore/ConstructGore" + k);
			for (int j = 1; j <= 3; j++)
				GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, AssetDirectory.VitricNpc + "Gore/GruntSwordGore" + j);
		}

		public override void SetDefaults()
		{
			NPC.width = 30;
			NPC.height = 48;
			NPC.damage = 10;
			NPC.defense = 5;
			NPC.lifeMax = 250;
			NPC.value = 10f;
			NPC.knockBackResist = 0.6f;
			NPC.HitSound = SoundID.Item27 with
			{
				Pitch = -0.3f
			};
			NPC.DeathSound = SoundID.Shatter;
			cooldownDuration = Main.rand.Next(65, 90);
			maxSpeed = Main.rand.NextFloat(4.5f, 5.5f);
			acceleration = Main.rand.NextFloat(0.22f, 0.35f);
		}

		public override void AI()
		{
			if (xFrame == 2 && yFrame == 6 && frameCounter == 1)
			{
				for (int i = 0; i < 15; i++)
				{
					Vector2 dustPos = NPC.Center + new Vector2(NPC.spriteDirection * 40, 0) + Main.rand.NextVector2Circular(20, 20);
					Dust.NewDustPerfect(dustPos, DustType<Dusts.Cinder>(), Vector2.Normalize(NPC.velocity).RotatedByRandom(0.2f) * Main.rand.NextFloat(0.5f, 1f) * 12f, 0, new Color(255, 150, 50), Main.rand.NextFloat(0.75f, 1.25f)).noGravity = false;
				}
			}

			NPC.TargetClosest(false);
			Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);
			attackCooldown--;

			enemyRotation *= 0.9f;
			if (Math.Abs(enemyRotation) < 0.4f)
				enemyRotation = 0;
			NPC.rotation = enemyRotation;

			NPC tempPartner = Main.npc.Where(x =>
			x.active &&
			x.type == ModContent.NPCType<ShieldConstruct>() &&
			(x.ModNPC as ShieldConstruct).guarding &&
			(x.ModNPC as ShieldConstruct).bounceCooldown <= 0 &&
			x.spriteDirection == NPC.spriteDirection &&
			NPC.Distance(x.Center) > 50 &&
			NPC.Distance(x.Center) < 600 &&
			Math.Sign(x.Center.X - NPC.Center.X) == NPC.spriteDirection).OrderBy(x => NPC.Distance(x.Center)).FirstOrDefault();

			if (tempPartner != default && !doingCombo)
			{
				doingCombo = true;
				partner = tempPartner;
				(partner.ModNPC as ShieldConstruct).bounceCooldown = 300;
			}

			if (doingCombo)
			{
				if (partner.active && (partner.ModNPC as ShieldConstruct).guarding)
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
								NPC.velocity = ArcVelocityHelper.GetArcVel(NPC.Center, target.Center + new Vector2(NPC.spriteDirection * 15, 0), 0.2f, 120, 250);
								NPC.velocity.X *= 2f;
								enemyRotation = -6.28f * NPC.spriteDirection * 0.95f;
								comboJumpedTwice = true;

								var ring = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Bottom, NPC.Bottom.DirectionTo(partner.Center), ModContent.ProjectileType<StarlightRiver.Content.Items.Vitric.IgnitionGauntlets.IgnitionGauntletsImpactRing>(), 0, 0, target.whoAmI, Main.rand.Next(25, 35), NPC.Center.DirectionTo(partner.Center).ToRotation());
								ring.extraUpdates = 0;
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

				return;
			}

			if (Math.Abs(target.Center.X - NPC.Center.X) < 400 && !idling)
			{
				if (Math.Abs(target.Center.X - NPC.Center.X) < 100 || attacking)
				{
					if (attackCooldown < 0)
					{
						attacking = true;
						AttackBehavior();
						return;
					}
				}

				NPC closestPelter = Main.npc.Where(x =>
				x.active &&
				x.type == ModContent.NPCType<PelterConstruct>() &&
				NPC.Distance(x.Center) < 600).OrderBy(x => NPC.Distance(x.Center)).FirstOrDefault();

				if (closestPelter != default && !attacking)
				{

					xPosToBe = (int)MathHelper.Lerp(closestPelter.Center.X, target.Center.X, 0.8f);
					if (Math.Abs(xPosToBe - NPC.Center.X) < 25 || idling)
					{
						idling = true;
						IdleBehavior();
						return;
					}
				}
				else
				{
					xPosToBe = (int)target.Center.X;
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
				NPC.spriteDirection = xSign;

			}
			else
			{
				IdleBehavior();
			}
		}

		public override void HitEffect(int hitDirection, double damage)
		{
			if (NPC.life <= 0 && Main.netMode != NetmodeID.Server)
			{
				for (int i = 0; i < 9; i++)
					Dust.NewDustPerfect(NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), DustType<Dusts.Cinder>(), Main.rand.NextVector2Circular(3, 3), 0, new Color(255, 150, 50), Main.rand.NextFloat(0.75f, 1.25f)).noGravity = false;

				for (int k = 1; k <= 12; k++)
					Gore.NewGoreDirect(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), Main.rand.NextVector2Circular(3, 3), Mod.Find<ModGore>("ConstructGore" + k).Type);
				for (int j = 1; j <= 3; j++)
					Gore.NewGoreDirect(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), Main.rand.NextVector2Circular(3, 3), Mod.Find<ModGore>("GruntSwordGore" + j).Type);
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D mainTex = ModContent.Request<Texture2D>(Texture).Value;
			Texture2D glowTex = ModContent.Request<Texture2D>(Texture + "_Glow").Value;

			int frameWidth = mainTex.Width / XFRAMES;
			int frameHeight = mainTex.Height / Main.npcFrameCount[NPC.type];
			var frameBox = new Rectangle(xFrame * frameWidth, yFrame * frameHeight + 2, frameWidth, frameHeight);

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
			Main.spriteBatch.Draw(mainTex, slopeOffset + NPC.Center - screenPos, frameBox, drawColor, NPC.rotation, origin, NPC.scale, effects, 0f);
			Main.spriteBatch.Draw(glowTex, slopeOffset + NPC.Center - screenPos, frameBox, Color.White, NPC.rotation, origin, NPC.scale, effects, 0f);
			return false;
		}

		public override bool CanHitPlayer(Player target, ref int cooldownSlot)
		{
			if (xFrame == 2 && yFrame >= 6)
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
				NPC.spriteDirection = Math.Sign(target.Center.X - NPC.Center.X);
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
				}
			}
		}
	}
}