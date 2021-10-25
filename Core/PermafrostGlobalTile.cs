using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.ModLoader;

/*
 * Fair warning to anyone reading this file, I'm fairly certain what you see here was only able to be made through demonic posession or similar supernatural events.
 * Do not attempt to reverse engineer this yourself, for everyone's sake.
 * If this is broken in the future, just cancel the mod. Its not worth it.
 * Please.
 */

namespace StarlightRiver.Core
{
	class PermafrostGlobalTile : ModWorld, ILoadable
	{
		public static RenderTarget2D auroraTarget;
		public static RenderTarget2D auroraBackTarget;

		public float Priority => 1;

		public void Load()
		{
			IL.Terraria.IO.WorldFile.SaveWorldTiles += SaveExtraBits;
			IL.Terraria.IO.WorldFile.LoadWorldTiles += LoadExtraBits;
			On.Terraria.Tile.isTheSameAs += CompareExtraBits;
			On.Terraria.Main.SetDisplayMode += RefreshWaterTargets;
			On.Terraria.Main.CheckMonoliths += DrawAuroraTarget;

			if (!Main.dedServ)
			{
				auroraBackTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight, false, default, default, default, RenderTargetUsage.PreserveContents);
				auroraTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight, false, default, default, default, RenderTargetUsage.PreserveContents);
			}
		}

		public void Unload() { }

		private void RefreshWaterTargets(On.Terraria.Main.orig_SetDisplayMode orig, int width, int height, bool fullscreen)
		{
			if (Main.dedServ)
				return;

			if (!Main.gameInactive && (width != Main.screenWidth || height != Main.screenHeight))
			{
				auroraBackTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, width, height, false, default, default, default, RenderTargetUsage.PreserveContents);
				auroraTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, width, height, false, default, default, default, RenderTargetUsage.PreserveContents);
			}

			orig(width, height, fullscreen);
		}

		private void DrawAuroraTarget(On.Terraria.Main.orig_CheckMonoliths orig)
		{
			orig();

			if (Main.dedServ || Main.gameMenu)
				return;

			Main.spriteBatch.Begin();

			Main.graphics.GraphicsDevice.SetRenderTarget(auroraTarget);
			Main.graphics.GraphicsDevice.Clear(Color.Transparent);

			for (int i = -2 + (int)(Main.screenPosition.X) / 16; i <= 2 + (int)(Main.screenPosition.X + Main.screenWidth) / 16; i++)
				for (int j = -2 + (int)(Main.screenPosition.Y) / 16; j <= 2 + (int)(Main.screenPosition.Y + Main.screenHeight) / 16; j++)
				{
					if (WorldGen.InWorld(i, j))
					{
						Tile tile = Framing.GetTileSafely(i, j);
						if ((tile.bTileHeader3 & 0b11100000) >> 5 == 1)
						{
							Rectangle target = new Rectangle((int)(i * 16 - Main.screenPosition.X), (int)(j * 16 - Main.screenPosition.Y), 16, 16);
							Texture2D tex = ModContent.GetTexture(AssetDirectory.Assets + "Misc/AuroraWater");
							Main.spriteBatch.Draw(tex, target, findSource(i, j), Color.White * 0.5f);
						}

						tile = null;
					}
				}

			Main.spriteBatch.End();
			Main.graphics.GraphicsDevice.SetRenderTarget(null);

			Main.spriteBatch.Begin(default, BlendState.Additive, SamplerState.PointWrap, default, default, default, Main.GameViewMatrix.ZoomMatrix);

			Main.graphics.GraphicsDevice.SetRenderTarget(auroraBackTarget);
			Main.graphics.GraphicsDevice.Clear(Color.Transparent);

			Texture2D tex2 = ModContent.GetTexture("StarlightRiver/Assets/Misc/AuroraWaterMap");

			for (int i = -tex2.Width; i <= Main.screenWidth + tex2.Width; i += tex2.Width)
				for (int j = -tex2.Height; j <= Main.screenHeight + tex2.Height; j += tex2.Height)
				{
					Main.spriteBatch.Draw(tex2, new Vector2(i, j) * 1.5f, 
						new Rectangle(
							(int)((Main.screenPosition.X * 0.6f) % tex2.Width),
							(int)((Main.screenPosition.Y * 0.6f) % tex2.Height),
							tex2.Width,
							tex2.Height
							), 
						Color.White * 0.7f, default, default, 1.5f, 0, 0);

					Main.spriteBatch.Draw(tex2, new Vector2(i, j), 
						new Rectangle(
							(int)((Main.screenPosition.X * 0.8f) % tex2.Width),
							(int)((Main.screenPosition.Y * 0.8f) % tex2.Height),
							tex2.Width,
							tex2.Height
							), 
						Color.White);
				}

			Main.spriteBatch.End();
			Main.graphics.GraphicsDevice.SetRenderTarget(null);
		}

		private bool CompareExtraBits(On.Terraria.Tile.orig_isTheSameAs orig, Tile self, Tile compTile)
		{
			return orig(self, compTile) && (self.bTileHeader3 & 0b11100000) == (compTile.bTileHeader3 & 0b11100000);
		}

		private void SaveExtraBits(ILContext il)
		{
			ILCursor c = new ILCursor(il);
			if( c.TryGotoNext(n => n.MatchLdloc(0), n => n.MatchLdloc(12), n => n.MatchLdloc(7), n => n.MatchStelemI1()) )
			{
				c.Index += 3;

				c.Emit(OpCodes.Ldloc, 5);
				c.Emit(OpCodes.Ldfld, typeof(Tile).GetField("bTileHeader3"));
				c.Emit(OpCodes.Ldc_I4_1);
				c.Emit(OpCodes.Shl);
				c.Emit(OpCodes.Ldc_I4, 0b11000001);
				c.Emit(OpCodes.And);
				c.Emit(OpCodes.Or);
				c.Emit(OpCodes.Conv_U1);

				c.TryGotoPrev(n => n.MatchLdloc(8));
				ILLabel label = il.DefineLabel(c.Next);

				c.TryGotoPrev(n => n.MatchLdloc(7));
				c.Emit(OpCodes.Br, label);
			}
		}

		private void LoadExtraBits(ILContext il)
		{
			ILCursor c = new ILCursor(il);

			if( c.TryGotoNext(n => n.MatchLdloc(5), n => n.MatchLdcI4(2), n => n.MatchAnd(), n => n.MatchLdcI4(2) ))
			{
				c.Emit(OpCodes.Ldloc, 7);
				c.Emit(OpCodes.Dup);
				c.Emit(OpCodes.Ldfld, typeof(Tile).GetField("bTileHeader3"));

				c.Emit(OpCodes.Ldloc, 5);
				c.Emit(OpCodes.Ldc_I4_1);
				c.Emit(OpCodes.Shr_Un);
				c.Emit(OpCodes.Ldc_I4, 0b11100000);
				c.Emit(OpCodes.And);
				c.Emit(OpCodes.Conv_U1);
				c.Emit(OpCodes.Or);
				c.Emit(OpCodes.Conv_U1);
				c.Emit(OpCodes.Stfld, typeof(Tile).GetField("bTileHeader3"));
			}
		}

		public override void PostDrawTiles()
		{
			var shader = Terraria.Graphics.Effects.Filters.Scene["AuroraWaterShader"].GetShader().Shader;
			shader.Parameters["time"].SetValue(StarlightWorld.rottime);
			shader.Parameters["screenSize"].SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
			shader.Parameters["offset"].SetValue(new Vector2(Main.screenPosition.X % Main.screenWidth / Main.screenWidth, Main.screenPosition.Y % Main.screenHeight / Main.screenHeight));
			shader.Parameters["sampleTexture2"].SetValue(auroraBackTarget);

			Main.spriteBatch.Begin(default, BlendState.Additive, default, default, default, shader);

			Main.spriteBatch.Draw(auroraTarget, Vector2.Zero, Color.White);

			Main.spriteBatch.End();
		}

		private Rectangle findSource(int i, int j) //too tired to figure smth better. Ideally this would be cached on the tile and only updated when neccisary but not enough extra data exists on tiles to do so.
		{
			var u = ((Framing.GetTileSafely(i, j - 1).bTileHeader3 & 0b11100000) >> 5) == 1;
			var l = ((Framing.GetTileSafely(i - 1, j).bTileHeader3 & 0b11100000) >> 5) == 1;
			var d = ((Framing.GetTileSafely(i, j + 1).bTileHeader3 & 0b11100000) >> 5) == 1;
			var r = ((Framing.GetTileSafely(i + 1, j).bTileHeader3 & 0b11100000) >> 5) == 1;

			if (u && l && d && r)
				return new Rectangle(18 * 3, 18 * 2, 16, 16);

			else if (l && r && d)
				return new Rectangle(18, 0, 16, 16);
			else if (u && d && r)
				return new Rectangle(18, 18, 16, 16);
			else if (l && r && u)
				return new Rectangle(18, 18 * 2, 16, 16);
			else if (u && d && l)
				return new Rectangle(18, 18 * 3, 16, 16);

			else if (r && d)
				return new Rectangle(0, 0, 16, 16);
			else if (u && r)
				return new Rectangle(0, 18, 16, 16);
			else if (l && u)
				return new Rectangle(0, 18 * 2, 16, 16);
			else if (d && l)
				return new Rectangle(0, 18 * 3, 16, 16);

			else if (u && d)
				return new Rectangle(18 * 3, 0, 16, 16);
			else if (l && r)
				return new Rectangle(18 * 3, 18, 16, 16);


			else if (u)
				return new Rectangle(18 * 2, 0, 16, 16);
			else if (l)
				return new Rectangle(18 * 2, 18, 16, 16);
			else if (d)
				return new Rectangle(18 * 2, 18 * 2, 16, 16);
			else if (r)
				return new Rectangle(18 * 2, 18 * 3, 16, 16);

			return new Rectangle(18 * 3, 18 * 3, 16, 16);
		}
	}
}
