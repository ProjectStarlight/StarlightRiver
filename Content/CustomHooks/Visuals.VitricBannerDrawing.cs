using StarlightRiver.Content.Physics;
using Terraria.Graphics.Effects;

namespace StarlightRiver.Content.CustomHooks
{
	class VitricBannerDrawing : HookGroup
	{
		//Creates a RenderTarget and then renders the banners in the vitric desert. Should be fairly safe, as its just drawing.
		public override void Load()
		{
			if (Main.dedServ)
				return;

			On.Terraria.Main.DrawProjectiles += DrawVerletBanners;
			On.Terraria.Main.SetDisplayMode += RefreshBannerTarget;
			On.Terraria.Main.CheckMonoliths += DrawVerletBannerTarget;
		}

		private void DrawVerletBanners(On.Terraria.Main.orig_DrawProjectiles orig, Main self)
		{
			Effect shader = Filters.Scene["Outline"].GetShader().Shader;

			if (shader is null)
				return;

			shader.Parameters["resolution"].SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
			shader.Parameters["outlineColor"].SetValue(new Vector3(0, 0, 0));

			Main.spriteBatch.Begin(default, default, SamplerState.PointClamp, default, default, Filters.Scene["Outline"].GetShader().Shader, Main.GameViewMatrix.ZoomMatrix);

			VerletChain.DrawStripsPixelated(Main.spriteBatch);

			Main.spriteBatch.End();

			orig(self);
		}

		private void RefreshBannerTarget(On.Terraria.Main.orig_SetDisplayMode orig, int width, int height, bool fullscreen)
		{
			if (width != Main.screenWidth || height != Main.screenHeight)
				VerletChainSystem.target = Main.dedServ ? null : new RenderTarget2D(Main.instance.GraphicsDevice, width / 2, height / 2, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);

			orig(width, height, fullscreen);
		}

		private void DrawVerletBannerTarget(On.Terraria.Main.orig_CheckMonoliths orig)
		{
			orig();

			if (Main.gameMenu)
			{
				VerletChainSystem.toDraw.Clear(); // we clear because the toDraw list is static and we need to manually clear when we're not in a world so we don't get ghost freezeframes when rejoining a multiPlayer world (singlePlayer could be cleared on world load potentially)
				return;
			}

			GraphicsDevice graphics = Main.instance.GraphicsDevice;

			graphics.SetRenderTarget(VerletChainSystem.target);
			graphics.Clear(Color.Transparent);

			graphics.BlendState = BlendState.Opaque;

			VerletChainSystem.toDraw.RemoveAll(n => IsBannerDead(n));

			foreach (VerletChain i in VerletChainSystem.toDraw)
				i.DrawStrip(i.scale);

			graphics.SetRenderTarget(null);
		}

		private bool IsBannerDead(VerletChain chain)
		{
			if (chain.parent is null)
				return true;

			if (chain.parent is NPC)
				return !(chain.parent as NPC).active;

			if (chain.parent is Projectile)
				return !(chain.parent as Projectile).active;

			if (chain.parent is Player)
				return !(chain.parent as Player).active;

			return false;
		}
	}
}