using System;
using Terraria.Audio;
using Terraria.ID;

namespace StarlightRiver.Content.Bosses.BrainRedux
{
	internal class HorrifyingVisage : ModNPC
	{
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
			NPC.width = 64;
			NPC.height = 64;
			NPC.noGravity = true;
			NPC.aiStyle = -1;
		}

		public override void AI()
		{
			Timer++;

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

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D tex = Terraria.GameContent.TextureAssets.Npc[NPCID.BrainofCthulhu].Value;

			var frame = new Rectangle(0, 182 * 4 + 182 * (int)(Timer / 10f % 4), 200, 182);
			float opacity = Math.Min(1f, Timer / 30f);

			if (Decay > 30)
				opacity *= 1 - (Decay - 30) / 30f;

			if (opacity >= 1)
			{
				Main.spriteBatch.Draw(tex, NPC.Center - Main.screenPosition, frame, drawColor * opacity, 0, frame.Size() / 2f, 1f, 0, 0);
			}
			else
			{
				for (int k = 0; k < 6; k++)
				{
					float rot = k / 6f * 6.28f + opacity * 3.14f;
					Vector2 offset = Vector2.UnitX.RotatedBy(rot) * (1 - opacity) * 64;
					Main.spriteBatch.Draw(tex, NPC.Center + offset - Main.screenPosition, frame, drawColor * opacity * 0.2f, 0, frame.Size() / 2f, 1f, 0, 0);
				}
			}

			return false;
		}
	}
}
