using StarlightRiver.Content.NPCs.BaseTypes;
using StarlightRiver.Core.Systems.LightingSystem;
using Terraria.ID;

namespace StarlightRiver.Content.Bosses.BrainRedux
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
			Vector3 lightColor = new Vector3(0.4f, 0.1f, 0.12f) * ThisThinker.ArenaOpacity;

			lightColor += new Vector3(0.8f, 0.4f, 0.4f) * glow * 0.1f;

			Lighting.AddLight(NPC.Center, lightColor);
			Lighting.AddLight(NPC.Center + Vector2.UnitX * 80, lightColor);
			Lighting.AddLight(NPC.Center - Vector2.UnitX * 80, lightColor);
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			float dist = Vector2.Distance(NPC.Center, targetPos);

			Texture2D tex = Assets.Bosses.BrainRedux.BrainPlatform.Value;
			LightingBufferRenderer.DrawWithLighting(NPC.position - Main.screenPosition, tex, tex.Bounds, Color.White * (ThisThinker?.ArenaOpacity ?? 1f), Vector2.One);

			Texture2D glowTex = Assets.Bosses.BrainRedux.BrainPlatformGlow.Value;
			Texture2D glowTex2 = Assets.Keys.GlowAlpha.Value;
			Color glowColor = new Color(255, 100, 100, 0) * glow * 0.15f;

			Rectangle target = NPC.Hitbox;
			target.Offset((-Main.screenPosition).ToPoint());
			target.Height = tex.Height;

			spriteBatch.Draw(glowTex2, target, null, glowColor, 0, default, 0, 0);

			spriteBatch.Draw(glowTex, targetPos - NPC.Size / 2f - Main.screenPosition, glowColor);
			target = new Rectangle((int)(targetPos.X - NPC.width / 2 - Main.screenPosition.X), (int)(targetPos.Y - NPC.height / 2 - Main.screenPosition.Y), tex.Width, tex.Height);
			spriteBatch.Draw(glowTex2, target, null, glowColor, 0, default, 0, 0);

			return false;
		}
	}
}