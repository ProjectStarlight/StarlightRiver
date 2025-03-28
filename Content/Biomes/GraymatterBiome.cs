﻿using ReLogic.Utilities;
using StarlightRiver.Content.Bosses.TheThinkerBoss;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Tiles.Crimson;
using StarlightRiver.Core.Loaders;
using StarlightRiver.Core.Systems;
using StarlightRiver.Core.Systems.ScreenTargetSystem;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;

namespace StarlightRiver.Content.Biomes
{
	internal class GraymatterBiome : ModBiome
	{
		public static bool forceGrayMatter;
		public static int forceTimer;

		public Vector2 lastGrayPos;

		public static ScreenTarget hallucinationMap;
		public static ScreenTarget overHallucinationMap;

		/// <summary>
		/// Can be subscribed to to add additional hallucinatory areas to the screen
		/// </summary>
		public static Action<SpriteBatch> onDrawHallucinationMap;

		/// <summary>
		/// Can be subscribed to to add additional 'hallucinatory' objects that can only be seen in a hallucinatory area
		/// </summary>
		public static Action<SpriteBatch> onDrawOverHallucinationMap;

		/// <summary>
		/// Can be subscribed to for drawing hallucinatory tiles, seperated to be able to be called from one iteration
		/// for optimization.
		/// </summary>
		public static Action<SpriteBatch, int, int> onDrawOverPerTile;

		public static int fullscreenTimer = 0;

		/// <summary>
		/// List of tiles with graymatter emissions.
		/// </summary>
		public static HashSet<int> grayEmissionTypes = new();

		public override SceneEffectPriority Priority => SceneEffectPriority.None;

		public override int Music => -1;

		public override void Load()
		{
			hallucinationMap = new(DrawHallucinationMap, () => IsBiomeActive(Main.LocalPlayer), 1);
			overHallucinationMap = new(DrawOverHallucinationMap, () => IsBiomeActive(Main.LocalPlayer), 1.1f);

			ScreenspaceShaderSystem.AddScreenspacePass(new(0, DrawAuras, () => IsBiomeActive(Main.LocalPlayer)));
		}

		public override bool IsBiomeActive(Player player)
		{
			return forceGrayMatter || forceTimer > 0 || fullscreenTimer > 0 || ModContent.GetInstance<GraymatterBiomeSystem>().anyTiles;
		}

		public override void OnInBiome(Player player)
		{
			if (player == Main.LocalPlayer)
			{
				if (player.HasBuff(ModContent.BuffType<CrimsonHallucination>()))
				{
					lastGrayPos = player.Center;
					if (fullscreenTimer < 40)
						fullscreenTimer++;
				}
				else if (fullscreenTimer > 0)
				{
					fullscreenTimer--;
				}
			}
		}

		/// <summary>
		/// Draws the graymatter aura for all tiles marked as emitting graymatter
		/// </summary>
		/// <param name="spriteBatch"></param>
		private void DrawTileMap(SpriteBatch spriteBatch)
		{
			if (!StarlightWorld.HasFlag(WorldFlags.ThinkerBossOpen))
				return;

			Texture2D glow = Assets.Masks.GlowAlpha.Value;
			var pos = (Main.screenPosition / 16).ToPoint16();

			int width = Main.screenWidth / 16 + 1;
			int height = Main.screenHeight / 16 + 1;

			Color color = new(0.7f, 0.7f, 0.7f, 0f);
			Vector2 origin = glow.Size() / 2f;

			for (int x = pos.X; x < pos.X + width; x++)
			{
				for (int y = pos.Y; y < pos.Y + height; y++)
				{
					Tile tile = Main.tile[x, y];

					if (grayEmissionTypes.Contains(tile.TileType))
					{
						Vector2 drawPos = new Vector2(x, y) * 16 + Vector2.One * 8 - Main.screenPosition;

						// Draw to map
						spriteBatch.Draw(glow, drawPos, null, color, 0, origin, 1.1f + 0.4f * MathF.Sin(Main.GameUpdateCount * 0.05f + (x ^ y)), 0, 0);
					}
				}
			}
		}

		/// <summary>
		/// draws the tiles that emit gray matter as themselves over
		/// </summary>
		/// <param name="spriteBatch"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		private void DrawTileOverlay(SpriteBatch spriteBatch, int x, int y)
		{
			Tile tile = Main.tile[x, y];

			if (grayEmissionTypes.Contains(tile.TileType))
			{
				Texture2D tex = Terraria.GameContent.TextureAssets.Tile[tile.TileType].Value;
				spriteBatch.Draw(tex, new Vector2(x, y) * 16 - Main.screenPosition, new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16), Color.White * 0.1f);
			}
		}

