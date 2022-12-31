namespace StarlightRiver.Content.CustomHooks
{
	public class PrimitiveDrawing : HookGroup
	{
		// Should not interfere with anything.
		public override void Load()
		{
			if (Main.dedServ)
				return;

			On.Terraria.Main.DrawDust += DrawPrimitives;
		}

		private void DrawPrimitives(On.Terraria.Main.orig_DrawDust orig, Main self)
		{
			orig(self);

			if (Main.gameMenu)
				return;

			Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;

			for (int k = 0; k < Main.maxProjectiles; k++) // Projectiles.
			{
				if (Main.projectile[k].active && Main.projectile[k].ModProjectile is IDrawPrimitive)
					(Main.projectile[k].ModProjectile as IDrawPrimitive).DrawPrimitives();
			}

			for (int k = 0; k < Main.maxNPCs; k++) // NPCs.
			{
				if (Main.npc[k].active && Main.npc[k].ModNPC is IDrawPrimitive)
					(Main.npc[k].ModNPC as IDrawPrimitive).DrawPrimitives();
			}
		}
	}
}
