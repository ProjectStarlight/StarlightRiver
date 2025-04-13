using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.ID;

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

		public bool autoPositionFollowers = true;

		public List<NPC> myFollowers = new();

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
			NPC.knockBackResist = 0.1f;
		}

		public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers)
		{
			if (State == GestaltCellState.WaitingForMerge || !Leader && MyLeaderCell.State == GestaltCellState.WaitingForMerge)
				modifiers.FinalDamage *= 0.1f;
		}

		/// <summary>
		/// Finds an appropriate target if possible, sets NPC.target and sends netupdate.
		/// </summary>
		public void GetTarget()
		{
			List<Player> pool = new();

			foreach(Player player in Main.ActivePlayers)
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
				arena = new Rectangle((int)NPC.Center.X - 600, (int)NPC.Center.Y - 500, 1200, 500);

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
					var rot = NPC.velocity.X * -0.05f;
					var off = Vector2.UnitY.RotatedBy(rot) * -28;
					lastPos += off;
					lastRot += rot;

					myFollowers[0].Center = lastPos;
					myFollowers[0].rotation = lastRot;
				}

				if (CellCount > 2)
				{
					var rot = NPC.velocity.X * -0.07f;
					var off = Vector2.UnitY.RotatedBy(rot) * -32 + Vector2.UnitX * -NPC.velocity;
					lastPos += off;
					lastRot += rot;

					myFollowers[1].Center = lastPos;
					myFollowers[1].rotation = lastRot;
				}

				if (CellCount > 3)
				{
					myFollowers[2].Center = lastPos + Vector2.UnitX.RotatedBy(lastRot) * -32;
					myFollowers[2].rotation = lastRot + NPC.velocity.X * -0.03f;
				}

				if (CellCount > 4)
				{
					myFollowers[3].Center = lastPos + Vector2.UnitX.RotatedBy(lastRot) * 32;
					myFollowers[3].rotation = lastRot + NPC.velocity.X * -0.03f;
				}
			}

			if (State == GestaltCellState.Fleeing)
			{
				if (Timer > 30)
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
					var off = Main.rand.NextVector2Circular(1, 1);
					Dust.NewDustPerfect(NPC.Center + off * 64, ModContent.DustType<Dusts.PixelatedImpactLineDust>(), off * Main.rand.NextFloat(3), 0, new Color(255, 100, 100, 0), Main.rand.NextFloat(0.1f, 0.2f));
				}

				if (Timer == 30)
				{
					bool spawnedOnLeft = Main.rand.NextBool();

					if (CellCount == 2)
						spawnedOnLeft = true;

					if (CellCount == 3)
						spawnedOnLeft = false;

					SpawnFollower(arena.X + (spawnedOnLeft ? 0 : arena.Width - NPC.width), (int)NPC.Center.Y);
					NPC.direction = spawnedOnLeft ? 1 : -1;
				}

				return;
			}
			
			bool targetChanged = CheckTarget();

			if (targetChanged && Target is null)
			{
				State = GestaltCellState.Fleeing;
				Timer = 0;
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
				return;
			}

			FrameCell();

			NPC.direction = NPC.Center.X > myLeader.Center.X ? 1 : -1;
			float targetX = myLeader.Center.X + 90 * (NPC.direction * 1);
			Vector2 target = new Vector2(targetX, spawnPos.Y);

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

				var mergeTarget = MyLeaderCell.GetMergeTarget();
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

				CombatText.NewText(NPC.Hitbox, new Color(255, 150, 150), $"{MyLeaderCell.CellCount}!");

				for (int k = 0; k < 30; k++)
				{
					var off = Main.rand.NextVector2Circular(1, 0.3f);
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
			GestaltCell followerCell = follower.ModNPC as GestaltCell;

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
			switch(State)
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

				if (Timer > 120 && CellCount > 2)
				{
					State = GestaltCellState.MultiCellAttack;
					Timer = 0;
				}
			}

			if (State == GestaltCellState.MultiCellAttack)
			{
				switch(CellCount)
				{
					case 3:
						ThreeCellAttack();
						break;

					case 4:
						FourCellAttack();
						break;

					case 5:
						FiveCellAttack();
						break;
				}
			}
		}

		public void ThreeCellAttack()
		{
			if (Timer < 30)
				NPC.velocity.X *= 0.5f;

			if (Timer == 30)
			{
				autoPositionFollowers = false;

				for (int k = 0; k < myFollowers.Count; k++)
				{
					var follower = myFollowers[k];
					var followerCell = follower.ModNPC as GestaltCell;

					followerCell.savedPos = follower.Center;
				}
			}

			if (Timer > 30 && Timer <= 60)
			{
				for (int k = 0; k < myFollowers.Count; k++)
				{
					var follower = myFollowers[k];
					var followerCell = follower.ModNPC as GestaltCell;

					Vector2 target = NPC.Center + Vector2.UnitY * -100 * (k + 1);
					follower.Center = Vector2.Lerp(followerCell.savedPos, target, Eases.EaseCubicOut((Timer - 30) / 30f));
				}
			}

			if (Timer == 60)
			{
				for (int k = 0; k < myFollowers.Count; k++)
				{
					var follower = myFollowers[k];
					var followerCell = follower.ModNPC as GestaltCell;

					followerCell.savedPos = follower.Center;
				}
			}

			if (Timer > 60 && Timer <= 130)
			{
				for (int k = 0; k < myFollowers.Count; k++)
				{
					var follower = myFollowers[k];
					var followerCell = follower.ModNPC as GestaltCell;

					float rot = MathF.Sin((Timer - 60) / 60f * 6.28f) * 1.57f;

					if (Timer <= 90)
						rot = Eases.EaseCubicIn((Timer - 60) / 30f) * 1.57f;

					if (Timer > 90)
						rot = 1.57f - Eases.EaseCubicIn((Timer - 90) / 40f) * 3.14f;

					rot *= NPC.direction * -1;

					follower.rotation = rot;

					Vector2 target = followerCell.savedPos.RotatedBy(rot, NPC.Center);
					follower.Center = target;
				}
			}

			if (Timer == 90 || Timer == 130)
			{
				SoundHelper.PlayPitched("Impacts/StoneStrike", 1f, Main.rand.NextFloat(-0.5f, 0f), myFollowers[1].Center);

				for(int k = 0; k < 30; k++)
				{
					Dust.NewDustPerfect(myFollowers[1].Center, DustID.Crimson, Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(10));
					Dust.NewDustPerfect(myFollowers[1].Center, DustID.Blood, Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(15));
				}
			}

			if (Timer == 150)
			{
				for (int k = 0; k < myFollowers.Count; k++)
				{
					var follower = myFollowers[k];
					var followerCell = follower.ModNPC as GestaltCell;

					followerCell.savedPos = follower.Center;
				}
			}

			if (Timer > 150 && Timer <= 180)
			{
				for (int k = 0; k < myFollowers.Count; k++)
				{
					var follower = myFollowers[k];
					var followerCell = follower.ModNPC as GestaltCell;

					Vector2 target = NPC.Center + Vector2.UnitY * -30 * (k + 1);
					follower.Center = Vector2.Lerp(followerCell.savedPos, target, Eases.EaseCubicIn((Timer - 150) / 30f));
				}
			}

			if (Timer > 180)
			{
				autoPositionFollowers = true;
				State = GestaltCellState.RestingOrMoving;
				Timer = 0;
			}
		}

		public void FourCellAttack()
		{
			if (Timer > 60)
			{
				State = GestaltCellState.RestingOrMoving;
				Timer = 0;
			}
		}

		public void FiveCellAttack()
		{
			if (Timer > 60)
			{
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
				3 => NPC.Center + new Vector2(-32, -60),
				4 => NPC.Center + new Vector2(32, -60),
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
			if (StarlightRiver.debugMode)
			{
				var box = arena;
				box.Offset((-Main.screenPosition).ToPoint());

				spriteBatch.Draw(Assets.MagicPixel.Value, box, Color.White * 0.15f);
			}

			var tex = Assets.NPCs.Crimson.GestaltCell.Value;

			Vector2 scale = Vector2.One * NPC.scale;
			scale.X += squish * 2;
			scale.Y -= squish;

			spriteBatch.Draw(tex, NPC.Center - screenPos, NPC.frame, drawColor * opacity, NPC.rotation, NPC.frame.Size() / 2f, scale, NPC.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);

			Rectangle otherCellFrame = new Rectangle(0, 39 * 2, NPC.width, NPC.height);

			return false;
		}
	}
}
