using StarlightRiver.Core.Systems.CameraSystem;
using System;
using System.Linq;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Bosses.GlassMiniboss
{
	public partial class Glassweaver : ModNPC
	{
		public const int BUBBLE_RECOIL_TIME = 300;
		private const int JAVELIN_SPAWN_TIME = 30;
		private const int HAMMER_SPAWN_TIME = 90;
		private const int SIDE_OFFSET_X = 480;
		private const int SIDE_OFFSET_Y = 30;
		private const int SHORT_OFFSET_X = 130;
		private const int SHORT_OFFSET_Y = -10;

		private readonly int[] slashTimes = new int[] { 70, 125, 160 };

		public Vector2 moveStart;
		public Vector2 moveTarget;

		private int javelinTime;
		private int hammerTime;

		public int bubbleIndex;
		public int whirlIndex;
		public int spearIndex;

		private int jumpStart;
		private int jumpEnd;

		Player Target => Main.player[NPC.target];
		private int Direction => NPC.Center.X > arenaPos.X ? -1 : 1;

		private void ResetAttack()
		{
			AttackTimer = 0;
			TryEndFight();
			NPC.netUpdate = true;
		}

		private void TryEndFight()
		{
			if (NPC.life <= 1)
				Phase = (int)Phases.DeathEffects;
		}

		//according to the targets position,
		private Vector2 PickSpot(int x = 1)
		{
			return Target.Center.X > arenaPos.X ? arenaPos + new Vector2(-SIDE_OFFSET_X * x, SIDE_OFFSET_Y) : arenaPos + new Vector2(SIDE_OFFSET_X * x, SIDE_OFFSET_Y); //picks the outer side.
		}

		private Vector2 PickCloseSpot(int x = 1)
		{
			return Target.Center.X > arenaPos.X ? arenaPos + new Vector2(-SHORT_OFFSET_X * x, -SHORT_OFFSET_Y) : arenaPos + new Vector2(SHORT_OFFSET_X * x, -SHORT_OFFSET_Y); //picks the inner side.
		}

		private Vector2 PickSpotSelf(int x = 1)
		{
			return NPC.Center.X > arenaPos.X ? arenaPos + new Vector2(SIDE_OFFSET_X * x, SIDE_OFFSET_Y) : arenaPos + new Vector2(-SIDE_OFFSET_X * x, SIDE_OFFSET_Y); //picks the outer side.
		}

		private Vector2 PickNearestSpot(Vector2 target)
		{
			if (target.Distance(PickSpot(-1)) < target.Distance(PickCloseSpot(-1)))
				return PickSpot(-1);
			return PickCloseSpot(-1);
		}

		private void JumpToTarget(int timeStart, int timeEnd, float yStrength = 0.5f, bool spin = false)
		{
			jumpStart = timeStart;
			jumpEnd = timeEnd;

			if (AttackTimer < timeStart + 5 && Math.Abs(moveStart.X - moveTarget.X) < 8f)
			{
				AttackTimer = timeEnd + 1;
				return;
			}

			float progress = Utils.GetLerpValue(timeStart, timeEnd, AttackTimer, true);

			if (!spin)
				animationType = (int)AttackTypes.Jump;
			else
				animationType = (int)AttackTypes.SpinJump;

			if (AttackTimer <= timeStart)
			{
				moveStart = NPC.Center;
				NPC.velocity.Y = -MathHelper.Lerp(7f, 8f, moveStart.Distance(moveTarget) * 0.003f) * yStrength;
			}

			if (AttackTimer == timeStart + 3 && !spin && !disableJumpSound)
				Helpers.Helper.PlayPitched("GlassMiniboss/RippedSoundJump", 1f, 0.7f, NPC.Center);

			if (progress <= 0.6f)
				moveStart.X += NPC.velocity.X * 0.15f;
			else
				NPC.velocity.Y += (float)Math.Pow(progress * 0.7f, 2);

			if (AttackTimer >= timeStart && AttackTimer <= timeEnd)
				NPC.position.X = MathHelper.Lerp(MathHelper.SmoothStep(moveStart.X, moveTarget.X, MathHelper.Min(progress * 1.1f, 1f)), moveTarget.X, progress) - NPC.width / 2f;
			else
				NPC.velocity.X *= 0.01f;
		}

		private void SpinJumpToTarget(int timeStart, int timeEnd, float totalRotations = 5, int direction = 1)
		{
			JumpToTarget(timeStart, timeEnd, 0.4f, true);
			float progress = Helpers.Helper.BezierEase(Utils.GetLerpValue(timeStart, timeEnd, AttackTimer, true));
			NPC.rotation = MathHelper.WrapAngle(progress * MathHelper.TwoPi * totalRotations) * NPC.direction * direction;
		}

		/// <summary>
		/// The boss leaps into the air and performs 3 staggered charges, slashing a sword each time
		/// </summary>
		private void TripleSlash()
		{
			animationType = (int)AttackTypes.TripleSlash;

			if (AttackTimer == 1)
			{
				NPC.TargetClosest();
				moveTarget = PickNearestSpot(Target.Center) - new Vector2(0, 100);
				moveStart = NPC.Center;
			}

			if (AttackTimer < 50)
			{
				NPC.FaceTarget();
				JumpToTarget(2, 50);
				NPC.velocity.Y = MathHelper.Lerp(NPC.velocity.Y, NPC.DirectionTo(moveTarget).Y * NPC.Distance(moveTarget) * 0.05f, 0.05f);
			}

			if (AttackTimer < 65 && AttackTimer > 30)
				NPC.velocity.Y *= 0.3f;

			NPC.noGravity = AttackTimer > 40 && AttackTimer < slashTimes[2];

			if (AttackTimer == 48)
			{
				NPC.velocity.X = -NPC.direction * 4f;
				NPC.FaceTarget();

				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					for (int s = 0; s < 3; s++)
					{
						GlassSword.variantStatic = s;
						Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ProjectileType<GlassSword>(), 10, 0.2f, Owner: -1, AttackTimer - 2, NPC.whoAmI);
					}
				}
			}

			if (AttackTimer < slashTimes[2] + 60 && AttackTimer > 40)
			{
				NPC.velocity.X *= 0.92f;
				NPC.velocity.Y = MathHelper.Lerp(NPC.velocity.Y, (Target.Top - NPC.Center).Y * 0.001f, 0.05f);
			}

			if (AttackTimer == slashTimes[0] || AttackTimer == slashTimes[1] || AttackTimer == slashTimes[2] - 1)
			{
				Helpers.Helper.PlayPitched("GlassMiniboss/GlassSlash", 1f, 0.1f, NPC.Center);

				if (Main.masterMode && Main.netMode != NetmodeID.MultiplayerClient) //Projectile swords on master mode
					Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.UnitX * NPC.direction * 15, ProjectileType<GlassBubbleFragment>(), 12, 1, Owner: -1);

				if (AttackTimer == slashTimes[2] - 1)
				{
					NPC.TargetClosest();
					NPC.FaceTarget();
				}

				NPC.velocity.X += NPC.direction * MathHelper.Lerp(7f, 50f, (float)Math.Pow((AttackTimer - slashTimes[0] - 1) / 150f, 2f));
			}

			if (AttackTimer > slashTimes[2] + 50)
				ResetAttack();
		}

		/// <summary>
		/// The boss conjures a spear and leaps into the air, crashing to the ground and summoning 3 bouncing lava orbs after landing
		/// </summary>
		private void MagmaSpear()
		{
			animationType = (int)AttackTypes.MagmaSpear;

			if (AttackTimer == 1)
			{
				NPC.TargetClosest();
				moveStart = NPC.Center;
				moveTarget = PickSpot() - new Vector2(0, 70);
				NPC.velocity.Y -= 9f;
				if (Main.netMode != NetmodeID.MultiplayerClient)
					spearIndex = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ProjectileType<GlassSpear>(), 10, 0.2f, Owner: -1, 0, NPC.whoAmI);
				Helpers.Helper.PlayPitched("GlassMiniboss/RippedSoundJump", 1f, 0.7f, NPC.Center);
			}

			if (AttackTimer == 5 && Main.netMode != NetmodeID.MultiplayerClient)
			{
				NPC.netUpdate = true; // this shouldn't be necessary but this attack seems to have a lot of visual drift

				if (Main.projectile[spearIndex].ModProjectile is GlassSpear spear)
					Glint.SpawnGlint(spear.Projectile.Center, new Color(150, 200, 255), new Color(150, 150, 255));
			}

			if (AttackTimer <= 65)
			{
				NPC.FaceTarget();
				float jumpProgress = Utils.GetLerpValue(5, 65, AttackTimer, true);
				NPC.position.X = MathHelper.Lerp(MathHelper.SmoothStep(moveStart.X, moveTarget.X, MathHelper.Min(jumpProgress * 1.1f, 1f)), moveTarget.X, jumpProgress) - NPC.width / 2f;
				NPC.velocity.Y *= 0.94f;
				NPC.noGravity = true;
			}
			else if (AttackTimer < 85 && !NPC.collideY)
			{
				moveTarget = arenaPos;
				NPC.velocity.X = Vector2.Lerp(NPC.velocity, NPC.DirectionTo(moveTarget) * 5, 0.4f).X;
				NPC.velocity.Y += 1.5f;
			}
			else
			{
				NPC.velocity.X *= 0.5f;
			}

			if (AttackTimer > 220)
				ResetAttack();
		}

		/// <summary>
		/// The boss conjures a spear and leaps into the air, but this time chucks it at the player to create burning ground
		/// </summary>
		private void MagmaSpearAlt()
		{
			animationType = (int)AttackTypes.MagmaSpear;

			if (AttackTimer == 1)
			{
				NPC.TargetClosest();
				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					NPC.direction = Main.rand.NextBool() ? 1 : -1;
					NPC.netUpdate = true;
				}

				moveStart = NPC.Center;
				moveTarget.X = Target.Center.X;
				moveTarget.Y = arenaPos.Y;

				if (moveTarget.X - arenaPos.X < -SIDE_OFFSET_X + 200)
					moveTarget.X = arenaPos.X - SIDE_OFFSET_X + 200;

				if (moveTarget.X - arenaPos.X > SIDE_OFFSET_X - 200)
					moveTarget.X = arenaPos.X + SIDE_OFFSET_X - 200;

				NPC.velocity.Y -= 9f;

				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					GlassSpear.setMagmaVariant = true;
					spearIndex = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ProjectileType<GlassSpear>(), 10, 0.2f, Owner: -1, 0, NPC.whoAmI);
				}

				Helpers.Helper.PlayPitched("GlassMiniboss/RippedSoundJump", 1f, 0.7f, NPC.Center);
			}

			if (AttackTimer == 5 && Main.netMode != NetmodeID.MultiplayerClient)
			{
				if (Main.projectile[spearIndex].ModProjectile is GlassSpear spear)
					Glint.SpawnGlint(spear.Projectile.Center, new Color(255, 200, 150), new Color(255, 150, 150));
			}

			if (AttackTimer <= 65)
			{
				float jumpProgress = Utils.GetLerpValue(5, 65, AttackTimer, true);
				NPC.position.X = MathHelper.Lerp(MathHelper.SmoothStep(moveStart.X, moveTarget.X, MathHelper.Min(jumpProgress * 1.1f, 1f)), moveTarget.X + 200 * -NPC.direction, jumpProgress) - NPC.width / 2f;
				NPC.velocity.Y *= 0.94f;
				NPC.noGravity = true;
			}

			if (AttackTimer > 65 && AttackTimer < 160)
			{
				NPC.velocity *= 0.94f;
			}

			if (AttackTimer == 160)
				NPC.noGravity = false;

			if (AttackTimer > 220)
				ResetAttack();
		}

		/// <summary>
		/// Currently unused
		/// </summary>
		private void Whirlwind()
		{
			animationType = (int)AttackTypes.Whirlwind;

			//if (AttackTimer == 1)
			//{
			//    NPC.TargetClosest();
			//    moveTarget = PickNearestSpot(Target.Center) - new Vector2(0, 100);
			//}

			//if (AttackTimer < 50)
			//{
			//    NPC.FaceTarget();
			//    JumpToTarget(2, 50, 0.8f);
			//}

			//if (AttackTimer == 80)
			//    whirlIndex = Projectile.NewProjectile(Entity.InheritSource(NPC), NPC.Center, Vector2.Zero, ProjectileType<Whirlwind>(), 12, 0.5f, Owner: -1, 0, NPC.whoAmI);

			//if (AttackTimer > 10 && AttackTimer < 80)
			//    NPC.velocity.Y = MathHelper.Lerp(NPC.velocity.Y, NPC.DirectionTo(moveTarget).Y * 5f, 0.1f);
			//else if (AttackTimer > 80)
			//{
			//    NPC.velocity += NPC.DirectionTo(moveTarget) * 0.01f;
			//    if (AttackTimer < 100)
			//        moveTarget = Target.Center;
			//}

			//if (AttackTimer > 120)
			//    NPC.velocity *= 0.85f;
			//else if (AttackTimer > 100)
			//    NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.DirectionTo(moveTarget) * 10, 0.5f);

			//NPC.noGravity = AttackTimer > 30 && AttackTimer < 160;
			//if (NPC.noGravity)
			//    NPC.velocity.Y *= 0.8f;

			ResetAttack();
		}

		/// <summary>
		/// The boss leaps up and summons a large amount of small glass daggers, aimed at the player
		/// </summary>
		private void JavelinRain()
		{
			animationType = (int)AttackTypes.JavelinRain;

			int spearCount = 10;
			int betweenSpearTime = 5;

			if (Main.masterMode)
			{
				spearCount = 14;
				betweenSpearTime = 4;
			}

			javelinTime = 50 + JAVELIN_SPAWN_TIME + spearCount * betweenSpearTime;

			NPC.TargetClosest();
			NPC.FaceTarget();

			if (AttackTimer == 1)
			{
				moveStart = NPC.Center;
				moveTarget = PickCloseSpot() - new Vector2(0, 100);
			}

			if (AttackTimer > 1 && AttackTimer < 25)
				NPC.velocity.Y = -Utils.GetLerpValue(25, 10, AttackTimer, true) * 5f;

			moveTarget.X = MathHelper.Lerp(moveTarget.X, Target.Center.X, 0.002f);

			if (AttackTimer > 10 && AttackTimer < javelinTime - JAVELIN_SPAWN_TIME)
			{
				NPC.noGravity = true;
				NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.DirectionTo(moveTarget) * NPC.Distance(moveTarget), 0.01f) * 0.5f;
				NPC.Center += new Vector2((float)Math.Sin(AttackTimer * 0.04f % MathHelper.TwoPi), (float)Math.Cos(AttackTimer * 0.04f % MathHelper.TwoPi)) * 0.2f;
			}
			else
			{
				NPC.velocity.X *= 0.8f;
			}

			if (Main.netMode != NetmodeID.MultiplayerClient
					&& AttackTimer % betweenSpearTime == 0 && AttackTimer >= JAVELIN_SPAWN_TIME
					&& AttackTimer < JAVELIN_SPAWN_TIME + spearCount * betweenSpearTime)
			{
				NPC.FaceTarget();
				float whatSpear = (AttackTimer - JAVELIN_SPAWN_TIME) / spearCount;

				Vector2 staffPos = NPC.Center + new Vector2(32 * NPC.direction, -105).RotatedBy(NPC.rotation);
				Vector2 spearTarget = Target.Center;//arenaPos + new Vector2(whatSpear * 130 * NPC.direction, 40);
				Vector2 spearVel = new Vector2(Main.rand.NextFloat(9, 12) * NPC.direction, 3f).RotatedBy(whatSpear * 4f);
				float angle = staffPos.AngleTo(spearTarget - spearVel * 2f);
				Projectile.NewProjectile(NPC.GetSource_FromAI(), staffPos, spearVel, ProjectileType<GlassJavelin>(), 12, 1, Owner: -1, angle);
			}

			if (AttackTimer > javelinTime)
				ResetAttack();
		}

		/// <summary>
		/// The boss jumps to one side of the arena, slams a hammer, and summons telegraphed pillars of glass
		/// </summary>
		private void GlassRaise()
		{
			animationType = (int)AttackTypes.GlassRaise;
			hammerTime = 80;

			if (AttackTimer == 1)
			{
				NPC.TargetClosest();
				moveTarget = PickSpot();
				moveStart = NPC.Center;
			}

			if (AttackTimer > 1 && AttackTimer <= 75)
			{
				JumpToTarget(2, 75);
				NPC.velocity.X = -Direction * 0.3f;
				NPC.direction = Direction;
			}

			if (!(AttackTimer > 75 && AttackTimer < 100))
				NPC.velocity.X *= 0.7f;

			if (AttackTimer == HAMMER_SPAWN_TIME && Main.netMode != NetmodeID.MultiplayerClient)
				Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ProjectileType<GlassHammer>(), 40, 1, Owner: -1, ai0: NPC.whoAmI, ai1: hammerTime);

			int spikeCount = 5;

			if (Main.expertMode)
				spikeCount = 7;

			int spikeSpawn = HAMMER_SPAWN_TIME + hammerTime - 65;
			int betweenSpikes = 5;
			float dist = Utils.GetLerpValue(spikeSpawn - 1.5f, spikeSpawn + spikeCount * betweenSpikes, AttackTimer, true);

			if (Main.netMode != NetmodeID.MultiplayerClient
					&& AttackTimer >= spikeSpawn
					&& AttackTimer < spikeSpawn + spikeCount * betweenSpikes
					&& AttackTimer % betweenSpikes == 0)
			{
				float spikeX = MathHelper.Lerp(PickSpotSelf().X, PickSpotSelf(-1).X + 102 * Direction, dist);
				var spikePos = new Vector2(spikeX, arenaPos.Y - 100);
				Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), spikePos, Vector2.Zero, ProjectileType<GlassRaiseSpike>(), 20, 1f, owner: -1, -20, dist);
			}

			if (AttackTimer > spikeSpawn + spikeCount * betweenSpikes + 120)
				ResetAttack();

		}

		/// <summary>
		/// The boss jumps to the center of the arena, slams a hammer, and summons telegraphed pillars radiating from the center
		/// </summary>
		private void GlassRaiseAlt()
		{
			animationType = (int)AttackTypes.GlassRaise;

			hammerTime = 110;

			if (AttackTimer == 1)
			{
				NPC.TargetClosest();
				moveTarget = PickCloseSpot();
				moveStart = NPC.Center;
			}

			if (AttackTimer > 1 && AttackTimer <= 75)
			{
				JumpToTarget(3, 75, 0.8f);
				NPC.velocity.X = -Direction * 0.3f;
				NPC.direction = Direction;
			}

			if (!(AttackTimer > 75 && AttackTimer < 100))
				NPC.velocity.X *= 0.7f;

			if (AttackTimer == HAMMER_SPAWN_TIME && Main.netMode != NetmodeID.MultiplayerClient)
				Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ProjectileType<GlassHammer>(), 40, 1, Owner: -1, ai0: NPC.whoAmI, ai1: hammerTime);

			int spikeCount = 3;

			if (Main.expertMode)
				spikeCount = 4;

			int spikeSpawn = HAMMER_SPAWN_TIME + hammerTime - 65;
			int betweenSpikes = 5;
			float dist = Utils.GetLerpValue(spikeSpawn - 1.5f, spikeSpawn + spikeCount * betweenSpikes, AttackTimer, true);

			if (Main.netMode != NetmodeID.MultiplayerClient
					&& AttackTimer >= spikeSpawn - 1
					&& AttackTimer < spikeSpawn + spikeCount * betweenSpikes
					&& AttackTimer % betweenSpikes == 0)
			{
				float spikeX = MathHelper.Lerp(arenaPos.X, PickSpotSelf(-1).X + 102 * Direction, dist);
				var spikePos = new Vector2(spikeX, arenaPos.Y - 120);
				Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), spikePos, Vector2.Zero, ProjectileType<GlassRaiseSpike>(), 40, 1f, owner: -1, -20, dist);
			}

			if (Main.netMode != NetmodeID.MultiplayerClient
					&& AttackTimer >= spikeSpawn
					&& AttackTimer < spikeSpawn + spikeCount * betweenSpikes
					&& (AttackTimer - 1) % betweenSpikes == 0)
			{

				float spikeX = MathHelper.Lerp(arenaPos.X, PickSpotSelf().X + 102 * -Direction, dist);
				var spikePos = new Vector2(spikeX, arenaPos.Y - 120);
				var raise = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), spikePos, Vector2.Zero, ProjectileType<GlassRaiseSpike>(), 40, 1f, owner: -1, -20, dist);
				raise.direction = -NPC.direction; //TODO: not actually synced for multiplayer but this is such a minor visual maybe we don't need to
			}

			if (AttackTimer > spikeSpawn + spikeCount * betweenSpikes + 120)
				ResetAttack();
		}

		/// <summary>
		/// The boss charges a large sphere of glass, then hits it to have it drift around the arena. After a bit, it explodes into directed high-velocity shards
		/// </summary>
		private void BigBrightBubble()
		{
			animationType = (int)AttackTypes.BigBrightBubble;

			if (AttackTimer == 1)
			{
				NPC.TargetClosest();
				moveStart = NPC.Center;
				moveTarget = PickCloseSpot();
			}

			if (AttackTimer <= 70)
			{
				NPC.FaceTarget();
				JumpToTarget(1, 70);
			}

			NPC.noGravity = AttackTimer > 75 && AttackTimer < BUBBLE_RECOIL_TIME;

			Vector2 staffPos = NPC.Center + new Vector2(5 * NPC.direction, -100).RotatedBy(NPC.rotation);

			if (AttackTimer == 80)
			{
				NPC.direction = Direction;
				NPC.velocity.Y -= 1.5f;

				if (Main.netMode != NetmodeID.MultiplayerClient)
					bubbleIndex = Projectile.NewProjectile(NPC.GetSource_FromAI(), staffPos, Vector2.Zero, ProjectileType<GlassBubble>(), 20, 2f, Owner: -1, NPC.whoAmI);
			}

			if (AttackTimer > 240 && AttackTimer < BUBBLE_RECOIL_TIME - 1)
			{
				var target = Vector2.Lerp(Target.Center, PickSpotSelf(-1) - new Vector2(0, 150), 0.8f);

				if (AttackTimer < BUBBLE_RECOIL_TIME - 20)
				{
					if (AttackTimer > BUBBLE_RECOIL_TIME - 30)
						NPC.Top = Vector2.SmoothStep(NPC.Top, moveTarget - new Vector2(150 * Utils.GetLerpValue(BUBBLE_RECOIL_TIME - 23, BUBBLE_RECOIL_TIME - 30, AttackTimer, true), 0).RotatedBy(moveTarget.AngleTo(target)), 0.3f);
					else
						NPC.Top = Vector2.Lerp(NPC.Top, moveTarget - new Vector2(120, 0).RotatedBy(moveTarget.AngleTo(target)), 0.05f);
				}
				else if (AttackTimer == BUBBLE_RECOIL_TIME - 20)
				{
					HitBubble(NPC.DirectionTo(target));
				}

				NPC.direction = Direction;

				NPC.rotation = (float)Math.Pow(Utils.GetLerpValue(390, BUBBLE_RECOIL_TIME - 20, AttackTimer, true), 2) * MathHelper.TwoPi * 4f * NPC.direction;
			}

			if (AttackTimer == BUBBLE_RECOIL_TIME - 1)
				moveTarget = PickSpotSelf();

			if (AttackTimer > BUBBLE_RECOIL_TIME - 1)
			{
				if (AttackTimer <= BUBBLE_RECOIL_TIME + 50)
				{
					SpinJumpToTarget(BUBBLE_RECOIL_TIME, BUBBLE_RECOIL_TIME + 50, 3, -1);
					NPC.velocity.Y += 0.06f;
				}
				else
				{
					NPC.velocity.X *= 0.87f;
					NPC.FaceTarget();
				}
			}

			if (AttackTimer > BUBBLE_RECOIL_TIME + 20)
				ResetAttack();
		}

		/// <summary>
		/// The boss' behavior for hitting the bubble for BigBrightBubble
		/// </summary>
		/// <param name="direction">The direction of this vector is used to set the direction of the glass bubble projectile</param>
		private void HitBubble(Vector2 direction)
		{
			Projectile bubble;
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				//using proj.scale == 1f is a little bit tenuous of a way of determining that this is actually the original one. if this breaks in the future may need to have a specific synced var to determine
				bubble = Main.projectile.Where(proj => proj.active && proj.type == ModContent.ProjectileType<GlassBubble>() && proj.scale == 1f).FirstOrDefault();
			}
			else
			{
				bubble = Main.projectile[bubbleIndex];
			}

			float speed = 6.77f;

			if (bubble.active && bubble.type == ProjectileType<GlassBubble>())
			{
				bubble.velocity = direction * speed;
				bubble.ai[1] = 1;

				if (Main.netMode != NetmodeID.Server)
				{
					Helpers.Helper.PlayPitched("GlassMiniboss/GlassBounce", 1f, 0f, NPC.Center);
					CameraSystem.shake += 5;
					for (int i = 0; i < 30; i++)
					{
						Dust.NewDustPerfect(bubble.Center, DustType<Dusts.Cinder>(), Main.rand.NextVector2Circular(6, 3).RotatedBy(NPC.AngleTo(bubble.Center)), 0, GlassColor);
					}
				}
			}

			if (Main.masterMode && Main.netMode != NetmodeID.MultiplayerClient)
			{
				GlassBubble.staticScaleToSet = 0.8f;
				Projectile.NewProjectile(NPC.GetSource_FromAI(), bubble.Center, direction.RotatedBy(1) * speed * 0.6f, ProjectileType<GlassBubble>(), 20, 2f, Owner: -1, NPC.whoAmI, 1, 180);

				GlassBubble.staticScaleToSet = 0.8f;
				Projectile.NewProjectile(NPC.GetSource_FromAI(), bubble.Center, direction.RotatedBy(-1) * speed * 0.6f, ProjectileType<GlassBubble>(), 20, 2f, Owner: -1, NPC.whoAmI, 1, 180);
			}
		}
	}
}