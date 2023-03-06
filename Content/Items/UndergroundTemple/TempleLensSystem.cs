using StarlightRiver.Core.Systems.ScreenTargetSystem;
using System.Linq;
using Terraria.Graphics.Effects;

namespace StarlightRiver.Content.Items.UndergroundTemple
{
	public class TempleLensSystem : IOrderedLoadable
	{
		public static ScreenTarget NPCTarget = new(DrawNPCTarget, () => Active, 1);
		public static ScreenTarget LensTarget = new(DrawLensTarget, () => Active, 1);

		public float Priority => 1.1f;

		public static bool Active => Main.npc.Any(n => n.active && n.HasBuff(ModContent.BuffType<Buffs.Illuminant>())) || Main.npc.Any(n => n.active && n.HasBuff(ModContent.BuffType<Exposed>()));

		public void Load()
		{
			if (Main.dedServ)
				return;

			On.Terraria.Main.DrawNPCs += DrawLens;
		}

		public void Unload()
		{
			if (Main.dedServ)
				return;

			On.Terraria.Main.DrawNPCs -= DrawLens;
		}

		private static void DrawNPCTarget(SpriteBatch spriteBatch)
		{
			Main.graphics.GraphicsDevice.Clear(Color.Transparent);

			spriteBatch.End();
			spriteBatch.Begin(default, default, default, default, default, null, Main.GameViewMatrix.ZoomMatrix);

			for (int i = 0; i < Main.npc.Length; i++)
			{
				NPC NPC = Main.npc[i];

				if (NPC.active)
				{
					if (NPC.ModNPC != null)
					{
						ModNPC ModNPC = NPC.ModNPC;

						if (ModNPC.PreDraw(spriteBatch, Main.screenPosition, NPC.GetAlpha(Color.White)))
							Main.instance.DrawNPC(i, false);

						ModNPC.PostDraw(spriteBatch, Main.screenPosition, NPC.GetAlpha(Color.White));
					}
					else
					{
						Main.instance.DrawNPC(i, false);
					}
				}
			}
		}

		private static void DrawLensTarget(SpriteBatch spriteBatch)
		{
			Main.graphics.GraphicsDevice.Clear(Color.Transparent);

			spriteBatch.End();
			spriteBatch.Begin(default, default, default, default, default, null, Main.GameViewMatrix.ZoomMatrix);

			Texture2D bloom = ModContent.Request<Texture2D>(AssetDirectory.Keys + "GlowAlpha").Value;
			Color gold = Color.Orange;
			gold.A = 0;

			for (int i = 0; i < Main.npc.Length; i++)
			{
				NPC NPC = Main.npc[i];

				if (NPC.active && NPC.HasBuff(ModContent.BuffType<Buffs.Illuminant>()))
					spriteBatch.Draw(bloom, NPC.Center - Main.screenPosition, null, gold, 0, bloom.Size() / 2, 5, SpriteEffects.None, 0f);

				if (NPC.active && NPC.HasBuff(ModContent.BuffType<Exposed>()))
					spriteBatch.Draw(bloom, NPC.Center - Main.screenPosition, null, gold, 0, bloom.Size() / 2, 2, SpriteEffects.None, 0f);
			}
		}

		private void DrawLens(On.Terraria.Main.orig_DrawNPCs orig, Main self, bool behindTiles)
		{
			orig(self, behindTiles);
			if (!behindTiles && Active)
			{
				GraphicsDevice gD = Main.graphics.GraphicsDevice;
				SpriteBatch spriteBatch = Main.spriteBatch;

				if (Main.dedServ || spriteBatch == null || NPCTarget == null || gD == null)
					return;

				Effect effect = Filters.Scene["TempleLens"].GetShader().Shader;
				effect.Parameters["LensTarget"].SetValue(LensTarget.RenderTarget);

				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, effect);

				spriteBatch.Draw(NPCTarget.RenderTarget, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);

				spriteBatch.End();
				spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
			}
		}
	}
}
