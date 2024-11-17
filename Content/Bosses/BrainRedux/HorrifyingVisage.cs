using StarlightRiver.Core.Systems.PixelationSystem;
using StarlightRiver.Core;
using System;
using Terraria.Audio;
using Terraria.ID;
using StarlightRiver.Content.Physics;
using System.Collections.Generic;
using System.Linq;
using StarlightRiver.Helpers;
using Terraria.Graphics.Effects;

namespace StarlightRiver.Content.Bosses.BrainRedux
{
	internal class HorrifyingVisage : ModNPC
	{
		public VerletChain chain;
		private List<Vector2> chainCache;
		private Trail chainTrail;

		public ref float Timer => ref NPC.ai[0];
		public ref float State => ref NPC.ai[1];
		public ref float Decay => ref NPC.ai[2];

		public override string Texture => AssetDirectory.Invisible;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Horrifying Visage");
		}

		public override void SetDefaults()
		{
			NPC.lifeMax = 25;
			NPC.width = 160;
			NPC.height = 110;
			NPC.damage = 30;
			NPC.noGravity = true;
			NPC.aiStyle = -1;
			NPC.chaseable = false;
			NPC.knockBackResist = 0f;

			chain = new VerletChain(33, true, NPC.Center, 4);
		}

		public override void AI()
		{
			Timer++;

			// Update the chain
			if (chain != null)
			{
				chain.startPoint = NPC.Center + Vector2.UnitY * 90;
				chain.useEndPoint = false;
				chain.drag = 1.1f;
				chain.forceGravity = Vector2.UnitY * 1f;
				chain.constraintRepetitions = 30;
				chain.UpdateChain();

				chain.IterateRope(a => chain.ropeSegments[a].posNow.X += (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.1f);

				Lighting.AddLight(chain.ropeSegments.Last().posNow, Color.Red.ToVector3() * 0.45f);
				for (int k = 0; k < 5; k++)
				{
					Dust.NewDust(chain.ropeSegments.Last().posNow, 8, 8, DustID.Blood);
				}
			}

			Lighting.AddLight(NPC.Center, new Vector3(0.5f, 0.4f, 0.2f));

			if (Timer > 540)
				Decay++;

			if (State == 1)
			{
				NPC.TargetClosest();

				if (NPC.target >= 0)
					NPC.velocity += NPC.Center.DirectionTo(Main.player[NPC.target].Center) * 2.5f;

				Decay++;
			}

			if (Decay >= 60)
				NPC.active = false;

			// Update cache/trail on client only
			if (!Main.dedServ)
			{
				chain.UpdateCacheFromChain(ref chainCache);

				if (chainTrail is null || chainTrail.IsDisposed)
				{
					chainTrail = new Trail(Main.instance.GraphicsDevice, chain.segmentCount, new NoTip(), factor => 32,
					factor =>
					{
						int index = (int)(factor.X * chain.segmentCount);
						index = Math.Clamp(index, 0, chain.segmentCount - 1);

						float opacity = Math.Min(1f, Timer / 30f);
						return Lighting.GetColor((chain.ropeSegments[index].posNow / 16).ToPoint()) * opacity;
					});
				}

				chainTrail.Positions = chainCache.ToArray();
			}
		}

		public override bool CheckDead()
		{
			if (State != 1)
			{
				SoundEngine.PlaySound(SoundID.NPCDeath10.WithPitchOffset(0.5f), NPC.Center);

				State = 1;
				NPC.life = NPC.lifeMax;
				NPC.dontTakeDamage = true;
				NPC.immortal = true;
				return false;
			}

			return false;
		}

		/// <summary>
		/// Draws the trail to simulate the brain connected trail on the dead brain
		/// </summary>
		private void DrawTrail()
		{
			Effect effect = Filters.Scene["RepeatingChain"].GetShader().Shader;

			var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.TransformationMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["alpha"].SetValue(1f);
			effect.Parameters["repeats"].SetValue(3.3f);
			effect.Parameters["transformMatrix"].SetValue(world * view * projection);

			effect.Parameters["sampleTexture"].SetValue(Assets.Bosses.BrainRedux.DeadTeather.Value);
			chainTrail?.Render(effect);
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			DrawTrail();

			var frame = new Rectangle(0, 182 * 4 + 182 * (int)(Timer / 10f % 4), 200, 182);
			float opacity = Math.Min(1f, Timer / 30f);

			if (Decay > 30)
				opacity *= 1 - (Decay - 30) / 30f;

			if (opacity >= 1)
			{
				DeadBrain.DrawBrainSegments(spriteBatch, NPC, NPC.Center - Main.screenPosition, drawColor, NPC.rotation, NPC.scale, opacity);
			}
			else
			{
				for (int k = 0; k < 6; k++)
				{
					float rot = k / 6f * 6.28f + opacity * 3.14f;
					Vector2 offset = Vector2.UnitX.RotatedBy(rot) * (1 - opacity) * 64;

					DeadBrain.DrawBrainSegments(spriteBatch, NPC, NPC.Center + offset - Main.screenPosition, drawColor, NPC.rotation, NPC.scale, opacity * 0.2f);
				}
			}

			return false;
		}
	}
}
