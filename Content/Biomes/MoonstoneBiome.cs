using StarlightRiver.Content.Items.Utility;
using StarlightRiver.Content.Tiles.Moonstone;
using StarlightRiver.Core.Loaders;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Core.Systems.ScreenTargetSystem;
using System;
using System.Linq;
using System.Reflection;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader.IO;

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
		public ScreenTarget target;
		public ScreenTarget backgroundTarget;

		public int moonstoneBlockCount;
		public bool overrideVFXActive = false;

		private float opacity = 0;
		private float distortion = 0;

		private bool drawingBGtarget = false;

		public bool moonstoneForced;
		public bool meteorForced;

		public ParticleSystem particleSystem;
		public ParticleSystem particleSystemMedium;
		public ParticleSystem particleSystemLarge;

		public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)
		{
			Main.ColorOfTheSkies = Color.Lerp(Main.ColorOfTheSkies, new Color(25, 15, 35), opacity);
			backgroundColor = Color.Lerp(backgroundColor, new Color(120, 65, 120), opacity);
			tileColor = Color.Lerp(tileColor, new Color(120, 65, 120), opacity);
		}

		public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts)
		{
			moonstoneBlockCount = tileCounts[ModContent.TileType<MoonstoneOre>()];
		}

		public override void Load()
		{
			if (Main.dedServ)
				return;

			particleSystem = new ParticleSystem("StarlightRiver/Assets/Tiles/Moonstone/MoonstoneRunes", UpdateMoonParticles);
			particleSystemMedium = new ParticleSystem("StarlightRiver/Assets/Tiles/Moonstone/MoonstoneRunesMedium", UpdateMoonParticles);
			particleSystemLarge = new ParticleSystem("StarlightRiver/Assets/Tiles/Moonstone/MoonstoneRunesLarge", UpdateMoonParticles);

			target = new(DrawTargetOne, () => opacity > 0, 1);
			backgroundTarget = new(DrawTargetTwo, () => opacity > 0 || distortion > 0, 1);

			On_Main.DrawBackgroundBlackFill += DrawParticleTarget;
			On_Main.DrawSurfaceBG += DistortBG;
			On_WorldGen.meteor += ReplaceMeteorWithMoonstone;
		}

		public override void PostUpdateEverything()
		{
			if (moonstoneBlockCount < 150 && !overrideVFXActive)
			{
				if (distortion > 0)
					distortion -= 0.005f;

				if (opacity > 0)
					opacity -= 0.05f;
			}
			else
			{
				if (distortion < 1)
					distortion += 0.001f * (overrideVFXActive ? 5 : 1);

				if (opacity < 1)
					opacity += 0.001f * (overrideVFXActive ? 10 : 1);
			}

			if (opacity > 0)
			{
				particleSystem.UpdateParticles();
				particleSystemMedium.UpdateParticles();
				particleSystemLarge.UpdateParticles();
			}
		}

		public override void ResetNearbyTileEffects()
		{
			overrideVFXActive = false;
		}

		private void DistortBG(On_Main.orig_DrawSurfaceBG orig, Main self)
		{
			if (distortion > 0 && !drawingBGtarget && !Main.gameMenu)
			{
				Effect effect = ShaderLoader.GetShader("MoonstoneDistortion").Value;

				if (effect != null)
				{
					Main.spriteBatch.End();

					effect.Parameters["intensity"].SetValue(0.01f * distortion);
					effect.Parameters["repeats"].SetValue(2);
					effect.Parameters["time"].SetValue((float)Main.timeForVisualEffects * 0.003f);
					effect.Parameters["noiseTexture1"].SetValue(Assets.Noise.SwirlyNoiseLooping.Value);
					effect.Parameters["noiseTexture2"].SetValue(Assets.Noise.MiscNoise1.Value);
					effect.Parameters["screenPosition"].SetValue(Main.screenPosition * new Vector2(0.5f, 0.1f) / backgroundTarget.RenderTarget.Size());
					effect.Parameters["distortionColor1"].SetValue(Color.DarkBlue.ToVector3());
					effect.Parameters["distortionColor2"].SetValue(new Color(120, 65, 120).ToVector3());
					effect.Parameters["colorIntensity"].SetValue(0.03f * distortion);
					effect.Parameters["color"].SetValue(false);

					Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, effect, Main.GameViewMatrix.TransformationMatrix);
					Main.spriteBatch.Draw(backgroundTarget.RenderTarget, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);
					Main.spriteBatch.End();

					effect.Parameters["color"].SetValue(true);

					Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, effect, Main.GameViewMatrix.TransformationMatrix);
					Main.spriteBatch.Draw(backgroundTarget.RenderTarget, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);
					Main.spriteBatch.End();
					Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.Default, RasterizerState.CullNone);
				}
			}
			else
			{
				orig(self);
			}
		}

		private void DrawTargetOne(SpriteBatch sb)
		{
			Main.graphics.GraphicsDevice.Clear(Color.Transparent);

			sb.End();
			sb.Begin(default, default, Main.DefaultSamplerState, default, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

			particleSystem.DrawParticles(sb);
			particleSystemMedium.DrawParticles(sb);
			particleSystemLarge.DrawParticles(sb);
		}

		private void DrawTargetTwo(SpriteBatch sb)
		{
			Main.graphics.GraphicsDevice.Clear(Color.Transparent);

			sb.End();
			sb.Begin(default, default, Main.DefaultSamplerState, default, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

			drawingBGtarget = true;
			Main.instance.DrawSurfaceBG();
			drawingBGtarget = false;
		}

		private void DrawParticleTarget(On_Main.orig_DrawBackgroundBlackFill orig, Main self)
		{
			orig(self);

			if (opacity <= 0 || Main.gameMenu)
				return;

			Main.spriteBatch.End();
			Effect effect = ShaderLoader.GetShader("MoonstoneRunes").Value;

			if (effect != null)
			{
				effect.Parameters["intensity"].SetValue(10f);
				effect.Parameters["time"].SetValue((float)Main.timeForVisualEffects * 0.1f);

				effect.Parameters["noiseTexture1"].SetValue(Assets.Noise.MiscNoise3.Value);
				effect.Parameters["noiseTexture2"].SetValue(Assets.Noise.MiscNoise4.Value);
				effect.Parameters["color1"].SetValue(Color.Magenta.ToVector4());
				effect.Parameters["color2"].SetValue(Color.Cyan.ToVector4());
				effect.Parameters["opacity"].SetValue(1f);

				effect.Parameters["screenWidth"].SetValue(Main.screenWidth);
				effect.Parameters["screenHeight"].SetValue(Main.screenHeight);
				effect.Parameters["screenPosition"].SetValue(Main.screenPosition);
				effect.Parameters["drawOriginal"].SetValue(false);

				Main.spriteBatch.Begin(default, BlendState.Additive, Main.DefaultSamplerState, default, RasterizerState.CullNone, effect);

				Main.spriteBatch.Draw(target.RenderTarget, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);

				if (Main.rand.NextBool(150))
				{
					particleSystem.AddParticle(Vector2.Zero, Main.rand.NextVector2Circular(0.35f, 0.35f), 0, Main.rand.NextFloat(0.8f, 1.2f), Color.White,
						2000, new Vector2(Main.screenPosition.X + Main.rand.Next(Main.screenWidth), Main.screenPosition.Y + Main.rand.Next(Main.screenHeight)), new Rectangle(0, 32 * Main.rand.Next(6), 32, 32));
				}

				if (Main.rand.NextBool(300))
				{
					particleSystemMedium.AddParticle(Vector2.Zero, Main.rand.NextVector2Circular(0.25f, 0.25f), 0, Main.rand.NextFloat(0.8f, 1.2f), Color.White,
						2000, new Vector2(Main.screenPosition.X + Main.rand.Next(Main.screenWidth), Main.screenPosition.Y + Main.rand.Next(Main.screenHeight)), new Rectangle(0, 46 * Main.rand.Next(4), 50, 46));
				}

				if (Main.rand.NextBool(600))
				{
					particleSystemLarge.AddParticle(Vector2.Zero, Main.rand.NextVector2Circular(0.2f, 0.2f), 0, Main.rand.NextFloat(0.8f, 1.2f), Color.White,
						2000, new Vector2(Main.screenPosition.X + Main.rand.Next(Main.screenWidth), Main.screenPosition.Y + Main.rand.Next(Main.screenHeight)), new Rectangle(0, 60 * Main.rand.Next(4), 50, 60));
				}

				Main.spriteBatch.End();
			}

			Main.spriteBatch.Begin(default, default, Main.DefaultSamplerState, default, RasterizerState.CullNone, default);

			Main.spriteBatch.Draw(target.RenderTarget, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White * 0.9f);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, default, Main.DefaultSamplerState, default, Main.Rasterizer, default, Main.GameViewMatrix.TransformationMatrix);
		}

		/// <summary>
		/// If the next meteor-like event to spawn should me a moonstone or not
		/// </summary>
		/// <returns></returns>
		private bool ShouldBeMoonstone()
		{
			if (moonstoneForced)
			{
				moonstoneForced = false;
				return true;
			}

			if (meteorForced)
			{
				meteorForced = false;
				return false;
			}

			return Main.rand.NextBool();
		}

		private bool ReplaceMeteorWithMoonstone(On_WorldGen.orig_meteor orig, int i, int j, bool ignorePlayers)
		{
			CameraSystem.shake += 80;
			Terraria.Audio.SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode);

			if (ShouldBeMoonstone())
			{
				var target = new Point16();

				while (!SafeToSpawnMoonstone(target))
				{
					int x = Main.rand.Next(Main.maxTilesX);

					for (int y = 0; y < Main.maxTilesY; y++)
					{
						if (Framing.GetTileSafely(x, y).HasTile)
						{
							target = new Point16(x, y - 20);
							break;
						}
					}
				}

				for (int x = -10; x < 10; x++)
				{
					for (int y = -30; y < 30; y++)
					{
						if (Math.Abs(x) < 10 - Math.Abs(y) / 3 + StarlightWorld.genNoise.GetPerlin(x * 4, y * 4) * 8)
							WorldGen.PlaceTile(target.X + x, target.Y + y, ModContent.TileType<Tiles.Moonstone.MoonstoneOre>(), true, true);
					}
				}

				for (int x = -15; x < 15; x++)
				{
					for (int y = 0; y < 40; y++)
					{
						if (Math.Abs(x) < 10 - Math.Abs(y) / 3 + StarlightWorld.genNoise.GetPerlin(x * 4, y * 4) * 8)
							WorldGen.PlaceTile(target.X + x, target.Y + y, ModContent.TileType<Tiles.Moonstone.MoonstoneOre>(), true, true);
					}
				}

				Terraria.Chat.ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("A shard of the moon has landed!"), new Color(107, 233, 231));

				if (Main.netMode == NetmodeID.Server)
					NetMessage.SendTileSquare(Main.myPlayer, target.X - 30, target.Y - 30, 60, 70, TileChangeType.None);

				return true;
			}
			else
			{
				return orig(i, j, ignorePlayers);
			}
		}

		/// <summary>
		/// Emulates teh various safety checks vanilla uses for if a meteor can be spawned
		/// </summary>
		/// <param name="test">The center point that a moonstone is attempting to be spawned at</param>
		/// <returns></returns>
		private bool SafeToSpawnMoonstone(Point16 test)
		{
			if (test == Point16.Zero)
				return false;

			for (int x = -35; x < 35; x++)
			{
				for (int y = -35; y < 35; y++)
				{
					if (WorldGen.InWorld(test.X + x, test.Y + y))
					{
						Tile tile = Framing.GetTileSafely(test + new Point16(x, y));

						if (tile.TileType == TileID.Containers || tile.TileType == TileID.Containers2)
							return false;
					}
				}
			}

			if (Main.npc.Any(n => n.active && n.friendly && Vector2.Distance(n.Center, test.ToVector2() * 16) <= 35 * 16))
				return false;
			else
				return true;
		}

		protected void UpdateMoonParticles(Particle particle)
		{
			float parallax = 0.6f;

			if (particle.Frame.Y % 46 == 0)
				parallax = 0.8f;

			if (particle.Frame.Y % 60 == 0)
				parallax = 1f;

			if (particle.Position == Vector2.Zero)
				particle.StoredPosition -= Main.screenPosition * (1 - parallax);

			particle.Position = particle.StoredPosition - Main.screenPosition * parallax;
			particle.StoredPosition += particle.Velocity;
			particle.Velocity = particle.Velocity.RotatedByRandom(0.1f);
			float fade = MathHelper.Min(MathHelper.Min(particle.Timer / 200f, (2000 - particle.Timer) / 200f), 0.4f);
			Color color = Color.White;
			particle.Color = color * opacity * fade;
			particle.Timer--;
		}

		public override void SaveWorldData(TagCompound tag)
		{
			tag.Add("moonstoneForced", moonstoneForced);
			tag.Add("meteorForced", meteorForced);
		}

		public override void LoadWorldData(TagCompound tag)
		{
			moonstoneForced = tag.GetBool("moonstoneForced");
			meteorForced = tag.GetBool("meteorForced");
		}
	}
}