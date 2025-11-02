using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Bosses.TheThinkerBoss;
using StarlightRiver.Content.Projectiles;
using StarlightRiver.Core.DrawingRigs;
using StarlightRiver.Core.Loaders;
using StarlightRiver.Core.Systems.ArmatureSystem;
using StarlightRiver.Core.Systems.PixelationSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Content.NPCs.Crimson
{
	internal class Revenant : ModNPC
	{
		public const int maxFlipTime = 20;

		public const int bodySegment = 1;
		public const int swordSegment0 = 4;
		public const int swordSegment1 = 5;
		public const int swordSegment2 = 6;

		public const int windupDuration = 50;
		public const int swingDuration = 40;

		public const int reviveDuration = 600;

		private static StaticRig rig;

		private static readonly List<(int, int)> connections =
		[
			(1, 0),
			(1, 2),
			(2, 3),
			(1, 4),
			(4, 5),
			(5, 6)
		];

		private float yTarget;
		private int flipTimer;

		private float maxSpeed;

		private readonly Vector2[] stringPoints = new Vector2[7];
		private readonly float[] stringRotations = new float[7];

		private readonly Vector2[] gorePoints = new Vector2[7];
		private readonly Vector2[] goreVels = new Vector2[7];

		public Arm swordArm;
		public bool useArm;

		private List<Vector2> swingTrailCache;
		private Trail swingTrail;

		public ref float Timer => ref NPC.ai[0];
		public ref float State => ref NPC.ai[1];
		public ref float AttackTimer => ref NPC.ai[2];
		public ref float ReviveTimer => ref NPC.ai[3];

		private bool Flipping => flipTimer > 0;
		private float FlipProg => (maxFlipTime - flipTimer) / (float)maxFlipTime;
		private int AttackDuration => windupDuration + swingDuration;

		public override string Texture => AssetDirectory.Invisible;

		public override void Load()
		{
			Stream stream = StarlightRiver.Instance.GetFileStream("Assets/NPCs/Crimson/RevenantRig.json");
			rig = JsonSerializer.Deserialize<StaticRig>(stream);
			stream.Close();
		}

		public override void SetDefaults()
		{
			NPC.lifeMax = 500;
			NPC.width = 64;
			NPC.height = 128;
			NPC.aiStyle = -1;
			NPC.knockBackResist = 0.2f;
			NPC.damage = 30;
			NPC.noGravity = true;

			// We dont really care about the offsets here at all since this is just relative distances for segment
			// length. Start point is arbitrary and will be moved later
			swordArm = new(NPC.Center,
				new ArmSegment[]{
				new(6),
				new(Vector2.Distance(rig.Points[swordSegment0].Pos, rig.Points[swordSegment1].Pos)),
				new(Vector2.Distance(rig.Points[swordSegment1].Pos, rig.Points[swordSegment2].Pos)),
				}, Assets.Invisible.Value);
		}

		/// <summary>
		/// Forcibly resets the arm back to the rigged positions, called before attacks start to make sure
		/// the IK starts from the correct state
		/// </summary>
		public void ResetArmToRig()
		{
			swordArm.start = stringPoints[bodySegment] + Vector2.UnitX * -10 * NPC.direction;
			swordArm.segments[0].rotation = stringPoints[bodySegment].DirectionTo(stringPoints[swordSegment0]).ToRotation();
			swordArm.segments[1].start = stringPoints[swordSegment0];
			swordArm.segments[1].rotation = stringPoints[swordSegment0].DirectionTo(stringPoints[swordSegment1]).ToRotation();
			swordArm.segments[2].start = stringPoints[swordSegment1];
			swordArm.segments[2].rotation = stringPoints[swordSegment1].DirectionTo(stringPoints[swordSegment2]).ToRotation();
		}

		/// <summary>
		/// While using the arm this will overwrite the points in stringPoints related to the sword arm after processing IK
		/// </summary>
		public void ProcessAndSetFromArm()
		{
			swordArm.start = stringPoints[bodySegment] + Vector2.UnitX * -10 * NPC.direction;

			float prog = AttackTimer < 20 ? AttackTimer / 20f : 1f;
			if (AttackTimer > AttackDuration - 20)
				prog = 1f - (AttackTimer - (AttackDuration - 20)) / 20f;

			swordArm.Update();
			stringPoints[swordSegment0] = Vector2.Lerp(stringPoints[swordSegment0], swordArm.segments[0].Endpoint, prog);
			stringPoints[swordSegment1] = Vector2.Lerp(stringPoints[swordSegment1], swordArm.segments[1].Endpoint, prog);
			stringPoints[swordSegment2] = Vector2.Lerp(stringPoints[swordSegment2], swordArm.segments[2].Endpoint, prog);

			float rotAdjust = NPC.direction == 1 ? -1.1f : -2f;

			stringRotations[swordSegment0] = (swordArm.segments[0].rotation + rotAdjust) * prog;
			stringRotations[swordSegment1] = (swordArm.segments[1].rotation + rotAdjust) * prog;
			stringRotations[swordSegment2] = (swordArm.segments[2].rotation + rotAdjust) * prog;
		}

		/// <summary>
		/// Copies the current string points to gore points to start the death animation
		/// </summary>
		public void SetGorePointsToRig()
		{
			for (int k = 0; k < stringPoints.Length; k++)
			{
				gorePoints[k] = stringPoints[k];
			}
		}

		/// <summary>
		/// Updates the stringPoints positions as if they're falling with gravity, for when the revenant is temporarily dead
		/// </summary>
		public void ProcessSegmentGravityWhenDead()
		{
			for (int k = 0; k < stringPoints.Length; k++)
			{
				if (!CollisionHelper.PointInTile(gorePoints[k]))
				{
					if (goreVels[k].Y < 16)
						goreVels[k].Y += 0.4f;

					goreVels[k].X += 0.1f * Vector2.Normalize(gorePoints[k] - NPC.Center).X;
				}
				else
				{
					goreVels[k].Y *= 0;
					goreVels[k].X *= 0.95f;
				}

				if (ReviveTimer > reviveDuration - 240)
				{
					float prog = (ReviveTimer - (reviveDuration - 240)) / 240f;
					goreVels[k] = Main.rand.NextVector2Circular(prog * 2, prog * 2);
				}

				gorePoints[k] += goreVels[k];

				Vector2 target = gorePoints[k];
				Vector2 final = ReviveTimer < (reviveDuration - 30) ? target : Vector2.Lerp(target, stringPoints[k], Eases.EaseCircularInOut((ReviveTimer - (reviveDuration - 30)) / 30f));

				stringRotations[k] = (final.X - stringPoints[k].X) * 0.05f;
				stringPoints[k] = final;
			}
		}

		/// <summary>
		/// Calculates the final segment points into the stringPoints array, includes the rig and turning animations, and if useArm is true
		/// then the arm will override the rigged positions.
		/// </summary>
		public void CalculateSegmentPoints()
		{
			int offsetAmount = 10;

			foreach (StaticRigPoint point in rig.Points)
			{
				Vector2 offset = NPC.direction == 1 ? new Vector2(-offsetAmount, 0) : new Vector2(offsetAmount, 0);
				Vector2 origin = NPC.direction == 1 ? Vector2.Zero : new Vector2(42, 0);

				Vector2 apos = offset + (NPC.direction == 1 ? point.Pos : new Vector2(point.Pos.X * -1 + NPC.width, point.Pos.Y));

				if (Flipping)
				{
					if (FlipProg > 0.5f)
					{
						origin = NPC.direction == -1 ? Vector2.Zero : new Vector2(42, 0);
						offset = NPC.direction == -1 ? new Vector2(-offsetAmount, 0) : new Vector2(offsetAmount, 0);
					}

					Vector2 flipTarget = offset + (NPC.direction == -1 ? point.Pos : new Vector2(point.Pos.X * -1 + NPC.width, point.Pos.Y));
					apos = Vector2.Lerp(apos, flipTarget, Eases.EaseCircularInOut(FlipProg));
				}

				Vector2 targetPos = NPC.position + apos + offset;
				targetPos.Y += 2 * MathF.Sin(Timer * 0.02f * 6.28f + point.Frame * 0.3f);

				Vector2 centerPos = targetPos + new Vector2(21, 27) - origin;
				stringPoints[rig.Points.IndexOf(point)] = centerPos.RotatedBy(NPC.rotation, NPC.Center);
			}

			if (useArm)
				ProcessAndSetFromArm();

			if (State == 3)
				ProcessSegmentGravityWhenDead();
		}

		public override void AI()
		{
			Timer++;

			if (State != 3)
				Hover();

			CalculateSegmentPoints();

			// passive
			if (State == 0)
			{
				Lighting.AddLight(NPC.Center, new Vector3(0.35f, 0.5f, 0.3f));
				NPC.velocity.X *= 0.99f;
			}

			// hostile
			if (State == 1)
			{
				NPC.TargetClosest(false);

				maxSpeed = 4;

				if (NPC.HasValidTarget)
				{
					Player target = Main.player[NPC.target];

					if (!Flipping)
						NPC.velocity.X += NPC.direction * 0.05f;

					if (Math.Abs(NPC.velocity.X) > maxSpeed)
						NPC.velocity.X = NPC.velocity.X > 0 ? maxSpeed : -maxSpeed;

					int targetDir = target.Center.X > NPC.Center.X ? 1 : -1;
					HandleFlip(targetDir);

					if (Vector2.Distance(NPC.Center, target.Center) < 256 && !Flipping)
						StartAttack();
				}

				Lighting.AddLight(NPC.Center, new Vector3(0.5f, 0.3f, 0.3f));
			}

			// attack
			if (State == 2)
			{
				if (NPC.HasValidTarget)
				{
					Player target = Main.player[NPC.target];

					if (Math.Sign(target.Center.X - NPC.Center.X) == NPC.direction && AttackTimer < windupDuration / 2)
						NPC.velocity.X += NPC.direction * 0.05f;
					else
						NPC.velocity.X *= 0.92f;

					if (Math.Abs(NPC.velocity.X) > maxSpeed)
						NPC.velocity.X = NPC.velocity.X > 0 ? maxSpeed : -maxSpeed;
				}

				maxSpeed = 4;
				Attack();
				Lighting.AddLight(NPC.Center, new Vector3(0.5f, 0.3f, 0.3f));
			}

			if (State == 3)
			{
				NPC.velocity *= 0;

				AttackTimer = 0;
				ReviveTimer++;

				if (ReviveTimer >= 600)
				{
					State = 1;
					NPC.life = NPC.lifeMax;
					NPC.dontTakeDamage = false;
				}
			}

			if (!Main.dedServ)
			{
				ManageCaches();
				ManageTrail();
			}
		}

		/// <summary>
		/// Processes the flipping animation for the given target direction
		/// </summary>
		/// <param name="targetDir"></param>
		public void HandleFlip(int targetDir)
		{
			if (NPC.direction == 0)
				NPC.direction = 1;

			if (!Flipping && NPC.direction != targetDir)
				flipTimer = maxFlipTime;

			if (Flipping)
			{
				flipTimer--;

				if (flipTimer == 0)
					NPC.direction *= -1;
			}
		}

		/// <summary>
		/// Logic for hovering above the ground, sets the Y position
		/// </summary>
		public void Hover()
		{
			bool fall = true;

			if (yTarget == 0)
				yTarget = NPC.position.Y;

			for (int k = -3; k < 8; k++)
			{
				Tile tile = Framing.GetTileSafely((int)NPC.position.X / 16 + k, (int)(yTarget + NPC.height + 32) / 16);

				if (tile.HasTile && (Main.tileSolid[tile.type] || Main.tileSolidTop[tile.type]))
					fall = false;
			}

			if (fall)
				yTarget += 4;
			else
				yTarget -= 2;

			float finalTarget = yTarget + MathF.Sin(Timer * 0.05f) * 16;

			if (Math.Abs(NPC.position.Y - finalTarget) > 2)
				NPC.position.Y += (finalTarget - NPC.position.Y) * 0.1f;
		}

		/// <summary>
		/// Sets the state and resets the timer to start an attack
		/// </summary>
		public void StartAttack()
		{
			ResetArmToRig();
			useArm = true;

			State = 2;
			AttackTimer = 0;
		}

		/// <summary>
		/// Triggers the IK logic for the arm during the attack and calculates collision
		/// </summary>
		public void Attack()
		{
			AttackTimer++;

			Player target = Main.player[NPC.target];

			if (Vector2.Distance(NPC.Center, target.Center) < 96 && AttackTimer < windupDuration)
				NPC.velocity.X *= 0.9f;

			if (AttackTimer <= windupDuration)
			{
				var spline = new SplineHelper.SplineData(
					NPC.Center + new Vector2(0, swordArm.MaxLen * 0.8f),
					NPC.Center + new Vector2(swordArm.MaxLen * 0.9f * NPC.direction, 0),
					NPC.Center + new Vector2(30 * NPC.direction, -swordArm.MaxLen * 0.95f));

				Vector2 endPoint = SplineHelper.PointOnSpline(Eases.EaseCircularInOut(AttackTimer / windupDuration), spline);
				swordArm.IKToPoint(endPoint);
			}

			if (AttackTimer == windupDuration)
			{
				if (Main.rand.NextBool())
					SoundHelper.PlayPitched("NPC/Crimson/RevenantSlash1", 1f, 0f, NPC.Center);
				else
					SoundHelper.PlayPitched("NPC/Crimson/RevenantSlash2", 1f, 0f, NPC.Center);

				NPC.velocity.X += 8 * NPC.direction;

				swingTrailCache.Clear();

				for (int i = 0; i < 20; i++)
				{
					swingTrailCache.Add(NPC.Center + new Vector2(30 * NPC.direction, -70 * 0.95f));
				}
			}

			if (AttackTimer >= windupDuration && AttackTimer < (windupDuration + swingDuration / 2f))
			{
				maxSpeed = 8;
				NPC.velocity.X *= 0.98f;
			}

			if (AttackTimer > windupDuration && AttackTimer <= (windupDuration + swingDuration))
			{
				float prog = Eases.EaseQuinticOut((AttackTimer - windupDuration) / swingDuration);
				prog = prog > 0.5f ? 0.5f - prog / 2f : prog;
				swordArm.segments[1].length = Vector2.Distance(rig.Points[swordSegment0].Pos, rig.Points[swordSegment1].Pos) + 40 * prog;
				swordArm.segments[2].length = Vector2.Distance(rig.Points[swordSegment1].Pos, rig.Points[swordSegment2].Pos) + 20 * prog;

				float rot = Eases.EaseQuadInOut((AttackTimer - (windupDuration - 20)) / (swingDuration + 20));
				rot = rot > 0.5f ? 0.5f - rot / 2f : rot;
				NPC.rotation = rot * 0.5f * NPC.direction;

				var spline = new SplineHelper.SplineData(
					new Vector2(30 * NPC.direction, -70 * 0.95f),
					new Vector2(160 * NPC.direction, 0),
					Vector2.UnitY.RotatedBy(0.8f * NPC.direction) * 70 * 1.5f);

				Vector2 splinePoint = NPC.Center + SplineHelper.PointOnSpline(Eases.EaseQuinticOut((AttackTimer - windupDuration) / swingDuration), spline);
				swordArm.IKToPoint(splinePoint);

				if (AttackTimer < (windupDuration + swingDuration / 3f))
				{
					for (int k = 0; k < 2f; k++)
					{
						Vector2 prev = NPC.Center + NPC.velocity * k / 2f + SplineHelper.PointOnSpline(Eases.EaseQuinticOut((AttackTimer - 1 - windupDuration + k / 2f) / swingDuration), spline);
						Vector2 endPoint = NPC.Center + NPC.velocity * k / 2f + SplineHelper.PointOnSpline(Eases.EaseQuinticOut((AttackTimer - windupDuration + k / 2f) / swingDuration), spline);

						swingTrailCache.Add(endPoint);

						float colProg = (AttackTimer - windupDuration) / swingDuration;
						Dust.NewDustPerfect(endPoint + Main.rand.NextVector2Circular(4f, 4f), ModContent.DustType<Dusts.PixelatedImpactLineDust>(), endPoint.DirectionTo(prev).RotatedBy(0.5f * NPC.direction) * (3 + 3 * prog), 0, new Color(1, colProg, colProg * 0.8f, 0), 0.5f * colProg);
					}

					// Collision
					foreach (Player player in Main.ActivePlayers)
					{
						if (CollisionHelper.CheckLinearCollision(NPC.Center, splinePoint + NPC.Center.DirectionTo(splinePoint) * 8, player.Hitbox, out _))
							player.Hurt(PlayerDeathReason.ByNPC(NPC.whoAmI), NPC.damage, 0);
					}
				}
			}

			if (AttackTimer >= AttackDuration)
			{
				State = 1;
				AttackTimer = 0;
				useArm = false;
			}
		}

		public override bool CanHitPlayer(Player target, ref int cooldownSlot)
		{
			return false;
		}

		public override void HitEffect(NPC.HitInfo hit)
		{
			if (State == 0)
				State = 1;
		}

		public override bool CheckDead()
		{
			NPC.life = 1;
			NPC.dontTakeDamage = true;
			SetGorePointsToRig();

			State = 3;
			ReviveTimer = 0;

			SoundEngine.PlaySound(SoundID.DD2_EtherianPortalOpen, NPC.Center);
			SoundEngine.PlaySound(SoundID.DD2_MonkStaffGroundImpact, NPC.Center);

			for (int k = 0; k < 50; k++)
			{
				Dust.NewDust(NPC.position + Vector2.UnitY * 32, NPC.width, NPC.height, ModContent.DustType<Dusts.PixelatedEmber>(), 0, -Main.rand.NextFloat(5), 0, new Color(255, Main.rand.Next(50, 200), Main.rand.Next(50, 200), 0), Main.rand.NextFloat(0.25f));
			}

			Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + new Vector2(0, NPC.height / 2f), Vector2.Zero, ModContent.ProjectileType<ReusableHallucinationZone>(), 0, 0, Main.myPlayer, 120, 600);

			return false;
		}

		protected void ManageCaches()
		{
			if (swingTrailCache == null)
			{
				swingTrailCache = [];

				for (int i = 0; i < 20; i++)
				{
					swingTrailCache.Add(NPC.Center);
				}
			}

			//swingTrailCache.Add(NPC.Center);

			while (swingTrailCache.Count > 20)
			{
				swingTrailCache.RemoveAt(0);
			}
		}

		protected void ManageTrail()
		{
			if (swingTrail is null || swingTrail.IsDisposed)
			{
				swingTrail = new Trail(Main.instance.GraphicsDevice, 20, new NoTip(), factor =>
				{
					float attackProg = Math.Clamp((AttackTimer - windupDuration) * 3 / swingDuration, 0, 1);
					float trueFactor = (factor - (1 - attackProg)) * (1 / attackProg);
					return MathF.Sin(trueFactor * 3.14f) * 16;
				}, factor =>
				{
					float attackProg = Math.Clamp((AttackTimer - windupDuration) * 3 / swingDuration, 0, 1);
					float trueFactor = (factor.X - (1 - attackProg)) * (1 / attackProg);

					float alpha = trueFactor;

					if (factor.X == 1)
						alpha = 0;

					if (AttackTimer <= windupDuration + 5)
						alpha *= Math.Max(0, (AttackTimer - windupDuration) / 5f);

					if (AttackTimer >= AttackDuration - 20)
						alpha *= 1f - (AttackTimer - (AttackDuration - 20)) / 20f;

					Color color;

					float r = 1f;
					float g = 0.5f * trueFactor;
					float b = 0.4f * trueFactor;
					color = new Color(r, g, b, 1);

					return color * alpha;
				});
			}

			swingTrail.Positions = swingTrailCache.ToArray();
			swingTrail.NextPosition = NPC.Center;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D tex = Assets.NPCs.Crimson.Revenant.Value;
			Texture2D texGlow = Assets.NPCs.Crimson.RevenantGlow.Value;
			Texture2D indi = Assets.NPCs.Crimson.DepressorChain.Value;

			int offsetAmount = 10;

			int frameY = 0; // (int)(Timer / 10f) % 3;

			/*if (State > 0)
				frameY += 3;*/

			SpriteEffects effects = NPC.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

			if (Flipping)
			{
				if (FlipProg > 0.5f)
				{
					effects = NPC.direction == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
				}
			}

			for (int k = 0; k < stringPoints.Length; k++)
			{
				Vector2 point = stringPoints[k];
				StaticRigPoint rigPoint = rig.Points[k];

				Vector2 targetPos = point - Main.screenPosition;
				var frame = new Rectangle(rigPoint.Frame * 48, frameY * 52, 48, 52);
				Color color = new(Lighting.GetSubLight(point));

				spriteBatch.Draw(tex, targetPos, frame, color, stringRotations[k] + NPC.rotation, new Vector2(24, 26), 1f, effects, 0);
				//spriteBatch.Draw(texGlow, targetPos, frame, new Color(100, 100, 100, 100), stringRotations[k] + NPC.rotation, new Vector2(21, 27), 1f, effects, 0);
			}

			// Enqueues the pixelated links
			if (State != 3)
			{
				ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderNPCs", () =>
				{
					Effect effect = ShaderLoader.GetShader("GestaltLine").Value;

					if (effect != null)
					{
						effect.Parameters["u_time"].SetValue(Main.GameUpdateCount * 0.1f);
						effect.Parameters["u_speed"].SetValue(1f);
						effect.Parameters["color1"].SetValue(new Vector3(0.9f, 0.7f, 0.6f));
						effect.Parameters["color2"].SetValue(new Vector3(0.9f, 0.8f, 0.6f));
						effect.Parameters["color3"].SetValue(new Vector3(0.95f, 0.6f, 0.8f));

						spriteBatch.End();
						spriteBatch.Begin(default, default, default, default, Main.Rasterizer, effect);

						foreach ((int, int) edge in connections)
						{
							Texture2D tex = Assets.Misc.Line.Value;

							Vector2 lastpos = stringPoints[edge.Item1];
							Vector2 pos = stringPoints[edge.Item2];
							float dist = Vector2.Distance(pos, lastpos);
							float rot = pos.DirectionTo(lastpos).ToRotation();

							var target = new Rectangle((int)(pos.X - Main.screenPosition.X), (int)(pos.Y - Main.screenPosition.Y), (int)dist, 46);
							var color = Color.Lerp(new Color(255, 160, 100, 150), new Color(255, 100, 220, 150), 0.5f + MathF.Sin(Main.GameUpdateCount / 4f) * 0.5f);

							spriteBatch.Draw(tex, target, null, color, rot, new Vector2(0, tex.Height / 2f), 0, 0);
						}

						spriteBatch.End();
						spriteBatch.Begin(default, default, default, default, Main.Rasterizer, default);
					}
				});
			}

			// Enqueue trail to draw
			if (State == 2 && AttackTimer > windupDuration)
			{
				ModContent.GetInstance<PixelationSystem>().QueueRenderAction("OverPlayers", () =>
				{
					Effect effect = ShaderLoader.GetShader("CeirosRing").Value;

					if (effect != null)
					{
						var world = Matrix.CreateTranslation(-Main.screenPosition.ToVector3());
						Matrix view = Matrix.Identity;
						var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

						effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.01f);
						effect.Parameters["repeats"].SetValue(1f);
						effect.Parameters["transformMatrix"].SetValue(world * view * projection);
						effect.Parameters["sampleTexture"].SetValue(Assets.ShadowTrail.Value);
						swingTrail?.Render(effect);

						effect.Parameters["sampleTexture"].SetValue(Assets.LiquidTrailAlt.Value);
						swingTrail?.Render(effect);
					}
				});
			}

			return false;
		}
	}
}