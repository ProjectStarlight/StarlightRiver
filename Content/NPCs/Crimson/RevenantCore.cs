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

		public NPC ParentRevenant
		{
			get => Main.npc[(int)NPC.ai[0]];
			set => NPC.ai[0] = value.whoAmI;
		}

		public ref float Timer => ref NPC.ai[1];
		public ref float NextChange => ref NPC.ai[2];

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
			NPC.lifeMax = 200;
			NPC.scale = 0;

			NPC.GetGlobalNPC<BarrierNPC>().maxBarrier = 200;
			NPC.GetGlobalNPC<BarrierNPC>().barrier = 200;
		}

		public override void AI()
		{
			Timer++;

			if (NPC.scale < 1)
				NPC.scale += 0.05f;

			if (!ParentRevenant.active || ParentRevenant.type != ModContent.NPCType<Revenant>())
				NPC.active = false;

			NPC.position += (ParentRevenant.Center - NPC.Center) * 0.05f;

			NPC.velocity += randomDirection * 0.05f;
			NPC.velocity *= 0.98f;

			if (Timer >= NextChange)
			{
				randomDirection = Main.rand.NextVector2Circular(1, 1);
				NextChange += Main.rand.Next(30, 120);
			}
		}

		public override void OnKill()
		{
			ParentRevenant.Kill();
		}

		private void DrawRevenantCoreHallucination(SpriteBatch spriteBatch)
		{
			if (toRender != null && toRender.Count > 0)
			{
				Effect petalShader = ShaderLoader.GetShader("ThinkerPetal").Value;

				if (petalShader != null)
				{
					foreach (RevenantCore core in toRender)
					{
						petalShader.Parameters["u_resolution"].SetValue(Assets.Bosses.TheThinkerBoss.PetalSmall.Size());
						petalShader.Parameters["u_time"].SetValue(Main.GameUpdateCount * 0.015f);

						petalShader.Parameters["mainbody_t"].SetValue(Assets.GlowTrail.Value);
						petalShader.Parameters["linemap_t"].SetValue(Assets.GlowTopTrail.Value);
						petalShader.Parameters["noisemap_t"].SetValue(Assets.Noise.ShaderNoise.Value);
						petalShader.Parameters["overlay_t"].SetValue(Assets.Bosses.TheThinkerBoss.HeartOver.Value);
						petalShader.Parameters["u_color"].SetValue(new Vector3(0.5f, 0.3f, 0.3f) * core.NPC.scale);
						petalShader.Parameters["u_fade"].SetValue(new Vector3(0.1f, 0.2f, 0.1f));

						Vector2 pos = core.NPC.Center - Main.screenPosition;

						spriteBatch.End();
						spriteBatch.Begin(SpriteSortMode.Immediate, default, SamplerState.PointWrap, default, default, petalShader, Main.GameViewMatrix.ZoomMatrix);

						Texture2D smallPetal = Assets.Bosses.TheThinkerBoss.PetalSmall.Value;

						var origin = new Vector2(0, smallPetal.Height / 2f);

						for (int k = 0; k < 5; k++)
						{
							petalShader.Parameters["u_time"].SetValue(Main.GameUpdateCount * 0.015f + k * 0.2f);
							float thisScale = Eases.EaseQuadInOut((core.NPC.scale - 0.5f) / 0.5f);
							float rot = k / 5f * 6.28f;
							spriteBatch.Draw(smallPetal, pos + Vector2.UnitX.RotatedBy(rot) * 64, null, Color.White, rot, origin, 1, 0, 0);
						}

						spriteBatch.End();
						spriteBatch.Begin(default, default, default, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);
					}
				}

				toRender.RemoveAll(n => n is null || !n.NPC.active);
			}
		}
	}
}
