using StarlightRiver.Content.Biomes;
using StarlightRiver.Core.Loaders;
using StarlightRiver.Core.Systems.BarrierSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Content.NPCs.Crimson
{
	internal class RevenantCore : ModNPC
	{
		public static List<RevenantCore> toRender = [];

		public Vector2 randomDirection;
		public Vector2 savedPos;

		public NPC ParentNPC
		{
			get => Main.npc[(int)NPC.ai[0]];
			set => NPC.ai[0] = value.whoAmI;
		}

		public ref float Timer => ref NPC.ai[1];
		public ref float NextChange => ref NPC.ai[2];

		public Revenant ParentRevenant => ParentNPC.ModNPC as Revenant;

		public override string Texture => AssetDirectory.Invisible;

		public override void Load()
		{
			GraymatterBiome.onDrawOverHallucinationMap += DrawRevenantCoreHallucination;
		}

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
		}

		public override void SetDefaults()
		{
			NPC.width = 32;
			NPC.height = 32;
			NPC.lifeMax = 100;
			NPC.scale = 0;
			NPC.noGravity = true;

			NPC.GetGlobalNPC<BarrierNPC>().maxBarrier = 150;
			NPC.GetGlobalNPC<BarrierNPC>().barrier = 150;

			toRender.Add(this);
		}

		public override void AI()
		{
			Timer++;

			if (NPC.scale < 1)
			{
				NPC.scale += 0.02f;
				Dust.NewDustPerfect(NPC.Center, ModContent.DustType<Dusts.GraymatterDust>(), Main.rand.NextVector2Circular(2, 2), 0, default, 0.6f);
			}

			if (!ParentNPC.active || ParentNPC.type != ModContent.NPCType<Revenant>())
				NPC.active = false;

			if (ParentRevenant.ReviveTimer == 540)
				savedPos = NPC.Center;

			if (ParentRevenant.ReviveTimer > 540)
			{
				float prog = (ParentRevenant.ReviveTimer - 540) / 60f;
				NPC.Center = Vector2.SmoothStep(savedPos, ParentNPC.Center, prog);
				savedPos += NPC.velocity * (1f - prog);
				NPC.scale = 1f - prog;
			}

			if (ParentRevenant.State != 3)
			{
				NPC.active = false;
			}

			NPC.position += (ParentNPC.Center - NPC.Center) * 0.05f;

			NPC.velocity += randomDirection * 0.2f;
			NPC.velocity *= 0.99f;

			if (Timer >= NextChange)
			{
				randomDirection = Main.rand.NextVector2Circular(1, 1);
				NextChange += Main.rand.Next(20, 30);
			}

			NPC.TargetClosest();

			if (NPC.target != -1)
				NPC.velocity += Main.player[NPC.target].Center.DirectionTo(NPC.Center) * 0.1f;
		}

		public override void OnKill()
		{
			for (int k = 0; k < 25; k++)
			{
				Dust.NewDustPerfect(NPC.Center, ModContent.DustType<Dusts.PixelatedImpactLineDust>(), Main.rand.NextVector2Circular(1, 1) * Main.rand.NextFloat(5, 40), 0, new Color(255, Main.rand.Next(50, 255), 50, 0), Main.rand.NextFloat(0.5f));
			}

			ParentNPC.Kill();
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			
			return false;
		}

		public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
		{
			position = ParentNPC.Center;
			return true;
		}

		private void DrawRevenantCoreHallucination(SpriteBatch spriteBatch)
		{
			if (toRender != null && toRender.Count > 0)
			{
				Effect petalShader = ShaderLoader.GetShader("ThinkerBody").Value;

				if (petalShader != null)
				{
					foreach (RevenantCore core in toRender)
					{
						petalShader.Parameters["u_resolution"].SetValue(Assets.NPCs.Crimson.RevenantCorePetal.Size());
						petalShader.Parameters["u_time"].SetValue(Main.GameUpdateCount * 0.015f);

						petalShader.Parameters["mainbody_t"].SetValue(Assets.GlowTrail.Value);
						petalShader.Parameters["linemap_t"].SetValue(Assets.StarTexture.Value);
						petalShader.Parameters["noisemap_t"].SetValue(Assets.Noise.ShaderNoise.Value);
						petalShader.Parameters["overlay_t"].SetValue(Assets.Invisible.Value);
						petalShader.Parameters["u_color"].SetValue(new Vector3(0.7f, 0.3f, 0.3f) * core.NPC.scale);
						petalShader.Parameters["u_fade"].SetValue(new Vector3(0.3f, 0.5f, 0.3f));

						petalShader.Parameters["normal_t"].SetValue(Assets.Invisible.Value);
						petalShader.Parameters["mask_t"].SetValue(Assets.MagicPixel.Value);

						Vector2 pos = core.NPC.Center - Main.screenPosition;

						spriteBatch.End();
						spriteBatch.Begin(SpriteSortMode.Immediate, default, SamplerState.PointWrap, default, default, petalShader, Main.GameViewMatrix.ZoomMatrix);

						Texture2D smallPetal = Assets.NPCs.Crimson.RevenantCorePetal.Value;

						spriteBatch.Draw(smallPetal, core.NPC.Center - Main.screenPosition, null, Color.White, core.NPC.rotation, smallPetal.Size() / 2f, core.NPC.scale, 0, 0);

						spriteBatch.End();
						spriteBatch.Begin(default, default, default, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);
					}
				}

				toRender.RemoveAll(n => n is null || !n.NPC.active);
			}
		}
	}
}
