using StarlightRiver.Core.Systems.PixelationSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace StarlightRiver.Content.NPCs.Crimson
{
	enum GrossquitoState : int
	{
		Idle,
		Aggroed
	}

	internal class Grossquito : ModNPC
	{
		public ref float Timer => ref NPC.ai[0];

		public GrossquitoState State
		{
			get => (GrossquitoState)NPC.ai[1];
			set => NPC.ai[1] = (float)value;
		}

		public ref float NextJerk => ref NPC.ai[2];
		public ref float FuseTimer => ref NPC.ai[3];

		public Player Target => NPC.target > -1 ? Main.player[NPC.target] : null;

		public override string Texture => AssetDirectory.CrimsonNPCs + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Grossquito");
		}

		public override void SetDefaults()
		{
			NPC.width = 48;
			NPC.height = 36;
			NPC.knockBackResist = 1.8f;
			NPC.lifeMax = 64;
			NPC.noGravity = true;
			NPC.noTileCollide = false;
			NPC.damage = 10;
			NPC.aiStyle = -1;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath4;
		}

		public override void AI()
		{
			Timer++;

			switch (State)
			{
				case GrossquitoState.Idle:

					if (Timer >= NextJerk)
					{
						NPC.velocity += Main.rand.NextVector2CircularEdge(5, 5);
						NextJerk += Main.rand.Next(10, 40);

						NPC.TargetClosest();

						if (Target != null)
						{
							NPC.velocity += NPC.Center.DirectionTo(Target.Center) * 9;

							NPC.direction = NPC.Center.X > Target.Center.X ? 1 : -1;

							if (Vector2.Distance(NPC.Center, Target.Center) < 120)
								State = GrossquitoState.Aggroed;
						}

						NPC.netUpdate = true;
					}

					NPC.velocity *= 0.92f;

					break;

				case GrossquitoState.Aggroed:

					NPC.velocity *= 0.92f;
					FuseTimer++;

					if (FuseTimer == 120)
					{
						NPC.Kill();
					}

					break;
			}
		}

		public override void OnKill()
		{
			for (int k = 0; k < 40; k++)
			{
				Dust.NewDustPerfect(NPC.Center, DustID.Blood, Main.rand.NextVector2Circular(10, 10));
			}

			for (int k = 0; k < 20; k++)
			{
				Dust.NewDustPerfect(NPC.Center, ModContent.DustType<Dusts.BloodMetaballDust>(), Vector2.UnitY.RotatedByRandom(0.5f) * -Main.rand.NextFloat(6), 0, Color.White, Main.rand.NextFloat(0.2f, 0.4f));
				Dust.NewDustPerfect(NPC.Center, ModContent.DustType<Dusts.BloodMetaballDustLight>(), Vector2.UnitY.RotatedByRandom(0.8f) * -Main.rand.NextFloat(9), 0, Color.White, Main.rand.NextFloat(0.1f, 0.3f));
			}
		}

		public override void FindFrame(int frameHeight)
		{
			NPC.frame = new(48 * (int)(Timer / 6f % 2), 0, 48, 36);
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			var tex = Assets.NPCs.Crimson.Grossquito.Value;
			var eff = NPC.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
			Main.EntitySpriteDraw(tex, NPC.Center - screenPos, NPC.frame, drawColor, NPC.rotation, NPC.frame.Size() / 2f, NPC.scale, eff, 0);
			return false;
		}

		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			if (State == GrossquitoState.Aggroed)
			{
				ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderTiles", () =>
				{
					if (FuseTimer <= 120)
					{
						Texture2D tex = Assets.Masks.GlowWithRing.Value;

						Color color = Color.Lerp(Color.Red, new Color(255, 50, 50), FuseTimer / 120f) * (FuseTimer / 120f);
						color.A = 0;

						if (FuseTimer > 100)
							color *= 1f - (FuseTimer - 100) / 20f;

						float scale = 220 + (1f - FuseTimer / 120f) * 30;

						spriteBatch.Draw(tex, NPC.Center - screenPos, null, color, 0, tex.Size() / 2f, scale / tex.Width, 0, 0);
					}
				});
			}
		}
	}
}
