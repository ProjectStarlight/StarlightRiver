using StarlightRiver.Content.Biomes;
using StarlightRiver.Content.Buffs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Content.Bosses.BrainRedux
{
	internal class TheThinker : ModNPC
	{
		public static readonly List<TheThinker> toRender = new();

		public override string Texture => AssetDirectory.BrainRedux + Name;

		public override void Load()
		{
			GraymatterBiome.onDrawHallucinationMap += DrawAura;
			GraymatterBiome.onDrawOverHallucinationMap += DrawMe;
		}

		public override void SetDefaults()
		{
			NPC.noGravity = true;
			NPC.aiStyle = -1;
			NPC.width = 128;
			NPC.height = 128;
			NPC.immortal = true;
			NPC.dontTakeDamage = true;
			NPC.damage = 10;
			NPC.lifeMax = 1000;

			toRender.Add(this);
		}

		public override void AI()
		{
			GraymatterBiome.forceGrayMatter = true;

			Lighting.AddLight(NPC.Center, new Vector3(1f, 0.2f, 0.2f));

			for(int k = 0; k < Main.maxPlayers; k++)
			{
				var player = Main.player[k];

				if (Vector2.DistanceSquared(player.Center, NPC.Center) <= Math.Pow(256, 2))
					player.AddBuff(ModContent.BuffType<CrimsonHallucination>(), 10);
			}
		}

		public override bool CanHitPlayer(Player target, ref int cooldownSlot)
		{
			return Helpers.Helper.CheckCircularCollision(NPC.Center, 64, target.Hitbox);
		}

		private void DrawAura(SpriteBatch sb)
		{
			Texture2D glow = ModContent.Request<Texture2D>("StarlightRiver/Assets/Keys/GlowAlpha").Value;
			var color = Color.White;
			color.A = 0;

			foreach (TheThinker thinker in toRender)
			{
				sb.Draw(glow, thinker.NPC.Center - Main.screenPosition, null, color, 0, glow.Size() / 2f, 6f, 0, 0);
				sb.Draw(glow, thinker.NPC.Center - Main.screenPosition, null, color, 0, glow.Size() / 2f, 6f, 0, 0);
				sb.Draw(glow, thinker.NPC.Center - Main.screenPosition, null, color, 0, glow.Size() / 2f, 6f, 0, 0);
			}

			toRender.RemoveAll(n => n is null || !n.NPC.active);
		}

		private void DrawMe(SpriteBatch sb)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;

			foreach (TheThinker thinker in toRender)
			{
				sb.Draw(tex, thinker.NPC.Center - Main.screenPosition, null, Color.White, 0, tex.Size() / 2f, 1, 0, 0);
			}
		}
	}
}
