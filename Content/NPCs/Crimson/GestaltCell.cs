using StarlightRiver.Content.Biomes;
using StarlightRiver.Content.Bosses.TheThinkerBoss;
using StarlightRiver.Core.Systems.PixelationSystem;
using System;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.ID;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace StarlightRiver.Content.NPCs.Crimson
{
	public enum GestaltCellState : int
	{
		Fleeing = -1,
		RestingOrMoving = 0,
		OneCellJumping = 1,
		MultiCellAttack = 2,
		WaitingForMerge = 3
	}

	internal class GestaltCell : ModNPC
	{
		public Rectangle arena;
		public float opacity;
		public NPC myLeader;
		public Vector2 spawnPos;
		public Vector2 savedPos;

		public int attackChoice;

		public bool autoPositionFollowers = true;
		public bool contactDamage = true;

		public List<NPC> myFollowers = [];

		public float squish;

		public override string Texture => AssetDirectory.CrimsonNPCs + Name;

		public ref float Timer => ref NPC.ai[0];
		public ref float CellCount => ref NPC.ai[1];
		public GestaltCellState State
		{
			get => (GestaltCellState)NPC.ai[2];
			set => NPC.ai[2] = (float)value;
		}
		public bool Leader
		{
			get => NPC.ai[3] == 1;
			set => NPC.ai[3] = value ? 1 : 0;
		}

		public Player Target => NPC.target == -1 ? null : Main.player[NPC.target];
		public GestaltCell MyLeaderCell => myLeader.ModNPC as GestaltCell;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Gestalt Cell");
		}

		public override void SetDefaults()
		{
			NPC.lifeMax = 1500;
			NPC.damage = 15;
			NPC.defense = 7;
			NPC.width = 42;
			NPC.height = 38;
			NPC.knockBackResist = 0.33f;
			NPC.HitSound = SoundID.NPCHit8;
			NPC.DeathSound = SoundID.NPCDeath12;
		}

		public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers)
		{
			if (State == GestaltCellState.WaitingForMerge || !Leader && MyLeaderCell.State == GestaltCellState.WaitingForMerge)
				modifiers.FinalDamage *= 0.01f;

			modifiers.FinalDamage *= 1f - CellCount * 0.15f;
		}

		public override bool CanHitPlayer(Player target, ref int cooldownSlot)
		{
			return MyLeaderCell.contactDamage;
		}

		/// <summary>
		/// Finds an appropriate target if possible, sets NPC.target and sends netupdate.
		/// </summary>
		public void GetTarget()
		{
			List<Player> pool = [];

			foreach (Player player in Main.ActivePlayers)
			{
				if (player.Hitbox.Intersects(arena) && !player.dead)
					pool.Add(player);
			}

			if (pool.Count == 0)
			{
				NPC.target = -1;
				NPC.netUpdate = true;
				return;
			}

			NPC.target = pool[Main.rand.Next(pool.Count)].whoAmI;
			NPC.netUpdate = true;
		}

		/// <summary>
		/// Checks if the current target is still valid, and if not, changes the target. Returns false if the target changed.
		/// </summary>
		public bool CheckTarget()
		{
			if (NPC.target == -1)
			{
				GetTarget();
				return false;
			}

			if (!Target.active || Target.dead || !Target.Hitbox.Intersects(arena))
			{
				GetTarget();
				return false;
			}

			return true;
		}

		public override void AI()
		{
			Timer++;

			squish += squish * -0.1f;

			if (opacity < 1)
				opacity += 0.05f;

			if (arena == default)
				arena = GestaltCellArenaSystem.ArenaWorld;

			if (spawnPos == default)
				spawnPos = NPC.Center;

			if (CellCount <= 0)
				CellCount = 1;

			if (Leader)
			{
				myLeader = NPC;
				LeaderAI();
			}
			else
			{
				FollowerAI();
			}
		}

		/// <summary>
		/// Behavior for the leader cell
		/// </summary>
		public void LeaderAI()
		{
			FrameCell();

			Vector2 lastPos = NPC.Center;
			float lastRot = 0f;

			if (autoPositionFollowers)
			{
				if (CellCount > 1)
				{
					float rot = NPC.velocity.X * -0.05f;
					Vector2 off = Vector2.UnitY.RotatedBy(rot) * -30;
					lastPos += off;
					lastRot += rot;

					myFollowers[0].Center = lastPos;
					myFollowers[0].rotation = lastRot;
				}

				if (CellCount > 2)
				{
					float rot = NPC.velocity.X * -0.07f;
					Vector2 off = Vector2.UnitY.RotatedBy(rot) * -32 + Vector2.UnitX * -NPC.velocity;
					//lastPos += off;
					//lastRot += rot;

					myFollowers[1].Center = lastPos + off;
					myFollowers[1].rotation = lastRot + rot;
				}

				if (CellCount > 3)
				{
					myFollowers[2].Center = lastPos + Vector2.UnitX.RotatedBy(lastRot) * -32;
					myFollowers[2].rotation = lastRot;
				}

				if (CellCount > 4)
				{
					myFollowers[3].Center = lastPos + Vector2.UnitX.RotatedBy(lastRot) * 32;
					myFollowers[3].rotation = lastRot;
				}
			}

			if (State == GestaltCellState.Fleeing)
			{
				CheckTarget();

				NPC.velocity.X *= 0.95f;

				if (Target != null)
				{
					State = GestaltCellState.RestingOrMoving;
					Timer = 0;
					return;

				}

				if (Timer > 240)
					opacity = 1f - (Timer - 240) / 60f;

				if (Timer > 300)
					NPC.active = false;

				return;
			}

			// Special behavior for while we wait for a merge
			if (State == GestaltCellState.WaitingForMerge)
			{
				NPC.velocity.X *= 0.5f;

				if (Timer == 1)
					SoundEngine.PlaySound(SoundID.Roar.WithPitchOffset(1.5f), NPC.Center);

				if (Timer < 20)
				{
					Vector2 off = Main.rand.NextVector2Circular(1, 1);
					Dust.NewDustPerfect(NPC.Center + off * 64, ModContent.DustType<Dusts.PixelatedImpactLineDust>(), off * Main.rand.NextFloat(3), 0, new Color(255, 100, 100, 0), Main.rand.NextFloat(0.1f, 0.2f));
				}

				if (Timer == 30)
				{
					bool spawnedOnLeft = Main.rand.NextBool();

					if (CellCount == 3)
						spawnedOnLeft = true;

					if (CellCount == 4)
						spawnedOnLeft = false;

					SpawnFollower(arena.X + (spawnedOnLeft ? 0 : arena.Width - NPC.width), (int)NPC.Center.Y);
					NPC.direction = spawnedOnLeft ? 1 : -1;
				}

				return;
			}

			bool targetChanged = CheckTarget();

			if (Target is null)
			{
				State = GestaltCellState.Fleeing;
				Timer = 0;

				return;
			}

			// Main AI, check cell count then choose an action
			if (CellCount == 1)
			{
				OneCellBehavior();
			}

			if (CellCount >= 2)
			{
				MultiCellMovement();

				NPC.knockBackResist = 0f;
			}
		}

		public void FollowerAI()
		{
			// If this cell dosent have a leader, it becomes its own leader!
			if (myLeader is null)
			{
				Leader = true;
				Timer = 0;
				return;
			}

			// Indicates we have merged already and just need to let ourselves be led
			if (CellCount > 1)
			{
				if (!myLeader.active)
					NPC.active = false;

				NPC.direction = myLeader.direction;
				NPC.velocity *= 0;
				return;
			}

			FrameCell();

			NPC.direction = NPC.Center.X > myLeader.Center.X ? 1 : -1;
			float targetX = myLeader.Center.X + 90 * (NPC.direction * 1);
			var target = new Vector2(targetX, spawnPos.Y);

			float dist = Math.Abs(spawnPos.X - target.X) / 3f;

			if (Timer <= dist)
			{
				spawnPos.Y = NPC.Center.Y;
				NPC.Center = Vector2.Lerp(spawnPos, target, Eases.EaseQuadInOut(Timer / dist));
			}

			if (Timer > dist)
			{
				NPC.frame = new Rectangle(0, 38 * 3, NPC.width, NPC.height);

				NPC.velocity.Y = 0;
				NPC.noGravity = true;

				float fuseProg = Math.Min(1, (Timer - dist) / 30f);

				Vector2 mergeTarget = MyLeaderCell.GetMergeTarget();
				float yOff = target.Y - mergeTarget.Y;
				float amp = 34f + yOff;

				float period = MathF.Asin(yOff / amp);

				float offset = MathF.Sin(fuseProg * (MathF.PI - period)) * amp;
				Main.NewText($"Offset: {offset}");

				float finalX = Vector2.Lerp(target, mergeTarget, fuseProg).X;
				float finalY = target.Y - offset;

				NPC.Center = new(finalX, finalY);

				Main.NewText(NPC.Center - mergeTarget);
			}

			if (Timer >= dist + 30)
			{
				MyLeaderCell.CellCount += 1;
				MyLeaderCell.State = GestaltCellState.RestingOrMoving;
				MyLeaderCell.Timer = 0;
				MyLeaderCell.myFollowers.Add(NPC);

				NPC.realLife = myLeader.whoAmI;
				CellCount = MyLeaderCell.CellCount;
				MyLeaderCell.myFollowers.ForEach(n => (n.ModNPC as GestaltCell).CellCount = MyLeaderCell.CellCount);

				CombatText.NewText(NPC.Hitbox, new Color(255, 150, 150), $"{MyLeaderCell.CellCount}!");

				for (int k = 0; k < 30; k++)
				{
					Vector2 off = Main.rand.NextVector2Circular(1, 0.3f);
					Dust.NewDustPerfect(NPC.Center + Vector2.UnitY * 16 + off * 16, DustID.Crimson, off * Main.rand.NextFloat(14));
					Dust.NewDustPerfect(NPC.Center + Vector2.UnitY * 16 + off * 16, DustID.Blood, off * Main.rand.NextFloat(8));
				}

				SoundHelper.PlayPitched("Effects/Splat", 0.5f, 0.5f, NPC.Center);
			}
		}

		/// <summary>
		/// Spawns a follower cell with this cell as the leader
		/// </summary>
		/// <param name="x">The X position of the follower</param>
		/// <param name="y">The Y position of the follower</param>
		public void SpawnFollower(int x, int y)
		{
			int i = NPC.NewNPC(NPC.GetSource_FromThis(), x, y, Type, 0, 0, 1, 0, 0, NPC.target);
			NPC follower = Main.npc[i];
			var followerCell = follower.ModNPC as GestaltCell;

			if (followerCell is null)
				return;

			followerCell.arena = arena;
			followerCell.myLeader = NPC;
		}

		/// <summary>
		/// slime-like behavior for the first individual cell
		/// </summary>
		public void OneCellBehavior()
		{
			switch (State)
			{
				case GestaltCellState.RestingOrMoving:

					// Phase change check
					if (NPC.life < NPC.lifeMax / 5f * 4f)
					{
						State = GestaltCellState.WaitingForMerge;
						Timer = 0;
						return;
					}

					NPC.velocity.X *= 0.75f;

					if (Timer > 30 && NPC.velocity.Y == 0)
					{
						State = GestaltCellState.OneCellJumping;
						Timer = 0;
					}

					break;

				case GestaltCellState.OneCellJumping:

					if (Timer == 1)
					{
						NPC.direction = Target.Center.X > NPC.Center.X ? -1 : 1;
						NPC.velocity.X = Target.Center.X > NPC.Center.X ? 4 : -4;
						NPC.velocity.Y -= Math.Clamp((NPC.Center.Y - Target.Center.Y) * 0.1f, 4f, 8f);
						squish -= 0.2f;
					}

					if (NPC.velocity.Y == 0)
					{
						State = GestaltCellState.RestingOrMoving;
						Timer = 0;

						squish += 0.3f;
					}

					break;
			}
		}

		/// <summary>
		/// The more complex behaviors for the greater cell counts
		/// </summary>
		public void MultiCellMovement()
		{
			if (State == GestaltCellState.RestingOrMoving)
			{
				// Phase change check
				if (CellCount == 2 && NPC.life < NPC.lifeMax / 5f * 3f)
				{
					State = GestaltCellState.WaitingForMerge;
					Timer = 0;
					return;
				}

				if (CellCount == 3 && NPC.life < NPC.lifeMax / 5f * 2f)
				{
					State = GestaltCellState.WaitingForMerge;
					Timer = 0;
					return;
				}

				if (CellCount == 4 && NPC.life < NPC.lifeMax / 5f * 1f)
				{
					State = GestaltCellState.WaitingForMerge;
					Timer = 0;
					return;
				}

				NPC.velocity.X += Target.Center.X > NPC.Center.X ? 0.1f : -0.1f;

				if (Math.Abs(NPC.velocity.X) > 4)
					NPC.velocity.X *= 0.9f;

				NPC.direction = NPC.velocity.X > 0 ? -1 : 1;

				if (Timer > 90 && CellCount > 2)
				{
					attackChoice = Main.rand.Next((int)CellCount - 2);
					State = GestaltCellState.MultiCellAttack;
					Timer = 0;
				}
			}

			if (State == GestaltCellState.MultiCellAttack)
			{
				switch (attackChoice)
				{
					case 0:
						ThreeCellAttack();
						break;

					case 1:
						FourCellAttack();
						break;

					case 2:
						FiveCellAttack();
						break;
				}
			}
		}

		public void ThreeCellAttack()
		{
			if (Timer < 30)
				NPC.velocity.X *= 0.95f;

			if (Timer == 30)
			{
				autoPositionFollowers = false;

				for (int k = 0; k < myFollowers.Count; k++)
				{
					NPC follower = myFollowers[k];
					var followerCell = follower.ModNPC as GestaltCell;

					followerCell.savedPos = follower.Center;
				}
			}

			if (Timer > 30 && Timer <= 60)
			{
				NPC.velocity.X *= 0.5f;

				for (int k = 0; k < myFollowers.Count; k++)
				{
					NPC follower = myFollowers[k];
					var followerCell = follower.ModNPC as GestaltCell;

					Vector2 target = NPC.Center + Vector2.UnitY * -100 * (k + 1);
					follower.Center = Vector2.Lerp(followerCell.savedPos, target, Eases.EaseCubicOut((Timer - 30) / 30f));
				}
			}

			if (Timer == 60)
			{
				for (int k = 0; k < myFollowers.Count; k++)
				{
					NPC follower = myFollowers[k];
					var followerCell = follower.ModNPC as GestaltCell;

					followerCell.savedPos = follower.Center;
				}
			}

			if (Timer > 60 && Timer <= 150)
			{
				for (int k = 0; k < myFollowers.Count; k++)
				{
					NPC follower = myFollowers[k];
					var followerCell = follower.ModNPC as GestaltCell;

					float relTime = Math.Min(130, Timer - k * 4);
					float rot = 0f;

					if (relTime <= 90)
						rot = Eases.EaseCubicIn((relTime - 60) / 30f) * 1.57f;

					if (relTime > 90)
						rot = 1.57f - Eases.EaseCubicIn((relTime - 90) / 40f) * 3.14f;

					rot *= NPC.direction * -1;

					follower.rotation = rot;

					Vector2 target = followerCell.savedPos.RotatedBy(rot, NPC.Center);
					follower.Center = target;
				}
			}

			if (Timer == 90 || Timer == 130)
			{
				SoundHelper.PlayPitched("Impacts/StoneStrike", 0.5f, Main.rand.NextFloat(-0.5f, -0.3f), myFollowers[0].Center);

				for (int k = 0; k < 10; k++)
				{
					Dust.NewDustPerfect(myFollowers[0].Center, DustID.Crimson, Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(5));
					Dust.NewDustPerfect(myFollowers[0].Center, DustID.Blood, Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(8));
				}
			}

			if (Timer == 90 + (CellCount - 2) * 4 || Timer == 130 + (CellCount - 2) * 4)
			{
				SoundHelper.PlayPitched("Impacts/StoneStrike", 1f, Main.rand.NextFloat(-0.8f, -0.6f), myFollowers[1].Center);

				for (int k = 0; k < 30; k++)
				{
					Dust.NewDustPerfect(myFollowers[(int)CellCount - 2].Center, DustID.Crimson, Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(10));
					Dust.NewDustPerfect(myFollowers[(int)CellCount - 2].Center, DustID.Blood, Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(15));
				}
			}

			if (Timer == 150)
			{
				contactDamage = false;

				for (int k = 0; k < myFollowers.Count; k++)
				{
					NPC follower = myFollowers[k];
					var followerCell = follower.ModNPC as GestaltCell;

					followerCell.savedPos = follower.Center;
				}
			}

			if (Timer > 150 && Timer <= 180)
			{
				for (int k = 0; k < myFollowers.Count; k++)
				{
					NPC follower = myFollowers[k];
					var followerCell = follower.ModNPC as GestaltCell;
					int direction = k == 2 ? -1 : 1;

					Vector2 targetPos;

					if (k >= 2)
						targetPos = NPC.Center + new Vector2(direction * 30, -30);
					else
						targetPos = NPC.Center + new Vector2(0, -30 * (k + 1));

					follower.Center = Vector2.Lerp(followerCell.savedPos, targetPos, Eases.EaseCubicIn((Timer - 150) / 30f));
				}
			}

			if (Timer > 180)
			{
				contactDamage = true;
				autoPositionFollowers = true;
				State = GestaltCellState.RestingOrMoving;
				Timer = 0;
			}
		}

		public void FourCellAttack()
		{
			if (Timer < 30)
				NPC.velocity.X *= 0.95f;

			if (Timer == 30)
			{
				NPC follower = myFollowers[2];
				var followerCell = follower.ModNPC as GestaltCell;

				followerCell.savedPos = follower.Center;
			}

			if (Timer > 30 && Timer <= 60)
			{
				NPC follower = myFollowers[2];
				var followerCell = follower.ModNPC as GestaltCell;

				Vector2 targetPos = NPC.Center + new Vector2(0, -140);

				follower.Center = Vector2.Lerp(followerCell.savedPos, targetPos, Eases.EaseCubicOut((Timer - 30) / 30f));
			}

			if (Timer == 60)
			{
				NPC follower = myFollowers[2];
				var followerCell = follower.ModNPC as GestaltCell;

				followerCell.savedPos = follower.Center;

				for (int k = -1; k <= 1; k++)
				{
					Projectile.NewProjectile(follower.GetSource_FromThis(), follower.Center, follower.Center.DirectionTo(Target.Center).RotatedBy(k * 0.4f) * (5 + k), ModContent.ProjectileType<BrainBolt>(), 20, 0, Main.myPlayer, 210, 0, 20);
				}
			}

			if (Timer > 60 && Timer <= 90)
			{
				NPC follower = myFollowers[2];
				var followerCell = follower.ModNPC as GestaltCell;

				Vector2 targetPos = NPC.Center + new Vector2(-32, -30);

				follower.Center = Vector2.Lerp(followerCell.savedPos, targetPos, Eases.EaseCubicOut((Timer - 60) / 30f));
			}

			if (Timer > 90)
			{
				State = GestaltCellState.RestingOrMoving;
				Timer = 0;
			}
		}

		public void FiveCellAttack()
		{
			if (Timer < 30)
				NPC.velocity.X *= 0.85f;

			if (Timer == 30)
			{
				autoPositionFollowers = false;

				for (int k = 0; k < myFollowers.Count; k++)
				{
					NPC follower = myFollowers[k];
					var followerCell = follower.ModNPC as GestaltCell;

					followerCell.savedPos = follower.Center;
				}
			}

			if (Timer > 30 && Timer <= 60)
			{
				for (int k = 0; k < myFollowers.Count; k++)
				{
					NPC follower = myFollowers[k];
					var followerCell = follower.ModNPC as GestaltCell;

					Vector2 targetPos;

					if (k >= 2)
					{
						int direction = k == 2 ? -1 : 1;
						targetPos = NPC.Center + new Vector2(direction * 80, -160);
					}
					else
					{
						targetPos = NPC.Center + Vector2.UnitY * -50 * (k + 1);
					}

					follower.Center = Vector2.Lerp(followerCell.savedPos, targetPos, Eases.EaseCubicOut((Timer - 30) / 30f));
				}
			}

			if (Timer == 60)
			{
				for (int k = 0; k < myFollowers.Count; k++)
				{
					NPC follower = myFollowers[k];
					var followerCell = follower.ModNPC as GestaltCell;

					followerCell.savedPos = follower.Center;
				}
			}

			if (Timer == 60)
			{
				NPC follower = myFollowers[2];

				for (int i = 0; i < 7; i++)
				{
					Projectile.NewProjectile(follower.GetSource_FromThis(), follower.Center, new Vector2((-3 + i) * 3, -20), ModContent.ProjectileType<GestaltMortar>(), 20, 0, Main.myPlayer, Main.rand.Next(3));
				}
			}

			if (Timer == 80)
			{
				NPC follower = myFollowers[3];

				for (int i = 0; i < 6; i++)
				{
					Projectile.NewProjectile(follower.GetSource_FromThis(), follower.Center, new Vector2((-3 + i) * 3, -20), ModContent.ProjectileType<GestaltMortar>(), 20, 0, Main.myPlayer, Main.rand.Next(3));
				}
			}

			if (Timer > 90 && Timer <= 120)
			{
				for (int k = 0; k < myFollowers.Count; k++)
				{
					NPC follower = myFollowers[k];
					var followerCell = follower.ModNPC as GestaltCell;
					int direction = k == 2 ? -1 : 1;

					Vector2 targetPos;

					if (k >= 2)
						targetPos = NPC.Center + new Vector2(direction * 30, -30);
					else
						targetPos = NPC.Center + new Vector2(0, -30 * (k + 1));

					follower.Center = Vector2.Lerp(followerCell.savedPos, targetPos, Eases.EaseCubicOut((Timer - 90) / 30f));
				}
			}

			if (Timer > 120)
			{
				autoPositionFollowers = true;

				State = GestaltCellState.RestingOrMoving;
				Timer = 0;
			}
		}

		/// <summary>
		/// Gets the point at which a merging cell should jump to smoothly animate into this one
		/// </summary>
		/// <returns></returns>
		public Vector2 GetMergeTarget()
		{
			return CellCount switch
			{
				1 => NPC.Center + Vector2.UnitY * -28,
				2 => NPC.Center + Vector2.UnitY * -60,
				3 => NPC.Center + new Vector2(-32, -30),
				4 => NPC.Center + new Vector2(32, -30),
				_ => NPC.Center,
			};
		}

		/// <summary>
		/// Sets the NPCs frame for the leader
		/// </summary>
		public void FrameCell()
		{
			if (NPC.velocity.Y != 0)
				NPC.frame = new Rectangle(0, 38 * 3, NPC.width, NPC.height);

			else
				NPC.frame = new Rectangle(42 * (int)(NPC.Center.X * 0.1f % 5), 38, NPC.width, NPC.height);
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			if (StarlightRiver.debugMode && Leader)
			{
				Rectangle ar = new(arena.X - (int)Main.screenPosition.X, arena.Y - (int)Main.screenPosition.Y, arena.Width, arena.Height);
				spriteBatch.Draw(Assets.MagicPixel.Value, ar, Color.Red * 0.25f);
			}

			// Draw arena dev art
			Rectangle floor = new(arena.X - (int)Main.screenPosition.X - 16, arena.Y + arena.Height - (int)Main.screenPosition.Y, arena.Width + 32, 16);
			spriteBatch.Draw(Assets.MagicPixel.Value, floor, Color.Lerp(Main.DiscoColor, Color.White, 0.5f) * 0.75f * opacity);

			spriteBatch.End();
			spriteBatch.Begin(default, default, SamplerState.PointWrap, default, Main.Rasterizer, default, Main.GameViewMatrix.TransformationMatrix);

			Texture2D barrier = Assets.MotionTrail.Value;
			var sourceRect = new Rectangle(0, (int)(Main.GameUpdateCount * 0.4f), barrier.Width, barrier.Height * 4);
			var sourceRect2 = new Rectangle(0, (int)(Main.GameUpdateCount * -0.73f), barrier.Width, barrier.Height * 4);

			var targetRect = new Rectangle((int)(arena.X - Main.screenPosition.X) - 64, (int)(arena.Y - Main.screenPosition.Y), 64, 30 * 16);
			spriteBatch.Draw(barrier, targetRect, sourceRect, new Color(255, 100, 100, 0) * 0.3f * opacity);
			spriteBatch.Draw(barrier, targetRect, sourceRect2, new Color(255, 50, 50, 0) * 0.2f * opacity);
			targetRect.Inflate(-24, 0);
			targetRect.Offset(24, 0);
			spriteBatch.Draw(barrier, targetRect, sourceRect2, new Color(255, 255, 255, 0) * 0.5f * opacity);

			targetRect = new Rectangle((int)(arena.X - Main.screenPosition.X) + 75 * 16, (int)(arena.Y - Main.screenPosition.Y), 64, 30 * 16);
			spriteBatch.Draw(barrier, targetRect, sourceRect, new Color(255, 100, 100, 0) * 0.3f * opacity, 0, default, SpriteEffects.FlipHorizontally, 0);
			spriteBatch.Draw(barrier, targetRect, sourceRect2, new Color(255, 50, 50, 0) * 0.2f * opacity, 0, default, SpriteEffects.FlipHorizontally, 0);
			targetRect.Inflate(-24, 0);
			targetRect.Offset(-24, 0);
			spriteBatch.Draw(barrier, targetRect, sourceRect2, new Color(255, 255, 255, 0) * 0.5f * opacity, 0, default, SpriteEffects.FlipHorizontally, 0);

			spriteBatch.End();
			spriteBatch.Begin(default, default, Main.DefaultSamplerState, default, Main.Rasterizer, default, Main.GameViewMatrix.TransformationMatrix);

			// draw cell
			Texture2D tex = Assets.NPCs.Crimson.GestaltCell.Value;

			Vector2 scale = Vector2.One * NPC.scale;
			scale.X += squish * 2;
			scale.Y -= squish;

			spriteBatch.Draw(tex, NPC.Center - screenPos, NPC.frame, drawColor * opacity, NPC.rotation, NPC.frame.Size() / 2f, scale, NPC.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);

			ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderNPCs", () =>
			{
				Vector2 lastpos = NPC.Center;

				for (int k = 0; k < myFollowers.Count; k++)
				{
					Texture2D tex = Assets.MagicPixel.Value;

					Vector2 pos = myFollowers[k].Center;
					float dist = Vector2.Distance(pos, lastpos);
					float rot = pos.DirectionTo(lastpos).ToRotation();

					var target = new Rectangle((int)(pos.X - Main.screenPosition.X), (int)(pos.Y - Main.screenPosition.Y), (int)dist, 2);
					var color = Color.Lerp(new Color(255, 160, 100, 150), new Color(255, 100, 220, 150), 0.5f + MathF.Sin((Main.GameUpdateCount + k * 5) / 4f) * 0.5f);

					pos += Vector2.UnitY.RotatedBy(rot) * MathF.Sin(Main.GameUpdateCount * 0.1f + k * 0.2f) * 4;
					var target2 = new Rectangle((int)(pos.X - Main.screenPosition.X), (int)(pos.Y - Main.screenPosition.Y), (int)dist, 2);
					var color2 = Color.Lerp(new Color(100, 220, 255, 150), new Color(120, 255, 120, 150), 0.5f + MathF.Sin((Main.GameUpdateCount + k * 5) / 5f) * 0.5f);

					pos += Vector2.UnitY.RotatedBy(rot) * MathF.Sin(Main.GameUpdateCount * 0.1f + k * 0.2f) * 4;
					var target3 = new Rectangle((int)(pos.X - Main.screenPosition.X), (int)(pos.Y - Main.screenPosition.Y), (int)dist, 2);
					var color3 = Color.Lerp(new Color(200, 255, 255, 150), new Color(210, 100, 255, 150), 0.5f + MathF.Sin((Main.GameUpdateCount + k * 5) / 6f) * 0.5f);

					spriteBatch.Draw(tex, target, null, color, rot, new Vector2(0, tex.Height / 2f), 0, 0);
					spriteBatch.Draw(tex, target2, null, color2, rot, new Vector2(0, tex.Height / 2f), 0, 0);
					spriteBatch.Draw(tex, target3, null, color3, rot, new Vector2(0, tex.Height / 2f), 0, 0);

					pos = myFollowers[k].Center;

					if (k < 2 || attackChoice == 0)
						lastpos = pos;
				}
			});

			return false;
		}
	}
}
