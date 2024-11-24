using StarlightRiver.Content.NPCs.BaseTypes;
using StarlightRiver.Core.Systems.LightingSystem;
using Terraria.ID;

namespace StarlightRiver.Content.Bosses.BrainRedux
{
	internal class BrainPlatform : MovingPlatform
	{
		public Vector2 targetPos;

		public override string Texture => "StarlightRiver/Assets/MagicPixel";

		public override void SetStaticDefaults()
		{
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
			Lighting.AddLight(NPC.Center, new Vector3(0.4f, 0.1f, 0.12f) * DeadBrain.ArenaOpacity);
			Lighting.AddLight(NPC.Center + Vector2.UnitX * 80, new Vector3(0.4f, 0.1f, 0.12f) * DeadBrain.ArenaOpacity);
			Lighting.AddLight(NPC.Center - Vector2.UnitX * 80, new Vector3(0.4f, 0.1f, 0.12f) * DeadBrain.ArenaOpacity);
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			float dist = Vector2.Distance(NPC.Center, targetPos);

			Texture2D tex = Assets.Bosses.BrainRedux.BrainPlatform.Value;
			LightingBufferRenderer.DrawWithLighting(NPC.position - Main.screenPosition, tex, tex.Bounds, Color.White * DeadBrain.ArenaOpacity, Vector2.One);

			return false;
		}
	}
}
