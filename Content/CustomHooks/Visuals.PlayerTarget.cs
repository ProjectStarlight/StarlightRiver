using System.Collections.Generic;
using System.Linq;

namespace StarlightRiver.Content.CustomHooks
{
	class PlayerTarget : HookGroup
	{
		//Drawing Player to Target. Should be safe. Excuse me if im duplicating something that alr exists :p
		public static RenderTarget2D Target;

		public static bool canUseTarget = false;

		public static int sheetSquareX;
		public static int sheetSquareY;

		/// <summary>
		/// we use a dictionary for the Player indexes because they are not guarenteed to be 0, 1, 2 etc. the Player at index 1 leaving could result in 2 Players being numbered 0, 2
		/// but we don't want a gigantic RT with all 255 possible Players getting template space so we resize and keep track of their effective index
		/// </summary>
		private static Dictionary<int, int> PlayerIndexLookup;

		/// <summary>
		/// to keep track of Player counts as they change
		/// </summary>
		private static int prevNumPlayers;

		//stored vars so we can determine original lighting for the Player / potentially other uses
		Vector2 oldPos;
		Vector2 oldCenter;
		Vector2 oldMountedCenter;
		Vector2 oldScreen;
		Vector2 oldItemLocation;
		Vector2 positionOffset;

		public override void Load()
		{
			if (Main.dedServ)
				return;

			sheetSquareX = 200;
			sheetSquareY = 300;

			PlayerIndexLookup = new Dictionary<int, int>();
			prevNumPlayers = -1;

			Main.QueueMainThreadAction(() => Target = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight));

			On_Main.CheckMonoliths += DrawTargets;
			On_Lighting.GetColor_int_int += getColorOverride;
			On_Lighting.GetColor_Point += getColorOverride;
			On_Lighting.GetColor_int_int_Color += getColorOverride;
			On_Lighting.GetColor_Point_Color += GetColorOverride;
			On_Lighting.GetColorClamped += GetColorOverride;
		}

		private Color GetColorOverride(On_Lighting.orig_GetColorClamped orig, int x, int y, Color oldColor)
		{
			if (canUseTarget)
				return orig.Invoke(x, y, oldColor);

			return orig.Invoke(x + (int)((oldPos.X - positionOffset.X) / 16), y + (int)((oldPos.Y - positionOffset.Y) / 16), oldColor);
		}

		private Color GetColorOverride(On_Lighting.orig_GetColor_Point_Color orig, Point point, Color originalColor)
		{
			if (canUseTarget)
				return orig.Invoke(point, originalColor);

			return orig.Invoke(new Point(point.X + (int)((oldPos.X - positionOffset.X) / 16), point.Y + (int)((oldPos.Y - positionOffset.Y) / 16)), originalColor);
		}

		public Color getColorOverride(On_Lighting.orig_GetColor_Point orig, Point point)
		{
			if (canUseTarget)
				return orig.Invoke(point);

			return orig.Invoke(new Point(point.X + (int)((oldPos.X - positionOffset.X) / 16), point.Y + (int)((oldPos.Y - positionOffset.Y) / 16)));
		}

		public Color getColorOverride(On_Lighting.orig_GetColor_int_int orig, int x, int y)
		{
			if (canUseTarget)
				return orig.Invoke(x, y);

			return orig.Invoke(x + (int)((oldPos.X - positionOffset.X) / 16), y + (int)((oldPos.Y - positionOffset.Y) / 16));
		}

		public Color getColorOverride(On_Lighting.orig_GetColor_int_int_Color orig, int x, int y, Color c)
		{
			if (canUseTarget)
				return orig.Invoke(x, y, c);

			return orig.Invoke(x + (int)((oldPos.X - positionOffset.X) / 16), y + (int)((oldPos.Y - positionOffset.Y) / 16), c);
		}

		public static Rectangle getPlayerTargetSourceRectangle(int whoAmI)
		{
			if (PlayerIndexLookup.ContainsKey(whoAmI))
				return new Rectangle(PlayerIndexLookup[whoAmI] * sheetSquareX, 0, sheetSquareX, sheetSquareY);

			return Rectangle.Empty;
		}

		/// <summary>
		/// gets the whoAmI's Player's renderTarget and returns a Vector2 that represents the rendertarget's position overlapping with the Player's position in terms of screen coordinates
		/// comes preshifted for reverse gravity
		/// </summary>
		/// <param name="whoAmI"></param>
		/// <returns></returns>
		public static Vector2 getPlayerTargetPosition(int whoAmI)
		{
			Vector2 gravPosition = Main.ReverseGravitySupport(Main.player[whoAmI].position - Main.screenPosition);
			return gravPosition - new Vector2(sheetSquareX / 2, sheetSquareY / 2);
		}

		private void DrawTargets(On_Main.orig_CheckMonoliths orig)
		{
			orig();

			if (Main.gameMenu)
				return;

			if (Main.player.Any(n => n.active))
				DrawPlayerTarget();

			if (Main.instance.tileTarget.IsDisposed)
				return;
		}

		public static Vector2 getPositionOffset(int whoAmI)
		{
			if (PlayerIndexLookup.ContainsKey(whoAmI))
				return new Vector2(PlayerIndexLookup[whoAmI] * sheetSquareX + sheetSquareX / 2, sheetSquareY / 2);

			return Vector2.Zero;
		}

		private void DrawPlayerTarget()
		{
			int activePlayerCount = Main.player.Count(n => n.active);

			if (activePlayerCount != prevNumPlayers)
			{
				prevNumPlayers = activePlayerCount;

				Target.Dispose();
				Target = new RenderTarget2D(Main.graphics.GraphicsDevice, sheetSquareY * activePlayerCount, sheetSquareY);

				int activeCount = 0;

				for (int i = 0; i < Main.maxPlayers; i++)
				{
					if (Main.player[i].active)
					{
						PlayerIndexLookup[i] = activeCount;
						activeCount++;
					}
				}
			}

			RenderTargetBinding[] oldtargets2 = Main.graphics.GraphicsDevice.GetRenderTargets();
			canUseTarget = false;

			Main.graphics.GraphicsDevice.SetRenderTarget(Target);
			Main.graphics.GraphicsDevice.Clear(Color.Transparent);

			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.EffectMatrix);

			for (int i = 0; i < Main.maxPlayers; i++)
			{
				Player player = Main.player[i];

				if (player.active && player.dye.Length > 0)
				{
					oldPos = player.position;
					oldCenter = player.Center;
					oldMountedCenter = player.MountedCenter;
					oldScreen = Main.screenPosition;
					oldItemLocation = player.itemLocation;
					int oldHeldProj = player.heldProj;
					
					//temp change Player's actual position to lock into their frame
					positionOffset = getPositionOffset(i);
					player.position = positionOffset;
					player.Center = oldCenter - oldPos + positionOffset;
					player.itemLocation = oldItemLocation - oldPos + positionOffset;
					player.MountedCenter = oldMountedCenter - oldPos + positionOffset;
					player.heldProj = -1;
					Main.screenPosition = Vector2.Zero;

					Main.PlayerRenderer.DrawPlayer(Main.Camera, player, player.position, player.fullRotation, player.fullRotationOrigin, 0f);

					player.position = oldPos;
					player.Center = oldCenter;
					Main.screenPosition = oldScreen;
					player.itemLocation = oldItemLocation;
					player.MountedCenter = oldMountedCenter;
					player.heldProj = oldHeldProj;
				}
			}

			Main.spriteBatch.End();

			Main.graphics.GraphicsDevice.SetRenderTargets(oldtargets2);
			canUseTarget = true;
		}
	}
}