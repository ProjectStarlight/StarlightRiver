using StarlightRiver.Content.NPCs.BaseTypes;
using StarlightRiver.Core.Systems.LightingSystem;
using Terraria.ID;

namespace StarlightRiver.Content.Bosses.TheThinkerBoss
{
	internal class BrainPlatform : MovingPlatform
	{
		public NPC thinker;
		public Vector2 targetPos;
		public float glow = 0;

		public TheThinker ThisThinker => thinker?.ModNPC as TheThinker;

		public override string Texture => "StarlightRiver/Assets/MagicPixel";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("");
			NPCID.Sets.TrailCacheLength[Type] = 30;
			NPCID.Sets.TrailingMode[Type] = 1;
		}

		public override void SafeSetDefaults()
		{
			NPC.width = 260;
			NPC.height = 12;
			NPC.noTileCollide = true;
		}

		public override void SafeAI()
		{
			Vector3 lightColor = new Vector3(0.45f, 0.3f, 0.3f) * (ThisThinker?.ArenaOpacity ?? 1);

			Lighting.AddLight(NPC.Center, lightColor);
			Lighting.AddLight(NPC.Center + Vector2.UnitX * 80, lightColor * 0.5f);
			Lighting.AddLight(NPC.Center - Vector2.UnitX * 80, lightColor * 0.5f);

			if (glow > 0.9f && Main.rand.NextBool(6))
			{
				Dust.NewDustPerfect(NPC.Center + Vector2.UnitX * Main.rand.NextFloat(-130, 130), ModContent.DustType<Dusts.PixelatedImpactLineDust>(), Vector2.Normalize(NPC.Center - targetPos) * Main.rand.NextFloat(6), 0, new Color(0.25f, 0.1f, 0.1f, 0f), Main.rand.NextFloat(0.05f, 0.1f));
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			float dist = Vector2.Distance(NPC.Center, targetPos);

			Texture2D tex = Assets.Bosses.TheThinkerBoss.BrainPlatform.Value;
			LightingBufferRenderer.DrawWithLighting(tex, NPC.position - screenPos, tex.Bounds, Color.White * (ThisThinker?.ArenaOpacity ?? 1));

			Texture2D glowTex = Assets.Masks.GlowAlpha.Value;
			Color glowColor = new Color(255, 100, 100, 0) * glow * 0.15f;

			Rectangle target = NPC.Hitbox;
			target.Offset((-screenPos).ToPoint());
			target.Height = tex.Height;

			spriteBatch.Draw(glowTex, target, null, glowColor, 0, default, 0, 0);

			target = new Rectangle((int)(targetPos.X - NPC.width / 2 - screenPos.X), (int)(targetPos.Y - NPC.height / 2 - screenPos.Y), tex.Width, tex.Height);
			spriteBatch.Draw(glowTex, target, null, glowColor, 0, default, 0, 0);

			return false;
		}
	}
}