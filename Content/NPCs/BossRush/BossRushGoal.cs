using StarlightRiver.Content.Items.Misc;
using StarlightRiver.Core.Systems.BossRushSystem;

namespace StarlightRiver.Content.NPCs.BossRush
{
	internal class BossRushGoal : ModNPC
	{
		public override string Texture => "StarlightRiver/Assets/Items/Misc/JadeStopwatch";

		public override void SetDefaults()
		{
			NPC.lifeMax = 1000;
			NPC.width = 42;
			NPC.height = 42;
			NPC.aiStyle = -1;
			NPC.noGravity = true;
			NPC.knockBackResist = 0f;
			NPC.friendly = true;
		}

		public override void AI()
		{
			float rot = Main.rand.NextFloat(6.28f);
			var d = Dust.NewDustPerfect(NPC.Center + Vector2.One.RotatedBy(rot) * 36, ModContent.DustType<Dusts.Glow>(), Vector2.One.RotatedBy(rot + 1.5f) * 1.5f, 0, new Color(50, 255, 150) * 0.5f, 0.25f);

			// Boss rush is SP only so this is OK
			if (Main.LocalPlayer.Hitbox.Intersects(NPC.Hitbox))
			{
				var item = new Item();
				item.SetDefaults(ModContent.ItemType<JadeStopwatch>());

				var mi = item.ModItem as JadeStopwatch;

				mi.feat =
					Main.GameMode == 0 ? "Boss rush" :
					Main.GameMode == 1 ? "Boss blitz" :
					Main.GameMode == 2 ? "Starlight showdown" :
					"Invalid";

				mi.time = Helpers.Helper.TicksToTime(BossRushSystem.scoreTimer);

				Main.LocalPlayer.QuickSpawnClonedItemDirect(null, item);

				NPC.active = false;
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Misc/Clock").Value;

			float alpha = 1;

			var color = new Color(50, 255, 150)
			{
				A = 0
			};
			color *= alpha;

			float speed = 1.5f;

			Vector2 pos = NPC.Center - Main.screenPosition;

			spriteBatch.Draw(tex, pos, null, color * 0.3f * alpha, 0, tex.Size() / 2, 0.6f, 0, 0);

			Texture2D armTex = ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrailOneEnd").Value;

			var target = new Rectangle((int)pos.X, (int)pos.Y, 40, 12);
			spriteBatch.Draw(armTex, target, null, color * 0.5f * alpha, Main.GameUpdateCount * 0.12f * speed, new Vector2(0, armTex.Height / 2), 0, 0);

			var target2 = new Rectangle((int)pos.X, (int)pos.Y, 34, 12);
			spriteBatch.Draw(armTex, target2, null, color * 0.5f * alpha, Main.GameUpdateCount * 0.01f * speed, new Vector2(0, armTex.Height / 2), 0, 0);

			return true;
		}
	}
}