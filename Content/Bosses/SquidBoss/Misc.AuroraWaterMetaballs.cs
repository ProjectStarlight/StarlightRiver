using StarlightRiver.Core;
using StarlightRiver.Core.Loaders;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Items.Vitric;
using StarlightRiver.Helpers;
using Terraria;
using Terraria.ID;
using Terraria.Enums;
using Terraria.ModLoader;
using System;
using System.Linq;
using System.Collections.Generic;
using Terraria.Graphics.Effects;
using Terraria.DataStructures;
using Terraria.GameContent;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using StarlightRiver.Core.Systems.MetaballSystem;
using StarlightRiver.Content.CustomHooks;

namespace StarlightRiver.Content.Bosses.SquidBoss
{
	internal class AuroraWaterMetaballs : MetaballActor
	{
		public override bool Active => Main.LocalPlayer.InModBiome(ModContent.GetInstance<Biomes.PermafrostTempleBiome>());

		public override Color outlineColor => new Color(255, 254, 255);

		public override void DrawShapes(SpriteBatch spriteBatch)
		{
			var tex = ModContent.Request<Texture2D>(AssetDirectory.MiscItem + "MagmaGunProj").Value;

			for (int k = 0; k < Main.maxNPCs; k++)
			{
				NPC NPC = Main.npc[k];
				if (NPC.active && NPC.ModNPC is Bosses.SquidBoss.ArenaActor)
					(NPC.ModNPC as Bosses.SquidBoss.ArenaActor).DrawWater(Main.spriteBatch);
			}

			Effect borderNoise = Filters.Scene["BorderNoise"].GetShader().Shader;

			if (borderNoise is null)
				return;

			borderNoise.Parameters["offset"].SetValue((float)Main.time / 100f);

			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);
			//borderNoise.CurrentTechnique.Passes[0].Apply();

			foreach (Dust dust in Main.dust)
			{
				if (dust.active && dust.type == ModContent.DustType<AuroraWater>())
				{
					borderNoise.Parameters["offset"].SetValue((float)Main.time / 1000f + dust.rotation);
					spriteBatch.Draw(tex, (dust.position - Main.screenPosition) / 2, null, new Color(0.4f, 1, 1), 0f, Vector2.One * 256f, dust.scale * 0.05f, SpriteEffects.None, 0);
				}
			}

			foreach (Projectile proj in Main.projectile.Where(n => n.active && n.type == ModContent.ProjectileType<AuroraWaterSplash>()))
			{
				var tex2 = ModContent.Request<Texture2D>(AssetDirectory.SquidBoss + "AuroraWaterSplash").Value;
				var frame = new Rectangle(0, (int)(6 - proj.timeLeft / 40f * 6) * 106, 72, 106);

				spriteBatch.Draw(tex2, (proj.Center - Main.screenPosition) / 2f, frame, new Color(0.4f, 1, 1), 0, new Vector2(36, 53), 0.5f, 0, 0);
			}

			spriteBatch.End();
			spriteBatch.Begin();
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Texture2D target)
		{
			var effect = Filters.Scene["WavesDistort"].GetShader().Shader;

			if (effect is null)
				return true;

			effect.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * 0.02f);
			effect.Parameters["power"].SetValue(0.005f);
			effect.Parameters["offset"].SetValue(new Vector2(Main.screenPosition.X / Main.screenWidth * 0.5f, 0));
			effect.Parameters["speed"].SetValue(50f);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, default, SamplerState.PointClamp, default, default, effect, Main.GameViewMatrix.ZoomMatrix);

			Main.spriteBatch.Draw(target, Vector2.Zero, null, Color.Red * 0.4f, 0, Vector2.Zero, 1, 0, 0);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, default, SamplerState.PointClamp, default, default, default, Main.GameViewMatrix.ZoomMatrix);

			return false;
		}

		public override bool PostDraw(SpriteBatch spriteBatch, Texture2D target)
		{
			var effect = Filters.Scene["Waves"].GetShader().Shader;

			if (effect is null)
				return true;

			effect.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * 0.02f);
			effect.Parameters["power"].SetValue(0.01f);
			effect.Parameters["offset"].SetValue(new Vector2(Main.screenPosition.X / Main.screenWidth * -0.5f, Main.screenPosition.Y / Main.screenHeight * -0.5f));
			effect.Parameters["sampleTexture"].SetValue(AuroraWaterSystem.auroraBackTarget);
			effect.Parameters["speed"].SetValue(50f);
			effect.Parameters["lightTexture"].SetValue(StarlightRiver.LightingBufferInstance.ScreenLightingTexture);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, default, SamplerState.PointClamp, default, default, effect, Main.GameViewMatrix.ZoomMatrix);

			Main.spriteBatch.Draw(target, Vector2.Zero, null, Color.Red * 0.4f, 0, Vector2.Zero, 2, 0, 0);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, default, SamplerState.PointClamp, default, default, default, Main.GameViewMatrix.ZoomMatrix);

			return false;
		}
	}
}
