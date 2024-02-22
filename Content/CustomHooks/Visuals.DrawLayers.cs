namespace StarlightRiver.Content.CustomHooks
{
	class DrawLayers : HookGroup
	{
		//A few different hooks for drawing on certain layers. Orig is always run and its just draw calls.
		public override void Load()
		{
			if (Main.dedServ)
				return;

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
	}
}