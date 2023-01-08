using ReLogic.Content;
using StarlightRiver.Content.GUI;
using StarlightRiver.Core.Loaders.UILoading;
using System;
using System.IO;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Bosses.GlassMiniboss
{
	public partial class Glassweaver : ModNPC
	{
		public static readonly Color GlowDustOrange = new(6255, 108, 0);
		public static readonly Color GlassColor = new(60, 170, 205);

		public bool attackVariant = false;
		public bool disableJumpSound = false;

		float attackType;
		public Vector2 arenaPos;

		internal ref float Phase => ref NPC.ai[0];
		internal ref float GlobalTimer => ref NPC.ai[1];
		internal ref float AttackPhase => ref NPC.ai[2];
		internal ref float AttackTimer => ref NPC.ai[3];

		public override string Texture => AssetDirectory.Glassweaver + Name;

		public override string BossHeadTexture => AssetDirectory.Glassweaver + Name + "_BossHead";

		//Phase tracking utils
		public enum Phases
		{
			SpawnEffects,
			DespawnEffects,
			JumpToBackground,
			GlassGauntlet,
			ReturnToForeground,
			DirectPhase,
			DeathEffects
		}

		public enum AttackTypes
		{
			None,
			Jump,
			SpinJump,
			TripleSlash,
			MagmaSpear,
			Whirlwind,
			JavelinRain,
			GlassRaise,
			BigBrightBubble
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Glassweaver");
			NPCID.Sets.TrailCacheLength[Type] = 10;
			NPCID.Sets.TrailingMode[Type] = 1;
			NPCID.Sets.ShouldBeCountedAsBoss[Type] = true;
			NPCID.Sets.BossBestiaryPriority.Add(Type);

			var drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers(0)
			{

			};
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
		}

		public override void SetDefaults()
		{
			NPC.width = 82;
			NPC.height = 75;
			NPC.lifeMax = 1800;
			NPC.damage = 20;
			NPC.aiStyle = -1;
			NPC.noGravity = true;
			NPC.knockBackResist = 0;
			NPC.boss = true;
			NPC.defense = 14;
			NPC.HitSound = SoundID.NPCHit52;
			Music = MusicLoader.GetMusicSlot(Mod, "Sounds/Music/Miniboss");
			NPC.dontTakeDamage = true;
			NPC.npcSlots = 100;
		}

		private SpriteEffects GetSpriteEffects()
		{
			return NPC.direction < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
		}

		public override void BossHeadSpriteEffects(ref SpriteEffects spriteEffects)
		{
			spriteEffects = GetSpriteEffects();
		}

		public override bool CanHitPlayer(Player target, ref int cooldownSlot)
		{
			return false; //no contact damage!
		}

		public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
		{
			NPC.lifeMax = (int)(2000 * bossLifeScale);
		}

		public override bool CheckDead()
		{
			StarlightWorld.Flag(WorldFlags.DesertOpen);

			NPC.life = 1;
			NPC.dontTakeDamage = true;

			return false;
		}

		public override void OnHitByProjectile(Projectile projectile, int damage, float knockback, bool crit)
		{
			Main.NewText($"Boss hit by projectile: [Name: {projectile.ModProjectile?.Name}, Damage: {damage}, Hostile: {projectile.hostile}, Friendly: {projectile.friendly}]");
		}

		public override void AI()
		{
			AttackTimer++;

			Dust.NewDustPerfect(arenaPos, ModContent.DustType<Dusts.BlueStamina>());

			Dust.NewDustPerfect(PickSpot(), ModContent.DustType<Dusts.Stamina>());
			Dust.NewDustPerfect(PickCloseSpot(), ModContent.DustType<Dusts.Void>());
			Dust.NewDustPerfect(PickSpotSelf(), ModContent.DustType<Dusts.LavaSpark>());

			switch (Phase)
			{
				case (int)Phases.SpawnEffects:

					arenaPos = StarlightWorld.vitricBiome.TopLeft() * 16 + new Vector2(0, 80 * 16) + new Vector2(0, 256);
					Phase = (int)Phases.JumpToBackground;
					Projectile.NewProjectile(NPC.GetSource_FromThis(), arenaPos + new Vector2(528 + 48, -46), Vector2.Zero, ProjectileType<GlassweaverDoor>(), Main.myPlayer, 0, NPC.target);
					ResetAttack();

					break;

				case (int)Phases.JumpToBackground:

					if (AttackTimer <= 120)
					{
						SpawnAnimation();
					}
					else
					{
						Phase = (int)Phases.GlassGauntlet;
						ResetAttack();
					}

					break;

				case (int)Phases.GlassGauntlet:

					switch (AttackPhase)
					{
						case 0: GauntletWave0(); break;

						case 1: GauntletWave1(); break;

						case 2: GauntletWave2(); break;

						case 3: GauntletWave3(); break;

						case 4: GauntletWave4(); break;

						case 5: GauntletWave5(); break;

						case 6: GauntletWave6(); break;

						case 7: EndGauntlet(); break;
					}

					break;

				case (int)Phases.ReturnToForeground:

					if (AttackTimer == 1)
						UILoader.GetUIState<TextCard>().Display("Glassweaver", "Worker of the Anvil", null, 240, 1.2f, false);

					JumpBackAnimation();

					break;

				case (int)Phases.DirectPhase:

					NPC.rotation = MathHelper.Lerp(NPC.rotation, 0, 0.33f);

					if (NPC.velocity.Y > 0f && NPC.collideY && !disableJumpSound)
						Helpers.Helper.PlayPitched("GlassMiniboss/RippedSoundJump", 1f, -0.1f, NPC.Center);

					if (AttackTimer == 1)
					{
						AttackPhase++;

						if (AttackPhase > 8)
							AttackPhase = 0;

						attackVariant = Main.rand.NextBool(2);
						NPC.netUpdate = true;
					}

					//target
					//target
					//side specific
					//sweep
					//target

					switch (AttackPhase)
					{
						case 0:
							TripleSlash();
							break;

						case 1:
							Whirlwind();
							break;

						case 2:
							if (attackVariant)
								MagmaSpear();
							else
								JavelinRain();
							break;

						case 3:
							BigBrightBubble();
							break;

						case 4:
							if (attackVariant)
								GlassRaise();
							else
								GlassRaiseAlt();
							break;

						case 5:
							JavelinRain();
							break;

						case 6:
							TripleSlash();
							break;

						case 7:
							if (attackVariant)
								MagmaSpear();
							else
								MagmaSpearAlt();
							break;

						case 8:
							BigBrightBubble();
							break;

						default:
							TripleSlash();
							break;
					}

					break;

				case (int)Phases.DeathEffects:

					NPC.noGravity = false;
					NPC.velocity.X = (arenaPos.X - NPC.Center.X) * 0.2f;

					if (Math.Abs(NPC.Center.X - arenaPos.X) < 5)
					{
						NPC.NewNPC(NPC.GetSource_FromThis(), (int)NPC.Center.X, (int)NPC.Center.Y, NPCType<GlassweaverWaiting>(), 0, 0, 3);
						NPC.active = false;
					}

					break;
			}

			disableJumpSound = false;
		}

		public override bool? CanFallThroughPlatforms()
		{
			return Target.Bottom.Y > NPC.Top.Y;
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write(attackVariant);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			attackVariant = reader.ReadBoolean();
		}

		//i hate this specific thing right here
		public override ModNPC Clone(NPC npc)
		{
			var newNPC = base.Clone(npc) as Glassweaver;
			newNPC.moveTarget = new Vector2();
			newNPC.moveStart = new Vector2();
			newNPC.attackVariant = false;
			newNPC.hammerIndex = -1;
			newNPC.bubbleIndex = -1;
			return newNPC;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Asset<Texture2D> weaver = Request<Texture2D>(AssetDirectory.Glassweaver + Name);
			Asset<Texture2D> weaverGlow = Request<Texture2D>(AssetDirectory.Glassweaver + Name + "Glow");

			if (NPC.IsABestiaryIconDummy)
			{
				Rectangle bestiaryFrame = weaver.Frame(1, 6, 0, 4);
				spriteBatch.Draw(weaver.Value, NPC.Center - screenPos, bestiaryFrame, Color.White, 0, bestiaryFrame.Size() * 0.5f, 1f, 0, 0);
				return false;
			}

			Rectangle frame = weaver.Frame(1, 6, 0, 0);
			frame.X = 0;
			frame.Width = 144;
			const int frameHeight = 152;

			Vector2 origin = frame.Size() * new Vector2(0.5f, 0.5f);
			Vector2 drawPos = new Vector2(0, -35) - screenPos;

			Color baseColor = drawColor;
			var glowColor = new Color(255, 255, 255, 128);

			//gravity frame
			if (NPC.velocity.Y > 0)
				frame.Y = frameHeight * 2;

			switch (Phase)
			{
				case (int)Phases.GlassGauntlet:

					break;

				case (int)Phases.ReturnToForeground:

					break;

				case (int)Phases.DirectPhase:

					switch (attackType)
					{
						case (int)AttackTypes.Jump:

							float jumpProgress = Utils.GetLerpValue(jumpStart, jumpEnd, AttackTimer, true);
							if (jumpProgress < 0.33f || NPC.velocity.Y < 0f)
								frame.Y = frameHeight;

							break;

						case (int)AttackTypes.SpinJump:

							frame.Y = frameHeight * 5;

							break;

						case (int)AttackTypes.TripleSlash:

							if (AttackTimer > 40 && AttackTimer < 240)
							{
								//using a lerp wouldn't look well with the animation, so a little bit of clunk
								if (AttackTimer < slashTimes[2] + 30)
								{
									frame.X = 142;

									if (AttackTimer > slashTimes[2])
										frame.Y = frameHeight * 3;
									else if (AttackTimer > slashTimes[1])
										frame.Y = frameHeight * 2;
									else if (AttackTimer > slashTimes[0])
										frame.Y = frameHeight;
								}
							}

							break;

						case (int)AttackTypes.MagmaSpear:

							if (AttackTimer < 170 && AttackTimer > 10)
							{
								frame.X = 142;
								frame.Y = frameHeight * (4 + (NPC.velocity.Y != 0 ? 0 : 1));
							}

							break;

						case (int)AttackTypes.Whirlwind:

							break;

						case (int)AttackTypes.JavelinRain:

							if (AttackTimer < javelinTime - JAVELIN_SPAWN_TIME + 10)
								frame.Y = frameHeight * 3;
							break;

						case (int)AttackTypes.GlassRaise:

							float hammerTimer = AttackTimer - HAMMER_SPAWN_TIME + 5;

							if (hammerTimer <= hammerTime + 55 && AttackTimer > 50)
							{
								frame.X = 288;
								frame.Width = 180;
								origin.X = frame.Width / 2f;

								if (hammerTimer <= hammerTime * 0.87f)
								{
									frame.Y = 0;
									bool secFrame = hammerTimer >= hammerTime * 0.33f && hammerTimer < hammerTime * 0.66f;

									if (secFrame)
										frame.Y = frameHeight;
								}
								else
								{
									float swingTime = Utils.GetLerpValue(hammerTime * 0.87f, hammerTime * 0.98f, hammerTimer, true);
									frame.Y = frameHeight + frameHeight * (int)(1f + swingTime * 2f);
								}
							}

							break;

						case (int)AttackTypes.BigBrightBubble:

							if (AttackTimer > 50)
							{
								if (AttackTimer < 270)
									frame.Y = frameHeight * 4;
								else if (AttackTimer < BUBBLE_RECOIL_TIME - 60)
									frame.Y = frameHeight;
								else if (AttackTimer < BUBBLE_RECOIL_TIME + 10)
									frame.Y = frameHeight * 5;
							}

							break;
					}

					break;
			}

			spriteBatch.Draw(weaver.Value, NPC.Center + drawPos, frame, baseColor, NPC.rotation, origin, NPC.scale, GetSpriteEffects(), 0);
			spriteBatch.Draw(weaverGlow.Value, NPC.Center + drawPos, frame, glowColor, NPC.rotation, origin, NPC.scale, GetSpriteEffects(), 0);

			return false;
		}
	}
}
