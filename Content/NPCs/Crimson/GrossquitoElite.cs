using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Items.Crimson;
using StarlightRiver.Content.NPCs.Misc;
using StarlightRiver.Core.Systems.InstancedBuffSystem;
using StarlightRiver.Core.Systems.PixelationSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using static StarlightRiver.Content.Items.Crimson.Graymatter;

namespace StarlightRiver.Content.NPCs.Crimson
{
	internal class GrossquitoElite : Grossquito
	{

		public override string Texture => AssetDirectory.CrimsonNPCs + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Thoughtful Grossquito");
			NPCID.Sets.TrailCacheLength[NPC.type] = 5;
			NPCID.Sets.TrailingMode[NPC.type] = 3;
		}

		public override void SetDefaults()
		{
			NPC.width = 60;
			NPC.height = 54;
			NPC.knockBackResist = 1.8f;
			NPC.lifeMax = 94;
			NPC.noGravity = true;
			NPC.noTileCollide = false;
			NPC.damage = 35;
			NPC.aiStyle = -1;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;
			speed = 5;
			tex = Assets.NPCs.Crimson.GrossquitoElite.Value;
		}

		public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone)
		{
			if (player.HasBuff(ModContent.BuffType<CrimsonHallucination>()))
			{
				NPC.Kill();
			}
		}

		public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
		{
			Player player = Main.player[projectile.owner];
			if (player.HasBuff(ModContent.BuffType<CrimsonHallucination>()))
			{
				NPC.Kill();
			}
		}

		public override void OnKill()
		{
			if (State == GrossquitoState.Aggroed)
			{
				foreach (Player player in Main.ActivePlayers)
				{
					if (CollisionHelper.CheckCircularCollision(NPC.Center, 125, player.Hitbox))
					{
						int explosionDamage = 100;
						player.Hurt(PlayerDeathReason.ByNPC(NPC.whoAmI), explosionDamage, 0, false, false, false, 0);

						if (!player.noKnockback)
							player.velocity += NPC.Center.DirectionTo(player.Center) * 5;
					}
				}

				for (int k = 0; k < 40; k++)
				{
					Dust.NewDustPerfect(NPC.Center, DustID.Blood, Main.rand.NextVector2Circular(10, 10), 0, default, Main.rand.NextFloat(3));
					Dust.NewDustPerfect(NPC.Center, ModContent.DustType<Dusts.PixelatedImpactLineDust>(), Main.rand.NextVector2Circular(20, 20), 0, new Color(255, 30, Main.rand.Next(90), 0), Main.rand.NextFloat(0.2f));
				}

				if (Main.rand.NextBool())
				{
					SoundHelper.PlayPitched("Impacts/GoreHeavy", 0.15f, 0.5f, NPC.Center);

					SoundHelper.PlayPitched("NPC/Crimson/GrossquitoBoom1", 1f, 0.5f, NPC.Center);
				}
				else
				{
					SoundHelper.PlayPitched("Impacts/GoreHeavy", 0.15f, 0.5f, NPC.Center);

					SoundHelper.PlayPitched("NPC/Crimson/GrossquitoBoom2", 1f, 0.5f, NPC.Center);
				}
			}
			else
			{
				SoundEngine.PlaySound(SoundID.NPCDeath1, NPC.Center);

				for (int k = 0; k < 3; k++)
				{
					ItemHelper.NewItemPerfect(NPC.Center, Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(3), ItemID.Heart);
					ItemHelper.NewItemPerfect(NPC.Center, Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(3), ModContent.ItemType<Graymatter>());
				}
			}

			for (int k = 0; k < 20; k++)
			{
				Dust.NewDustPerfect(NPC.Center, ModContent.DustType<Dusts.BloodMetaballDust>(), Vector2.UnitY.RotatedByRandom(0.5f) * -Main.rand.NextFloat(6), 0, Color.White, Main.rand.NextFloat(0.2f, 0.4f));
				Dust.NewDustPerfect(NPC.Center, ModContent.DustType<Dusts.BloodMetaballDustLight>(), Vector2.UnitY.RotatedByRandom(0.8f) * -Main.rand.NextFloat(9), 0, Color.White, Main.rand.NextFloat(0.1f, 0.3f));
			}
		}

		public override void FindFrame(int frameHeight)
		{
			NPC.frame = new(60 * (int)(Timer / 6f % 2), 0, 60, 54);
		}

		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			if (State == GrossquitoState.Aggroed)
			{
				ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderTiles", () =>
				{
					if (FuseTimer <= 60)
					{
						Texture2D tex = Assets.Misc.GlowRing.Value;
						Texture2D tex2 = Assets.StarTexture.Value;

						Color color = Color.Lerp(new Color(255, 90, 90), new Color(50, 0, 0), FuseTimer / 60f) * Math.Min(1f, FuseTimer / 15f);
						color.A = 0;

						if (FuseTimer > 50)
							color *= 1f - (FuseTimer - 50) / 10f;

						if (FuseTimer > 30)
							color *= 1f - (FuseTimer - 95) / 10f;

						float scale = 30 + Eases.EaseQuarticOut(FuseTimer / 60f) * 220;
						float scale2 = 30 + Eases.SwoopEase(FuseTimer / 60f) * 260;
						float scale3 = 30 + Eases.SwoopEase((FuseTimer - 20) / 40f) * 160;

						spriteBatch.Draw(tex, NPC.Center - screenPos, null, color * 0.25f, 0, tex.Size() / 2f, scale / tex.Width, 0, 0);
						spriteBatch.Draw(tex2, NPC.Center - screenPos, null, color, 0, tex2.Size() / 2f, scale2 / tex2.Width, 0, 0);
						spriteBatch.Draw(tex2, NPC.Center - screenPos, null, color, 1.57f / 2f, tex2.Size() / 2f, scale3 / tex2.Width, 0, 0);
					}
				});
			}
		}
	}
}