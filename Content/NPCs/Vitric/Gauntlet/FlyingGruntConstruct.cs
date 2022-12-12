using System;
using System.Linq;
using Terraria.DataStructures;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.NPCs.Vitric.Gauntlet
{

	internal enum AttackPhase
	{
		charging = 0,
		slowing = 1,
		swinging = 2,
	}
	internal class FlyingGruntConstruct : ModNPC, IGauntletNPC
	{
		public override string Texture => AssetDirectory.GauntletNpc + "FlyingGruntConstruct";

		private Player target => Main.player[NPC.target];

		private int XFRAMES = 2;

		private int xFrame = 0;

		private int yFrame = 0;

		public bool attacking = false;

		private NPC archerPartner = default;

		private Vector2 posToBe = Vector2.Zero;

		private Vector2 oldPosition = Vector2.Zero;

		private float bobCounter = 0f;

		private AttackPhase attackPhase = AttackPhase.charging;

		private int frameCounter = 0;

		private int attackCooldown = 0;

		private int swingDirection = 1;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Flying Grunt Construct");
			Main.npcFrameCount[NPC.type] = 12;
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
			NPC.noGravity = true;
		}

		public override void OnSpawn(IEntitySource source)
		{
			posToBe = oldPosition = NPC.Center;
		}

		public override void AI()
		{
			attackCooldown--;
			bobCounter += 0.02f;
			NPC.TargetClosest(true);
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
			{
				AttackBehavior();
			}

			NPC.velocity.X *= 1.05f;
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D mainTex = ModContent.Request<Texture2D>(Texture).Value;
			Texture2D glowTex = ModContent.Request<Texture2D>(Texture + "_Glow").Value;

			int frameWidth = mainTex.Width / XFRAMES;
			int frameHeight = mainTex.Height / Main.npcFrameCount[NPC.type];
			var frameBox = new Rectangle(xFrame * frameWidth, yFrame * frameHeight + 2, frameWidth, frameHeight);

			SpriteEffects effects = SpriteEffects.None;
			var origin = new Vector2(frameWidth / 2.5f, frameHeight * 0.4f);

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

		public override void HitEffect(int hitDirection, double damage)
		{
			attacking = true;
			if (NPC.life <= 0 && Main.netMode != NetmodeID.Server)
			{
				for (int i = 0; i < 9; i++)
					Dust.NewDustPerfect(NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), DustType<Dusts.Cinder>(), Main.rand.NextVector2Circular(3, 3), 0, new Color(255, 150, 50), Main.rand.NextFloat(0.75f, 1.25f)).noGravity = false;

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
			if (GoToPos(posToBe, oldPosition))
			{
				oldPosition = NPC.Center;
				posToBe = Main.rand.NextVector2Circular(500, 400);
				posToBe.Y *= -Math.Sign(posToBe.Y);
				posToBe += target.Center;
			}
		}

		private void AttackBehavior()
		{
			if (attackCooldown > 0)
			{
				attacking = false;
				return;
			}

			posToBe = NPC.Center + Vector2.One;
			oldPosition = NPC.Center;
			Vector2 direction = NPC.DirectionTo(target.Center);

			switch (attackPhase)
			{
				case AttackPhase.charging:

					AnimateIdle();
					NPC.velocity = Vector2.Lerp(NPC.velocity, direction.RotatedByRandom(0.6f) * 10, 0.05f);
					if (NPC.Distance(target.Center) < 200)
					{
						attackPhase = AttackPhase.slowing;
					}

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
						{
							yFrame++;
						}
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
							NPC.velocity = direction * 15;
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
			if (GoToPos(posToBe, oldPosition) && potentialPos.Distance(NPC.Center) > 60)
			{
				oldPosition = NPC.Center;
				posToBe = Vector2.Lerp(archerPartner.Center, target.Center, 0.5f);
				NPC.velocity.X = 0;
				NPC.velocity.Y = (float)Math.Cos(bobCounter) * 0.15f;
			}
		}

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
	}
}