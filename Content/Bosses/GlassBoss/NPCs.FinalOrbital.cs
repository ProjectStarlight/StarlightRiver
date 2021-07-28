using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Core.Loaders;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using StarlightRiver.NPCs;
using StarlightRiver.Content.Items.Vitric;
using StarlightRiver.Content.GUI;
using StarlightRiver.Helpers;
using StarlightRiver.Content.Foregrounds;
using static StarlightRiver.Helpers.Helper;

namespace StarlightRiver.Content.Bosses.GlassBoss
{
	class FinalOrbital : ModNPC
	{
		public NPC parent;

		public ref float Timer => ref npc.ai[0];
		public ref float Phase => ref npc.ai[1];
		public ref float RotationPosition => ref npc.ai[2];

		public bool CanBeHit => Timer > 30 && Phase < 1;

		public override string Texture => AssetDirectory.GlassBoss + Name;

		public override bool? CanBeHitByItem(Player player, Item item) => false;

		public override bool? CanBeHitByProjectile(Projectile projectile) => false;

		public override void SetDefaults()
		{
			npc.dontTakeDamage = true;
			npc.lifeMax = 101;
			npc.life = 100;
			npc.aiStyle = -1;
			npc.width = 64;
			npc.height = 64;
			npc.knockBackResist = 0;
		}

		public override void AI()
		{
			Timer++;

			npc.dontTakeDamage = !CanBeHit;

			if(Phase == 0)
			{
				if (Timer == 1)
				{
					npc.life = 100;

					for (int k = 0; k < 20; k++)
					{
						var velocity = Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(20);
						var scale = Main.rand.NextFloat(0.8f, 2.0f);

						var d = Dust.NewDustPerfect(parent.Center, DustType<Dusts.GlassAttracted>(), velocity, Scale: scale);
						d.customData = npc;

						var d2 = Dust.NewDustPerfect(parent.Center, DustType<Dusts.GlassAttractedGlow>(), velocity, Scale: scale);
						d2.customData = npc;
						d2.frame = d.frame;
					}
				}

				if (Timer <= 60)
				{
					RotationPosition += 0.05f * (Timer / 60f);
					npc.Center = parent.Center + Vector2.One.RotatedBy(RotationPosition) * Timer * 2;
					npc.scale = Timer / 60f;

					npc.rotation = RotationPosition - (1 - Timer / 60f) + (float)Math.PI / 2;
				}

				else
				{
					RotationPosition += 0.025f;
					npc.Center = parent.Center + Vector2.One.RotatedBy(RotationPosition) * 120;

					npc.rotation = RotationPosition + (float)Math.PI / 2;
				}
			}

			if (Phase == 1)
			{
				float progress = (Timer / 60f);

				if (progress > 1)
					progress = 1;

				float rotationAdd = progress * 3.14f + (float)Math.Sin(Timer * (1.5f - progress)) * (1 - progress);

				RotationPosition += 0.025f;

				npc.Center = parent.Center + Vector2.One.RotatedBy(RotationPosition) * 120;
				npc.rotation = RotationPosition + (float)Math.PI / 2 + rotationAdd;
			}

			if (Phase == 2)
			{
				float distance = 0;

				if (Timer < 10)
					distance = 120 + BezierEase(Timer/ 10f) * 40f;

				else if (Timer >= 10)
					distance = 160 - BezierEase((Timer - 10) / 60f) * 320;

				RotationPosition += 0.025f;

				npc.Center = parent.Center + Vector2.One.RotatedBy(RotationPosition) * distance;

				if (Timer > 40)
				{
					parent.dontTakeDamage = false;
					parent.immortal = false;
					parent.friendly = false;

					for (int k = 0; k < 30; k++)
					{
						var d = Dust.NewDustPerfect(npc.Center, DustType<Dusts.GlassAttracted>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(20), 0, default, Main.rand.NextFloat(2));
						d.customData = parent;
					}

					Main.PlaySound(SoundID.Shatter, npc.Center);
					npc.active = false;
				}

				return;
			}
		}

		public override bool CheckDead()
		{
			if (Phase < 1)
				Phase = 1;

			Timer = 0;

			npc.life = 1;

			return false;
		}

		public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
		{
			if (npc.life == 1)
				return false;

			position.Y += 10;

			var spriteBatch = Main.spriteBatch;

			var tex = GetTexture(AssetDirectory.GUI + "ShieldBar0");
			var texOver = GetTexture(AssetDirectory.GUI + "ShieldBar1");
			var progress = 1 - (float)npc.life / npc.lifeMax;
			var alpha = Math.Min(1, (Timer - 60) / 60f);

			Rectangle target = new Rectangle((int)(position.X - Main.screenPosition.X) + 2, (int)(position.Y - Main.screenPosition.Y), (int)(progress * tex.Width - 4), tex.Height);
			Rectangle source = new Rectangle(2, 0, (int)(progress * tex.Width - 4), tex.Height);

			var color = progress > 0.5f ?
				Color.Lerp(Color.Yellow, Color.White, progress * 2 - 1) :
				Color.Lerp(Color.Red, Color.Yellow, progress * 2) * alpha;

			spriteBatch.Draw(tex, position - Main.screenPosition, null, color, 0, tex.Size() / 2, 1, 0, 0);
			spriteBatch.Draw(texOver, target, source, color, 0, tex.Size() / 2, 0, 0);

			return false;
		}

		public override void PostDraw(SpriteBatch spriteBatch, Color drawColor)
		{
			var tex = GetTexture(Texture + "Glow");
			spriteBatch.Draw(tex, npc.Center - Main.screenPosition, null, VitricSummonOrb.MoltenGlow(Timer * 2), npc.rotation, tex.Size() / 2, npc.scale, 0, 0);

			spriteBatch.Draw(tex, npc.Center - Main.screenPosition, null, VitricSummonOrb.MoltenGlow((npc.life / (float)npc.lifeMax) * 120f), npc.rotation, tex.Size() / 2, npc.scale, 0, 0);
		}
	}
}
