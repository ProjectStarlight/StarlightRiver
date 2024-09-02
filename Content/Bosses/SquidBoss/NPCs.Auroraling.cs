﻿using System;
using System.Linq;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Bosses.SquidBoss
{
	class Auroraling : ModNPC
	{
		public ref float Timer => ref NPC.ai[0];

		public override string Texture => AssetDirectory.SquidBoss + Name;

		public override void SetDefaults()
		{
			NPC.width = 26;
			NPC.height = 30;
			NPC.lifeMax = 8;
			NPC.damage = 25;
			NPC.noGravity = true;
			NPC.aiStyle = -1;
			NPC.knockBackResist = 3f;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;
		}

		public override void OnSpawn(IEntitySource source)
		{
			NPC.velocity += Vector2.UnitY.RotatedByRandom(1) * 20;
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
			{
				Bestiary.SLRSpawnConditions.AuroraSquid,
				new FlavorTextBestiaryInfoElement("Baby aurora squid are born with their light-sacs fully charged from the glow of their mother, and will rely on this energy untill they are old enough to venture to the surface to gather their own.")
			});
		}

		public override void AI()
		{
			Timer++;
			NPC.frame = new Rectangle(26 * ((int)(Timer / 5) % 3), 0, 26, 30);

			NPC.TargetClosest();
			Player Player = Main.player[NPC.target];

			NPC.velocity += Vector2.Normalize(NPC.Center - Player.Center) * -0.175f;

			if (NPC.velocity.LengthSquared() > 6)
				NPC.velocity *= 0.95f;

			if (NPC.ai[0] % 15 == 0)
				NPC.velocity.Y -= 0.5f;

			NPC.rotation = NPC.velocity.X * 0.25f;

			foreach (NPC npc in Main.npc.Where(n => n.active && n.type == Type && Vector2.Distance(n.Center, NPC.Center) < 32))
			{
				npc.velocity += (npc.Center - NPC.Center) * 0.05f;
			}
		}

		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
		{
			target.noKnockback = true;

			for (int i = 0; i < 20; i++)
			{
				Dust.NewDustPerfect(NPC.Center + Main.rand.NextVector2Circular(8, 8), DustType<Dusts.Glow>(), Main.rand.NextVector2Circular(8, 8), 0, new Color(150, 200, 255) * 0.5f);
			}

			Helpers.Helper.PlayPitched("SquidBoss/MagicSplash", 1, 0.2f, NPC.Center);

			NPC.active = false;
		}

		public override void HitEffect(NPC.HitInfo hit)
		{
			if (NPC.life <= 0)
			{
				for (int i = 0; i < 8; i++)
				{
					Dust.NewDustPerfect(NPC.Center + Main.rand.NextVector2Circular(8, 8), DustType<Dusts.Glow>(), Main.rand.NextVector2Circular(5, 5), 0, new Color(150, 200, 255) * 0.5f);
				}
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			if (NPC.IsABestiaryIconDummy)
			{
				Timer++;
				NPC.frame = new Rectangle(26 * ((int)(NPC.ai[0] / 5) % 3), 0, 26, 30);
			}

			Texture2D tex = Assets.Bosses.SquidBoss.AuroralingGlow.Value;
			Texture2D tex2 = Assets.Bosses.SquidBoss.AuroralingGlow2.Value;

			float sin = 1 + (float)Math.Sin(NPC.ai[0] / 10f);
			float cos = 1 + (float)Math.Cos(NPC.ai[0] / 10f);
			var color = new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f);

			spriteBatch.Draw(Request<Texture2D>(Texture).Value, NPC.Center - screenPos, NPC.frame, drawColor * 1.2f, NPC.rotation, NPC.Size / 2, 1, 0, 0);
			spriteBatch.Draw(tex, NPC.Center - screenPos, NPC.frame, color * 0.8f, NPC.rotation, NPC.Size / 2, 1, 0, 0);
			spriteBatch.Draw(tex2, NPC.Center - screenPos, NPC.frame, color, NPC.rotation, NPC.Size / 2, 1, 0, 0);

			Lighting.AddLight(NPC.Center, color.ToVector3() * 0.5f);
			return false;
		}
	}
}