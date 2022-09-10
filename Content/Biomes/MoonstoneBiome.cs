using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using StarlightRiver.Codex.Entries;
using StarlightRiver.Content.Tiles.Underground;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StarlightRiver.Content.Tiles.Moonstone;
using Terraria;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;

namespace StarlightRiver.Content.Biomes
{
	public class MoonstoneBiome : ModBiome
	{
		public override int Music => MusicLoader.GetMusicSlot("StarlightRiver/Sounds/Music/Moonstone");

		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeMedium;
        public override void Load()
        {
			Filters.Scene["MoonstoneTower"] = new Filter(new ScreenShaderData("FilterMiniTower").UseColor(0.1f, 0.0f, 0.255f).UseOpacity(0.6f), EffectPriority.VeryHigh);
			base.Load();
        }

        public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Moonstone");
		}

        public override bool IsBiomeActive(Player player)
		{
			return ModContent.GetInstance<MoonstoneBiomeSystem>().moonstoneBlockCount >= 150;
		}

        public override void OnLeave(Player player)
        {
			Filters.Scene.Deactivate("MoonstoneTower", player.position);
		}

		public override void OnInBiome(Player player)
		{
			Filters.Scene.Activate("MoonstoneTower", player.position);
		}
	}

	public class MoonstoneBiomeSystem : ModSystem
	{ 
		public static RenderTarget2D target;

		public int moonstoneBlockCount;

		private float opacity = 0;

		public ParticleSystem particleSystem;
		public ParticleSystem particleSystem_Blue;
		public ParticleSystem particleSystem_Purple;


		public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts)
		{
			moonstoneBlockCount = tileCounts[ModContent.TileType<MoonstoneOre>()];
		}

        public override void Load()
        {
			if (Main.dedServ)
				return;

			ResizeTarget();

			particleSystem = new ParticleSystem("StarlightRiver/Assets/Tiles/Moonstone/MoonstoneRunes", UpdateMoonParticles);

			On.Terraria.Main.DrawBackgroundBlackFill += DrawParticleTarget;
			Main.OnPreDraw += DrawToParticleTarget;
		}

		public static void ResizeTarget()
		{
			Main.QueueMainThreadAction(() =>
			{
				target = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);
			});
		}

		private void DrawToParticleTarget(GameTime obj)
		{
			if (opacity == 0)
				return;

			GraphicsDevice gD = Main.graphics.GraphicsDevice;
			SpriteBatch spriteBatch = Main.spriteBatch;

			if (Main.gameMenu || Main.dedServ || spriteBatch is null || target is null || gD is null)
				return;

			RenderTargetBinding[] bindings = gD.GetRenderTargets();
			gD.SetRenderTarget(target);
			gD.Clear(Color.Transparent);

			Main.spriteBatch.Begin(default, default, default, default, default, null, Main.GameViewMatrix.ZoomMatrix);

			particleSystem.DrawParticles(Main.spriteBatch);

			spriteBatch.End();
			gD.SetRenderTargets(bindings);
		}

		private void DrawParticleTarget(On.Terraria.Main.orig_DrawBackgroundBlackFill orig, Main self)
        {
			orig(self);

			if (moonstoneBlockCount < 150)
			{
				if (opacity > 0)
					opacity -= 0.05f;
				else
					return;
			}
			else if (opacity < 1)
				opacity += 0.05f;

			Main.spriteBatch.End();
			Effect effect = Filters.Scene["MoonstoneRunes"].GetShader().Shader;
			effect.Parameters["intensity"].SetValue(10f);
			effect.Parameters["time"].SetValue((float)Main.timeForVisualEffects * 0.1f);

			effect.Parameters["noiseTexture1"].SetValue(ModContent.Request<Texture2D>(AssetDirectory.Assets + "Noise/MiscNoise3").Value);
			effect.Parameters["noiseTexture2"].SetValue(ModContent.Request<Texture2D>(AssetDirectory.Assets + "Noise/MiscNoise4").Value);
			effect.Parameters["color1"].SetValue(Color.Magenta.ToVector4());
			effect.Parameters["color2"].SetValue(Color.Cyan.ToVector4());
			effect.Parameters["opacity"].SetValue(1f);

			effect.Parameters["screenWidth"].SetValue(target.Width);
			effect.Parameters["screenHeight"].SetValue(target.Height);

			effect.Parameters["drawOriginal"].SetValue(false);

			Main.spriteBatch.Begin(default, BlendState.Additive, default, default, default, effect);

			Main.spriteBatch.Draw(target, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);

			if (Main.rand.NextBool(30))
				particleSystem.AddParticle(new Particle(Vector2.Zero, new Vector2(Main.rand.NextFloat(-0.1f, 0.1f), Main.rand.NextFloat(-1.4f, -0.8f)), 0, 1, Color.White,
					3000, new Vector2(Main.screenPosition.X + Main.rand.Next(Main.screenWidth), Main.screenPosition.Y + Main.screenHeight + 20), new Rectangle(0, 32 * Main.rand.Next(6), 32, 32)));

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, default, default, default, default, default);

			Main.spriteBatch.Draw(target, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White * 0.9f);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);
		}

        protected void UpdateMoonParticles(Particle particle)
        {
            particle.Position = particle.StoredPosition - Main.screenPosition;
            particle.StoredPosition += particle.Velocity;
            float opacity = 1;
            Color color = Color.White;
            particle.Color = color * opacity * this.opacity;
            particle.Timer--;
        }
	}
}
