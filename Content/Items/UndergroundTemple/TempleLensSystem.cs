using Microsoft.Xna.Framework.Graphics.PackedVector;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Content.Items.Breacher;
using System.Linq;
using Terraria.ID;
using Terraria.Graphics.Effects;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.UndergroundTemple
{
	public class TempleLensSystem : IOrderedLoadable
	{
		public static RenderTarget2D NPCTarget;

		public static RenderTarget2D LensTarget;
		public float Priority => 1.1f;

		public void Load()
		{
			if (Main.dedServ)
				return;

			ResizeTarget();

			Main.OnPreDraw += Main_OnPreDraw;
			On.Terraria.Main.DrawNPCs += DrawLens;
		}

		private void DrawLens(On.Terraria.Main.orig_DrawNPCs orig, Main self, bool behindTiles)
		{
			orig(self, behindTiles);
			if (Main.npc.Any(n => n.active && n.HasBuff(ModContent.BuffType<Buffs.Illuminant>())))
			{
				GraphicsDevice gD = Main.graphics.GraphicsDevice;
				SpriteBatch spriteBatch = Main.spriteBatch;

				if (Main.dedServ || spriteBatch == null || NPCTarget == null || gD == null)
					return;

				Effect effect = Filters.Scene["TempleLens"].GetShader().Shader;
				effect.Parameters["LensTarget"].SetValue(LensTarget);

				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, effect);

				spriteBatch.Draw(NPCTarget, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);

				spriteBatch.End();
				spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
			}
		}

		public static void ResizeTarget()
		{
			Main.QueueMainThreadAction(() => NPCTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight));
			Main.QueueMainThreadAction(() => LensTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight));
		}

		public void Unload()
		{
			Main.OnPreDraw -= Main_OnPreDraw;
		}

		private void Main_OnPreDraw(GameTime obj)
		{
			GraphicsDevice gD = Main.graphics.GraphicsDevice;
			SpriteBatch spriteBatch = Main.spriteBatch;

			if (Main.gameMenu || Main.dedServ || spriteBatch is null || NPCTarget is null || gD is null || !Main.npc.Any(n => n.active && n.HasBuff(ModContent.BuffType<Buffs.Illuminant>())))
				return;

			#region npctarget
			RenderTargetBinding[] bindings = gD.GetRenderTargets();
			gD.SetRenderTarget(NPCTarget);
			gD.Clear(Color.Transparent);

			Main.spriteBatch.Begin(default, default, default, default, default, null, Main.GameViewMatrix.ZoomMatrix);

			for (int i = 0; i < Main.npc.Length; i++)
			{
				NPC NPC = Main.npc[i];

				if (NPC.active)
				{
					if (NPC.ModNPC != null)
					{
						if (NPC.ModNPC != null && NPC.ModNPC is ModNPC ModNPC)
						{
							if (ModNPC.PreDraw(spriteBatch, Main.screenPosition, NPC.GetAlpha(Color.White)))
								Main.instance.DrawNPC(i, false);

							ModNPC.PostDraw(spriteBatch, Main.screenPosition, NPC.GetAlpha(Color.White));
						}
					}
					else
					{
						Main.instance.DrawNPC(i, false);
					}
				}
			}

			spriteBatch.End();

			gD.SetRenderTarget(LensTarget);
			gD.Clear(Color.Transparent);

			Main.spriteBatch.Begin(default, BlendState.AlphaBlend, default, default, default, null, Main.GameViewMatrix.ZoomMatrix);

			Texture2D bloom = ModContent.Request<Texture2D>(AssetDirectory.Keys + "GlowAlpha").Value;
			Color gold = Color.Gold;
			gold.A = 0;
			for (int i = 0; i < Main.npc.Length; i++)
			{
				NPC NPC = Main.npc[i];

				if (NPC.active && NPC.HasBuff(ModContent.BuffType<Buffs.Illuminant>()))
				{
					spriteBatch.Draw(bloom, NPC.Center - Main.screenPosition, null, gold, 0, bloom.Size() / 2, 5, SpriteEffects.None, 0f);
				}
			}
			spriteBatch.End();
			gD.SetRenderTargets(bindings);
			#endregion
		}
	}
}
