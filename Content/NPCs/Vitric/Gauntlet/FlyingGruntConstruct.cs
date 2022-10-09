using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Core;
using System;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.NPCs.Vitric.Gauntlet
{
	internal enum AttackPhase
	{
		charging = 0,
		slowing = 1,
		swinging = 2,
	}

	internal class FlyingGruntConstruct : VitricConstructNPC
	{
		public override string Texture => AssetDirectory.GauntletNpc + "FlyingGruntConstruct";

		private const int XFRAMES = 5;

		public int xFrame = 0;
		public int yFrame = 0;

		public bool attacking = false;

		private NPC archerPartner = default;

		private Vector2 movementTarget = Vector2.Zero;
		public Vector2 oldPosition = Vector2.Zero;

		private float bobCounter = 0f;

		private AttackPhase attackPhase = AttackPhase.charging;

		private int frameCounter = 0;

		private int attackCooldown = 0;

		private int swingDirection = 1;

		public bool doingPelterCombo = false;
		public NPC pelterPartner = default;
		public bool pelterComboCharging = false;
		public bool readyForPelterArrow = false;
		public bool hitPelterArrow = false;

		private Player target => Main.player[NPC.target];

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Flying Grunt Construct");
			Main.npcFrameCount[NPC.type] = 19;
		}

		public override void SetDefaults()
		{
			NPC.width = 40;
			NPC.height = 48;
			NPC.damage = 30;
			NPC.defense = 3;
			NPC.lifeMax = 170;
			NPC.value = 0f;
			NPC.knockBackResist = 0.6f;
			NPC.HitSound = SoundID.Item27 with
			{
				Pitch = -0.3f
			};
			NPC.DeathSound = SoundID.Shatter;
			NPC.noGravity = true;
		}

		public override void OnSpawn(IEntitySource source)
		{
			movementTarget = oldPosition = NPC.Center;
		}

		public override void SafeAI()
		{
			if (xFrame == 1 && yFrame == 7 && frameCounter == 1) //Dust when the enemy swings it's sword
			{
				for (int i = 0; i < 15; i++)
				{
					Vector2 dustPos = NPC.Center + new Vector2(NPC.spriteDirection * 10, 0) + Main.rand.NextVector2Circular(20, 20);
					Dust.NewDustPerfect(dustPos, DustType<Cinder>(), Vector2.Normalize(NPC.velocity).RotatedByRandom(0.2f) * Main.rand.NextFloat(0.5f, 1f) * 12f, 0, new Color(255, 150, 50), Main.rand.NextFloat(0.75f, 1.25f)).noGravity = false;
				}
			}

			attackCooldown--;
			bobCounter += 0.02f;

			NPC.TargetClosest(true);

			if (PelterComboLogic())
				return;

			if (archerPartner == default || !archerPartner.active)
			{
				archerPartner = Main.npc.Where(x =>
				x.active &&
				x.type == ModContent.NPCType<FlyingPelterConstruct>() &&
				x.Distance(NPC.Center) < 800 &&
				(x.ModNPC as FlyingPelterConstruct).pairedGrunt == default).OrderBy(x =>
				x.Distance(NPC.Center)).FirstOrDefault();
			}

			if (!attacking)
			{
				if (archerPartner == default)
					IdleBehavior();
				else
					PairedBehavior();

				if (NPC.Distance(target.Center) < 300 && attackCooldown <= 0)
					attacking = true;

				AnimateIdle();
				attackPhase = AttackPhase.charging;
			}
			else
				AttackBehavior();

			NPC.velocity.X *= 1.05f;
		}

		public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
		{
			position.X -= 16 * NPC.spriteDirection;
			position.Y += 10;
			return true;
		}

		public override void FindFrame(int frameHeight)
		{
			int frameWidth = 69;
			NPC.frame = new Rectangle(xFrame * frameWidth, yFrame * frameHeight, frameWidth, frameHeight);
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D mainTex = Request<Texture2D>(Texture).Value;
			Texture2D glowTex = Request<Texture2D>(Texture + "_Glow").Value;

			DrawConstruct(mainTex, glowTex, spriteBatch, screenPos, drawColor, NPC.IsABestiaryIconDummy ? new Vector2(-16, 0) : Vector2.Zero, true);
			return false;
		}

		private void DrawConstruct(Texture2D mainTex, Texture2D glowTex, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor, Vector2 offset, bool drawGlowTex)
		{

			int frameWidth = mainTex.Width / XFRAMES;
			int frameHeight = mainTex.Height / Main.npcFrameCount[NPC.type];

			SpriteEffects effects = SpriteEffects.None;
			var origin = new Vector2(frameWidth / 2.5f, frameHeight * 0.4f);

			if (xFrame == 2)
				origin.Y -= 1;

			if (xFrame == 0)
				origin.Y += 1;

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

		public override void HitEffect(int hitDirection, double damage)
		{
			attacking = true;
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

		public override bool CanHitPlayer(Player target, ref int cooldownSlot)
		{
			if (xFrame == 1 && yFrame >= 7)
				return base.CanHitPlayer(target, ref cooldownSlot);

			return false;
		}

		private void IdleBehavior()
		{
			if (GoToPos(movementTarget, oldPosition))
			{
				oldPosition = NPC.Center;
				movementTarget = Main.rand.NextVector2Circular(500, 400);
				movementTarget.Y *= -Math.Sign(movementTarget.Y);
				movementTarget += target.Center;
			}
		}

		private void AttackBehavior()
		{
			if (attackCooldown > 0)
			{
				attacking = false;
				return;
			}

			movementTarget = NPC.Center + Vector2.One;
			oldPosition = NPC.Center;
			Vector2 direction = NPC.DirectionTo(target.Center);

			switch (attackPhase)
			{
				case AttackPhase.charging:

					AnimateIdle();
					NPC.velocity = Vector2.Lerp(NPC.velocity, direction.RotatedByRandom(0.6f) * 10, 0.05f);

					if (NPC.Distance(target.Center) < 200)
						attackPhase = AttackPhase.slowing;

					break;

				case AttackPhase.slowing:

					NPC.velocity *= 0.8f;

					if (NPC.velocity.Length() < 2)
					{
						frameCounter = 0;
						attackPhase = AttackPhase.swinging;
					}

					break;

				case AttackPhase.swinging:

					xFrame = 1;
					frameCounter++;

					if (frameCounter > 4)
					{
						frameCounter = 0;

						if (yFrame < 11)
							yFrame++;
						else
						{
							attacking = false;
							attackCooldown = 200;
							xFrame = 0;
							frameCounter = 0;
							yFrame = 0;
						}

						if (yFrame == 7)
						{
							NPC.velocity = direction * 25;
							swingDirection = Math.Sign(NPC.velocity.X);
						}
					}

					if (yFrame >= 7)
					{
						NPC.velocity *= 0.92f;
						NPC.spriteDirection = swingDirection;
					}

					break;
			}
		}

		private void PairedBehavior()
		{
			var potentialPos = Vector2.Lerp(archerPartner.Center, target.Center, 0.5f);

			if (GoToPos(movementTarget, oldPosition) && potentialPos.Distance(NPC.Center) > 60)
			{
				oldPosition = NPC.Center;
				movementTarget = Vector2.Lerp(archerPartner.Center, target.Center, 0.5f);
				NPC.velocity.X = 0;
				NPC.velocity.Y = (float)Math.Cos(bobCounter) * 0.15f;
			}
		}

		/// <summary>
		/// Attempts to navigate to the given position 
		/// </summary>
		/// <param name="pos"> the destination for the NPC </param>
		/// <param name="oldPos"> the point at which the NPC started it's journey </param>
		/// <returns> If the enemy has reached it's destination </returns>
		private bool GoToPos(Vector2 pos, Vector2 oldPos)
		{
			float distance = pos.X - oldPos.X;
			float progress = MathHelper.Clamp((NPC.Center.X - oldPos.X) / distance, 0, 1);

			Vector2 dir = NPC.DirectionTo(pos);

			if (NPC.Distance(pos) > 7 && !NPC.collideY && !NPC.collideX)
			{
				NPC.velocity = dir * ((float)Math.Sin(progress * 3.14f) + 0.1f) * 5;
				NPC.velocity.Y += (float)Math.Cos(bobCounter) * 0.15f;
				return false;
			}

			NPC.velocity.Y = (float)Math.Cos(bobCounter) * 0.15f;
			return true;
		}

		private void AnimateIdle()
		{
			xFrame = 0;
			frameCounter++;
			if (frameCounter > 3)
			{
				frameCounter = 0;
				yFrame++;
				yFrame %= 7;
			}
		}

		private bool PelterComboLogic()
		{
			if (!ableToDoCombo)
				return false;

			if (doingPelterCombo)
			{
				if (pelterPartner == default || pelterPartner == null || !pelterPartner.active)
				{
					xFrame = 0;
					yFrame = 0;
					pelterPartner = default;
					doingPelterCombo = false;
					readyForPelterArrow = false;
					pelterComboCharging = false;
					hitPelterArrow = false;
					return false;
				}

				if (hitPelterArrow)
				{
					xFrame = 4;
					frameCounter++;

					if (frameCounter > 4)
					{
						frameCounter = 0;
						yFrame++;

						if (yFrame == 7)
						{
							xFrame = 0;
							yFrame = 0;

							oldPosition = NPC.Center;
							pelterPartner = default;
							doingPelterCombo = false;
							readyForPelterArrow = false;
							pelterComboCharging = false;
							hitPelterArrow = false;
							return false;
						}
					}

					return true;
				}

				NPC.spriteDirection = Math.Sign(target.Center.X - NPC.Center.X);

				if (!readyForPelterArrow)
				{
					if (!pelterComboCharging) //go to combo spot
					{
						var posToGoTo = new Vector2(MathHelper.Lerp(target.Center.X, pelterPartner.Center.X, 0.7f), pelterPartner.Center.Y - 300);

						if (GoToPos(posToGoTo, oldPosition))
						{
							oldPosition = NPC.Center;
							pelterComboCharging = true;
						}

						AnimateIdle();
					}
					else //charge up animation
					{
						NPC.velocity *= 0.9f;
						xFrame = 2;
						frameCounter++;

						if (frameCounter > 4)
						{
							frameCounter = 0;
							yFrame++;

							if (yFrame == 13)
							{
								xFrame = 3;
								yFrame = 0;
								readyForPelterArrow = true;
							}
						}
					}
				}
				else //ready to hit arrow!
				{
					NPC.velocity *= 0.9f;
					Vector2 arrowPos = NPC.Center + new Vector2(NPC.spriteDirection * 20, 10);
					xFrame = 3;
					frameCounter++;
					if (frameCounter > 3)
					{
						frameCounter = 0;
						yFrame++;
						yFrame %= 6;
					}

					Projectile potentialArrow = Main.projectile.Where(p =>
					p.active &&
					p.type == ProjectileType<PelterConstructArrow>() &&
					p.Distance(arrowPos) < 20).OrderBy(p => p.Distance(NPC.Center + new Vector2(NPC.spriteDirection * 20, 0))).FirstOrDefault();

					if (potentialArrow != default)
					{
						xFrame = 4;
						yFrame = 0;
						frameCounter = 0;
						hitPelterArrow = true;
						potentialArrow.active = false;

						for (float i = -1; i < 1.1f; i += 1)
						{
							Vector2 arrowVel = arrowPos.DirectionTo(target.Center).RotatedBy(i / 3f) * 20;
							int damage = (int)(NPC.damage * (Main.expertMode || Main.masterMode ? 0.3f : 1));
							var proj = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), arrowPos, arrowVel, ProjectileType<PelterConstructArrow>(), damage, NPC.knockBackResist);

							for (int k = 0; k < 15; k++)
							{
								Vector2 dustPos = arrowPos + Main.rand.NextVector2Circular(10, 10);
								Dust.NewDustPerfect(dustPos, DustType<Glow>(), arrowPos.DirectionTo(target.Center).RotatedByRandom(0.7f + i) * Main.rand.NextFloat(0.1f, 1f) * 4f, 0, new Color(255, 150, 50), Main.rand.NextFloat(0.75f, 1.25f)).noGravity = true;
							}
						}
					}
				}

				return true;
			}

			return false;
		}

		public override void DrawHealingGlow(SpriteBatch spriteBatch)
		{
			spriteBatch.End();
			spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);

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
			spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
		}

		public override void ModifyHoverBoundingBox(ref Rectangle boundingBox)
		{
			int offset = 25 * NPC.direction;
			boundingBox = new Rectangle(boundingBox.X - offset, boundingBox.Y + 15, (int)(boundingBox.Width * 0.8f), (int)(boundingBox.Height * 1.25f));
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				Bestiary.SLRSpawnConditions.VitricDesert,
				new FlavorTextBestiaryInfoElement("One of the Glassweaver's constructs. An already formidable duelist, made airborne - speed is war.")
			});
		}
	}
}