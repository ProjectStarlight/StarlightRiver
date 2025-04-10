using StarlightRiver.Core.Systems.PixelationSystem;
using System;
using Terraria.Audio;
using Terraria.ID;

namespace StarlightRiver.Content.NPCs.Crimson
{
	enum NeuronChomperState : int
	{
		Idle,
		Charging,
		Dashing
	}

	internal class NeuronChomper : ModNPC
	{
		public Vector2 homePos;
		public Vector2 targetPos;

		public ref float Timer => ref NPC.ai[0];

		public NeuronChomperState State
		{
			get => (NeuronChomperState)NPC.ai[1];
			set => NPC.ai[1] = (float)value;
		}

		public override string Texture => AssetDirectory.CrimsonNPCs + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("big chungus");

			NPCID.Sets.TrailCacheLength[Type] = 10;
			NPCID.Sets.TrailingMode[Type] = 1;
		}

		public override void SetDefaults()
		{
			NPC.width = 72;
			NPC.height = 72;
			NPC.lifeMax = 200;
			NPC.damage = 30;
			NPC.aiStyle = -1;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.knockBackResist = 0.1f;
			NPC.defense = 8;
		}

		public override bool CanHitPlayer(Player target, ref int cooldownSlot)
		{
			return State == NeuronChomperState.Dashing;
		}

		public override void AI()
		{
			if (homePos == default)
				homePos = NPC.Center;

			Timer++;

			Player target = NPC.target != -1 ? Main.player[NPC.target] : null;

			NPC.rotation = NPC.Center.DirectionFrom(homePos).ToRotation() - 1.57f;

			switch (State)
			{
				case NeuronChomperState.Idle:

					NPC.TargetClosest();

					if (target != null && target.active && !target.dead && Vector2.Distance(homePos, target.Center) <= 900)
					{
						State = NeuronChomperState.Charging;
						Timer = 0;
					}

					break;

				case NeuronChomperState.Charging:

					if (target is null || !target.active || target.dead || Vector2.Distance(homePos, target.Center) > 900)
					{
						State = NeuronChomperState.Idle;
						Timer = 0;
					}

					// Spawn charge up dusts
					if (Timer > 60 && Timer < 140)
					{
						float rot = Main.rand.NextFloat(6.28f);
						float opacity = (Timer - 60) / 80f;
						Dust.NewDustPerfect(NPC.Center + Vector2.UnitX.RotatedBy(rot) * 60 * opacity, ModContent.DustType<Dusts.PixelatedImpactLineDust>(), Vector2.UnitX.RotatedBy(rot) * -Main.rand.NextFloat(3) * opacity, 0, new Color(255, 80, 80, 0) * opacity, Main.rand.NextFloat(0.1f, 0.2f) * opacity);
					}

					// Float into position
					if (Timer < 120)
					{
						Vector2 targetPos = homePos + homePos.DirectionTo(target.Center) * -(400 * Timer / 120f);
						NPC.velocity = (targetPos - NPC.Center) * 0.05f * (Timer > 100 ? 1f - (Timer - 100) / 20f : 1f);
					}

					if (Timer == 150)
					{
						NPC.velocity = NPC.Center.DirectionTo(target.Center) * -12;
						targetPos = target.Center;

						SoundEngine.PlaySound(SoundID.Roar.WithPitchOffset(-0.5f), NPC.Center);

						State = NeuronChomperState.Dashing;
						Timer = 0;
					}

					break;

				case NeuronChomperState.Dashing:

					if (Timer < 30)
					{
						NPC.velocity += NPC.Center.DirectionTo(targetPos) * 0.09f * Timer;
					}

					if (Timer < 70)
					{
						NPC.velocity *= 0.99f;
						Dust.NewDust(NPC.position, NPC.width, NPC.height, ModContent.DustType<Dusts.GraymatterDust>(), 0, 0, 0, default, MathF.Sin(Timer / 70f * 3.14f));
					}

					if (Timer > 50 && Timer < 90)
						NPC.velocity *= 0.95f;

					if (Timer == 90)
					{
						NPC.velocity *= 0;
						State = NeuronChomperState.Idle;
						Timer = 0;
					}

					break;
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D texture = Assets.NPCs.Crimson.NeuronChomper.Value;
			Texture2D chain = Assets.NPCs.Crimson.NeuronChomperChain.Value;

			var dist = Vector2.Distance(NPC.Center, homePos);
			for (int k = 0; k < dist; k += 26)
			{
				Vector2 pos = Vector2.Lerp(NPC.Center, homePos, k / dist) - screenPos;
				pos.Y += MathF.Sin(k / dist * 3.14f) * 32;

				Vector2 next = Vector2.Lerp(NPC.Center, homePos, (k + 1) / dist) - screenPos;
				next.Y += MathF.Sin((k + 1) / dist * 3.14f) * 32;

				float opacity = 1f;

				if (k + 26 > dist)
					opacity *= (dist % 26) / 26f;

				spriteBatch.Draw(chain, pos, null, Color.Lerp(Color.LightYellow, Color.LightPink, k / dist) * opacity, pos.DirectionTo(next).ToRotation() + 1.57f, chain.Size() / 2f, 1f, 0, 0);
			}

			if (State == NeuronChomperState.Dashing)
			{
				for (int k = 0; k < NPC.oldPos.Length; k++)
				{
					float progress = (float)((NPC.oldPos.Length - k) / (float)NPC.oldPos.Length);
					Vector2 drawPos = NPC.oldPos[k] + NPC.Size / 2f - screenPos + new Vector2(0f, NPC.gfxOffY);
					Color color = drawColor * Eases.EaseQuarticOut(progress);

					color *= 1f - Timer / 90f;

					Main.EntitySpriteDraw(texture, drawPos, null, color, NPC.rotation, NPC.Size / 2f, NPC.scale * Eases.EaseQuadOut(progress), SpriteEffects.None, 0);
				}
			}

			spriteBatch.Draw(texture, NPC.Center - Main.screenPosition, null, drawColor, NPC.rotation, texture.Size() / 2f, NPC.scale, 0, 0);

			if (State == NeuronChomperState.Charging)
			{
				float opacity = Timer < 140 ? (Timer - 60) / 80f : 1f - (Timer - 140) / 10f;

				spriteBatch.Draw(texture, NPC.Center - Main.screenPosition, null, new Color(255, 100, 100, 0) * opacity, NPC.rotation, texture.Size() / 2f, NPC.scale, 0, 0);

				if (Timer > 90 && Timer <= 150)
				{
					ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderTiles", () =>
					{
						Texture2D tex = Assets.Misc.GlowRing.Value;

						Color color = new Color(255, 100, 100) * ((Timer - 90) / 60f);
						color.A = 0;

						if (Timer > 140)
							color *= 1f - (Timer - 140) / 10f;

						float scale = 50 + (1f - (Timer - 90) / 20f) * 60;
						spriteBatch.Draw(tex, NPC.Center - screenPos, null, color, 0, tex.Size() / 2f, scale / tex.Width, 0, 0);

						scale = 50 + (1f - (Timer - 100) / 20f) * 60;
						spriteBatch.Draw(tex, NPC.Center - screenPos, null, color, 0, tex.Size() / 2f, scale / tex.Width, 0, 0);

						scale = 50 + (1f - (Timer - 110) / 20f) * 60;
						spriteBatch.Draw(tex, NPC.Center - screenPos, null, color, 0, tex.Size() / 2f, scale / tex.Width, 0, 0);
					});
				}
			}

			return false;
		}
	}
}
