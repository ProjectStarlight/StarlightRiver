using Terraria.Graphics;

namespace StarlightRiver.Content.CustomHooks
{
	class DrawLayers : HookGroup
	{
		//A few different hooks for drawing on certain layers. Orig is always run and its just draw calls.
		public override void Load()
		{
			if (Main.dedServ)
				return;

			//On.Terraria.Graphics.Renderers.LegacyPlayerRenderer.DrawPlayer += PostDrawPlayer;
			//On.Terraria.Graphics.Renderers.ReturnGatePlayerRenderer.DrawPlayer += PostDrawPlayerGate;

			Terraria.DataStructures.On_PlayerDrawLayers.DrawPlayer_RenderAllLayers += PostDrawPlayerLayer;
		}

		private void PostDrawPlayerLayer(Terraria.DataStructures.On_PlayerDrawLayers.orig_DrawPlayer_RenderAllLayers orig, ref Terraria.DataStructures.PlayerDrawSet drawinfo)
		{
			Player drawPlayer = drawinfo.drawPlayer;
			float shadow = drawinfo.shadow;

			if (!Main.gameMenu && shadow == 0)
				drawPlayer.GetModPlayer<StarlightPlayer>().PreDraw(drawPlayer, Main.spriteBatch);

			orig(ref drawinfo);

			if (!Main.gameMenu && shadow == 0)
				drawPlayer.GetModPlayer<StarlightPlayer>().PostDraw(drawPlayer, Main.spriteBatch);
		}

		private void PostDrawPlayer(Terraria.Graphics.Renderers.On_LegacyPlayerRenderer.orig_DrawPlayer orig, Terraria.Graphics.Renderers.LegacyPlayerRenderer self, Camera camera, Player drawPlayer, Vector2 position, float rotation, Vector2 rotationOrigin, float shadow, float scale)
		{
			if (!Main.gameMenu && shadow == 0)
				drawPlayer.GetModPlayer<StarlightPlayer>().PreDraw(drawPlayer, Main.spriteBatch);

			orig(self, camera, drawPlayer, position, rotation, rotationOrigin, shadow, scale);

			if (!Main.gameMenu && shadow == 0)
				drawPlayer.GetModPlayer<StarlightPlayer>().PostDraw(drawPlayer, Main.spriteBatch);
		}

		private void PostDrawPlayerGate(Terraria.Graphics.Renderers.On_ReturnGatePlayerRenderer.orig_DrawPlayer orig, object self, Camera camera, Player drawPlayer, Vector2 position, float rotation, Vector2 rotationOrigin, float shadow, float scale)
		{
			if (!Main.gameMenu && shadow == 0)
				drawPlayer.GetModPlayer<StarlightPlayer>().PreDraw(drawPlayer, Main.spriteBatch);

			orig(self, camera, drawPlayer, position, rotation, rotationOrigin, shadow, scale);

			if (!Main.gameMenu && shadow == 0)
				drawPlayer.GetModPlayer<StarlightPlayer>().PostDraw(drawPlayer, Main.spriteBatch);
		}
	}
}