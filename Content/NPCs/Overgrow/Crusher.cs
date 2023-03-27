using StarlightRiver.Core.Systems.CameraSystem;
using System;
using System.Linq;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.NPCs.Overgrow
{
	internal class Crusher : ModNPC
	{
		public Tile Parent;

		public override string Texture => "StarlightRiver/Assets/NPCs/Overgrow/Crusher";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Masher");
		}

		public override void SetDefaults()
		{
			NPC.width = 160;
			NPC.height = 10;
			NPC.immortal = true;
			NPC.dontTakeDamage = true;
			NPC.lifeMax = 1;
			NPC.dontCountMe = true;
			NPC.aiStyle = -1;
			NPC.noGravity = true;
			NPC.knockBackResist = 0;
			NPC.behindTiles = true;
		}

		public override void AI()
		{
			if (NPC.ai[0] < 10)
			{
				NPC.velocity.Y += 1.5f;
				NPC.damage = 120;
			}

			if (NPC.ai[0] > 40 && NPC.ai[0] < 50)
			{
				NPC.velocity.Y = -3;
				NPC.damage = 0;
			}

			if (NPC.ai[0]++ > 80)
			{
				NPC.ai[0] = 0;
				NPC.velocity.Y = 0.01f;
				NPC.ai[1] = 0;
			}

			if (NPC.velocity.Y == 0 && NPC.ai[1] != 1)
			{
				for (float k = 0; k <= 0.3f; k += 0.007f)
				{
					Vector2 vel = new Vector2(1, 0).RotatedBy(-k) * Main.rand.NextFloat(8);
					if (Main.rand.NextBool(2))
						vel = new Vector2(-1, 0).RotatedBy(k) * Main.rand.NextFloat(8);
					Dust.NewDustPerfect(NPC.Center + new Vector2(vel.X * 3, 5), DustID.Stone, vel * 0.7f);
					Dust.NewDustPerfect(NPC.Center + new Vector2(vel.X * 3, 5), DustType<Dusts.Stamina>(), vel);
				}

				Terraria.Audio.SoundEngine.PlaySound(SoundID.Item70 with { PitchVariance = 0.6f }, NPC.Center);

				foreach (Player Player in Main.player.Where(Player => Vector2.Distance(Player.Center, NPC.Center) <= 250))
					CameraSystem.shake = (250 - (int)Vector2.Distance(Player.Center, NPC.Center)) / 12;
				NPC.ai[1] = 1;
			}
		}

		public override void OnHitPlayer(Player target, int damage, bool crit)
		{
			target.AddBuff(BuffType<Buffs.Squash>(), 450);
		}

		public override bool? CanHitNPC(NPC target)
		{
			return true;
		}

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit)
		{
			if (target.type == NPCID.Bunny)
			{
				damage *= 99;
				crit = true;
				for (int k = 0; k < 1000; k++)
					Dust.NewDustPerfect(target.Center, DustID.Blood, Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(20), 0, default, 3);
			}
		}
		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D tex = Request<Texture2D>("StarlightRiver/Assets/NPCs/Overgrow/CrusherGlow").Value;
			Texture2D tex2 = Request<Texture2D>("StarlightRiver/Assets/NPCs/Overgrow/CrusherTile").Value;

			spriteBatch.Draw(tex, NPC.Center - screenPos + new Vector2(0, -24), tex.Bounds, Color.White * 0.8f, 0, tex.Size() / 2, 1.2f + (float)Math.Sin(NPC.ai[0] / 80f * 6.28f) * 0.2f, 0, 0);

			int count = NPC.ai[0] < 10 ? (int)NPC.ai[0] / 3 : NPC.ai[0] > 40 ? (60 - (int)NPC.ai[0]) / 4 : 3;
			for (int k = 1; k <= count; k++)
				spriteBatch.Draw(tex2, NPC.position - screenPos + new Vector2(8, -48 - k * 28), drawColor);
		}
	}
}