		public void DrawHallucinationMap(SpriteBatch spriteBatch)
		{
			spriteBatch.End();
			spriteBatch.Begin(default, default, SamplerState.PointWrap, default, default, default, Main.GameViewMatrix.TransformationMatrix);

			DrawTileMap(spriteBatch);
			onDrawHallucinationMap?.Invoke(spriteBatch);

			// Draw the screen overlay for when the player is actively standing on gray matter
			Texture2D glow = Assets.Masks.GlowAlpha.Value;
			spriteBatch.Draw(glow, lastGrayPos - Main.screenPosition, null, new Color(1, 1, 1f, 0), 0, glow.Size() / 2f, Main.screenWidth / glow.Width * (fullscreenTimer / 9f), 0, 0);
			spriteBatch.Draw(glow, lastGrayPos - Main.screenPosition, null, new Color(1, 1, 1f, 0), 0, glow.Size() / 2f, Main.screenWidth / glow.Width * (fullscreenTimer / 9f), 0, 0);

			spriteBatch.End();
			spriteBatch.Begin(default, default, SamplerState.PointWrap, default, default);
		}

		public void DrawOverHallucinationMap(SpriteBatch spriteBatch)
		{
			spriteBatch.End();
			spriteBatch.Begin(default, default, SamplerState.PointWrap, default, default, default, Main.GameViewMatrix.TransformationMatrix);

			onDrawOverHallucinationMap?.Invoke(spriteBatch);

			var pos = (Main.screenPosition / 16).ToPoint16();

			int width = Main.screenWidth / 16 + 1;
			int height = Main.screenHeight / 16 + 1;

			for (int x = pos.X; x < pos.X + width; x++)
			{
				for (int y = pos.Y; y < pos.Y + height; y++)
				{
					onDrawOverPerTile.Invoke(spriteBatch, x, y);
					DrawTileOverlay(spriteBatch, x, y);
				}
			}

			spriteBatch.End();
			spriteBatch.Begin(default, default, SamplerState.PointWrap, default, default);
		}

		private void DrawAuras(SpriteBatch spriteBatch, Texture2D screen)
		{
			if (IsBiomeActive(Main.LocalPlayer))
			{
				Effect shader = ShaderLoader.GetShader("GrayMatter").Value;

				if (shader != null)
				{
					Texture2D noise = Assets.Noise.SwirlyNoiseLooping.Value;

					shader.Parameters["background"].SetValue(screen);
					shader.Parameters["map"].SetValue(hallucinationMap.RenderTarget);
					shader.Parameters["noise"].SetValue(noise);
					shader.Parameters["over"].SetValue(overHallucinationMap.RenderTarget);
					shader.Parameters["time"].SetValue(Main.GameUpdateCount * 0.02f);
					shader.Parameters["screensize"].SetValue(noise.Size() / new Vector2(Main.screenWidth, Main.screenHeight));
					shader.Parameters["screenpos"].SetValue(-Main.screenPosition / Main.ScreenSize.ToVector2());

					shader.Parameters["distortionpow"].SetValue(0.1f);
					shader.Parameters["chromepow"].SetValue(1.25f);

					spriteBatch.Begin(default, default, SamplerState.PointWrap, default, default, shader);

					spriteBatch.Draw(screen, Vector2.Zero, Color.White);

					spriteBatch.End();
				}
			}
		}
	}

	internal class GraymatterBiomeSystem : ModSystem
	{
		public bool anyTiles;

		public List<Vector2> thinkerPositions = new();

		public override void Load()
		{
			On_WorldGen.CrimVein += AddThinker;
		}

		private void AddThinker(On_WorldGen.orig_CrimVein orig, Vector2D position, Vector2D velocity)
		{
			if (!thinkerPositions.Contains(position.ToPoint().ToVector2()) && !thinkerPositions.Any(n => Vector2.Distance(position.ToPoint().ToVector2(), n) < 100))
				thinkerPositions.Add(position.ToPoint().ToVector2());

			orig(position, velocity);
		}

		public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts)
		{
			anyTiles = tileCounts[ModContent.TileType<GrayMatter>()] > 0;
		}

		public override void SaveWorldData(TagCompound tag)
		{
			tag.Add("ThinkerPositions", thinkerPositions);
		}

		public override void ClearWorld()
		{
			thinkerPositions.Clear();
			TheThinker.toRender.Clear();
		}

		public override void PreUpdateEntities()
		{
			if (GraymatterBiome.forceGrayMatter)
				GraymatterBiome.forceTimer = 4;

			if (GraymatterBiome.forceTimer > 0)
				GraymatterBiome.forceTimer--;

			GraymatterBiome.forceGrayMatter = false;
		}

		public static void SpawnThinkers()
		{
			foreach (Vector2 pos in ModContent.GetInstance<GraymatterBiomeSystem>().thinkerPositions)
			{
				if (!Main.npc.Any(n => n.active && n.type == ModContent.NPCType<TheThinker>() && Vector2.Distance(n.Center, pos) < 100))
					NPC.NewNPC(null, (int)pos.X * 16, (int)pos.Y * 16, ModContent.NPCType<TheThinker>());
			}
		}

		public override void LoadWorldData(TagCompound tag)
		{
			thinkerPositions = tag.GetList<Vector2>("ThinkerPositions") as List<Vector2>;

			foreach (Vector2 pos in thinkerPositions)
			{
				if (!Main.npc.Any(n => n.active && n.type == ModContent.NPCType<TheThinker>() && Vector2.Distance(n.Center, pos) < 100))
					NPC.NewNPC(null, (int)pos.X * 16, (int)pos.Y * 16, ModContent.NPCType<TheThinker>());
			}
		}
	}
}