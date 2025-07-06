using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core.DrawingRigs;
using StarlightRiver.Core.Loaders;
using StarlightRiver.Core.Systems.ArmatureSystem;
using StarlightRiver.Core.Systems.PixelationSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Terraria;

namespace StarlightRiver.Content.NPCs.Crimson
{
	internal class Revenant : ModNPC
	{
		public const int maxFlipTime = 20;

		public const int bodySegment = 7;
		public const int swordSegment0 = 3;
		public const int swordSegment1 = 1;
		public const int swordSegment2 = 4;

		private static StaticRig rig;

		private float yTarget;
		private int flipTimer;

		private Vector2[] stringPoints = new Vector2[8];
		private float[] stringRotations = new float[8];

		private List<(int, int)> connections =
		[
			(7, 2),
			(2, 0),
			(7, 3),
			(3, 1),
			(1, 4),
			(7, 6)
		];

		public Arm swordArm;
		public bool useArm;

		public ref float Timer => ref NPC.ai[0];
		public ref float State => ref NPC.ai[1];
		public ref float AttackTimer => ref NPC.ai[2];

		private bool Flipping => flipTimer > 0;
		private float FlipProg => (maxFlipTime - flipTimer) / (float)maxFlipTime;

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
			NPC.noGravity = true;

			// We dont really care about the offsets here at all since this is just relative distances for segment
			// length. Start point is arbitrary and will be moved later
			swordArm = new(NPC.Center,
				new ArmSegment[]{
				new(Vector2.Distance(rig.Points[bodySegment].Pos, rig.Points[swordSegment0].Pos)),
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
			swordArm.start = stringPoints[bodySegment];
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
			swordArm.start = stringPoints[bodySegment];

			float prog = AttackTimer < 10 ? AttackTimer / 10f : 1f;
			if (AttackTimer > 110)
				prog = 1f - (AttackTimer - 110) / 10f;

			swordArm.Update();
			stringPoints[swordSegment0] = Vector2.Lerp(stringPoints[swordSegment0], swordArm.segments[0].Endpoint, prog);
			stringPoints[swordSegment1] = Vector2.Lerp(stringPoints[swordSegment1], swordArm.segments[1].Endpoint, prog);
			stringPoints[swordSegment2] = Vector2.Lerp(stringPoints[swordSegment2], swordArm.segments[2].Endpoint, prog);

			stringRotations[swordSegment0] = swordArm.segments[0].rotation;
			stringRotations[swordSegment1] = swordArm.segments[1].rotation;
			stringRotations[swordSegment2] = swordArm.segments[2].rotation + (NPC.direction == 1 ? 5.1f : -2f);
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

				Vector2 targetPos = NPC.position + apos + offset - Main.screenPosition;
				targetPos.Y += 2 * MathF.Sin(Timer * 0.02f * 6.28f + point.Frame * 0.3f);

				Vector2 centerPos = targetPos + new Vector2(21, 27) - origin;
				stringPoints[rig.Points.IndexOf(point)] = centerPos + Main.screenPosition;
			}

			if (useArm)
				ProcessAndSetFromArm();
			else
			{
				for (int k = 0; k < stringRotations.Length; k++)
					stringRotations[k] *= 0.9f;
			}
		}

		public override void AI()
		{
			Timer++;

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

				if (NPC.HasValidTarget)
				{
					Player target = Main.player[NPC.target];

					if (!Flipping)
						NPC.velocity.X += NPC.direction * 0.05f;

					if (Math.Abs(NPC.velocity.X) > 2)
						NPC.velocity.X = NPC.velocity.X > 0 ? 2 : -2;

					int targetDir = target.Center.X > NPC.Center.X ? 1 : -1;
					HandleFlip(targetDir);
				}

				if (Timer % 300 == 0)
					StartAttack();

				Lighting.AddLight(NPC.Center, new Vector3(0.5f, 0.3f, 0.3f));
			}

			// attack
			if (State == 2)
			{
				NPC.velocity.X *= 0.92f;

				Attack();
				Lighting.AddLight(NPC.Center, new Vector3(0.5f, 0.3f, 0.3f));
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

			if (AttackTimer < 120)
			{
				//var spline = 
				//Vector2 endPoint = NPC.Center
				swordArm.IKToPoint(Main.MouseWorld);
			}

			if (AttackTimer >= 120)
			{
				State = 1;
				AttackTimer = 0;
				useArm = false;
			}	

			// Calculate collision here 
		}

		public override void HitEffect(NPC.HitInfo hit)
		{
			if (State == 0)
				State = 1;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D tex = Assets.NPCs.Crimson.Revenant.Value;
			Texture2D texGlow = Assets.NPCs.Crimson.RevenantGlow.Value;
			Texture2D indi = Assets.NPCs.Crimson.DepressorChain.Value;

			int offsetAmount = 10;

			var frameY = (int)(Timer / 40f) % 3;

			if (State > 0)
				frameY += 3;

			for(int k = 0; k < stringPoints.Length; k++)
			{
				Vector2 point = stringPoints[k];
				StaticRigPoint rigPoint = rig.Points[k];

				SpriteEffects effects = NPC.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
				Vector2 origin = NPC.direction == 1 ? Vector2.Zero : new Vector2(0, 0);

				if (Flipping)
				{
					if (FlipProg > 0.5f)
					{
						effects = NPC.direction == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
						origin = NPC.direction == -1 ? Vector2.Zero : new Vector2(0, 0);
					}
				}
				Vector2 targetPos = point + origin - Main.screenPosition;
				var frame = new Rectangle(rigPoint.Frame * 42, frameY * 54, 42, 54);

				spriteBatch.Draw(tex, targetPos, frame, drawColor, stringRotations[k], new Vector2(21, 27), 1f, effects, 0);
				spriteBatch.Draw(texGlow, targetPos, frame, Color.White, stringRotations[k], new Vector2(21, 27), 1f, effects, 0);
			}

			// Enqueues the pixelated links
			ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderNPCs", () =>
			{
				Effect effect = ShaderLoader.GetShader("GestaltLine").Value;

				if (effect != null)
				{
					effect.Parameters["u_time"].SetValue(Main.GameUpdateCount * 0.1f);
					effect.Parameters["u_speed"].SetValue(1f);

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

			return false;
		}
	}
}
