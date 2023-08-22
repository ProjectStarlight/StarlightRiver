﻿using StarlightRiver.Core.Systems.CameraSystem;
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
	internal class ShieldConstruct : VitricConstructNPC
	{
		private const int XFRAMES = 2;
		private const int MAXSTACK = 4; //How many shielders can stack

		public ref float Timer => ref NPC.ai[0];
		public ref float MaxSpeed => ref NPC.ai[1];
		public ref float Acceleration => ref NPC.ai[2];
		public ref float TimerTickSpeed => ref NPC.ai[3];

		public float bounceCooldown = 0;

		private Vector2 shieldOffset;

		private int xFrame = 0;
		private int yFrame = 0;
		private int frameCounter = 0;

		public bool stacked = false;
		public bool jumpingUp = false;
		public NPC stackPartnerBelow = default;
		public NPC stackPartnerAbove = default;
		public int stacksLeft = 5;
		public int stackCooldown = 0;
		public Vector2 stackOffset = Vector2.Zero; //The offset of the stacker when they first land

		private int savedDirection = 1;

		private int ExplosionTimer = 120;

		private Player Target => Main.player[NPC.target];

		public bool Guarding => Timer > 260;

		public override string Texture => AssetDirectory.GauntletNpc + "ShieldConstruct";

		public override Vector2 PreviewOffset => new(0, 8);

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Shield Construct");
			Main.npcFrameCount[NPC.type] = 8;
		}

		public override void SetDefaults()
		{
			NPC.width = 52;
			NPC.height = 56;
			NPC.damage = 50;
			NPC.defense = 3;
			NPC.lifeMax = 150;
			NPC.value = 0f;
			NPC.knockBackResist = 0.2f;
			NPC.HitSound = new SoundStyle($"{nameof(StarlightRiver)}/Sounds/Impacts/IceHit") with { Pitch = -0.3f, PitchVariance = 0.3f };
			NPC.DeathSound = new SoundStyle($"{nameof(StarlightRiver)}/Sounds/Impacts/EnergyBreak") with { Pitch = -0.3f, PitchVariance = 0.3f };
			NPC.behindTiles = true;
		}

		public override void OnSpawn(IEntitySource source)
		{
			FindFrame(56);
			MaxSpeed = Main.rand.NextFloat(1f, 1.25f);
			Acceleration = Main.rand.NextFloat(0.12f, 0.25f);
			TimerTickSpeed = Main.rand.NextFloat(0.85f, 1f);
			Timer = Main.rand.Next(100);
		}

		public override void SafeAI()
		{
			NPC.TargetClosest(false);

			if (!AnyOtherConstructs())
			{
				ExplosionTimer--;

				if (ExplosionTimer <= 0)
					NPC.Kill();
			}
			else
			{
				ExplosionTimer = 120;
			}

			if (bounceCooldown > 0)
				bounceCooldown--;

			if (StackingComboLogic())
				return;

			if (Timer < 300 || Timer >= 400)
				Timer += TimerTickSpeed;

			Timer %= 500;

			if (Timer > 200)
			{
				float shieldAnimationProgress;
				xFrame = 1;
				yFrame = 0;

				var up = new Vector2(0, -12);
				var down = new Vector2(0, 14);

				NPC.spriteDirection = savedDirection;

				if (Timer < 400)
				{
					if (Timer < 250) //Shield Raising, preparing to slam
					{
						shieldAnimationProgress = EaseFunction.EaseCubicInOut.Ease((Timer - 200) / 50f);
						shieldOffset = up * shieldAnimationProgress;
					}
					else if (Timer <= 260) //Shield lowering towards the ground
					{
						shieldAnimationProgress = EaseFunction.EaseQuarticIn.Ease((Timer - 250) / 10f);
						shieldOffset = Vector2.Lerp(up, down, shieldAnimationProgress);
					}

					if ((int)Timer == 260 && Main.netMode != NetmodeID.Server) //Shield hits the ground
					{
						Helper.PlayPitched("GlassMiniboss/GlassSmash", 0.5f, 0.3f, NPC.Center);
						CameraSystem.shake += 4;

						for (int i = 0; i < 10; i++)
						{
							Dust.NewDustPerfect(NPC.Center + new Vector2(16 * NPC.spriteDirection, 20), DustID.Copper);
							Dust.NewDustPerfect(NPC.Center + new Vector2(16 * NPC.spriteDirection, 20), DustType<Dusts.GlassGravity>());
						}
					}
				}
				else
				{
					if (Timer < 464) //Shield slowly sliding out of the ground
					{
						shieldAnimationProgress = EaseFunction.EaseQuadIn.Ease((Timer - 400) / 64f);
						shieldOffset = Vector2.Lerp(down, new Vector2(0, 4), shieldAnimationProgress);
					}
					else if (Timer < 470) //Shield jolts out of the ground
					{
						shieldAnimationProgress = EaseFunction.EaseQuadOut.Ease((Timer - 464) / 6f);
						shieldOffset = Vector2.Lerp(new Vector2(0, 4), up, shieldAnimationProgress);
					}
					else //Shield lowers back into place
					{
						shieldAnimationProgress = EaseFunction.EaseQuinticInOut.Ease((Timer - 470) / 30f);
						shieldOffset = up * (1 - shieldAnimationProgress);
					}

					if ((int)Timer == 421)
						Helper.PlayPitched("StoneSlide", 0.5f, -1f, NPC.Center);

					if ((int)Timer == 464 && Main.netMode != NetmodeID.Server) //Shield exits the ground
					{
						CameraSystem.shake += 2;

						for (int i = 0; i < 6; i++)
						{
							Dust.NewDustPerfect(NPC.Center + new Vector2(16 * NPC.spriteDirection, 20), DustID.Copper);
							Dust.NewDustPerfect(NPC.Center + new Vector2(16 * NPC.spriteDirection, 20), DustType<Dusts.GlassGravity>());
						}
					}
				}

				if (Guarding && (Math.Sign(NPC.Center.DirectionTo(Target.Center).X) != NPC.spriteDirection || NPC.Distance(Target.Center) > 350) && Timer < 400)
					Timer = 400;

				NPC.velocity.X *= 0.9f;
				return;
			}

			shieldOffset = Vector2.Zero;
			savedDirection = NPC.spriteDirection = Math.Sign(NPC.Center.DirectionTo(Target.Center).X);

			RegularMovement();
		}

		public override void FindFrame(int frameHeight)
		{
			int frameWidth = 46;
			NPC.frame = new Rectangle(xFrame * frameWidth, yFrame * frameHeight + 1, frameWidth, frameHeight);
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D mainTex = Request<Texture2D>(Texture).Value;
			Texture2D glowTex = Request<Texture2D>(Texture + "_Glow").Value;
			Texture2D shieldTex = Request<Texture2D>(Texture + "_Shield").Value;

			if (NPC.IsABestiaryIconDummy)
			{
				DrawConstruct(mainTex, shieldTex, glowTex, spriteBatch, screenPos, Color.White, Vector2.Zero, false);
				return false;
			}

			DrawConstruct(mainTex, shieldTex, glowTex, spriteBatch, screenPos, drawColor, Vector2.Zero, true);

			return false;
		}

		private void DrawConstruct(Texture2D mainTex, Texture2D shieldTex, Texture2D glowTex, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor, Vector2 offset, bool drawGlowTex)
		{
			SpriteEffects effects = SpriteEffects.None;

			var bodyOffset = new Vector2(-6 * NPC.spriteDirection + 4, 9);

			if (NPC.spriteDirection != 1)
				effects = SpriteEffects.FlipHorizontally;

			spriteBatch.Draw(mainTex, offset + bodyOffset + NPC.Center - screenPos, NPC.frame, drawColor, 0f, NPC.frame.Size() / 2 + new Vector2(0, 8), NPC.scale, effects, 0f);

			if (drawGlowTex)
				spriteBatch.Draw(glowTex, offset + bodyOffset + NPC.Center - screenPos, NPC.frame, Color.White, 0f, NPC.frame.Size() / 2 + new Vector2(0, 8), NPC.scale, effects, 0f);

			spriteBatch.Draw(shieldTex, offset + NPC.Center - screenPos + shieldOffset, null, drawColor, 0f, NPC.frame.Size() / 2 + new Vector2(0, 8), NPC.scale, effects, 0f);
		}

		public override bool CanHitPlayer(Player target, ref int cooldownSlot)
		{
			if (Guarding || stacked)
				return base.CanHitPlayer(target, ref cooldownSlot);

			return false;
		}

		public override void ModifyHitByItem(Player player, Item item, ref NPC.HitModifiers modifiers)
		{
			if (Guarding || Math.Sign(NPC.Center.DirectionTo(player.Center).X) == NPC.spriteDirection || stacked)
				modifiers.Knockback *= 0f;

			if (Math.Sign(NPC.Center.DirectionTo(player.Center).X) == NPC.spriteDirection)
			{
				SoundEngine.PlaySound(SoundID.Item27 with { Pitch = 0.1f }, NPC.Center);

				if (Guarding || stacked)
				{
					modifiers.FinalDamage -= int.MaxValue;
					CombatText.NewText(NPC.Hitbox, Color.OrangeRed, "Blocked!");
				}
				else
				{
					modifiers.FinalDamage *= 0.4f;
				}
			}
			else
			{
				SoundEngine.PlaySound(SoundID.Item27 with { Pitch = -0.3f }, NPC.Center);
			}
		}

		public override void ModifyHitByProjectile(Projectile projectile, ref NPC.HitModifiers modifiers)
		{
			if (Guarding || Math.Sign(NPC.Center.DirectionTo(Target.Center).X) == NPC.spriteDirection || stacked)
				modifiers.Knockback *= 0f;

			if (Math.Sign(NPC.Center.DirectionTo(Target.Center).X) == NPC.spriteDirection)
			{
				SoundEngine.PlaySound(SoundID.Item27 with { Pitch = -0.6f }, NPC.Center);

				if (Guarding || stacked)
				{
					modifiers.FinalDamage -= int.MaxValue;
					CombatText.NewText(NPC.Hitbox, Color.OrangeRed, "Blocked!");
				}
				else
				{
					modifiers.FinalDamage *= 0.4f;
				}
			}
			else
			{
				SoundEngine.PlaySound(SoundID.Item27 with { Pitch = -0.3f }, NPC.Center);
			}
		}

		public override void HitEffect(NPC.HitInfo hit)
		{
			if (NPC.life <= 0 && Main.netMode != NetmodeID.Server)
			{
				for (int i = 0; i < 12; i++)
				{
					Dust.NewDustPerfect(NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), DustType<Dusts.Cinder>(), Main.rand.NextVector2Circular(3, 3), 0, new Color(255, 150, 50), Main.rand.NextFloat(0.75f, 1.25f)).noGravity = false;
				}

				for (int k = 1; k <= 17; k++)
				{
					Gore.NewGoreDirect(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), Main.rand.NextVector2Circular(3, 3), Mod.Find<ModGore>("ConstructGore" + k).Type);
				}
			}
		}

		public override void DrawHealingGlow(SpriteBatch spriteBatch)
		{
			spriteBatch.End();
			spriteBatch.Begin(default, BlendState.Additive, default, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);

			Texture2D tex = Request<Texture2D>(Texture).Value;
			Texture2D shieldTex = Request<Texture2D>(Texture + "_Shield").Value;

			float sin = 0.5f + (float)Math.Sin(Main.timeForVisualEffects * 0.04f) * 0.5f;
			float distance = sin * 3 + 4;

			for (int i = 0; i < 8; i++)
			{
				float rad = i * 6.28f / 8;
				Vector2 offset = Vector2.UnitX.RotatedBy(rad) * distance + NPC.netOffset;
				Color color = Color.OrangeRed * (1.75f - sin) * 0.7f;

				DrawConstruct(tex, shieldTex, null, spriteBatch, Main.screenPosition, color, offset, false);
			}

			spriteBatch.End();
			spriteBatch.Begin(default, default, default, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				Bestiary.SLRSpawnConditions.VitricDesert,
				new FlavorTextBestiaryInfoElement("One of the Glassweaver's constructs. Once its spiked shield is dug into the ground, this stalwart protector is immovable.")
			});
		}

		private void RegularMovement() //Movement if it isn't shielding or in a combo
		{
			int xPosToBe = (int)Target.Center.X;

			int velDir = Math.Sign(xPosToBe - NPC.Center.X);

			NPC.velocity.X += Acceleration * velDir;
			NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -MaxSpeed, MaxSpeed);

			if (NPC.velocity.Y == 0)
			{
				if (NPC.collideX)
					NPC.velocity.Y = -8;
				xFrame = 0;
				frameCounter++;

				if (frameCounter % 3 == 0)
					yFrame++;

				yFrame %= Main.npcFrameCount[NPC.type] = 8;
			}
			else
			{
				xFrame = 1;
				yFrame = 1;
			}
		}

		private bool StackingComboLogic() //return true if stacked
		{
			if (!ableToDoCombo)
				return false;

			if (!stacked)
			{
				stacksLeft = MAXSTACK - 1;
				stackCooldown--;
			}

			if (stackPartnerAbove == null || stackPartnerAbove == default || !stackPartnerAbove.active || (stackPartnerAbove.ModNPC as ShieldConstruct).stackPartnerBelow != NPC)
				stackPartnerAbove = default;

			if (stackCooldown > 0)
				return false;

			if (!stacked && !jumpingUp && stackPartnerAbove == default)
			{
				NPC potentialPartner = Main.npc.Where(x =>
				x.active &&
				x.type == NPC.type &&
				x != NPC &&
				Math.Abs(NPC.Center.X - x.Center.X) < 150 &&
				!(x.ModNPC as ShieldConstruct).jumpingUp &&
				(x.ModNPC as ShieldConstruct).stackPartnerAbove == default &&
				(!(x.ModNPC as ShieldConstruct).stacked && (x.ModNPC as ShieldConstruct).Guarding || (x.ModNPC as ShieldConstruct).stacksLeft > 0)
				).OrderBy(x => Math.Abs(NPC.Center.X - x.Center.X) + (x.ModNPC as ShieldConstruct).stacksLeft * 50).FirstOrDefault();

				if (potentialPartner != default)
				{
					stackPartnerBelow = potentialPartner;
					jumpingUp = true;
				}
				else
				{
					return false;
				}
			}

			if (stackPartnerBelow == null || stackPartnerBelow == default || !stackPartnerBelow.active)
			{
				stackPartnerBelow = default;
				jumpingUp = false;
				stacked = false;
				stackCooldown = 300;
				return false;
			}

			var partnerModNPC = stackPartnerBelow.ModNPC as ShieldConstruct;

			if (!partnerModNPC.stacked && !partnerModNPC.Guarding)
			{
				stackPartnerBelow = default;
				jumpingUp = false;
				stacked = false;
				stackCooldown = 300;
				return false;
			}

			partnerModNPC.stackPartnerAbove = NPC;

			NPC.spriteDirection = stackPartnerBelow.spriteDirection;
			stacksLeft = partnerModNPC.stacksLeft - 1;
			Timer = 0;
			shieldOffset = Vector2.Zero;
			xFrame = 1;

			if (jumpingUp)
			{
				yFrame = 1;
				int directionToPartner = Math.Sign(stackPartnerBelow.Center.X - NPC.Center.X);

				NPC.velocity.X *= 1.05f;

				if (NPC.velocity.Y == 0)
					NPC.velocity = ArcVelocityHelper.GetArcVel(NPC.Bottom, stackPartnerBelow.Top + new Vector2(directionToPartner * 15, 0), 0.3f, 120, 850);

				if (NPC.velocity.Y > 0 && Collision.CheckAABBvAABBCollision(NPC.position, NPC.Size, stackPartnerBelow.position, stackPartnerBelow.Size))
				{
					NPC.velocity = Vector2.Zero;
					stackOffset = NPC.Center - stackPartnerBelow.Center;
					jumpingUp = false;
					stacked = true;
				}
				else
				{
					NPC.velocity.Y += 0.1f;
				}

				return true;
			}

			if (stacked)
			{
				NPC.spriteDirection = savedDirection;
				yFrame = 0;
				NPC.velocity = Vector2.Zero;

				int partnersAboveOffset = 3 * GetPartnersAbove();
				shieldOffset = new Vector2(0, -partnersAboveOffset);
				stackOffset = Vector2.Lerp(stackOffset, new Vector2(0, -48 + partnersAboveOffset), 0.1f);
				NPC.Center = stackOffset + stackPartnerBelow.Center;
				return true;
			}

			return false;
		}

		private int GetPartnersAbove()
		{
			if (stackPartnerAbove == default)
				return 0;

			int ret = 1;
			NPC highestPartner = stackPartnerAbove;
			NPC highestPartnerNext = (highestPartner.ModNPC as ShieldConstruct).stackPartnerAbove;

			while (highestPartnerNext != null && highestPartnerNext != default && highestPartnerNext.active && (highestPartnerNext.ModNPC as ShieldConstruct).stacked)
			{
				ret++;
				highestPartner = (highestPartner.ModNPC as ShieldConstruct).stackPartnerAbove;

				if (ret > MAXSTACK)
					return ret;

				highestPartnerNext = (highestPartner.ModNPC as ShieldConstruct).stackPartnerAbove;
			}

			return ret;
		}

		private bool AnyOtherConstructs()
		{
			NPC otherConstruct = Main.npc.Where(x =>
			x.active &&
			x.ModNPC is VitricConstructNPC &&
			x.type != NPCType<ShieldConstruct>() &&
			x.Distance(NPC.Center) < 2000f).FirstOrDefault();

			if (otherConstruct == null || otherConstruct == default)
				return false;
			else
				return true;
		}
	}
}