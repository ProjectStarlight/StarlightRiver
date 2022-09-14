//TODO:
//Make moon runes handled in a separate file from tilecounts
//Make moonstone visuals not clip downward
//Make moonstone particles not persist onto the menu

//TODO ON WORLDGEN:
//Make it a garaunteed spawn
//Move to GenerateMoonstone
//Wall updates
//General cleanup of post-shape stuff

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
		public ParticleSystem particleSystemMedium;
		public ParticleSystem particleSystemLarge;

        public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)
        {
			//Main.ColorOfTheSkies = Color.Lerp(Main.ColorOfTheSkies, new Color(15, 10, 25), opacity);
			//backgroundColor = Color.Lerp(backgroundColor, new Color(75, 35, 75), opacity);
			//tileColor = Color.Lerp(tileColor, new Color(65, 25, 65), opacity);
		}

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
			particleSystemMedium = new ParticleSystem("StarlightRiver/Assets/Tiles/Moonstone/MoonstoneRunesMedium", UpdateMoonParticles);
			particleSystemLarge = new ParticleSystem("StarlightRiver/Assets/Tiles/Moonstone/MoonstoneRunesLarge", UpdateMoonParticles);

			On.Terraria.Main.DrawBackgroundBlackFill += DrawParticleTarget;
           // On.Terraria.Main.DrawSurfaceBG += DistortBG;
			Main.OnPreDraw += DrawToParticleTarget;
		}

        /*private void DistortBG(On.Terraria.Main.orig_DrawSurfaceBG orig, Main self)
        {
			orig(self);
			if (opacity > 0)
			{
				Main.NewText(Main.drawToScreen.ToString());
				Main.spriteBatch.End();

				Effect effect = Filters.Scene["MoonstoneDistortion"].GetShader().Shader;
				effect.Parameters["intensity"].SetValue(0.3f);
				effect.Parameters["repeats"].SetValue(1);
				effect.Parameters["time"].SetValue((float)Main.timeForVisualEffects * 0.01f);
				effect.Parameters["noiseTexture1"].SetValue(ModContent.Request<Texture2D>(AssetDirectory.Assets + "Noise/SwirlyNoiseLooping").Value);
				effect.Parameters["noiseTexture2"].SetValue(ModContent.Request<Texture2D>(AssetDirectory.Assets + "Noise/MiscNoise1").Value);
				Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, effect, Main.Transform);

				float x2 = (Main.sceneBackgroundPos.X + (float)Main.offScreenRange) * Main.caveParallax - (float)Main.offScreenRange;
				Main.spriteBatch.Draw(Main.instance.backgroundTarget, new Vector2(x2, Main.sceneBackgroundPos.Y), Color.Red);

				Main.spriteBatch.End();
				Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.Default, RasterizerState.CullNone);
			}
		}*/

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
			particleSystemMedium.DrawParticles(Main.spriteBatch);
			particleSystemLarge.DrawParticles(Main.spriteBatch);

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
				opacity += 0.025f;

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
			effect.Parameters["screenPosition"].SetValue(Main.screenPosition); 
			effect.Parameters["drawOriginal"].SetValue(false);

			Main.spriteBatch.Begin(default, BlendState.Additive, default, default, default, effect);

			Main.spriteBatch.Draw(target, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);

			if (Main.rand.NextBool(150))
				particleSystem.AddParticle(new Particle(Vector2.Zero, Main.rand.NextVector2Circular(0.35f, 0.35f), 0, Main.rand.NextFloat(0.8f, 1.2f), Color.White,
					2000, new Vector2(Main.screenPosition.X + Main.rand.Next(Main.screenWidth), Main.screenPosition.Y + Main.rand.Next(Main.screenHeight)), new Rectangle(0, 32 * Main.rand.Next(6), 32, 32)));

			if (Main.rand.NextBool(300))
				particleSystemMedium.AddParticle(new Particle(Vector2.Zero, Main.rand.NextVector2Circular(0.25f, 0.25f), 0, Main.rand.NextFloat(0.8f, 1.2f), Color.White,
					2000, new Vector2(Main.screenPosition.X + Main.rand.Next(Main.screenWidth), Main.screenPosition.Y + Main.rand.Next(Main.screenHeight)), new Rectangle(0, 46 * Main.rand.Next(4), 50, 46)));

			if (Main.rand.NextBool(600))
				particleSystemLarge.AddParticle(new Particle(Vector2.Zero, Main.rand.NextVector2Circular(0.2f, 0.2f), 0, Main.rand.NextFloat(0.8f, 1.2f), Color.White,
					2000, new Vector2(Main.screenPosition.X + Main.rand.Next(Main.screenWidth), Main.screenPosition.Y + Main.rand.Next(Main.screenHeight)), new Rectangle(0, 60 * Main.rand.Next(4), 50, 60)));

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, default, default, default, default, default);

			Main.spriteBatch.Draw(target, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White * 0.9f);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);
		}

        protected void UpdateMoonParticles(Particle particle)
        {
			float parallax = 0.6f;
			if (particle.Frame.Y % 46 == 0)
				parallax = 0.8f;
			if (particle.Frame.Y % 60 == 0)
				parallax = 1f;


			if (particle.Position == Vector2.Zero)
            {
				particle.StoredPosition -= Main.screenPosition * (1 - parallax);
            }

            particle.Position = particle.StoredPosition - (Main.screenPosition * parallax);
            particle.StoredPosition += particle.Velocity;
			particle.Velocity = particle.Velocity.RotatedByRandom(0.1f);
			float fade = MathHelper.Min(MathHelper.Min(particle.Timer / 200f, (2000 - particle.Timer) / 200f), 0.4f);
            Color color = Color.White;
            particle.Color = color * this.opacity * fade;
            particle.Timer--;
        }
	}
}
